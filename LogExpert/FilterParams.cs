using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Drawing;

namespace LogExpert
{
  [Serializable]
  public class FilterParams
  {
    public bool isRegex;
    public bool isCaseSensitive;
    public string _searchText = "";
    public string _rangeSearchText = "";
    public bool isFilterTail;
    public int spreadBefore;
    public int spreadBehind;
    public bool isInvert;
    //public bool isFuzzy;
    public int fuzzyValue = 0;
    public bool isRangeSearch = false;
    //public List<string> historyList = new List<string>();
    //public List<string> rangeHistoryList = new List<string>();
    public List<int> columnList = new List<int>();    // list of columns in which to search
    public bool emptyColumnUsePrev;
    public bool emptyColumnHit;
    public bool columnRestrict = false;
    public bool exactColumnMatch = false;
    public Color color = Color.Black;

    // transient members:
    [NonSerialized]
    public string lowerSearchText = "";
    [NonSerialized]
    public string lowerRangeSearchText = "";
    [NonSerialized]
    public Regex rex;
    [NonSerialized]
    public Regex rangeRex;
    [NonSerialized]
    public bool isInRange = false;    // false=looking for start, true=looking for end
    [NonSerialized]
    public string lastLine = "";
    [NonSerialized]
    public bool lastResult;
    [NonSerialized]
    public ILogLineColumnizer currentColumnizer;
    [NonSerialized]
    public Hashtable lastNonEmptyCols = new Hashtable();


    public FilterParams CreateCopy2()
    {
      FilterParams newParams = CreateCopy();
      newParams.Init();
      // removed cloning of columnizer for filtering, because this causes issues with columnizers that hold internal states (like CsvColumnizer)
      // newParams.currentColumnizer = Util.CloneColumnizer(this.currentColumnizer);
      newParams.currentColumnizer = this.currentColumnizer;
      return newParams;
    }

    // call after deserialization!
    public void Init()
    {
      this.lastNonEmptyCols = new Hashtable();
      this.lowerRangeSearchText = this._rangeSearchText.ToLower();
      this.lowerSearchText = this._searchText.ToLower();
      this.lastLine = "";
    }

    // Reset before a new search
    public void Reset()
    {
      lastNonEmptyCols.Clear();
      isInRange = false;
    }

    public void CreateRegex()
    {
      if (this._searchText != null)
        this.rex = new Regex(this._searchText, this.isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
      if (this._rangeSearchText != null && this.isRangeSearch)
        this.rangeRex = new Regex(this._rangeSearchText, this.isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
    }

    public string searchText
    {
      get { return this._searchText; }
      set { this._searchText = value; this.lowerSearchText = this._searchText.ToLower(); }
    }

    public string rangeSearchText
    {
      get { return this._rangeSearchText; }
      set { this._rangeSearchText = value; this.lowerRangeSearchText = this._rangeSearchText.ToLower(); }
    }

    public bool SpreadEnabled
    {
      get {return (this.spreadBefore > 0 || this.spreadBehind > 0);}
    }

    public FilterParams CreateCopy()
    {
      return (FilterParams)this.MemberwiseClone();
    }
  }
}
