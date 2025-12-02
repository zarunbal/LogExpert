namespace ColumnizerLib;

public interface ILogLine : ITextValue
{
    #region Properties

    string FullLine { get; }

    int LineNumber { get; }

    #endregion
}

public readonly struct LogLine (string fullLine, int lineNumber) : ILogLine
{
    public string FullLine { get; } = fullLine;

    public int LineNumber { get; } = lineNumber;

    public string Text => FullLine;

    public override bool Equals (object obj)
    {
        return obj is LogLine other &&
               FullLine == other.FullLine &&
               LineNumber == other.LineNumber;
    }

    public override int GetHashCode ()
    {
        return HashCode.Combine(FullLine, LineNumber);
    }

    public static bool operator == (LogLine left, LogLine right) => left.Equals(right);

    public static bool operator != (LogLine left, LogLine right) => !(left == right);
}