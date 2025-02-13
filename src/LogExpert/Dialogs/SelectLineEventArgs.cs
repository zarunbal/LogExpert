using System;

namespace LogExpert.Dialogs
{
    public class SelectLineEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public SelectLineEventArgs(int line)
        {
            Line = line;
        }

        #endregion

        #region Properties

        public int Line { get; }

        #endregion
    }
}