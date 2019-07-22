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
using Autofac;
using LogExpert.Dialogs;
using ITDM;
using LogExpert.Classes;
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
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            _logger.Info("\r\n============================================================================\r\nLogExpert {0} started.\r\n============================================================================",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3));

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
                    ContainerBuilder builder = new ContainerBuilder();

                    builder.RegisterModule<AutofacLogExpertModule>();

                    using (IContainer rooContainer = builder.Build())
                    using (ILifetimeScope rootScope = rooContainer.BeginLifetimeScope("RootScope"))
                    {
                        // first application instance
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);

                        LogTabWindow logWin = rootScope.Resolve<LogTabWindow>(
                            new TypedParameter(typeof(string[]), args.Length > 0 ? args : null),
                            new TypedParameter(typeof(int), 1),
                            new TypedParameter(typeof(bool), false));

                        // first instance
                        //WindowsIdentity wi = WindowsIdentity.GetCurrent();
                        IpcServerChannel ipcChannel = new IpcServerChannel("LogExpert" + pId);
                        ChannelServices.RegisterChannel(ipcChannel, false);
                        RemotingConfiguration.RegisterWellKnownServiceType(typeof(LogExpertProxy),
                            "LogExpertProxy",
                            WellKnownObjectMode.Singleton);

                        //TODO check why typedParamter LogTabWindo is needed
                        LogExpertProxy proxy = rootScope.Resolve<LogExpertProxy>(new TypedParameter(typeof(LogTabWindow), logWin));
                        RemotingServices.Marshal(proxy, "LogExpertProxy");

                        LogExpertApplicationContext context = rootScope.Resolve<LogExpertApplicationContext>(new TypedParameter(typeof(LogTabWindow), logWin));
                        Application.Run(context);

                        ChannelServices.UnregisterChannel(ipcChannel);
                    }
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
                            LogExpertProxy proxy = (LogExpertProxy) Activator.GetObject(typeof(LogExpertProxy),
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
            string errorText = "";
            string stackTrace = "";
            if (exceptionObject is Exception)
            {
                errorText = (exceptionObject as Exception).Message;
                stackTrace = "\r\n" + (exceptionObject as Exception).GetType().Name + "\r\n" +
                             (exceptionObject as Exception).StackTrace;
            }
            else
            {
                stackTrace = exceptionObject.ToString();
                string[] lines = stackTrace.Split(new char[] {'\n'});
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
            Thread thread = new Thread(new ParameterizedThreadStart(ShowUnhandledException));
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
            Thread thread = new Thread(new ParameterizedThreadStart(ShowUnhandledException));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(exceptionObject);
            thread.Join();
        }

        #endregion
    }
}