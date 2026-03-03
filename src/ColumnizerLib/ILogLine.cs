namespace ColumnizerLib;

/// <summary>
/// Represents a single line from a log file, including its content and line number.
/// </summary>
/// <remarks>Implementations of this interface provide access to both the full text of the log line and its
/// position within the source log. This can be used to correlate log entries with their original context or for
/// processing log files line by line.</remarks>
public interface ILogLine : ITextValue
{
    #region Properties

    /// <summary>
    /// Gets the full text of the line, including all characters and whitespace.
    /// </summary>
    string FullLine { get; }

    /// <summary>
    /// Gets the line number in the source text associated with this element.
    /// </summary>
    int LineNumber { get; }

    #endregion
}