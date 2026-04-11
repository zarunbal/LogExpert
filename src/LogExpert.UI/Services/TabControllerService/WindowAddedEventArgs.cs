using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services.TabControllerService;

internal class WindowAddedEventArgs (LogWindow window) : EventArgs
{
    public LogWindow Window { get; } = window;
}
