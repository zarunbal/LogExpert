namespace ColumnizerLib;

/// <summary>
/// Represents a text value that exposes its underlying memory as a read-only span of characters.
/// </summary>
public interface ITextValueMemory
{
    #region Properties

    /// <summary>
    /// Gets the text content as a read-only region of memory.
    /// </summary>
    ReadOnlyMemory<char> Text { get; }

    #endregion
}