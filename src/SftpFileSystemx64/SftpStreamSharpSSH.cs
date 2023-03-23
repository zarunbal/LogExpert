using System;
using System.IO;
using Tamir.SharpSsh.jsch;

namespace SftpFileSystem
{
  class SftpStreamSharpSSH : Stream
  {
    private ChannelSftp sftpChannel;
    private long currentPos;
    private string remoteFileName;

    internal SftpStreamSharpSSH(ChannelSftp sftpChannel, string remoteFileName)
    {
      this.sftpChannel = sftpChannel;
      this.currentPos = 0;
      this.remoteFileName = remoteFileName;
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
      get
      {
        SftpATTRS sftpAttrs = this.sftpChannel.lstat(this.remoteFileName);
        return sftpAttrs.getSize();
      }
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
      Tamir.SharpSsh.java.io.OutputStream os = new Tamir.SharpSsh.java.io.OutputStream();

      this.sftpChannel.get(Tamir.SharpSsh.java.String(this.remoteFileName), os, null, ChannelSftp.RESUME,
                           this.currentPos);
      
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