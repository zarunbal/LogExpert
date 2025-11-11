using System.Globalization;
using System.Reflection;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Entities;
using LogExpert.Core.Interface;
using LogExpert.PluginRegistry.FileSystem;

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

    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private static PluginRegistry? _instance;
    private static readonly object _lock = new();

    private readonly IFileSystemCallback _fileSystemCallback = new FileSystemCallback();
    private readonly IList<ILogExpertPlugin> _pluginList = [];
    private readonly IDictionary<string, IKeywordAction> _registeredKeywordsDict = new Dictionary<string, IKeywordAction>();

    #endregion

    private static string _applicationConfigurationFolder = string.Empty;
    private static int _pollingInterval = 250;

    #region cTor
    // Private constructor to prevent instantiation
    private PluginRegistry (string applicationConfigurationFolder, int pollingInterval)
    {
        _applicationConfigurationFolder = applicationConfigurationFolder;
        _pollingInterval = pollingInterval;
    }

    public PluginRegistry Create (string applicationConfigurationFolder, int pollingInterval)
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
        _pollingInterval = pollingInterval;

        _instance.LoadPlugins();
        return Instance;
    }

    #endregion

    #region Properties

    public static PluginRegistry Instance => _instance ?? new PluginRegistry(_applicationConfigurationFolder, _pollingInterval);

    public IList<ILogLineColumnizer> RegisteredColumnizers { get; private set; }

    public IList<IContextMenuEntry> RegisteredContextMenuPlugins { get; } = [];

    public IList<IKeywordAction> RegisteredKeywordActions { get; } = [];

    public IList<IFileSystemPlugin> RegisteredFileSystemPlugins { get; } = [];

    #endregion

    #region Public methods

    public static int PollingInterval => _pollingInterval;

    #endregion

    #region Internals

    internal void LoadPlugins ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "Loading plugins with security validation and manifest support...");

        // **NEW**: Load plugin permissions from configuration
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

        var interfaceName = typeof(ILogLineColumnizer).FullName
            ?? throw new NotImplementedException("The interface name is null. How did this happen? Let's fix this.");

        var loadedCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        foreach (var dllName in Directory.EnumerateFiles(pluginDir, "*.dll"))
        {
            try
            {
                // **SECURITY**: Validate plugin before loading (with manifest support)
                if (!PluginValidator.ValidatePlugin(dllName, out var manifest))
                {
                    skippedCount++;
                    _logger.Info("Skipped plugin (failed validation): {FileName}", Path.GetFileName(dllName));
                    continue;
                }

                // **NEW**: Log manifest information if available
                if (manifest != null)
                {
                    _logger.Info("Plugin {PluginName} v{Version} by {Author}", 
                        manifest.Name, manifest.Version, manifest.Author ?? "Unknown");
                    if (manifest.Permissions != null && manifest.Permissions.Count > 0)
                    {
                        _logger.Debug("  Permissions: {Permissions}", string.Join(", ", manifest.Permissions));
                    }
                }

                // **SECURITY**: Load plugin with timeout and exception handling
                if (LoadPluginAssemblySafe(dllName, interfaceName))
                {
                    loadedCount++;
                }
                else
                {
                    failedCount++;
                }
            }
            catch (Exception ex) when (ex is BadImageFormatException or FileLoadException)
            {
                // Can happen when a 32bit-only DLL is loaded on a 64bit system (or vice versa)
                // or could be a not columnizer DLL (e.g. A DLL that is needed by a plugin).
                _logger.Warn(ex, "Plugin load failed (bad format): {FileName}", Path.GetFileName(dllName));
                failedCount++;
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
                // Don't throw - continue loading other plugins
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "General exception loading plugin: {FileName}", Path.GetFileName(dllName));
                failedCount++;
                // Don't throw - continue loading other plugins
            }
        }

        _logger.Info("Plugin loading complete. Loaded: {LoadedCount}, Skipped: {SkippedCount}, Failed: {FailedCount}", 
            loadedCount, skippedCount, failedCount);
        
        // **NEW**: Save any permission changes
        PluginPermissionManager.SavePermissions(_applicationConfigurationFolder);
    }

    /// <summary>
    /// Loads a plugin assembly with security measures: timeout protection and exception handling.
    /// </summary>
    /// <returns>True if plugin loaded successfully, false otherwise</returns>
    private bool LoadPluginAssemblySafe(string dllName, string interfaceName)
    {
        try
        {
            // **SECURITY**: Use timeout to prevent plugin hangs during loading
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var loadTask = Task.Run(() => LoadPluginAssembly(dllName, interfaceName), cts.Token);
            
            // Wait for plugin to load with timeout
            if (!loadTask.Wait(TimeSpan.FromSeconds(10)))
            {
                _logger.Error("Plugin loading timed out: {FileName}", Path.GetFileName(dllName));
                return false;
            }

            return true;
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

    private void LoadPluginAssembly (string dllName, string interfaceName)
    {
        // **SECURITY**: Log plugin loading for audit trail
        _logger.Info("Loading plugin assembly: {FileName}", Path.GetFileName(dllName));

        var assembly = Assembly.LoadFrom(dllName);
        var types = assembly.GetTypes();
        var pluginLoadedCount = 0;

        foreach (var type in types)
        {
            _logger.Debug("Checking type {TypeName} in assembly {AssemblyName}", type.FullName, assembly.FullName);

            if (type.GetInterfaces().Any(i => i.FullName == interfaceName))
            {
                // **SECURITY**: Instantiate plugin safely with timeout
                if (TryInstantiatePluginSafe(type, out var instance))
                {
                    RegisteredColumnizers.Add((ILogLineColumnizer)instance);

                    if (instance is IColumnizerConfigurator configurator)
                    {
                        // **SECURITY**: Wrap config loading in try-catch
                        try
                        {
                            configurator.LoadConfig(_applicationConfigurationFolder);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Plugin config loading failed: {TypeName}", type.Name);
                            // Continue - don't fail entire plugin for config error
                        }
                    }

                    if (instance is ILogExpertPlugin plugin)
                    {
                        _pluginList.Add(plugin);
                        
                        // **SECURITY**: Wrap plugin initialization in try-catch
                        try
                        {
                            plugin.PluginLoaded();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Plugin initialization failed: {TypeName}", type.Name);
                            // Continue - plugin is loaded but initialization failed
                        }
                    }

                    _logger.Info("Added columnizer: {TypeName}", type.Name);
                    pluginLoadedCount++;
                }
            }
            else
            {
                if (TryAsContextMenu(type))
                {
                    pluginLoadedCount++;
                    continue;
                }

                if (TryAsKeywordAction(type))
                {
                    pluginLoadedCount++;
                    continue;
                }

                if (TryAsFileSystem(type))
                {
                    pluginLoadedCount++;
                    continue;
                }
            }
        }

        if (pluginLoadedCount == 0)
        {
            _logger.Warn("No plugins found in assembly: {FileName}", Path.GetFileName(dllName));
        }
    }

    /// <summary>
    /// Safely instantiates a plugin with timeout protection.
    /// </summary>
    private bool TryInstantiatePluginSafe(Type type, out object instance)
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var instantiateTask = Task.Run(() => cti.Invoke([]), cts.Token);
            
            if (!instantiateTask.Wait(TimeSpan.FromSeconds(5)))
            {
                _logger.Error("Plugin instantiation timed out: {TypeName}", type.Name);
                return false;
            }

            instance = instantiateTask.Result;
            return instance != null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to instantiate plugin: {TypeName}", type.Name);
            return false;
        }
    }

    public IKeywordAction FindKeywordActionPluginByName (string name)
    {
        _registeredKeywordsDict.TryGetValue(name, out var action);
        return action;
    }

    public void CleanupPlugins ()
    {
        foreach (var plugin in _pluginList)
        {
            plugin.AppExiting();
        }
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

    //TODO: Can this be delted?
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