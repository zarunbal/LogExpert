using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class FilterListChangedEventArgs
	{
		public FilterListChangedEventArgs(LogWindow logWindow)
		{
			this.LogWindow = logWindow;
		}

		public LogWindow LogWindow { get; private set; }
	}
}