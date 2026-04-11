using LogExpert.PluginRegistry;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

[TestFixture]
public class AssemblyInspectorTests
{
    [Test]
    public void InspectAssembly_WithNullPath_ReturnsEmptyInfo ()
    {
        // Act
        var result = AssemblyInspector.InspectAssembly(null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsEmpty, Is.True);
    }

    [Test]
    public void InspectAssembly_WithEmptyPath_ReturnsEmptyInfo ()
    {
        // Act
        var result = AssemblyInspector.InspectAssembly(string.Empty);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsEmpty, Is.True);
    }

    [Test]
    public void InspectAssembly_WithNonExistentFile_ReturnsEmptyInfo ()
    {
        // Arrange
        var nonExistentPath = Path.Join(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid() + ".dll");

        // Act
        var result = AssemblyInspector.InspectAssembly(nonExistentPath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsEmpty, Is.True);
    }

    [Test]
    public void InspectAssembly_WithInvalidDll_ReturnsEmptyInfo ()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "This is not a DLL");

        try
        {
            // Act
            var result = AssemblyInspector.InspectAssembly(tempFile);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsEmpty, Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Test]
    public void IsLikelyPluginAssembly_WithColumnizerInName_ReturnsTrue ()
    {
        // Arrange
        var path = "CsvColumnizer.dll";

        // Act
        var result = AssemblyInspector.IsLikelyPluginAssembly(path);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsLikelyPluginAssembly_WithPluginInName_ReturnsTrue ()
    {
        // Arrange
        var path = "MyCustomPlugin.dll";

        // Act
        var result = AssemblyInspector.IsLikelyPluginAssembly(path);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsLikelyPluginAssembly_WithFileSystemInName_ReturnsTrue ()
    {
        // Arrange
        var path = "SftpFileSystem.dll";

        // Act
        var result = AssemblyInspector.IsLikelyPluginAssembly(path);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsLikelyPluginAssembly_WithHighlighterInName_ReturnsTrue ()
    {
        // Arrange
        var path = "FlashIconHighlighter.dll";

        // Act
        var result = AssemblyInspector.IsLikelyPluginAssembly(path);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsLikelyPluginAssembly_WithNormalDllName_ReturnsFalse ()
    {
        // Arrange
        var path = "System.Text.Json.dll";

        // Act
        var result = AssemblyInspector.IsLikelyPluginAssembly(path);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsLikelyPluginAssembly_WithNullPath_ReturnsFalse ()
    {
        // Act
        var result = AssemblyInspector.IsLikelyPluginAssembly(null);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsLikelyPluginAssembly_WithEmptyPath_ReturnsFalse ()
    {
        // Act
        var result = AssemblyInspector.IsLikelyPluginAssembly(string.Empty);

        // Assert
        Assert.That(result, Is.False);
    }

    // NOTE: Integration tests that load actual plugin DLLs should be in a separate test class
    // marked with [Category("Integration")] to allow selective test execution
}
