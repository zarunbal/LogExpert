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
using ITDM;

namespace LogExpert
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] orgArgs)
		{
			try
			{
				Sub_Main(orgArgs);
			}
			catch (SecurityException se)
			{
				MessageBox.Show("Insufficient system rights for LogExpert. Maybe you have started it from a network drive. Please start LogExpert from a local drive.\n(" + se.Message + ")", "LogExpert Error");
			}
		}

		static void Sub_Main(string[] orgArgs)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			Logger.logInfo("============================================================================");
			Logger.logInfo("LogExpert " + Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
					 Assembly.GetExecutingAssembly().GetName().Version.Minor + "/" +
					 Assembly.GetExecutingAssembly().GetName().Version.Build.ToString() +
					 " started.");
			Logger.logInfo("============================================================================");

		  CmdLine cmdLine = new CmdLine();
		  CmdLineString configFile = new CmdLineString("config", false, "A configuration (settings) file");
		  cmdLine.RegisterParameter(configFile);
		  string[] remainingArgs = cmdLine.Parse(orgArgs);

			List<string> argsList = new List<string>();

      // This loop tries to convert relative file names into absolute file names (assuming that platform file names are given).
      // It tolerates errors, to give file system plugins (e.g. sftp) a change later.
      // TODO: possibly should be moved to LocalFileSystem plugin
      foreach (string fileArg in remainingArgs)
			{
				try
				{
					FileInfo info = new FileInfo(fileArg);
					if (info.Exists)
						argsList.Add(info.FullName);
					else
						argsList.Add(fileArg);
				}
				catch (Exception)
				{
          argsList.Add(fileArg);
				}
			}
			string[] args = argsList.ToArray();
      if (configFile.Exists)
      {
        FileInfo cfgFileInfo = new FileInfo(configFile.Value);
        if (cfgFileInfo.Exists)
        {
          ConfigManager.Import(cfgFileInfo, ExportImportFlags.All);
        }
        else
        {
          MessageBox.Show("Config file not found", "LogExpert");
        }
      }

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
							Logger.logError("IpcClientChannel error: " + e.Message);
							errMsg = e.Message;
							counter--;
							Thread.Sleep(500);
						}
					}
					if (counter == 0)
					{
						Logger.logError("IpcClientChannel error, giving up: " + errMsg);
						MessageBox.Show("Cannot open connection to first instance (" + errMsg + ")", "LogExpert");
					}
				}
				mutex.Close();
			}
			catch (Exception ex)
			{
				Logger.logError("Mutex error, giving up: " + ex.Message);
				MessageBox.Show("Cannot open connection to first instance (" + ex.Message + ")", "LogExpert");
			}
		}

		static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			//ShowUnhandledException(e.Exception);
			Thread thread = new Thread(new ParameterizedThreadStart(ShowUnhandledException));
			thread.IsBackground = true;
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start(e.Exception);
			thread.Join();
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
			Logger.logError("Exception: " + exceptionObject.ToString());
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
