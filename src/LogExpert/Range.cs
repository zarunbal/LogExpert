using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  class Range
  {
    private int startLine;
    private int endLine;

    public Range()
    {
    }
    public Range(int startLine, int endLine)
    {
      this.StartLine = startLine;
      this.EndLine = endLine;
    }

    public int StartLine
    {
      get { return this.startLine; }
      set { this.startLine = value; }
    }

    public int EndLine
    {
      get { return this.endLine; }
      set { this.endLine = value; }
    }
  }
}
