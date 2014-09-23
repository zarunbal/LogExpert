using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace LogExpert
{
	public class BookmarkCollection : ReadOnlyCollection<Bookmark>
	{
		private SortedList<int, Bookmark> bookmarkList;

		internal BookmarkCollection(SortedList<int, Bookmark> bookmarkList)
			: base(bookmarkList.Values)
		{
			this.bookmarkList = bookmarkList;
		}
	}
}