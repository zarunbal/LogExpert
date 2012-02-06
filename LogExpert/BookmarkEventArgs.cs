using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  class BookmarkEventArgs : EventArgs
  {
    private Bookmark bookmark;

    public BookmarkEventArgs(Bookmark bookmark)
    {
      this.bookmark = bookmark;
    }

    public Bookmark Bookmark
    {
      get { return bookmark; }
    }
  }
}
