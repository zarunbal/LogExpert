using System;
using System.Collections.Generic;
using System.Text;


namespace LogExpert
{
    internal class DefaultLogfileColumnizer : ILogLineColumnizer
    {
        #region ILogLineColumnizer Members

        public string GetName()
        {
            return "Default (single line)";
        }

        public string GetDescription()
        {
            return "No column splitting. The whole line is displayed in a single column.";
        }

        public int GetColumnCount()
        {
            return 1;
        }

        public string[] GetColumnNames()
        {
            return new string[] {"Text"};
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            ColumnizedLogLine cLogLine = new ColumnizedLogLine();
            cLogLine.LogLine = line;
            cLogLine.ColumnValues = new IColumn[]
            {
                new Column
                {
                    FullValue = line.FullLine,
                    Parent = cLogLine
                }
            };


            return cLogLine;
        }

        public string Text
        {
            get { return GetName(); }
        }

        public Priority GetPriority(string fileName, IEnumerable<ILogLine> samples)
        {
            return Priority.CanSupport;
        }
        #endregion

        #region ILogLineColumnizer Not implemented Members

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
        }

        #endregion
    }
}