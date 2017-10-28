using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    internal class ConfigChangedEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        internal ConfigChangedEventArgs(SettingsFlags changeFlags)
        {
            this.Flags = changeFlags;
        }

        #endregion

        #region Properties

        public SettingsFlags Flags { get; }

        #endregion
    }
}