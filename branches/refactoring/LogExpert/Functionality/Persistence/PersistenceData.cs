using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class PersistenceData
	{
		public SortedList<int, Bookmark> bookmarkList = new SortedList<int, Bookmark>();
		public SortedList<int, RowHeightEntry> rowHeightList = new SortedList<int, RowHeightEntry>();

		public bool multiFile = false;
		public int currentLine = -1;
		public int firstDisplayedLine = -1;
		public bool filterVisible = false;
		public bool filterAdvanced = false;
		public int filterPosition = 222;
		public bool bookmarkListVisible = false;
		public int bookmarkListPosition = 300;
		public bool followTail = true;
		public string fileName = null;
		public string tabName = null;
		public string columnizerName;
		public List<FilterParams> filterParamsList = new List<FilterParams>();
		public List<FilterTabData> filterTabDataList = new List<FilterTabData>();
		public int lineCount;
		public string highlightGroupName;
		public List<string> multiFileNames = new List<string>();
		public bool showBookmarkCommentColumn;
		public bool filterSaveListVisible = false;
		public Encoding encoding;
		public string multiFilePattern;
		public int multiFileMaxDays;
	}
}