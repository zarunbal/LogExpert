using System;

namespace LogExpert
{
    public class SyncModeEventArgs : EventArgs
    {
        #region Ctor

        public SyncModeEventArgs(bool isSynced)
        {
            IsTimeSynced = isSynced;
        }

        #endregion

        #region Properties / Indexers

        public bool IsTimeSynced { get; }

        #endregion
    }
}
