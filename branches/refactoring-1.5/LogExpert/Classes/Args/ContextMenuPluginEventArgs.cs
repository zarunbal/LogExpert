using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class ContextMenuPluginEventArgs : EventArgs
	{
		public IContextMenuEntry Entry { get; private set; }

		public IList<int> LogLines { get; private set; }

		public ILogLineColumnizer Columnizer { get; private set; }

		public ILogExpertCallback Callback { get; private set; }

		public ContextMenuPluginEventArgs(IContextMenuEntry entry, IList<int> logLines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
		{
			this.Entry = entry;
			this.LogLines = logLines;
			this.Columnizer = columnizer;
			this.Callback = callback;
		}
	}
}