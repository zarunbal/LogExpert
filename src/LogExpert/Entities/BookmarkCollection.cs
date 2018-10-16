using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LogExpert
{
    public class BookmarkCollection : ReadOnlyCollection<Bookmark>
    {
        #region Private Fields

        private SortedList<int, Bookmark> bookmarkList;

        #endregion

        #region Ctor

        internal BookmarkCollection(SortedList<int, Bookmark> bookmarkList)
            : base(bookmarkList.Values)
        {
            this.bookmarkList = bookmarkList;
        }

        #endregion
    }
}
