using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LogExpert
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class AutoColumnizer : ILogLineColumnizer
    {
        protected int timeOffset = 0;
        private readonly TimeFormatDeterminer _timeFormatDeterminer = new TimeFormatDeterminer();

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

        public ILogLineColumnizer FindColumnizer(string fileName, LogfileReader logFileReader)
        {
            if (logFileReader == null || string.IsNullOrEmpty(fileName))
            {
                return new DefaultLogfileColumnizer();
            }

            ILogLineColumnizer lineColumnizer = null;
            var timeDeterminer = new TimeFormatDeterminer();
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

            lineColumnizer = PluginRegistry.GetInstance().RegisteredColumnizers.OrderByDescending(x => x.GetPriority(fileName, loglines)).First();

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

        public Priority GetPriority(string fileName, IEnumerable<ILogLine> samples)
        {
            return Priority.NotSupport;
        }

        #endregion ILogLineColumnizer implementation
    }
}