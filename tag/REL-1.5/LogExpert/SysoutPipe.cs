using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LogExpert
{
	class SysoutPipe
	{
		string fileName;
		StreamWriter writer;
		StreamReader sysout;

		public SysoutPipe(StreamReader sysout)
		{
			this.sysout = sysout;
			this.fileName = Path.GetTempFileName();
			Logger.logInfo("sysoutPipe created temp file: " + this.FileName);
			FileStream fStream = new FileStream(this.fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
			this.writer = new StreamWriter(fStream, Encoding.Unicode);
			Thread thread = new Thread(new ThreadStart(this.ReaderThread));
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
					int read = this.sysout.Read(buff, 0, 256);
					if (read == 0)
						break;
					writer.Write(buff, 0, read);
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
			this.writer.Close();
			this.writer = null;
		}

		public string FileName
		{
			get { return fileName; }
		}


		public void DataReceivedEventHandler(Object sender, DataReceivedEventArgs e)
		{
			this.writer.WriteLine(e.Data);
		}

		public void ProcessExitedEventHandler(object sender, System.EventArgs e)
		{
			//ClosePipe();
			if (sender.GetType() == typeof(Process))
			{
				((Process)sender).Exited -= this.ProcessExitedEventHandler;
				((Process)sender).OutputDataReceived -= this.DataReceivedEventHandler;
			}
		}
	}
}
