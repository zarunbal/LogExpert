using System;
using System.Collections.Generic;
using NLog;

namespace LogExpert.Classes.Bookmark
{
    internal class BookmarkDataProvider : IBookmarkData
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region cTor

        internal BookmarkDataProvider()
        {
            BookmarkList = new SortedList<int, LogExpert.Bookmark>();
        }

        internal BookmarkDataProvider(SortedList<int, LogExpert.Bookmark> bookmarkList)
        {
            BookmarkList = bookmarkList;
        }

        #endregion

        #region Delegates

        public delegate void AllBookmarksRemovedEventHandler(object sender, EventArgs e);

        public delegate void BookmarkAddedEventHandler(object sender, EventArgs e);

        public delegate void BookmarkRemovedEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event BookmarkAddedEventHandler BookmarkAdded;
        public event BookmarkRemovedEventHandler BookmarkRemoved;
        public event AllBookmarksRemovedEventHandler AllBookmarksRemoved;

        #endregion

        #region Properties
        
        public BookmarkCollection Bookmarks => new BookmarkCollection(BookmarkList);

        internal SortedList<int, LogExpert.Bookmark> BookmarkList { get; set; }

        #endregion

        #region Public methods

        public void ToggleBookmark(int lineNum)
        {
            if (IsBookmarkAtLine(lineNum))
            {
                RemoveBookmarkForLine(lineNum);
            }
            else
            {
                AddBookmark(new LogExpert.Bookmark(lineNum));
            }
        }

        public bool IsBookmarkAtLine(int lineNum)
        {
            return BookmarkList.ContainsKey(lineNum);
        }

        public int GetBookmarkIndexForLine(int lineNum)
        {
            return BookmarkList.IndexOfKey(lineNum);
        }

        public LogExpert.Bookmark GetBookmarkForLine(int lineNum)
        {
            return BookmarkList[lineNum];
        }

        #endregion

        #region Internals

        internal void ShiftBookmarks(int offset)
        {
            SortedList<int, LogExpert.Bookmark> newBookmarkList = new SortedList<int, LogExpert.Bookmark>();
            foreach (LogExpert.Bookmark bookmark in BookmarkList.Values)
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
            IList<LogExpert.Bookmark> values = BookmarkList.Values;
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
            IList<LogExpert.Bookmark> values = BookmarkList.Values;
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


        internal void AddBookmark(LogExpert.Bookmark bookmark)
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

        #endregion

        protected void OnBookmarkAdded()
        {
            BookmarkAdded?.Invoke(this, EventArgs.Empty);
        }

        protected void OnBookmarkRemoved()
        {
            BookmarkRemoved?.Invoke(this, EventArgs.Empty);
        }

        protected void OnAllBookmarksRemoved()
        {
            AllBookmarksRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}