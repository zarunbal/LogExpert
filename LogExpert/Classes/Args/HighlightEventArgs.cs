using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class HighlightEventArgs : EventArgs
	{
		public HighlightEventArgs(int startLine, int count)
		{
			this.StartLine = startLine;
			this.Count = count;
		}

		public int StartLine { get; private set; }

		public int Count { get; private set; }
	}
}