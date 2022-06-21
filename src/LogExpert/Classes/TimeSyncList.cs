using System;
using System.Collections.Generic;
using LogExpert.Controls.LogWindow;

namespace LogExpert.Classes
{
    /// <summary>
    /// Holds all windows which are in sync via timestamp
    /// </summary>
    public class TimeSyncList
    {
        #region Fields

        private readonly IList<LogWindow> logWindowList = new List<LogWindow>();

        #endregion

        #region Delegates

        public delegate void WindowRemovedEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event WindowRemovedEventHandler WindowRemoved;

        #endregion

        #region Properties

        public DateTime CurrentTimestamp { get; set; }

        public int Count
        {
            get { return this.logWindowList.Count; }
        }

        #endregion

        #region Public methods

        public void AddWindow(LogWindow logWindow)
        {
            lock (this.logWindowList)
            {
                if (!this.logWindowList.Contains(logWindow))
                {
                    this.logWindowList.Add(logWindow);
                }
            }
        }


        public void RemoveWindow(LogWindow logWindow)
        {
            lock (this.logWindowList)
            {
                this.logWindowList.Remove(logWindow);
            }
            OnWindowRemoved();
        }


        /// <summary>
        /// Scrolls all LogWindows to the given timestamp
        /// </summary>
        /// <param name="timestamp"></param>
        public void NavigateToTimestamp(DateTime timestamp, LogWindow sender)
        {
            this.CurrentTimestamp = timestamp;
            lock (this.logWindowList)
            {
                foreach (LogWindow logWindow in this.logWindowList)
                {
                    if (sender != logWindow)
                    {
                        logWindow.ScrollToTimestamp(timestamp, false, false);
                    }
                }
            }
        }


        public bool Contains(LogWindow logWindow)
        {
            return this.logWindowList.Contains(logWindow);
        }

        #endregion

        #region Private Methods

        private void OnWindowRemoved()
        {
            if (WindowRemoved != null)
            {
                WindowRemoved(this, new EventArgs());
            }
        }

        #endregion
    }
}