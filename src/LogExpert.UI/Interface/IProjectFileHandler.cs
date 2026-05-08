using LogExpert.Core.Classes.Persister;
using LogExpert.UI.Services.ProjectFileHandlerService;

namespace LogExpert.UI.Interface;

internal interface IProjectFileHandler
{
    ProjectLoadOutcome LoadProject (string projectFileName);

    ContinueLoadResult ContinueLoad (ProjectLoadOutcome loadOutcome, MissingFilesResolution? resolution, bool restoreLayout);

    bool SaveProject (string projectFileName, ProjectData projectData, out string? errorMessage);
}