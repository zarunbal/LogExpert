using System;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class DockContentEventArgs : EventArgs
    {
        #region Ctor

        public DockContentEventArgs(IDockContent content)
        {
            Content = content;
        }

        #endregion

        #region Properties / Indexers

        public IDockContent Content { get; }

        #endregion
    }
}
