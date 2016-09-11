using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class RowHeightEntry
  {
    public RowHeightEntry()
    {
      LineNum = 0;
      Height = 0;
    }
    public RowHeightEntry(int lineNum, int height)
    {
      LineNum = lineNum;
      Height = height;
    }

    int lineNum;

    public int LineNum
    {
      get { return lineNum; }
      set { lineNum = value; }
    }

    int height;

    public int Height
    {
      get { return height; }
      set { height = value; }
    }
  }
}
