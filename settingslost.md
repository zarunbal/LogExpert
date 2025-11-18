# Settings Loss Analysis

## Problem Statement

Users report that `settings.json` is sometimes overwritten with a clean/default settings file after program restart, causing loss of all previously saved configurations.

## Investigation Progress

### Step 1: Initial Code Review - ConfigManager.cs

**File Location**: `src\LogExpert\Config\ConfigManager.cs`

**Key Observations:**

#### 1.1 Settings Load Flow
- Settings loaded in constructor via `Load()` method (line ~134)
- Load process:
  1. Checks for portable mode by looking for `portableMode.json` in `PortableModeDir`
  2. Determines config directory (AppData or application startup path)
  3. Looks for `settings.json` in determined directory
  4. If file exists, calls `LoadOrCreateNew(fileInfo)`
  5. If file doesn't exist, calls `LoadOrCreateNew(null)` which creates new settings

#### 1.2 Critical Code Path - Load() Method (Lines ~134-178)

```csharp
if (!File.Exists(Path.Combine(dir, "settings.json")))
{
    return LoadOrCreateNew(null);  // ⚠️ Creates NEW settings if file not found
}

try
{
    FileInfo fileInfo = new(Path.Combine(dir, "settings.json"));
    return LoadOrCreateNew(fileInfo);
}
catch (IOException ex)
{
    _logger.Error($"File system error: {ex.Message}");
}
catch (UnauthorizedAccessException ex)
{
    _logger.Error($"Access denied: {ex.Message}");
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    _logger.Error($"Unexpected error: {ex.Message}");
}

return LoadOrCreateNew(null);  // ⚠️ FALLBACK: Creates NEW settings on ANY exception
```

**🔴 FINDING #1: Exception Swallowing Leading to Data Loss**
- Any exception during file read (lines 167-177) returns `LoadOrCreateNew(null)`
- This creates brand new Settings object, discarding existing file
- No attempt to recover or preserve existing settings
- User sees no error - settings silently reset

---

### Step 2: Deserialization Error Handling

**File**: `ConfigManager.cs`, `LoadOrCreateNew()` method (lines 186-208)

```csharp
try
{
    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($"{fileInfo.FullName}"));
}
catch (Exception e)
{
    _logger.Error($"Error while deserializing config data: {e}");
    settings = new Settings();  // ⚠️ Silently creates new settings
}
```

**🔴 FINDING #2: Deserialization Failure Creates Empty Settings**
- If JSON deserialization fails (corrupt file, schema change, version mismatch), catch block creates `new Settings()`
- Original corrupted file remains on disk
- New empty settings loaded into memory
- When application saves (on exit or setting change), corrupted file overwritten with empty settings
- **Root Cause Scenario**: Corruption → Load fails → Empty settings in memory → Save overwrites good file

---

### Step 3: Command-Line Config Import at Startup

**File**: `Program.cs`, `Main()` method (lines 58-66)

```csharp
if (configFile.Exists)
{
    FileInfo cfgFileInfo = new(configFile.Value);
    if (cfgFileInfo.Exists)
    {
        ConfigManager.Instance.Import(cfgFileInfo, ExportImportFlags.All);  // Line 66
    }
    else
    {
        MessageBox.Show(@"Config file not found", @"LogExpert");
    }
}
```

**Analysis:**
- `--config` command-line parameter allows importing config at startup
- Import calls `ConfigManager.Instance` which triggers constructor
- Constructor loads existing settings FIRST
- Then Import() called which may overwrite with imported settings
- Import then calls `Save(SettingsFlags.All)` (line 119 of ConfigManager)

**🔴 FINDING #3: Import Operation Saves Immediately**
```csharp
public void Import (FileInfo fileInfo, ExportImportFlags importFlags)
{
    Instance._settings = Instance.Import(Instance._settings, fileInfo, importFlags);
    Save(SettingsFlags.All);  // ⚠️ IMMEDIATE SAVE after import
}
```

**Risk**: If imported config file is corrupt or empty, it overwrites good settings immediately

---

### Step 4: File Write Operation - No Atomic Safety

**File**: `ConfigManager.cs`, `SaveAsJSON()` method (line 345-350)

```csharp
private static void SaveAsJSON (FileInfo fileInfo, Settings settings)
{
    settings.VersionBuild = Assembly.GetExecutingAssembly().GetName().Version.Build;
    var json = JsonConvert.SerializeObject(settings, _jsonSettings);
    File.WriteAllText(fileInfo.FullName, json, Encoding.UTF8);  // ⚠️ Direct overwrite
}
```

**🔴 FINDING #4: No Atomic Write Protection**
- `File.WriteAllText()` directly overwrites `settings.json`
- If application crashes/power loss during write, file is corrupted or truncated
- No temp file + rename pattern for atomic writes
- No backup before overwrite

**Write-Tear Scenario:**
1. Application starts saving settings
2. `File.WriteAllText()` opens file, truncates to 0 bytes
3. Crash/power loss occurs during write
4. Result: `settings.json` is empty or partially written
5. Next startup: LoadOrCreateNew() gets corrupt JSON → returns `new Settings()`
6. Empty settings then overwrite the partial file

---

### Step 5: Save Frequency and Timing

**Multiple save locations found:**

1. **On window close** - `LogTabWindow.cs` line 2051
   ```csharp
   SaveWindowPosition();
   ConfigManager.Save(SettingsFlags.WindowPosition | SettingsFlags.FileHistory);
   ```

2. **On setting changes** - Various locations save immediately:
   - Filter changes (line 4163)
   - Regex history (line 1866)
   - File history (line 848, 1030, 3006)
   - Highlight settings (line 972, 2447)

3. **On application exit with single instance dialog** - `Program.cs` line 137

**🔴 FINDING #5: Frequent Saves Increase Corruption Risk**
- Settings saved on EVERY filter change, search, file open
- Each save is a corruption opportunity (crash, disk full, permission denied)
- No save throttling or batching
- High-frequency operations (filtering 100 files) trigger 100+ saves

---

### Step 6: Locking Analysis

**Lock usage:**
- `_loadSaveLock` object used in `LoadOrCreateNew()` (line 188) and `Save()` (line 305)
- Prevents concurrent load/save within same process

**🔴 FINDING #6: No Cross-Process File Locking**
- Multiple LogExpert instances can run (single-instance mode is optional)
- No file-level locking on `settings.json`
- Two instances can:
  1. Both load same settings
  2. Instance A modifies and saves
  3. Instance B modifies and saves → overwrites A's changes
  4. Last write wins, data loss for first instance's changes

---

## Root Cause Analysis

### Primary Failure Modes

**Mode 1: Deserialization Failure → Empty Settings** (Most Likely)
```
Good settings.json exists
→ Deserialization fails (JSON.NET throws exception)
→ Catch block creates new Settings()
→ Empty settings loaded in memory
→ User changes any setting OR closes application
→ Empty settings.json saved, overwriting good file
→ Data permanently lost
```

**Triggers:**
- File corruption (disk errors, crash during write)
- JSON format changes between versions
- Encoding issues (BOM, UTF-8 vs UTF-16)
- Schema evolution (new required fields)

**Mode 2: Write Interruption → Partial File** (High Risk)
```
Application starts save operation
→ File.WriteAllText truncates settings.json to 0 bytes
→ Crash/power loss/disk full occurs
→ settings.json left empty or partially written
→ Next startup loads corrupt file → deserialization fails → Mode 1 cascade
```

**Mode 3: Multiple Instance Race Condition** (If single-instance disabled)
```
Instance A loads settings
Instance B loads same settings
→ A modifies and saves
→ B modifies and saves (overwrites A)
→ A's changes lost
```

**Mode 4: Import Corruption Cascade**
```
User imports corrupt/empty config via --config parameter
→ Import loads corrupt data
→ Import() immediately calls Save(SettingsFlags.All)
→ Corrupt data overwrites good settings.json
→ All data lost before user can intervene
```

---

## Reproduction Steps

### Scenario A: Simulate Deserialization Failure

1. Start LogExpert, configure settings (filters, highlights, history)
2. Close LogExpert (settings.json saved successfully)
3. Open `settings.json` in text editor
4. Corrupt JSON: Remove closing brace `}` or add invalid character
5. Start LogExpert
6. **Result**: Empty settings loaded, previous configuration lost

### Scenario B: Simulate Write Interruption

1. Start LogExpert with configured settings
2. Attach debugger or use Task Manager to monitor file system
3. Open a new file (triggers save)
4. During save operation, kill process via Task Manager
5. Check `settings.json` - may be 0 bytes or truncated
6. Start LogExpert
7. **Result**: Settings reset to defaults

### Scenario C: Multiple Instance Conflict

1. Disable single-instance mode in settings
2. Start Instance A, add filter "ERROR"
3. Start Instance B, add filter "WARNING"
4. Close Instance A (saves with "ERROR" filter)
5. Close Instance B (saves with "WARNING" filter, overwrites A)
6. **Result**: "ERROR" filter lost

---

## Solutions and Recommendations

### Priority 1: Critical Fixes (Prevent Data Loss)

#### Solution 1: Implement Atomic Write Pattern

Replace direct `File.WriteAllText()` with safe write-temp-rename pattern:

```csharp
private static void SaveAsJSON(FileInfo fileInfo, Settings settings)
{
    settings.VersionBuild = Assembly.GetExecutingAssembly().GetName().Version.Build;
    var json = JsonConvert.SerializeObject(settings, _jsonSettings);
    
    // Write to temp file first
    var tempFile = fileInfo.FullName + ".tmp";
    var backupFile = fileInfo.FullName + ".bak";
    
    try
    {
        // Write to temp file
        File.WriteAllText(tempFile, json, Encoding.UTF8);
        
        // Create backup of existing file
        if (File.Exists(fileInfo.FullName))
        {
            File.Copy(fileInfo.FullName, backupFile, overwrite: true);
        }
        
        // Atomic rename (on Windows, may need P/Invoke MoveFileEx with MOVEFILE_REPLACE_EXISTING)
        File.Move(tempFile, fileInfo.FullName, overwrite: true);
        
        // Keep backup for 1 generation
        // Consider keeping multiple generations or timestamped backups
    }
    catch (Exception ex)
    {
        _logger.Error($"Failed to save settings: {ex}");
        
        // Restore from backup if save failed mid-way
        if (File.Exists(backupFile) && (!File.Exists(fileInfo.FullName) || new FileInfo(fileInfo.FullName).Length == 0))
        {
            File.Copy(backupFile, fileInfo.FullName, overwrite: true);
            _logger.Warn("Settings save failed, restored from backup");
        }
        
        throw; // Re-throw to notify caller
    }
    finally
    {
        // Cleanup temp file
        if (File.Exists(tempFile))
        {
            try { File.Delete(tempFile); } catch { /* ignore */ }
        }
    }
}
```

**Benefits:**
- Write interruption leaves temp file, not corrupted main file
- Backup available for manual recovery
- Atomic rename ensures file always in valid state

#### Solution 2: Robust Deserialization with Recovery

Replace catch-all exception handler with recovery logic:

```csharp
private Settings LoadOrCreateNew(FileInfo fileInfo)
{
    lock (_loadSaveLock)
    {
        Settings settings = null;
        Exception loadException = null;

        if (fileInfo != null && fileInfo.Exists)
        {
            // Try loading main file
            try
            {
                var json = File.ReadAllText(fileInfo.FullName);
                settings = JsonConvert.DeserializeObject<Settings>(json, _jsonSettings);
                _logger.Info("Settings loaded successfully");
            }
            catch (Exception e)
            {
                _logger.Error($"Error deserializing settings.json: {e}");
                loadException = e;
                
                // Try loading from backup
                var backupFile = fileInfo.FullName + ".bak";
                if (File.Exists(backupFile))
                {
                    try
                    {
                        _logger.Warn("Attempting to load from backup file");
                        var json = File.ReadAllText(backupFile);
                        settings = JsonConvert.DeserializeObject<Settings>(json, _jsonSettings);
                        _logger.Info("Settings recovered from backup");
                        
                        // Show user notification
                        MessageBox.Show(
                            "Settings file was corrupted but recovered from backup.\n" +
                            $"Original error: {e.Message}\n\n" +
                            "A copy of the corrupted file has been saved as settings.json.corrupt",
                            "Settings Recovered",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        
                        // Save corrupted file for analysis
                        File.Copy(fileInfo.FullName, fileInfo.FullName + ".corrupt", overwrite: true);
                    }
                    catch (Exception backupEx)
                    {
                        _logger.Error($"Backup file also corrupted: {backupEx}");
                    }
                }
            }
        }

        // If all loading attempts failed, ask user before creating new settings
        if (settings == null)
        {
            if (loadException != null)
            {
                var result = MessageBox.Show(
                    "Failed to load settings file. All configuration will be lost if you continue.\n\n" +
                    $"Error: {loadException.Message}\n\n" +
                    "Do you want to:\n" +
                    "YES - Create new settings (loses all configuration)\n" +
                    "NO - Exit application (allows manual recovery)",
                    "Critical: Settings Load Failed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                
                if (result == DialogResult.No)
                {
                    // Exit gracefully without saving
                    Environment.Exit(1);
                }
            }
            
            _logger.Warn("Creating new default settings");
            settings = new Settings();
        }

        // ... rest of initialization code ...
        
        return settings;
    }
}
```

**Benefits:**
- Automatic backup recovery
- User notification before data loss
- Corrupted files preserved for analysis
- User can abort and manually recover

#### Solution 3: Validate Before Import

Add validation to Import() method:

```csharp
public void Import(FileInfo fileInfo, ExportImportFlags importFlags)
{
    // Validate import file before applying
    Settings testLoad = null;
    try
    {
        testLoad = LoadOrCreateNew(fileInfo);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Import file is invalid or corrupted:\n{ex.Message}\n\nImport cancelled.",
            "Import Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        return;
    }
    
    // Warn if import file looks suspiciously empty
    if (testLoad.FilterList?.Count == 0 && 
        testLoad.FileHistoryList?.Count == 0 &&
        testLoad.Preferences?.HighlightGroupList?.Count == 0)
    {
        var result = MessageBox.Show(
            "Warning: Import file appears to be empty or default settings.\n\n" +
            "This will overwrite your current configuration with empty settings.\n\n" +
            "Continue with import?",
            "Confirm Import",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        
        if (result == DialogResult.No)
        {
            return;
        }
    }
    
    // Proceed with import
    Instance._settings = Instance.Import(Instance._settings, fileInfo, importFlags);
    Save(SettingsFlags.All);
}
```

### Priority 2: Reduce Corruption Risk

#### Solution 4: Save Throttling/Debouncing

Implement save debouncing to reduce write frequency:

```csharp
private System.Threading.Timer _saveTimer;
private SettingsFlags _pendingSaveFlags = SettingsFlags.None;
private readonly object _saveTimerLock = new();

public void Save(SettingsFlags flags)
{
    lock (_saveTimerLock)
    {
        _pendingSaveFlags |= flags;
        
        // Reset timer - save after 2 seconds of inactivity
        _saveTimer?.Dispose();
        _saveTimer = new System.Threading.Timer(
            _ => FlushPendingSaves(),
            null,
            TimeSpan.FromSeconds(2),
            Timeout.InfiniteTimeSpan);
    }
}

private void FlushPendingSaves()
{
    lock (_saveTimerLock)
    {
        if (_pendingSaveFlags != SettingsFlags.None)
        {
            Instance.Save(Settings, _pendingSaveFlags);
            _pendingSaveFlags = SettingsFlags.None;
        }
    }
}

// Call on application exit to ensure saves complete
public void FlushPendingSavesSync()
{
    _saveTimer?.Dispose();
    FlushPendingSaves();
}
```

**Benefits:**
- Reduces save frequency from 100s to ~1 per 2 seconds
- Lower corruption risk
- Better performance (less I/O)

#### Solution 5: Cross-Process File Locking

Implement file-level locking for multi-instance safety:

```csharp
private FileStream _settingsFileLock;

private void AcquireSettingsFileLock()
{
    var lockFile = Path.Combine(ConfigDir, "settings.json.lock");
    try
    {
        _settingsFileLock = new FileStream(
            lockFile,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None, // Exclusive lock
            1,
            FileOptions.DeleteOnClose);
    }
    catch (IOException)
    {
        _logger.Warn("Another instance has settings file locked");
        // Could implement retry logic or read-only mode
    }
}

private void ReleaseSettingsFileLock()
{
    _settingsFileLock?.Dispose();
    _settingsFileLock = null;
}
```

### Priority 3: Enhanced Diagnostics

#### Solution 6: Settings Validation and Logging

Add validation before save:

```csharp
private bool ValidateSettings(Settings settings)
{
    if (settings == null)
    {
        _logger.Error("Attempted to save null settings");
        return false;
    }
    
    if (settings.Preferences == null)
    {
        _logger.Error("Settings.Preferences is null");
        return false;
    }
    
    // Check for suspiciously empty settings
    if (settings.FilterList?.Count == 0 &&
        settings.FileHistoryList?.Count == 0 &&
        settings.SearchHistoryList?.Count == 0 &&
        settings.Preferences.HighlightGroupList?.Count == 0)
    {
        _logger.Warn("Settings appear to be empty - this may indicate data loss");
        
        // Log what was in memory before
        _logger.Warn($"Previous settings had: " +
            $"Filters={_settings.FilterList?.Count}, " +
            $"History={_settings.FileHistoryList?.Count}, " +
            $"Highlights={_settings.Preferences.HighlightGroupList?.Count}");
    }
    
    return true;
}

private void SaveAsJSON(FileInfo fileInfo, Settings settings)
{
    if (!ValidateSettings(settings))
    {
        throw new InvalidOperationException("Settings validation failed - refusing to save");
    }
    
    _logger.Info($"Saving settings: Filters={settings.FilterList?.Count}, " +
        $"History={settings.FileHistoryList?.Count}, " +
        $"Size={JsonConvert.SerializeObject(settings).Length} bytes");
    
    // ... rest of save logic ...
}
```

### Priority 4: User-Facing Features

#### Solution 7: Settings Backup Management

Add automatic backup retention:

```csharp
private void CreateTimestampedBackup(FileInfo fileInfo)
{
    if (!fileInfo.Exists) return;
    
    var backupDir = Path.Combine(ConfigDir, "backups");
    Directory.CreateDirectory(backupDir);
    
    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    var backupFile = Path.Combine(backupDir, $"settings_{timestamp}.json");
    
    File.Copy(fileInfo.FullName, backupFile);
    _logger.Info($"Created backup: {backupFile}");
    
    // Keep only last 10 backups
    var backups = Directory.GetFiles(backupDir, "settings_*.json")
        .OrderByDescending(f => f)
        .Skip(10);
    
    foreach (var old in backups)
    {
        try { File.Delete(old); } catch { }
    }
}
```

#### Solution 8: Settings Recovery UI

Add menu option for recovery:

```
Tools → Settings → Recover from Backup
- Shows list of available backups with timestamps and sizes
- Preview backup contents before restoring
- Allows selective restore (e.g., only filters, only highlights)
```

---

## Testing Recommendations

### Test Cases

1. **Corruption Recovery Test**
   - Manually corrupt settings.json
   - Verify backup recovery works
   - Verify user is notified

2. **Write Interruption Test**
   - Kill process during save (Task Manager)
   - Verify settings.json not corrupted
   - Verify backup used on restart

3. **Multiple Instance Test**
   - Run 2 instances simultaneously
   - Make changes in both
   - Verify no data loss

4. **Import Validation Test**
   - Try importing empty file
   - Try importing corrupted file
   - Verify warnings shown and import rejected

5. **Save Debouncing Test**
   - Perform 100 rapid filter changes
   - Verify only 1-2 actual saves occur
   - Verify all changes preserved

---

## Questions for Further Investigation

1. **Has this been reported by users?**
   - Check issue tracker for "settings lost", "configuration reset"
   - Review crash logs for deserialization exceptions

2. **What JSON.NET version is used?**
   - Check if version differences cause schema incompatibility
   - Consider pinning version or handling version migrations

3. **Are there schema changes between versions?**
   - Review git history for Settings class changes
   - Implement schema versioning and migration

4. **What percentage of saves occur at shutdown vs. during use?**
   - Add telemetry to understand save patterns
   - May inform where to focus optimization

5. **Is portable mode more/less affected?**
   - Portable mode writes to different directory
   - USB drive writes more prone to interruption

---

## Implementation Priority Matrix

| Fix | Impact | Effort | Priority |
|-----|--------|--------|----------|
| Solution 1: Atomic write | High | Medium | P0 - Critical |
| Solution 2: Deserialize recovery | High | Medium | P0 - Critical |
| Solution 3: Import validation | High | Low | P0 - Critical |
| Solution 6: Settings validation | Medium | Low | P1 - High |
| Solution 4: Save debouncing | Medium | Medium | P2 - Medium |
| Solution 7: Backup management | Medium | Low | P2 - Medium |
| Solution 5: File locking | Low | High | P3 - Low |
| Solution 8: Recovery UI | Low | High | P3 - Low |

**Recommended Implementation Order:**
1. Solution 3 (Quick win - prevent bad imports)
2. Solution 1 (Atomic write - prevents corruption)
3. Solution 2 (Recovery - handles existing corruption)
4. Solution 6 (Validation - early warning system)
5. Solution 4 (Debouncing - reduces risk frequency)
6. Solution 7 (Backups - user safety net)

---

## Summary

**Root Cause:** Settings loss occurs primarily due to deserialization failures after file corruption, with no recovery mechanism, causing empty settings to overwrite good data.

**Contributing Factors:**
- No atomic write protection
- No backup before overwrite
- Silent exception handling
- High save frequency
- No validation before save

**Critical Fixes Needed:**
1. Atomic write with temp file pattern
2. Backup creation before every save
3. Automatic backup recovery on load failure
4. User confirmation before creating new settings
5. Import validation

**Estimated Effort:** 3-5 developer days for critical fixes (Solutions 1-3, 6)

**Risk if Not Fixed:** Users will continue to lose configuration randomly, leading to poor user experience and support burden.
