using System;

namespace LogExpert
{
    internal class ConfigChangedEventArgs : EventArgs
    {
        #region Ctor

        internal ConfigChangedEventArgs(SettingsFlags changeFlags)
        {
            Flags = changeFlags;
        }

        #endregion

        #region Properties / Indexers

        public SettingsFlags Flags { get; }

        #endregion
    }
}
