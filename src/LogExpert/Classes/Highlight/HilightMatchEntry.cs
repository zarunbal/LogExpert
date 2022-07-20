namespace LogExpert.Classes.Highlight
{
    /// <summary>
    /// Class for storing word-wise hilight matches. Used for colouring different matches on one line.
    /// </summary>
    public class HilightMatchEntry
    {
        #region Fields

        #endregion

        #region Properties

        public HilightEntry HilightEntry { get; set; }

        public int StartPos { get; set; }

        public int Length { get; set; }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.HilightEntry.SearchText + "/" + this.StartPos + "/" + this.Length;
        }

        #endregion
    }
}