using System.Collections.ObjectModel;

using LogExpert.Core.Interfaces;

namespace LogExpert.Core.Classes.Persister;

/// <summary>
/// Helper class to resolve Session file references to actual log files.
/// Handles Session File (.lxp) entries by extracting the actual log file path.
/// </summary>
public static class SessionFileResolver
{
    /// <summary>
    /// Resolves Session file names to actual log files.
    /// If a file is a Session File (.lxp) entry, extracts the log file path from it.
    /// </summary>
    /// <param name="sessionData">The Session data containing file references</param>
    /// <param name="pluginRegistry">Plugin registry for file system resolution (optional)</param>
    /// <returns>List of tuples containing (logFilePath, originalFilePath)</returns>
    public static ReadOnlyCollection<(string LogFile, string OriginalFile)> ResolveSessionFiles (SessionData sessionData, IPluginRegistry pluginRegistry = null)
    {
        ArgumentNullException.ThrowIfNull(sessionData);

        var resolved = new List<(string LogFile, string OriginalFile)>();

        foreach (var fileName in sessionData.FileNames)
        {
            var logFile = PersisterHelpers.FindFilenameForSettings(fileName, pluginRegistry);
            resolved.Add((logFile, fileName));
        }

        return resolved.AsReadOnly();
    }
}
