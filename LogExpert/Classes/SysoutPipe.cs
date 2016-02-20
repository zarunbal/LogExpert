using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LogExpert
{
	public class SysoutPipe
	{
		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private StreamWriter _writer;
		private StreamReader _sysout;

		public SysoutPipe(StreamReader sysout)
		{
			_sysout = sysout;
			FileName = Path.GetTempFileName();
			_logger.Info("sysoutPipe created temp file: " + this.FileName);
			FileStream fStream = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
			_writer = new StreamWriter(fStream, Encoding.Unicode);
			Thread thread = new Thread(ReaderThread);
			thread.IsBackground = true;
			thread.Start();
		}

		protected void ReaderThread()
		{
			char[] buff = new char[256];
			while (true)
			{
				try
				{
					int read = _sysout.Read(buff, 0, 256);
					if (read == 0)
					{
						break;
					}
					_writer.Write(buff, 0, read);
				}
				catch (IOException)
				{
					break;
				}
			}
			ClosePipe();
		}

		public void ClosePipe()
		{
			_writer.Close();
			_writer = null;
		}

		public string FileName { get; private set; }

		public void DataReceivedEventHandler(Object sender, DataReceivedEventArgs e)
		{
			_writer.WriteLine(e.Data);
		}

		public void ProcessExitedEventHandler(object sender, System.EventArgs e)
		{
			//ClosePipe();
			if (sender.GetType() == typeof(Process))
			{
				((Process)sender).Exited -= ProcessExitedEventHandler;
				((Process)sender).OutputDataReceived -= DataReceivedEventHandler;
			}
		}
	}
}