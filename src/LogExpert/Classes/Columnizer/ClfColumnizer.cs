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
    using System.Text.RegularExpressions;

    public class ClfColumnizer : ILogLineColumnizer
    {
        #region Fields

        private readonly Regex lineRegex = new Regex("(.*) (-) (.*) (\\[.*\\]) (\".*\") (.*) (.*) (\".*\") (\".*\")");

        protected CultureInfo cultureInfo = new CultureInfo("de-DE");
        protected int timeOffset = 0;

        #endregion

        #region cTor

        // anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html" 

        public ClfColumnizer()
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
            if (cols == null || cols.ColumnValues.Length < 8)
            {
                return DateTime.MinValue;
            }
            if (cols.ColumnValues[2].FullValue.Length == 0)
            {
                return DateTime.MinValue;
            }
            try
            {
                DateTime dateTime = DateTime.ParseExact(cols.ColumnValues[2].FullValue, "dd/MMM/yyyy:HH:mm:ss zzz",
                    new CultureInfo("en-US"));
                return dateTime;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }


        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            if (column == 2)
            {
                try
                {
                    DateTime newDateTime =
                        DateTime.ParseExact(value, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
                    DateTime oldDateTime =
                        DateTime.ParseExact(oldValue, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
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
            return "Webserver CLF Columnizer";
        }

        public string GetDescription()
        {
            return "Common Logfile Format used by webservers.";
        }

        public int GetColumnCount()
        {
            return 8;
        }

        public string[] GetColumnNames()
        {
            return new string[] {"IP", "User", "Date/Time", "Request", "Status", "Bytes", "Referrer", "User agent"};
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            ColumnizedLogLine cLogLine = new ColumnizedLogLine
            {
                LogLine = line
            };

            Column[] columns = new Column[8]
            {
                new Column {FullValue = "", Parent = cLogLine},
                new Column {FullValue = "", Parent = cLogLine},
                new Column {FullValue = "", Parent = cLogLine},
                new Column {FullValue = "", Parent = cLogLine},
                new Column {FullValue = "", Parent = cLogLine},
                new Column {FullValue = "", Parent = cLogLine},
                new Column {FullValue = "", Parent = cLogLine},
                new Column {FullValue = "", Parent = cLogLine}
            };


            cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();


            string temp = line.FullLine;
            if (temp.Length > 1024)
            {
                // spam 
                temp = temp.Substring(0, 1024);
                columns[3].FullValue = temp;
                return cLogLine;
            }
            // 0         1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16
            // anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html" 

            if (this.lineRegex.IsMatch(temp))
            {
                Match match = this.lineRegex.Match(temp);
                GroupCollection groups = match.Groups;
                if (groups.Count == 10)
                {
                    columns[0].FullValue = groups[1].Value;
                    columns[1].FullValue = groups[3].Value;
                    columns[3].FullValue = groups[5].Value;
                    columns[4].FullValue = groups[6].Value;
                    columns[5].FullValue = groups[7].Value;
                    columns[6].FullValue = groups[8].Value;
                    columns[7].FullValue = groups[9].Value;

                    string dateTimeStr = groups[4].Value.Substring(1, 26);

                    // dirty probing of date/time format (much faster than DateTime.ParseExact()
                    if (dateTimeStr[2] == '/' && dateTimeStr[6] == '/' && dateTimeStr[11] == ':')
                    {
                        if (this.timeOffset != 0)
                        {
                            try
                            {
                                DateTime dateTime = DateTime.ParseExact(dateTimeStr, "dd/MMM/yyyy:HH:mm:ss zzz",
                                    new CultureInfo("en-US"));
                                dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, this.timeOffset));
                                string newDate = dateTime.ToString("dd/MMM/yyyy:HH:mm:ss zzz",
                                    new CultureInfo("en-US"));
                                columns[2].FullValue = newDate;
                            }
                            catch (Exception)
                            {
                                columns[2].FullValue = "n/a";
                            }
                        }
                        else
                        {
                            columns[2].FullValue = dateTimeStr;
                        }
                    }
                    else
                    {
                        columns[2].FullValue = dateTimeStr;
                    }
                }
            }
            else
            {
                columns[3].FullValue = temp;
            }
            return cLogLine;
        }

        public Priority GetPriority(string fileName, IEnumerable<ILogLine> samples)
        {
            return Priority.NotSupport;
        }

        #endregion
    }
}