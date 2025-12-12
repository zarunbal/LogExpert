
using ColumnizerLib;

namespace GlassfishColumnizer;

internal class GlassFishLogLine (ReadOnlyMemory<char> fullLine, ReadOnlyMemory<char> text, int lineNumber) : ILogLineMemory
{
    #region Properties

    public ReadOnlyMemory<char> FullLine { get; } = fullLine;

    public ReadOnlyMemory<char> Text { get; } = text;

    string ILogLine.FullLine { get; }

    public int LineNumber { get; set; } = lineNumber;

    string ITextValue.Text => FullLine.ToString();

    #endregion
}