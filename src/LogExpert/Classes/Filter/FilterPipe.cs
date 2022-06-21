using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LogExpert.Controls.LogWindow;
using NLog;

namespace LogExpert.Classes.Filter
{
    public class FilterPipe
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private IList<int> _lineMappingList = new List<int>();
        private StreamWriter _writer;

        #endregion

        #region cTor

        public FilterPipe(FilterParams filterParams, LogWindow logWindow)
        {
            FilterParams = filterParams;
            LogWindow = logWindow;
            IsStopped = false;
            FileName = Path.GetTempFileName();

            _logger.Info("Created temp file: {0}", FileName);
        }

        #endregion

        #region Delegates

        public delegate void ClosedEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event ClosedEventHandler Closed;

        #endregion

        #region Properties

        public bool IsStopped { get; set; }

        public string FileName { get; }

        public FilterParams FilterParams { get; }

        public IList<int> LastLinesHistoryList { get; } = new List<int>();

        public LogWindow LogWindow { get; }

        public LogWindow OwnLogWindow { get; set; }

        #endregion

        #region Public methods

        public void OpenFile()
        {
            FileStream fStream = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(fStream, new UnicodeEncoding(false, false));
        }

        public void CloseFile()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
        }

        public bool WriteToPipe(ILogLine textLine, int orgLineNum)
        {
            try
            {
                lock (FileName)
                {
                    lock (_lineMappingList)
                    {
                        try
                        {
                            _writer.WriteLine(textLine.FullLine);
                            _lineMappingList.Add(orgLineNum);
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

        public int GetOriginalLineNum(int lineNum)
        {
            lock (_lineMappingList)
            {
                if (_lineMappingList.Count > lineNum)
                {
                    return _lineMappingList[lineNum];
                }

                return -1;
            }
        }

        public void ShiftLineNums(int offset)
        {
            _logger.Debug("FilterPipe.ShiftLineNums() offset={0}", offset);
            List<int> newList = new List<int>();
            lock (_lineMappingList)
            {
                foreach (int lineNum in _lineMappingList)
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
                _lineMappingList = newList;
            }
        }

        public void ClearLineNums()
        {
            _logger.Debug("FilterPipe.ClearLineNums()");
            lock (_lineMappingList)
            {
                for (int i = 0; i < _lineMappingList.Count; ++i)
                {
                    _lineMappingList[i] = -1;
                }
            }
        }

        public void ClearLineList()
        {
            lock (_lineMappingList)
            {
                _lineMappingList.Clear();
            }
        }

        public void RecreateTempFile()
        {
            lock (_lineMappingList)
            {
                _lineMappingList = new List<int>();
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

        public void CloseAndDisconnect()
        {
            ClearLineList();
            OnClosed();
        }

        #endregion

        #region Private Methods

        private void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}