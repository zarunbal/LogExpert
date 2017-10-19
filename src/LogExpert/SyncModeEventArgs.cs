using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class SyncModeEventArgs : EventArgs
  {
    private bool isTimeSynced;

    public SyncModeEventArgs(bool isSynced)
    {
      this.isTimeSynced = isSynced;
    }


    public bool IsTimeSynced
    {
      get { return this.isTimeSynced; }
    }
  }
}
