namespace LogExpert.Core.Classes.Log.Buffers;

/// <summary>
/// Allocates <see cref="ReadOnlyMemory{Char}"/> slices from large char[] blocks.
/// Multiple lines are packed into each block to reduce per-line allocation overhead.
/// </summary>
/// <remarks>
/// Blocks are plain arrays (not pooled) because their lifetime extends beyond the allocator:
/// the UI thread may hold <see cref="ReadOnlyMemory{Char}"/> slices long after the backing
/// <see cref="LogBuffer"/> is evicted. Using <see cref="System.Buffers.ArrayPool{T}"/> here would
/// cause use-after-return corruption when evicted blocks are re-rented by new reads.
///
/// We still get the primary GC benefit: hundreds of short-lived strings from
/// <see cref="System.IO.StreamReader.ReadLine"/> are copied into a few large blocks, keeping
/// the strings Gen0-eligible and reducing Gen1/Gen2 promotions.
///
/// This class is NOT thread-safe. Each reader/fill operation should use its own instance.
/// </remarks>
public sealed class CharBlockAllocator : IDisposable
{
    private const int DEFAULT_BLOCK_SIZE = 65_536; // 128 KB in chars (64K chars × 2 bytes)

    private readonly int _blockSize;
    private List<char[]> _blocks = [];
    private readonly List<char[]> _oversizedBlocks = [];
    private char[] _currentBlock;
    private int _currentOffset;
    private bool _disposed;

    public CharBlockAllocator (int blockSize = DEFAULT_BLOCK_SIZE)
    {
        _blockSize = blockSize;
        _currentBlock = new char[_blockSize];
        _blocks.Add(_currentBlock);
        _currentOffset = 0;
    }

    /// <summary>
    /// Gets the number of normal (fixed-size) blocks currently rented from the pool.
    /// </summary>
    public int BlockCount => _blocks.Count;

    /// <summary>
    /// Gets the number of oversized (standalone) blocks currently rented from the pool.
    /// Useful for diagnostics — a high count indicates pathological line lengths.
    /// </summary>
    public int OversizedBlockCount => _oversizedBlocks.Count;

    /// <summary>
    /// Allocates a <see cref="Memory{Char}"/> region of the specified length from the current block.
    /// If the current block has insufficient space, a new block is rented.
    /// Lines longer than the block size receive a standalone rental tracked separately.
    /// </summary>
    public Memory<char> Rent (int length)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (length <= 0)
        {
            return Memory<char>.Empty;
        }

        // Oversized line: give it its own array, tracked separately
        if (length > _blockSize)
        {
            var oversized = new char[length];
            _oversizedBlocks.Add(oversized);
            return oversized.AsMemory(0, length);
        }

        // Current block has space
        if (_currentOffset + length <= _currentBlock.Length)
        {
            var memory = _currentBlock.AsMemory(_currentOffset, length);
            _currentOffset += length;
            return memory;
        }

        // Need a new block
        _currentBlock = new char[_blockSize];
        _blocks.Add(_currentBlock);
        _currentOffset = length;
        return _currentBlock.AsMemory(0, length);
    }

    /// <summary>
    /// Detaches and returns the list of all blocks (normal + oversized). After this call,
    /// the allocator no longer owns those blocks — the caller (LogBuffer) holds them
    /// until GC collects them after all <see cref="ReadOnlyMemory{Char}"/> slices are released.
    /// </summary>
    public List<char[]> DetachBlocks ()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Merge oversized blocks into the main list so the caller owns everything
        if (_oversizedBlocks.Count > 0)
        {
            _blocks.AddRange(_oversizedBlocks);
            _oversizedBlocks.Clear();
        }

        // Swap the list — O(1), no copy. Caller owns the old list.
        var blocks = _blocks;
        _currentBlock = new char[_blockSize];
        _blocks = [_currentBlock];
        _currentOffset = 0;
        return blocks;
    }

    /// <summary>
    /// Releases all block references. The actual char[] memory is collected by GC
    /// once all <see cref="ReadOnlyMemory{Char}"/> slices pointing into them are released.
    /// </summary>
    public void ReturnAll ()
    {
        _blocks.Clear();
        _oversizedBlocks.Clear();
        _currentBlock = null!;
        _currentOffset = 0;
    }

    public void Dispose ()
    {
        if (_disposed)
        {
            return;
        }

        ReturnAll();
        _disposed = true;
    }
}