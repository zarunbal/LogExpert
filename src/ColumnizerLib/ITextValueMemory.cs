namespace ColumnizerLib;

/// <summary>
/// Represents a text value that exposes its underlying memory as a read-only span of characters.
/// </summary>
/// <remarks>This interface extends <see cref="ITextValue"/> to provide direct access to the underlying character
/// memory, enabling efficient operations without additional string allocations. Implementations may use this to support
/// high-performance text processing scenarios.</remarks>
public interface ITextValueMemory : ITextValue
{
    #region Properties

    /// <summary>
    /// Gets the text content as a read-only region of memory.
    /// </summary>
    ReadOnlyMemory<char> TextMemory { get; }

    #endregion
}

/// <summary>
/// Provides extension methods for retrieving the textual content from log line and column memory representations.
/// </summary>
public static class TextValueMemoryExtensions
{
    /// <summary>
    /// Gets the full text content of the specified log line as a read-only memory region.
    /// </summary>
    /// <param name="logLine">The log line from which to retrieve the text content. Cannot be null.</param>
    /// <returns>A read-only memory region containing the characters of the entire log line.</returns>
    public static ReadOnlyMemory<char> GetText (this ILogLineMemory logLine) => logLine.FullLineMemory;

    /// <summary>
    /// Gets the display text of the column as a read-only block of memory.
    /// </summary>
    /// <param name="column">The column from which to retrieve the display text. Cannot be null.</param>
    /// <returns>A read-only memory region containing the display text of the specified column.</returns>
    public static ReadOnlyMemory<char> GetText (this IColumnMemory column) => column.DisplayValueMemory;
}