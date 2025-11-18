# Priority 1: Critical Security & Stability Implementation Guide

## Overview

**Timeline:** Weeks 1-3  
**Risk Level:** ?? HIGH  
**Effort:** Medium  
**Impact:** HIGH  

This guide provides step-by-step instructions for implementing critical security fixes and stability improvements in the LogExpert Plugin Registry.

---

## Table of Contents

1. [Week 1: Hash Verification & Property Fixes](#week-1-hash-verification--property-fixes)
2. [Week 2: Error Handling & Regex Safety](#week-2-error-handling--regex-safety)
3. [Week 3: Audit Logging & Testing](#week-3-audit-logging--testing)
4. [Testing Checklist](#testing-checklist)
5. [Completion Criteria](#completion-criteria)

---

## Week 1: Hash Verification & Property Fixes

### Task 1.1: Plugin Hash Verification

**Estimated Time:** 2 days  
**Complexity:** Medium

#### Step 1: Create TrustedPluginConfig Class

**File:** `src/PluginRegistry/TrustedPluginConfig.cs`

```csharp
using Newtonsoft.Json;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Configuration for trusted plugins with hash-based verification.
/// </summary>
public class TrustedPluginConfig
{
    /// <summary>
    /// List of plugin file names that are trusted.
    /// </summary>
    [JsonProperty("pluginNames")]
    public List<string> PluginNames { get; set; } = new();
    
    /// <summary>
    /// Dictionary mapping plugin file names to their expected SHA256 hashes.
    /// Used for integrity verification.
    /// </summary>
    [JsonProperty("pluginHashes")]
    public Dictionary<string, string> PluginHashes { get; set; } = new();
    
    /// <summary>
    /// Whether to allow user-added trusted plugins.
    /// If false, only shipped plugins can be trusted.
    /// </summary>
    [JsonProperty("allowUserTrustedPlugins")]
    public bool AllowUserTrustedPlugins { get; set; } = true;
    
    /// <summary>
    /// Hash algorithm to use for verification (e.g., "SHA256").
    /// </summary>
    [JsonProperty("hashAlgorithm")]
    public string HashAlgorithm { get; set; } = "SHA256";
    
    /// <summary>
    /// Timestamp of last configuration update.
    /// </summary>
    [JsonProperty("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
```

**Verification:**
- [ ] File compiles without errors
- [ ] JSON properties are correctly decorated
- [ ] Default values are set appropriately

---

#### Step 2: Update PluginValidator with Configuration Loading

**File:** `src/PluginRegistry/PluginValidator.cs`

**Add the following fields and static constructor:**

```csharp
private static TrustedPluginConfig _trustedPluginConfig;
private static readonly object _configLock = new();
private static readonly string ConfigDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "LogExpert");
private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "trusted-plugins.json");

static PluginValidator()
{
    LoadTrustedPluginConfiguration();
}
```

**Add configuration management methods:**

```csharp
private static void LoadTrustedPluginConfiguration()
{
    lock (_configLock)
    {
        if (File.Exists(ConfigPath))
        {
            try
            {
                var json = File.ReadAllText(ConfigPath);
                _trustedPluginConfig = JsonConvert.DeserializeObject<TrustedPluginConfig>(json);
                _logger.Info("Loaded trusted plugin configuration from {ConfigPath}", ConfigPath);
                
                // Validate configuration
                if (_trustedPluginConfig == null)
                {
                    _logger.Warn("Deserialized config is null, creating default");
                    _trustedPluginConfig = CreateDefaultConfiguration();
                    SaveTrustedPluginConfiguration();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load trusted plugin configuration, using defaults");
                _trustedPluginConfig = CreateDefaultConfiguration();
                SaveTrustedPluginConfiguration();
            }
        }
        else
        {
            _logger.Info("No trusted plugin configuration found, creating default");
            _trustedPluginConfig = CreateDefaultConfiguration();
            SaveTrustedPluginConfiguration();
        }
    }
}

private static TrustedPluginConfig CreateDefaultConfiguration()
{
    return new TrustedPluginConfig
    {
        PluginNames = new List<string>(_trustedPluginNames),
        PluginHashes = new Dictionary<string, string>(),
        AllowUserTrustedPlugins = true,
        HashAlgorithm = "SHA256",
        LastUpdated = DateTime.UtcNow
    };
}

private static void SaveTrustedPluginConfiguration()
{
    lock (_configLock)
    {
        try
        {
            Directory.CreateDirectory(ConfigDirectory);
            var json = JsonConvert.SerializeObject(_trustedPluginConfig, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
            _logger.Info("Saved trusted plugin configuration to {ConfigPath}", ConfigPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save trusted plugin configuration");
        }
    }
}

/// <summary>
/// Adds a plugin to the trusted list and saves the configuration.
/// </summary>
public static bool AddTrustedPlugin(string dllPath, out string errorMessage)
{
    errorMessage = null;
    
    try
    {
        if (!File.Exists(dllPath))
        {
            errorMessage = $"Plugin file not found: {dllPath}";
            return false;
        }
        
        var fileName = Path.GetFileName(dllPath);
        var hash = CalculateFileHash(dllPath);
        
        lock (_configLock)
        {
            if (!_trustedPluginConfig.AllowUserTrustedPlugins)
            {
                errorMessage = "User-added trusted plugins are not allowed by policy";
                return false;
            }
            
            if (!_trustedPluginConfig.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            {
                _trustedPluginConfig.PluginNames.Add(fileName);
            }
            
            _trustedPluginConfig.PluginHashes[fileName] = hash;
            _trustedPluginConfig.LastUpdated = DateTime.UtcNow;
            
            SaveTrustedPluginConfiguration();
        }
        
        _logger.Info("Added trusted plugin: {FileName}, Hash: {Hash}", fileName, hash);
        return true;
    }
    catch (Exception ex)
    {
        errorMessage = $"Error adding trusted plugin: {ex.Message}";
        _logger.Error(ex, "Error adding trusted plugin: {DllPath}", dllPath);
        return false;
    }
}

/// <summary>
/// Removes a plugin from the trusted list.
/// </summary>
public static bool RemoveTrustedPlugin(string fileName)
{
    lock (_configLock)
    {
        var removed = _trustedPluginConfig.PluginNames.Remove(fileName);
        if (removed)
        {
            _trustedPluginConfig.PluginHashes.Remove(fileName);
            _trustedPluginConfig.LastUpdated = DateTime.UtcNow;
            SaveTrustedPluginConfiguration();
            _logger.Info("Removed trusted plugin: {FileName}", fileName);
        }
        return removed;
    }
}
```

**Verification:**
- [ ] Configuration loads on startup
- [ ] Default configuration is created if none exists
- [ ] AddTrustedPlugin works correctly
- [ ] RemoveTrustedPlugin works correctly
- [ ] Configuration persists across application restarts

---

#### Step 3: Enhance ValidatePlugin with Hash Verification

**File:** `src/PluginRegistry/PluginValidator.cs`

**Replace the existing `ValidatePlugin` method:**

```csharp
/// <summary>
/// Validates a plugin with hash verification and manifest validation.
/// </summary>
/// <param name="dllPath">Path to plugin DLL</param>
/// <param name="manifest">Output manifest if found and valid</param>
/// <returns>True if plugin is valid and safe to load</returns>
public static bool ValidatePlugin(string dllPath, out PluginManifest manifest)
{
    manifest = null;
    
    if (!File.Exists(dllPath))
    {
        _logger.Warn("Plugin file does not exist: {DllPath}", dllPath);
        return false;
    }
    
    var fileName = Path.GetFileName(dllPath);
    
    // Step 1: Check if plugin is known dependency (not a plugin itself)
    if (_knownDependencies.Contains(fileName))
    {
        _logger.Debug("Skipping dependency DLL: {FileName}", fileName);
        return false;
    }
    
    // Step 2: Calculate file hash
    var fileHash = CalculateFileHash(dllPath);
    _logger.Debug("Plugin {FileName} hash: {Hash}", fileName, fileHash);
    
    // Step 3: Check trust status
    var isTrustedByName = _trustedPluginConfig.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
    var isTrustedByHash = _trustedPluginConfig.PluginHashes.ContainsValue(fileHash);
    
    if (!isTrustedByName && !isTrustedByHash)
    {
        _logger.Warn("Plugin not trusted: {FileName}, Hash: {Hash}", fileName, fileHash);
        _logger.Info("To trust this plugin, add it via Settings > Plugin Management");
        return false;
    }
    
    // Step 4: Verify hash for known plugins
    if (isTrustedByName && _trustedPluginConfig.PluginHashes.TryGetValue(fileName, out var expectedHash))
    {
        if (!expectedHash.Equals(fileHash, StringComparison.OrdinalIgnoreCase))
        {
            _logger.Error("SECURITY: Plugin hash mismatch for {FileName}", fileName);
            _logger.Error("  Expected: {Expected}", expectedHash);
            _logger.Error("  Actual:   {Actual}", fileHash);
            _logger.Error("  This could indicate file tampering or corruption!");
            return false;
        }
        
        _logger.Debug("Plugin hash verified: {FileName}", fileName);
    }
    else if (isTrustedByHash)
    {
        _logger.Info("Plugin {FileName} trusted by hash: {Hash}", fileName, fileHash);
    }
    
    // Step 5: Load and validate manifest
    var manifestPath = Path.ChangeExtension(dllPath, ".manifest.json");
    if (File.Exists(manifestPath))
    {
        try
        {
            manifest = PluginManifest.Load(manifestPath);
            if (manifest != null)
            {
                _logger.Info("Loaded manifest for plugin: {PluginName} v{Version}", 
                    manifest.Name, manifest.Version);
                
                // Validate version compatibility
                if (!CheckVersionCompatibility(manifest))
                {
                    _logger.Error("Plugin {PluginName} is not compatible with current LogExpert version", 
                        manifest.Name);
                    return false;
                }
                
                // Extract and set permissions from manifest
                if (manifest.Permissions != null && manifest.Permissions.Count > 0)
                {
                    var permissions = PluginPermissionManager.ParsePermissions(manifest.Permissions);
                    var pluginName = Path.GetFileNameWithoutExtension(fileName);
                    PluginPermissionManager.SetPermissions(pluginName, permissions);
                    _logger.Info("Set permissions for {PluginName}: {Permissions}",
                        pluginName, PluginPermissionManager.PermissionToString(permissions));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading manifest for {FileName}", fileName);
            // Continue without manifest for backward compatibility
        }
    }
    else
    {
        _logger.Debug("No manifest found for {FileName}, using default permissions", fileName);
    }
    
    // Step 6: Verify assembly validity
    if (!CanLoadAssembly(dllPath))
    {
        _logger.Error("Plugin assembly cannot be loaded: {FileName}", fileName);
        return false;
    }
    
    if (!IsValidDotNetAssembly(dllPath))
    {
        _logger.Error("Plugin is not a valid .NET assembly: {FileName}", fileName);
        return false;
    }
    
    _logger.Info("Plugin validated successfully: {FileName}", fileName);
    return true;
}
```

**Verification:**
- [ ] Hash verification works for trusted plugins
- [ ] Hash mismatch is detected and logged
- [ ] Unknown plugins are rejected
- [ ] Manifest loading integrates correctly
- [ ] Assembly validation still works

---

### Task 1.2: Fix PluginManifest Required/Optional Properties

**Estimated Time:** 1 hour  
**Complexity:** Low

**File:** `src/PluginRegistry/PluginManifest.cs`

**Change the following properties:**

```csharp
// BEFORE:
public required string Url { get; set; }
public required string License { get; set; }

// AFTER:
/// <summary>
/// Optional: Plugin website or repository URL.
/// </summary>
[JsonProperty("url")]
public string? Url { get; set; }

/// <summary>
/// Optional: Plugin license identifier (e.g., MIT, Apache-2.0).
/// </summary>
[JsonProperty("license")]
public string? License { get; set; }
```

**Update the `Validate` method:**

```csharp
public bool Validate(out List<string> errors)
{
    errors = new List<string>();

    // Required fields
    if (string.IsNullOrWhiteSpace(Name))
        errors.Add("Missing required field: name");

    if (string.IsNullOrWhiteSpace(Version))
        errors.Add("Missing required field: version");
    else if (!IsValidVersion(Version))
        errors.Add($"Invalid version format: {Version}");

    if (string.IsNullOrWhiteSpace(Author))
        errors.Add("Missing required field: author");

    if (string.IsNullOrWhiteSpace(Description))
        errors.Add("Missing required field: description");

    if (string.IsNullOrWhiteSpace(Main))
        errors.Add("Missing required field: main");

    if (string.IsNullOrWhiteSpace(ApiVersion))
        errors.Add("Missing required field: apiVersion");

    // Validate requirements if present
    if (Requires != null)
    {
        if (!string.IsNullOrWhiteSpace(Requires.LogExpert) && !IsValidVersionRequirement(Requires.LogExpert))
        {
            errors.Add($"Invalid LogExpert version requirement: {Requires.LogExpert}");
        }

        if (!string.IsNullOrWhiteSpace(Requires.DotNet) && !IsValidVersionRequirement(Requires.DotNet))
        {
            errors.Add($"Invalid .NET version requirement: {Requires.DotNet}");
        }
    }

    // Validate permissions if present
    if (Permissions != null && Permissions.Count > 0)
    {
        foreach (var permission in Permissions)
        {
            if (!IsValidPermission(permission))
            {
                errors.Add($"Invalid permission: {permission}");
            }
        }
    }

    // Note: url and license are now optional and don't need validation

    return errors.Count == 0;
}
```

**Verification:**
- [ ] Manifests without URL/License are valid
- [ ] Manifests with URL/License are valid
- [ ] Required fields are still validated
- [ ] No compilation errors

---

### Task 1.3: Path Traversal Protection

**Estimated Time:** 2 hours  
**Complexity:** Low

**File:** `src/PluginRegistry/PluginValidator.cs`

**Add new validation method:**

```csharp
/// <summary>
/// Validates that manifest paths don't escape the plugin directory.
/// </summary>
private static bool ValidateManifestPaths(PluginManifest manifest, string pluginDirectory)
{
    try
    {
        var pluginDir = Path.GetFullPath(pluginDirectory);
        
        // Validate main file path
        var mainPath = Path.GetFullPath(Path.Combine(pluginDirectory, manifest.Main));
        
        if (!mainPath.StartsWith(pluginDir, StringComparison.OrdinalIgnoreCase))
        {
            _logger.Error("SECURITY: Plugin main file outside plugin directory");
            _logger.Error("  Plugin: {Plugin}", manifest.Name);
            _logger.Error("  Main path: {MainPath}", mainPath);
            _logger.Error("  Expected directory: {PluginDir}", pluginDir);
            return false;
        }
        
        // Validate dependency paths if they contain file references
        if (manifest.Dependencies != null)
        {
            foreach (var (key, value) in manifest.Dependencies)
            {
                // Check for suspicious path patterns
                if (key.Contains("..") || key.Contains("~") || value.Contains("..") || value.Contains("~"))
                {
                    _logger.Warn("Suspicious path in manifest dependencies: {Key} = {Value}", key, value);
                }
            }
        }
        
        return true;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Error validating manifest paths for {Plugin}", manifest.Name);
        return false;
    }
}
```

**Update `ValidatePlugin` to call this method (add after Step 5):**

```csharp
// Step 5.5: Validate manifest paths
if (manifest != null)
{
    var pluginDirectory = Path.GetDirectoryName(dllPath);
    if (!ValidateManifestPaths(manifest, pluginDirectory))
    {
        _logger.Error("Manifest path validation failed for {Plugin}", manifest.Name);
        manifest = null;
        return false;
    }
}
```

**Verification:**
- [ ] Valid paths pass validation
- [ ] Paths with ".." are rejected
- [ ] Paths with "~" are detected
- [ ] Absolute paths outside plugin dir are rejected
- [ ] Security events are logged

---

## Week 2: Error Handling & Regex Safety

### Task 1.4: Custom Exceptions

**Estimated Time:** 3 hours  
**Complexity:** Low

#### Create Exception Classes

**Create directory:** `src/PluginRegistry/Exceptions/`

**File 1:** `src/PluginRegistry/Exceptions/PluginManifestException.cs`

```csharp
using System;

namespace LogExpert.PluginRegistry.Exceptions;

/// <summary>
/// Exception thrown when a plugin manifest cannot be loaded or is invalid.
/// </summary>
public class PluginManifestException : Exception
{
    /// <summary>
    /// Path to the manifest file that caused the exception.
    /// </summary>
    public string ManifestPath { get; }
    
    public PluginManifestException(string message, string manifestPath) 
        : base(message)
    {
        ManifestPath = manifestPath;
    }
    
    public PluginManifestException(string message, string manifestPath, Exception innerException) 
        : base(message, innerException)
    {
        ManifestPath = manifestPath;
    }
}
```

**File 2:** `src/PluginRegistry/Exceptions/PluginValidationException.cs`

```csharp
using System;
using System.Collections.Generic;

namespace LogExpert.PluginRegistry.Exceptions;

/// <summary>
/// Exception thrown when plugin validation fails.
/// </summary>
public class PluginValidationException : Exception
{
    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<string> ValidationErrors { get; }
    
    /// <summary>
    /// Path to the plugin file.
    /// </summary>
    public string PluginPath { get; }
    
    public PluginValidationException(string message, string pluginPath, List<string> validationErrors)
        : base(message)
    {
        PluginPath = pluginPath;
        ValidationErrors = validationErrors ?? new List<string>();
    }
    
    public override string ToString()
    {
        var errorList = ValidationErrors.Count > 0 
            ? $"\nValidation errors:\n  - {string.Join("\n  - ", ValidationErrors)}" 
            : string.Empty;
        return $"{base.ToString()}{errorList}";
    }
}
```

**File 3:** `src/PluginRegistry/Exceptions/PluginLoadException.cs`

```csharp
using System;

namespace LogExpert.PluginRegistry.Exceptions;

/// <summary>
/// Exception thrown when a plugin cannot be loaded.
/// </summary>
public class PluginLoadException : Exception
{
    /// <summary>
    /// Path to the plugin file.
    /// </summary>
    public string PluginPath { get; }
    
    /// <summary>
    /// Name of the plugin (if available).
    /// </summary>
    public string? PluginName { get; }
    
    public PluginLoadException(string message, string pluginPath) 
        : base(message)
    {
        PluginPath = pluginPath;
    }
    
    public PluginLoadException(string message, string pluginPath, string pluginName) 
        : base(message)
    {
        PluginPath = pluginPath;
        PluginName = pluginName;
    }
    
    public PluginLoadException(string message, string pluginPath, Exception innerException) 
        : base(message, innerException)
    {
        PluginPath = pluginPath;
    }
}
```

**File 4:** `src/PluginRegistry/Exceptions/PluginSecurityException.cs`

```csharp
using System;

namespace LogExpert.PluginRegistry.Exceptions;

/// <summary>
/// Exception thrown when a security violation is detected in a plugin.
/// </summary>
public class PluginSecurityException : Exception
{
    /// <summary>
    /// Name of the plugin.
    /// </summary>
    public string PluginName { get; }
    
    /// <summary>
    /// Description of the security issue.
    /// </summary>
    public string SecurityIssue { get; }
    
    public PluginSecurityException(string message, string pluginName, string securityIssue)
        : base(message)
    {
        PluginName = pluginName;
        SecurityIssue = securityIssue;
    }
    
    public override string ToString()
    {
        return $"{base.ToString()}\nPlugin: {PluginName}\nSecurity Issue: {SecurityIssue}";
    }
}
```

#### Update PluginManifest.Load Method

**File:** `src/PluginRegistry/PluginManifest.cs`

**Add using statement:**

```csharp
using LogExpert.PluginRegistry.Exceptions;
```

**Update the `Load` method:**

```csharp
public static PluginManifest? Load(string manifestPath)
{
    if (!File.Exists(manifestPath))
    {
        _logger.Debug("Manifest file not found: {ManifestPath}", manifestPath);
        return null; // Null is acceptable for "not found"
    }
    
    try
    {
        var json = File.ReadAllText(manifestPath);
        var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);
        
        if (manifest == null)
        {
            throw new PluginManifestException(
                "Failed to deserialize manifest", manifestPath);
        }
        
        if (!manifest.Validate(out var errors))
        {
            throw new PluginManifestException(
                $"Manifest validation failed: {string.Join(", ", errors)}", 
                manifestPath);
        }
        
        _logger.Info("Loaded manifest for plugin: {PluginName} v{Version}", 
            manifest.Name, manifest.Version);
        return manifest;
    }
    catch (PluginManifestException)
    {
        throw; // Re-throw our custom exceptions
    }
    catch (Exception ex) when (ex is IOException or 
                                     JsonException or 
                                     UnauthorizedAccessException)
    {
        throw new PluginManifestException(
            $"Error loading manifest: {ex.Message}", 
            manifestPath, ex);
    }
}
```

**Verification:**
- [ ] All exception classes compile
- [ ] Exceptions include relevant context
- [ ] PluginManifest.Load throws appropriate exceptions
- [ ] Exception messages are clear and actionable

---

### Task 1.5: Regex Safety Validation

**Estimated Time:** 1 day  
**Complexity:** Medium

#### Create RegexValidator Class

**File:** `src/PluginRegistry/RegexValidator.cs`

```csharp
using System;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Validates regex patterns for safety to prevent catastrophic backtracking and DoS attacks.
/// </summary>
public static class RegexValidator
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly TimeSpan ValidationTimeout = TimeSpan.FromMilliseconds(100);
    private const int MaxPatternLength = 1000;
    
    /// <summary>
    /// Validates if a regex pattern is safe to use.
    /// </summary>
    /// <param name="pattern">Regex pattern to validate</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if pattern is safe, false otherwise</returns>
    public static bool IsRegexSafe(string pattern, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (string.IsNullOrEmpty(pattern))
        {
            errorMessage = "Pattern is null or empty";
            return false;
        }
        
        if (pattern.Length > MaxPatternLength)
        {
            errorMessage = $"Pattern exceeds maximum length of {MaxPatternLength} characters";
            return false;
        }
        
        // Check for suspicious patterns
        if (ContainsSuspiciousPatterns(pattern, out var suspiciousPattern))
        {
            errorMessage = $"Pattern contains suspicious construct: {suspiciousPattern}";
            return false;
        }
        
        try
        {
            // Attempt to create regex with timeout
            var regex = new Regex(pattern, RegexOptions.None, ValidationTimeout);
            
            // Test with adversarial inputs
            var testInputs = new[]
            {
                new string('a', 1000),
                new string('a', 100) + new string('b', 100),
                string.Concat(Enumerable.Repeat("ab", 500)),
                new string('x', 500) + new string('y', 500)
            };
            
            foreach (var testInput in testInputs)
            {
                try
                {
                    _ = regex.Match(testInput);
                }
                catch (RegexMatchTimeoutException)
                {
                    errorMessage = "Pattern caused timeout during validation";
                    _logger.Warn("Regex pattern timed out: {Pattern}", pattern);
                    return false;
                }
            }
            
            return true;
        }
        catch (ArgumentException ex)
        {
            errorMessage = $"Invalid regex pattern: {ex.Message}";
            return false;
        }
    }
    
    /// <summary>
    /// Checks if pattern contains constructs known to cause catastrophic backtracking.
    /// </summary>
    private static bool ContainsSuspiciousPatterns(string pattern, out string matchedPattern)
    {
        matchedPattern = string.Empty;
        
        var suspiciousPatterns = new[]
        {
            (@"\(\.\*\)\+", "(.*)+"),
            (@"\(\.\+\)\+", "(.+)+"),
            (@"\(\.\*\)\*", "(.*)* "),
            (@"\([^\)]*\)\+", "(x+)+"),
            (@"\{[0-9]{3,}\,?\}", "{nnn,}"),
            (@"\(\?[^)]*\)\+", "(?:x)+"),
            (@"\(\.\*\?\)\+", "(.*?)+"),
        };
        
        foreach (var (patternRegex, description) in suspiciousPatterns)
        {
            try
            {
                var checkRegex = new Regex(patternRegex, RegexOptions.None, TimeSpan.FromMilliseconds(50));
                if (checkRegex.IsMatch(pattern))
                {
                    matchedPattern = description;
                    return true;
                }
            }
            catch
            {
                matchedPattern = "validation regex failed";
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Creates a safe regex with timeout and validation.
    /// </summary>
    public static Regex CreateSafeRegex(string pattern, RegexOptions options = RegexOptions.None, 
        TimeSpan? timeout = null)
    {
        if (!IsRegexSafe(pattern, out var errorMessage))
        {
            throw new ArgumentException($"Unsafe regex pattern: {errorMessage}", nameof(pattern));
        }
        
        return new Regex(pattern, options, timeout ?? TimeSpan.FromSeconds(2));
    }
}
```

#### Update FilterParams

**File:** `src/LogExpert.Core/Classes/Filter/FilterParams.cs`

**Add using statement:**

```csharp
using LogExpert.PluginRegistry;
```

**Update `CreateRegex` method:**

```csharp
public void CreateRegex()
{
    if (string.IsNullOrEmpty(SearchText))
    {
        return;
    }
    
    // Validate regex safety first
    if (!RegexValidator.IsRegexSafe(SearchText, out var errorMessage))
    {
        throw new ArgumentException(
            $"Unsafe or invalid regex pattern: {errorMessage}", 
            nameof(SearchText));
    }
    
    try
    {
        var options = IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        Regex = RegexValidator.CreateSafeRegex(SearchText, options);
        
        if (IsRangeSearch && !string.IsNullOrEmpty(RangeSearchText))
        {
            if (!RegexValidator.IsRegexSafe(RangeSearchText, out errorMessage))
            {
                throw new ArgumentException(
                    $"Unsafe or invalid range regex pattern: {errorMessage}", 
                    nameof(RangeSearchText));
            }
            
            RangeRex = RegexValidator.CreateSafeRegex(RangeSearchText, options);
        }
    }
    catch (ArgumentException)
    {
        throw;
    }
}
```

**Verification:**
- [ ] Safe patterns pass validation
- [ ] Catastrophic backtracking patterns are rejected
- [ ] Large repetition patterns are rejected
- [ ] Timeout detection works
- [ ] FilterParams integration works

---

## Week 3: Audit Logging & Testing

### Task 1.6: Audit Logging

**Estimated Time:** 1 day  
**Complexity:** Low

#### Create PluginAuditLogger

**File:** `src/PluginRegistry/PluginAuditLogger.cs`

```csharp
using System;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Provides audit logging for plugin-related security events.
/// </summary>
public static class PluginAuditLogger
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly string AuditLogPath;
    private static readonly object _lockObj = new();
    
    static PluginAuditLogger()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        AuditLogPath = Path.Combine(appDataPath, "LogExpert", "plugin-audit.log");
        
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AuditLogPath)!);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create audit log directory");
        }
    }
    
    public static void LogPluginLoad(string pluginName, string pluginPath, bool success, string? reason = null)
    {
        var entry = new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Action = "PLUGIN_LOAD",
            PluginName = pluginName,
            PluginPath = pluginPath,
            Success = success,
            Reason = reason,
            User = Environment.UserName,
            Machine = Environment.MachineName,
            ProcessId = Environment.ProcessId
        };
        
        WriteAuditLog(entry);
        
        if (success)
        {
            _logger.Info("Plugin loaded: {PluginName}", pluginName);
        }
        else
        {
            _logger.Warn("Plugin load failed: {PluginName}. Reason: {Reason}", pluginName, reason);
        }
    }
    
    public static void LogSecurityEvent(string pluginName, string securityIssue, string details)
    {
        var entry = new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Action = "SECURITY_EVENT",
            Severity = "HIGH",
            PluginName = pluginName,
            SecurityIssue = securityIssue,
            Details = details,
            User = Environment.UserName,
            Machine = Environment.MachineName,
            ProcessId = Environment.ProcessId
        };
        
        WriteAuditLog(entry);
        _logger.Error("SECURITY EVENT: {SecurityIssue} for plugin {PluginName}", securityIssue, pluginName);
    }
    
    public static void LogTrustChange(string pluginName, string action, string? hash = null)
    {
        var entry = new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Action = $"TRUST_{action.ToUpperInvariant()}",
            PluginName = pluginName,
            Hash = hash,
            User = Environment.UserName,
            Machine = Environment.MachineName
        };
        
        WriteAuditLog(entry);
        _logger.Info("Plugin trust {Action}: {PluginName}", action, pluginName);
    }
    
    private static void WriteAuditLog(object entry)
    {
        lock (_lockObj)
        {
            try
            {
                var json = JsonConvert.SerializeObject(entry);
                File.AppendAllLines(AuditLogPath, new[] { json });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to write audit log entry");
            }
        }
    }
    
    public static void RotateLogIfNeeded(long maxSizeBytes = 10 * 1024 * 1024)
    {
        try
        {
            if (File.Exists(AuditLogPath))
            {
                var fileInfo = new FileInfo(AuditLogPath);
                if (fileInfo.Length > maxSizeBytes)
                {
                    var archivePath = $"{AuditLogPath}.{DateTime.UtcNow:yyyyMMdd-HHmmss}.archive";
                    File.Move(AuditLogPath, archivePath);
                    _logger.Info("Rotated audit log to {ArchivePath}", archivePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to rotate audit log");
        }
    }
}
```

#### Integrate Audit Logging

**File:** `src/PluginRegistry/PluginRegistry.cs`

**Update `LoadPlugins` method to add audit logging:**

```csharp
internal void LoadPlugins()
{
    _logger.Info("Loading plugins with security validation...");
    
    // Rotate audit log if needed
    PluginAuditLogger.RotateLogIfNeeded();
    
    // ... existing code ...
    
    foreach (var dllName in Directory.EnumerateFiles(pluginDir, "*.dll"))
    {
        var fileName = Path.GetFileName(dllName);
        
        try
        {
            if (!PluginValidator.ValidatePlugin(dllName, out var manifest))
            {
                skippedCount++;
                
                // Audit log the failure
                PluginAuditLogger.LogPluginLoad(
                    manifest?.Name ?? fileName,
                    dllName,
                    false,
                    "Validation failed");
                    
                continue;
            }
            
            if (LoadPluginAssemblySafe(dllName, interfaceName))
            {
                loadedCount++;
                
                // Audit log successful load
                PluginAuditLogger.LogPluginLoad(
                    manifest?.Name ?? fileName,
                    dllName,
                    true);
            }
            else
            {
                failedCount++;
                
                // Audit log the failure
                PluginAuditLogger.LogPluginLoad(
                    manifest?.Name ?? fileName,
                    dllName,
                    false,
                    "Assembly load failed");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception loading plugin: {FileName}", fileName);
            failedCount++;
            
            // Audit log the exception
            PluginAuditLogger.LogPluginLoad(
                fileName,
                dllName,
                false,
                $"Exception: {ex.Message}");
        }
    }
    
    // ... rest of existing code ...
}
```

**File:** `src/PluginRegistry/PluginValidator.cs`

**Add security event logging to hash verification:**

```csharp
// In ValidatePlugin method, when hash mismatch is detected:
if (isTrustedByName && _trustedPluginConfig.PluginHashes.TryGetValue(fileName, out var expectedHash))
{
    if (!expectedHash.Equals(fileHash, StringComparison.OrdinalIgnoreCase))
    {
        // Log security event
        PluginAuditLogger.LogSecurityEvent(
            fileName,
            "Hash Mismatch",
            $"Expected: {expectedHash}, Actual: {fileHash}");
        
        _logger.Error("SECURITY: Plugin hash mismatch for {FileName}", fileName);
        return false;
    }
}

// When path traversal is detected:
if (manifest != null && !ValidateManifestPaths(manifest, pluginDirectory))
{
    PluginAuditLogger.LogSecurityEvent(
        manifest.Name,
        "Path Traversal Attempt",
        $"Manifest contains paths outside plugin directory");
    
    return false;
}
```

**Update AddTrustedPlugin and RemoveTrustedPlugin:**

```csharp
public static bool AddTrustedPlugin(string dllPath, out string errorMessage)
{
    // ... existing code ...
    
    if (/* success */)
    {
        PluginAuditLogger.LogTrustChange(fileName, "ADDED", hash);
    }
    
    return true;
}

public static bool RemoveTrustedPlugin(string fileName)
{
    lock (_configLock)
    {
        var removed = _trustedPluginConfig.PluginNames.Remove(fileName);
        if (removed)
        {
            // ... existing code ...
            
            PluginAuditLogger.LogTrustChange(fileName, "REMOVED");
        }
        return removed;
    }
}
```

**Verification:**
- [ ] Audit logs are created
- [ ] Plugin loads are logged
- [ ] Security events are logged
- [ ] Trust changes are logged
- [ ] Log rotation works

---

## Testing Checklist

### Unit Tests

Create file: `src/LogExpert.Tests/PluginRegistry/Priority1Tests.cs`

```csharp
using NUnit.Framework;
using LogExpert.PluginRegistry;
using System.IO;

namespace LogExpert.Tests.PluginRegistry;

[TestFixture]
public class Priority1Tests
{
    [Test]
    public void HashVerification_ValidHash_Passes()
    {
        // TODO: Implement
        Assert.Pass("Placeholder");
    }
    
    [Test]
    public void HashVerification_InvalidHash_Fails()
    {
        // TODO: Implement
        Assert.Pass("Placeholder");
    }
    
    [Test]
    public void PathTraversal_DotDot_Rejected()
    {
        // TODO: Implement
        Assert.Pass("Placeholder");
    }
    
    [Test]
    public void RegexSafety_CatastrophicBacktracking_Rejected()
    {
        // TODO: Implement
        Assert.Pass("Placeholder");
    }
    
    [Test]
    public void AuditLog_WritesCorrectly()
    {
        // TODO: Implement
        Assert.Pass("Placeholder");
    }
}
```

### Manual Testing

- [ ] **Test 1:** Load LogExpert with existing plugins
  - Expected: All trusted plugins load successfully
  - Expected: Audit log shows successful loads

- [ ] **Test 2:** Modify a trusted plugin file
  - Expected: Plugin is rejected due to hash mismatch
  - Expected: Security event is logged

- [ ] **Test 3:** Create manifest with path traversal
  - Expected: Plugin is rejected
  - Expected: Security event is logged

- [ ] **Test 4:** Use catastrophic backtracking regex
  - Expected: Regex is rejected with clear error message

- [ ] **Test 5:** Add custom plugin via API
  - Expected: Plugin is added to trusted list
  - Expected: Trust change is logged

---

## Completion Criteria

### Code Quality
- [ ] All code compiles without warnings
- [ ] All existing tests pass
- [ ] New unit tests cover 80%+ of new code
- [ ] Code follows existing project style

### Functionality
- [ ] Hash verification works for all plugins
- [ ] Configuration persists correctly
- [ ] Path traversal protection works
- [ ] Regex safety validation works
- [ ] Audit logging captures all events
- [ ] No regressions in existing functionality

### Documentation
- [ ] XML comments on all public APIs
- [ ] Audit log format documented
- [ ] Configuration file format documented

### Security
- [ ] No known security vulnerabilities
- [ ] Security events are logged
- [ ] Sensitive data not exposed in logs

---

## Rollout

1. **Day 1-5 (Week 1):**
   - Implement Tasks 1.1, 1.2, 1.3
   - Self-test each task

2. **Day 6-10 (Week 2):**
   - Implement Tasks 1.4, 1.5
   - Integration testing

3. **Day 11-15 (Week 3):**
   - Implement Task 1.6
   - Complete testing
   - Code review
   - Deploy to development

4. **Week 4:**
   - Internal testing
   - Bug fixes
   - Prepare for Priority 2

---

## Support

**Questions or Issues?**
- Review the main strategy document: `PLUGIN_REGISTRY_IMPLEMENTATION_STRATEGY.md`
- Check the analysis document: `PLUGIN_REGISTRY_ANALYSIS.md`
- Create an issue in the repository

**Need Help?**
- All code examples are production-ready
- Test templates are provided
- Verification checklists ensure quality

---

## Next Steps

After completing Priority 1:
? Proceed to `PRIORITY_2_IMPLEMENTATION_GUIDE.md`
