using System.Runtime.Versioning;

using LogExpert.Core.Classes.Persister;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Controls.LogWindow;
using LogExpert.UI.Interface;
using LogExpert.UI.Services.FileOperationService;

using NLog;

namespace LogExpert.UI.Services.ProjectFileHandlerService;

[SupportedOSPlatform("windows")]
internal sealed class ProjectFileHandler (
    IPluginRegistry pluginRegistry,
    Func<FileTabRequest, LogWindow> addFileTab)
    : IProjectFileHandler
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public ProjectLoadOutcome LoadProject (string projectFileName)
    {
        try
        {
            if (!File.Exists(projectFileName))
            {
                _logger.Warn("LoadProject: File does not exist: {FileName}", projectFileName);

                return new ProjectLoadOutcome
                {
                    Status = ProjectLoadOutcome.LoadStatus.Error,
                    ErrorMessage = $"Project file not found: {projectFileName}"
                };
            }

            var loadResult = ProjectPersister.LoadProjectData(projectFileName, pluginRegistry);

            if (loadResult?.ProjectData == null)
            {
                _logger.Warn("LoadProject: ProjectData is null for {FileName}", projectFileName);

                return new ProjectLoadOutcome
                {
                    Status = ProjectLoadOutcome.LoadStatus.Error,
                    ErrorMessage = Resources.LoadProject_UI_Message_Error_FileMaybeCorruptedOrInaccessible
                };
            }

            var projectData = loadResult.ProjectData;

            if (projectData.FileNames.Count == 0)
            {
                _logger.Warn("LoadProject: No files in project {FileName}", projectFileName);

                return new ProjectLoadOutcome
                {
                    Status = ProjectLoadOutcome.LoadStatus.EmptyProject,
                    ErrorMessage = Resources.LoadProject_UI_Message_Message_FilesForSessionCouldNotBeFound
                };
            }

            var layoutXml = projectData.TabLayoutXml;

            return loadResult.RequiresUserIntervention
                ? new ProjectLoadOutcome
                {
                    Status = ProjectLoadOutcome.LoadStatus.NeedsIntervention,
                    ProjectData = projectData,
                    ValidationResult = loadResult.ValidationResult,
                    LayoutXml = layoutXml
                }
                : new ProjectLoadOutcome
                {
                    Status = ProjectLoadOutcome.LoadStatus.Success,
                    ProjectData = projectData,
                    LayoutXml = layoutXml
                };
        }
        catch (Exception ex) when (ex is IOException or
                                         UnauthorizedAccessException or
                                         InvalidOperationException or
                                         Newtonsoft.Json.JsonException)
        {
            _logger.Error(ex, "LoadProject: Exception loading {FileName}", projectFileName);

            return new ProjectLoadOutcome
            {
                Status = ProjectLoadOutcome.LoadStatus.Error,
                ErrorMessage = $"Error loading project: {ex.Message}"
            };
        }
    }

    public ContinueLoadResult ContinueLoad (ProjectLoadOutcome loadOutcome, MissingFilesResolution? resolution, bool restoreLayout)
    {
        ArgumentNullException.ThrowIfNull(loadOutcome);
        ArgumentNullException.ThrowIfNull(loadOutcome.ProjectData);

        var projectData = loadOutcome.ProjectData;

        // Apply selected alternatives to file paths (in-place, exact string match)
        if (resolution?.SelectedAlternatives is { } alternatives)
        {
            for (int i = 0; i < projectData.FileNames.Count; i++)
            {
                var originalPath = projectData.FileNames[i];
                if (alternatives.TryGetValue(originalPath, out var replacement))
                {
                    projectData.FileNames[i] = replacement;
                }
            }
        }

        // Save updated session file if requested
        if (resolution is { UpdateSessionFile: true } && !string.IsNullOrEmpty(projectData.ProjectFilePath))
        {
            try
            {
                ProjectPersister.SaveProjectData(projectData.ProjectFilePath, projectData);
                _logger.Info("ContinueLoad: Updated session file {FileName}", projectData.ProjectFilePath);
            }
            catch (Exception ex) when (ex is IOException or
                                             UnauthorizedAccessException or
                                             InvalidOperationException or
                                             ArgumentException)
            {
                _logger.Error(ex, "ContinueLoad: Failed to update session file {FileName}", projectData.ProjectFilePath);
            }
        }

        // Handle "Open in new window" — resolve file names but do NOT open tabs
        if (resolution is { OpenInNewWindow: true })
        {
            var resolvedFiles = PersisterHelpers.FindFilenameForSettings(
                projectData.FileNames.AsReadOnly(), pluginRegistry);

            return new ContinueLoadResult
            {
                OpenedTabs = false,
                CloseAllTabs = false,
                OpenInNewWindowFiles = [.. resolvedFiles]
            };
        }

        // Open file tabs
        bool deferForLayout = loadOutcome.HasLayoutData && restoreLayout;
        int openedCount = 0;

        foreach (var fileName in projectData.FileNames)
        {
            var request = new FileTabRequest
            {
                FileName = fileName,
                ForcePersistenceLoading = true,
                DoNotAddToDockPanel = deferForLayout
            };

            try
            {
                _ = addFileTab(request);
                openedCount++;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException
                                         and not StackOverflowException
                                         and not AccessViolationException
                                         and not AppDomainUnloadedException
                                         and not BadImageFormatException
                                         and not CannotUnloadAppDomainException
                                         and not InvalidProgramException
                                         and not ThreadAbortException)
            {
                _logger.Error(ex, "ContinueLoad: Failed to open tab for {FileName}", fileName);
            }
        }

        return new ContinueLoadResult
        {
            OpenedTabs = openedCount > 0,
            CloseAllTabs = resolution?.CloseAllTabs ?? false,
            OpenInNewWindowFiles = null
        };
    }

    public bool SaveProject (string projectFileName, ProjectData projectData, out string? errorMessage)
    {
        try
        {
            ProjectPersister.SaveProjectData(projectFileName, projectData);
            errorMessage = null;
            return true;
        }
        catch (Exception ex) when (ex is IOException or
                                         UnauthorizedAccessException or
                                         InvalidOperationException or
                                         ArgumentException)
        {
            _logger.Error(ex, "SaveProject: Failed to save {FileName}", projectFileName);
            errorMessage = $"Error saving project: {ex.Message}";
            return false;
        }
    }
}