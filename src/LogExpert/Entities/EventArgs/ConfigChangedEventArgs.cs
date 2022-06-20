using System;

namespace LogExpert
{
    internal class ConfigChangedEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        internal ConfigChangedEventArgs(SettingsFlags changeFlags)
        {
            Flags = changeFlags;
        }

        #endregion

        #region Properties

        public SettingsFlags Flags { get; }

        #endregion
    }
}