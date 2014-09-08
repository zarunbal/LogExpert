using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace LogExpert
{
  [Serializable]
  public class ActionEntry
  {
    public string pluginName;
    public string actionParam;
    public ActionEntry Copy()
    {
      ActionEntry e = new ActionEntry();
      e.pluginName = this.pluginName;
      e.actionParam = this.actionParam;
      return e;
    }
  }

  [Serializable]
  public class HilightEntry
  {
    private string searchText = "";
    private Color fgColor;
    private Color bgColor;
    private bool isRegEx;
    private bool isCaseSensitive;
    private bool isLedSwitch;
    private bool isStopTail;
    private bool isSetBookmark;
    private bool isActionEntry;
    private ActionEntry actionEntry;
    private string bookmarkComment;
    private bool isWordMatch;
    private bool isBold;
    private bool noBackground;

    [NonSerialized]
    private Regex regex = null;

    [NonSerialized]
    private bool isSearchHit;   // highlightes search result


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
      set { isCaseSensitive = value; this.regex = null; }
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
      set { this.searchText = value; this.regex = null; }
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
            this.regex = new Regex(this.SearchText, this.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
          }
          else
          {
            this.regex = new Regex(Regex.Escape(this.SearchText), this.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
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
  }

  /// <summary>
  /// Class for storing word-wise hilight matches. Used for colouring different matches on one line.
  /// </summary>
  public class HilightMatchEntry
  {
    private HilightEntry hilightEntry;
    private int startPos;
    private int length;

    public HilightEntry HilightEntry
    {
      get { return this.hilightEntry; }
      set { this.hilightEntry = value; }
    }

    public int StartPos
    {
      get { return this.startPos; }
      set { this.startPos = value; }
    }

    public int Length
    {
      get { return this.length; }
      set { this.length = value; }
    }

    public override String ToString()
    {
      return this.HilightEntry.SearchText + "/" + this.StartPos + "/" + this.Length;
    }
  }

}
