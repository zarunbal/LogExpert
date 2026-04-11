namespace LogExpert.Core.Config;

/// <summary>
/// Result of a settings import operation
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Indicates whether the import operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The error message describing why the import failed.
    /// Populated when <see cref="Success"/> is false and <see cref="RequiresUserConfirmation"/> is false.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// The title for the error message.
    /// Populated when <see cref="Success"/> is false and <see cref="RequiresUserConfirmation"/> is false.
    /// </summary>
    public string ErrorTitle { get; set; }

    /// <summary>
    /// Indicates whether the import operation requires user confirmation to proceed.
    /// When true, <see cref="ConfirmationMessage"/> and <see cref="ConfirmationTitle"/> are populated.
    /// </summary>
    public bool RequiresUserConfirmation { get; set; }

    /// <summary>
    /// The message to display when user confirmation is required.
    /// Populated when <see cref="RequiresUserConfirmation"/> is true.
    /// </summary>
    public string ConfirmationMessage { get; set; }

    /// <summary>
    /// The title for the confirmation message.
    /// Populated when <see cref="RequiresUserConfirmation"/> is true.
    /// </summary>
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
