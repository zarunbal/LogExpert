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
        #region Fields

        public List<string> expressionHistoryList = new List<string>();
        public List<string> testtextHistoryList = new List<string>();

        #endregion
    }

    [Serializable]
    public class ColumnizerHistoryEntry
    {
        #region Fields

        public string columnizerName;
        public string fileName;

        #endregion

        #region cTor

        public ColumnizerHistoryEntry(string fileName, string columnizerName)
        {
            this.fileName = fileName;
            this.columnizerName = columnizerName;
        }

        #endregion
    }

    [Serializable]
    public class ColorEntry
    {
        #region Fields

        public Color color;
        public string fileName;

        #endregion

        #region cTor

        public ColorEntry(string fileName, Color color)
        {
            this.fileName = fileName;
            this.color = color;
        }

        #endregion
    }


    [Serializable]
    public class Settings
    {
        #region Fields

        public bool alwaysOnTop;
        public Rectangle appBounds;
        public Rectangle appBoundsFullscreen;
        public IList<ColumnizerHistoryEntry> columnizerHistoryList = new List<ColumnizerHistoryEntry>();
        public List<ColorEntry> fileColors = new List<ColorEntry>();
        public List<string> fileHistoryList = new List<string>();
        public List<string> filterHistoryList = new List<string>();
        public List<FilterParams> filterList = new List<FilterParams>();
        public FilterParams filterParams = new FilterParams();
        public List<string> filterRangeHistoryList = new List<string>();
        public bool hideLineColumn;

        public List<HilightEntry> hilightEntryList = new List<HilightEntry>()
            ; // legacy. is automatically converted to highlight groups on settings load

        public List<HilightGroup> hilightGroupList = new List<HilightGroup>()
            ; // should be in Preferences but is here for mistake. Maybe I migrate it some day.

        public bool isMaximized;
        public string lastDirectory;
        public List<string> lastOpenFilesList = new List<string>();
        public Preferences preferences = new Preferences();
        public RegexHistory regexHistory = new RegexHistory();
        public List<string> searchHistoryList = new List<string>();
        public SearchParams searchParams = new SearchParams();
        public IList<string> uriHistoryList = new List<string>();
        public int versionBuild;

        #endregion
    }

    [Serializable]
    public class ToolEntry
    {
        #region Fields

        public string args = "";
        public string cmd = "";
        public string columnizerName = "";
        public string iconFile;
        public int iconIndex;
        public bool isFavourite;
        public string name;
        public bool sysout = false;
        public string workingDir = "";

        #endregion

        #region Public methods

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

        #endregion
    }

    [Serializable]
    public class ColumnizerMaskEntry
    {
        #region Fields

        public string columnizerName;
        public string mask;

        #endregion
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
        #region Fields

        public string highlightGroupName;
        public string mask;

        #endregion
    }

    [Serializable]
    public class Preferences
    {
        #region Fields

        public bool allowOnlyOneInstance;
        public bool askForClose = false;
        public int bufferCount = 100;
        public List<ColumnizerMaskEntry> columnizerMaskList = new List<ColumnizerMaskEntry>();
        public string defaultEncoding;
        public bool filterSync = true;
        public bool filterTail = true;
        public bool followTail = true;
        public string fontName = "Courier New";
        public float fontSize = 9;
        public List<HighlightMaskEntry> highlightMaskList = new List<HighlightMaskEntry>();
        public bool isAutoHideFilterList = false;
        public bool isFilterOnLoad;
        public int lastColumnWidth = 2000;
        public int linesPerBuffer = 500;
        public bool maskPrio;
        public MultiFileOption multiFileOption;
        public MultifileOptions multifileOptions;
        public bool multiThreadFilter = true;
        public bool openLastFiles = true;
        public int pollingInterval = 250;
        public bool reverseAlpha = false;
        public string saveDirectory = null;
        public bool saveFilters = true;
        public SessionSaveLocation saveLocation = SessionSaveLocation.DocumentsDir;
        public bool saveSessions = true;
        public bool setLastColumnWidth;
        public bool showBubbles = true;
        public bool showColumnFinder;
        public Color showTailColor = Color.FromKnownColor(KnownColor.Blue);
        public bool showTailState = true;
        public bool showTimeSpread = false;
        public Color timeSpreadColor = Color.FromKnownColor(KnownColor.Gray);
        public bool timeSpreadTimeMode;
        public bool timestampControl = true;

        public DateTimeDragControl.DragOrientations timestampControlDragOrientation =
            DateTimeDragControl.DragOrientations.Horizontal;

        public List<ToolEntry> toolEntries = new List<ToolEntry>();
        public bool useLegacyReader;

        #endregion
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

    [FlagsAttribute]
    public enum ExportImportFlags : long
    {
        None = 0,
        HighlightSettings = 1,
        ColumnizerMasks = 2,
        HighlightMasks = 4,
        ToolEntries = 8,
        Other = 16,
        All = HighlightSettings | ColumnizerMasks | HighlightMasks | ToolEntries | Other
    }


    public class ConfigManager
    {
        #region Fields

        private static readonly object monitor = new object();
        private static ConfigManager instance = null;
        private readonly object loadSaveLock = new object();
        private Settings settings = null;

        #endregion

        #region cTor

        private ConfigManager()
        {
            this.settings = this.Load();
        }

        #endregion

        #region Events

        internal event ConfigChangedEventHandler ConfigChanged;

        #endregion

        #region Properties

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


        public static string ConfigDir
        {
            get
            {
                string tmp = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string tmp2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LogExpert";
            }
        }

        public static Settings Settings
        {
            get { return Instance.settings; }
        }

        #endregion

        #region Public methods

        public static void Save(SettingsFlags flags)
        {
            Instance.Save(Settings, flags);
        }

        public static void Export(Stream fs)
        {
            Instance.Save(fs, Settings, SettingsFlags.None);
        }

        public static void Import(Stream fs, ExportImportFlags flags)
        {
            Instance.settings = Instance.Import(Instance.settings, fs, flags);
            Save(SettingsFlags.All);
        }

        public static void Import(FileInfo fileInfo, ExportImportFlags flags)
        {
            Stream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
            ConfigManager.Import(fs, flags);
            fs.Close();
        }

        #endregion

        #region Private Methods

        private Settings Load()
        {
            Logger.logInfo("Loading settings");
            string dir = ConfigDir;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(dir + "\\settings.dat"))
            {
                return LoadOrCreateNew(null);
            }
            else
            {
                Stream fs = File.OpenRead(dir + "\\settings.dat");
                try
                {
                    return LoadOrCreateNew(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        private Settings LoadOrCreateNew(Stream fs)
        {
            lock (this.loadSaveLock)
            {
                Settings settings;
                if (fs == null)
                {
                    settings = new Settings();
                }
                else
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        settings = (Settings) formatter.Deserialize(fs);
                    }
                    catch (SerializationException)
                    {
                        //Logger.logError("Error while deserializing config data: " + e.Message); 
                        settings = new Settings();
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
                if (settings.preferences.defaultEncoding == null)
                {
                    settings.preferences.defaultEncoding = Encoding.Default.HeaderName;
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
                    Stream fs = new FileStream(dir + "\\settings.dat", FileMode.Create, FileAccess.Write);
                    Save(fs, settings, flags);
                    fs.Close();
                }
                OnConfigChanged(flags);
            }
        }

        private void Save(Stream fs, Settings settings, SettingsFlags flags)
        {
            settings.versionBuild = Assembly.GetExecutingAssembly().GetName().Version.Build;
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, settings);
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


        /// <summary>
        /// Imports all or some of the settings/prefs stored in the inpute stream.
        /// This will overwrite appropriate parts of the current (own) settings with the imported ones.
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="flags">Flags to indicate which parts shall be imported</param>
        private Settings Import(Settings currentSettings, Stream fs, ExportImportFlags flags)
        {
            Settings importSettings = LoadOrCreateNew(fs);
            Settings ownSettings = ObjectClone.Clone<Settings>(currentSettings);
            Settings newSettings;

            // at first check for 'Other' as this are the most options.
            if ((flags & ExportImportFlags.Other) == ExportImportFlags.Other)
            {
                newSettings = ownSettings;
                newSettings.preferences = ObjectClone.Clone<Preferences>(importSettings.preferences);
                newSettings.preferences.columnizerMaskList = ownSettings.preferences.columnizerMaskList;
                newSettings.preferences.highlightMaskList = ownSettings.preferences.highlightMaskList;
                newSettings.hilightGroupList = ownSettings.hilightGroupList;
                newSettings.preferences.toolEntries = ownSettings.preferences.toolEntries;
            }
            else
            {
                newSettings = ownSettings;
            }

            if ((flags & ExportImportFlags.ColumnizerMasks) == ExportImportFlags.ColumnizerMasks)
            {
                newSettings.preferences.columnizerMaskList = importSettings.preferences.columnizerMaskList;
            }
            if ((flags & ExportImportFlags.HighlightMasks) == ExportImportFlags.HighlightMasks)
            {
                newSettings.preferences.highlightMaskList = importSettings.preferences.highlightMaskList;
            }
            if ((flags & ExportImportFlags.HighlightSettings) == ExportImportFlags.HighlightSettings)
            {
                newSettings.hilightGroupList = importSettings.hilightGroupList;
            }
            if ((flags & ExportImportFlags.ToolEntries) == ExportImportFlags.ToolEntries)
            {
                newSettings.preferences.toolEntries = importSettings.preferences.toolEntries;
            }

            return newSettings;
        }

        #endregion

        protected void OnConfigChanged(SettingsFlags flags)
        {
            ConfigChangedEventHandler handler = ConfigChanged;
            if (handler != null)
            {
                Logger.logInfo("Fire config changed event");
                handler(this, new ConfigChangedEventArgs(flags));
            }
        }

        internal delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs e);
    }
}