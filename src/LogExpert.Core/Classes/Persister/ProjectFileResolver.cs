using System.Collections.ObjectModel;

using LogExpert.Core.Interface;

namespace LogExpert.Core.Classes.Persister;

/// <summary>
/// Helper class to resolve project file references to actual log files.
/// Handles .lxp (persistence) files by extracting the actual log file path.
/// </summary>
public static class ProjectFileResolver
{
    /// <summary>
    /// Resolves project file names to actual log files.
    /// If a file is a .lxp persistence file, extracts the log file path from it.
    /// </summary>
    /// <param name="projectData">The project data containing file references</param>
    /// <param name="pluginRegistry">Plugin registry for file system resolution (optional)</param>
    /// <returns>List of tuples containing (logFilePath, originalFilePath)</returns>
    public static ReadOnlyCollection<(string LogFile, string OriginalFile)> ResolveProjectFiles (ProjectData projectData, IPluginRegistry pluginRegistry = null)
    {
        ArgumentNullException.ThrowIfNull(projectData);

        var resolved = new List<(string LogFile, string OriginalFile)>();

        foreach (var fileName in projectData.FileNames)
        {
            var logFile = PersisterHelpers.FindFilenameForSettings(fileName, pluginRegistry);
            resolved.Add((logFile, fileName));
        }

        return resolved.AsReadOnly();
    }
}