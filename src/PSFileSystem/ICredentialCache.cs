using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace LogExpert
{
    public interface ICredentialCache
    {
        void Add(string host, string username, string password);
    }
}
