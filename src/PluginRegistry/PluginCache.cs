using System.Collections.Concurrent;

using LogExpert.PluginRegistry.Interfaces;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Caches loaded plugins to improve performance on subsequent loads.
/// Implements a time-based expiration policy to balance performance and memory usage.
/// </summary>
public class PluginCache
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<string, CachedPlugin> _cache = new();
    private readonly TimeSpan _cacheExpiration;
    private readonly IPluginLoader _loader;

    /// <summary>
    /// Creates a new plugin cache.
    /// </summary>
    /// <param name="cacheExpiration">How long to keep cached plugins (default: 24 hours)</param>
    /// <param name="loader">Plugin loader to use for cache misses</param>
    public PluginCache (TimeSpan? cacheExpiration = null, IPluginLoader? loader = null)
    {
        _cacheExpiration = cacheExpiration ?? TimeSpan.FromHours(24);
        _loader = loader ?? new DefaultPluginLoader();

        _logger.Info("Plugin cache initialized with expiration: {Expiration}", _cacheExpiration);
    }

    /// <summary>
    /// Loads a plugin using the cache if available.
    /// On cache miss, loads the plugin and adds it to cache.
    /// </summary>
    /// <param name="pluginPath">Path to the plugin assembly</param>
    /// <returns>Plugin load result</returns>
    public PluginLoadResult LoadPluginWithCache (string pluginPath)
    {
        ArgumentNullException.ThrowIfNull(pluginPath);

        if (!File.Exists(pluginPath))
        {
            _logger.Error("Plugin file not found: {Path}", pluginPath);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = $"Plugin file not found: {pluginPath}"
            };
        }

        try
        {
            // Calculate hash for cache key
            var hash = PluginHashCalculator.CalculateHash(pluginPath);
            var cacheKey = $"{Path.GetFileName(pluginPath)}_{hash}";

            // Try to get from cache
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                if (IsCacheValid(cached))
                {
                    _logger.Debug("Loading plugin from cache: {CacheKey}", cacheKey);

                    // Update last access time
                    cached.LastAccess = DateTime.UtcNow;

                    return new PluginLoadResult
                    {
                        Success = true,
                        Plugin = cached.Plugin,
                        Manifest = cached.Manifest
                    };
                }
                else
                {
                    _logger.Debug("Cache entry expired for: {CacheKey}", cacheKey);
                    _ = _cache.TryRemove(cacheKey, out _);
                }
            }

            // Cache miss - load plugin
            _logger.Debug("Cache miss for: {Plugin}, loading from disk", Path.GetFileName(pluginPath));
            var result = _loader.LoadPlugin(pluginPath);

            if (result.Success && result.Plugin != null)
            {
                // Add to cache
                _cache[cacheKey] = new CachedPlugin
                {
                    Plugin = result.Plugin,
                    Manifest = result.Manifest,
                    LoadTime = DateTime.UtcNow,
                    LastAccess = DateTime.UtcNow,
                    FileHash = hash,
                    FilePath = pluginPath
                };

                _logger.Info("Cached plugin: {CacheKey}", cacheKey);
            }

            return result;
        }
        catch (Exception ex) when (ex is ArgumentNullException or
                                         ArgumentException or
                                         FileNotFoundException or
                                         IOException)
        {
            _logger.Error(ex, "Error loading plugin with cache: {Path}", pluginPath);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = $"Error loading plugin: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Loads a plugin asynchronously using the cache if available.
    /// </summary>
    /// <param name="pluginPath">Path to the plugin assembly</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Plugin load result</returns>
    public async Task<PluginLoadResult> LoadPluginWithCacheAsync (string pluginPath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => LoadPluginWithCache(pluginPath), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a cached plugin is still valid based on expiration time.
    /// </summary>
    private bool IsCacheValid (CachedPlugin cached)
    {
        var age = DateTime.UtcNow - cached.LoadTime;
        return age < _cacheExpiration;
    }

    /// <summary>
    /// Clears all cached plugins.
    /// </summary>
    public void ClearCache ()
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.Info("Plugin cache cleared ({Count} entries removed)", count);
    }

    /// <summary>
    /// Removes expired entries from the cache.
    /// </summary>
    /// <returns>Number of entries removed</returns>
    public int RemoveExpiredEntries ()
    {
        var removedCount = 0;
        var keysToRemove = new List<string>();

        foreach (var kvp in _cache)
        {
            if (!IsCacheValid(kvp.Value))
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (_cache.TryRemove(key, out _))
            {
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            _logger.Info("Removed {Count} expired cache entries", removedCount);
        }

        return removedCount;
    }

    /// <summary>
    /// Checks if a plugin is in the cache.
    /// </summary>
    /// <param name="pluginPath">Path to the plugin</param>
    /// <returns>True if plugin is cached and valid</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentionally Ignore Errors")]
    public bool IsCached (string pluginPath)
    {
        if (!File.Exists(pluginPath))
        {
            return false;
        }

        try
        {
            var hash = PluginHashCalculator.CalculateHash(pluginPath);
            var cacheKey = $"{Path.GetFileName(pluginPath)}_{hash}";

            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return IsCacheValid(cached);
            }
        }
        catch
        {
            // Ignore errors
        }

        return false;
    }

    /// <summary>
    /// Gets the number of cached plugins.
    /// </summary>
    public int CacheSize => _cache.Count;

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public CacheStatistics GetStatistics ()
    {
        var stats = new CacheStatistics
        {
            TotalEntries = _cache.Count,
            ExpiredEntries = _cache.Count(kvp => !IsCacheValid(kvp.Value)),
            OldestEntry = _cache.Values.Count != 0 ? _cache.Values.Min(c => c.LoadTime) : null,
            NewestEntry = _cache.Values.Count != 0 ? _cache.Values.Max(c => c.LoadTime) : null
        };

        return stats;
    }

    /// <summary>
    /// Represents a cached plugin entry.
    /// </summary>
    private class CachedPlugin
    {
        public object Plugin { get; init; }
        public PluginManifest? Manifest { get; init; }
        public DateTime LoadTime { get; init; }
        public DateTime LastAccess { get; set; }
        public string FileHash { get; init; }
        public string FilePath { get; init; }
    }
}