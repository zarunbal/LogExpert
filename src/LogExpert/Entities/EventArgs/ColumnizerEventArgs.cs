using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class ColumnizerEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public ColumnizerEventArgs(ILogLineColumnizer columnizer)
        {
            this.Columnizer = columnizer;
        }

        #endregion

        #region Properties

        public ILogLineColumnizer Columnizer { get; }

        #endregion
    }
}