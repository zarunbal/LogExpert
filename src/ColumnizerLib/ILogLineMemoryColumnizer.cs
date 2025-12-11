namespace ColumnizerLib;

/// <summary>
/// Defines methods for splitting log lines into columns and extracting column values from in-memory log data using a
/// callback-based approach.
/// </summary>
/// <remarks>Implementations of this interface enable advanced log line parsing and columnization scenarios,
/// allowing consumers to process log data efficiently in memory. The interface is designed for use with log sources
/// that provide direct memory access to log lines, supporting custom column extraction and value notification
/// workflows. Thread safety and performance characteristics depend on the specific implementation.</remarks>
public interface ILogLineMemoryColumnizer : ILogLineColumnizer
{
    #region Public methods

    /// <summary>
    /// Splits a log line into columns using the specified callback for columnization.
    /// </summary>
    /// <param name="callback">The callback used to determine how the log line is split into columns. Cannot be null.</param>
    /// <param name="line">The log line to be split into columns. Cannot be null.</param>
    /// <returns>An object representing the columnized log line. The returned object contains the extracted columns from the
    /// input line.</returns>
    IColumnizedLogLineMemory SplitLine (ILogLineMemoryColumnizerCallback callback, ILogLineMemory line);

    /// <summary>
    /// Retrieves the timestamp associated with the specified log line.
    /// </summary>
    /// <param name="callback">An object that provides access to columnizer services for the log line. Used to obtain additional context or
    /// data required for timestamp extraction.</param>
    /// <param name="line">The log line from which to extract the timestamp.</param>
    /// <returns>A DateTime value representing the timestamp of the specified log line.</returns>
    DateTime GetTimestamp (ILogLineMemoryColumnizerCallback callback, ILogLineMemory line);

    /// <summary>
    /// Notifies the callback of a new value for the specified column, providing both the current and previous values.
    /// </summary>
    /// <param name="callback">The callback interface that receives the value update notification. Cannot be null.</param>
    /// <param name="column">The zero-based index of the column for which the value is being updated.</param>
    /// <param name="value">The new value to be associated with the specified column.</param>
    /// <param name="oldValue">The previous value that was associated with the specified column before the update.</param>
    void PushValue (ILogLineMemoryColumnizerCallback callback, int column, string value, string oldValue);

    #endregion
}