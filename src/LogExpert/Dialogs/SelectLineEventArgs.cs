using System;

namespace LogExpert.Dialogs
{
    public class SelectLineEventArgs : EventArgs
    {
        #region Ctor

        public SelectLineEventArgs(int line)
        {
            Line = line;
        }

        #endregion

        #region Properties / Indexers

        public int Line { get; }

        #endregion
    }
}
