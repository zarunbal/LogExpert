
using ColumnizerLib;

namespace GlassfishColumnizer;

internal class GlassFishLogLine (ReadOnlyMemory<char> fullLine, ReadOnlyMemory<char> text, int lineNumber) : ILogLineMemory
{
    #region Properties

    public ReadOnlyMemory<char> FullLine { get; } = fullLine;

    public ReadOnlyMemory<char> Text { get; } = text;

    public int LineNumber { get; set; } = lineNumber;

    #endregion
}