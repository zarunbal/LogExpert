using System;
using System.Collections.Generic;
using System.Text;

//using System.Linq;

namespace LogExpert
{
	public class LogBuffer
	{
		#region Fields
		
		public int MAX_LINES = 500;
		
		IList<string> _logLines = new List<string>();
		
		long size = 0;
		
		#if DEBUG
		IList<long> _filePositions = new List<long>();   // file position for every line
		
		#endif 
		
		#endregion
		
		#region cTor
		
		public LogBuffer(ILogFileInfo fileInfo, int maxLines)
		{
			FileInfo = fileInfo;
			MAX_LINES = maxLines;
		}
		
		#endregion
		
		#region Properties
		
		public long StartPos { get; set; }
		
		public long Size
		{
			set
			{
				size = value;
				#if DEBUG
				if (_filePositions.Count > 0)
				{
					if (size < _filePositions[_filePositions.Count - 1] - StartPos)
					{
						Logger.logError("LogBuffer overall Size must be greater than last line file position!");
					}
				}
				#endif
			}
			get
			{
				return size;
			}
		}
		
		public int StartLine { get; set; }
		
		public int LineCount { get; private set; }
		
		public bool IsDisposed { get; private set; }
		
		public ILogFileInfo FileInfo { get; set; }
		
		public int DroppedLinesCount { get; set; }
		
		public int PrevBuffersDroppedLinesSum { get; set; }
		
		#if DEBUG
		public long DisposeCount { get; set; }
		
		#endif
		
		#endregion
		
		#region Public Methods
		
		public void AddLine(string line, long filePos)
		{
			_logLines.Add(line);
			#if DEBUG
			_filePositions.Add(filePos);
			#endif
			LineCount++;
			IsDisposed = false;
		}
		
		public void ClearLines()
		{
			_logLines.Clear();
			LineCount = 0;
		}
		
		public void DisposeContent()
		{
			_logLines.Clear();
			IsDisposed = true;
			#if DEBUG
			DisposeCount++;
			#endif
		}
		
		public string GetLineOfBlock(int num)
		{
			if (num < _logLines.Count && num >= 0)
			{
				return _logLines[num];
			}
			else
			{
				return null;
			}
		}
		
		#if DEBUG
		
		public long GetFilePosForLineOfBlock(int line)
		{
			if (line >= 0 && line < _filePositions.Count)
			{
				return _filePositions[line];
			}
			else
			{
				return -1;
			}
		}
		#endif
	
		#endregion
	}
}