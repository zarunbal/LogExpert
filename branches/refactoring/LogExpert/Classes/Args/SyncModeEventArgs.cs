using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class SyncModeEventArgs : EventArgs
	{
		public SyncModeEventArgs(bool isSynced)
		{
			this.IsTimeSynced = isSynced;
		}

		public bool IsTimeSynced { get; private set; }
	}
}