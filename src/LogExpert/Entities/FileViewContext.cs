using LogExpert.Interface;

namespace LogExpert.Entities
{
    public class FileViewContext
    {
        #region Fields

        #endregion

        #region cTor

        internal FileViewContext(ILogPaintContext logPaintContext, ILogView logView)
        {
            this.LogPaintContext = logPaintContext;
            this.LogView = logView;
        }

        #endregion

        #region Properties

        public ILogPaintContext LogPaintContext { get; }

        public ILogView LogView { get; }

        #endregion
    }
}