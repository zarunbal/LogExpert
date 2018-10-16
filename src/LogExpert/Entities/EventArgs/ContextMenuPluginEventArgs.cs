using System;
using System.Collections.Generic;

namespace LogExpert
{
    public class ContextMenuPluginEventArgs : EventArgs
    {
        #region Ctor

        public ContextMenuPluginEventArgs(IContextMenuEntry entry, IList<int> logLines, ILogLineColumnizer columnizer,
                                          ILogExpertCallback callback)
        {
            Entry = entry;
            LogLines = logLines;
            Columnizer = columnizer;
            Callback = callback;
        }

        #endregion

        #region Properties / Indexers

        public ILogExpertCallback Callback { get; }

        public ILogLineColumnizer Columnizer { get; }

        public IContextMenuEntry Entry { get; }

        public IList<int> LogLines { get; }

        #endregion
    }
}
