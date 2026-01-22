namespace ColumnizerLib.Extensions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "Intentionally")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Intentionally")]
public static class Extensions
{
    extension(ILogLine logLine)
    {
        public string ToClipBoardText () => logLine == null ? string.Empty : $"\t{logLine.LineNumber + 1}\t{logLine.FullLine}";
    }

    extension(ILogLineMemory logLine)
    {
        public string ToClipBoardText () => logLine == null ? string.Empty : $"\t{logLine.LineNumber + 1}\t{logLine.FullLine}";

    }
}