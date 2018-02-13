using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace SftpFileSystem
{
  internal class CredentialCache
  {
    private IDictionary<string, Credentials> credentialDict = new Dictionary<string, Credentials>();

    private IList<Credentials> credList = new List<Credentials>(); 

  
    internal IList<string> GetUsersForHost(string host)
    {
      IList<string> result = new List<string>();
      foreach (Credentials cred in this.credList)
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
      foreach (Credentials cred in this.credList)
      {
        if (cred.Host.Equals(host) && cred.UserName.Equals(user))
        {
          return cred;
        }
      }
      return null;
    }


    internal void RemoveCredentials(string host, string user)
    {
      Credentials credentials = GetCredentials(host, user);
      if (credentials != null)
      {
        this.credList.Remove(credentials);
      }
    }

    internal void Add(Credentials cred)
    {
      RemoveCredentials(cred.Host, cred.UserName);
      this.credList.Add(cred);
    }




  }
}
