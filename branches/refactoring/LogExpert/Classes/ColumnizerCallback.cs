using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
	public class ColumnizerCallback : ILogLineColumnizerCallback
	{
		protected LogWindow _logWindow;

		public int LineNum { get; set; }

		public ColumnizerCallback(LogWindow logWindow)
		{
			_logWindow = logWindow;
		}

		private ColumnizerCallback(ColumnizerCallback original)
		{
			_logWindow = original._logWindow;
			LineNum = original.LineNum;
		}

		public ColumnizerCallback createCopy()
		{
			return new ColumnizerCallback(this);
		}

		public int GetLineNum()
		{
			return LineNum;
		}

		public string GetFileName()
		{
			return _logWindow.GetCurrentFileName(LineNum);
		}

		public string GetLogLine(int lineNum)
		{
			return _logWindow.GetLine(lineNum);
		}

		public int GetLineCount()
		{
			return _logWindow.CurrentLogFileReader.LineCount;
		}
	}
}
