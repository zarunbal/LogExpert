using System;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
//using System.Linq;

namespace LogExpert.Classes.Highlight
{
    [Serializable]
    public class HilightEntry
    {
        #region Fields

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

        [NonSerialized] private Regex regex = null;

        private string searchText = "";

        #endregion

        #region cTor

        [JsonConstructor]
        public HilightEntry()
        {

        }

        public HilightEntry(string searchText, Color fgColor, Color bgColor, bool isWordMatch)
        {
            this.searchText = searchText;
            this.fgColor = fgColor;
            this.bgColor = bgColor;
            this.isRegEx = false;
            this.isCaseSensitive = false;
            this.isLedSwitch = false;
            this.isStopTail = false;
            this.isSetBookmark = false;
            this.isActionEntry = false;
            this.actionEntry = null;
            this.IsWordMatch = isWordMatch;
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
            this.IsWordMatch = isWordMatch;
        }

        #endregion

        #region Properties

        public bool IsStopTail
        {
            get { return isStopTail; }
            set { isStopTail = value; }
        }

        public bool IsSetBookmark
        {
            get { return isSetBookmark; }
            set { isSetBookmark = value; }
        }

        public bool IsRegEx
        {
            get { return isRegEx; }
            set { isRegEx = value; }
        }

        public bool IsCaseSensitive
        {
            get { return isCaseSensitive; }
            set
            {
                isCaseSensitive = value;
                this.regex = null;
            }
        }


        public Color ForegroundColor
        {
            get { return this.fgColor; }
            set { this.fgColor = value; }
        }

        public Color BackgroundColor
        {
            get { return this.bgColor; }
            set { this.bgColor = value; }
        }

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                this.regex = null;
            }
        }

        public bool IsLedSwitch
        {
            get { return this.isLedSwitch; }
            set { this.isLedSwitch = value; }
        }

        public ActionEntry ActionEntry
        {
            get { return this.actionEntry; }
            set { this.actionEntry = value; }
        }

        public bool IsActionEntry
        {
            get { return this.isActionEntry; }
            set { this.isActionEntry = value; }
        }

        public string BookmarkComment
        {
            get { return this.bookmarkComment; }
            set { this.bookmarkComment = value; }
        }

        public Regex Regex
        {
            get
            {
                if (this.regex == null)
                {
                    if (this.IsRegEx)
                    {
                        this.regex = new Regex(this.SearchText,
                            this.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        this.regex = new Regex(Regex.Escape(this.SearchText),
                            this.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    }
                }
                return this.regex;
            }
        }

        public bool IsWordMatch
        {
            get { return this.isWordMatch; }
            set { this.isWordMatch = value; }
        }

        public bool IsSearchHit
        {
            get { return this.isSearchHit; }
            set { this.isSearchHit = value; }
        }

        public bool IsBold
        {
            get { return this.isBold; }
            set { this.isBold = value; }
        }

        public bool NoBackground
        {
            get { return noBackground; }
            set { noBackground = value; }
        }

        #endregion
    }
}