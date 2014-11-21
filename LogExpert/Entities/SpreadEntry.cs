using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
	public class SpreadEntry
	{
		public SpreadEntry(int lineNum, int diff, DateTime timestamp)
		{
			LineNum = lineNum;
			Diff = diff;
			Timestamp = timestamp;
		}

		public int LineNum { get; set; }

		public int Value { get; set; }

		public int Diff { get; set; }

		public DateTime Timestamp { get; set; }
	}
}