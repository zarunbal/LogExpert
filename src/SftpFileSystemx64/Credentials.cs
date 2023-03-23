namespace SftpFileSystem
{
    internal class Credentials
    {
        #region Ctor

        internal Credentials(string host, string userName, string password)
        {
            Host = host;
            UserName = userName;
            Password = password;
        }

        #endregion

        #region Properties / Indexers

        public string Host { get; }

        public string Password { get; }

        public string UserName { get; }

        #endregion
    }
}
