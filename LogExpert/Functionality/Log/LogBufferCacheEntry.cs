using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
	internal class LogBufferCacheEntry
	{
		public LogBufferCacheEntry()
		{
			Touch();
		}

		public void Touch()
		{
			LastUseTimeStamp = (Environment.TickCount & Int32.MaxValue);
		}

		internal LogBuffer LogBuffer { get; set; }

		public long LastUseTimeStamp { get; private set; }
	}
}