using NUnit.Framework;
using LogExpert.PluginRegistry;
using System.IO;

namespace LogExpert.PluginRegistry.Tests;

/// <summary>
/// Tests for PluginManifest validation and version compatibility.
/// Phase 1: Manifest validation, version requirements, permissions
/// </summary>
[TestFixture]
public class PluginManifestTests
{
    private string _testDataPath = null!;

    [SetUp]
    public void SetUp()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "LogExpertManifestTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDataPath))
        {
            try
            {
                Directory.Delete(_testDataPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Validation Tests

    [Test]
    public void Validate_WithAllRequiredFields_ShouldSucceed()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_WithMissingName_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Does.Contain("Missing required field: name"));
    }

    [Test]
    public void Validate_WithMissingVersion_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Does.Contain("Missing required field: version"));
    }

    [Test]
    public void Validate_WithInvalidVersionFormat_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "not-a-version",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors.Any(e => e.Contains("Invalid version format")), Is.True);
    }

    [Test]
    public void Validate_WithMissingAuthor_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Does.Contain("Missing required field: author"));
    }

    [Test]
    public void Validate_WithMissingDescription_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Does.Contain("Missing required field: description"));
    }

    [Test]
    public void Validate_WithMissingMain_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = ""
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Does.Contain("Missing required field: main"));
    }

    [Test]
    public void Validate_WithMissingApiVersion_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Does.Contain("Missing required field: apiVersion"));
    }

    [Test]
    public void Validate_WithMultipleMissingFields_ShouldReportAll()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "",
            Version = "",
            Author = "",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(errors, Does.Contain("Missing required field: name"));
        Assert.That(errors, Does.Contain("Missing required field: version"));
        Assert.That(errors, Does.Contain("Missing required field: author"));
    }

    [Test]
    public void Validate_WithValidSemanticVersion_ShouldSucceed()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.2.3-beta+build.456",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors, Is.Empty);
    }

    #endregion

    #region Version Requirement Validation Tests

    [Test]
    public void Validate_WithInvalidLogExpertVersionRequirement_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("invalid-version", ">=8.0")
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors.Any(e => e.Contains("Invalid LogExpert version requirement")), Is.True);
    }

    [Test]
    public void Validate_WithInvalidDotNetVersionRequirement_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">=2.0.0", "not-a-version")
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors.Any(e => e.Contains("Invalid .NET version requirement")), Is.True);
    }

    [Test]
    public void Validate_WithValidVersionRequirements_ShouldSucceed()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">=2.0.0", ">=8.0")
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors, Is.Empty);
    }

    #endregion

    #region Permission Validation Tests

    [Test]
    public void Validate_WithValidPermissions_ShouldSucceed()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Permissions = new List<string> { "filesystem:read", "filesystem:write", "network:connect" }
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_WithInvalidPermission_ShouldFail()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Permissions = new List<string> { "invalid:permission" }
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors.Any(e => e.Contains("Invalid permission")), Is.True);
    }

    [Test]
    public void Validate_WithMixedValidAndInvalidPermissions_ShouldFailAndReportInvalid()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Permissions = new List<string> { "filesystem:read", "invalid:permission", "network:connect" }
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors.Count, Is.EqualTo(1));
        Assert.That(errors[0], Does.Contain("invalid:permission"));
    }

    [Test]
    public void Validate_WithAllValidPermissions_ShouldSucceed()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Permissions = new List<string> 
            { 
                "filesystem:read",
                "filesystem:write",
                "network:connect",
                "config:read",
                "config:write",
                "registry:read"
            }
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors, Is.Empty);
    }

    #endregion

    #region Version Compatibility Tests

    [Test]
    public void IsCompatibleWith_WithNoRequirement_ShouldReturnTrue()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll"
        };

        // Act
        var isCompatible = manifest.IsCompatibleWith(new Version("1.0.0"));

        // Assert
        Assert.That(isCompatible, Is.True);
    }

    [Test]
    public void IsCompatibleWith_GreaterThanOrEqual_WithCompatibleVersion_ShouldReturnTrue()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">=2.0.0", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("2.0.0")), Is.True);
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.0")), Is.True);
        Assert.That(manifest.IsCompatibleWith(new Version("3.0.0")), Is.True);
    }

    [Test]
    public void IsCompatibleWith_GreaterThanOrEqual_WithIncompatibleVersion_ShouldReturnFalse()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">=2.0.0", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("1.9.9")), Is.False);
        Assert.That(manifest.IsCompatibleWith(new Version("1.0.0")), Is.False);
    }

    [Test]
    public void IsCompatibleWith_ExactVersion_ShouldWorkCorrectly()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("[2.5.0]", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.0")), Is.True);
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.1")), Is.False);
        Assert.That(manifest.IsCompatibleWith(new Version("2.4.9")), Is.False);
    }

    [Test]
    public void IsCompatibleWith_VersionRange_ShouldRespectBounds()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("[2.0.0, 3.0.0)", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("2.0.0")), Is.True, "Lower bound inclusive");
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.0")), Is.True, "Within range");
        Assert.That(manifest.IsCompatibleWith(new Version("2.9.9")), Is.True, "Just below upper bound");
        Assert.That(manifest.IsCompatibleWith(new Version("3.0.0")), Is.False, "Upper bound exclusive");
        Assert.That(manifest.IsCompatibleWith(new Version("1.9.9")), Is.False, "Below range");
    }

    [Test]
    public void IsCompatibleWith_GreaterThan_ShouldExcludeLowerBound()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">2.0.0", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("2.0.0")), Is.False, "Exact version excluded");
        Assert.That(manifest.IsCompatibleWith(new Version("2.0.1")), Is.True, "Higher version included");
    }

    [Test]
    public void IsCompatibleWith_LessThanOrEqual_ShouldIncludeUpperBound()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("<=3.0.0", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("3.0.0")), Is.True, "Upper bound inclusive");
        Assert.That(manifest.IsCompatibleWith(new Version("2.9.9")), Is.True, "Below upper bound");
        Assert.That(manifest.IsCompatibleWith(new Version("3.0.1")), Is.False, "Above upper bound");
    }

    [Test]
    public void IsCompatibleWith_LessThan_ShouldExcludeUpperBound()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("<3.0.0", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("3.0.0")), Is.False, "Upper bound excluded");
        Assert.That(manifest.IsCompatibleWith(new Version("2.9.9")), Is.True, "Below upper bound");
    }

    [Test]
    public void IsCompatibleWith_TildeOperator_ShouldAllowPatchUpdates()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("~2.5.0", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.0")), Is.True, "Exact version");
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.1")), Is.True, "Patch update");
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.9")), Is.True, "Higher patch");
        Assert.That(manifest.IsCompatibleWith(new Version("2.6.0")), Is.False, "Minor update excluded");
        Assert.That(manifest.IsCompatibleWith(new Version("2.4.9")), Is.False, "Below range");
    }

    [Test]
    public void IsCompatibleWith_CaretOperator_ShouldAllowMinorAndPatchUpdates()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("^2.5.0", ">=8.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.0")), Is.True, "Exact version");
        Assert.That(manifest.IsCompatibleWith(new Version("2.5.1")), Is.True, "Patch update");
        Assert.That(manifest.IsCompatibleWith(new Version("2.6.0")), Is.True, "Minor update");
        Assert.That(manifest.IsCompatibleWith(new Version("2.9.9")), Is.True, "Higher minor");
        Assert.That(manifest.IsCompatibleWith(new Version("3.0.0")), Is.False, "Major update excluded");
        Assert.That(manifest.IsCompatibleWith(new Version("2.4.9")), Is.False, "Below range");
    }

    [Test]
    public void IsCompatibleWith_WithInvalidRequirement_ShouldReturnFalse()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("invalid-version-format", ">=8.0")
        };

        // Act
        var isCompatible = manifest.IsCompatibleWith(new Version("2.5.0"));

        // Assert
        Assert.That(isCompatible, Is.False, "Invalid requirement should fail closed");
    }

    #endregion

    #region Load Manifest Tests

    [Test]
    public void Load_WithValidJsonFile_ShouldLoadSuccessfully()
    {
        // Arrange
        var manifestPath = Path.Combine(_testDataPath, "valid.manifest.json");
        var json = @"{
            ""name"": ""TestPlugin"",
            ""version"": ""1.0.0"",
            ""author"": ""Test Author"",
            ""description"": ""Test plugin"",
            ""apiVersion"": ""1.0"",
            ""main"": ""TestPlugin.dll"",
            ""requires"": {
                ""logExpert"": "">=2.0.0"",
                ""dotnet"": "">=8.0""
            },
            ""permissions"": [""filesystem:read""],
            ""url"": ""https://example.com"",
            ""license"": ""MIT""
        }";
        File.WriteAllText(manifestPath, json);

        // Act
        var manifest = PluginManifest.Load(manifestPath);

        // Assert
        Assert.That(manifest, Is.Not.Null);
        Assert.That(manifest!.Name, Is.EqualTo("TestPlugin"));
        Assert.That(manifest.Version, Is.EqualTo("1.0.0"));
        Assert.That(manifest.Author, Is.EqualTo("Test Author"));
        Assert.That(manifest.Description, Is.EqualTo("Test plugin"));
        Assert.That(manifest.ApiVersion, Is.EqualTo("1.0"));
        Assert.That(manifest.Main, Is.EqualTo("TestPlugin.dll"));
        Assert.That(manifest.Requires, Is.Not.Null);
        Assert.That(manifest.Requires!.LogExpert, Is.EqualTo(">=2.0.0"));
        Assert.That(manifest.Requires.DotNet, Is.EqualTo(">=8.0"));
        Assert.That(manifest.Permissions, Has.Count.EqualTo(1));
        Assert.That(manifest.Permissions[0], Is.EqualTo("filesystem:read"));
        Assert.That(manifest.Url, Is.EqualTo("https://example.com"));
        Assert.That(manifest.License, Is.EqualTo("MIT"));
    }

    [Test]
    public void Load_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        var manifestPath = Path.Combine(_testDataPath, "nonexistent.manifest.json");

        // Act
        var manifest = PluginManifest.Load(manifestPath);

        // Assert
        Assert.That(manifest, Is.Null);
    }

    [Test]
    public void Load_WithInvalidJson_ShouldReturnNull()
    {
        // Arrange
        var manifestPath = Path.Combine(_testDataPath, "invalid.manifest.json");
        File.WriteAllText(manifestPath, "{ invalid json }");

        // Act
        var manifest = PluginManifest.Load(manifestPath);

        // Assert
        Assert.That(manifest, Is.Null);
    }

    [Test]
    public void Load_WithEmptyFile_ShouldReturnNull()
    {
        // Arrange
        var manifestPath = Path.Combine(_testDataPath, "empty.manifest.json");
        File.WriteAllText(manifestPath, "");

        // Act
        var manifest = PluginManifest.Load(manifestPath);

        // Assert
        Assert.That(manifest, Is.Null);
    }

    [Test]
    public void Load_WithMinimalValidJson_ShouldLoadWithDefaults()
    {
        // Arrange
        var manifestPath = Path.Combine(_testDataPath, "minimal.manifest.json");
        var json = @"{
            ""name"": ""MinimalPlugin"",
            ""version"": ""1.0.0"",
            ""author"": ""Test"",
            ""description"": ""Minimal"",
            ""apiVersion"": ""1.0"",
            ""main"": ""plugin.dll""
        }";
        File.WriteAllText(manifestPath, json);

        // Act
        var manifest = PluginManifest.Load(manifestPath);

        // Assert
        Assert.That(manifest, Is.Not.Null);
        Assert.That(manifest!.Permissions, Is.Not.Null);
        Assert.That(manifest.Permissions, Is.Empty);
        Assert.That(manifest.Dependencies, Is.Not.Null);
        Assert.That(manifest.Dependencies, Is.Empty);
    }

    #endregion

    #region Optional Fields Tests

    [Test]
    public void Manifest_WithOptionalUrl_ShouldStoreCorrectly()
    {
        // Arrange & Act
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Url = "https://github.com/test/plugin"
        };

        // Assert
        Assert.That(manifest.Url, Is.EqualTo("https://github.com/test/plugin"));
    }

    [Test]
    public void Manifest_WithOptionalLicense_ShouldStoreCorrectly()
    {
        // Arrange & Act
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            License = "Apache-2.0"
        };

        // Assert
        Assert.That(manifest.License, Is.EqualTo("Apache-2.0"));
    }

    [Test]
    public void Manifest_WithDependencies_ShouldStoreCorrectly()
    {
        // Arrange & Act
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test plugin",
            ApiVersion = "1.0",
            Main = "TestPlugin.dll",
            Dependencies = new Dictionary<string, string>
            {
                { "Newtonsoft.Json", ">=13.0.0" },
                { "NLog", ">=5.0.0" }
            }
        };

        // Assert
        Assert.That(manifest.Dependencies, Has.Count.EqualTo(2));
        Assert.That(manifest.Dependencies["Newtonsoft.Json"], Is.EqualTo(">=13.0.0"));
        Assert.That(manifest.Dependencies["NLog"], Is.EqualTo(">=5.0.0"));
    }

    #endregion
}
