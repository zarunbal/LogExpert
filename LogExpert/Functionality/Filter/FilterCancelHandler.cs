using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	internal class FilterCancelHandler : IBackgroundProcessCancelHandler
	{
		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		public FilterStarter FilterStarter { get; set; }

		public FilterCancelHandler(FilterStarter filterStarter)
		{
			this.FilterStarter = filterStarter;
		}

		#region BackgroundProcessCancelHandler Member

		public void EscapePressed()
		{
			_logger.Info("FilterCancelHandler called.");
			this.FilterStarter.CancelFilter();
		}

		#endregion BackgroundProcessCancelHandler Member
	}
}