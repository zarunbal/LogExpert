namespace ColumnizerLib;

public interface ILogLineSpanColumnizer : ILogLineMemoryColumnizer
{
    /// <summary>
    /// Span-based version of SplitLine that avoids string allocations
    /// </summary>
    IColumnizedLogLineMemory SplitLine (ILogLineColumnizerCallback callback, ReadOnlySpan<char> lineSpan, int lineNumber);

    /// <summary>
    /// Span-based timestamp extraction
    /// </summary>
    DateTime GetTimestamp (ILogLineColumnizerCallback callback, ReadOnlySpan<char> lineSpan, int lineNumber);

    /// <summary>
    /// Indicates if this columnizer supports span-based operations
    /// </summary>
    bool IsSpanSupported { get; }
}
