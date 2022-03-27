using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Chilkat;

namespace SftpFileSystem
{
    internal class SftpLogFileInfoChilkat : ILogFileInfo
    {
        #region Fields

        private const int RetryCount = 20;
        private const int RetrySleep = 250;
        private readonly ILogExpertLogger _logger;
        private readonly string _remoteFileName;

        private readonly SftpFileSystem _sftFileSystem;
        private readonly SFtp _sftp = new SFtp();
        private readonly object _sshKeyMonitor = new object();
        private DateTime _lastChange = DateTime.Now;
        private long _lastLength;

        #endregion

        #region cTor

        internal SftpLogFileInfoChilkat(SftpFileSystem sftFileSystem, Uri fileUri, ILogExpertLogger logger)
        {
            _logger = logger;
            _sftFileSystem = sftFileSystem;
            Uri = fileUri;
            _remoteFileName = Uri.PathAndQuery;

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = _sftp.UnlockComponent("PUT_SERIAL_HERE");
            if (success != true)
            {
                _logger.LogError(_sftp.LastErrorText);
                MessageBox.Show(_sftp.LastErrorText);
                return;
            }

            int port = Uri.Port != -1 ? Uri.Port : 22;
            success = _sftp.Connect(Uri.Host, port);
            if (success != true)
            {
                _logger.LogError(_sftp.LastErrorText);
                MessageBox.Show(_sftp.LastErrorText);
                return;
            }

            success = false;
            bool cancelled = false;
            if (_sftFileSystem.ConfigData.UseKeyfile)
            {
                lock (_sshKeyMonitor) // prevent multiple password dialogs when opening multiple files at once
                {
                    while (_sftFileSystem.SshKey == null)
                    {
                        // Load key from file, possibly encrypted by password

                        SshKey sshKey = new SshKey();
                        string keyText = sshKey.LoadText(_sftFileSystem.ConfigData.KeyFile);

                        PrivateKeyPasswordDialog dlg = new PrivateKeyPasswordDialog();
                        DialogResult dialogResult = dlg.ShowDialog();
                        if (dialogResult == DialogResult.Cancel)
                        {
                            cancelled = true;
                            break;
                        }

                        sshKey.Password = dlg.Password;
                        if (_sftFileSystem.ConfigData.KeyType == KeyType.Ssh)
                        {
                            logger.Info("Loading SSH key from " + _sftFileSystem.ConfigData.KeyFile);
                            success = sshKey.FromOpenSshPrivateKey(keyText);
                        }
                        else
                        {
                            logger.Info("Loading Putty key from " + _sftFileSystem.ConfigData.KeyFile);
                            success = sshKey.FromPuttyPrivateKey(keyText);
                        }

                        if (!success)
                        {
                            MessageBox.Show("Loading key file failed");
                        }
                        else
                        {
                            _sftFileSystem.SshKey = sshKey;
                        }
                    }
                }

                if (!cancelled)
                {
                    success = false;
                    Credentials credentials = _sftFileSystem.GetCredentials(Uri, true, true);
                    while (!success)
                    {
                        success = _sftp.AuthenticatePk(credentials.UserName, _sftFileSystem.SshKey);
                        if (!success)
                        {
                            FailedKeyDialog dlg = new FailedKeyDialog();
                            DialogResult res = dlg.ShowDialog();
                            dlg.Dispose();
                            if (res == DialogResult.Cancel)
                            {
                                return;
                            }

                            if (res == DialogResult.OK)
                            {
                                break; // go to user/pw auth
                            }

                            // retries with disabled cache
                            credentials = _sftFileSystem.GetCredentials(Uri, false, true);
                        }
                    }
                }
            }

            if (!success)
            {
                // username/password auth

                Credentials credentials = _sftFileSystem.GetCredentials(Uri, true, false);
                success = _sftp.AuthenticatePw(credentials.UserName, credentials.Password);
                if (success != true)
                {
                    // first fail -> try again with disabled cache
                    credentials = _sftFileSystem.GetCredentials(Uri, false, false);
                    success = _sftp.AuthenticatePw(credentials.UserName, credentials.Password);
                    if (success != true)
                    {
                        // 2nd fail -> abort
                        MessageBox.Show("Authentication failed!");
                        //MessageBox.Show(sftp.LastErrorText);
                        return;
                    }
                }
            }

            success = _sftp.InitializeSftp();
            if (success != true)
            {
                _logger.LogError(_sftp.LastErrorText);
                MessageBox.Show(_sftp.LastErrorText);
                return;
            }

            OriginalLength = _lastLength = Length;
        }

        #endregion

        #region Properties

        public string FullName
        {
            get { return Uri.ToString(); }
        }

        public string FileName
        {
            get
            {
                string full = FullName;
                int i = full.LastIndexOf(DirectorySeparatorChar);
                return full.Substring(i + 1);
            }
        }

        public string DirectoryName
        {
            get
            {
                string full = FullName;
                int i = full.LastIndexOf(DirectorySeparatorChar);
                if (i != -1)
                {
                    return full.Substring(0, i);
                }

                return ".";
            }
        }

        public char DirectorySeparatorChar
        {
            get { return '/'; }
        }

        public Uri Uri { get; }

        public long Length
        {
            get { return _sftp.GetFileSize64(_remoteFileName, true, false); }
        }

        public long OriginalLength { get; } = -1;

        public bool FileExists
        {
            get
            {
                try
                {
                    long len = _sftp.GetFileSize64(_remoteFileName, true, false);
                    return len != -1;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return false;
                }
            }
        }

        public int PollInterval
        {
            get
            {
                TimeSpan diff = DateTime.Now - _lastChange;
                if (diff.TotalSeconds < 4)
                {
                    return 400;
                }
                else if (diff.TotalSeconds < 30)
                {
                    return (int)diff.TotalSeconds * 100;
                }
                else
                {
                    return 5000;
                }
            }
        }

        #endregion

        #region Public methods

        public System.IO.Stream OpenStream()
        {
            int retry = RetryCount;
            while (true)
            {
                try
                {
                    return new SftpStreamChilkat(_sftp, _remoteFileName);
                }
                catch (IOException)
                {
                    if (--retry <= 0)
                    {
                        throw;
                    }

                    Thread.Sleep(RetrySleep);
                }
            }
        }

        public bool FileHasChanged()
        {
            if (Length != _lastLength)
            {
                _lastLength = Length;
                _lastChange = DateTime.Now;
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods

        private void Configure()
        {
            //  Set some timeouts, in milliseconds:
            _sftp.ConnectTimeoutMs = 5000;
            _sftp.IdleTimeoutMs = 15000;
        }

        #endregion
    }
}