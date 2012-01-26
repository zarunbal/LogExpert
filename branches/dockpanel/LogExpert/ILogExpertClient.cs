using System;
using System.Security.Permissions;
namespace LogExpert
{
  public interface ILogExpertClient
  {
    int Id
    {
      get;
    }

    void NotifySettingsChanged(ILogExpertProxy server, Object cookie);

    void OnSettingsChanged(Object cookie);

    ILogExpertProxy Proxy
    {
      get;
    }
    
  }

}