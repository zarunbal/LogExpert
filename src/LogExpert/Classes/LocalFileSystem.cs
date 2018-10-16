using System;

namespace LogExpert
{
    public class LocalFileSystem : IFileSystemPlugin
    {
        #region Interface IFileSystemPlugin

        public string Description => "Access files from normal file system.";

        public string Text => "Local file system";

        public bool CanHandleUri(string uriString)
        {
            try
            {
                Uri uri = new Uri(uriString);
                return uri.IsFile;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ILogFileInfo GetLogfileInfo(string uriString)
        {
            Uri uri = new Uri(uriString);
            if (uri.IsFile)
            {
                ILogFileInfo logFileInfo = new LogFileInfo(uri);
                return logFileInfo;
            }

            throw new UriFormatException("Uri " + uriString + " is no file Uri");
        }

        #endregion
    }
}
