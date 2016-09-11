using LogExpert.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert.Interfaces
{
	public interface ILogWindowSearch
	{
		PatternArgs PatternArgs { get; set; }

		ProgressEventArgs ProgressEventArgs { get; }
		bool IsSearching { get; set; }

		bool ShouldCancel { get; set; }

		void SendProgressBarUpdate();
		void UpdateProgressBar(int value);
		PatternWindow PatternWindow { get; }
		BufferedDataGridView DataGridView { get; }
		ILogLineColumnizer CurrentColumnizer { get; }
		int LineCount { get; }

		//HACK Zarunbal: remove this
		LogWindow CurrentLogWindows { get; }

		string GetLogLine(int i);
	}
}
