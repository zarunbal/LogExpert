using System.Buffers;
using System.Text;

using LogExpert.Core.Classes.Log.Buffers;
using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;

namespace LogExpert.Core.Classes.Log.Streamreaders;

/// <summary>
/// Reads decoded characters directly into pooled char[] blocks via StreamReader.Read(),
/// scans for newline boundaries, and returns ReadOnlyMemory&lt;char&gt; slices without
/// allocating per-line strings. Eliminates the StreamReader.ReadLine() string allocation.
/// </summary>
public class PositionAwareStreamReaderDirect : PositionAwareStreamReaderBase, ILogStreamReaderMemory
{
    #region Constants

    private const int BLOCK_SIZE = 32_768; // 64 KB (32K chars × 2 bytes), under LOH threshold
    private const char CHAR_LF = '\n';
    private const char CHAR_CR = '\r';

    #endregion

    #region Fields

    private char[] _readBlock;
    private int _readBlockLength;  // valid chars in _readBlock
    private int _scanOffset;       // current scan position in _readBlock
    private bool _eof;
    private int _newLineSequenceLength;
    private readonly List<char[]> _completedBlocks = [];

    public override bool IsDisposed { get; protected set; }

    #endregion

    #region cTor

    public PositionAwareStreamReaderDirect (Stream stream, EncodingOptions encodingOptions, int maximumLineLength)
        : base(stream, encodingOptions, maximumLineLength)
    {
        _readBlock = ArrayPool<char>.Shared.Rent(BLOCK_SIZE);
        _readBlockLength = 0;
        _scanOffset = 0;
        _eof = false;
    }

    #endregion

    #region Public methods

    public override string ReadLine ()
    {
        return TryReadLine(out var memory) ? memory.ToString() : null;
    }

    /// <summary>
    /// Reads the next line by scanning the current block for '\n'. If the block is exhausted,
    /// tail-copies the partial line to a new block and refills. Returns a zero-copy
    /// ReadOnlyMemory&lt;char&gt; slice into the pooled block.
    /// </summary>
    public bool TryReadLine (out ReadOnlyMemory<char> lineMemory)
    {
        var reader = GetStreamReader();

        if (_newLineSequenceLength == 0)
        {
            _newLineSequenceLength = GuessNewLineSequenceLength(reader);
        }

        while (true)
        {
            // If we have data to scan, look for \n
            if (_scanOffset < _readBlockLength)
            {
                var searchSpan = _readBlock.AsSpan(_scanOffset, _readBlockLength - _scanOffset);
                var lfIndex = searchSpan.IndexOf(CHAR_LF);

                if (lfIndex >= 0)
                {
                    // Found a line boundary
                    var lineLength = lfIndex;

                    // Strip \r if present before \n
                    if (lineLength > 0 && searchSpan[lineLength - 1] == CHAR_CR)
                    {
                        lineLength--;
                    }

                    // Enforce MaximumLineLength
                    var cappedLength = Math.Min(lineLength, MaximumLineLength);

                    lineMemory = _readBlock.AsMemory(_scanOffset, cappedLength);

                    // Update byte position: line chars + lineLength
                    var contentSpan = _readBlock.AsSpan(_scanOffset, lineLength);
                    MovePosition(Encoding.GetByteCount(contentSpan) + _newLineSequenceLength);

                    // Advance scan past the \n
                    _scanOffset += lfIndex + 1;

                    return true;
                }
            }

            // No \n found (or no data at all). Need to refill.
            if (_eof)
            {
                // Emit remaining content as final line (no trailing newline)
                if (_scanOffset < _readBlockLength)
                {
                    var remaining = _readBlockLength - _scanOffset;
                    var cappedLength = Math.Min(remaining, MaximumLineLength);
                    lineMemory = _readBlock.AsMemory(_scanOffset, cappedLength);

                    var fullSpan = _readBlock.AsSpan(_scanOffset, remaining);
                    MovePosition(Encoding.GetByteCount(fullSpan));

                    _scanOffset = _readBlockLength;
                    return true;
                }

                lineMemory = default;
                return false;
            }

            // Tail-copy: move unconsumed chars to a new block and refill
            RefillBlock(reader);
        }
    }

    public void ReturnMemory (ReadOnlyMemory<char> memory)
    {
        // Bulk return via DetachBlocks()/Dispose(). Individual return not needed.
    }

    /// <summary>
    /// Gets the block allocator for compatibility with the DetachBlocks pattern.
    /// This reader manages its own blocks directly rather than through CharBlockAllocator.
    /// </summary>
    public CharBlockAllocator? BlockAllocator => null;

    /// <summary>
    /// Detaches completed blocks (fully scanned) for transfer to the LogBuffer.
    /// The current _readBlock (partially scanned) stays with the reader.
    /// </summary>
    public List<char[]> DetachBlocks ()
    {
        // Nothing to detach: no completed blocks and no lines were scanned from the current block.
        if (_completedBlocks.Count == 0 && _scanOffset == 0)
        {
            return [];
        }

        // The current _readBlock contains memory backing lines already added to the LogBuffer.
        // It must be transferred to the buffer along with any completed blocks.
        _completedBlocks.Add(_readBlock);

        // Rent a fresh block and carry over any unscanned data (partial line in progress)
        var tailLength = _readBlockLength - _scanOffset;

        // The tail may exceed BLOCK_SIZE after reading a long line (buffer was grown).
        var newBlockSize = BLOCK_SIZE;
        while (tailLength > newBlockSize)
        {
            newBlockSize *= 2;
        }

        var newBlock = ArrayPool<char>.Shared.Rent(newBlockSize);

        if (tailLength > 0)
        {
            _readBlock.AsSpan(_scanOffset, tailLength).CopyTo(newBlock.AsSpan(0, tailLength));
        }

        _readBlock = newBlock;
        _readBlockLength = tailLength;
        _scanOffset = 0;

        var blocks = _completedBlocks.ToList();
        _completedBlocks.Clear();
        return blocks;
    }

    #endregion

    #region Private Methods

    private void RefillBlock (StreamReader reader)
    {
        var tailLength = _readBlockLength - _scanOffset;

        // Determine new block size: if the tail already fills a standard block,
        // grow the buffer so there's room to read more data. This handles lines
        // longer than BLOCK_SIZE (e.g. huge XML payloads).
        var newBlockSize = BLOCK_SIZE;
        while (tailLength >= newBlockSize)
        {
            newBlockSize *= 2;
        }

        // Rent a new block (may be larger than BLOCK_SIZE for very long lines)
        var newBlock = ArrayPool<char>.Shared.Rent(newBlockSize);

        // Copy the tail (partial line) to the start of the new block
        if (tailLength > 0)
        {
            _readBlock.AsSpan(_scanOffset, tailLength).CopyTo(newBlock.AsSpan(0, tailLength));
        }

        // The old block is fully scanned — add to completed list
        _completedBlocks.Add(_readBlock);

        _readBlock = newBlock;
        _scanOffset = 0;

        // Fill the rest of the block from the stream
        var available = newBlock.Length - tailLength;
        var charsRead = reader.Read(newBlock, tailLength, available);

        _readBlockLength = tailLength + charsRead;

        if (charsRead == 0)
        {
            _eof = true;
        }
    }

    private int GuessNewLineSequenceLength (StreamReader reader)
    {
        var currentPos = Position;

        try
        {
            // Fill initial block
            var charsRead = reader.Read(_readBlock, 0, BLOCK_SIZE);
            _readBlockLength = charsRead;
            _scanOffset = 0;

            if (charsRead == 0)
            {
                _eof = true;
                return 0;
            }

            // Find first \n to determine newline sequence
            var span = _readBlock.AsSpan(0, _readBlockLength);
            var lfIndex = span.IndexOf(CHAR_LF);

            if (lfIndex < 0)
            {
                // No newline found in first block — assume single-byte
                return 1;
            }

            if (lfIndex > 0 && span[lfIndex - 1] == CHAR_CR)
            {
                // \r\n
                Span<char> newline = ['\r', '\n'];
                return Encoding.GetByteCount(newline);
            }

            // \n only
            Span<char> singleLf = ['\n'];
            return Encoding.GetByteCount(singleLf);
        }
        finally
        {
            // Reset position — the filled block data is kept, no re-read needed
            // Position is reset to original so byte tracking starts clean
            Position = currentPos;

            // Re-read the block since Position setter resets the stream
            _scanOffset = 0;
            var charsRead = reader.Read(_readBlock, 0, BLOCK_SIZE);
            _readBlockLength = charsRead;
            if (charsRead == 0)
            {
                _eof = true;
            }
        }
    }

    protected override void Dispose (bool disposing)
    {
        if (disposing)
        {
            // Return the current (partially scanned) block
            if (_readBlock != null)
            {
                ArrayPool<char>.Shared.Return(_readBlock);
                _readBlock = null!;
            }

            // Return any completed blocks not yet detached
            foreach (var block in _completedBlocks)
            {
                ArrayPool<char>.Shared.Return(block);
            }

            _completedBlocks.Clear();
        }

        base.Dispose(disposing);
    }

    #endregion
}