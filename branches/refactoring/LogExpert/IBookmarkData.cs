using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace LogExpert
{
  public interface IBookmarkData
  {
    void ToggleBookmark(int lineNum);
    bool IsBookmarkAtLine(int lineNum);
    int GetBookmarkIndexForLine(int lineNum);
    Bookmark GetBookmarkForLine(int lineNum);
    BookmarkCollection Bookmarks { get; }
  }
}
