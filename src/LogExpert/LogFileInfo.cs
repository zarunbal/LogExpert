using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace LogExpert
{
  public class LogFileInfo : ILogFileInfo
  {
    const int RETRY_COUNT = 5;
    const int RETRY_SLEEP = 250;
    //FileStream fStream;
    FileInfo fInfo;
    private Uri fileUri;
    long originalLength;
    private long lastLength;


    public LogFileInfo(Uri fileUri)
    {
      this.fInfo = new FileInfo(fileUri.LocalPath);
      this.fileUri = fileUri;
      this.originalLength = lastLength = LengthWithoutRetry;
      //this.oldLength = 0;
    }

    /// <summary>
    /// Creates a new FileStream for the file. The caller is responsible for closing.
    /// If file opening fails it will be tried RETRY_COUNT times. This may be needed sometimes
    /// if the file is locked for a short amount of time or temporarly unaccessible because of
    /// rollover situations.
    /// </summary>
    /// <returns></returns>
    public Stream OpenStream()
    {
      int retry = RETRY_COUNT;
      while (true)
      {
        try
        {
          return new FileStream(this.fInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        }
        catch (IOException fe)
        {
          Logger.logDebug("LogFileInfo.OpenFile(): " + fe.ToString());
          Logger.logDebug("Retry counter: " + retry);
          if (--retry <= 0)
            throw fe;
          Thread.Sleep(RETRY_SLEEP);
        }
        catch (UnauthorizedAccessException uae)
        {
          Logger.logDebug("LogFileInfo.OpenFile(): " + uae.ToString());
          Logger.logDebug("Retry counter: " + retry);
          if (--retry <= 0)
            throw new IOException("Error opening file", uae);
          Thread.Sleep(RETRY_SLEEP);
        }
      }
    }

       
    public string FullName
    {
      get { return this.fInfo.FullName; }
    }

    public string FileName
    {
      get { return this.fInfo.Name; }
    }


    public string DirectoryName
    {
      get { return this.fInfo.DirectoryName; }
    }

    public char DirectorySeparatorChar
    {
      get { return Path.DirectorySeparatorChar; }
    }

    public Uri Uri
    {
      get { return this.fileUri; }
    }



    public long Length
    {
      get { 
        if (fInfo == null) return -1;
        int retry = RETRY_COUNT;
        while (retry > 0)
        {
          try
          {
            this.fInfo.Refresh();
            return this.fInfo.Length;
          }
          catch (IOException e)
          {
            if (--retry <= 0)
            {
              Logger.logWarn("LogFileInfo.Length: " + e.ToString());
              return -1;
            }
            Thread.Sleep(RETRY_SLEEP);
          }
        }
        return -1;
      }
    }

    public long OriginalLength
    {
      get { return this.originalLength; }
    }

    public bool FileExists 
    {
      get 
      { 
        this.fInfo.Refresh(); 
        return this.fInfo.Exists; 
      }
    }

    public int PollInterval
    {
      get { return ConfigManager.Settings.preferences.pollingInterval; }
    }

    public bool FileHasChanged()
    {
      if (this.LengthWithoutRetry != this.lastLength)
      {
        this.lastLength = this.LengthWithoutRetry;
        return true;
      }
      return false;
    }


    public override string ToString() 
    {
      return this.fInfo.FullName + ", OldLen: " + OriginalLength + ", Len: " + Length;
    }

    private static string GetNameFromPath(string fileName)
    {
      int i = fileName.LastIndexOf('\\');
      if (i < 0)
        i = -1;
      return fileName.Substring(i + 1);
    }

    public long LengthWithoutRetry
    {
      get
      {
        if (fInfo == null)
        {
          return -1;
        }
        try
        {
          this.fInfo.Refresh();
          return this.fInfo.Length;
        }
        catch (IOException)
        {
          return -1;
        }
      }
    }


  }
}
