# Bug Fix: SftpFileSystem Plugin Loaded as ILogLineColumnizer Instead of IFileSystemPlugin

## Issue Summary
The SftpFileSystem plugin (and potentially other non-columnizer plugins) were being incorrectly handled by the plugin loading system. The plugin loading logic was only checking for other plugin types (IFileSystemPlugin, IContextMenuEntry, IKeywordAction) in the `else` branch when a type did NOT implement `ILogLineColumnizer`, which meant these plugins were being skipped or incorrectly categorized.

## Root Cause

### Before Fix - Incorrect Logic
```csharp
foreach (var type in types)
{
    if (type.GetInterfaces().Any(i => i.FullName == interfaceName))
    {
        // Only process ILogLineColumnizer here
        if (TryInstantiatePluginSafe(type, out var instance))
        {
            if (instance is ILogLineColumnizer columnizer)
            {
                ProcessLoadedPlugin(columnizer, manifest, dllName);
                pluginLoadedCount++;
            }
        }
    }
    else  // ? PROBLEM: Only check other types if NOT ILogLineColumnizer
    {
        if (TryAsContextMenu(type))
        {
            pluginLoadedCount++;
            continue;
        }
        // ... other checks
    }
}
```

**The Problem:**
1. `LoadPlugins()` only searches for `ILogLineColumnizer` interface
2. `LoadPluginAssembly()` receives `interfaceName = "ILogLineColumnizer"`
3. The main `if` block only processes `ILogLineColumnizer`
4. The `else` block checks for other plugin types
5. **Result:** Plugins like `SftpFileSystem` that implement `IFileSystemPlugin` but NOT `ILogLineColumnizer` were only checked in the `else` branch
6. But since the logic was looking specifically for `ILogLineColumnizer`, non-columnizer plugins in the `else` branch would be processed, but the overall architecture assumed every plugin validated is a columnizer

## Solution

Changed the logic to check for **all plugin types independently**, not in an if-else pattern:

```csharp
foreach (var type in types)
{
    // Check for ILogLineColumnizer (primary interface being searched)
    if (type.GetInterfaces().Any(i => i.FullName == interfaceName))
    {
        if (TryInstantiatePluginSafe(type, out var instance))
        {
            if (instance is ILogLineColumnizer columnizer)
            {
                ProcessLoadedPlugin(columnizer, manifest, dllName);
                pluginLoadedCount++;
            }
        }
    }
    
    // ? Check for other plugin types (regardless of ILogLineColumnizer)
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
```

## What Changed

### Key Changes:
1. **Removed `else` branch** - All plugin type checks now happen independently
2. **Check all plugin types for every type in assembly** - A type can implement multiple interfaces
3. **Allow multiple plugins per assembly** - An assembly might contain both columnizers and file system plugins

### Benefits:
- **Correct plugin registration:** SftpFileSystem is now properly registered as `IFileSystemPlugin`
- **No mutual exclusion:** A type can implement multiple plugin interfaces
- **Better separation of concerns:** Each plugin type is checked independently
- **Future-proof:** Easy to add new plugin types without complex branching logic

## Impact

### Before Fix:
- ? SftpFileSystem (IFileSystemPlugin) would be checked in `else` block
- ? Plugin would be processed through fallback logic meant for non-columnizer types
- ? Potential for plugins to be registered in wrong collection
- ? Confusion between what interface the plugin actually implements

### After Fix:
- ? SftpFileSystem correctly registered in `RegisteredFileSystemPlugins`
- ? Each plugin type checked independently
- ? No confusion about plugin types
- ? Supports assemblies with multiple plugin types
- ? Clear, maintainable code structure

## Testing Verification

### Expected Behavior:
1. **SftpFileSystem Plugin:**
   - Should appear in `RegisteredFileSystemPlugins` collection
   - Should NOT appear in `RegisteredColumnizers` collection
   - Should be callable via `FindFileSystemForUri("sftp://...")`

2. **Multi-type Plugins:**
   - If a plugin implements both `ILogLineColumnizer` AND `IFileSystemPlugin`, both should be registered
   - Each interface should be handled independently

3. **Existing Plugins:**
   - All existing columnizer plugins should still load correctly
   - No regression in plugin loading behavior

## Files Modified
- `PluginRegistry/PluginRegistry.cs` - `LoadPluginAssembly` method

## Related Issues
This fix addresses the architectural issue where the plugin loading system assumed a plugin could only be one type. The new approach allows:
- Plugins that implement multiple interfaces
- Proper categorization based on actual interface implementation
- Better extensibility for future plugin types

## Notes
The `interfaceName` parameter in `LoadPluginAssembly` is still used for the primary search (ILogLineColumnizer) but now doesn't prevent other plugin types from being discovered. This maintains backward compatibility while fixing the bug.
