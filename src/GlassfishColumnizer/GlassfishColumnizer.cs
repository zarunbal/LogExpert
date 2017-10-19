using System;
using System.Collections.Generic;
using System.Text;

using LogExpert;
using System.Globalization;

namespace GlassfishColumnizer
{

  class XmlConfig : IXmlLogConfiguration
  {
    private string startTag = "[#|";
    private string endTag = "|#]";
    private string xsl = null;


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
      get {return null;}
    }

    #endregion
  }


  class GlassfishColumnizer : ILogLineXmlColumnizer
  {
    public const int COLUMN_COUNT = 2;

    private static XmlConfig xmlConfig = new XmlConfig();
    protected int timeOffset = 0;
    protected CultureInfo cultureInfo = new CultureInfo("en-US");
    protected const String DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzzz";
    protected const String DATETIME_FORMAT_OUT = "yyyy-MM-dd HH:mm:ss.fff";
    private char[] trimChars = new char[] { '|' };
    private char separatorChar = '|';


    public GlassfishColumnizer()
    {
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
      return new String[]{"Date/Time", "Message"};
    }

    public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
    {
      string[] cols = new string[COLUMN_COUNT] { "", ""};

      // delete '[#|' and '|#]'
      if (line.StartsWith("[#|"))
        line = line.Substring(3);
      if (line.EndsWith("|#]"))
        line = line.Substring(0, line.Length - 3);

      // If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
      // in colum 8 (the log message column). Date and time column will be left blank.
      if (line.Length < 28)
      {
        cols[1] = line;
      }
      else
      {
        try
        {
          DateTime dateTime = GetTimestamp(callback, line);
          if (dateTime == DateTime.MinValue)
          {
            cols = new string[COLUMN_COUNT] { "", line };
          }
          string newDate = dateTime.ToString(DATETIME_FORMAT_OUT);
          cols[0] = newDate;
        }
        catch (Exception)
        {
          cols[0] = "n/a";
        }

        string timestmp = cols[0];
        cols = GetColsFromLine(line);
        if (cols.Length != COLUMN_COUNT)
        {
          cols = new string[COLUMN_COUNT]{"", line};
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

    public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
    {
      // delete '[#|' and '|#]'
      if (line.StartsWith("[#|"))
        line = line.Substring(3);
      if (line.EndsWith("|#]"))
        line = line.Substring(0, line.Length - 3);

      if (line.Length < 28)
      {
        return DateTime.MinValue;
      }

      int endIndex = line.IndexOf(separatorChar, 1);
      if (endIndex > 28 || endIndex < 0)
      {
        return DateTime.MinValue;
      }
      string value = line.Substring(0, endIndex);

      try
      {
        // convert glassfish timestamp into a readable format:
        DateTime timestamp;
        if (DateTime.TryParseExact(value, DATETIME_FORMAT, cultureInfo, System.Globalization.DateTimeStyles.None, out timestamp))
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
          this.timeOffset = (int)(mSecsNew - mSecsOld);
        }
        catch (FormatException)
        { }
      }
    }

    #endregion


    public string Text
    {
      get { return GetName(); }
    }

  }
}
