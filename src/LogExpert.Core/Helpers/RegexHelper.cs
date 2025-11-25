using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogExpert.Core.Helpers;

/// <summary>
/// Helper class for creating and managing regex instances with safety features.
/// Provides timeout protection against catastrophic backtracking (DoS attacks)
/// and caching for improved performance.
/// </summary>
public static class RegexHelper
{
    /// <summary>
    /// Default timeout for all regex operations to prevent DoS attacks.
    /// This prevents catastrophic backtracking from freezing the application.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    private static readonly ConcurrentDictionary<RegexCacheKey, Regex> _cache = new();
    private const int MAX_CACHE_SIZE = 100;

    /// <summary>
    /// Creates a regex with timeout protection.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="options">Regex options to use.</param>
    /// <param name="timeout">Optional timeout override. Uses DefaultTimeout if not specified.</param>
    /// <returns>A Regex instance with timeout protection.</returns>
    /// <exception cref="ArgumentNullException">Thrown if pattern is null.</exception>
    /// <exception cref="ArgumentException">Thrown if pattern is invalid.</exception>
    public static Regex CreateSafeRegex (string pattern, RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        return new Regex(pattern, options, timeout ?? DefaultTimeout);
    }

    /// <summary>
    /// Gets or creates a cached regex instance.
    /// This improves performance by reusing compiled regex patterns.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="options">Regex options to use.</param>
    /// <returns>A cached Regex instance with timeout protection.</returns>
    public static Regex GetOrCreateCached (string pattern, RegexOptions options = RegexOptions.None)
    {
        var key = new RegexCacheKey(pattern, options);

        return _cache.GetOrAdd(key, k =>
        {
            // Evict oldest entries if cache is full
            if (_cache.Count >= MAX_CACHE_SIZE)
            {
                TrimCache();
            }

            return CreateSafeRegex(k.Pattern, k.Options);
        });
    }

    /// <summary>
    /// Validates a regex pattern without executing it.
    /// </summary>
    /// <param name="pattern">The pattern to validate.</param>
    /// <param name="error">Output parameter containing error message if validation fails.</param>
    /// <returns>True if the pattern is valid, false otherwise.</returns>
    public static bool IsValidPattern (string pattern, out string? error)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            error = "Pattern cannot be null or empty.";
            return false;
        }

        try
        {
            _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
            error = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            // Pattern is valid syntactically, but may be complex
            error = null;
            return true;
        }
    }

    /// <summary>
    /// Clears the regex cache. Useful for testing or memory management.
    /// </summary>
    public static void ClearCache ()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets the current cache size.
    /// </summary>
    public static int CacheSize => _cache.Count;

    private static void TrimCache ()
    {
        // Keep most recent 50 entries (half of max)
        var toRemove = _cache.Keys.Take(_cache.Count - MAX_CACHE_SIZE / 2).ToList();
        foreach (var key in toRemove)
        {
            _ = _cache.TryRemove(key, out _);
        }
    }

    private record RegexCacheKey (string Pattern, RegexOptions Options);
}
