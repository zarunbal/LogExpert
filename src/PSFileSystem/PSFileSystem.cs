using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using LogExpert;
using System.Windows.Forms;

namespace PSFileSystem
{
    public class PSFileSystem : IFileSystemPlugin
    {
        #region Fields

        private readonly ILogExpertLogger _logger;

        #endregion

        #region cTor

        public PSFileSystem(IFileSystemCallback callback)
        {
            _logger = callback.GetLogger();
            CredentialsCache = new CredentialCache();
        }

        #endregion

        #region Properties

        private CredentialCache CredentialsCache { get; }

        public string Text
        {
            get { return "Powershell plugin"; }
        }

        public string Description => "Can read log files directly from Powershell server.";

        #endregion

        #region Public methods

        public bool CanHandleUri(string uriString)
        {
            try
            {
                Uri uri = new Uri(uriString);
                return uri.Scheme.Equals("ps", StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }

        public ILogFileInfo GetLogfileInfo(string uriString)
        {
            try
            {
                ILogFileInfo fileInfo = null;
                Uri uri = new Uri(uriString.Replace('\\', '/'));

                UNCLogFileInfo logFileInfo = new UNCLogFileInfo(this, uri, _logger);
                if (!logFileInfo.IsConnected())
                {
                    fileInfo = new PSLogFileInfo(this, uri, _logger);
                }
                else
                {
                    fileInfo = logFileInfo;
                }

                return fileInfo;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public bool HasEmbeddedForm()
        {
            return true;
        }

        public void StartConfig()
        {

        }

        #endregion

        #region Internals

        internal Credentials GetCredentials(Uri uri, bool cacheAllowed, bool hidePasswordField)
        {
            // Synchronized access to the GetCredentials() method prevents multiple login dialogs when loading multiple files at once 
            // (e.g. on startup). So the user only needs to enter credentials once for the same host.
            lock (this)
            {
                string userName = null;
                string password = null;
                if (uri.UserInfo != null && uri.UserInfo.Length > 0)
                {
                    string[] split = uri.UserInfo.Split(new char[] {':'});
                    if (split.Length > 0)
                    {
                        userName = split[0];
                    }

                    if (split.Length > 1)
                    {
                        password = split[1];
                    }
                }

                IList<string> usersForHost = CredentialsCache.GetUsersForHost(uri.Host);
                if (userName == null && cacheAllowed)
                {
                    if (usersForHost.Count == 1)
                    {
                        userName = usersForHost[0];
                    }
                }

                if (userName != null && password == null && cacheAllowed)
                {
                    Credentials cred = CredentialsCache.GetCredentials(uri.Host, userName);
                    if (cred != null)
                    {
                        return cred;
                    }
                }

                if (userName == null || password == null)
                {
                    LoginDialog dlg = new LoginDialog(uri.Host, usersForHost, hidePasswordField);
                    dlg.UserName = userName;
                    if (DialogResult.OK == dlg.ShowDialog())
                    {
                        password = dlg.Password;
                        userName = dlg.UserName;
                    }

                    dlg.Dispose();
                }

                Credentials credentials = new Credentials(uri.Host, userName, password);
                CredentialsCache.Add(credentials);
                return credentials;
            }
        }

        #endregion
    }
}