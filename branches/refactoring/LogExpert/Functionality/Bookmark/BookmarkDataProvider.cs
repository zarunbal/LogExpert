using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	internal class BookmarkDataProvider : IBookmarkData
	{
		#region cTor
		
		internal BookmarkDataProvider()
		{
			BookmarkList = new SortedList<int, Bookmark>();
		}
		
		internal BookmarkDataProvider(SortedList<int, Bookmark> bookmarkList)
		{
			this.BookmarkList = bookmarkList;
		}
		
		#endregion
		
		#region Properties
		
		internal SortedList<int, Bookmark> BookmarkList { get; set; }
		
		#endregion
		
		#region Events
		
		public delegate void BookmarkAddedEventHandler(object sender, EventArgs e);
		
		public event BookmarkAddedEventHandler BookmarkAdded;
		
		public delegate void BookmarkRemovedEventHandler(object sender, EventArgs e);
		
		public event BookmarkRemovedEventHandler BookmarkRemoved;
		
		public delegate void AllBookmarksRemovedEventHandler(object sender, EventArgs e);
		
		public event AllBookmarksRemovedEventHandler AllBookmarksRemoved;
		
		#endregion
		
		#region Internal methods
		
		internal void ShiftBookmarks(int offset)
		{
			SortedList<int, Bookmark> newBookmarkList = new SortedList<int, Bookmark>();
			foreach (Bookmark bookmark in this.BookmarkList.Values)
			{
				int line = bookmark.LineNum - offset;
				if (line >= 0)
				{
					bookmark.LineNum = line;
					newBookmarkList.Add(line, bookmark);
				}
			}
			this.BookmarkList = newBookmarkList;
		}
		
		internal int FindPrevBookmarkIndex(int lineNum)
		{
			IList<Bookmark> values = this.BookmarkList.Values;
			for (int i = this.BookmarkList.Count - 1; i >= 0; --i)
			{
				if (values[i].LineNum <= lineNum)
				{
					return i;
				}
			}
			return this.BookmarkList.Count - 1;
		}
		
		internal int FindNextBookmarkIndex(int lineNum)
		{
			IList<Bookmark> values = this.BookmarkList.Values;
			for (int i = 0; i < this.BookmarkList.Count; ++i)
			{
				if (values[i].LineNum >= lineNum)
				{
					return i;
				}
			}
			return 0;
		}
		
		internal void RemoveBookmarkForLine(int lineNum)
		{
			this.BookmarkList.Remove(lineNum);
			OnBookmarkRemoved();
		}
		
		internal void RemoveBookmarksForLines(List<int> lineNumList)
		{
			foreach (int lineNum in lineNumList)
			{
				this.BookmarkList.Remove(lineNum);
			}
			OnBookmarkRemoved();
		}
		
		internal void AddBookmark(Bookmark bookmark)
		{
			this.BookmarkList.Add(bookmark.LineNum, bookmark);
			OnBookmarkAdded();
		}
		
		internal void ClearAllBookmarks()
		{
			Logger.logDebug("Removing all bookmarks");
			this.BookmarkList.Clear();
			OnAllBookmarksRemoved();
		}
		
		#endregion
		
		#region Public Methods
		
		public void ToggleBookmark(int lineNum)
		{
			if (IsBookmarkAtLine(lineNum))
			{
				RemoveBookmarkForLine(lineNum);
			}
			else
			{
				AddBookmark(new Bookmark(lineNum));
			}
		}
		
		public bool IsBookmarkAtLine(int lineNum)
		{
			return this.BookmarkList.ContainsKey(lineNum);
		}
		
		public int GetBookmarkIndexForLine(int lineNum)
		{
			return this.BookmarkList.IndexOfKey(lineNum);
		}
		
		public Bookmark GetBookmarkForLine(int lineNum)
		{
			return this.BookmarkList[lineNum];
		}
		
		public BookmarkCollection Bookmarks
		{
			get
			{
				return new BookmarkCollection(this.BookmarkList);
			}
		}
		
		#endregion
		
		#region Event Methods
		
		protected void OnBookmarkAdded()
		{
			if (BookmarkAdded != null)
			{
				BookmarkAdded(this, new EventArgs());
			}
		}
		
		protected void OnBookmarkRemoved()
		{
			if (BookmarkRemoved != null)
			{
				BookmarkRemoved(this, new EventArgs());
			}
		}
		
		protected void OnAllBookmarksRemoved()
		{
			if (AllBookmarksRemoved != null)
			{
				AllBookmarksRemoved(this, new EventArgs());
			}
		}

		#endregion
	}
}