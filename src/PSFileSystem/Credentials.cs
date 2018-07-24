using System;
using System.Collections.Generic;
using System.Text;

namespace PSFileSystem
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

        public string Host { get; }

        public string UserName { get; }

        public string Password { get; }

        #endregion
    }
}