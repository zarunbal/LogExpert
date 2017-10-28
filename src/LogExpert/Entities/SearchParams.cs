using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    [Serializable]
    public class SearchParams
    {
        #region Fields

        public int currentLine;
        public List<string> historyList = new List<string>();
        public bool isCaseSensitive = false;
        public bool isFindNext;

        public bool isForward = true;
        public bool isFromTop = false;
        public bool isRegex = false;

        [NonSerialized] public bool isShiftF3Pressed = false;

        public string searchText;

        #endregion
    }
}