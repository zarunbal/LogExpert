using LogExpert;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonColumnizer
{
    /// <summary>
    ///     This Columnizer can parse JSON files.
    /// </summary>
    public class JsonCompactColumnizer : JsonColumnizer
    {
        #region Public methods

        public override string GetName()
        {
            return "JSON Compact Columnizer";
        }

        public override string GetDescription()
        {
            return "A JSON columnier for Serilog.Formatting.Compact format.";
        }

        public override void Selected(ILogLineColumnizerCallback callback)
        {
            ColumnList.Clear();
            // Create column header with cached column list.

            foreach (var col in _tagDict.Keys)
            {
                ColumnList.Add(new JsonColumn(_tagDict[col]));
            }
        }

        public override Priority GetPriority(string fileName, IEnumerable<ILogLine> samples)
        {
            Priority result = Priority.NotSupport;
            if (fileName.EndsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                result = Priority.WellSupport;
            }

            if (samples != null && samples.Count() > 0)
            {
                try
                {
                    var line = samples.First();
                    JObject json = ParseJson(line);
                    if (json != null)
                    {
                        var columns = SplitJsonLine(samples.First(), json);
                        if (columns.ColumnValues.Count() > 0 && Array.Exists(columns.ColumnValues, x => !string.IsNullOrEmpty(x.FullValue)))
                        {
                            result = Priority.PerfectlySupport;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore errors when determine priority.
                }
            }
            return result;
        }

        #endregion

        #region Private Methods

        protected Dictionary<string, string> _tagDict = new Dictionary<string, string>(){
            { "@t", "Timestamp" },
            { "@l", "Level" },
            { "@m", "Message" },
            { "@x", "Exception" },
            { "@i", "Event id" },
            { "@r", "Renderings" },
            { "@mt", "Message Template" },
        };

        protected override IColumnizedLogLine SplitJsonLine(ILogLine line, JObject json)
        {
            List<IColumn> returnColumns = new List<IColumn>();
            var cLogLine = new ColumnizedLogLine { LogLine = line };

            var columns = json.Properties().Select(property => new ColumnWithName { FullValue = property.Value.ToString(), ColumneName = property.Name.ToString(), Parent = cLogLine }).ToList();

            //
            // Always rearrage the order of all json fields within a line to follow the sequence of columnNameList.
            // This will make sure the log line displayed correct even the order of json fields changed.
            //
            foreach (var column in _tagDict.Keys)
            {
                if (column.StartsWith("@"))
                {
                    var existingColumn = columns.Find(x => x.ColumneName == column);

                    if (existingColumn != null)
                    {
                        returnColumns.Add(new Column() { FullValue = existingColumn.FullValue, Parent = cLogLine });
                        continue;
                    }
                    // Fields that is missing in current line should be shown as empty.
                    returnColumns.Add(new Column() { FullValue = "", Parent = cLogLine });
                }
            }

            cLogLine.ColumnValues = returnColumns.ToArray();

            return cLogLine;
        }

        #endregion
    }
}