using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SftpFileSystem
{
    internal class SftpStreamChilkat : Stream
    {
        #region Fields

        private long currentPos;
        private readonly string handle;
        private readonly string remoteFileName;
        private readonly Chilkat.SFtp sftp;

        #endregion

        #region cTor

        internal SftpStreamChilkat(Chilkat.SFtp sftp, string remoteFileName)
        {
            this.sftp = sftp;
            currentPos = 0;
            this.remoteFileName = remoteFileName;
            handle = this.sftp.OpenFile(this.remoteFileName, "readOnly", "openExisting");
            if (handle == null)
            {
                throw new IOException("Cannot open " + remoteFileName);
            }
        }

        #endregion

        #region Properties

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

        public override long Length
        {
            get { return sftp.GetFileSize64(remoteFileName, true, false); }
        }

        public override long Position
        {
            get { return currentPos; }
            set { currentPos = value; }
        }

        #endregion

        #region Public methods

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] bytes = sftp.ReadFileBytes64(handle, Position, count);
            currentPos += bytes.Length;
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
            return bytes.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                Position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                Position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                Position = Length - offset;
            }

            return Position;
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
            sftp.CloseHandle(handle);
        }

        #endregion
    }
}