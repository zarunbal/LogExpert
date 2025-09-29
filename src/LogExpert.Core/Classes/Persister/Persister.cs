using System.Text;

using LogExpert.Core.Config;

using Newtonsoft.Json;

using NLog;

namespace LogExpert.Core.Classes.Persister;

public static class Persister
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public methods

    /// <summary>
    /// Saves the specified persistence data to a file and returns the file name used.
    /// </summary>
    /// <remarks>If the <see cref="PersistenceData.SessionFileName"/> property of <paramref
    /// name="persistenceData"/> is not set, a file name is generated based on <paramref name="logFileName"/> and the
    /// provided <paramref name="preferences"/>. If the save location specified in <paramref name="preferences"/> is
    /// <see cref="SessionSaveLocation.SameDir"/>, the file name is adjusted to be relative to the log file's
    /// directory.</remarks>
    /// <param name="logFileName">The name of the log file associated with the session. This is used to generate the file name if one is not
    /// provided in <paramref name="persistenceData"/>.</param>
    /// <param name="persistenceData">The persistence data to save. This parameter cannot be <see langword="null"/>.</param>
    /// <param name="preferences">The user preferences that determine the save location and other settings. This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <returns>The full path of the file where the persistence data was saved.</returns>
    public static string SavePersistenceData (string logFileName, PersistenceData persistenceData, Preferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        ArgumentNullException.ThrowIfNull(persistenceData);

        var fileName = persistenceData.SessionFileName ?? BuildPersisterFileName(logFileName, preferences);

        if (preferences.SaveLocation == SessionSaveLocation.SameDir)
        {
            // make to log file in .lxp file relative
            var filePart = Path.GetFileName(persistenceData.FileName);
            persistenceData.FileName = filePart;
        }

        Save(fileName, persistenceData);
        return fileName;
    }

    /// <summary>
    /// Saves the specified persistence data to a file with the given name.
    /// </summary>
    /// <param name="persistenceFileName">The name of the file to save the persistence data to. Must not be null or empty.</param>
    /// <param name="persistenceData">The persistence data to be saved. Must not be null.</param>
    /// <returns>The name of the file where the persistence data was saved.</returns>
    public static string SavePersistenceDataWithFixedName (string persistenceFileName, PersistenceData persistenceData)
    {
        Save(persistenceFileName, persistenceData);
        return persistenceFileName;
    }

    /// <summary>
    /// Loads persistence data from the specified log file using the provided preferences.
    /// </summary>
    /// <param name="logFileName">The name of the log file to load persistence data from. This value cannot be null.</param>
    /// <param name="preferences">The preferences used to determine the file path and loading behaviour. This value cannot be null.</param>
    /// <returns>The loaded <see cref="PersistenceData"/> object containing the persistence information.</returns>
    public static PersistenceData LoadPersistenceData (string logFileName, Preferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        var fileName = BuildPersisterFileName(logFileName, preferences);
        return LoadInternal(fileName);
    }

    /// <summary>
    /// Loads persistence data based on the specified log file name and preferences.
    /// </summary>
    /// <param name="logFileName">The name of the log file used to determine the persistence data file.</param>
    /// <param name="preferences">The preferences that influence the file name generation. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="PersistenceData"/> object containing the loaded data.</returns>
    public static PersistenceData LoadPersistenceDataOptionsOnly (string logFileName, Preferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        var fileName = BuildPersisterFileName(logFileName, preferences);
        return LoadInternal(fileName);
    }

    /// <summary>
    /// Loads persistence data options from a specified file.
    /// </summary>
    /// <remarks>This method only loads the options portion of the persistence data from the specified file.
    /// Ensure the file format is valid and compatible with the expected structure of <see
    /// cref="PersistenceData"/>.</remarks>
    /// <param name="persistenceFile">The path to the file containing the persistence data options. The file must exist and be accessible.</param>
    /// <returns>A <see cref="PersistenceData"/> object containing the loaded options.</returns>
    public static PersistenceData LoadPersistenceDataOptionsOnlyFromFixedFile (string persistenceFile)
    {
        return LoadInternal(persistenceFile);
    }

    /// <summary>
    /// Loads persistence data from the specified file.
    /// </summary>
    /// <param name="persistenceFile">The path to the file containing the persistence data. The file must exist and be accessible.</param>
    /// <returns>A <see cref="PersistenceData"/> object containing the data loaded from the file.</returns>
    public static PersistenceData LoadPersistenceDataFromFixedFile (string persistenceFile)
    {
        return LoadInternal(persistenceFile);
    }

    /// <summary>
    /// Loads persistence data from the specified file.
    /// </summary>
    /// <param name="fileName">The path to the file containing the persistence data. The file must exist and be accessible.</param>
    /// <returns>A <see cref="PersistenceData"/> object representing the data loaded from the file.</returns>
    public static PersistenceData Load (string fileName)
    {
        return LoadInternal(fileName);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Constructs the file path for the persister file based on the specified log file name and preferences.
    /// </summary>
    /// <remarks>The method determines the save location for the persister file based on the <see
    /// cref="Preferences.SaveLocation"/> property. If the specified directory does not exist, the method attempts to
    /// create it. If directory creation fails, an error is logged.</remarks>
    /// <param name="logFileName">The name of the log file for which the persister file path is being generated.</param>
    /// <param name="preferences">The preferences that determine the save location and directory structure for the persister file.</param>
    /// <returns>The full file path of the persister file, including the directory and file name, based on the specified log file
    /// name and preferences.</returns>
    private static string BuildPersisterFileName (string logFileName, Preferences preferences)
    {
        string dir;
        string file;

        switch (preferences.SaveLocation)
        {
            case SessionSaveLocation.SameDir:
            default:
                {
                    FileInfo fileInfo = new(logFileName);
                    dir = fileInfo.DirectoryName;
                    file = fileInfo.DirectoryName + Path.DirectorySeparatorChar + fileInfo.Name + ".lxp";
                    break;
                }
            case SessionSaveLocation.DocumentsDir:
                {
                    dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                          Path.DirectorySeparatorChar +
                          "LogExpert";
                    file = dir + Path.DirectorySeparatorChar + BuildSessionFileNameFromPath(logFileName);
                    break;
                }
            case SessionSaveLocation.OwnDir:
                {
                    dir = preferences.SessionSaveDirectory;
                    file = dir + Path.DirectorySeparatorChar + BuildSessionFileNameFromPath(logFileName);
                    break;
                }
            case SessionSaveLocation.ApplicationStartupDir:
                {
                    //TODO Add Application.StartupPath as Variable
                    dir = string.Empty;// Application.StartupPath + Path.DirectorySeparatorChar + "sessionfiles";
                    file = dir + Path.DirectorySeparatorChar + BuildSessionFileNameFromPath(logFileName);
                    break;
                }
        }

        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            try
            {
                _ = Directory.CreateDirectory(dir);
            }
            catch (Exception ex) when (ex is IOException or
                                             UnauthorizedAccessException or
                                             PathTooLongException or
                                             DirectoryNotFoundException)
            {
                _logger.Error(ex, $"Error creating directory {dir}");
            }
        }

        return file;
    }

    /// <summary>
    /// Generates a session file name based on the specified log file path.
    /// </summary>
    /// <param name="logFileName">The full path of the log file to be converted into a session file name.</param>
    /// <returns>A string representing the session file name, where directory, volume, and path separators are replaced with
    /// underscores, and the file name is appended with the ".lxp" extension.</returns>
    private static string BuildSessionFileNameFromPath (string logFileName)
    {
        var result = logFileName;
        result = result.Replace(Path.DirectorySeparatorChar, '_');
        result = result.Replace(Path.AltDirectorySeparatorChar, '_');
        result = result.Replace(Path.VolumeSeparatorChar, '_');
        result += ".lxp";
        return result;
    }

    /// <summary>
    /// Saves the specified persistence data to a file in JSON format.
    /// </summary>
    /// <remarks>The method serializes the <paramref name="persistenceData"/> object to JSON using specific
    /// settings,  including a custom JSON converter. The resulting JSON is written to the specified file with UTF-8
    /// encoding.</remarks>
    /// <param name="fileName">The full path of the file where the data will be saved. This cannot be null or empty.</param>
    /// <param name="persistenceData">The data to be persisted. This cannot be null.</param>
    private static void Save (string fileName, PersistenceData persistenceData)
    {
        var settings = new JsonSerializerSettings
        {
            Converters =
            {
                new ColumnizerJsonConverter()
            },
            Formatting = Formatting.Indented,
        };

        try
        {
            var json = JsonConvert.SerializeObject(persistenceData, settings);
            File.WriteAllText(fileName, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error saving persistence data to {fileName}");
            throw;
        }
    }

    /// <summary>
    /// Loads persistence data from the specified file.
    /// </summary>
    /// <remarks>This method attempts to deserialize the file's contents into a <see cref="PersistenceData"/>
    /// object using JSON. If the deserialization is successful, it initializes any filter parameters within the loaded
    /// data. If an error occurs during file access or deserialization, the method logs the error and returns <see
    /// langword="null"/>.</remarks>
    /// <param name="fileName">The full path to the file containing the persistence data. The file must exist and be accessible.</param>
    /// <returns>An instance of <see cref="PersistenceData"/> containing the deserialized data from the file, or <see
    /// langword="null"/> if the file does not exist, an error occurs during loading, or the data is invalid.</returns>
    private static PersistenceData LoadInternal (string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }

        try
        {
            var settings = new JsonSerializerSettings
            {
                Converters =
                {
                    new ColumnizerJsonConverter()
                },
                Formatting = Formatting.Indented,
            };

            var json = File.ReadAllText(fileName, Encoding.UTF8);
            var data = JsonConvert.DeserializeObject<PersistenceData>(json, settings);
            // Call Init on all FilterParams if needed
            if (data?.FilterParamsList != null)
            {
                foreach (var filter in data.FilterParamsList)
                {
                    filter?.Init();
                }
            }

            if (data?.FilterTabDataList != null)
            {
                foreach (var tab in data.FilterTabDataList)
                {
                    tab?.FilterParams?.Init();
                }
            }

            return data;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or
                                         IOException)
        {
            _logger.Error(ex, $"Error loading persistence data from {fileName}");
            return null;
        }
    }

    #endregion
}