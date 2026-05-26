namespace LogExpert.PluginRegistry;

/// <summary>
/// Provides data for plugin load progress events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PluginLoadProgressEventArgs"/> class.
/// </remarks>
/// <param name="pluginPath">The path to the plugin being processed.</param>
/// <param name="pluginName">The name of the plugin being processed.</param>
/// <param name="currentIndex">The index of the current plugin being processed (0-based).</param>
/// <param name="totalPlugins">The total number of plugins to be processed.</param>
/// <param name="status">The current status of the plugin load operation.</param>
/// <param name="message">An optional message providing additional details.</param>
public class PluginLoadProgressEventArgs (
    string pluginPath,
    string pluginName,
    int currentIndex,
    int totalPlugins,
    PluginLoadStatus status,
    string? message = null) : EventArgs
{

    /// <summary>
    /// Gets the full path to the plugin being processed.
    /// </summary>
    public string PluginPath { get; } = pluginPath;

    /// <summary>
    /// Gets the name of the plugin being processed.
    /// </summary>
    public string PluginName { get; } = pluginName;

    /// <summary>
    /// Gets the index of the current plugin being processed (0-based).
    /// </summary>
    public int CurrentIndex { get; } = currentIndex;

    /// <summary>
    /// Gets the total number of plugins to be processed.
    /// </summary>
    public int TotalPlugins { get; } = totalPlugins;

    /// <summary>
    /// Gets the current status of the plugin load operation.
    /// </summary>
    public PluginLoadStatus Status { get; } = status;

    /// <summary>
    /// Gets an optional message providing additional details about the operation.
    /// </summary>
    public string? Message { get; } = message;

    /// <summary>
    /// Gets the timestamp when this event was created.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the percentage of completion (0-100).
    /// </summary>
    public double PercentComplete => TotalPlugins > 0 ? ((double)(CurrentIndex + 1) / TotalPlugins) * 100 : 0;

    /// <summary>
    /// Returns a string representation of the progress event.
    /// </summary>
    public override string ToString ()
    {
        return $"[{CurrentIndex + 1}/{TotalPlugins}] {Status}: {PluginName} - {Message ?? "(no details)"}";
    }
}