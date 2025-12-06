namespace LogExpert.Core.Interface;

public interface ILogStreamReaderSpan : ILogStreamReader
{

    /// <summary>
    /// Attempts to read the next line from the stream.
    /// </summary>
    /// <param name="lineMemory">
    /// When this method returns <c>true</c>, contains a <see cref="ReadOnlyMemory{Char}"/> representing the next line read from the stream.
    /// The memory is only valid until the next call to <see cref="TryReadLine"/> or until <see cref="ReturnMemory"/> is called.
    /// </param>
    /// <returns>
    /// <c>true</c> if a line was successfully read; <c>false</c> if the end of the stream has been reached or no more lines are available.
    /// </returns>
    /// <remarks>
    /// The returned memory is only valid until the next call to <see cref="TryReadLine"/> or until <see cref="ReturnMemory"/> is called.
    /// This method is not guaranteed to be thread-safe; concurrent access should be synchronized externally.
    /// </remarks>
    bool TryReadLine (out ReadOnlyMemory<char> lineMemory);

    /// <summary>
    /// Returns the memory buffer previously obtained from <see cref="TryReadLine"/> to the underlying pool or resource manager.
    /// </summary>
    /// <param name="memory">
    /// The <see cref="ReadOnlyMemory{Char}"/> instance previously obtained from <see cref="TryReadLine"/>.
    /// </param>
    /// <remarks>
    /// Call this method when you are done processing the memory returned by <see cref="TryReadLine"/> to avoid memory leaks or resource retention.
    /// Failing to call this method may result in increased memory usage.
    /// It is safe to call this method multiple times for the same memory, but only the first call will have an effect.
    /// </remarks>
    void ReturnMemory (ReadOnlyMemory<char> memory);
}
