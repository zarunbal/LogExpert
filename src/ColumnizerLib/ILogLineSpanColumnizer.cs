namespace ColumnizerLib;

public interface ILogLineSpanColumnizer : ILogLineMemoryColumnizer
{
    /// <summary>
    /// Span-based version of SplitLine that avoids string allocations
    /// </summary>
    IColumnizedLogLineMemory SplitLine (ILogLineMemoryColumnizerCallback callback, ReadOnlySpan<char> lineSpan, int lineNumber);

    /// <summary>
    /// Span-based timestamp extraction
    /// </summary>
    DateTime GetTimestamp (ILogLineMemoryColumnizerCallback callback, ReadOnlySpan<char> lineSpan, int lineNumber);

    /// <summary>
    /// Indicates if this columnizer supports span-based operations
    /// </summary>
    bool IsSpanSupported { get; }
}
