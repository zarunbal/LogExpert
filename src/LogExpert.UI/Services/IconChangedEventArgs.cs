using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services;

/// <summary>
/// Event arguments for icon change notifications
/// </summary>
internal class IconChangedEventArgs (LogWindow window, Icon newIcon) : EventArgs
{
    public LogWindow Window { get; } = window;

    public Icon NewIcon { get; } = newIcon;
}