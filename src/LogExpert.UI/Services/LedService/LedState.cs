namespace LogExpert.UI.Services.LedService;

/// <summary>
/// LED state information
/// </summary>
public class LedState
{
    /// <summary>
    /// Current activity level (0-100)
    /// </summary>
    public int DiffSum { get; set; }

    /// <summary>
    /// Whether the tab content has changed since last viewed
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Tail follow state
    /// </summary>
    public TailFollowState TailState { get; set; }

    /// <summary>
    /// Time synchronization state
    /// </summary>
    public TimeSyncState SyncState { get; set; }

    /// <summary>
    /// Creates a copy of this state
    /// </summary>
    public LedState Clone ()
    {
        return new LedState
        {
            DiffSum = DiffSum,
            IsDirty = IsDirty,
            TailState = TailState,
            SyncState = SyncState
        };
    }
}
