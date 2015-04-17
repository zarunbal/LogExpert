using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace LogExpert
{
	public class LogExpertProxy : MarshalByRefObject, ILogExpertProxy
	{
		#region Fields
		
		[NonSerialized]
		List<LogTabWindow> _windowList = new List<LogTabWindow>();
		[NonSerialized]
		LogTabWindow _firstLogTabWindow;
		[NonSerialized]
		int _logWindowIndex = 1; 
		
		#endregion
		
		#region cTor
		
		public LogExpertProxy(LogTabWindow logTabWindow)
		{
			AddWindow(logTabWindow);
			logTabWindow.LogExpertProxy = this;
			_firstLogTabWindow = logTabWindow;
		}
		
		#endregion
		
		#region Properties
		
		public int GetLogWindowCount()
		{
			return _windowList.Count;
		}
		
		#endregion
		
		#region Event
		
		public event Action LastWindowClosed;
		
		#endregion
		
		#region Overrides
		
		public override object InitializeLifetimeService()
		{
			return null;
		}
		
		#endregion
		
		#region Public Methods
		
		public void LoadFiles(string[] fileNames)
		{
			Logger.logInfo("Loading files into existing LogTabWindow");
			LogTabWindow logWin = _windowList[_windowList.Count - 1];
			logWin.Invoke(new MethodInvoker(logWin.SetForeground));
			logWin.LoadFiles(fileNames);
		}
		
		public void NewWindow(string[] fileNames)
		{
			if (_firstLogTabWindow.IsDisposed)
			{
				Logger.logWarn("first GUI thread window is disposed. Setting a new one.");
				// may occur if a window is closed because of unhandled exception.
				// Determine a new 'firstWindow'. If no window is left, start a new one.
				RemoveWindow(_firstLogTabWindow);
				if (_windowList.Count == 0)
				{
					Logger.logInfo("No windows left. New created window will be the new 'first' GUI window");
					LoadFiles(fileNames);
				}
				else
				{
					_firstLogTabWindow = _windowList[_windowList.Count - 1];
					NewWindow(fileNames);
				}
			}
			else
			{
				_firstLogTabWindow.Invoke(new Action<string[]>(NewWindowWorker), new object[] { fileNames });
			}
		}
		
		public void NewWindowOrLockedWindow(string[] fileNames)
		{
			foreach (LogTabWindow logWin in _windowList)
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
			LogTabWindow logWin = new LogTabWindow(fileNames.Length > 0 ? fileNames : null, _logWindowIndex++, true);
			logWin.LogExpertProxy = this;
			AddWindow(logWin);
			logWin.Show();
			logWin.Activate();
		}
		
		public void WindowClosed(LogTabWindow logWin)
		{
			RemoveWindow(logWin);
			if (_windowList.Count == 0)
			{
				Logger.logInfo("Last LogTabWindow was closed");
				PluginRegistry.GetInstance().CleanupPlugins();
				OnLastWindowClosed();
			}
			else
			{
				if (_firstLogTabWindow == logWin)
				{
					// valid firstLogTabWindow is needed for the Invoke()-Calls in NewWindow()
					_firstLogTabWindow = _windowList[_windowList.Count - 1];
				}
			}
		}
		
		#endregion
		
		#region Private Methods
		
		private void AddWindow(LogTabWindow window)
		{
			Logger.logInfo("Adding window to list");
			_windowList.Add(window);
		}
		
		private void RemoveWindow(LogTabWindow window)
		{
			Logger.logInfo("Removing window from list");
			_windowList.Remove(window);
		}
		
		private void OnLastWindowClosed()
		{
			if (LastWindowClosed != null)
			{
				LastWindowClosed();
			}
		}
	
		#endregion
	}
}