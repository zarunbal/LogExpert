using System.Text;

namespace LogExpert.Core.Classes.Persister;

/// <summary>
/// A neutral, UI-free capture of one Log Window's persistable state (the Session Snapshot of
/// CONTEXT.md). Gathered by the Log Window when a Session File is saved and applied back in two
/// Log-Window-side phases when one is loaded. Mapped to and from <see cref="PersistenceData"/> by
/// <see cref="SessionFileComposer"/>; see docs/specs/session-file-composer.md for the full field
/// mapping table.
/// </summary>
public class SessionSnapshot
{
    public bool FollowTail { get; set; }

    public Encoding Encoding { get; set; }

    /// <summary>
    /// The log file's line count at save time. Never applied to the window — it is the input to
    /// the Rollover staleness rule (<see cref="SessionFileComposer.IsStale"/>).
    /// </summary>
    public int LineCount { get; set; }
}
