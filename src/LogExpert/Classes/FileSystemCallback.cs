using System;
using System.Collections.Generic;
using System.Text;


namespace LogExpert
{
    internal class FileSystemCallback : IFileSystemCallback
    {
        #region Public methods

        #region IFileSystemCallback Member

        public ILogExpertLogger GetLogger()
        {
            return Logger.GetLogger();
        }

        #endregion

        #endregion
    }
}