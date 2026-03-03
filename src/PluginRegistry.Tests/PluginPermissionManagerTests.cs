using LogExpert.PluginRegistry;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

/// <summary>
/// Tests for PluginPermissionManager functionality.
/// Tests permission parsing, validation, persistence, and management.
/// </summary>
[TestFixture]
public class PluginPermissionManagerTests
{
    private string _testConfigDir = string.Empty;

    [SetUp]
    public void SetUp ()
    {
        _testConfigDir = Path.Join(Path.GetTempPath(), $"PluginPermissionTests_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(_testConfigDir);
    }

    [TearDown]
    public void TearDown ()
    {
        try
        {
            if (Directory.Exists(_testConfigDir))
            {
                Directory.Delete(_testConfigDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    #region Permission Parsing Tests

    [Test]
    public void ParsePermission_WithValidFileSystemRead_ShouldReturnCorrectPermission ()
    {
        // Arrange
        var permissionString = "filesystem:read";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.FileSystemRead));
    }

    [Test]
    public void ParsePermission_WithValidFileSystemWrite_ShouldReturnCorrectPermission ()
    {
        // Arrange
        var permissionString = "filesystem:write";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.FileSystemWrite));
    }

    [Test]
    public void ParsePermission_WithValidNetworkConnect_ShouldReturnCorrectPermission ()
    {
        // Arrange
        var permissionString = "network:connect";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.NetworkConnect));
    }

    [Test]
    public void ParsePermission_WithValidConfigRead_ShouldReturnCorrectPermission ()
    {
        // Arrange
        var permissionString = "config:read";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.ConfigRead));
    }

    [Test]
    public void ParsePermission_WithValidConfigWrite_ShouldReturnCorrectPermission ()
    {
        // Arrange
        var permissionString = "config:write";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.ConfigWrite));
    }

    [Test]
    public void ParsePermission_WithValidRegistryRead_ShouldReturnCorrectPermission ()
    {
        // Arrange
        var permissionString = "registry:read";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.RegistryRead));
    }

    [Test]
    public void ParsePermission_WithInvalidString_ShouldReturnNone ()
    {
        // Arrange
        var permissionString = "invalid:permission";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.None));
    }

    [Test]
    public void ParsePermission_WithNullString_ShouldReturnNone ()
    {
        // Arrange
        string? permissionString = null;

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString!);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.None));
    }

    [Test]
    public void ParsePermission_WithEmptyString_ShouldReturnNone ()
    {
        // Arrange
        var permissionString = "";

        // Act
        var result = PluginPermissionManager.ParsePermission(permissionString);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.None));
    }

    [Test]
    public void ParsePermission_WithCaseVariations_ShouldBeCaseInsensitive ()
    {
        // Arrange & Act & Assert
        Assert.That(PluginPermissionManager.ParsePermission("FILESYSTEM:READ"), Is.EqualTo(PluginPermission.FileSystemRead));
        Assert.That(PluginPermissionManager.ParsePermission("FileSystem:Read"), Is.EqualTo(PluginPermission.FileSystemRead));
        Assert.That(PluginPermissionManager.ParsePermission("filesystem:READ"), Is.EqualTo(PluginPermission.FileSystemRead));
    }

    [Test]
    public void ParsePermissions_WithMultiplePermissions_ShouldCombineFlags ()
    {
        // Arrange
        var permissionStrings = new[]
        {
            "filesystem:read",
            "filesystem:write",
            "network:connect"
        };

        // Act
        var result = PluginPermissionManager.ParsePermissions(permissionStrings);

        // Assert
        Assert.That(result, Is.EqualTo(
            PluginPermission.FileSystemRead |
            PluginPermission.FileSystemWrite |
            PluginPermission.NetworkConnect));
    }

    [Test]
    public void ParsePermissions_WithNullList_ShouldReturnNone ()
    {
        // Arrange
        IEnumerable<string>? permissionStrings = null;

        // Act
        var result = PluginPermissionManager.ParsePermissions(permissionStrings!);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.None));
    }

    [Test]
    public void ParsePermissions_WithEmptyList_ShouldReturnNone ()
    {
        // Arrange
        var permissionStrings = Array.Empty<string>();

        // Act
        var result = PluginPermissionManager.ParsePermissions(permissionStrings);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.None));
    }

    [Test]
    public void ParsePermissions_WithMixedValidInvalid_ShouldParseValidOnes ()
    {
        // Arrange
        var permissionStrings = new[]
        {
            "filesystem:read",
            "invalid:permission",
            "network:connect",
            "another:invalid"
        };

        // Act
        var result = PluginPermissionManager.ParsePermissions(permissionStrings);

        // Assert
        Assert.That(result, Is.EqualTo(
            PluginPermission.FileSystemRead |
            PluginPermission.NetworkConnect));
    }

    #endregion

    #region Permission String Conversion Tests

    [Test]
    public void PermissionToString_WithNone_ShouldReturnNone ()
    {
        // Act
        var result = PluginPermissionManager.PermissionToString(PluginPermission.None);

        // Assert
        Assert.That(result, Is.EqualTo("None"));
    }

    [Test]
    public void PermissionToString_WithAll_ShouldReturnAll ()
    {
        // Act
        var result = PluginPermissionManager.PermissionToString(PluginPermission.All);

        // Assert
        Assert.That(result, Is.EqualTo("All"));
    }

    [Test]
    public void PermissionToString_WithSinglePermission_ShouldReturnReadableString ()
    {
        // Act
        var result = PluginPermissionManager.PermissionToString(PluginPermission.FileSystemRead);

        // Assert
        Assert.That(result, Is.EqualTo("File System Read"));
    }

    [Test]
    public void PermissionToString_WithMultiplePermissions_ShouldReturnCommaSeparated ()
    {
        // Arrange
        var permissions = PluginPermission.FileSystemRead | PluginPermission.NetworkConnect;

        // Act
        var result = PluginPermissionManager.PermissionToString(permissions);

        // Assert
        Assert.That(result, Does.Contain("File System Read"));
        Assert.That(result, Does.Contain("Network Connect"));
        Assert.That(result, Does.Contain(","));
    }

    [Test]
    public void PermissionToString_WithAllIndividualPermissions_ShouldReturnAll ()
    {
        // Arrange - combine all 6 individual permissions (equals All flag)
        var permissions = PluginPermission.FileSystemRead |
                         PluginPermission.FileSystemWrite |
                         PluginPermission.NetworkConnect |
                         PluginPermission.ConfigRead |
                         PluginPermission.ConfigWrite |
                         PluginPermission.RegistryRead;

        // Act
        var result = PluginPermissionManager.PermissionToString(permissions);

        // Assert - when all individual flags are set, it equals All and returns "All"
        Assert.That(result, Is.EqualTo("All"));
    }

    #endregion

    #region HasPermission Tests

    [Test]
    public void HasPermission_WithNullPluginName_ShouldReturnFalse ()
    {
        // Act
        var result = PluginPermissionManager.HasPermission(null!, PluginPermission.FileSystemRead);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasPermission_WithEmptyPluginName_ShouldReturnFalse ()
    {
        // Act
        var result = PluginPermissionManager.HasPermission("", PluginPermission.FileSystemRead);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasPermission_WithDefaultPermissions_ShouldAllowFileSystemRead ()
    {
        // Arrange
        var pluginName = "TestPlugin";

        // Act
        var result = PluginPermissionManager.HasPermission(pluginName, PluginPermission.FileSystemRead);

        // Assert
        Assert.That(result, Is.True, "Default permissions should include FileSystemRead");
    }

    [Test]
    public void HasPermission_WithDefaultPermissions_ShouldAllowConfigRead ()
    {
        // Arrange
        var pluginName = "TestPlugin";

        // Act
        var result = PluginPermissionManager.HasPermission(pluginName, PluginPermission.ConfigRead);

        // Assert
        Assert.That(result, Is.True, "Default permissions should include ConfigRead");
    }

    [Test]
    public void HasPermission_WithDefaultPermissions_ShouldDenyNetworkConnect ()
    {
        // Arrange
        var pluginName = "TestPlugin";

        // Act
        var result = PluginPermissionManager.HasPermission(pluginName, PluginPermission.NetworkConnect);

        // Assert
        Assert.That(result, Is.False, "Default permissions should NOT include NetworkConnect");
    }

    [Test]
    public void HasPermission_WithExplicitPermissions_ShouldUseExplicitSettings ()
    {
        // Arrange
        var pluginName = "TestPlugin";
        PluginPermissionManager.SetPermissions(pluginName, PluginPermission.NetworkConnect);

        // Act
        var hasNetwork = PluginPermissionManager.HasPermission(pluginName, PluginPermission.NetworkConnect);
        var hasFileSystem = PluginPermissionManager.HasPermission(pluginName, PluginPermission.FileSystemRead);

        // Assert
        Assert.That(hasNetwork, Is.True, "Explicit permissions should allow NetworkConnect");
        Assert.That(hasFileSystem, Is.False, "Explicit permissions should override defaults");
    }

    #endregion

    #region SetPermissions Tests

    [Test]
    public void SetPermissions_WithValidPluginName_ShouldSetPermissions ()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var permissions = PluginPermission.FileSystemRead | PluginPermission.NetworkConnect;

        // Act
        PluginPermissionManager.SetPermissions(pluginName, permissions);

        // Assert
        var hasFileRead = PluginPermissionManager.HasPermission(pluginName, PluginPermission.FileSystemRead);
        var hasNetwork = PluginPermissionManager.HasPermission(pluginName, PluginPermission.NetworkConnect);
        var hasConfigWrite = PluginPermissionManager.HasPermission(pluginName, PluginPermission.ConfigWrite);

        Assert.That(hasFileRead, Is.True);
        Assert.That(hasNetwork, Is.True);
        Assert.That(hasConfigWrite, Is.False);
    }

    [Test]
    public void SetPermissions_WithNullPluginName_ShouldThrowArgumentNullException ()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PluginPermissionManager.SetPermissions(null!, PluginPermission.FileSystemRead));
    }

    [Test]
    public void SetPermissions_WithEmptyPluginName_ShouldThrowArgumentNullException ()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PluginPermissionManager.SetPermissions("", PluginPermission.FileSystemRead));
    }

    [Test]
    public void SetPermissions_CalledTwice_ShouldUpdatePermissions ()
    {
        // Arrange
        var pluginName = "TestPlugin";

        // Act
        PluginPermissionManager.SetPermissions(pluginName, PluginPermission.FileSystemRead);
        var firstCheck = PluginPermissionManager.HasPermission(pluginName, PluginPermission.FileSystemRead);

        PluginPermissionManager.SetPermissions(pluginName, PluginPermission.NetworkConnect);
        var hasFileRead = PluginPermissionManager.HasPermission(pluginName, PluginPermission.FileSystemRead);
        var hasNetwork = PluginPermissionManager.HasPermission(pluginName, PluginPermission.NetworkConnect);

        // Assert
        Assert.That(firstCheck, Is.True, "Initial permission should be set");
        Assert.That(hasFileRead, Is.False, "Old permission should be replaced");
        Assert.That(hasNetwork, Is.True, "New permission should be set");
    }

    #endregion

    #region GetPermissions Tests

    [Test]
    public void GetPermissions_WithNullPluginName_ShouldReturnNone ()
    {
        // Act
        var result = PluginPermissionManager.GetPermissions(null!);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.None));
    }

    [Test]
    public void GetPermissions_WithEmptyPluginName_ShouldReturnNone ()
    {
        // Act
        var result = PluginPermissionManager.GetPermissions("");

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.None));
    }

    [Test]
    public void GetPermissions_WithUnconfiguredPlugin_ShouldReturnDefaultPermissions ()
    {
        // Arrange
        var pluginName = "UnconfiguredPlugin";

        // Act
        var result = PluginPermissionManager.GetPermissions(pluginName);

        // Assert
        Assert.That(result, Is.EqualTo(PluginPermission.FileSystemRead | PluginPermission.ConfigRead));
    }

    [Test]
    public void GetPermissions_WithConfiguredPlugin_ShouldReturnConfiguredPermissions ()
    {
        // Arrange
        var pluginName = "ConfiguredPlugin";
        var expectedPermissions = PluginPermission.NetworkConnect | PluginPermission.FileSystemWrite;
        PluginPermissionManager.SetPermissions(pluginName, expectedPermissions);

        // Act
        var result = PluginPermissionManager.GetPermissions(pluginName);

        // Assert
        Assert.That(result, Is.EqualTo(expectedPermissions));
    }

    #endregion

    #region Persistence Tests

    [Test]
    public void SavePermissions_WithValidDirectory_ShouldCreateFile ()
    {
        // Arrange
        var pluginName = "TestPlugin";
        PluginPermissionManager.SetPermissions(pluginName, PluginPermission.All);
        var expectedFile = Path.Join(_testConfigDir, "plugin-permissions.json");

        // Act
        PluginPermissionManager.SavePermissions(_testConfigDir);

        // Assert
        Assert.That(File.Exists(expectedFile), Is.True, "Permissions file should be created");
    }

    [Test]
    public void SavePermissions_ThenLoadPermissions_ShouldPersistData ()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var permissions = PluginPermission.FileSystemRead | PluginPermission.NetworkConnect;
        PluginPermissionManager.SetPermissions(pluginName, permissions);

        // Act
        PluginPermissionManager.SavePermissions(_testConfigDir);

        // Reset by setting different permissions
        PluginPermissionManager.SetPermissions(pluginName, PluginPermission.None);
        Assert.That(PluginPermissionManager.GetPermissions(pluginName), Is.EqualTo(PluginPermission.None));

        // Load saved permissions
        PluginPermissionManager.LoadPermissions(_testConfigDir);

        // Assert
        var loadedPermissions = PluginPermissionManager.GetPermissions(pluginName);
        Assert.That(loadedPermissions, Is.EqualTo(permissions), "Loaded permissions should match saved permissions");
    }

    [Test]
    public void LoadPermissions_WithNonExistentFile_ShouldNotThrow ()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => PluginPermissionManager.LoadPermissions(_testConfigDir));
    }

    [Test]
    public void LoadPermissions_WithInvalidJson_ShouldNotThrow ()
    {
        // Arrange
        var permissionsFile = Path.Join(_testConfigDir, "plugin-permissions.json");
        File.WriteAllText(permissionsFile, "{ invalid json content }");

        // Act & Assert
        Assert.DoesNotThrow(() => PluginPermissionManager.LoadPermissions(_testConfigDir));
    }

    [Test]
    public void SavePermissions_WithInvalidDirectory_ShouldNotThrow ()
    {
        // Arrange
        var invalidDir = "Z:\\NonExistent\\Directory\\Path";

        // Act & Assert
        Assert.DoesNotThrow(() => PluginPermissionManager.SavePermissions(invalidDir));
    }

    [Test]
    public void SavePermissions_WithMultiplePlugins_ShouldSaveAll ()
    {
        // Arrange
        PluginPermissionManager.SetPermissions("Plugin1", PluginPermission.FileSystemRead);
        PluginPermissionManager.SetPermissions("Plugin2", PluginPermission.NetworkConnect);
        PluginPermissionManager.SetPermissions("Plugin3", PluginPermission.All);

        // Act
        PluginPermissionManager.SavePermissions(_testConfigDir);

        // Reset permissions
        PluginPermissionManager.SetPermissions("Plugin1", PluginPermission.None);
        PluginPermissionManager.SetPermissions("Plugin2", PluginPermission.None);
        PluginPermissionManager.SetPermissions("Plugin3", PluginPermission.None);

        // Load
        PluginPermissionManager.LoadPermissions(_testConfigDir);

        // Assert
        Assert.That(PluginPermissionManager.GetPermissions("Plugin1"), Is.EqualTo(PluginPermission.FileSystemRead));
        Assert.That(PluginPermissionManager.GetPermissions("Plugin2"), Is.EqualTo(PluginPermission.NetworkConnect));
        Assert.That(PluginPermissionManager.GetPermissions("Plugin3"), Is.EqualTo(PluginPermission.All));
    }

    #endregion

    #region Permission Flag Tests

    [Test]
    public void PluginPermission_AllFlag_ShouldIncludeAllPermissions ()
    {
        // Arrange
        var all = PluginPermission.All;

        // Assert
        Assert.That(all.HasFlag(PluginPermission.FileSystemRead), Is.True);
        Assert.That(all.HasFlag(PluginPermission.FileSystemWrite), Is.True);
        Assert.That(all.HasFlag(PluginPermission.NetworkConnect), Is.True);
        Assert.That(all.HasFlag(PluginPermission.ConfigRead), Is.True);
        Assert.That(all.HasFlag(PluginPermission.ConfigWrite), Is.True);
        Assert.That(all.HasFlag(PluginPermission.RegistryRead), Is.True);
    }

    [Test]
    public void PluginPermission_NoneFlag_ShouldNotIncludeAnyPermissions ()
    {
        // Arrange
        var none = PluginPermission.None;

        // Assert
        Assert.That(none.HasFlag(PluginPermission.FileSystemRead), Is.False);
        Assert.That(none.HasFlag(PluginPermission.FileSystemWrite), Is.False);
        Assert.That(none.HasFlag(PluginPermission.NetworkConnect), Is.False);
        Assert.That(none.HasFlag(PluginPermission.ConfigRead), Is.False);
        Assert.That(none.HasFlag(PluginPermission.ConfigWrite), Is.False);
        Assert.That(none.HasFlag(PluginPermission.RegistryRead), Is.False);
    }

    [Test]
    public void PluginPermission_CombinedFlags_ShouldWorkCorrectly ()
    {
        // Arrange
        var combined = PluginPermission.FileSystemRead | PluginPermission.FileSystemWrite;

        // Assert
        Assert.That(combined.HasFlag(PluginPermission.FileSystemRead), Is.True);
        Assert.That(combined.HasFlag(PluginPermission.FileSystemWrite), Is.True);
        Assert.That(combined.HasFlag(PluginPermission.NetworkConnect), Is.False);
    }

    #endregion
}
