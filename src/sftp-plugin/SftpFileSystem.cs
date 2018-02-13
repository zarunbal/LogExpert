using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using Chilkat;
using LogExpert;

namespace SftpFileSystem
{
    public class SftpFileSystem : IFileSystemPlugin, ILogExpertPluginConfigurator
    {
        #region Fields

        private readonly ILogExpertLogger _logger;

        private ConfigDialog _configDialog;
        private volatile SshKey _sshKey;

        #endregion

        #region cTor

        public SftpFileSystem(IFileSystemCallback callback)
        {
            _logger = callback.GetLogger();
            CredentialsCache = new CredentialCache();
        }

        #endregion

        #region Properties

        internal CredentialCache CredentialsCache { get; }

        public string Text
        {
            get { return "SFTP plugin"; }
        }

        public string Description
        {
            get { return "Can read log files directly from SFTP server."; }
        }

        public ConfigData ConfigData { get; private set; } = new ConfigData();

        public SshKey SshKey
        {
            get { return _sshKey; }
            set { _sshKey = value; }
        }

        #endregion

        #region Public methods

        public bool CanHandleUri(string uriString)
        {
            try
            {
                Uri uri = new Uri(uriString);
                return uri.Scheme.Equals("sftp", StringComparison.InvariantCultureIgnoreCase);
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
                Uri uri = new Uri(uriString.Replace('\\', '/'));
                return new SftpLogFileInfoChilkat(this, uri, _logger);
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

        public void HideConfigForm()
        {
            ConfigData = _configDialog.ConfigData;
            _configDialog.Hide();
            _configDialog.Dispose();
        }

        public void LoadConfig(string configDir)
        {
            XmlSerializer xml = new XmlSerializer(ConfigData.GetType());
            try
            {
                FileStream fs = new FileStream(configDir + "\\" + "sftpfilesystem.cfg", FileMode.Open);
                ConfigData = (ConfigData) xml.Deserialize(fs);
                fs.Close();
            }
            catch (IOException e)
            {
                _logger.LogError(e.Message);
            }
        }

        public void SaveConfig(string configDir)
        {
            _logger.Info("Saving SFTP config");
            XmlSerializer xml = new XmlSerializer(ConfigData.GetType());
            try
            {
                FileStream fs = new FileStream(configDir + "\\" + "sftpfilesystem.cfg", FileMode.Create);
                xml.Serialize(fs, ConfigData);
                fs.Close();
            }
            catch (IOException e)
            {
                _logger.LogError(e.Message);
            }
        }

        public void ShowConfigDialog(Form owner)
        {
            throw new NotImplementedException();
        }

        public void ShowConfigForm(Panel parentPanel)
        {
            _configDialog = new ConfigDialog(ConfigData);
            _configDialog.Parent = parentPanel;
            _configDialog.Show();
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