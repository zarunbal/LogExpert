using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace LogExpert
{
    public class BookmarkCollection : ReadOnlyCollection<Bookmark>
    {
        #region Fields

        private SortedList<int, Bookmark> bookmarkList;

        #endregion

        #region cTor

        internal BookmarkCollection(SortedList<int, Bookmark> bookmarkList)
            : base(bookmarkList.Values)
        {
            this.bookmarkList = bookmarkList;
        }

        #endregion
    }
}