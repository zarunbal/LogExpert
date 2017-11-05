using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace LogExpert
{
    internal class FilterCancelHandler : BackgroundProcessCancelHandler
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        #region Fields

        private readonly FilterStarter filterStarter;

        #endregion

        #region cTor

        public FilterCancelHandler(FilterStarter filterStarter)
        {
            this.filterStarter = filterStarter;
        }

        #endregion

        #region Public methods

        public void EscapePressed()
        {
            _logger.Info("FilterCancelHandler called.");
            this.filterStarter.CancelFilter();
        }

        #endregion
    }
}