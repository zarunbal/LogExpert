using System;
using System.Collections.Generic;
using System.Text;

namespace SftpFileSystem
{
  internal class Credentials
  {
    private string host;
    private string userName;
    private string password;

    internal Credentials(string host, string userName, string password)
    {
      this.Host = host;
      this.UserName = userName;
      this.Password = password;
    }

    public string Host
    {
      get { return host; }
      set { host = value; }
    }

    public string UserName
    {
      get { return userName; }
      set { userName = value; }
    }

    public string Password
    {
      get { return password; }
      set { password = value; }
    }
  }
}
