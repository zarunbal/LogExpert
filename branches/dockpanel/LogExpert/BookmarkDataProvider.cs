using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  internal class BookmarkDataProvider : IBookmarkData
  {
    private SortedList<int, Bookmark> bookmarkList;

    internal BookmarkDataProvider()
    {
      bookmarkList = new SortedList<int, Bookmark>();
    }

    internal BookmarkDataProvider(SortedList<int, Bookmark> bookmarkList)
    {
      this.bookmarkList = bookmarkList;
    }

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
      this.bookmarkList = newBookmarkList;
    }

    internal int FindPrevBookmarkIndex(int lineNum)
    {
      IList<Bookmark> values = this.bookmarkList.Values;
      for (int i = this.bookmarkList.Count - 1; i >= 0; --i)
      {
        if (values[i].LineNum <= lineNum)
        {
          return i;
        }
      }
      return this.bookmarkList.Count - 1;
    }

    internal int FindNextBookmarkIndex(int lineNum)
    {
      IList<Bookmark> values = this.bookmarkList.Values;
      for (int i = 0; i < this.bookmarkList.Count; ++i)
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
      this.bookmarkList.Remove(lineNum);
      OnBookmarkRemoved();
    }

    internal void AddBookmark(Bookmark bookmark)
    {
      this.bookmarkList.Add(bookmark.LineNum, bookmark);
      OnBookmarkAdded();
    }

    internal void ClearAllBookmarks()
    {
      this.bookmarkList.Clear();
      OnBookmarkRemoved();
    }

    #region IBookmarkData Member

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
      get { return new BookmarkCollection(this.BookmarkList); }
    }

    internal SortedList<int, Bookmark> BookmarkList
    {
      get { return bookmarkList; }
      set { this.bookmarkList = value;}
    }

    #endregion

    public delegate void BookmarkAddedEventHandler(object sender, EventArgs e);
    public event BookmarkAddedEventHandler BookmarkAdded;
    protected void OnBookmarkAdded()
    {
      if (BookmarkAdded != null)
      {
        BookmarkAdded(this, new EventArgs());
      }
    }

    public delegate void BookmarkRemovedEventHandler(object sender, EventArgs e);
    public event BookmarkRemovedEventHandler BookmarkRemoved;
    protected void OnBookmarkRemoved()
    {
      if (BookmarkRemoved != null)
      {
        BookmarkRemoved(this, new EventArgs());
      }
    }


  }
}
