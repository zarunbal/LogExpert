using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace LogExpert
{
	public class LoadFileEventArgs
	{
		public LoadFileEventArgs(string fileName, long pos, bool finished, long fileSize, bool newFile)
		{
			this.FileName = fileName;
			this.ReadPos = pos;
			this.Finished = finished;
			this.FileSize = fileSize;
			this.NewFile = newFile;
		}

		public long ReadPos { get; private set; }

		public bool Finished { get; private set; }

		public long FileSize { get; private set; }

		public bool NewFile { get; private set; }

		public string FileName { get; private set; }
	}
}