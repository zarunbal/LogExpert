using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services.TabControllerService;

internal class WindowActivatedEventArgs (LogWindow window, LogWindow previousWindow) : EventArgs
{
    public LogWindow Window { get; } = window;

    public LogWindow PreviousWindow { get; } = previousWindow;
}
