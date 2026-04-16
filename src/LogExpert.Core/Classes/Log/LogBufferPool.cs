using System.Collections.Concurrent;

using ColumnizerLib;

namespace LogExpert.Core.Classes.Log;

public sealed class LogBufferPool (int maxSize)
{
    private readonly ConcurrentBag<LogBuffer> _pool = [];
    private readonly int _maxSize = maxSize;

    public LogBuffer Rent (ILogFileInfo fileInfo, int maxLines)
    {
        if (_pool.TryTake(out var buffer))
        {
            buffer.Reinitialise(fileInfo, maxLines);
            return buffer;
        }

        return new LogBuffer(fileInfo, maxLines);
    }

    public void Return (LogBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        buffer.DisposeContent();
        if (_pool.Count < _maxSize)
        {
            _pool.Add(buffer);
        }
    }
}
