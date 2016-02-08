using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using ColumnizerLib;

namespace LogExpert
{
	/// <summary>
	/// Holds all registered plugins.
	/// </summary>
	/// <remarks>
	/// It all has started with Columnizers only. So the different types of plugins have no common super interface. I didn't change it
	/// to keep existing plugin API stable. In a future version this may change.
	/// </remarks>
	class PluginRegistry
	{
		private static Object lockObject = new Object();
		private static PluginRegistry instance = null;

		private IList<IContextMenuEntry> _registeredContextMenuPlugins = new List<IContextMenuEntry>();
		private IList<IKeywordAction> _registeredKeywordActions = new List<IKeywordAction>();
		private IDictionary<string, IKeywordAction> _registeredKeywordsDict = new Dictionary<string, IKeywordAction>();
		private IList<ILogExpertPlugin> _pluginList = new List<ILogExpertPlugin>();
		private IList<IFileSystemPlugin> _fileSystemPlugins = new List<IFileSystemPlugin>();

		private IFileSystemCallback _fileSystemCallback = new FileSystemCallback();

		public IList<ILogLineColumnizer> RegisteredColumnizers { get; private set; }

		public IList<IContextMenuEntry> RegisteredContextMenuPlugins
		{
			get
			{
				return _registeredContextMenuPlugins;
			}
		}

		public IList<IKeywordAction> RegisteredKeywordActions
		{
			get
			{
				return _registeredKeywordActions;
			}
		}

		public IList<IFileSystemPlugin> RegisteredFileSystemPlugins
		{
			get
			{
				return _fileSystemPlugins;
			}
		}

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

		private PluginRegistry()
		{
			LoadPlugins();
		}

		internal void LoadPlugins()
		{
			Logger.logInfo("Loading plugins...");
			RegisteredColumnizers = new List<ILogLineColumnizer>();
			RegisteredColumnizers.Add(new DefaultLogfileColumnizer());
			RegisteredColumnizers.Add(new TimestampColumnizer());
			RegisteredColumnizers.Add(new ClfColumnizer());
			RegisteredFileSystemPlugins.Add(new LocalFileSystem());

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
											RegisteredColumnizers.Add((ILogLineColumnizer)o);
											if (o is IColumnizerConfigurator)
											{
												((IColumnizerConfigurator)o).LoadConfig(ConfigManager.ConfigDir);
											}
											if (o is ILogExpertPlugin)
											{
												_pluginList.Add(o as ILogExpertPlugin);
												(o as ILogExpertPlugin).PluginLoaded();
											}
											Logger.logInfo("Added columnizer " + type.Name);
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
					catch (BadImageFormatException)
					{
						// nothing... could be a DLL which is needed by any plugin
					}
					catch (FileLoadException e)
					{
						// can happen when a 32bit-only DLL is loaded on a 64bit system (or vice versa)
						Logger.logError(e.Message);
					}
				}
			}
			Logger.logInfo("Plugin loading complete.");
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
					_pluginList.Add(me as ILogExpertPlugin);
					(me as ILogExpertPlugin).PluginLoaded();
				}
				Logger.logInfo("Added context menu plugin " + type.Name);
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
				_registeredKeywordsDict.Add(ka.GetName(), ka);
				if (ka is ILogExpertPluginConfigurator)
				{
					((ILogExpertPluginConfigurator)ka).LoadConfig(ConfigManager.ConfigDir);
				}
				if (ka is ILogExpertPlugin)
				{
					_pluginList.Add(ka as ILogExpertPlugin);
					(ka as ILogExpertPlugin).PluginLoaded();
				}
				Logger.logInfo("Added keyword plugin " + type.Name);
				return true;
			}
			return false;
		}

		private bool TryAsFileSystem(Type type)
		{
			// file system plugins can have optional constructor with IFileSystemCallback argument
			IFileSystemPlugin fs = TryInstantiate<IFileSystemPlugin>(type, _fileSystemCallback);
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
					_pluginList.Add(fs as ILogExpertPlugin);
					(fs as ILogExpertPlugin).PluginLoaded();
				}
				Logger.logInfo("Added file system plugin " + type.Name);
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
				ConstructorInfo cti = loadedType.GetConstructor(new Type[] { typeof(IFileSystemCallback) });
				if (cti != null)
				{
					object o = cti.Invoke(new object[] { fsCallback });
					return o as T;
				}
			}
			return default(T);
		}

		static Assembly ColumnizerResolveEventHandler(object sender, ResolveEventArgs args)
		{
			AssemblyName name = new AssemblyName(args.Name);

			//TODO add check for exe files
			string path = Path.Combine(Application.StartupPath, string.Format("{0}.dll", name.Name));
			Assembly output = null;
			//First check local folder
			if (File.Exists(path))
			{
				output = Assembly.LoadFrom(path);
			}

			path = Path.Combine(Application.StartupPath, "plugins", string.Format("{0}.dll", name.Name));
			//Check plugins folder
			if (File.Exists(path))
			{
				output = Assembly.LoadFrom(path);
			}

			return output;
		}

		internal IKeywordAction FindKeywordActionPluginByName(string name)
		{
			IKeywordAction action = null;
			_registeredKeywordsDict.TryGetValue(name, out action);
			return action;
		}

		internal void CleanupPlugins()
		{
			foreach (ILogExpertPlugin plugin in _pluginList)
			{
				plugin.AppExiting();
			}
		}

		internal IFileSystemPlugin FindFileSystemForUri(string uriString)
		{
			if (Logger.IsDebug)
				Logger.logDebug("Trying to find file system plugin for uri " + uriString);
			foreach (IFileSystemPlugin fs in RegisteredFileSystemPlugins)
			{
				if (Logger.IsDebug)
					Logger.logDebug("Checking " + fs.Text);
				if (fs.CanHandleUri(uriString))
				{
					if (Logger.IsDebug)
						Logger.logDebug("Found match " + fs.Text);
					return fs;
				}
			}
			Logger.logError("No file system plugin found for uri " + uriString);
			return null;
		}
	}
}