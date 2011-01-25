using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace LogExpert
{
  class PluginRegistry
  {
    static Object lockObject = new Object();
    static PluginRegistry instance = null;

    private IList<ILogLineColumnizer> registeredColumnizers;
    private IList<IContextMenuEntry> registeredContextMenuPlugins = new List<IContextMenuEntry>();
    private IList<IKeywordAction> registeredKeywordActions = new List<IKeywordAction>();
    private IDictionary<string, IKeywordAction> registeredKeywordsDict = new Dictionary<string, IKeywordAction>();
    private IList<ILogExpertPlugin> pluginList = new List<ILogExpertPlugin>();

    public IList<ILogLineColumnizer> RegisteredColumnizers
    {
      get { return this.registeredColumnizers; }
    }

    public IList<IContextMenuEntry> RegisteredContextMenuPlugins
    {
      get { return this.registeredContextMenuPlugins; }
    }

    public IList<IKeywordAction> RegisteredKeywordActions
    {
      get { return this.registeredKeywordActions; }
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
      this.registeredColumnizers = new List<ILogLineColumnizer>();
      this.registeredColumnizers.Add(new DefaultLogfileColumnizer());
      this.registeredColumnizers.Add(new TimestampColumnizer());
      this.registeredColumnizers.Add(new ClfColumnizer());

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
                  continue;
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
                      this.RegisteredColumnizers.Add((ILogLineColumnizer)o);
                      if (o is IColumnizerConfigurator)
                      {
                        ((IColumnizerConfigurator)o).LoadConfig(ConfigManager.ConfigDir);
                      }
                      if (o is ILogExpertPlugin)
                      {
                        this.pluginList.Add(o as ILogExpertPlugin);
                        (o as ILogExpertPlugin).PluginLoaded();
                      }
                    }
                  }
                }
                else
                {
                  IContextMenuEntry me = TryInstantiate<IContextMenuEntry>(type);
                  if (me != null)
                  {
                    this.RegisteredContextMenuPlugins.Add(me);
                    if (me is ILogExpertPluginConfigurator)
                    {
                      ((ILogExpertPluginConfigurator)me).LoadConfig(ConfigManager.ConfigDir);
                    }
                    if (me is ILogExpertPlugin)
                    {
                      this.pluginList.Add(me as ILogExpertPlugin);
                      (me as ILogExpertPlugin).PluginLoaded();
                    }
                  }
                  else
                  {
                    IKeywordAction ka = TryInstantiate<IKeywordAction>(type);
                    if (ka != null)
                    {
                      this.RegisteredKeywordActions.Add(ka);
                      this.registeredKeywordsDict.Add(ka.GetName(), ka);
                      if (ka is ILogExpertPluginConfigurator)
                      {
                        ((ILogExpertPluginConfigurator)ka).LoadConfig(ConfigManager.ConfigDir);
                      }
                      if (ka is ILogExpertPlugin)
                      {
                        this.pluginList.Add(ka as ILogExpertPlugin);
                        (ka as ILogExpertPlugin).PluginLoaded();
                      }
                    }
                  }
                }
              }
            }
          }
          catch (BadImageFormatException)
          {
            // nothing... could be a legacy DLL which is needed by any plugin
          }
        }
      }
      Logger.logInfo("Plugin loading complete.");
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


    static Assembly ColumnizerResolveEventHandler(object sender, ResolveEventArgs args)
    {
      string pluginDir = Application.StartupPath + Path.DirectorySeparatorChar + "plugins\\";
      Assembly assembly = Assembly.LoadFrom(pluginDir + new AssemblyName(args.Name).Name + ".dll");
      return assembly;
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

  }
}
