using LogExpert.UI.Controls.LogWindow;

using NUnit.Framework;

namespace LogExpert.Tests.Controls;

/// <summary>
/// Tests for the splitter-distance clamping used by the filter row's split container
/// (see issue #560). The text filter (Panel1) must not be growable to a size that pushes
/// the Panel2 controls ("Search", checkboxes, "Show advanced...") outside the app.
/// </summary>
[TestFixture]
public class FilterSplitterLayoutTests
{
    [Test]
    public void ClampSplitterDistance_DesiredWithinBounds_ReturnedUnchanged ()
    {
        var result = FilterSplitterLayout.ClampSplitterDistance(
            desiredDistance: 600,
            containerWidth: 1855,
            splitterWidth: 4,
            panel1MinSize: 200,
            panel2MinSize: 660);

        Assert.That(result, Is.EqualTo(600));
    }

    [Test]
    public void ClampSplitterDistance_DesiredTooLarge_ClampedSoPanel2KeepsMinWidth ()
    {
        // 1855 - 4 (splitter) - 660 (panel2 min) = 1191 is the furthest the splitter may go.
        var result = FilterSplitterLayout.ClampSplitterDistance(
            desiredDistance: 1800,
            containerWidth: 1855,
            splitterWidth: 4,
            panel1MinSize: 200,
            panel2MinSize: 660);

        Assert.That(result, Is.EqualTo(1191));
    }

    [Test]
    public void ClampSplitterDistance_DesiredTooSmall_ClampedUpToPanel1MinWidth ()
    {
        var result = FilterSplitterLayout.ClampSplitterDistance(
            desiredDistance: 50,
            containerWidth: 1855,
            splitterWidth: 4,
            panel1MinSize: 200,
            panel2MinSize: 660);

        Assert.That(result, Is.EqualTo(200));
    }

    [Test]
    public void ClampSplitterDistance_ContainerTooSmallForBothMinimums_DoesNotThrowAndKeepsPanel1Min ()
    {
        // 700 - 4 - 660 = 36, which is below panel1MinSize. Must not throw (min > max for Math.Clamp).
        var result = FilterSplitterLayout.ClampSplitterDistance(
            desiredDistance: 500,
            containerWidth: 700,
            splitterWidth: 4,
            panel1MinSize: 200,
            panel2MinSize: 660);

        Assert.That(result, Is.EqualTo(200));
    }
}
