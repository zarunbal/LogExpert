using System.Text;

using Newtonsoft.Json;

using NLog;

namespace LogExpert.Core.Classes.Persister;

public static class ProjectPersister
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    #region Public methods

    /// <summary>
    /// Loads the project session data from a specified file.
    /// </summary>
    /// <param name="projectFileName"></param>
    /// <returns></returns>
    public static ProjectData LoadProjectData (string projectFileName)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var json = File.ReadAllText(projectFileName, Encoding.UTF8);
            return JsonConvert.DeserializeObject<ProjectData>(json, settings);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or
                                         IOException or
                                         JsonSerializationException)
        {

            _logger.Warn($"Error loading persistence data from {projectFileName}, trying old xml version");
            return ProjectPersisterXML.LoadProjectData(projectFileName);
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