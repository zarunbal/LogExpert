using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LogExpert.Classes.Log
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
        #region Fields

        private string _condContent;
        private Group _condGroup;
        private string _currentFileName;

        private Group _dateGroup;

        //private Regex regexCond;
        private DateTime _dateTime;

        //private DateTimeFormatInfo dateFormat;
        private string _dateTimeFormat;

        private bool _hideZeroIndex;
        private Group _indexGroup;
        private Regex _regex;

        #endregion

        #region cTor

        public RolloverFilenameBuilder(string formatString)
        {
            ParseFormatString(formatString);
        }

        #endregion

        #region Properties

        public int Index { get; set; }

        public bool IsDatePattern => _dateGroup != null && _dateGroup.Success;

        public bool IsIndexPattern => _indexGroup != null && _indexGroup.Success;

        #endregion

        #region Public methods

        public void SetFileName(string fileName)
        {
            _currentFileName = fileName;
            Match match = _regex.Match(fileName);
            if (match.Success)
            {
                _dateGroup = match.Groups["date"];
                if (_dateGroup.Success)
                {
                    string date = fileName.Substring(_dateGroup.Index, _dateGroup.Length);
                    if (DateTime.TryParseExact(date, _dateTimeFormat, DateTimeFormatInfo.InvariantInfo,
                        DateTimeStyles.None,
                        out _dateTime))
                    {
                    }
                }
                _indexGroup = match.Groups["index"];
                if (_indexGroup.Success)
                {
                    Index = _indexGroup.Value.Length > 0 ? int.Parse(_indexGroup.Value) : 0;
                }
                _condGroup = match.Groups["cond"];
            }
        }

        public void IncrementDate()
        {
            _dateTime = _dateTime.AddDays(1);
        }

        public void DecrementDate()
        {
            _dateTime = _dateTime.AddDays(-1);
        }


        public string BuildFileName()
        {
            string fileName = _currentFileName;
            if (_dateGroup != null && _dateGroup.Success)
            {
                string newDate = _dateTime.ToString(_dateTimeFormat, DateTimeFormatInfo.InvariantInfo);
                fileName = fileName.Remove(_dateGroup.Index, _dateGroup.Length);
                fileName = fileName.Insert(_dateGroup.Index, newDate);
            }
            if (_indexGroup != null && _indexGroup.Success)
            {
                fileName = fileName.Remove(_indexGroup.Index, _indexGroup.Length);
                string fileNameBak = fileName;
                if (!_hideZeroIndex || Index > 0)
                {
                    string format = "D" + _indexGroup.Length;
                    fileName = fileName.Insert(_indexGroup.Index, Index.ToString(format));
                    if (_hideZeroIndex && _condContent != null)
                    {
                        fileName = fileName.Insert(_indexGroup.Index, _condContent);
                    }
                }
            }
//      this.currentFileName = fileName;
//      SetFileName(fileName);
            return fileName;
        }

        #endregion

        #region Private Methods

        private void ParseFormatString(string formatString)
        {
            string fmt = EscapeNonvarRegions(formatString);
            int datePos = formatString.IndexOf("$D(");
            if (datePos != -1)
            {
                int endPos = formatString.IndexOf(')', datePos);
                if (endPos != -1)
                {
                    _dateTimeFormat = formatString.Substring(datePos + 3, endPos - datePos - 3);
                    _dateTimeFormat = _dateTimeFormat.ToUpper();
                    _dateTimeFormat = _dateTimeFormat.Replace('D', 'd').Replace('Y', 'y');

                    string dtf = _dateTimeFormat;
                    dtf = dtf.ToUpper();
                    dtf = dtf.Replace("D", "\\d");
                    dtf = dtf.Replace("Y", "\\d");
                    dtf = dtf.Replace("M", "\\d");
                    fmt = fmt.Remove(datePos, 2); // remove $D
                    fmt = fmt.Remove(datePos + 1, _dateTimeFormat.Length); // replace with regex version of format
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
                    _condContent = fmt.Substring(condPos + 3, endPos - condPos - 3);
                    fmt = fmt.Remove(condPos + 2, endPos - condPos - 1);
                }
            }

            fmt = fmt.Replace("*", ".*");
            _hideZeroIndex = fmt.Contains("$J");
            fmt = fmt.Replace("$I", "(?'index'[\\d]+)");
            fmt = fmt.Replace("$J", "(?'index'[\\d]*)");

            _regex = new Regex(fmt);
        }

        private string EscapeNonvarRegions(string formatString)
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
                        result.Append(segment.ToString());
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

        #endregion
    }
}