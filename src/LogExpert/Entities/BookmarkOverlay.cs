using System.Drawing;

namespace LogExpert.Entities
{
    public class BookmarkOverlay
    {
        #region Fields

        #endregion

        #region Properties

        public Bookmark Bookmark { get; set; }

        public Point Position { get; set; }

        public Rectangle BubbleRect { get; set; }

        #endregion
    }
}