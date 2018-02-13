using System;
using System.Collections.Generic;
using System.Text;

namespace SftpFileSystem
{
    internal class Credentials
    {
        #region cTor

        internal Credentials(string host, string userName, string password)
        {
            Host = host;
            UserName = userName;
            Password = password;
        }

        #endregion

        #region Properties

        public string Host { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        #endregion
    }
}