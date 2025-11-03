using LogExpert.Core.Entities;

namespace LogExpert.Core.Config;

/// <summary>
/// Result of a settings load operation
/// </summary>
public class LoadResult
{
    public Settings Settings { get; set; }
    public bool LoadedFromBackup { get; set; }
    public string RecoveryMessage { get; set; }
    public string RecoveryTitle { get; set; }
    public bool CriticalFailure { get; set; }
    public string CriticalMessage { get; set; }
    public string CriticalTitle { get; set; }
    public bool RequiresUserChoice { get; set; }
    
    public static LoadResult Success(Settings settings) => new()
    {
        Settings = settings
    };
    
    public static LoadResult FromBackup(Settings settings, string message, string title) => new()
    {
        Settings = settings,
        LoadedFromBackup = true,
        RecoveryMessage = message,
        RecoveryTitle = title
    };
    
    public static LoadResult Critical(Settings settings, string title, string message) => new()
    {
        Settings = settings,
        CriticalFailure = true,
        CriticalTitle = title,
        CriticalMessage = message,
        RequiresUserChoice = true
    };
}
