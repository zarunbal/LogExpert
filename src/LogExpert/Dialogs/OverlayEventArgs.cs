using System;
using System.Collections.Generic;
using System.Text;
using LogExpert.Entities;

namespace LogExpert.Dialogs
{
    public class OverlayEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public OverlayEventArgs(BookmarkOverlay overlay)
        {
            this.BookmarkOverlay = overlay;
        }

        #endregion

        #region Properties

        public BookmarkOverlay BookmarkOverlay { get; set; }

        #endregion
    }
}