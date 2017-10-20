using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Globalization;
using ColumnizerLib;

namespace GlassfishColumnizer
{
    internal class XmlConfig : IXmlLogConfiguration
    {
        #region Fields

        #endregion


        #region IXmlLogConfiguration Member

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

        private static XmlConfig xmlConfig = new XmlConfig();
        protected CultureInfo cultureInfo = new CultureInfo("en-US");
        private char separatorChar = '|';
        protected int timeOffset = 0;
        private char[] trimChars = new char[] {'|'};

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

        private class GlassFishLogLine : ILogLine
        {
            #region Fields

            private static readonly int _maxLength = 20000 - 3;
            private string _fullLine;

            #endregion

            #region Properties

            public string FullLine
            {
                get { return _fullLine; }
                set
                {
                    _fullLine = value;
                    if (_fullLine.Length > _maxLength)
                    {
                        DisplayLine = _fullLine.Substring(0, _maxLength) + "...";
                    }
                    else
                    {
                        DisplayLine = FullLine;
                    }
                }
            }

            public int LineNumber { get; set; }
            public string DisplayLine { get; private set; }

            #endregion
        }

        #region ILogLineXmlColumnizer Member

        public IXmlLogConfiguration GetXmlLogConfiguration()
        {
            return xmlConfig;
        }

        public ILogLine GetLineTextForClipboard(string logLine, ILogLineColumnizerCallback callback)
        {
            GlassFishLogLine line = new GlassFishLogLine
            {
                FullLine = logLine.Replace(separatorChar, '|'),
                LineNumber = callback.GetLineNum()
            };

            return line;
        }

        #endregion


        #region ILogLineColumnizer Member

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

        public string[] SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            string temp = line.FullLine;
            string[] cols = new string[COLUMN_COUNT] {"", ""};

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
                cols[1] = temp;
            }
            else
            {
                try
                {
                    DateTime dateTime = GetTimestamp(callback, line);
                    if (dateTime == DateTime.MinValue)
                    {
                        cols = new string[COLUMN_COUNT] {"", temp};
                    }
                    string newDate = dateTime.ToString(DATETIME_FORMAT_OUT);
                    cols[0] = newDate;
                }
                catch (Exception)
                {
                    cols[0] = "n/a";
                }

                string timestmp = cols[0];
                cols = GetColsFromLine(temp);
                if (cols.Length != COLUMN_COUNT)
                {
                    cols = new string[COLUMN_COUNT] {"", temp};
                }
                else
                {
                    cols[0] = timestmp;
                }
            }
            return cols;
        }


        private string[] GetColsFromLine(string line)
        {
            string[] cols;
            cols = line.Split(this.trimChars, COLUMN_COUNT, StringSplitOptions.None);
            return cols;
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

        #endregion
    }
}