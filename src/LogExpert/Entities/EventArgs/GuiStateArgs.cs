using System;
using System.Text;

namespace LogExpert
{
    public class GuiStateArgs : EventArgs
    {
        #region Properties / Indexers

        public bool CellSelectMode { get; set; } = false;

        public bool ColumnFinderVisible { get; set; }

        public Encoding CurrentEncoding { get; set; }

        public bool FilterEnabled { get; set; } = true;

        public bool FollowTail { get; set; }

        public string HighlightGroupName { get; set; }

        public bool IsMultiFileActive { get; set; }

        public DateTime MaxTimestamp { get; set; }

        public bool MenuEnabled { get; set; } = true;

        public DateTime MinTimestamp { get; set; }

        public bool MultiFileEnabled { get; set; } = true;

        public bool ShowBookmarkBubbles { get; set; }

        public bool ShowHiddenLines { get; set; } = true;

        public bool TimeshiftEnabled { get; set; }

        public bool TimeshiftPossible { get; set; } = false;

        public string TimeshiftText { get; set; }

        public DateTime Timestamp { get; set; }

        #endregion
    }
}
