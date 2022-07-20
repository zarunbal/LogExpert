using System;
using LogExpert.Interface;

namespace LogExpert.Classes.Bookmark
{
    internal class BookmarkView : IBookmarkView
    {
        #region Fields

        #endregion


        #region IBookmarkView Member

        public void UpdateView()
        {
            throw new NotImplementedException();
        }

        public void BookmarkTextChanged(Entities.Bookmark bookmark)
        {
            throw new NotImplementedException();
        }

        public void SelectBookmark(int lineNum)
        {
            throw new NotImplementedException();
        }

        public bool LineColumnVisible
        {
            set { throw new NotImplementedException(); }
        }

        public bool IsActive { get; set; } = false;

        public void SetBookmarkData(IBookmarkData bookmarkData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}