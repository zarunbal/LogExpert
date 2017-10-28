using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class HighlightEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public HighlightEventArgs(int startLine, int count)
        {
            this.StartLine = startLine;
            this.Count = count;
        }

        #endregion

        #region Properties

        public int StartLine { get; }

        public int Count { get; }

        #endregion
    }
}