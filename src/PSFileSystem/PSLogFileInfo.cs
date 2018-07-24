using System;
using LogExpert;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace PSFileSystem
{
    internal class PSLogFileInfo : ILogFileInfo
    {
        #region Fields

        private const int RetryCount = 20;
        private const int RetrySleep = 250;
        private readonly ILogExpertLogger _logger;
        private readonly string _remoteFileName;

        private readonly PSFileSystem _fileSystem;

        private DateTime _lastChange = DateTime.Now;
        private long _lastLength;

        private Runspace _runspace;
        private int _port = 5985;
        private string _lastErrorText;

        #endregion

        #region cTor

        internal PSLogFileInfo(PSFileSystem fileSystem, Uri fileUri, ILogExpertLogger logger)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            Uri = fileUri;
            _remoteFileName = Uri.PathAndQuery;
            _remoteFileName = _remoteFileName.Substring(1, _remoteFileName.Length - 1);

            _port = Uri.Port != -1 ? Uri.Port : 5985;
            
            Credentials credentials = _fileSystem.GetCredentials(Uri, true, false);
            bool success = Authenticate(Uri.Host, credentials.UserName, credentials.Password);
            if (success != true)
            {
                // first fail -> try again with disabled cache
                credentials = _fileSystem.GetCredentials(Uri, false, false);
                success = Authenticate(Uri.Host, credentials.UserName, credentials.Password);
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

        private bool Authenticate(string host, string login, string password)
        {
            bool result = false;
            try
            {
                string shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
                System.Security.SecureString sString = new System.Security.SecureString();
                foreach (char passwordChar in password)
                {
                    sString.AppendChar(passwordChar);
                }
                PSCredential credential = new PSCredential(login, sString);
                var connectionInfo = new WSManConnectionInfo(false, host, _port, "/wsman", shellUri, credential);

                _runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                _runspace.Open();

                result = true;
            }catch(Exception e)
            {
                _logger.LogError(e.Message);
                _lastErrorText = e.Message;
            }
            return result;
        }

        private bool IsConnected()
        {
            bool result = false;
            if (_runspace != null)
            {
                result = _runspace.RunspaceStateInfo.State == RunspaceState.Opened;
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
            get {
                Pipeline pipe = _runspace.CreatePipeline();
                pipe.Commands.AddScript("$(Get-ChildItem " + _remoteFileName + ").Length | Write-Output");
                Collection<PSObject> files = pipe.Invoke();
                pipe.Dispose();

                return int.Parse(files[0].ToString());
            }
        }

        public long OriginalLength { get; } = -1;

        public bool FileExists
        {
            get
            {
                try
                {
                    Pipeline pipe = _runspace.CreatePipeline();
                    pipe.Commands.AddScript("$(Get-ChildItem " + _remoteFileName + ").Length | Write-Output");
                    Collection<PSObject> files = pipe.Invoke();
                    pipe.Dispose();

                    return files.Count != 0;
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
                    Pipeline pipe = _runspace.CreatePipeline();
                    pipe.Commands.AddScript("$fs = New-Object IO.FileStream \"" + _remoteFileName + "\" ,'Open','Read','ReadWrite'; $sr = New-Object IO.StreamReader $fs; while($sr.EndOfStream -ne $true){ $sr.ReadLine(); };");
                    Collection<PSObject> oblines = pipe.Invoke();
                    pipe.Dispose();

                    if (oblines.Count > 0)
                    {
                        string pathTemp = Path.GetTempPath() + Path.GetRandomFileName();
                        foreach (var obline in oblines)
                        {
                            File.AppendAllText(pathTemp, obline.ToString() + Environment.NewLine);
                        }
                        if (File.Exists(pathTemp))
                        {
                            return File.OpenText(pathTemp).BaseStream;
                        }
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