using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services;

internal class WindowAddedEventArgs (LogWindow window) : EventArgs
{
    public LogWindow Window { get; } = window;
}
