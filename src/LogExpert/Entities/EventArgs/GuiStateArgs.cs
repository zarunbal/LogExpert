using System;
using System.Text;

namespace LogExpert.Entities.EventArgs
{
    public class GuiStateArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region Properties

        public bool TimeshiftEnabled { get; set; }

        public bool FollowTail { get; set; }

        public bool MenuEnabled { get; set; } = true;

        public string TimeshiftText { get; set; }

        public bool TimeshiftPossible { get; set; } = false;

        public bool MultiFileEnabled { get; set; } = true;

        public bool FilterEnabled { get; set; } = true;

        public bool CellSelectMode { get; set; } = false;

        public Encoding CurrentEncoding { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime MinTimestamp { get; set; }

        public DateTime MaxTimestamp { get; set; }

        public bool ShowBookmarkBubbles { get; set; }

        public bool IsMultiFileActive { get; set; }

        public bool ShowHiddenLines { get; set; } = true;

        public string HighlightGroupName { get; set; }

        public bool ColumnFinderVisible { get; set; }

        #endregion
    }
}