using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace LogExpert
{
  class LogExpertProxy : MarshalByRefObject, ILogExpertProxy
  {
    delegate void NewWindowFx(string[] fileNames);

    [NonSerialized]
    List<LogTabWindow> windowList = new List<LogTabWindow>();
    [NonSerialized]
    LogTabWindow firstLogTabWindow;
    [NonSerialized]
    int logWindowIndex = 1;

    public LogExpertProxy(LogTabWindow logTabWindow)
    {
      AddWindow(logTabWindow);
      logTabWindow.LogExpertProxy = this;
      firstLogTabWindow = logTabWindow;
    }

    public void LoadFiles(string[] fileNames)
    {
      Logger.logInfo("Loading files into existing LogTabWindow");
      LogTabWindow logWin = this.windowList[this.windowList.Count - 1];
      logWin.Invoke(new MethodInvoker(logWin.SetForeground));
      logWin.LoadFiles(fileNames);
    }

    public void NewWindow(string[] fileNames)
    {
      if (this.firstLogTabWindow.IsDisposed)
      {
        Logger.logWarn("first GUI thread window is disposed. Setting a new one.");
        // may occur if a window is closed because of unhandled exception.
        // Determine a new 'firstWindow'. If no window is left, start a new one.
        RemoveWindow(this.firstLogTabWindow);
        if (this.windowList.Count == 0)
        {
          Logger.logInfo("No windows left. New created window will be the new 'first' GUI window");
          LoadFiles(fileNames);
        }
        else
        {
          this.firstLogTabWindow = this.windowList[this.windowList.Count - 1];
          NewWindow(fileNames);
        }
      }
      else
      {
        this.firstLogTabWindow.Invoke(new NewWindowFx(NewWindowWorker), new object[] { fileNames });
      }
    }

    public void NewWindowOrLockedWindow(string[] fileNames)
    {
      foreach (LogTabWindow logWin in this.windowList)
      {
        if (LogTabWindow.StaticData.CurrentLockedMainWindow == logWin)
        {
          logWin.Invoke(new MethodInvoker(logWin.SetForeground));
          logWin.LoadFiles(fileNames);
          return;
        }
      }
      // No locked window was found --> create a new one
      NewWindow(fileNames);
    }



    public void NewWindowWorker(string[] fileNames)
    {
      Logger.logInfo("Creating new LogTabWindow");
      LogTabWindow logWin = new LogTabWindow(fileNames.Length > 0 ? fileNames : null, logWindowIndex++, true);
      logWin.LogExpertProxy = this;
      AddWindow(logWin);
      logWin.Show();
      logWin.Activate();
    }


    public void WindowClosed(LogTabWindow logWin)
    {
      RemoveWindow(logWin);
      if (this.windowList.Count == 0)
      {
        Logger.logInfo("Last LogTabWindow was closed");
        PluginRegistry.GetInstance().CleanupPlugins();
        OnLastWindowClosed();
      }
      else
      {
        if (this.firstLogTabWindow == logWin)
        {
          // valid firstLogTabWindow is needed for the Invoke()-Calls in NewWindow()
          this.firstLogTabWindow = this.windowList[this.windowList.Count - 1];
        }
      }
    }

    private void AddWindow(LogTabWindow window)
    {
      Logger.logInfo("Adding window to list");
      this.windowList.Add(window);
    }

    private void RemoveWindow(LogTabWindow window)
    {
      Logger.logInfo("Removing window from list");
      this.windowList.Remove(window);
    }

    public int GetLogWindowCount()
    {
      return this.windowList.Count;
    }

    //public void BroadcastSettingsChanged(Object cookie)
    //{
    //  lock (this.windowList)
    //  {
    //    foreach (LogTabWindow logTabWindow in this.windowList)
    //    {
    //      logTabWindow.NotifySettingsChanged(cookie);
    //    }
    //  }
    //}


    public delegate void LastWindowClosedEventHandler(object sender, EventArgs e);
    public event LastWindowClosedEventHandler LastWindowClosed;
    protected void OnLastWindowClosed()
    {
      if (LastWindowClosed != null)
      {
        LastWindowClosed(this, new EventArgs());
      }
    }
    public override object InitializeLifetimeService()
    {
      return null;
    }


  }
}
