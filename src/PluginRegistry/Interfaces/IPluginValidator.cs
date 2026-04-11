namespace LogExpert.PluginRegistry.Interfaces;

/// <summary>
/// Responsible for validating plugins before loading.
/// </summary>
public interface IPluginValidator
{
    /// <summary>
    /// Validates a plugin at the specified path.
    /// </summary>
    /// <param name="pluginPath">Path to the plugin file</param>
    /// <param name="manifest">Optional manifest for additional validation</param>
    /// <returns>Validation result with errors and warnings</returns>
    ValidationResult ValidatePlugin(string pluginPath, PluginManifest? manifest);
}

/// <summary>
/// Result of plugin validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates whether the plugin passed validation.
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// List of validation errors that prevent plugin loading.
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// List of validation warnings (non-critical issues).
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// User-friendly error message suitable for display.
    /// </summary>
    public string? UserFriendlyError { get; set; }
}
