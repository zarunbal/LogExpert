using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class ColumnizerEventArgs : EventArgs
  {
    private ILogLineColumnizer columnizer;

    public ColumnizerEventArgs(ILogLineColumnizer columnizer)
    {
      this.columnizer = columnizer;
    }


    public ILogLineColumnizer Columnizer
    {
      get { return columnizer; }
    }
  }
}
