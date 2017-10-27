using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class CurrentHighlightGroupChangedEventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public CurrentHighlightGroupChangedEventArgs(LogWindow logWindow, HilightGroup currentGroup)
        {
            this.LogWindow = logWindow;
            this.CurrentGroup = currentGroup;
        }

        #endregion

        #region Properties

        public LogWindow LogWindow { get; }

        public HilightGroup CurrentGroup { get; }

        #endregion
    }
}