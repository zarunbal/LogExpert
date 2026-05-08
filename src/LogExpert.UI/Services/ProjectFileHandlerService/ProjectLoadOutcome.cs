using LogExpert.Core.Classes.Persister;

namespace LogExpert.UI.Services.ProjectFileHandlerService;

internal sealed class ProjectLoadOutcome
{
    public enum LoadStatus
    {
        Success,
        NeedsIntervention,
        Error,
        EmptyProject
    }

    public required LoadStatus Status { get; init; }

    public ProjectData? ProjectData { get; init; }

    public ProjectValidationResult? ValidationResult { get; init; }

    public string? LayoutXml { get; init; }

    public bool HasLayoutData => !string.IsNullOrEmpty(LayoutXml);

    public string? ErrorMessage { get; init; }
}