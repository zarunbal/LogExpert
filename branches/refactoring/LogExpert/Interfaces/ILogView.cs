using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	/// <summary>
	/// Methods to control the LogWindow from other views. 
	/// </summary>
	public interface ILogView
	{
		void SelectLogLine(int lineNumber);

		void SelectAndEnsureVisible(int line, bool triggerSyncCall);

		void RefreshLogView();

		ILogLineColumnizer CurrentColumnizer { get; }

		void DeleteBookmarks(List<int> lineNumList);

		string FileName { get; }
	}
}