using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert.Dialogs
{
    public class SelectLineEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public SelectLineEventArgs(int line)
        {
            this.Line = line;
        }

        #endregion

        #region Properties

        public int Line { get; }

        #endregion
    }
}