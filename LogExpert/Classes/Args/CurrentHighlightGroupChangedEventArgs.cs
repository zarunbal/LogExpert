using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class CurrentHighlightGroupChangedEventArgs
	{
		public CurrentHighlightGroupChangedEventArgs(LogWindow logWindow, HilightGroup currentGroup)
		{
			this.LogWindow = logWindow;
			this.CurrentGroup = currentGroup;
		}

		public LogWindow LogWindow { get; private set; }

		public HilightGroup CurrentGroup { get; private set; }
	}
}