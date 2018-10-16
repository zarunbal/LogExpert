using System;
using System.IO;
using Chilkat;
using Stream = System.IO.Stream;

namespace SftpFileSystem
{
    internal class SftpStreamChilkat : Stream
    {
        #region Private Fields

        private readonly string _handle;
        private readonly string _remoteFileName;
        private readonly SFtp _sftp;

        private long _currentPos;

        #endregion

        #region Ctor

        internal SftpStreamChilkat(SFtp sftp, string remoteFileName)
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

        #region Properties / Indexers

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _sftp.GetFileSize64(_remoteFileName, true, false);

        public override long Position
        {
            get => _currentPos;
            set => _currentPos = value;
        }

        #endregion

        #region Overrides

        public override void Close()
        {
            base.Close();
            _sftp.CloseHandle(_handle);
        }

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

        #endregion
    }
}
