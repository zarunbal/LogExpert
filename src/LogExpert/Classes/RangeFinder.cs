using System;
using System.Collections.Generic;
using System.Text;
using NLog;


namespace LogExpert
{
    /// <summary>
    /// Delivers the range (from..to) that matches the current range filter settings starting from a given line.
    /// </summary>
    internal class RangeFinder
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly LogExpert.LogWindow.ColumnizerCallback callback;
        private readonly FilterParams filterParams;

        #endregion

        #region cTor

        public RangeFinder(FilterParams filterParams, LogExpert.LogWindow.ColumnizerCallback callback)
        {
            this.filterParams = filterParams.CreateCopy2();
            this.callback = callback;
        }

        #endregion

        #region Public methods

        public Range FindRange(int startLine)
        {
            _logger.Info("Starting range search for {0} ... {1}", this.filterParams.searchText, this.filterParams.rangeSearchText);
            if (this.filterParams.rangeSearchText == null || this.filterParams.rangeSearchText.Trim().Length == 0)
            {
                _logger.Info("Range search text not set. Cancelling range search.");
                return null;
            }
            if (this.filterParams.searchText == null || this.filterParams.searchText.Trim().Length == 0)
            {
                _logger.Info("Search text not set. Cancelling range search.");
                return null;
            }

            this.filterParams.isRangeSearch = false;
            this.filterParams.isInRange = false;
            ILogLine line;
            int lineCount = this.callback.GetLineCount();
            int lineNum = startLine;
            bool foundStartLine = false;
            Range range = new Range();
            FilterParams tmpParam = this.filterParams.CreateCopy2();
            tmpParam.searchText = this.filterParams.rangeSearchText;

            // search backward for starting keyword
            line = this.callback.GetLogLine(lineNum);
            while (lineNum >= 0)
            {
                this.callback.LineNum = lineNum;
                if (Util.TestFilterCondition(this.filterParams, line, this.callback))
                {
                    foundStartLine = true;
                    break;
                }
                lineNum--;
                line = this.callback.GetLogLine(lineNum);
                if (lineNum < 0 || Util.TestFilterCondition(tmpParam, line, this.callback)
                ) // do not crash on Ctrl+R when there is not start line found
                {
                    // lower range bound found --> we are not in between a valid range
                    break;
                }
            }
            if (!foundStartLine)
            {
                _logger.Info("Range start not found");
                return null;
            }
            range.StartLine = lineNum;
            this.filterParams.isRangeSearch = true;
            this.filterParams.isInRange = true;
            lineNum++;
            while (lineNum < lineCount)
            {
                line = this.callback.GetLogLine(lineNum);
                this.callback.LineNum = lineNum;
                if (!Util.TestFilterCondition(this.filterParams, line, this.callback))
                {
                    break;
                }
                lineNum++;
            }
            lineNum--;
            range.EndLine = lineNum;

            _logger.Info("Range search finished. Found {0} lines", range.EndLine - range.StartLine);

            return range;
        }

        #endregion
    }
}