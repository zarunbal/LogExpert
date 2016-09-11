using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	/// <summary>
	/// To be implemented by the bookmark window. Will be informed from LogWindow about changes in bookmarks. 
	/// </summary>
	interface IBookmarkView
	{
		void UpdateView();

		void BookmarkTextChanged(Bookmark bookmark);

		void SelectBookmark(int lineNum);

		bool LineColumnVisible { set; }

		void SetBookmarkData(IBookmarkData bookmarkData);
	}
}