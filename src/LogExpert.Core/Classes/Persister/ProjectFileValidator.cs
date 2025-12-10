using LogExpert.Core.Interface;

namespace LogExpert.Core.Classes.Persister;

public static class ProjectFileValidator
{
    public static ProjectValidationResult ValidateProject (ProjectData projectData, IPluginRegistry pluginRegistry)
    {
        ArgumentNullException.ThrowIfNull(projectData);
        ArgumentNullException.ThrowIfNull(pluginRegistry);

        var result = new ProjectValidationResult();

        foreach (var fileName in projectData.FileNames)
        {
            var normalizedPath = NormalizeFilePath(fileName);

            if (File.Exists(normalizedPath))
            {
                result.ValidFiles.Add(fileName);
            }
            else if (IsUri(fileName))
            {
                // Check if URI-based file system plugin is available
                var fs = pluginRegistry.FindFileSystemForUri(fileName);
                if (fs != null)
                {
                    result.ValidFiles.Add(fileName);
                }
                else
                {
                    result.MissingFiles.Add(fileName);
                }
            }
            else
            {
                result.MissingFiles.Add(fileName);

                // Try to find file with relative path
                var alternativePaths = FindAlternativePaths(fileName, projectData.ProjectFilePath);
                result.PossibleAlternatives[fileName] = alternativePaths;
            }
        }

        return result;
    }

    private static string NormalizeFilePath (string fileName)
    {
        // Handle .lxp files (persistence files)
        if (fileName.EndsWith(".lxp", StringComparison.OrdinalIgnoreCase))
        {
            var persistenceData = Persister.Load(fileName);
            return persistenceData?.FileName ?? fileName;
        }

        return fileName;
    }

    private static bool IsUri (string fileName)
    {
        // Check if the string is a valid URI with a scheme (protocol)
        // URIs typically have the format: scheme://path or scheme:/path
        // Examples: sftp://server/file.log, http://example.com/log.txt

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        // Try to parse as URI
        if (Uri.TryCreate(fileName, UriKind.Absolute, out var uri))
        {
            // Check if it has a scheme other than file://
            // file:// URIs are local file paths and should be handled as regular files
            return !string.IsNullOrEmpty(uri.Scheme) &&
                   !uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static List<string> FindAlternativePaths (string fileName, string projectFilePath)
    {
        var alternatives = new List<string>();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return alternatives;
        }

        var baseName = Path.GetFileName(fileName);

        if (string.IsNullOrWhiteSpace(baseName))
        {
            return alternatives;
        }

        // Search in directory of .lxj project file
        if (!string.IsNullOrWhiteSpace(projectFilePath))
        {
            try
            {
                var projectDir = Path.GetDirectoryName(projectFilePath);
                if (!string.IsNullOrEmpty(projectDir) && Directory.Exists(projectDir))
                {
                    var candidatePath = Path.Combine(projectDir, baseName);
                    if (File.Exists(candidatePath))
                    {
                        alternatives.Add(candidatePath);
                    }

                    // Also check subdirectories (one level deep)
                    var subdirs = Directory.GetDirectories(projectDir);
                    foreach (var subdir in subdirs)
                    {
                        var subdirCandidate = Path.Combine(subdir, baseName);
                        if (File.Exists(subdirCandidate))
                        {
                            alternatives.Add(subdirCandidate);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException or
                                            ArgumentNullException or
                                            PathTooLongException or
                                            UnauthorizedAccessException or
                                            IOException)
            {
                // Ignore errors when searching in project directory
            }
        }

        // Search in Documents/LogExpert folder
        try
        {
            var documentsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "LogExpert");

            if (Directory.Exists(documentsPath))
            {
                var docCandidate = Path.Combine(documentsPath, baseName);
                if (File.Exists(docCandidate) && !alternatives.Contains(docCandidate))
                {
                    alternatives.Add(docCandidate);
                }
            }
        }
        catch (Exception ex) when (ex is ArgumentException or
                                        ArgumentNullException or
                                        PathTooLongException or
                                        UnauthorizedAccessException or
                                        IOException)
        {
            // Ignore errors when searching in Documents folder
        }

        // If the original path is absolute, try to find the file in the same directory structure
        // but on a different drive (useful when drive letters change)
        if (Path.IsPathRooted(fileName))
        {
            try
            {
                var driveLetters = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .Select(d => d.Name[0])
                    .ToList();

                var originalDrive = Path.GetPathRoot(fileName)?[0];
                var pathWithoutDrive = fileName.Length > 3 ? fileName[3..] : string.Empty;

                foreach (var drive in driveLetters)
                {
                    if (drive != originalDrive && !string.IsNullOrEmpty(pathWithoutDrive))
                    {
                        var alternatePath = $"{drive}:\\{pathWithoutDrive}";
                        if (File.Exists(alternatePath) && !alternatives.Contains(alternatePath))
                        {
                            alternatives.Add(alternatePath);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException or
                                            ArgumentNullException or
                                            PathTooLongException or
                                            UnauthorizedAccessException or
                                            IOException)
            {
                // Ignore errors when searching on different drives
            }
        }

        // Try relative path resolution from project directory
        if (!Path.IsPathRooted(fileName) && !string.IsNullOrWhiteSpace(projectFilePath))
        {
            try
            {
                var projectDir = Path.GetDirectoryName(projectFilePath);
                if (!string.IsNullOrEmpty(projectDir))
                {
                    var relativePath = Path.Combine(projectDir, fileName);
                    var normalizedPath = Path.GetFullPath(relativePath);

                    if (File.Exists(normalizedPath) && !alternatives.Contains(normalizedPath))
                    {
                        alternatives.Add(normalizedPath);
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException or
                                            ArgumentNullException or
                                            PathTooLongException or
                                            UnauthorizedAccessException or
                                            IOException or
                                            NotSupportedException)
            {
                // Ignore errors with relative path resolution
            }
        }

        return alternatives;
    }
}
