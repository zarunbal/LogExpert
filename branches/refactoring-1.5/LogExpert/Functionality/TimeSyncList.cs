using System;
using System.Collections.Generic;

namespace LogExpert
{
	/// <summary>
	/// Holds all windows which are in sync via timestamp
	/// </summary>
	public class TimeSyncList
	{
		private IList<LogWindow> _logWindowList = new List<LogWindow>();

		public DateTime CurrentTimestamp { get; set; }

		public void AddWindow(LogWindow logWindow)
		{
			lock (_logWindowList)
			{
				if (!_logWindowList.Contains(logWindow))
				{
					_logWindowList.Add(logWindow);
				}
			}
		}

		public void RemoveWindow(LogWindow logWindow)
		{
			lock (_logWindowList)
			{
				_logWindowList.Remove(logWindow);
			}
			OnWindowRemoved();
		}

		/// <summary>
		/// Scrolls all LogWindows to the given timestamp
		/// </summary>
		/// <param name="timestamp"></param>
		public void NavigateToTimestamp(DateTime timestamp, LogWindow sender)
		{
			CurrentTimestamp = timestamp;
			lock (_logWindowList)
			{
				foreach (LogWindow logWindow in _logWindowList)
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
			return _logWindowList.Contains(logWindow);
		}

		public int Count
		{
			get
			{
				return _logWindowList.Count;
			}
		}

		public delegate void WindowRemovedEventHandler(object sender, EventArgs e);

		public event WindowRemovedEventHandler WindowRemoved;

		private void OnWindowRemoved()
		{
			if (WindowRemoved != null)
				WindowRemoved(this, new EventArgs());
		}
	}
}