namespace LogExpert.Core.Classes.Filter;

/// <summary>
/// The single home of the per-hit filter accumulation recipe: hit → <see cref="FilterSpread.Expand"/> →
/// append to results/history → <see cref="FilterSpread.TrimHistory"/>. Used internally by the serial
/// Filter Engine and per line by the Log Window's tail filter path, so the recipe exists exactly once.
/// Not thread-safe — callers that share an instance with concurrent readers (the tail path's GUI)
/// synchronize around it.
/// </summary>
public class FilterAccumulator
{
    /// <summary>Filter result lines: every hit plus its spread context, in accumulation order.</summary>
    public List<int> ResultLines { get; } = [];

    /// <summary>The hit lines only, without spread.</summary>
    public List<int> HitLines { get; } = [];

    /// <summary>The trailing dedup window (spread history) recent expansions were checked against.</summary>
    public List<int> History { get; } = [];

    /// <summary>
    /// Records a filter hit: adds it to <see cref="HitLines"/> and appends its spread expansion
    /// (deduplicated against <see cref="History"/>, clamped to the file's line range) to
    /// <see cref="ResultLines"/>.
    /// </summary>
    public void AddHit (int lineNum, int spreadBefore, int spreadBehind, int lineCount)
    {
        HitLines.Add(lineNum);
        var expanded = FilterSpread.Expand(lineNum, spreadBefore, spreadBehind, lineCount, History);
        ResultLines.AddRange(expanded);
        History.AddRange(expanded);
        FilterSpread.TrimHistory(History);
    }
}
