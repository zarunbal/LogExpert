# Plugin Hash Verification - Pre-Priority 3 Checklist

## Overview

**Purpose:** Ensure all built-in plugins have hash verification before Priority 3  
**Status:** ? **IN PROGRESS**  
**Priority:** ?? **CRITICAL** (Security requirement)

---

## Why This Matters

### Security Requirements

Before proceeding to Priority 3, we must ensure:

1. ? All built-in plugins have calculated SHA256 hashes
2. ? Hashes are stored in default trusted configuration
3. ? Hash verification is enabled for all plugins
4. ? Tamper detection is functional

### Risk Without Hash Verification

? **Plugin Tampering:** Modified plugins could be loaded  
? **Supply Chain Attack:** Compromised plugins undetected  
? **File Corruption:** Damaged plugins loaded without warning  
? **Security Baseline:** No known-good state to verify against  

---

## Built-in Plugins to Verify

### List from PluginValidator.cs

```csharp
private static readonly HashSet<string> _trustedPluginNames = new()
{
    "AutoColumnizer.dll",
    "CsvColumnizer.dll",
    "JsonColumnizer.dll",
    "JsonCompactColumnizer.dll",
    "RegexColumnizer.dll",
    "Log4jXmlColumnizer.dll",
    "GlassfishColumnizer.dll",
    "DefaultPlugins.dll",
    "FlashIconHighlighter.dll",
    "SftpFileSystem.dll",
    "SftpFileSystemx86.dll",
    "SftpFileSystemx64.dll"
};
```

**Total:** 12 plugins

---

## Implementation Plan

### Step 1: Create Hash Generation Utility ?

**Tool:** Command-line utility or PowerShell script  
**Purpose:** Calculate hashes for all built-in plugins  
**Output:** JSON file with plugin hashes  

### Step 2: Update Default Configuration ?

**File:** `PluginValidator.cs` - `CreateDefaultConfiguration()`  
**Action:** Include pre-calculated hashes in default config  
**Benefit:** Out-of-box hash verification  

### Step 3: Verification Script ?

**Purpose:** Verify all plugins have hashes  
**Check:** No missing entries in configuration  
**Alert:** Fail build if hashes missing  

---

## Implementation

### Hash Generation Tool

Let's create a simple utility to generate hashes for all plugins:

```csharp
// PluginHashGenerator.cs - Utility tool
public static class PluginHashGenerator
{
    public static void GenerateHashesForPlugins(string pluginsDirectory, string outputPath)
    {
        var builtInPlugins = new[]
        {
            "AutoColumnizer.dll",
            "CsvColumnizer.dll",
            "JsonColumnizer.dll",
            "JsonCompactColumnizer.dll",
            "RegexColumnizer.dll",
            "Log4jXmlColumnizer.dll",
            "GlassfishColumnizer.dll",
            "DefaultPlugins.dll",
            "FlashIconHighlighter.dll",
            "SftpFileSystem.dll",
            "SftpFileSystemx86.dll",
            "SftpFileSystemx64.dll"
        };

        var hashes = new Dictionary<string, string>();

        foreach (var plugin in builtInPlugins)
        {
            var pluginPath = Path.Combine(pluginsDirectory, plugin);
            
            if (File.Exists(pluginPath))
            {
                try
                {
                    var hash = PluginHashCalculator.CalculateHash(pluginPath);
                    hashes[plugin] = hash;
                    Console.WriteLine($"? {plugin}: {hash.Substring(0, 32)}...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? {plugin}: ERROR - {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"? {plugin}: FILE NOT FOUND");
            }
        }

        // Save to JSON
        var json = JsonConvert.SerializeObject(new { PluginHashes = hashes }, Formatting.Indented);
        File.WriteAllText(outputPath, json);
        
        Console.WriteLine($"\nHashes saved to: {outputPath}");
        Console.WriteLine($"Total: {hashes.Count}/{builtInPlugins.Length} plugins");
    }
}
```

### PowerShell Script Alternative

```powershell
# GeneratePluginHashes.ps1

param(
    [string]$PluginsDirectory = ".\bin\Release\plugins",
    [string]$OutputFile = "plugin-hashes.json"
)

$builtInPlugins = @(
    "AutoColumnizer.dll",
    "CsvColumnizer.dll",
    "JsonColumnizer.dll",
    "JsonCompactColumnizer.dll",
    "RegexColumnizer.dll",
    "Log4jXmlColumnizer.dll",
    "GlassfishColumnizer.dll",
    "DefaultPlugins.dll",
    "FlashIconHighlighter.dll",
    "SftpFileSystem.dll",
    "SftpFileSystemx86.dll",
    "SftpFileSystemx64.dll"
)

$hashes = @{}

foreach ($plugin in $builtInPlugins) {
    $pluginPath = Join-Path $PluginsDirectory $plugin
    
    if (Test-Path $pluginPath) {
        $hash = Get-FileHash -Path $pluginPath -Algorithm SHA256
        $hashes[$plugin] = $hash.Hash
        Write-Host "? $plugin : $($hash.Hash.Substring(0,32))..." -ForegroundColor Green
    }
    else {
        Write-Host "? $plugin : FILE NOT FOUND" -ForegroundColor Yellow
    }
}

$output = @{
    PluginHashes = $hashes
    GeneratedAt = (Get-Date).ToString("o")
    TotalPlugins = $builtInPlugins.Count
    HashedPlugins = $hashes.Count
} | ConvertTo-Json -Depth 10

$output | Out-File -FilePath $OutputFile -Encoding UTF8

Write-Host "`nHashes saved to: $OutputFile" -ForegroundColor Cyan
Write-Host "Total: $($hashes.Count)/$($builtInPlugins.Count) plugins" -ForegroundColor Cyan
```

---

## Updated Default Configuration

### Current Implementation

```csharp
private static TrustedPluginConfig CreateDefaultConfiguration()
{
    return new TrustedPluginConfig
    {
        PluginNames = new List<string>(_trustedPluginNames),
        PluginHashes = new Dictionary<string, string>(), // ? EMPTY!
        AllowUserTrustedPlugins = true,
        HashAlgorithm = "SHA256",
        LastUpdated = DateTime.UtcNow
    };
}
```

### Required Implementation

```csharp
private static TrustedPluginConfig CreateDefaultConfiguration()
{
    return new TrustedPluginConfig
    {
        PluginNames = new List<string>(_trustedPluginNames),
        PluginHashes = GetBuiltInPluginHashes(), // ? PRE-CALCULATED
        AllowUserTrustedPlugins = true,
        HashAlgorithm = "SHA256",
        LastUpdated = DateTime.UtcNow
    };
}

private static Dictionary<string, string> GetBuiltInPluginHashes()
{
    // Pre-calculated hashes for built-in plugins
    // These should be generated during build and embedded as a resource
    // or loaded from a secure configuration file
    return new Dictionary<string, string>
    {
        // NOTE: These are EXAMPLE hashes - must be calculated from actual built plugins
        ["AutoColumnizer.dll"] = "ABC123...", // Real hash needed
        ["CsvColumnizer.dll"] = "DEF456...",  // Real hash needed
        // ... rest of plugins
    };
}
```

---

## Execution Plan

### Option 1: Manual Hash Generation (Quick)

**Steps:**
1. Build LogExpert in Release mode
2. Navigate to `bin\Release\plugins\`
3. Run PowerShell script to generate hashes
4. Copy hashes into `GetBuiltInPluginHashes()` method
5. Rebuild and test

**Time:** 15-30 minutes  
**Pros:** Quick, simple  
**Cons:** Manual process, error-prone  

### Option 2: Automated Build Integration (Proper)

**Steps:**
1. Create MSBuild task to calculate hashes post-build
2. Generate C# code file with hash constants
3. Include generated file in compilation
4. Automatic hash updates on rebuild

**Time:** 1-2 hours  
**Pros:** Automated, always up-to-date  
**Cons:** More complex setup  

### Option 3: Embedded Resource (Recommended)

**Steps:**
1. Run hash generation tool once
2. Save hashes to `plugin-hashes.json`
3. Embed as resource in PluginRegistry project
4. Load from resource at runtime

**Time:** 30 minutes  
**Pros:** Balance of automation and simplicity  
**Cons:** Requires rebuild when plugins change  

---

## Recommended Approach

### Hybrid Solution

**Phase 1: Immediate (Manual)**
1. ? Run PowerShell script to generate hashes
2. ? Update `GetBuiltInPluginHashes()` with real hashes
3. ? Test hash verification
4. ? Commit to repository

**Phase 2: Future (Automated)**
1. Create MSBuild task for automated hash generation
2. Integrate into CI/CD pipeline
3. Fail build if hash verification fails

---

## Testing Plan

### Verification Tests

1. **All Plugins Have Hashes**
   - Check configuration contains all 12 plugins
   - Verify no empty hash values

2. **Hash Verification Works**
   - Load trusted plugin ? succeeds
   - Modify plugin file ? hash mismatch detected
   - Wrong hash in config ? fails validation

3. **Default Configuration**
   - Fresh install creates config with hashes
   - All built-in plugins load successfully

4. **Tamper Detection**
   - Modify one byte in plugin
   - Verify hash mismatch alert
   - Plugin loading blocked

---

## Acceptance Criteria

### Before Moving to Priority 3

? **All 12 built-in plugins have calculated SHA256 hashes**  
? **Hashes stored in default configuration**  
? **Hash verification tested and working**  
? **Tamper detection confirmed functional**  
? **Documentation updated**  
? **Tests passing**  

### Verification Command

```powershell
# Verify all plugins have hashes
$config = Get-Content "trusted-plugins.json" | ConvertFrom-Json
$expectedPlugins = 12
$actualHashes = $config.PluginHashes.PSObject.Properties.Count

if ($actualHashes -eq $expectedPlugins) {
    Write-Host "? All $expectedPlugins plugins have hashes" -ForegroundColor Green
} else {
    Write-Host "? Only $actualHashes of $expectedPlugins plugins have hashes" -ForegroundColor Red
    exit 1
}
```

---

## Risk Assessment

### Without Hash Verification

| Risk | Severity | Likelihood | Impact |
|------|----------|------------|---------|
| Plugin Tampering | ?? HIGH | Medium | Critical |
| Supply Chain Attack | ?? HIGH | Low | Critical |
| File Corruption | ?? MEDIUM | Medium | Moderate |
| Compliance Failure | ?? MEDIUM | High | Moderate |

### With Hash Verification

| Risk | Severity | Likelihood | Impact |
|------|----------|------------|---------|
| Plugin Tampering | ?? LOW | Low | Minimal |
| Supply Chain Attack | ?? LOW | Low | Minimal |
| File Corruption | ?? LOW | Low | Minimal |
| Compliance Failure | ?? LOW | Low | Minimal |

---

## Next Steps

### Immediate Actions

1. ? **Generate Hashes**
   - Run hash generation tool
   - Capture all plugin hashes

2. ? **Update Code**
   - Implement `GetBuiltInPluginHashes()`
   - Add real hashes to configuration

3. ? **Test**
   - Verify all plugins load
   - Test tamper detection

4. ? **Document**
   - Update security documentation
   - Add hash generation instructions

### Timeline

**Target:** Complete before Priority 3  
**Estimated Time:** 1-2 hours  
**Priority:** ?? **BLOCKING** - Must complete first  

---

## Status Tracking

### Current Status: ? **PENDING**

- [ ] Hash generation tool created
- [ ] Hashes calculated for all plugins
- [ ] Default configuration updated
- [ ] Hash verification tested
- [ ] Tamper detection verified
- [ ] Documentation updated
- [ ] Ready for Priority 3

---

**Last Updated:** January 2025  
**Status:** ? **CRITICAL PRE-PRIORITY 3 REQUIREMENT**  
**Blocker:** Must complete hash verification before Priority 3  
**Next Action:** Generate and store plugin hashes
