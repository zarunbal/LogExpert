using LogExpert;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace RegexColumnizer
{
	public class RegexColumnizerConfig
	{
		public string Expression { get; set; } = "(?<text>.*)";
	}

	public class Regex1Columnizer : ILogLineColumnizer, IColumnizerConfigurator
	{
		private readonly XmlSerializer xml = new XmlSerializer(typeof(RegexColumnizerConfig));
		private string[] columns;
		public RegexColumnizerConfig Config { get; private set; }
		public Regex Regex { get; private set; }

		public string GetName() => "Regex";
		public string GetDescription() => "Columns are filled by regular expression named capture groups";
		public int GetColumnCount() => columns.Length;
		public string[] GetColumnNames() => columns;

		public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
		{
			ColumnizedLogLine logLine = new ColumnizedLogLine();

			logLine.ColumnValues = new IColumn[columns.Length];
			if (Regex != null)
			{
				var m = Regex.Match(line.FullLine);
				for (int i = m.Groups.Count - 1; i > 0; i--)
				{
					logLine.ColumnValues[i-1] = new Column
					{
						Parent = logLine,
						FullValue = m.Groups[i].Value
					};
				}
			}
			else
			{
				IColumn colVal = new Column
				{
					Parent = logLine,
					FullValue = line.FullLine
				};

				logLine.ColumnValues[0] = colVal;
			}
			logLine.LogLine = line;
			return logLine;
		}


		#region timeshift       

		public bool IsTimeshiftImplemented() => false;

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

		#endregion

		public void Configure(ILogLineColumnizerCallback callback, string configDir)
		{
			RegexColumnizerConfigDialog d = new RegexColumnizerConfigDialog { Config = Config };
			if (d.ShowDialog() == DialogResult.OK)
			{
				var configFile = GetConfigFile(configDir);
				using (var w = new FileStream(configFile, FileMode.Create))
				{
					xml.Serialize(w, d.Config);
				}

		                Init(d.Config);
			}
		}

		public void LoadConfig(string configDir)
		{
			var configFile = GetConfigFile(configDir);
			RegexColumnizerConfig config;
			if (!File.Exists(configFile))
			{
				config = new RegexColumnizerConfig();
			}
			else
			{
				using (var reader = new StreamReader(configFile))
				{
					config = xml.Deserialize(reader) as RegexColumnizerConfig;
				}
			}

			Init(config);
		}

		private void Init(RegexColumnizerConfig config)
		{
			Config = config;

			try
			{
				Regex = new Regex(Config.Expression, RegexOptions.Compiled);
				int skip = Regex.GetGroupNames().Length == 1 ? 0 : 1;
				columns = Regex.GetGroupNames().Skip(skip).ToArray();
			}
			catch
			{
				Regex = null;
			}
		}

		public string GetConfigFile(string configDir)
		{
			var name = GetType().Name;
			string configPath = Path.Combine(configDir, name);
			configPath = Path.ChangeExtension(configPath, "xml");
			return configPath;
		}
	}

	public class Regex2Columnizer : Regex1Columnizer { }
	public class Regex3Columnizer : Regex1Columnizer { }
	public class Regex4Columnizer : Regex1Columnizer { }
	public class Regex5Columnizer : Regex1Columnizer { }
	public class Regex6Columnizer : Regex1Columnizer { }
	public class Regex7Columnizer : Regex1Columnizer { }
	public class Regex8Columnizer : Regex1Columnizer { }
	public class Regex9Columnizer : Regex1Columnizer { }
}
