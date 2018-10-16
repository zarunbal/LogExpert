using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using NLog;

namespace LogExpert
{
    internal class SysoutPipe
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        private readonly StreamReader sysout;
        private StreamWriter writer;

        #endregion

        #region Ctor

        public SysoutPipe(StreamReader sysout)
        {
            this.sysout = sysout;
            FileName = Path.GetTempFileName();
            _logger.Info("sysoutPipe created temp file: {0}", FileName);
            FileStream fStream = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(fStream, Encoding.Unicode);
            Thread thread = new Thread(ReaderThread);
            thread.IsBackground = true;
            thread.Start();
        }

        #endregion

        #region Properties / Indexers

        public string FileName { get; }

        #endregion

        #region Public Methods

        public void ClosePipe()
        {
            writer.Close();
            writer = null;
        }


        public void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            writer.WriteLine(e.Data);
        }

        public void ProcessExitedEventHandler(object sender, EventArgs e)
        {
            // ClosePipe();
            if (sender.GetType() == typeof(Process))
            {
                ((Process)sender).Exited -= ProcessExitedEventHandler;
                ((Process)sender).OutputDataReceived -= DataReceivedEventHandler;
            }
        }

        #endregion

        #region Private Methods

        protected void ReaderThread()
        {
            char[] buff = new char[256];
            while (true)
            {
                try
                {
                    int read = sysout.Read(buff, 0, 256);
                    if (read == 0)
                    {
                        break;
                    }

                    writer.Write(buff, 0, read);
                }
                catch (IOException e)
                {
                    _logger.Error(e);
                    break;
                }
            }

            ClosePipe();
        }

        #endregion
    }
}
