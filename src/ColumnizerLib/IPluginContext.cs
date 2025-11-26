namespace ColumnizerLib;

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
