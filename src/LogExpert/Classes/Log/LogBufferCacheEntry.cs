using System;

namespace LogExpert
{
    internal class LogBufferCacheEntry
    {
        #region Ctor

        public LogBufferCacheEntry()
        {
            Touch();
        }

        #endregion

        #region Properties / Indexers

        public long LastUseTimeStamp { get; private set; }

        internal LogBuffer LogBuffer { get; set; }

        #endregion

        #region Public Methods

        public void Touch()
        {
            LastUseTimeStamp = Environment.TickCount & int.MaxValue;
        }

        #endregion
    }
}
