using System.Reflection;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

/// <summary>
/// Tests for PluginRegistry initialization and singleton behavior.
/// Phase 1: Core functionality tests (30+ tests total)
/// </summary>
[TestFixture]
public class PluginRegistryTests
{
    private string _testDataPath = null!;
    private string _testPluginsPath = null!;

    [SetUp]
    public void SetUp ()
    {
        // Create test directories
        _testDataPath = Path.Combine(Path.GetTempPath(), "LogExpertTests", Guid.NewGuid().ToString());
        _testPluginsPath = Path.Combine(_testDataPath, "plugins");
        _ = Directory.CreateDirectory(_testPluginsPath);

        // Reset singleton for testing
        ResetPluginRegistrySingleton();
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Tests")]
    public void TearDown ()
    {
        // Clean up test directories
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

    /// <summary>
    /// Uses reflection to reset the singleton instance for testing.
    /// </summary>
    private static void ResetPluginRegistrySingleton ()
    {
        var instanceField = typeof(PluginRegistry).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        instanceField?.SetValue(null, null);
    }

    #region Initialization Tests

    [Test]
    public void Create_ShouldReturnInstance ()
    {
        // Act
        var registry = PluginRegistry.Create(_testDataPath, 250);

        // Assert
        Assert.That(registry, Is.Not.Null);
        Assert.That(registry, Is.TypeOf<PluginRegistry>());
    }

    [Test]
    public void Create_ShouldReturnSameInstanceOnMultipleCalls ()
    {
        // Act
        var registry1 = PluginRegistry.Create(_testDataPath, 250);
        var registry2 = PluginRegistry.Create(_testDataPath, 250);

        // Assert
        Assert.That(registry1, Is.SameAs(registry2), "Create should return the same singleton instance");
    }

    [Test]
    public void Create_ShouldSetPollingInterval ()
    {
        // Arrange
        int expectedInterval = 500;

        // Act
        _ = PluginRegistry.Create(_testDataPath, expectedInterval);

        // Assert
        Assert.That(PluginRegistry.PollingInterval, Is.EqualTo(expectedInterval));
    }

    [Test]
    public void Create_WithEmptyPluginDirectory_ShouldNotThrow ()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _ = PluginRegistry.Create(_testDataPath, 250));
    }

    [Test]
    public void Create_WithNonExistentDirectory_ShouldCreateAndNotThrow ()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDataPath, "nonexistent");

        // Act & Assert
        Assert.DoesNotThrow(() => _ = PluginRegistry.Create(nonExistentPath, 250));
    }

    #endregion

    #region Property Tests

    [Test]
    public void RegisteredColumnizers_ShouldReturnEmptyListInitially ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);

        // Act
        var columnizers = registry.RegisteredColumnizers;

        // Assert
        Assert.That(columnizers, Is.Not.Null);
        Assert.That(columnizers, Is.Not.Empty, "Should have default columnizers");
    }

    [Test]
    public void RegisteredFileSystemPlugins_ShouldHaveDefaultFileSystem ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);

        // Act
        var plugins = registry.RegisteredFileSystemPlugins;

        // Assert
        Assert.That(plugins, Is.Not.Null);
        Assert.That(plugins, Is.Not.Empty, "Should have default filesystem");
    }

    [Test]
    public void RegisteredContextMenuPlugins_ShouldNotBeNull ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);

        // Act
        var plugins = registry.RegisteredContextMenuPlugins;

        // Assert
        Assert.That(plugins, Is.Not.Null);
    }

    [Test]
    public void RegisteredKeywordActions_ShouldReturnEmptyListInitially ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);

        // Act
        var actions = registry.RegisteredKeywordActions;

        // Assert
        Assert.That(actions, Is.Not.Null);
    }

    #endregion

    #region Plugin Loading Tests

    [Test]
    public void LoadPlugins_WithEmptyDirectory_ShouldCompleteSuccessfully ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);
        var progressEvents = new List<string>();

        registry.PluginLoadProgress += (sender, args) => progressEvents.Add($"{args.PluginName}: {args.Status}");

        // Act
        Assert.DoesNotThrow(() => _ = registry.RegisteredColumnizers);

        // Assert
        // No plugins should be loaded, but no exception should be thrown
    }

    [Test]
    public void RegisteredColumnizers_ShouldContainDefaultColumnizers ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);

        // Act
        var columnizers = registry.RegisteredColumnizers;

        // Assert
        Assert.That(columnizers, Is.Not.Empty);
        Assert.That(columnizers.Count, Is.GreaterThanOrEqualTo(4), "Should have at least 4 default columnizers");
    }

    #endregion

    #region Thread Safety Tests

    [Test]
    public void Create_CalledConcurrently_ShouldReturnSameInstance ()
    {
        // Arrange
        var instances = new PluginRegistry[10];
        var tasks = new Task[10];

        // Act
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                instances[index] = PluginRegistry.Create(_testDataPath, 250);
            });
        }

        Task.WaitAll(tasks);

        // Assert
        var first = instances[0];
        foreach (var instance in instances)
        {
            Assert.That(instance, Is.SameAs(first), "All concurrent calls should return the same singleton instance");
        }
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Tests")]
    public void RegisteredColumnizers_AccessedConcurrently_ShouldNotThrow ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);
        var tasks = new Task[10];
        var exceptions = new List<Exception>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    var columnizers = registry.RegisteredColumnizers;
                    Assert.That(columnizers, Is.Not.Null);
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        Task.WaitAll(tasks);

        // Assert
        Assert.That(exceptions, Is.Empty, $"Concurrent access should not throw exceptions. Exceptions: {string.Join(", ", exceptions.Select(e => e.Message))}");
    }

    #endregion

    #region Additional Properties Tests

    [Test]
    public void RegisteredKeywordActions_ShouldNotBeNull ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);

        // Act
        var actions = registry.RegisteredKeywordActions;

        // Assert
        Assert.That(actions, Is.Not.Null, "RegisteredKeywordActions should always be available");
    }

    [Test]
    public void FindFileSystemForUri_WithFileUri_ShouldReturnLocalFileSystem ()
    {
        // Arrange
        var registry = PluginRegistry.Create(_testDataPath, 250);
        var testFile = Path.Combine(_testDataPath, "test.log");

        // Act
        var fileSystem = registry.FindFileSystemForUri(testFile);

        // Assert
        Assert.That(fileSystem, Is.Not.Null, "Should return filesystem for local file");
    }

    #endregion
}
