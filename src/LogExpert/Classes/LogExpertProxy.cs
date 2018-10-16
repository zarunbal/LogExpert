using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NLog;

namespace LogExpert
{
    internal class LogExpertProxy : MarshalByRefObject, ILogExpertProxy
    {
        #region Delegates

        // public void BroadcastSettingsChanged(Object cookie)
        // {
        // lock (this.windowList)
        // {
        // foreach (LogTabWindow logTabWindow in this.windowList)
        // {
        // logTabWindow.NotifySettingsChanged(cookie);
        // }
        // }
        // }
        public delegate void LastWindowClosedEventHandler(object sender, EventArgs e);

        private delegate void NewWindowFx(string[] fileNames);

        #endregion

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        [NonSerialized] private readonly List<LogTabWindow> windowList = new List<LogTabWindow>();

        [NonSerialized] private LogTabWindow firstLogTabWindow;

        [NonSerialized] private int logWindowIndex = 1;

        #endregion

        #region Public Events

        public event LastWindowClosedEventHandler LastWindowClosed;

        #endregion

        #region Ctor

        public LogExpertProxy(LogTabWindow logTabWindow)
        {
            AddWindow(logTabWindow);
            logTabWindow.LogExpertProxy = this;
            firstLogTabWindow = logTabWindow;
        }

        #endregion

        #region Interface ILogExpertProxy

        public int GetLogWindowCount()
        {
            return windowList.Count;
        }

        public void LoadFiles(string[] fileNames)
        {
            _logger.Info("Loading files into existing LogTabWindow");
            LogTabWindow logWin = windowList[windowList.Count - 1];
            logWin.Invoke(new MethodInvoker(logWin.SetForeground));
            logWin.LoadFiles(fileNames);
        }

        public void NewWindow(string[] fileNames)
        {
            if (firstLogTabWindow.IsDisposed)
            {
                _logger.Warn("first GUI thread window is disposed. Setting a new one.");


// may occur if a window is closed because of unhandled exception.
                // Determine a new 'firstWindow'. If no window is left, start a new one.
                RemoveWindow(firstLogTabWindow);
                if (windowList.Count == 0)
                {
                    _logger.Info("No windows left. New created window will be the new 'first' GUI window");
                    LoadFiles(fileNames);
                }
                else
                {
                    firstLogTabWindow = windowList[windowList.Count - 1];
                    NewWindow(fileNames);
                }
            }
            else
            {
                firstLogTabWindow.Invoke(new NewWindowFx(NewWindowWorker), new object[] {fileNames});
            }
        }

        public void NewWindowOrLockedWindow(string[] fileNames)
        {
            foreach (LogTabWindow logWin in windowList)
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


        public void WindowClosed(LogTabWindow logWin)
        {
            RemoveWindow(logWin);
            if (windowList.Count == 0)
            {
                _logger.Info("Last LogTabWindow was closed");
                PluginRegistry.GetInstance().CleanupPlugins();
                OnLastWindowClosed();
            }
            else
            {
                if (firstLogTabWindow == logWin)
                {
                    // valid firstLogTabWindow is needed for the Invoke()-Calls in NewWindow()
                    firstLogTabWindow = windowList[windowList.Count - 1];
                }
            }
        }

        #endregion

        #region Public Methods

        public void NewWindowWorker(string[] fileNames)
        {
            _logger.Info("Creating new LogTabWindow");
            LogTabWindow logWin = new LogTabWindow(fileNames.Length > 0 ? fileNames : null, logWindowIndex++, true);
            logWin.LogExpertProxy = this;
            AddWindow(logWin);
            logWin.Show();
            logWin.Activate();
        }

        #endregion

        #region Overrides

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Event handling Methods

        protected void OnLastWindowClosed()
        {
            if (LastWindowClosed != null)
            {
                LastWindowClosed(this, new EventArgs());
            }
        }

        #endregion

        #region Private Methods

        private void AddWindow(LogTabWindow window)
        {
            _logger.Info("Adding window to list");
            windowList.Add(window);
        }

        private void RemoveWindow(LogTabWindow window)
        {
            _logger.Info("Removing window from list");
            windowList.Remove(window);
        }

        #endregion
    }
}
