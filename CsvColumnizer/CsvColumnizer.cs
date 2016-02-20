using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace CsvColumnizer
{
	internal class CsvColumn
	{
		private string name;

		public CsvColumn(string name)
		{
			this.name = name;
		}

		public string Name
		{
			get { return this.name; }
		}
	}

	[Serializable]
	public class CsvColumnizerConfig
	{
		public char delimiterChar;
		public char quoteChar;
		public char escapeChar;
		public bool hasFieldNames;
		public char commentChar;
		public int minColumns;

		public void InitDefaults()
		{
			this.delimiterChar = ';';
			this.escapeChar = '"';
			this.quoteChar = '"';
			this.commentChar = '#';
			this.hasFieldNames = true;
			this.minColumns = 0;
		}
	}

	/// <summary>
	/// This Columnizer can parse CSV files. It uses the IInitColumnizer interface for support of dynamic field count.
	/// The IPreProcessColumnizer is implemented to read field names from the very first line of the file. Then
	/// the line is dropped. So it's not seen by LogExpert. The field names will be used as column names.
	/// </summary>
	public class CsvColumnizer : ILogLineColumnizer, IInitColumnizer, IColumnizerConfigurator, IPreProcessColumnizer
	{
		private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private CsvColumnizerConfig config;

		private IList<CsvColumn> columnList = new List<CsvColumn>();

		private string firstLine;

		// if CSV is detected to be 'invalid' the columnizer will behave like a default columnizer
		private bool isValidCsv;

		#region ILogLineColumnizer Member

		public string GetName()
		{
			return "CSV Columnizer";
		}

		public string GetDescription()
		{
			return "Splits CSV files into columns.\r\n\r\nCredits:\r\nThis Columnizer uses the CsvReader class written by Sébastien Lorion. Downloaded from codeproject.com.\r\n";
		}

		public int GetColumnCount()
		{
			return this.isValidCsv ? this.columnList.Count : 1;
		}

		public string[] GetColumnNames()
		{
			string[] names = new string[GetColumnCount()];
			if (this.isValidCsv)
			{
				int i = 0;
				foreach (CsvColumn column in this.columnList)
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

		public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
		{
			if (this.isValidCsv)
			{
				return SplitCsvLine(line);
			}
			else
			{
				return new string[] { line };
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

		public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
		{
			throw new NotImplementedException();
		}

		public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
		{
			throw new NotImplementedException();
		}

		#endregion ILogLineColumnizer Member

		#region IInitColumnizer Member

		public void Selected(ILogLineColumnizerCallback callback)
		{
			if (this.isValidCsv) // see PreProcessLine()
			{
				this.columnList.Clear();
				string line = this.config.hasFieldNames ? this.firstLine : callback.GetLogLine(0);
				int i = 1;
				if (line != null)
				{
					string[] fields = SplitCsvLine(line);
					foreach (string field in fields)
					{
						if (this.config.hasFieldNames)
							this.columnList.Add(new CsvColumn(field));
						else
							this.columnList.Add(new CsvColumn("Column " + (i++)));
					}
				}
			}
		}

		public void DeSelected(ILogLineColumnizerCallback callback)
		{
			// nothing to do
		}

		#endregion IInitColumnizer Member

		public string Text
		{
			get { return GetName(); }
		}

		private string[] SplitCsvLine(string line)
		{
			CsvReader csv = new CsvReader(new StringReader(line),
			  false,
			  this.config.delimiterChar,
			  this.config.quoteChar,
			  this.config.escapeChar,   // is '\0' when not checked in config dlg
			  this.config.commentChar,
			  false);
			csv.ReadNextRecord();
			int fieldCount = csv.FieldCount;
			string[] fields = new string[fieldCount];
			for (int i = 0; i < fieldCount; ++i)
			{
				fields[i] = csv[i];
			}
			csv.Dispose();
			return fields;
		}

		#region IColumnizerConfigurator Member

		public void Configure(ILogLineColumnizerCallback callback, string configDir)
		{
			string configPath = configDir + "\\csvcolumnizer.dat";
			CsvColumnizerConfigDlg dlg = new CsvColumnizerConfigDlg(this.config);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				BinaryFormatter formatter = new BinaryFormatter();
				Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
				formatter.Serialize(fs, this.config);
				fs.Close();
				Selected(callback);
			}
		}

		public void LoadConfig(string configDir)
		{
			string configPath = configDir + "\\csvcolumnizer.dat";

			if (!File.Exists(configPath))
			{
				this.config = new CsvColumnizerConfig();
				this.config.InitDefaults();
			}
			else
			{
				Stream fs = File.OpenRead(configPath);
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					this.config = (CsvColumnizerConfig)formatter.Deserialize(fs);
				}
				catch (SerializationException e)
				{
					_logger.Error(e);
					MessageBox.Show(e.Message, "Deserialize");
					this.config = new CsvColumnizerConfig();
					this.config.InitDefaults();
				}
				finally
				{
					fs.Close();
				}
			}
		}

		#endregion IColumnizerConfigurator Member

		#region IPreProcessColumnizer Member

		public string PreProcessLine(string logLine, int lineNum, int realLineNum)
		{
			if (realLineNum == 0)
			{
				this.firstLine = logLine;   // store for later field names and field count retrieval
				if (this.config.minColumns > 0)
				{
					string[] headers = SplitCsvLine(logLine);
					if (headers.Length < this.config.minColumns)
					{
						// on invalid CSV don't hide the first line from LogExpert, since the file will be displayed in plain mode
						this.isValidCsv = false;
						return logLine;
					}
				}
				this.isValidCsv = true;
			}

			if (this.config.hasFieldNames && realLineNum == 0)
			{
				return null; // hide from LogExpert
			}

			if (this.config.commentChar != ' ' && logLine.StartsWith("" + this.config.commentChar))
			{
				return null;
			}

			return logLine;
		}

		#endregion IPreProcessColumnizer Member
	}
}