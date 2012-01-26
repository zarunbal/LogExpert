using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class LogEventArgs : EventArgs 
  {
    private long  fileSize;
    private int   lineCount;
    private int   prevLineCount;
    private long  prevFileSize;
    private bool isRollover = false;
    private int rolloverOffset = 0;

    public int RolloverOffset
    {
      get { return rolloverOffset; }
      set { rolloverOffset = value; }
    } 

    public bool IsRollover
    {
      get { return isRollover; }
      set { isRollover = value; }
    }

    public long FileSize
    {
      get { return this.fileSize; }
      set { this.fileSize = value; }
    }

    public int LineCount
    {
      get { return this.lineCount; }
      set { this.lineCount = value;}
    }

    public int PrevLineCount
    {
      get { return this.prevLineCount; }
      set { this.prevLineCount = value; }
    }

    public long PrevFileSize
    {
      get { return this.prevFileSize; }
      set { this.prevFileSize = value; }
    }


  }
}
