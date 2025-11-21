using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security;
using System.Windows.Forms;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Classes.JsonConverters;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.EventArguments;
using LogExpert.Core.Interface;

using Newtonsoft.Json;

using NLog;

namespace LogExpert.Config;

public class ConfigManager : IConfigManager
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly Lock _monitor = new();
    private readonly Lock _loadSaveLock = new();
    private Settings _settings;

    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        Converters =
            {
                new ColumnizerJsonConverter(),
                new EncodingJsonConverter()
            },
        Formatting = Formatting.Indented,
        //This is needed for the BookmarkList and the Bookmark Overlay
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
    };

    private const string SETTINGS_FILE_NAME = "settings.json";

    #endregion

    #region cTor

    private ConfigManager ()
    {
        _settings = Load();
    }

    #endregion

    #region Events

    public event EventHandler<ConfigChangedEventArgs> ConfigChanged;

    #endregion

    #region Properties

    //TODO: Change to init
    public static ConfigManager Instance
    {
        get
        {
            lock (_monitor)
            {
                field ??= new ConfigManager();
            }

            return field;
        }
    }

    public string ConfigDir => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogExpert");

    /// <summary>
    /// Application.StartUpPath + portable
    /// </summary>
    public string PortableModeDir => Path.Join(Application.StartupPath, "portable");

    /// <summary>
    /// portableMode.json
    /// </summary>
    public string PortableModeSettingsFileName => "portableMode.json";

    public Settings Settings => Instance._settings;

    IConfigManager IConfigManager.Instance => Instance;

    //Action<object, ConfigChangedEventArgs> IConfigManager.ConfigChanged { get => ((IConfigManager)_instance).ConfigChanged; set => ((IConfigManager)_instance).ConfigChanged = value; }
    //public string PortableModeSettingsFileName => ((IConfigManager)_instance).PortableModeSettingsFileName;

    #endregion

    #region Public methods

    /// <summary>
    /// Saves the current settings with the specified flags.
    /// </summary>
    /// <remarks>The method saves the settings based on the provided <paramref name="flags"/>. Ensure that the
    /// flags are correctly set to avoid saving unintended settings.</remarks>
    /// <param name="flags">The flags that determine which settings to save. This parameter cannot be null.</param>
    [SupportedOSPlatform("windows")]
    public void Save (SettingsFlags flags)
    {
        Instance.Save(Settings, flags);
    }

    /// <summary>
    /// Exports the current instance data to the specified file.
    /// </summary>
    /// <remarks>The method saves the current instance data using the provided settings. Ensure that the file
    /// path specified in <paramref name="fileInfo"/> is accessible and writable.</remarks>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object representing the file to which the data will be exported. Cannot be null.</param>
    [SupportedOSPlatform("windows")]
    public void Export (FileInfo fileInfo)
    {
        Save(fileInfo, Settings);
    }

    /// <summary>
    /// Exports only the highlight settings to the specified file.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <param name="highlightSettings"></param>
    [SupportedOSPlatform("windows")]
    public void Export (FileInfo fileInfo, SettingsFlags highlightSettings)
    {
        Instance.Save(fileInfo, Settings, highlightSettings);
    }

    /// <summary>
    /// Import settings from a file.
    /// Returns ImportResult indicating success, error, or user confirmation requirement.
    /// </summary>
    /// <param name="fileInfo">The file to import from</param>
    /// <param name="importFlags">Flags controlling what to import</param>
    /// <returns>ImportResult with operation outcome</returns>
    [SupportedOSPlatform("windows")]
    public ImportResult Import (FileInfo fileInfo, ExportImportFlags importFlags)
    {
        _logger.Info($"Importing settings from: {fileInfo?.FullName ?? "null"}");

        // Validate import file exists
        if (fileInfo == null || !fileInfo.Exists)
        {
            _logger.Error($"Import file does not exist: {fileInfo?.FullName ?? "null"}");
            return ImportResult.Failed("Import Failed", $"Import file not found:\n{fileInfo?.FullName ?? "unknown"}");
        }

        // Try to load and validate the import file before applying
        Settings importedSettings;
        try
        {
            _logger.Info("Validating import file...");
            LoadResult loadResult = LoadOrCreateNew(fileInfo);

            // Handle any critical errors from loading
            if (loadResult.CriticalFailure)
            {
                return ImportResult.Failed("Import Failed", $"Import file is invalid or corrupted:\n\n{loadResult.CriticalMessage}\n\nImport cancelled.");
            }

            importedSettings = loadResult.Settings;
        }
        catch (Exception ex) when (ex is InvalidDataException or
                                         JsonSerializationException)
        {
            _logger.Error($"Import file is invalid or corrupted: {ex}");
            return ImportResult.Failed("Import Failed", $"Import file is invalid or corrupted:\n\n{ex.Message}\n\nImport cancelled.");
        }

        if (SettingsAreEmptyOrDefault(importedSettings))
        {
            _logger.Warn("Import file appears to contain empty or default settings");

            string confirmationMessage =
                "Warning: Import file appears to be empty or contains default settings.\n\n" +
                "This will overwrite your current configuration with empty settings.\n\n" +
                $"Import file: {fileInfo.Name}\n" +
                $"Filters: {importedSettings.FilterList?.Count ?? 0}\n" +
                $"History: {importedSettings.FileHistoryList?.Count ?? 0}\n" +
                $"Highlights: {importedSettings.Preferences?.HighlightGroupList?.Count ?? 0}\n\n" +
                "Continue with import?";

            return ImportResult.RequiresConfirmation("Confirm Import", confirmationMessage);
        }

        _logger.Info($"Importing: Filters={importedSettings.FilterList?.Count ?? 0}, " +
            $"History={importedSettings.FileHistoryList?.Count ?? 0}, " +
            $"Highlights={importedSettings.Preferences?.HighlightGroupList?.Count ?? 0}");

        // Proceed with import
        Instance._settings = Instance.Import(Instance._settings, fileInfo, importFlags);
        Save(SettingsFlags.All);

        _logger.Info("Import completed successfully");
        return ImportResult.Successful();
    }

    /// <summary>
    /// Imports the highlight settings from a file.
    /// Throws ArgumentNullException if fileInfo is null, this should not happen.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <param name="importFlags"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [SupportedOSPlatform("windows")]
    public void ImportHighlightSettings (FileInfo fileInfo, ExportImportFlags importFlags)
    {
        ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));

        Instance.Settings.Preferences.HighlightGroupList = Import(Instance.Settings.Preferences.HighlightGroupList, fileInfo, importFlags);
        Save(SettingsFlags.All);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads the Settings from file or creates new settings if the file does not exist.
    /// </summary>
    /// <returns></returns>
    private Settings Load ()
    {
        _logger.Info($"### {nameof(Load)}: Loading settings");

        string dir;

        if (!File.Exists(Path.Join(PortableModeDir, PortableModeSettingsFileName)))
        {
            _logger.Info($"### {nameof(Load)}: Load settings standard mode");
            dir = ConfigDir;
        }
        else
        {
            _logger.Info($"### {nameof(Load)}: Load settings portable mode");
            dir = Application.StartupPath;
        }

        if (!Directory.Exists(dir))
        {
            _ = Directory.CreateDirectory(dir);
        }

        LoadResult result;

        if (!File.Exists(Path.Join(dir, SETTINGS_FILE_NAME)))
        {
            result = LoadOrCreateNew(null);
        }
        else
        {
            try
            {
                FileInfo fileInfo = new(Path.Join(dir, SETTINGS_FILE_NAME));
                result = LoadOrCreateNew(fileInfo);
            }
            catch (IOException ex)
            {
                _logger.Error($"File system error: {ex.Message}");
                result = LoadOrCreateNew(null);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error($"Access denied: {ex}");
                result = LoadOrCreateNew(null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error($"Access denied: {ex}");
                result = LoadOrCreateNew(null);
            }
        }

        // Handle recovery notifications (if loaded from backup)
        if (result.LoadedFromBackup)
        {
            _logger.Info($"### {nameof(Load)}: Settings recovered from backup");
        }

        // Handle critical failures
        if (result.CriticalFailure)
        {
            _logger.Error($"### {nameof(Load)}: settings load failure. Set to default settings");
            result = LoadOrCreateNew(null);
        }

        return result.Settings;
    }

    /// <summary>
    /// Loads Settings of a given file or creates new settings if the file does not exist.
    /// Includes automatic backup recovery if main file is corrupted.
    /// Returns LoadResult with the settings and any recovery information.
    /// </summary>
    /// <param name="fileInfo">file that has settings saved</param>
    /// <returns>LoadResult containing loaded/created settings and status</returns>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="JsonSerializationException"></exception>
    private LoadResult LoadOrCreateNew (FileInfo fileInfo)
    {
        //TODO this needs to be refactord, its quite big
        lock (_loadSaveLock)
        {
            Settings settings = null;
            Exception loadException = null;

            if (fileInfo == null || !fileInfo.Exists)
            {
                _logger.Info("No settings file found, creating new default settings");
                settings = new Settings();
            }
            else
            {
                // Try loading main settings file
                try
                {
                    _logger.Info($"Loading settings from: {fileInfo.FullName}");
                    string json = File.ReadAllText(fileInfo.FullName);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        throw new InvalidDataException("Settings file is empty");
                    }

                    settings = JsonConvert.DeserializeObject<Settings>(json, _jsonSettings);

                    if (settings == null)
                    {
                        throw new JsonSerializationException("Deserialization returned null");
                    }

                    _logger.Info("Settings loaded successfully");
                }
                catch (Exception e) when (e is ArgumentException or
                                               ArgumentNullException or
                                               DirectoryNotFoundException or
                                               FileNotFoundException or
                                               IOException or
                                               InvalidDataException or
                                               NotSupportedException or
                                               PathTooLongException or
                                               UnauthorizedAccessException or
                                               SecurityException or
                                               JsonException or
                                               JsonSerializationException or
                                               JsonReaderException)
                {
                    _logger.Error($"Error deserializing settings.json: {e}");
                    loadException = e;

                    // Try loading from backup file
                    string backupFile = fileInfo.FullName + ".bak";
                    if (File.Exists(backupFile))
                    {
                        try
                        {
                            _logger.Warn($"Attempting to load from backup file: {backupFile}");
                            string backupJson = File.ReadAllText(backupFile);

                            if (!string.IsNullOrWhiteSpace(backupJson))
                            {
                                settings = JsonConvert.DeserializeObject<Settings>(backupJson, _jsonSettings);

                                if (settings != null)
                                {
                                    _logger.Info("Settings recovered from backup successfully");

                                    // Save corrupted file for analysis
                                    string corruptFile = fileInfo.FullName + ".corrupt";
                                    try
                                    {
                                        File.Copy(fileInfo.FullName, corruptFile, overwrite: true);
                                        _logger.Info($"Corrupted file saved to: {corruptFile}");
                                    }
                                    catch (Exception copyException) when (copyException is ArgumentException or
                                                                         ArgumentNullException or
                                                                         DirectoryNotFoundException or
                                                                         FileNotFoundException or
                                                                         IOException or
                                                                         NotSupportedException or
                                                                         PathTooLongException or
                                                                         UnauthorizedAccessException)

                                    {
                                        _logger.Warn($"Could not save corrupted file: {copyException.Message}");
                                    }

                                    // Return recovery result instead of showing MessageBox
                                    settings = InitializeSettings(settings);
                                    return LoadResult.FromBackup(
                                        settings,
                                        "Settings file was corrupted but recovered from backup.\n\n" +
                                        $"Original error: {e.Message}\n\n" +
                                        $"A copy of the corrupted file has been saved as:\n{corruptFile}",
                                        "Settings Recovered from Backup");
                                }
                            }
                        }
                        catch (Exception backupException) when (backupException is ArgumentException or
                                                                                   ArgumentNullException or
                                                                                   DirectoryNotFoundException or
                                                                                   FileNotFoundException or
                                                                                   IOException or
                                                                                   NotSupportedException or
                                                                                   PathTooLongException or
                                                                                   UnauthorizedAccessException or
                                                                                   SecurityException)
                        {
                            _logger.Error($"Backup file also corrupted: {backupException}");
                        }
                    }
                    else
                    {
                        _logger.Error("No backup file available for recovery");
                    }
                }
            }

            // If all loading attempts failed, return critical failure result
            if (settings == null)
            {
                if (loadException != null)
                {
                    _logger.Error("All attempts to load settings failed");

                    // Create new settings for critical failure case
                    settings = new Settings();
                    settings = InitializeSettings(settings);

                    return LoadResult.Critical(
                        settings,
                        "Critical: Settings Load Failed",
                        "Failed to load settings file. All configuration will be lost if you continue.\n\n" +
                        $"Error: {loadException.Message}\n\n" +
                        "Do you want to:\n" +
                        "YES - Create new settings (loses all configuration)\n" +
                        "NO - Exit application (allows manual recovery)\n\n" +
                        "Your corrupted settings file will be preserved for manual recovery.");
                }

                settings = new Settings();
            }

            settings = InitializeSettings(settings);
            return LoadResult.Success(settings);
        }
    }

    /// <summary>
    /// Initialize settings with required default values
    /// </summary>
    private static Settings InitializeSettings (Settings settings)
    {
        settings.Preferences ??= new Preferences();
        settings.Preferences.ToolEntries ??= [];
        settings.Preferences.ColumnizerMaskList ??= [];

        settings.FileHistoryList ??= [];

        settings.LastOpenFilesList ??= [];

        settings.FileColors ??= [];

        try
        {
            using var fontFamily = new FontFamily(settings.Preferences.FontName);
            settings.Preferences.FontName = fontFamily.Name;
        }
        catch (ArgumentException)
        {
            string genericMonospaceFont = FontFamily.GenericMonospace.Name;
            _logger.Warn($"Specified font '{settings.Preferences.FontName}' not found. Falling back to default: '{genericMonospaceFont}'.");
            settings.Preferences.FontName = genericMonospaceFont;
        }

        if (settings.Preferences.ShowTailColor == Color.Empty)
        {
            settings.Preferences.ShowTailColor = Color.FromKnownColor(KnownColor.Blue);
        }

        if (settings.Preferences.TimeSpreadColor == Color.Empty)
        {
            settings.Preferences.TimeSpreadColor = Color.Gray;
        }

        if (settings.Preferences.BufferCount < 10)
        {
            settings.Preferences.BufferCount = 100;
        }

        if (settings.Preferences.LinesPerBuffer < 1)
        {
            settings.Preferences.LinesPerBuffer = 500;
        }

        settings.FilterList ??= [];

        settings.SearchHistoryList ??= [];

        settings.FilterHistoryList ??= [];

        settings.FilterRangeHistoryList ??= [];

        foreach (FilterParams filterParams in settings.FilterList)
        {
            filterParams.Init();
        }

        if (settings.Preferences.HighlightGroupList == null)
        {
            settings.Preferences.HighlightGroupList = [];
        }

        settings.Preferences.HighlightMaskList ??= [];

        if (settings.Preferences.PollingInterval < 20)
        {
            settings.Preferences.PollingInterval = 250;
        }

        settings.Preferences.MultiFileOptions ??= new MultiFileOptions();

        settings.Preferences.DefaultEncoding ??= System.Text.Encoding.Default.HeaderName;

        settings.Preferences.DefaultLanguage ??= CultureInfo.GetCultureInfo("en-US").Name;

        if (settings.Preferences.MaximumFilterEntriesDisplayed == 0)
        {
            settings.Preferences.MaximumFilterEntriesDisplayed = 20;
        }

        if (settings.Preferences.MaximumFilterEntries == 0)
        {
            settings.Preferences.MaximumFilterEntries = 30;
        }

        SetBoundsWithinVirtualScreen(settings);

        return settings;
    }

    /// <summary>
    /// Saves the Settings to file, fires OnConfigChanged Event so LogTabWindow is updated
    /// </summary>
    /// <param name="settings">Settings to be saved</param>
    /// <param name="flags">Settings that "changed"</param>
    [SupportedOSPlatform("windows")]
    private void Save (Settings settings, SettingsFlags flags)
    {
        lock (_loadSaveLock)
        {
            string dir = Settings.Preferences.PortableMode ? Application.StartupPath : ConfigDir;

            if (!Directory.Exists(dir))
            {
                _ = Directory.CreateDirectory(dir);
            }

            FileInfo fileInfo = new(dir + Path.DirectorySeparatorChar + SETTINGS_FILE_NAME);
            Save(fileInfo, settings);

            OnConfigChanged(flags);
        }
    }

    /// <summary>
    /// Saves the file in any defined format
    /// </summary>
    /// <param name="fileInfo">FileInfo for creating the file (if exists will be overwritten)</param>
    /// <param name="settings">Current Settings</param>
    private void Save (FileInfo fileInfo, Settings settings)
    {
        //Currently only fileFormat, maybe add some other formats later (YAML or XML?)
        SaveAsJSON(fileInfo, settings);
    }

    private void Save (FileInfo fileInfo, Settings settings, SettingsFlags flags)
    {
        switch (flags)
        {
            case SettingsFlags.HighlightSettings:
                SaveHighlightGroupsAsJSON(fileInfo, settings.Preferences.HighlightGroupList);
                break;
            case SettingsFlags.None:
                // No action required for SettingsFlags.None
                break;
            case SettingsFlags.WindowPosition:
                // No action required for SettingsFlags.WindowPosition
                break;
            case SettingsFlags.FileHistory:
                // No action required for SettingsFlags.FileHistory
                break;
            case SettingsFlags.FilterList:
                // No action required for SettingsFlags.FilterList
                break;
            case SettingsFlags.RegexHistory:
                // No action required for SettingsFlags.RegexHistory
                break;
            case SettingsFlags.ToolSettings:
                // No action required for SettingsFlags.ToolSettings
                break;
            case SettingsFlags.GuiOrColors:
                // No action required for SettingsFlags.GuiOrColors
                break;
            case SettingsFlags.FilterHistory:
                // No action required for SettingsFlags.FilterHistory
                break;
            case SettingsFlags.All:
                // No action required for SettingsFlags.All
                break;
            case SettingsFlags.Settings:
                // No action required for SettingsFlags.Settings
                break;
            default:
                break;
        }

        OnConfigChanged(flags);
    }

    /// <summary>
    /// Saves the settings as JSON file.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <param name="settings"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void SaveAsJSON (FileInfo fileInfo, Settings settings)
    {
        if (!ValidateSettings(settings))
        {
            _logger.Error("Settings validation failed - refusing to save");
            throw new InvalidOperationException("Settings validation failed - refusing to save potentially corrupted data");
        }

        settings.VersionBuild = Assembly.GetExecutingAssembly().GetName().Version.Build;
        string json = JsonConvert.SerializeObject(settings, _jsonSettings);

        _logger.Info($"Saving settings: " +
            $"Filters={settings.FilterList?.Count ?? 0}, " +
            $"History={settings.FileHistoryList?.Count ?? 0}, " +
            $"Highlights={settings.Preferences?.HighlightGroupList?.Count ?? 0}, " +
            $"Size={json.Length} bytes");

        WriteSettingsFile(fileInfo, json);
    }

    private static void WriteSettingsFile (FileInfo fileInfo, string json)
    {
        string tempFile = fileInfo.FullName + ".tmp";
        string backupFile = fileInfo.FullName + ".bak";

        try
        {
            _logger.Info($"Writing to {fileInfo.FullName}");
            File.WriteAllText(tempFile, json, System.Text.Encoding.UTF8);

            if (File.Exists(fileInfo.FullName))
            {
                long existingSize = new FileInfo(fileInfo.FullName).Length;
                if (existingSize > 0)
                {
                    File.Copy(fileInfo.FullName, backupFile, overwrite: true);
                    _logger.Info($"Created backup: {backupFile} ({existingSize} bytes)");
                }
                else
                {
                    _logger.Warn($"Existing settings file is empty ({existingSize} bytes), skipping backup");
                }
            }

            File.Move(tempFile, fileInfo.FullName, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save settings: {ex}");

            // Attempt recovery: restore from backup if main file was corrupted
            try
            {
                if (File.Exists(backupFile))
                {
                    var mainFileExists = File.Exists(fileInfo.FullName);
                    var mainFileSize = mainFileExists ? new FileInfo(fileInfo.FullName).Length : 0;

                    if (!mainFileExists || mainFileSize == 0)
                    {
                        File.Copy(backupFile, fileInfo.FullName, overwrite: true);
                        _logger.Warn("Settings save failed, restored from backup");
                    }
                }
            }
            catch (Exception recoverException) when (recoverException is ArgumentException or
                                                                         ArgumentNullException or
                                                                         DirectoryNotFoundException or
                                                                         FileNotFoundException or
                                                                         IOException or
                                                                         NotSupportedException or
                                                                         PathTooLongException or
                                                                         UnauthorizedAccessException)
            {
                _logger.Error($"Failed to recover from backup: {recoverException}");
            }

            throw;
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception cleanUpException) when (cleanUpException is ArgumentException or
                                                                             DirectoryNotFoundException or
                                                                             IOException or
                                                                             NotSupportedException or
                                                                             PathTooLongException or
                                                                             UnauthorizedAccessException)
                {
                    _logger.Warn($"Failed to clean up temp file: {cleanUpException.Message}");
                }
            }
        }
    }

    private static void SaveHighlightGroupsAsJSON (FileInfo fileInfo, List<HighlightGroup> groups)
    {
        using StreamWriter sw = new(fileInfo.Create());
        JsonSerializer serializer = new();
        serializer.Serialize(sw, groups);
    }

    /// <summary>
    /// Imports only the highlight groups from the specified file.
    /// </summary>
    /// <param name="currentGroups"></param>
    /// <param name="fileInfo"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    private static List<HighlightGroup> Import (List<HighlightGroup> currentGroups, FileInfo fileInfo, ExportImportFlags flags)
    {
        List<HighlightGroup> newGroups;

        try
        {
            newGroups = JsonConvert.DeserializeObject<List<HighlightGroup>>(File.ReadAllText($"{fileInfo.FullName}"));
        }
        catch (Exception e) when (e is ArgumentException or
                                       ArgumentNullException or
                                       DirectoryNotFoundException or
                                       FileNotFoundException or
                                       IOException or
                                       NotSupportedException or
                                       PathTooLongException or
                                       UnauthorizedAccessException or
                                       SecurityException)
        {
            _logger.Error($"Error while deserializing config data: {e}");
            newGroups = [];
        }

        if (flags.HasFlag(ExportImportFlags.KeepExisting))
        {
            currentGroups.AddRange(newGroups);
        }
        else
        {
            currentGroups.Clear();
            currentGroups.AddRange(newGroups);
        }

        return currentGroups;
    }

    /// <summary>
    /// Imports all or some of the settings/preferences stored in the input stream.
    /// This will overwrite appropriate parts of the current (own) settings with the imported ones.
    /// </summary>
    /// <param name="currentSettings"></param>
    /// <param name="fileInfo"></param>
    /// <param name="flags">Flags to indicate which parts shall be imported</param>
    [SupportedOSPlatform("windows")]
    private Settings Import (Settings currentSettings, FileInfo fileInfo, ExportImportFlags flags)
    {
        LoadResult loadResult = LoadOrCreateNew(fileInfo);
        Settings importSettings = loadResult.Settings;
        Settings ownSettings = ObjectClone.Clone(currentSettings);
        Settings newSettings;

        // at first check for 'Other' as this are the most options.
        if ((flags & ExportImportFlags.Other) == ExportImportFlags.Other)
        {
            newSettings = ownSettings;
            newSettings.Preferences = ObjectClone.Clone(importSettings.Preferences);
            newSettings.Preferences.ColumnizerMaskList = ownSettings.Preferences.ColumnizerMaskList;
            newSettings.Preferences.HighlightMaskList = ownSettings.Preferences.HighlightMaskList;
            newSettings.Preferences.HighlightGroupList = ownSettings.Preferences.HighlightGroupList;
            newSettings.Preferences.ToolEntries = ownSettings.Preferences.ToolEntries;
        }
        else
        {
            newSettings = ownSettings;
        }

        if ((flags & ExportImportFlags.ColumnizerMasks) == ExportImportFlags.ColumnizerMasks)
        {
            newSettings.Preferences.ColumnizerMaskList = ReplaceOrKeepExisting(flags, ownSettings.Preferences.ColumnizerMaskList, importSettings.Preferences.ColumnizerMaskList);
        }

        if ((flags & ExportImportFlags.HighlightMasks) == ExportImportFlags.HighlightMasks)
        {
            newSettings.Preferences.HighlightMaskList = ReplaceOrKeepExisting(flags, ownSettings.Preferences.HighlightMaskList, importSettings.Preferences.HighlightMaskList);
        }

        if ((flags & ExportImportFlags.HighlightSettings) == ExportImportFlags.HighlightSettings)
        {
            newSettings.Preferences.HighlightGroupList = ReplaceOrKeepExisting(flags, ownSettings.Preferences.HighlightGroupList, importSettings.Preferences.HighlightGroupList);
        }

        if ((flags & ExportImportFlags.ToolEntries) == ExportImportFlags.ToolEntries)
        {
            newSettings.Preferences.ToolEntries = ReplaceOrKeepExisting(flags, ownSettings.Preferences.ToolEntries, importSettings.Preferences.ToolEntries);
        }

        return newSettings;
    }

    /// <summary>
    /// Replaces the existing list with the new list or keeps existing entries based on the flags.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="flags"></param>
    /// <param name="existingList"></param>
    /// <param name="newList"></param>
    /// <returns></returns>
    private static List<T> ReplaceOrKeepExisting<T> (ExportImportFlags flags, List<T> existingList, List<T> newList)
    {
        return (flags & ExportImportFlags.KeepExisting) == ExportImportFlags.KeepExisting
            ? [.. existingList.Union(newList)]
            : newList;
    }

    // Checking if the appBounds values are outside the current virtual screen.
    // If so, the appBounds values are set to 0.
    [SupportedOSPlatform("windows")]
    private static void SetBoundsWithinVirtualScreen (Settings settings)
    {
        Rectangle vs = SystemInformation.VirtualScreen;
        if (vs.X + vs.Width < settings.AppBounds.X + settings.AppBounds.Width ||
            vs.Y + vs.Height < settings.AppBounds.Y + settings.AppBounds.Height)
        {
            settings.AppBounds = new Rectangle();
        }
    }

    /// <summary>
    /// Checks if settings object appears to be empty or default.
    /// This helps detect corrupted or uninitialized settings files.
    /// </summary>
    /// <param name="settings">Settings object to validate</param>
    /// <returns>True if settings appear empty/default, false if they contain user data</returns>
    private static bool SettingsAreEmptyOrDefault (Settings settings)
    {
        if (settings == null)
        {
            return true;
        }

        if (settings.Preferences == null)
        {
            return true;
        }

        var filterCount = settings.FilterList?.Count ?? 0;
        var historyCount = settings.FileHistoryList?.Count ?? 0;
        var searchHistoryCount = settings.SearchHistoryList?.Count ?? 0;
        var highlightCount = settings.Preferences.HighlightGroupList?.Count ?? 0;
        var columnizerMaskCount = settings.Preferences.ColumnizerMaskList?.Count ?? 0;

        return filterCount == 0 &&
               historyCount == 0 &&
               searchHistoryCount == 0 &&
               highlightCount == 0 &&
               columnizerMaskCount == 0;
    }

    /// <summary>
    /// Validates settings object for basic integrity.
    /// Logs warnings for suspicious conditions.
    /// </summary>
    /// <param name="settings">Settings to validate</param>
    /// <returns>True if settings pass validation</returns>
    private bool ValidateSettings (Settings settings)
    {
        if (settings == null)
        {
            _logger.Error("Attempted to save null settings");
            return false;
        }

        if (settings.Preferences == null)
        {
            _logger.Error("Settings.Preferences is null");
            return false;
        }

        if (SettingsAreEmptyOrDefault(settings))
        {
            _logger.Warn("Settings appear to be empty - this may indicate data loss");

            if (_settings != null && !SettingsAreEmptyOrDefault(_settings))
            {
                _logger.Warn($"Previous settings: " +
                    $"Filters={_settings.FilterList?.Count ?? 0}, " +
                    $"History={_settings.FileHistoryList?.Count ?? 0}, " +
                    $"SearchHistory={_settings.SearchHistoryList?.Count ?? 0}, " +
                    $"Highlights={_settings.Preferences?.HighlightGroupList?.Count ?? 0}");
            }
        }

        return true;
    }
    #endregion

    /// <summary>
    /// Fires the ConfigChanged event
    /// </summary>
    /// <param name="flags"></param>
    protected void OnConfigChanged (SettingsFlags flags)
    {
        ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(flags));
    }
}