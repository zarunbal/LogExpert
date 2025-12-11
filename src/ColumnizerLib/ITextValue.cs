namespace ColumnizerLib;

/// <summary>
/// Represents a read-only text value.
/// </summary>
/// <remarks>This interface is deprecated and maintained only for backward compatibility. Use direct access to
/// FullLine or FullValue properties instead of relying on this interface.</remarks>
[Obsolete("ITextValue is deprecated. Access FullLine or FullValue directly instead.", false)]
public interface ITextValue
{
    #region Properties

    /// <summary>
    /// Gets the text content associated with this instance.
    /// </summary>
    [Obsolete("Use FullLine or FullValue properties directly instead of this property.")]
    string Text { get; }

    #endregion
}

/// <summary>
/// Provides extension methods for retrieving text representations from log line and column memory objects.
/// </summary>
/// <remarks>These extension methods are obsolete. Use the corresponding properties on the target interfaces or
/// classes directly instead of these methods.</remarks>
public static class TextValueExtensions
{
    [Obsolete("Use ILogLine.FullLine property directly instead of this extension method")]
    public static string GetText (this ILogLine logLine) => logLine.FullLine;

    [Obsolete("Use DisplayValue property directly")]
    public static string GetText (this IColumnMemory column) => column.DisplayValue;
}