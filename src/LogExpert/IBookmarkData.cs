using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace LogExpert
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