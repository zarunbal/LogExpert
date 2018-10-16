using System.Collections.Generic;
using NLog;

namespace LogExpert
{
    public class LogBuffer
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        #if DEBUG
        private readonly IList<long> filePositions = new List<long>(); // file position for every line
        #endif

        private readonly IList<ILogLine> logLines = new List<ILogLine>();
        private int MAX_LINES = 500;
        private long size;

        #endregion

        #region Ctor

        // public LogBuffer() { }
        public LogBuffer(ILogFileInfo fileInfo, int maxLines)
        {
            FileInfo = fileInfo;
            MAX_LINES = maxLines;
        }

        #endregion

        #region Properties / Indexers

        public int DroppedLinesCount { get; set; } = 0;

        public ILogFileInfo FileInfo { get; set; }

        public bool IsDisposed { get; private set; }

        public int LineCount { get; private set; }

        public int PrevBuffersDroppedLinesSum { get; set; } = 0;

        public long Size
        {
            get { return size; }
            set
            {
                size = value;
                #if DEBUG
                if (filePositions.Count > 0)
                {
                    if (size < filePositions[filePositions.Count - 1] - StartPos)
                    {
                        _logger.Error("LogBuffer overall Size must be greater than last line file position!");
                    }
                }

                #endif
            }
        }

        public int StartLine { get; set; } = 0;

        public long StartPos { get; set; } = 0;

        #endregion

        #region Public Methods

        public void AddLine(ILogLine line, long filePos)
        {
            logLines.Add(line);
            #if DEBUG
            filePositions.Add(filePos);
            #endif
            LineCount++;
            IsDisposed = false;
        }

        public void ClearLines()
        {
            logLines.Clear();
            LineCount = 0;
        }

        public void DisposeContent()
        {
            logLines.Clear();
            IsDisposed = true;
            #if DEBUG
            DisposeCount++;
            #endif
        }

        public ILogLine GetLineOfBlock(int num)
        {
            if (num < logLines.Count && num >= 0)
            {
                return logLines[num];
            }

            return null;
        }

        #endregion

        #if DEBUG
        public long DisposeCount { get; private set; }


        public long GetFilePosForLineOfBlock(int line)
        {
            if (line >= 0 && line < filePositions.Count)
            {
                return filePositions[line];
            }

            return -1;
        }

        #endif
    }
}
