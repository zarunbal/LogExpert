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
        #region Ctor

        public JsonColumn(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties / Indexers

        public string Name { get; }

        #endregion
    }

    /// <summary>
    ///     This Columnizer can parse JSON files.
    /// </summary>
    public class JsonColumnizer : ILogLineColumnizer, IInitColumnizer
    {
        #region Static/Constants

        private static readonly JsonColumn _initialColumn = new JsonColumn("Text");

        #endregion

        #region Private Fields

        private readonly IList<JsonColumn> _columnList = new List<JsonColumn>(new[] {_initialColumn});

        #endregion

        #region Interface IInitColumnizer

        public void DeSelected(ILogLineColumnizerCallback callback)
        {
            // nothing to do
        }

        public void Selected(ILogLineColumnizerCallback callback)
        {
            _columnList.Clear();

            ILogLine line = callback.GetLogLine(0);

            if (line != null)
            {
                JObject json = ParseJson(line);
                if (json != null)
                {
                    int fieldCount = json.Properties().Count();

                    for (int i = 0; i < fieldCount; ++i)
                    {
                        _columnList.Add(new JsonColumn(json.Properties().ToArray()[i].Name));
                    }
                }
                else
                {
                    _columnList.Add(_initialColumn);
                }
            }
            else
            {
                _columnList.Add(_initialColumn);
            }
        }

        #endregion

        #region Interface ILogLineColumnizer

        public int GetColumnCount()
        {
            return _columnList.Count;
        }

        public string[] GetColumnNames()
        {
            string[] names = new string[GetColumnCount()];
            int i = 0;
            foreach (JsonColumn column in _columnList)
            {
                names[i++] = column.Name;
            }

            return names;
        }

        public string GetDescription()
        {
            return "Splits JSON files into columns.\r\n\r\nCredits:\r\nThis Columnizer uses the Newtonsoft json package.\r\n\r\nFirst line must be valid or else only one column will be displayed and the other values dropped!";
        }

        public string GetName()
        {
            return "JSON Columnizer";
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
            JObject json = ParseJson(line);

            if (json != null)
            {
                return SplitJsonLine(line, json);
            }

            ColumnizedLogLine cLogLine = new ColumnizedLogLine {LogLine = line};

            Column[] columns = Column.CreateColumns(_columnList.Count, cLogLine);

            columns.Last().FullValue = line.FullLine;

            cLogLine.ColumnValues = columns.Select(a => (IColumn)a).ToArray();

            return cLogLine;
        }

        #endregion

        #region Properties / Indexers

        public string Text => GetName();

        #endregion

        #region Private Methods

        private static JObject ParseJson(ILogLine line)
        {
            return JsonConvert.DeserializeObject<JObject>(line.FullLine, new JsonSerializerSettings
            {
                Error = (sender, args) => { args.ErrorContext.Handled = true; } // We ignore the error and handle the null value
            });
        }

        private IColumnizedLogLine SplitJsonLine(ILogLine line, JObject json)
        {
            ColumnizedLogLine cLogLine = new ColumnizedLogLine {LogLine = line};

            List<Column> columns = json.Properties().Select(property => new Column {FullValue = property.Value.ToString(), Parent = cLogLine}).ToList();

            cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

            return cLogLine;
        }

        #endregion
    }
}
