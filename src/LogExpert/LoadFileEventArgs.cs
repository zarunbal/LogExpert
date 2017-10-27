using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace LogExpert
{
    public class LoadFileEventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public LoadFileEventArgs(string fileName, long pos, bool finished, long fileSize, bool newFile)
        {
            this.FileName = fileName;
            this.ReadPos = pos;
            this.Finished = finished;
            this.FileSize = fileSize;
            this.NewFile = newFile;
        }

        #endregion

        #region Properties

        public long ReadPos { get; }

        public bool Finished { get; }

        public long FileSize { get; }

        public bool NewFile { get; }

        public string FileName { get; }

        #endregion
    }
}