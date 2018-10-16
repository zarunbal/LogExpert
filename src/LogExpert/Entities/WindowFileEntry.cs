namespace LogExpert
{
    /// <summary>
    ///     Represents a log file and its window. Used as a kind of handle for menus or list of open files.
    /// </summary>
    public class WindowFileEntry
    {
        #region Static/Constants

        private const int MAX_LEN = 40;

        #endregion

        #region Ctor

        public WindowFileEntry(LogWindow logWindow)
        {
            LogWindow = logWindow;
        }

        #endregion

        #region Properties / Indexers

        public string FileName => LogWindow.FileName;


        public LogWindow LogWindow { get; }

        public string Title
        {
            get
            {
                string title = LogWindow.Text;
                if (title.Length > MAX_LEN)
                {
                    title = "..." + title.Substring(title.Length - MAX_LEN);
                }

                return title;
            }
        }

        #endregion
    }
}
