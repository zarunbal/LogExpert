using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using LogExpert;

namespace PSFileSystem
{
    internal class CredentialCache : MarshalByRefObject, ICredentialCache
    {
        #region Fields

        private readonly IList<Credentials> _credList = new List<Credentials>();

        #endregion

        #region Internals

        internal IList<string> GetUsersForHost(string host)
        {
            IList<string> result = new List<string>();
            foreach (Credentials cred in _credList)
            {
                if (cred.Host.ToLower().Equals(host.ToLower()))
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
                if (cred.Host.ToLower().Equals(host.ToLower()) && cred.UserName.ToLower().Equals(user.ToLower()))
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

        #region Public Methods

        public void Add(string host, string username, string password)
        {
            RemoveCredentials(host, username);
            Credentials cred = new Credentials(host, username, password);
            _credList.Add(cred);
        }

        #endregion
    }
}