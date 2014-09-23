using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	internal class ConfigChangedEventArgs : EventArgs
	{
		internal ConfigChangedEventArgs(SettingsFlags changeFlags)
		{
			this.Flags = changeFlags;
		}

		public SettingsFlags Flags { get; private set; }
	}
}