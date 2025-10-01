using System.Text;

using Newtonsoft.Json;

using NLog;

namespace LogExpert.Core.Classes.Persister;

public static class ProjectPersister
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    #region Public methods

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
                                 IOException)
        {
            _logger.Error(ex, $"Error loading persistence data from {projectFileName}");
            return null;
        }
    }

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