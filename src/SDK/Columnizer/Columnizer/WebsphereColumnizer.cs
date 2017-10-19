using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Globalization;

namespace Columnizer
{
  /// <summary>
  /// The WebsphereColumnizer splits the log line into 2 fields (timestamp, message).
  /// It can parse timestamps of the format M/d/yy H:m:s:fff. This means there may be 1 or 2 digits
  /// for hour, minute, second, day or month.
  /// </summary>
  public class WebsphereColumnizer : ILogLineColumnizer
  {
    protected int timeOffset = 0;
    protected CultureInfo cultureInfo = new CultureInfo("en-US");
    protected const String DATETIME_FORMAT_OUT = "MM/dd/yy HH:mm:ss:fff";
    protected const String DATETIME_FORMAT_IN = "M/d/yy H:m:s:fff";

    #region ILogLineColumnizer Member

    public int GetColumnCount()
    {
      return 2;
    }

    public string[] GetColumnNames()
    {
      return new string[] {"Timestamp", "Log-Message" };
    }

    public string GetDescription()
    {
      return "IBM Websphere like timestamps";
    }

    public string GetName()
    {
      return "IBM Websphere";
    }

    public int GetTimeOffset()
    {
      return this.timeOffset;
    }

    /// <summary>
    /// This function has to return the timestamp of the given log line.
    /// It takes a substring of the line (between [ and ])
    /// and converts this into a DateTime object.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
    {
      
      if (line.Length < 24)
      {
        return DateTime.MinValue;
      }
      
      int end = line.IndexOf(" CET");
      if (end == -1)
        return DateTime.MinValue;

      String s = line.Substring(1, end - 1);

      // Parse into a DateTime
      
      DateTime dateTime;
      if (!DateTime.TryParseExact(s, DATETIME_FORMAT_IN, this.cultureInfo, DateTimeStyles.None, out dateTime))
        return DateTime.MinValue;

      // Add the time offset before returning
      return dateTime.AddMilliseconds(this.timeOffset);
    }

    public bool IsTimeshiftImplemented()
    {
      return true;
    }


    /// <summary>
    /// This function is called if the user changes a value in a column (edit mode in the log view).
    /// The purpose if the function is to determine a new timestamp offset. So you have to handle the
    /// call only if the given column displays a timestamp.
    /// </summary>
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

    public void SetTimeOffset(int msecOffset)
    {
      this.timeOffset = msecOffset;
    }


    /// <summary>
    /// Given a single line of the logfile this function splits the line content into columns. The function returns 
    /// a string array containing the splitted content.
    /// </summary>
    /// <remarks>
    /// This function is called by LogExpert for every line that has to be drawn in the grid view. The faster your code
    /// handles the splitting, the faster LogExpert can draw the grid view content.
    /// </remarks>
    /// <param name="callback">Callback interface with functions which can be used by the columnizer</param>
    /// <param name="line">The line content to be splitted</param>
    public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
    {
      string[] cols = new string[2] { "", ""};

      // If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
      // in colum 1 (the log message column). Timestamp column will be left blank.
      if (line.Length < 23)
      {
        cols[1] = line;
        return cols;
      }

      try
      {
        DateTime dateTime = GetTimestamp(callback, line);
        string newDate = dateTime.ToString(DATETIME_FORMAT_OUT, this.cultureInfo);
        cols[0] = newDate;
      }
      catch (Exception)
      {
        cols[0] = "n/a";
      }
      int end = line.IndexOf(']');
      if (end != -1)
      {
        cols[1] = line.Substring(end + 1);    // rest of line
      }
      else
      {
        cols[1] = line;     // fail behaviour
      }
      return cols;
    }

    #endregion

    /// <summary>
    /// Implement this property to let LogExpert display the name of the Columnizer
    /// in its Colummnizer selection dialog.
    /// </summary>
    public string Text
    {
      get { return GetName(); }
    }

  }
}
