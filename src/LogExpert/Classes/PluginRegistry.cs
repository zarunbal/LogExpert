using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using NLog;


namespace LogExpert
{
    /// <summary>
    /// Holds all registered plugins.
    /// </summary>
    /// <remarks>
    /// It all has started with Columnizers only. So the different types of plugins have no common super interface. I didn't change it
    /// to keep existing plugin API stable. In a future version this may change.
    /// </remarks>
    internal class PluginRegistry
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private static readonly object lockObject = new object();
        private static PluginRegistry instance = null;

        private readonly IFileSystemCallback fileSystemCallback = new FileSystemCallback();
        private readonly IList<ILogExpertPlugin> pluginList = new List<ILogExpertPlugin>();

        private readonly IDictionary<string, IKeywordAction> registeredKeywordsDict =
            new Dictionary<string, IKeywordAction>();

        #endregion

        #region cTor

        private PluginRegistry()
        {
            LoadPlugins();
        }

        #endregion

        #region Properties

        public IList<ILogLineColumnizer> RegisteredColumnizers { get; private set; }

        public IList<IContextMenuEntry> RegisteredContextMenuPlugins { get; } = new List<IContextMenuEntry>();

        public IList<IKeywordAction> RegisteredKeywordActions { get; } = new List<IKeywordAction>();

        public IList<IFileSystemPlugin> RegisteredFileSystemPlugins { get; } = new List<IFileSystemPlugin>();

        #endregion

        #region Public methods

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

        #region Internals

        internal void LoadPlugins()
        {
            _logger.Info("Loading plugins...");
            this.RegisteredColumnizers = new List<ILogLineColumnizer>();
            this.RegisteredColumnizers.Add(new AutoColumnizer());
            this.RegisteredColumnizers.Add(new DefaultLogfileColumnizer());
            this.RegisteredColumnizers.Add(new TimestampColumnizer());
            this.RegisteredColumnizers.Add(new SquareBracketColumnizer());
            this.RegisteredColumnizers.Add(new ClfColumnizer());
            this.RegisteredFileSystemPlugins.Add(new LocalFileSystem());

            string pluginDir = Application.StartupPath + Path.DirectorySeparatorChar + "plugins";
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(ColumnizerResolveEventHandler);
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
                                            this.RegisteredColumnizers.Add((ILogLineColumnizer) o);
                                            if (o is IColumnizerConfigurator)
                                            {
                                                ((IColumnizerConfigurator) o).LoadConfig(ConfigManager.ConfigDir);
                                            }
                                            if (o is ILogExpertPlugin)
                                            {
                                                this.pluginList.Add(o as ILogExpertPlugin);
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
                                        continue;
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
            this.registeredKeywordsDict.TryGetValue(name, out action);
            return action;
        }

        internal void CleanupPlugins()
        {
            foreach (ILogExpertPlugin plugin in this.pluginList)
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
            foreach (IFileSystemPlugin fs in this.RegisteredFileSystemPlugins)
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

        #endregion

        #region Private Methods

        private bool TryAsContextMenu(Type type)
        {
            IContextMenuEntry me = TryInstantiate<IContextMenuEntry>(type);
            if (me != null)
            {
                this.RegisteredContextMenuPlugins.Add(me);
                if (me is ILogExpertPluginConfigurator)
                {
                    ((ILogExpertPluginConfigurator) me).LoadConfig(ConfigManager.ConfigDir);
                }
                if (me is ILogExpertPlugin)
                {
                    this.pluginList.Add(me as ILogExpertPlugin);
                    (me as ILogExpertPlugin).PluginLoaded();
                }
                _logger.Info("Added context menu plugin {0}", type);
                return true;
            }
            return false;
        }

        private bool TryAsKeywordAction(Type type)
        {
            IKeywordAction ka = TryInstantiate<IKeywordAction>(type);
            if (ka != null)
            {
                this.RegisteredKeywordActions.Add(ka);
                this.registeredKeywordsDict.Add(ka.GetName(), ka);
                if (ka is ILogExpertPluginConfigurator)
                {
                    ((ILogExpertPluginConfigurator) ka).LoadConfig(ConfigManager.ConfigDir);
                }
                if (ka is ILogExpertPlugin)
                {
                    this.pluginList.Add(ka as ILogExpertPlugin);
                    (ka as ILogExpertPlugin).PluginLoaded();
                }
                _logger.Info("Added keyword plugin {0}", type);
                return true;
            }
            return false;
        }

        private bool TryAsFileSystem(Type type)
        {
            // file system plugins can have optional constructor with IFileSystemCallback argument
            IFileSystemPlugin fs = TryInstantiate<IFileSystemPlugin>(type, this.fileSystemCallback);
            if (fs == null)
            {
                fs = TryInstantiate<IFileSystemPlugin>(type);
            }
            if (fs != null)
            {
                this.RegisteredFileSystemPlugins.Add(fs);
                if (fs is ILogExpertPluginConfigurator)
                {
                    ((ILogExpertPluginConfigurator) fs).LoadConfig(ConfigManager.ConfigDir);
                }
                if (fs is ILogExpertPlugin)
                {
                    this.pluginList.Add(fs as ILogExpertPlugin);
                    (fs as ILogExpertPlugin).PluginLoaded();
                }
                _logger.Info("Added file system plugin {0}", type);
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
                ConstructorInfo cti = loadedType.GetConstructor(new Type[] {typeof(IFileSystemCallback)});
                if (cti != null)
                {
                    object o = cti.Invoke(new object[] {fsCallback});
                    return o as T;
                }
            }
            return default(T);
        }

        #endregion

        #region Events handler

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
            else if (pluginFile.Exists)
            {
                return Assembly.LoadFrom(pluginFile.FullName);
            }

            return null;
        }

        #endregion
    }
}