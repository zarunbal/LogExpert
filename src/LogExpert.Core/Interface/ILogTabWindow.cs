namespace LogExpert.Core.Interface;

//TODO: Add documentation
public interface ILogTabWindow
{
    /// <summary>
    ///
    /// </summary>
    ILogExpertProxy LogExpertProxy { get; set; }

    /// <summary>
    ///
    /// </summary>
    bool IsDisposed { get; }

    void Activate ();

    object Invoke (Delegate method, params object?[]? objects);

    /// <summary>
    /// Load given files into a new or current Logwindow
    /// </summary>
    /// <param name="fileNames"></param>
    void LoadFiles (string[] fileNames);

    /// <summary>
    /// Show an error message, if this is the only allowed instance, and the error message should be displayed
    /// </summary>
    void ShowOnlyOneInstanceError ();

    /// <summary>
    /// Set the current Logwindow to be first in line
    /// </summary>
    void SetForeground ();

    /// <summary>
    /// Show the current logwindow
    /// </summary>
    void Show ();
}