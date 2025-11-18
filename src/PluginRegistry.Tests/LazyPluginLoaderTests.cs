using NUnit.Framework;
using LogExpert.PluginRegistry;
using System.IO;
using LogExpert.Core.Interface;

namespace LogExpert.PluginRegistry.Tests;

[TestFixture]
public class LazyPluginLoaderTests
{
    [Test]
    public void Constructor_WithValidPath_CreatesInstance()
    {
        // Arrange
        var dllPath = "test.dll";
        var manifest = new PluginManifest { Name = "TestPlugin", Version = "1.0.0" };

        // Act
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllPath, manifest);

        // Assert
        Assert.IsNotNull(loader);
        Assert.AreEqual(dllPath, loader.DllPath);
        Assert.AreEqual(manifest, loader.Manifest);
        Assert.IsFalse(loader.IsLoaded);
    }

    [Test]
    public void Constructor_WithNullManifest_CreatesInstance()
    {
        // Arrange
        var dllPath = "test.dll";

        // Act
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllPath, null);

        // Assert
        Assert.IsNotNull(loader);
        Assert.AreEqual(dllPath, loader.DllPath);
        Assert.IsNull(loader.Manifest);
        Assert.IsFalse(loader.IsLoaded);
    }

    [Test]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new LazyPluginLoader<ILogLineColumnizer>(null, null));
    }

    [Test]
    public void GetInstance_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid() + ".dll");
        var loader = new LazyPluginLoader<ILogLineColumnizer>(nonExistentPath, null);

        // Act
        var instance = loader.GetInstance();

        // Assert
        Assert.IsNull(instance);
        Assert.IsTrue(loader.IsLoaded); // Marked as loaded even on failure
    }

    [Test]
    public void GetInstance_CalledTwice_ReturnsSameInstance()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid() + ".dll");
        var loader = new LazyPluginLoader<ILogLineColumnizer>(nonExistentPath, null);

        // Act
        var instance1 = loader.GetInstance();
        var instance2 = loader.GetInstance();

        // Assert
        Assert.AreSame(instance1, instance2);
        Assert.IsTrue(loader.IsLoaded);
    }

    [Test]
    public void IsLoaded_BeforeGetInstance_ReturnsFalse()
    {
        // Arrange
        var loader = new LazyPluginLoader<ILogLineColumnizer>("test.dll", null);

        // Assert
        Assert.IsFalse(loader.IsLoaded);
    }

    [Test]
    public void IsLoaded_AfterGetInstance_ReturnsTrue()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid() + ".dll");
        var loader = new LazyPluginLoader<ILogLineColumnizer>(nonExistentPath, null);

        // Act
        _ = loader.GetInstance();

        // Assert
        Assert.IsTrue(loader.IsLoaded);
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var dllPath = "C:\\plugins\\TestPlugin.dll";
        var loader = new LazyPluginLoader<ILogLineColumnizer>(dllPath, null);

        // Act
        var result = loader.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result, Does.Contain("LazyPluginLoader"));
        Assert.That(result, Does.Contain("ILogLineColumnizer"));
        Assert.That(result, Does.Contain("TestPlugin.dll"));
        Assert.That(result, Does.Contain("Loaded: False"));
    }

    // NOTE: Integration tests that load actual plugin DLLs should be in a separate test class
    // marked with [Category("Integration")] to test actual plugin loading behavior
}
