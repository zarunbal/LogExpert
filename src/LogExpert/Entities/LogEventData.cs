using System;

namespace LogExpert
{
    public class LogEventArgs : EventArgs
    {
        #region Properties / Indexers

        public long FileSize { get; set; }

        public bool IsRollover { get; set; } = false;

        public int LineCount { get; set; }

        public long PrevFileSize { get; set; }

        public int PrevLineCount { get; set; }

        public int RolloverOffset { get; set; } = 0;

        #endregion
    }
}
