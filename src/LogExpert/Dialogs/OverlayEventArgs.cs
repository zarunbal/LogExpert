using System;

namespace LogExpert.Dialogs
{
    public class OverlayEventArgs : EventArgs
    {
        #region Ctor

        public OverlayEventArgs(BookmarkOverlay overlay)
        {
            BookmarkOverlay = overlay;
        }

        #endregion

        #region Properties / Indexers

        public BookmarkOverlay BookmarkOverlay { get; set; }

        #endregion
    }
}
