using System;
using System.Collections.Generic;


namespace LogExpert
{
  /// <summary>
  /// Holds all windows which are in sync via timestamp
  /// </summary>
  public class TimeSyncList
  {
    private IList<LogWindow>   logWindowList = new List<LogWindow>();
    private DateTime  currentTimestamp;

    public DateTime CurrentTimestamp
    {
      get { return this.currentTimestamp; }
      set { this.currentTimestamp = value; }
    }


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
      this.currentTimestamp = timestamp;
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

    public int Count
    {
      get { return this.logWindowList.Count; }
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