using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LogExpert
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;

    public class TimestampColumnizer : ILogLineColumnizer
    {
        #region FormatInfo helper class

        protected class FormatInfo
        {
            #region cTor

            public FormatInfo(string dateFormat, string timeFormat, CultureInfo cultureInfo)
            {
                this.DateFormat = dateFormat;
                this.TimeFormat = timeFormat;
                this.CultureInfo = cultureInfo;
            }

            #endregion

            #region Properties

            public string DateFormat { get; }

            public string TimeFormat { get; }

            public CultureInfo CultureInfo { get; }

            public string DateTimeFormat
            {
                get { return this.DateFormat + " " + this.TimeFormat; }
            }

            public bool IgnoreFirstChar { get; set; }

            #endregion
        }

        #endregion

        #region ILogLineColumnizer implementation

        protected int timeOffset = 0;
        protected FormatInfo formatInfo1 = new FormatInfo("dd.MM.yyyy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo2 = new FormatInfo("dd.MM.yyyy", "HH:mm:ss", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo3 = new FormatInfo("yyyy/MM/dd", "HH:mm:ss.fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo4 = new FormatInfo("yyyy/MM/dd", "HH:mm:ss", new CultureInfo("en-US"));
        protected FormatInfo formatInfo5 = new FormatInfo("yyyy.MM.dd", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo6 = new FormatInfo("yyyy.MM.dd", "HH:mm:ss", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo7 = new FormatInfo("dd.MM.yyyy", "HH:mm:ss,fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo8 = new FormatInfo("yyyy/MM/dd", "HH:mm:ss,fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo9 = new FormatInfo("yyyy.MM.dd", "HH:mm:ss,fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo10 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss.fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo11 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss,fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo12 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss", new CultureInfo("en-US"));
        protected FormatInfo formatInfo13 = new FormatInfo("dd MMM yyyy", "HH:mm:ss,fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo14 = new FormatInfo("dd MMM yyyy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo15 = new FormatInfo("dd MMM yyyy", "HH:mm:ss", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo16 = new FormatInfo("dd.MM.yy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo17 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss:ffff", new CultureInfo("en-US"));

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
            if (cols == null || cols.ColumnValues.Length < 2)
            {
                return DateTime.MinValue;
            }
            if (cols.ColumnValues[0].FullValue.Length == 0 || cols.ColumnValues[1].FullValue.Length == 0)
            {
                return DateTime.MinValue;
            }
            FormatInfo formatInfo = DetermineDateTimeFormatInfo(line);
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
                    FormatInfo formatInfo = DetermineTimeFormatInfo(oldValue);
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
            return "Timestamp Columnizer";
        }

        public string GetDescription()
        {
            return "Splits every line into 3 fields: Date, Time and the rest of the log message";
        }

        public int GetColumnCount()
        {
            return 3;
        }

        public string[] GetColumnNames()
        {
            return new string[] {"Date", "Time", "Message"};
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            // 0         1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16
            // 012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            // 03.01.2008 14:48:00.066 <rest of line>

            ColumnizedLogLine clogLine = new ColumnizedLogLine();
            clogLine.LogLine = line;

            Column[] columns = new Column[3]
            {
                new Column {FullValue = "", Parent = clogLine},
                new Column {FullValue = "", Parent = clogLine},
                new Column {FullValue = "", Parent = clogLine},
            };

            clogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

            string temp = line.FullLine;


            if (temp.Length < 21)
            {
                columns[2].FullValue = temp;
                return clogLine;
            }

            FormatInfo formatInfo = DetermineDateTimeFormatInfo(line);
            if (formatInfo == null)
            {
                columns[2].FullValue = temp;
                return clogLine;
            }
            int endPos = formatInfo.DateTimeFormat.Length;
            int timeLen = formatInfo.TimeFormat.Length;
            int dateLen = formatInfo.DateFormat.Length;
            try
            {
                if (this.timeOffset != 0)
                {
                    if (formatInfo.IgnoreFirstChar)
                    {
                        // First character is a bracket and should be ignored
                        DateTime dateTime = DateTime.ParseExact(temp.Substring(1, endPos), formatInfo.DateTimeFormat,
                            formatInfo.CultureInfo);
                        dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, this.timeOffset));
                        string newDate = dateTime.ToString(formatInfo.DateTimeFormat, formatInfo.CultureInfo);
                        columns[0].FullValue = newDate.Substring(0, dateLen); // date
                        columns[1].FullValue = newDate.Substring(dateLen + 1, timeLen); // time
                        columns[2].FullValue = temp.Substring(endPos + 2); // rest of line
                    }
                    else
                    {
                        DateTime dateTime = DateTime.ParseExact(temp.Substring(0, endPos), formatInfo.DateTimeFormat,
                            formatInfo.CultureInfo);
                        dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, this.timeOffset));
                        string newDate = dateTime.ToString(formatInfo.DateTimeFormat, formatInfo.CultureInfo);
                        columns[0].FullValue = newDate.Substring(0, dateLen); // date
                        columns[1].FullValue = newDate.Substring(dateLen + 1, timeLen); // time
                        columns[2].FullValue = temp.Substring(endPos); // rest of line
                    }
                }
                else
                {
                    if (formatInfo.IgnoreFirstChar)
                    {
                        // First character is a bracket and should be ignored
                        columns[0].FullValue = temp.Substring(1, dateLen); // date
                        columns[1].FullValue = temp.Substring(dateLen + 2, timeLen); // time
                        columns[2].FullValue = temp.Substring(endPos + 2); // rest of line
                    }
                    else
                    {
                        columns[0].FullValue = temp.Substring(0, dateLen); // date
                        columns[1].FullValue = temp.Substring(dateLen + 1, timeLen); // time
                        columns[2].FullValue = temp.Substring(endPos); // rest of line
                    }
                }
            }
            catch (Exception)
            {
                columns[0].FullValue = "n/a";
                columns[1].FullValue = "n/a";
                columns[2].FullValue = temp;
            }
            return clogLine;
        }

        #endregion

        #region internal stuff

        public string Text
        {
            get { return GetName(); }
        }

        protected FormatInfo DetermineDateTimeFormatInfo(ILogLine line)
        {
            string temp = line.FullLine;
            bool ignoreFirst = false;

            // determine if string starts with bracket and remove it
            if (temp[0] == '[' || temp[0] == '(' || temp[0] == '{')
            {
                temp = temp.Substring(1);
                ignoreFirst = true;

            }

            // dirty hardcoded probing of date/time format (much faster than DateTime.ParseExact()
            if (temp[2] == '.' && temp[5] == '.' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo1.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo1;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo7.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo7;
                }
                else
                {
                    this.formatInfo2.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo2;
                }
            }
            else if (temp[4] == '/' && temp[7] == '/' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo3.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo3;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo8.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo8;
                }
                else
                {
                    this.formatInfo4.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo4;
                }
            }
            else if (temp[4] == '.' && temp[7] == '.' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo5.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo5;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo9.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo9;
                }
                else
                {
                    this.formatInfo6.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo6;
                }
            }
            else if (temp[4] == '-' && temp[7] == '-' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo10.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo10;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo11.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo11;
                }
                else if (temp[19] == ':')
                {
                    this.formatInfo17.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo17;
                }
                else
                {
                    this.formatInfo12.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo12;
                }
            }
            else if (temp[2] == ' ' && temp[6] == ' ' && temp[14] == ':' && temp[17] == ':')
            {
                if (temp[20] == ',')
                {
                    this.formatInfo13.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo13;
                }
                else if (temp[20] == '.')
                {
                    this.formatInfo14.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo14;
                }
                else
                {
                    this.formatInfo15.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo15;
                }
            }
            //dd.MM.yy HH:mm:ss.fff
            else if (temp[2] == '.' && temp[5] == '.' && temp[11] == ':' && temp[14] == ':' && temp[17] == '.')
            {
                this.formatInfo16.IgnoreFirstChar = ignoreFirst;
                return this.formatInfo16;
            }

            return null;
        }

        protected FormatInfo DetermineTimeFormatInfo(string field)
        {
            // dirty hardcoded probing of time format (much faster than DateTime.ParseExact()
            if (field[2] == ':' && field[5] == ':')
            {
                if (field.Length > 8)
                {
                    if (field[8] == '.')
                    {
                        return this.formatInfo1;
                    }
                    else if (field[8] == ',')
                    {
                        return this.formatInfo7;
                    }
                }
                else
                {
                    return this.formatInfo2;
                }
            }
            return null;
        }

        #endregion
    }
}