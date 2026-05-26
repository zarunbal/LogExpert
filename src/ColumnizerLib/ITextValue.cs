namespace ColumnizerLib;

/// <summary>
/// Represents a read-only text value.
/// </summary>
/// <remarks>This interface is deprecated and maintained only for backward compatibility. Use direct access to
/// FullLine or FullValue properties instead of relying on this interface.</remarks>
[Obsolete("ITextValue is deprecated. ITextValueMemory for a memory based implementation", false)]
public interface ITextValue
{
    #region Properties

    /// <summary>
    /// Gets the text content associated with this instance.
    /// </summary>
    string Text { get; }

    #endregion
}