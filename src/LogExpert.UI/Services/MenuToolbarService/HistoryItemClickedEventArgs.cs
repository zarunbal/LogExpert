using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert.UI.Services.MenuToolbarService;

/// <summary>Event args for file history item interactions.</summary>
internal class HistoryItemClickedEventArgs (string fileName) : EventArgs
{
    public string FileName { get; } = fileName;
}
