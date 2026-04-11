namespace LogExpert.UI.Services.LedService;

/// <summary>
/// Represents the tail follow state for a log window
/// </summary>
public enum TailFollowState
{
    /// <summary>
    /// Tail following is active
    /// </summary>
    On = 0,

    /// <summary>
    /// Tail following is disabled
    /// </summary>
    Off = 1,

    /// <summary>
    /// Tail following is paused (e.g., by trigger)
    /// </summary>
    Paused = 2,

    /// <summary>
    /// Tail state indicator is hidden (not shown in icon)
    /// </summary>
    Hidden = 3
}
