namespace LogExpert;

/// <summary>
/// Defines lifecycle events for plugins.
/// Plugins can optionally implement this interface to receive lifecycle notifications.
/// </summary>
public interface IPluginLifecycle
{
    /// <summary>
    /// Called when the plugin is first loaded.
    /// Use this to initialize resources, load configuration, etc.
    /// </summary>
    /// <param name="context">Context providing information about the host environment</param>
    void Initialize(IPluginContext context);
    
    /// <summary>
    /// Called when the application is shutting down.
    /// Use this to cleanup resources, save state, etc.
    /// </summary>
    void Shutdown();
    
    /// <summary>
    /// Called when the plugin should reload its configuration.
    /// Use this to refresh settings without restarting the application.
    /// </summary>
    void Reload();
}

/// <summary>
/// Provides context information to plugins during initialization.
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// Logger for the plugin to use for diagnostic output.
    /// </summary>
    ILogExpertLogger Logger { get; }
    
    /// <summary>
    /// Directory where the plugin assembly is located.
    /// </summary>
    string PluginDirectory { get; }
    
    /// <summary>
    /// Version of the host application (LogExpert).
    /// </summary>
    Version HostVersion { get; }
    
    /// <summary>
    /// Directory where the plugin can store configuration files.
    /// Typically %APPDATA%\LogExpert\Plugins\{PluginName}\
    /// </summary>
    string ConfigurationDirectory { get; }
}
