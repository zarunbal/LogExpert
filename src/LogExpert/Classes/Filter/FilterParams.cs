using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace LogExpert.Classes.Filter
{
    [Serializable]
    public class FilterParams
    {
        #region Fields

        public string _rangeSearchText = "";
        public string _searchText = "";

        public Color color = Color.Black;

        //public List<string> historyList = new List<string>();
        //public List<string> rangeHistoryList = new List<string>();
        public List<int> columnList = new List<int>(); // list of columns in which to search

        public bool columnRestrict = false;

        [NonSerialized] public ILogLineColumnizer currentColumnizer;

        public bool emptyColumnHit;
        public bool emptyColumnUsePrev;

        public bool exactColumnMatch = false;

        //public bool isFuzzy;
        public int fuzzyValue = 0;

        public bool isCaseSensitive;
        public bool isFilterTail;

        [NonSerialized] public bool isInRange = false; // false=looking for start, true=looking for end

        public bool isInvert;
        public bool isRangeSearch = false;
        public bool isRegex;

        [NonSerialized] public string lastLine = "";

        [NonSerialized] public Hashtable lastNonEmptyCols = new Hashtable();

        [NonSerialized] public bool lastResult;

        [NonSerialized] public string lowerRangeSearchText = "";

        // transient members:
        [NonSerialized] public string lowerSearchText = "";

        [NonSerialized] public Regex rangeRex;

        [NonSerialized] public Regex rex;

        public int spreadBefore;
        public int spreadBehind;

        #endregion

        #region Properties

        public string searchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                lowerSearchText = _searchText.ToLower();
            }
        }

        public string rangeSearchText
        {
            get => _rangeSearchText;
            set
            {
                _rangeSearchText = value;
                lowerRangeSearchText = _rangeSearchText.ToLower();
            }
        }

        public bool SpreadEnabled => spreadBefore > 0 || spreadBehind > 0;

        #endregion

        #region Public methods

        public FilterParams CreateCopy2()
        {
            FilterParams newParams = CreateCopy();
            newParams.Init();
            // removed cloning of columnizer for filtering, because this causes issues with columnizers that hold internal states (like CsvColumnizer)
            // newParams.currentColumnizer = Util.CloneColumnizer(this.currentColumnizer);
            newParams.currentColumnizer = currentColumnizer;
            return newParams;
        }

        // call after deserialization!
        public void Init()
        {
            lastNonEmptyCols = new Hashtable();
            lowerRangeSearchText = _rangeSearchText.ToLower();
            lowerSearchText = _searchText.ToLower();
            lastLine = "";
        }

        // Reset before a new search
        public void Reset()
        {
            lastNonEmptyCols.Clear();
            isInRange = false;
        }

        public void CreateRegex()
        {
            if (_searchText != null)
            {
                rex = new Regex(_searchText,
                    isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
            if (_rangeSearchText != null && isRangeSearch)
            {
                rangeRex = new Regex(_rangeSearchText,
                    isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
        }

        public FilterParams CreateCopy()
        {
            return (FilterParams) MemberwiseClone();
        }

        #endregion
    }
}