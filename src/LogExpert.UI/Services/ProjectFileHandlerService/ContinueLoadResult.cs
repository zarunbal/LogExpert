namespace LogExpert.UI.Services.ProjectFileHandlerService;

internal readonly record struct ContinueLoadResult
{
    public bool OpenedTabs { get; init; }

    public bool CloseAllTabs { get; init; }

    public string[]? OpenInNewWindowFiles { get; init; }
}