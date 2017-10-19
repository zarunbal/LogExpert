using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class ContextMenuPluginEventArgs : EventArgs
  {
    IContextMenuEntry entry;

    public IContextMenuEntry Entry
    {
      get { return entry; }
    }

    IList<int> logLines;

    public IList<int> LogLines
    {
      get { return logLines; }
    }
    ILogLineColumnizer columnizer;

    public ILogLineColumnizer Columnizer
    {
      get { return columnizer; }
    }
    ILogExpertCallback callback;

    public ILogExpertCallback Callback
    {
      get { return callback; }
    }

    public ContextMenuPluginEventArgs(IContextMenuEntry entry, IList<int> logLines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
    {
      this.entry = entry;
      this.logLines = logLines;
      this.columnizer = columnizer;
      this.callback = callback;
    }
  }
}
