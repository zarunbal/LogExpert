using Newtonsoft.Json;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Configuration for trusted plugins with hash-based verification.
/// </summary>
public class TrustedPluginConfig
{
    /// <summary>
    /// List of plugin file names that are trusted.
    /// </summary>
    [JsonProperty("pluginNames")]
    public List<string> PluginNames { get; set; } = new();
    
    /// <summary>
    /// Dictionary mapping plugin file names to their expected SHA256 hashes.
    /// Used for integrity verification.
    /// </summary>
    [JsonProperty("pluginHashes")]
    public Dictionary<string, string> PluginHashes { get; set; } = new();
    
    /// <summary>
    /// Whether to allow user-added trusted plugins.
    /// If false, only shipped plugins can be trusted.
    /// </summary>
    [JsonProperty("allowUserTrustedPlugins")]
    public bool AllowUserTrustedPlugins { get; set; } = true;
    
    /// <summary>
    /// Hash algorithm to use for verification (e.g., "SHA256").
    /// </summary>
    [JsonProperty("hashAlgorithm")]
    public string HashAlgorithm { get; set; } = "SHA256";
    
    /// <summary>
    /// Timestamp of last configuration update.
    /// </summary>
    [JsonProperty("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
