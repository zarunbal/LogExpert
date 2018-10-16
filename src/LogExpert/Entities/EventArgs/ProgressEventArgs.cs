using System;

namespace LogExpert
{
    public class ProgressEventArgs : EventArgs
    {
        #region Properties / Indexers

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        public int Value { get; set; }

        public bool Visible { get; set; }

        #endregion
    }
}
