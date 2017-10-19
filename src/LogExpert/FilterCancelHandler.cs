using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  class FilterCancelHandler : BackgroundProcessCancelHandler
  {
    FilterStarter filterStarter;

    public FilterCancelHandler(FilterStarter filterStarter)
    {
      this.filterStarter = filterStarter;
    }

    #region BackgroundProcessCancelHandler Member

    public void EscapePressed()
    {
      Logger.logInfo("FilterCancelHandler called.");
      this.filterStarter.CancelFilter();
    }

    #endregion
  }
}
