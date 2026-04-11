using ColumnizerLib;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

[TestFixture]
public class PerformanceTests
{
    private string _testPluginDirectory;

    [SetUp]
    public void Setup ()
    {
        _testPluginDirectory = Path.Join(Path.GetTempPath(), "LogExpertTestPlugins");
        _ = Directory.CreateDirectory(_testPluginDirectory);
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Tests")]
    public void Teardown ()
    {
        if (Directory.Exists(_testPluginDirectory))
        {
            try
            {
                Directory.Delete(_testPluginDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region LazyPluginProxy Tests

    [Test]
    public void LazyPluginProxy_CreatesWithoutLoading ()
    {
        // Arrange
        var pluginPath = "test.dll";
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test",
            Main = "test.dll",
            ApiVersion = "1.0"
        };

        // Act
        var proxy = new LazyPluginProxy<ILogLineMemoryColumnizer>(pluginPath, manifest);

        // Assert
        Assert.That(proxy.IsLoaded, Is.False, "Plugin should not be loaded on proxy creation");
        Assert.That(proxy.PluginName, Is.EqualTo("TestPlugin"));
        Assert.That(proxy.AssemblyPath, Is.EqualTo(pluginPath));
        Assert.That(proxy.Manifest, Is.Not.Null);
    }

    [Test]
    public void LazyPluginProxy_LoadsOnFirstAccess ()
    {
        // Note: This test would need a real plugin DLL to work properly
        // For now, we test that accessing Instance doesn't crash

        // Arrange
        var pluginPath = "nonexistent.dll";
        var proxy = new LazyPluginProxy<ILogLineMemoryColumnizer>(pluginPath, null);

        // Act & Assert
        Assert.That(proxy.IsLoaded, Is.False);

        // Accessing Instance will attempt to load (will fail for nonexistent file)
        var instance = proxy.Instance;

        Assert.That(proxy.IsLoaded, Is.True, "Plugin should be marked as loaded after access attempt");
        Assert.That(instance, Is.Null, "Instance should be null for nonexistent file");
    }

    [Test]
    public void LazyPluginProxy_ToString_ReportsLoadState ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test",
            Description = "Test",
            Main = "test.dll",
            ApiVersion = "1.0"
        };
        var proxy = new LazyPluginProxy<ILogLineMemoryColumnizer>("test.dll", manifest);

        // Act
        var beforeLoad = proxy.ToString();
        _ = proxy.Instance; // Attempt to load
        var afterLoad = proxy.ToString();

        // Assert
        Assert.That(beforeLoad, Does.Contain("Not Loaded"));
        Assert.That(afterLoad, Does.Contain("Loaded"));
    }

    [Test]
    public void LazyPluginProxy_TryPreload_ReturnsFalseForInvalidPlugin ()
    {
        // Arrange
        var proxy = new LazyPluginProxy<ILogLineMemoryColumnizer>("nonexistent.dll", null);

        // Act
        var result = proxy.TryPreload();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(proxy.IsLoaded, Is.True);
    }

    #endregion

    #region PluginCache Tests

    [Test]
    public void PluginCache_Initializes_WithDefaultExpiration ()
    {
        // Arrange & Act
        var cache = new PluginCache();

        // Assert
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void PluginCache_Initializes_WithCustomExpiration ()
    {
        // Arrange & Act
        var cache = new PluginCache(TimeSpan.FromMinutes(30));

        // Assert
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void PluginCache_LoadPluginWithCache_ReturnsErrorForNonexistentFile ()
    {
        // Arrange
        var cache = new PluginCache();
        var pluginPath = "nonexistent.dll";

        // Act
        var result = cache.LoadPluginWithCache(pluginPath);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("not found"));
    }

    [Test]
    public void PluginCache_ClearCache_RemovesAllEntries ()
    {
        // Arrange
        var cache = new PluginCache();

        // Act
        cache.ClearCache();

        // Assert
        Assert.That(cache.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void PluginCache_IsCached_ReturnsFalseForNonexistentFile ()
    {
        // Arrange
        var cache = new PluginCache();

        // Act
        var result = cache.IsCached("nonexistent.dll");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void PluginCache_GetStatistics_ReturnsEmptyStats ()
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
    public void PluginCache_RemoveExpiredEntries_ReturnsZeroForEmptyCache ()
    {
        // Arrange
        var cache = new PluginCache();

        // Act
        var removed = cache.RemoveExpiredEntries();

        // Assert
        Assert.That(removed, Is.EqualTo(0));
    }

    [Test]
    public async Task PluginCache_LoadPluginWithCacheAsync_ReturnsErrorForNonexistentFile ()
    {
        // Arrange
        var cache = new PluginCache();
        var pluginPath = "nonexistent.dll";

        // Act
        var result = await cache.LoadPluginWithCacheAsync(pluginPath).ConfigureAwait(false);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("not found"));
    }

    [Test]
    public void CacheStatistics_ActiveEntries_CalculatesCorrectly ()
    {
        // Arrange
        var stats = new CacheStatistics
        {
            TotalEntries = 10,
            ExpiredEntries = 3
        };

        // Act
        var activeEntries = stats.ActiveEntries;

        // Assert
        Assert.That(activeEntries, Is.EqualTo(7));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void LazyPluginProxy_ThrowsArgumentNullException_ForNullPath ()
    {
        // Arrange, Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => new LazyPluginProxy<ILogLineMemoryColumnizer>(null, null));
    }

    [Test]
    public void PluginCache_LoadPluginWithCache_ThrowsArgumentNullException_ForNullPath ()
    {
        // Arrange
        var cache = new PluginCache();

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => cache.LoadPluginWithCache(null));
    }

    #endregion
}
