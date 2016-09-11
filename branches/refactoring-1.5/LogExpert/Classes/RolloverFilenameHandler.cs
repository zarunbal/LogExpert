using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LogExpert
{
	/// <summary>
	/// Handles rollover naming. The names built by the RolloverFilenameBuilder will be used
	/// to check if the file exist. Names will be built by incrmenting an index and decrementing a date.
	/// A configurable number of days in the past will be checked (date gaps may occur on days without log file activity).
	/// Date checking is only performed if the format pattern contains a date format. Index checking
	/// is only performed of the format pattern contains an index placeholder.
	/// </summary>
	public class RolloverFilenameHandler
	{
		private RolloverFilenameBuilder filenameBuilder;
		private ILogFileInfo logFileInfo;
		private MultifileOptions options;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filePath">The complete path of the logfile</param>
		/// <param name="options">Multifile option (e.g. format pattern)</param>
		public RolloverFilenameHandler(ILogFileInfo logFileInfo, MultifileOptions options)
		{
			this.options = options;
			this.logFileInfo = logFileInfo;
			this.filenameBuilder = new RolloverFilenameBuilder(this.options.FormatPattern);
			this.filenameBuilder.SetFileName(logFileInfo.FileName);
		}

		/// <summary>
		/// Returns a list of the built file names (complete path) which also exists on disk.
		/// The list is created by using the RolloverFilenameBuilder and checking for file existence.
		/// The first entry in the list contains the oldest file. The last entry contains the file given
		/// in the contructor.
		/// </summary>
		/// <returns></returns>
		public LinkedList<string> GetNameList()
		{
			LinkedList<string> fileList = new LinkedList<string>();
			string fileName = filenameBuilder.BuildFileName();
			string filePath = this.logFileInfo.DirectoryName + this.logFileInfo.DirectorySeparatorChar + fileName;
			fileList.AddFirst(filePath);
			bool found = true;
			while (found)
			{
				found = false;
				// increment index and check if file exists
				if (this.filenameBuilder.IsIndexPattern)
				{
					this.filenameBuilder.Index = this.filenameBuilder.Index + 1;
					fileName = filenameBuilder.BuildFileName();
					filePath = this.logFileInfo.DirectoryName + this.logFileInfo.DirectorySeparatorChar + fileName;
					if (FileExists(filePath))
					{
						fileList.AddFirst(filePath);
						found = true;
						continue;
					}
				}
				// if file with index isn't found or no index is in format pattern, decrement the current date
				if (this.filenameBuilder.IsDatePattern)
				{
					int tryCounter = 0;
					this.filenameBuilder.Index = 0;
					while (tryCounter < this.options.MaxDayTry)
					{
						this.filenameBuilder.DecrementDate();
						fileName = filenameBuilder.BuildFileName();
						filePath = this.logFileInfo.DirectoryName + this.logFileInfo.DirectorySeparatorChar + fileName;
						if (FileExists(filePath))
						{
							fileList.AddFirst(filePath);
							found = true;
							break;
						}
						else
						{
							tryCounter++;
						}
					}
				}
			}
			return fileList;
		}

		private bool FileExists(string filePath)
		{
			IFileSystemPlugin fs = PluginRegistry.GetInstance().FindFileSystemForUri(filePath);
			ILogFileInfo info = fs.GetLogfileInfo(filePath);
			return info.FileExists;
		}
	}
}