namespace ColumnizerLib;

/// <summary>
/// Defines a callback interface for retrieving memory-based representations of individual log lines by line number.
/// </summary>
/// <remarks>Implementations of this interface enable columnizers to access log line data in a memory-efficient
/// format, which may improve performance when processing large log files.</remarks>
public interface ILogLineMemoryColumnizerCallback
{
    #region Public methods

    /// <summary>
    /// This property returns the current line number. That is the line number of the log line
    /// a ILogLineColumnizer function is called for (e.g. the line that has to be painted).
    /// </summary>
    /// <returns>The current line number starting at 0</returns>
    int LineNum { get; }

    /// <summary>
    /// Returns the full file name (path + name) of the current log file.
    /// </summary>
    /// <returns>File name of current log file</returns>
    string GetFileName ();

    /// <summary>
    /// Returns the number of lines of the logfile.
    /// </summary>
    /// <returns>Number of lines.</returns>
    int GetLineCount ();

    /// <summary>
    /// Retrieves the memory representation of the log line at the specified line number.
    /// </summary>
    /// <param name="lineNum">The zero-based index of the log line to retrieve. Must be greater than or equal to 0 and less than the total
    /// number of log lines.</param>
    /// <returns>An object implementing <see cref="ILogLineMemory"/> that represents the specified log line. Returns <see
    /// langword="null"/> if the line number is out of range.</returns>
    ILogLineMemory GetLogLineMemory (int lineNum);

    #endregion
}