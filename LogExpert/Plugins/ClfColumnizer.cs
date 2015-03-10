using System;
using System.Collections.Generic;
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
		protected int _timeOffset = 0;
		protected CultureInfo _cultureInfo = new CultureInfo("de-DE");
		private Regex _lineRegex = new Regex("(.*) (-) (.*) (\\[.*\\]) (\".*\") (.*) (.*) (\".*\") (\".*\")");

		// anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html" 

		public bool IsTimeshiftImplemented()
		{
			return true;
		}

		public void SetTimeOffset(int msecOffset)
		{
			_timeOffset = msecOffset;
		}

		public int GetTimeOffset()
		{
			return _timeOffset;
		}

		public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
		{
			string[] cols = SplitLine(callback, line);
			if (cols == null || cols.Length < 8)
			{
				return DateTime.MinValue;
			}
			if (cols[2].Length == 0)
			{
				return DateTime.MinValue;
			}
			try
			{
				DateTime dateTime = DateTime.ParseExact(cols[2], "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
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
					DateTime newDateTime = DateTime.ParseExact(value, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
					DateTime oldDateTime = DateTime.ParseExact(oldValue, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
					long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
					long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
					_timeOffset = (int)(mSecsNew - mSecsOld);
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
			return new string[] { "IP", "User", "Date/Time", "Request", "Status", "Bytes", "Referrer", "User agent" };
		}

		public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
		{
			string[] cols = new string[8] { "", "", "", "", "", "", "", "" };
			if (line.Length > 1024)
			{
				// spam 
				line = line.Substring(0, 1024);
				cols[3] = line;
				return cols;
			}
			// 0         1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16
			// anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html" 

			if (_lineRegex.IsMatch(line))
			{
				Match match = _lineRegex.Match(line);
				GroupCollection groups = match.Groups;
				if (groups.Count == 10)
				{
					cols[0] = groups[1].Value;
					cols[1] = groups[3].Value;
					cols[3] = groups[5].Value;
					cols[4] = groups[6].Value;
					cols[5] = groups[7].Value;
					cols[6] = groups[8].Value;
					cols[7] = groups[9].Value;

					string dateTimeStr = groups[4].Value.Substring(1, 26);

					// dirty probing of date/time format (much faster than DateTime.ParseExact()
					if (dateTimeStr[2] == '/' && dateTimeStr[6] == '/' && dateTimeStr[11] == ':')
					{
						if (_timeOffset != 0)
						{
							try
							{
								DateTime dateTime = DateTime.ParseExact(dateTimeStr, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
								dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, _timeOffset));
								string newDate = dateTime.ToString("dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
								cols[2] = newDate;
							}
							catch (Exception)
							{
								cols[2] = "n/a";
							}
						}
						else
						{
							cols[2] = dateTimeStr;
						}
					}
					else
					{
						cols[2] = dateTimeStr;
					}
				}
			}
			else
			{
				cols[3] = line;
			}
			return cols;
		}

		public string Text
		{
			get
			{
				return GetName();
			}
		}
	}
}