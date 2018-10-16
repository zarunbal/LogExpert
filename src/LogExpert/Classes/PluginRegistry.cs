using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NLog;

namespace LogExpert
{
    /// <summary>
    ///     Holds all registered plugins.
    /// </summary>
    /// <remarks>
    ///     It all has started with Columnizers only. So the different types of plugins have no common super interface. I
    ///     didn't change it
    ///     to keep existing plugin API stable. In a future version this may change.
    /// </remarks>
    internal class PluginRegistry
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Static/Constants

        private static PluginRegistry instance;

        private static readonly object lockObject = new object();

        #endregion

        #region Private Fields

        private readonly IFileSystemCallback fileSystemCallback = new FileSystemCallback();
        private readonly IList<ILogExpertPlugin> pluginList = new List<ILogExpertPlugin>();

        private readonly IDictionary<string, IKeywordAction> registeredKeywordsDict =
            new Dictionary<string, IKeywordAction>();

        #endregion

        #region Ctor

        private PluginRegistry()
        {
            LoadPlugins();
        }

        #endregion

        #region Properties / Indexers

        public IList<ILogLineColumnizer> RegisteredColumnizers { get; private set; }

        public IList<IContextMenuEntry> RegisteredContextMenuPlugins { get; } = new List<IContextMenuEntry>();

        public IList<IFileSystemPlugin> RegisteredFileSystemPlugins { get; } = new List<IFileSystemPlugin>();

        public IList<IKeywordAction> RegisteredKeywordActions { get; } = new List<IKeywordAction>();

        #endregion

        #region Public Methods

        public static PluginRegistry GetInstance()
        {
            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = new PluginRegistry();
                }

                return instance;
            }
        }

        #endregion

        #region Private Methods

        private static Assembly ColumnizerResolveEventHandler(object sender, ResolveEventArgs args)
        {
            string file = new AssemblyName(args.Name).Name + ".dll";

            string mainDir = Application.StartupPath + Path.DirectorySeparatorChar;
            string pluginDir = mainDir + "plugins\\";

            FileInfo mainFile = new FileInfo(mainDir + file);

            FileInfo pluginFile = new FileInfo(pluginDir + file);
            if (mainFile.Exists)
            {
                return Assembly.LoadFrom(mainFile.FullName);
            }

            if (pluginFile.Exists)
            {
                return Assembly.LoadFrom(pluginFile.FullName);
            }

            return null;
        }

        private bool TryAsContextMenu(Type type)
        {
            IContextMenuEntry me = TryInstantiate<IContextMenuEntry>(type);
            if (me != null)
            {
                RegisteredContextMenuPlugins.Add(me);
                if (me is ILogExpertPluginConfigurator)
                {
                    ((ILogExpertPluginConfigurator)me).LoadConfig(ConfigManager.ConfigDir);
                }

                if (me is ILogExpertPlugin)
                {
                    pluginList.Add(me as ILogExpertPlugin);
                    (me as ILogExpertPlugin).PluginLoaded();
                }

                _logger.Info("Added context menu plugin {0}", type);
                return true;
            }

            return false;
        }

        private bool TryAsFileSystem(Type type)
        {
            // file system plugins can have optional constructor with IFileSystemCallback argument
            IFileSystemPlugin fs = TryInstantiate<IFileSystemPlugin>(type, fileSystemCallback);
            if (fs == null)
            {
                fs = TryInstantiate<IFileSystemPlugin>(type);
            }

            if (fs != null)
            {
                RegisteredFileSystemPlugins.Add(fs);
                if (fs is ILogExpertPluginConfigurator)
                {
                    ((ILogExpertPluginConfigurator)fs).LoadConfig(ConfigManager.ConfigDir);
                }

                if (fs is ILogExpertPlugin)
                {
                    pluginList.Add(fs as ILogExpertPlugin);
                    (fs as ILogExpertPlugin).PluginLoaded();
                }

                _logger.Info("Added file system plugin {0}", type);
                return true;
            }

            return false;
        }

        private bool TryAsKeywordAction(Type type)
        {
            IKeywordAction ka = TryInstantiate<IKeywordAction>(type);
            if (ka != null)
            {
                RegisteredKeywordActions.Add(ka);
                registeredKeywordsDict.Add(ka.GetName(), ka);
                if (ka is ILogExpertPluginConfigurator)
                {
                    ((ILogExpertPluginConfigurator)ka).LoadConfig(ConfigManager.ConfigDir);
                }

                if (ka is ILogExpertPlugin)
                {
                    pluginList.Add(ka as ILogExpertPlugin);
                    (ka as ILogExpertPlugin).PluginLoaded();
                }

                _logger.Info("Added keyword plugin {0}", type);
                return true;
            }

            return false;
        }


        private T TryInstantiate<T>(Type loadedType) where T : class
        {
            Type t = typeof(T);
            Type inter = loadedType.GetInterface(t.Name);
            if (inter != null)
            {
                ConstructorInfo cti = loadedType.GetConstructor(Type.EmptyTypes);
                if (cti != null)
                {
                    object o = cti.Invoke(new object[] { });
                    return o as T;
                }
            }

            return default(T);
        }

        private T TryInstantiate<T>(Type loadedType, IFileSystemCallback fsCallback) where T : class
        {
            Type t = typeof(T);
            Type inter = loadedType.GetInterface(t.Name);
            if (inter != null)
            {
                ConstructorInfo cti = loadedType.GetConstructor(new[] {typeof(IFileSystemCallback)});
                if (cti != null)
                {
                    object o = cti.Invoke(new object[] {fsCallback});
                    return o as T;
                }
            }

            return default(T);
        }

        #endregion

        internal void LoadPlugins()
        {
            _logger.Info("Loading plugins...");
            RegisteredColumnizers = new List<ILogLineColumnizer>();
            RegisteredColumnizers.Add(new DefaultLogfileColumnizer());
            RegisteredColumnizers.Add(new TimestampColumnizer());
            RegisteredColumnizers.Add(new ClfColumnizer());
            RegisteredFileSystemPlugins.Add(new LocalFileSystem());

            string pluginDir = Application.StartupPath + Path.DirectorySeparatorChar + "plugins";
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += ColumnizerResolveEventHandler;
            if (Directory.Exists(pluginDir))
            {
                string[] dllNames = Directory.GetFiles(pluginDir, "*.dll");
                foreach (string dllName in dllNames)
                {
                    try
                    {
                        Assembly assemblyTmp = Assembly.ReflectionOnlyLoadFrom(dllName);
                        Assembly assembly = Assembly.Load(assemblyTmp.FullName);

                        Module[] modules = assembly.GetModules(false);
                        foreach (Module module in modules)
                        {
                            Type[] types = module.FindTypes(Module.FilterTypeName, "*");
                            foreach (Type type in types)
                            {
                                if (type.IsInterface)
                                {
                                    continue;
                                }

                                if (type.Name.EndsWith("Columnizer"))
                                {
                                    Type t = typeof(ILogLineColumnizer);
                                    Type inter = type.GetInterface(t.Name);
                                    if (inter != null)
                                    {
                                        ConstructorInfo cti = type.GetConstructor(Type.EmptyTypes);
                                        if (cti != null)
                                        {
                                            object o = cti.Invoke(new object[] { });
                                            RegisteredColumnizers.Add((ILogLineColumnizer)o);
                                            if (o is IColumnizerConfigurator)
                                            {
                                                ((IColumnizerConfigurator)o).LoadConfig(ConfigManager.ConfigDir);
                                            }

                                            if (o is ILogExpertPlugin)
                                            {
                                                pluginList.Add(o as ILogExpertPlugin);
                                                (o as ILogExpertPlugin).PluginLoaded();
                                            }

                                            _logger.Info("Added columnizer {0}", type.Name);
                                        }
                                    }
                                }
                                else
                                {
                                    if (TryAsContextMenu(type))
                                    {
                                        continue;
                                    }

                                    if (TryAsKeywordAction(type))
                                    {
                                        continue;
                                    }

                                    if (TryAsFileSystem(type))
                                    {
                                    }
                                }
                            }
                        }
                    }
                    catch (BadImageFormatException e)
                    {
                        _logger.Error(e, dllName);


// nothing... could be a DLL which is needed by any plugin
                    }
                    catch (FileLoadException e)
                    {
                        // can happen when a 32bit-only DLL is loaded on a 64bit system (or vice versa)
                        _logger.Error(e, dllName);
                    }
                }
            }

            _logger.Info("Plugin loading complete.");
        }

        internal IKeywordAction FindKeywordActionPluginByName(string name)
        {
            IKeywordAction action = null;
            registeredKeywordsDict.TryGetValue(name, out action);
            return action;
        }

        internal void CleanupPlugins()
        {
            foreach (ILogExpertPlugin plugin in pluginList)
            {
                plugin.AppExiting();
            }
        }

        internal IFileSystemPlugin FindFileSystemForUri(string uriString)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("Trying to find file system plugin for uri {0}", uriString);
            }

            foreach (IFileSystemPlugin fs in RegisteredFileSystemPlugins)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("Checking {0}", fs.Text);
                }

                if (fs.CanHandleUri(uriString))
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug("Found match {0}", fs.Text);
                    }

                    return fs;
                }
            }

            _logger.Error("No file system plugin found for uri {0}", uriString);
            return null;
        }
    }
}
