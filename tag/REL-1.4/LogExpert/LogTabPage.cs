using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace LogExpert
{
	class LogTabPage : TabPage
	{
		LogWindow logWindow;
		Thread ledThread;
		bool shouldStop = false;
		int diffSum = 0;
		object diffSumLock = new Object();
		bool dirty = false;
		bool isActiveTab = false;
		string title = "";

		const int DIFF_MAX = 100;

		public LogTabPage(LogWindow logWindow, String title)
			: base("MMi" + (title == null ? Util.GetNameFromPath(logWindow.FileName) : title))
		{
			this.title = title;
			if (this.title == null)
				this.title = Util.GetNameFromPath(logWindow.FileName);
			this.logWindow = logWindow;
			this.logWindow.FileSizeChanged += FileSizeChanged;
			this.logWindow.TailFollowed += TailFollowed;
			this.ledThread = new Thread(new ThreadStart(this.LedThreadProc));
			this.ledThread.IsBackground = true;
			this.ledThread.Start();
		}

		public void Delete()
		{
			this.shouldStop = true;
			this.ledThread.Interrupt();
			this.ledThread.Join();
		}

		public LogWindow LogWindow
		{
			get { return this.logWindow; }
		}

		void FileSizeChanged(object sender, LogEventArgs e)
		{
			int diff = e.LineCount - e.PrevLineCount;
			if (diff < 0)
			{
				diff = DIFF_MAX;
				return;
			}
			lock (this.diffSumLock)
			{
				this.diffSum = this.diffSum + diff;
				if (this.diffSum > DIFF_MAX)
					this.diffSum = DIFF_MAX;
			}
			Dirty = true;
			Parent.Invalidate();
		}

		void TailFollowed(object sender, EventArgs e)
		{
			if (this.IsActiveTab)
			{
				Dirty = false;
				Parent.Invalidate();
			}
		}


		public int LineDiff
		{
			get
			{
				lock (this.diffSumLock)
				{
					return this.diffSum;
				}
			}
		}

		private void LedThreadProc()
		{
			while (!this.shouldStop)
			{
				try
				{
					Thread.Sleep(200);
				}
				catch (Exception)
				{
					return;
				}
				lock (this.diffSumLock)
				{
					if (this.diffSum > 0)
					{
						this.diffSum -= 10;
						if (this.diffSum < 0)
						{
							this.diffSum = 0;
						}
						if (Parent != null)
							Parent.Invalidate();    // redraw LEDs
					}
				}
			}
		}

		public bool Dirty
		{
			get { return dirty; }
			set { dirty = value; }
		}

		public bool IsActiveTab
		{
			get { return isActiveTab; }
			set { isActiveTab = value; }
		}

		public string TabTitle
		{
			get { return title; }
			set { title = value; }
		}
	}
}
