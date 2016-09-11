using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class BookmarkEventArgs : EventArgs
	{
		public BookmarkEventArgs(Bookmark bookmark)
		{
			this.Bookmark = bookmark;
		}

		public Bookmark Bookmark { get; private set; }
	}
}