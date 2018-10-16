namespace LogExpert
{
    public class CurrentHighlightGroupChangedEventArgs
    {
        #region Ctor

        public CurrentHighlightGroupChangedEventArgs(LogWindow logWindow, HilightGroup currentGroup)
        {
            LogWindow = logWindow;
            CurrentGroup = currentGroup;
        }

        #endregion

        #region Properties / Indexers

        public HilightGroup CurrentGroup { get; }

        public LogWindow LogWindow { get; }

        #endregion
    }
}
