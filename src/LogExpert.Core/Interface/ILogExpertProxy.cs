namespace LogExpert.Core.Interface;

public interface ILogExpertProxy
{
    #region Public methods

    /// <summary>
    /// Load the given files into the existing window.
    /// </summary>
    /// <param name="fileNames"></param>
    void LoadFiles(string[] fileNames);

    /// <summary>
    /// Open a new LogExpert window and load the given files.
    /// </summary>
    /// <param name="fileNames"></param>
    void NewWindow(string[] fileNames);

    /// <summary>
    /// load given files into the locked window or open a new window if no window is locked.
    /// </summary>
    /// <param name="fileNames"></param>
    void NewWindowOrLockedWindow(string[] fileNames);


    /// <summary>
    /// Called from LogTabWindow when the window is about to be closed.
    /// </summary>
    /// <param name="logWin"></param>
    void WindowClosed(ILogTabWindow logWin);

    /// <summary>
    /// Notifies the proxy that a window has been activated by the user.
    /// Used to track which window should receive new files when "Allow Only One Instance" is enabled.
    /// </summary>
    /// <param name="window">The window that was activated</param>
    void NotifyWindowActivated(ILogTabWindow window);

    int GetLogWindowCount();

    #endregion

    //void BroadcastSettingsChanged(Object cookie);
}