namespace LogExpert
{
    /// <summary>
    ///     Class for storing word-wise hilight matches. Used for colouring different matches on one line.
    /// </summary>
    public class HilightMatchEntry
    {
        #region Properties / Indexers

        public HilightEntry HilightEntry { get; set; }

        public int Length { get; set; }

        public int StartPos { get; set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return HilightEntry.SearchText + "/" + StartPos + "/" + Length;
        }

        #endregion
    }
}
