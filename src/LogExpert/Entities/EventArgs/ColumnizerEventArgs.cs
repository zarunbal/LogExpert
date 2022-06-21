namespace LogExpert.Entities.EventArgs
{
    public class ColumnizerEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public ColumnizerEventArgs(ILogLineColumnizer columnizer)
        {
            this.Columnizer = columnizer;
        }

        #endregion

        #region Properties

        public ILogLineColumnizer Columnizer { get; }

        #endregion
    }
}