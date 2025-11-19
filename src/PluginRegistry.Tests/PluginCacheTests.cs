using LogExpert.PluginRegistry.Interfaces;

using Moq;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

/// <summary>
/// Tests for PluginCache functionality.
/// Phase 1: Cache behavior, expiration, and statistics tests
/// </summary>
[TestFixture]
public class PluginCacheTests
{
    private string _testDataPath = null!;
    private string _testPluginsPath = null!;
    private Mock<IPluginLoader> _mockLoader = null!;

    [SetUp]
    public void SetUp ()
    {
        // Create test directories
        _testDataPath = Path.Combine(Path.GetTempPath(), "LogExpertCacheTests", Guid.NewGuid().ToString());
        _testPluginsPath = Path.Combine(_testDataPath, "plugins");
        _ = Directory.CreateDirectory(_testPluginsPath);

        // Create mock loader
        _mockLoader = new Mock<IPluginLoader>();
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Test")]
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

    #region Constructor Tests

    [Test]
    public void Constructor_WithDefaultExpiration_ShouldCreate ()
    {
        // Act
        var cache = new PluginCache();

        // Assert
        Assert.That(cache, Is.Not.Null);
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithCustomExpiration_ShouldCreate ()
    {
        // Arrange
        var expiration = TimeSpan.FromMinutes(30);

        // Act
        var cache = new PluginCache(expiration, _mockLoader.Object);

        // Assert
        Assert.That(cache, Is.Not.Null);
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithCustomLoader_ShouldUseLoader ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act
        var result = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(result.Success, Is.True);
        _mockLoader.Verify(l => l.LoadPlugin(pluginPath), Times.Once);
    }

    #endregion

    #region Load With Cache Tests

    [Test]
    public void LoadPluginWithCache_WithNonExistentFile_ShouldReturnFailure ()
    {
        // Arrange
        var cache = new PluginCache();
        var nonExistentPath = Path.Combine(_testPluginsPath, "NonExistent.dll");

        // Act
        var result = cache.LoadPluginWithCache(nonExistentPath);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("not found"));
    }

    [Test]
    public void LoadPluginWithCache_WithNullPath_ShouldThrowArgumentNullException ()
    {
        // Arrange
        var cache = new PluginCache();

        // Act & Assert
        Assert.That(() => cache.LoadPluginWithCache(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void LoadPluginWithCache_FirstLoad_ShouldLoadFromDisk ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act
        var result = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Plugin, Is.Not.Null);
        _mockLoader.Verify(l => l.LoadPlugin(pluginPath), Times.Once);
        Assert.That(cache.CacheSize, Is.EqualTo(1));
    }

    [Test]
    public void LoadPluginWithCache_SecondLoad_ShouldLoadFromCache ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act - Load twice
        var result1 = cache.LoadPluginWithCache(pluginPath);
        var result2 = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(result1.Success, Is.True);
        Assert.That(result2.Success, Is.True);
        Assert.That(result1.Plugin, Is.SameAs(result2.Plugin), "Should return same cached instance");
        _mockLoader.Verify(l => l.LoadPlugin(pluginPath), Times.Once, "Should only load from disk once");
        Assert.That(cache.CacheSize, Is.EqualTo(1));
    }

    [Test]
    public void LoadPluginWithCache_ModifiedFile_ShouldLoadNewVersion ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll", "Original Content");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act - Load, modify file, load again
        var result1 = cache.LoadPluginWithCache(pluginPath);

        // Modify file (changes hash)
        File.WriteAllText(pluginPath, "Modified Content");
        SetupSuccessfulLoad(pluginPath, "TestPlugin_Modified");

        var result2 = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(result1.Success, Is.True);
        Assert.That(result2.Success, Is.True);
        _mockLoader.Verify(l => l.LoadPlugin(pluginPath), Times.Exactly(2), "Should load twice due to file change");
        Assert.That(cache.CacheSize, Is.EqualTo(2), "Both versions should be in cache");
    }

    [Test]
    public void LoadPluginWithCache_LoaderFailure_ShouldReturnFailure ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("FailingPlugin.dll");

        _ = _mockLoader.Setup(l => l.LoadPlugin(pluginPath))
            .Returns(new PluginLoadResult
            {
                Success = false,
                ErrorMessage = "Failed to load plugin"
            });

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act
        var result = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Failed to load"));
        Assert.That(cache.CacheSize, Is.EqualTo(0), "Failed loads should not be cached");
    }

    #endregion

    #region Cache Expiration Tests

    [Test]
    public void LoadPluginWithCache_AfterExpiration_ShouldReload ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        // Very short expiration for testing
        var cache = new PluginCache(TimeSpan.FromMilliseconds(100), _mockLoader.Object);

        // Act - Load, wait for expiration, load again
        var result1 = cache.LoadPluginWithCache(pluginPath);

        Thread.Sleep(150); // Wait for cache to expire

        var result2 = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(result1.Success, Is.True);
        Assert.That(result2.Success, Is.True);
        _mockLoader.Verify(l => l.LoadPlugin(pluginPath), Times.Exactly(2), "Should reload after expiration");
    }

    [Test]
    public void RemoveExpiredEntries_WithExpiredEntries_ShouldRemoveThem ()
    {
        // Arrange
        var pluginPath1 = CreateDummyPlugin("Plugin1.dll");
        var pluginPath2 = CreateDummyPlugin("Plugin2.dll");

        SetupSuccessfulLoad(pluginPath1, "Plugin1");
        SetupSuccessfulLoad(pluginPath2, "Plugin2");

        // Very short expiration
        var cache = new PluginCache(TimeSpan.FromMilliseconds(100), _mockLoader.Object);

        // Load both plugins
        _ = cache.LoadPluginWithCache(pluginPath1);
        _ = cache.LoadPluginWithCache(pluginPath2);

        Assert.That(cache.CacheSize, Is.EqualTo(2));

        // Wait for expiration
        Thread.Sleep(150);

        // Act
        var removedCount = cache.RemoveExpiredEntries();

        // Assert
        Assert.That(removedCount, Is.EqualTo(2));
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void RemoveExpiredEntries_WithNoExpiredEntries_ShouldRemoveNone ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);
        _ = cache.LoadPluginWithCache(pluginPath);

        // Act
        var removedCount = cache.RemoveExpiredEntries();

        // Assert
        Assert.That(removedCount, Is.EqualTo(0));
        Assert.That(cache.CacheSize, Is.EqualTo(1));
    }

    #endregion

    #region IsCached Tests

    [Test]
    public void IsCached_WithCachedPlugin_ReturnsTrue ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);
        _ = cache.LoadPluginWithCache(pluginPath);

        // Act
        var isCached = cache.IsCached(pluginPath);

        // Assert
        Assert.That(isCached, Is.True);
    }

    [Test]
    public void IsCached_WithNonCachedPlugin_ReturnsFalse ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act
        var isCached = cache.IsCached(pluginPath);

        // Assert
        Assert.That(isCached, Is.False);
    }

    [Test]
    public void IsCached_WithExpiredPlugin_ReturnsFalse ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromMilliseconds(100), _mockLoader.Object);
        _ = cache.LoadPluginWithCache(pluginPath);

        // Wait for expiration
        Thread.Sleep(150);

        // Act
        var isCached = cache.IsCached(pluginPath);

        // Assert
        Assert.That(isCached, Is.False);
    }

    [Test]
    public void IsCached_WithNonExistentFile_ReturnsFalse ()
    {
        // Arrange
        var cache = new PluginCache();
        var nonExistentPath = Path.Combine(_testPluginsPath, "NonExistent.dll");

        // Act
        var isCached = cache.IsCached(nonExistentPath);

        // Assert
        Assert.That(isCached, Is.False);
    }

    #endregion

    #region Clear Cache Tests

    [Test]
    public void ClearCache_WithMultipleEntries_ShouldRemoveAll ()
    {
        // Arrange
        var pluginPath1 = CreateDummyPlugin("Plugin1.dll");
        var pluginPath2 = CreateDummyPlugin("Plugin2.dll");
        var pluginPath3 = CreateDummyPlugin("Plugin3.dll");

        SetupSuccessfulLoad(pluginPath1, "Plugin1");
        SetupSuccessfulLoad(pluginPath2, "Plugin2");
        SetupSuccessfulLoad(pluginPath3, "Plugin3");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        _ = cache.LoadPluginWithCache(pluginPath1);
        _ = cache.LoadPluginWithCache(pluginPath2);
        _ = cache.LoadPluginWithCache(pluginPath3);

        Assert.That(cache.CacheSize, Is.EqualTo(3));

        // Act
        cache.ClearCache();

        // Assert
        Assert.That(cache.CacheSize, Is.EqualTo(0));
        Assert.That(cache.IsCached(pluginPath1), Is.False);
        Assert.That(cache.IsCached(pluginPath2), Is.False);
        Assert.That(cache.IsCached(pluginPath3), Is.False);
    }

    [Test]
    public void ClearCache_WithEmptyCache_ShouldNotThrow ()
    {
        // Arrange
        var cache = new PluginCache();

        // Act & Assert
        Assert.That(cache.ClearCache, Throws.Nothing);
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    #endregion

    #region Statistics Tests

    [Test]
    public void GetStatistics_WithEmptyCache_ShouldReturnEmptyStats ()
    {
        // Arrange
        var cache = new PluginCache();

        // Act
        var stats = cache.GetStatistics();

        // Assert
        Assert.That(stats.TotalEntries, Is.EqualTo(0));
        Assert.That(stats.ExpiredEntries, Is.EqualTo(0));
        Assert.That(stats.ActiveEntries, Is.EqualTo(0));
        Assert.That(stats.OldestEntry, Is.Null);
        Assert.That(stats.NewestEntry, Is.Null);
    }

    [Test]
    public void GetStatistics_WithMultipleEntries_ShouldReturnCorrectStats ()
    {
        // Arrange
        var pluginPath1 = CreateDummyPlugin("Plugin1.dll");
        var pluginPath2 = CreateDummyPlugin("Plugin2.dll");

        SetupSuccessfulLoad(pluginPath1, "Plugin1");
        SetupSuccessfulLoad(pluginPath2, "Plugin2");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        _ = cache.LoadPluginWithCache(pluginPath1);
        Thread.Sleep(50); // Small delay to ensure different timestamps
        _ = cache.LoadPluginWithCache(pluginPath2);

        // Act
        var stats = cache.GetStatistics();

        // Assert
        Assert.That(stats.TotalEntries, Is.EqualTo(2));
        Assert.That(stats.ExpiredEntries, Is.EqualTo(0));
        Assert.That(stats.ActiveEntries, Is.EqualTo(2));
        Assert.That(stats.OldestEntry, Is.Not.Null);
        Assert.That(stats.NewestEntry, Is.Not.Null);
        Assert.That(stats.NewestEntry, Is.GreaterThanOrEqualTo(stats.OldestEntry));
    }

    [Test]
    public void GetStatistics_WithExpiredEntries_ShouldCountCorrectly ()
    {
        // Arrange
        var pluginPath1 = CreateDummyPlugin("Plugin1.dll");
        var pluginPath2 = CreateDummyPlugin("Plugin2.dll");

        SetupSuccessfulLoad(pluginPath1, "Plugin1");
        SetupSuccessfulLoad(pluginPath2, "Plugin2");

        // Very short expiration
        var cache = new PluginCache(TimeSpan.FromMilliseconds(100), _mockLoader.Object);

        _ = cache.LoadPluginWithCache(pluginPath1);

        Thread.Sleep(150); // Wait for first to expire

        _ = cache.LoadPluginWithCache(pluginPath2); // Load second after first expires

        // Act
        var stats = cache.GetStatistics();

        // Assert
        Assert.That(stats.TotalEntries, Is.EqualTo(2));
        Assert.That(stats.ExpiredEntries, Is.EqualTo(1), "First plugin should be expired");
        Assert.That(stats.ActiveEntries, Is.EqualTo(1), "Only second plugin should be active");
    }

    #endregion

    #region CacheSize Tests

    [Test]
    public void CacheSize_InitiallyZero ()
    {
        // Arrange & Act
        var cache = new PluginCache();

        // Assert
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void CacheSize_IncreasesWithLoads ()
    {
        // Arrange
        var pluginPath1 = CreateDummyPlugin("Plugin1.dll");
        var pluginPath2 = CreateDummyPlugin("Plugin2.dll");

        SetupSuccessfulLoad(pluginPath1, "Plugin1");
        SetupSuccessfulLoad(pluginPath2, "Plugin2");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act & Assert
        Assert.That(cache.CacheSize, Is.EqualTo(0));

        _ = cache.LoadPluginWithCache(pluginPath1);
        Assert.That(cache.CacheSize, Is.EqualTo(1));

        _ = cache.LoadPluginWithCache(pluginPath2);
        Assert.That(cache.CacheSize, Is.EqualTo(2));
    }

    [Test]
    public void CacheSize_DoesNotIncreaseForDuplicateLoads ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act
        _ = cache.LoadPluginWithCache(pluginPath);
        _ = cache.LoadPluginWithCache(pluginPath);
        _ = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(cache.CacheSize, Is.EqualTo(1), "Multiple loads of same plugin should not increase cache size");
    }

    #endregion

    #region Async Tests

    [Test]
    public async Task LoadPluginWithCacheAsync_ShouldLoadSuccessfully ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        // Act
        var result = await cache.LoadPluginWithCacheAsync(pluginPath).ConfigureAwait(false);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Plugin, Is.Not.Null);
        _mockLoader.Verify(l => l.LoadPlugin(pluginPath), Times.Once);
    }

    [Test]
    public async Task LoadPluginWithCacheAsync_WithCancellation_ShouldCancel ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);

        using var cts = new CancellationTokenSource();
        {
            await cts.CancelAsync().ConfigureAwait(false); // Cancel immediately

            // Act & Assert
            _ = Assert.ThrowsAsync<TaskCanceledException>(async () => await cache.LoadPluginWithCacheAsync(pluginPath, cts.Token).ConfigureAwait(false));
        }
    }

    #endregion

    #region Concurrent Access Tests

    [Test]
    public void LoadPluginWithCache_ConcurrentAccess_ShouldBeSafe ()
    {
        // Arrange
        var pluginPath = CreateDummyPlugin("TestPlugin.dll");
        SetupSuccessfulLoad(pluginPath, "TestPlugin");

        var cache = new PluginCache(TimeSpan.FromHours(1), _mockLoader.Object);
        var tasks = new Task<PluginLoadResult>[10];
        var results = new List<PluginLoadResult>();

        // Act - Load same plugin concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() => cache.LoadPluginWithCache(pluginPath));
        }

        Task.WaitAll(tasks);
        results.AddRange(tasks.Select(t => t.Result));

        // Assert
        Assert.That(results.All(r => r.Success), Is.True, "All concurrent loads should succeed");
        Assert.That(cache.CacheSize, Is.GreaterThanOrEqualTo(1), "Plugin should be cached");

        // All successful results should reference the same cached instance
        var successfulPlugins = results.Where(r => r.Plugin != null).Select(r => r.Plugin).ToList();
        Assert.That(successfulPlugins.Distinct().Count(), Is.EqualTo(1),
            "All concurrent loads should get the same cached instance");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a dummy plugin DLL file for testing.
    /// </summary>
    private string CreateDummyPlugin (string fileName, string content = "Dummy Plugin Content")
    {
        var path = Path.Combine(_testPluginsPath, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Sets up the mock loader to return a successful load result.
    /// </summary>
    private void SetupSuccessfulLoad (string pluginPath, string pluginName)
    {
        _ = _mockLoader.Setup(l => l.LoadPlugin(pluginPath))
            .Returns(new PluginLoadResult
            {
                Success = true,
                Plugin = new object(), // Simple object as plugin
                Manifest = new PluginManifest
                {
                    Name = pluginName,
                    Version = "1.0.0",
                    Author = "Test",
                    Description = "Test plugin",
                    ApiVersion = "1.0",
                    Main = pluginPath
                }
            });
    }

    #endregion
}
