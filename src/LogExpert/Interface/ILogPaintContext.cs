using System.Collections.Generic;
using System.Drawing;

namespace LogExpert
{
    /// <summary>
    ///     Declares methods that are needed for drawing log lines. Used by PaintHelper.
    /// </summary>
    public interface ILogPaintContext
    {
        #region Properties / Indexers

        Font BoldFont { get; }
        Color BookmarkColor { get; }

        Font MonospacedFont { get; } // Font font = new Font("Courier New", this.Preferences.fontSize, FontStyle.Bold);
        Font NormalFont { get; }

        #endregion

        #region Public Methods

        HilightEntry FindHilightEntry(ITextValue line, bool noWordMatches);

        IList<HilightMatchEntry> FindHilightMatches(ITextValue line);

        Bookmark GetBookmarkForLine(int lineNum);

        IColumn GetCellValue(int rowIndex, int columnIndex);

        ILogLine GetLogLine(int lineNum);

        #endregion
    }
}
