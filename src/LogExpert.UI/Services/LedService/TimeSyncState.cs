namespace LogExpert.UI.Services.LedService;

/// <summary>
/// Represents the time synchronization state for a log window
/// </summary>
public enum TimeSyncState
{
    /// <summary>
    /// Time synchronization is not active (normal mode)
    /// </summary>
    NotSynced = 0,
    
    /// <summary>
    /// Time synchronization is active (synced with other windows)
    /// </summary>
    Synced = 1
}
