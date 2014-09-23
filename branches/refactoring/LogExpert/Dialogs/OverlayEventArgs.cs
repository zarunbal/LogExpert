using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert.Dialogs
{
	public class OverlayEventArgs : EventArgs
	{
		public OverlayEventArgs(BookmarkOverlay overlay)
		{
			this.BookmarkOverlay = overlay;
		}

		public BookmarkOverlay BookmarkOverlay { get; set; }
	}
}