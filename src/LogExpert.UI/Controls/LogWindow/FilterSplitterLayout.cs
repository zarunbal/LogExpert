namespace LogExpert.UI.Controls.LogWindow;

/// <summary>
/// Pure layout math for the filter row's split container (issue #560).
/// Keeps the text filter (Panel1) from being grown so large that the Panel2 controls
/// ("Search", the filter checkboxes and the "Show advanced..." button) are pushed
/// outside the visible area of the application.
/// </summary>
internal static class FilterSplitterLayout
{
    /// <summary>
    /// Clamps a desired splitter distance so that neither panel shrinks below its minimum size.
    /// </summary>
    /// <param name="desiredDistance">The requested distance (Panel1 width) in pixels.</param>
    /// <param name="containerWidth">Total width of the split container.</param>
    /// <param name="splitterWidth">Width of the splitter bar.</param>
    /// <param name="panel1MinSize">Minimum width of Panel1 (the text filter).</param>
    /// <param name="panel2MinSize">Minimum width of Panel2 (the buttons/checkboxes).</param>
    /// <returns>A distance guaranteed to keep Panel2 at least <paramref name="panel2MinSize"/> wide.</returns>
    public static int ClampSplitterDistance (int desiredDistance, int containerWidth, int splitterWidth, int panel1MinSize, int panel2MinSize)
    {
        // When the container is too small to honour both minimums there is no perfect answer;
        // keep Panel1 at its minimum (matching the SplitContainer's own preference) and avoid
        // an invalid (min > max) clamp range.
        var maxDistance = Math.Max(containerWidth - splitterWidth - panel2MinSize, panel1MinSize);
        return Math.Clamp(desiredDistance, panel1MinSize, maxDistance);
    }
}
