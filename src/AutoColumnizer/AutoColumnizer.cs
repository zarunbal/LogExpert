using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
    public class AutoColumnizer : ILogLineColumnizer, IAutoColumnizer
    {
        #region ILogLineColumnizer implementation

        public string Text => GetName();

        public bool IsTimeshiftImplemented()
        {
            return true;
        }

        public string GetName()
        {
            return "Auto Columnizer";
        }

        public string GetDescription()
        {
            return "Automatically find the right columnizer for any file";
        }

        public ILogLineColumnizer FindColumnizer(string fileName, IAutoLogLineColumnizerCallback logFileReader)
        {
            if (logFileReader == null || string.IsNullOrEmpty(fileName))
            {
                return logFileReader.GetDefaultColumnizer();
            }

            List<ILogLine> loglines = new List<ILogLine>
            {
                // Sampling a few lines to select the correct columnizer
                logFileReader.GetLogLine(0),
                logFileReader.GetLogLine(1),
                logFileReader.GetLogLine(2),
                logFileReader.GetLogLine(3),
                logFileReader.GetLogLine(4),
                logFileReader.GetLogLine(5),
                logFileReader.GetLogLine(25),
                logFileReader.GetLogLine(100),
                logFileReader.GetLogLine(200),
                logFileReader.GetLogLine(400)
            };

            var registeredColumnizer = logFileReader.GetRegisteredColumnizers();

            List<Tuple<Priority, ILogLineColumnizer>> priorityListOfColumnizers = new List<Tuple<Priority, ILogLineColumnizer>>();

            foreach (ILogLineColumnizer logLineColumnizer in registeredColumnizer)
            {
                var columnizerPriority = logLineColumnizer as IColumnizerPriority;
                Priority priority = default(Priority);
                if (columnizerPriority != null)
                {
                    priority = columnizerPriority.GetPriority(fileName, loglines);
                }

                priorityListOfColumnizers.Add(new Tuple<Priority, ILogLineColumnizer>(priority, logLineColumnizer));
            }

            ILogLineColumnizer lineColumnizer = priorityListOfColumnizers.OrderByDescending(a => a.Item1).Select(a => a.Item2).First();

            return lineColumnizer;
        }

        public int GetColumnCount()
        {
            throw new NotImplementedException();
        }

        public string[] GetColumnNames()
        {
            throw new NotImplementedException();
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            throw new NotImplementedException();
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

        #endregion ILogLineColumnizer implementation
    }
}