using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Globalization;

namespace Columnizer
{
  /// <summary>
  /// This is an example implementation of the ILogLineColumnizer interface.
  /// The ExampleColumnizer splits the log line into 3 fields (date, time, message).
  /// It can parse timestamps on the format dd.MM.yy HH:mm:ss,fff.
  /// </summary>
  /// <remarks>
  /// The callback functions are not used in this example.
  /// </remarks>
  public class ExampleColumnizer : ILogLineColumnizer
  {
    protected int timeOffset = 0;
    protected CultureInfo cultureInfo = new CultureInfo("de-DE");
    protected const String DATETIME_FORMAT = "dd.MM.yy HH:mm:ss,fff";

    #region ILogLineColumnizer Member

    public int GetColumnCount()
    {
      return 3;
    }

    public string[] GetColumnNames()
    {
      return new string[] {"Date", "Time", "Log-Message" };
    }

    public string GetDescription()
    {
      return "This is a sample columnizer that splits a line into 3 columns." +
        "Timestamp parsing is supported for the format 'dd.MM.yy HH:mm:ss,fff'.";
    }

    public string GetName()
    {
      return "ExampleColumnizer";
    }

    public int GetTimeOffset()
    {
      return this.timeOffset;
    }

    /// <summary>
    /// This function has to return the timestamp of the given log line.
    /// It takes a substring of the line (first 21 chars containing the date and the time) and converts this into a DateTime object.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
    {
      
      if (line.Length < 21)
      {
        return DateTime.MinValue;
      }
      
      // A first check if this could be a valid date/time string. This is for performance reasons, because 
      // DateTime.ParseExact() is slow when receiving an invalid input.
      if (line[2] != '.' || line[5] != '.' || line[11] != ':' || line[14] != ':')
      {
        return DateTime.MinValue;
      }

      try
      {
        // Parse into a DateTime
        DateTime dateTime = DateTime.ParseExact(line.Substring(0, 21), DATETIME_FORMAT, this.cultureInfo);

        // Add the time offset before returning
        return dateTime.AddMilliseconds(this.timeOffset);
      }
      catch (Exception)
      {
        return DateTime.MinValue;
      }
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
      if (column == 1)
      {
        try
        {
          DateTime newDateTime = DateTime.ParseExact(value, DATETIME_FORMAT, this.cultureInfo);
          DateTime oldDateTime = DateTime.ParseExact(oldValue, DATETIME_FORMAT, this.cultureInfo);
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
      // 0         1         2         3         4         5         6         7         8         9         10        11 
      // 0123456789012345678901234567890
      // 16.05.08 15:07:15,328 this is a log message line

      string[] cols = new string[3] { "", "", "" };

      // If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
      // in colum 2 (the log message column). Date and time column will be left blank.
      if (line.Length < 23)
      {
        cols[2] = line;
        return cols;
      }

      // A first check if this could be a valid date/time string. This is for performance reasons, because 
      // DateTime.ParseExact() is slow when receiving an invalid input.
      // If there's no valid date/time, the content will be returned in column 2. Date and time column will be left blank.
      if (line[2] != '.' || line[5] != '.' || line[11] != ':' || line[14] != ':')
      {
        cols[2] = line;
        return cols;
      }

      // If the time offset is not 0 we have to do some more work:
      // - parse the date/time part of the log line
      // - add the time offset
      // - convert back to string
      if (this.timeOffset != 0)
      {
        try
        {
          DateTime dateTime = DateTime.ParseExact(line.Substring(0, 21), DATETIME_FORMAT, this.cultureInfo);
          dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, this.timeOffset));
          string newDate = dateTime.ToString(DATETIME_FORMAT);
          cols[0] = newDate.Substring(0, 8);    // date
          cols[1] = newDate.Substring(09, 12);   // time
        }
        catch (Exception)
        {
          cols[0] = "n/a";
          cols[1] = "n/a";
        }
      }
      else
      {
        cols[0] = line.Substring(0, 8);    // date
        cols[1] = line.Substring(9, 12);   // time
      }
      cols[2] = line.Substring(22);    // rest of line
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
