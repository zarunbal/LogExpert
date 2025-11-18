using LogExpert.PluginRegistry;

using NUnit.Framework;

namespace LogExpert.Tests.PluginRegistry;

[TestFixture]
public class PluginManifestVersionParsingTests
{
    [Test]
    public void Validate_WithVersionRequirementWithSpaces_ShouldPass ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">= 1.10.0", ">= 8.0.0")
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True, "Manifest should be valid with spaces in version requirements");
        Assert.That(errors, Is.Empty, "Should have no validation errors");
    }

    [Test]
    public void Validate_WithVersionRequirementWithoutSpaces_ShouldPass ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">=1.10.0", ">=8.0.0")
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True, "Manifest should be valid without spaces in version requirements");
        Assert.That(errors, Is.Empty, "Should have no validation errors");
    }

    [Test]
    [TestCase(">= 1.10.0")]
    [TestCase(">=1.10.0")]
    [TestCase("> 1.10.0")]
    [TestCase(">1.10.0")]
    [TestCase("<= 2.0.0")]
    [TestCase("<=2.0.0")]
    [TestCase("< 2.0.0")]
    [TestCase("<2.0.0")]
    [TestCase("~ 1.10.0")]
    [TestCase("~1.10.0")]
    [TestCase("^ 1.10.0")]
    [TestCase("^1.10.0")]
    public void Validate_WithVariousVersionRequirementFormats_ShouldPass (string requirement)
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(requirement, ">=8.0.0")
        };

        // Act
        var isValid = manifest.Validate(out var errors);

        // Assert
        Assert.That(isValid, Is.True, $"Manifest should be valid with requirement: {requirement}");
        Assert.That(errors, Is.Empty, $"Should have no validation errors for: {requirement}");
    }

    [Test]
    public void IsCompatibleWith_WithVersionRequirementWithSpaces_ShouldWorkCorrectly ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">= 1.10.0", ">= 8.0.0")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 0)), Is.True, "Should be compatible with 1.10.0");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 11, 0)), Is.True, "Should be compatible with 1.11.0");
        Assert.That(manifest.IsCompatibleWith(new Version(2, 0, 0)), Is.True, "Should be compatible with 2.0.0");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 9, 0)), Is.False, "Should NOT be compatible with 1.9.0");
    }

    [Test]
    public void IsCompatibleWith_WithCaretRange_ShouldAllowMinorUpdates ()
    {
        // Arrange - ^ allows minor and patch updates but not major
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("^ 1.10.0", null)
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 0)), Is.True, "Should be compatible with 1.10.0");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 11, 0)), Is.True, "Should be compatible with 1.11.0");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 5)), Is.True, "Should be compatible with 1.10.5");
        Assert.That(manifest.IsCompatibleWith(new Version(2, 0, 0)), Is.False, "Should NOT be compatible with 2.0.0");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 9, 0)), Is.False, "Should NOT be compatible with 1.9.0");
    }

    [Test]
    public void IsCompatibleWith_WithTildeRange_ShouldAllowPatchUpdates ()
    {
        // Arrange - ~ allows patch updates but not minor or major
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("~ 1.10.0", null)
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 0)), Is.True, "Should be compatible with 1.10.0");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 1)), Is.True, "Should be compatible with 1.10.1");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 99)), Is.True, "Should be compatible with 1.10.99");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 11, 0)), Is.False, "Should NOT be compatible with 1.11.0");
        Assert.That(manifest.IsCompatibleWith(new Version(2, 0, 0)), Is.False, "Should NOT be compatible with 2.0.0");
    }

    [Test]
    public void IsCompatibleWith_WithGreaterThan_ShouldExcludeEqualVersion ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("> 1.10.0", null)
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 0)), Is.False, "Should NOT be compatible with 1.10.0 (must be greater)");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 1)), Is.True, "Should be compatible with 1.10.1");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 11, 0)), Is.True, "Should be compatible with 1.11.0");
    }

    [Test]
    public void IsCompatibleWith_WithLessThan_ShouldExcludeEqualVersion ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("< 2.0.0", null)
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version(1, 11, 0)), Is.True, "Should be compatible with 1.11.0");
        Assert.That(manifest.IsCompatibleWith(new Version(1, 99, 0)), Is.True, "Should be compatible with 1.99.0");
        Assert.That(manifest.IsCompatibleWith(new Version(2, 0, 0)), Is.False, "Should NOT be compatible with 2.0.0 (must be less)");
        Assert.That(manifest.IsCompatibleWith(new Version(2, 1, 0)), Is.False, "Should NOT be compatible with 2.1.0");
    }

    [Test]
    public void IsCompatibleWith_WithNoRequirement_ShouldAlwaysBeCompatible ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = null
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version(1, 0, 0)), Is.True);
        Assert.That(manifest.IsCompatibleWith(new Version(1, 10, 0)), Is.True);
        Assert.That(manifest.IsCompatibleWith(new Version(2, 0, 0)), Is.True);
        Assert.That(manifest.IsCompatibleWith(new Version(99, 99, 99)), Is.True);
    }

    [Test]
    public void IsCompatibleWith_WithEmptyRequirement_ShouldAlwaysBeCompatible ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements("", "")
        };

        // Act & Assert
        Assert.That(manifest.IsCompatibleWith(new Version(1, 0, 0)), Is.True);
        Assert.That(manifest.IsCompatibleWith(new Version(99, 99, 99)), Is.True);
    }

    [Test]
    public void DebugVersionParsing_WithExactInputFromUser ()
    {
        // Arrange - Test the EXACT input that's causing the issue
        var requirement = ">= 1.10.0";

        // Act - Try to parse it directly
        try
        {
            var normalized = requirement
                .Replace(">= ", ">=", StringComparison.Ordinal)
                .Trim();

            TestContext.WriteLine($"Original: '{requirement}'");
            TestContext.WriteLine($"Normalized: '{normalized}'");

            var range = NuGet.Versioning.VersionRange.Parse(normalized);
            TestContext.WriteLine($"Parsed successfully: {range}");

            // Assert
            Assert.That(range, Is.Not.Null);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Exception: {ex.GetType().Name}");
            TestContext.WriteLine($"Message: {ex.Message}");
            TestContext.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    [Test]
    public void Manifest_Validate_WithSpacesInRequirement_ShouldNotThrow ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test plugin",
            ApiVersion = "2.0",
            Main = "TestPlugin.dll",
            Requires = new PluginRequirements(">= 1.10.0", ">= 8.0.0")
        };

        // Act & Assert - Should not throw
        try
        {
            var isValid = manifest.Validate(out var errors);

            if (!isValid)
            {
                TestContext.WriteLine("Validation errors:");
                foreach (var error in errors)
                {
                    TestContext.WriteLine($"  - {error}");
                }
            }

            Assert.That(isValid, Is.True, "Validation should pass");
            Assert.That(errors, Is.Empty, "Should have no errors");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Exception during validation: {ex.GetType().Name}");
            TestContext.WriteLine($"Message: {ex.Message}");
            TestContext.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}
