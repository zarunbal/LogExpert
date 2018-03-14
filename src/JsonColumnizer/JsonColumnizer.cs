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
    public class JsonColumnizer : ILogLineColumnizer, IInitColumnizer
    {
        #region Fields

        private static readonly JsonColumn _initialColumn = new JsonColumn("Text");

        private readonly IList<JsonColumn> _columnList = new List<JsonColumn>(new[] {_initialColumn});

        #endregion

        #region Properties

        public string Text => GetName();

        #endregion

        #region Public methods

        public void Selected(ILogLineColumnizerCallback callback)
        {
            _columnList.Clear();
            _columnList.Add(_initialColumn);
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
            return "Splits JSON files into columns.\r\n\r\nCredits:\r\nThis Columnizer uses the Newtonsoft json package.\r\n";
        }

        public int GetColumnCount()
        {
            return _columnList.Count;
        }

        public string[] GetColumnNames()
        {
            string[] names = new string[GetColumnCount()];
            int i = 0;
            foreach (var column in _columnList)
            {
                names[i++] = column.Name;
            }

            return names;
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            JObject json = JsonConvert.DeserializeObject<JObject>(line.FullLine);

            if (json != null)
            {
                return SplitJsonLine(line, json);
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

        #endregion

        #region Private Methods

        private IColumnizedLogLine SplitJsonLine(ILogLine line, JObject json)
        {
            var cLogLine = new ColumnizedLogLine {LogLine = line};

            var columns = json.Properties().Select(property => new Column {FullValue = property.Value.ToString(), Parent = cLogLine}).ToList();

            cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

            return cLogLine;
        }

        #endregion
    }
}