namespace LogExpert.Entities.EventArgs
{
    public class SyncModeEventArgs : System.EventArgs
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