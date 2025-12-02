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

    private readonly IList<ILogLine> _logLines = [];
    private int MAX_LINES = 500;
    private long _size;

    #endregion

    #region cTor

    //public LogBuffer() { }

    //Dont use Primary Constructor, MAX_LINES will not be initialized correctly
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

    public void AddLine (ILogLine line, long filePos)
    {
        _logLines.Add(line);
#if DEBUG
        _filePositions.Add(filePos);
#endif
        LineCount++;
        IsDisposed = false;
    }

    public void ClearLines ()
    {
        _logLines.Clear();
        LineCount = 0;
    }

    public void DisposeContent ()
    {
        _logLines.Clear();
        IsDisposed = true;
#if DEBUG
        DisposeCount++;
#endif
    }

    public ILogLine GetLineOfBlock (int num)
    {
        if (num < _logLines.Count && num >= 0)
        {
            return _logLines[num];
        }

        return null;
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