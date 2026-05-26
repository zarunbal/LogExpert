using LogExpert.PluginRegistry.Interfaces;

namespace LogExpert.PluginRegistry.Events;

/// <summary>
/// Event raised when a log file is loaded.
/// </summary>
public class LogFileLoadedEvent : IPluginEvent
{
    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Source of the event (typically "LogExpert").
    /// </summary>
    public string Source { get; init; }
    
    /// <summary>
    /// Full path to the loaded file.
    /// </summary>
    public string FileName { get; init; }
    
    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    public long FileSize { get; init; }
    
    /// <summary>
    /// Number of lines in the file (if known).
    /// </summary>
    public int? LineCount { get; init; }
}

/// <summary>
/// Event raised when a log file is closed.
/// </summary>
public class LogFileClosedEvent : IPluginEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Source { get; init; }
    public string FileName { get; init; }
}

/// <summary>
/// Event raised when a plugin is loaded.
/// </summary>
public class PluginLoadedEvent : IPluginEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Source { get; init; } = "LogExpert";
    public string PluginName { get; init; }
    public string PluginVersion { get; init; }
}

/// <summary>
/// Event raised when a plugin is unloaded.
/// </summary>
public class PluginUnloadedEvent : IPluginEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Source { get; init; } = "LogExpert";
    public string PluginName { get; init; }
}

/// <summary>
/// Event raised when application settings change.
/// </summary>
public class SettingsChangedEvent : IPluginEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Source { get; init; } = "LogExpert";
    public string SettingName { get; init; }
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
