using System;

namespace LogExpert
{
    public class StatusLineEventArgs : EventArgs
    {
        #region Properties / Indexers

        public int CurrentLineNum { get; set; }

        public long FileSize { get; set; }

        public int LineCount { get; set; }

        public string StatusText { get; set; }

        #endregion

        #region Public Methods

        public StatusLineEventArgs Clone()
        {
            StatusLineEventArgs e = new StatusLineEventArgs();
            e.StatusText = StatusText;
            e.CurrentLineNum = CurrentLineNum;
            e.LineCount = LineCount;
            e.FileSize = FileSize;
            return e;
        }

        #endregion
    }
}
