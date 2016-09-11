using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
	public class LogExpertCallback : ColumnizerCallback, ILogExpertCallback
	{
		public LogExpertCallback(LogWindow logWindow)
			: base(logWindow)
		{
		}

		#region ILogExpertCallback Member

		public void AddTempFileTab(string fileName, string title)
		{
			_logWindow.AddTempFileTab(fileName, title);
		}

		public void AddPipedTab(IList<LineEntry> lineEntryList, string title)
		{
			_logWindow.WritePipeTab(lineEntryList, title);
		}

		public string GetTabTitle()
		{
			return _logWindow.Text;
		}

		#endregion
	}
}
