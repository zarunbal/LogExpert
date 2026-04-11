using ColumnizerLib;

using LogExpert.Core.Callback;
using LogExpert.Core.Classes.Log;

namespace LogExpert.UI.Controls.LogWindow;

internal class ColumnCache
{
    #region Fields

    private IColumnizedLogLineMemory _cachedColumns;
    private ILogLineMemoryColumnizer _lastColumnizer;
    private int _lastLineNumber = -1;

    #endregion

    #region Internals

    internal IColumnizedLogLineMemory GetColumnsForLine (LogfileReader logFileReader, int lineNumber, ILogLineMemoryColumnizer columnizer, ColumnizerCallback columnizerCallback)
    {
        if (_lastColumnizer != columnizer || (_lastLineNumber != lineNumber && _cachedColumns != null) || columnizerCallback.LineNum != lineNumber)
        {
            _lastColumnizer = columnizer;
            _lastLineNumber = lineNumber;
            var line = logFileReader.GetLogLineMemoryWithWait(lineNumber).Result;

            if (line != null)
            {
                columnizerCallback.SetLineNum(lineNumber);
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