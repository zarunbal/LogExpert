using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class LogEventArgs : EventArgs 
	{
		private bool isRollover = false;
		private int rolloverOffset = 0;

		public int RolloverOffset
		{
			get
			{
				return rolloverOffset;
			}
			set
			{
				rolloverOffset = value;
			}
		}

		public bool IsRollover
		{
			get
			{
				return isRollover;
			}
			set
			{
				isRollover = value;
			}
		}

		public long FileSize { get; set; }

		public int LineCount { get; set; }

		public int PrevLineCount { get; set; }

		public long PrevFileSize { get; set; }
	}
}