using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace LogExpert.Classes
{
	public static class Environment2
	{
		private static string _startupPath;

		/// <summary>
		/// Gets the path for the executable file that started the application, not including the executable name.
		/// </summary>
		public static string StartupPath
		{
			get
			{
				if (string.IsNullOrEmpty(_startupPath))
				{
					_startupPath = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath);
				}

				return _startupPath;
			}
		}
	}
}
