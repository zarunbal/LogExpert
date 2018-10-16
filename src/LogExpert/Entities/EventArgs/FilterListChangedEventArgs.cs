namespace LogExpert
{
    public class FilterListChangedEventArgs
    {
        #region Ctor

        public FilterListChangedEventArgs(LogWindow logWindow)
        {
            LogWindow = logWindow;
        }

        #endregion

        #region Properties / Indexers

        public LogWindow LogWindow { get; }

        #endregion
    }
}
