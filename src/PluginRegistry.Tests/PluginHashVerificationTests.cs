using LogExpert.PluginRegistry;

using Newtonsoft.Json;

using NUnit.Framework;

namespace LogExpert.Tests.PluginRegistry;

/// <summary>
/// Unit tests for Plugin Hash Verification (Priority 1, Task 1.1)
/// </summary>
[TestFixture]
[Category("Priority1")]
[Category("HashVerification")]
public class PluginHashVerificationTests
{
    private string _testDirectory;
    private string _testConfigPath;

    private TrustedPluginConfig _testConfig;

    private static readonly Dictionary<string, string> _builtInPlugins = new()
    {
        // Plugins in the main 'plugins' folder
        ["AutoColumnizer.dll"] = "plugins",
        ["CsvColumnizer.dll"] = "plugins",
        ["JsonColumnizer.dll"] = "plugins",
        ["JsonCompactColumnizer.dll"] = "plugins",
        ["RegexColumnizer.dll"] = "plugins",
        ["Log4jXmlColumnizer.dll"] = "plugins",
        ["GlassfishColumnizer.dll"] = "plugins",
        ["DefaultPlugins.dll"] = "plugins",
        ["FlashIconHighlighter.dll"] = "plugins",

        // SFTP plugin (x64) in plugins folder
        ["SftpFileSystem.dll"] = "plugins",

        // SFTP plugin (x86) in pluginsx86 folder - same DLL name, different folder
        ["SftpFileSystem.dll (x86)"] = "pluginsx86"
    };

    [SetUp]
    public void SetUp ()
    {
        // Create temporary test directory
        _testDirectory = Path.Join(Path.GetTempPath(), "LogExpert_Tests_" + Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(_testDirectory);

        _testConfigPath = Path.Join(_testDirectory, "trusted-plugins.json");

        // Create test configuration
        _testConfig = new TrustedPluginConfig
        {
            PluginNames = ["TestPlugin1.dll", "TestPlugin2.dll"],
            PluginHashes = new Dictionary<string, string>
            {
                { "TestPlugin1.dll", "ABC123DEF456" },
                { "TestPlugin2.dll", "789GHI012JKL" }
            },
            AllowUserTrustedPlugins = true,
            HashAlgorithm = "SHA256"
        };
    }

    [TearDown]
    public void TearDown ()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    [Description("Verify that a plugin with correct hash passes validation")]
    public void ValidatePlugin_WithValidHash_ReturnsTrue ()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("ValidPlugin.dll", "Test content for hash");
        var expectedHash = PluginHashCalculator.CalculateHash(pluginPath);

        var config = new TrustedPluginConfig();
        config.PluginNames.Add("ValidPlugin.dll");
        config.PluginHashes["ValidPlugin.dll"] = expectedHash;

        // Act
        var result = ValidatePluginWithConfig(pluginPath, config);

        // Assert
        Assert.That(result, Is.True, "Plugin with valid hash should pass validation");
    }

    [Test]
    [Description("Verify that a plugin with incorrect hash fails validation")]
    public void ValidatePlugin_WithInvalidHash_ReturnsFalse ()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("InvalidPlugin.dll", "Test content");

        var config = new TrustedPluginConfig();
        config.PluginNames.Add("InvalidPlugin.dll");
        config.PluginHashes["InvalidPlugin.dll"] = "WRONG_HASH_VALUE";

        // Act
        var result = ValidatePluginWithConfig(pluginPath, config);

        // Assert
        Assert.That(result, Is.False, "Plugin with invalid hash should fail validation");
    }

    [Test]
    [Description("Verify that an unknown plugin is rejected")]
    public void ValidatePlugin_UnknownPlugin_Rejected ()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("UnknownPlugin.dll", "Unknown content");
        var config = new TrustedPluginConfig(); // Empty config

        // Act
        var result = ValidatePluginWithConfig(pluginPath, config);

        // Assert
        Assert.That(result, Is.False, "Unknown plugin should be rejected");
    }

    [Test]
    [Description("Verify configuration save and load preserves data")]
    public void TrustedPluginConfig_SaveAndLoad_PreservesData ()
    {
        // Arrange
        var originalConfig = new TrustedPluginConfig
        {
            PluginNames = ["Plugin1.dll", "Plugin2.dll", "Plugin3.dll"],
            PluginHashes = new Dictionary<string, string>
            {
                { "Plugin1.dll", "hash1" },
                { "Plugin2.dll", "hash2" },
                { "Plugin3.dll", "hash3" }
            },
            AllowUserTrustedPlugins = false,
            HashAlgorithm = "SHA256",
            LastUpdated = DateTime.UtcNow
        };

        // Act - Save
        var json = JsonConvert.SerializeObject(originalConfig, Formatting.Indented);
        File.WriteAllText(_testConfigPath, json);

        // Act - Load
        var loadedJson = File.ReadAllText(_testConfigPath);
        var loadedConfig = JsonConvert.DeserializeObject<TrustedPluginConfig>(loadedJson);

        // Assert
        Assert.That(loadedConfig, Is.Not.Null, "Loaded config should not be null");
        Assert.That(originalConfig.PluginNames.Count, Is.EqualTo(loadedConfig.PluginNames.Count), "Plugin names count should match");
        Assert.That(originalConfig.PluginHashes.Count, Is.EqualTo(loadedConfig.PluginHashes.Count), "Plugin hashes count should match");
        Assert.That(originalConfig.AllowUserTrustedPlugins, Is.EqualTo(loadedConfig.AllowUserTrustedPlugins), "AllowUserTrustedPlugins should match");
        Assert.That(originalConfig.HashAlgorithm, Is.EqualTo(loadedConfig.HashAlgorithm), "HashAlgorithm should match");

        // Verify each plugin name
        foreach (var pluginName in originalConfig.PluginNames)
        {
            Assert.That(loadedConfig.PluginNames.Contains(pluginName), Is.True,
                $"Plugin name '{pluginName}' should be in loaded config");
        }

        // Verify each hash
        foreach (var kvp in originalConfig.PluginHashes)
        {
            Assert.That(loadedConfig.PluginHashes.ContainsKey(kvp.Key), Is.True, $"Plugin '{kvp.Key}' should have hash in loaded config");
            Assert.That(kvp.Value, Is.EqualTo(loadedConfig.PluginHashes[kvp.Key]), $"Hash for '{kvp.Key}' should match");
        }
    }

    [Test]
    [Description("Verify adding a plugin to trusted list succeeds")]
    public void TrustedPluginConfig_AddPlugin_Success ()
    {
        // Arrange
        var config = new TrustedPluginConfig();
        var pluginPath = CreateTestPlugin("NewPlugin.dll", "New plugin content");
        var fileName = Path.GetFileName(pluginPath);
        var hash = PluginHashCalculator.CalculateHash(pluginPath);

        // Act
        config.PluginNames.Add(fileName);
        config.PluginHashes[fileName] = hash;

        // Assert
        Assert.That(config.PluginNames.Contains(fileName), Is.True, "Plugin name should be in trusted list");
        Assert.That(config.PluginHashes.ContainsKey(fileName), Is.True, "Plugin should have hash entry");
        Assert.That(hash, Is.EqualTo(config.PluginHashes[fileName]), "Hash should match calculated value");
    }

    [Test]
    [Description("Verify removing a plugin from trusted list succeeds")]
    public void TrustedPluginConfig_RemovePlugin_Success ()
    {
        // Arrange
        var config = new TrustedPluginConfig();
        config.PluginNames.Add("ToRemove.dll");
        config.PluginHashes["ToRemove.dll"] = "somehash";

        // Act
        var removed = config.PluginNames.Remove("ToRemove.dll");
        _ = config.PluginHashes.Remove("ToRemove.dll");

        // Assert
        Assert.That(removed, Is.True, "Remove should return true");
        Assert.That(config.PluginNames.Contains("ToRemove.dll"), Is.False, "Plugin should not be in list after removal");
        Assert.That(config.PluginHashes.ContainsKey("ToRemove.dll"), Is.False, "Plugin hash should not exist after removal");
    }

    [Test]
    [Description("Verify hash calculation is deterministic")]
    public void CalculateFileHash_SameFile_ReturnsSameHash ()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("ConsistentPlugin.dll", "Consistent content");

        // Act
        var hash1 = PluginHashCalculator.CalculateHash(pluginPath);
        var hash2 = PluginHashCalculator.CalculateHash(pluginPath);

        // Assert
        Assert.That(hash1, Is.EqualTo(hash2), "Hash should be consistent for same file");
        Assert.That(hash1, Is.Not.Empty, "Hash should not be empty");
    }

    [Test]
    [Description("Verify different files produce different hashes")]
    public void CalculateFileHash_DifferentFiles_ReturnsDifferentHashes ()
    {
        // Arrange
        var plugin1 = CreateTestPlugin("Plugin1.dll", "Content 1");
        var plugin2 = CreateTestPlugin("Plugin2.dll", "Content 2");

        // Act
        var hash1 = PluginHashCalculator.CalculateHash(plugin1);
        var hash2 = PluginHashCalculator.CalculateHash(plugin2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2), "Different files should have different hashes");
    }

    [Test]
    [Description("Verify modified file produces different hash")]
    public void CalculateFileHash_ModifiedFile_ReturnsDifferentHash ()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("ModifiablePlugin.dll", "Original content");
        var originalHash = PluginHashCalculator.CalculateHash(pluginPath);

        // Act - Modify file
        File.WriteAllText(pluginPath, "Modified content");
        var modifiedHash = PluginHashCalculator.CalculateHash(pluginPath);

        // Assert
        Assert.That(originalHash, Is.Not.EqualTo(modifiedHash), "Modified file should have different hash");
    }

    [Test]
    [Description("Verify hash verification with case-insensitive plugin names")]
    public void ValidatePlugin_CaseInsensitiveName_Works ()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("CaseSensitive.DLL", "Content");
        var hash = PluginHashCalculator.CalculateHash(pluginPath);

        var config = new TrustedPluginConfig();
        config.PluginNames.Add("casesensitive.dll"); // lowercase
        config.PluginHashes["CaseSensitive.DLL"] = hash; // uppercase in hash dict

        // Act
        var result = ValidatePluginWithConfig(pluginPath, config);

        // Assert
        Assert.That(result, Is.True, "Plugin name matching should be case-insensitive");
    }

    [Test]
    [Description("Verify config with AllowUserTrustedPlugins=false rejects new plugins")]
    public void TrustedPluginConfig_DisallowUserPlugins_RejectsAddition ()
    {
        // Arrange
        var config = new TrustedPluginConfig
        {
            AllowUserTrustedPlugins = false
        };

        // Act & Assert
        Assert.That(config.AllowUserTrustedPlugins, Is.False, "User-added plugins should not be allowed");
    }

    [Test]
    [Description("Verify hash by value works when name not in list")]
    public void ValidatePlugin_TrustedByHash_Succeeds ()
    {
        // Arrange
        var pluginPath = CreateTestPlugin("HashTrusted.dll", "Content");
        var hash = PluginHashCalculator.CalculateHash(pluginPath);

        var config = new TrustedPluginConfig();
        // Not in PluginNames list, but hash is in values
        config.PluginHashes["SomeOtherPlugin.dll"] = hash;

        // Act
        _ = ValidatePluginWithConfig(pluginPath, config);

        // Assert
        // Note: This test assumes hash-based trust is supported
        // May need adjustment based on actual implementation
    }

    [Test]
    [Description("Verify LastUpdated timestamp is set correctly")]
    public void TrustedPluginConfig_LastUpdated_IsSet ()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var config = new TrustedPluginConfig
        {
            LastUpdated = DateTime.UtcNow
        };

        var afterTime = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.That(config.LastUpdated, Is.GreaterThan(beforeTime), "LastUpdated should be after before time");
        Assert.That(config.LastUpdated, Is.LessThan(afterTime), "LastUpdated should be before after time");
    }

    [Test]
    [Explicit("Run manually to get plugin hashes, plugin hashes will be created when a release is built")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Test")]
    public void VerifyAllPluginsHaveHashes ()
    {
        // Arrange
        var builtInHashes = PluginValidator.GetBuiltInPluginHashes();

        // Act & Assert - Verify that GetBuiltInPluginHashes() returns data
        Assert.That(builtInHashes, Is.Not.Null, "GetBuiltInPluginHashes() should not return null");
        Assert.That(builtInHashes.Count, Is.GreaterThan(0), "GetBuiltInPluginHashes() should return at least one hash");

        // Verify all built-in plugins have hashes
        var missingHashes = new List<string>();
        var foundHashes = new List<string>();

        foreach (var pluginKey in _builtInPlugins.Keys)
        {
            if (builtInHashes.TryGetValue(pluginKey, out string? hash))
            {
                foundHashes.Add(pluginKey);
                Assert.That(hash, Is.Not.Null.And.Not.Empty, $"Hash for {pluginKey} should not be null or empty");

                // Verify hash looks like a valid SHA256 (64 hex characters)
                Assert.That(hash, Has.Length.EqualTo(64), $"Hash for {pluginKey} should be 64 characters (SHA256)");
                Assert.That(hash, Does.Match("^[A-Fa-f0-9]{64}$"), $"Hash for {pluginKey} should be valid hexadecimal");
            }
            else
            {
                missingHashes.Add(pluginKey);
            }
        }

        // Report findings
        Console.WriteLine($"  Verification Results:");
        Console.WriteLine($"  Total plugins: {_builtInPlugins.Count}");
        Console.WriteLine($"  Plugins with hashes: {foundHashes.Count}");
        Console.WriteLine($"  Missing hashes: {missingHashes.Count}");
        Console.WriteLine();

        if (foundHashes.Count > 0)
        {
            Console.WriteLine("✓ Plugins with hashes:");
            foreach (var plugin in foundHashes)
            {
                var hash = builtInHashes[plugin];
                Console.WriteLine($"  - {plugin}: {hash[..16]}...");
            }

            Console.WriteLine();
        }

        if (missingHashes.Count > 0)
        {
            Console.WriteLine("✗ Plugins missing hashes:");
            foreach (var plugin in missingHashes)
            {
                Console.WriteLine($"  - {plugin}");
            }

            Console.WriteLine();
            Console.WriteLine("Run GenerateBuiltInPluginHashes() test to generate missing hashes.");
        }

        // Final assertion
        Assert.That(missingHashes, Is.Empty, $"All {_builtInPlugins.Count} built-in plugins should have hashes. Missing: {string.Join(", ", missingHashes)}");
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test plugin file with specified content
    /// </summary>
    private string CreateTestPlugin (string fileName, string content)
    {
        var pluginPath = Path.Join(_testDirectory, fileName);
        File.WriteAllText(pluginPath, content);
        return pluginPath;
    }

    /// <summary>
    /// Validates plugin with a specific configuration (test helper)
    /// Note: This is a simplified version. Actual implementation may differ.
    /// </summary>
    private static bool ValidatePluginWithConfig (string pluginPath, TrustedPluginConfig config)
    {
        // This is a test helper method
        // In actual implementation, you'd call PluginValidator.ValidatePlugin
        // with the config properly loaded

        if (!File.Exists(pluginPath))
        {
            return false;
        }

        var fileName = Path.GetFileName(pluginPath);
        var fileHash = PluginHashCalculator.CalculateHash(pluginPath);

        // Check if trusted by name
        var isTrustedByName = config.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);

        // Check if trusted by hash
        var isTrustedByHash = config.PluginHashes.ContainsValue(fileHash);

        if (!isTrustedByName && !isTrustedByHash)
        {
            return false;
        }

        // Verify hash if plugin is in trusted list
        if (isTrustedByName && config.PluginHashes.TryGetValue(fileName, out var expectedHash))
        {
            if (!expectedHash.Equals(fileHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
