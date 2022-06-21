using LogExpert.Entities;

namespace LogExpert.Interface
{
    public interface IBookmarkData
    {
        #region Properties

        BookmarkCollection Bookmarks { get; }

        #endregion

        #region Public methods

        void ToggleBookmark(int lineNum);
        bool IsBookmarkAtLine(int lineNum);
        int GetBookmarkIndexForLine(int lineNum);
        Bookmark GetBookmarkForLine(int lineNum);

        #endregion
    }
}