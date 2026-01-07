using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services;

/// <summary>
/// Service for managing LED indicator icons on log window tabs
/// </summary>
/// <remarks>
/// This service is thread-safe and can be called from any thread.
/// Icon updates are automatically marshaled to the UI thread via events.
/// </remarks>
internal interface ILedIndicatorService : IDisposable
{
    /// <summary>
    /// Initializes the LED service with the specified tail color
    /// </summary>
    /// <param name="tailColor">Color to use for the tail-follow indicator</param>
    /// <exception cref="InvalidOperationException">Thrown if already initialized</exception>
    void Initialize (Color tailColor);

    /// <summary>
    /// Gets the appropriate icon for the specified diff level and state
    /// </summary>
    /// <param name="diffLevel">Activity level (0-100)</param>
    /// <param name="state">Current LED state (dirty, tail, sync)</param>
    /// <returns>Icon representing the current state</returns>
    Icon GetIcon (int diffLevel, LedState state);

    /// <summary>
    /// Gets the "dead" icon shown when a file is missing
    /// </summary>
    /// <returns>Dead file icon</returns>
    Icon GetDeadIcon ();

    /// <summary>
    /// Starts the LED animation thread
    /// </summary>
    void Start ();

    /// <summary>
    /// Stops the LED animation thread
    /// </summary>
    void Stop ();

    /// <summary>
    /// Registers a window for LED state tracking
    /// </summary>
    /// <param name="window">LogWindow to track</param>
    void RegisterWindow (LogWindow window);

    /// <summary>
    /// Unregisters a window from LED state tracking
    /// </summary>
    /// <param name="window">LogWindow to stop tracking</param>
    void UnregisterWindow (LogWindow window);

    /// <summary>
    /// Updates the activity level for a window
    /// </summary>
    /// <param name="window">Window to update</param>
    /// <param name="lineDiff">Number of new lines added</param>
    void UpdateWindowActivity (LogWindow window, int lineDiff);

    /// <summary>
    /// Regenerates all icons with new color
    /// </summary>
    /// <param name="tailColor">New tail color</param>
    void RegenerateIcons (Color tailColor);

    /// <summary>
    /// Gets the current tail color used for LED indicators
    /// </summary>
    Color CurrentTailColor { get; }

    /// <summary>
    /// Event fired when a window's icon should be updated
    /// </summary>
    event EventHandler<IconChangedEventArgs> IconChanged;
}
