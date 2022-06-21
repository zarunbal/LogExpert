using LogExpert.Classes.Filter;
using LogExpert.Classes.ILogLineColumnizerCallback;
using LogExpert.Entities;
using NLog;

namespace LogExpert.Classes
{
    /// <summary>
    /// Delivers the range (from..to) that matches the current range filter settings starting from a given line.
    /// </summary>
    internal class RangeFinder
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly ColumnizerCallback _callback;
        private readonly FilterParams _filterParams;

        #endregion

        #region cTor

        public RangeFinder(FilterParams filterParams, ColumnizerCallback callback)
        {
            _filterParams = filterParams.CreateCopy2();
            _callback = callback;
        }

        #endregion

        #region Public methods

        public Range FindRange(int startLine)
        {
            _logger.Info($"Starting range search for {_filterParams.searchText} ... {_filterParams.rangeSearchText}");
            if (_filterParams.rangeSearchText == null || _filterParams.rangeSearchText.Trim().Length == 0)
            {
                _logger.Info("Range search text not set. Cancelling range search.");
                return null;
            }
            if (_filterParams.searchText == null || _filterParams.searchText.Trim().Length == 0)
            {
                _logger.Info("Search text not set. Cancelling range search.");
                return null;
            }

            _filterParams.isRangeSearch = false;
            _filterParams.isInRange = false;
            int lineCount = _callback.GetLineCount();
            int lineNum = startLine;
            bool foundStartLine = false;
            Range range = new Range();
            FilterParams tmpParam = _filterParams.CreateCopy2();
            tmpParam.searchText = _filterParams.rangeSearchText;

            // search backward for starting keyword
            var line = _callback.GetLogLine(lineNum);
            while (lineNum >= 0)
            {
                _callback.LineNum = lineNum;
                if (Util.TestFilterCondition(_filterParams, line, _callback))
                {
                    foundStartLine = true;
                    break;
                }
                lineNum--;
                line = _callback.GetLogLine(lineNum);
                if (lineNum < 0 || Util.TestFilterCondition(tmpParam, line, _callback)
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
            _filterParams.isRangeSearch = true;
            _filterParams.isInRange = true;
            lineNum++;
            while (lineNum < lineCount)
            {
                line = _callback.GetLogLine(lineNum);
                _callback.LineNum = lineNum;
                if (!Util.TestFilterCondition(_filterParams, line, _callback))
                {
                    break;
                }
                lineNum++;
            }
            lineNum--;
            range.EndLine = lineNum;

            _logger.Info($"Range search finished. Found {range.EndLine - range.StartLine} lines");

            return range;
        }

        #endregion
    }
}