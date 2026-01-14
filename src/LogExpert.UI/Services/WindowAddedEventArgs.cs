using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services;

internal class WindowAddedEventArgs (LogWindow window, string title) : EventArgs
{
    public LogWindow Window { get; } = window;

    public string Title { get; } = title;
}
