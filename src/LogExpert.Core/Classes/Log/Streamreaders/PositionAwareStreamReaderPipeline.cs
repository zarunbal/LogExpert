using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;

using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;

namespace LogExpert.Core.Classes.Log.Streamreaders;

public class PositionAwareStreamReaderPipeline : LogStreamReaderBase, ILogStreamReaderMemory
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
    private readonly int _byteBufferSize;
    private readonly int _charBufferSize;
    private readonly long _preambleLength;

    private LineSegment? _currentSegment;

    private PipeReader _pipeReader;
    private CancellationTokenSource _cts;
    private Task _producerTask;
    private bool _isDisposed;
    private long _position;

    // Line queue - using BlockingCollection for thread-safe, race-free synchronization
    private BlockingCollection<LineSegment> _lineQueue;
    private Exception _producerException;

    public PositionAwareStreamReaderPipeline (Stream stream, EncodingOptions encodingOptions, int maximumLineLength)
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

        _stream = stream;
        _charBufferSize = Math.Max(_encoding.GetMaxCharCount(_byteBufferSize), _maximumLineLength + 2);

        // Start the pipeline (will create the collection)
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
            return new string(lineMemory.Span); // Only allocate when explicitly requested
        }

        return null;

        //ObjectDisposedException.ThrowIf(IsDisposed, GetType());

        //// Check for producer exception
        //var producerEx = Volatile.Read(ref _producerException);
        //if (producerEx != null)
        //{
        //    throw new InvalidOperationException("Producer task encountered an error.", producerEx);
        //}

        //LineSegment segment;
        //try
        //{
        //    // BlockingCollection.Take() blocks until an item is available or collection is completed
        //    // This eliminates the race condition present in the semaphore + queue approach
        //    segment = _lineQueue.Take(_cts?.Token ?? CancellationToken.None);
        //}
        //catch (OperationCanceledException)
        //{
        //    return null;
        //}
        //catch (InvalidOperationException) // Thrown when collection is marked as completed and empty
        //{
        //    return null;
        //}

        //using (segment)
        //{
        //    if (segment.IsEof)
        //    {
        //        return null;
        //    }

        //    var line = new string(segment.Buffer, 0, segment.Length);
        //    _ = Interlocked.Exchange(ref _position, segment.ByteOffset + segment.ByteLength);
        //    return line;
        //}
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

                // Clean up remaining items and dispose collection
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
        // Seek stream to start position (accounting for preamble)
        _ = _stream.Seek(_preambleLength + startPosition, SeekOrigin.Begin);

        // Create PipeReader
        _pipeReader = PipeReader.Create(_stream, _streamPipeReaderOptions);

        _lineQueue = new BlockingCollection<LineSegment>(new ConcurrentQueue<LineSegment>(), DEFAULT_CHANNEL_CAPACITY);

        Volatile.Write(ref _producerException, null);

        // Create cancellation token
        _cts = new CancellationTokenSource();

        // Start producer task
        _producerTask = Task.Run(() => ProduceAsync(startPosition, _cts.Token), CancellationToken.None);

        // Update position
        _ = Interlocked.Exchange(ref _position, startPosition);
    }

    private void RestartPipeline (long newPosition)
    {
        using (_reconfigureLock.EnterScope())
        {
            CancelPipelineLocked();
            RestartPipelineInternal(newPosition);
        }
    }

    /// <summary>
    /// Cancels the current pipeline operation and releases associated resources. This method should be called while
    /// holding the appropriate lock to ensure thread safety.
    /// </summary>
    /// <remarks>This method cancels any ongoing producer task, marks the internal queue as complete to
    /// unblock waiting consumers, and disposes of pipeline resources. It is intended for internal use and must be
    /// invoked only when the pipeline is in a valid state for cancellation.</remarks>
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
            // Ignore if already disposed
        }

        try
        {
            _producerTask?.Wait();
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
        {
            // Expected cancellation
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }

        // Mark collection as complete to unblock any waiting Take() calls
        // This must happen AFTER the producer task is cancelled and finished
        if (_lineQueue != null && !_lineQueue.IsAddingCompleted)
        {
            _lineQueue.CompleteAdding();
        }

        // Complete and dispose the PipeReader
        if (_pipeReader != null)
        {
            try
            {
                _pipeReader.Complete();
            }
            catch (Exception)
            {
                // Ignore errors during completion
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
            // Allocate char buffer
            charBuffer = charPool.Rent(_charBufferSize);
            decoder = _encoding.GetDecoder();

            var charsInBuffer = 0;
            var byteOffset = startByteOffset;

            while (!token.IsCancellationRequested)
            {
                // Read from pipe
                ReadResult result = await _pipeReader.ReadAsync(token).ConfigureAwait(false);
                ReadOnlySequence<byte> buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    // Process the buffer - decode and extract lines
                    var state = ProcessBuffer(buffer, charBuffer, charsInBuffer, decoder, byteOffset, result.IsCompleted);
                    charsInBuffer = state.charsInBuffer;
                    byteOffset = state.byteOffset;

                    // Advance the reader
                    _pipeReader.AdvanceTo(buffer.End);
                }

                if (result.IsCompleted)
                {
                    // Handle any remaining chars in buffer as final line
                    if (charsInBuffer > 0)
                    {
                        var segment = CreateSegment(charBuffer, 0, charsInBuffer, 0, byteOffset);
                        EnqueueLine(segment);
                        byteOffset += segment.ByteLength;
                    }

                    // Send EOF marker
                    EnqueueLine(LineSegment.CreateEof(byteOffset));
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when Position is changed or disposed
        }
        catch (Exception ex)
        {
            // Store exception to rethrow in ReadLine
            Volatile.Write(ref _producerException, ex);
        }
        finally
        {
            // Always mark collection as complete when producer finishes
            try
            {
                _lineQueue?.CompleteAdding();
            }
            catch (ObjectDisposedException)
            {
                // Collection was already disposed
            }

            if (charBuffer != null)
            {
                charPool.Return(charBuffer);
            }
        }
    }

    private void EnqueueLine (LineSegment segment)
    {
        try
        {
            _lineQueue.Add(segment, _cts.Token);
        }
        catch (InvalidOperationException)
        {
            // Collection was marked as complete, dispose the segment
            segment.Dispose();
        }
    }

    private (int charsInBuffer, long byteOffset) ProcessBuffer (
        ReadOnlySequence<byte> buffer,
        char[] charBuffer,
        int charsInBuffer,
        Decoder decoder,
        long byteOffset,
        bool isCompleted)
    {
        var localByteOffset = byteOffset;
        var localCharsInBuffer = charsInBuffer;

        // Decode bytes to chars
        if (buffer.IsSingleSegment)
        {
            // Fast path for single segment
            var span = buffer.FirstSpan;
            (localCharsInBuffer, localByteOffset) = DecodeAndProcessSegment(span, charBuffer, localCharsInBuffer, decoder, localByteOffset, isCompleted);
        }
        else
        {
            // Slow path for multi-segment
            foreach (var segment in buffer)
            {
                (localCharsInBuffer, localByteOffset) = DecodeAndProcessSegment(segment.Span, charBuffer, localCharsInBuffer, decoder, localByteOffset, false);
            }

            if (isCompleted)
            {
                // Flush decoder on completion
                decoder.Convert([], 0, 0, charBuffer, localCharsInBuffer,
                    _charBufferSize - localCharsInBuffer, true,
                    out _, out var charsProduced, out _);
                localCharsInBuffer += charsProduced;
            }
        }

        // Scan for complete lines
        var searchIndex = 0;
        while (true)
        {
            var (newlineIndex, newlineChars) = FindNewlineIndex(charBuffer, searchIndex, localCharsInBuffer - searchIndex, isCompleted);

            if (newlineIndex == -1)
            {
                break;
            }

            var lineLength = newlineIndex - searchIndex;
            var segment = CreateSegment(charBuffer, searchIndex, lineLength, newlineChars, localByteOffset);
            localByteOffset += segment.ByteLength;
            EnqueueLine(segment);
            searchIndex = newlineIndex + newlineChars;
        }

        // Move remaining chars to beginning of buffer
        var remaining = localCharsInBuffer - searchIndex;
        if (remaining > 0 && searchIndex > 0)
        {
            charBuffer.AsSpan(searchIndex, remaining).CopyTo(charBuffer.AsSpan(0, remaining));
            //Array.Copy(charBuffer, searchIndex, charBuffer, 0, remaining);
        }

        return (remaining, localByteOffset);
    }

    private (int charsInBuffer, long byteOffset) DecodeAndProcessSegment (ReadOnlySpan<byte> bytes, char[] charBuffer, int charsInBuffer, Decoder decoder, long byteOffset, bool flush)
    {
        var bytesConsumed = 0;

        while (bytesConsumed < bytes.Length)
        {
            var charsAvailable = _charBufferSize - charsInBuffer;

            // CRITICAL FIX: Process lines when buffer is getting full
            if (charsAvailable < 100) // Leave room for multi-byte sequences
            {
                // Process lines to free up space
                var searchIndex = 0;
                while (searchIndex < charsInBuffer)
                {
                    var available = charsInBuffer - searchIndex;
                    var (newlineIndex, newlineChars) = FindNewlineIndex(charBuffer, searchIndex, available, false);

                    if (newlineIndex == -1)
                    {
                        // No more complete lines found
                        var remaining = charsInBuffer - searchIndex;
                        if (remaining > 0 && searchIndex > 0)
                        {
                            charBuffer.AsSpan(searchIndex, remaining).CopyTo(charBuffer.AsSpan(0, remaining));
                            //Array.Copy(charBuffer, searchIndex, charBuffer, 0, remaining);
                        }

                        charsInBuffer = remaining;
                        break;
                    }

                    // Found a line - create and enqueue it
                    var lineLength = newlineIndex - searchIndex;
                    var segment = CreateSegment(charBuffer, searchIndex, lineLength, newlineChars, byteOffset);
                    byteOffset += segment.ByteLength;
                    EnqueueLine(segment);
                    searchIndex = newlineIndex + newlineChars;
                }

                // If still no space, force process current content as truncated line
                if (charsInBuffer >= _charBufferSize - 100 && charsInBuffer > 0)
                {
                    var segment = CreateSegment(charBuffer, 0, charsInBuffer, 0, byteOffset);
                    byteOffset += segment.ByteLength;
                    EnqueueLine(segment);
                    charsInBuffer = 0;
                }

                charsAvailable = _charBufferSize - charsInBuffer;

                if (charsAvailable < 10)
                {
                    // Still no space - exit to avoid infinite loop
                    break;
                }
            }

            decoder.Convert(
                bytes[bytesConsumed..],
                charBuffer.AsSpan(charsInBuffer),
                flush && bytesConsumed == bytes.Length,
                out var usedBytes,
                out var charsProduced,
                out _);

            bytesConsumed += usedBytes;
            charsInBuffer += charsProduced;
        }

        return (charsInBuffer, byteOffset);
    }

    /// <summary>
    /// Finds the next newline in the char buffer.
    /// Handles \r, \n, and \r\n as newline delimiters.
    /// </summary>
    /// <param name="buffer">The char buffer to search</param>
    /// <param name="start">Start index for search</param>
    /// <param name="available">Number of chars available to search</param>
    /// <param name="allowStandaloneCr">If true, treats \r at end of buffer as newline</param>
    /// <returns>Tuple of (newline index, newline char count)</returns>
    private static (int newLineIndex, int newLineChars) FindNewlineIndex (
        char[] buffer,
        int start,
        int available,
        bool allowStandaloneCr)
    {
        var span = buffer.AsSpan(start, available);

        //Vectorized Search for \n
        var lfIndex = span.IndexOf('\n');
        if (lfIndex != -1)
        {
            // Found \n - check if preceded by \r
            if (lfIndex > 0 && span[lfIndex - 1] == '\r')
            {
                return (newLineIndex: start + lfIndex - 1, newLineChars: 2);
            }

            return (newLineIndex: start + lfIndex, newLineChars: 1);
        }

        //Vectorized search for \r
        var crIndex = span.IndexOf('\r');
        if (crIndex != -1)
        {
            // Check if at end of buffer
            if (crIndex + 1 >= span.Length)
            {
                if (allowStandaloneCr)
                {
                    return (newLineIndex: start + crIndex, newLineChars: 1);
                }

                return (newLineIndex: -1, newLineChars: 0);
            }

            // Check next char
            if (span[crIndex + 1] != '\n')
            {
                return (newLineIndex: start + crIndex, newLineChars: 1);
            }
        }

        return (newLineIndex: -1, newLineChars: 0);
    }

    /// <summary>
    /// Creates a LineSegment from the char buffer, handling truncation.
    /// </summary>
    private LineSegment CreateSegment (
        char[] source,
        int start,
        int lineLength,
        int newlineChars,
        long byteOffset)
    {
        var consumedChars = lineLength + newlineChars;

        // Calculate byte length for position tracking
        var byteLength = consumedChars == 0
            ? 0
            : _encoding.GetByteCount(source, start, consumedChars);

        // Apply maximum line length constraint
        var logicalLength = Math.Min(lineLength, _maximumLineLength);
        var truncated = lineLength > logicalLength;

        // Rent buffer from pool (ensure at least size 1)
        var rentalLength = Math.Max(logicalLength, 1);
        var buffer = ArrayPool<char>.Shared.Rent(rentalLength);

        // Copy line content (excluding newline)
        if (logicalLength > 0)
        {
            source.AsSpan(start, logicalLength).CopyTo(buffer.AsSpan(0, logicalLength));
            //Array.Copy(source, start, buffer, 0, logicalLength);
        }

        return new LineSegment(buffer, logicalLength, byteOffset, byteLength, truncated, false);
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

    public bool TryReadLine (out ReadOnlyMemory<char> lineMemory)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, GetType());

        var producerEx = Volatile.Read(ref _producerException);
        if (producerEx != null)
        {
            throw new InvalidOperationException("Producer task encountered an error.", producerEx);
        }

        if (!_lineQueue.TryTake(out var segment, 100, _cts?.Token ?? CancellationToken.None))
        {
            lineMemory = default;
            return false;
        }

        // Store segment for lifetime management
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

    public void ReturnMemory (ReadOnlyMemory<char> memory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a line segment with its position and metadata.
    /// Uses ArrayPool for efficient char buffer management.
    /// </summary>
    private readonly struct LineSegment : IDisposable
    {
        /// <summary>
        /// The rented char buffer from ArrayPool. May be larger than Length.
        /// </summary>
        public char[] Buffer { get; }

        /// <summary>
        /// The actual length of the line content in the buffer.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The byte offset in the stream where this line starts.
        /// </summary>
        public long ByteOffset { get; }

        /// <summary>
        /// The number of bytes consumed from the stream for this line (including newline).
        /// </summary>
        public int ByteLength { get; }

        /// <summary>
        /// True if the line was truncated due to maximum line length constraint.
        /// </summary>
        public bool IsTruncated { get; }

        /// <summary>
        /// True if this is an EOF marker segment.
        /// </summary>
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

        /// <summary>
        /// Creates an EOF marker segment.
        /// </summary>
        public static LineSegment CreateEof (long byteOffset)
        {
            return new LineSegment(null, 0, byteOffset, 0, false, true);
        }
    }
}
