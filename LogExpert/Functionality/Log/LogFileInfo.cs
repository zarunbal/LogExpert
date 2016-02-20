using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace LogExpert
{
	public class LogFileInfo : ILogFileInfo
	{
		#region Fields

		private const int RETRY_COUNT = 5;
		private const int RETRY_SLEEP = 250;

		//FileStream fStream;
		private FileInfo _fInfo;

		private long _lastLength;
		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		#endregion Fields

		#region cTor

		public LogFileInfo(Uri fileUri)
		{
			_fInfo = new FileInfo(fileUri.LocalPath);
			Uri = fileUri;
			OriginalLength = _lastLength = LengthWithoutRetry;
		}

		#endregion cTor

		#region Properties

		public string FullName
		{
			get
			{
				return _fInfo.FullName;
			}
		}

		public string FileName
		{
			get
			{
				return _fInfo.Name;
			}
		}

		public string DirectoryName
		{
			get
			{
				return _fInfo.DirectoryName;
			}
		}

		public char DirectorySeparatorChar
		{
			get
			{
				return Path.DirectorySeparatorChar;
			}
		}

		public Uri Uri { get; private set; }

		public long Length
		{
			get
			{
				if (_fInfo == null)
				{
					return -1;
				}
				int retry = RETRY_COUNT;
				while (retry > 0)
				{
					try
					{
						_fInfo.Refresh();
						return _fInfo.Length;
					}
					catch (IOException e)
					{
						_logger.Error(e);
						if (--retry <= 0)
						{
							_logger.Warn(e, "LogFileInfo.Length: ");
							return -1;
						}
						Thread.Sleep(RETRY_SLEEP);
					}
				}
				return -1;
			}
		}

		public long OriginalLength { get; private set; }

		public bool FileExists
		{
			get
			{
				_fInfo.Refresh();
				return _fInfo.Exists;
			}
		}

		public int PollInterval
		{
			get
			{
				return ConfigManager.Settings.preferences.pollingInterval;
			}
		}

		public long LengthWithoutRetry
		{
			get
			{
				if (_fInfo == null)
				{
					return -1;
				}
				try
				{
					_fInfo.Refresh();
					return _fInfo.Length;
				}
				catch (IOException ex)
				{
					_logger.Error(ex);
					return -1;
				}
			}
		}

		#endregion Properties

		#region Overrides

		public override string ToString()
		{
			return string.Format("{0}, OldLen: {1}, Len: {2}", _fInfo.FullName, OriginalLength, Length);
		}

		#endregion Overrides

		#region Public Methods

		/// <summary>
		/// Creates a new FileStream for the file. The caller is responsible for closing.
		/// If file opening fails it will be tried RETRY_COUNT times. This may be needed sometimes
		/// if the file is locked for a short amount of time or temporarly unaccessible because of
		/// rollover situations.
		/// </summary>
		/// <returns></returns>
		public Stream OpenStream()
		{
			int retry = RETRY_COUNT;
			while (true)
			{
				try
				{
					return new FileStream(_fInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
				}
				catch (IOException fe)
				{
					_logger.Debug(fe, "LogFileInfo.OpenFile(): Retry counter {0}", retry);
					if (--retry <= 0)
					{
						throw fe;
					}
					Thread.Sleep(RETRY_SLEEP);
				}
				catch (UnauthorizedAccessException uae)
				{
					_logger.Debug(uae, "LogFileInfo.OpenFile(): Retry counter '{0}'", retry);
					if (--retry <= 0)
					{
						throw new IOException("Error opening file", uae);
					}
					Thread.Sleep(RETRY_SLEEP);
				}
			}
		}

		public bool FileHasChanged()
		{
			if (LengthWithoutRetry != _lastLength)
			{
				_lastLength = LengthWithoutRetry;
				return true;
			}
			return false;
		}

		#endregion Public Methods
	}
}