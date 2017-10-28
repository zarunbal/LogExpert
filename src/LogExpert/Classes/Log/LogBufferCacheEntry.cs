using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
    internal class LogBufferCacheEntry
    {
        #region Fields

        #endregion

        #region cTor

        public LogBufferCacheEntry()
        {
            Touch();
        }

        #endregion

        #region Properties

        internal LogBuffer LogBuffer { get; set; }

        public long LastUseTimeStamp { get; private set; }

        #endregion

        #region Public methods

        public void Touch()
        {
            LastUseTimeStamp = (long) (Environment.TickCount & int.MaxValue);
        }

        #endregion
    }
}