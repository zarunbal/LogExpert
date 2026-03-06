using System.Collections.Concurrent;

using LogExpert.Core.EventArguments;

namespace LogExpert.Core.Classes.Log;

/// <summary>
/// Batches progress updates to reduce UI thread marshalling overhead.
/// Collects updates in a thread-safe queue and processes them on a timer.
/// </summary>
//TODO Refactor
public sealed class BatchedProgressReporter : IDisposable
{
    private readonly ConcurrentQueue<LoadFileEventArgs> _progressQueue = new();
    private readonly Timer _timer;
    private readonly Action<LoadFileEventArgs> _progressCallback;
    private readonly int _updateIntervalMs;
    private bool _disposed;

    /// <summary>
    /// Creates a new batched progress reporter.
    /// </summary>
    /// <param name="progressCallback">Callback to invoke with latest progress</param>
    /// <param name="updateIntervalMs">Update interval in milliseconds (default: 100ms)</param>
    public BatchedProgressReporter (Action<LoadFileEventArgs> progressCallback, int updateIntervalMs = 100)
    {
        _progressCallback = progressCallback ?? throw new ArgumentNullException(nameof(progressCallback));
        _updateIntervalMs = updateIntervalMs;

        // Start timer
        _timer = new Timer(ProcessQueue, null, updateIntervalMs, updateIntervalMs);
    }

    /// <summary>
    /// Reports progress (thread-safe, non-blocking)
    /// </summary>
    public void ReportProgress (LoadFileEventArgs args)
    {
        if (_disposed)
        {
            return;
        }

        // Only keep the latest update - discard old ones
        _progressQueue.Enqueue(args);

        // Keep queue size bounded (max 10 items)
        while (_progressQueue.Count > 10)
        {
            _ = _progressQueue.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Flushes any pending updates immediately
    /// </summary>
    public void Flush ()
    {
        ProcessQueue(null);
    }

    private void ProcessQueue (object state)
    {
        if (_disposed)
        {
            return;
        }

        // Get only the LATEST update (discard intermediate ones)
        LoadFileEventArgs latestUpdate = null;
        while (_progressQueue.TryDequeue(out var update))
        {
            latestUpdate = update;
        }

        // Invoke callback with latest update
        if (latestUpdate != null)
        {
            try
            {
                _progressCallback(latestUpdate);
            }
            catch (Exception ex)
            {
                // Log but don't crash
                System.Diagnostics.Debug.WriteLine($"Error in progress callback: {ex.Message}");
            }
        }
    }

    public void Dispose ()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Flush();
        _timer?.Dispose();

        // Clear queue
        _progressQueue.Clear();
    }
}