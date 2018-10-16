namespace LogExpert
{
    public interface IBookmarkData
    {
        #region Properties / Indexers

        BookmarkCollection Bookmarks { get; }

        #endregion

        #region Public Methods

        Bookmark GetBookmarkForLine(int lineNum);
        int GetBookmarkIndexForLine(int lineNum);
        bool IsBookmarkAtLine(int lineNum);

        void ToggleBookmark(int lineNum);

        #endregion
    }
}
