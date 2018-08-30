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

        #region ILogLineColumnizer implementation

        protected int timeOffset = 0;
        private TimeFormatDeterminer _timeFormatDeterminer = new TimeFormatDeterminer();

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

            if (fileName.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
            {
                return PluginRegistry.GetInstance().RegisteredColumnizers.First(
                    x => x.GetName().IndexOf("xml", 0, StringComparison.CurrentCultureIgnoreCase) != -1);
            }

            if (fileName.EndsWith("json", StringComparison.InvariantCultureIgnoreCase))
            {
                return PluginRegistry.GetInstance().RegisteredColumnizers.First(
                    x => x.GetName().IndexOf("json", 0, StringComparison.CurrentCultureIgnoreCase) != -1);
            }

            if (fileName.EndsWith("csv", StringComparison.InvariantCultureIgnoreCase))
            {
                return PluginRegistry.GetInstance().RegisteredColumnizers.First(
                    x => x.GetName().IndexOf("csv", 0, StringComparison.CurrentCultureIgnoreCase) != -1);
            }


            int timeStampExistsCount = 0;
            bool isSquareBracketsExists = false;
            ILogLineColumnizer lineColumnizer = null;
            var timeDeterminer = new TimeFormatDeterminer();
            int maxBracketNumbers = 1;

            List<ILogLine> loglines = new List<ILogLine>();
            // Sampling a few lines to select the correct columnizer
            loglines.Add(logFileReader.GetLogLine(5));
            loglines.Add(logFileReader.GetLogLine(25));
            loglines.Add(logFileReader.GetLogLine(100));
            loglines.Add(logFileReader.GetLogLine(200));
            loglines.Add(logFileReader.GetLogLine(300));
            loglines.Add(logFileReader.GetLogLine(400));
            loglines.Add(logFileReader.GetLogLine(500));

            foreach (var line in loglines)
            {
                if (line == null || string.IsNullOrEmpty(line.FullLine))
                {
                    continue;
                }
                int bracketNumbers = 1;
                if (null != timeDeterminer.DetermineDateTimeFormatInfo(line.FullLine))
                {
                    timeStampExistsCount++;
                }
                else
                {
                    timeStampExistsCount--;
                }
                string noSpaceLine = line.FullLine.Replace(" ", string.Empty);
                if (noSpaceLine.IndexOf('[') >= 0 && noSpaceLine.IndexOf(']') >= 0
                    && noSpaceLine.IndexOf('[') < noSpaceLine.IndexOf(']'))
                {
                    bracketNumbers += Regex.Matches(noSpaceLine, @"\]\[").Count;
                    isSquareBracketsExists = true;
                }
                maxBracketNumbers = Math.Max(bracketNumbers, maxBracketNumbers);
            }

            if (isSquareBracketsExists)
            {
                lineColumnizer = new SquareBracketColumnizer(maxBracketNumbers, timeStampExistsCount > 0);
            }
            else if (timeStampExistsCount > 0)
            {
                lineColumnizer = new TimestampColumnizer();
            }
            else
            {
                lineColumnizer = new DefaultLogfileColumnizer();
            }

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