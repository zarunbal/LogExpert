using LogExpert.Config;

namespace LogExpert.Entities.EventArgs
{
    internal class ConfigChangedEventArgs : System.EventArgs
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