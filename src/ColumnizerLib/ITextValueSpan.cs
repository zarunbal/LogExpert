namespace ColumnizerLib;

[Obsolete("ITextValue is deprecated. ITextValueMemory for a memory based implementation instead.", false)]
public interface ITextValueSpan
{
    #region Properties

    string Text { get; }

    #endregion
}

[Obsolete("TextValueSpanExtensions is deprecated. ITextValueMemory for a memory based implementation instead.", false)]
public static class TextValueSpanExtensions
{
    public static string GetText (this ILogLine logLine) => logLine.FullLine;

    public static string GetText (this IColumn column) => column.DisplayValue;
}