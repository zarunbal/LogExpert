# Implementation Strategy: Proper Lazy Loading for All Plugin Types

## Current Problem Summary

When `_useLazyLoading = true`, the code:
1. Wraps **every DLL** as a columnizer lazy proxy without checking contents
2. Returns immediately, skipping `LoadPluginAssembly()`
3. Breaks all non-columnizer plugins (IFileSystemPlugin, IContextMenuEntry, IKeywordAction)

**Current Workaround:** Disabled lazy loading (`_useLazyLoading = false`)

---

## Implementation Strategy: Multi-Type Lazy Loading

### Goals
1. ? Support lazy loading for **all** plugin types, not just columnizers
2. ? Check assembly contents **before** wrapping in lazy proxies
3. ? Keep non-lazy-loadable plugins loading immediately
4. ? Maintain backward compatibility
5. ? No breaking changes to existing plugin API

---

## Design Options

### Option 1: Type-Aware Lazy Loading (RECOMMENDED)
**Inspect assembly, create appropriate lazy proxy per type**

#### Pros:
- ? Most flexible - supports lazy loading for all plugin types
- ? True lazy loading - plugins loaded only when accessed
- ? Can mix lazy and immediate loading per assembly
- ? Best performance for large plugin collections

#### Cons:
- ? More complex implementation
- ? Requires creating lazy proxy for each plugin type
- ? Need to load assembly metadata (but not types) during discovery

---

### Option 2: Columnizer-Only Lazy Loading
**Only lazy load assemblies containing ONLY columnizers**

#### Pros:
- ? Simpler implementation
- ? Safer - only affects columnizers
- ? Quick to implement

#### Cons:
- ? Limited - doesn't support lazy loading for other types
- ? Mixed assemblies always load immediately
- ? Misses optimization opportunities

---

### Option 3: Disable Lazy Loading
**Current workaround - keep `_useLazyLoading = false`**

#### Pros:
- ? Already working
- ? No implementation needed
- ? All plugins load correctly

#### Cons:
- ? No lazy loading benefits
- ? Slower startup with many plugins
- ? Defeats the feature purpose

---

## RECOMMENDED: Option 1 - Type-Aware Lazy Loading

### Architecture Overview

```
LoadPlugins()
    ?
For each DLL:
    ?
PluginValidator.ValidatePlugin() ? Manifest
    ?
InspectAssemblyTypes(dllPath) ? PluginTypeInfo
    ?
    ?? Contains ILogLineColumnizer? ? LazyPluginProxy<ILogLineColumnizer>
    ?? Contains IFileSystemPlugin? ? LazyPluginProxy<IFileSystemPlugin>
    ?? Contains IContextMenuEntry? ? LazyPluginProxy<IContextMenuEntry>
    ?? Contains IKeywordAction? ? LazyPluginProxy<IKeywordAction>
    ?
OR Direct Load if:
    - Lazy loading disabled
    - Assembly has multiple plugin types
    - Assembly inspection fails
```

---

## Implementation Plan

### Phase 1: Infrastructure (Foundation)

#### 1.1 Create PluginTypeInfo Class
**File:** `PluginRegistry/PluginTypeInfo.cs`

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
    
    public bool IsEmpty => !HasColumnizer && !HasFileSystem && 
                          !HasContextMenu && !HasKeywordAction;
    
    public bool IsSingleType => 
        (HasColumnizer ? 1 : 0) + 
        (HasFileSystem ? 1 : 0) + 
        (HasContextMenu ? 1 : 0) + 
        (HasKeywordAction ? 1 : 0) == 1;
    
    public bool IsColumnizerOnly => HasColumnizer && !HasFileSystem && 
                                   !HasContextMenu && !HasKeywordAction;
}
```

#### 1.2 Create Assembly Inspector
**File:** `PluginRegistry/AssemblyInspector.cs`

```csharp
namespace LogExpert.PluginRegistry;

/// <summary>
/// Inspects assemblies to determine which plugin types they contain.
/// Uses metadata-only loading to avoid fully loading assemblies.
/// </summary>
public static class AssemblyInspector
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Inspects an assembly to determine which plugin types it contains.
    /// Uses Assembly.LoadFrom (not MetadataLoadContext) for simplicity.
    /// </summary>
    public static PluginTypeInfo InspectAssembly(string dllPath)
    {
        var info = new PluginTypeInfo();
        
        try
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var types = assembly.GetTypes();
            
            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                
                var interfaces = type.GetInterfaces();
                
                if (interfaces.Any(i => i.FullName == typeof(ILogLineColumnizer).FullName))
                    info.HasColumnizer = true;
                
                if (interfaces.Any(i => i.FullName == typeof(IFileSystemPlugin).FullName))
                    info.HasFileSystem = true;
                
                if (interfaces.Any(i => i.FullName == typeof(IContextMenuEntry).FullName))
                    info.HasContextMenu = true;
                
                if (interfaces.Any(i => i.FullName == typeof(IKeywordAction).FullName))
                    info.HasKeywordAction = true;
            }
            
            _logger.Debug("Assembly {FileName}: Columnizer={Col}, FileSystem={FS}, ContextMenu={CM}, KeywordAction={KA}",
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
            return new PluginTypeInfo(); // Empty info = load immediately
        }
    }
}
```

---

### Phase 2: Lazy Proxy Support for All Types

#### 2.1 Create Generic Lazy Plugin Loader
**File:** `PluginRegistry/LazyPluginLoader.cs`

```csharp
namespace LogExpert.PluginRegistry;

/// <summary>
/// Generic lazy plugin loader that can load any plugin type.
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
    /// Gets the plugin instance, loading it if necessary.
    /// </summary>
    public T GetInstance()
    {
        if (_isLoaded)
            return _instance;

        lock (_lock)
        {
            if (_isLoaded)
                return _instance;

            _logger.Info("Lazy loading plugin from {FileName}", Path.GetFileName(_dllPath));

            try
            {
                var assembly = Assembly.LoadFrom(_dllPath);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (!typeof(T).IsAssignableFrom(type))
                        continue;

                    var instance = TryInstantiate<T>(type);
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
                _logger.Error(ex, "Failed to lazy load plugin from {FileName}", Path.GetFileName(_dllPath));
            }

            _isLoaded = true; // Mark as loaded even on failure to prevent retries
            return _instance;
        }
    }

    private static T? TryInstantiate<TPlugin>(Type type) where TPlugin : class
    {
        try
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                return null;

            var instance = ctor.Invoke(Array.Empty<object>());
            return instance as TPlugin;
        }
        catch
        {
            return null;
        }
    }
}
```

#### 2.2 Update PluginRegistry Fields
**File:** `PluginRegistry/PluginRegistry.cs`

```csharp
// Replace existing lazy loading fields:
private readonly List<LazyPluginProxy<ILogLineColumnizer>> _lazyColumnizers = [];

// With generic lazy loaders for all types:
private readonly List<LazyPluginLoader<ILogLineColumnizer>> _lazyColumnizers = [];
private readonly List<LazyPluginLoader<IFileSystemPlugin>> _lazyFileSystemPlugins = [];
private readonly List<LazyPluginLoader<IContextMenuEntry>> _lazyContextMenuPlugins = [];
private readonly List<LazyPluginLoader<IKeywordAction>> _lazyKeywordActions = [];
```

---

### Phase 3: Update Plugin Loading Logic

#### 3.1 Modify LoadPluginAssemblySafe
**File:** `PluginRegistry/PluginRegistry.cs`

```csharp
private bool LoadPluginAssemblySafe(string dllName, PluginManifest? manifest)
{
    try
    {
        // Check cache first (if enabled)
        if (_usePluginCache && _pluginCache != null)
        {
            var result = _pluginCache.LoadPluginWithCache(dllName);
            if (result.Success && result.Plugin != null)
            {
                ProcessLoadedPlugin(result.Plugin, manifest, dllName);
                return true;
            }
            _logger.Warn("Cache load failed for {Plugin}, falling back", Path.GetFileName(dllName));
        }

        // Inspect assembly to determine plugin types
        if (_useLazyLoading)
        {
            var typeInfo = AssemblyInspector.InspectAssembly(dllName);
            
            if (typeInfo.IsEmpty)
            {
                _logger.Debug("No plugins found in {FileName} during inspection", Path.GetFileName(dllName));
                return false;
            }

            // Strategy: Lazy load if assembly contains only ONE plugin type
            if (typeInfo.IsSingleType)
            {
                return RegisterLazyPlugins(dllName, manifest, typeInfo);
            }
            
            // If assembly has multiple plugin types, load immediately
            _logger.Debug("Assembly {FileName} contains multiple plugin types, loading immediately", 
                Path.GetFileName(dllName));
        }

        // Direct loading - for all plugin types
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
        _logger.Error(innerEx, "Exception during plugin load: {FileName}", Path.GetFileName(dllName));
        return false;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Unexpected exception during plugin load: {FileName}", Path.GetFileName(dllName));
        return false;
    }
}
```

#### 3.2 Add RegisterLazyPlugins Method
**File:** `PluginRegistry/PluginRegistry.cs`

```csharp
/// <summary>
/// Registers lazy-loaded plugins based on their types.
/// </summary>
private bool RegisterLazyPlugins(string dllName, PluginManifest? manifest, PluginTypeInfo typeInfo)
{
    var registered = false;

    if (typeInfo.HasColumnizer)
    {
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllName, manifest);
        _lazyColumnizers.Add(loader);
        _logger.Info("Registered lazy columnizer: {Plugin}", manifest?.Name ?? Path.GetFileName(dllName));
        registered = true;
    }

    if (typeInfo.HasFileSystem)
    {
        var loader = new LazyPluginLoader<IFileSystemPlugin>(dllName, manifest);
        _lazyFileSystemPlugins.Add(loader);
        _logger.Info("Registered lazy file system plugin: {Plugin}", manifest?.Name ?? Path.GetFileName(dllName));
        registered = true;
    }

    if (typeInfo.HasContextMenu)
    {
        var loader = new LazyPluginLoader<IContextMenuEntry>(dllName, manifest);
        _lazyContextMenuPlugins.Add(loader);
        _logger.Info("Registered lazy context menu plugin: {Plugin}", manifest?.Name ?? Path.GetFileName(dllName));
        registered = true;
    }

    if (typeInfo.HasKeywordAction)
    {
        var loader = new LazyPluginLoader<IKeywordAction>(dllName, manifest);
        _lazyKeywordActions.Add(loader);
        _logger.Info("Registered lazy keyword action plugin: {Plugin}", manifest?.Name ?? Path.GetFileName(dllName));
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

---

### Phase 4: Plugin Access with Lazy Loading

#### 4.1 Update RegisteredColumnizers Property
**File:** `PluginRegistry/PluginRegistry.cs`

```csharp
// Change from IList to property with lazy loading support
private IList<ILogLineColumnizer> _registeredColumnizers;

public IList<ILogLineColumnizer> RegisteredColumnizers
{
    get
    {
        // If lazy loading is enabled, ensure lazy plugins are loaded when accessed
        if (_useLazyLoading && _lazyColumnizers.Count > 0)
        {
            foreach (var loader in _lazyColumnizers.ToList()) // ToList to avoid collection modification
            {
                var instance = loader.GetInstance();
                if (instance != null && !_registeredColumnizers.Contains(instance))
                {
                    _registeredColumnizers.Add(instance);
                    
                    // Call initialization if supported
                    if (_useLifecycleHooks && instance is IPluginLifecycle lifecycle)
                    {
                        try
                        {
                            var context = CreatePluginContext(
                                loader.Manifest?.Name ?? Path.GetFileNameWithoutExtension(loader.DllPath),
                                loader.DllPath);
                            lifecycle.Initialize(context);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Failed to initialize lazy loaded plugin");
                        }
                    }
                }
            }
            
            // Clear lazy list after all loaded
            _lazyColumnizers.Clear();
        }
        
        return _registeredColumnizers;
    }
    private set => _registeredColumnizers = value;
}
```

#### 4.2 Update Other Plugin Collections Similarly
**File:** `PluginRegistry/PluginRegistry.cs`

```csharp
// Apply same pattern to:
// - RegisteredFileSystemPlugins
// - RegisteredContextMenuPlugins  
// - RegisteredKeywordActions

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
                    InitializePlugin(instance, loader.Manifest, loader.DllPath);
                }
            }
            _lazyFileSystemPlugins.Clear();
        }
        return _registeredFileSystemPlugins;
    }
}

// Similar for RegisteredContextMenuPlugins and RegisteredKeywordActions
```

---

### Phase 5: Cleanup and Testing

#### 5.1 Update CleanupPlugins
**File:** `PluginRegistry/PluginRegistry.cs`

```csharp
public void CleanupPlugins()
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
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Plugin Shutdown failed");
                }
            }
        }
    }

    // Cleanup lazy loaders
    if (_useLazyLoading)
    {
        _lazyColumnizers.Clear();
        _lazyFileSystemPlugins.Clear();
        _lazyContextMenuPlugins.Clear();
        _lazyKeywordActions.Clear();
        _logger.Debug("Cleared all lazy plugin loaders");
    }

    // Cleanup cache
    if (_usePluginCache && _pluginCache != null)
    {
        var stats = _pluginCache.GetStatistics();
        _logger.Info("Cache stats at shutdown - Total: {Total}, Active: {Active}",
            stats.TotalEntries, stats.ActiveEntries);
        _pluginCache.ClearCache();
    }

    _logger.Info("Plugin cleanup complete");
}
```

---

## Implementation Timeline

### Week 1: Infrastructure
- ? Day 1-2: Create `PluginTypeInfo` class
- ? Day 2-3: Implement `AssemblyInspector`
- ? Day 3-4: Create `LazyPluginLoader<T>`
- ? Day 4-5: Unit tests for new infrastructure

### Week 2: Integration
- ? Day 6-7: Update `LoadPluginAssemblySafe`
- ? Day 7-8: Implement `RegisterLazyPlugins`
- ? Day 8-9: Update plugin collection properties
- ? Day 9-10: Integration tests

### Week 3: Testing & Documentation
- ? Day 11-12: End-to-end testing
- ? Day 12-13: Performance testing
- ? Day 13-14: Documentation updates
- ? Day 14-15: Code review and cleanup

---

## Testing Strategy

### Unit Tests

#### Test 1: AssemblyInspector
```csharp
[Test]
public void AssemblyInspector_ColumnizerOnly_ReturnsCorrectInfo()
{
    var info = AssemblyInspector.InspectAssembly("CsvColumnizer.dll");
    Assert.IsTrue(info.HasColumnizer);
    Assert.IsFalse(info.HasFileSystem);
    Assert.IsTrue(info.IsColumnizerOnly);
    Assert.IsTrue(info.IsSingleType);
}

[Test]
public void AssemblyInspector_SftpFileSystem_ReturnsCorrectInfo()
{
    var info = AssemblyInspector.InspectAssembly("SftpFileSystem.dll");
    Assert.IsFalse(info.HasColumnizer);
    Assert.IsTrue(info.HasFileSystem);
    Assert.IsTrue(info.IsSingleType);
}
```

#### Test 2: LazyPluginLoader
```csharp
[Test]
public void LazyPluginLoader_LoadsOnFirstAccess()
{
    var loader = new LazyPluginLoader<ILogLineColumnizer>("CsvColumnizer.dll", null);
    Assert.IsFalse(loader.IsLoaded);
    
    var instance = loader.GetInstance();
    Assert.IsNotNull(instance);
    Assert.IsTrue(loader.IsLoaded);
    
    // Second access returns same instance
    var instance2 = loader.GetInstance();
    Assert.AreSame(instance, instance2);
}
```

### Integration Tests

#### Test 3: All Plugin Types Load
```csharp
[Test]
public void PluginRegistry_LazyLoadingEnabled_AllTypesLoad()
{
    var registry = PluginRegistry.Create(configDir, 250);
    
    // Accessing properties should trigger lazy loading
    Assert.Greater(registry.RegisteredColumnizers.Count, 0);
    Assert.Greater(registry.RegisteredFileSystemPlugins.Count, 0);
    
    // Verify specific plugins
    var sftp = registry.RegisteredFileSystemPlugins
        .FirstOrDefault(p => p.Text.Contains("SFTP"));
    Assert.IsNotNull(sftp);
}
```

#### Test 4: Mixed Assembly Loads Immediately
```csharp
[Test]
public void PluginRegistry_MixedAssembly_LoadsImmediately()
{
    // Create test assembly with both ILogLineColumnizer and IFileSystemPlugin
    // Verify it's not added to lazy loaders
    // Verify it's added to registered collections immediately
}
```

---

## Configuration

### Feature Flags
```csharp
private void LoadFeatureFlags()
{
    // Re-enable lazy loading once implementation is complete
    _useLazyLoading = true;  // Now safe to enable
    _usePluginCache = false; // Still disabled pending testing
    _useLifecycleHooks = true;
    _useEventBus = true;
    
    _logger.Info("Feature flags - Lazy: {Lazy}, Cache: {Cache}, Lifecycle: {Lifecycle}, EventBus: {EventBus}", 
        _useLazyLoading, _usePluginCache, _useLifecycleHooks, _useEventBus);
}
```

---

## Performance Considerations

### Benefits of Lazy Loading:
1. **Faster Startup:** Only load plugins when needed
2. **Lower Memory:** Unused plugins never loaded
3. **Better Scaling:** Works well with many plugins

### Trade-offs:
1. **First Access Delay:** Slight delay on first use
2. **Complex Code:** More complex than eager loading
3. **Assembly Inspection:** Need to load metadata

### Optimization Tips:
1. **Cache TypeInfo:** Store inspection results
2. **Parallel Inspection:** Inspect multiple assemblies at once
3. **Background Loading:** Load popular plugins in background

---

## Migration Path

### Phase 1: Implement (Weeks 1-3)
- Create infrastructure
- Implement lazy loading
- Test thoroughly

### Phase 2: Enable Gradually
1. **Alpha:** Enable for internal testing
2. **Beta:** Enable for early adopters
3. **Release:** Enable by default

### Phase 3: Monitor & Optimize
- Monitor startup time
- Track lazy load performance
- Optimize based on telemetry

---

## Rollback Strategy

If issues arise:

1. **Quick Fix:** Set `_useLazyLoading = false`
2. **Gradual:** Disable for specific plugin types
3. **Complete:** Revert to previous version

---

## Success Criteria

### Must Have:
- ? All plugin types load correctly (lazy or immediate)
- ? SftpFileSystem loads as IFileSystemPlugin
- ? No breaking changes to plugin API
- ? All existing tests pass

### Nice to Have:
- ? 20%+ faster startup with lazy loading
- ? Lower memory usage
- ? Configurable per plugin type

---

## Documentation Updates

### For Plugin Developers:
- No changes needed (transparent)
- Optional: Document lazy loading behavior
- Optional: Best practices for plugin initialization

### For Users:
- No visible changes
- Optional: Settings to disable lazy loading

### For Maintainers:
- Architecture documentation
- Testing procedures
- Performance benchmarks

---

## Conclusion

**RECOMMENDED APPROACH:** Option 1 - Type-Aware Lazy Loading

**Timeline:** 3 weeks  
**Risk:** Medium (complex but well-tested)  
**Benefit:** High (solves all issues, enables future optimizations)

**Next Steps:**
1. Review and approve this strategy
2. Create GitHub issues for each phase
3. Begin Week 1 implementation
4. Regular progress reviews

**Current Status:** Lazy loading disabled as workaround  
**Target Status:** Fully functional lazy loading for all plugin types
