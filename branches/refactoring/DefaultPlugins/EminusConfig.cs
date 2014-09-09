using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  [Serializable]
  public class EminusConfig
  {
    public string host = "127.0.0.1";
    public int port = 12345;
    public string password = "";

    public EminusConfig Clone()
    {
      EminusConfig config = new EminusConfig();
      config.host = this.host;
      config.port = this.port;
      config.password = this.password;
      return config;
    }

  }
}
