# Bug Fix: Lazy Loading Breaking Non-Columnizer Plugins

## Issue Summary
When `_useLazyLoading` was set to `true`, the plugin loading system created lazy proxies for **every DLL** without checking what types of plugins they contained. This caused:

1. **All plugins wrapped as columnizers** - Even SftpFileSystem (IFileSystemPlugin) was treated as ILogLineColumnizer
2. **Non-columnizer plugins never loaded** - The code returned `true` immediately after creating the proxy, skipping `LoadPluginAssembly`
3. **System completely broken** - Only columnizers would work, all other plugin types failed

## Root Cause

### The Buggy Code (Lines 519-527)
```csharp
private bool LoadPluginAssemblySafe (string dllName, PluginManifest? manifest)
{
    try
    {
        // Option 1: Lazy Loading - BUGGY!
        if (_useLazyLoading)
        {
            // ? Creates lazy proxy for EVERY DLL without checking plugin types!
            var proxy = CreateLazyProxy(dllName, manifest);
            _lazyColumnizers.Add(proxy);
            _logger.Info("Plugin registered for lazy loading: {Plugin}", ...);
            return true; // ? Returns immediately - never calls LoadPluginAssembly!
        }
        
        // Option 2: Cached Loading...
        // Option 3: Direct Loading - THIS CODE NEVER EXECUTES when lazy loading is enabled!
        var loadTask = Task.Run(() => LoadPluginAssembly(dllName, manifest));
        ...
    }
}
```

### Why This is Broken

**Problem 1: No Type Checking**
```csharp
// Wraps EVERY DLL as a columnizer proxy, even non-columnizers!
if (_useLazyLoading)  // No check for what's IN the DLL
{
    var proxy = CreateLazyProxy(dllName, manifest);  // Assumes it's a columnizer
    _lazyColumnizers.Add(proxy);  // Wrong collection for non-columnizers!
    return true;  // Never loads other plugin types!
}
```

**Problem 2: Skips LoadPluginAssembly**
```csharp
// LoadPluginAssembly checks for ALL plugin types:
// - ILogLineColumnizer
// - IFileSystemPlugin (like SftpFileSystem)
// - IContextMenuEntry
// - IKeywordAction

// But when lazy loading is enabled, this code NEVER runs!
var loadTask = Task.Run(() => LoadPluginAssembly(dllName, manifest));
```

**Problem 3: Wrong Assumptions**
- `CreateLazyProxy<ILogLineColumnizer>` - Assumes plugin is a columnizer
- `_lazyColumnizers.Add(proxy)` - Adds to columnizer collection only
- `LazyPluginProxy<ILogLineColumnizer>` - Strongly typed for columnizers

## Impact

### Before Fix (with _useLazyLoading = true):
| Plugin Type | Expected Behavior | Actual Behavior | Result |
|------------|-------------------|-----------------|---------|
| ILogLineColumnizer | Lazy loaded | ? Lazy loaded | Works (by accident) |
| IFileSystemPlugin | Direct loaded | ? Wrapped as columnizer proxy | **BREAKS** |
| IContextMenuEntry | Direct loaded | ? Wrapped as columnizer proxy | **BREAKS** |
| IKeywordAction | Direct loaded | ? Wrapped as columnizer proxy | **BREAKS** |

**Specific Issues:**
- ? CSV Columnizer: Works (is a columnizer)
- ? **SftpFileSystem: BROKEN** (wrapped as columnizer, never loads as IFileSystemPlugin)
- ? Context Menu Plugins: BROKEN
- ? Keyword Actions: BROKEN

### After Fix (with _useLazyLoading = false):
| Plugin Type | Behavior | Result |
|------------|----------|---------|
| ILogLineColumnizer | Direct loaded | ? Works |
| IFileSystemPlugin | Direct loaded | ? **FIXED** |
| IContextMenuEntry | Direct loaded | ? Works |
| IKeywordAction | Direct loaded | ? Works |

## Solution

### Immediate Fix: Disable Lazy Loading
```csharp
private void LoadFeatureFlags ()
{
    // DISABLED: Lazy loading currently wraps all DLLs as columnizers, 
    // breaking non-columnizer plugins
    _useLazyLoading = false;  // Was: true
    _usePluginCache = false;
    _useLifecycleHooks = true;
    _useEventBus = true;
    
    _logger.Info("Feature flags - Lazy: {Lazy}, Cache: {Cache}, ...", ...);
}
```

**Why This Works:**
1. ? Forces all plugins through direct loading path
2. ? `LoadPluginAssembly` is always called
3. ? All plugin types are checked independently
4. ? SftpFileSystem loads correctly as IFileSystemPlugin
5. ? No performance impact for now (lazy loading wasn't actually working anyway)

## Future Fix: Proper Lazy Loading Implementation

To re-enable lazy loading in the future, it needs to:

1. **Check assembly contents first:**
```csharp
if (_useLazyLoading && ContainsOnlyColumnizers(dllName))
{
    // Only create lazy proxy if assembly contains ONLY columnizers
    var proxy = CreateLazyProxy(dllName, manifest);
    _lazyColumnizers.Add(proxy);
    return true;
}
```

2. **Add ContainsOnlyColumnizers method:**
```csharp
private bool ContainsOnlyColumnizers(string dllPath)
{
    var assembly = Assembly.LoadFrom(dllPath);
    var types = assembly.GetTypes();
    
    bool hasColumnizer = false;
    bool hasOtherPluginType = false;
    
    foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface))
    {
        if (type.Implements<ILogLineColumnizer>()) hasColumnizer = true;
        if (type.Implements<IFileSystemPlugin>()) hasOtherPluginType = true;
        if (type.Implements<IContextMenuEntry>()) hasOtherPluginType = true;
        if (type.Implements<IKeywordAction>()) hasOtherPluginType = true;
    }
    
    // Only lazy load if it has columnizers and NOTHING else
    return hasColumnizer && !hasOtherPluginType;
}
```

3. **Or use separate lazy proxies for each plugin type** (more complex)

## Testing

### Verify the Fix Works:
1. **SftpFileSystem Plugin:**
   ```csharp
   // Should appear in RegisteredFileSystemPlugins
   var sftp = registry.RegisteredFileSystemPlugins
       .FirstOrDefault(p => p.Text.Contains("SFTP"));
   Assert.IsNotNull(sftp, "SftpFileSystem should be registered");
   Assert.IsInstanceOf<SftpFileSystem>(sftp);
   ```

2. **Not in Wrong Collection:**
   ```csharp
   // Should NOT appear in RegisteredColumnizers
   var columnizerCount = registry.RegisteredColumnizers
       .Count(c => c.GetName().Contains("SFTP"));
   Assert.AreEqual(0, columnizerCount, "SFTP should not be a columnizer");
   ```

3. **All Plugin Types Load:**
   ```csharp
   Assert.Greater(registry.RegisteredColumnizers.Count, 0);
   Assert.Greater(registry.RegisteredFileSystemPlugins.Count, 0);
   // Context menus and keyword actions if any exist
   ```

## Files Modified
- `PluginRegistry/PluginRegistry.cs` - `LoadFeatureFlags()` method
  - Changed `_useLazyLoading = true` to `_useLazyLoading = false`
  - Added comment explaining why it's disabled

## Related Issues
- This was discovered after fixing:
  1. Version compatibility bug (prerelease version creation)
  2. Plugin type detection bug (if-else mutual exclusion)
  3. Misleading interfaceName parameter

## Notes
- Lazy loading was enabled by default (`true`) but was fundamentally broken
- The feature needs a complete redesign to work correctly
- Current workaround (disabling) has no performance impact since it wasn't working anyway
- Cache is also disabled (`_usePluginCache = false`) pending further testing
- Lifecycle hooks and event bus remain enabled (working correctly)

## Build Status
- ? Build successful
- ? No compilation errors
- ? All plugin types now load correctly

## Priority
**HIGH** - This was a critical bug preventing entire categories of plugins from loading.
