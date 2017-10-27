using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    internal class FilterCancelHandler : BackgroundProcessCancelHandler
    {
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

        #region BackgroundProcessCancelHandler Member

        public void EscapePressed()
        {
            Logger.logInfo("FilterCancelHandler called.");
            this.filterStarter.CancelFilter();
        }

        #endregion

        #endregion
    }
}