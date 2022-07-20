namespace LogExpert.Entities.EventArgs
{
    public class ProgressEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region Properties

        public int Value { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public bool Visible { get; set; }

        #endregion
    }
}