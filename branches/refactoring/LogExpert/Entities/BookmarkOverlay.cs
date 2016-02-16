using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LogExpert
{
	public class BookmarkOverlay
	{
		[System.Xml.Serialization.XmlIgnore]
		public Bookmark Bookmark { get; set; }

		public Point Position { get; set; }

		public Rectangle BubbleRect { get; set; }
	}
}