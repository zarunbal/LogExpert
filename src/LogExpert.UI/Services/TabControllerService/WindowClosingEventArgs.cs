
using System.ComponentModel;

using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Services.TabControllerService;

internal class WindowClosingEventArgs (LogWindow window, bool skipConfirmation) : CancelEventArgs
{
    public LogWindow Window { get; } = window;

    public bool SkipConfirmation { get; } = skipConfirmation;
}
