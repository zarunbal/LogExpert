using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LogExpert
{
  public class BookmarkOverlay
  {
    Bookmark bookmark;
    public Bookmark Bookmark
    {
      get { return bookmark; }
      set { bookmark = value; }
    }

    Point position;
    public Point Position
    {
      get { return position; }
      set { position = value; }
    }

    Rectangle bubbleRect;

    public Rectangle BubbleRect
    {
      get { return bubbleRect; }
      set { bubbleRect = value; }
    }


  }
}
