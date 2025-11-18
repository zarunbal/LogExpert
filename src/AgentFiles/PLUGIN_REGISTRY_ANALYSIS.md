# Plugin Registry Implementation Analysis and Recommended Changes

## Executive Summary

This document provides a comprehensive analysis of the PluginRegistry implementation in LogExpert, including PluginManifest, PluginValidator, and PluginPermissions. The analysis identifies potential issues, security concerns, architectural improvements, and API enhancements.

---

## Table of Contents

1. [Critical Issues](#critical-issues)
2. [Security Concerns](#security-concerns)
3. [Architectural Improvements](#architectural-improvements)
4. [API Enhancements](#api-enhancements)
5. [Performance Optimizations](#performance-optimizations)
6. [Documentation and Testing](#documentation-and-testing)
7. [Implementation Roadmap](#implementation-roadmap)

---

## 1. Critical Issues

### 1.1 PluginManifest.cs

#### Issue: Required Properties Not Validated at Construction Time
**Location:** Lines 24-52 (Property Declarations)

**Problem:**
```csharp
public required string Url { get; set; }
public required string License { get; set; }
```
These properties are marked as `required` but the XML documentation says they are "Optional". This is contradictory.

**Impact:** 
- Runtime errors when deserializing manifests without these fields
- Confusing API for plugin developers

**Recommendation:**
```csharp
// Option 1: Make truly optional by removing 'required'
[JsonProperty("url")]
public string? Url { get; set; }

[JsonProperty("license")]
public string? License { get; set; }

// Option 2: If they should be required, update documentation
/// <summary>
/// Required: Plugin website or repository URL.
/// </summary>
[JsonProperty("url")]
public required string Url { get; set; }
```

#### Issue: Load Method Returns Null Instead of Throwing
**Location:** Lines 113-143 (Load Method)

**Problem:**
```csharp
public static PluginManifest Load(string manifestPath)
{
    // ...
    if (!File.Exists(manifestPath))
    {
        _logger.Debug("Manifest file not found: {ManifestPath}", manifestPath);
        return null; // ? Null return pattern is error-prone
    }
    // ...
}
```

**Impact:**
- Forces null checks throughout the codebase
- Easy to forget null checks leading to NullReferenceExceptions
- Difficult to distinguish between different error conditions

**Recommendation:**
```csharp
// Use Result<T> pattern or throw exceptions with specific types
public static PluginManifest? Load(string manifestPath)
{
    if (!File.Exists(manifestPath))
    {
        _logger.Debug("Manifest file not found: {ManifestPath}", manifestPath);
        return null; // Nullable return type makes intent clear
    }
    
    try
    {
        var json = File.ReadAllText(manifestPath);
        var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);
        
        if (manifest == null)
        {
            throw new PluginManifestException($"Failed to deserialize manifest: {manifestPath}");
        }
        
        _logger.Info("Loaded manifest for plugin: {PluginName} v{Version}", manifest.Name, manifest.Version);
        return manifest;
    }
    catch (Exception ex) when (ex is IOException or JsonException)
    {
        _logger.Error(ex, "Error loading manifest from: {ManifestPath}", manifestPath);
        throw new PluginManifestException($"Error loading manifest: {manifestPath}", ex);
    }
}

// Add custom exception type
public class PluginManifestException : Exception
{
    public PluginManifestException(string message) : base(message) { }
    public PluginManifestException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

#### Issue: Version Compatibility Logic Doesn't Handle Pre-release Versions
**Location:** Lines 188-254 (IsCompatibleWith Method)

**Problem:**
The current implementation doesn't support semantic versioning pre-release identifiers (e.g., "1.0.0-beta", "2.1.0-rc.1").

**Recommendation:**
```csharp
// Use NuGet.Versioning package
public bool IsCompatibleWith(SemanticVersion logExpertVersion)
{
    if (Requires == null || string.IsNullOrWhiteSpace(Requires.LogExpert))
    {
        return true;
    }
    
    try
    {
        var requirement = Requires.LogExpert;
        var versionRange = VersionRange.Parse(requirement);
        
        return versionRange.Satisfies(logExpertVersion);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Error parsing version requirement: {Requirement}", Requires.LogExpert);
        return false;
    }
}
```

### 1.2 PluginValidator.cs

#### Issue: Hardcoded Whitelist Is Not Extensible
**Location:** Lines 26-43 (_trustedPluginNames)

**Problem:**
```csharp
private static readonly HashSet<string> _trustedPluginNames = new(StringComparer.OrdinalIgnoreCase)
{
    "AutoColumnizer.dll",
    "CsvColumnizer.dll",
    // ... hardcoded list
};
```

**Impact:**
- Requires code changes to trust new plugins
- No user-friendly way to trust custom plugins
- Difficult to manage in enterprise environments

**Recommendation:**
```csharp
// Load trusted plugins from configuration file
private static HashSet<string> _trustedPluginNames;
private static HashSet<string> _trustedPluginHashes; // For hash-based verification

static PluginValidator()
{
    LoadTrustedPlugins();
}

private static void LoadTrustedPlugins()
{
    var configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LogExpert", "trusted-plugins.json");
    
    if (File.Exists(configPath))
    {
        var json = File.ReadAllText(configPath);
        var config = JsonConvert.DeserializeObject<TrustedPluginConfig>(json);
        _trustedPluginNames = new HashSet<string>(config.PluginNames, StringComparer.OrdinalIgnoreCase);
        _trustedPluginHashes = new HashSet<string>(config.PluginHashes, StringComparer.OrdinalIgnoreCase);
    }
    else
    {
        // Fallback to hardcoded defaults
        _trustedPluginNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "AutoColumnizer.dll",
            "CsvColumnizer.dll",
            // ...
        };
        _trustedPluginHashes = new(StringComparer.OrdinalIgnoreCase);
    }
}

public class TrustedPluginConfig
{
    public List<string> PluginNames { get; set; } = new();
    public List<string> PluginHashes { get; set; } = new();
    public bool AllowUserTrustedPlugins { get; set; } = true;
}
```

#### Issue: ValidatePlugin Doesn't Verify Plugin Hash
**Location:** Lines 62-129 (ValidatePlugin Method)

**Problem:**
The current implementation checks whitelist but doesn't verify file integrity. A malicious actor could replace a trusted plugin with a modified version.

**Recommendation:**
```csharp
public static bool ValidatePlugin(string dllPath, out PluginManifest manifest)
{
    manifest = null;
    
    if (!File.Exists(dllPath))
    {
        _logger.Warn("Plugin file does not exist: {DllPath}", dllPath);
        return false;
    }
    
    var fileName = Path.GetFileName(dllPath);
    
    // Check whitelist
    if (!_trustedPluginNames.Contains(fileName))
    {
        _logger.Warn("Plugin not in whitelist: {FileName}", fileName);
        
        // Check if hash is trusted
        var hash = CalculateFileHash(dllPath);
        if (!_trustedPluginHashes.Contains(hash))
        {
            _logger.Error("Plugin hash not trusted: {FileName}, Hash: {Hash}", fileName, hash);
            return false;
        }
    }
    else
    {
        // Even trusted plugins should verify hash for known versions
        var hash = CalculateFileHash(dllPath);
        if (_knownPluginHashes.TryGetValue(fileName, out var expectedHash))
        {
            if (!expectedHash.Equals(hash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warn("Plugin hash mismatch: {FileName}, Expected: {ExpectedHash}, Actual: {Hash}", 
                    fileName, expectedHash, hash);
                // Optional: Prompt user or fail validation
            }
        }
    }
    
    // Continue with manifest validation...
}
```

### 1.3 PluginPermissions.cs

#### Issue: Permission Checks Don't Enforce Anything
**Location:** Lines 64-85 (HasPermission Method)

**Problem:**
The permission system only *checks* if a plugin has permission but doesn't *enforce* it. Plugins can still access resources without checking permissions.

**Impact:**
- False sense of security
- Permissions are advisory only
- No actual sandboxing

**Recommendation:**
```csharp
// Create a PluginSecurityManager that wraps plugin calls
public class PluginSecurityManager
{
    private readonly string _pluginName;
    
    public PluginSecurityManager(string pluginName)
    {
        _pluginName = pluginName;
    }
    
    public string ReadFile(string path)
    {
        if (!PluginPermissionManager.HasPermission(_pluginName, PluginPermission.FileSystemRead))
        {
            throw new UnauthorizedAccessException(
                $"Plugin {_pluginName} does not have FileSystemRead permission");
        }
        
        return File.ReadAllText(path);
    }
    
    public void WriteFile(string path, string content)
    {
        if (!PluginPermissionManager.HasPermission(_pluginName, PluginPermission.FileSystemWrite))
        {
            throw new UnauthorizedAccessException(
                $"Plugin {_pluginName} does not have FileSystemWrite permission");
        }
        
        File.WriteAllText(path, content);
    }
    
    // Similar wrappers for network, config, registry access
}

// Provide this to plugins via ILogExpertCallback
public interface ILogExpertCallback
{
    // Existing members...
    
    IPluginSecurityContext SecurityContext { get; }
}

public interface IPluginSecurityContext
{
    string ReadFile(string path);
    void WriteFile(string path, string content);
    HttpResponseMessage SendHttpRequest(HttpRequestMessage request);
    // etc.
}
```

---

## 2. Security Concerns

### 2.1 Code Injection via Untrusted Plugins

**Risk Level:** HIGH

**Problem:**
Plugins run in the same AppDomain as the main application with full trust.

**Recommendation:**
```csharp
// Load plugins in separate AppDomain with restricted permissions
public class SecurePluginLoader
{
    public ILogLineColumnizer LoadColumnizer(string assemblyPath)
    {
        // Create restricted AppDomain
        var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
        var setup = new AppDomainSetup
        {
            ApplicationBase = Path.GetDirectoryName(assemblyPath),
            DisallowBindingRedirects = true,
            DisallowCodeDownload = true
        };
        
        var permissions = new PermissionSet(PermissionState.None);
        permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, assemblyPath));
        
        var appDomain = AppDomain.CreateDomain(
            "PluginDomain_" + Path.GetFileNameWithoutExtension(assemblyPath),
            evidence,
            setup,
            permissions);
        
        try
        {
            var loader = (PluginLoader)appDomain.CreateInstanceAndUnwrap(
                typeof(PluginLoader).Assembly.FullName,
                typeof(PluginLoader).FullName);
            
            return loader.LoadColumnizer(assemblyPath);
        }
        catch
        {
            AppDomain.Unload(appDomain);
            throw;
        }
    }
}
```

### 2.2 Path Traversal in Manifest Files

**Risk Level:** MEDIUM

**Problem:**
The manifest's `main` field could specify paths outside the plugin directory.

**Recommendation:**
```csharp
private static bool ValidateManifestPaths(PluginManifest manifest, string manifestDirectory)
{
    var mainPath = Path.GetFullPath(Path.Combine(manifestDirectory, manifest.Main));
    var pluginDir = Path.GetFullPath(manifestDirectory);
    
    if (!mainPath.StartsWith(pluginDir, StringComparison.OrdinalIgnoreCase))
    {
        _logger.Error("Plugin main file outside plugin directory: {MainPath}", mainPath);
        return false;
    }
    
    return true;
}
```

### 2.3 Denial of Service via Malicious Regex

**Risk Level:** MEDIUM

**Problem:**
Plugin manifests can contain regex patterns for permissions/filters. Malicious regex can cause catastrophic backtracking.

**Recommendation:**
```csharp
// Already partially addressed in Program.cs:
AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2));

// Additionally validate regex complexity
public static bool IsRegexSafe(string pattern)
{
    try
    {
        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        
        // Test with adversarial input
        var testInput = new string('a', 1000);
        var match = regex.Match(testInput);
        
        return true;
    }
    catch (RegexMatchTimeoutException)
    {
        _logger.Warn("Regex pattern timed out during validation: {Pattern}", pattern);
        return false;
    }
}
```

---

## 3. Architectural Improvements

### 3.1 Separate Plugin Loading from Validation

**Current Issue:**
PluginRegistry mixes loading, validation, and execution concerns.

**Recommendation:**
```csharp
// Separate responsibilities
public interface IPluginLoader
{
    PluginLoadResult LoadPlugin(string path);
}

public interface IPluginValidator
{
    ValidationResult ValidatePlugin(string path, PluginManifest manifest);
}

public interface IPluginRegistry
{
    void RegisterPlugin(IPlugin plugin);
    IEnumerable<T> GetPlugins<T>() where T : IPlugin;
}

public class PluginManager
{
    private readonly IPluginLoader _loader;
    private readonly IPluginValidator _validator;
    private readonly IPluginRegistry _registry;
    
    public PluginManager(IPluginLoader loader, IPluginValidator validator, IPluginRegistry registry)
    {
        _loader = loader;
        _validator = validator;
        _registry = registry;
    }
    
    public void LoadPluginsFromDirectory(string directory)
    {
        foreach (var dllFile in Directory.GetFiles(directory, "*.dll"))
        {
            try
            {
                var manifest = PluginManifest.Load(Path.ChangeExtension(dllFile, ".manifest.json"));
                var validationResult = _validator.ValidatePlugin(dllFile, manifest);
                
                if (!validationResult.IsValid)
                {
                    _logger.Warn("Plugin validation failed: {DllFile}, Errors: {Errors}", 
                        dllFile, string.Join(", ", validationResult.Errors));
                    continue;
                }
                
                var loadResult = _loader.LoadPlugin(dllFile);
                if (loadResult.Success)
                {
                    _registry.RegisterPlugin(loadResult.Plugin);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load plugin: {DllFile}", dllFile);
            }
        }
    }
}
```

### 3.2 Plugin Lifecycle Management

**Recommendation:**
```csharp
public interface IPluginLifecycle
{
    void Initialize(IPluginContext context);
    void Shutdown();
    void Reload();
}

public class PluginContext : IPluginContext
{
    public ILogExpertLogger Logger { get; }
    public IPluginConfiguration Configuration { get; }
    public IPluginSecurityContext SecurityContext { get; }
    public string PluginDirectory { get; }
    public Version HostVersion { get; }
}

// Update plugin interfaces to inherit from IPluginLifecycle
public interface ILogLineColumnizer : IPluginLifecycle
{
    // Existing members...
}
```

### 3.3 Dependency Injection for Plugins

**Recommendation:**
```csharp
// Allow plugins to declare dependencies
public interface IPluginDependencyProvider
{
    T GetService<T>() where T : class;
    object GetService(Type serviceType);
}

// In plugin manifest
public class PluginDependencies
{
    [JsonProperty("requiredServices")]
    public List<string> RequiredServices { get; set; } = new();
    
    [JsonProperty("optionalServices")]
    public List<string> OptionalServices { get; set; } = new();
}

// Update PluginManifest
[JsonProperty("dependencies")]
public PluginDependencies ServiceDependencies { get; set; }
```

---

## 4. API Enhancements

### 4.1 Async Plugin Loading

**Recommendation:**
```csharp
public interface IPluginLoader
{
    Task<PluginLoadResult> LoadPluginAsync(string path, CancellationToken cancellationToken);
}

public class PluginRegistry
{
    public async Task LoadPluginsAsync(string directory, IProgress<PluginLoadProgress> progress, CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(directory, "*.dll");
        var totalFiles = files.Length;
        var loadedCount = 0;
        
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            try
            {
                var result = await _loader.LoadPluginAsync(file, cancellationToken);
                if (result.Success)
                {
                    RegisterPlugin(result.Plugin);
                    loadedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load plugin: {File}", file);
            }
            
            progress?.Report(new PluginLoadProgress
            {
                TotalPlugins = totalFiles,
                LoadedPlugins = loadedCount,
                CurrentPlugin = Path.GetFileName(file)
            });
        }
    }
}
```

### 4.2 Plugin Metadata API

**Recommendation:**
```csharp
public interface IPluginMetadata
{
    string Name { get; }
    Version Version { get; }
    string Author { get; }
    string Description { get; }
    Uri HomepageUrl { get; }
    string License { get; }
    IReadOnlyCollection<PluginCapability> Capabilities { get; }
    IReadOnlyDictionary<string, object> ExtendedMetadata { get; }
}

public enum PluginCapability
{
    LogParsing,
    LogFiltering,
    LogHighlighting,
    LogExporting,
    LogTransformation,
    CustomContextMenu,
    KeywordAction,
    FileSystemAccess
}
```

### 4.3 Plugin Event System

**Recommendation:**
```csharp
public interface IPluginEventBus
{
    void Subscribe<TEvent>(IPlugin plugin, Action<TEvent> handler) where TEvent : IPluginEvent;
    void Unsubscribe<TEvent>(IPlugin plugin) where TEvent : IPluginEvent;
    void Publish<TEvent>(TEvent pluginEvent) where TEvent : IPluginEvent;
}

public interface IPluginEvent
{
    DateTime Timestamp { get; }
    string Source { get; }
}

public class LogFileLoadedEvent : IPluginEvent
{
    public DateTime Timestamp { get; init; }
    public string Source { get; init; }
    public string FileName { get; init; }
    public long FileSize { get; init; }
}

public class LogLineMatchedEvent : IPluginEvent
{
    public DateTime Timestamp { get; init; }
    public string Source { get; init; }
    public int LineNumber { get; init; }
    public string LineContent { get; init; }
    public string MatchedPattern { get; init; }
}
```

---

## 5. Performance Optimizations

### 5.1 Lazy Plugin Loading

**Problem:**
All plugins are loaded on startup, even if not used.

**Recommendation:**
```csharp
public class LazyPluginProxy<T> : IPlugin where T : IPlugin
{
    private readonly Lazy<T> _plugin;
    private readonly PluginManifest _manifest;
    
    public LazyPluginProxy(string assemblyPath, PluginManifest manifest)
    {
        _manifest = manifest;
        _plugin = new Lazy<T>(() => LoadPlugin(assemblyPath));
    }
    
    private T LoadPlugin(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        var pluginType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract);
        
        return (T)Activator.CreateInstance(pluginType);
    }
    
    public T Instance => _plugin.Value;
    public bool IsLoaded => _plugin.IsValueCreated;
    public PluginManifest Manifest => _manifest;
}
```

### 5.2 Plugin Caching

**Recommendation:**
```csharp
public class PluginCache
{
    private readonly ConcurrentDictionary<string, CachedPlugin> _cache = new();
    
    public PluginLoadResult LoadPluginWithCache(string path)
    {
        var hash = CalculateFileHash(path);
        var cacheKey = $"{Path.GetFileName(path)}_{hash}";
        
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            if (IsCacheValid(cached))
            {
                return new PluginLoadResult { Success = true, Plugin = cached.Plugin };
            }
        }
        
        var result = LoadPluginInternal(path);
        if (result.Success)
        {
            _cache[cacheKey] = new CachedPlugin
            {
                Plugin = result.Plugin,
                LoadTime = DateTime.UtcNow,
                FileHash = hash
            };
        }
        
        return result;
    }
    
    private class CachedPlugin
    {
        public IPlugin Plugin { get; init; }
        public DateTime LoadTime { get; init; }
        public string FileHash { get; init; }
    }
}
```

---

## 6. Documentation and Testing

### 6.1 Plugin Development Guide

**Create:** `docs/PLUGIN_DEVELOPMENT_GUIDE.md`

**Contents:**
1. Plugin types and interfaces
2. Manifest file format and examples
3. Permission system usage
4. Testing plugin locally
5. Packaging and distribution
6. Security best practices
7. API reference

### 6.2 Manifest Schema

**Create:** `schemas/plugin-manifest.schema.json`

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "LogExpert Plugin Manifest",
  "type": "object",
  "required": ["name", "version", "author", "description", "apiVersion", "main"],
  "properties": {
    "name": {
      "type": "string",
      "description": "Plugin name (must match DLL name without extension)"
    },
    "version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+\\.\\d+(-[a-zA-Z0-9.]+)?$",
      "description": "Semantic version (e.g., 1.0.0 or 1.0.0-beta)"
    },
    "author": {
      "type": "string",
      "description": "Plugin author or organization"
    },
    "description": {
      "type": "string",
      "description": "Brief description of plugin functionality"
    },
    "apiVersion": {
      "type": "string",
      "description": "LogExpert plugin API version this plugin targets"
    },
    "main": {
      "type": "string",
      "description": "Main DLL file name"
    },
    "url": {
      "type": "string",
      "format": "uri",
      "description": "Plugin website or repository URL"
    },
    "license": {
      "type": "string",
      "description": "Plugin license identifier (e.g., MIT, Apache-2.0)"
    },
    "requires": {
      "type": "object",
      "properties": {
        "logExpert": {
          "type": "string",
          "description": "LogExpert version requirement (e.g., >=1.10.0)"
        },
        "dotnet": {
          "type": "string",
          "description": ".NET runtime version requirement"
        }
      }
    },
    "permissions": {
      "type": "array",
      "items": {
        "type": "string",
        "enum": [
          "filesystem:read",
          "filesystem:write",
          "network:connect",
          "config:read",
          "config:write",
          "registry:read"
        ]
      }
    },
    "dependencies": {
      "type": "object",
      "additionalProperties": {
        "type": "string"
      }
    }
  }
}
```

### 6.3 Unit Tests

**Recommendations:**

```csharp
// Add to PluginRegistry.Tests project
[TestFixture]
public class PluginManifestTests
{
    [Test]
    public void Load_ValidManifest_ReturnsManifest()
    {
        // Arrange
        var manifestPath = CreateTestManifest();
        
        // Act
        var manifest = PluginManifest.Load(manifestPath);
        
        // Assert
        Assert.IsNotNull(manifest);
        Assert.AreEqual("TestPlugin", manifest.Name);
    }
    
    [Test]
    public void Load_MissingFile_ReturnsNull()
    {
        // Arrange
        var manifestPath = "nonexistent.manifest.json";
        
        // Act
        var manifest = PluginManifest.Load(manifestPath);
        
        // Assert
        Assert.IsNull(manifest);
    }
    
    [Test]
    public void Validate_MissingRequiredField_ReturnsErrors()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            // Missing required fields
        };
        
        // Act
        var isValid = manifest.Validate(out var errors);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.That(errors, Has.Some.Matches<string>(e => e.Contains("required field")));
    }
    
    [Test]
    [TestCase(">=1.10.0", "1.10.0", true)]
    [TestCase(">=1.10.0", "1.9.0", false)]
    [TestCase("~1.10.0", "1.10.5", true)]
    [TestCase("~1.10.0", "1.11.0", false)]
    [TestCase("^1.10.0", "1.11.0", true)]
    [TestCase("^1.10.0", "2.0.0", false)]
    public void IsCompatibleWith_VersionRequirements_ReturnsExpected(
        string requirement, string version, bool expected)
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Requires = new PluginRequirements(requirement, ">=8.0.0")
        };
        
        // Act
        var isCompatible = manifest.IsCompatibleWith(Version.Parse(version));
        
        // Assert
        Assert.AreEqual(expected, isCompatible);
    }
}

[TestFixture]
public class PluginValidatorTests
{
    [Test]
    public void ValidatePlugin_TrustedPlugin_ReturnsTrue()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("CsvColumnizer.dll");
        
        // Act
        var isValid = PluginValidator.ValidatePlugin(pluginPath, out var manifest);
        
        // Assert
        Assert.IsTrue(isValid);
    }
    
    [Test]
    public void ValidatePlugin_UntrustedPlugin_ReturnsFalse()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("MaliciousPlugin.dll");
        
        // Act
        var isValid = PluginValidator.ValidatePlugin(pluginPath, out var manifest);
        
        // Assert
        Assert.IsFalse(isValid);
    }
    
    [Test]
    public void ValidatePlugin_InvalidManifest_ReturnsFalse()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("ValidPlugin.dll");
        CreateInvalidManifest(pluginPath);
        
        // Act
        var isValid = PluginValidator.ValidatePlugin(pluginPath, out var manifest);
        
        // Assert
        Assert.IsFalse(isValid);
        Assert.IsNull(manifest);
    }
}

[TestFixture]
public class PluginPermissionManagerTests
{
    [Test]
    public void HasPermission_WithPermission_ReturnsTrue()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var permission = PluginPermission.FileSystemRead;
        PluginPermissionManager.SetPermissions(pluginName, permission);
        
        // Act
        var hasPermission = PluginPermissionManager.HasPermission(pluginName, permission);
        
        // Assert
        Assert.IsTrue(hasPermission);
    }
    
    [Test]
    public void HasPermission_WithoutPermission_ReturnsFalse()
    {
        // Arrange
        var pluginName = "TestPlugin";
        PluginPermissionManager.SetPermissions(pluginName, PluginPermission.FileSystemRead);
        
        // Act
        var hasPermission = PluginPermissionManager.HasPermission(pluginName, PluginPermission.FileSystemWrite);
        
        // Assert
        Assert.IsFalse(hasPermission);
    }
    
    [Test]
    public void ParsePermissions_ValidStrings_ReturnsCorrectPermissions()
    {
        // Arrange
        var permissionStrings = new[] { "filesystem:read", "filesystem:write", "network:connect" };
        
        // Act
        var permissions = PluginPermissionManager.ParsePermissions(permissionStrings);
        
        // Assert
        Assert.That(permissions, Is.EqualTo(
            PluginPermission.FileSystemRead | 
            PluginPermission.FileSystemWrite | 
            PluginPermission.NetworkConnect));
    }
}
```

---

## 7. Implementation Roadmap

### Phase 1: Critical Fixes (Week 1-2)
1. ? Fix required vs. optional properties in PluginManifest
2. ? Improve error handling (exceptions vs. null returns)
3. ? Add plugin hash verification
4. ? Implement trusted plugin configuration file
5. ? Add path traversal validation

### Phase 2: Security Enhancements (Week 3-4)
1. ? Implement plugin sandboxing (AppDomain isolation)
2. ? Add permission enforcement (not just checking)
3. ? Regex safety validation
4. ? Audit logging for plugin actions

### Phase 3: Architectural Improvements (Week 5-6)
1. ? Separate concerns (loader, validator, registry)
2. ? Add plugin lifecycle management
3. ? Implement dependency injection for plugins
4. ? Add plugin event system

### Phase 4: API Enhancements (Week 7-8)
1. ? Async plugin loading
2. ? Plugin metadata API
3. ? Lazy loading for plugins
4. ? Plugin caching mechanism

### Phase 5: Documentation and Testing (Week 9-10)
1. ? Create plugin development guide
2. ? Add JSON schema for manifests
3. ? Write comprehensive unit tests
4. ? Create example plugins
5. ? Add integration tests

### Phase 6: Polish and Release (Week 11-12)
1. ? Performance testing and optimization
2. ? UI for managing trusted plugins
3. ? Migration tool for existing plugins
4. ? Beta testing with community
5. ? Documentation review
6. ? Final release

---

## Appendix A: Example Plugin Manifest

```json
{
  "name": "CustomLogColumnizer",
  "version": "1.0.0",
  "author": "LogExpert Community",
  "description": "A custom columnizer for parsing application-specific log formats",
  "apiVersion": "2.0",
  "main": "CustomLogColumnizer.dll",
  "url": "https://github.com/logexperts/custom-columnizer",
  "license": "MIT",
  "requires": {
    "logExpert": ">=1.10.0",
    "dotnet": ">=8.0.0"
  },
  "permissions": [
    "filesystem:read",
    "config:read"
  ],
  "dependencies": {
    "Newtonsoft.Json": ">=13.0.0",
    "System.Text.RegularExpressions": ">=8.0.0"
  }
}
```

---

## Appendix B: Migration Strategy

### For Existing Plugins

1. **Create Manifest Files**
   - Generate manifests for all existing plugins
   - Use conservative permissions (only what's needed)
   - Version as 1.0.0 unless otherwise specified

2. **Update Plugin Code** (Optional but Recommended)
   - Implement IPluginLifecycle if not already
   - Use IPluginSecurityContext for file/network access
   - Add logging via ILogExpertLogger

3. **Testing**
   - Test plugins with new security model
   - Verify permissions are adequate
   - Ensure backward compatibility

### For LogExpert Core

1. **Maintain Backward Compatibility**
   - Support plugins without manifests (with warnings)
   - Default to restrictive permissions for legacy plugins
   - Provide migration wizard for users

2. **Deprecation Timeline**
   - v1.11: Warnings for plugins without manifests
   - v1.12: Manifests recommended
   - v2.0: Manifests required

---

## Appendix C: Security Checklist

Before loading a plugin:

- [ ] Plugin file exists
- [ ] Plugin is in trusted list OR hash matches known hash
- [ ] Manifest exists and is valid JSON
- [ ] Manifest version requirements are met
- [ ] Manifest paths don't escape plugin directory
- [ ] Regex patterns in manifest are safe
- [ ] Required dependencies are available
- [ ] Assembly can be loaded
- [ ] Assembly is valid .NET assembly
- [ ] Assembly doesn't contain suspicious code patterns
- [ ] Plugin has necessary permissions
- [ ] Plugin can be sandboxed successfully

---

## Conclusion

The current PluginRegistry implementation provides a good foundation but has several areas that need improvement, particularly around security, error handling, and extensibility. The recommendations in this document provide a roadmap for making the plugin system more robust, secure, and developer-friendly.

**Priority Order:**
1. Critical security fixes (hash verification, sandboxing)
2. API improvements (error handling, validation)
3. Architectural improvements (separation of concerns)
4. Performance optimizations
5. Documentation and tooling

Implementation of these recommendations will result in a production-ready plugin system that balances security, performance, and ease of use.
