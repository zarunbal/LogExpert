using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class FilterListChangedEventArgs
  {
    private LogWindow logWindow;

    public FilterListChangedEventArgs(LogWindow logWindow)
    {
      this.logWindow = logWindow;
    }


    public LogWindow LogWindow
    {
      get { return this.logWindow; }
    }
  }
}
