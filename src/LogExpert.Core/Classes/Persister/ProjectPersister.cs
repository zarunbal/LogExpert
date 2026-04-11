using System.Text;

using LogExpert.Core.Interfaces;

using Newtonsoft.Json;

using NLog;

namespace LogExpert.Core.Classes.Persister;

public static class ProjectPersister
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    #region Public methods

    /// <summary>
    /// Loads the project session data from a specified file, including validation of referenced files.
    /// Resolves .lxp persistence files to actual .log files before validation.
    /// </summary>
    /// <param name="projectFileName">The path to the project file (.lxj)</param>
    /// <param name="pluginRegistry">The plugin registry for file system validation</param>
    /// <returns>A <see cref="ProjectLoadResult"/> containing the project data and validation results</returns>
    public static ProjectLoadResult LoadProjectData (string projectFileName, IPluginRegistry pluginRegistry)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var json = File.ReadAllText(projectFileName, Encoding.UTF8);
            var projectData = JsonConvert.DeserializeObject<ProjectData>(json, settings);

            // Set project file path for alternative file search
            projectData.ProjectFilePath = projectFileName;

            // Resolve .lxp files to actual .log files
            var resolvedFiles = ProjectFileResolver.ResolveProjectFiles(projectData, pluginRegistry);

            // Create mapping: logFile → originalFile
            var logToOriginalMapping = new Dictionary<string, string>();
            foreach (var (logFile, originalFile) in resolvedFiles)
            {
                logToOriginalMapping[logFile] = originalFile;
            }

            // Create new ProjectData with resolved log file paths
            var resolvedProjectData = new ProjectData
            {
                FileNames = [.. resolvedFiles.Select(r => r.LogFile)],
                TabLayoutXml = projectData.TabLayoutXml,
                ProjectFilePath = projectData.ProjectFilePath
            };

            // Validate the actual log files (not .lxp files)
            var validationResult = ProjectFileValidator.ValidateProject(resolvedProjectData, pluginRegistry);

            return new ProjectLoadResult
            {
                ProjectData = resolvedProjectData,
                ValidationResult = validationResult,
                LogToOriginalFileMapping = logToOriginalMapping
            };
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or
                                         IOException or
                                         JsonSerializationException)
        {
            _logger.Warn($"Error loading persistence data from {projectFileName}, trying old xml version");

            var projectData = ProjectPersisterXML.LoadProjectData(projectFileName);

            // Set project file path for alternative file search
            projectData.ProjectFilePath = projectFileName;

            // Resolve .lxp files for XML fallback as well
            var resolvedFiles = ProjectFileResolver.ResolveProjectFiles(projectData, pluginRegistry);

            var logToOriginalMapping = new Dictionary<string, string>();
            foreach (var (logFile, originalFile) in resolvedFiles)
            {
                logToOriginalMapping[logFile] = originalFile;
            }

            var resolvedProjectData = new ProjectData
            {
                FileNames = [.. resolvedFiles.Select(r => r.LogFile)],
                TabLayoutXml = projectData.TabLayoutXml,
                ProjectFilePath = projectData.ProjectFilePath
            };

            var validationResult = ProjectFileValidator.ValidateProject(resolvedProjectData, pluginRegistry);

            return new ProjectLoadResult
            {
                ProjectData = resolvedProjectData,
                ValidationResult = validationResult,
                LogToOriginalFileMapping = logToOriginalMapping
            };
        }
    }

    /// <summary>
    /// Saves the specified project data to a file in JSON format.
    /// </summary>
    /// <remarks>The method serializes the <paramref name="projectData"/> into a JSON string with indented
    /// formatting and writes it to the specified <paramref name="projectFileName"/> using UTF-8 encoding.</remarks>
    /// <param name="projectFileName">The path to the file where the project data will be saved. Cannot be null or empty.</param>
    /// <param name="projectData">The project data to be serialized and saved. Cannot be null.</param>
    public static void SaveProjectData (string projectFileName, ProjectData projectData)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        };

        try
        {
            var json = JsonConvert.SerializeObject(projectData, settings);
            File.WriteAllText(projectFileName, json, Encoding.UTF8);
        }
        catch (Exception ex) when (ex is JsonSerializationException or
                                         UnauthorizedAccessException or
                                         IOException)
        {
            _logger.Error(ex, $"Error saving persistence data to {projectFileName}");
        }
    }

    #endregion
}