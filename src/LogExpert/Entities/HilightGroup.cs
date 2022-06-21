using System;
using System.Collections.Generic;
using LogExpert.Classes.Highlight;

namespace LogExpert.Entities
{
    [Serializable]
    public class HilightGroup
    {
        #region Fields

        private string groupName = "";
        private List<HilightEntry> hilightEntryList = new List<HilightEntry>();

        #endregion

        #region Properties

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