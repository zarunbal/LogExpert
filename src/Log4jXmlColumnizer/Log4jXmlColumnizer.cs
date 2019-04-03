using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;


namespace LogExpert
{
    /// <summary>
    /// XMl configuration for parsing log4j XML files. The XSL will transform every block of log entries
    /// into text lines. The fields in the text lines are separated by a special character (0xFFFD).
    /// The special character will be used in the Split() function of the columnizer to split the line
    /// into columns.
    /// </summary>
    internal class XmlConfig : IXmlLogConfiguration
    {
        #region Properties

        public string XmlStartTag { get; } = "<log4j:event";

        public string XmlEndTag { get; } = "</log4j:event>";

        public string Stylesheet { get; } = "" +
                                            "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"no\"?>" +
                                            "<xsl:stylesheet version=\"2.0\"" +
                                            "        xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"" +
                                            "        xmlns:log4j=\"http://jakarta.apache.org/log4j\">" +
                                            "<xsl:output method=\"text\" />" +
                                            "<xsl:template match=\"/log4j:event\"><xsl:value-of select=\"//@timestamp\"/>&#xFFFD;<xsl:value-of select=\"//@level\"/>&#xFFFD;<xsl:value-of select=\"//@logger\"/>&#xFFFD;<xsl:value-of select=\"//@thread\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@class\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@method\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@file\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@line\"/>&#xFFFD;<xsl:value-of select=\"//log4j:message\"/><xsl:value-of select=\"//log4j:throwable\"/>" +
                                            "</xsl:template>" +
                                            "</xsl:stylesheet>";

        public string[] Namespace
        {
            get { return new string[] {"log4j", "http://jakarta.apache.org/log4j"}; }
        }

        #endregion
    }


    /// <summary>
    /// Helper class for configuration of the colums.
    /// </summary>
    [Serializable]
    public class Log4jColumnEntry
    {
        #region Fields

        public int columnIndex;

        public string columnName;
        public int maxLen;
        public bool visible;

        #endregion

        #region cTor

        public Log4jColumnEntry(string name, int index, int maxLen)
        {
            this.columnName = name;
            this.columnIndex = index;
            this.visible = true;
            this.maxLen = maxLen;
        }

        #endregion
    }

    [Serializable]
    public class Log4jXmlColumnizerConfig
    {
        #region Fields

        public List<Log4jColumnEntry> columnList = new List<Log4jColumnEntry>();
        public bool localTimestamps = true;

        #endregion

        #region cTor

        public Log4jXmlColumnizerConfig(string[] columnNames)
        {
            FillDefaults(columnNames);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the column count. Because the user can deactivate columns in the config
        /// the actual column count may be smaller than the number of available columns.
        /// </summary>
        public int ActiveColumnCount
        {
            get
            {
                int count = 0;
                foreach (Log4jColumnEntry entry in this.columnList)
                {
                    if (entry.visible)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Returns the names of all active columns.
        /// </summary>
        public string[] ActiveColumnNames
        {
            get
            {
                string[] names = new string[ActiveColumnCount];
                int index = 0;
                foreach (Log4jColumnEntry entry in this.columnList)
                {
                    if (entry.visible)
                    {
                        names[index++] = entry.columnName;
                    }
                }
                return names;
            }
        }

        #endregion

        #region Public methods

        public void FillDefaults(string[] columnNames)
        {
            this.columnList.Clear();
            for (int i = 0; i < columnNames.Length; ++i)
            {
                this.columnList.Add(new Log4jColumnEntry(columnNames[i], i, 0));
            }
        }

        #endregion
    }


    public class Log4jXmlColumnizer : ILogLineXmlColumnizer, IColumnizerConfigurator
    {
        #region Fields

        public const int COLUMN_COUNT = 9;
        protected const string DATETIME_FORMAT = "dd.MM.yyyy HH:mm:ss.fff";

        private static readonly XmlConfig xmlConfig = new XmlConfig();
        private readonly char separatorChar = '\xFFFD';
        private readonly char[] trimChars = new char[] {'\xFFFD'};
        private Log4jXmlColumnizerConfig config;
        protected CultureInfo cultureInfo = new CultureInfo("de-DE");
        protected int timeOffset = 0;

        #endregion

        #region cTor

        public Log4jXmlColumnizer()
        {
            config = new Log4jXmlColumnizerConfig(GetAllColumnNames());
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
            Log4JLogLine line = new Log4JLogLine
            {
                FullLine = logLine.FullLine.Replace(separatorChar, '|'),
                LineNumber = logLine.LineNumber
            };

            return line;
        }

        public string GetName()
        {
            return "Log4j XML";
        }

        public string GetDescription()
        {
            return "Reads and formats XML log files written with log4j.";
        }

        public int GetColumnCount()
        {
            return config.ActiveColumnCount;
        }

        public string[] GetColumnNames()
        {
            return config.ActiveColumnNames;
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            ColumnizedLogLine clogLine = new ColumnizedLogLine();
            clogLine.LogLine = line;

            Column[] columns = Column.CreateColumns(Log4jXmlColumnizer.COLUMN_COUNT, clogLine);


            // If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
            // in colum 8 (the log message column). Date and time column will be left blank.
            if (line.FullLine.Length < 15)
            {
                columns[8].FullValue = line.FullLine;
            }
            else
            {
                try
                {
                    DateTime dateTime = GetTimestamp(callback, line);
                    if (dateTime == DateTime.MinValue)
                    {
                        columns[8].FullValue = line.FullLine;
                    }
                    string newDate = dateTime.ToString(DATETIME_FORMAT);
                    columns[0].FullValue = newDate;
                }
                catch (Exception)
                {
                    columns[0].FullValue = "n/a";
                }

                Column timestmp = columns[0];

                string[] cols;
                cols = line.FullLine.Split(this.trimChars, Log4jXmlColumnizer.COLUMN_COUNT, StringSplitOptions.None);

                if (cols.Length != Log4jXmlColumnizer.COLUMN_COUNT)
                {
                    columns[0].FullValue = "";
                    columns[1].FullValue = "";
                    columns[2].FullValue = "";
                    columns[3].FullValue = "";
                    columns[4].FullValue = "";
                    columns[5].FullValue = "";
                    columns[6].FullValue = "";
                    columns[7].FullValue = "";
                    columns[8].FullValue = line.FullLine;
                }
                else
                {
                    columns[0] = timestmp;

                    for (int i = 1; i < cols.Length; i++)
                    {
                        columns[i].FullValue = cols[i];
                    }
                }
            }

            Column[] filteredColumns = MapColumns(columns);

            clogLine.ColumnValues = filteredColumns.Select(a => a as IColumn).ToArray();


            return clogLine;
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
            if (line.FullLine.Length < 15)
            {
                return DateTime.MinValue;
            }

            int endIndex = line.FullLine.IndexOf(separatorChar, 1);
            if (endIndex > 20 || endIndex < 0)
            {
                return DateTime.MinValue;
            }
            string value = line.FullLine.Substring(0, endIndex);

            try
            {
                // convert log4j timestamp into a readable format:
                long timestamp;
                if (long.TryParse(value, out timestamp))
                {
                    // Add the time offset before returning
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddMilliseconds(timestamp);
                    if (this.config.localTimestamps)
                    {
                        dateTime = dateTime.ToLocalTime();
                    }
                    return dateTime.AddMilliseconds(this.timeOffset);
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
                    DateTime newDateTime = DateTime.ParseExact(value, DATETIME_FORMAT, this.cultureInfo);
                    DateTime oldDateTime = DateTime.ParseExact(oldValue, DATETIME_FORMAT, this.cultureInfo);
                    long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    this.timeOffset = (int) (mSecsNew - mSecsOld);
                }
                catch (FormatException)
                {
                }
            }
        }

        public void Configure(ILogLineColumnizerCallback callback, string configDir)
        {
            string configPath = configDir + "\\log4jxmlcolumnizer.dat";
            Log4jXmlColumnizerConfigDlg dlg = new Log4jXmlColumnizerConfigDlg(this.config);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(fs, this.config);
                fs.Close();
            }
        }

        public void LoadConfig(string configDir)
        {
            string configPath = configDir + "\\log4jxmlcolumnizer.dat";

            if (!File.Exists(configPath))
            {
                this.config = new Log4jXmlColumnizerConfig(GetAllColumnNames());
            }
            else
            {
                Stream fs = File.OpenRead(configPath);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    this.config = (Log4jXmlColumnizerConfig) formatter.Deserialize(fs);
                    if (this.config.columnList.Count < Log4jXmlColumnizer.COLUMN_COUNT)
                    {
                        this.config = new Log4jXmlColumnizerConfig(GetAllColumnNames());
                    }
                }
                catch (SerializationException e)
                {
                    MessageBox.Show(e.Message, "Deserialize");
                    this.config = new Log4jXmlColumnizerConfig(GetAllColumnNames());
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public Priority GetPriority(string fileName, IEnumerable<ILogLine> samples)
        {
            Priority result = Priority.NotSupport;
            if (fileName.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
            {
                result = Priority.CanSupport;
            }
            return result;
        }

        #endregion

        #region Private Methods

        private string[] GetAllColumnNames()
        {
            return new string[]
                {"Timestamp", "Level", "Logger", "Thread", "Class", "Method", "File", "Line", "Message"};
        }


        /// <summary>
        /// Returns only the columns which are "active". The order of the columns depends on the column order in the config
        /// </summary>
        /// <param name="cols"></param>
        /// <returns></returns>
        private Column[] MapColumns(Column[] cols)
        {
            List<Column> output = new List<Column>();
            int index = 0;
            foreach (Log4jColumnEntry entry in config.columnList)
            {
                if (entry.visible)
                {
                    Column column = cols[index];
                    output.Add(column);

                    if (entry.maxLen > 0 && column.FullValue.Length > entry.maxLen)
                    {
                        column.FullValue = column.FullValue.Substring(column.FullValue.Length - entry.maxLen);
                    }
                }
                index++;
            }


            return output.ToArray();
        }

        #endregion

        private class Log4JLogLine : ILogLine
        {
            #region Properties

            public string FullLine { get; set; }

            public int LineNumber { get; set; }

            string ITextValue.Text => FullLine;

            #endregion
        }
    }
}