using System.Buffers;
using System.Text;
using System.Threading.Channels;

using LogExpert.Core.Entities;

namespace LogExpert.Core.Classes.Log;

/// <summary>
/// Experimental channel-based reader that produces lines in a background task and feeds them through a bounded Channel.
/// The class is intentionally self-contained so that it can be wired in parallel to the existing PositionAware readers.
/// </summary>
public class PositionAwareStreamReaderChannel : LogStreamReaderBase
{
    private const int DEFAULT_BYTE_BUFFER_SIZE = 64 * 1024;
    private const int DEFAULT_CHANNEL_CAPACITY = 128;

    private static readonly Encoding[] _preambleEncodings =
    [
        Encoding.UTF8,
        Encoding.Unicode,
        Encoding.BigEndianUnicode,
        Encoding.UTF32
    ];

    private readonly Lock _reconfigureLock = new();
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private readonly int _maximumLineLength;
    private readonly int _byteBufferSize;
    private readonly int _charBufferSize;
    private readonly long _preambleLength;

    private Channel<LineSegment> _channel;
    private CancellationTokenSource _cts;
    private Task _producerTask;
    private bool _isDisposed;
    private long _position;

    public PositionAwareStreamReaderChannel (Stream stream, EncodingOptions encodingOptions, int maximumLineLength)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream must support reading.", nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must support seeking.", nameof(stream));
        }

        if (maximumLineLength <= 0)
        {
            maximumLineLength = 1024;
        }

        _maximumLineLength = maximumLineLength;
        _byteBufferSize = DEFAULT_BYTE_BUFFER_SIZE;
        var (length, detectedEncoding) = DetectPreambleLength(stream);
        _preambleLength = length;
        _encoding = DetermineEncoding(encodingOptions, detectedEncoding);

        _ = stream.Seek(_preambleLength, SeekOrigin.Begin);
        _stream = stream;

        _charBufferSize = Math.Max(_encoding.GetMaxCharCount(_byteBufferSize), _maximumLineLength + 2);

        RestartPipelineInternal(0);
    }

    public override long Position
    {
        get => Interlocked.Read(ref _position);
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            RestartPipeline(value);
        }
    }

    public override bool IsBufferComplete => true;

    public override Encoding Encoding => _encoding;

    public override bool IsDisposed
    {
        get => _isDisposed;
        protected set => _isDisposed = value;
    }

    public override int ReadChar ()
    {
        throw new NotSupportedException("ChannelLogStreamReader currently supports line-based reads only.");
    }

    public override string ReadLine ()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, GetType());

        var reader = GetChannelReader();
        LineSegment segment;
        try
        {
            if (!reader.TryRead(out segment))
            {
                var readTask = reader.ReadAsync(_cts.Token).AsTask();
                segment = readTask.GetAwaiter().GetResult();
            }
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (ChannelClosedException)
        {
            return null;
        }

        using (segment)
        {
            if (segment.IsEof)
            {
                return null;
            }

            var line = new string(segment.Buffer, 0, segment.Length);
            _ = Interlocked.Exchange(ref _position, segment.ByteOffset + segment.ByteLength);
            return line;
        }
    }

    protected override void Dispose (bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            using (_reconfigureLock.EnterScope())
            {
                CancelPipelineLocked();
                _stream.Dispose();
            }
        }

        _isDisposed = true;
    }

    private void RestartPipeline (long newPosition)
    {
        using (_reconfigureLock.EnterScope())
        {
            CancelPipelineLocked();
            RestartPipelineInternal(newPosition);
        }
    }

    private void RestartPipelineInternal (long startPosition)
    {
        _ = _stream.Seek(_preambleLength + startPosition, SeekOrigin.Begin);
        _channel = Channel.CreateBounded<LineSegment>(new BoundedChannelOptions(DEFAULT_CHANNEL_CAPACITY)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = false
        });

        _cts = new CancellationTokenSource();
        _producerTask = Task.Run(() => ProduceAsync(_channel.Writer, startPosition, _cts.Token), CancellationToken.None);
        _ = Interlocked.Exchange(ref _position, startPosition);
    }

    private void CancelPipelineLocked ()
    {
        if (_cts == null)
        {
            return;
        }

        try
        {
            _cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Ignore
        }

        try
        {
            _producerTask?.Wait();
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
        {
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }
    }

    private ChannelReader<LineSegment> GetChannelReader ()
    {
        using var scope = _reconfigureLock.EnterScope();
        return _channel.Reader;
    }

    private async Task ProduceAsync (ChannelWriter<LineSegment> writer, long startByteOffset, CancellationToken token)
    {
        var bytePool = ArrayPool<byte>.Shared;
        var charPool = ArrayPool<char>.Shared;
        var byteBuffer = bytePool.Rent(_byteBufferSize);
        var charBuffer = charPool.Rent(_charBufferSize);
        var decoder = _encoding.GetDecoder();

        var charsInBuffer = 0;
        var byteOffset = startByteOffset;
        var reachedEof = false;

        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                var bytesRead = await _stream.ReadAsync(byteBuffer.AsMemory(0, _byteBufferSize), token).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    reachedEof = true;
                    decoder.Convert([], 0, 0, charBuffer, charsInBuffer, _charBufferSize - charsInBuffer, true, out _, out var charsProduced, out _);
                    charsInBuffer += charsProduced;
                    var flushState = await FlushBufferAsync(writer, charBuffer, charsInBuffer, byteOffset, true, token).ConfigureAwait(false);
                    charsInBuffer = flushState.charsInBuffer;
                    byteOffset = flushState.byteOffset;
                    break;
                }

                var bytesConsumed = 0;
                while (bytesConsumed < bytesRead)
                {
                    decoder.Convert(byteBuffer, bytesConsumed, bytesRead - bytesConsumed, charBuffer, charsInBuffer, _charBufferSize - charsInBuffer, false, out var usedBytes, out var charsProduced, out _);
                    bytesConsumed += usedBytes;
                    charsInBuffer += charsProduced;

                    if (charsInBuffer == _charBufferSize)
                    {
                        var flushState = await FlushBufferAsync(writer, charBuffer, charsInBuffer, byteOffset, false, token).ConfigureAwait(false);
                        charsInBuffer = flushState.charsInBuffer;
                        byteOffset = flushState.byteOffset;
                    }
                }

                var state = await FlushBufferAsync(writer, charBuffer, charsInBuffer, byteOffset, false, token).ConfigureAwait(false);
                charsInBuffer = state.charsInBuffer;
                byteOffset = state.byteOffset;
            }

            if (reachedEof)
            {
                await writer.WriteAsync(LineSegment.CreateEof(byteOffset), token).ConfigureAwait(false);
            }
            else
            {
                await writer.WriteAsync(LineSegment.CreateEof(byteOffset), token).ConfigureAwait(false);
            }

            _ = writer.TryComplete();
        }
        catch (OperationCanceledException)
        {
            _ = writer.TryComplete();
        }
        catch (Exception ex)
        {
            _ = writer.TryComplete(ex);
        }
        finally
        {
            bytePool.Return(byteBuffer);
            charPool.Return(charBuffer);
        }
    }

    private async Task<(int charsInBuffer, long byteOffset)> FlushBufferAsync (ChannelWriter<LineSegment> writer, char[] charBuffer, int charsInBuffer, long byteOffset, bool finalFlush, CancellationToken token)
    {
        var segments = new List<LineSegment>();
        var searchIndex = 0;
        var localByteOffset = byteOffset;

        try
        {
            while (true)
            {
                var (newlineIndex, newlineChars) = FindNewlineIndex(charBuffer, searchIndex, charsInBuffer - searchIndex, finalFlush);
                if (newlineIndex == -1)
                {
                    break;
                }

                var lineLength = newlineIndex - searchIndex;
                var segment = CreateSegment(charBuffer, searchIndex, lineLength, newlineChars, localByteOffset);
                localByteOffset += segment.ByteLength;
                segments.Add(segment);
                searchIndex = newlineIndex + newlineChars;
            }

            foreach (var segment in segments)
            {
                await writer.WriteAsync(segment, token).ConfigureAwait(false);
            }
        }
        catch
        {
            foreach (var segment in segments)
            {
                segment.Dispose();
            }

            throw;
        }

        var remaining = charsInBuffer - searchIndex;
        if (remaining > 0 && searchIndex > 0)
        {
            Array.Copy(charBuffer, searchIndex, charBuffer, 0, remaining);
        }

        return (remaining, localByteOffset);
    }

    private LineSegment CreateSegment (char[] source, int start, int lineLength, int newlineChars, long byteOffset)
    {
        var consumedChars = lineLength + newlineChars;
        var byteLength = consumedChars == 0
            ? 0
            : _encoding.GetByteCount(source, start, consumedChars);

        var logicalLength = Math.Min(lineLength, _maximumLineLength);
        var rentalLength = Math.Max(logicalLength, 1);
        var buffer = ArrayPool<char>.Shared.Rent(rentalLength);

        if (logicalLength > 0)
        {
            Array.Copy(source, start, buffer, 0, logicalLength);
        }

        var truncated = lineLength > logicalLength;
        return new LineSegment(buffer, logicalLength, byteOffset, byteLength, truncated, false);
    }

    private static (int newLineIndex, int newLineChars) FindNewlineIndex (char[] buffer, int start, int available, bool allowStandaloneCr)
    {
        var end = start + available;
        for (var i = start; i < end; i++)
        {
            var current = buffer[i];
            if (current == '\n')
            {
                if (i > start && buffer[i - 1] == '\r')
                {
                    return (newLineIndex: i - 1, newLineChars: 2);
                }

                return (newLineIndex: i, newLineChars: 1);
            }

            if (current == '\r')
            {
                if (i + 1 >= end)
                {
                    if (allowStandaloneCr)
                    {
                        return (newLineIndex: i, newLineChars: 1);
                    }

                    break;
                }

                if (buffer[i + 1] != '\n')
                {
                    return (newLineIndex: i, newLineChars: 1);
                }
            }
        }

        return (newLineIndex: -1, newLineChars: 0);
    }

    private static (int length, Encoding? detectedEncoding) DetectPreambleLength (Stream stream)
    {
        if (!stream.CanSeek)
        {
            return (0, null);
        }

        var originalPos = stream.Position;
        var buffer = new byte[4];
        _ = stream.Seek(0, SeekOrigin.Begin);
        var readBytes = stream.Read(buffer, 0, buffer.Length);
        _ = stream.Seek(originalPos, SeekOrigin.Begin);

        if (readBytes >= 2)
        {
            foreach (var encoding in _preambleEncodings)
            {
                var preamble = encoding.GetPreamble();
                var fail = false;
                for (var i = 0; i < readBytes && i < preamble.Length; ++i)
                {
                    if (buffer[i] != preamble[i])
                    {
                        fail = true;
                        break;
                    }
                }

                if (!fail)
                {
                    return (preamble.Length, encoding);
                }
            }
        }

        return (0, null);
    }

    private static Encoding DetermineEncoding (EncodingOptions options, Encoding detectedEncoding)
    {
        return options?.Encoding != null
            ? options.Encoding
            : detectedEncoding ?? options?.DefaultEncoding ?? Encoding.Default;
    }

    private readonly struct LineSegment (char[] buffer, int length, long byteOffset, int byteLength, bool isTruncated, bool isEof) : IDisposable
    {
        public char[] Buffer { get; } = buffer;

        public int Length { get; } = length;

        public long ByteOffset { get; } = byteOffset;

        public int ByteLength { get; } = byteLength;

        public bool IsTruncated { get; } = isTruncated;

        public bool IsEof { get; } = isEof;

        public void Dispose ()
        {
            if (Buffer != null)
            {
                ArrayPool<char>.Shared.Return(Buffer);
            }
        }

        public static LineSegment CreateEof (long byteOffset)
        {
            return new LineSegment(null, 0, byteOffset, 0, false, true);
        }
    }
}
