using System;
using System.Threading;
using System.Windows.Forms;
using LogExpert.Classes;
using LogExpert.Entities;

namespace LogExpert.Controls
{
    internal class LogTabPage : TabPage
    {
        #region Fields

        private const int DIFF_MAX = 100;
        private int diffSum = 0;
        private readonly object diffSumLock = new object();
        private readonly Thread ledThread;
        private bool shouldStop = false;

        #endregion

        #region cTor

        public LogTabPage(LogWindow.LogWindow logWindow, string title)
            : base("MMi" + (title == null ? Util.GetNameFromPath(logWindow.FileName) : title))
        {
            this.TabTitle = title;
            if (this.TabTitle == null)
            {
                this.TabTitle = Util.GetNameFromPath(logWindow.FileName);
            }
            this.LogWindow = logWindow;
            this.LogWindow.FileSizeChanged += FileSizeChanged;
            this.LogWindow.TailFollowed += TailFollowed;
            this.ledThread = new Thread(new ThreadStart(this.LedThreadProc));
            this.ledThread.IsBackground = true;
            this.ledThread.Start();
        }

        #endregion

        #region Properties

        public LogWindow.LogWindow LogWindow { get; }


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

        public bool Dirty { get; set; } = false;

        public bool IsActiveTab { get; set; } = false;

        public string TabTitle { get; set; } = "";

        #endregion

        #region Public methods

        public void Delete()
        {
            this.shouldStop = true;
            this.ledThread.Interrupt();
            this.ledThread.Join();
        }

        #endregion

        #region Private Methods

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
                        {
                            Parent.Invalidate(); // redraw LEDs
                        }
                    }
                }
            }
        }

        #endregion

        #region Events handler

        private void FileSizeChanged(object sender, LogEventArgs e)
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
                {
                    this.diffSum = DIFF_MAX;
                }
            }
            Dirty = true;
            Parent.Invalidate();
        }

        private void TailFollowed(object sender, EventArgs e)
        {
            if (this.IsActiveTab)
            {
                Dirty = false;
                Parent.Invalidate();
            }
        }

        #endregion
    }
}