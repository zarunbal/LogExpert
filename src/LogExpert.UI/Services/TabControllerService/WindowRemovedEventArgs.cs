using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services.TabControllerService;

internal class WindowRemovedEventArgs (LogWindow window) : EventArgs
{
    public LogWindow Window { get; } = window;
}
