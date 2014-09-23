using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	class Range
	{
		public Range()
		{
		}

		public Range(int startLine, int endLine)
		{
			this.StartLine = startLine;
			this.EndLine = endLine;
		}

		public int StartLine { get; set; }

		public int EndLine { get; set; }
	}
}