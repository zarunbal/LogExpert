using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LogExpert
{
  public class Bookmark
  {
    public Bookmark(int lineNum)
    {
      LineNum = lineNum;
      Text = "";
      overlay = new BookmarkOverlay();
    }

    public Bookmark(int lineNum, string comment)
    {
      LineNum = lineNum;
      Text = comment;
      overlay = new BookmarkOverlay();
    }

    int lineNum;
    public int LineNum
    {
      get { return lineNum; }
      set { lineNum = value; }
    }

    string text;
    public string Text
    {
      get { return text; }
      set { text = value; }
    }

    BookmarkOverlay overlay;

    public BookmarkOverlay Overlay
    {
      get { return overlay; }
      set { overlay = value; }
    }


    Size overlayOffset;

    /// <summary>
    /// Position offset of the overlay as set by the user by dragging the overlay with the mouse.
    /// </summary>
    public Size OverlayOffset
    {
      get { return overlayOffset; }
      set { overlayOffset = value; }
    }




  }
}
