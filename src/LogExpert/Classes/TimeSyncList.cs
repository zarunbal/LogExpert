using System;
using System.Collections.Generic;

namespace LogExpert
{
    /// <summary>
    ///     Holds all windows which are in sync via timestamp
    /// </summary>
    public class TimeSyncList
    {
        #region Delegates

        public delegate void WindowRemovedEventHandler(object sender, EventArgs e);

        #endregion

        #region Private Fields

        private readonly IList<LogWindow> logWindowList = new List<LogWindow>();

        #endregion

        #region Public Events

        public event WindowRemovedEventHandler WindowRemoved;

        #endregion

        #region Properties / Indexers

        public int Count => logWindowList.Count;

        public DateTime CurrentTimestamp { get; set; }

        #endregion

        #region Public Methods

        public void AddWindow(LogWindow logWindow)
        {
            lock (logWindowList)
            {
                if (!logWindowList.Contains(logWindow))
                {
                    logWindowList.Add(logWindow);
                }
            }
        }


        public bool Contains(LogWindow logWindow)
        {
            return logWindowList.Contains(logWindow);
        }


        /// <summary>
        ///     Scrolls all LogWindows to the given timestamp
        /// </summary>
        /// <param name="timestamp"></param>
        public void NavigateToTimestamp(DateTime timestamp, LogWindow sender)
        {
            CurrentTimestamp = timestamp;
            lock (logWindowList)
            {
                foreach (LogWindow logWindow in logWindowList)
                {
                    if (sender != logWindow)
                    {
                        logWindow.ScrollToTimestamp(timestamp, false, false);
                    }
                }
            }
        }


        public void RemoveWindow(LogWindow logWindow)
        {
            lock (logWindowList)
            {
                logWindowList.Remove(logWindow);
            }

            OnWindowRemoved();
        }

        #endregion

        #region Event handling Methods

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
