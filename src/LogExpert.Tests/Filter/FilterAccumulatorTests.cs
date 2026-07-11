using LogExpert.Core.Classes.Filter;

using NUnit.Framework;

namespace LogExpert.Tests.Filter;

[TestFixture]
public class FilterAccumulatorTests
{
    /// <summary>
    /// The accumulator is the single home of the per-hit recipe (hit → Expand → append →
    /// TrimHistory). Expected values are the worked example pinned by
    /// <see cref="FilterSpreadTests"/>' accumulation-recipe guard: hits 5, 7 and 50 with
    /// spread 2/2 in a 100-line file.
    /// </summary>
    [Test]
    public void AddHit_OverlappingHitSequence_ProducesPinnedResultHitAndHistoryLists ()
    {
        var accumulator = new FilterAccumulator();

        accumulator.AddHit(5, spreadBefore: 2, spreadBehind: 2, lineCount: 100);
        accumulator.AddHit(7, spreadBefore: 2, spreadBehind: 2, lineCount: 100);
        accumulator.AddHit(50, spreadBefore: 2, spreadBehind: 2, lineCount: 100);

        Assert.Multiple(() =>
        {
            // Hit 5 contributes 3..7; hit 7 contributes only 8,9 (5..7 already taken); hit 50 contributes 48..52.
            Assert.That(accumulator.ResultLines, Is.EqualTo(new[] { 3, 4, 5, 6, 7, 8, 9, 48, 49, 50, 51, 52 }));
            Assert.That(accumulator.HitLines, Is.EqualTo(new[] { 5, 7, 50 }));
            Assert.That(accumulator.History, Is.EqualTo(accumulator.ResultLines),
                "history below the window size mirrors the result lines");
        });
    }
}
