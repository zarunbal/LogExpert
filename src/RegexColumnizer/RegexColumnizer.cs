using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using ColumnizerLib;

using LogExpert;
using LogExpert.Core.Helpers;

using Newtonsoft.Json;

[assembly: SupportedOSPlatform("windows")]
namespace RegexColumnizer;

public abstract class BaseRegexColumnizer : ILogLineColumnizer, IColumnizerConfigurator
{
    #region Fields

    private readonly XmlSerializer _xml = new(typeof(RegexColumnizerConfig));
    private string[] columns;
    private RegexColumnizerConfig _config;
    #endregion

    #region Properties

    public Regex Regex { get; private set; }

    #endregion

    #region Public methods

    public string GetName ()
    {
        return string.IsNullOrWhiteSpace(_config?.Name)
            ? GetNameInternal()
            : _config.Name;
    }

    public string GetCustomName ()
    {
        return string.IsNullOrWhiteSpace(_config?.CustomName)
            ? GetNameInternal()
            : _config.CustomName;
    }

    public string GetDescription () => "Columns are filled by regular expression named capture groups";

    public int GetColumnCount () => columns.Length;

    public string[] GetColumnNames () => columns;

    public IColumnizedLogLine SplitLine (ILogLineColumnizerCallback callback, ILogLine line)
    {
        var logLine = new ColumnizedLogLine
        {
            ColumnValues = new IColumn[columns.Length]
        };

        if (Regex != null)
        {
            var m = Regex.Match(line.FullLine);

            if (m.Success)
            {
                for (var i = m.Groups.Count - 1; i > 0; i--)
                {
                    logLine.ColumnValues[i - 1] = new Column
                    {
                        Parent = logLine,
                        FullValue = m.Groups[i].Value
                    };
                }
            }
            else
            {
                //Move non matching lines in the last column
                logLine.ColumnValues[columns.Length - 1] = new Column
                {
                    Parent = logLine,
                    FullValue = line.FullLine
                };


                //Fill other columns with empty string to avoid null pointer exceptions in unexpected places
                for (var i = 0; i < columns.Length - 1; i++)
                {
                    logLine.ColumnValues[i] = new Column
                    {
                        Parent = logLine,
                        FullValue = string.Empty
                    };
                }
            }
        }
        else
        {
            IColumn colVal = new Column
            {
                Parent = logLine,
                FullValue = line.FullLine
            };

            logLine.ColumnValues[0] = colVal;
        }

        logLine.LogLine = line;
        return logLine;
    }

    public bool IsTimeshiftImplemented () => false;

    public void SetTimeOffset (int msecOffset)
    {
        throw new NotImplementedException();
    }

    public int GetTimeOffset ()
    {
        throw new NotImplementedException();
    }

    public DateTime GetTimestamp (ILogLineColumnizerCallback callback, ILogLine line)
    {
        throw new NotImplementedException();
    }

    public void PushValue (ILogLineColumnizerCallback callback, int column, string value, string oldValue)
    {
        throw new NotImplementedException();
    }

    public void Configure (ILogLineColumnizerCallback callback, string configDir)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(configDir))
        {
            throw new ArgumentException("Configuration directory cannot be null or empty", nameof(configDir));
        }

        string name = GetName();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Columnizer name cannot be null or empty");
        }

        // Ensure directory exists
        if (!Directory.Exists(configDir))
        {
            try
            {
                _ = Directory.CreateDirectory(configDir);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _ = MessageBox.Show($"Failed to create configuration directory: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        string filePath = Path.Combine(configDir, $"{name}.json");

        RegexColumnizerConfigDialog dlg = new(_config);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            try
            {
                // Only validate regex if expression is provided (empty is allowed and uses default)
                if (!string.IsNullOrWhiteSpace(dlg.Config.Expression))
                {
                    // Test regex compilation to catch errors early
                    _ = RegexHelper.CreateSafeRegex(dlg.Config.Expression);
                }

                // Save configuration
                string json = JsonConvert.SerializeObject(dlg.Config, Formatting.Indented);
                File.WriteAllText(filePath, json);

                _config = dlg.Config;
                Init();
            }
            catch (RegexMatchTimeoutException ex)
            {
                _ = MessageBox.Show($"Regex pattern may cause performance issues: {ex.Message}", "Configuration Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                _ = MessageBox.Show($"Invalid regex pattern: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _ = MessageBox.Show($"Failed to save configuration: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public void LoadConfig (string configDir)
    {
        var configFile = GetConfigFileJSON(configDir);

        if (!File.Exists(configFile))
        {
            configFile = GetConfigFileXML(configDir);

            if (!File.Exists(configFile))
            {
                _config = new RegexColumnizerConfig
                {
                    Name = GetName()
                };
            }
            else
            {
                try
                {
                    using var reader = new StreamReader(configFile);
                    _config = _xml.Deserialize(reader) as RegexColumnizerConfig;
                }
                catch (Exception ex) when (ex is InvalidOperationException or
                                                 IOException or
                                                 ArgumentException or
                                                 ArgumentNullException or
                                                 FileNotFoundException or
                                                 DirectoryNotFoundException)
                {
                    _ = MessageBox.Show(ex.Message, "Deserialize");
                    _config = new RegexColumnizerConfig
                    {
                        Name = GetName()
                    };
                }
            }
        }
        else
        {
            try
            {
                string jsonContent = File.ReadAllText(configFile);

                _config = JsonConvert.DeserializeObject<RegexColumnizerConfig>(jsonContent)
                    ?? new RegexColumnizerConfig { Name = GetName() };

            }
            catch (JsonException ex)
            {
                _ = MessageBox.Show(ex.Message, "Deserialize");
                _config = new RegexColumnizerConfig
                {
                    Name = GetName()
                };
            }
        }

        Init();
    }

    /// <summary>
    /// Validates that the columnizer name contains no path separators or invalid characters
    /// to prevent path traversal attacks (SEC-02)
    /// </summary>
    private static void ValidateColumnizerName (string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Columnizer name cannot be null or empty");
        }

        // Check for path separators (both Windows and Unix)
        if (name.Contains(Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            name.Contains(Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            name.Contains('/', StringComparison.OrdinalIgnoreCase) ||
            name.Contains('\\', StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Columnizer name '{name}' contains path separators which are not allowed");
        }

        // Check for invalid filename characters
        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (name.IndexOfAny(invalidChars) >= 0)
        {
            throw new InvalidOperationException($"Columnizer name '{name}' contains invalid filename characters");
        }

        // Check for path traversal patterns
        if (name.Contains("..", StringComparison.OrdinalIgnoreCase) || name.Contains('~', StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Columnizer name '{name}' contains path traversal patterns which are not allowed");
        }
    }

    [Obsolete("XML Configuration is deprecated, use JSON instead")]
    private string GetConfigFileXML (string configDir)
    {
        var name = GetType().Name;
        ValidateColumnizerName(name);

        var configPath = Path.Join(configDir, name);
        configPath = Path.ChangeExtension(configPath, "xml");
        return configPath;
    }

    private string GetConfigFileJSON (string configDir)
    {
        var name = GetType().Name;
        ValidateColumnizerName(name);

        var configPath = Path.Join(configDir, name);
        configPath = Path.ChangeExtension(configPath, "json");
        return configPath;
    }

    #endregion

    /// <summary>
    /// ToString, this is displayed in the columnizer picker combobox only in the FilterSelectionDialog
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
        return GetName();
    }

    #region Private Methods

    protected abstract string GetNameInternal ();

    public void Init ()
    {
        try
        {
            Regex = RegexHelper.GetOrCreateCached(_config.Expression, RegexOptions.Compiled);
            var skip = Regex.GetGroupNames().Length == 1 ? 0 : 1;
            columns = [.. Regex.GetGroupNames().Skip(skip)];
        }
        catch (Exception ex)
        {
            Regex = null;
            columns = ["text"];
        }
    }

    #endregion
}

#region RegexColumnizer Implementations

public class Regex1Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex1";
}

public class Regex2Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex2";
}

public class Regex3Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex3";
}

public class Regex4Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex4";
}

public class Regex5Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex5";
}

public class Regex6Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex6";
}

public class Regex7Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex7";
}

public class Regex8Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex8";
}

public class Regex9Columnizer : BaseRegexColumnizer
{
    protected override string GetNameInternal () => "Regex9";
}

#endregion