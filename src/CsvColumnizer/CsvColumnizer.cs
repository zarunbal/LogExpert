using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace CsvColumnizer
{
    internal class CsvColumn
    {
        #region cTor

        public CsvColumn(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name { get; }

        #endregion
    }

    [Serializable]
    public class CsvColumnizerConfig
    {
        #region Fields

        public char commentChar;
        public char delimiterChar;
        public char escapeChar;
        public bool hasFieldNames;
        public int minColumns;
        public char quoteChar;

        #endregion

        #region Public methods

        public void InitDefaults()
        {
            delimiterChar = ';';
            escapeChar = '"';
            quoteChar = '"';
            commentChar = '#';
            hasFieldNames = true;
            minColumns = 0;
        }

        #endregion
    }

    /// <summary>
    /// This Columnizer can parse CSV files. It uses the IInitColumnizer interface for support of dynamic field count.
    /// The IPreProcessColumnizer is implemented to read field names from the very first line of the file. Then
    /// the line is dropped. So it's not seen by LogExpert. The field names will be used as column names.
    /// </summary>
    public class CsvColumnizer : ILogLineColumnizer, IInitColumnizer, IColumnizerConfigurator, IPreProcessColumnizer
    {
        #region Fields

        private readonly IList<CsvColumn> columnList = new List<CsvColumn>();
        private CsvColumnizerConfig config;

        private ILogLine firstLine;

        // if CSV is detected to be 'invalid' the columnizer will behave like a default columnizer
        private bool isValidCsv;

        #endregion

        #region Properties

        public string Text
        {
            get { return GetName(); }
        }

        #endregion

        #region Public methods

        public string PreProcessLine(string logLine, int lineNum, int realLineNum)
        {
            if (realLineNum == 0)
            {
                // store for later field names and field count retrieval
                firstLine = new CsvLogLine
                {
                    FullLine = logLine,
                    LineNumber = 0
                };
                if (config.minColumns > 0)
                {
                    using (CsvReader csv = new CsvReader(new StringReader(logLine),
                        false,
                        config.delimiterChar,
                        config.quoteChar,
                        config.escapeChar, // is '\0' when not checked in config dlg
                        config.commentChar,
                        ValueTrimmingOptions.None))
                    {
                        if (csv.FieldCount < config.minColumns)
                        {
                            // on invalid CSV don't hide the first line from LogExpert, since the file will be displayed in plain mode
                            isValidCsv = false;
                            return logLine;
                        }
                    }
                }

                isValidCsv = true;
            }

            if (config.hasFieldNames && realLineNum == 0)
            {
                return null; // hide from LogExpert
            }

            if (config.commentChar != ' ' && logLine.StartsWith("" + config.commentChar))
            {
                return null;
            }

            return logLine;
        }

        public string GetName()
        {
            return "CSV Columnizer";
        }

        public string GetDescription()
        {
            return
                "Splits CSV files into columns.\r\n\r\nCredits:\r\nThis Columnizer uses the CsvReader class written by Sébastien Lorion. Downloaded from codeproject.com.\r\n";
        }

        public int GetColumnCount()
        {
            return isValidCsv ? columnList.Count : 1;
        }

        public string[] GetColumnNames()
        {
            string[] names = new string[GetColumnCount()];
            if (isValidCsv)
            {
                int i = 0;
                foreach (CsvColumn column in columnList)
                {
                    names[i++] = column.Name;
                }
            }
            else
            {
                names[0] = "Text";
            }

            return names;
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            if (isValidCsv)
            {
                return SplitCsvLine(line);
            }
            else
            {
                ColumnizedLogLine cLogLine = new ColumnizedLogLine();
                cLogLine.LogLine = line;
                cLogLine.ColumnValues = new IColumn[] {new Column {FullValue = line.FullLine, Parent = cLogLine}};

                return cLogLine;
            }
        }

        public bool IsTimeshiftImplemented()
        {
            return false;
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
            throw new NotImplementedException();
        }

        public void Selected(ILogLineColumnizerCallback callback)
        {
            if (isValidCsv) // see PreProcessLine()
            {
                columnList.Clear();
                ILogLine line = config.hasFieldNames ? firstLine : callback.GetLogLine(0);

                if (line != null)
                {
                    using (CsvReader csv = new CsvReader(new StringReader(line.FullLine),
                        false,
                        config.delimiterChar,
                        config.quoteChar,
                        config.escapeChar, // is '\0' when not checked in config dlg
                        config.commentChar,
                        ValueTrimmingOptions.None))
                    {
                        csv.ReadNextRecord();
                        int fieldCount = csv.FieldCount;

                        List<Column> columns = new List<Column>();

                        for (int i = 0; i < fieldCount; ++i)
                        {
                            if (config.hasFieldNames)
                            {
                                columnList.Add(new CsvColumn(csv[i]));
                            }
                            else
                            {
                                columnList.Add(new CsvColumn("Column " + i + 1));
                            }
                        }
                    }
                }
            }
        }

        public void DeSelected(ILogLineColumnizerCallback callback)
        {
            // nothing to do 
        }

        public void Configure(ILogLineColumnizerCallback callback, string configDir)
        {
            string configPath = configDir + "\\csvcolumnizer.dat";
            CsvColumnizerConfigDlg dlg = new CsvColumnizerConfigDlg(config);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(fs, config);
                fs.Close();
                Selected(callback);
            }
        }

        public void LoadConfig(string configDir)
        {
            string configPath = configDir + "\\csvcolumnizer.dat";

            if (!File.Exists(configPath))
            {
                config = new CsvColumnizerConfig();
                config.InitDefaults();
            }
            else
            {
                Stream fs = File.OpenRead(configPath);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    config = (CsvColumnizerConfig) formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    MessageBox.Show(e.Message, "Deserialize");
                    config = new CsvColumnizerConfig();
                    config.InitDefaults();
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
            if (fileName.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
            {
                result = Priority.CanSupport;
            }
            return result;
        }

        #endregion

        #region Private Methods

        private IColumnizedLogLine SplitCsvLine(ILogLine line)
        {
            ColumnizedLogLine cLogLine = new ColumnizedLogLine();
            cLogLine.LogLine = line;

            using (CsvReader csv = new CsvReader(new StringReader(line.FullLine),
                false,
                config.delimiterChar,
                config.quoteChar,
                config.escapeChar, // is '\0' when not checked in config dlg
                config.commentChar,
                ValueTrimmingOptions.None))
            {
                csv.ReadNextRecord();
                int fieldCount = csv.FieldCount;

                List<Column> columns = new List<Column>();

                for (int i = 0; i < fieldCount; ++i)
                {
                    columns.Add(new Column {FullValue = csv[i], Parent = cLogLine});
                }

                cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

                return cLogLine;
            }
        }

        #endregion

        private class CsvLogLine : ILogLine
        {
            #region Properties

            public string FullLine { get; set; }

            public int LineNumber { get; set; }

            string ITextValue.Text => FullLine;

            #endregion
        }
    }
}