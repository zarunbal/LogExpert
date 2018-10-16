using System;

namespace LogExpert
{
    public class HighlightEventArgs : EventArgs
    {
        #region Ctor

        public HighlightEventArgs(int startLine, int count)
        {
            StartLine = startLine;
            Count = count;
        }

        #endregion

        #region Properties / Indexers

        public int Count { get; }

        public int StartLine { get; }

        #endregion
    }
}
