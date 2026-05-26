namespace ColumnizerLib;

/// <summary>
/// Represents a log line that exposes its full content as a contiguous block of memory.
/// </summary>
/// <remarks>Implementations provide access to the entire log line as a <see cref="ReadOnlyMemory{T}"/>, enabling efficient,
/// allocation-free operations on the underlying character data. This is useful for scenarios where high-performance
/// parsing or processing of log lines is required.</remarks>
public interface ILogLineMemory : ITextValueMemory
{
    /// <summary>
    /// Gets the full content of the line as a read-only region of memory.
    /// </summary>
    ReadOnlyMemory<char> FullLine { get; }

    /// <summary>
    /// Gets the line number in the source text associated with this element.
    /// </summary>
    int LineNumber { get; }
}