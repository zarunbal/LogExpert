using ColumnizerLib;

namespace Log4jXmlColumnizer;

internal class Log4JLogLine (ReadOnlyMemory<char> fullLine, ReadOnlyMemory<char> text, int lineNumber) : ILogLineMemory
{
    #region Properties

    public ReadOnlyMemory<char> FullLine { get; set; } = fullLine;

    public int LineNumber { get; set; } = lineNumber;

    public ReadOnlyMemory<char> Text { get; } = text;

    string ITextValue.Text => FullLine.ToString();

    string ILogLine.FullLine { get; }

    #endregion
}