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

    // Window Activation
    void ActivateWindow (LogWindow window);

    LogWindow GetActiveWindow ();

    void SwitchToNextWindow ();

    void SwitchToPreviousWindow ();

    // Window Queries
    LogWindow FindWindowByFileName (string fileName);

    IReadOnlyList<LogWindow> GetAllWindows ();

    int GetWindowCount ();

    bool HasWindow (LogWindow window);

    // DockPanel Integration
    void InitializeDockPanel (DockPanel dockPanel);

    // Events
    event EventHandler<WindowAddedEventArgs> WindowAdded;
    event EventHandler<WindowRemovedEventArgs> WindowRemoved;
    event EventHandler<WindowActivatedEventArgs> WindowActivated;
    event EventHandler<WindowClosingEventArgs> WindowClosing;
}
