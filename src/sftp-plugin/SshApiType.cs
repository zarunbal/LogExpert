using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SftpFileSystem
{
    public enum SshApiType
    {
        [Description("RenciSSH")]
        Renci,
        [Description("Chilkat")]
        Chilkat
    }
}
