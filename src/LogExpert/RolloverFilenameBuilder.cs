using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LogExpert
{
  /* Needed info:
   * - Date/time mask
   * - index counters 
   * - counter direction (up/down)
   * - counter limit
   * - whether the files are shifted or not
   * - whether the indexes start with zero (or n/a) on a new date period
   * 
   * Format:
   * *$D(yyyy-MM-dd)$I
   * *$J(.)
   * 
   * *(yyyy-MM-dd)[I]
   * 
   */

  /// <summary>
  /// This class is responsible for building file names for multifile.
  /// </summary>
  public class RolloverFilenameBuilder
  {
    //private DateTimeFormatInfo dateFormat;
    private String dateTimeFormat = null;
    private Regex regex;
    //private Regex regexCond;
    private DateTime dateTime;
    private int index;
    private string currentFileName;
    private Group indexGroup;
    private Group dateGroup;
    private Group condGroup;
    private bool hideZeroIndex;
    private string condContent;


    public RolloverFilenameBuilder(string formatString)
    {
      ParseFormatString(formatString);
    }

    public int Index
    {
      get { return index; }
      set { index = value; }
    }

    private void ParseFormatString(string formatString)
    {
      string fmt = escapeNonvarRegions(formatString);
      int datePos = formatString.IndexOf("$D(");
      if (datePos != -1)
      {
        int endPos = formatString.IndexOf(')', datePos);
        if (endPos != -1)
        {
          this.dateTimeFormat = formatString.Substring(datePos + 3, endPos - datePos - 3);
          this.dateTimeFormat = this.dateTimeFormat.ToUpper();
          this.dateTimeFormat = this.dateTimeFormat.Replace('D', 'd').Replace('Y', 'y');

          string dtf = this.dateTimeFormat;
          dtf = dtf.ToUpper();
          dtf = dtf.Replace("D", "\\d");
          dtf = dtf.Replace("Y", "\\d");
          dtf = dtf.Replace("M", "\\d");
          fmt = fmt.Remove(datePos, 2);   // remove $D
          fmt = fmt.Remove(datePos + 1, this.dateTimeFormat.Length); // replace with regex version of format
          fmt = fmt.Insert(datePos + 1, dtf);
          fmt = fmt.Insert(datePos + 1, "?'date'");  // name the regex group 
        }
      }

      int condPos = fmt.IndexOf("$J(");
      if (condPos != -1)
      {
        int endPos = fmt.IndexOf(')', condPos);
        if (endPos != -1)
        {
          this.condContent = fmt.Substring(condPos + 3, endPos - condPos - 3);
          fmt = fmt.Remove(condPos + 2, endPos - condPos - 1);
        }
      }

      fmt = fmt.Replace("*", ".*");
      this.hideZeroIndex = fmt.Contains("$J");
      fmt = fmt.Replace("$I", "(?'index'[\\d]+)");
      fmt = fmt.Replace("$J", "(?'index'[\\d]*)");

      this.regex = new Regex(fmt);
    }

    private string escapeNonvarRegions(string formatString)
    {
      string fmt = formatString.Replace('*', '\xFFFD');
      StringBuilder result = new StringBuilder();
      int state = 0;
      StringBuilder segment = new StringBuilder();
      for (int i = 0; i < fmt.Length; ++i)
      {
        switch (state)
        {
          case 0:   // looking for $
            if (fmt[i] == '$')
            {
              result.Append(Regex.Escape(segment.ToString()));
              segment = new StringBuilder();
              state = 1;
            }
            segment.Append(fmt[i]);
            break;
          case 1:   // the char behind $
            segment.Append(fmt[i]);
            result.Append(segment.ToString());
            segment = new StringBuilder();
            state = 2;
            break;
          case 2:   // checking if ( or other char
            if (fmt[i] == '(')
            {
              segment.Append(fmt[i]);
              state = 3;
            }
            else
            {
              segment.Append(fmt[i]);
              state = 0;
            }
            break;
          case 3:   // looking for )
            segment.Append(fmt[i]);
            if (fmt[i] == ')')
            {
              result.Append(segment.ToString());
              segment = new StringBuilder();
              state = 0;
            }
            break;
        }
      }
      fmt = result.ToString().Replace('\xFFFD', '*');
      return fmt;
    }


    public void SetFileName(string fileName)
    {
      this.currentFileName = fileName;
      Match match = this.regex.Match(fileName);
      if (match.Success)
      {
        this.dateGroup = match.Groups["date"];
        if (this.dateGroup.Success)
        {
          string date = fileName.Substring(dateGroup.Index, dateGroup.Length);
          if (DateTime.TryParseExact(date, this.dateTimeFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None,
                                     out this.dateTime))
          {
          }
        }
        this.indexGroup = match.Groups["index"];
        if (this.indexGroup.Success)
        {
          this.Index = this.indexGroup.Value.Length > 0 ? Int32.Parse(indexGroup.Value) : 0;
        }
        this.condGroup = match.Groups["cond"];
      }
    }

    public void IncrementDate()
    {
      this.dateTime = dateTime.AddDays(1);
    }

    public void DecrementDate()
    {
      this.dateTime = dateTime.AddDays(-1);
    }


    public string BuildFileName()
    {
      string fileName = this.currentFileName;
      if (this.dateGroup != null && this.dateGroup.Success)
      {
        string newDate = dateTime.ToString(this.dateTimeFormat, DateTimeFormatInfo.InvariantInfo);
        fileName = fileName.Remove(this.dateGroup.Index, this.dateGroup.Length);
        fileName = fileName.Insert(this.dateGroup.Index, newDate);
      }
      if (this.indexGroup != null && this.indexGroup.Success)
      {
        fileName = fileName.Remove(this.indexGroup.Index, this.indexGroup.Length);
        string fileNameBak = fileName;
        if (!this.hideZeroIndex || this.Index > 0)
        {
          string format = "D" + this.indexGroup.Length;
          fileName = fileName.Insert(this.indexGroup.Index, this.index.ToString(format));
          if (this.hideZeroIndex && this.condContent != null)
          {
            fileName = fileName.Insert(this.indexGroup.Index, this.condContent);
          }
        }
      }
//      this.currentFileName = fileName;
//      SetFileName(fileName);
      return fileName;
    }

    public bool IsDatePattern
    {
      get { return this.dateGroup != null && this.dateGroup.Success; }
    }

    public bool IsIndexPattern
    {
      get { return this.indexGroup != null && this.indexGroup.Success; }
    }



  }
}
