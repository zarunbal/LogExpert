using ColumnizerLib;

using LogExpert.Core.Classes.Persister;

namespace LogExpert.Core.Interfaces;

/// <summary>
/// Represents a log window that displays and manages a log file in LogExpert.
/// This interface provides access to log file content, timestamp operations, line selection,
/// and persistence functionality.
/// </summary>
/// <remarks>
/// This interface is primarily implemented by the LogWindow class in LogExpert.UI.
/// It serves as an abstraction layer between the core functionality and the UI layer,
/// allowing for loose coupling between components.
/// </remarks>
public interface ILogWindow
{
    /// <summary>
    /// Gets the file name of the log file that contains the specified line number.
    /// </summary>
    /// <param name="lineNum">The zero-based line number.</param>
    /// <returns>The file name (without path) of the log file containing the specified line.
    /// In multi-file mode, this may return different file names for different line numbers.</returns>
    /// <remarks>
    /// This method is particularly useful in multi-file mode where multiple log files
    /// are viewed together as one virtual file.
    /// </remarks>
    string GetCurrentFileName (int lineNum);

    /// <summary>
    /// Retrieves the memory representation of a log line at the specified line number.
    /// </summary>
    /// <param name="lineNum">The zero-based index of the log line to retrieve. Must be greater than or equal to 0 and less than the total
    /// number of lines.</param>
    /// <returns>An object that provides access to the memory of the specified log line.</returns>
    ILogLineMemory GetLineMemory (int lineNum);

    /// <summary>
    /// Retrieves the log line memory for the specified line number, waiting if the data is not immediately available.
    /// </summary>
    /// <param name="lineNum">The zero-based index of the log line to retrieve. Must be greater than or equal to 0.</param>
    /// <returns>An object representing the memory for the specified log line. The returned object provides access to the log
    /// line's content and associated metadata.</returns>
    ILogLineMemory GetLogLineMemoryWithWait (int lineNum);

    /// <summary>
    /// Gets the timestamp for the line at or after the specified line number,
    /// searching forward through the file.
    /// </summary>
    /// <param name="lineNum">
    /// A reference to the line number to start searching from.
    /// This value is updated to the line number where the timestamp was found.
    /// </param>
    /// <param name="roundToSeconds">
    /// If <c>true</c>, the returned timestamp is rounded to the nearest second.
    /// </param>
    /// <returns>
    /// The timestamp of the line at or after the specified line number,
    /// or <see cref="DateTime.MinValue"/> if no valid timestamp is found.
    /// </returns>
    /// <remarks>
    /// Not all log lines may contain timestamps. This method searches forward
    /// from the given line number until it finds a line with a valid timestamp.
    /// The <paramref name="lineNum"/> parameter is updated to reflect the line
    /// where the timestamp was found.
    /// </remarks>
    //TODO Find a way to not use a referenced int (https://github.com/LogExperts/LogExpert/issues/404)
    DateTime GetTimestampForLineForward (ref int lineNum, bool roundToSeconds);

    /// <summary>
    /// Gets the timestamp for the line at or before the specified line number,
    /// searching backward through the file.
    /// second.
    /// </summary>
    /// <param name="lastLineNum">A reference to the line number to start searching from. This value is updated to the line number where the timestamp was found.</param>
    /// <param name="roundToSeconds">true to round the timestamp to the nearest second; otherwise, false to return the precise timestamp.</param>
    /// <returns>A tuple containing the timestamp for the specified line and the last line number for which a timestamp is
    /// available.</returns>
    /// <remarks>
    /// Not all log lines may contain timestamps. This method searches backward
    /// from the given line number until it finds a line with a valid timestamp.
    /// the returned tuple contains the lastLineNumber
    /// </remarks>
    (DateTime timeStamp, int lastLineNumber) GetTimestampForLine (int lastLineNum, bool roundToSeconds);

    /// <summary>
    /// Finds the line number that corresponds to the specified timestamp within
    /// the given range, using a binary search algorithm.
    /// </summary>
    /// <param name="lineNum">The starting line number for the search.</param>
    /// <param name="rangeStart">The first line number of the search range (inclusive).</param>
    /// <param name="rangeEnd">The last line number of the search range (inclusive).</param>
    /// <param name="timestamp">The timestamp to search for.</param>
    /// <param name="roundToSeconds">
    /// If <c>true</c>, timestamps are rounded to seconds for comparison.
    /// </param>
    /// <returns>
    /// The line number of the line with a timestamp closest to the specified timestamp,
    /// or -1 if no matching line is found within the range.
    /// </returns>
    /// <remarks>
    /// This method is used for timestamp-based navigation and synchronization between
    /// multiple log windows. It performs a binary search for optimal performance.
    /// </remarks>
    int FindTimestampLineInternal (int lineNum, int rangeStart, int rangeEnd, DateTime timestamp, bool roundToSeconds);

    /// <summary>
    /// Selects the specified line in the log view and optionally scrolls to make it visible.
    /// </summary>
    /// <param name="lineNum">The zero-based line number to select.</param>
    /// <param name="triggerSyncCall">
    /// If <c>true</c>, triggers timestamp synchronization with other log windows
    /// that are in sync mode.
    /// </param>
    /// <param name="shouldScroll">
    /// If <c>true</c>, scrolls the view to ensure the selected line is visible.
    /// </param>
    /// <remarks>
    /// This method is used for programmatic line selection, such as when jumping
    /// to bookmarks, navigating through search results, or synchronizing with other windows.
    /// </remarks>
    void SelectLine (int lineNum, bool triggerSyncCall, bool shouldScroll);

    /// <summary>
    /// Gathers a Session Snapshot of this log window's persistable state, including recursive
    /// child snapshots for its filter pipe tabs.
    /// </summary>
    /// <returns>
    /// A <see cref="SessionSnapshot"/> capturing the current state of the window: current line,
    /// filters, columnizer configuration, and other settings.
    /// </returns>
    /// <remarks>
    /// The snapshot is mapped to the Session File's serialized form by
    /// <see cref="SessionFileComposer"/> and applied back in two phases when a session is
    /// loaded.
    /// </remarks>
    SessionSnapshot GatherSessionSnapshot ();

    /// <summary>
    /// Creates a new temporary file tab with the specified content.
    /// </summary>
    /// <param name="fileName">The path to the temporary file to display.</param>
    /// <param name="title">The title to display on the tab.</param>
    /// <remarks>
    /// Temporary file tabs are used for displaying filtered content, search results,
    /// or other derived views of the original log file.
    /// </remarks>
    void AddTempFileTab (string fileName, string title);

    /// <summary>
    /// Creates a new tab containing the specified list of log line entries.
    /// </summary>
    /// <param name="lineEntryList">
    /// A list of <see cref="LineEntryMemory"/> objects containing the lines and their
    /// original line numbers to display in the new tab.
    /// </param>
    /// <param name="title">The title to display on the tab.</param>
    /// <remarks>
    /// This method is used to pipe filtered or selected content into a new tab
    /// without creating a physical file. The new tab maintains references to the
    /// original line numbers for context.
    /// </remarks>
    void WritePipeTab (IList<LineEntryMemory> lineEntryList, string title);

    /// <summary>
    /// Activates this log window and brings it to the foreground.
    /// </summary>
    /// <remarks>
    /// This method is typically called when switching between multiple log windows
    /// or when a background operation completes and needs to notify the user.
    /// </remarks>
    void Activate ();

    /// <summary>
    /// Gets the <see cref="ILogfileReader"/> instance that provides access to the
    /// underlying log file content.
    /// </summary>
    /// <value>
    /// The reader that manages file access, buffering, and
    /// multi-file coordination for this log window.
    /// </value>
    /// <remarks>
    /// The Logfile Reader handles all file I/O operations, including reading lines,
    /// monitoring for file changes, and managing the buffer cache.
    /// Query for capability interfaces (<see cref="IMultiFileNavigation"/>,
    /// <see cref="ILogfileReaderConfiguration"/>, <see cref="IBufferPinning"/>)
    /// when specialized operations are needed.
    /// </remarks>
    ILogfileReader LogFileReader { get; }

    /// <summary>
    /// Gets the text content of the currently selected cell or line.
    /// </summary>
    /// <value>
    /// The text content of the current selection, or an empty string if nothing is selected.
    /// </value>
    string Text { get; }

    /// <summary>
    /// Gets the file name (with full path) of the primary log file being displayed.
    /// </summary>
    /// <value>
    /// The full file path of the log file, or an empty string if no file is loaded.
    /// </value>
    /// <remarks>
    /// In multi-file mode, this returns the path of the first file in the multi-file set.
    /// Use <see cref="GetCurrentFileName"/> to get the file name for a specific line number.
    /// </remarks>
    string FileName { get; }

    //event EventHandler<LogEventArgs> FileSizeChanged; //TODO: All handlers should be moved to Core

    //event EventHandler TailFollowed;
    //LogExpert.UI.Controls.LogTabWindow.LogTabWindow.LogWindowData Tag { get; }
}