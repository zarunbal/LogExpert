using NLog;

namespace LogExpert
{
    /// <summary>
    ///     Delivers the range (from..to) that matches the current range filter settings starting from a given line.
    /// </summary>
    internal class RangeFinder
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        private readonly LogWindow.ColumnizerCallback callback;
        private readonly FilterParams filterParams;

        #endregion

        #region Ctor

        public RangeFinder(FilterParams filterParams, LogWindow.ColumnizerCallback callback)
        {
            this.filterParams = filterParams.CreateCopy2();
            this.callback = callback;
        }

        #endregion

        #region Public Methods

        public Range FindRange(int startLine)
        {
            _logger.Info("Starting range search for {0} ... {1}", filterParams.searchText, filterParams.rangeSearchText);
            if (filterParams.rangeSearchText == null || filterParams.rangeSearchText.Trim().Length == 0)
            {
                _logger.Info("Range search text not set. Cancelling range search.");
                return null;
            }

            if (filterParams.searchText == null || filterParams.searchText.Trim().Length == 0)
            {
                _logger.Info("Search text not set. Cancelling range search.");
                return null;
            }

            filterParams.isRangeSearch = false;
            filterParams.isInRange = false;
            ILogLine line;
            int lineCount = callback.GetLineCount();
            int lineNum = startLine;
            bool foundStartLine = false;
            Range range = new Range();
            FilterParams tmpParam = filterParams.CreateCopy2();
            tmpParam.searchText = filterParams.rangeSearchText;

            // search backward for starting keyword
            line = callback.GetLogLine(lineNum);
            while (lineNum >= 0)
            {
                callback.LineNum = lineNum;
                if (Util.TestFilterCondition(filterParams, line, callback))
                {
                    foundStartLine = true;
                    break;
                }

                lineNum--;
                line = callback.GetLogLine(lineNum);
                if (lineNum < 0 || Util.TestFilterCondition(tmpParam, line, callback)
                )
                {
// do not crash on Ctrl+R when there is not start line found
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
            filterParams.isRangeSearch = true;
            filterParams.isInRange = true;
            lineNum++;
            while (lineNum < lineCount)
            {
                line = callback.GetLogLine(lineNum);
                callback.LineNum = lineNum;
                if (!Util.TestFilterCondition(filterParams, line, callback))
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
