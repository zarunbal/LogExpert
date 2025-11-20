namespace LogExpert.PluginRegistry;

/// <summary>
/// Default implementation of IPluginContext.
/// </summary>
public class PluginContext : IPluginContext
{
    /// <summary>
    /// Logger for the plugin to use.
    /// </summary>
    public ILogExpertLogger Logger { get; init; }

    /// <summary>
    /// Directory where the plugin is located.
    /// </summary>
    public string PluginDirectory { get; init; }

    /// <summary>
    /// Version of the host application.
    /// </summary>
    public Version HostVersion { get; init; }

    /// <summary>
    /// Configuration directory for the plugin.
    /// </summary>
    public string ConfigurationDirectory { get; init; }
}