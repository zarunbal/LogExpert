using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class ProgressEventArgs : EventArgs
	{
		public int Value { get; set; }

		public int MinValue { get; set; }

		public int MaxValue { get; set; }

		public bool Visible { get; set; }
	}
}