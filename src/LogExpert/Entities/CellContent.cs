namespace LogExpert
{
    public class CellContent
    {
        #region Ctor

        public CellContent(string value, int x)
        {
            Value = value;
            CellPosX = x;
        }

        #endregion

        #region Properties / Indexers

        public int CellPosX { get; set; }

        public string Value { get; set; }

        #endregion
    }
}
