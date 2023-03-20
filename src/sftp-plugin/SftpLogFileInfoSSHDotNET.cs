using LogExpert;

using Renci.SshNet;
using Renci.SshNet.Sftp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SftpFileSystem
{
    internal class SftpLogFileInfoSSHDotNET : ILogFileInfo
    {

        private const int RetryCount = 20;
        private const int RetrySleep = 250;
        private readonly ILogExpertLogger _logger;
        private readonly string _remoteFileName;

        private readonly SftpFileSystem _sftFileSystem;
        private readonly SftpClient _sftp;
        private readonly object _sshKeyMonitor = new object();
        private DateTime _lastChange = DateTime.Now;
        private long _lastLength;

        PrivateKeyFile _key;

        internal SftpLogFileInfoSSHDotNET(SftpFileSystem sftpFileSystem, Uri fileUri, ILogExpertLogger logger)
        {
            _logger = logger;
            _sftFileSystem = sftpFileSystem;
            Uri = fileUri;
            _remoteFileName = Uri.PathAndQuery;

            int port = Uri.Port != -1 ? Uri.Port : 22;

            bool success = false;
            bool cancelled = false;
            if (_sftFileSystem.ConfigData.UseKeyfile)
            {
                lock (_sshKeyMonitor) // prevent multiple password dialogs when opening multiple files at once
                {
                    while (_sftFileSystem.PrivateKeyFile == null)
                    {
                        PrivateKeyPasswordDialog dlg = new PrivateKeyPasswordDialog();
                        DialogResult dialogResult = dlg.ShowDialog();
                        if (dialogResult == DialogResult.Cancel)
                        {
                            cancelled = true;
                            break;
                        }

                        PrivateKeyFile privateKeyFile = new PrivateKeyFile(_sftFileSystem.ConfigData.KeyFile, dlg.Password);

                        if (privateKeyFile != null)
                        {
                            _sftFileSystem.PrivateKeyFile = privateKeyFile;
                        }
                        else
                        {
                            MessageBox.Show("Loading key file failed");
                        }
                    }
                }

                if (cancelled == false)
                {
                    success = false;
                    Credentials credentials = _sftFileSystem.GetCredentials(Uri, true, true);
                    while (success == false)
                    {
                        //Add ConnectionInfo object
                        _sftp = new SftpClient(Uri.Host, credentials.UserName, new[] { _sftFileSystem.PrivateKeyFile });

                        if (_sftp != null)
                        {
                            success = true;
                        }

                        if (success == false)
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

                if (success == false)
                {
                    // username/password auth

                    Credentials credentials = _sftFileSystem.GetCredentials(Uri, true, false);
                    _sftp = new SftpClient(credentials.UserName, credentials.Password);

                    if (_sftp == null)
                    {
                        // first fail -> try again with disabled cache
                        credentials = _sftFileSystem.GetCredentials(Uri, false, false);
                        _sftp = new SftpClient(credentials.UserName, credentials.Password);
                        
                        if (_sftp == null)
                        {
                            // 2nd fail -> abort
                            MessageBox.Show("Authentication failed!");
                            //MessageBox.Show(sftp.LastErrorText);
                            return;
                        }
                    }
                }

                if (_sftp.IsConnected == false)
                {
                    MessageBox.Show("Sftp is not connected");
                    return;
                }

                OriginalLength = _lastLength = Length;
            }
        }

        public string FullName => Uri.ToString();
        
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

        public char DirectorySeparatorChar => '/';

        public Uri Uri { get; }

        public long Length => _sftp.;
        public long OriginalLength { get; }
        public bool FileExists { get; }
        public int PollInterval { get; }

        public bool FileHasChanged()
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream()
        {
            throw new NotImplementedException();
        }
    }
}
