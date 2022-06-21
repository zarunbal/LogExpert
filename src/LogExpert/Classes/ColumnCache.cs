using LogExpert.Classes.ILogLineColumnizerCallback;
using LogExpert.Classes.Log;

namespace LogExpert.Classes
{
    internal class ColumnCache
    {
        #region Fields

        private IColumnizedLogLine _cachedColumns;
        private ILogLineColumnizer _lastColumnizer;
        private int _lastLineNumber = -1;

        #endregion

        #region Internals

        internal IColumnizedLogLine GetColumnsForLine(LogfileReader logFileReader, int lineNumber, ILogLineColumnizer columnizer, ColumnizerCallback columnizerCallback)
        {
            if (_lastColumnizer != columnizer || _lastLineNumber != lineNumber && _cachedColumns != null || columnizerCallback.LineNum != lineNumber)
            {
                _lastColumnizer = columnizer;
                _lastLineNumber = lineNumber;
                ILogLine line = logFileReader.GetLogLineWithWait(lineNumber);
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

        #endregion
    }
}