namespace LogExpert.Entities.EventArgs
{
    public class StatusLineEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region Properties

        public long FileSize { get; set; } = 0;

        public string StatusText { get; set; } = null;

        public int LineCount { get; set; } = 0;

        public int CurrentLineNum { get; set; } = 0;

        #endregion

        #region Public methods

        public StatusLineEventArgs Clone()
        {
            StatusLineEventArgs e = new StatusLineEventArgs();
            e.StatusText = StatusText;
            e.CurrentLineNum = CurrentLineNum;
            e.LineCount = LineCount;
            e.FileSize = FileSize;
            return e;
        }

        #endregion
    }
}