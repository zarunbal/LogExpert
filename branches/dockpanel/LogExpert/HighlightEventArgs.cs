using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class HighlightEventArgs : EventArgs
  {
    private int startLine;
    private int count;

    public HighlightEventArgs(int startLine, int count)
    {
      this.startLine = startLine;
      this.count = count;
    }


    public int StartLine
    {
      get { return this.startLine; }
    }

    public int Count
    {
      get { return this.count; }
    }
  }
}
