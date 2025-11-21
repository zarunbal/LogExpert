using System.Security.Cryptography;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Provides hash calculation functionality for plugin DLL files.
/// Used for integrity verification and tamper detection.
/// </summary>
public static class PluginHashCalculator
{
    /// <summary>
    /// Calculates the SHA256 hash of a plugin DLL file.
    /// </summary>
    /// <param name="filePath">Full path to the plugin DLL file.</param>
    /// <returns>Uppercase hexadecimal string representation of the SHA256 hash (no hyphens).</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be read.</exception>
    public static string CalculateHash (string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Plugin file not found: {filePath}", filePath);
        }

        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);

            return Convert.ToHexString(hashBytes);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Access denied reading plugin file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Verifies that a plugin file matches an expected hash.
    /// </summary>
    /// <param name="filePath">Full path to the plugin DLL file.</param>
    /// <param name="expectedHash">Expected SHA256 hash (case-insensitive).</param>
    /// <returns>True if the file's hash matches the expected hash, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath or expectedHash is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static bool VerifyHash (string filePath, string expectedHash)
    {
        // In Debug mode we trust the plugins (so testing and developing is possible)
#if DEBUG
        return true;
#else
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedHash);

        var actualHash = CalculateHash(filePath);

        // Case-insensitive comparison
        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
#endif
    }

    /// <summary>
    /// Calculates hashes for multiple plugin files.
    /// </summary>
    /// <param name="filePaths">Collection of file paths to process.</param>
    /// <returns>Dictionary mapping file paths to their SHA256 hashes.</returns>
    /// <remarks>Files that cannot be processed are omitted from the result (logged but not thrown).</remarks>
    public static Dictionary<string, string> CalculateHashes (IEnumerable<string> filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in filePaths)
        {
            try
            {
                var hash = CalculateHash(filePath);
                results[filePath] = hash;
            }
            catch (Exception ex) when (ex is ArgumentNullException or
                                             ArgumentException or
                                             FileNotFoundException or
                                             IOException)
            {
                // Skip files that cannot be processed
                // Caller should check for missing entries if needed
                continue;
            }
        }

        return results;
    }
}
