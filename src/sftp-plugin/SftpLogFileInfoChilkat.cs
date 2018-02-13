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

        private const int RETRY_COUNT = 20;
        private const int RETRY_SLEEP = 250;
        private DateTime lastChange = DateTime.Now;
        private long lastLength;
        private readonly ILogExpertLogger logger;
        private readonly string remoteFileName;

        private readonly SftpFileSystem sftFileSystem;
        private readonly SFtp sftp = new SFtp();
        private readonly object sshKeyMonitor = new object();

        #endregion

        #region cTor

        internal SftpLogFileInfoChilkat(SftpFileSystem sftFileSystem, Uri fileUri, ILogExpertLogger logger)
        {
            this.logger = logger;
            this.sftFileSystem = sftFileSystem;
            Uri = fileUri;
            remoteFileName = Uri.PathAndQuery;

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = sftp.UnlockComponent("PUT_SERIAL_HERE");
            if (success != true)
            {
                this.logger.LogError(sftp.LastErrorText);
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            int port = Uri.Port != -1 ? Uri.Port : 22;
            success = sftp.Connect(Uri.Host, port);
            if (success != true)
            {
                this.logger.LogError(sftp.LastErrorText);
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            success = false;
            bool cancelled = false;
            if (this.sftFileSystem.ConfigData.UseKeyfile)
            {
                lock (sshKeyMonitor) // prevent multiple password dialogs when opening multiple files at once
                {
                    while (this.sftFileSystem.SshKey == null)
                    {
                        // Load key from file, possibly encrypted by password

                        SshKey sshKey = new SshKey();
                        string keyText = sshKey.LoadText(this.sftFileSystem.ConfigData.KeyFile);

                        PrivateKeyPasswordDialog dlg = new PrivateKeyPasswordDialog();
                        DialogResult dialogResult = dlg.ShowDialog();
                        if (dialogResult == DialogResult.Cancel)
                        {
                            cancelled = true;
                            break;
                        }

                        sshKey.Password = dlg.Password;
                        if (this.sftFileSystem.ConfigData.KeyType == KeyType.Ssh)
                        {
                            logger.Info("Loading SSH key from " + this.sftFileSystem.ConfigData.KeyFile);
                            success = sshKey.FromOpenSshPrivateKey(keyText);
                        }
                        else
                        {
                            logger.Info("Loading Putty key from " + this.sftFileSystem.ConfigData.KeyFile);
                            success = sshKey.FromPuttyPrivateKey(keyText);
                        }

                        if (!success)
                        {
                            MessageBox.Show("Loading key file failed");
                        }
                        else
                        {
                            this.sftFileSystem.SshKey = sshKey;
                        }
                    }
                }

                if (!cancelled)
                {
                    success = false;
                    Credentials credentials = this.sftFileSystem.GetCredentials(Uri, true, true);
                    while (!success)
                    {
                        success = sftp.AuthenticatePk(credentials.UserName, this.sftFileSystem.SshKey);
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
                            credentials = this.sftFileSystem.GetCredentials(Uri, false, true);
                        }
                    }
                }
            }

            if (!success)
            {
                // username/password auth

                Credentials credentials = this.sftFileSystem.GetCredentials(Uri, true, false);
                success = sftp.AuthenticatePw(credentials.UserName, credentials.Password);
                if (success != true)
                {
                    // first fail -> try again with disabled cache
                    credentials = this.sftFileSystem.GetCredentials(Uri, false, false);
                    success = sftp.AuthenticatePw(credentials.UserName, credentials.Password);
                    if (success != true)
                    {
                        // 2nd fail -> abort
                        MessageBox.Show("Authentication failed!");
                        //MessageBox.Show(sftp.LastErrorText);
                        return;
                    }
                }
            }

            success = sftp.InitializeSftp();
            if (success != true)
            {
                this.logger.LogError(sftp.LastErrorText);
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            OriginalLength = lastLength = Length;
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
            get { return sftp.GetFileSize64(remoteFileName, true, false); }
        }

        public long OriginalLength { get; } = -1;

        public bool FileExists
        {
            get
            {
                try
                {
                    long len = sftp.GetFileSize64(remoteFileName, true, false);
                    return len != -1;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public int PollInterval
        {
            get
            {
                TimeSpan diff = DateTime.Now - lastChange;
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
            int retry = RETRY_COUNT;
            while (true)
            {
                try
                {
                    return new SftpStreamChilkat(sftp, remoteFileName);
                }
                catch (IOException fe)
                {
                    if (--retry <= 0)
                    {
                        throw fe;
                    }

                    Thread.Sleep(RETRY_SLEEP);
                }
            }
        }

        public bool FileHasChanged()
        {
            if (Length != lastLength)
            {
                lastLength = Length;
                lastChange = DateTime.Now;
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods

        private void Configure()
        {
            //  Set some timeouts, in milliseconds:
            sftp.ConnectTimeoutMs = 5000;
            sftp.IdleTimeoutMs = 15000;
        }

        #endregion
    }
}