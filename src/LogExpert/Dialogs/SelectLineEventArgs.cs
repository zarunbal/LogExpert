using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert.Dialogs
{
  public class SelectLineEventArgs : EventArgs
  {
    int line;

    public int Line
    {
      get { return line; }
    }

    public SelectLineEventArgs(int line)
    {
      this.line = line;
    }
  }
}
