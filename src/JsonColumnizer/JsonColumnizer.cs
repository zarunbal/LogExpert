using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using LogExpert;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonColumnizer
{
    internal class JsonColumn
    {
        public JsonColumn(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [Serializable]
    public class JsonColumnizerConfig
    {
        public void InitDefaults()
        {
        }
    }

    /// <summary>
    ///     This Columnizer can parse JSON files. It uses the IInitColumnizer interface for support of dynamic field count.
    ///     The IPreProcessColumnizer is implemented to read field names from the very first line of the file. Then
    ///     the line is dropped. So it's not seen by LogExpert. The field names will be used as column names.
    /// </summary>
    public class JsonColumnizer : ILogLineColumnizer, IInitColumnizer, IColumnizerConfigurator, IPreProcessColumnizer
    {
        private readonly IList<JsonColumn> _columnList = new List<JsonColumn>();
        private JsonColumnizerConfig _config;

        private bool _isValidJson;
        public string Text => GetName();

        public void Configure(ILogLineColumnizerCallback callback, string configDir)
        {
            var configPath = configDir + "\\jsoncolumnizer.dat";
            var dlg = new JsonColumnizerConfigDlg(_config);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var formatter = new BinaryFormatter();
                Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(fs, _config);
                fs.Close();
                Selected(callback);
            }
        }

        public void LoadConfig(string configDir)
        {
            var configPath = configDir + "\\jsoncolumnizer.dat";

            if (!File.Exists(configPath))
            {
                _config = new JsonColumnizerConfig();
                _config.InitDefaults();
            }
            else
            {
                Stream fs = File.OpenRead(configPath);
                var formatter = new BinaryFormatter();
                try
                {
                    _config = (JsonColumnizerConfig) formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    MessageBox.Show(e.Message, "Deserialize");
                    _config = new JsonColumnizerConfig();
                    _config.InitDefaults();
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public void Selected(ILogLineColumnizerCallback callback)
        {
            if (_isValidJson)
            {
                _columnList.Clear();
                var line = callback.GetLogLine(0);

                if (line != null)
                {
                    var json = JsonConvert.DeserializeObject<JObject>(line.FullLine);

                    var fieldCount = json.Properties().Count();

                    for (var i = 0; i < fieldCount; ++i)
                    {
                        _columnList.Add(new JsonColumn(json.Properties().ToArray()[i].Name));
                    }
                }
            }
        }


        public void DeSelected(ILogLineColumnizerCallback callback)
        {
            // nothing to do 
        }

        public string GetName()
        {
            return "JSON Columnizer";
        }

        public string GetDescription()
        {
            return
                "Splits JSON files into columns.\r\n\r\nCredits:\r\nThis Columnizer uses the Newtonsoft json package written by someone. Downloaded from codeproject.com.\r\n";
        }

        public int GetColumnCount()
        {
            return _isValidJson ? _columnList.Count : 1;
        }

        public string[] GetColumnNames()
        {
            var names = new string[GetColumnCount()];
            if (_isValidJson)
            {
                var i = 0;
                foreach (var column in _columnList)
                {
                    names[i++] = column.Name;
                }
            }
            else
            {
                names[0] = "Text";
            }
            return names;
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            if (_isValidJson)
            {
                return SplitJsonLine(line);
            }
            var cLogLine = new ColumnizedLogLine {LogLine = line};
            cLogLine.ColumnValues = new IColumn[] {new Column {FullValue = line.FullLine, Parent = cLogLine}};

            return cLogLine;
        }

        public bool IsTimeshiftImplemented()
        {
            return false;
        }

        public void SetTimeOffset(int msecOffset)
        {
            throw new NotImplementedException();
        }

        public int GetTimeOffset()
        {
            throw new NotImplementedException();
        }

        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, ILogLine line)
        {
            throw new NotImplementedException();
        }

        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            throw new NotImplementedException();
        }

        public string PreProcessLine(string logLine, int lineNum, int realLineNum)
        {
            _isValidJson = JsonConvert.DeserializeObject<JObject>(logLine) != null;
            return logLine;
        }

        private IColumnizedLogLine SplitJsonLine(ILogLine line)
        {
            var cLogLine = new ColumnizedLogLine {LogLine = line};
            
            var json = JsonConvert.DeserializeObject<JObject>(line.FullLine);

            var columns = json.Properties().Select(property => new Column {FullValue = property.Value.ToString(), Parent = cLogLine}).ToList();

            cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

            return cLogLine;
        }
    }
}