using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
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
		private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private string startTag = "<log4j:event";
		private string endTag = "</log4j:event>";

		private string xsl = "" +
	"<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"no\"?>" +
	"<xsl:stylesheet version=\"2.0\"" +
	"        xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"" +
	"        xmlns:log4j=\"http://jakarta.apache.org/log4j\">" +
	"<xsl:output method=\"text\" />" +
	"<xsl:template match=\"/log4j:event\"><xsl:value-of select=\"//@timestamp\"/>&#xFFFD;<xsl:value-of select=\"//@level\"/>&#xFFFD;<xsl:value-of select=\"//@logger\"/>&#xFFFD;<xsl:value-of select=\"//@thread\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@class\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@method\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@file\"/>&#xFFFD;<xsl:value-of select=\"//log4j:locationInfo/@line\"/>&#xFFFD;<xsl:value-of select=\"//log4j:message\"/><xsl:value-of select=\"//log4j:throwable\"/>" +
	"</xsl:template>" +
	"</xsl:stylesheet>";

		#region IXmlLogConfiguration Member

		public string XmlStartTag
		{
			get { return startTag; }
		}

		public string XmlEndTag
		{
			get { return endTag; }
		}

		public string Stylesheet
		{
			get { return xsl; }
		}

		public string[] Namespace
		{
			get { return new string[] { "log4j", "http://jakarta.apache.org/log4j" }; }
		}

		#endregion IXmlLogConfiguration Member
	}

	/// <summary>
	/// Helper class for configuration of the colums.
	/// </summary>
	[Serializable]
	public class Log4jColumnEntry
	{
		public Log4jColumnEntry(string name, int index, int maxLen)
		{
			this.columnName = name;
			this.columnIndex = index;
			this.visible = true;
			this.maxLen = maxLen;
		}

		public string columnName;
		public int columnIndex;
		public bool visible;
		public int maxLen;
	}

	[Serializable]
	public class Log4jXmlColumnizerConfig
	{
		public List<Log4jColumnEntry> columnList = new List<Log4jColumnEntry>();
		public bool localTimestamps = true;

		public Log4jXmlColumnizerConfig(string[] columnNames)
		{
			FillDefaults(columnNames);
		}

		public void FillDefaults(string[] columnNames)
		{
			this.columnList.Clear();
			for (int i = 0; i < columnNames.Length; ++i)
			{
				this.columnList.Add(new Log4jColumnEntry(columnNames[i], i, 0));
			}
		}

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
						count++;
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
						names[index++] = entry.columnName;
				}
				return names;
			}
		}
	}

	public class Log4jXmlColumnizer : ILogLineXmlColumnizer, IColumnizerConfigurator
	{
		private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		public const int COLUMN_COUNT = 9;

		private static XmlConfig xmlConfig = new XmlConfig();
		protected int timeOffset = 0;
		protected CultureInfo cultureInfo = new CultureInfo("de-DE");
		protected const String DATETIME_FORMAT = "dd.MM.yyyy HH:mm:ss.fff";
		private char[] trimChars = new char[] { '\xFFFD' };
		private char separatorChar = '\xFFFD';
		private Log4jXmlColumnizerConfig config;

		public Log4jXmlColumnizer()
		{
			config = new Log4jXmlColumnizerConfig(GetAllColumnNames());
		}

		#region ILogLineXmlColumnizer Member

		public IXmlLogConfiguration GetXmlLogConfiguration()
		{
			return xmlConfig;
		}

		public string GetLineTextForClipboard(string logLine, ILogLineColumnizerCallback callback)
		{
			return logLine.Replace(separatorChar, '|');
		}

		#endregion ILogLineXmlColumnizer Member

		#region ILogLineColumnizer Member

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

		public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
		{
			string[] cols = new string[Log4jXmlColumnizer.COLUMN_COUNT] { "", "", "", "", "", "", "", "", "" };

			// If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
			// in colum 8 (the log message column). Date and time column will be left blank.
			if (line.Length < 15)
			{
				cols[8] = line;
			}
			else
			{
				try
				{
					DateTime dateTime = GetTimestamp(callback, line);
					if (dateTime == DateTime.MinValue)
					{
						cols = new string[Log4jXmlColumnizer.COLUMN_COUNT] { "", "", "", "", "", "", "", "", line };
					}
					string newDate = dateTime.ToString(DATETIME_FORMAT);
					cols[0] = newDate;
				}
				catch (Exception ex)
				{
					_logger.Error(ex);
					cols[0] = "n/a";
				}

				string timestmp = cols[0];
				cols = GetColsFromLine(line);
				if (cols.Length != Log4jXmlColumnizer.COLUMN_COUNT)
				{
					cols = new string[Log4jXmlColumnizer.COLUMN_COUNT] { "", "", "", "", "", "", "", "", line };
				}
				else
				{
					cols[0] = timestmp;
				}
			}

			return MapColumns(cols);
		}

		private string[] GetColsFromLine(string line)
		{
			string[] cols;
			cols = line.Split(this.trimChars, Log4jXmlColumnizer.COLUMN_COUNT, StringSplitOptions.None);
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

		public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
		{
			if (line.Length < 15)
			{
				return DateTime.MinValue;
			}

			int endIndex = line.IndexOf(separatorChar, 1);
			if (endIndex > 20 || endIndex < 0)
			{
				return DateTime.MinValue;
			}
			string value = line.Substring(0, endIndex);

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
						dateTime = dateTime.ToLocalTime();
					return dateTime.AddMilliseconds(this.timeOffset);
				}
				else
				{
					return DateTime.MinValue;
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
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
					this.timeOffset = (int)(mSecsNew - mSecsOld);
				}
				catch (FormatException ex)
				{
					_logger.Error(ex);
				}
			}
		}

		#endregion ILogLineColumnizer Member

		private string[] GetAllColumnNames()
		{
			return new string[] { "Timestamp", "Level", "Logger", "Thread", "Class", "Method", "File", "Line", "Message" };
		}

		/// <summary>
		/// Returns only the columns which are "active". The order of the columns depends on the column order in the config
		/// </summary>
		/// <param name="cols"></param>
		/// <returns></returns>
		private string[] MapColumns(string[] cols)
		{
			string[] result = new string[GetColumnCount()];
			int index = 0;
			foreach (Log4jColumnEntry entry in this.config.columnList)
			{
				if (entry.visible)
				{
					result[index] = cols[entry.columnIndex];
					if (entry.maxLen > 0 && result[index].Length > entry.maxLen)
					{
						result[index] = result[index].Substring(result[index].Length - entry.maxLen);
					}
					index++;
				}
			}
			return result;
		}

		public string Text
		{
			get { return GetName(); }
		}

		#region IColumnizerConfigurator Member

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
					this.config = (Log4jXmlColumnizerConfig)formatter.Deserialize(fs);
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

		#endregion IColumnizerConfigurator Member
	}
}