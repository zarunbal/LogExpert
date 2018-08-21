using LogExpert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace PSFileSystem
{
    internal class UNCLogFileInfo : ILogFileInfo
    {
        #region Fields

        private const int RetryCount = 20;
        private const int RetrySleep = 250;
        private readonly ILogExpertLogger _logger;
        private readonly string _remoteFileName;
        private readonly string _host;

        private readonly PSFileSystem _fileSystem;

        private DateTime _lastChange = DateTime.Now;
        private long _lastLength;

        private NetworkConnection _connection;
        private NetworkCredential _creds;
        private string _lastErrorText;

        #endregion

        #region cTor

        internal UNCLogFileInfo(PSFileSystem fileSystem, Uri fileUri, ILogExpertLogger logger)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            Uri = fileUri;
            _remoteFileName = Uri.LocalPath;
            _remoteFileName = _remoteFileName.Substring(1, _remoteFileName.Length - 1);
            _remoteFileName = _remoteFileName.Replace(":", "$");
            _remoteFileName = _remoteFileName.Replace("/", @"\");
            _host = @"\\" + Uri.Host;

            Credentials credentials = _fileSystem.GetCredentials(Uri, true, false);
            bool success = Authenticate(credentials.UserName, credentials.Password);
            if (success != true)
            {
                // first fail -> try again with disabled cache
                credentials = _fileSystem.GetCredentials(Uri, false, false);
                success = Authenticate(credentials.UserName, credentials.Password);
                if (success != true)
                {
                    // 2nd fail -> abort
                    MessageBox.Show("Authentication failed!");
                    //MessageBox.Show(sftp.LastErrorText);
                    return;
                }
            }

            success = IsConnected();
            if (success != true)
            {
                _logger.LogError(_lastErrorText);
                MessageBox.Show(_lastErrorText);
                return;
            }

            OriginalLength = _lastLength = Length;
        }


        public bool IsConnected()
        {
            bool result = false;
            try
            {
                _connection = new NetworkConnection(_host, _creds);

                _connection.Dispose();

                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        private bool Authenticate(string login, string password)
        {
            bool result = false;
            try
            {
                System.Security.SecureString sString = new System.Security.SecureString();
                foreach (char passwordChar in password)
                {
                    sString.AppendChar(passwordChar);
                }
                _creds = new NetworkCredential(login, sString);
                _connection = new NetworkConnection(_host, _creds);

                _connection.Dispose();

                result = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _lastErrorText = e.Message;
            }
            return result;
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
            get
            {
                using(_connection = new NetworkConnection(_host, _creds))
                {
                    using (var stream = File.Open(_host + @"\" + _remoteFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return stream.Length;
                    }
                }
            }
        }

        public long OriginalLength { get; } = -1;

        public bool FileExists
        {
            get
            {
                using (_connection = new NetworkConnection(_host, _creds))
                {
                    return File.Exists(_host + @"\" + _remoteFileName);
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
                    using (_connection = new NetworkConnection(_host, _creds))
                    {
                        return File.Open(_host + @"\" + _remoteFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
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
    }
}
