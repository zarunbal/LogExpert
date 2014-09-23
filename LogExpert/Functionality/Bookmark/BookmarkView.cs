using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	class BookmarkView : IBookmarkView
	{
		private bool isActive = false;

		#region IBookmarkView Member

		public void UpdateView()
		{
			throw new NotImplementedException();
		}

		public void BookmarkTextChanged(Bookmark bookmark)
		{
			throw new NotImplementedException();
		}

		public void SelectBookmark(int lineNum)
		{
			throw new NotImplementedException();
		}

		public bool LineColumnVisible
		{
			set
			{
				throw new NotImplementedException();
			}
		}

		public bool IsActive
		{
			get
			{
				return isActive;
			}
			set
			{
				isActive = value;
			}
		}

		public void SetBookmarkData(IBookmarkData bookmarkData)
		{
			throw new NotImplementedException();
		}
		
		#endregion
	}
}