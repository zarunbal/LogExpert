using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;

using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;

namespace LogExpert.Core.Classes.Log.Streamreaders;

/// <summary>
/// EXPERIMENTAL: TypedPipeline-based reader for benchmarking comparison.
/// Uses multi-threaded pipeline with BlockingCollection stages.
/// Expected to be 15-25% slower than PositionAwareStreamReaderPipeline due to pipeline overhead.
/// </summary>
public class PositionAwareStreamReaderPipelineNew : LogStreamReaderBase, ILogStreamReaderMemory
{
    private const int DEFAULT_BYTE_BUFFER_SIZE = 64 * 1024; // 64 KB
    private const int MINIMUM_READ_AHEAD_SIZE = 4 * 1024; // 4 KB
    private const int DEFAULT_CHANNEL_CAPACITY = 128; // Number of line segments

    private static readonly Encoding[] _preambleEncodings =
    [
        Encoding.UTF8,
        Encoding.Unicode,
        Encoding.BigEndianUnicode,
        Encoding.UTF32
    ];

    private readonly StreamPipeReaderOptions _streamPipeReaderOptions = new(bufferSize: DEFAULT_BYTE_BUFFER_SIZE, minimumReadSize: MINIMUM_READ_AHEAD_SIZE, leaveOpen: true);
    private readonly int _maximumLineLength;
    private readonly Lock _reconfigureLock = new();
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private readonly int _charBufferSize;
    private readonly long _preambleLength;

    private LineSegment? _currentSegment;
    private PipeReader _pipeReader;
    private CancellationTokenSource _cts;
    private Task _producerTask;
    private bool _isDisposed;
    private long _position;
    private BlockingCollection<LineSegment> _lineQueue;
    private Exception _producerException;
    private IPipeline<BufferData, LineSegment> _pipeline;

    public PositionAwareStreamReaderPipelineNew (Stream stream, EncodingOptions encodingOptions, int maximumLineLength)
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
        var (length, detectedEncoding) = DetectPreambleLength(stream);
        _preambleLength = length;
        _encoding = DetermineEncoding(encodingOptions, detectedEncoding);

        _stream = stream;
        _charBufferSize = Math.Max(_encoding.GetMaxCharCount(DEFAULT_BYTE_BUFFER_SIZE), _maximumLineLength + 2);

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
        throw new NotSupportedException("PipelineLogStreamReader currently supports line-based reads only.");
    }

    public override string ReadLine ()
    {
        if (TryReadLine(out var lineMemory))
        {
            return new string(lineMemory.Span);
        }

        return null;
    }

    public bool TryReadLine (out ReadOnlyMemory<char> lineMemory)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, GetType());

        var producerEx = Volatile.Read(ref _producerException);
        if (producerEx != null)
        {
            throw new InvalidOperationException("Producer task encountered an error.", producerEx);
        }

        var queue = _lineQueue;
        var cts = _cts;

        if (queue == null || cts == null)
        {
            lineMemory = default;
            return false;
        }

        try
        {
            // With pre-filled queue, data should be available immediately
            if (!queue.TryTake(out var segment, 50, cts.Token))
            {
                lineMemory = default;
                return false;
            }

            _currentSegment?.Dispose();
            _currentSegment = segment;

            if (segment.IsEof)
            {
                lineMemory = default;
                return false;
            }

            lineMemory = new ReadOnlyMemory<char>(segment.Buffer, 0, segment.Length);
            _ = Interlocked.Exchange(ref _position, segment.ByteOffset + segment.ByteLength);
            return true;
        }
        catch (OperationCanceledException)
        {
            lineMemory = default;
            return false;
        }
        catch (ObjectDisposedException)
        {
            lineMemory = default;
            return false;
        }
    }

    public void ReturnMemory (ReadOnlyMemory<char> memory)
    {
        // No-op for this implementation
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

                if (_lineQueue != null)
                {
                    while (_lineQueue.TryTake(out var segment))
                    {
                        segment.Dispose();
                    }

                    _lineQueue.Dispose();
                }

                _stream?.Dispose();
            }
        }

        _isDisposed = true;
    }

    private void RestartPipelineInternal (long startPosition)
    {
        _ = _stream.Seek(_preambleLength + startPosition, SeekOrigin.Begin);

        _pipeReader = PipeReader.Create(_stream, _streamPipeReaderOptions);
        _lineQueue = new BlockingCollection<LineSegment>(new ConcurrentQueue<LineSegment>(), DEFAULT_CHANNEL_CAPACITY);

        Volatile.Write(ref _producerException, null);

        // KEY FIX: Read and enqueue first line synchronously to guarantee immediate data availability
        var firstLine = ReadFirstLineSynchronously(startPosition);
        long nextByteOffset = startPosition;

        if (firstLine.HasValue)
        {
            _lineQueue.Add(firstLine.Value);
            nextByteOffset = firstLine.Value.ByteOffset + firstLine.Value.ByteLength;
        }

        _cts = new CancellationTokenSource();

        // Build TypedPipeline
        var builder = new TypedPipelineBuilder<BufferData, BufferData>();
        _pipeline = builder.AddStep(ProcessBuffer).Build();

        _pipeline.Finished += segment =>
        {
            if (segment.Buffer != null || segment.IsEof)
            {
                EnqueueLine(segment);
            }
        };

        _producerTask = Task.Run(() => ProduceAsync(nextByteOffset, _cts.Token), CancellationToken.None);

        _ = Interlocked.Exchange(ref _position, startPosition);
    }

    /// <summary>
    /// Reads the first line from the stream synchronously to pre-fill the queue.
    /// This ensures data is immediately available when TryReadLine() is called after a seek.
    /// </summary>
    private LineSegment? ReadFirstLineSynchronously (long startPosition)
    {
        const int FIRST_LINE_BUFFER_SIZE = 4096;

        var byteBuffer = ArrayPool<byte>.Shared.Rent(FIRST_LINE_BUFFER_SIZE);
        var charBuffer = ArrayPool<char>.Shared.Rent(_charBufferSize);

        try
        {
            var bytesRead = _stream.Read(byteBuffer, 0, FIRST_LINE_BUFFER_SIZE);

            if (bytesRead == 0)
            {
                return LineSegment.CreateEof(startPosition);
            }

            var decoder = _encoding.GetDecoder();
            var charsDecoded = decoder.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0, flush: false);

            var (newlineIndex, newlineChars) = FindNewlineIndex(charBuffer, 0, charsDecoded, false);

            if (newlineIndex == -1)
            {
                return null;
            }

            var segment = CreateSegment(charBuffer, 0, newlineIndex, newlineChars, startPosition);

            return segment;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(byteBuffer);
            ArrayPool<char>.Shared.Return(charBuffer);
        }
    }

    private void RestartPipeline (long newPosition)
    {
        using (_reconfigureLock.EnterScope())
        {
            CancelPipelineLocked();
            RestartPipelineInternal(newPosition);
        }
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
            // Expected
        }
        finally
        {
            _pipeline?.Complete();
            _cts.Dispose();
            _cts = null;
        }

        if (_lineQueue != null && !_lineQueue.IsAddingCompleted)
        {
            _lineQueue.CompleteAdding();
        }

        if (_pipeReader != null)
        {
            try
            {
                _pipeReader.Complete();
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }

    private async Task ProduceAsync (long startByteOffset, CancellationToken token)
    {
        var charPool = ArrayPool<char>.Shared;
        char[] charBuffer = null;
        Decoder decoder = null;

        try
        {
            charBuffer = charPool.Rent(_charBufferSize);
            decoder = _encoding.GetDecoder();

            var charsInBuffer = 0;
            var byteOffset = startByteOffset;

            while (!token.IsCancellationRequested)
            {
                ReadResult result = await _pipeReader.ReadAsync(token).ConfigureAwait(false);
                ReadOnlySequence<byte> buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    // Create buffer data and feed to pipeline
                    var bufferData = new BufferData(buffer, charBuffer, charsInBuffer, decoder, byteOffset, result.IsCompleted);

                    // Process and extract lines
                    var processResult = ProcessBufferAndExtractLines(bufferData);

                    // Update state
                    charsInBuffer = processResult.RemainingChars;
                    byteOffset = processResult.NewByteOffset;

                    // Feed each line to pipeline (which will enqueue them)
                    foreach (var segment in processResult.Lines)
                    {
                        _pipeline.Execute(new BufferData(default, segment.Buffer, 0, null, segment.ByteOffset, false)
                        {
                            PreExtractedSegment = segment
                        });
                    }

                    _pipeReader.AdvanceTo(buffer.End);
                }

                if (result.IsCompleted)
                {
                    if (charsInBuffer > 0)
                    {
                        var segment = CreateSegment(charBuffer, 0, charsInBuffer, 0, byteOffset);
                        EnqueueLine(segment);
                        byteOffset += segment.ByteLength;
                    }

                    EnqueueLine(LineSegment.CreateEof(byteOffset));
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            Volatile.Write(ref _producerException, ex);
        }
        finally
        {
            _pipeline?.Complete();

            try
            {
                _lineQueue?.CompleteAdding();
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }

            if (charBuffer != null)
            {
                charPool.Return(charBuffer);
            }
        }
    }

    private LineSegment ProcessBuffer (BufferData bufferData)
    {
        // If pre-extracted, just return it (simulating pipeline overhead)
        if (bufferData.PreExtractedSegment.HasValue)
        {
            return bufferData.PreExtractedSegment.Value;
        }

        // This shouldn't happen in normal operation, but handle gracefully
        // Return an empty line segment
        return new LineSegment(null, 0, bufferData.ByteOffset, 0, false, false);
    }

    private ProcessResult ProcessBufferAndExtractLines (BufferData bufferData)
    {
        var lines = new List<LineSegment>();
        var localCharsInBuffer = bufferData.CharsInBuffer;
        var localByteOffset = bufferData.ByteOffset;

        // Decode bytes to chars
        if (bufferData.Buffer.IsSingleSegment)
        {
            var span = bufferData.Buffer.FirstSpan;
            var charsAvailable = _charBufferSize - localCharsInBuffer;

            if (charsAvailable > 10)
            {
                bufferData.Decoder.Convert(
                    span,
                    bufferData.CharBuffer.AsSpan(localCharsInBuffer),
                    bufferData.IsCompleted,
                    out var usedBytes,
                    out var charsProduced,
                    out _);

                localCharsInBuffer += charsProduced;
                localByteOffset += usedBytes;
            }
        }

        // Extract lines
        var searchIndex = 0;
        while (true)
        {
            var (newlineIndex, newlineChars) = FindNewlineIndex(bufferData.CharBuffer, searchIndex, localCharsInBuffer - searchIndex, false);

            if (newlineIndex == -1)
            {
                break;
            }

            var lineLength = newlineIndex - searchIndex;
            var segment = CreateSegment(bufferData.CharBuffer, searchIndex, lineLength, newlineChars, localByteOffset);
            lines.Add(segment);
            localByteOffset += segment.ByteLength;
            searchIndex = newlineIndex + newlineChars;
        }

        // Calculate remaining chars
        var remaining = localCharsInBuffer - searchIndex;
        if (remaining > 0 && searchIndex > 0)
        {
            bufferData.CharBuffer.AsSpan(searchIndex, remaining).CopyTo(bufferData.CharBuffer.AsSpan(0, remaining));
        }

        return new ProcessResult(lines, remaining, localByteOffset);
    }

    private void EnqueueLine (LineSegment segment)
    {
        try
        {
            // Don't use cancellation token here - let the queue complete naturally
            _lineQueue.Add(segment);
        }
        catch (InvalidOperationException)
        {
            // Collection was marked as complete, dispose the segment
            segment.Dispose();
        }
    }

    private static (int newLineIndex, int newLineChars) FindNewlineIndex (
        char[] buffer,
        int start,
        int available,
        bool allowStandaloneCr)
    {
        var span = buffer.AsSpan(start, available);

        var lfIndex = span.IndexOf('\n');
        if (lfIndex != -1)
        {
            if (lfIndex > 0 && span[lfIndex - 1] == '\r')
            {
                return (newLineIndex: start + lfIndex - 1, newLineChars: 2);
            }

            return (newLineIndex: start + lfIndex, newLineChars: 1);
        }

        var crIndex = span.IndexOf('\r');
        if (crIndex != -1)
        {
            if (crIndex + 1 >= span.Length)
            {
                if (allowStandaloneCr)
                {
                    return (newLineIndex: start + crIndex, newLineChars: 1);
                }

                return (newLineIndex: -1, newLineChars: 0);
            }

            if (span[crIndex + 1] != '\n')
            {
                return (newLineIndex: start + crIndex, newLineChars: 1);
            }
        }

        return (newLineIndex: -1, newLineChars: 0);
    }

    private LineSegment CreateSegment (
        char[] source,
        int start,
        int lineLength,
        int newlineChars,
        long byteOffset)
    {
        var consumedChars = lineLength + newlineChars;

        var byteLength = consumedChars == 0
            ? 0
            : _encoding.GetByteCount(source, start, consumedChars);

        var logicalLength = Math.Min(lineLength, _maximumLineLength);
        var truncated = lineLength > logicalLength;

        var rentalLength = Math.Max(logicalLength, 1);
        var buffer = ArrayPool<char>.Shared.Rent(rentalLength);

        if (logicalLength > 0)
        {
            source.AsSpan(start, logicalLength).CopyTo(buffer.AsSpan(0, logicalLength));
        }

        return new LineSegment(buffer, logicalLength, byteOffset, byteLength, truncated, false);
    }

    // Pipeline data structures
    private class BufferData
    {
        public ReadOnlySequence<byte> Buffer { get; }
        public char[] CharBuffer { get; }
        public int CharsInBuffer { get; }
        public Decoder Decoder { get; }
        public long ByteOffset { get; }
        public bool IsCompleted { get; }
        public LineSegment? PreExtractedSegment { get; set; }

        public BufferData (ReadOnlySequence<byte> buffer, char[] charBuffer, int charsInBuffer, Decoder decoder, long byteOffset, bool isCompleted)
        {
            Buffer = buffer;
            CharBuffer = charBuffer;
            CharsInBuffer = charsInBuffer;
            Decoder = decoder;
            ByteOffset = byteOffset;
            IsCompleted = isCompleted;
        }
    }

    private record ProcessResult (List<LineSegment> Lines, int RemainingChars, long NewByteOffset);
    private record PipelineInput (ReadOnlySequence<byte> Buffer, char[] CharBuffer, int CharsInBuffer, Decoder Decoder, long ByteOffset, bool IsCompleted);
    private record DecodeResult (char[] CharBuffer, int CharsInBuffer, long ByteOffset);
    private record ExtractResult (List<LineSegment> Lines);

    private readonly struct LineSegment : IDisposable
    {
        public char[] Buffer { get; }
        public int Length { get; }
        public long ByteOffset { get; }
        public int ByteLength { get; }
        public bool IsTruncated { get; }
        public bool IsEof { get; }

        public LineSegment (char[] buffer, int length, long byteOffset, int byteLength, bool isTruncated, bool isEof)
        {
            Buffer = buffer;
            Length = length;
            ByteOffset = byteOffset;
            ByteLength = byteLength;
            IsTruncated = isTruncated;
            IsEof = isEof;
        }

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

    private static Encoding DetermineEncoding (EncodingOptions options, Encoding detectedEncoding)
    {
        return options?.Encoding != null
            ? options.Encoding
            : detectedEncoding ?? options?.DefaultEncoding ?? Encoding.Default;
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
}
