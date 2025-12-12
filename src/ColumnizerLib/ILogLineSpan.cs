public interface ILogLineSpan
{
    ReadOnlySpan<char> GetFullLineSpan ();

    int LineNumber { get; }
}

public readonly ref struct LogLineSpan : ILogLineSpan
{
    private readonly ReadOnlyMemory<char> _lineMemory;

    public LogLineSpan (ReadOnlyMemory<char> lineMemory, int lineNumber)
    {
        _lineMemory = lineMemory;
        LineNumber = lineNumber;
    }

    public static LogLineSpan Create (ReadOnlyMemory<char> lineMemory, int lineNumber) => new LogLineSpan(lineMemory, lineNumber);

    public ReadOnlySpan<char> GetFullLineSpan () => _lineMemory.Span;

    public int LineNumber { get; }
}