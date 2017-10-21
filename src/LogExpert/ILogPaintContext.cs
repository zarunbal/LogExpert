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
        #region Properties

        Font MonospacedFont { get; } // Font font = new Font("Courier New", this.Preferences.fontSize, FontStyle.Bold);
        Font NormalFont { get; }
        Font BoldFont { get; }
        Color BookmarkColor { get; }

        #endregion

        #region Public methods

        ILogLine GetLogLine(int lineNum);

        IColumn GetCellValue(int rowIndex, int columnIndex);

        Bookmark GetBookmarkForLine(int lineNum);

        HilightEntry FindHilightEntry(ITextValue line, bool noWordMatches);

        IList<HilightMatchEntry> FindHilightMatches(ITextValue line);

        #endregion
    }
}