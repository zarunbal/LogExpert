using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	internal class ColumnCache
	{
		private string[] _cachedColumns = null;
		private ILogLineColumnizer _lastColumnizer;
		private int _lastLineNumber = -1;

		internal ColumnCache()
		{
		}

		internal string[] GetColumnsForLine(LogfileReader logFileReader, int lineNumber, ILogLineColumnizer columnizer, LogExpert.LogWindow.ColumnizerCallback columnizerCallback)
		{
			if (_lastColumnizer != columnizer || _lastLineNumber != lineNumber || _cachedColumns == null || columnizerCallback.LineNum != lineNumber)
			{
				_lastColumnizer = columnizer;
				_lastLineNumber = lineNumber;
				string line = logFileReader.GetLogLineWithWait(lineNumber);
				if (line != null)
				{
					columnizerCallback.LineNum = lineNumber;
					_cachedColumns = columnizer.SplitLine(columnizerCallback, line);
				}
				else
				{
					_cachedColumns = null;
				}
			}

			return _cachedColumns;
		}
	}
}