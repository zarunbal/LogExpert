using System;
using System.Collections.Generic;
using System.Text;

namespace SftpFileSystem
{
    public class ConfigData
    {
        #region Properties

        public string KeyFile { get; set; }

        public bool UseKeyfile { get; set; }

        public KeyType KeyType { get; set; }

        public SshApiType SshApiType { get; set; }

        public string ChilkatKey { get; set; }

        #endregion
    }
}