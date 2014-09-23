using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class StatusLineEventArgs : EventArgs
	{
		string statusText = null;
		int currentLineNum = 0;
		int lineCount = 0;
		long fileSize = 0;

		public StatusLineEventArgs Clone()
		{
			StatusLineEventArgs e = new StatusLineEventArgs();
			e.StatusText = StatusText;
			e.CurrentLineNum = CurrentLineNum;
			e.LineCount = LineCount;
			e.FileSize = FileSize;
			return e;
		}

		public long FileSize
		{
			get
			{
				return fileSize;
			}
			set
			{
				fileSize = value;
			}
		}

		public string StatusText
		{
			get
			{
				return statusText;
			}
			set
			{
				statusText = value;
			}
		}

		public int LineCount
		{
			get
			{
				return lineCount;
			}
			set
			{
				lineCount = value;
			}
		}

		public int CurrentLineNum
		{
			get
			{
				return currentLineNum;
			}
			set
			{
				currentLineNum = value;
			}
		}
	}
}