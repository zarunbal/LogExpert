using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ColumnizerLib;

namespace LogExpert
{
  /// <summary>
  /// Declares methods that are needed for drawing log lines. Used by PaintHelper.
  /// </summary>
  public interface ILogPaintContext
  {
      ILogLine GetLogLine(int lineNum);

    string GetCellValue(int rowIndex, int columnIndex);

    Bookmark GetBookmarkForLine(int lineNum);

    HilightEntry FindHilightEntry(ILogLine line, bool noWordMatches);

    IList<HilightMatchEntry> FindHilightMatches(ILogLine line);

    Font MonospacedFont { get; }   // Font font = new Font("Courier New", this.Preferences.fontSize, FontStyle.Bold);
    Font NormalFont { get; }
    Font BoldFont { get; }
    Color BookmarkColor { get; }
    
  }
}
