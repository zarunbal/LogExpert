using System.IO;

namespace LogExpert
{
  public interface ILogFileInfo
  {
    /// <summary>
    /// Creates a new FileStream for the file. The caller is responsible for closing.
    /// If file opening fails it will be tried RETRY_COUNT times. This may be needed sometimes
    /// if the file is locked for a short amount of time or temporarly unaccessible because of
    /// rollover situations.
    /// </summary>
    /// <returns></returns>
    Stream OpenStream();

    string FileName { get; }
    long Length { get; }
    long OldLength { get; }
  }
}