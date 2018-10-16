using System;
using System.Collections.Generic;

namespace LogExpert
{
    [Serializable]
    public class HilightGroup
    {
        #region Private Fields

        private string groupName = string.Empty;
        private List<HilightEntry> hilightEntryList = new List<HilightEntry>();

        #endregion

        #region Properties / Indexers

        public string GroupName
        {
            get => groupName;
            set => groupName = value;
        }

        public List<HilightEntry> HilightEntryList
        {
            get => hilightEntryList;
            set => hilightEntryList = value;
        }

        #endregion
    }
}
