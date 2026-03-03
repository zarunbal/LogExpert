using System.Buffers;

namespace ColumnizerLib;

/// <summary>
/// <para>
/// Implement this interface in your columnizer if you want to pre-process every line
/// directly when it's loaded from file system.</para>
/// <para>
/// You can also use this to drop lines.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// By implementing this interface with your Columnizer you get the ability to modify the
/// content of a log file right before it will be seen by LogExpert.
/// </para>
/// <para>
/// Note that the <see cref="PreProcessLine"/>
/// method is only used when loading a line from disk. Because of internal buffering a log line may
/// be read only once or multiple times. You have to ensure that the behaviour is consistent
/// for every call to <see cref="PreProcessLine"/> for a specific line. That's especially true
/// when dropping lines. Dropping a line changes the line count seen by LogExpert. That has implications
/// for things like bookmarks etc.
/// </para>
/// </remarks>
public interface IPreProcessColumnizerMemory : IPreProcessColumnizer
{
    #region Public methods

    /// <summary>
    /// Memory-optimized preprocessing method that returns <see cref="ReadOnlyMemory{T}"/> to avoid string allocations.
    /// </summary>
    /// <param name="logLine">Line content as ReadOnlyMemory</param>
    /// <param name="lineNum">Line number as seen by LogExpert</param>
    /// <param name="realLineNum">Actual line number in the file</param>
    /// <returns>The changed content as <see cref="ReadOnlyMemory{T}"/>, the original memory if unchanged, or <see cref="ReadOnlyMemory{T}"/>.Empty to drop the line </returns>
    /// <remarks>
    /// <para>
    /// Return values:
    /// - Original memory: Line unchanged, no allocation
    /// - <see cref="ReadOnlyMemory{T}"/>.Empty: Drop the line
    /// - New memory: Modified line content
    /// </para>
    /// <para>
    /// When creating modified content, consider using <see cref="ArrayPool{T}"/> to reduce allocations
    /// for temporary buffers, but the returned memory must be owned (not pooled).
    /// </para>
    /// </remarks>
    ///
    ReadOnlyMemory<char> PreProcessLine (ReadOnlyMemory<char> logLine, int lineNum, int realLineNum);

    #endregion
}

