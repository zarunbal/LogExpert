using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace LogExpert
{
	[Serializable]
	public class ActionEntry
	{
		public string pluginName;
		public string actionParam;

		public ActionEntry Copy()
		{
			ActionEntry e = new ActionEntry();
			e.pluginName = pluginName;
			e.actionParam = actionParam;
			return e;
		}
	}

	[Serializable]
	public class HilightEntry
	{
		private string searchText = "";
		private bool isCaseSensitive;
		[NonSerialized]
		private Regex regex = null;

		[NonSerialized]
		private bool isSearchHit;   // highlightes search result

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
				regex = null;
			}
		}

		public HilightEntry(string searchText, Color fgColor, Color bgColor, bool isWordMatch)
		{
			SearchText = searchText;
			ForegroundColor = fgColor;
			BackgroundColor = bgColor;
			IsRegEx = false;
			isCaseSensitive = false;
			IsLedSwitch = false;
			IsStopTail = false;
			IsSetBookmark = false;
			IsActionEntry = false;
			ActionEntry = null;
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
				regex = null;
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
				if (regex == null)
				{
					if (IsRegEx)
					{
						regex = new Regex(SearchText, IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					}
					else
					{
						regex = new Regex(Regex.Escape(SearchText), IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					}
				}
				return regex; 
			}
		}

		public bool IsWordMatch { get; set; }

		public bool IsSearchHit
		{
			get
			{
				return isSearchHit;
			}
			set
			{
				isSearchHit = value;
			}
		}

		public bool IsBold { get; set; }

		public bool NoBackground { get; set; }
	}

	/// <summary>
	/// Class for storing word-wise hilight matches. Used for colouring different matches on one line.
	/// </summary>
	public class HilightMatchEntry
	{
		public HilightEntry HilightEntry { get; set; }

		public int StartPos { get; set; }

		public int Length { get; set; }

		public override String ToString()
		{
			return HilightEntry.SearchText + "/" + StartPos + "/" + Length;
		}
	}
}