using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	/// <summary>
	/// Delivers the range (from..to) that matches the current range filter settings starting from a given line.
	/// </summary>
	class RangeFinder
	{
		FilterParams _filterParams;
		private LogExpert.ColumnizerCallback _callback;

		public RangeFinder(FilterParams filterParams, LogExpert.ColumnizerCallback callback)
		{
			_filterParams = filterParams.CreateCopy2();
			_callback = callback;
		}

		public Range FindRange(int startLine)
		{
			Logger.logInfo("Starting range search for " + _filterParams.searchText + " ... " + _filterParams.rangeSearchText);
			if (_filterParams.rangeSearchText == null || _filterParams.rangeSearchText.Trim().Length == 0)
			{
				Logger.logInfo("Range search text not set. Cancelling range search.");
				return null;
			}
			if (_filterParams.searchText == null || _filterParams.searchText.Trim().Length == 0)
			{
				Logger.logInfo("Search text not set. Cancelling range search.");
				return null;
			}

			_filterParams.isRangeSearch = false;
			_filterParams.isInRange = false;
			string line;
			int lineCount = _callback.GetLineCount();
			int lineNum = startLine;
			bool foundStartLine = false;
			Range range = new Range();
			FilterParams tmpParam = _filterParams.CreateCopy2();
			tmpParam.searchText = _filterParams.rangeSearchText;

			// search backward for starting keyword
			line = _callback.GetLogLine(lineNum);
			while (lineNum >= 0)
			{
				_callback.LineNum = lineNum;
				if (Classes.DamerauLevenshtein.TestFilterCondition(_filterParams, line, _callback))
				{
					foundStartLine = true;
					break;
				}
				lineNum--;
				line = _callback.GetLogLine(lineNum);
				if (lineNum < 0 || Classes.DamerauLevenshtein.TestFilterCondition(tmpParam, line, _callback)) // do not crash on Ctrl+R when there is not start line found
				{
					// lower range bound found --> we are not in between a valid range
					break;
				}
			}
			if (!foundStartLine)
			{
				Logger.logInfo("Range start not found");
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
				if (!Classes.DamerauLevenshtein.TestFilterCondition(_filterParams, line, _callback))
				{
					break;
				}
				lineNum++;
			}
			lineNum--;
			range.EndLine = lineNum;
			#if DEBUG
			Logger.logInfo("Range search finished. Found " + (range.EndLine - range.StartLine) + " lines");
			#endif
			return range;
		}
	}
}