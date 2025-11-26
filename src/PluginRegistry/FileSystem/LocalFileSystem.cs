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