using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class HighlightResults
    {
        #region Fields

        #endregion

        #region Properties

        public IList<HilightEntry> HighlightEntryList { get; set; } = new List<HilightEntry>();

        #endregion
    }
}