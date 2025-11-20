using System.Reflection;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Lazy-loading proxy for plugins that defers actual loading until first use.
/// Improves startup performance by only loading plugins when needed.
/// </summary>
/// <typeparam name="T">Type of plugin to load</typeparam>
public class LazyPluginProxy<T> where T : class
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly Lazy<T?> _plugin;

    /// <summary>
    /// Plugin manifest containing metadata about the plugin.
    /// </summary>
    public PluginManifest? Manifest { get; }

    /// <summary>
    /// Indicates whether the plugin has been loaded yet.
    /// </summary>
    public bool IsLoaded => _plugin.IsValueCreated;

    /// <summary>
    /// Name of the plugin from manifest or filename.
    /// </summary>
    public string PluginName { get; }

    /// <summary>
    /// Path to the plugin assembly file.
    /// </summary>
    public string AssemblyPath { get; }

    /// <summary>
    /// Creates a new lazy plugin proxy.
    /// </summary>
    /// <param name="assemblyPath">Path to the plugin assembly</param>
    /// <param name="manifest">Optional manifest with plugin metadata</param>
    public LazyPluginProxy (string assemblyPath, PluginManifest? manifest)
    {
        ArgumentNullException.ThrowIfNull(assemblyPath);

        AssemblyPath = assemblyPath;
        Manifest = manifest;
        PluginName = manifest?.Name ?? Path.GetFileNameWithoutExtension(assemblyPath);

        // Create lazy initializer with thread-safety
        _plugin = new Lazy<T?>(LoadPlugin, isThreadSafe: true);
    }

    /// <summary>
    /// Gets the plugin instance, loading it if necessary.
    /// This property will trigger plugin loading on first access.
    /// </summary>
    public T? Instance => _plugin.Value;

    /// <summary>
    /// Loads the plugin from the assembly file.
    /// This is called automatically on first access to Instance property.
    /// </summary>
    /// <returns>The plugin instance or null if loading fails</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch Unexpected Errors")]
    private T? LoadPlugin ()
    {
        try
        {
            _logger.Info("Lazy-loading plugin: {PluginName} from {Path}", PluginName, AssemblyPath);

            // Verify file still exists
            if (!File.Exists(AssemblyPath))
            {
                _logger.Error("Plugin assembly not found: {Path}", AssemblyPath);
                return null;
            }

            // Load the assembly
            var assembly = Assembly.LoadFrom(AssemblyPath);
            _logger.Debug("Assembly loaded: {Name}", assembly.FullName);

            // Find types implementing the plugin interface
            var pluginType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(T).IsAssignableFrom(t) &&
                                   !t.IsAbstract &&
                                   !t.IsInterface);

            if (pluginType == null)
            {
                _logger.Error("No suitable plugin type found in {Path}. Looking for type assignable to {Type}",
                    AssemblyPath, typeof(T).Name);
                return null;
            }

            _logger.Debug("Found plugin type: {Type}", pluginType.FullName);

            // Create instance
            var instance = (T?)Activator.CreateInstance(pluginType);

            if (instance == null)
            {
                _logger.Error("Failed to create instance of plugin type: {Type}", pluginType.FullName);
                return null;
            }

            _logger.Info("Successfully lazy-loaded plugin: {PluginName}", PluginName);
            return instance;
        }
        catch (FileLoadException ex)
        {
            _logger.Error(ex, "Failed to load plugin assembly (file load error): {PluginName}", PluginName);
            return null;
        }
        catch (BadImageFormatException ex)
        {
            _logger.Error(ex, "Failed to load plugin assembly (bad format): {PluginName}", PluginName);
            return null;
        }
        catch (ReflectionTypeLoadException ex)
        {
            _logger.Error(ex, "Failed to load types from plugin assembly: {PluginName}", PluginName);

            // Log loader exceptions for more detail
            if (ex.LoaderExceptions != null)
            {
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    _logger.Error(loaderEx, "Loader exception");
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error lazy-loading plugin: {PluginName}", PluginName);
            return null;
        }
    }

    /// <summary>
    /// Attempts to preload the plugin without accessing the Instance property.
    /// Useful for warming up the cache or testing plugin availability.
    /// </summary>
    /// <returns>True if plugin loaded successfully, false otherwise</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentionally Catch All")]
    public bool TryPreload ()
    {
        try
        {
            return Instance != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets information about the plugin without loading it.
    /// </summary>
    /// <returns>A string describing the plugin</returns>
    public override string ToString ()
    {
        return IsLoaded
            ? $"{PluginName} (Loaded)"
            : $"{PluginName} (Not Loaded)";
    }
}
