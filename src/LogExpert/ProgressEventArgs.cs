using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class ProgressEventArgs : EventArgs
  {
    int value;

    public int Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
    int minValue;

    public int MinValue
    {
      get { return minValue; }
      set { minValue = value; }
    }
    int maxValue;

    public int MaxValue
    {
      get { return maxValue; }
      set { maxValue = value; }
    }

    bool visible;

    public bool Visible
    {
      get { return visible; }
      set { visible = value; }
    }

  }
}
