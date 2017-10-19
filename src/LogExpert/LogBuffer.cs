using System;
using System.Collections.Generic;
using System.Text;
//using System.Linq;

namespace LogExpert
{
  public class LogBuffer
  {
    public int MAX_LINES = 500;

    IList<string> logLines = new List<string>();
    int startLine = 0;
    int lineCount = 0;
    long startPos = 0;
    long size = 0;
    bool isDisposed = false;
    ILogFileInfo fileInfo;
    private int droppedLinesCount = 0;
    private int prevBuffersDroppedLinesSum = 0;    // sum over all preceeding log buffers
#if DEBUG
    private long disposeCount = 0;
    IList<long> filePositions = new List<long>();   // file position for every line
#endif

    //public LogBuffer() { }

    public LogBuffer(ILogFileInfo fileInfo, int maxLines)
    {
      this.fileInfo = fileInfo;
      this.MAX_LINES = maxLines;
    }

    public void AddLine(string line, long filePos)
    {
      this.logLines.Add(line);
#if DEBUG
      this.filePositions.Add(filePos);
#endif
      this.lineCount++;
      isDisposed = false;
    }

    public void ClearLines()
    {
      this.logLines.Clear();
      this.lineCount = 0;
    }

    public void DisposeContent()
    {
      this.logLines.Clear();
      isDisposed = true;
#if DEBUG
      disposeCount++;
#endif
    }

    public string GetLineOfBlock(int num)
    {
      if (num < this.logLines.Count && num >= 0)
        return this.logLines[num];
      else
        return null;
    }

    public long StartPos
    {
      set {this.startPos = value;}
      get {return this.startPos;}
    }

    public long Size
    {
      set 
      {
        this.size = value;
#if DEBUG
        if (this.filePositions.Count > 0)
        {
          if (this.size < this.filePositions[this.filePositions.Count - 1] - this.StartPos)
          {
            Logger.logError("LogBuffer overall Size must be greater than last line file position!");
          }
        }
#endif
      }
      get {return this.size;}
    }

    public int StartLine
    {
      set { this.startLine = value; }
      get { return this.startLine; }
    }

    public int LineCount
    {
      get { return this.lineCount; }
    }

    public bool IsDisposed
    {
      get { return this.isDisposed; }
    }

    public ILogFileInfo FileInfo
    {
      get { return this.fileInfo; }
      set { this.fileInfo = value; }
    }

    public int DroppedLinesCount
    {
      get { return this.droppedLinesCount; }
      set { this.droppedLinesCount = value; }
    }
#if DEBUG
    public long DisposeCount
    {
      get { return this.disposeCount; }
    }



    public long GetFilePosForLineOfBlock(int line)
    {
      if (line >=0 && line < this.filePositions.Count)
        return this.filePositions[line];
      else
        return -1;
    }

#endif

    public int PrevBuffersDroppedLinesSum
    {
      get { return this.prevBuffersDroppedLinesSum; }
      set { this.prevBuffersDroppedLinesSum = value; }
    }

  }
}
