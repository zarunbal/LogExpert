using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
  internal class LogBufferCacheEntry
  {
    private LogBuffer logBuffer;
    private long lastUseTimeStamp;
    public LogBufferCacheEntry()
    {
      Touch();
    }

    public void Touch()
    {
      lastUseTimeStamp = (long)(Environment.TickCount & Int32.MaxValue);
    }

    internal LogBuffer LogBuffer
    {
      get { return this.logBuffer; }
      set { this.logBuffer = value; }
    }

    public long LastUseTimeStamp
    {
      get { return this.lastUseTimeStamp; }
    }
  }
}
