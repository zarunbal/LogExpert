using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Globalization;
using System.Linq;


namespace GlassfishColumnizer
{
    internal class XmlConfig : IXmlLogConfiguration
    {
        #region Properties

        public string XmlStartTag { get; } = "[#|";

        public string XmlEndTag { get; } = "|#]";

        public string Stylesheet { get; } = null;

        public string[] Namespace
        {
            get { return null; }
        }

        #endregion
    }


    internal class GlassfishColumnizer : ILogLineXmlColumnizer
    {
        #region Fields

        public const int COLUMN_COUNT = 2;
        protected const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzzz";
        protected const string DATETIME_FORMAT_OUT = "yyyy-MM-dd HH:mm:ss.fff";

        private static readonly XmlConfig xmlConfig = new XmlConfig();
        private readonly char separatorChar = '|';
        private readonly char[] trimChars = new char[] {'|'};
        protected CultureInfo cultureInfo = new CultureInfo("en-US");
        protected int timeOffset = 0;

        #endregion

        #region cTor

        public GlassfishColumnizer()
        {
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return GetName(); }
        }

        #endregion

        #region Public methods

        public IXmlLogConfiguration GetXmlLogConfiguration()
        {
            return xmlConfig;
        }

        public ILogLine GetLineTextForClipboard(ILogLine logLine, ILogLineColumnizerCallback callback)
        {
            GlassFishLogLine line = new GlassFishLogLine
            {
                FullLine = logLine.FullLine.Replace(separatorChar, '|'),
                LineNumber = logLine.LineNumber
            };

            return line;
        }

        public string GetName()
        {
            return "Classfish";
        }

        public string GetDescription()
        {
            return "Parse the timestamps in Glassfish logfiles.";
        }

        public int GetColumnCount()
        {
            return COLUMN_COUNT;
        }

        public string[] GetColumnNames()
        {
            return new string[] {"Date/Time", "Message"};
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            ColumnizedLogLine cLogLine = new ColumnizedLogLine();
            cLogLine.LogLine = line;

            string temp = line.FullLine;

            Column[] columns = Column.CreateColumns(COLUMN_COUNT, cLogLine);
            cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();


            // delete '[#|' and '|#]'
            if (temp.StartsWith("[#|"))
            {
                temp = temp.Substring(3);
            }
            if (temp.EndsWith("|#]"))
            {
                temp = temp.Substring(0, temp.Length - 3);
            }

            // If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
            // in colum 8 (the log message column). Date and time column will be left blank.
            if (temp.Length < 28)
            {
                columns[1].FullValue = temp;
            }
            else
            {
                try
                {
                    DateTime dateTime = GetTimestamp(callback, line);
                    if (dateTime == DateTime.MinValue)
                    {
                        columns[1].FullValue = temp;
                    }
                    string newDate = dateTime.ToString(DATETIME_FORMAT_OUT);
                    columns[0].FullValue = newDate;
                }
                catch (Exception)
                {
                    columns[0].FullValue = "n/a";
                }

                Column timestmp = columns[0];

                string[] cols;
                cols = temp.Split(this.trimChars, COLUMN_COUNT, StringSplitOptions.None);

                if (cols.Length != COLUMN_COUNT)
                {
                    columns[0].FullValue = string.Empty;
                    columns[1].FullValue = temp;
                }
                else
                {
                    columns[0] = timestmp;
                    columns[1].FullValue = cols[1];
                }
            }
            return cLogLine;
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

        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, ILogLine logLine)
        {
            string temp = logLine.FullLine;

            // delete '[#|' and '|#]'
            if (temp.StartsWith("[#|"))
            {
                temp = temp.Substring(3);
            }
            if (temp.EndsWith("|#]"))
            {
                temp = temp.Substring(0, temp.Length - 3);
            }

            if (temp.Length < 28)
            {
                return DateTime.MinValue;
            }

            int endIndex = temp.IndexOf(separatorChar, 1);
            if (endIndex > 28 || endIndex < 0)
            {
                return DateTime.MinValue;
            }
            string value = temp.Substring(0, endIndex);

            try
            {
                // convert glassfish timestamp into a readable format:
                DateTime timestamp;
                if (DateTime.TryParseExact(value, DATETIME_FORMAT, cultureInfo,
                    System.Globalization.DateTimeStyles.None, out timestamp))
                {
                    return timestamp.AddMilliseconds(this.timeOffset);
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            if (column == 0)
            {
                try
                {
                    DateTime newDateTime = DateTime.ParseExact(value, DATETIME_FORMAT_OUT, this.cultureInfo);
                    DateTime oldDateTime = DateTime.ParseExact(oldValue, DATETIME_FORMAT_OUT, this.cultureInfo);
                    long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    this.timeOffset = (int) (mSecsNew - mSecsOld);
                }
                catch (FormatException)
                {
                }
            }
        }

        public Priority GetPriority(string fileName, IEnumerable<ILogLine> samples)
        {
            return Priority.NotSupport;
        }

        #endregion

        private class GlassFishLogLine : ILogLine
        {
            #region Properties

            public string FullLine { get; set; }

            public int LineNumber { get; set; }

            string ITextValue.Text => FullLine;

            #endregion
        }
    }
}