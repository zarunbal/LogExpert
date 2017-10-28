using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class BookmarkEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public BookmarkEventArgs(Bookmark bookmark)
        {
            this.Bookmark = bookmark;
        }

        #endregion

        #region Properties

        public Bookmark Bookmark { get; }

        #endregion
    }
}