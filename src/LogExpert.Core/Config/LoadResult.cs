namespace LogExpert.Core.Config;

/// <summary>
/// Result of a settings load operation
/// </summary>
public class LoadResult
{
    /// <summary>
    /// The loaded settings object. Always populated on success or recovery.
    /// </summary>

    public Settings Settings { get; set; }

    /// <summary>
    /// Indicates whether the settings were loaded from a backup file due to a failure loading the primary file.
    /// </summary>
    public bool LoadedFromBackup { get; set; }

    /// <summary>
    /// A message describing the recovery process. Only meaningful when <see cref="LoadedFromBackup"/> is true.
    /// </summary>
    public string RecoveryMessage { get; set; }

    /// <summary>
    /// A title for the recovery message dialog. Only meaningful when <see cref="LoadedFromBackup"/> is true.
    /// </summary>
    public string RecoveryTitle { get; set; }

    /// <summary>
    /// Indicates a critical failure occurred during loading. When true, loading could not complete normally.
    /// </summary>
    public bool CriticalFailure { get; set; }

    /// <summary>
    /// A message describing the critical failure. Only meaningful when <see cref="CriticalFailure"/> is true.
    /// </summary>
    public string CriticalMessage { get; set; }

    /// <summary>
    /// A title for the critical failure dialog. Only meaningful when <see cref="CriticalFailure"/> is true.
    /// </summary>
    public string CriticalTitle { get; set; }

    /// <summary>
    /// Indicates whether user input is required to proceed after a critical failure.
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