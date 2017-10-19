using System;
using System.Collections.Generic;
using System.Text;

namespace ColumnizerLib
{
  /// <summary>
  /// Service interface implemented by LogExpert. This can be used by IFileSystemPlugin implementations to get certain services.
  /// </summary>
  public interface IFileSystemCallback
  {
    /// <summary>
    /// Retrieve a logger. The plugin can use the logger to write log messages into LogExpert's log file.
    /// </summary>
    /// <returns></returns>
    ILogExpertLogger GetLogger();
  }
}
