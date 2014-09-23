using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class RowHeightEntry
	{
		public RowHeightEntry()
		{
			LineNum = 0;
			Height = 0;
		}

		public RowHeightEntry(int lineNum, int height)
		{
			LineNum = lineNum;
			Height = height;
		}

		public int LineNum { get; set; }

		public int Height { get; set; }
	}
}