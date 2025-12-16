using ColumnizerLib;

namespace LogExpert.PluginRegistry.FileSystem;

public class LocalFileSystem : IFileSystemPlugin
{
    #region IFileSystemPlugin Member

    public bool CanHandleUri (string uriString)
    {
        try
        {
            Uri uri = new(uriString);
            return uri.IsFile;
        }
        catch (Exception ex) when (ex is UriFormatException or
                                         ArgumentNullException or
                                         ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves information about a log file specified by a file URI.
    /// </summary>
    /// <param name="uriString">The URI string that identifies the log file. Must be a valid file URI.</param>
    /// <returns>An object that provides information about the specified log file.</returns>
    /// <exception cref="UriFormatException">Thrown if the provided URI string does not represent a file URI.</exception>
    public ILogFileInfo GetLogfileInfo (string uriString)
    {
        Uri uri = new(uriString);
        if (uri.IsFile)
        {
            ILogFileInfo logFileInfo = new LogFileInfo(uri);
            return logFileInfo;
        }
        else
        {
            throw new UriFormatException("Uri " + uriString + " is no file Uri");
        }
    }

    public string Text => "Local file system";

    public string Description => "Access files from normal file system.";

    #endregion
}