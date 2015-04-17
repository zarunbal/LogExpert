using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace LogExpert
{
	public class LogfileReader
	{
		private int MAX_BUFFERS = 10;
		private int MAX_LINES_PER_BUFFER = 100;

		private string _fileName;
		private long _currFileSize = 0;
		private int _currLineCount = 0;
		private IList<LogBuffer> _bufferList;
		private Dictionary<int, LogBufferCacheEntry> _lruCacheDict;

		private ReaderWriterLock _lruCacheDictLock;
		private ReaderWriterLock _bufferListLock;
		private ReaderWriterLock _disposeLock;

		private Thread _monitorThread = null;
		private Thread _garbageCollectorThread = null;
		private bool _shouldStop;
		private long _fileLength;
		private bool _isDeleted;
		private bool _isFastFailOnGetLogLine = false;
		private bool _isFailModeCheckCallPending = false;
		private ILogFileInfo _watchedILogFileInfo;
		private IList<ILogFileInfo> _logFileInfoList = new List<ILogFileInfo>();
		private bool _isLineCountDirty = true;
		private bool _isMultiFile = false;
		private EncodingOptions _encodingOptions;
		private bool _isXmlMode = false;
		private IPreProcessColumnizer _preProcessColumnizer = null;
		private bool _contentDeleted = false;
		private MultifileOptions _mutlifileOptions;

		private delegate string GetLogLineFx(int lineNum);

		private Object monitor = new Object();

		public LogfileReader(string fileName, EncodingOptions encodingOptions, bool multiFile, int bufferCount, int linesPerBuffer, MultifileOptions mutlifileOptions)
		{
			if (fileName == null)
			{
				return;
			}
			_fileName = fileName;
			EncodingOptions = encodingOptions;
			_isMultiFile = multiFile;
			MAX_BUFFERS = bufferCount;
			MAX_LINES_PER_BUFFER = linesPerBuffer;
			_mutlifileOptions = mutlifileOptions;

			InitLruBuffers();

			if (multiFile)
			{
				ILogFileInfo info = GetLogFileInfo(fileName);
				RolloverFilenameHandler rolloverHandler = new RolloverFilenameHandler(info, _mutlifileOptions);
				LinkedList<string> nameList = rolloverHandler.GetNameList();

				ILogFileInfo fileInfo = null;
				foreach (string name in nameList)
				{
					fileInfo = AddFile(name);
				}
				_watchedILogFileInfo = fileInfo;  // last added file in the list is the watched file
			}
			else
			{
				_watchedILogFileInfo = AddFile(fileName);
			}
			StartGCThread();
		}

		public LogfileReader(string[] fileNames, EncodingOptions encodingOptions, int bufferCount, int linesPerBuffer, MultifileOptions mutlifileOptions)
		{
			if (fileNames == null || fileNames.Length < 1)
			{
				return;
			}
			EncodingOptions = encodingOptions;
			_isMultiFile = true;
			MAX_BUFFERS = bufferCount;
			MAX_LINES_PER_BUFFER = linesPerBuffer;
			_mutlifileOptions = mutlifileOptions;

			InitLruBuffers();

			ILogFileInfo fileInfo = null;
			foreach (string name in fileNames)
			{
				fileInfo = AddFile(name);
			}
			_watchedILogFileInfo = fileInfo;
			_fileName = fileInfo.FullName;

			StartGCThread();
		}

		~LogfileReader()
		{
			DeleteAllContent();
		}

		private void InitLruBuffers()
		{
			_bufferList = new List<LogBuffer>();
			//lruDict = new Dictionary<int, int>(MAX_BUFFERS + 1);  // key=startline, value = index in bufferLru
			_lruCacheDict = new Dictionary<int, LogBufferCacheEntry>(MAX_BUFFERS + 1);
			_lruCacheDictLock = new ReaderWriterLock();
			_bufferListLock = new ReaderWriterLock();
			_disposeLock = new ReaderWriterLock();
		}

		private void StartGCThread()
		{
			_garbageCollectorThread = new Thread(new ThreadStart(GarbageCollectorThreadProc));
			_garbageCollectorThread.IsBackground = true;
			_garbageCollectorThread.Start();
		}

		public ILogFileInfo AddFile(string fileName)
		{
			Logger.logInfo(string.Format("Adding file to ILogFileInfoList: {0}", fileName));
			ILogFileInfo info = GetLogFileInfo(fileName);
			_logFileInfoList.Add(info);
			return info;
		}

		private void ResetBufferCache()
		{
			_currFileSize = 0;
			LineCount = 0;
			//lastReturnedLine = "";
			//lastReturnedLineNum = -1;
			//lastReturnedLineNumForBuffer = -1;
		}

		private void CloseFiles()
		{
			//foreach (ILogFileInfo info in ILogFileInfoList)
			//{
			//  info.CloseFile();
			//}
			_currFileSize = 0;
			LineCount = 0;
			//lastReturnedLine = "";
			//lastReturnedLineNum = -1;
			//lastReturnedLineNumForBuffer = -1;
		}

		private ILogFileInfo GetLogFileInfo(string fileNameOrUri)
		{
			IFileSystemPlugin fs = PluginRegistry.GetInstance().FindFileSystemForUri(fileNameOrUri);
			if (fs == null)
			{
				throw new LogFileException(string.Format("No file system plugin found for {0}", fileNameOrUri));
			}
			ILogFileInfo logFileInfo = fs.GetLogfileInfo(fileNameOrUri);
			if (logFileInfo == null)
			{
				throw new LogFileException(string.Format("Cannot find {0}", fileNameOrUri));
			}
			return logFileInfo;
		}

		/// <summary>
		/// Public for unit test reasons
		/// </summary>
		public void ReadFiles()
		{
			_currFileSize = 0;
			LineCount = 0;
			//lastReturnedLine = "";
			//lastReturnedLineNum = -1;
			//lastReturnedLineNumForBuffer = -1;
			_isDeleted = false;
			ClearLru();
			AcquireBufferListWriterLock();
			_bufferList.Clear();
			ReleaseBufferListWriterLock();
			try
			{
				foreach (ILogFileInfo info in _logFileInfoList)
				{
					//info.OpenFile();
					ReadToBufferList(info, 0, LineCount);
				}
				if (_logFileInfoList.Count > 0)
				{
					ILogFileInfo info = _logFileInfoList[_logFileInfoList.Count - 1];
					_fileLength = info.Length;
					_watchedILogFileInfo = info;
				}
			}
			catch (IOException e)
			{
				Logger.logWarn(string.Format("IOException: {0}", e.Message));
				_fileLength = 0;
				_isDeleted = true;
				LineCount = 0;
			}
			LogEventArgs args = new LogEventArgs();
			args.PrevFileSize = 0;
			args.PrevLineCount = 0;
			args.LineCount = LineCount;
			args.FileSize = _currFileSize;
			OnFileSizeChanged(args);
		}

		/// <summary>
		/// Public for unit tests.
		/// </summary>
		/// <returns></returns>
		public int ShiftBuffers()
		{
			Logger.logInfo(string.Format("ShiftBuffers() begin for {0}{1}", _fileName, IsMultiFile ? " (MultiFile)" : ""));
			AcquireBufferListWriterLock();
			int offset = 0;
			_isLineCountDirty = true;
			lock (monitor)
			{
				RolloverFilenameHandler rolloverHandler = new RolloverFilenameHandler(_watchedILogFileInfo, _mutlifileOptions);
				LinkedList<string> fileNameList = rolloverHandler.GetNameList();

				ResetBufferCache();
				IList<ILogFileInfo> lostILogFileInfoList = new List<ILogFileInfo>();
				IList<ILogFileInfo> readNewILogFileInfoList = new List<ILogFileInfo>();
				IList<ILogFileInfo> newFileInfoList = new List<ILogFileInfo>();
				IEnumerator<ILogFileInfo> enumerator = _logFileInfoList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ILogFileInfo logFileInfo = enumerator.Current;
					string fileName = logFileInfo.FullName;
					Logger.logDebug(string.Format("Testing file {0}", fileName));
					LinkedListNode<string> node = fileNameList.Find(fileName);
					if (node == null)
					{
						Logger.logWarn(string.Format("File {0} not found", fileName));
						continue;
					}
					if (node.Previous != null)
					{
						fileName = node.Previous.Value;
						ILogFileInfo newILogFileInfo = GetLogFileInfo(fileName);
						Logger.logDebug(string.Format("{0} exists", fileName));
						Logger.logDebug(string.Format("Old size={0}, new size={1}", logFileInfo.OriginalLength, newILogFileInfo.Length));
						// is the new file the same as the old buffer info?
						if (newILogFileInfo.Length == logFileInfo.OriginalLength)
						{
							ReplaceBufferInfos(logFileInfo, newILogFileInfo);
							newFileInfoList.Add(newILogFileInfo);
						}
						else
						{
							Logger.logDebug(string.Format("Buffer for {0} must be re-read.", fileName));
							// not the same. so must read the rest of the list anew from the files
							readNewILogFileInfoList.Add(newILogFileInfo);
							while (enumerator.MoveNext())
							{
								fileName = enumerator.Current.FullName;
								node = fileNameList.Find(fileName);
								if (node == null)
								{
									Logger.logWarn(string.Format("File {0} not found", fileName));
									continue;
								}
								if (node.Previous != null)
								{
									fileName = node.Previous.Value;
									Logger.logDebug(string.Format("New name is {0}", fileName));
									readNewILogFileInfoList.Add(GetLogFileInfo(fileName));
								}
								else
								{
									Logger.logWarn(string.Format("No previous file for {0} found", fileName));
								}
							}
						}
					}
					else
					{
						Logger.logInfo(string.Format("{0} does not exist", fileName));
						lostILogFileInfoList.Add(logFileInfo);
						#if DEBUG
						// for better overview in logfile:
						//ILogFileInfo newILogFileInfo = new ILogFileInfo(fileName);
						//ReplaceBufferInfos(ILogFileInfo, newILogFileInfo);
						#endif
					}
				}
				if (lostILogFileInfoList.Count > 0)
				{
					Logger.logInfo("Deleting buffers for lost files");
					foreach (ILogFileInfo ILogFileInfo in lostILogFileInfoList)
					{
						//ILogFileInfoList.Remove(ILogFileInfo);
						LogBuffer lastBuffer = DeleteBuffersForInfo(ILogFileInfo, false);
						if (lastBuffer != null)
						{
							offset += lastBuffer.StartLine + lastBuffer.LineCount;
						}
					}
					_lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
					Logger.logInfo(string.Format("Adjusting StartLine values in {0} buffers by offset {1}", _bufferList.Count, offset));
					foreach (LogBuffer buffer in _bufferList)
					{
						SetNewStartLineForBuffer(buffer, buffer.StartLine - offset);
					}
					_lruCacheDictLock.ReleaseWriterLock();
					#if DEBUG
					if (_bufferList.Count > 0)
					{
						Logger.logInfo(string.Format("First buffer now has StartLine {0}", _bufferList[0].StartLine));
					}
					#endif
				}
				// Read anew all buffers following a buffer info that couldn't be matched with the corresponding existing file
				Logger.logInfo("Deleting buffers for files that must be re-read");
				foreach (ILogFileInfo ILogFileInfo in readNewILogFileInfoList)
				{
					DeleteBuffersForInfo(ILogFileInfo, true);
					//ILogFileInfoList.Remove(ILogFileInfo);
				}
				Logger.logInfo("Deleting buffers for the watched file");
				DeleteBuffersForInfo(_watchedILogFileInfo, true);
				int startLine = LineCount - 1;
				Logger.logInfo("Re-Reading files");
				foreach (ILogFileInfo ILogFileInfo in readNewILogFileInfoList)
				{
					//ILogFileInfo.OpenFile();
					ReadToBufferList(ILogFileInfo, 0, LineCount);
					//ILogFileInfoList.Add(ILogFileInfo);
					newFileInfoList.Add(ILogFileInfo);
				}
				//watchedILogFileInfo = ILogFileInfoList[ILogFileInfoList.Count - 1];
				_logFileInfoList = newFileInfoList;
				_watchedILogFileInfo = GetLogFileInfo(_watchedILogFileInfo.FullName);
				_logFileInfoList.Add(_watchedILogFileInfo);
				Logger.logInfo("Reading watched file");
				ReadToBufferList(_watchedILogFileInfo, 0, LineCount);
			}
			Logger.logInfo(string.Format("ShiftBuffers() end. offset={0}", offset));
			ReleaseBufferListWriterLock();
			return offset;
		}

		private void ReplaceBufferInfos(ILogFileInfo oldLogFileInfo, ILogFileInfo newLogFileInfo)
		{
			Logger.logDebug(string.Format("ReplaceBufferInfos() {0} -> {1}", oldLogFileInfo.FullName, newLogFileInfo.FullName));
			AcquireBufferListReaderLock();
			foreach (LogBuffer buffer in _bufferList)
			{
				if (buffer.FileInfo == oldLogFileInfo)
				{
					Logger.logDebug(string.Format("Buffer with startLine={0}, lineCount={1}, filePos={2}, size={3} gets new filename {4}", buffer.StartLine, buffer.LineCount, buffer.StartPos, buffer.Size, newLogFileInfo.FullName));
					buffer.FileInfo = newLogFileInfo;
				}
			}
			ReleaseBufferListReaderLock();
		}

		private LogBuffer DeleteBuffersForInfo(ILogFileInfo ILogFileInfo, bool matchNamesOnly)
		{
			Logger.logInfo(string.Format("Deleting buffers for file {0}", ILogFileInfo.FullName));
			LogBuffer lastRemovedBuffer = null;
			IList<LogBuffer> deleteList = new List<LogBuffer>();
			AcquireBufferListWriterLock();
			_lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			if (matchNamesOnly)
			{
				foreach (LogBuffer buffer in _bufferList)
				{
					if (buffer.FileInfo.FullName.ToLower().Equals(ILogFileInfo.FullName.ToLower()))
					{
						lastRemovedBuffer = buffer;
						deleteList.Add(buffer);
					}
				}
			}
			else
			{
				foreach (LogBuffer buffer in _bufferList)
				{
					if (buffer.FileInfo == ILogFileInfo)
					{
						lastRemovedBuffer = buffer;
						deleteList.Add(buffer);
					}
				}
			}
			foreach (LogBuffer buffer in deleteList)
			{
				RemoveFromBufferList(buffer);
			}
			_lruCacheDictLock.ReleaseWriterLock();
			ReleaseBufferListWriterLock();
			if (lastRemovedBuffer == null)
			{
				Logger.logInfo("lastRemovedBuffer is null");
			}
			else
			{
				Logger.logInfo(string.Format("lastRemovedBuffer: startLine={0}", lastRemovedBuffer.StartLine));
			}
			return lastRemovedBuffer;
		}

		/// <summary>
		/// The caller must have writer locks for lruCache and buffer list!
		/// </summary>
		/// <param name="buffer"></param>
		private void RemoveFromBufferList(LogBuffer buffer)
		{
			Util.AssertTrue(_lruCacheDictLock.IsWriterLockHeld, "No writer lock for lru cache");
			Util.AssertTrue(_bufferListLock.IsWriterLockHeld, "No writer lock for buffer list");
			_lruCacheDict.Remove(buffer.StartLine);
			_bufferList.Remove(buffer);
		}

		private void ReadToBufferList(ILogFileInfo logFileInfo, long filePos, int startLine)
		{
			#if DEBUG
			//Logger.logDebug("ReadToBufferList(): " + ILogFileInfo.FileName + ", filePos " + filePos + ", startLine: " + startLine);
			#endif
			Stream fileStream;
			ILogStreamReader reader = null;
			try
			{
				fileStream = logFileInfo.OpenStream();
				bool canSeek = fileStream.CanSeek;
			}
			catch (IOException fe)
			{
				Logger.logWarn(string.Format("IOException: {0}", fe.ToString()));
				_isDeleted = true;
				LineCount = 0;
				_currFileSize = 0;
				OnFileNotFound(); // notify LogWindow
				return;
			}
			try
			{
				reader = GetLogStreamReader(fileStream, EncodingOptions, UseNewReader);
				reader.Position = filePos;
				_fileLength = logFileInfo.Length;
				String line;
				int lineNum = startLine;
				LogBuffer logBuffer;
				AcquireBufferListReaderLock();
				if (_bufferList.Count == 0)
				{
					logBuffer = new LogBuffer(logFileInfo, MAX_LINES_PER_BUFFER);
					logBuffer.StartLine = startLine;
					logBuffer.StartPos = filePos;
					LockCookie cookie = UpgradeBufferListLockToWriter();
					AddBufferToList(logBuffer);
					DowngradeBufferListLockFromWriter(ref cookie);
					#if DEBUG
					//Logger.logDebug("ReadToBufferList(): new buffer created");
					#endif
				}
				else
				{
					logBuffer = _bufferList[_bufferList.Count - 1];
					//if (logBuffer.FileInfo != ILogFileInfo)
					if (!logBuffer.FileInfo.FullName.Equals(logFileInfo.FullName))
					{
						logBuffer = new LogBuffer(logFileInfo, MAX_LINES_PER_BUFFER);
						logBuffer.StartLine = startLine;
						logBuffer.StartPos = filePos;
						LockCookie cookie = UpgradeBufferListLockToWriter();
						AddBufferToList(logBuffer);
						DowngradeBufferListLockFromWriter(ref cookie);
						#if DEBUG
						//Logger.logDebug("ReadToBufferList(): new buffer created because new ILogFileInfo");
						#endif
					}
					_disposeLock.AcquireReaderLock(Timeout.Infinite);
					if (logBuffer.IsDisposed)
					{
						LockCookie cookie = _disposeLock.UpgradeToWriterLock(Timeout.Infinite);
						ReReadBuffer(logBuffer);
						_disposeLock.DowngradeFromWriterLock(ref cookie);
					}
					_disposeLock.ReleaseReaderLock();
				}

				Monitor.Enter(logBuffer); // Lock the buffer
				ReleaseBufferListReaderLock();
				int lineCount = logBuffer.LineCount;
				int droppedLines = logBuffer.PrevBuffersDroppedLinesSum;
				filePos = reader.Position;
				while (ReadLine(reader, logBuffer.StartLine + logBuffer.LineCount,
					logBuffer.StartLine + logBuffer.LineCount + droppedLines,
					out line))
				{
					if (_shouldStop)
					{
						Monitor.Exit(logBuffer);
						return;
					}

					if (line == null)
					{
						logBuffer.DroppedLinesCount = logBuffer.DroppedLinesCount + 1;
						droppedLines++;
						continue;
					}

					lineCount++;
					if (lineCount > MAX_LINES_PER_BUFFER && reader.IsBufferComplete)
					{
						OnLoadFile(new LoadFileEventArgs(logFileInfo.FullName, filePos, false, logFileInfo.Length, false));
						#if DEBUG
						//Logger.logDebug("ReadToBufferList(): new buffer created. lineCount: " + lineCount + ", lineNum:" + lineNum + ", text: " + line);
						#endif
						//logBuffer.Size = filePos - logBuffer.StartPos;
						Monitor.Exit(logBuffer);
						logBuffer = new LogBuffer(logFileInfo, MAX_LINES_PER_BUFFER);
						Monitor.Enter(logBuffer);
						logBuffer.StartLine = lineNum;
						logBuffer.StartPos = filePos;
						logBuffer.PrevBuffersDroppedLinesSum = droppedLines;
						AcquireBufferListWriterLock();
						AddBufferToList(logBuffer);
						ReleaseBufferListWriterLock();
						lineCount = 1;
					}
					logBuffer.AddLine(line, filePos);
					filePos = reader.Position;
					lineNum++;
				}
				logBuffer.Size = filePos - logBuffer.StartPos;
				Monitor.Exit(logBuffer);
				_isLineCountDirty = true;
				_currFileSize = reader.Position;
				CurrentEncoding = reader.Encoding; // Reader may have detected another encoding
				if (!_shouldStop)
				{
					OnLoadFile(new LoadFileEventArgs(logFileInfo.FullName, filePos, true, _fileLength, false));
					// Fire "Ready" Event
				}
				GC.KeepAlive(fileStream);
			}
			catch (IOException ioex)
			{
				Logger.logWarn(string.Format("{0}: {1}", ioex.GetType().Name, ioex.Message));
			}
			finally
			{
				fileStream.Close();
			}
		}

		private void AddBufferToList(LogBuffer logBuffer)
		{
			#if DEBUG
			Logger.logDebug(string.Format("AddBufferToList(): {0}/{1}/{2}", logBuffer.StartLine, logBuffer.LineCount, logBuffer.FileInfo.FullName));
			#endif
			_bufferList.Add(logBuffer);
			//UpdateLru(logBuffer);
			UpdateLruCache(logBuffer);
		}

		private void UpdateLruCache(LogBuffer logBuffer)
		{
			LogBufferCacheEntry cacheEntry;
			_lruCacheDictLock.AcquireReaderLock(Timeout.Infinite);
			if (_lruCacheDict.TryGetValue(logBuffer.StartLine, out cacheEntry))
			{
				cacheEntry.Touch();
			}
			else
			{
				LockCookie cookie = _lruCacheDictLock.UpgradeToWriterLock(Timeout.Infinite);
				if (!_lruCacheDict.TryGetValue(logBuffer.StartLine, out cacheEntry))  // #536: re-test, because multiple threads may have been waiting for writer lock
				{
					cacheEntry = new LogBufferCacheEntry();
					cacheEntry.LogBuffer = logBuffer;
					try
					{
						_lruCacheDict.Add(logBuffer.StartLine, cacheEntry);
					}
					catch (ArgumentException e)
					{
						#if DEBUG
						// there seems to be a bug with double added key
						Logger.logError(string.Format("Error in LRU cache: {0}", e.Message));
						Logger.logInfo("Added buffer:");
						DumpBufferInfos(logBuffer);
						LogBufferCacheEntry exisingEntry;
						if (_lruCacheDict.TryGetValue(logBuffer.StartLine, out exisingEntry))
						{
							Logger.logInfo("Existing buffer: ");
							DumpBufferInfos(exisingEntry.LogBuffer);
						}
						else
						{
							Logger.logWarn("Ooops? Cannot find the already existing entry in LRU.");
						}
						#endif
						_lruCacheDictLock.ReleaseLock();
						throw e;
					}
				}
				_lruCacheDictLock.DowngradeFromWriterLock(ref cookie);
			}
			_lruCacheDictLock.ReleaseReaderLock();
		}

		/// <summary>
		/// Sets a new start line in the given buffer and updates the LRU cache, if the buffer
		/// is present in the cache. The caller must have write lock for 'lruCacheDictLock';
		/// </summary>
		/// <param name="logBuffer"></param>
		/// <param name="newLineNum"></param>
		private void SetNewStartLineForBuffer(LogBuffer logBuffer, int newLineNum)
		{
			Util.AssertTrue(_lruCacheDictLock.IsWriterLockHeld, "No writer lock for lru cache");
			if (_lruCacheDict.ContainsKey(logBuffer.StartLine))
			{
				_lruCacheDict.Remove(logBuffer.StartLine);
				logBuffer.StartLine = newLineNum;
				LogBufferCacheEntry cacheEntry = new LogBufferCacheEntry();
				cacheEntry.LogBuffer = logBuffer;
				_lruCacheDict.Add(logBuffer.StartLine, cacheEntry);
			}
			else
			{
				logBuffer.StartLine = newLineNum;
			}
		}

		private void GarbageCollectLruCache()
		{
			#if DEBUG
			long startTime = Environment.TickCount;
			#endif
			Logger.logDebug("Starting garbage collection");
			int threshold = 10;
			_lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			int diff = 0;
			if (_lruCacheDict.Count - (MAX_BUFFERS + threshold) > 0)
			{
				diff = _lruCacheDict.Count - MAX_BUFFERS;
				#if DEBUG
				if (diff > 0)
				{
					Logger.logInfo(string.Format("Removing {0} entries from LRU cache for {1}", diff, Util.GetNameFromPath(_fileName)));
				}
				#endif
				SortedList<long, int> useSorterList = new SortedList<long, int>();
				// sort by usage counter
				foreach (LogBufferCacheEntry entry in _lruCacheDict.Values)
				{
					if (!useSorterList.ContainsKey(entry.LastUseTimeStamp))
					{
						useSorterList.Add(entry.LastUseTimeStamp, entry.LogBuffer.StartLine);
					}
				}
				// remove first <diff> entries (least usage)
				_disposeLock.AcquireWriterLock(Timeout.Infinite);
				for (int i = 0; i < diff; ++i)
				{
					if (i >= useSorterList.Count)
					{
						break;
					}
					int startLine = useSorterList.Values[i];
					LogBufferCacheEntry entry = _lruCacheDict[startLine];
					_lruCacheDict.Remove(startLine);
					entry.LogBuffer.DisposeContent();
				}
				_disposeLock.ReleaseWriterLock();
			}
			_lruCacheDictLock.ReleaseWriterLock();
			#if DEBUG
			if (diff > 0)
			{
				long endTime = Environment.TickCount;
				Logger.logInfo(string.Format("Garbage collector time: {0} ms.", endTime - startTime));
			}
			#endif
		}

		private void GarbageCollectorThreadProc()
		{
			while (!_shouldStop)
			{
				try
				{
					Thread.Sleep(10000);
				}
				catch (Exception)
				{
				}
				GarbageCollectLruCache();
			}
		}

		//    private void UpdateLru(LogBuffer logBuffer)
		//    {
		//      lock (monitor)
		//      {
		//        int index;
		//        if (lruDict.TryGetValue(logBuffer.StartLine, out index))
		//        {
		//          RemoveBufferFromLru(logBuffer, index);
		//          AddBufferToLru(logBuffer);
		//        }
		//        else
		//        {
		//          if (bufferLru.Count > MAX_BUFFERS - 1)
		//          {
		//            LogBuffer looser = bufferLru[0];
		//            if (looser != null)
		//            {
		//#if DEBUG
		//              Logger.logDebug("Disposing buffer: " + looser.StartLine + "/" + looser.LineCount + "/" + looser.FileInfo.FileName);
		//#endif
		//              looser.DisposeContent();
		//              RemoveBufferFromLru(looser);
		//            }
		//          }
		//          AddBufferToLru(logBuffer);
		//        }
		//      }
		//    }

		///// <summary>
		///// Removes a LogBuffer from the LRU. Note that the LogBuffer is searched in the lruDict
		///// via StartLine. So this property must have a consistent value.
		///// </summary>
		///// <param name="buffer"></param>
		//private void RemoveBufferFromLru(LogBuffer buffer)
		//{
		//  int index;
		//  lock (monitor)
		//  {
		//    if (lruDict.TryGetValue(buffer.StartLine, out index))
		//    {
		//      RemoveBufferFromLru(buffer, index);
		//    }
		//  }
		//}

		///// <summary>
		///// Removes a LogBuffer from the LRU with known index. Note that the LogBuffer is searched in the lruDict
		///// via StartLine. So this property must have a consistent value.
		///// </summary>
		///// <param name="buffer"></param>
		///// <param name="index"></param>
		//private void RemoveBufferFromLru(LogBuffer buffer, int index)
		//{
		//  lock (monitor)
		//  {
		//    bufferLru.RemoveAt(index);
		//    lruDict.Remove(buffer.StartLine);
		//    // adjust indizes, they have changed because of the remove 
		//    for (int i = index; i < bufferLru.Count; ++i)
		//    {
		//      lruDict[bufferLru[i].StartLine] = lruDict[bufferLru[i].StartLine] - 1;
		//    }
		//  }
		//}

		//private void AddBufferToLru(LogBuffer logBuffer)
		//{
		//  lock (monitor)
		//  {
		//    bufferLru.Add(logBuffer);
		//    int newIndex = bufferLru.Count - 1;
		//    lruDict[logBuffer.StartLine] = newIndex;
		//  }
		//}

		private void ClearLru()
		{
			//lock (monitor)
			//{
			//  foreach (LogBuffer buffer in bufferLru)
			//  {
			//    buffer.DisposeContent();
			//  }
			//  bufferLru.Clear();
			//  lruDict.Clear();
			//}
			Logger.logInfo("Clearing LRU cache.");
			_lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			_disposeLock.AcquireWriterLock(Timeout.Infinite);
			foreach (LogBufferCacheEntry entry in _lruCacheDict.Values)
			{
				entry.LogBuffer.DisposeContent();
			}
			_lruCacheDict.Clear();
			_disposeLock.ReleaseWriterLock();
			_lruCacheDictLock.ReleaseWriterLock();
			Logger.logInfo("Clearing done.");
		}

		private void ReReadBuffer(LogBuffer logBuffer)
		{
			#if DEBUG
			Logger.logInfo(string.Format("re-reading buffer: {0}/{1}/{2}", logBuffer.StartLine, logBuffer.LineCount, logBuffer.FileInfo.FullName));
			#endif
			try
			{
				Monitor.Enter(logBuffer);
				Stream fileStream = null;
				try
				{
					fileStream = logBuffer.FileInfo.OpenStream();
				}
				catch (IOException e)
				{
					Logger.logWarn(e);
					return;
				}
				try
				{
					ILogStreamReader reader = GetLogStreamReader(fileStream, EncodingOptions, UseNewReader);
					string line;
					long filePos = logBuffer.StartPos;
					reader.Position = logBuffer.StartPos;
					int maxLinesCount = logBuffer.LineCount;
					int lineCount = 0;
					int dropCount = logBuffer.PrevBuffersDroppedLinesSum;
					logBuffer.ClearLines();
					while (ReadLine(reader, logBuffer.StartLine + logBuffer.LineCount,
						logBuffer.StartLine + logBuffer.LineCount + dropCount,
						out line))
					{
						if (lineCount >= maxLinesCount)
						{
							break;
						}

						if (line == null)
						{
							dropCount++;
							continue;
						}

						logBuffer.AddLine(line, filePos);
						filePos = reader.Position;
						lineCount++;
					}
					if (maxLinesCount != logBuffer.LineCount)
					{
						Logger.logWarn(string.Format("LineCount in buffer differs after re-reading. old={0}, new={1}", maxLinesCount, logBuffer.LineCount));
					}
					if (dropCount - logBuffer.PrevBuffersDroppedLinesSum != logBuffer.DroppedLinesCount)
					{
						Logger.logWarn(string.Format("DroppedLinesCount in buffer differs after re-reading. old={0}, new={1}", logBuffer.DroppedLinesCount, dropCount));
						logBuffer.DroppedLinesCount = dropCount - logBuffer.PrevBuffersDroppedLinesSum;
					}
					GC.KeepAlive(fileStream);
				}
				catch (IOException e)
				{
					Logger.logWarn(e);
				}
				finally
				{
					fileStream.Close();
				}
			}
			finally
			{
				Monitor.Exit(logBuffer);
			}
		}

		private LogBuffer getBufferForLine(int lineNum)
		{
			#if DEBUG
			long startTime = Environment.TickCount;
			#endif
			LogBuffer logBuffer = null;
			AcquireBufferListReaderLock();
			//if (lineNum == lastReturnedLineNumForBuffer)
			//{
			//  return lastReturnedBuffer;
			//}

			//int startIndex = lineNum / LogBuffer.MAX_LINES;  // doesn't work anymore since XML buffer may contain more lines than MAX_LINES
			int startIndex = 0;
			int count = _bufferList.Count;
			for (int i = startIndex; i < count; ++i)
			{
				logBuffer = _bufferList[i];
				if (lineNum >= logBuffer.StartLine && lineNum < logBuffer.StartLine + logBuffer.LineCount)
				{
					//UpdateLru(logBuffer);
					UpdateLruCache(logBuffer);
					//lastReturnedLineNumForBuffer = lineNum;
					//lastReturnedBuffer = logBuffer;
					break;
				}
			}
			#if DEBUG
			long endTime = Environment.TickCount;
			//Logger.logDebug("getBufferForLine(" + lineNum + ") duration: " + ((endTime - startTime)) + " ms. Buffer start line: " + logBuffer.StartLine);
			#endif
			ReleaseBufferListReaderLock();
			return logBuffer;
		}

		public string GetLogLine(int lineNum)
		{
			return GetLogLineInternal(lineNum);
		}

		/// <summary>
		/// Get the text content of the given line number.
		/// The actual work is done in an async thread. This method waits for thread completion for only 1 second. If the async
		/// thread has not returned, the method will return <code>null</code>. This is because this method is also called from GUI thread
		/// (e.g. LogWindow draw events). Under some circumstances, repeated calls to this method would lead the GUI to freeze. E.g. when 
		/// trying to re-load content from disk but the file was deleted. Especially on network shares.
		/// </summary>
		/// <remarks>
		/// Once the method detects a timeout it will enter a kind of 'fast fail mode'. That means all following calls will be returned with
		/// <code>null</code> immediately (without 1 second wait). A background call to GetLogLineInternal() will check if a result is available.
		/// If so, the 'fast fail mode' is switched off. In most cases a fail is caused by a deleted file. But it may also be caused by slow
		/// network connections. So all this effort is needed to prevent entering an endless 'fast fail mode' just because of temporary problems.
		/// </remarks>
		/// <param name="lineNum">line to retrieve</param>
		/// <returns></returns>
		public string GetLogLineWithWait(int lineNum)
		{
			const int WAIT_TIME = 1000;
			GetLogLineFx logLineFx = new GetLogLineFx(GetLogLineInternal);
			string result = null;

			if (!_isFastFailOnGetLogLine)
			{
				IAsyncResult asyncResult = logLineFx.BeginInvoke(lineNum, null, null);
				if (asyncResult.AsyncWaitHandle.WaitOne(WAIT_TIME, false))
				{
					result = logLineFx.EndInvoke(asyncResult);
					_isFastFailOnGetLogLine = false;
				}
				else
				{
					logLineFx.EndInvoke(asyncResult);    // must be called according to MSDN docs... :(
					_isFastFailOnGetLogLine = true;
					#if DEBUG
					Logger.logDebug(string.Format("No result after {0}ms. Returning <null>.", WAIT_TIME));
					#endif
				}
			}
			else
			{
				Logger.logDebug("Fast failing GetLogLine()");
				if (!_isFailModeCheckCallPending)
				{
					_isFailModeCheckCallPending = true;
					IAsyncResult asyncResult = logLineFx.BeginInvoke(lineNum, new AsyncCallback(GetLineFinishedCallback), logLineFx);
				}
			}
			return result;
		}

		/// <summary>
		/// Async callback used to check if the GetLogLine() call is succeeding again after a detected timeout.
		/// </summary>
		private void GetLineFinishedCallback(IAsyncResult res)
		{
			_isFailModeCheckCallPending = false;
			GetLogLineFx logLineFx = (GetLogLineFx)res.AsyncState;
			string line = logLineFx.EndInvoke(res);
			if (line != null)
			{
				Logger.logDebug("'isFastFailOnGetLogLine' flag was reset");
				_isFastFailOnGetLogLine = false;
			}
			Logger.logDebug("'isLogLineCallPending' flag was reset.");
		}

		public string GetLogLineInternal(int lineNum)
		{
			if (_isDeleted)
			{
				#if DEBUG
				Logger.logDebug(string.Format("Returning null for line {0} because file is deleted.", lineNum));
				#endif
				// fast fail if dead file was detected. Prevents repeated lags in GUI thread caused by callbacks from control (e.g. repaint)
				return null;
			}

			AcquireBufferListReaderLock();
			LogBuffer logBuffer = getBufferForLine(lineNum);
			if (logBuffer == null)
			{
				ReleaseBufferListReaderLock();
				Logger.logError(string.Format("Cannot find buffer for line {0}, file: {1}{2}", lineNum, _fileName, IsMultiFile ? " (MultiFile)" : ""));
				return null;
			}
			// disposeLock prevents that the garbage collector is disposing just in the moment we use the buffer
			string line = null;
			_disposeLock.AcquireReaderLock(Timeout.Infinite);
			if (logBuffer.IsDisposed)
			{
				LockCookie cookie = _disposeLock.UpgradeToWriterLock(Timeout.Infinite);
				lock (logBuffer.FileInfo)
				{
					ReReadBuffer(logBuffer);
				}
				_disposeLock.DowngradeFromWriterLock(ref cookie);
			}
			line = logBuffer.GetLineOfBlock(lineNum - logBuffer.StartLine);
			_disposeLock.ReleaseReaderLock();
			ReleaseBufferListReaderLock();
			return line;
		}

		/// <summary>
		/// Returns the file name of the actual file for the given line. Needed for MultiFile.
		/// </summary>
		/// <param name="lineNum"></param>
		/// <returns></returns>
		public string GetLogFileNameForLine(int lineNum)
		{
			AcquireBufferListReaderLock();
			LogBuffer logBuffer = getBufferForLine(lineNum);
			string fileName = logBuffer != null ? logBuffer.FileInfo.FullName : null;
			ReleaseBufferListReaderLock();
			return fileName;
		}

		/// <summary>
		/// Returns the ILogFileInfo for the actual file for the given line. Needed for MultiFile.
		/// </summary>
		/// <param name="lineNum"></param>
		/// <returns></returns>
		public ILogFileInfo GetLogFileInfoForLine(int lineNum)
		{
			AcquireBufferListReaderLock();
			LogBuffer logBuffer = getBufferForLine(lineNum);
			ILogFileInfo info = logBuffer != null ? logBuffer.FileInfo : null;
			ReleaseBufferListReaderLock();
			return info;
		}

		/// <summary>
		/// Returns the line number (starting from the given number) where the next multi file
		/// starts.
		/// </summary>
		/// <param name="lineNum"></param>
		/// <returns></returns>
		public int GetNextMultiFileLine(int lineNum)
		{
			int result = -1;
			AcquireBufferListReaderLock();
			LogBuffer logBuffer = getBufferForLine(lineNum);
			if (logBuffer != null)
			{
				int index = _bufferList.IndexOf(logBuffer);
				if (index != -1)
				{
					for (int i = index; i < _bufferList.Count; ++i)
					{
						if (_bufferList[i].FileInfo != logBuffer.FileInfo)
						{
							result = _bufferList[i].StartLine;
							break;
						}
					}
				}
			}
			ReleaseBufferListReaderLock();
			return result;
		}

		public int GetPrevMultiFileLine(int lineNum)
		{
			int result = -1;
			AcquireBufferListReaderLock();
			LogBuffer logBuffer = getBufferForLine(lineNum);
			if (logBuffer != null)
			{
				int index = _bufferList.IndexOf(logBuffer);
				if (index != -1)
				{
					for (int i = index; i >= 0; --i)
					{
						if (_bufferList[i].FileInfo != logBuffer.FileInfo)
						{
							result = _bufferList[i].StartLine + _bufferList[i].LineCount;
							break;
						}
					}
				}
			}
			ReleaseBufferListReaderLock();
			return result;
		}

		/// <summary>
		/// Returns the actual line number in the file for the given 'virtual line num'.
		/// This is needed for multi file mode. 'Virtual' means that the given line num is a line
		/// number in the collections of the files currently viewed together in multi file mode as one large virtual file.
		/// This method finds the real file for the line number and maps the line number to the correct position
		/// in that file. This is needed when launching external tools to provide correct line number arguments.
		/// </summary>
		/// <param name="lineNum"></param>
		/// <returns></returns>
		public int GetRealLineNumForVirtualLineNum(int lineNum)
		{
			AcquireBufferListReaderLock();
			LogBuffer logBuffer = getBufferForLine(lineNum);
			int result = -1;
			if (logBuffer != null)
			{
				logBuffer = GetFirstBufferForFileByLogBuffer(logBuffer);
				if (logBuffer != null)
				{
					result = lineNum - logBuffer.StartLine;
				}
			}
			ReleaseBufferListReaderLock();
			return result;
		}

		private LogBuffer GetFirstBufferForFileByLogBuffer(LogBuffer logBuffer)
		{
			ILogFileInfo info = logBuffer.FileInfo;
			AcquireBufferListReaderLock();
			int index = _bufferList.IndexOf(logBuffer);
			if (index == -1)
			{
				ReleaseBufferListReaderLock();
				return null;
			}
			LogBuffer resultBuffer = logBuffer;
			while (true)
			{
				index--;
				if (index < 0 || _bufferList[index].FileInfo != info)
				{
					break;
				}
				resultBuffer = _bufferList[index];
			}
			ReleaseBufferListReaderLock();
			return resultBuffer;
		}

		public void startMonitoring()
		{
			Logger.logInfo("startMonitoring()");
			_monitorThread = new Thread(new ThreadStart(MonitorThreadProc));
			_monitorThread.IsBackground = true;
			_shouldStop = false;
			_monitorThread.Start();
		}

		public void stopMonitoring()
		{
			Logger.logInfo("stopMonitoring()");
			_shouldStop = true;

			System.Threading.Thread.Sleep(_watchedILogFileInfo.PollInterval); // leave time for the threads to stop by themselves

			if (_monitorThread != null)
			{
				if (_monitorThread.IsAlive) // if thread has not finished, abort it
				{
					_monitorThread.Interrupt();
					_monitorThread.Abort();
					_monitorThread.Join();
				}
			}
			if (_garbageCollectorThread != null)
			{
				if (_garbageCollectorThread.IsAlive) // if thread has not finished, abort it
				{
					_garbageCollectorThread.Interrupt();
					_garbageCollectorThread.Abort();
					_garbageCollectorThread.Join();
				}
			}
			//loadThread = null;
			_monitorThread = null;
			_garbageCollectorThread = null; // preventive call
			CloseFiles();
		}

		/// <summary>
		/// calls stopMonitoring() in a background thread and returns to the caller immediately. 
		/// This is useful for a fast responding GUI (e.g. when closing a file tab)
		/// </summary>
		public void StopMonitoringAsync()
		{
			Thread stopperThread = new Thread(new ThreadStart(stopMonitoring));
			stopperThread.IsBackground = true;
			stopperThread.Start();
		}

		/// <summary>
		/// Deletes all buffer lines and disposes their content. Use only when the LogfileReader
		/// is about to be closed!
		/// </summary>
		public void DeleteAllContent()
		{
			if (_contentDeleted)
			{
				Logger.logDebug(string.Format("Buffers for {0} already deleted.", Util.GetNameFromPath(_fileName)));
				return;
			}
			Logger.logInfo(string.Format("Deleting all log buffers for {0}. Used mem: {1}", Util.GetNameFromPath(_fileName), GC.GetTotalMemory(true).ToString("N0")));
			AcquireBufferListWriterLock();
			_lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			_disposeLock.AcquireWriterLock(Timeout.Infinite);

			foreach (LogBuffer logBuffer in _bufferList)
			{
				if (!logBuffer.IsDisposed)
				{
					logBuffer.DisposeContent();
				}
			}
			_lruCacheDict.Clear();
			_bufferList.Clear();

			_disposeLock.ReleaseWriterLock();
			_lruCacheDictLock.ReleaseWriterLock();
			ReleaseBufferListWriterLock();
			GC.Collect();
			_contentDeleted = true;
			Logger.logInfo(string.Format("Deleting complete. Used mem: {0}", GC.GetTotalMemory(true).ToString("N0")));
		}

		private void MonitorThreadProc()
		{
			Thread.CurrentThread.Name = "MonitorThread";
			//IFileSystemPlugin fs = PluginRegistry.GetInstance().FindFileSystemForUri(watchedILogFileInfo.FullName);
			Logger.logInfo(string.Format("MonitorThreadProc() for file {0}", _watchedILogFileInfo.FullName));

			long oldSize = 0;
			try
			{
				OnLoadingStarted(new LoadFileEventArgs(_fileName, 0, false, 0, false));
				ReadFiles();
				if (!_isDeleted)
				{
					oldSize = _fileLength;
					OnLoadingFinished();
				}
			}
			catch (Exception e)
			{
				Logger.logError(e.Message);
			}

			while (!_shouldStop)
			{
				try
				{
					int pollInterval = _watchedILogFileInfo.PollInterval;
					//#if DEBUG
					//          if (Logger.IsDebug)
					//          {
					//            Logger.logDebug("Poll interval for " + fileName + ": " + pollInterval);
					//          }
					//#endif
					Thread.Sleep(pollInterval);
				}
				catch (Exception e)
				{
					Logger.logError(e.Message);
				}
				if (_shouldStop)
				{
					return;
				}
				try
				{
					if (_watchedILogFileInfo.FileHasChanged())
					{
						_fileLength = _watchedILogFileInfo.Length;
						if (_fileLength == -1)
						{
							MonitoredFileNotFound();
						}
						else
						{
							oldSize = _fileLength;
							FileChanged();
						}
					}
				}
				catch (FileNotFoundException)
				{
					MonitoredFileNotFound();
				}
			}
		}

		private void MonitoredFileNotFound()
		{
			long oldSize;
			if (!_isDeleted)
			{
				Logger.logDebug("File not FileNotFoundException catched. Switching to 'deleted' mode.");
				_isDeleted = true;
				oldSize = _fileLength = -1;
				_currFileSize = 0;
				OnFileNotFound(); // notify LogWindow
			}
			#if DEBUG
			else
			{
				Logger.logDebug("File not FileNotFoundException catched. Already in deleted mode.");
			}
			#endif
		}

		private void FileChanged()
		{
			if (_isDeleted)
			{
				OnRespawned();
				// prevent size update events. The window should reload the complete file.
				_currFileSize = _fileLength;
			}
			long newSize = _fileLength;
			//if (currFileSize != newSize)
			{
				Logger.logInfo(string.Format("file size changed. new size={0}, file: {1}", newSize, _fileName));
				FireChangeEvent();
			}
		}

		private void FireChangeEvent()
		{
			LogEventArgs args = new LogEventArgs();
			args.PrevFileSize = _currFileSize;
			args.PrevLineCount = LineCount;
			long newSize = _fileLength;
			if (newSize < _currFileSize || _isDeleted)
			{
				Logger.logInfo(string.Format("File was created anew: new size={0}, oldSize={1}", newSize, _currFileSize));
				// Fire "New File" event
				_currFileSize = 0;
				LineCount = 0;
				try
				{
					if (!IsMultiFile)
					{
						// ReloadBufferList();  // removed because reloading is triggered by owning LogWindow
						// Trigger "new file" handling (reload)
						OnLoadFile(new LoadFileEventArgs(_fileName, 0, true, _fileLength, true));

						if (_isDeleted)
						{
							args.FileSize = newSize;
							args.LineCount = LineCount;
							if (args.PrevLineCount != args.LineCount && !_shouldStop)
							{
								OnFileSizeChanged(args);
							}
						}
						_isDeleted = false;
					}
					else
					{
						int offset = ShiftBuffers();
						//currFileSize = newSize;    // removed because ShiftBuffers() calls ReadToBuffer() which will set the actual read size
						args.FileSize = newSize;
						args.LineCount = LineCount;
						args.IsRollover = true;
						args.RolloverOffset = offset;
						_isDeleted = false;
						if (!_shouldStop)
						{
							OnFileSizeChanged(args);
						}
					}
				}
				catch (FileNotFoundException e)
				{
					// trying anew in next poll intervall. So let currFileSize untouched.
					Logger.logWarn(e.ToString());
					return;
				}
			}
			else
			{
				ReadToBufferList(_watchedILogFileInfo, _currFileSize > 0 ? _currFileSize : _currFileSize, LineCount);
				args.FileSize = newSize;
				args.LineCount = LineCount;
				//if (args.PrevLineCount != args.LineCount && !shouldStop)
				OnFileSizeChanged(args);
			}
		}

		public delegate void FileSizeChangedEventHandler(object sender, LogEventArgs e);

		public event FileSizeChangedEventHandler FileSizeChanged;

		protected virtual void OnFileSizeChanged(LogEventArgs e)
		{
			if (FileSizeChanged != null)
			{
				FileSizeChanged(this, e);
			}
		}

		public delegate void BlockLoadedEventHandler(object sender, LoadFileEventArgs e);

		public event BlockLoadedEventHandler LoadFile;

		protected virtual void OnLoadFile(LoadFileEventArgs e)
		{
			if (LoadFile != null)
			{
				LoadFile(this, e);
			}
		}

		public delegate void LoadingStartedEventHandler(object sender, LoadFileEventArgs e);

		public event LoadingStartedEventHandler LoadingStarted;

		protected virtual void OnLoadingStarted(LoadFileEventArgs e)
		{
			if (LoadingStarted != null)
			{
				LoadingStarted(this, e);
			}
		}

		public delegate void FinishedLoadingEventHandler(object sender, EventArgs e);

		public event FinishedLoadingEventHandler LoadingFinished;

		protected virtual void OnLoadingFinished()
		{
			if (LoadingFinished != null)
			{
				LoadingFinished(this, new EventArgs());
			}
		}

		public delegate void FileNotFoundEventHandler(object sender, EventArgs e);

		public event FileNotFoundEventHandler FileNotFound;

		protected virtual void OnFileNotFound()
		{
			if (FileNotFound != null)
			{
				FileNotFound(this, new EventArgs());
			}
		}

		public delegate void FileRespawnedEventHandler(object sender, EventArgs e);

		public event FileRespawnedEventHandler Respawned;

		protected virtual void OnRespawned()
		{
			Logger.logInfo("OnRespawned()");
			if (Respawned != null)
			{
				Respawned(this, new EventArgs());
			}
		}

		public int LineCount
		{
			get
			{
				if (_isLineCountDirty)
				{
					_currLineCount = 0;
					AcquireBufferListReaderLock();
					foreach (LogBuffer buffer in _bufferList)
					{
						_currLineCount += buffer.LineCount;
					}
					ReleaseBufferListReaderLock();
					_isLineCountDirty = false;
				}
				return _currLineCount;
			}
			set
			{
				_currLineCount = value;
			}
		}

		public bool IsMultiFile
		{
			get
			{
				return _isMultiFile;
			}
		}

		/// <summary>
		/// Explicit change the encoding.
		/// </summary>
		/// <param name="encoding"></param>
		public void ChangeEncoding(Encoding encoding)
		{
			CurrentEncoding = encoding;
			EncodingOptions.Encoding = encoding;
			ResetBufferCache();
			ClearLru();
		}

		public Encoding CurrentEncoding { get; private set; }

		public long FileSize
		{
			get
			{
				return _currFileSize;
			}
		}

		public bool IsXmlMode
		{
			get
			{
				return _isXmlMode;
			}
			set
			{
				_isXmlMode = value;
			}
		}

		public IXmlLogConfiguration XmlLogConfig { get; set; }

		private ILogStreamReader GetLogStreamReader(Stream stream, EncodingOptions encodingOptions, bool useNewReader)
		{
			if (IsXmlMode)
			{
				return new XmlBlockSplitter(new XmlLogReader(new PositionAwareStreamReader(stream, encodingOptions, useNewReader)), XmlLogConfig);
			}
			else
			{
				return new PositionAwareStreamReader(stream, encodingOptions, useNewReader);
			}
		}

		public IPreProcessColumnizer PreProcessColumnizer
		{
			get
			{
				return _preProcessColumnizer;
			}
			set
			{
				_preProcessColumnizer = value;
			}
		}

		public EncodingOptions EncodingOptions
		{
			get
			{
				return _encodingOptions;
			}
			set
			{
				{
					_encodingOptions = new EncodingOptions();
					_encodingOptions.DefaultEncoding = value.DefaultEncoding;
					_encodingOptions.Encoding = value.Encoding;
				}
			}
		}

		public bool UseNewReader { get; set; }

		private bool ReadLine(ILogStreamReader reader, int lineNum, int realLineNum, out string outLine)
		{
			string line = null;
			try
			{
				line = reader.ReadLine();
			}
			catch (IOException e)
			{
				Logger.logWarn(e.Message);
			}
			catch (NotSupportedException e)
			{
				// Bug#11: "Lesevorgänge werden vom Stream nicht unterstützt"
				// Nicht reproduzierbar. Wahrscheinlich, wenn File in ungünstigem Moment (nach dem Öffnen)
				// gelöscht wird (rolling). Wird hier als EOF behandelt.
				Logger.logWarn(e.Message);
			}
			if (line == null)   // EOF or catched Exception
			{
				outLine = null;
				return false;
			}

			if (PreProcessColumnizer != null)
			{
				line = PreProcessColumnizer.PreProcessLine(line, lineNum, realLineNum);
			}
			outLine = line;
			return true;
		}

		private void AcquireBufferListReaderLock()
		{
			try
			{
				_bufferListLock.AcquireReaderLock(10000);
				#if DEBUG && TRACE_LOCKS
		StackTrace st = new StackTrace(true);
		StackFrame callerFrame = st.GetFrame(2);
		bufferListLockInfo = "Read lock from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
				#endif
			}
			catch (ApplicationException)
			{
				Logger.logWarn("Reader lock wait for bufferList timed out. Now trying infinite.");
				#if DEBUG && TRACE_LOCKS
		Logger.logInfo(bufferListLockInfo);
				#endif
				_bufferListLock.AcquireReaderLock(Timeout.Infinite);
			}
		}

		private void ReleaseBufferListReaderLock()
		{
			_bufferListLock.ReleaseReaderLock();
		}

		private void AcquireBufferListWriterLock()
		{
			try
			{
				_bufferListLock.AcquireWriterLock(10000);
				#if DEBUG && TRACE_LOCKS
		StackTrace st = new StackTrace(true);
		StackFrame callerFrame = st.GetFrame(1);
		bufferListLockInfo = "Write lock from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
		callerFrame.GetFileName();
				#endif
			}
			catch (ApplicationException)
			{
				Logger.logWarn("Writer lock wait for bufferList timed out. Now trying infinite.");
				#if DEBUG && TRACE_LOCKS
		Logger.logInfo(bufferListLockInfo);
				#endif
				_bufferListLock.AcquireWriterLock(Timeout.Infinite);
			}
		}

		private void ReleaseBufferListWriterLock()
		{
			_bufferListLock.ReleaseWriterLock();
		}

		private LockCookie UpgradeBufferListLockToWriter()
		{
			try
			{
				LockCookie cookie = _bufferListLock.UpgradeToWriterLock(10000);
				#if DEBUG && TRACE_LOCKS
		StackTrace st = new StackTrace(true);
		StackFrame callerFrame = st.GetFrame(2);
		bufferListLockInfo += ", upgraded to writer from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
				#endif
				return cookie;
			}
			catch (ApplicationException)
			{
				Logger.logWarn("Writer lock update wait for bufferList timed out. Now trying infinite.");
				#if DEBUG && TRACE_LOCKS
		Logger.logInfo(bufferListLockInfo);
				#endif
				return _bufferListLock.UpgradeToWriterLock(Timeout.Infinite);
			}
		}

		private void DowngradeBufferListLockFromWriter(ref LockCookie cookie)
		{
			_bufferListLock.DowngradeFromWriterLock(ref cookie);
			#if DEBUG && TRACE_LOCKS
	  StackTrace st = new StackTrace(true);
	  StackFrame callerFrame = st.GetFrame(2);
	  bufferListLockInfo += ", downgraded to reader from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
			#endif
		}

		/// <summary>
		/// For unit tests only.
		/// </summary>
		/// <returns></returns>
		public IList<ILogFileInfo> GetLogFileInfoList()
		{
			return _logFileInfoList;
		}

		/// <summary>
		/// For unit tests only 
		/// </summary>
		/// <returns></returns>
		public IList<LogBuffer> GetBufferList()
		{
			return _bufferList;
		}
		
		#if DEBUG
		
		internal void LogBufferInfoForLine(int lineNum)
		{
			AcquireBufferListReaderLock();
			LogBuffer buffer = getBufferForLine(lineNum);
			if (buffer == null)
			{
				ReleaseBufferListReaderLock();
				Logger.logError(string.Format("Cannot find buffer for line {0}, file: {1}{2}", lineNum, _fileName, IsMultiFile ? " (MultiFile)" : ""));
				return;
			}
			Logger.logInfo("-----------------------------------");
			_disposeLock.AcquireReaderLock(Timeout.Infinite);
			Logger.logInfo(string.Format("Buffer info for line {0}", lineNum));
			DumpBufferInfos(buffer);
			Logger.logInfo(string.Format("File pos for current line: {0}", buffer.GetFilePosForLineOfBlock(lineNum - buffer.StartLine)));
			_disposeLock.ReleaseReaderLock();
			Logger.logInfo("-----------------------------------");
			ReleaseBufferListReaderLock();
		}
		
		#endif
		
		#if DEBUG
		private void DumpBufferInfos(LogBuffer buffer)
		{
			Logger.logInfo(string.Format("StartLine: {0}", buffer.StartLine));
			Logger.logInfo(string.Format("LineCount: {0}", buffer.LineCount));
			Logger.logInfo(string.Format("StartPos: {0}", buffer.StartPos));
			Logger.logInfo(string.Format("Size: {0}", buffer.Size));
			Logger.logInfo(string.Format("Disposed: {0}", buffer.IsDisposed ? "yes" : "no"));
			Logger.logInfo(string.Format("DisposeCount: {0}", buffer.DisposeCount));
			Logger.logInfo(string.Format("File: {0}", buffer.FileInfo.FullName));
		}
		
		#endif
		
		#if DEBUG
		internal void LogBufferDiagnostic()
		{
			Logger.logInfo("-------- Buffer diagnostics -------");
			_lruCacheDictLock.AcquireReaderLock(Timeout.Infinite);
			int cacheCount = _lruCacheDict.Count;
			Logger.logInfo(string.Format("LRU entries: {0}", cacheCount));
			_lruCacheDictLock.ReleaseReaderLock();
			
			AcquireBufferListReaderLock();
			Logger.logInfo(string.Format("File: {0}", _fileName));
			Logger.logInfo(string.Format("Buffer count: {0}", _bufferList.Count));
			Logger.logInfo(string.Format("Disposed buffers: {0}", _bufferList.Count - cacheCount));
			int lineNum = 0;
			long disposeSum = 0;
			long maxDispose = 0;
			long minDispose = Int32.MaxValue;
			for (int i = 0; i < _bufferList.Count; ++i)
			{
				LogBuffer buffer = _bufferList[i];
				_disposeLock.AcquireReaderLock(Timeout.Infinite);
				if (buffer.StartLine != lineNum)
				{
					Logger.logError(string.Format("Start line of buffer is: {0}, expected: {1}", buffer.StartLine, lineNum));
					Logger.logInfo("Info of buffer follows:");
					DumpBufferInfos(buffer);
				}
				lineNum += buffer.LineCount;
				disposeSum += buffer.DisposeCount;
				maxDispose = Math.Max(maxDispose, buffer.DisposeCount);
				minDispose = Math.Min(minDispose, buffer.DisposeCount);
				_disposeLock.ReleaseReaderLock();
			}
			ReleaseBufferListReaderLock();
			Logger.logInfo(string.Format("Dispose count sum is: {0}", disposeSum));
			Logger.logInfo(string.Format("Min dispose count is: {0}", minDispose));
			Logger.logInfo(string.Format("Max dispose count is: {0}", maxDispose));
			Logger.logInfo("-----------------------------------");
		}
		#endif
	}
}