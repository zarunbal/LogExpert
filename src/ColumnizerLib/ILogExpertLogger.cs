using System;
using System.Collections.Generic;
using System.Text;

namespace ColumnizerLib
{
  /// <summary>
  /// Simple Logger interface to let plugins log into LogExpert's application log file.
  /// </summary>
  public interface ILogExpertLogger
  {
    /// <summary>
    /// Logs a message on INFO level to LogExpert#s log file. The logfile is only active in debug builds.
    /// The logger in LogExpert will automatically add the class and the method name of the caller.
    /// </summary>
    /// <param name="msg">A message to be logged.</param>
    void LogInfo(string msg);

    /// <summary>
    /// Logs a message on DEBUG level to LogExpert#s log file. The logfile is only active in debug builds.
    /// The logger in LogExpert will automatically add the class and the method name of the caller.
    /// </summary>
    /// <param name="msg">A message to be logged.</param>
    void LogDebug(string msg);

    /// <summary>
    /// Logs a message on WARN level to LogExpert#s log file. The logfile is only active in debug builds.
    /// The logger in LogExpert will automatically add the class and the method name of the caller.
    /// </summary>
    /// <param name="msg">A message to be logged.</param>
    void LogWarn(string msg);

    /// <summary>
    /// Logs a message on ERROR level to LogExpert#s log file. The logfile is only active in debug builds.
    /// The logger in LogExpert will automatically add the class and the method name of the caller.
    /// </summary>
    /// <param name="msg">A message to be logged.</param>
    void LogError(string msg);
  }
}
