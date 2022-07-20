namespace LogExpert.Entities.EventArgs
{
    public class BookmarkEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public BookmarkEventArgs(Bookmark bookmark)
        {
            this.Bookmark = bookmark;
        }

        #endregion

        #region Properties

        public Bookmark Bookmark { get; }

        #endregion
    }
}