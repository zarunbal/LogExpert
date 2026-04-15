using ColumnizerLib;

using NLog;

namespace LogExpert.Core.Classes.Log;

public class LogBuffer
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

#if DEBUG
    private readonly List<long> _filePositions; // file position for every line
#endif

    private readonly List<LogLine> _lineList;

    private int MAX_LINES = 500;

    #endregion

    #region cTor

    //public LogBuffer() { }

    // Don't use a primary constructor here: field initializers (like MAX_LINES) run before primary constructor parameters are assigned,
    // so MAX_LINES would always be set to its default value before the constructor body can assign it. Use a regular constructor instead.
    public LogBuffer (ILogFileInfo fileInfo, int maxLines)
    {
        FileInfo = fileInfo;
        MAX_LINES = maxLines;
        _lineList = new(MAX_LINES);
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
        _lineList.Add(lineMemory);
#if DEBUG
        _filePositions.Add(filePos);
#endif
        LineCount++;
        IsDisposed = false;
    }

    public void ClearLines ()
    {
        _lineList.Clear();
        LineCount = 0;
    }

    public void DisposeContent ()
    {
        _lineList.Clear();
        IsDisposed = true;
#if DEBUG
        DisposeCount++;
#endif
    }

    public LogLine? GetLineMemoryOfBlock (int num)
    {
        return num < _lineList.Count && num >= 0
            ? _lineList[num]
            : null;
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
}