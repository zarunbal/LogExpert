using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Drawing;
using System.Collections;
using System.Reflection;
using NLog;

namespace LogExpert
{
    public class ConfigManager
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private static readonly object monitor = new object();
        private static ConfigManager instance;
        private readonly object loadSaveLock = new object();
        private Settings settings;

        #endregion

        #region cTor

        private ConfigManager()
        {
            settings = Load();
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
        
        public static string ConfigDir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LogExpert";

        public static Settings Settings => Instance.settings;

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
            _logger.Info("Loading settings");
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
                using (Stream fs = File.OpenRead(dir + "\\settings.dat"))
                {
                    try
                    {
                        return LoadOrCreateNew(fs);
                    }   
                    catch (Exception e)
                    {
                        _logger.Error(e,"Error loading settings: {0}");
                        return LoadOrCreateNew(null);
                    }
                }
            }
        }

        private Settings LoadOrCreateNew(Stream fs)
        {
            lock (loadSaveLock)
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
                    catch (SerializationException e)
                    {
                        _logger.Error(e, "Error while deserializing config data: ");
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

                if (settings.preferences.maximumFilterEntriesDisplayed == 0)
                {
                    settings.preferences.maximumFilterEntriesDisplayed = 20;
                }

                if (settings.preferences.maximumFilterEntries == 0)
                {
                    settings.preferences.maximumFilterEntries = 30;
                }

                ConvertSettings(settings, Assembly.GetExecutingAssembly().GetName().Version.Build);

                return settings;
            }
        }

        private void Save(Settings settings, SettingsFlags flags)
        {
            lock (loadSaveLock)
            {
                _logger.Info("Saving settings");
                lock (this)
                {
                    string dir = ConfigDir;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    using (Stream fs = new FileStream(dir + "\\settings.dat", FileMode.Create, FileAccess.Write))
                    {
                        Save(fs, settings, flags);
                    }
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
                _logger.Info("Fire config changed event");
                handler(this, new ConfigChangedEventArgs(flags));
            }
        }

        internal delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs e);
    }
}