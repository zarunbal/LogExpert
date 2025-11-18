# Lazy Loading Implementation - Code Changes Summary

## Files to Create

### 1. PluginRegistry/PluginTypeInfo.cs (NEW FILE)
```csharp
namespace LogExpert.PluginRegistry;

/// <summary>
/// Information about plugin types contained in an assembly.
/// </summary>
public class PluginTypeInfo
{
    public bool HasColumnizer { get; set; }
    public bool HasFileSystem { get; set; }
    public bool HasContextMenu { get; set; }
    public bool HasKeywordAction { get; set; }
    
    /// <summary>
    /// Returns true if no plugin types were found.
    /// </summary>
    public bool IsEmpty => !HasColumnizer && !HasFileSystem && 
                          !HasContextMenu && !HasKeywordAction;
    
    /// <summary>
    /// Returns true if exactly one plugin type was found.
    /// </summary>
    public bool IsSingleType => 
        (HasColumnizer ? 1 : 0) + 
        (HasFileSystem ? 1 : 0) + 
        (HasContextMenu ? 1 : 0) + 
        (HasKeywordAction ? 1 : 0) == 1;
    
    /// <summary>
    /// Returns true if only columnizer plugins were found.
    /// </summary>
    public bool IsColumnizerOnly => HasColumnizer && !HasFileSystem && 
                                   !HasContextMenu && !HasKeywordAction;
}
```

---

### 2. PluginRegistry/AssemblyInspector.cs (NEW FILE)
```csharp
using System.Reflection;
using LogExpert.Core.Interface;
using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Inspects assemblies to determine which plugin types they contain.
/// </summary>
public static class AssemblyInspector
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Inspects an assembly to determine which plugin types it contains.
    /// </summary>
    /// <param name="dllPath">Path to the DLL to inspect</param>
    /// <returns>Information about plugin types in the assembly</returns>
    public static PluginTypeInfo InspectAssembly(string dllPath)
    {
        var info = new PluginTypeInfo();
        
        try
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var types = assembly.GetTypes();
            
            foreach (var type in types)
            {
                // Skip abstract classes and interfaces
                if (type.IsAbstract || type.IsInterface)
                    continue;
                
                var interfaces = type.GetInterfaces();
                
                // Check for each plugin interface type
                if (interfaces.Any(i => i.FullName == typeof(ILogLineColumnizer).FullName))
                    info.HasColumnizer = true;
                
                if (interfaces.Any(i => i.FullName == typeof(IFileSystemPlugin).FullName))
                    info.HasFileSystem = true;
                
                if (interfaces.Any(i => i.FullName == typeof(IContextMenuEntry).FullName))
                    info.HasContextMenu = true;
                
                if (interfaces.Any(i => i.FullName == typeof(IKeywordAction).FullName))
                    info.HasKeywordAction = true;
            }
            
            _logger.Debug("Inspected {FileName}: Columnizer={Col}, FileSystem={FS}, ContextMenu={CM}, KeywordAction={KA}",
                Path.GetFileName(dllPath), 
                info.HasColumnizer, 
                info.HasFileSystem, 
                info.HasContextMenu, 
                info.HasKeywordAction);
            
            return info;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to inspect assembly: {FileName}", Path.GetFileName(dllPath));
            // Return empty info - will trigger direct loading as fallback
            return new PluginTypeInfo();
        }
    }
}
```

---

### 3. PluginRegistry/LazyPluginLoader.cs (NEW FILE)
```csharp
using System.Reflection;
using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Generic lazy plugin loader that defers loading until first access.
/// Thread-safe singleton pattern for plugin instances.
/// </summary>
public class LazyPluginLoader<T> where T : class
{
    private readonly string _dllPath;
    private readonly PluginManifest? _manifest;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private T? _instance;
    private bool _isLoaded;
    private readonly object _lock = new();

    public LazyPluginLoader(string dllPath, PluginManifest? manifest)
    {
        _dllPath = dllPath;
        _manifest = manifest;
    }

    public string DllPath => _dllPath;
    public PluginManifest? Manifest => _manifest;
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Gets the plugin instance, loading it on first access.
    /// Thread-safe - multiple calls return the same instance.
    /// </summary>
    public T? GetInstance()
    {
        if (_isLoaded)
            return _instance;

        lock (_lock)
        {
            if (_isLoaded)
                return _instance;

            _logger.Info("Lazy loading {PluginType} from {FileName}", 
                typeof(T).Name, Path.GetFileName(_dllPath));

            try
            {
                var assembly = Assembly.LoadFrom(_dllPath);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    // Check if type implements T
                    if (!typeof(T).IsAssignableFrom(type))
                        continue;

                    // Try to instantiate
                    var instance = TryInstantiate(type);
                    if (instance != null)
                    {
                        _instance = instance;
                        _isLoaded = true;
                        _logger.Info("Successfully lazy loaded: {TypeName}", type.Name);
                        return _instance;
                    }
                }

                _logger.Warn("No compatible type found in {FileName} for {InterfaceType}",
                    Path.GetFileName(_dllPath), typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to lazy load plugin from {FileName}", 
                    Path.GetFileName(_dllPath));
            }

            _isLoaded = true; // Mark as loaded even on failure to prevent retries
            return _instance;
        }
    }

    private T? TryInstantiate(Type type)
    {
        try
        {
            // Try parameterless constructor
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
            {
                var instance = ctor.Invoke(Array.Empty<object>());
                return instance as T;
            }

            _logger.Warn("Type {TypeName} has no parameterless constructor", type.Name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to instantiate {TypeName}", type.Name);
            return null;
        }
    }
}
```

---

## Files to Modify

### 4. PluginRegistry/PluginRegistry.cs (MODIFY)

#### Change 1: Add Lazy Loader Collections
```csharp
// REPLACE:
private readonly List<LazyPluginProxy<ILogLineColumnizer>> _lazyColumnizers = [];

// WITH:
private readonly List<LazyPluginLoader<ILogLineColumnizer>> _lazyColumnizers = [];
private readonly List<LazyPluginLoader<IFileSystemPlugin>> _lazyFileSystemPlugins = [];
private readonly List<LazyPluginLoader<IContextMenuEntry>> _lazyContextMenuPlugins = [];
private readonly List<LazyPluginLoader<IKeywordAction>> _lazyKeywordActions = [];
```

#### Change 2: Update LoadPluginAssemblySafe Method
```csharp
// REPLACE the entire LoadPluginAssemblySafe method with:
private bool LoadPluginAssemblySafe(string dllName, PluginManifest? manifest)
{
    try
    {
        // Option 1: Cached Loading (if enabled)
        if (_usePluginCache && _pluginCache != null)
        {
            var result = _pluginCache.LoadPluginWithCache(dllName);
            if (result.Success && result.Plugin != null)
            {
                ProcessLoadedPlugin(result.Plugin, manifest, dllName);
                return true;
            }
            _logger.Warn("Cache load failed for {Plugin}, falling back to direct load", 
                Path.GetFileName(dllName));
        }

        // Option 2: Lazy Loading (if enabled)
        if (_useLazyLoading)
        {
            // Inspect assembly to determine plugin types
            var typeInfo = AssemblyInspector.InspectAssembly(dllName);
            
            if (typeInfo.IsEmpty)
            {
                _logger.Debug("No plugins found in {FileName} during inspection", 
                    Path.GetFileName(dllName));
                return false;
            }

            // Strategy: Lazy load if assembly contains only ONE plugin type
            // This avoids the issue of mixed assemblies where one type might
            // be accessed before another
            if (typeInfo.IsSingleType)
            {
                return RegisterLazyPlugins(dllName, manifest, typeInfo);
            }
            
            // If assembly has multiple plugin types, load immediately
            _logger.Debug("Assembly {FileName} contains multiple plugin types, loading immediately", 
                Path.GetFileName(dllName));
        }

        // Option 3: Direct Loading - for all plugin types
        var loadTask = Task.Run(() => LoadPluginAssembly(dllName, manifest));

        if (!loadTask.Wait(TimeSpan.FromSeconds(10)))
        {
            _logger.Error("Plugin loading timed out: {FileName}", Path.GetFileName(dllName));
            return false;
        }

        return loadTask.Result;
    }
    catch (AggregateException ex)
    {
        var innerEx = ex.InnerException ?? ex;
        _logger.Error(innerEx, "Exception during plugin load: {FileName}", 
            Path.GetFileName(dllName));
        return false;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Unexpected exception during plugin load: {FileName}", 
            Path.GetFileName(dllName));
        return false;
    }
}
```

#### Change 3: Add RegisterLazyPlugins Method
```csharp
// ADD this new method to PluginRegistry class:
/// <summary>
/// Registers lazy-loaded plugins based on their types.
/// Creates appropriate LazyPluginLoader for each plugin type found.
/// </summary>
private bool RegisterLazyPlugins(string dllName, PluginManifest? manifest, PluginTypeInfo typeInfo)
{
    var registered = false;

    if (typeInfo.HasColumnizer)
    {
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllName, manifest);
        _lazyColumnizers.Add(loader);
        _logger.Info("Registered lazy columnizer: {Plugin}", 
            manifest?.Name ?? Path.GetFileName(dllName));
        registered = true;
    }

    if (typeInfo.HasFileSystem)
    {
        var loader = new LazyPluginLoader<IFileSystemPlugin>(dllName, manifest);
        _lazyFileSystemPlugins.Add(loader);
        _logger.Info("Registered lazy file system plugin: {Plugin}", 
            manifest?.Name ?? Path.GetFileName(dllName));
        registered = true;
    }

    if (typeInfo.HasContextMenu)
    {
        var loader = new LazyPluginLoader<IContextMenuEntry>(dllName, manifest);
        _lazyContextMenuPlugins.Add(loader);
        _logger.Info("Registered lazy context menu plugin: {Plugin}", 
            manifest?.Name ?? Path.GetFileName(dllName));
        registered = true;
    }

    if (typeInfo.HasKeywordAction)
    {
        var loader = new LazyPluginLoader<IKeywordAction>(dllName, manifest);
        _lazyKeywordActions.Add(loader);
        _logger.Info("Registered lazy keyword action plugin: {Plugin}", 
            manifest?.Name ?? Path.GetFileName(dllName));
        registered = true;
    }

    // Publish event for each registered lazy plugin
    if (registered && _useEventBus)
    {
        _eventBus.Publish(new PluginLoadedEvent
        {
            Source = "PluginRegistry",
            PluginName = manifest?.Name ?? Path.GetFileName(dllName),
            PluginVersion = manifest?.Version ?? "Unknown"
        });
    }

    return registered;
}
```

#### Change 4: Remove CreateLazyProxy Method
```csharp
// DELETE this method - no longer needed:
private LazyPluginProxy<ILogLineColumnizer> CreateLazyProxy(string dllPath, PluginManifest? manifest)
{
    // DELETE entire method body
}
```

#### Change 5: Update RegisteredColumnizers Property
```csharp
// REPLACE:
public IList<ILogLineColumnizer> RegisteredColumnizers { get; private set; }

// WITH:
private IList<ILogLineColumnizer> _registeredColumnizers;

public IList<ILogLineColumnizer> RegisteredColumnizers
{
    get
    {
        // Trigger lazy loading on first access
        if (_useLazyLoading && _lazyColumnizers.Count > 0)
        {
            foreach (var loader in _lazyColumnizers.ToList())
            {
                var instance = loader.GetInstance();
                if (instance != null && !_registeredColumnizers.Contains(instance))
                {
                    _registeredColumnizers.Add(instance);
                    InitializePluginIfNeeded(instance, loader.Manifest, loader.DllPath);
                }
            }
            _lazyColumnizers.Clear();
        }
        
        return _registeredColumnizers;
    }
    private set => _registeredColumnizers = value;
}
```

#### Change 6: Add InitializePluginIfNeeded Helper
```csharp
// ADD this new helper method:
/// <summary>
/// Initializes a plugin if it supports lifecycle hooks.
/// </summary>
private void InitializePluginIfNeeded(object plugin, PluginManifest? manifest, string dllPath)
{
    // Call lifecycle Initialize if supported
    if (_useLifecycleHooks && plugin is IPluginLifecycle lifecycle)
    {
        try
        {
            var context = CreatePluginContext(
                manifest?.Name ?? Path.GetFileNameWithoutExtension(dllPath),
                dllPath);
            lifecycle.Initialize(context);
            _logger.Debug("Initialized lazy-loaded plugin: {Plugin}", manifest?.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize lazy-loaded plugin");
        }
    }

    // Call IColumnizerConfigurator.LoadConfig if supported
    if (plugin is IColumnizerConfigurator configurator)
    {
        try
        {
            configurator.LoadConfig(_applicationConfigurationFolder);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load config for lazy-loaded plugin");
        }
    }

    // Call ILogExpertPlugin.PluginLoaded if supported
    if (plugin is ILogExpertPlugin legacyPlugin)
    {
        _pluginList.Add(legacyPlugin);
        try
        {
            legacyPlugin.PluginLoaded();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to call PluginLoaded on lazy-loaded plugin");
        }
    }
}
```

#### Change 7: Update Other Plugin Collection Properties
```csharp
// ADD similar pattern for other plugin types:

// RegisteredFileSystemPlugins
private IList<IFileSystemPlugin> _registeredFileSystemPlugins = [];

public IList<IFileSystemPlugin> RegisteredFileSystemPlugins
{
    get
    {
        if (_useLazyLoading && _lazyFileSystemPlugins.Count > 0)
        {
            foreach (var loader in _lazyFileSystemPlugins.ToList())
            {
                var instance = loader.GetInstance();
                if (instance != null && !_registeredFileSystemPlugins.Contains(instance))
                {
                    _registeredFileSystemPlugins.Add(instance);
                    InitializePluginIfNeeded(instance, loader.Manifest, loader.DllPath);
                }
            }
            _lazyFileSystemPlugins.Clear();
        }
        return _registeredFileSystemPlugins;
    }
}

// RegisteredContextMenuPlugins
private IList<IContextMenuEntry> _registeredContextMenuPlugins = [];

public IList<IContextMenuEntry> RegisteredContextMenuPlugins
{
    get
    {
        if (_useLazyLoading && _lazyContextMenuPlugins.Count > 0)
        {
            foreach (var loader in _lazyContextMenuPlugins.ToList())
            {
                var instance = loader.GetInstance();
                if (instance != null && !_registeredContextMenuPlugins.Contains(instance))
                {
                    _registeredContextMenuPlugins.Add(instance);
                    InitializePluginIfNeeded(instance, loader.Manifest, loader.DllPath);
                }
            }
            _lazyContextMenuPlugins.Clear();
        }
        return _registeredContextMenuPlugins;
    }
}

// RegisteredKeywordActions
private IList<IKeywordAction> _registeredKeywordActions = [];

public IList<IKeywordAction> RegisteredKeywordActions
{
    get
    {
        if (_useLazyLoading && _lazyKeywordActions.Count > 0)
        {
            foreach (var loader in _lazyKeywordActions.ToList())
            {
                var instance = loader.GetInstance();
                if (instance != null && !_registeredKeywordActions.Contains(instance))
                {
                    _registeredKeywordActions.Add(instance);
                    InitializePluginIfNeeded(instance, loader.Manifest, loader.DllPath);
                }
            }
            _lazyKeywordActions.Clear();
        }
        return _registeredKeywordActions;
    }
}
```

#### Change 8: Update CleanupPlugins Method
```csharp
// MODIFY CleanupPlugins to include lazy loader cleanup:
public void CleanupPlugins()
{
    _logger.Info("Cleaning up plugins...");

    // ...existing cleanup code...

    // ADD: Cleanup lazy loaders
    if (_useLazyLoading)
    {
        _lazyColumnizers.Clear();
        _lazyFileSystemPlugins.Clear();
        _lazyContextMenuPlugins.Clear();
        _lazyKeywordActions.Clear();
        _logger.Debug("Cleared all lazy plugin loaders");
    }

    // ...rest of cleanup code...
}
```

#### Change 9: Re-Enable Lazy Loading
```csharp
// IN LoadFeatureFlags() method, CHANGE:
_useLazyLoading = false;  // Currently disabled

// TO:
_useLazyLoading = true;  // Re-enabled with proper implementation
```

---

## Summary of Changes

### New Files (3)
1. ? `PluginRegistry/PluginTypeInfo.cs` - Data class for plugin type information
2. ? `PluginRegistry/AssemblyInspector.cs` - Inspects assemblies for plugin types
3. ? `PluginRegistry/LazyPluginLoader.cs` - Generic lazy loader

### Modified Files (1)
4. ? `PluginRegistry/PluginRegistry.cs` - Multiple changes:
   - Add lazy loader collections (all types)
   - Replace `LoadPluginAssemblySafe` implementation
   - Add `RegisterLazyPlugins` method
   - Remove `CreateLazyProxy` method
   - Convert properties to lazy-loading getters
   - Add `InitializePluginIfNeeded` helper
   - Update `CleanupPlugins`
   - Re-enable `_useLazyLoading`

---

## Testing After Implementation

### Unit Tests to Add
```csharp
[Test]
public void AssemblyInspector_ColumnizerOnly_Detected()
{
    var info = AssemblyInspector.InspectAssembly("CsvColumnizer.dll");
    Assert.IsTrue(info.IsColumnizerOnly);
}

[Test]
public void LazyPluginLoader_LoadsOnFirstAccess()
{
    var loader = new LazyPluginLoader<ILogLineColumnizer>("CsvColumnizer.dll", null);
    Assert.IsFalse(loader.IsLoaded);
    
    var instance = loader.GetInstance();
    Assert.IsTrue(loader.IsLoaded);
    Assert.IsNotNull(instance);
}

[Test]
public void PluginRegistry_LazyLoading_AllTypesWork()
{
    var registry = PluginRegistry.Create(configDir, 250);
    
    // Access should trigger lazy loading
    Assert.Greater(registry.RegisteredColumnizers.Count, 0);
    Assert.Greater(registry.RegisteredFileSystemPlugins.Count, 0);
}
```

---

## Build & Deploy Checklist

- [ ] Create 3 new files
- [ ] Modify PluginRegistry.cs (9 changes)
- [ ] Build solution - verify no errors
- [ ] Run unit tests - all pass
- [ ] Run integration tests - all pass
- [ ] Test with real plugins
- [ ] Verify SftpFileSystem loads correctly
- [ ] Check startup performance
- [ ] Update documentation
- [ ] Code review
- [ ] Merge to main branch

---

## Rollback Plan

If issues arise after deployment:

1. **Quick Fix:** Change `_useLazyLoading = true` back to `false`
2. **Partial Fix:** Disable specific plugin type lazy loading
3. **Full Rollback:** Revert all changes via git

---

## Estimated Implementation Time

- **New Files:** 2 hours
- **Modify PluginRegistry:** 4 hours
- **Unit Tests:** 2 hours
- **Integration Tests:** 2 hours
- **Testing & Debug:** 2 hours
- **Documentation:** 1 hour

**Total:** ~13 hours (2 days)
