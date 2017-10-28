using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class FilterListChangedEventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public FilterListChangedEventArgs(LogWindow logWindow)
        {
            this.LogWindow = logWindow;
        }

        #endregion

        #region Properties

        public LogWindow LogWindow { get; }

        #endregion
    }
}