using LogExpert.Entities;

namespace LogExpert.Interface
{
    /// <summary>
    /// To be implemented by the bookmark window. Will be informed from LogWindow about changes in bookmarks. 
    /// </summary>
    internal interface IBookmarkView
    {
        #region Properties

        bool LineColumnVisible { set; }

        #endregion

        #region Public methods

        void UpdateView();
        void BookmarkTextChanged(Bookmark bookmark);
        void SelectBookmark(int lineNum);
        void SetBookmarkData(IBookmarkData bookmarkData);

        #endregion
    }
}