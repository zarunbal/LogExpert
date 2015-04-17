using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace LogExpert
{
	[Serializable]
	public class HilightEntry
	{
		#region Fields
		
		private string searchText = "";
		private bool isCaseSensitive;
		[NonSerialized]
		private Regex _regex = null;
		
		[NonSerialized]
		private bool _isSearchHit;   // highlightes search result 
		
		#endregion

		#region cTor

		public HilightEntry(string searchText, Color fgColor, Color bgColor, bool isWordMatch)
		{
			SearchText = searchText;
			ForegroundColor = fgColor;
			BackgroundColor = bgColor;
			IsWordMatch = isWordMatch;
		}

		public HilightEntry(string searchText, Color fgColor, Color bgColor,
			bool isRegEx, bool isCaseSensitive, bool isLedSwitch,
			bool isStopTail, bool isSetBookmark, bool isActionEntry, ActionEntry actionEntry, bool isWordMatch)
		{
			SearchText = searchText;
			ForegroundColor = fgColor;
			BackgroundColor = bgColor;
			IsRegEx = isRegEx;
			IsCaseSensitive = isCaseSensitive;
			IsLedSwitch = isLedSwitch;
			IsStopTail = isStopTail;
			IsSetBookmark = isSetBookmark;
			IsActionEntry = isActionEntry;
			ActionEntry = actionEntry;
			IsWordMatch = isWordMatch;
		}

		#endregion

		#region Properties
		
		public bool IsStopTail { get; set; }
		
		public bool IsSetBookmark { get; set; }
		
		public bool IsRegEx { get; set; }
		
		public bool IsCaseSensitive
		{
			get
			{
				return isCaseSensitive;
			}
			set
			{
				isCaseSensitive = value;
				_regex = null;
			}
		}
		
		public Color ForegroundColor { get; set; }
		
		public Color BackgroundColor { get; set; }
		
		public string SearchText
		{
			get
			{
				return searchText;
			}
			set
			{
				searchText = value;
				_regex = null;
			}
		}
		
		public bool IsLedSwitch { get; set; }
		
		public ActionEntry ActionEntry { get; set; }
		
		public bool IsActionEntry { get; set; }
		
		public string BookmarkComment { get; set; }
		
		public Regex Regex
		{
			get
			{
				if (_regex == null)
				{
					if (IsRegEx)
					{
						_regex = new Regex(SearchText, IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					}
					else
					{
						_regex = new Regex(Regex.Escape(SearchText), IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					}
				}
				return _regex;
			}
		}
		
		public bool IsWordMatch { get; set; }
		
		public bool IsSearchHit
		{
			get
			{
				return _isSearchHit;
			}
			set
			{
				_isSearchHit = value;
			}
		}
		
		public bool IsBold { get; set; }
		
		public bool NoBackground { get; set; }
		
		#endregion
		

	}
}