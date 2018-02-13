using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SftpFileSystem
{
  class SftpStreamChilkat : Stream
  {
    private Chilkat.SFtp sftp;
    private long currentPos;
    private string remoteFileName;
    private string handle;

    internal SftpStreamChilkat(Chilkat.SFtp sftp, string remoteFileName)
    {
      this.sftp = sftp;
      this.currentPos = 0;
      this.remoteFileName = remoteFileName;
      this.handle = this.sftp.OpenFile(this.remoteFileName, "readOnly", "openExisting");
      if (this.handle == null)
      {
        throw new IOException("Cannot open " + remoteFileName);
      }
    }

    public override bool CanRead
    {
      get { return true; }
    }

    public override bool CanSeek
    {
      get { return true; }
    }

    public override bool CanWrite
    {
      get { return false; }
    }

    public override void Flush()
    {
      throw new NotImplementedException();
    }

    public override long Length
    {
      get { return this.sftp.GetFileSize64(this.remoteFileName, true, false); }
    }

    public override long Position
    {
      get { return this.currentPos; }
      set
      {
        this.currentPos = value;
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      byte[] bytes = this.sftp.ReadFileBytes64(this.handle, this.Position, count);
      this.currentPos += bytes.Length;
      Array.Copy(bytes, 0, buffer, offset, bytes.Length);
      return bytes.Length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      if (origin == SeekOrigin.Begin)
      {
        this.Position = offset;
      } 
      else if (origin == SeekOrigin.Current)
      {
        this.Position += offset;
      }
      else if (origin == SeekOrigin.End)
      {
        this.Position = this.Length - offset;
      }
      return this.Position;
    }

    public override void SetLength(long value)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }

    public override void Close()
    {
      base.Close();
      this.sftp.CloseHandle(this.handle);
    }

  }
}
