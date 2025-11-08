using System.Drawing;

using Newtonsoft.Json;

namespace LogExpert.Core.Entities;

[Serializable]
public class Bookmark
{
    #region cTor

    [JsonConstructor]
    public Bookmark () { }

    public Bookmark (int lineNum)
    {
        LineNum = lineNum;
        Text = string.Empty;
        Overlay = new BookmarkOverlay();
    }

    public Bookmark (int lineNum, string comment)
    {
        LineNum = lineNum;
        Text = comment;
        Overlay = new BookmarkOverlay();
    }

    #endregion

    #region Properties

    public int LineNum { get; set; }

    public string Text { get; set; }

    public BookmarkOverlay Overlay { get; set; }

    /// <summary>
    /// Position offset of the overlay as set by the user by dragging the overlay with the mouse.
    /// </summary>
    public Size OverlayOffset { get; set; }

    #endregion
}