using System.Linq;


namespace LogExpert
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using static LogExpert.TimeFormatDeterminer;

    public class SquareBracketColumnizer : ILogLineColumnizer
    {

        #region ILogLineColumnizer implementation

        protected int timeOffset = 0;
        private TimeFormatDeterminer _timeFormatDeterminer = new TimeFormatDeterminer();

        // TODO: need preparing this columnizer with sample log lines before use it.
        private int _columnCount = 5;
        private bool _isTimeExists = false;

        public SquareBracketColumnizer()
        {
        }

        public SquareBracketColumnizer(int columnCount, bool isTimeExists) : this()
        {
            // Add message column
            _columnCount = columnCount + 1;
            _isTimeExists = isTimeExists;
            if (_isTimeExists)
            {
                // Time and date
                _columnCount += 2;
            }
        }

        public bool IsTimeshiftImplemented()
        {
            return true;
        }

        public void SetTimeOffset(int msecOffset)
        {
            this.timeOffset = msecOffset;
        }

        public int GetTimeOffset()
        {
            return this.timeOffset;
        }


        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, ILogLine line)
        {
            IColumnizedLogLine cols = SplitLine(callback, line);
            if (cols == null || cols.ColumnValues == null || cols.ColumnValues.Length < 2)
            {
                return DateTime.MinValue;
            }
            if (cols.ColumnValues[0].FullValue.Length == 0 || cols.ColumnValues[1].FullValue.Length == 0)
            {
                return DateTime.MinValue;
            }
            FormatInfo formatInfo = _timeFormatDeterminer.DetermineDateTimeFormatInfo(line.FullLine);
            if (formatInfo == null)
            {
                return DateTime.MinValue;
            }

            try
            {
                DateTime dateTime = DateTime.ParseExact(
                    cols.ColumnValues[0].FullValue + " " + cols.ColumnValues[1].FullValue, formatInfo.DateTimeFormat,
                    formatInfo.CultureInfo);
                return dateTime;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }


        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            if (column == 1)
            {
                try
                {
                    FormatInfo formatInfo = _timeFormatDeterminer.DetermineTimeFormatInfo(oldValue);
                    if (formatInfo == null)
                    {
                        return;
                    }
                    DateTime newDateTime = DateTime.ParseExact(value, formatInfo.TimeFormat, formatInfo.CultureInfo);
                    DateTime oldDateTime = DateTime.ParseExact(oldValue, formatInfo.TimeFormat, formatInfo.CultureInfo);
                    long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    this.timeOffset = (int) (mSecsNew - mSecsOld);
                }
                catch (FormatException)
                {
                }
            }
        }

        public string GetName()
        {
            return "Square Bracket Columnizer";
        }

        public string GetDescription()
        {
            return "Splits every line into n fields: Date, Time and the rest of the log message";
        }

        public int GetColumnCount()
        {
            return _columnCount;
        }

        public string[] GetColumnNames()
        {
            var columnNames = new List<string>(GetColumnCount());
            if (_isTimeExists)
            {
                columnNames.Add("Date");
                columnNames.Add("Time");
            }


            // TODO: Make this configurable.
            if (GetColumnCount() > 3)
            {
                columnNames.Add("Level");
            }

            if (GetColumnCount() > 4)
            {
                columnNames.Add("Source");
            }

            // Last column is the message
            columnNames.Add("Message");
            int i = 1;
            while (columnNames.Count < GetColumnCount())
            {
                columnNames.Insert(columnNames.Count - 1 , "Source" + i++.ToString());
            }
            return columnNames.ToArray();
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            // 0         1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16
            // 012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            // 03.01.2008 14:48:00.066 <rest of line>

            ColumnizedLogLine clogLine = new ColumnizedLogLine
            {
                LogLine = line
            };

            Column[] columns = new Column[]
            {
                new Column {FullValue = "", Parent = clogLine},
                new Column {FullValue = "", Parent = clogLine},
                new Column {FullValue = "", Parent = clogLine},
            };


            string temp = line.FullLine;


            if (temp.Length < 3)
            {
                columns[2].FullValue = temp;
                return clogLine;
            }

            FormatInfo formatInfo = _timeFormatDeterminer.DetermineDateTimeFormatInfo(line.FullLine);
            if (formatInfo == null)
            {
                columns[2].FullValue = temp;
                SquareSplit(ref columns, temp, 0, 0, 0, clogLine);
            }
            else
            {
                int endPos = formatInfo.DateTimeFormat.Length;
                int timeLen = formatInfo.TimeFormat.Length;
                int dateLen = formatInfo.DateFormat.Length;
                try
                {
                    if (this.timeOffset != 0)
                    {
                        DateTime dateTime = DateTime.ParseExact(temp.Substring(0, endPos), formatInfo.DateTimeFormat,
                            formatInfo.CultureInfo);
                        dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, this.timeOffset));
                        string newDate = dateTime.ToString(formatInfo.DateTimeFormat, formatInfo.CultureInfo);

                        SquareSplit(ref columns, newDate, dateLen, timeLen, endPos, clogLine);
                    }
                    else
                    {

                        SquareSplit(ref columns, temp, dateLen, timeLen, endPos, clogLine);
                    }
                }
                catch (Exception)
                {
                    columns[0].FullValue = "n/a";
                    columns[1].FullValue = "n/a";
                    columns[2].FullValue = temp;
                }
            }
            clogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

            return clogLine;
        }

        void SquareSplit(ref Column[] columns, string line, int dateLen, int timeLen, int dateTimeEndPos, ColumnizedLogLine clogLine)
        {
            List<Column> columnList = new List<Column>();
            int restColumn = _columnCount;
            if (_isTimeExists)
            {
                columnList.Add(new Column { FullValue = line.Substring(0, dateLen), Parent = clogLine });
                columnList.Add(new Column { FullValue = line.Substring(dateLen + 1, timeLen), Parent = clogLine });
                restColumn -= 2;
            }
            int nextPos = dateTimeEndPos;

            string rest = line;

            for (int i = 0; i < restColumn; i++)
            {
                rest = rest.Substring(nextPos);
                //var fullValue = rest.Substring(0, rest.IndexOf(']')).TrimStart(new char[] {' '}).TrimEnd(new char[] { ' ' });
                var trimmed = rest.TrimStart(new char[] { ' ' });
                if (string.IsNullOrEmpty(trimmed) || trimmed[0] != '[' || rest.IndexOf(']') < 0 || i == restColumn - 1)
                {
                    columnList.Add(new Column { FullValue = rest, Parent = clogLine });
                    break;
                }
                nextPos = rest.IndexOf(']') + 1;
                var fullValue = rest.Substring(0, nextPos);
                columnList.Add(new Column { FullValue = fullValue, Parent = clogLine });
            }

            while (columnList.Count < _columnCount)
            {
                columnList.Insert(columnList.Count - 1, new Column { FullValue = "", Parent = clogLine });
            }

            columns = columnList.ToArray();

        }

        public Priority GetPriority(string fileName, IEnumerable<ILogLine> samples)
        {
            Priority result = Priority.NotSupport;
            TimeFormatDeterminer timeDeterminer = new TimeFormatDeterminer();
            int timeStampExistsCount = 0;
            int bracketsExistsCount = 0;
            int maxBracketNumbers = 1;

            foreach(var logline in samples)
            {
                string line = logline?.FullLine;
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                int bracketNumbers = 1;
                if (null != timeDeterminer.DetermineDateTimeFormatInfo(line))
                {
                    timeStampExistsCount++;
                }
                else
                {
                    timeStampExistsCount--;
                }
                string noSpaceLine = line.Replace(" ", string.Empty);
                if (noSpaceLine.IndexOf('[') >= 0 && noSpaceLine.IndexOf(']') >= 0
                    && noSpaceLine.IndexOf('[') < noSpaceLine.IndexOf(']'))
                {
                    bracketNumbers += Regex.Matches(noSpaceLine, @"\]\[").Count;
                    bracketsExistsCount++;
                }
                else
                {
                    bracketsExistsCount--;
                }
                maxBracketNumbers = Math.Max(bracketNumbers, maxBracketNumbers);
            }

            // Add message
            _columnCount = maxBracketNumbers + 1;
            _isTimeExists = timeStampExistsCount > 0;
            if (_isTimeExists)
            {
                _columnCount += 2;
            }

            if (maxBracketNumbers > 1)
            {
                result = Priority.WellSupport;
                if (bracketsExistsCount > 0)
                {
                    result = Priority.PerfectlySupport;
                }
            }

            return result;
        }

        #endregion


        #region internal stuff

        public string Text
        {
            get { return GetName(); }
        }


        #endregion
    }
}