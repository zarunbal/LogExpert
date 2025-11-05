using System.Drawing;

namespace LogExpert.Core.Entities;

[Serializable]
public class BookmarkOverlay
{
    #region Properties

    public Bookmark Bookmark { get; set; }

    public Point Position { get; set; }

    public Rectangle BubbleRect { get; set; }

    #endregion
}