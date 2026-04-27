using System.Buffers;

namespace LogExpert.Core.Classes.Log.Buffers;

/// <summary>
/// Allocates <see cref="ReadOnlyMemory{Char}"/> slices from large pooled char[] blocks.
/// Multiple lines are packed into each block to reduce per-line allocation overhead.
/// </summary>
/// <remarks>
/// Each block is rented from <see cref="ArrayPool{Char}.Shared"/>. When the current block
/// has insufficient space for a requested allocation, a new block is rented. All blocks
/// are returned to the pool when <see cref="ReturnAll"/> is called.
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
        _currentBlock = ArrayPool<char>.Shared.Rent(_blockSize);
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
            var oversized = ArrayPool<char>.Shared.Rent(length);
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
        _currentBlock = ArrayPool<char>.Shared.Rent(_blockSize);
        _blocks.Add(_currentBlock);
        _currentOffset = length;
        return _currentBlock.AsMemory(0, length);
    }

    /// <summary>
    /// Detaches and returns the list of normal (fixed-size) blocks. After this call,
    /// the allocator no longer owns those blocks — the caller (LogBuffer) is responsible
    /// for returning them to <see cref="ArrayPool{Char}.Shared"/>.
    ///
    /// Oversized blocks are returned to the pool immediately during this call, since
    /// each one backs exactly one line and has already been copied into the caller's
    /// data structures.
    /// </summary>
    public List<char[]> DetachBlocks ()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Return oversized blocks immediately — they are one-off rentals
        foreach (var oversized in _oversizedBlocks)
        {
            ArrayPool<char>.Shared.Return(oversized);
        }

        _oversizedBlocks.Clear();

        // Swap the list — O(1), no copy. Caller owns the old list.
        var blocks = _blocks;
        _currentBlock = ArrayPool<char>.Shared.Rent(_blockSize);
        _blocks = [_currentBlock];
        _currentOffset = 0;
        return blocks;
    }

    /// <summary>
    /// Returns all rented blocks (both normal and oversized) to <see cref="ArrayPool{Char}.Shared"/>.
    /// </summary>
    public void ReturnAll ()
    {
        foreach (var block in _blocks)
        {
            ArrayPool<char>.Shared.Return(block);
        }

        foreach (var oversized in _oversizedBlocks)
        {
            ArrayPool<char>.Shared.Return(oversized);
        }

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