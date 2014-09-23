using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	/// <summary>
	/// Collects some system information (OS, Runtime etc.)
	/// </summary>
	internal class SystemInfo
	{
		StringBuilder info = new StringBuilder();

		internal SystemInfo()
		{
			info.Append("OS:  ").AppendLine(System.Environment.OSVersion.ToString());
			info.Append("CLR: ").AppendLine(System.Environment.Version.ToString());
		}

		public String Info
		{
			get
			{
				return this.info.ToString();
			}
		}
	}
}