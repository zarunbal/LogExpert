namespace LogExpert
{
    public class RowHeightEntry
    {
        #region Ctor

        public RowHeightEntry()
        {
            LineNum = 0;
            Height = 0;
        }

        public RowHeightEntry(int lineNum, int height)
        {
            LineNum = lineNum;
            Height = height;
        }

        #endregion

        #region Properties / Indexers

        public int Height { get; set; }

        public int LineNum { get; set; }

        #endregion
    }
}
