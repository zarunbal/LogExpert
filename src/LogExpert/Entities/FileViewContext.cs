namespace LogExpert
{
    public class FileViewContext
    {
        #region Ctor

        internal FileViewContext(ILogPaintContext logPaintContext, ILogView logView)
        {
            LogPaintContext = logPaintContext;
            LogView = logView;
        }

        #endregion

        #region Properties / Indexers

        public ILogPaintContext LogPaintContext { get; }

        public ILogView LogView { get; }

        #endregion
    }
}
