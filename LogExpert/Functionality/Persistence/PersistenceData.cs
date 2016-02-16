using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace LogExpert
{
	[XmlRoot("LogExpert", Namespace = "")]
	public class PersistenceData
	{
		public PersistenceData()
		{
			BookmarkList = new List<Bookmark>();
			RowHeightList = new List<RowHeightEntry>();

			CurrentLine = -1;
			FirstDisplayedLine = -1;
			FilterPosition = 222;
			BookmarkListPosition = 300;
			FollowTail = true;
			FilterParamsList = new List<FilterParams>();
			FilterTabDataList = new List<FilterTabData>();
			MultiFileNames = new List<string>();
		}

		[XmlArray("BookmarkList")]
		[XmlArrayItem("BookmarkEntry")]
		public List<Bookmark> BookmarkList
		{
			get; set;
		}

		[XmlArray("RowHeightList")]
		[XmlArrayItem("RowHeightEntry")]
		public List<RowHeightEntry> RowHeightList
		{
			get; set;
		}

		[XmlArray("FilterParamsList")]
		[XmlArrayItem("FilterParamEntry")]
		public List<FilterParams> FilterParamsList
		{
			get; set;
		}

		[XmlArray("FilterTabDataList")]
		[XmlArrayItem("FilterTabData")]
		public List<FilterTabData> FilterTabDataList
		{
			get; set;
		}

		[XmlElement]
		public bool MultiFile
		{
			get; set;
		}

		[XmlElement]
		public int CurrentLine
		{
			get; set;
		}

		[XmlElement]
		public int FirstDisplayedLine
		{
			get; set;
		}

		[XmlElement]
		public bool FilterVisible
		{
			get; set;
		}

		[XmlElement]
		public bool FilterAdvanced
		{
			get; set;
		}

		[XmlElement]
		public int FilterPosition
		{
			get; set;
		}

		[XmlElement]
		public bool BookmarkListVisible
		{
			get; set;
		}

		[XmlElement]
		public int BookmarkListPosition
		{
			get; set;
		}

		[XmlElement]
		public bool FollowTail
		{
			get; set;
		}

		[XmlElement]
		public string FileName
		{
			get; set;
		}

		[XmlElement]
		public string TabName
		{
			get; set;
		}

		[XmlElement]
		public string ColumnizerName
		{
			get; set;
		}

		[XmlElement]
		public int LineCount
		{
			get; set;
		}

		[XmlElement]
		public string HighlightGroupName
		{
			get; set;
		}

		[XmlElement]
		public List<string> MultiFileNames
		{
			get; set;
		}

		[XmlElement]
		public bool ShowBookmarkCommentColumn
		{
			get; set;
		}

		[XmlElement]
		public bool FilterSaveListVisible
		{
			get; set;
		}

		[XmlElement("Encoding")]
		public string EncodingString
		{
			get; set;
		}

		[XmlIgnore]
		public Encoding Encoding
		{
			get { return System.Text.Encoding.GetEncoding(EncodingString); }
			set { EncodingString = value.ToString(); }
		}

		[XmlElement]
		public string MultiFilePattern
		{
			get; set;
		}

		[XmlElement]
		public int MultiFileMaxDays
		{
			get; set;
		}
	}
}