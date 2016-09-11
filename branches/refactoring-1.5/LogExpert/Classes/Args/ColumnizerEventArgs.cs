using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class ColumnizerEventArgs : EventArgs
	{
		public ColumnizerEventArgs(ILogLineColumnizer columnizer)
		{
			this.Columnizer = columnizer;
		}

		public ILogLineColumnizer Columnizer { get; private set; }
	}
}