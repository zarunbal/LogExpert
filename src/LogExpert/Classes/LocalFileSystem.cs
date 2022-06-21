using System;
using LogExpert.Classes.Log;

namespace LogExpert.Classes
{
    public class LocalFileSystem : IFileSystemPlugin
    {
        #region IFileSystemPlugin Member

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
            else
            {
                throw new UriFormatException("Uri " + uriString + " is no file Uri");
            }
        }

        public string Text
        {
            get { return "Local file system"; }
        }

        public string Description
        {
            get { return "Access files from normal file system."; }
        }

        #endregion
    }
}