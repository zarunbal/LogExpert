
using ColumnizerLib;

namespace CsvColumnizer;

public class CsvLogLine (string fullLine, int lineNumber) : ILogLine, ILogLineMemory
{
    #region Properties

    public string FullLine { get; set; } = fullLine;

    public int LineNumberMemory { get; set; } = lineNumber;

    string ITextValue.Text => FullLine;

    public ReadOnlyMemory<char> FullLineMemory { get; }

    public ReadOnlyMemory<char> TextMemory { get; }

    #endregion

    public CsvLogLine (ReadOnlyMemory<char> fullLine, int lineNumber) : this(fullLine.ToString(), lineNumber)
    {
        FullLine = fullLine.ToString();
        LineNumberMemory = lineNumber;
        FullLineMemory = fullLine;
        TextMemory = fullLine;
    }
}