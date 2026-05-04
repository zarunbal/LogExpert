using ColumnizerLib;

using LogExpert.Core.Callback;
using LogExpert.Core.Classes.Log.Buffers;
using LogExpert.Core.Interfaces;

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
    private PinHandle? _pinHandle;

    #endregion

    #region Internals

    /// <summary>
    /// Prefetch a range of lines in a single batch call.
    /// Call this before a paint cycle with the visible row range.
    /// Pin-before-read: pins are acquired BEFORE reading lines to prevent
    /// the GC eviction thread from returning char blocks between read and pin.
    /// </summary>
    internal void Prefetch (ILogfileReader logFileReader, int startLine, int count)
    {
        if (startLine == _prefetchStartLine && count == _prefetchCount)
        {
            return; // already prefetched this exact range
        }

        //Pin BEFORE reading — this should prevent GC thread from evicting buffers
        //between GetLogLineMemories() and PinRange().
        PinHandle? newHandle = null;
        if (logFileReader is IBufferPinning pinning)
        {
            newHandle = pinning.PinRange(startLine, startLine + count - 1);
        }

        var newLines = logFileReader.GetLogLineMemories(startLine, count);

        //Atomic swap: old handle released AFTER new is in place
        var oldHandle = _pinHandle;
        _pinHandle = newHandle;
        _prefetchedLines = newLines;
        _prefetchStartLine = startLine;
        _prefetchCount = newLines.Length;

        // Now safe to release old pins — new pins cover the new visible range
        oldHandle?.Dispose();
    }

    /// <summary>
    /// Returns the prefetched line for the given line number if it falls within the
    /// currently pinned range. Returns null if not available.
    /// </summary>
    internal ILogLineMemory? GetPrefetchedLine (int lineNumber)
    {
        return _prefetchedLines != null
            && lineNumber >= _prefetchStartLine
            && lineNumber < _prefetchStartLine + _prefetchCount
                ? _prefetchedLines[lineNumber - _prefetchStartLine]
                : null;
    }

    /// <summary>
    /// Invalidates the prefetch cache. Call on scroll, data change, or columnizer change.
    /// </summary>
    internal void InvalidatePrefetch ()
    {
        _pinHandle?.Dispose();
        _pinHandle = null;

        _prefetchedLines = null;
        _prefetchStartLine = -1;
        _prefetchCount = 0;
        _lastLineNumber = -1;
    }

    /// <summary>
    /// Forces the next <see cref="Prefetch"/> call to re-fetch and re-pin, without releasing
    /// current pins. Use this instead of <see cref="InvalidatePrefetch"/> when the visible data
    /// may be stale but the underlying buffers must remain pinned until replacement pins are acquired.
    /// </summary>
    internal void MarkPrefetchStale ()
    {
        _prefetchStartLine = -1;
        _prefetchCount = 0;
        _lastLineNumber = -1;
        _cachedColumns = null;
    }

    internal IColumnizedLogLineMemory GetColumnsForLine (ILogfileReader logFileReader, int lineNumber, ILogLineMemoryColumnizer columnizer, ColumnizerCallback columnizerCallback)
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

            // Fallback: read directly. This is safe because the caller (CellValueNeeded)
            // has already called Prefetch/PrefetchFilterVisibleLines which pins the relevant
            // buffers. Pinned buffers won't have their char blocks returned to the pool.
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