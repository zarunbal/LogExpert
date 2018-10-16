using System;
using System.Drawing;
using System.Text.RegularExpressions;

// using System.Linq;
namespace LogExpert
{
    [Serializable]
    public class HilightEntry
    {
        #region Private Fields

        private ActionEntry actionEntry;
        private Color bgColor;
        private string bookmarkComment;
        private Color fgColor;
        private bool isActionEntry;
        private bool isBold;
        private bool isCaseSensitive;
        private bool isLedSwitch;
        private bool isRegEx;

        [NonSerialized] private bool isSearchHit; // highlightes search result

        private bool isSetBookmark;
        private bool isStopTail;
        private bool isWordMatch;
        private bool noBackground;

        [NonSerialized] private Regex regex;

        private string searchText = string.Empty;

        #endregion

        #region Ctor

        public HilightEntry(string searchText, Color fgColor, Color bgColor, bool isWordMatch)
        {
            this.searchText = searchText;
            this.fgColor = fgColor;
            this.bgColor = bgColor;
            isRegEx = false;
            isCaseSensitive = false;
            isLedSwitch = false;
            isStopTail = false;
            isSetBookmark = false;
            isActionEntry = false;
            actionEntry = null;
            IsWordMatch = isWordMatch;
        }


        public HilightEntry(string searchText, Color fgColor, Color bgColor,
                            bool isRegEx, bool isCaseSensitive, bool isLedSwitch,
                            bool isStopTail, bool isSetBookmark, bool isActionEntry, ActionEntry actionEntry, bool isWordMatch)
        {
            this.searchText = searchText;
            this.fgColor = fgColor;
            this.bgColor = bgColor;
            this.isRegEx = isRegEx;
            this.isCaseSensitive = isCaseSensitive;
            this.isLedSwitch = isLedSwitch;
            this.isStopTail = isStopTail;
            this.isSetBookmark = isSetBookmark;
            this.isActionEntry = isActionEntry;
            this.actionEntry = actionEntry;
            IsWordMatch = isWordMatch;
        }

        #endregion

        #region Properties / Indexers

        public ActionEntry ActionEntry
        {
            get => actionEntry;
            set => actionEntry = value;
        }

        public Color BackgroundColor
        {
            get => bgColor;
            set => bgColor = value;
        }

        public string BookmarkComment
        {
            get => bookmarkComment;
            set => bookmarkComment = value;
        }


        public Color ForegroundColor
        {
            get => fgColor;
            set => fgColor = value;
        }

        public bool IsActionEntry
        {
            get => isActionEntry;
            set => isActionEntry = value;
        }

        public bool IsBold
        {
            get => isBold;
            set => isBold = value;
        }

        public bool IsCaseSensitive
        {
            get => isCaseSensitive;
            set
            {
                isCaseSensitive = value;
                regex = null;
            }
        }

        public bool IsLedSwitch
        {
            get => isLedSwitch;
            set => isLedSwitch = value;
        }

        public bool IsRegEx
        {
            get => isRegEx;
            set => isRegEx = value;
        }

        public bool IsSearchHit
        {
            get => isSearchHit;
            set => isSearchHit = value;
        }

        public bool IsSetBookmark
        {
            get => isSetBookmark;
            set => isSetBookmark = value;
        }

        public bool IsStopTail
        {
            get => isStopTail;
            set => isStopTail = value;
        }

        public bool IsWordMatch
        {
            get => isWordMatch;
            set => isWordMatch = value;
        }

        public bool NoBackground
        {
            get => noBackground;
            set => noBackground = value;
        }

        public Regex Regex
        {
            get
            {
                if (regex == null)
                {
                    if (IsRegEx)
                    {
                        regex = new Regex(SearchText,
                            IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        regex = new Regex(Regex.Escape(SearchText),
                            IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    }
                }

                return regex;
            }
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                regex = null;
            }
        }

        #endregion
    }
}
