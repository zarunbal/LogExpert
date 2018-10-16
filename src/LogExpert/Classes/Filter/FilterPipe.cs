using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;

namespace LogExpert
{
    public class FilterPipe
    {
        #region Delegates

        public delegate void ClosedEventHandler(object sender, EventArgs e);

        #endregion

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        private IList<int> lineMappingList = new List<int>();
        private StreamWriter writer;

        #endregion

        #region Public Events

        public event ClosedEventHandler Closed;

        #endregion

        #region Ctor

        public FilterPipe(FilterParams filterParams, LogWindow logWindow)
        {
            FilterParams = filterParams;
            LogWindow = logWindow;
            IsStopped = false;
            FileName = Path.GetTempFileName();

            _logger.Info("Created temp file: {0}", FileName);
        }

        #endregion

        #region Properties / Indexers

        public string FileName { get; }

        public FilterParams FilterParams { get; }

        public bool IsStopped { get; set; }

        public IList<int> LastLinesHistoryList { get; } = new List<int>();

        public LogWindow LogWindow { get; }

        public LogWindow OwnLogWindow { get; set; }

        #endregion

        #region Public Methods

        public void ClearLineList()
        {
            lock (lineMappingList)
            {
                lineMappingList.Clear();
            }
        }

        public void ClearLineNums()
        {
            _logger.Debug("FilterPipe.ClearLineNums()");
            lock (lineMappingList)
            {
                for (int i = 0; i < lineMappingList.Count; ++i)
                {
                    lineMappingList[i] = -1;
                }
            }
        }

        public void CloseAndDisconnect()
        {
            ClearLineList();
            OnClosed();
        }

        public void CloseFile()
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }

        public int GetOriginalLineNum(int lineNum)
        {
            lock (lineMappingList)
            {
                if (lineMappingList.Count > lineNum)
                {
                    return lineMappingList[lineNum];
                }

                return -1;
            }
        }

        public void OpenFile()
        {
            FileStream fStream = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(fStream, new UnicodeEncoding(false, false));
        }

        public void RecreateTempFile()
        {
            lock (lineMappingList)
            {
                lineMappingList = new List<int>();
            }

            lock (FileName)
            {
                CloseFile();


// trunc file
                FileStream fStream = new FileStream(FileName, FileMode.Truncate, FileAccess.Write, FileShare.Read);
                fStream.SetLength(0);
                fStream.Close();
            }
        }

        public void ShiftLineNums(int offset)
        {
            _logger.Debug("FilterPipe.ShiftLineNums() offset={0}", offset);
            List<int> newList = new List<int>();
            lock (lineMappingList)
            {
                foreach (int lineNum in lineMappingList)
                {
                    int line = lineNum - offset;
                    if (line >= 0)
                    {
                        newList.Add(line);
                    }
                    else
                    {
                        newList.Add(-1);
                    }
                }

                lineMappingList = newList;
            }
        }

        public bool WriteToPipe(ILogLine textLine, int orgLineNum)
        {
            try
            {
                lock (FileName)
                {
                    lock (lineMappingList)
                    {
                        try
                        {
                            writer.WriteLine(textLine.FullLine);
                            lineMappingList.Add(orgLineNum);
                            return true;
                        }
                        catch (IOException e)
                        {
                            _logger.Error(e, "writeToPipe()");
                            return false;
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "writeToPipe(): file was closed");
                return false;
            }
        }

        #endregion

        #region Event handling Methods

        private void OnClosed()
        {
            if (Closed != null)
            {
                Closed(this, new EventArgs());
            }
        }

        #endregion
    }
}
