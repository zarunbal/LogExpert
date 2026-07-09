namespace LogExpert.Core.Classes.Filter;

/// <summary>
/// Owner of the Filter Spread rules: expanding a filter hit into its back/fore spread
/// context lines and trimming the duplicate-suppression history.
/// Pure — no dependency on controls, callbacks, readers or locks.
/// </summary>
public static class FilterSpread
{
    /// <summary>
    /// Maximum configurable spread. The duplicate-suppression history window derives from
    /// this value so it always covers the largest configurable spread.
    /// </summary>
    public const int SPREAD_MAX = 99;

    /// <summary>
    /// Trims the duplicate-suppression history to the last 2 × <see cref="SPREAD_MAX"/> entries.
    /// </summary>
    public static void TrimHistory (IList<int> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        while (history.Count > SPREAD_MAX * 2)
        {
            history.RemoveAt(0);
        }
    }

    /// <summary>
    /// Returns the lines a filter hit contributes to the filter result: the hit itself,
    /// plus (if configured) back-spread and fore-spread context lines. Lines already present
    /// in <paramref name="alreadyTaken"/> are suppressed. This function does not evaluate
    /// the filter condition.
    /// </summary>
    public static IList<int> Expand (int lineNum, int spreadBefore, int spreadBehind, int lineCount, IList<int> alreadyTaken)
    {
        ArgumentNullException.ThrowIfNull(alreadyTaken);

        IList<int> resultList = [];

        if (spreadBefore == 0 && spreadBehind == 0)
        {
            resultList.Add(lineNum);
            return resultList;
        }

        // back spread (ascending, clamped at line 0 — line 0 is a valid context line)
        for (var i = spreadBefore; i > 0; --i)
        {
            var line = lineNum - i;
            if (line >= 0 && !resultList.Contains(line) && !alreadyTaken.Contains(line))
            {
                resultList.Add(line);
            }
        }

        // the hit itself
        if (!resultList.Contains(lineNum) && !alreadyTaken.Contains(lineNum))
        {
            resultList.Add(lineNum);
        }

        // fore spread (ascending, clamped at the last line)
        for (var i = 1; i <= spreadBehind; ++i)
        {
            var line = lineNum + i;
            if (line < lineCount && !resultList.Contains(line) && !alreadyTaken.Contains(line))
            {
                resultList.Add(line);
            }
        }

        return resultList;
    }
}
