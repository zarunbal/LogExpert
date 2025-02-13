using LogExpert.Interface;
using NLog;

namespace LogExpert.Classes.Filter
{
    internal class FilterCancelHandler : BackgroundProcessCancelHandler
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        #region Fields

        private readonly FilterStarter _filterStarter;

        #endregion

        #region cTor

        public FilterCancelHandler(FilterStarter filterStarter)
        {
            _filterStarter = filterStarter;
        }

        #endregion

        #region Public methods

        public void EscapePressed()
        {
            _logger.Info("FilterCancelHandler called.");
            _filterStarter.CancelFilter();
        }

        #endregion
    }
}