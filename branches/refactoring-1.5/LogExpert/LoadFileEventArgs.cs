using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace LogExpert
{
  public class LoadFileEventArgs
  {
    private long readPos;
    private bool finished;
    private long fileSize;
    private bool newFile;
    private string fileName;

    public LoadFileEventArgs(string fileName, long pos, bool finished, long fileSize, bool newFile)
    {
      this.fileName = fileName;
      this.readPos = pos;
      this.finished = finished;
      this.fileSize = fileSize;
      this.newFile = newFile;
    }

    public long ReadPos
    {
      get {return this.readPos;}
    }

    public bool Finished
    {
      get {return this.finished;}
    }

    public long FileSize
    {
      get { return this.fileSize; }
    }

    public bool NewFile
    {
      get { return this.newFile; }
    }

    public string FileName
    {
      get { return this.fileName; }
    }
  }
}
