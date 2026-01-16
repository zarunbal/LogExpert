using LogExpert.UI.Controls.LogWindow;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.UI.Services;

internal interface ITabController : IDisposable
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="window"></param>
    /// <param name="title"></param>
    /// <param name="doNotAddToDockPanel"></param>
    void AddWindow (LogWindow window, string title, bool doNotAddToDockPanel = false);

    /// <summary>
    ///
    /// </summary>
    /// <param name="window"></param>
    void RemoveWindow (LogWindow window);

    void CloseWindow (LogWindow window, bool skipConfirmation = false);

    void CloseAllWindows ();

    void CloseAllExcept (LogWindow window);

    void ActivateWindow (LogWindow window);

    LogWindow GetActiveWindow ();

    void SwitchToNextWindow ();

    void SwitchToPreviousWindow ();

    LogWindow FindWindowByFileName (string fileName);

    IReadOnlyList<LogWindow> GetAllWindows ();

    /// <summary>
    /// Gets all LogWindow instances from the DockPanel's Contents collection.
    /// This returns windows that are currently displayed in the DockPanel,
    /// which may include windows not explicitly tracked by TabController
    /// (e.g., windows restored from layout serialization).
    /// </summary>
    /// <remarks>
    /// Use this method when you need to iterate over all visible LogWindows,
    /// particularly for operations like:
    /// - Saving project/session data
    /// - Saving last open files list
    /// - Closing all tabs
    /// - Applying settings to all windows
    ///
    /// For most other operations, prefer <see cref="GetAllWindows"/> which
    /// returns only explicitly tracked windows.
    /// </remarks>
    /// <returns>Read-only list of all LogWindows in the DockPanel</returns>
    IReadOnlyList<LogWindow> GetAllWindowsFromDockPanel ();

    int GetWindowCount ();

    bool HasWindow (LogWindow window);

    void InitializeDockPanel (DockPanel dockPanel);

    event EventHandler<WindowAddedEventArgs> WindowAdded;
    event EventHandler<WindowRemovedEventArgs> WindowRemoved;
    event EventHandler<WindowActivatedEventArgs> WindowActivated;
    event EventHandler<WindowClosingEventArgs> WindowClosing;
}
