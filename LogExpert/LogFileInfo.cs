using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace LogExpert
{
  public class LogFileInfo : ILogFileInfo
  {
    const int RETRY_COUNT = 20;
    const int RETRY_SLEEP = 250;
    //FileStream fStream;
    FileInfo fInfo;
    string fileName;
    long oldLength;


    public LogFileInfo(string fileName)
    {
      this.fileName = fileName;
      this.fInfo = new FileInfo(this.fileName);
      this.oldLength = Length;
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
          return new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
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


    //public void CloseFile()
    //{
    //  if (this.fStream != null)
    //    this.fStream.Close();
    //}

    //public FileStream FileStream
    //{
    //  get { return this.fStream; }
    //}

    public string FileName
    {
      get { return this.fileName; }
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

    public long OldLength
    {
      get { return this.oldLength; }
    }

    public override string ToString() 
    {
      return this.fileName + ", OldLen: " + OldLength + ", Len: " + Length;
    }
  }
}
