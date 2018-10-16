using System;
using System.Collections.Generic;
using NLog;

namespace LogExpert
{
    internal class BookmarkDataProvider : IBookmarkData
    {
        #region Delegates

        public delegate void AllBookmarksRemovedEventHandler(object sender, EventArgs e);

        public delegate void BookmarkAddedEventHandler(object sender, EventArgs e);

        public delegate void BookmarkRemovedEventHandler(object sender, EventArgs e);

        #endregion

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Public Events

        public event AllBookmarksRemovedEventHandler AllBookmarksRemoved;

        public event BookmarkAddedEventHandler BookmarkAdded;
        public event BookmarkRemovedEventHandler BookmarkRemoved;

        #endregion

        #region Ctor

        internal BookmarkDataProvider()
        {
            BookmarkList = new SortedList<int, Bookmark>();
        }

        internal BookmarkDataProvider(SortedList<int, Bookmark> bookmarkList)
        {
            BookmarkList = bookmarkList;
        }

        #endregion

        #region Interface IBookmarkData

        public BookmarkCollection Bookmarks => new BookmarkCollection(BookmarkList);

        public Bookmark GetBookmarkForLine(int lineNum)
        {
            return BookmarkList[lineNum];
        }

        public int GetBookmarkIndexForLine(int lineNum)
        {
            return BookmarkList.IndexOfKey(lineNum);
        }

        public bool IsBookmarkAtLine(int lineNum)
        {
            return BookmarkList.ContainsKey(lineNum);
        }

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

        #endregion

        #region Properties / Indexers

        internal SortedList<int, Bookmark> BookmarkList { get; set; }

        #endregion

        #region Event handling Methods

        protected void OnAllBookmarksRemoved()
        {
            if (AllBookmarksRemoved != null)
            {
                AllBookmarksRemoved(this, new EventArgs());
            }
        }

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

        #endregion

        internal void ShiftBookmarks(int offset)
        {
            SortedList<int, Bookmark> newBookmarkList = new SortedList<int, Bookmark>();
            foreach (Bookmark bookmark in BookmarkList.Values)
            {
                int line = bookmark.LineNum - offset;
                if (line >= 0)
                {
                    bookmark.LineNum = line;
                    newBookmarkList.Add(line, bookmark);
                }
            }

            BookmarkList = newBookmarkList;
        }

        internal int FindPrevBookmarkIndex(int lineNum)
        {
            IList<Bookmark> values = BookmarkList.Values;
            for (int i = BookmarkList.Count - 1; i >= 0; --i)
            {
                if (values[i].LineNum <= lineNum)
                {
                    return i;
                }
            }

            return BookmarkList.Count - 1;
        }

        internal int FindNextBookmarkIndex(int lineNum)
        {
            IList<Bookmark> values = BookmarkList.Values;
            for (int i = 0; i < BookmarkList.Count; ++i)
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
            BookmarkList.Remove(lineNum);
            OnBookmarkRemoved();
        }

        internal void RemoveBookmarksForLines(List<int> lineNumList)
        {
            foreach (int lineNum in lineNumList)
            {
                BookmarkList.Remove(lineNum);
            }

            OnBookmarkRemoved();
        }


        internal void AddBookmark(Bookmark bookmark)
        {
            BookmarkList.Add(bookmark.LineNum, bookmark);
            OnBookmarkAdded();
        }

        internal void ClearAllBookmarks()
        {
            _logger.Debug("Removing all bookmarks");
            BookmarkList.Clear();
            OnAllBookmarksRemoved();
        }
    }
}
