using System;
using System.Collections.Generic;
using System.Text;
using ColumnizerLib;

//using System.Linq;

namespace LogExpert
{
    public class LogBuffer
    {
        #region Fields

        private int droppedLinesCount = 0;
        private ILogFileInfo fileInfo;
        private bool isDisposed = false;
        private int lineCount = 0;

        private IList<ILogLine> logLines = new List<ILogLine>();
        private int MAX_LINES = 500;
        private int prevBuffersDroppedLinesSum = 0; // sum over all preceeding log buffers
        private long size = 0;
        private int startLine = 0;
        private long startPos = 0;

        #endregion

        #region cTor

        //public LogBuffer() { }

        public LogBuffer(ILogFileInfo fileInfo, int maxLines)
        {
            this.fileInfo = fileInfo;
            this.MAX_LINES = maxLines;
        }

        #endregion

        #region Properties

        public long StartPos
        {
            set { this.startPos = value; }
            get { return this.startPos; }
        }

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

        public int StartLine
        {
            set { this.startLine = value; }
            get { return this.startLine; }
        }

        public int LineCount
        {
            get { return this.lineCount; }
        }

        public bool IsDisposed
        {
            get { return this.isDisposed; }
        }

        public ILogFileInfo FileInfo
        {
            get { return this.fileInfo; }
            set { this.fileInfo = value; }
        }

        public int DroppedLinesCount
        {
            get { return this.droppedLinesCount; }
            set { this.droppedLinesCount = value; }
        }

        public int PrevBuffersDroppedLinesSum
        {
            get { return this.prevBuffersDroppedLinesSum; }
            set { this.prevBuffersDroppedLinesSum = value; }
        }

        #endregion

        #region Public methods

        public void AddLine(ILogLine line, long filePos)
        {
            this.logLines.Add(line);
#if DEBUG
            this.filePositions.Add(filePos);
#endif
            this.lineCount++;
            isDisposed = false;
        }

        public void ClearLines()
        {
            this.logLines.Clear();
            this.lineCount = 0;
        }

        public void DisposeContent()
        {
            this.logLines.Clear();
            isDisposed = true;
#if DEBUG
            disposeCount++;
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
        private long disposeCount = 0;
        private IList<long> filePositions = new List<long>(); // file position for every line
#endif
#if DEBUG
        public long DisposeCount
        {
            get { return this.disposeCount; }
        }


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