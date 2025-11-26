using System.Reflection;

using LogExpert.Configuration;
using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;

using Newtonsoft.Json;

using NUnit.Framework;

namespace LogExpert.Tests;

/// <summary>
/// Unit tests for ConfigManager settings loss prevention fixes.
/// Tests all 4 Priority 1 implementations: Import Validation, Atomic Write, Deserialization Recovery, Settings Validation.
/// </summary>
[TestFixture]
public class ConfigManagerTest
{
    private string _testDir;
    private FileInfo _testSettingsFile;
    private ConfigManager _configManager;

    [SetUp]
    public void SetUp ()
    {
        // Create isolated test directory for each test
        _testDir = Path.Join(Path.GetTempPath(), "LogExpert_Test_" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_testDir);
        _testSettingsFile = new FileInfo(Path.Join(_testDir, "settings.json"));

        // Initialize ConfigManager for testing
        _configManager = ConfigManager.Instance;
        _configManager.Initialize(_testDir, new Rectangle(0, 0, 1920, 1080));
    }

    [TearDown]
    public void TearDown ()
    {
        // Cleanup test directory
        if (Directory.Exists(_testDir))
        {
            try
            {
                Directory.Delete(_testDir, recursive: true);
            }
            catch (IOException)
            {
                // Ignore IO errors during cleanup
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore access errors during cleanup
            }
        }
    }

    #region Helper Methods

    /// <summary>
    /// Invokes a private static method using reflection.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "Unit Tests")]
    private static T InvokePrivateStaticMethod<T> (string methodName, params object[] parameters)
    {
        MethodInfo? method = typeof(ConfigManager).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        return method == null
            ? throw new Exception($"Static method {methodName} not found")
            : (T)method.Invoke(null, parameters);
    }

    /// <summary>
    /// Invokes a private instance method using reflection.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "Unit Tests")]
    private T InvokePrivateInstanceMethod<T> (string methodName, params object[] parameters)
    {
        MethodInfo? method = typeof(ConfigManager).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        return method == null
            ? throw new Exception($"Instance method {methodName} not found")
            : (T)method.Invoke(_configManager, parameters);
    }

    /// <summary>
    /// Invokes a private instance method with no return value using reflection.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "Unit Tests")]
    private void InvokePrivateInstanceMethod (string methodName, params object[] parameters)
    {
        MethodInfo? method = typeof(ConfigManager).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new Exception($"Instance method {methodName} not found");

        _ = method.Invoke(_configManager, parameters);
    }

    /// <summary>
    /// Creates a basic test Settings object with valid defaults.
    /// </summary>
    private static Settings CreateTestSettings ()
    {
        var settings = new Settings
        {
            Preferences = new Preferences()
        };

        return settings;
    }

    /// <summary>
    /// Creates a populated Settings object with sample data.
    /// </summary>
    private static Settings CreatePopulatedSettings ()
    {
        Settings settings = CreateTestSettings();
        settings.FilterList.Add(new FilterParams { SearchText = "ERROR" });
        settings.FilterList.Add(new FilterParams { SearchText = "WARNING" });
        settings.SearchHistoryList.Add("exception");
        settings.SearchHistoryList.Add("error");
        settings.Preferences.HighlightGroupList.Add(new HighlightGroup { GroupName = "Errors" });
        return settings;
    }

    #endregion

    #region Import Validation Tests

    [Test]
    [Category("ImportValidation")]
    [Description("SettingsAreEmptyOrDefault should return true for null settings")]
    public void SettingsAreEmptyOrDefault_NullSettings_ReturnsTrue ()
    {
        // Arrange
        Settings settings = null;

        // Act
        bool result = InvokePrivateStaticMethod<bool>("SettingsAreEmptyOrDefault", settings);

        // Assert
        Assert.That(result, Is.True, "Null settings should be detected as empty/default");
    }

    [Test]
    [Category("ImportValidation")]
    [Description("SettingsAreEmptyOrDefault should return true for empty settings")]
    public void SettingsAreEmptyOrDefault_EmptySettings_ReturnsTrue ()
    {
        // Arrange
        Settings settings = CreateTestSettings();

        // Act
        bool result = InvokePrivateStaticMethod<bool>("SettingsAreEmptyOrDefault", settings);

        // Assert
        Assert.That(result, Is.True, "Empty settings should be detected as empty/default");
    }

    [Test]
    [Category("ImportValidation")]
    [Description("SettingsAreEmptyOrDefault should return false for settings with filters")]
    public void SettingsAreEmptyOrDefault_SettingsWithFilters_ReturnsFalse ()
    {
        // Arrange
        Settings settings = CreateTestSettings();
        settings.FilterList.Add(new FilterParams { SearchText = "TEST_FILTER" });

        // Act
        bool result = InvokePrivateStaticMethod<bool>("SettingsAreEmptyOrDefault", settings);

        // Assert
        Assert.That(result, Is.False, "Settings with filters should not be empty/default");
    }

    [Test]
    [Category("ImportValidation")]
    [Description("SettingsAreEmptyOrDefault should return false for settings with search history")]
    public void SettingsAreEmptyOrDefault_SettingsWithHistory_ReturnsFalse ()
    {
        // Arrange
        Settings settings = CreateTestSettings();
        settings.SearchHistoryList.Add("test search");

        // Act
        bool result = InvokePrivateStaticMethod<bool>("SettingsAreEmptyOrDefault", settings);

        // Assert
        Assert.That(result, Is.False, "Settings with search history should not be empty/default");
    }

    [Test]
    [Category("ImportValidation")]
    [Description("SettingsAreEmptyOrDefault should return false for settings with highlights")]
    public void SettingsAreEmptyOrDefault_SettingsWithHighlights_ReturnsFalse ()
    {
        // Arrange
        Settings settings = CreateTestSettings();
        settings.Preferences.HighlightGroupList.Add(new HighlightGroup { GroupName = "Test" });

        // Act
        bool result = InvokePrivateStaticMethod<bool>("SettingsAreEmptyOrDefault", settings);

        // Assert
        Assert.That(result, Is.False, "Settings with highlights should not be empty/default");
    }

    [Test]
    [Category("ImportValidation")]
    [Description("ValidateSettings should handle null settings gracefully")]
    public void ValidateSettings_NullSettings_ReturnsFalse ()
    {
        // Arrange
        Settings settings = null;

        // Act
        bool result = InvokePrivateInstanceMethod<bool>("ValidateSettings", settings);

        // Assert
        Assert.That(result, Is.False, "Null settings should fail validation");
    }

    [Test]
    [Category("ImportValidation")]
    [Description("ValidateSettings should return true for valid populated settings")]
    public void ValidateSettings_ValidPopulatedSettings_ReturnsTrue ()
    {
        // Arrange
        Settings settings = CreatePopulatedSettings();

        // Act
        bool result = InvokePrivateInstanceMethod<bool>("ValidateSettings", settings);

        // Assert
        Assert.That(result, Is.True, "Valid populated settings should pass validation");
    }

    [Test]
    [Category("ImportValidation")]
    [Description("ValidateSettings should return true for valid empty settings")]
    public void ValidateSettings_ValidEmptySettings_ReturnsTrue ()
    {
        // Arrange
        Settings settings = CreateTestSettings();

        // Act
        bool result = InvokePrivateInstanceMethod<bool>("ValidateSettings", settings);

        // Assert
        Assert.That(result, Is.True, "Valid empty settings should pass validation (may log warning)");
    }

    #endregion

    #region Atomic Write Tests

    [Test]
    [Category("AtomicWrite")]
    [Description("SaveAsJSON should create main file and cleanup temp file")]
    public void SaveAsJSON_CreatesMainFileAndCleanupsTempFile ()
    {
        // Arrange
        Settings settings = CreatePopulatedSettings();
        settings.AlwaysOnTop = true;

        // Act
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings);

        // Assert
        string tempFile = _testSettingsFile.FullName + ".tmp";
        Assert.That(File.Exists(tempFile), Is.False, "Temp file should be cleaned up");
        Assert.That(_testSettingsFile.Exists, Is.True, "Main file should exist");

        // Verify content
        string json = File.ReadAllText(_testSettingsFile.FullName);
        Assert.That(json, Does.Contain("AlwaysOnTop"));
        Assert.That(json, Does.Contain("ERROR").Or.Contain("WARNING"));
    }

    [Test]
    [Category("AtomicWrite")]
    [Description("SaveAsJSON should create backup file on second save")]
    public void SaveAsJSON_CreatesBackupFile ()
    {
        // Arrange
        Settings settings1 = CreateTestSettings();
        settings1.AlwaysOnTop = true;
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings1);

        Settings settings2 = CreateTestSettings();
        settings2.AlwaysOnTop = false;

        // Act
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings2);

        // Assert
        string backupFile = _testSettingsFile.FullName + ".bak";
        Assert.That(File.Exists(backupFile), Is.True, "Backup file should exist");

        string backupContent = File.ReadAllText(backupFile);
        Settings backupSettings = JsonConvert.DeserializeObject<Settings>(backupContent);
        Assert.That(backupSettings.AlwaysOnTop, Is.True, "Backup should contain previous settings");

        string mainContent = File.ReadAllText(_testSettingsFile.FullName);
        Settings mainSettings = JsonConvert.DeserializeObject<Settings>(mainContent);
        Assert.That(mainSettings.AlwaysOnTop, Is.False, "Main file should contain new settings");
    }

    [Test]
    [Category("AtomicWrite")]
    [Description("SaveAsJSON should not backup empty/zero-byte files")]
    public void SaveAsJSON_DoesNotBackupEmptyFile ()
    {
        // Arrange
        File.WriteAllText(_testSettingsFile.FullName, ""); // Create empty file
        Settings settings = CreateTestSettings();

        // Act
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings);

        // Assert
        string backupFile = _testSettingsFile.FullName + ".bak";
        Assert.That(File.Exists(backupFile), Is.False, "Empty file should not be backed up");
        Assert.That(_testSettingsFile.Exists, Is.True, "Main file should exist");
        Assert.That(new FileInfo(_testSettingsFile.FullName).Length, Is.GreaterThan(0), "Main file should not be empty");
    }

    [Test]
    [Category("AtomicWrite")]
    [Description("SaveAsJSON should save complete valid JSON that can be deserialized")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Tests")]
    public void SaveAsJSON_SavesCompleteValidJSON ()
    {
        // Arrange
        Settings settings = CreateTestSettings();
        settings.FilterList.Add(new FilterParams { SearchText = "TEST_FILTER_123" });
        settings.SearchHistoryList.Add("TEST_SEARCH_456");
        settings.AlwaysOnTop = true;

        // Act
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings);

        // Assert
        Assert.That(_testSettingsFile.Exists, Is.True);
        string json = File.ReadAllText(_testSettingsFile.FullName);

        // Verify content present
        Assert.That(json, Does.Contain("TEST_FILTER_123"));
        Assert.That(json, Does.Contain("TEST_SEARCH_456"));

        // Verify can deserialize
        Settings loaded = null;
        Assert.DoesNotThrow(() => loaded = JsonConvert.DeserializeObject<Settings>(json), "Saved JSON should be valid and deserializable");

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded.FilterList.Count, Is.EqualTo(1));
        Assert.That(loaded.FilterList[0].SearchText, Is.EqualTo("TEST_FILTER_123"));
        Assert.That(loaded.SearchHistoryList.Count, Is.EqualTo(1));
        Assert.That(loaded.AlwaysOnTop, Is.True);
    }

    [Test]
    [Category("AtomicWrite")]
    [Description("SaveAsJSON validation should prevent saving null settings")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Tests")]
    public void SaveAsJSON_ValidationFailure_PreventsNullSettingsSave ()
    {
        // Arrange
        Settings settings = null;

        // Act & Assert
        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings), "Saving null settings should throw exception");

        // The inner exception should be InvalidOperationException from ValidateSettings
        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(_testSettingsFile.Exists, Is.False, "File should not be created if validation fails");
    }

    [Test]
    [Category("AtomicWrite")]
    [Description("SaveAsJSON should maintain file integrity across multiple saves")]
    public void SaveAsJSON_MultipleSaves_MaintainsIntegrity ()
    {
        // Arrange & Act - Multiple saves
        for (int i = 0; i < 5; i++)
        {
            Settings settings = CreateTestSettings();
            settings.FilterList.Add(new FilterParams { SearchText = $"FILTER_{i}" });
            InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings);
        }

        // Assert
        Assert.That(_testSettingsFile.Exists, Is.True);
        string json = File.ReadAllText(_testSettingsFile.FullName);
        Settings? loaded = JsonConvert.DeserializeObject<Settings>(json);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded.FilterList.Count, Is.EqualTo(1), "Last save should have 1 filter");
        Assert.That(loaded.FilterList[0].SearchText, Is.EqualTo("FILTER_4"), "Should have last filter");

        // Verify backup has previous version
        string backupFile = _testSettingsFile.FullName + ".bak";
        if (File.Exists(backupFile))
        {
            string backupJson = File.ReadAllText(backupFile);
            Settings? backupLoaded = JsonConvert.DeserializeObject<Settings>(backupJson);
            Assert.That(backupLoaded.FilterList[0].SearchText, Is.EqualTo("FILTER_3"), "Backup should have previous version");
        }
    }

    #endregion

    #region Deserialization Recovery Tests

    [Test]
    [Category("DeserializationRecovery")]
    [Description("LoadOrCreateNew should load valid settings file successfully")]
    public void LoadOrCreateNew_ValidFile_LoadsSuccessfully ()
    {
        // Arrange
        Settings settings = CreatePopulatedSettings();
        settings.FilterList.Clear();
        settings.FilterList.Add(new FilterParams { SearchText = "VALID_FILTER_TEST" });
        string json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(_testSettingsFile.FullName, json);

        // Act
        LoadResult loadResult = InvokePrivateInstanceMethod<LoadResult>("LoadOrCreateNew", _testSettingsFile);

        // Assert
        Assert.That(loadResult, Is.Not.Null);
        Assert.That(loadResult.Settings, Is.Not.Null);
        Assert.That(loadResult.LoadedFromBackup, Is.False, "Should not load from backup for valid file");
        Assert.That(loadResult.CriticalFailure, Is.False, "Should not have critical failure");
        Assert.That(loadResult.Settings.FilterList.Count, Is.EqualTo(1));
        Assert.That(loadResult.Settings.FilterList[0].SearchText, Is.EqualTo("VALID_FILTER_TEST"));
    }

    [Test]
    [Category("DeserializationRecovery")]
    [Description("LoadOrCreateNew should handle missing file by creating new settings")]
    public void LoadOrCreateNew_MissingFile_CreatesNewSettings ()
    {
        // Arrange - file doesn't exist

        // Act
        LoadResult loadResult = InvokePrivateInstanceMethod<LoadResult>("LoadOrCreateNew", (FileInfo)null);

        // Assert
        Assert.That(loadResult, Is.Not.Null);
        Assert.That(loadResult.Settings, Is.Not.Null, "Should create new settings when file is null");
        Assert.That(loadResult.Settings.Preferences, Is.Not.Null, "Settings should have preferences initialized");
        Assert.That(loadResult.LoadedFromBackup, Is.False);
        Assert.That(loadResult.CriticalFailure, Is.False);
    }

    [Test]
    [Category("DeserializationRecovery")]
    [Description("LoadOrCreateNew should handle empty file gracefully")]
    public void LoadOrCreateNew_EmptyFile_HandlesGracefully ()
    {
        // Arrange
        File.WriteAllText(_testSettingsFile.FullName, "");

        // Act
        LoadResult loadResult = InvokePrivateInstanceMethod<LoadResult>("LoadOrCreateNew", _testSettingsFile);

        // Assert
        Assert.That(loadResult, Is.Not.Null);
        Assert.That(loadResult.Settings, Is.Not.Null, "Should return valid settings object, not null");
        Assert.That(loadResult.Settings.Preferences, Is.Not.Null, "Settings should have preferences");
        // Empty file triggers recovery, creates new settings with critical failure
        Assert.That(loadResult.CriticalFailure, Is.True, "Empty file should be treated as critical failure");
    }

    [Test]
    [Category("DeserializationRecovery")]
    [Description("LoadOrCreateNew should handle null JSON deserialization result")]
    public void LoadOrCreateNew_NullDeserializationResult_HandlesGracefully ()
    {
        // Arrange
        File.WriteAllText(_testSettingsFile.FullName, "null");

        // Act
        LoadResult loadResult = InvokePrivateInstanceMethod<LoadResult>("LoadOrCreateNew", _testSettingsFile);

        // Assert
        Assert.That(loadResult, Is.Not.Null);
        Assert.That(loadResult.Settings, Is.Not.Null, "Should not return null settings");
        Assert.That(loadResult.Settings.Preferences, Is.Not.Null);
        // Null deserialization is treated as critical failure
        Assert.That(loadResult.CriticalFailure, Is.True);
    }

    [Test]
    [Category("DeserializationRecovery")]
    [Description("LoadOrCreateNew should recover from backup when main file is corrupt")]
    public void LoadOrCreateNew_CorruptFileWithBackup_RecoversFromBackup ()
    {
        // Arrange - Create good backup
        Settings goodSettings = CreateTestSettings();
        goodSettings.FilterList.Add(new FilterParams { SearchText = "BACKUP_FILTER_TEST" });
        string backupFile = _testSettingsFile.FullName + ".bak";
        File.WriteAllText(backupFile, JsonConvert.SerializeObject(goodSettings));

        // Create corrupt main file
        File.WriteAllText(_testSettingsFile.FullName, "{\"corrupt\": json}");

        // Act
        LoadResult loadResult = InvokePrivateInstanceMethod<LoadResult>("LoadOrCreateNew", _testSettingsFile);

        // Assert
        Assert.That(loadResult, Is.Not.Null);
        Assert.That(loadResult.Settings, Is.Not.Null);
        Assert.That(loadResult.LoadedFromBackup, Is.True, "Should indicate loaded from backup");
        Assert.That(loadResult.RecoveryMessage, Is.Not.Null.And.Not.Empty, "Should provide recovery message");
        Assert.That(loadResult.RecoveryTitle, Is.Not.Null.And.Not.Empty, "Should provide recovery title");

        // Verify backup recovery worked - should have BACKUP_FILTER_TEST
        Assert.That(loadResult.Settings.FilterList.Count, Is.EqualTo(1));
        Assert.That(loadResult.Settings.FilterList[0].SearchText, Is.EqualTo("BACKUP_FILTER_TEST"));

        // Verify corrupt file preserved
        string corruptFile = _testSettingsFile.FullName + ".corrupt";
        Assert.That(File.Exists(corruptFile), Is.True, "Corrupt file should be preserved");
    }

    [Test]
    [Category("DeserializationRecovery")]
    [Description("LoadOrCreateNew should handle corrupt JSON with invalid syntax")]
    public void LoadOrCreateNew_InvalidJSON_HandlesGracefully ()
    {
        // Arrange
        File.WriteAllText(_testSettingsFile.FullName, "{invalid json syntax");

        // Act
        LoadResult loadResult = InvokePrivateInstanceMethod<LoadResult>("LoadOrCreateNew", _testSettingsFile);

        // Assert
        Assert.That(loadResult, Is.Not.Null);
        Assert.That(loadResult.Settings, Is.Not.Null, "Should return valid settings object");
        // Without backup, will return CriticalFailure or create new settings
        Assert.That(loadResult.Settings.Preferences, Is.Not.Null);
        Assert.That(loadResult.CriticalFailure, Is.True, "Invalid JSON should result in critical failure");
    }

    #endregion

    #region Integration Tests

    [Test]
    [Category("Integration")]
    [Description("End-to-end save and load should preserve all settings")]
    public void SaveAndLoad_PreservesAllSettings ()
    {
        // Arrange
        Settings originalSettings = CreateTestSettings();
        originalSettings.AlwaysOnTop = true;
        originalSettings.FilterList.Add(new FilterParams { SearchText = "FILTER1" });
        originalSettings.FilterList.Add(new FilterParams { SearchText = "FILTER2" });
        originalSettings.SearchHistoryList.Add("SEARCH1");
        originalSettings.SearchHistoryList.Add("SEARCH2");
        originalSettings.Preferences.HighlightGroupList.Add(new HighlightGroup { GroupName = "GROUP1" });

        // Act - Save
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, originalSettings);

        // Act - Load
        LoadResult loadResult = InvokePrivateInstanceMethod<LoadResult>("LoadOrCreateNew", _testSettingsFile);

        // Assert
        Assert.That(loadResult, Is.Not.Null);
        Assert.That(loadResult.Settings, Is.Not.Null);
        Assert.That(loadResult.Settings.AlwaysOnTop, Is.EqualTo(originalSettings.AlwaysOnTop));
        Assert.That(loadResult.Settings.FilterList.Count, Is.EqualTo(2));
        Assert.That(loadResult.Settings.SearchHistoryList.Count, Is.EqualTo(2));
        Assert.That(loadResult.Settings.Preferences.HighlightGroupList.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Integration")]
    [Description("Multiple save operations should maintain backup chain correctly")]
    public void MultipleSaves_MaintainsBackupChain ()
    {
        // Arrange & Act - Save 1
        Settings settings1 = CreateTestSettings();
        settings1.AlwaysOnTop = true;
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings1);

        // Act - Save 2
        Settings settings2 = CreateTestSettings();
        settings2.AlwaysOnTop = false;
        settings2.FilterList.Add(new FilterParams { SearchText = "FILTER1" });
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings2);

        // Act - Save 3
        Settings settings3 = CreateTestSettings();
        settings3.AlwaysOnTop = true;
        settings3.FilterList.Add(new FilterParams { SearchText = "FILTER1" });
        settings3.FilterList.Add(new FilterParams { SearchText = "FILTER2" });
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings3);

        // Assert - Main file has latest
        string mainContent = File.ReadAllText(_testSettingsFile.FullName);
        Assert.That(mainContent, Does.Contain("FILTER2"), "Main file should have latest settings");

        // Assert - Backup has previous version
        string backupFile = _testSettingsFile.FullName + ".bak";
        Assert.That(File.Exists(backupFile), Is.True, "Backup file should exist");

        string backupContent = File.ReadAllText(backupFile);
        Assert.That(backupContent, Does.Contain("FILTER1"), "Backup should have previous version");
        Assert.That(backupContent, Does.Not.Contain("FILTER2"), "Backup should not have latest changes");
    }

    [Test]
    [Category("Integration")]
    [Description("Save operation should be atomic - file always in valid state")]
    public void Save_IsAtomic_FileAlwaysValid ()
    {
        // Arrange
        Settings settings1 = CreateTestSettings();
        settings1.FilterList.Add(new FilterParams { SearchText = "INITIAL" });
        InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings1);

        // Act - Save multiple times rapidly
        for (int i = 0; i < 10; i++)
        {
            Settings settings = CreateTestSettings();
            settings.FilterList.Add(new FilterParams { SearchText = $"FILTER_{i}" });
            InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings);

            // Verify file is always readable
            Assert.That(_testSettingsFile.Exists, Is.True, $"File should exist after save {i}");
            Assert.DoesNotThrow(() =>
            {
                string json = File.ReadAllText(_testSettingsFile.FullName);
                Settings? loaded = JsonConvert.DeserializeObject<Settings>(json);
                Assert.That(loaded, Is.Not.Null);
            }, $"File should always be valid JSON after save {i}");
        }
    }

    [Test]
    [Category("Integration")]
    [Description("Backup file should always be valid when it exists")]
    public void BackupFile_AlwaysValid_WhenExists ()
    {
        // Arrange & Act
        for (int i = 0; i < 5; i++)
        {
            Settings settings = CreateTestSettings();
            settings.FilterList.Add(new FilterParams { SearchText = $"FILTER_{i}" });
            InvokePrivateInstanceMethod("SaveAsJSON", _testSettingsFile, settings);

            // Check backup if it exists
            string backupFile = _testSettingsFile.FullName + ".bak";
            if (File.Exists(backupFile))
            {
                Assert.DoesNotThrow(() =>
                {
                    string json = File.ReadAllText(backupFile);
                    Settings? loaded = JsonConvert.DeserializeObject<Settings>(json);
                    Assert.That(loaded, Is.Not.Null, $"Backup should be valid JSON after save {i}");
                }, $"Backup file should always be valid when it exists (iteration {i})");
            }
        }
    }

    #endregion
}
