using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using ColumnizerLib;


namespace LogExpert
{
  public class Logger : ILogExpertLogger
  {
    delegate void LogCallback(string msg, Level level, DateTime dateTime, string callerInfo, int threadId);

    public enum Level
    {
      DEBUG = 0,
      INFO,
      WARN,
      ERROR
    };

    const int MAX_SIZE = 1024*1024*3;
    const int MAX_FILES = 9;

    private Level currLevel = Level.INFO;

    String fileName;
    FileInfo fileInfo;
    StreamWriter writer;
    Object lockObj = new Object();

    private static Logger instance = null;

    public Level LogLevel
    {
      get { return this.currLevel; }
      set { this.currLevel = value; }
    }


    public static Logger GetLogger()
    {
      if (instance == null)
        instance = new Logger();
      return instance;
    }

    private Logger()
    {
      try
      {
        createLogFile();
      }
      catch (IOException)
      { }
    }

    private void createLogFile()
    {
#if DEBUG
      string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\LogExpert";
      if (!Directory.Exists(dir))
      {
        Directory.CreateDirectory(dir);
      }
      this.fileName = dir + "\\logfile.txt";
      this.writer = new StreamWriter(new FileStream(this.fileName, FileMode.Append, FileAccess.Write, FileShare.Read|FileShare.Write));
      this.fileInfo = new FileInfo(this.fileName);
#endif
    }

    public static void logInfo(string msg)
    {
      GetLogger().log(msg, Level.INFO);
    }

    [Conditional("DEBUG")]
    public static void logDebug(string msg)
    {
#if DEBUG
      GetLogger().log(msg, Level.DEBUG);
#endif
    }

    public static void logWarn(string msg)
    {
      GetLogger().log(msg, Level.WARN);
    }

    public static void logWarn(Exception e)
    {
      GetLogger().log(e.GetType().Name + ": " + e.Message, Level.WARN);
    }

    public static void logWarn(string msg, Exception e)
    {
      GetLogger().log(msg + " " + e.GetType().Name + ": " + e.Message, Level.WARN);
    }

    public static void logError(string msg)
    {
      GetLogger().log(msg, Level.ERROR);
    }

    public static bool IsDebug
    { 
      get {return GetLogger().LogLevel <= Level.DEBUG;}
    }

    protected void log(string msg, Level level)
    {
#if DEBUG
      StackTrace st = new StackTrace();
      StackFrame callerFrame = st.GetFrame(2);
      string callerInfo = callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "()";
      LogCallback callback = new LogCallback(logCallback);
      callback.BeginInvoke(msg, level, DateTime.Now, callerInfo, Thread.CurrentThread.ManagedThreadId, null, null);
#endif
    }

    protected void logCallback(string msg, Level level, DateTime dateTime, string callerInfo, int threadId)
    {
      if (level >= this.LogLevel)
      {
        String caller = String.Format("{0,-45}", callerInfo);
        if (caller.Length > 45)
        {
          caller = caller.Substring(caller.Length - 45);
        }
        String threadIdStr = String.Format("{0,4}", threadId);
        String levelStr = String.Format("{0,5}", level.ToString());
        string output = dateTime.ToString("dd.MM.yyyy HH:mm:ss.fff [") + threadIdStr + "] [" + levelStr + "] [" + caller + "] " + msg;

        WriteToLogfile(output);
      }
    }

    public delegate void WriteToLogfileFx(string text); 
    private void WriteToLogfile(string text)
    {
      lock (this.lockObj)
      {
        try
        {
          this.writer.WriteLine(text);
          this.writer.Flush();
          this.fileInfo.Refresh();
          if (this.fileInfo.Length > MAX_SIZE)
          {
            RolloverLogfile();
          }
        }
        catch (Exception)
        { }
      }
    }

    private void RolloverLogfile()
    {
      this.writer.Close();
      for (int i = MAX_FILES; i > 1; --i)
      {
        string currentName = this.fileName + "." +i;
        string nextName = this.fileName + "." + (i - 1);
        try
        {
          File.Delete(currentName);
          if (File.Exists(nextName))
          {
            File.Move(nextName, currentName);
          }
        }
        catch (IOException)
        { }
      }
      File.Delete(this.fileName + ".1");
      File.Move(this.fileName, this.fileName + ".1");
      createLogFile();
    }


    #region ILogExpertLogger Member

    public void LogInfo(string msg)
    {
      GetLogger().log(msg, Level.INFO);
    }

    public void LogDebug(string msg)
    {
      GetLogger().log(msg, Level.DEBUG);
    }

    public void LogWarn(string msg)
    {
      GetLogger().log(msg, Level.WARN);
    }

    public void LogError(string msg)
    {
      GetLogger().log(msg, Level.ERROR);
    }

    #endregion
  }
}
