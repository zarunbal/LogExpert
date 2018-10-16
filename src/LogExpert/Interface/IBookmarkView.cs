namespace LogExpert
{
    /// <summary>
    ///     To be implemented by the bookmark window. Will be informed from LogWindow about changes in bookmarks.
    /// </summary>
    internal interface IBookmarkView
    {
        #region Properties / Indexers

        bool LineColumnVisible { set; }

        #endregion

        #region Public Methods

        void BookmarkTextChanged(Bookmark bookmark);
        void SelectBookmark(int lineNum);
        void SetBookmarkData(IBookmarkData bookmarkData);

        void UpdateView();

        #endregion
    }
}
