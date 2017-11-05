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


namespace LogExpert
{
    [Obsolete]
    public class Logger
    {
        #region Fields

        private const int MAX_SIZE = 1024 * 1024 * 3;
        private const int MAX_FILES = 9;

        private static Logger instance = null;
        private readonly object lockObj = new object();

        private FileInfo fileInfo;

        private string fileName;
        private StreamWriter writer;

        #endregion

        #region cTor

        private Logger()
        {
            try
            {
                createLogFile();
            }
            catch (IOException)
            {
            }
        }

        #endregion

        #region Delegates

        public delegate void WriteToLogfileFx(string text);

        #endregion

        #region Properties

        public Level LogLevel { get; set; } = Level.INFO;

        public static bool IsDebug
        {
            get { return GetLogger().LogLevel <= Level.DEBUG; }
        }

        #endregion

        #region Public methods

        public static Logger GetLogger()
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }

        public static void Info(string msg)
        {
            GetLogger().log(msg, Level.INFO);
        }

        [Conditional("DEBUG")]
        public static void Debug(string msg)
        {
#if DEBUG
            GetLogger().log(msg, Level.DEBUG);
#endif
        }

        public static void Warn(string msg)
        {
            GetLogger().log(msg, Level.WARN);
        }

        public static void Warn(Exception e)
        {
            GetLogger().log(e.GetType().Name + ": " + e.Message, Level.WARN);
        }

        public static void Warn(string msg, Exception e)
        {
            GetLogger().log(msg + " " + e.GetType().Name + ": " + e.Message, Level.WARN);
        }

        public static void Error(string msg)
        {
            GetLogger().log(msg, Level.ERROR);
        }

        #endregion

        #region Private Methods

        private void createLogFile()
        {
#if DEBUG
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\LogExpert";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            this.fileName = dir + "\\logfile.txt";
            this.writer =
                new StreamWriter(new FileStream(this.fileName, FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Write));
            this.fileInfo = new FileInfo(this.fileName);
#endif
        }

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
                {
                }
            }
        }

        private void RolloverLogfile()
        {
            this.writer.Close();
            for (int i = MAX_FILES; i > 1; --i)
            {
                string currentName = this.fileName + "." + i;
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
                {
                }
            }
            File.Delete(this.fileName + ".1");
            File.Move(this.fileName, this.fileName + ".1");
            createLogFile();
        }

        #endregion

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
                string caller = string.Format("{0,-45}", callerInfo);
                if (caller.Length > 45)
                {
                    caller = caller.Substring(caller.Length - 45);
                }
                string threadIdStr = string.Format("{0,4}", threadId);
                string levelStr = string.Format("{0,5}", level.ToString());
                string output = dateTime.ToString("dd.MM.yyyy HH:mm:ss.fff [") + threadIdStr + "] [" + levelStr +
                                "] [" + caller + "] " + msg;

                WriteToLogfile(output);
            }
        }

        private delegate void LogCallback(string msg, Level level, DateTime dateTime, string callerInfo, int threadId);

        public enum Level
        {
            DEBUG = 0,
            INFO,
            WARN,
            ERROR
        };
    }
}