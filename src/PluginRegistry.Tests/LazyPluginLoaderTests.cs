using ColumnizerLib;

using LogExpert.PluginRegistry;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

[TestFixture]
public class LazyPluginLoaderTests
{
    [Test]
    public void Constructor_WithValidPath_CreatesInstance ()
    {
        // Arrange
        var dllPath = "test.dll";
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test Plugin",
            ApiVersion = "1.0",
            Main = "test.dll"
        };

        // Act
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllPath, manifest);

        // Assert
        Assert.That(loader, Is.Not.Null);
        Assert.That(loader.DllPath, Is.EqualTo(dllPath));
        Assert.That(loader.Manifest, Is.EqualTo(manifest));
        Assert.That(loader.IsLoaded, Is.False);
    }

    [Test]
    public void Constructor_WithNullManifest_CreatesInstance ()
    {
        // Arrange
        var dllPath = "test.dll";

        // Act
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllPath, null);

        // Assert
        Assert.That(loader, Is.Not.Null);
        Assert.That(loader.DllPath, Is.EqualTo(dllPath));
        Assert.That(loader.Manifest, Is.Null);
        Assert.That(loader.IsLoaded, Is.False);
    }

    [Test]
    public void Constructor_WithNullPath_ThrowsArgumentNullException ()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new LazyPluginLoader<ILogLineColumnizer>(null, null));
    }

    [Test]
    public void GetInstance_WithNonExistentFile_ReturnsNull ()
    {
        // Arrange
        var nonExistentPath = Path.Join(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid() + ".dll");
        var loader = new LazyPluginLoader<ILogLineColumnizer>(nonExistentPath, null);

        // Act
        var instance = loader.GetInstance();

        // Assert
        Assert.That(instance, Is.Null);
        Assert.That(loader.IsLoaded, Is.True); // Marked as loaded even on failure
    }

    [Test]
    public void GetInstance_CalledTwice_ReturnsSameInstance ()
    {
        // Arrange
        var nonExistentPath = Path.Join(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid() + ".dll");
        var loader = new LazyPluginLoader<ILogLineColumnizer>(nonExistentPath, null);

        // Act
        var instance1 = loader.GetInstance();
        var instance2 = loader.GetInstance();

        // Assert
        Assert.That(instance1, Is.SameAs(instance2));
        Assert.That(loader.IsLoaded, Is.True);
    }

    [Test]
    public void IsLoaded_BeforeGetInstance_ReturnsFalse ()
    {
        // Arrange
        var loader = new LazyPluginLoader<ILogLineColumnizer>("test.dll", null);

        // Assert
        Assert.That(loader.IsLoaded, Is.False);
    }

    [Test]
    public void IsLoaded_AfterGetInstance_ReturnsTrue ()
    {
        // Arrange
        var nonExistentPath = Path.Join(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid() + ".dll");
        var loader = new LazyPluginLoader<ILogLineColumnizer>(nonExistentPath, null);

        // Act
        _ = loader.GetInstance();

        // Assert
        Assert.That(loader.IsLoaded, Is.True);
    }

    [Test]
    public void ToString_ReturnsFormattedString ()
    {
        // Arrange
        var dllPath = "C:\\plugins\\TestPlugin.dll";
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllPath, null);

        // Act
        var result = loader.ToString();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("LazyPluginLoader"));
        Assert.That(result, Does.Contain("ILogLineColumnizer"));
        Assert.That(result, Does.Contain("TestPlugin.dll"));
        Assert.That(result, Does.Contain("Loaded: False"));
    }

    // NOTE: Integration tests that load actual plugin DLLs should be in a separate test class
    // marked with [Category("Integration")] to test actual plugin loading behavior
}
