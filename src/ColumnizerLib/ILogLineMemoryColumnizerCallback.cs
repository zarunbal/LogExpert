namespace ColumnizerLib;

/// <summary>
/// Defines a callback interface for retrieving memory-based representations of individual log lines by line number.
/// </summary>
/// <remarks>Implementations of this interface enable columnizers to access log line data in a memory-efficient
/// format, which may improve performance when processing large log files. This interface extends <see
/// cref="ILogLineColumnizerCallback"/> to provide additional capabilities for memory-based log line access.</remarks>
public interface ILogLineMemoryColumnizerCallback : ILogLineColumnizerCallback
{
    #region Public methods

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