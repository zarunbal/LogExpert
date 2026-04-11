namespace LogExpert.PluginRegistry;

/// <summary>
/// Statistics about the plugin cache.
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// Total number of entries in cache.
    /// </summary>
    public int TotalEntries { get; init; }

    /// <summary>
    /// Number of expired entries (still in cache but past expiration).
    /// </summary>
    public int ExpiredEntries { get; init; }

    /// <summary>
    /// Load time of oldest cached plugin.
    /// </summary>
    public DateTime? OldestEntry { get; init; }

    /// <summary>
    /// Load time of newest cached plugin.
    /// </summary>
    public DateTime? NewestEntry { get; init; }

    /// <summary>
    /// Number of active (non-expired) entries.
    /// </summary>
    public int ActiveEntries => TotalEntries - ExpiredEntries;
}