using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SftpFileSystem
{
    internal class SftpStreamChilkat : Stream
    {
        #region Fields

        private readonly string _handle;
        private readonly string _remoteFileName;
        private readonly Chilkat.SFtp _sftp;

        private long _currentPos;

        #endregion

        #region cTor

        internal SftpStreamChilkat(Chilkat.SFtp sftp, string remoteFileName)
        {
            _sftp = sftp;
            _currentPos = 0;
            _remoteFileName = remoteFileName;
            _handle = _sftp.OpenFile(_remoteFileName, "readOnly", "openExisting");
            if (_handle == null)
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
            get { return _sftp.GetFileSize64(_remoteFileName, true, false); }
        }

        public override long Position
        {
            get { return _currentPos; }
            set { _currentPos = value; }
        }

        #endregion

        #region Public methods

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] bytes = _sftp.ReadFileBytes64(_handle, Position, count);
            _currentPos += bytes.Length;
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
            _sftp.CloseHandle(_handle);
        }

        #endregion
    }
}