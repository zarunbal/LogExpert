using System;
using System.Collections.Generic;
using System.Linq;
using LogExpert;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonColumnizer
{
    internal class JsonColumn
    {
        #region cTor

        public JsonColumn(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name { get; }

        #endregion
    }

    /// <summary>
    ///     This Columnizer can parse JSON files.
    /// </summary>
    public class JsonColumnizer : ILogLineColumnizer, IInitColumnizer, IPreProcessColumnizer
    {
        #region Fields

        private readonly IList<JsonColumn> _columnList = new List<JsonColumn>();

        private bool _isValidJson;

        #endregion

        #region Properties

        public string Text => GetName();

        #endregion

        #region Public methods

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
                "Splits JSON files into columns.\r\n\r\nCredits:\r\nThis Columnizer uses the Newtonsoft json package.\r\n";
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

        #endregion

        #region Private Methods

        private IColumnizedLogLine SplitJsonLine(ILogLine line)
        {
            var cLogLine = new ColumnizedLogLine {LogLine = line};

            var json = JsonConvert.DeserializeObject<JObject>(line.FullLine);

            var columns = json.Properties().Select(property => new Column {FullValue = property.Value.ToString(), Parent = cLogLine}).ToList();

            cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

            return cLogLine;
        }

        #endregion
    }
}