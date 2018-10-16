using System;
using System.Threading;
using System.Windows.Forms;

namespace LogExpert
{
    internal class LogTabPage : TabPage
    {
        #region Static/Constants

        private const int DIFF_MAX = 100;

        #endregion

        #region Private Fields

        private readonly object diffSumLock = new object();
        private readonly Thread ledThread;
        private int diffSum;
        private bool shouldStop;

        #endregion

        #region Ctor

        public LogTabPage(LogWindow logWindow, string title)
            : base("MMi" + (title == null ? Util.GetNameFromPath(logWindow.FileName) : title))
        {
            TabTitle = title;
            if (TabTitle == null)
            {
                TabTitle = Util.GetNameFromPath(logWindow.FileName);
            }

            LogWindow = logWindow;
            LogWindow.FileSizeChanged += FileSizeChanged;
            LogWindow.TailFollowed += TailFollowed;
            ledThread = new Thread(LedThreadProc);
            ledThread.IsBackground = true;
            ledThread.Start();
        }

        #endregion

        #region Properties / Indexers

        public bool Dirty { get; set; }

        public bool IsActiveTab { get; set; } = false;


        public int LineDiff
        {
            get
            {
                lock (diffSumLock)
                {
                    return diffSum;
                }
            }
        }

        public LogWindow LogWindow { get; }

        public string TabTitle { get; set; } = string.Empty;

        #endregion

        #region Public Methods

        public void Delete()
        {
            shouldStop = true;
            ledThread.Interrupt();
            ledThread.Join();
        }

        #endregion

        #region Private Methods

        private void FileSizeChanged(object sender, LogEventArgs e)
        {
            int diff = e.LineCount - e.PrevLineCount;
            if (diff < 0)
            {
                diff = DIFF_MAX;
                return;
            }

            lock (diffSumLock)
            {
                diffSum = diffSum + diff;
                if (diffSum > DIFF_MAX)
                {
                    diffSum = DIFF_MAX;
                }
            }

            Dirty = true;
            Parent.Invalidate();
        }

        private void LedThreadProc()
        {
            while (!shouldStop)
            {
                try
                {
                    Thread.Sleep(200);
                }
                catch (Exception)
                {
                    return;
                }

                lock (diffSumLock)
                {
                    if (diffSum > 0)
                    {
                        diffSum -= 10;
                        if (diffSum < 0)
                        {
                            diffSum = 0;
                        }

                        if (Parent != null)
                        {
                            Parent.Invalidate(); // redraw LEDs
                        }
                    }
                }
            }
        }

        private void TailFollowed(object sender, EventArgs e)
        {
            if (IsActiveTab)
            {
                Dirty = false;
                Parent.Invalidate();
            }
        }

        #endregion
    }
}
