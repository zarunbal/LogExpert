using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.IO;
using System.Diagnostics;
using System.Security;
using System.Reflection;
using System.Security.Principal;
using LogExpert.Dialogs;

namespace LogExpert
{
	internal static class Program
	{
		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] orgArgs)
		{
			try
			{
#if DEBUG
				if (!Debugger.IsAttached)
				{
					Debugger.Break();
				}
#endif
				Sub_Main(orgArgs);
			}
			catch (SecurityException se)
			{
				MessageBox.Show("Insufficient system rights for LogExpert. Maybe you have started it from a network drive. Please start LogExpert from a local drive.\n(" + se.Message + ")", "LogExpert Error");
			}
		}

		private static void Sub_Main(string[] orgArgs)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			Exten.Info(_logger, "============================================================================");
			Exten.Info(_logger, "LogExpert " + Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
						   Assembly.GetExecutingAssembly().GetName().Version.Minor + "/" +
						   Assembly.GetExecutingAssembly().GetName().Version.Build.ToString() +
						   " started.");
			Exten.Info(_logger, "============================================================================");

			List<string> argsList = new List<string>();
			foreach (string fileArg in orgArgs)
			{
				try
				{
					FileInfo info = new FileInfo(fileArg);
					if (info.Exists)
					{
						argsList.Add(info.FullName);
					}
					else
					{
						argsList.Add(fileArg);
					}
				}
				catch (Exception)
				{
					MessageBox.Show("File name " + fileArg + " is not a valid file name!", "LogExpert Error");
				}
			}
			string[] args = argsList.ToArray();

			int pId = Process.GetCurrentProcess().SessionId;

			try
			{
				Settings settings = ConfigManager.Settings;
				bool isCreated = false;
				Mutex mutex = new System.Threading.Mutex(false, "Local\\LogExpertInstanceMutex" + pId, out isCreated);
				if (isCreated)
				{
					// first application instance
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					LogTabWindow logWin = new LogTabWindow(args.Length > 0 ? args : null, 1, false);

					// first instance
					//WindowsIdentity wi = WindowsIdentity.GetCurrent();
					IpcServerChannel ipcChannel = new IpcServerChannel("LogExpert" + pId);
					ChannelServices.RegisterChannel(ipcChannel, false);
					RemotingConfiguration.RegisterWellKnownServiceType(typeof(LogExpertProxy),
						"LogExpertProxy",
						WellKnownObjectMode.Singleton);
					LogExpertProxy proxy = new LogExpertProxy(logWin);
					RemotingServices.Marshal(proxy, "LogExpertProxy");

					LogExpertApplicationContext context = new LogExpertApplicationContext(proxy, logWin);
					Application.Run(context);

					ChannelServices.UnregisterChannel(ipcChannel);
				}
				else
				{
					int counter = 3;
					string errMsg = "";
					IpcClientChannel ipcChannel = new IpcClientChannel("LogExpertClient#" + pId, null);
					ChannelServices.RegisterChannel(ipcChannel, false);
					while (counter > 0)
					{
						try
						{
							// another instance already exists
							//WindowsIdentity wi = WindowsIdentity.GetCurrent();
							LogExpertProxy proxy = (LogExpertProxy)Activator.GetObject(typeof(LogExpertProxy),
								"ipc://LogExpert" + pId + "/LogExpertProxy");
							if (settings.preferences.allowOnlyOneInstance)
							{
								proxy.LoadFiles(args);
							}
							else
							{
								proxy.NewWindowOrLockedWindow(args);
							}
							break;
						}
						catch (RemotingException e)
						{
							_logger.logError("IpcClientChannel error: " + e.Message);
							errMsg = e.Message;
							counter--;
							Thread.Sleep(500);
						}
					}
					if (counter == 0)
					{
						_logger.logError("IpcClientChannel error, giving up: " + errMsg);
						MessageBox.Show("Cannot open connection to first instance (" + errMsg + ")", "LogExpert");
					}
				}
				mutex.Close();
			}
			catch (Exception ex)
			{
				_logger.logError("Mutex error, giving up: " + ex.Message);
				MessageBox.Show("Cannot open connection to first instance (" + ex.Message + ")", "LogExpert");
			}
		}

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			//ShowUnhandledException(e.Exception);
			Thread thread = new Thread(new ParameterizedThreadStart(ShowUnhandledException));
			thread.IsBackground = true;
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start(e.Exception);
			thread.Join();
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			object exceptionObject = e.ExceptionObject;
			//ShowUnhandledException(exceptionObject);
			Thread thread = new Thread(new ParameterizedThreadStart(ShowUnhandledException));
			thread.IsBackground = true;
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start(exceptionObject);
			thread.Join();
		}

		[STAThread]
		private static void ShowUnhandledException(object exceptionObject)
		{
			_logger.logError("Exception: " + exceptionObject.ToString());
			String errorText = "";
			String stackTrace = "";
			if (exceptionObject is Exception)
			{
				errorText = (exceptionObject as Exception).Message;
				stackTrace = "\r\n" + (exceptionObject as Exception).GetType().Name + "\r\n" + (exceptionObject as Exception).StackTrace;
			}
			else
			{
				stackTrace = exceptionObject.ToString();
				String[] lines = stackTrace.Split(new char[] { '\n' });
				if (lines != null && lines.Length > 0)
				{
					errorText = lines[0];
				}
			}
			ExceptionWindow win = new ExceptionWindow(errorText, stackTrace);
			win.ShowDialog();
		}
	}
}