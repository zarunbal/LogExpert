# Recovery Plan: Fix PluginRegistry.cs Compilation Errors

## Problem Summary

During the implementation of type-aware lazy loading, several methods in `PluginRegistry.cs` were accidentally removed or corrupted during file edits. The following compilation errors exist:

1. **CS1038**: #endregion directive expected (line 584)
2. **CS0535**: Interface member 'FindFileSystemForUri' not implemented
3. **CS1061**: 'LoadPlugins' method not found
4. **CS0103**: 'ProcessLoadedPlugin' name does not exist
5. **CS0103**: 'LoadPluginAssembly' name does not exist

## Recovery Strategy

### Option 1: Manual Restoration (RECOMMENDED)
Restore the missing methods from git history and integrate with new lazy loading code.

### Option 2: Targeted File Reversion
Revert PluginRegistry.cs and reapply changes more carefully.

---

## Missing Methods to Restore

### 1. LoadPlugins() Method

**Location:** Should be in `#region Internals`

```csharp
internal void LoadPlugins()
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
    if (!Directory.Exists(pluginDir))
    {
        _logger.Warn("Plugin directory not found: {PluginDir}. Skipping plugin loading.", pluginDir);
        pluginDir = ".";
    }

    AppDomain.CurrentDomain.AssemblyResolve += ColumnizerResolveEventHandler;

    var loadedCount = 0;
    var skippedCount = 0;
    var failedCount = 0;

    var dllFiles = Directory.EnumerateFiles(pluginDir, "*.dll").ToList();
    var totalPlugins = dllFiles.Count;

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
            OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                dllName,
                fileName,
                currentIndex,
                totalPlugins,
                PluginLoadStatus.Validating,
                "Validating plugin security and manifest"));

            if (!PluginValidator.ValidatePlugin(dllName, out var manifest))
            {
                skippedCount++;
                _logger.Info("Skipped plugin (failed validation): {FileName}", fileName);

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

            OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                dllName,
                fileName,
                currentIndex,
                totalPlugins,
                PluginLoadStatus.Validated,
                manifest != null ? $"Validated: {manifest.Name} v{manifest.Version}" : "Validated successfully"));

            if (manifest != null)
            {
                _logger.Info("Plugin {PluginName} v{Version} by {Author}",
                    manifest.Name, manifest.Version, manifest.Author ?? "Unknown");
                if (manifest.Permissions != null && manifest.Permissions.Count > 0)
                {
                    _logger.Debug("  Permissions: {Permissions}", string.Join(", ", manifest.Permissions));
                }
            }

            OnPluginLoadProgress(new PluginLoadProgressEventArgs(
                dllName,
                fileName,
                currentIndex,
                totalPlugins,
                PluginLoadStatus.Loading,
                "Loading plugin assembly"));

            if (LoadPluginAssemblySafe(dllName, manifest))
            {
                loadedCount++;

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
            _logger.Warn(ex, "Plugin load failed (bad format): {FileName}", fileName);
            failedCount++;

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
            if (ex.LoaderExceptions != null && ex.LoaderExceptions.Length != 0)
            {
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    _logger.Error(loaderException, "Plugin load failed with '{0}'", dllName);
                }
            }

            _logger.Error(ex, "Loader exception during load of dll '{0}'", dllName);
            failedCount++;

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

    _logger.Info("Plugin loading complete. Loaded: {LoadedCount}, Skipped: {SkippedCount}, Failed: {FailedCount}", 
        loadedCount, skippedCount, failedCount);

    OnPluginLoadProgress(new PluginLoadProgressEventArgs(
        pluginDir,
        "Plugin Loading",
        totalPlugins,
        totalPlugins,
        PluginLoadStatus.Completed,
        $"Completed: {loadedCount} loaded, {skippedCount} skipped, {failedCount} failed"));

    PluginPermissionManager.SavePermissions(_applicationConfigurationFolder);
}
```

### 2. OnPluginLoadProgress() Method

```csharp
protected virtual void OnPluginLoadProgress(PluginLoadProgressEventArgs e)
{
    PluginLoadProgress?.Invoke(this, e);
}
```

### 3. ProcessLoadedPlugin() Method

**NOTE:** This method is only for columnizers in cache scenario. With lazy loading, we use InitializePluginIfNeeded instead.

```csharp
private void ProcessLoadedPlugin(object plugin, PluginManifest? manifest, string dllPath)
{
    if (plugin is not ILogLineColumnizer columnizer)
    {
        _logger.Warn("Loaded plugin is not ILogLineColumnizer: {Type}", plugin.GetType().Name);
        return;
    }

    RegisteredColumnizers.Add(columnizer);
    InitializePluginIfNeeded(columnizer, manifest, dllPath);
    
    _logger.Info("Plugin processed: {Plugin}", manifest?.Name ?? Path.GetFileNameWithoutExtension(dllPath));
}
```

### 4. LoadPluginAssembly() Method

```csharp
private bool LoadPluginAssembly(string dllName, PluginManifest? manifest)
{
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
            if (TryInstantiatePluginSafe(type, out var instance))
            {
                if (instance is ILogLineColumnizer columnizer)
                {
                    RegisteredColumnizers.Add(columnizer);
                    InitializePluginIfNeeded(columnizer, manifest, dllName);
                    pluginLoadedCount++;
                }
            }
        }

        // Check for other plugin types
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
```

### 5. TryInstantiatePluginSafe() Method

```csharp
private static bool TryInstantiatePluginSafe(Type type, out object instance)
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
```

### 6. FindKeywordActionPluginByName() Method

```csharp
public IKeywordAction FindKeywordActionPluginByName(string name)
{
    _ = _registeredKeywordsDict.TryGetValue(name, out var action);
    return action;
}
```

### 7. FindFileSystemForUri() Method

```csharp
public IFileSystemPlugin FindFileSystemForUri(string uriString)
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
```

### 8. TryAsContextMenu() Method

```csharp
private bool TryAsContextMenu(Type type)
{
    var me = TryInstantiate<IContextMenuEntry>(type);

    if (me != null)
    {
        RegisteredContextMenuPlugins.Add(me);
        InitializePluginIfNeeded(me, null, string.Empty);

        _logger.Info(CultureInfo.InvariantCulture, "Added context menu plugin {0}", type);
        return true;
    }

    return false;
}
```

### 9. TryAsKeywordAction() Method

```csharp
private bool TryAsKeywordAction(Type type)
{
    var ka = TryInstantiate<IKeywordAction>(type);
    if (ka != null)
    {
        RegisteredKeywordActions.Add(ka);
        _registeredKeywordsDict.Add(ka.GetName(), ka);
        InitializePluginIfNeeded(ka, null, string.Empty);

        _logger.Info(CultureInfo.InvariantCulture, "Added keyword plugin {0}", type);
        return true;
    }

    return false;
}
```

### 10. TryAsFileSystem() Method

```csharp
private bool TryAsFileSystem(Type type)
{
    var fs = TryInstantiate<IFileSystemPlugin>(type, _fileSystemCallback);
    fs ??= TryInstantiate<IFileSystemPlugin>(type);

    if (fs != null)
    {
        RegisteredFileSystemPlugins.Add(fs);
        InitializePluginIfNeeded(fs, null, string.Empty);

        _logger.Info(CultureInfo.InvariantCulture, "Added file system plugin {0}", type);
        return true;
    }

    return false;
}
```

### 11. TryInstantiate<T>() Methods

```csharp
private static T TryInstantiate<T>(Type loadedType) where T : class
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

private static T TryInstantiate<T>(Type loadedType, IFileSystemCallback fsCallback) where T : class
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
```

### 12. ColumnizerResolveEventHandler() Method

```csharp
private static Assembly ColumnizerResolveEventHandler(object? sender, ResolveEventArgs args)
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
```

---

## Fix Region Directives

Ensure proper region structure:

```csharp
#region Fields
// ... fields ...
#endregion

#region Events
// ... events ...
#endregion

#region cTor
// ... constructors ...
#endregion

#region Properties
// ... properties ...
#endregion

#region Public methods
// ... public methods ...
#endregion

#region Internals
// ... internal methods ...
#endregion

#region Private Methods
// ... private methods ...
#endregion

#region Events handler
// ... event handlers ...
#endregion
```

---

## Implementation Steps

1. **Backup Current File**
   ```bash
   cp PluginRegistry/PluginRegistry.cs PluginRegistry/PluginRegistry.cs.backup
   ```

2. **Add Missing Methods**
   - Copy each method listed above
   - Place in correct region
   - Maintain alphabetical order where appropriate

3. **Fix InitializePluginIfNeeded Calls**
   - Update TryAs* methods to use InitializePluginIfNeeded
   - Simplify by passing null/empty for manifest/dllPath when not available

4. **Verify Regions**
   - Ensure all #region have matching #endregion
   - Check proper nesting

5. **Build and Test**
   ```bash
   dotnet build PluginRegistry/LogExpert.PluginRegistry.csproj
   dotnet test
   ```

---

## Alternative: Git-Based Recovery

If manual restoration is too complex:

```bash
# View original file
git show HEAD~1:src/PluginRegistry/PluginRegistry.cs > PluginRegistry.original.cs

# Compare and merge
code --diff PluginRegistry.original.cs PluginRegistry/PluginRegistry.cs
```

---

## Verification Checklist

After restoration:

- [ ] No compilation errors
- [ ] All interface methods implemented
- [ ] All regions properly closed
- [ ] LoadPlugins method present and called
- [ ] All helper methods present
- [ ] Build succeeds
- [ ] Existing tests pass
- [ ] New lazy loading tests can be added

---

## Timeline

**Immediate** (30-60 minutes): Restore missing methods  
**Next** (15 minutes): Fix regions and build  
**Then** (30 minutes): Run tests and verify  
**Finally** (15 minutes): Update status document

---

**Status:** ?? CRITICAL - Must fix before continuing with Phase 3
