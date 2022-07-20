namespace LogExpert.Entities.EventArgs
{
    public class PatternArgs
    {
        #region Fields

        public int endLine = 0;
        public int fuzzy = 6;
        public int maxDiffInBlock = 5;
        public int maxMisses = 5;
        public int minWeight = 15;
        public int startLine = 0;

        #endregion
    }
}