using System;
using System.Collections.Generic;
using System.Text;
using ColumnizerLib;
using LogExpert.Classes;

namespace LogExpert
{
	internal class FileSystemCallback : IFileSystemCallback
	{
		private static readonly LogExpertNlogWrapper _logger = new LogExpertNlogWrapper();

		#region IFileSystemCallback Member

		public ILogExpertLogger GetLogger()
		{
			return _logger;
		}

		#endregion IFileSystemCallback Member
	}
}