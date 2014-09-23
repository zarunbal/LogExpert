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
			Overlay = new BookmarkOverlay();
		}

		public Bookmark(int lineNum, string comment)
		{
			LineNum = lineNum;
			Text = comment;
			Overlay = new BookmarkOverlay();
		}

		public int LineNum { get; set; }

		public string Text { get; set; }

		public BookmarkOverlay Overlay { get; set; }

		/// <summary>
		/// Position offset of the overlay as set by the user by dragging the overlay with the mouse.
		/// </summary>
		public Size OverlayOffset { get; set; }
	}
}