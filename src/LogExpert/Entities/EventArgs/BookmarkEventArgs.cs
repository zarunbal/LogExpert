using System;

namespace LogExpert
{
    public class BookmarkEventArgs : EventArgs
    {
        #region Ctor

        public BookmarkEventArgs(Bookmark bookmark)
        {
            Bookmark = bookmark;
        }

        #endregion

        #region Properties / Indexers

        public Bookmark Bookmark { get; }

        #endregion
    }
}
