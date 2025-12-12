namespace ColumnizerLib;

/// <summary>
/// This helper struct holds a log line and its line number (zero based).
/// This struct is used by <see cref="ILogExpertCallbackMemory"/>.
/// </summary>
/// <seealso cref="ILogExpertCallbackMemory.AddPipedTab"/>
public struct LineEntryMemory : IEquatable<LineEntryMemory>
{
    /// <summary>
    /// The content of the line.
    /// </summary>
    public ILogLineMemory LogLine { get; set; }

    /// <summary>
    /// The line number. See <see cref="ILogExpertCallbackMemory.AddPipedTab"/> for an explanation of the line number.
    /// </summary>
    public int LineNum { get; set; }

    public override bool Equals (object obj)
    {
        return obj is LineEntryMemory other && Equals(other);
    }

    public readonly bool Equals (LineEntryMemory other)
    {
        return LineNum == other.LineNum && Equals(LogLine, other.LogLine);
    }

    public override readonly int GetHashCode ()
    {
        return HashCode.Combine(LineNum, LogLine);
    }

    public static bool operator == (LineEntryMemory left, LineEntryMemory right) => left.Equals(right);

    public static bool operator != (LineEntryMemory left, LineEntryMemory right) => !left.Equals(right);
}