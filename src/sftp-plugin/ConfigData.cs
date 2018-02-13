using System;
using System.Collections.Generic;
using System.Text;

namespace SftpFileSystem
{
    public class ConfigData
    {
        #region Fields

        #endregion

        #region Properties

        public string KeyFile { get; set; }

        public bool UseKeyfile { get; set; }

        public KeyType KeyType { get; set; }

        #endregion
    }
}