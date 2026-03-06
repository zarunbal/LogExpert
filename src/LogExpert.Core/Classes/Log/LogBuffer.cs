using ColumnizerLib;

using NLog;

namespace LogExpert.Core.Classes.Log;

public class LogBuffer
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

#if DEBUG
    private readonly IList<long> _filePositions = []; // file position for every line
#endif

    private readonly List<ILogLineMemory> _lineList = [];

    private readonly IList<ILogLine> _logLines = [];
    private int MAX_LINES = 500;
    private long _size;

    #endregion

    #region cTor

    //public LogBuffer() { }

    // Don't use a primary constructor here: field initializers (like MAX_LINES) run before primary constructor parameters are assigned,
    // so MAX_LINES would always be set to its default value before the constructor body can assign it. Use a regular constructor instead.
    public LogBuffer (ILogFileInfo fileInfo, int maxLines)
    {
        FileInfo = fileInfo;
        MAX_LINES = maxLines;
    }

    #endregion

    #region Properties

    public long StartPos { set; get; }

    public long Size
    {
        set
        {
            _size = value;
#if DEBUG
            if (_filePositions.Count > 0)
            {
                if (_size < _filePositions[_filePositions.Count - 1] - StartPos)
                {
                    _logger.Error("LogBuffer overall Size must be greater than last line file position!");
                }
            }
#endif
        }
        get => _size;
    }

    public int StartLine { set; get; }

    public int LineCount { get; private set; }

    public bool IsDisposed { get; private set; }

    public ILogFileInfo FileInfo { get; set; }

    public int DroppedLinesCount { get; set; }

    public int PrevBuffersDroppedLinesSum { get; set; }

    #endregion

    #region Public methods

    public void AddLine (ILogLineMemory lineMemory, long filePos)
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
        _logLines.Clear();
        _lineList.Clear();
        LineCount = 0;
    }

    public void DisposeContent ()
    {
        _logLines.Clear();
        _lineList.Clear();
        IsDisposed = true;
#if DEBUG
        DisposeCount++;
#endif
    }

    public ILogLineMemory GetLineMemoryOfBlock (int num)
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