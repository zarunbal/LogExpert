using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class GuiStateArgs : EventArgs
	{
		public bool TimeshiftEnabled { get; set; }

		public bool FollowTail { get; set; }

		bool menuEnabled = true;

		public bool MenuEnabled
		{
			get
			{
				return menuEnabled;
			}
			set
			{
				menuEnabled = value;
			}
		}

		public string TimeshiftText { get; set; }

		bool timeshiftPossible = false;

		public bool TimeshiftPossible
		{
			get
			{
				return timeshiftPossible;
			}
			set
			{
				timeshiftPossible = value;
			}
		}

		bool multiFileEnabled = true;

		public bool MultiFileEnabled
		{
			get
			{
				return multiFileEnabled;
			}
			set
			{
				multiFileEnabled = value;
			}
		}

		bool filterEnabled = true;

		public bool FilterEnabled
		{
			get
			{
				return this.filterEnabled;
			}
			set
			{
				this.filterEnabled = value;
			}
		}

		bool cellSelectMode = false;

		public bool CellSelectMode
		{
			get
			{
				return this.cellSelectMode;
			}
			set
			{
				this.cellSelectMode = value;
			}
		}

		public Encoding CurrentEncoding { get; set; }

		public DateTime Timestamp { get; set; }

		public DateTime MinTimestamp { get; set; }

		public DateTime MaxTimestamp { get; set; }

		public bool ShowBookmarkBubbles { get; set; }

		public bool IsMultiFileActive { get; set; }

		public bool ShowHiddenLines
		{
			get
			{
				return this.showHiddenLines;
			}
			set
			{
				this.showHiddenLines = value;
			}
		}

		public string HighlightGroupName { get; set; }

		public bool ColumnFinderVisible { get; set; }

		private bool showHiddenLines = true;
	}
}