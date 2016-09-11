using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class GuiStateArgs : EventArgs
  {
    bool timeshiftEnabled;

    public bool TimeshiftEnabled
    {
      get { return timeshiftEnabled; }
      set { timeshiftEnabled = value; }
    }
    bool followTail;

    public bool FollowTail
    {
      get { return followTail; }
      set { followTail = value; }
    }

    bool menuEnabled = true;

    public bool MenuEnabled
    {
      get { return menuEnabled; }
      set { menuEnabled = value; }
    }

    string timeshiftText;

    public string TimeshiftText
    {
      get { return timeshiftText; }
      set { timeshiftText = value; }
    }

    bool timeshiftPossible = false;

    public bool TimeshiftPossible
    {
      get { return timeshiftPossible; }
      set { timeshiftPossible = value; }
    }

    bool multiFileEnabled = true;

    public bool MultiFileEnabled
    {
      get { return multiFileEnabled; }
      set { multiFileEnabled = value; }
    }

    bool filterEnabled = true;

    public bool FilterEnabled
    {
      get {return this.filterEnabled;}
      set { this.filterEnabled = value; }
    }

    bool cellSelectMode = false;
    public bool CellSelectMode
    {
      get { return this.cellSelectMode; }
      set { this.cellSelectMode = value; }
    }

    Encoding encoding;
    public Encoding CurrentEncoding
    {
      get { return this.encoding; }
      set { this.encoding = value; }
    }

    DateTime timestamp;
    public DateTime Timestamp
    {
      get { return this.timestamp; }
      set { this.timestamp = value; }
    }

    DateTime minTimestamp;

    public DateTime MinTimestamp
    {
      get { return minTimestamp; }
      set { minTimestamp = value; }
    }

    DateTime maxTimestamp;

    public DateTime MaxTimestamp
    {
      get { return maxTimestamp; }
      set { maxTimestamp = value; }
    }

    bool showBookmarkBubbles;

    public bool ShowBookmarkBubbles
    {
      get { return showBookmarkBubbles; }
      set { showBookmarkBubbles = value; }
    }

    private bool isMultiFileActive;

    public bool IsMultiFileActive
    {
      get { return isMultiFileActive; }
      set { isMultiFileActive = value; }
    }

    public bool ShowHiddenLines
    {
      get { return this.showHiddenLines; }
      set { this.showHiddenLines = value; }
    }

    public string HighlightGroupName
    {
      get { return this.highlightGroupName; }
      set { this.highlightGroupName = value; }
    }

    public bool ColumnFinderVisible
    {
      get { return columnFinderVisible; }
      set { columnFinderVisible = value; }
    }

    private bool showHiddenLines = true;

    private string highlightGroupName;

    private bool columnFinderVisible;
  }
}
