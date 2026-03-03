
using ColumnizerLib;

namespace CsvColumnizer;

public class CsvLogLine (string fullLine, int lineNumber) : ILogLineMemory
{
    #region Properties

    string ILogLine.FullLine { get; }

    string ITextValue.Text => FullLine.ToString();

    public ReadOnlyMemory<char> FullLine { get; } = fullLine.AsMemory();

    public ReadOnlyMemory<char> Text { get; }

    public int LineNumber { get; } = lineNumber;

    #endregion

    public CsvLogLine (ReadOnlyMemory<char> fullLine, int lineNumber) : this(fullLine.ToString(), lineNumber)
    {
        FullLine = fullLine;
        LineNumber = lineNumber;
        Text = fullLine;
    }
}