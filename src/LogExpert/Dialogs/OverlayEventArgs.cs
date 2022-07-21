using System;
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
            BookmarkOverlay = overlay;
        }

        #endregion

        #region Properties

        public BookmarkOverlay BookmarkOverlay { get; set; }

        #endregion
    }
}