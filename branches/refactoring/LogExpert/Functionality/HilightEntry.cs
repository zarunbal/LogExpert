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
			e.pluginName = this.pluginName;
			e.actionParam = this.actionParam;
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
				this.regex = null;
			}
		}

		public HilightEntry(string searchText, Color fgColor, Color bgColor, bool isWordMatch)
		{
			this.searchText = searchText;
			this.ForegroundColor = fgColor;
			this.BackgroundColor = bgColor;
			this.IsRegEx = false;
			this.isCaseSensitive = false;
			this.IsLedSwitch = false;
			this.IsStopTail = false;
			this.IsSetBookmark = false;
			this.IsActionEntry = false;
			this.ActionEntry = null;
			this.IsWordMatch = isWordMatch;
		}

		public HilightEntry(string searchText, Color fgColor, Color bgColor,
			bool isRegEx, bool isCaseSensitive, bool isLedSwitch,
			bool isStopTail, bool isSetBookmark, bool isActionEntry, ActionEntry actionEntry, bool isWordMatch)
		{
			this.searchText = searchText;
			this.ForegroundColor = fgColor;
			this.BackgroundColor = bgColor;
			this.IsRegEx = isRegEx;
			this.isCaseSensitive = isCaseSensitive;
			this.IsLedSwitch = isLedSwitch;
			this.IsStopTail = isStopTail;
			this.IsSetBookmark = isSetBookmark;
			this.IsActionEntry = isActionEntry;
			this.ActionEntry = actionEntry;
			this.IsWordMatch = isWordMatch;
		}

		public Color ForegroundColor { get; set; }

		public Color BackgroundColor { get; set; }

		public string SearchText
		{
			get
			{
				return this.searchText;
			}
			set
			{
				this.searchText = value;
				this.regex = null;
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
				if (this.regex == null)
				{
					if (this.IsRegEx)
					{
						this.regex = new Regex(this.SearchText, this.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					}
					else
					{
						this.regex = new Regex(Regex.Escape(this.SearchText), this.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					}
				}
				return this.regex; 
			}
		}

		public bool IsWordMatch { get; set; }

		public bool IsSearchHit
		{
			get
			{
				return this.isSearchHit;
			}
			set
			{
				this.isSearchHit = value;
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
			return this.HilightEntry.SearchText + "/" + this.StartPos + "/" + this.Length;
		}
	}
}