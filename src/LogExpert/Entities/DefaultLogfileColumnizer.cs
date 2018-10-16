using System;

namespace LogExpert
{
    internal class DefaultLogfileColumnizer : ILogLineColumnizer
    {
        #region Interface ILogLineColumnizer

        public int GetColumnCount()
        {
            return 1;
        }

        public string[] GetColumnNames()
        {
            return new[] {"Text"};
        }

        public string GetDescription()
        {
            return "No column splitting. The whole line is displayed in a single column.";
        }

        public string GetName()
        {
            return "Default (single line)";
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
        }

        public void SetTimeOffset(int msecOffset)
        {
            throw new NotImplementedException();
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

        #endregion

        #region Properties / Indexers

        public string Text => GetName();

        #endregion
    }
}
