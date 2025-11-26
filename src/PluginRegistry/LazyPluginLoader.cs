using System.Reflection;
using System.Security;

using ColumnizerLib;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Generic lazy plugin loader that defers loading until first access.
/// Thread-safe singleton pattern for plugin instances.
/// </summary>
/// <typeparam name="T">The plugin interface type (ILogLineColumnizer, IFileSystemPlugin, etc.)</typeparam>
/// <remarks>
/// Initializes a new instance of the LazyPluginLoader class.
/// </remarks>
/// <param name="dllPath">Path to the plugin DLL</param>
/// <param name="manifest">Optional plugin manifest</param>
/// <param name="fileSystemCallback">Optional file system callback for IFileSystemPlugin</param>
public class LazyPluginLoader<T> (string dllPath, PluginManifest? manifest, IFileSystemCallback? fileSystemCallback = null) where T : class
{
    private readonly IFileSystemCallback? _fileSystemCallback = fileSystemCallback;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private T? _instance;
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the path to the plugin DLL.
    /// </summary>
    public string DllPath { get; } = dllPath ?? throw new ArgumentNullException(nameof(dllPath));

    /// <summary>
    /// Gets the plugin manifest if available.
    /// </summary>
    public PluginManifest? Manifest { get; } = manifest;

    /// <summary>
    /// Gets a value indicating whether the plugin has been loaded.
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Gets the plugin instance, loading it on first access.
    /// Thread-safe - multiple calls return the same instance.
    /// </summary>
    /// <returns>The plugin instance, or null if loading failed</returns>
    public T? GetInstance ()
    {
        // Fast path - already loaded
        if (IsLoaded)
        {
            return _instance;
        }

        lock (_lock)
        {
            // Double-check after acquiring lock
            if (IsLoaded)
            {
                return _instance;
            }

            _logger.Info("Lazy loading {PluginType} from {FileName}", typeof(T).Name, Path.GetFileName(DllPath));

            try
            {
                var assembly = Assembly.LoadFrom(DllPath);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    // Check if type implements T
                    if (!typeof(T).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    // Try to instantiate
                    var instance = TryInstantiate(type);
                    if (instance != null)
                    {
                        _instance = instance;
                        IsLoaded = true;
                        _logger.Info("Successfully lazy loaded: {TypeName}", type.Name);
                        return _instance;
                    }
                }

                _logger.Warn("No compatible type found in {FileName} for {InterfaceType}", Path.GetFileName(DllPath), typeof(T).Name);
            }
            catch (Exception ex) when (ex is ArgumentException or
                                             FileNotFoundException or
                                             FileLoadException or
                                             BadImageFormatException or
                                             SecurityException or
                                             ArgumentNullException or
                                             PathTooLongException or
                                             ReflectionTypeLoadException)
            {
                _logger.Error(ex, "Failed to lazy load plugin from {FileName}", Path.GetFileName(DllPath));
            }

            // Mark as loaded even on failure to prevent retries
            IsLoaded = true;
            return _instance;
        }
    }

    /// <summary>
    /// Attempts to instantiate a plugin of the specified type.
    /// Tries parameterized constructor first (for IFileSystemPlugin), then parameterless.
    /// </summary>
    private T? TryInstantiate (Type type)
    {
        try
        {
            // For IFileSystemPlugin, try constructor with IFileSystemCallback first
            if (typeof(T) == typeof(IFileSystemPlugin) && _fileSystemCallback != null)
            {
                var ctorWithCallback = type.GetConstructor([typeof(IFileSystemCallback)]);
                if (ctorWithCallback != null)
                {
                    _logger.Debug("Instantiating {TypeName} with IFileSystemCallback", type.Name);
                    var instance = ctorWithCallback.Invoke([_fileSystemCallback]);
                    return instance as T;
                }
            }

            // Try parameterless constructor
            var ctor = type.GetConstructor(Type.EmptyTypes);

            if (ctor != null)
            {
                _logger.Debug("Instantiating {TypeName} with parameterless constructor", type.Name);
                var instance = ctor.Invoke([]);
                return instance as T;
            }

            _logger.Warn("Type {TypeName} has no suitable constructor", type.Name);
            return null;
        }
        catch (TargetInvocationException ex)
        {
            _logger.Error(ex.InnerException ?? ex, "Constructor threw exception for {TypeName}", type.Name);
            return null;
        }
        catch (Exception ex) when (ex is ArgumentException or
                                         ArgumentNullException or
                                         MemberAccessException or
                                         NotSupportedException or
                                         MethodAccessException or
                                         TargetParameterCountException or
                                         SecurityException)
        {
            _logger.Error(ex, "Failed to instantiate {TypeName}", type.Name);
            return null;
        }
    }

    /// <summary>
    /// Returns a string representation of this lazy loader.
    /// </summary>
    public override string ToString ()
    {
        return $"LazyPluginLoader<{typeof(T).Name}>: {Path.GetFileName(DllPath)} (Loaded: {IsLoaded})";
    }
}
