using System.Collections.Generic;

namespace LogExpert
{
    public class HighlightResults
    {
        #region Properties / Indexers

        public IList<HilightEntry> HighlightEntryList { get; set; } = new List<HilightEntry>();

        #endregion
    }
}
