using System.Collections.ObjectModel;

using LogExpert.Core.Classes.Filter;

using NUnit.Framework;

namespace LogExpert.Tests.Filter;

[TestFixture]
public class FilterSpreadTests
{
    // Row format: hit line, spread before, spread behind, line count, already-taken history, expected lines.
    // Expected values are worked examples from the spec (docs/specs/filter-spread-extraction.md).
    [TestCase(5, 0, 0, 100, new int[0], new[] { 5 },
        TestName = "Expand_NoSpreadConfigured_ReturnsOnlyTheHitLine")]
    [TestCase(5, 0, 0, 100, new[] { 5 }, new[] { 5 },
        TestName = "Expand_NoSpreadConfigured_IgnoresHistory_HistoricalQuirkPreserved")]
    [TestCase(0, 3, 2, 100, new int[0], new[] { 0, 1, 2 },
        TestName = "Expand_HitAtLineZero_EmitsNoNegativeLines")]
    [TestCase(3, 5, 0, 100, new int[0], new[] { 0, 1, 2, 3 },
        TestName = "Expand_BackSpreadReachingPastTopOfFile_ClampsAtLineZeroAndIncludesIt")]
    [TestCase(97, 0, 5, 100, new int[0], new[] { 97, 98, 99 },
        TestName = "Expand_ForeSpreadReachingPastEndOfFile_ClampsAtLastLine")]
    [TestCase(99, 0, 3, 100, new int[0], new[] { 99 },
        TestName = "Expand_HitAtLastLineWithForeSpread_ReturnsOnlyTheHit")]
    [TestCase(10, 1, 3, 100, new int[0], new[] { 9, 10, 11, 12, 13 },
        TestName = "Expand_AsymmetricSpread_ReturnsBackLinesHitThenForeLines")]
    [TestCase(12, 2, 2, 100, new[] { 8, 9, 10, 11, 12 }, new[] { 13, 14 },
        TestName = "Expand_OverlappingWithEarlierHit_SuppressesLinesAlreadyTaken")]
    [TestCase(5, 1, 1, 100, new[] { 5 }, new[] { 4, 6 },
        TestName = "Expand_HitItselfAlreadyTaken_IsNotEmittedAgain")]
    public void Expand_Table (int lineNum, int spreadBefore, int spreadBehind, int lineCount, int[] alreadyTaken, int[] expected)
    {
        var result = FilterSpread.Expand(lineNum, spreadBefore, spreadBehind, lineCount, alreadyTaken);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TrimHistory_LongerThanWindow_DropsOldestEntriesDownToWindowSize ()
    {
        // Window is 2 × 99 (the maximum configurable spread) — unifies the divergent 50/99 constants.
        var history = new List<int>(Enumerable.Range(0, 250));

        FilterSpread.TrimHistory(history);

        Assert.Multiple(() =>
        {
            Assert.That(history, Has.Count.EqualTo(198));
            Assert.That(history[0], Is.EqualTo(52));
            Assert.That(history[^1], Is.EqualTo(249));
        });
    }

    [Test]
    public void TrimHistory_WithinWindow_IsUnchanged ()
    {
        var history = new List<int>(Enumerable.Range(0, 198));

        FilterSpread.TrimHistory(history);

        Assert.That(history, Has.Count.EqualTo(198));
    }

    [Test]
    public void TrimHistory_WorksOnAnyIListImplementation ()
    {
        // The Filter Pipe history is an IList<int>, not a List<int> — the trim rule must not care.
        IList<int> history = new Collection<int>();
        foreach (var i in Enumerable.Range(0, 200))
        {
            history.Add(i);
        }

        FilterSpread.TrimHistory(history);

        Assert.Multiple(() =>
        {
            Assert.That(history, Has.Count.EqualTo(198));
            Assert.That(history[0], Is.EqualTo(2));
        });
    }

    /// <summary>
    /// Equivalence guard: the single-threaded Log Window filter and the multi-threaded
    /// Core filter accumulate per-hit results through the same recipe
    /// (hit → Expand → append to results/history → TrimHistory). This pins that shared
    /// recipe's output for a hit sequence with overlapping spreads; a call site that
    /// deviates from the recipe no longer matches this specification. Expected values are
    /// a worked example: hits 5, 7 and 50 with spread 2/2 in a 100-line file.
    /// </summary>
    [Test]
    public void AccumulationRecipe_OverlappingHitSequence_ProducesPinnedResultHitAndHistoryLists ()
    {
        int[] hits = [5, 7, 50];
        List<int> resultLines = [];
        List<int> history = [];
        List<int> hitList = [];

        foreach (var hit in hits)
        {
            hitList.Add(hit);
            var expanded = FilterSpread.Expand(hit, spreadBefore: 2, spreadBehind: 2, lineCount: 100, alreadyTaken: history);
            resultLines.AddRange(expanded);
            history.AddRange(expanded);
            FilterSpread.TrimHistory(history);
        }

        Assert.Multiple(() =>
        {
            // Hit 5 contributes 3..7; hit 7 contributes only 8,9 (5..7 already taken); hit 50 contributes 48..52.
            Assert.That(resultLines, Is.EqualTo(new[] { 3, 4, 5, 6, 7, 8, 9, 48, 49, 50, 51, 52 }));
            Assert.That(hitList, Is.EqualTo(new[] { 5, 7, 50 }));
            Assert.That(history, Is.EqualTo(resultLines), "history below the window size mirrors the result lines");
        });
    }
}
