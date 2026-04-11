using System.Globalization;

using LogExpert.Core.Interfaces;

using NLog;

namespace LogExpert.Core.Classes.Filter;

public class FilterCancelHandler (FilterStarter filterStarter) : IBackgroundProcessCancelHandler
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    #region Fields

    private readonly FilterStarter _filterStarter = filterStarter;

    #endregion
    #region cTor

    #endregion

    #region Public methods

    public void EscapePressed ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "FilterCancelHandler called.");
        _filterStarter.CancelFilter();
    }

    #endregion
}