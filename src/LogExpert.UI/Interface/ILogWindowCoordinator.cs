using LogExpert.Core.Entities;

namespace LogExpert.UI.Interface;

/// <summary>
/// Coordinates workspace-level operations for LogWindow instances.
/// Replaces the concrete LogTabWindow reference that LogWindow previously held.
/// </summary>
internal interface ILogWindowCoordinator
{
    /// <summary>
    /// Resolves the appropriate highlight group using a 4-tier fallback chain:
    /// 1. File-mask regex match (if fileName is provided)<br></br>
    /// 2. Name match (if groupName is provided)<br></br>
    /// 3. First group in the list<br></br>
    /// 4. New empty group<br></br>
    /// Never returns null.
    /// </summary>
    HighlightGroup ResolveHighlightGroup (string? groupName, string? fileName);

    /// <summary>
    /// Raised after highlight settings have been changed (e.g., after settings dialog closes).
    /// Subscribers should re-resolve their highlight groups.
    /// </summary>
    event EventHandler HighlightSettingsChanged;
}
