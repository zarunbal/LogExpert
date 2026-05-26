using System.Collections.ObjectModel;
using System.Drawing;
using System.Text.RegularExpressions;

using ColumnizerLib;

using LogExpert.Core.Classes.JsonConverters;
using LogExpert.Core.Helpers;

using Newtonsoft.Json;

namespace LogExpert.Core.Classes.Filter;

// TODO: Convert LastLine to ReadOnlyMemory<char> as part of memory optimization effort
// This will eliminate string allocations in TestFilterCondition and improve performance.
// Will require updating all callers that currently expect string type.
// Related to: ReadOnlyMemory migration in columnizers
[Serializable]
public class FilterParams : ICloneable
{
    #region Fields
    //public List<string> historyList = new List<string>();
    //public List<string> rangeHistoryList = new List<string>();

    #endregion

    #region Properties

    public string SearchText { get; set; }

    public string RangeSearchText { get; set; }

    //public bool SpreadEnabled => SpreadBefore > 0 || SpreadBehind > 0;

    public bool IsCaseSensitive { get; set; }

    public bool IsFilterTail { get; set; }

    public int FuzzyValue { get; set; }

    public bool EmptyColumnUsePrev { get; set; }

    public bool EmptyColumnHit { get; set; }

    public bool ExactColumnMatch { get; set; }

    public bool ColumnRestrict { get; set; }

    public Color Color { get; set; } = Color.Black;

    public int SpreadBefore { get; set; }

    public int SpreadBehind { get; set; }

    public bool IsInvert { get; set; }

    public bool IsRangeSearch { get; set; }

    public bool IsRegex { get; set; }

    // list of columns in which to search
    public Collection<int> ColumnList { get; } = [];

    [JsonConverter(typeof(ColumnizerJsonConverter))]
    public ILogLineMemoryColumnizer CurrentColumnizer { get; set; }

    /// <summary>
    /// false=looking for start
    /// true=looking for end
    /// </summary>
    [JsonIgnore]
    [field: NonSerialized]
    public bool IsInRange { get; set; }

    [JsonIgnore]
    [field: NonSerialized]
    public string LastLine { get; set; } = string.Empty;

    [JsonIgnore]
    [field: NonSerialized]
    public Dictionary<int, ReadOnlyMemory<char>> LastNonEmptyCols { get; set; } = [];

    [JsonIgnore]
    [field: NonSerialized]
    public bool LastResult { get; set; }

    ///Returns RangeSearchText.ToUpperInvariant
    [JsonIgnore]
    internal string NormalizedRangeSearchText => RangeSearchText?.ToUpperInvariant();

    ///Returns SearchText.ToUpperInvariant
    [JsonIgnore]
    internal string NormalizedSearchText => SearchText?.ToUpperInvariant();

    [JsonIgnore]
    [field: NonSerialized]
    public Regex RangeRex { get; set; }

    [JsonIgnore]
    [field: NonSerialized]
    public Regex Regex { get; set; }

    #endregion

    #region Public methods

    /// <summary>
    /// Returns a new FilterParams object with the current columnizer set to the one used in this object.
    /// </summary>
    /// <returns></returns>
    public FilterParams CloneWithCurrentColumnizer ()
    {
        var newParams = Clone();
        newParams.Init();
        // removed cloning of columnizer for filtering, because this causes issues with columnizers that hold internal states (like CsvColumnizer)
        // newParams.currentColumnizer = Util.CloneColumnizer(this.currentColumnizer);
        newParams.CurrentColumnizer = CurrentColumnizer;
        return newParams;
    }

    // call after deserialization!
    public void Init ()
    {
        LastNonEmptyCols = [];
        LastLine = string.Empty;
    }

    // Reset before a new search
    public void Reset ()
    {
        LastNonEmptyCols.Clear();
        IsInRange = false;
    }

    public void CreateRegex ()
    {
        if (SearchText != null)
        {
            Regex = RegexHelper.GetOrCreateCached(SearchText, IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
        }

        if (RangeSearchText != null && IsRangeSearch)
        {
            RangeRex = RegexHelper.GetOrCreateCached(RangeSearchText, IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
        }
    }

    /// <summary>
    /// Shallow Copy
    /// </summary>
    /// <returns></returns>
    public FilterParams Clone ()
    {
        return (FilterParams)MemberwiseClone();
    }

    /// <summary>
    /// Shallow Copy
    /// </summary>
    /// <returns></returns>
    object ICloneable.Clone ()
    {
        return Clone();
    }

    #endregion
}