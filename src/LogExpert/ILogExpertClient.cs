using System;
using System.Security.Permissions;

namespace LogExpert
{
    public interface ILogExpertClient
    {
        #region Properties

        int Id { get; }

        ILogExpertProxy Proxy { get; }

        #endregion

        #region Public methods

        void NotifySettingsChanged(ILogExpertProxy server, object cookie);

        void OnSettingsChanged(object cookie);

        #endregion
    }
}