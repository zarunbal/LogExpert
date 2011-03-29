using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class LocalFileSystem : IFileSystemPlugin
  {

    #region IFileSystemPlugin Member

    public bool CanHandleUri(string uriString)
    {
      try
      {
        Uri uri = new Uri(uriString);
        return (uri.IsFile && uri.Host.Length == 0);
      }
      catch (Exception e)
      {
        return false;
      }
    }

    public ILogFileInfo GetLogfileInfo(string uriString)
    {
      string fileName = BuildFileNameFromUri(uriString);
      ILogFileInfo logFileInfo = new LogFileInfo(fileName);
      return logFileInfo;
    }

    public string Text { get {return "Local file system"; } }

    public string Description { get { return "Access files from normal file system."; } }

    #endregion


    private string BuildFileNameFromUri(string uriString)
    {
      Uri uri = new Uri(uriString);
      if (uri.IsFile && uri.Host.Length == 0)
      {
        string path = uri.PathAndQuery;
        if (path.Contains("?"))
        {
          path = path.Substring(0, path.IndexOf("?"));
        }
        return Uri.UnescapeDataString(path);
      }
      else
      {
        throw new UriFormatException("Uri " + uriString + " is no file Uri");
      }
    }

  }
}
