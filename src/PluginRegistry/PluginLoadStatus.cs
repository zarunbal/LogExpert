namespace LogExpert.PluginRegistry;

/// <summary>
/// Represents the status of a plugin load operation.
/// </summary>
public enum PluginLoadStatus
{
    /// <summary>
    /// Plugin loading has started.
    /// </summary>
    Started,

    /// <summary>
    /// Plugin is being validated (security checks, manifest validation, etc.).
    /// </summary>
    Validating,

    /// <summary>
    /// Plugin validation completed successfully.
    /// </summary>
    Validated,

    /// <summary>
    /// Plugin is being loaded into memory.
    /// </summary>
    Loading,

    /// <summary>
    /// Plugin was loaded successfully.
    /// </summary>
    Loaded,

    /// <summary>
    /// Plugin was skipped (not a plugin, dependency, or failed validation).
    /// </summary>
    Skipped,

    /// <summary>
    /// Plugin load failed with an error.
    /// </summary>
    Failed,

    /// <summary>
    /// All plugins have finished loading (summary event).
    /// </summary>
    Completed
}