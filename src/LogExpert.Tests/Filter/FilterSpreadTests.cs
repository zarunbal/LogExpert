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
}
