using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using NLog;

namespace LogExpert.Classes
{
    internal class SysoutPipe
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly StreamReader sysout;
        private StreamWriter writer;

        #endregion

        #region cTor

        public SysoutPipe(StreamReader sysout)
        {
            this.sysout = sysout;
            this.FileName = Path.GetTempFileName();
            _logger.Info("sysoutPipe created temp file: {0}", this.FileName);
            FileStream fStream = new FileStream(this.FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            this.writer = new StreamWriter(fStream, Encoding.Unicode);
            Thread thread = new Thread(new ThreadStart(this.ReaderThread));
            thread.IsBackground = true;
            thread.Start();
        }

        #endregion

        #region Properties

        public string FileName { get; }

        #endregion

        #region Public methods

        public void ClosePipe()
        {
            this.writer.Close();
            this.writer = null;
        }


        public void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            this.writer.WriteLine(e.Data);
        }

        public void ProcessExitedEventHandler(object sender, System.EventArgs e)
        {
            //ClosePipe();
            if (sender.GetType() == typeof(Process))
            {
                ((Process) sender).Exited -= this.ProcessExitedEventHandler;
                ((Process) sender).OutputDataReceived -= this.DataReceivedEventHandler;
            }
        }

        #endregion

        protected void ReaderThread()
        {
            char[] buff = new char[256];
            while (true)
            {
                try
                {
                    int read = this.sysout.Read(buff, 0, 256);
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
    }
}