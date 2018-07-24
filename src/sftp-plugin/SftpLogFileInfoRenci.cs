using System;
using Renci.SshNet;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace SftpFileSystem
{
    internal class SftpLogFileInfoRenci : ILogFileInfo
    {
        #region Fields

        private const int RetryCount = 20;
        private const int RetrySleep = 250;
        private readonly ILogExpertLogger _logger;
        private readonly string _remoteFileName;

        private readonly SftpFileSystem _sftFileSystem;
        private readonly SftpClient _sftp;
        private readonly object _sshKeyMonitor = new object();
        private DateTime _lastChange = DateTime.Now;
        private long _lastLength;

        #endregion

        #region cTor

        internal SftpLogFileInfoRenci(SftpFileSystem sftFileSystem, Uri fileUri, ILogExpertLogger logger)
        {
            _logger = logger;
            _sftFileSystem = sftFileSystem;
            Uri = fileUri;
            _remoteFileName = Uri.PathAndQuery;
            
            int port = Uri.Port != -1 ? Uri.Port : 22;

            bool success = false;
            bool cancelled = false;
            if (_sftFileSystem.ConfigData.UseKeyfile)
            {
                lock (_sshKeyMonitor) // prevent multiple password dialogs when opening multiple files at once
                {
                    while (_sftFileSystem.SshKey == null)
                    {
                        // Load key from file, possibly encrypted by password
                        
                        PrivateKeyPasswordDialog dlg = new PrivateKeyPasswordDialog();
                        DialogResult dialogResult = dlg.ShowDialog();
                        if (dialogResult == DialogResult.Cancel)
                        {
                            cancelled = true;
                            break;
                        }

                        string password = dlg.Password;
                        PrivateKeyFile keyfile = new PrivateKeyFile(_sftFileSystem.ConfigData.KeyFile, password);
                        
                        _sftFileSystem.SshRenciKey = keyfile;
                    }
                }

                if (!cancelled)
                {
                    success = false;
                    Credentials credentials = _sftFileSystem.GetCredentials(Uri, true, true);
                    while (!success)
                    {
                        try
                        {
                            _sftp = new SftpClient(Uri.Host, credentials.UserName, _sftFileSystem.SshRenciKey);
                            _sftp.Connect();
                            success = true;
                        }catch(Exception e)
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
                try {
                    _sftp = new SftpClient(Uri.Host, credentials.UserName, credentials.Password);
                    _sftp.Connect();
                } catch (Exception e)
                {
                    // first fail -> try again with disabled cache
                    credentials = _sftFileSystem.GetCredentials(Uri, false, false);
                    try {
                        _sftp = new SftpClient(Uri.Host, credentials.UserName, credentials.Password);
                        _sftp.Connect();
                    }catch(Exception ex)
                    {
                        // 2nd fail -> abort
                        MessageBox.Show("Authentication failed!");
                        //MessageBox.Show(sftp.LastErrorText);
                        return;
                    }
                }
            }
            
            if (!_sftp.IsConnected)
            {
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

        public bool Connected
        {
            get
            {
                return _sftp.IsConnected;
            }
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
            get { return _sftp.Open(_remoteFileName, FileMode.Open).Length; }
        }

        public long OriginalLength { get; } = -1;

        public bool FileExists
        {
            get
            {
                try
                {
                    long len = _sftp.Open(_remoteFileName, FileMode.Open).Length;
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
                    return (int) diff.TotalSeconds * 100;
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
                    return _sftp.Open(_remoteFileName, FileMode.Open);
                }
                catch (IOException fe)
                {
                    if (--retry <= 0)
                    {
                        throw fe;
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
            _sftp.OperationTimeout = new TimeSpan(15000);
        }

        #endregion
    }
}