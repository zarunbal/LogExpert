using System.Threading;
using System.Threading.Tasks;

namespace LogExpert.PluginRegistry.Interfaces;

/// <summary>
/// Responsible for loading plugin assemblies.
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// Loads a plugin from the specified path.
    /// </summary>
    /// <param name="assemblyPath">Path to the plugin assembly</param>
    /// <returns>Result containing loaded plugin or error information</returns>
    PluginLoadResult LoadPlugin(string assemblyPath);
    
    /// <summary>
    /// Loads a plugin asynchronously.
    /// </summary>
    /// <param name="assemblyPath">Path to the plugin assembly</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Task containing the load result</returns>
    Task<PluginLoadResult> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken);
}

/// <summary>
/// Result of a plugin load operation.
/// </summary>
public class PluginLoadResult
{
    /// <summary>
    /// Indicates whether the plugin was loaded successfully.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The loaded plugin instance, if successful.
    /// </summary>
    public object? Plugin { get; set; }
    
    /// <summary>
    /// The plugin manifest, if available.
    /// </summary>
    public PluginManifest? Manifest { get; set; }
    
    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception that caused the failure, if any.
    /// </summary>
    public System.Exception? Exception { get; set; }
}
