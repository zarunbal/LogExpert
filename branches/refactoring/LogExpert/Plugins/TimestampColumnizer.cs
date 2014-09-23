using System;
using System.Collections.Generic;
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
			public FormatInfo(string dateFormat, string timeFormat, CultureInfo cultureInfo)
			{
				this.DateFormat = dateFormat;
				this.TimeFormat = timeFormat;
				this.CultureInfo = cultureInfo;
			}
			
			public string DateFormat { get; private set; }
			
			public string TimeFormat { get; private set; }
			
			public CultureInfo CultureInfo { get; private set; }
			
			public string DateTimeFormat
			{
				get
				{
					return this.DateFormat + " " + this.TimeFormat;
				}
			}
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
		
		public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
		{
			string[] cols = SplitLine(callback, line);
			if (cols == null || cols.Length < 2)
			{
				return DateTime.MinValue;
			}
			if (cols[0].Length == 0 || cols[1].Length == 0)
			{
				return DateTime.MinValue;
			}
			FormatInfo formatInfo = DetermineDateTimeFormatInfo(line);
			if (formatInfo == null)
				return DateTime.MinValue;
			
			try
			{
				DateTime dateTime = DateTime.ParseExact(cols[0] + " " + cols[1], formatInfo.DateTimeFormat, formatInfo.CultureInfo);
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
						return;
					DateTime newDateTime = DateTime.ParseExact(value, formatInfo.TimeFormat, formatInfo.CultureInfo);
					DateTime oldDateTime = DateTime.ParseExact(oldValue, formatInfo.TimeFormat, formatInfo.CultureInfo);
					long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
					long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
					this.timeOffset = (int)(mSecsNew - mSecsOld);
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
			return new string[] { "Date", "Time", "Message" };
		}
		
		public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
		{ // 0         1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16
			// 012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			// 03.01.2008 14:48:00.066 <rest of line>
			if (line.Length < 21)
			{
				return new string[] { "", "", line };
			}
			string[] cols = new string[3];
			FormatInfo formatInfo = DetermineDateTimeFormatInfo(line);
			if (formatInfo == null)
			{
				cols[0] = cols[1] = "";
				cols[2] = line;
				return cols;
			}
			int endPos = formatInfo.DateTimeFormat.Length;
			int timeLen = formatInfo.TimeFormat.Length;
			int dateLen = formatInfo.DateFormat.Length;
			try
			{
				if (this.timeOffset != 0)
				{
					DateTime dateTime = DateTime.ParseExact(line.Substring(0, endPos), formatInfo.DateTimeFormat, formatInfo.CultureInfo);
					dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, this.timeOffset));
					string newDate = dateTime.ToString(formatInfo.DateTimeFormat, formatInfo.CultureInfo);
					cols[0] = newDate.Substring(0, dateLen);         // date
					cols[1] = newDate.Substring(dateLen + 1, timeLen);   // time
					cols[2] = line.Substring(endPos);           // rest of line
				}
				else
				{
					cols[0] = line.Substring(0, dateLen);             // date
					cols[1] = line.Substring(dateLen + 1, timeLen);   // time
					cols[2] = line.Substring(endPos);                 // rest of line
				}
			}
			catch (Exception)
			{
				cols[0] = "n/a";
				cols[1] = "n/a";
				cols[2] = line;
			}
			return cols;
		}
		
		#endregion
		
		#region internal stuff
		
		public string Text
		{
			get
			{
				return GetName();
			}
		}
		
		protected FormatInfo DetermineDateTimeFormatInfo(string line)
		{
			// dirty hardcoded probing of date/time format (much faster than DateTime.ParseExact()
			if (line[2] == '.' && line[5] == '.' && line[13] == ':' && line[16] == ':')
			{
				if (line[19] == '.')
					return this.formatInfo1;
				else if (line[19] == ',')
					return this.formatInfo7;
				else
					return this.formatInfo2;
			}
			else if (line[4] == '/' && line[7] == '/' && line[13] == ':' && line[16] == ':')
			{
				if (line[19] == '.')
					return this.formatInfo3;
				else if (line[19] == ',')
					return this.formatInfo8;
				else
					return this.formatInfo4;
			}
			else if (line[4] == '.' && line[7] == '.' && line[13] == ':' && line[16] == ':')
			{
				if (line[19] == '.')
					return this.formatInfo5;
				else if (line[19] == ',')
					return this.formatInfo9;
				else
					return this.formatInfo6;
			}
			else if (line[4] == '-' && line[7] == '-' && line[13] == ':' && line[16] == ':')
			{
				if (line[19] == '.')
					return this.formatInfo10;
				else if (line[19] == ',')
					return this.formatInfo11;
				else if (line[19] == ':')
					return this.formatInfo17;
				else
					return this.formatInfo12;
			}
			else if (line[2] == ' ' && line[6] == ' ' && line[14] == ':' && line[17] == ':')
			{
				if (line[20] == ',')
					return this.formatInfo13;
				else if (line[20] == '.')
					return this.formatInfo14;
				else
					return this.formatInfo15;
			}
			//dd.MM.yy HH:mm:ss.fff
			else if (line[2] == '.' && line[5] == '.' && line[11] == ':' && line[14] == ':' && line[17] == '.')
			{
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
						return this.formatInfo1;
					else if (field[8] == ',')
						return this.formatInfo7;
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