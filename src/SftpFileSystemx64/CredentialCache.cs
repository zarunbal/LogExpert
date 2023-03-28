using System.Collections.Generic;

namespace SftpFileSystem
{
    internal class CredentialCache
    {
        #region Private Fields

        private readonly IList<Credentials> _credList = new List<Credentials>();

        #endregion

        #region Private Methods

        private void RemoveCredentials(string host, string user)
        {
            Credentials credentials = GetCredentials(host, user);
            if (credentials != null)
            {
                _credList.Remove(credentials);
            }
        }

        #endregion

        internal IList<string> GetUsersForHost(string host)
        {
            IList<string> result = new List<string>();

            foreach (Credentials cred in _credList)
            {
                if (cred.Host.Equals(host))
                {
                    result.Add(cred.UserName);
                }
            }

            return result;
        }

        internal Credentials GetCredentials(string host, string user)
        {
            foreach (Credentials cred in _credList)
            {
                if (cred.Host.Equals(host) && cred.UserName.Equals(user))
                {
                    return cred;
                }
            }

            return null;
        }

        internal void Add(Credentials cred)
        {
            RemoveCredentials(cred.Host, cred.UserName);
            _credList.Add(cred);
        }
    }
}
