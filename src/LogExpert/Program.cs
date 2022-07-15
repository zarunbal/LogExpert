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
using LogExpert.Classes;
using LogExpert.Config;
using LogExpert.Controls.LogTabWindow;
using LogExpert.Dialogs;
using NLog;

namespace LogExpert
{
    internal static class Program
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetLogger("Program");

        #endregion

        #region Private Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] orgArgs)
        {
            try
            {
                Sub_Main(orgArgs);
            }
            catch (SecurityException se)
            {
                MessageBox.Show(
                    "Insufficient system rights for LogExpert. Maybe you have started it from a network drive. Please start LogExpert from a local drive.\n(" +
                    se.Message + ")", "LogExpert Error");
            }
        }

        private static void Sub_Main(string[] orgArgs)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            _logger.Info("\r\n============================================================================\r\nLogExpert {0} started.\r\n============================================================================", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));

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
                    argsList.Add(info.Exists ? info.FullName : fileArg);
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
                    MessageBox.Show(@"Config file not found", @"LogExpert");
                }
            }

            int pId = Process.GetCurrentProcess().SessionId;

            try
            {
                Settings settings = ConfigManager.Settings;
                Mutex mutex = new Mutex(false, "Local\\LogExpertInstanceMutex" + pId, out var isCreated);
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
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(LogExpertProxy), "LogExpertProxy", WellKnownObjectMode.Singleton);
                    LogExpertProxy proxy = new LogExpertProxy(logWin);
                    RemotingServices.Marshal(proxy, "LogExpertProxy");

                    LogExpertApplicationContext context = new LogExpertApplicationContext(proxy, logWin);
                    Application.Run(context);

                    ChannelServices.UnregisterChannel(ipcChannel);
                }
                else
                {
                    int counter = 3;
                    Exception errMsg = null;
                    IpcClientChannel ipcChannel = new IpcClientChannel("LogExpertClient#" + pId, null);
                    ChannelServices.RegisterChannel(ipcChannel, false);

                    while (counter > 0)
                    {
                        try
                        {
                            // another instance already exists
                            //WindowsIdentity wi = WindowsIdentity.GetCurrent();
                            LogExpertProxy proxy = (LogExpertProxy) Activator.GetObject(typeof(LogExpertProxy), "ipc://LogExpert" + pId + "/LogExpertProxy");
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
                            _logger.Warn(e, "IpcClientChannel error: ");
                            errMsg = e;
                            counter--;
                            Thread.Sleep(500);
                        }
                    }

                    if (counter == 0)
                    {
                        _logger.Error(errMsg, "IpcClientChannel error, giving up: ");
                        MessageBox.Show($"Cannot open connection to first instance ({errMsg})", "LogExpert");
                    }

                    if (settings.preferences.allowOnlyOneInstance)
                    {
                        MessageBox.Show($"Only one instance allowed, uncheck \"View Settings => Allow only 1 Instances\" to start multiple instances!", "Logexpert");
                    }
                }

                mutex.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Mutex error, giving up: ");
                MessageBox.Show($"Cannot open connection to first instance ({ex.Message})", "LogExpert");
            }
        }

        [STAThread]
        private static void ShowUnhandledException(object exceptionObject)
        {
            string errorText = string.Empty;
            string stackTrace = string.Empty;
            if (exceptionObject is Exception exception)
            {
                errorText = exception.Message;
                stackTrace = "\r\n" + exception.GetType().Name + "\r\n" +
                             exception.StackTrace;
            }
            else
            {
                stackTrace = exceptionObject.ToString();
                string[] lines = stackTrace.Split('\n');
                if (lines != null && lines.Length > 0)
                {
                    errorText = lines[0];
                }
            }
            ExceptionWindow win = new ExceptionWindow(errorText, stackTrace);
            win.ShowDialog();
        }

        #endregion

        #region Events handler

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            _logger.Fatal(e);

            //ShowUnhandledException(e.Exception);
            Thread thread = new Thread(ShowUnhandledException);
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(e.Exception);
            thread.Join();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e);

            object exceptionObject = e.ExceptionObject;
            //ShowUnhandledException(exceptionObject);
            Thread thread = new Thread(ShowUnhandledException);
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(exceptionObject);
            thread.Join();
        }

        #endregion
    }
}