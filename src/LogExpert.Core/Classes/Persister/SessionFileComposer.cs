namespace LogExpert.Core.Classes.Persister;

/// <summary>
/// Pure mapping between a <see cref="SessionSnapshot"/> and the Session File's serialized form
/// (<see cref="PersistenceData"/>), in both directions, plus the Rollover staleness rule.
/// No I/O, no UI, no side effects — the Log Window owns gathering, applying, timing, and error
/// display; <see cref="Persister"/> owns serialization. Field-by-field contract:
/// docs/specs/session-file-composer.md.
/// </summary>
public static class SessionFileComposer
{
    /// <summary>
    /// Maps a snapshot to the Session File's serialized form. Fields the snapshot does not carry
    /// (the dead fields and <c>SessionFileName</c> named in the spec) are left at their type
    /// defaults. Note: <see cref="Persister"/> rewrites <c>FileName</c> on SameDir saves *after*
    /// compose — transformed by design, outside this contract.
    /// </summary>
    public static PersistenceData Compose (SessionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new PersistenceData
        {
            FollowTail = snapshot.FollowTail,
            Encoding = snapshot.Encoding,
            LineCount = snapshot.LineCount,
            CurrentLine = snapshot.CurrentLine,
            FirstDisplayedLine = snapshot.FirstDisplayedLine,
            FilterPosition = snapshot.FilterPosition,
            FilterVisible = snapshot.FilterVisible,
            FilterAdvanced = snapshot.FilterAdvanced,
            CellSelectMode = snapshot.CellSelectMode,
            FilterSaveListVisible = snapshot.FilterSaveListVisible,
            MultiFile = snapshot.MultiFile,
            MultiFileMaxDays = snapshot.MultiFileMaxDays,
            MultiFilePattern = snapshot.MultiFilePattern,
            TabName = snapshot.TabName,
            HighlightGroupName = snapshot.HighlightGroupName,
            FileName = snapshot.FileName,
            BookmarkList = snapshot.BookmarkList,
            RowHeightList = snapshot.RowHeightList,
            MultiFileNames = snapshot.MultiFileNames,
            FilterParamsList = snapshot.FilterParamsList,
            Columnizer = snapshot.Columnizer,
            FilterTabDataList =
            [
                .. snapshot.FilterTabs.Select(tab => new FilterTabData
                {
                    FilterParams = tab.FilterParams,
                    PersistenceData = Compose(tab.Snapshot),
                }),
            ],
        };
    }

    /// <summary>
    /// Maps a loaded Session File back to a snapshot. Fields the snapshot does not carry are
    /// ignored.
    /// </summary>
    public static SessionSnapshot Decompose (PersistenceData persistenceData)
    {
        ArgumentNullException.ThrowIfNull(persistenceData);

        return new SessionSnapshot
        {
            FollowTail = persistenceData.FollowTail,
            Encoding = persistenceData.Encoding,
            LineCount = persistenceData.LineCount,
            CurrentLine = persistenceData.CurrentLine,
            FirstDisplayedLine = persistenceData.FirstDisplayedLine,
            FilterPosition = persistenceData.FilterPosition,
            FilterVisible = persistenceData.FilterVisible,
            FilterAdvanced = persistenceData.FilterAdvanced,
            CellSelectMode = persistenceData.CellSelectMode,
            FilterSaveListVisible = persistenceData.FilterSaveListVisible,
            MultiFile = persistenceData.MultiFile,
            MultiFileMaxDays = persistenceData.MultiFileMaxDays,
            MultiFilePattern = persistenceData.MultiFilePattern,
            TabName = persistenceData.TabName,
            HighlightGroupName = persistenceData.HighlightGroupName,
            FileName = persistenceData.FileName,
            BookmarkList = persistenceData.BookmarkList,
            RowHeightList = persistenceData.RowHeightList,
            MultiFileNames = persistenceData.MultiFileNames,
            FilterParamsList = persistenceData.FilterParamsList,
            Columnizer = persistenceData.Columnizer,
            FilterTabs =
            [
                .. persistenceData.FilterTabDataList.Select(tab => new FilterTabSnapshot
                {
                    FilterParams = tab.FilterParams,
                    Snapshot = Decompose(tab.PersistenceData),
                }),
            ],
        };
    }

    /// <summary>
    /// Declares a snapshot stale because it was saved against a longer file than the one on disk
    /// — i.e. the log file has rolled over since the save. A stale snapshot's post-load state
    /// (bookmarks, scroll, filters) must be discarded by the Log Window; the pre-load options
    /// still apply.
    /// </summary>
    public static bool IsStale (SessionSnapshot snapshot, int currentLineCount)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return snapshot.LineCount > currentLineCount;
    }
}
