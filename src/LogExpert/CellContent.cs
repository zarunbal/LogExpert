using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class CellContent
  {
    private string value;
    private int cellPosX;

    public CellContent(string value, int x)
    {
      this.value = value;
      this.cellPosX = x;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }

    public int CellPosX
    {
      get { return this.cellPosX; }
      set { this.cellPosX = value; }
    }
  }
}
