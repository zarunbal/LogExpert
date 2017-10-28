using System;
using System.Collections.Generic;
using System.Text;


namespace LogExpert
{
    public class LogBuffer
    {
        #region Fields

#if DEBUG
        private IList<long> filePositions = new List<long>(); // file position for every line
#endif

        private readonly IList<ILogLine> logLines = new List<ILogLine>();
        private int MAX_LINES = 500;
        private long size = 0;

        #endregion

        #region cTor

        //public LogBuffer() { }

        public LogBuffer(ILogFileInfo fileInfo, int maxLines)
        {
            this.FileInfo = fileInfo;
            this.MAX_LINES = maxLines;
        }

        #endregion

        #region Properties

        public long StartPos { set; get; } = 0;

        public long Size
        {
            set
            {
                this.size = value;
#if DEBUG
                if (this.filePositions.Count > 0)
                {
                    if (this.size < this.filePositions[this.filePositions.Count - 1] - this.StartPos)
                    {
                        Logger.logError("LogBuffer overall Size must be greater than last line file position!");
                    }
                }
#endif
            }
            get { return this.size; }
        }

        public int StartLine { set; get; } = 0;

        public int LineCount { get; private set; } = 0;

        public bool IsDisposed { get; private set; } = false;

        public ILogFileInfo FileInfo { get; set; }

        public int DroppedLinesCount { get; set; } = 0;

        public int PrevBuffersDroppedLinesSum { get; set; } = 0;

        #endregion

        #region Public methods

        public void AddLine(ILogLine line, long filePos)
        {
            this.logLines.Add(line);
#if DEBUG
            this.filePositions.Add(filePos);
#endif
            this.LineCount++;
            IsDisposed = false;
        }

        public void ClearLines()
        {
            this.logLines.Clear();
            this.LineCount = 0;
        }

        public void DisposeContent()
        {
            this.logLines.Clear();
            IsDisposed = true;
#if DEBUG
            DisposeCount++;
#endif
        }

        public ILogLine GetLineOfBlock(int num)
        {
            if (num < this.logLines.Count && num >= 0)
            {
                return this.logLines[num];
            }
            else
            {
                return null;
            }
        }

        #endregion

#if DEBUG
        public long DisposeCount { get; private set; } = 0;


        public long GetFilePosForLineOfBlock(int line)
        {
            if (line >= 0 && line < this.filePositions.Count)
            {
                return this.filePositions[line];
            }
            else
            {
                return -1;
            }
        }

#endif
    }
}