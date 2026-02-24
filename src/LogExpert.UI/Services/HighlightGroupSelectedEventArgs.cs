using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert.UI.Services;

/// <summary>Event args for highlight group combo box selection.</summary>
internal class HighlightGroupSelectedEventArgs (string groupName) : EventArgs
{
    public string GroupName { get; } = groupName;
}
