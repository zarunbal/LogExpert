namespace ColumnizerLib.Extensions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Intentionally")]
public static class Extensions
{
    extension(ILogLineMemory logLine)
    {
        public string ToClipBoardText () => logLine == null ? string.Empty : $"\t{logLine.LineNumber + 1}\t{logLine.FullLine}";

    }
}