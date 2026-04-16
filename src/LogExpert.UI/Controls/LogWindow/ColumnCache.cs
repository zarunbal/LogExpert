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

    // Prefetch state
    private ILogLineMemory[] _prefetchedLines;
    private int _prefetchStartLine = -1;
    private int _prefetchCount;

    #endregion

    #region Internals

    /// <summary>
    /// Prefetch a range of lines in a single batch call.
    /// Call this before a paint cycle with the visible row range.
    /// </summary>
    internal void Prefetch (LogfileReader logFileReader, int startLine, int count)
    {
        if (startLine == _prefetchStartLine && count == _prefetchCount)
        {
            return; // already prefetched this exact range
        }

        _prefetchedLines = logFileReader.GetLogLineMemories(startLine, count);
        _prefetchStartLine = startLine;
        _prefetchCount = _prefetchedLines.Length;
    }

    /// <summary>
    /// Invalidates the prefetch cache. Call on scroll, data change, or columnizer change.
    /// </summary>
    internal void InvalidatePrefetch ()
    {
        _prefetchedLines = null;
        _prefetchStartLine = -1;
        _prefetchCount = 0;
        _lastLineNumber = -1;
    }

    internal IColumnizedLogLineMemory GetColumnsForLine (LogfileReader logFileReader, int lineNumber, ILogLineMemoryColumnizer columnizer, ColumnizerCallback columnizerCallback)
    {
        if (_lastColumnizer != columnizer || (_lastLineNumber != lineNumber && _cachedColumns != null) || columnizerCallback.LineNum != lineNumber)
        {
            _lastColumnizer = columnizer;
            _lastLineNumber = lineNumber;

            ILogLineMemory line = null;

            if (_prefetchedLines != null
                && lineNumber >= _prefetchStartLine
                && lineNumber < _prefetchStartLine + _prefetchCount)
            {
                line = _prefetchedLines[lineNumber - _prefetchStartLine];
            }

            line ??= logFileReader.GetLogLineMemoryWithWait(lineNumber).Result;

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