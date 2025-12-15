namespace LogExpert.UI.Dialogs;

/// <summary>
/// Represents the result of the Missing Files Dialog interaction.
/// </summary>
public enum MissingFilesDialogResult
{
    /// <summary>
    /// User cancelled the operation.
    /// </summary>
    Cancel,

    /// <summary>
    /// Load only the valid files that were found.
    /// </summary>
    LoadValidFiles,

    /// <summary>
    /// Load valid files and update the session file with new paths.
    /// </summary>
    LoadAndUpdateSession
}
