using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using LogExpert;

namespace RegexColumnizer
{
    public class RegexColumnizerConfig
    {
        #region Properties / Indexers

        public string Expression { get; set; } = "(?<text>.*)";

        #endregion
    }

    public class Regex1Columnizer : ILogLineColumnizer, IColumnizerConfigurator
    {
        #region Private Fields

        private readonly XmlSerializer xml = new XmlSerializer(typeof(RegexColumnizerConfig));
        private string[] columns;

        #endregion

        #region Interface IColumnizerConfigurator

        public void Configure(ILogLineColumnizerCallback callback, string configDir)
        {
            RegexColumnizerConfigDialog d = new RegexColumnizerConfigDialog {Config = Config};
            if (d.ShowDialog() == DialogResult.OK)
            {
                string configFile = GetConfigFile(configDir);
                using (FileStream w = new FileStream(configFile, FileMode.Create))
                {
                    xml.Serialize(w, d.Config);
                }
            }
        }

        public void LoadConfig(string configDir)
        {
            string configFile = GetConfigFile(configDir);
            RegexColumnizerConfig config;
            if (!File.Exists(configFile))
            {
                config = new RegexColumnizerConfig();
            }
            else
            {
                using (StreamReader reader = new StreamReader(configFile))
                {
                    config = xml.Deserialize(reader) as RegexColumnizerConfig;
                }
            }

            Init(config);
        }

        #endregion

        #region Interface ILogLineColumnizer

        public int GetColumnCount()
        {
            return columns.Length;
        }

        public string[] GetColumnNames()
        {
            return columns;
        }

        public string GetDescription()
        {
            return "Columns are filled by regular expression named capture groups";
        }

        public string GetName()
        {
            return "Regex";
        }

        public int GetTimeOffset()
        {
            throw new NotImplementedException();
        }

        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, ILogLine line)
        {
            throw new NotImplementedException();
        }

        public bool IsTimeshiftImplemented()
        {
            return false;
        }

        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            throw new NotImplementedException();
        }

        public void SetTimeOffset(int msecOffset)
        {
            throw new NotImplementedException();
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            ColumnizedLogLine logLine = new ColumnizedLogLine();

            logLine.ColumnValues = new IColumn[columns.Length];
            if (Regex != null)
            {
                Match m = Regex.Match(line.FullLine);
                for (int i = m.Groups.Count - 1; i > 0; i--)
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

        #endregion

        #region Properties / Indexers

        public RegexColumnizerConfig Config { get; private set; }
        public Regex Regex { get; private set; }

        #endregion

        #region Public Methods

        public string GetConfigFile(string configDir)
        {
            string name = GetType().Name;
            string configPath = Path.Combine(configDir, name);
            configPath = Path.ChangeExtension(configPath, "xml");
            return configPath;
        }

        #endregion

        #region Private Methods

        private void Init(RegexColumnizerConfig config)
        {
            Config = config;

            try
            {
                Regex = new Regex(Config.Expression, RegexOptions.Compiled);
                int skip = Regex.GetGroupNames().Length == 1 ? 0 : 1;
                columns = Regex.GetGroupNames().Skip(skip).ToArray();
            }
            catch
            {
                Regex = null;
            }
        }

        #endregion
    }

    public class Regex2Columnizer : Regex1Columnizer
    {
    }

    public class Regex3Columnizer : Regex1Columnizer
    {
    }

    public class Regex4Columnizer : Regex1Columnizer
    {
    }

    public class Regex5Columnizer : Regex1Columnizer
    {
    }

    public class Regex6Columnizer : Regex1Columnizer
    {
    }

    public class Regex7Columnizer : Regex1Columnizer
    {
    }

    public class Regex8Columnizer : Regex1Columnizer
    {
    }

    public class Regex9Columnizer : Regex1Columnizer
    {
    }
}
