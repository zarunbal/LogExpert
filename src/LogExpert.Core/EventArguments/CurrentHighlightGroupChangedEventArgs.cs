using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;

namespace LogExpert.Core.EventArguments;

public class CurrentHighlightGroupChangedEventArgs (ILogWindow logWindow, HighlightGroup currentGroup) : EventArgs
{
    #region Properties

    public ILogWindow LogWindow { get; } = logWindow;

    public HighlightGroup CurrentGroup { get; } = currentGroup;

    #endregion
}