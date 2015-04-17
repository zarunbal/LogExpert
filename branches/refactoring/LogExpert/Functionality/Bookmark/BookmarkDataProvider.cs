using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogExpert
{
	public class BookmarkDataProvider : IBookmarkData
	{
		#region Fields
		
		private const string REPLACEMENT_FOR_NEW_LINE = @"\n";
		
		#endregion
		
		#region cTor
		
		internal BookmarkDataProvider()
		{
			BookmarkList = new SortedList<int, Bookmark>();
		}
		
		internal BookmarkDataProvider(SortedList<int, Bookmark> bookmarkList)
		{
			BookmarkList = bookmarkList;
		}
		
		#endregion
		
		#region Properties
		
		internal SortedList<int, Bookmark> BookmarkList { get; set; }
		
		#endregion
		
		#region Events
		
		public event Action BookmarkAdded;
		
		public event Action BookmarkRemoved;
		
		public event Action AllBookmarksRemoved;
		
		public delegate void BookmarkTextChangedEventHandler(BookmarkEventArgs e);
		
		public event BookmarkTextChangedEventHandler BookmarkTextChanged;
		
		protected void OnBookmarkTextChanged(Bookmark bookmark)
		{
			if (BookmarkTextChanged != null)
			{
				BookmarkTextChanged(new BookmarkEventArgs(bookmark));
			}
		}
		
		#endregion
		
		#region Internal methods
		
		internal void ShiftBookmarks(int offset)
		{
			SortedList<int, Bookmark> newBookmarkList = new SortedList<int, Bookmark>();
			foreach (Bookmark bookmark in BookmarkList.Values)
			{
				int line = bookmark.LineNum - offset;
				if (line >= 0)
				{
					bookmark.LineNum = line;
					newBookmarkList.Add(line, bookmark);
				}
			}
			BookmarkList = newBookmarkList;
			
			OnBookmarkRemoved();
		}
		
		internal int FindPrevBookmarkIndex(int lineNum)
		{
			IList<Bookmark> values = BookmarkList.Values;
			for (int i = BookmarkList.Count - 1; i >= 0; --i)
			{
				if (values[i].LineNum <= lineNum)
				{
					return i;
				}
			}
			return BookmarkList.Count - 1;
		}
		
		internal int FindNextBookmarkIndex(int lineNum)
		{
			IList<Bookmark> values = BookmarkList.Values;
			for (int i = 0; i < BookmarkList.Count; ++i)
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
			BookmarkList.Remove(lineNum);
			OnBookmarkRemoved();
		}
		
		internal void RemoveBookmarksForLines(List<int> lineNumList)
		{
			foreach (int lineNum in lineNumList)
			{
				BookmarkList.Remove(lineNum);
			}
			OnBookmarkRemoved();
		}
		
		internal void AddBookmark(Bookmark bookmark)
		{
			BookmarkList.Add(bookmark.LineNum, bookmark);
			OnBookmarkAdded();
		}
		
		internal void AddOrUpdateBookmark(Bookmark bookmark)
		{
			if (BookmarkList.ContainsKey(bookmark.LineNum))
			{
				AddBookmark(bookmark);
			}
			else
			{
				Bookmark existingBookmark = BookmarkList[bookmark.LineNum];
				existingBookmark.Text = bookmark.Text; // replace existing bookmark for that line, preserving the overlay
				OnBookmarkTextChanged(bookmark);
			}
		}
		
		internal void UpdateBookmarkText(Bookmark bookmark, string text)
		{
			bookmark.Text = text;
			OnBookmarkTextChanged(bookmark);
		}
		
		internal void ClearAllBookmarks()
		{
			Logger.logDebug("Removing all bookmarks");
			BookmarkList.Clear();
			OnAllBookmarksRemoved();
		}
		
		#endregion
		
		#region Public Methods
		
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
			return BookmarkList.ContainsKey(lineNum);
		}
		
		public int GetBookmarkIndexForLine(int lineNum)
		{
			return BookmarkList.IndexOfKey(lineNum);
		}
		
		public Bookmark GetBookmarkForLine(int lineNum)
		{
			Bookmark output = null;
			if (!BookmarkList.TryGetValue(lineNum, out output))
			{
				return null;
			}
			
			return output;
		}
		
		public BookmarkCollection Bookmarks
		{
			get
			{
				return new BookmarkCollection(BookmarkList);
			}
		}
		
		public void ExportBookmarkList( string logfileName, string fileName)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			using (StreamWriter writer = new StreamWriter(fs))
			{
				writer.WriteLine("Log file name;Line number;Comment");
				foreach (Bookmark bookmark in BookmarkList.Values)
				{
					string text = bookmark.Text.Replace(REPLACEMENT_FOR_NEW_LINE, @"\" + REPLACEMENT_FOR_NEW_LINE).Replace("\r\n", REPLACEMENT_FOR_NEW_LINE);
					string line = string.Format("{0};{1};{2}", logfileName, bookmark.LineNum, text);
					
					writer.WriteLine(line);
				}
				writer.Flush();
				fs.Flush();
				
				writer.Close();
				fs.Close();
			}
		}
		
		public void ImportBookmarkList(string logfileName, string fileName)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			using (StreamReader reader = new StreamReader(fs))
			{
				if (!reader.EndOfStream)
				{
					reader.ReadLine(); // skip "Log file name;Line number;Comment"
				}
				
				while (!reader.EndOfStream)
				{
					try
					{
						string line = reader.ReadLine();
						line = line.Replace(REPLACEMENT_FOR_NEW_LINE, "\r\n").Replace("\\\r\n", REPLACEMENT_FOR_NEW_LINE);
						
						// Line is formatted: logfileName ";" bookmark.LineNum ";" bookmark.Text;
						int firstSeparator = line.IndexOf(';');
						int secondSeparator = line.IndexOf(';', firstSeparator + 1);
						
						string lineStr = line.Substring(firstSeparator + 1, secondSeparator - firstSeparator - 1);
						string comment = line.Substring(secondSeparator + 1);
						
						int lineNum;
						if (int.TryParse(lineStr, out lineNum))
						{
							Bookmark bookmark = new Bookmark(lineNum, comment);
							AddOrUpdateBookmark(bookmark);
						}
						else
						{
							//!!!log error: skipping a line entry
						}
					}
					catch
					{
						//!!!
					}
				}
			}
		}
		
		#endregion
		
		#region Event Methods
		
		protected void OnBookmarkAdded()
		{
			if (BookmarkAdded != null)
			{
				BookmarkAdded();
			}
		}
		
		protected void OnBookmarkRemoved()
		{
			if (BookmarkRemoved != null)
			{
				BookmarkRemoved();
			}
		}
		
		protected void OnAllBookmarksRemoved()
		{
			if (AllBookmarksRemoved != null)
			{
				AllBookmarksRemoved();
			}
		}
	
		#endregion
	}
}