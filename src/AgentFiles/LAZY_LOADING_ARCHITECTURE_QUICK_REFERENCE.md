# Lazy Loading Architecture - Quick Reference

## Current vs. Proposed Architecture

### CURRENT (Broken - Disabled)
```
LoadPluginAssemblySafe(dll)
    ?
if (_useLazyLoading) ? Always true for EVERY DLL
    ?
CreateLazyProxy<ILogLineColumnizer>(dll) ? Assumes it's a columnizer!
    ?
return true ? Never loads non-columnizers!
```

**Result:** ? Only columnizers work, everything else breaks

---

### PROPOSED (Fixed)
```
LoadPluginAssemblySafe(dll)
    ?
AssemblyInspector.InspectAssembly(dll)
    ?
    Returns: PluginTypeInfo
    {
        HasColumnizer: bool
        HasFileSystem: bool
        HasContextMenu: bool
        HasKeywordAction: bool
    }
    ?
Decision Tree:
    ?? IsSingleType? ? Lazy Load
    ?   ?? HasColumnizer ? LazyPluginLoader<ILogLineColumnizer>
    ?   ?? HasFileSystem ? LazyPluginLoader<IFileSystemPlugin>
    ?   ?? HasContextMenu ? LazyPluginLoader<IContextMenuEntry>
    ?   ?? HasKeywordAction ? LazyPluginLoader<IKeywordAction>
    ?
    ?? Multiple types? ? Direct Load
        ?? LoadPluginAssembly(dll) ? Loads all types immediately
```

**Result:** ? All plugin types work correctly

---

## Component Diagram

```
???????????????????????????????????????????????????????????????
?                       PluginRegistry                         ?
???????????????????????????????????????????????????????????????
?                                                              ?
?  ????????????????????          ????????????????????        ?
?  ?  Plugin Storage  ?          ?  Lazy Loaders    ?        ?
?  ????????????????????          ????????????????????        ?
?  ? • Columnizers    ???????????? • _lazyColumnizers?       ?
?  ? • FileSystem     ???????????? • _lazyFileSystem ?       ?
?  ? • ContextMenu    ???????????? • _lazyContextMenu?       ?
?  ? • KeywordActions ???????????? • _lazyKeywordActions?    ?
?  ????????????????????          ????????????????????        ?
?         ?                              ?                     ?
?         ?                              ?                     ?
?         ?      ??????????????????????????????              ?
?         ?      ?  LoadPluginAssemblySafe    ?              ?
?         ?      ??????????????????????????????              ?
?         ?                     ?                             ?
?         ?      ??????????????????????????????              ?
?         ?      ?   AssemblyInspector        ?              ?
?         ?      ?   ?? InspectAssembly()     ?              ?
?         ?      ?   ?? Returns PluginTypeInfo?              ?
?         ?      ??????????????????????????????              ?
?         ?                                                   ?
?         ????????????[Lazy Load on Access]                  ?
?                                                              ?
???????????????????????????????????????????????????????????????
```

---

## Class Relationships

```
                    ????????????????????
                    ?   IPluginRegistry?
                    ????????????????????
                             ?
                    ????????????????????
                    ?  PluginRegistry  ?
                    ????????????????????
                    ? • LoadPlugins()  ?
                    ? • LoadAssembly() ?
                    ? • RegisterLazy() ?
                    ????????????????????
                             ?
              ???????????????????????????????
              ?              ?              ?
      ????????????????  ????????????  ????????????????
      ? Assembly     ?  ? Lazy      ?  ? Plugin       ?
      ? Inspector    ?  ? Plugin    ?  ? Type         ?
      ?              ?  ? Loader<T> ?  ? Info         ?
      ????????????????  ?????????????  ????????????????
```

---

## Data Flow

### Plugin Discovery Phase
```
1. Scan plugins/ directory
   ?
2. For each DLL:
   ?
3. Validate (signature, manifest)
   ?
4. Inspect assembly types
   ?
5. Decision:
   ?? Single type + lazy enabled ? Register lazy loader
   ?? Multiple types OR lazy disabled ? Load immediately
```

### Plugin Access Phase (Lazy Loading)
```
User Code: registry.RegisteredColumnizers
   ?
Property Getter:
   ?
Check: _lazyColumnizers.Count > 0?
   ? YES
For each LazyPluginLoader:
   ?
   loader.GetInstance() ? Loads DLL here!
   ?
   Add to RegisteredColumnizers
   ?
   Initialize (lifecycle hooks)
   ?
Clear _lazyColumnizers
   ?
Return RegisteredColumnizers
```

---

## Plugin Type Detection Logic

```csharp
// In AssemblyInspector.InspectAssembly()

foreach (var type in assembly.GetTypes())
{
    if (IsAbstract || IsInterface)
        continue;
    
    var interfaces = type.GetInterfaces();
    
    if (implements ILogLineColumnizer)
        info.HasColumnizer = true;
    
    if (implements IFileSystemPlugin)
        info.HasFileSystem = true;
    
    if (implements IContextMenuEntry)
        info.HasContextMenu = true;
    
    if (implements IKeywordAction)
        info.HasKeywordAction = true;
}

return info;
```

---

## Loading Decision Matrix

| Assembly Contains | Lazy Loading Enabled? | Action |
|-------------------|----------------------|---------|
| Only ILogLineColumnizer | ? Yes | **Lazy Load** as LazyPluginLoader<ILogLineColumnizer> |
| Only IFileSystemPlugin | ? Yes | **Lazy Load** as LazyPluginLoader<IFileSystemPlugin> |
| Only IContextMenuEntry | ? Yes | **Lazy Load** as LazyPluginLoader<IContextMenuEntry> |
| Only IKeywordAction | ? Yes | **Lazy Load** as LazyPluginLoader<IKeywordAction> |
| Multiple plugin types | ? Yes | **Direct Load** (LoadPluginAssembly) |
| Any types | ? No | **Direct Load** (LoadPluginAssembly) |
| No plugin types | Any | **Skip** (not a plugin) |

---

## Example Scenarios

### Scenario 1: CsvColumnizer.dll (Single Type)
```
1. InspectAssembly(CsvColumnizer.dll)
   ? PluginTypeInfo { HasColumnizer: true, others: false }

2. IsSingleType? ? YES
   
3. RegisterLazyPlugins()
   ? Creates LazyPluginLoader<ILogLineColumnizer>
   ? Adds to _lazyColumnizers
   ? Plugin NOT loaded yet ?

4. User accesses: registry.RegisteredColumnizers
   ? Property getter triggers lazy loading
   ? loader.GetInstance() loads CsvColumnizer
   ? Adds to RegisteredColumnizers
   ? Returns collection
```

### Scenario 2: SftpFileSystem.dll (Single Type)
```
1. InspectAssembly(SftpFileSystem.dll)
   ? PluginTypeInfo { HasFileSystem: true, others: false }

2. IsSingleType? ? YES
   
3. RegisterLazyPlugins()
   ? Creates LazyPluginLoader<IFileSystemPlugin>
   ? Adds to _lazyFileSystemPlugins
   ? Plugin NOT loaded yet ?

4. User accesses: registry.RegisteredFileSystemPlugins
   ? Property getter triggers lazy loading
   ? loader.GetInstance() loads SftpFileSystem
   ? Adds to RegisteredFileSystemPlugins
   ? Returns collection
```

### Scenario 3: MixedPlugin.dll (Multiple Types)
```
1. InspectAssembly(MixedPlugin.dll)
   ? PluginTypeInfo { 
       HasColumnizer: true, 
       HasFileSystem: true, 
       others: false 
     }

2. IsSingleType? ? NO (has 2 types)
   
3. Load Immediately
   ? LoadPluginAssembly(MixedPlugin.dll)
   ? Checks all types
   ? Adds columnizer to RegisteredColumnizers
   ? Adds file system plugin to RegisteredFileSystemPlugins
   ? All loaded immediately ?
```

---

## Performance Characteristics

### Startup Time (with lazy loading)
```
Old (broken):
    Load Time = Validation + (N × Assembly Inspection) + 0ms loading
                                                          ? But broken!

New (fixed):
    Load Time = Validation + (N × Assembly Inspection) + 0ms loading
                                                          ? Really working!
    
First Access:
    Access Time = Load Plugin + Initialize
```

### Memory Usage
```
Before Fix (all eager):
    Memory = N plugins × Plugin Size

After Fix (lazy):
    Memory = Used plugins × Plugin Size
           + (Unused plugins × ~100 bytes for loader)
```

---

## Configuration Examples

### Enable/Disable Lazy Loading
```csharp
// In LoadFeatureFlags()

// Enable lazy loading (default after fix)
_useLazyLoading = true;

// Disable lazy loading (for troubleshooting)
_useLazyLoading = false;
```

### Per-Type Control (Future Enhancement)
```csharp
_lazyLoadColumnizers = true;
_lazyLoadFileSystem = false;  // Always load immediately
_lazyLoadContextMenu = true;
_lazyLoadKeywordActions = true;
```

---

## Error Handling

### Assembly Inspection Fails
```
InspectAssembly() throws exception
    ?
Catch exception
    ?
Log error
    ?
Return empty PluginTypeInfo
    ?
Fallback to Direct Load
```

### Lazy Load Fails
```
LazyPluginLoader.GetInstance() throws
    ?
Catch exception
    ?
Log error
    ?
Mark as loaded (prevent retry)
    ?
Return null
    ?
Plugin not available
```

---

## Migration Checklist

- [x] Create PluginTypeInfo class
- [x] Implement AssemblyInspector
- [x] Create LazyPluginLoader<T>
- [x] Update LoadPluginAssemblySafe
- [x] Implement RegisterLazyPlugins
- [x] Update property getters (with lazy loading)
- [ ] Add unit tests
- [ ] Add integration tests
- [ ] Performance testing
- [ ] Documentation
- [ ] Enable _useLazyLoading = true

---

## Quick Start Implementation

### Step 1: Add New Classes
1. `PluginTypeInfo.cs`
2. `AssemblyInspector.cs`
3. `LazyPluginLoader.cs`

### Step 2: Update PluginRegistry
1. Add lazy loader collections
2. Modify `LoadPluginAssemblySafe()`
3. Add `RegisterLazyPlugins()`
4. Update property getters

### Step 3: Test
1. Unit test each component
2. Integration test plugin loading
3. Verify all types load correctly

### Step 4: Enable
1. Set `_useLazyLoading = true`
2. Monitor startup performance
3. Watch for errors in logs

---

## Key Takeaways

? **All plugin types supported** - Not just columnizers  
? **Smart loading** - Lazy for single-type, immediate for mixed  
? **Safe fallback** - Errors result in direct loading  
? **Performance** - Faster startup with many plugins  
? **Transparent** - No plugin API changes needed  

?? **Goal:** Working lazy loading that doesn't break anything!
