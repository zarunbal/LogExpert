using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class LogEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region Properties

        public int RolloverOffset { get; set; } = 0;

        public bool IsRollover { get; set; } = false;

        public long FileSize { get; set; }

        public int LineCount { get; set; }

        public int PrevLineCount { get; set; }

        public long PrevFileSize { get; set; }

        #endregion
    }
}