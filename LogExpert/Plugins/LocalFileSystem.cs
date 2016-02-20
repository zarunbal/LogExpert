using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class LocalFileSystem : IFileSystemPlugin
	{
		private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		#region IFileSystemPlugin Member

		public bool CanHandleUri(string uriString)
		{
			try
			{
				Uri uri = new Uri(uriString);
				return (uri.IsFile);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				return false;
			}
		}

		public ILogFileInfo GetLogfileInfo(string uriString)
		{
			Uri uri = new Uri(uriString);
			if (uri.IsFile)
			{
				ILogFileInfo logFileInfo = new LogFileInfo(uri);
				return logFileInfo;
			}
			else
			{
				throw new UriFormatException("Uri " + uriString + " is no file Uri");
			}
		}

		public string Text
		{
			get
			{
				return "Local file system";
			}
		}

		public string Description
		{
			get
			{
				return "Access files from normal file system.";
			}
		}

		#endregion IFileSystemPlugin Member
	}
}