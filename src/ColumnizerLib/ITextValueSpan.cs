namespace ColumnizerLib;

// DEPRECATED: This interface adds no value and causes performance overhead.
// Keep for backward compatibility but mark as obsolete.
[Obsolete("ITextValue is deprecated. Access FullLine or FullValue directly instead.", false)]
public interface ITextValueSpan
{
    #region Properties

    string Text { get; }

    #endregion
}

public static class TextValueSpanExtensions
{
    [Obsolete("Use ILogLine.FullLine property directly instead of this extension method")]
    public static string GetText (this ILogLine logLine) => logLine.FullLine;

    [Obsolete("Use DisplayValue property directly")]
    public static string GetText (this IColumn column) => column.DisplayValue;
}