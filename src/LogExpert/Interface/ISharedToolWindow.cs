using LogExpert.Config;
using LogExpert.Entities;

namespace LogExpert.Interface
{
    /// <summary>
    /// Interface to be implemented by tools windows that are shared across multiple log files.
    /// The implementor will be called whenever the current log file changes. So it can draw new content 
    /// according to the current active log file.
    /// </summary>
    internal interface ISharedToolWindow
    {
        #region Public methods

        /// <summary>
        /// Called when a file becomes the active file (e.g. when user selects a tab).
        /// </summary>
        /// <param name="ctx"></param>
        void SetCurrentFile(FileViewContext ctx);

        /// <summary>
        /// Called whenever the current file has been changed.
        /// </summary>
        void FileChanged();

        void SetColumnizer(ILogLineColumnizer columnizer);

        void PreferencesChanged(Preferences newPreferences, bool isLoadTime, SettingsFlags flags);

        #endregion
    }
}