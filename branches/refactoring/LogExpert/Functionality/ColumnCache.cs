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
			if (this._lastColumnizer != columnizer || this._lastLineNumber != lineNumber && this._cachedColumns != null)
			{
				this._lastColumnizer = columnizer;
				this._lastLineNumber = lineNumber;
				string line = logFileReader.GetLogLineWithWait(lineNumber);
				if (line != null)
				{
					columnizerCallback.LineNum = lineNumber;
					this._cachedColumns = columnizer.SplitLine(columnizerCallback, line);
				}
				else
				{
					this._cachedColumns = null;
				}
			}
			return this._cachedColumns;
		}
	}
}