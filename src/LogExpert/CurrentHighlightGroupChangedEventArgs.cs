using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class CurrentHighlightGroupChangedEventArgs
  {
    private LogWindow logWindow;
    private HilightGroup currentGroup;

    public CurrentHighlightGroupChangedEventArgs(LogWindow logWindow, HilightGroup currentGroup)
    {
      this.logWindow = logWindow;
      this.currentGroup = currentGroup;
    }


    public LogWindow LogWindow
    {
      get { return this.logWindow; }
    }

    public HilightGroup CurrentGroup
    {
      get { return this.currentGroup; }
    }
  }
}
