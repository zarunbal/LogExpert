// using System.Linq;

namespace LogExpert
{
    public class LoadFileEventArgs
    {
        #region Ctor

        public LoadFileEventArgs(string fileName, long pos, bool finished, long fileSize, bool newFile)
        {
            FileName = fileName;
            ReadPos = pos;
            Finished = finished;
            FileSize = fileSize;
            NewFile = newFile;
        }

        #endregion

        #region Properties / Indexers

        public string FileName { get; }

        public long FileSize { get; }

        public bool Finished { get; }

        public bool NewFile { get; }

        public long ReadPos { get; }

        #endregion
    }
}
