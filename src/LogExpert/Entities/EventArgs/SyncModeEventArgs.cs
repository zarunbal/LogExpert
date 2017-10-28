using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class SyncModeEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public SyncModeEventArgs(bool isSynced)
        {
            this.IsTimeSynced = isSynced;
        }

        #endregion

        #region Properties

        public bool IsTimeSynced { get; }

        #endregion
    }
}