using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert.Dialogs
{
  public class OverlayEventArgs : EventArgs
  {
    private BookmarkOverlay bookmarkOverlay;

    public OverlayEventArgs(BookmarkOverlay overlay)
    {
      this.BookmarkOverlay = overlay;
    }

    public BookmarkOverlay BookmarkOverlay
    {
      get { return this.bookmarkOverlay; }
      set { this.bookmarkOverlay = value; }
    }
  }
}
