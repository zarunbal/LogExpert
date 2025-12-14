namespace ColumnizerLib;

/// <summary>
/// Represents a single log line, including its full text and line number.
/// </summary>
/// <remarks>
/// <para>
/// <b>Purpose:</b> <br/>
/// The <c>LogLine</c> struct encapsulates the content and line number of a log entry. It is used throughout the
/// columnizer and log processing infrastructure to provide a strongly-typed, immutable representation of a log line.
/// </para>
/// <para>
/// <b>Usage:</b> <br/>
/// This struct implements the <see cref="ILogLine"/> interface, allowing it to be used wherever an <c>ILogLine</c>
/// is expected. It provides value semantics and is intended to be lightweight and efficiently passed by value.
/// </para>
/// <para>
/// <b>Relationship to ILogLine:</b> <br/>
/// <c>LogLine</c> is a concrete, immutable implementation of the <see cref="ILogLine"/> interface, providing
/// properties for the full line text and its line number.
/// </para>
/// <para>
/// <b>Why struct instead of record:</b> <br/>
/// A <c>struct</c> is preferred over a <c>record</c> here to avoid heap allocations and to provide value-type
/// semantics, which are beneficial for performance when processing large numbers of log lines. The struct is
/// immutable (readonly), ensuring thread safety and predictability. The previous <c>record</c> implementation
/// was replaced to better align with these performance and semantic requirements.
/// </para>
/// </remarks>
public class LogLine : ILogLineMemory
{
    string ILogLine.FullLine { get; }

    public int LineNumber { get; }

    string ITextValue.Text { get; }

    public ReadOnlyMemory<char> FullLine { get; }

    public ReadOnlyMemory<char> Text { get; }

    public LogLine (string fullLine, int lineNumber)
    {
        LineNumber = lineNumber;
        FullLine = fullLine.AsMemory();
        Text = fullLine.AsMemory();
    }

    public LogLine (ReadOnlyMemory<char> fullLine, int lineNumber)
    {
        LineNumber = lineNumber;
        FullLine = fullLine;
        Text = fullLine;
    }
}
