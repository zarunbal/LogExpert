namespace LogExpert.Core.Classes.Log;

/// <summary>
/// Stores byte offsets for each line start in a file. Supports incremental appending for tail mode.
/// </summary>
internal sealed class LineOffsetIndex (int initialCapacity = 4096)
{
    private long[] _offsets = new long[initialCapacity];

    public int LineCount { get; private set; }

    /// <summary>
    /// Appends a line-start offset.
    /// </summary>
    public void Add (long offset)
    {
        if (LineCount == _offsets.Length)
        {
            Array.Resize(ref _offsets, _offsets.Length * 2);
        }

        _offsets[LineCount++] = offset;
    }

    /// <summary>
    /// Returns the byte offset of the start of the given line.
    /// </summary>
    public long GetOffset (int lineNum)
    {
        return (uint)lineNum < (uint)LineCount ? _offsets[lineNum] : -1;
    }

    /// <summary>
    /// Returns the byte length of the given line (from its start to the next line's start).
    /// For the last line, returns -1 (unknown length, read to end or newline).
    /// </summary>
    public long GetLineLength (int lineNum)
    {
        return (uint)lineNum >= (uint)LineCount
            ? -1
            : lineNum + 1 < LineCount
                ? _offsets[lineNum + 1] - _offsets[lineNum]
                : -1;
    }

    /// <summary>
    /// Removes all offsets, resetting the index.
    /// </summary>
    public void Clear ()
    {
        LineCount = 0;
    }
}
