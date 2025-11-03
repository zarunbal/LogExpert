namespace LogExpert.Core.Config;

/// <summary>
/// Result of a settings import operation
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorTitle { get; set; }

    public bool RequiresUserConfirmation { get; set; }

    public string ConfirmationMessage { get; set; }

    public string ConfirmationTitle { get; set; }

    public static ImportResult Successful () => new() { Success = true };

    public static ImportResult Failed (string title, string message) => new()
    {
        Success = false,
        ErrorTitle = title,
        ErrorMessage = message
    };

    public static ImportResult RequiresConfirmation (string title, string message) => new()
    {
        Success = false,
        RequiresUserConfirmation = true,
        ConfirmationTitle = title,
        ConfirmationMessage = message
    };
}
