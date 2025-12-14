using System.Reflection;

using ColumnizerLib;

using LogExpert.PluginRegistry.Interfaces;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Default implementation of IPluginLoader that loads plugins from assemblies.
/// </summary>
public class DefaultPluginLoader : IPluginLoader
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Loads all plugins from the specified assembly path.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch Unexpected errors")]
    public PluginLoadResult LoadPlugin (string assemblyPath)
    {
        try
        {
            _logger.Info("Loading plugin from: {Path}", assemblyPath);

            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyPath);
            _logger.Debug("Assembly loaded: {Name}", assembly.FullName);

            // Load manifest if available
            var manifestPath = Path.ChangeExtension(assemblyPath, ".manifest.json");
            PluginManifest? manifest = null;

            if (File.Exists(manifestPath))
            {
                manifest = PluginManifest.Load(manifestPath);
                _logger.Info("Loaded manifest for plugin: {Name} v{Version}", manifest?.Name, manifest?.Version);
            }
            else
            {
                _logger.Debug("No manifest found at: {Path}", manifestPath);
            }

            // Find plugin types (ILogLineColumnizer implementations)
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(ILogLineMemoryColumnizer).IsAssignableFrom(t) &&
                           !t.IsAbstract &&
                           !t.IsInterface)
                .ToList();

            if (pluginTypes.Count == 0)
            {
                _logger.Warn("No plugin types found in assembly: {Path}", assemblyPath);
                return new PluginLoadResult
                {
                    Success = false,
                    ErrorMessage = "No plugin types (ILogLineColumnizer implementations) found in assembly",
                    Manifest = manifest
                };
            }

            _logger.Debug("Found {Count} plugin type(s) in assembly", pluginTypes.Count);

            // Instantiate ALL plugin types, not just the first one
            var plugins = new List<object>();

            foreach (var pluginType in pluginTypes)
            {
                _logger.Debug("Instantiating plugin type: {Type}", pluginType.FullName);

                var plugin = Activator.CreateInstance(pluginType);

                if (plugin == null)
                {
                    _logger.Error("Failed to instantiate plugin type: {Type}", pluginType.FullName);
                    continue; // Skip this plugin but continue with others
                }

                plugins.Add(plugin);
                _logger.Info("Successfully instantiated plugin: {Type}", pluginType.Name);
            }

            if (plugins.Count == 0)
            {
                return new PluginLoadResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to create instances of any plugin types in assembly",
                    Manifest = manifest
                };
            }

            _logger.Info("Successfully loaded {Count} plugin(s) from assembly", plugins.Count);

            // For backward compatibility, return the first plugin instance
            // The PluginRegistry will handle loading all types via LoadPluginAssembly
            return new PluginLoadResult
            {
                Success = true,
                Plugin = plugins.First(),
                Manifest = manifest,
                AllPlugins = plugins
            };
        }
        catch (FileNotFoundException ex)
        {
            _logger.Error(ex, "Plugin assembly not found: {Path}", assemblyPath);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = $"Plugin file not found: {assemblyPath}",
                Exception = ex
            };
        }
        catch (BadImageFormatException ex)
        {
            _logger.Error(ex, "Invalid assembly format: {Path}", assemblyPath);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = $"Plugin has invalid format (wrong architecture or corrupted): {Path.GetFileName(assemblyPath)}",
                Exception = ex
            };
        }
        catch (ReflectionTypeLoadException ex)
        {
            _logger.Error(ex, "Failed to load types from assembly: {Path}", assemblyPath);

            // Log loader exceptions for more detail
            if (ex.LoaderExceptions != null)
            {
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    _logger.Error(loaderEx, "Loader exception");
                }
            }

            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = $"Failed to load plugin types: {ex.Message}",
                Exception = ex
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error loading plugin: {Path}", assemblyPath);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = $"Unexpected error loading plugin: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Loads a plugin asynchronously.
    /// </summary>
    public async Task<PluginLoadResult> LoadPluginAsync (string assemblyPath, CancellationToken cancellationToken)
    {
        _logger.Debug("Loading plugin asynchronously: {Path}", assemblyPath);
        return await Task.Run(() => LoadPlugin(assemblyPath), cancellationToken).ConfigureAwait(false);
    }
}