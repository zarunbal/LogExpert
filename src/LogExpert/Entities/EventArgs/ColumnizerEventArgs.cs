using System;

namespace LogExpert
{
    public class ColumnizerEventArgs : EventArgs
    {
        #region Ctor

        public ColumnizerEventArgs(ILogLineColumnizer columnizer)
        {
            Columnizer = columnizer;
        }

        #endregion

        #region Properties / Indexers

        public ILogLineColumnizer Columnizer { get; }

        #endregion
    }
}
