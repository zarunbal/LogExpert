using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// NLog-based implementation of ILogExpertLogger for plugins.
/// </summary>
/// <remarks>
/// Creates a logger for a specific plugin.
/// </remarks>
/// <param name="pluginName">Name of the plugin</param>
public class PluginLogger (string pluginName) : ILogExpertLogger
{
    private readonly Logger _logger = LogManager.GetLogger($"Plugin.{pluginName}");

    /// <summary>
    /// Log a debug message.
    /// </summary>
    public void Debug (string msg)
    {
        _logger.Debug(msg);
    }

    /// <summary>
    /// Log an informational message.
    /// </summary>
    public void Info (string msg)
    {
        _logger.Info(msg);
    }

    /// <summary>
    /// Log an informational message with format provider.
    /// </summary>
    public void Info (IFormatProvider formatProvider, string msg)
    {
        _logger.Info(formatProvider, msg);
    }

    /// <summary>
    /// Log a warning message.
    /// </summary>
    public void LogWarn (string msg)
    {
        _logger.Warn(msg);
    }

    /// <summary>
    /// Log an error message.
    /// </summary>
    public void LogError (string msg)
    {
        _logger.Error(msg);
    }
}
