using System;
using System.Collections.Generic;
using System.Text;


namespace LogExpert
{
  class FileSystemCallback : IFileSystemCallback
  {
    #region IFileSystemCallback Member

    public ILogExpertLogger GetLogger()
    {
      return Logger.GetLogger();
    }

    #endregion
  }
}
