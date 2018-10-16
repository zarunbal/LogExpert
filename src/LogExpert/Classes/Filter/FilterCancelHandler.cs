using NLog;

namespace LogExpert
{
    internal class FilterCancelHandler : BackgroundProcessCancelHandler
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        private readonly FilterStarter filterStarter;

        #endregion

        #region Ctor

        public FilterCancelHandler(FilterStarter filterStarter)
        {
            this.filterStarter = filterStarter;
        }

        #endregion

        #region Interface BackgroundProcessCancelHandler

        public void EscapePressed()
        {
            _logger.Info("FilterCancelHandler called.");
            filterStarter.CancelFilter();
        }

        #endregion
    }
}
