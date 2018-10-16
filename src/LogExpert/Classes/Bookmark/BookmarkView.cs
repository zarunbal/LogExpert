using System;

namespace LogExpert
{
    internal class BookmarkView : IBookmarkView
    {
        #region Interface IBookmarkView

        public bool LineColumnVisible
        {
            set => throw new NotImplementedException();
        }

        public void BookmarkTextChanged(Bookmark bookmark)
        {
            throw new NotImplementedException();
        }

        public void SelectBookmark(int lineNum)
        {
            throw new NotImplementedException();
        }

        public void SetBookmarkData(IBookmarkData bookmarkData)
        {
            throw new NotImplementedException();
        }

        public void UpdateView()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties / Indexers

        public bool IsActive { get; set; } = false;

        #endregion
    }
}
