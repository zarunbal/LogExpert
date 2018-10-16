using System;
using System.Globalization;
using System.Text;
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
    ///     This class is responsible for building file names for multifile.
    /// </summary>
    public class RolloverFilenameBuilder
    {
        #region Private Fields

        private string condContent;
        private Group condGroup;
        private string currentFileName;

        private Group dateGroup;

        // private Regex regexCond;
        private DateTime dateTime;

        // private DateTimeFormatInfo dateFormat;
        private string dateTimeFormat;

        private bool hideZeroIndex;
        private Group indexGroup;
        private Regex regex;

        #endregion

        #region Ctor

        public RolloverFilenameBuilder(string formatString)
        {
            ParseFormatString(formatString);
        }

        #endregion

        #region Properties / Indexers

        public int Index { get; set; }

        public bool IsDatePattern => dateGroup != null && dateGroup.Success;

        public bool IsIndexPattern => indexGroup != null && indexGroup.Success;

        #endregion

        #region Public Methods

        public string BuildFileName()
        {
            string fileName = currentFileName;
            if (dateGroup != null && dateGroup.Success)
            {
                string newDate = dateTime.ToString(dateTimeFormat, DateTimeFormatInfo.InvariantInfo);
                fileName = fileName.Remove(dateGroup.Index, dateGroup.Length);
                fileName = fileName.Insert(dateGroup.Index, newDate);
            }

            if (indexGroup != null && indexGroup.Success)
            {
                fileName = fileName.Remove(indexGroup.Index, indexGroup.Length);
                string fileNameBak = fileName;
                if (!hideZeroIndex || Index > 0)
                {
                    string format = "D" + indexGroup.Length;
                    fileName = fileName.Insert(indexGroup.Index, Index.ToString(format));
                    if (hideZeroIndex && condContent != null)
                    {
                        fileName = fileName.Insert(indexGroup.Index, condContent);
                    }
                }
            }


// this.currentFileName = fileName;

// SetFileName(fileName);
            return fileName;
        }

        public void DecrementDate()
        {
            dateTime = dateTime.AddDays(-1);
        }

        public void IncrementDate()
        {
            dateTime = dateTime.AddDays(1);
        }

        public void SetFileName(string fileName)
        {
            currentFileName = fileName;
            Match match = regex.Match(fileName);
            if (match.Success)
            {
                dateGroup = match.Groups["date"];
                if (dateGroup.Success)
                {
                    string date = fileName.Substring(dateGroup.Index, dateGroup.Length);
                    if (DateTime.TryParseExact(date, dateTimeFormat, DateTimeFormatInfo.InvariantInfo,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                    }
                }

                indexGroup = match.Groups["index"];
                if (indexGroup.Success)
                {
                    Index = indexGroup.Value.Length > 0 ? int.Parse(indexGroup.Value) : 0;
                }

                condGroup = match.Groups["cond"];
            }
        }

        #endregion

        #region Private Methods

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
                    case 0: // looking for $
                        if (fmt[i] == '$')
                        {
                            result.Append(Regex.Escape(segment.ToString()));
                            segment = new StringBuilder();
                            state = 1;
                        }

                        segment.Append(fmt[i]);
                        break;
                    case 1: // the char behind $
                        segment.Append(fmt[i]);
                        result.Append(segment);
                        segment = new StringBuilder();
                        state = 2;
                        break;
                    case 2: // checking if ( or other char
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
                    case 3: // looking for )
                        segment.Append(fmt[i]);
                        if (fmt[i] == ')')
                        {
                            result.Append(segment);
                            segment = new StringBuilder();
                            state = 0;
                        }

                        break;
                }
            }

            fmt = result.ToString().Replace('\xFFFD', '*');
            return fmt;
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
                    dateTimeFormat = formatString.Substring(datePos + 3, endPos - datePos - 3);
                    dateTimeFormat = dateTimeFormat.ToUpper();
                    dateTimeFormat = dateTimeFormat.Replace('D', 'd').Replace('Y', 'y');

                    string dtf = dateTimeFormat;
                    dtf = dtf.ToUpper();
                    dtf = dtf.Replace("D", "\\d");
                    dtf = dtf.Replace("Y", "\\d");
                    dtf = dtf.Replace("M", "\\d");
                    fmt = fmt.Remove(datePos, 2); // remove $D
                    fmt = fmt.Remove(datePos + 1, dateTimeFormat.Length); // replace with regex version of format
                    fmt = fmt.Insert(datePos + 1, dtf);
                    fmt = fmt.Insert(datePos + 1, "?'date'"); // name the regex group 
                }
            }

            int condPos = fmt.IndexOf("$J(");
            if (condPos != -1)
            {
                int endPos = fmt.IndexOf(')', condPos);
                if (endPos != -1)
                {
                    condContent = fmt.Substring(condPos + 3, endPos - condPos - 3);
                    fmt = fmt.Remove(condPos + 2, endPos - condPos - 1);
                }
            }

            fmt = fmt.Replace("*", ".*");
            hideZeroIndex = fmt.Contains("$J");
            fmt = fmt.Replace("$I", "(?'index'[\\d]+)");
            fmt = fmt.Replace("$J", "(?'index'[\\d]*)");

            regex = new Regex(fmt);
        }

        #endregion
    }
}
