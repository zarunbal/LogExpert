namespace LogExpert
{
    internal class Range
    {
        #region Ctor

        public Range()
        {
        }

        public Range(int startLine, int endLine)
        {
            StartLine = startLine;
            EndLine = endLine;
        }

        #endregion

        #region Properties / Indexers

        public int EndLine { get; set; }

        public int StartLine { get; set; }

        #endregion
    }
}
