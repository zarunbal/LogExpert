namespace LogExpert.Core.Config;

/// <summary>
/// Result of a settings load operation
/// </summary>
public class LoadResult
{
    /// <summary>
    /// Gets or sets the loaded settings.
    /// </summary>
    public Settings Settings { get; set; }

    /// <summary>
    /// Indicates whether the settings were loaded from a backup.
    /// </summary>
    public bool LoadedFromBackup { get; set; }

    /// <summary>
    /// Message to show to the user if settings were recovered from backup
    /// </summary>
    public string RecoveryMessage { get; set; }

    /// <summary>
    /// Gets or sets the title used for recovery operations.
    /// </summary>
    public string RecoveryTitle { get; set; }

    /// <summary>
    /// Indicates whether a critical failure occurred during loading.
    /// </summary>
    public bool CriticalFailure { get; set; }

    /// <summary>
    /// Message to show to the user in case of a critical failure
    /// </summary>
    public string CriticalMessage { get; set; }

    /// <summary>
    /// Gets or sets the title used to indicate critical messages.
    /// </summary>
    public string CriticalTitle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user choice is required.
    /// </summary>
    public bool RequiresUserChoice { get; set; }

    /// <summary>
    /// Creates a successful LoadResult.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static LoadResult Success (Settings settings) => new()
    {
        Settings = settings
    };

    /// <summary>
    /// Creates a LoadResult indicating settings were loaded from a backup.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static LoadResult FromBackup (Settings settings, string message, string title) => new()
    {
        Settings = settings,
        LoadedFromBackup = true,
        RecoveryMessage = message,
        RecoveryTitle = title
    };

    /// <summary>
    /// Creates a LoadResult indicating a critical failure occurred.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static LoadResult Critical (Settings settings, string title, string message) => new()
    {
        Settings = settings,
        CriticalFailure = true,
        CriticalTitle = title,
        CriticalMessage = message,
        RequiresUserChoice = true
    };
}
