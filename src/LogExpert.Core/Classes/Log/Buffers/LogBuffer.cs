using System.Buffers;

using ColumnizerLib;

using NLog;

namespace LogExpert.Core.Classes.Log.Buffers;

public class LogBuffer
{
    #region Fields

    private SpinLock _contentLock = new(enableThreadOwnerTracking: false);
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

#if DEBUG
    private readonly List<long> _filePositions; // file position for every line
#endif

    private LogLine[] _lineArray;
    private int _lineArrayLength; // capacity of the rented array
    private List<char[]> _charBlocks;

    private int MAX_LINES = 500;

    #endregion

    #region cTor

    // Don't use a primary constructor here: field initializers (like MAX_LINES) run before primary constructor parameters are assigned,
    // so MAX_LINES would always be set to its default value before the constructor body can assign it. Use a regular constructor instead.
    public LogBuffer (ILogFileInfo fileInfo, int maxLines)
    {
        FileInfo = fileInfo;
        MAX_LINES = maxLines;
        _lineArray = ArrayPool<LogLine>.Shared.Rent(maxLines);
        _lineArrayLength = _lineArray.Length;
#if DEBUG
        _filePositions = new(MAX_LINES);
#endif
    }

    #endregion

    #region Properties

    public long StartPos { set; get; }

    public long Size
    {
        set
        {
            field = value;
#if DEBUG
            if (_filePositions.Count > 0)
            {
                if (field < _filePositions[^1] - StartPos)
                {
                    _logger.Error("LogBuffer overall Size must be greater than last line file position!");
                }
            }
#endif
        }
        get;
    }

    public int EndLine => StartLine + LineCount;

    public int StartLine { set; get; }

    public int LineCount { get; private set; }

    public bool IsDisposed { get; private set; }

    public ILogFileInfo FileInfo { get; set; }

    public int DroppedLinesCount { get; set; }

    public int PrevBuffersDroppedLinesSum { get; set; }

    #endregion

    #region Public methods

    public void AddLine (LogLine lineMemory, long filePos)
    {
        if (LineCount < _lineArrayLength)
        {
            _lineArray[LineCount] = lineMemory;
            LineCount++;
        }
#if DEBUG
        else
        {
            _logger.Error("AddLine overflow: LineCount={0} >= _lineArrayLength={1}", LineCount, _lineArrayLength);
        }
#endif

#if DEBUG
        _filePositions.Add(filePos);
#endif
        IsDisposed = false;
    }

    public void ClearLines ()
    {
        if (_lineArray == null)
        {
            _lineArray = ArrayPool<LogLine>.Shared.Rent(MAX_LINES);
            _lineArrayLength = _lineArray.Length;
        }
        else
        {
            Array.Clear(_lineArray, 0, LineCount);
        }

        ReturnCharBlocks();

        LineCount = 0;
#if DEBUG
        _filePositions.Clear();
#endif
    }

    /// <summary>
    /// Prepares the buffer for reuse from the pool.
    /// </summary>
    public void Reinitialise (ILogFileInfo fileInfo, int maxLines)
    {
        ReturnCharBlocks();

        FileInfo = fileInfo;
        MAX_LINES = maxLines;
        StartLine = 0;
        StartPos = 0;
        Size = 0;
        LineCount = 0;
        DroppedLinesCount = 0;
        PrevBuffersDroppedLinesSum = 0;
        IsDisposed = false;
        _lineArray = ArrayPool<LogLine>.Shared.Rent(maxLines);
        _lineArrayLength = _lineArray.Length;
#if DEBUG
        _filePositions.Clear();
        DisposeCount = 0;
#endif
    }

    /// <summary>
    /// Evicts the buffer content to free memory while preserving metadata (LineCount, StartLine, StartPos, Size).
    /// The buffer remains findable in buffer list lookups and can be re-read from disk when accessed.
    /// </summary>
    public void EvictContent ()
    {
        if (_lineArray != null)
        {
            Array.Clear(_lineArray, 0, LineCount);
            ArrayPool<LogLine>.Shared.Return(_lineArray);
            _lineArray = null;
        }

        ReturnCharBlocks();

        //! Do NOT zero LineCount — it is needed for buffer lookup in GetBufferForLineWithIndex.
        //! Do NOT zero StartLine, StartPos, Size — they are needed for re-reading from disk.
        IsDisposed = true;
#if DEBUG
        DisposeCount++;
#endif
    }

    /// <summary>
    /// Fully disposes the buffer content and resets all metadata. Used when the buffer is being returned to the pool
    /// or completely removed from the buffer list.
    /// </summary>
    public void DisposeContent ()
    {
        if (_lineArray != null)
        {
            Array.Clear(_lineArray, 0, LineCount);
            ArrayPool<LogLine>.Shared.Return(_lineArray);
            _lineArray = null;
            LineCount = 0;
        }

        ReturnCharBlocks();

        IsDisposed = true;
#if DEBUG
        DisposeCount++;
#endif
    }

    public LogLine? GetLineMemoryOfBlock (int num)
    {
        return num < LineCount && num >= 0
        ? _lineArray[num]
        : null;
    }

    /// <summary>
    /// Acquires the content lock. The caller MUST call <see cref="ReleaseContentLock"/> in a finally block.
    /// </summary>
    public void AcquireContentLock (ref bool lockTaken)
    {
        _contentLock.Enter(ref lockTaken);
    }

    /// <summary>
    /// Releases the content lock previously acquired via <see cref="AcquireContentLock"/>.
    /// </summary>
    public void ReleaseContentLock ()
    {
        _contentLock.Exit(useMemoryBarrier: false);
    }

    /// <summary>
    /// Attaches pooled char[] blocks that back the ReadOnlyMemory in this buffer's LogLine entries.
    /// These blocks will be returned to ArrayPool when the buffer is evicted or disposed.
    /// </summary>
    public void AttachCharBlocks (List<char[]> blocks)
    {
        ReturnCharBlocks(); // return any previously held blocks
        _charBlocks = blocks;
    }

    #endregion

#if DEBUG
    public long DisposeCount { get; private set; }

    public long GetFilePosForLineOfBlock (int line)
    {
        return line >= 0 && line < _filePositions.Count
            ? _filePositions[line]
            : -1;
    }

#endif

    #region Private Methods

    private void ReturnCharBlocks ()
    {
        // Just drop the reference — do NOT return to ArrayPool.
        // The UI thread may still hold ReadOnlyMemory<char> slices into these blocks.
        // GC will collect them once all references (LogLine, UI snapshots) are released.
        _charBlocks = null;
    }

    #endregion
}