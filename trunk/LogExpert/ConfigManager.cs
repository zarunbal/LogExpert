using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Drawing;
using LogExpert.Dialogs;
using System.Collections;
using System.Reflection;

namespace LogExpert
{
  [Serializable]
  public class RegexHistory
  {
    public List<string>   expressionHistoryList = new List<string>();
    public List<string> testtextHistoryList = new List<string>();
  }

  [Serializable]
  public class ColumnizerHistoryEntry
  {
    public ColumnizerHistoryEntry(string fileName, string columnizerName)
    {
      this.fileName = fileName;
      this.columnizerName = columnizerName;
    }
    public string fileName;
    public string columnizerName;
  }

  [Serializable]
  public class ColorEntry
  {
    public string fileName;
    public Color color;

    public ColorEntry(string fileName, Color color)
    {
      this.fileName = fileName;
      this.color = color;
    }
  }


  [Serializable]
  public class Settings
  {
    public List<HilightEntry> hilightEntryList = new List<HilightEntry>();
    public SearchParams searchParams = new SearchParams();
    public RegexHistory regexHistory = new RegexHistory();
    public FilterParams filterParams = new FilterParams();
    public IList<ColumnizerHistoryEntry> columnizerHistoryList = new List<ColumnizerHistoryEntry>();
    public List<string> searchHistoryList = new List<string>();
    public List<string> filterHistoryList = new List<string>();
    public List<string> filterRangeHistoryList = new List<string>();
    public Rectangle appBounds;
    public bool isMaximized;
    public bool alwaysOnTop;
    public Preferences preferences = new Preferences();
    public String lastDirectory;
    public bool hideLineColumn;
    public List<string> fileHistoryList = new List<string>();
    public List<string> lastOpenFilesList = new List<string>();
    public List<ColorEntry> fileColors = new List<ColorEntry>();
    public List<FilterParams> filterList = new List<FilterParams>();
    public List<HilightGroup> hilightGroupList = new List<HilightGroup>();
    public Rectangle appBoundsFullscreen;
    public int versionBuild;
  }

  [Serializable]
  public class ToolEntry
  {
    public string cmd = "";
    public string args = "";
    public bool sysout = false;
    public string columnizerName = "";
    public string name;
    public bool isFavourite;
    public string iconFile;
    public int iconIndex;
    public string workingDir = "";

    public override string ToString()
    {
      return Util.IsNull(this.name) ? this.cmd : this.name;
    }

    public ToolEntry Clone()
    {
      ToolEntry clone = new ToolEntry();
      clone.cmd = this.cmd;
      clone.args = this.args;
      clone.name = this.name;
      clone.sysout = this.sysout;
      clone.columnizerName = this.columnizerName;
      clone.isFavourite = this.isFavourite;
      clone.iconFile = this.iconFile;
      clone.iconIndex = this.iconIndex;
      clone.workingDir = this.workingDir;
      return clone;
    }

  }

  [Serializable]
  public class ColumnizerMaskEntry
  {
    public string mask;
    public string columnizerName;
  }

  [Serializable]
  public enum MultiFileOption
  {
    SingleFiles,
    MultiFile,
    Ask
  }

  [Serializable]
  public enum SessionSaveLocation
  {
    DocumentsDir,
    SameDir,
    OwnDir,
    LoadedSessionFile
  }

  [Serializable]
  public class HighlightMaskEntry
  {
    public string mask;
    public string highlightGroupName;
  }

  [Serializable]
  public class Preferences
  {
    public string fontName = "Courier New";
    public float fontSize = 9;
    public bool timestampControl = true;
    public bool followTail = true;
    public bool filterTail = true;
    public bool filterSync = true;
    public DateTimeDragControl.DragOrientations timestampControlDragOrientation = DateTimeDragControl.DragOrientations.Horizonzal;
    public List<ToolEntry> toolEntries = new List<ToolEntry>();
    public List<ColumnizerMaskEntry> columnizerMaskList = new List<ColumnizerMaskEntry>();
    public bool maskPrio;
    public bool askForClose = false;
    public MultiFileOption multiFileOption;
    public bool allowOnlyOneInstance;
    public bool openLastFiles = true;
    public bool showTailState = true;
    public Color showTailColor = Color.FromKnownColor(KnownColor.Blue);
    public bool setLastColumnWidth;
    public int lastColumnWidth = 2000;
    public bool showTimeSpread = false;
    public bool reverseAlpha = false;
    public Color timeSpreadColor = Color.FromKnownColor(KnownColor.Gray);
    public bool timeSpreadTimeMode;
    public bool saveSessions = true;
    public SessionSaveLocation saveLocation = SessionSaveLocation.DocumentsDir;
    public string saveDirectory = null;
    public bool showBubbles = true;
    public bool saveFilters = true;
    public int bufferCount = 100;
    public int linesPerBuffer = 500;
    public List<HighlightMaskEntry> highlightMaskList = new List<HighlightMaskEntry>();
    public bool isFilterOnLoad;
    public bool multiThreadFilter = true;
    public int pollingInterval = 250;
    public bool isAutoHideFilterList = false;
    public MultifileOptions multifileOptions;
  }

  [FlagsAttribute]
  public enum SettingsFlags : long
  {
    None = 0,
    WindowPosition = 1,
    FileHistory = 2,
    HighlightSettings = 4,
    FilterList = 8,
    RegexHistory = 16,
    ToolSettings = 32,
    GuiOrColors = 64,
    FilterHistory = 128,

    All = WindowPosition | FileHistory | HighlightSettings | 
          FilterList | RegexHistory | ToolSettings | GuiOrColors |
          FilterHistory,

    Settings = All & ~WindowPosition & ~FileHistory,


  }


  public class ConfigManager
  {
    private static Object monitor = new Object();
    private static ConfigManager instance = null;
    private Settings settings = null;
    private Object loadSaveLock = new Object();

    private ConfigManager()
    {
      this.settings = this.Load();
    }

    public static ConfigManager Instance
    {
      get
      {
        lock (monitor)
        {
          if (instance == null)
          {
            instance = new ConfigManager();
          }
        }
        return instance;
      }
    }

    public static void Save(SettingsFlags flags)
    {
      Instance.Save(Settings, flags);
    }

    public static string ConfigDir
    {
      get 
      {
        String tmp = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        String tmp2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LogExpert"; 
      }
    }

    public static Settings Settings
    {
      get 
      {
        return Instance.settings;
      }
    }


    private Settings Load()
    {
      lock (this.loadSaveLock)
      {
        Logger.logInfo("Loading settings");
        Settings settings;
        string dir = ConfigDir;
        if (!Directory.Exists(dir))
        {
          Directory.CreateDirectory(dir);
        }

        if (!File.Exists(dir + "\\settings.dat"))
        {
          settings = new Settings();
        }
        else
        {
          Stream fs = File.OpenRead(dir + "\\settings.dat");
          BinaryFormatter formatter = new BinaryFormatter();
          try
          {
            settings = (Settings)formatter.Deserialize(fs);
          }
          catch (SerializationException)
          {
            //Logger.logError("Error while deserializing config data: " + e.Message); 
            return new Settings();
          }
          finally
          {
            fs.Close();
          }
        }
        if (settings.preferences == null)
        {
          settings.preferences = new Preferences();
        }
        if (settings.preferences.toolEntries == null)
        {
          settings.preferences.toolEntries = new List<ToolEntry>();
        }
        if (settings.preferences.columnizerMaskList == null)
        {
          settings.preferences.columnizerMaskList = new List<ColumnizerMaskEntry>();
        }
        if (settings.fileHistoryList == null)
        {
          settings.fileHistoryList = new List<string>();
        }
        if (settings.lastOpenFilesList == null)
        {
          settings.lastOpenFilesList = new List<string>();
        }
        if (settings.fileColors == null)
        {
          settings.fileColors = new List<ColorEntry>();
        }
        if (settings.preferences.showTailColor == Color.Empty)
        {
          settings.preferences.showTailColor = Color.FromKnownColor(KnownColor.Blue);
        }
        if (settings.preferences.timeSpreadColor == Color.Empty)
        {
          settings.preferences.timeSpreadColor = Color.Gray;
        }
        if (settings.preferences.bufferCount < 10)
        {
          settings.preferences.bufferCount = 100;
        }
        if (settings.preferences.linesPerBuffer < 1)
        {
          settings.preferences.linesPerBuffer = 500;
        }
        if (settings.filterList == null)
        {
          settings.filterList = new List<FilterParams>();
        }
        if (settings.searchHistoryList == null)
        {
          settings.searchHistoryList = new List<string>();
        }
        if (settings.filterHistoryList == null)
        {
          settings.filterHistoryList = new List<string>();
        }
        if (settings.filterRangeHistoryList == null)
        {
          settings.filterRangeHistoryList = new List<string>();
        }
        foreach (FilterParams filterParams in settings.filterList)
        {
          filterParams.Init();
        }
        if (settings.hilightGroupList == null)
        {
          settings.hilightGroupList = new List<HilightGroup>();
          // migrate old non-grouped entries
          HilightGroup defaultGroup = new HilightGroup();
          defaultGroup.GroupName = "[Default]";
          defaultGroup.HilightEntryList = settings.hilightEntryList;
          settings.hilightGroupList.Add(defaultGroup);
        }
        if (settings.preferences.highlightMaskList == null)
        {
          settings.preferences.highlightMaskList = new List<HighlightMaskEntry>();
        }
        if (settings.preferences.pollingInterval < 20)
        {
          settings.preferences.pollingInterval = 250;
        }
        if (settings.preferences.multifileOptions == null)
        {
          settings.preferences.multifileOptions = new MultifileOptions();
        }

        ConvertSettings(settings, Assembly.GetExecutingAssembly().GetName().Version.Build);

        return settings;
      }
    }

    private void Save(Settings settings, SettingsFlags flags)
    {
      lock (this.loadSaveLock)
      {
        Logger.logInfo("Saving settings");
        lock (this)
        {
          string dir = ConfigDir;
          if (!Directory.Exists(dir))
          {
            Directory.CreateDirectory(dir);
          }
          settings.versionBuild = Assembly.GetExecutingAssembly().GetName().Version.Build;
          BinaryFormatter formatter = new BinaryFormatter();
          Stream fs = new FileStream(dir + "\\settings.dat", FileMode.Create, FileAccess.Write);
          formatter.Serialize(fs, settings);
          fs.Close();
        }
        OnConfigChanged(flags);
      }
    }

    /// <summary>
    /// Convert settings loaded from previous versions.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="currentBuildNumber"></param>
    private void ConvertSettings(Settings settings, int currentBuildNumber)
    {
      int oldBuildNumber = settings.versionBuild;

      // All Versions before 3583
      if (oldBuildNumber < 3584)
      {
        // External tools
        List<ToolEntry> newList = new List<ToolEntry>();
        foreach (ToolEntry tool in settings.preferences.toolEntries)
        {
          // set favourite to true only when name is empty, because there are always version released without this conversion fx
          // remove empty tool entries (there were always 3 entries before, which can be empty if not used)
          if (Util.IsNull(tool.name))
          {
            if (!Util.IsNull(tool.cmd))
            {
              tool.name = tool.cmd;
              tool.isFavourite = true;
              newList.Add(tool);
            }
          }
          else
          {
            newList.Add(tool);
          }
          if (Util.IsNull(tool.iconFile))
          {
            tool.iconFile = tool.cmd;
            tool.iconIndex = 0;
          }
        }
        settings.preferences.toolEntries = newList;
      }

      if (oldBuildNumber < 3584)
      {
        // Set the color for the FilterList entries to default (black)
        foreach (FilterParams filterParam in settings.filterList)
        {
          filterParam.color = Color.FromKnownColor(KnownColor.Black);
        }
      }
    }

    internal delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs e);
    internal event ConfigChangedEventHandler ConfigChanged;
    protected void OnConfigChanged(SettingsFlags flags)
    {
      ConfigChangedEventHandler handler = ConfigChanged;
      if (handler != null)
      {
        Logger.logInfo("Fire config changed event");
        handler(this, new ConfigChangedEventArgs(flags));
      }
    }


  }
}
