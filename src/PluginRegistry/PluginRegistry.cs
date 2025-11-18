using System.Globalization;
using System.Reflection;
using System.Security;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Entities;
using LogExpert.Core.Interface;
using LogExpert.PluginRegistry.Events;
using LogExpert.PluginRegistry.FileSystem;
using LogExpert.PluginRegistry.Interfaces;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Holds all registered plugins.
/// </summary>
/// <remarks>
/// It all has started with Columnizers only. So the different types of plugins have no common super interface. I didn't change it
/// to keep existing plugin API stable. In a future version this may change.
/// </remarks>
public class PluginRegistry : IPluginRegistry
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static PluginRegistry? _instance;
    private static readonly Lock _lock = new();

    private readonly IFileSystemCallback _fileSystemCallback = new FileSystemCallback();
    private readonly IList<ILogExpertPlugin> _pluginList = [];
    private readonly Dictionary<string, IKeywordAction> _registeredKeywordsDict = [];

    // Priority 3 & 4 Integration - Feature-flagged implementation
    private readonly IPluginLoader _pluginLoader;
    private readonly PluginCache? _pluginCache;
    private readonly IPluginEventBus _eventBus;
    private readonly List<LazyPluginProxy<ILogLineColumnizer>> _lazyColumnizers = [];

    // Feature flags - Conservative defaults
    private bool _useLazyLoading;      // Disabled by default
    private bool _usePluginCache;      // Disabled by default
    private bool _useLifecycleHooks = true;    // Enabled by default (low risk)
    private bool _useEventBus = true;          // Enabled by default (low risk)

    #endregion

    private static string _applicationConfigurationFolder = string.Empty;

    #region Events

    /// <summary>
    /// Occurs when plugin loading progress changes.
    /// </summary>
    public event EventHandler<PluginLoadProgressEventArgs>? PluginLoadProgress;

    #endregion

    #region cTor
    // Private constructor to prevent instantiation
    private PluginRegistry (string applicationConfigurationFolder, int pollingInterval)
    {
        _applicationConfigurationFolder = applicationConfigurationFolder;
        PollingInterval = pollingInterval;

        // Initialize Priority 3 & 4 components
        _pluginLoader = new DefaultPluginLoader();
        _eventBus = new PluginEventBus();

        // Load feature flags from configuration
        LoadFeatureFlags();

        // Initialize cache if enabled
        if (_usePluginCache)
        {
            _pluginCache = new PluginCache(
                cacheExpiration: TimeSpan.FromHours(24),
                loader: _pluginLoader);
            _logger.Info("Plugin cache enabled (24-hour expiration)");
        }

        if (_useLazyLoading)
        {
            _logger.Info("Lazy plugin loading enabled");
        }
    }

    public static PluginRegistry Create (string applicationConfigurationFolder, int pollingInterval)
    {
        if (_instance != null)
        {
            return _instance;
        }

        lock (_lock)
        {
            _instance = new PluginRegistry(applicationConfigurationFolder, pollingInterval);
        }

        _applicationConfigurationFolder = applicationConfigurationFolder;
        PollingInterval = pollingInterval;

        _instance.LoadPlugins();
        return Instance;
    }

    #endregion

    #region Properties

    public static PluginRegistry Instance => _instance ?? new PluginRegistry(_applicationConfigurationFolder, PollingInterval);

    public IList<ILogLineColumnizer> RegisteredColumnizers { get; private set; }

    public IList<IContextMenuEntry> RegisteredContextMenuPlugins { get; } = [];

    public IList<IKeywordAction> RegisteredKeywordActions { get; } = [];

    public IList<IFileSystemPlugin> RegisteredFileSystemPlugins { get; } = [];

    #endregion

    #region Public methods

    public static int PollingInterval { get; private set; } = 250;

    #endregion

    #region Internals

    /// <summary>
    /// Loads feature flags from configuration.
    /// </summary>
    private void LoadFeatureFlags ()
    {
        // TODO: Load from app.config or appsettings.json in future
        // For now, these are hardcoded conservative defaults

        // Conservative defaults: disable performance features, enable architectural features
        _useLazyLoading = true;      // Disabled by default (requires more testing)
        _usePluginCache = true;      // Disabled by default (requires more testing)
        _useLifecycleHooks = true;    // Enabled (backward compatible, low risk)
        _useEventBus = true;          // Enabled (fire-and-forget, safe)

        _logger.Info("Feature flags - Lazy: {Lazy}, Cache: {Cache}, Lifecycle: {Lifecycle}, EventBus: {EventBus}", _useLazyLoading, _usePluginCache, _useLifecycleHooks, _useEventBus);
    }

    /// <summary>
    /// Creates a plugin context for lifecycle initialization.
    /// </summary>
    private PluginContext CreatePluginContext (string pluginName, string pluginPath)
    {
        var pluginDir = Path.GetDirectoryName(pluginPath) ?? AppDomain.CurrentDomain.BaseDirectory;
        var configDir = Path.Combine(_applicationConfigurationFolder, "Plugins", pluginName);

        // Ensure config directory exists
        _ = Directory.CreateDirectory(configDir);

        return new PluginContext
        {
            Logger = new PluginLogger(pluginName),
            PluginDirectory = pluginDir,
            HostVersion = typeof(PluginRegistry).Assembly.GetName().Version ?? new Version(1, 0),
            ConfigurationDirectory = configDir
        };
    }

    /// <summary>
    /// Creates a lazy proxy for a plugin instead of loading immediately.
    /// </summary>
    private LazyPluginProxy<ILogLineColumnizer> CreateLazyProxy (string dllPath, PluginManifest? manifest)
    {
        var proxy = new LazyPluginProxy<ILogLineColumnizer>(dllPath, manifest);

        _logger.Debug("Created lazy proxy for: {Plugin}", manifest?.Name ?? Path.GetFileName(dllPath));

        // Publish event when proxy is created
        if (_useEventBus)
        {
            _eventBus.Publish(new PluginLoadedEvent
            {
                Source = "PluginRegistry",
                PluginName = manifest?.Name ?? Path.GetFileName(dllPath),
                PluginVersion = manifest?.Version ?? "Unknown"
            });
        }

        return proxy;
    }

    /// <summary>
    /// Processes a loaded plugin (either from cache or fresh load).
    /// </summary>
    private void ProcessLoadedPlugin (object plugin, PluginManifest? manifest, string dllPath)
    {
        if (plugin is not ILogLineColumnizer columnizer)
        {
            _logger.Warn("Loaded plugin is not ILogLineColumnizer: {Type}", plugin.GetType().Name);
            return;
        }

        // Add to registered columnizers
        RegisteredColumnizers.Add(columnizer);

        // Call lifecycle Initialize if supported
        if (_useLifecycleHooks && columnizer is IPluginLifecycle lifecycle)
        {
            try
            {
                var context = CreatePluginContext(manifest?.Name ?? Path.GetFileNameWithoutExtension(dllPath), dllPath);
                lifecycle.Initialize(context);
                _logger.Debug("Called Initialize on {Plugin}", manifest?.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Plugin Initialize failed: {Plugin}", manifest?.Name);
            }
        }

        // Existing IColumnizerConfigurator support
        if (columnizer is IColumnizerConfigurator configurator)
        {
            try
            {
                configurator.LoadConfig(_applicationConfigurationFolder);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Plugin config loading failed: {Plugin}", manifest?.Name);
            }
        }

        // Existing ILogExpertPlugin support
        if (columnizer is ILogExpertPlugin legacyPlugin)
        {
            _pluginList.Add(legacyPlugin);
            try
            {
                legacyPlugin.PluginLoaded();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Plugin PluginLoaded callback failed: {Plugin}", manifest?.Name);
            }
        }

        // Publish loaded event
        if (_useEventBus)
        {
            _eventBus.Publish(new PluginLoadedEvent
            {
                Source = "PluginRegistry",
                PluginName = manifest?.Name ?? Path.GetFileNameWithoutExtension(dllPath),
                PluginVersion = manifest?.Version ?? "Unknown"
            });
        }

        _logger.Info("Plugin processed: {Plugin}", manifest?.Name ?? Path.GetFileNameWithoutExtension(dllPath));
    }

    /// <summary>
    /// Publishes an event via the event bus (if enabled).
    /// </summary>
    private void PublishEvent<TEvent> (TEvent ev) where TEvent : IPluginEvent
    {
        if (_useEventBus)
        {
            try
            {
                _eventBus.Publish(ev);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to publish event: {EventType}", typeof(TEvent).Name);
            }
        }
    }

    /// <summary>
    /// Gets cache statistics (if caching is enabled).
    /// </summary>
    public CacheStatistics? GetCacheStatistics ()
    {
        return _usePluginCache ? _pluginCache?.GetStatistics() : null;
    }

    internal void LoadPlugins ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "Loading plugins with security validation and manifest support...");

        // Load plugin permissions from configuration
        PluginPermissionManager.LoadPermissions(_applicationConfigurationFolder);

        RegisteredColumnizers =
        [
            //TODO: Remove these plugins and load them as any other plugin
            new DefaultLogfileColumnizer(),
            new TimestampColumnizer(),
            new SquareBracketColumnizer(),
            new ClfColumnizer(),
        ];
        RegisteredFileSystemPlugins.Add(new LocalFileSystem());

        var pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        //TODO: FIXME: This is a hack for the tests to pass. Need to find a better approach
        if (!Directory.Exists(pluginDir))
        {
            _logger.Warn("Plugin directory not found: {PluginDir}. Skipping plugin loading.", pluginDir);
            pluginDir = ".";
        }

        AppDomain.CurrentDomain.AssemblyResolve += ColumnizerResolveEventHandler;

        var loadedCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        // Get list of DLL files for progress tracking
        var dllFiles = Directory.EnumerateFiles(pluginDir, "*.dll").ToList();
        var totalPlugins = dllFiles.Count;

        // Fire Started event
        OnPluginLoadProgress(new PluginLoadProgressEventArgs(
            pluginDir,
            "Plugin Loading",
            0,
            totalPlugins,
            PluginLoadStatus.Started,
            $"Starting to load {totalPlugins} potential plugin(s)"));

        var currentIndex = 0;
        foreach (var dllName in dllFiles)
        {
            var fileName = Path.GetFileName(dllName);

            try
            {
                // Fire Validating event
                OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                    dllName,
                    fileName,
                    currentIndex,
                    totalPlugins,
                    PluginLoadStatus.Validating,
                    "Validating plugin security and manifest"));

                // Validate plugin before loading (with manifest support)
                if (!PluginValidator.ValidatePlugin(dllName, out var manifest))
                {
                    skippedCount++;
                    _logger.Info("Skipped plugin (failed validation): {FileName}", fileName);

                    // Fire Skipped event
                    OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                        dllName,
                        fileName,
                        currentIndex,
                        totalPlugins,
                        PluginLoadStatus.Skipped,
                        "Failed validation (not trusted or invalid manifest)"));

                    currentIndex++;
                    continue;
                }

                // Fire Validated event
                OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                    dllName,
                    fileName,
                    currentIndex,
                    totalPlugins,
                    PluginLoadStatus.Validated,
                    manifest != null ? $"Validated: {manifest.Name} v{manifest.Version}" : "Validated successfully"));

                // Log manifest information if available
                if (manifest != null)
                {
                    _logger.Info("Plugin {PluginName} v{Version} by {Author}",
                        manifest.Name, manifest.Version, manifest.Author ?? "Unknown");
                    if (manifest.Permissions != null && manifest.Permissions.Count > 0)
                    {
                        _logger.Debug("  Permissions: {Permissions}", string.Join(", ", manifest.Permissions));
                    }
                }

                // Fire Loading event
                OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                    dllName,
                    fileName,
                    currentIndex,
                    totalPlugins,
                    PluginLoadStatus.Loading,
                    "Loading plugin assembly"));

                // Load plugin with timeout and exception handling (with manifest support)
                // LoadPluginAssemblySafe will detect and register all plugin types (ILogLineColumnizer, IFileSystemPlugin, etc.)
                if (LoadPluginAssemblySafe(dllName, manifest))
                {
                    loadedCount++;

                    // Fire Loaded event
                    OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                        dllName,
                        fileName,
                        currentIndex,
                        totalPlugins,
                        PluginLoadStatus.Loaded,
                        manifest != null ? $"Loaded {manifest.Name}" : "Loaded successfully"));
                }
                else
                {
                    failedCount++;

                    // Fire Failed event
                    OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                        dllName,
                        fileName,
                        currentIndex,
                        totalPlugins,
                        PluginLoadStatus.Failed,
                        "Failed to load plugin assembly (timeout or error)"));
                }
            }
            catch (Exception ex) when (ex is BadImageFormatException or FileLoadException)
            {
                // Can happen when a 32bit-only DLL is loaded on a 64bit system (or vice versa)
                // or could be a not columnizer DLL (e.g. A DLL that is needed by a plugin).
                _logger.Warn(ex, "Plugin load failed (bad format): {FileName}", fileName);
                failedCount++;

                // Fire Failed event
                OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                    dllName,
                    fileName,
                    currentIndex,
                    totalPlugins,
                    PluginLoadStatus.Failed,
                    $"Bad format: {ex.Message}"));
            }
            catch (ReflectionTypeLoadException ex)
            {
                // can happen when a dll dependency is missing
                if (ex.LoaderExceptions != null && ex.LoaderExceptions.Length != 0)
                {
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        _logger.Error(loaderException, "Plugin load failed with '{0}'", dllName);
                    }
                }

                _logger.Error(ex, "Loader exception during load of dll '{0}'", dllName);
                failedCount++;

                // Fire Failed event
                OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                    dllName,
                    fileName,
                    currentIndex,
                    totalPlugins,
                    PluginLoadStatus.Failed,
                    $"Dependency missing: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "General exception loading plugin: {FileName}", fileName);
                failedCount++;

                // Fire Failed event
                OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                    dllName,
                    fileName,
                    currentIndex,
                    totalPlugins,
                    PluginLoadStatus.Failed,
                    $"Error: {ex.Message}"));
            }

            currentIndex++;
        }

        _logger.Info("Plugin loading complete. Loaded: {LoadedCount}, Skipped: {SkippedCount}, Failed: {FailedCount}", loadedCount, skippedCount, failedCount);

        // Fire Completed event
        OnPluginLoadProgress(new PluginLoadProgressEventArgs(
            pluginDir,
            "Plugin Loading",
            totalPlugins,
            totalPlugins,
            PluginLoadStatus.Completed,
            $"Completed: {loadedCount} loaded, {skippedCount} skipped, {failedCount} failed"));

        // Save any permission changes
        PluginPermissionManager.SavePermissions(_applicationConfigurationFolder);
    }

    /// <summary>
    /// Raises the PluginLoadProgress event.
    /// </summary>
    /// <param name="e">Event arguments containing progress information.</param>
    protected virtual void OnPluginLoadProgress (PluginLoadProgressEventArgs e)
    {
        PluginLoadProgress?.Invoke(this, e);
    }

    /// <summary>
    /// Loads a plugin assembly with security measures: timeout protection, exception handling, and optional caching/lazy loading.
    /// </summary>
    /// <param name="dllName">Path to the plugin DLL</param>
    /// <param name="manifest">Plugin manifest (if available)</param>
    /// <returns>True if plugin loaded successfully, false otherwise</returns>
    private bool LoadPluginAssemblySafe (string dllName, PluginManifest? manifest)
    {
        try
        {
            // Option 1: Lazy Loading (defer until first use) - Only for ILogLineColumnizer
            // Check if assembly might contain columnizer plugins before creating lazy proxy
            if (_useLazyLoading)
            {
                // For lazy loading, we create a proxy without loading the assembly yet
                // The actual type checking happens when the proxy is accessed
                var proxy = CreateLazyProxy(dllName, manifest);
                _lazyColumnizers.Add(proxy);
                _logger.Info("Plugin registered for lazy loading: {Plugin}", manifest?.Name ?? Path.GetFileName(dllName));
                return true;
            }

            // Option 2: Cached Loading (use cache if available) - Only for ILogLineColumnizer
            if (_usePluginCache && _pluginCache != null)
            {
                var result = _pluginCache.LoadPluginWithCache(dllName);
                if (result.Success && result.Plugin != null)
                {
                    // Add cached plugin to registry
                    ProcessLoadedPlugin(result.Plugin, manifest, dllName);
                    return true;
                }

                _logger.Warn("Cache load failed for {Plugin}, falling back to direct load", Path.GetFileName(dllName));
            }

            // Option 3: Direct Loading - For all plugin types
            // Use timeout to prevent plugin hangs during loading
            var loadTask = Task.Run(() => LoadPluginAssembly(dllName, manifest));

            // Wait for plugin to load with timeout
            if (!loadTask.Wait(TimeSpan.FromSeconds(10)))
            {
                _logger.Error("Plugin loading timed out: {FileName}", Path.GetFileName(dllName));
                return false;
            }

            return loadTask.Result;
        }
        catch (AggregateException ex)
        {
            // Unwrap AggregateException from Task
            var innerEx = ex.InnerException ?? ex;
            _logger.Error(innerEx, "Exception during plugin load: {FileName}", Path.GetFileName(dllName));
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected exception during plugin load: {FileName}", Path.GetFileName(dllName));
            return false;
        }
    }

    private bool LoadPluginAssembly (string dllName, PluginManifest? manifest)
    {
        // Log plugin loading for audit trail
        _logger.Info("Loading plugin assembly: {FileName}", Path.GetFileName(dllName));

        var assembly = Assembly.LoadFrom(dllName);
        var types = assembly.GetTypes();
        var pluginLoadedCount = 0;

        foreach (var type in types)
        {
            _logger.Debug("Checking type {TypeName} in assembly {AssemblyName}", type.FullName, assembly.FullName);

            // Check for ILogLineColumnizer
            if (type.GetInterfaces().Any(i => i.FullName == typeof(ILogLineColumnizer).FullName))
            {
                // Instantiate plugin safely with timeout
                if (TryInstantiatePluginSafe(type, out var instance))
                {
                    // Process as ILogLineColumnizer
                    if (instance is ILogLineColumnizer columnizer)
                    {
                        ProcessLoadedPlugin(columnizer, manifest, dllName);
                        pluginLoadedCount++;
                    }
                }
            }

            // Check for other plugin types (regardless of whether ILogLineColumnizer was found)
            // A single assembly can contain multiple plugin types
            if (TryAsFileSystem(type))
            {
                pluginLoadedCount++;
            }

            if (TryAsContextMenu(type))
            {
                pluginLoadedCount++;
            }

            if (TryAsKeywordAction(type))
            {
                pluginLoadedCount++;
            }
        }

        if (pluginLoadedCount == 0)
        {
            _logger.Warn("No plugins found in assembly: {FileName}", Path.GetFileName(dllName));
        }

        return pluginLoadedCount > 0;
    }

    /// <summary>
    /// Safely instantiates a plugin with timeout protection.
    /// </summary>
    private static bool TryInstantiatePluginSafe (Type type, out object instance)
    {
        instance = null;

        try
        {
            var cti = type.GetConstructor(Type.EmptyTypes);
            if (cti == null)
            {
                _logger.Warn("Plugin type has no parameterless constructor: {TypeName}", type.Name);
                return false;
            }

            // **SECURITY**: Use timeout for plugin instantiation
            var instantiateTask = Task.Run(() => cti.Invoke([]));

            if (!instantiateTask.Wait(TimeSpan.FromSeconds(5)))
            {
                _logger.Error("Plugin instantiation timed out: {TypeName}", type.Name);
                return false;
            }

            instance = instantiateTask.Result;
            return instance != null;
        }
        catch (Exception ex) when (ex is TargetInvocationException or
                                         MethodAccessException or
                                         MemberAccessException or
                                         ArgumentException or
                                         ArgumentNullException or
                                         TargetParameterCountException or
                                         NotSupportedException or
                                         SecurityException)
        {
            _logger.Error(ex, "Failed to instantiate plugin: {TypeName}", type.Name);
            return false;
        }
    }

    public IKeywordAction FindKeywordActionPluginByName (string name)
    {
        _ = _registeredKeywordsDict.TryGetValue(name, out var action);
        return action;
    }

    public void CleanupPlugins ()
    {
        _logger.Info("Cleaning up plugins...");

        // Call legacy AppExiting
        foreach (var plugin in _pluginList)
        {
            try
            {
                plugin.AppExiting();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Plugin AppExiting failed");
            }
        }

        // Call lifecycle Shutdown
        if (_useLifecycleHooks)
        {
            foreach (var columnizer in RegisteredColumnizers)
            {
                if (columnizer is IPluginLifecycle lifecycle)
                {
                    try
                    {
                        lifecycle.Shutdown();
                        _logger.Debug("Called Shutdown on plugin");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Plugin Shutdown failed");
                    }
                }
            }
        }

        // Cleanup lazy proxies
        if (_useLazyLoading)
        {
            _lazyColumnizers.Clear();
            _logger.Debug("Cleared lazy plugin proxies");
        }

        // Cleanup cache
        if (_usePluginCache && _pluginCache != null)
        {
            var stats = _pluginCache.GetStatistics();
            _logger.Info("Cache stats at shutdown - Total: {Total}, Active: {Active}",
                stats.TotalEntries, stats.ActiveEntries);
            _pluginCache.ClearCache();
        }

        // Cleanup event bus
        if (_useEventBus)
        {
            // Event bus cleanup (subscribers will be garbage collected)
            _logger.Debug("Event bus cleanup complete");
        }

        _logger.Info("Plugin cleanup complete");
    }

    public IFileSystemPlugin FindFileSystemForUri (string uriString)
    {
        if (_logger.IsDebugEnabled)
        {
            _logger.Debug(CultureInfo.InvariantCulture, "Trying to find file system plugin for uri {0}", uriString);
        }

        foreach (var fs in RegisteredFileSystemPlugins)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(CultureInfo.InvariantCulture, "Checking {0}", fs.Text);
            }

            if (fs.CanHandleUri(uriString))
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug(CultureInfo.InvariantCulture, "Found match {0}", fs.Text);
                }

                return fs;
            }
        }

        _logger.Error("No file system plugin found for uri {0}", uriString);
        return null;
    }

    #endregion

    #region Private Methods
    //TODO: Can this be deleted?
    private bool TryAsContextMenu (Type type)
    {
        var me = TryInstantiate<IContextMenuEntry>(type);

        if (me != null)
        {
            RegisteredContextMenuPlugins.Add(me);
            if (me is ILogExpertPluginConfigurator configurator)
            {
                configurator.LoadConfig(_applicationConfigurationFolder);
            }

            if (me is ILogExpertPlugin plugin)
            {
                _pluginList.Add(plugin);
                plugin.PluginLoaded();
            }

            _logger.Info(CultureInfo.InvariantCulture, "Added context menu plugin {0}", type);
            return true;
        }

        return false;
    }

    //TODO: Can this be delted?
    private bool TryAsKeywordAction (Type type)
    {
        var ka = TryInstantiate<IKeywordAction>(type);
        if (ka != null)
        {
            RegisteredKeywordActions.Add(ka);
            _registeredKeywordsDict.Add(ka.GetName(), ka);
            if (ka is ILogExpertPluginConfigurator configurator)
            {
                configurator.LoadConfig(_applicationConfigurationFolder);
            }

            if (ka is ILogExpertPlugin plugin)
            {
                _pluginList.Add(plugin);
                plugin.PluginLoaded();
            }

            _logger.Info(CultureInfo.InvariantCulture, "Added keyword plugin {0}", type);
            return true;
        }

        return false;
    }

    //TODO: Can this be deleted?
    private bool TryAsFileSystem (Type type)
    {
        // file system plugins can have optional constructor with IFileSystemCallback argument
        var fs = TryInstantiate<IFileSystemPlugin>(type, _fileSystemCallback);
        fs ??= TryInstantiate<IFileSystemPlugin>(type);

        if (fs != null)
        {
            RegisteredFileSystemPlugins.Add(fs);
            if (fs is ILogExpertPluginConfigurator configurator)
            {
                //TODO Refactor, this should be set from outside once and not loaded all the time
                configurator.LoadConfig(_applicationConfigurationFolder);
            }

            if (fs is ILogExpertPlugin plugin)
            {
                _pluginList.Add(plugin);
                plugin.PluginLoaded();
            }

            _logger.Info(CultureInfo.InvariantCulture, "Added file system plugin {0}", type);
            return true;
        }

        return false;
    }

    private static T TryInstantiate<T> (Type loadedType) where T : class
    {
        var t = typeof(T);
        var inter = loadedType.GetInterface(t.Name);
        if (inter != null)
        {
            var cti = loadedType.GetConstructor(Type.EmptyTypes);
            if (cti != null)
            {
                var o = cti.Invoke([]);
                return o as T;
            }
        }

        return default;
    }

    private static T TryInstantiate<T> (Type loadedType, IFileSystemCallback fsCallback) where T : class
    {
        var t = typeof(T);
        var inter = loadedType.GetInterface(t.Name);
        if (inter != null)
        {
            var cti = loadedType.GetConstructor([typeof(IFileSystemCallback)]);
            if (cti != null)
            {
                var o = cti.Invoke([fsCallback]);
                return o as T;
            }
        }

        return default;
    }

    #endregion

    #region Events handler

    private static Assembly ColumnizerResolveEventHandler (object? sender, ResolveEventArgs args)
    {
        var fileName = new AssemblyName(args.Name).Name + ".dll";

        var mainDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        var pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", fileName);

        if (File.Exists(mainDir))
        {
            return Assembly.LoadFrom(mainDir);
        }

        if (File.Exists(pluginDir))
        {
            return Assembly.LoadFrom(pluginDir);
        }

        return null;
    }

    #endregion
}