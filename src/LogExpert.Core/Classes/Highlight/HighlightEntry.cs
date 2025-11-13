using System.Drawing;
using System.Text.RegularExpressions;

using LogExpert.Core.Helpers;

using Newtonsoft.Json;

namespace LogExpert.Core.Classes.Highlight;

[Serializable]
[method: JsonConstructor]
public class HighlightEntry () : ICloneable
{
    #region Fields

    [NonSerialized] private Regex _regex = null;

    private string _searchText = string.Empty;

    #endregion Fields

    #region Properties

    public bool IsStopTail { get; set; }

    public bool IsSetBookmark { get; set; }

    public bool IsRegex { get; set; }

    public bool IsCaseSensitive { get; set; }

    public Color ForegroundColor { get; set; }

    public Color BackgroundColor { get; set; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            _regex = null;
        }
    }

    public bool IsLedSwitch { get; set; }

    public ActionEntry ActionEntry { get; set; }

    public bool IsActionEntry { get; set; }

    public string BookmarkComment { get; set; }

    public Regex Regex
    {
        get
        {
            _regex ??= IsRegex
                    ? RegexHelper.GetOrCreateCached(SearchText, IsCaseSensitive
                        ? RegexOptions.None
                        : RegexOptions.IgnoreCase)
                    : RegexHelper.GetOrCreateCached(System.Text.RegularExpressions.Regex.Escape(SearchText),
                                IsCaseSensitive
                                ? RegexOptions.None
                                : RegexOptions.IgnoreCase);

            return _regex;
        }
    }

    public bool IsWordMatch { get; set; }

    // Highlight search result
    [field: NonSerialized]
    public bool IsSearchHit { get; set; }

    public bool IsBold { get; set; }

    public bool NoBackground { get; set; }

    public object Clone ()
    {
        var highLightEntry = new HighlightEntry
        {
            SearchText = SearchText,
            ForegroundColor = ForegroundColor,
            BackgroundColor = BackgroundColor,
            IsRegex = IsRegex,
            IsCaseSensitive = IsCaseSensitive,
            IsLedSwitch = IsLedSwitch,
            IsStopTail = IsStopTail,
            IsSetBookmark = IsSetBookmark,
            IsActionEntry = IsActionEntry,
            ActionEntry = ActionEntry != null ? (ActionEntry)ActionEntry.Clone() : null,
            IsWordMatch = IsWordMatch,
            IsBold = IsBold,
            BookmarkComment = BookmarkComment,
            NoBackground = NoBackground,
            IsSearchHit = IsSearchHit
        };

        return highLightEntry;
    }

    #endregion Properties
}