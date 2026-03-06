namespace ColumnizerLib;

/// <summary>
/// Implement this interface to add a menu entry to the context menu of LogExpert.
/// </summary>
/// <remarks>
/// <para>
/// The methods in this interface will be called in the GUI thread. So make sure that there's no
/// heavyweight work to do in your implementations.
/// </para>
/// </remarks>
public interface IContextMenuEntry
{
    #region Public methods

    /// <summary>
    /// This function is called from LogExpert if the context menu is about to be displayed.
    /// Your implementation can control whether LogExpert will show a menu entry by returning
    /// an appropriate value.<br></br>
    /// </summary>
    /// <param name="loglines">A list containing all selected line numbers.</param>
    /// <param name="columnizer">The currently selected Columnizer. You can use it to split log lines,
    ///     if necessary.</param>
    /// <param name="callback">The callback interface implemented by LogExpert. You can use the functions
    ///     for retrieving log lines or pass it along to functions of the Columnizer if needed.</param>
    /// <returns>
    /// Return the string which should be displayed in the context menu.<br></br>
    /// You can control the menu behaviour by returning the the following values:<br></br>
    ///   <ul>
    ///   <li>Normal string:  The string is displayed as a menu entry</li>
    ///   <li>String starting with underscore: The string is displayed as a disabled menu entry</li>
    ///   <li>null: No menu entry is displayed.</li>
    ///   </ul>
    /// </returns>
    [Obsolete("Use the overload of GetMenuText that takes an ILogLineMemory parameter instead.")]
    string GetMenuText (IList<int> loglines, ILogLineMemoryColumnizer columnizer, ILogExpertCallback callback);

    /// <summary>
    /// This function is called from LogExpert if the context menu is about to be displayed.
    /// Your implementation can control whether LogExpert will show a menu entry by returning
    /// an appropriate value.<br></br>
    /// </summary>
    /// <remarks>Throws an exception if any parameter is null or if linesCount is less than 1.</remarks>
    /// <param name="linesCount">The number of lines to include in the generated menu text. Must be a positive integer.</param>
    /// <param name="columnizer">An implementation of the ILogLineMemoryColumnizer interface used to format the log line data for display.</param>
    /// <param name="logline">An instance of ILogLineMemory representing the log line to be included in the menu text.</param>
    /// <returns>A string containing the formatted menu text based on the provided log line and formatting options.</returns>
    string GetMenuText (int linesCount, ILogLineMemoryColumnizer columnizer, ILogLineMemory logline);


    /// <summary>
    /// This function is called from LogExpert if the menu entry is choosen by the user. <br></br>
    /// Note that this function is called from the GUI thread. So try to avoid time consuming operations.
    /// </summary>
    /// <param name="loglines">A list containing all selected line numbers.</param>
    /// <param name="columnizer">The currently selected Columnizer. You can use it to split log lines,
    ///     if necessary.</param>
    /// <param name="callback">The callback interface implemented by LogExpert. You can use the functions
    ///     for retrieving log lines or pass it along to functions of the Columnizer if needed.</param>
    [Obsolete("Use the overload of MenuSelected that takes an ILogLineMemory parameter instead.")]
    void MenuSelected (IList<int> loglines, ILogLineMemoryColumnizer columnizer, ILogExpertCallback callback);

    /// <summary>
    /// This function is called from LogExpert if the menu entry is choosen by the user. <br></br>
    /// Note that this function is called from the GUI thread. So try to avoid time consuming operations.
    /// </summary>
    /// <param name="linesCount"></param>
    /// <param name="columnizer"></param>
    /// <param name="logline"></param>
    void MenuSelected (int linesCount, ILogLineMemoryColumnizer columnizer, ILogLineMemory logline);

    #endregion
}