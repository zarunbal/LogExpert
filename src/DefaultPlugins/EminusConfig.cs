using System;

namespace LogExpert
{
    [Serializable]
    public class EminusConfig
    {
        #region Fields

        public string host = "127.0.0.1";
        public string password = "";
        public int port = 12345;

        #endregion

        #region Public methods

        public EminusConfig Clone()
        {
            EminusConfig config = new();
            config.host = host;
            config.port = port;
            config.password = password;
            return config;
        }

        #endregion
    }
}