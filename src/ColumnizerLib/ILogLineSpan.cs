namespace ColumnizerLib;

public interface ILogLineSpan
{
    ReadOnlySpan<char> GetFullLineSpan ();

    int LineNumber { get; }
}

public readonly ref struct LogLineSpan (ReadOnlyMemory<char> lineMemory, int lineNumber) : ILogLineSpan
{
    private readonly ReadOnlyMemory<char> _lineMemory = lineMemory;

    public static LogLineSpan Create (ReadOnlyMemory<char> lineMemory, int lineNumber) => new(lineMemory, lineNumber);

    public ReadOnlySpan<char> GetFullLineSpan () => _lineMemory.Span;

    public int LineNumber { get; } = lineNumber;
}