namespace LogExpert.Entities
{
    internal class Range
    {
        #region Fields

        #endregion

        #region cTor

        public Range()
        {
        }

        public Range(int startLine, int endLine)
        {
            this.StartLine = startLine;
            this.EndLine = endLine;
        }

        #endregion

        #region Properties

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        #endregion
    }
}