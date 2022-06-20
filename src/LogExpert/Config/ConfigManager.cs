using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using System.Linq;

namespace LogExpert
{
    public class ConfigManager
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private static readonly object _monitor = new object();
        private static ConfigManager _instance;
        private readonly object _loadSaveLock = new object();
        private Settings _settings;

        #endregion

        #region cTor

        private ConfigManager()
        {
            _settings = Load();
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
                lock (_monitor)
                {
                    if (_instance == null)
                    {
                        _instance = new ConfigManager();
                    }
                }
                return _instance;
            }
        }
        
        public static string ConfigDir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LogExpert";

        public static string PortableMode => Application.StartupPath + "\\portableMode.json";

        public static Settings Settings => Instance._settings;

        #endregion

        #region Public methods

        public static void Save(SettingsFlags flags)
        {
            Instance.Save(Settings, flags);
        }

        public static void Export(FileInfo fileInfo)
        {
            Instance.Save(fileInfo, Settings);
        }

        public static void Import(FileInfo fileInfo, ExportImportFlags flags)
        {
            Instance._settings = Instance.Import(Instance._settings, fileInfo, flags);
            Save(SettingsFlags.All);
        }
        
        #endregion

        #region Private Methods

        private Settings Load()
        {
            _logger.Info("Loading settings");

            string dir;

            if (!File.Exists(PortableMode))
            {
                _logger.Info("Load settings standard mode");
               dir = ConfigDir;
            }
            else
            {
                _logger.Info("Load settings portable mode");
                dir = Application.StartupPath;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(dir + "\\settings.json"))
            {
                return LoadOrCreateNew(null);
            }

            try
            {
                FileInfo fileInfo = new FileInfo(dir + "\\settings.json");
                return LoadOrCreateNew(fileInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"Error loading settings: {e}");
                return LoadOrCreateNew(null);
            }

        }

        private Settings LoadOrCreateNew(FileInfo fileInfo)
        {
            lock (_loadSaveLock)
            {
                Settings settings;

                if (fileInfo == null)
                {
                    settings = new Settings();
                }
                else
                {
                    try
                    {
                        settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($"{fileInfo.FullName}"));
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Error while deserializing config data: {e}");
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

                if (settings.preferences.multiFileOptions == null)
                {
                    settings.preferences.multiFileOptions = new MultiFileOptions();
                }

                if (settings.preferences.defaultEncoding == null)
                {
                    settings.preferences.defaultEncoding = Encoding.Default.HeaderName;
                }

                if (settings.preferences.maximumFilterEntriesDisplayed == 0)
                {
                    settings.preferences.maximumFilterEntriesDisplayed = 20;
                }

                if (settings.preferences.maximumFilterEntries == 0)
                {
                    settings.preferences.maximumFilterEntries = 30;
                }

                ConvertSettings(settings);

                return settings;
            }
        }

        /// <summary>
        /// Saves the Settings to file, fires OnConfigChanged Event so LogTabWindow is updated
        /// </summary>
        /// <param name="settings">Settings to be saved</param>
        /// <param name="flags">Settings that "changed"</param>
        private void Save(Settings settings, SettingsFlags flags)
        {
            lock (_loadSaveLock)
            {
                _logger.Info("Saving settings");
                lock (this)
                {
                    string dir = File.Exists(PortableMode) ? Application.StartupPath : ConfigDir;

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    FileInfo fileInfo = new FileInfo(dir + "\\settings.json");
                    Save(fileInfo, settings);
                }

                OnConfigChanged(flags);
            }
        }

        /// <summary>
        /// Saves the file in any defined format
        /// </summary>
        /// <param name="fileInfo">FileInfo for creating the file (if exists will be overwritten)</param>
        /// <param name="settings">Current Settings</param>
        /// <param name="flags"></param>
        private void Save(FileInfo fileInfo, Settings settings)
        {
            SaveAsJSON(fileInfo, settings);
        }

        private void SaveAsJSON(FileInfo fileInfo, Settings settings)
        {
            settings.versionBuild = Assembly.GetExecutingAssembly().GetName().Version.Build;

            using (StreamWriter sw = new StreamWriter(fileInfo.Create()))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(sw, settings);
            }
        }

        /// <summary>
        /// Convert settings loaded from previous versions.
        /// </summary>
        /// <param name="settings"></param>
        private void ConvertSettings(Settings settings)
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
        /// Imports all or some of the settings/prefs stored in the input stream.
        /// This will overwrite appropriate parts of the current (own) settings with the imported ones.
        /// </summary>
        /// <param name="currentSettings"></param>
        /// <param name="fileInfo"></param>
        /// <param name="flags">Flags to indicate which parts shall be imported</param>
        private Settings Import(Settings currentSettings, FileInfo fileInfo, ExportImportFlags flags)
        {
            Settings importSettings = LoadOrCreateNew(fileInfo);
            Settings ownSettings = ObjectClone.Clone(currentSettings);
            Settings newSettings;

            // at first check for 'Other' as this are the most options.
            if ((flags & ExportImportFlags.Other) == ExportImportFlags.Other)
            {
                newSettings = ownSettings;
                newSettings.preferences = ObjectClone.Clone(importSettings.preferences);
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
                newSettings.preferences.columnizerMaskList = ReplaceOrKeepExisting(flags, ownSettings.preferences.columnizerMaskList, importSettings.preferences.columnizerMaskList);
            }
            if ((flags & ExportImportFlags.HighlightMasks) == ExportImportFlags.HighlightMasks)
            {
                newSettings.preferences.highlightMaskList = ReplaceOrKeepExisting(flags, ownSettings.preferences.highlightMaskList, importSettings.preferences.highlightMaskList);
            }
            if ((flags & ExportImportFlags.HighlightSettings) == ExportImportFlags.HighlightSettings)
            {
               newSettings.hilightGroupList = ReplaceOrKeepExisting(flags, ownSettings.hilightGroupList, importSettings.hilightGroupList);
            }
            if ((flags & ExportImportFlags.ToolEntries) == ExportImportFlags.ToolEntries)
            {
                newSettings.preferences.toolEntries = ReplaceOrKeepExisting(flags, ownSettings.preferences.toolEntries, importSettings.preferences.toolEntries);
            }

            return newSettings;
        }

        private static List<T> ReplaceOrKeepExisting<T>(ExportImportFlags flags, List<T> existingList, List<T> newList)
        {
            if ((flags & ExportImportFlags.KeepExisting) == ExportImportFlags.KeepExisting)
            {
                return existingList.Union(newList).ToList();                
            }

            return newList;
        }

        #endregion

        protected void OnConfigChanged(SettingsFlags flags)
        {
            ConfigChangedEventHandler handler = ConfigChanged;
            if (handler != null)
            {
                _logger.Info("Fire config changed event");
                handler(this, new ConfigChangedEventArgs(flags));
            }
        }

        internal delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs e);
    }
}