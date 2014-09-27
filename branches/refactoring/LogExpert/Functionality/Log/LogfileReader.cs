using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;

namespace LogExpert
{
	public class LogfileReader
	{
		int MAX_BUFFERS = 10;
		int MAX_LINES_PER_BUFFER = 100;

		string fileName;
		long currFileSize = 0;
		int currLineCount = 0;
		IList<LogBuffer> bufferList;
		IList<LogBuffer> bufferLru;
		private Dictionary<int, LogBufferCacheEntry> lruCacheDict;

		private ReaderWriterLock lruCacheDictLock;
		private ReaderWriterLock bufferListLock;
		private ReaderWriterLock disposeLock;

		Thread monitorThread = null;
		Thread garbageCollectorThread = null;
		private bool shouldStop;
		private long fileLength;
		private bool isDeleted;
		private bool isFastFailOnGetLogLine = false;
		private bool isFailModeCheckCallPending = false;
		ILogFileInfo watchedILogFileInfo;
		IList<ILogFileInfo> logFileInfoList = new List<ILogFileInfo>();
		private bool isLineCountDirty = true;
		private bool isMultiFile = false;
		private EncodingOptions encodingOptions;
		private bool isXmlMode = false;
		private IPreProcessColumnizer preProcessColumnizer = null;
		private bool contentDeleted = false;
		private MultifileOptions mutlifileOptions;

		private delegate string GetLogLineFx(int lineNum);

		Object monitor = new Object();

		public LogfileReader(string fileName, EncodingOptions encodingOptions, bool multiFile, int bufferCount, int linesPerBuffer, MultifileOptions mutlifileOptions)
		{
			if (fileName == null)
				return;
			this.fileName = fileName;
			this.EncodingOptions = encodingOptions;
			this.isMultiFile = multiFile;
			this.MAX_BUFFERS = bufferCount;
			this.MAX_LINES_PER_BUFFER = linesPerBuffer;
			this.mutlifileOptions = mutlifileOptions;

			InitLruBuffers();

			if (multiFile)
			{
				ILogFileInfo info = GetLogFileInfo(fileName);
				RolloverFilenameHandler rolloverHandler = new RolloverFilenameHandler(info, this.mutlifileOptions);
				LinkedList<string> nameList = rolloverHandler.GetNameList();

				ILogFileInfo fileInfo = null;
				foreach (string name in nameList)
				{
					fileInfo = AddFile(name);
				}
				this.watchedILogFileInfo = fileInfo;  // last added file in the list is the watched file
			}
			else
			{
				this.watchedILogFileInfo = AddFile(fileName);
			}
			StartGCThread();
		}

		public LogfileReader(string[] fileNames, EncodingOptions encodingOptions, int bufferCount, int linesPerBuffer, MultifileOptions mutlifileOptions)
		{
			if (fileNames == null || fileNames.Length < 1)
				return;
			this.EncodingOptions = encodingOptions;
			this.isMultiFile = true;
			this.MAX_BUFFERS = bufferCount;
			this.MAX_LINES_PER_BUFFER = linesPerBuffer;
			this.mutlifileOptions = mutlifileOptions;

			InitLruBuffers();

			ILogFileInfo fileInfo = null;
			foreach (string name in fileNames)
			{
				fileInfo = AddFile(name);
			}
			this.watchedILogFileInfo = fileInfo;
			this.fileName = fileInfo.FullName;

			StartGCThread();
		}

		~LogfileReader()
		{
			DeleteAllContent();
		}

		private void InitLruBuffers()
		{
			this.bufferList = new List<LogBuffer>();
			this.bufferLru = new List<LogBuffer>(this.MAX_BUFFERS + 1);
			//this.lruDict = new Dictionary<int, int>(this.MAX_BUFFERS + 1);  // key=startline, value = index in bufferLru
			this.lruCacheDict = new Dictionary<int, LogBufferCacheEntry>(this.MAX_BUFFERS + 1);
			this.lruCacheDictLock = new ReaderWriterLock();
			this.bufferListLock = new ReaderWriterLock();
			this.disposeLock = new ReaderWriterLock();
		}

		private void StartGCThread()
		{
			this.garbageCollectorThread = new Thread(new ThreadStart(this.GarbageCollectorThreadProc));
			this.garbageCollectorThread.IsBackground = true;
			this.garbageCollectorThread.Start();
		}

		public ILogFileInfo AddFile(string fileName)
		{
			Logger.logInfo("Adding file to ILogFileInfoList: " + fileName);
			ILogFileInfo info = GetLogFileInfo(fileName);
			this.logFileInfoList.Add(info);
			return info;
		}

		private void ResetBufferCache()
		{
			this.currFileSize = 0;
			this.LineCount = 0;
			//this.lastReturnedLine = "";
			//this.lastReturnedLineNum = -1;
			//this.lastReturnedLineNumForBuffer = -1;
		}

		private void CloseFiles()
		{
			//foreach (ILogFileInfo info in this.ILogFileInfoList)
			//{
			//  info.CloseFile();
			//}
			this.currFileSize = 0;
			this.LineCount = 0;
			//this.lastReturnedLine = "";
			//this.lastReturnedLineNum = -1;
			//this.lastReturnedLineNumForBuffer = -1;
		}

		private ILogFileInfo GetLogFileInfo(string fileNameOrUri)
		{
			IFileSystemPlugin fs = PluginRegistry.GetInstance().FindFileSystemForUri(fileNameOrUri);
			if (fs == null)
			{
				throw new LogFileException("No file system plugin found for " + fileNameOrUri);
			}
			ILogFileInfo logFileInfo = fs.GetLogfileInfo(fileNameOrUri);
			if (logFileInfo == null)
			{
				throw new LogFileException("Cannot find " + fileNameOrUri);
			}
			return logFileInfo;
		}

		/// <summary>
		/// Public for unit test reasons
		/// </summary>
		public void ReadFiles()
		{
			this.currFileSize = 0;
			this.LineCount = 0;
			//this.lastReturnedLine = "";
			//this.lastReturnedLineNum = -1;
			//this.lastReturnedLineNumForBuffer = -1;
			this.isDeleted = false;
			ClearLru();
			AcquireBufferListWriterLock();
			this.bufferList.Clear();
			ReleaseBufferListWriterLock();
			try
			{
				foreach (ILogFileInfo info in this.logFileInfoList)
				{
					//info.OpenFile();
					ReadToBufferList(info, 0, this.LineCount);
				}
				if (this.logFileInfoList.Count > 0)
				{
					ILogFileInfo info = this.logFileInfoList[this.logFileInfoList.Count - 1];
					this.fileLength = info.Length;
					this.watchedILogFileInfo = info;
				}
			}
			catch (IOException e)
			{
				Logger.logWarn("IOException: " + e.Message);
				this.fileLength = 0;
				this.isDeleted = true;
				this.LineCount = 0;
			}
			LogEventArgs args = new LogEventArgs();
			args.PrevFileSize = 0;
			args.PrevLineCount = 0;
			args.LineCount = this.LineCount;
			args.FileSize = this.currFileSize;
			OnFileSizeChanged(args);
		}

		private void ReloadBufferList()
		{
			lock (this.monitor)
			{
				AcquireBufferListWriterLock();
				this.bufferList.Clear();
				ReleaseBufferListWriterLock();
				ClearLru();
				//CloseFiles();
				ReadFiles();
				// Trigger "new file" handling (reload)
				if (currFileSize > 0)
					OnLoadFile(new LoadFileEventArgs(this.fileName, 0, true, this.fileLength, true));
			}
		}

		/// <summary>
		/// Public for unit tests.
		/// </summary>
		/// <returns></returns>
		public int ShiftBuffers()
		{
			Logger.logInfo("ShiftBuffers() begin for " + this.fileName + (IsMultiFile ? " (MultiFile)" : ""));
			AcquireBufferListWriterLock();
			int offset = 0;
			this.isLineCountDirty = true;
			lock (this.monitor)
			{
				RolloverFilenameHandler rolloverHandler = new RolloverFilenameHandler(this.watchedILogFileInfo, this.mutlifileOptions);
				LinkedList<string> fileNameList = rolloverHandler.GetNameList();

				ResetBufferCache();
				IList<ILogFileInfo> lostILogFileInfoList = new List<ILogFileInfo>();
				IList<ILogFileInfo> readNewILogFileInfoList = new List<ILogFileInfo>();
				IList<ILogFileInfo> newFileInfoList = new List<ILogFileInfo>();
				IEnumerator<ILogFileInfo> enumerator = this.logFileInfoList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ILogFileInfo logFileInfo = enumerator.Current;
					string fileName = logFileInfo.FullName;
					Logger.logDebug("Testing file " + fileName);
					LinkedListNode<string> node = fileNameList.Find(fileName);
					if (node == null)
					{
						Logger.logWarn("File " + fileName + " not found");
						continue;
					}
					if (node.Previous != null)
					{
						fileName = node.Previous.Value;
						ILogFileInfo newILogFileInfo = GetLogFileInfo(fileName);
						Logger.logDebug(fileName + " exists");
						Logger.logDebug("Old size=" + logFileInfo.OriginalLength + ", new size=" + newILogFileInfo.Length);
						// is the new file the same as the old buffer info?
						if (newILogFileInfo.Length == logFileInfo.OriginalLength)
						{
							ReplaceBufferInfos(logFileInfo, newILogFileInfo);
							newFileInfoList.Add(newILogFileInfo);
						}
						else
						{
							Logger.logDebug("Buffer for " + fileName + " must be re-read.");
							// not the same. so must read the rest of the list anew from the files
							readNewILogFileInfoList.Add(newILogFileInfo);
							while (enumerator.MoveNext())
							{
								fileName = enumerator.Current.FullName;
								node = fileNameList.Find(fileName);
								if (node == null)
								{
									Logger.logWarn("File " + fileName + " not found");
									continue;
								}
								if (node.Previous != null)
								{
									fileName = node.Previous.Value;
									Logger.logDebug("New name is " + fileName);
									readNewILogFileInfoList.Add(GetLogFileInfo(fileName));
								}
								else
								{
									Logger.logWarn("No previous file for " + fileName + " found");
								}
							}
						}
					}
					else
					{
						Logger.logInfo(fileName + " does not exist");
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
						//this.ILogFileInfoList.Remove(ILogFileInfo);
						LogBuffer lastBuffer = DeleteBuffersForInfo(ILogFileInfo, false);
						if (lastBuffer != null)
						{
							offset += lastBuffer.StartLine + lastBuffer.LineCount;
						}
					}
					this.lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
					Logger.logInfo("Adjusting StartLine values in " + this.bufferList.Count + " buffers by offset " + offset);
					foreach (LogBuffer buffer in this.bufferList)
					{
						SetNewStartLineForBuffer(buffer, buffer.StartLine - offset);
					}
					this.lruCacheDictLock.ReleaseWriterLock();
					#if DEBUG
					if (this.bufferList.Count > 0)
					{
						Logger.logInfo("First buffer now has StartLine " + this.bufferList[0].StartLine);
					}
					#endif
				}
				// Read anew all buffers following a buffer info that couldn't be matched with the corresponding existing file
				Logger.logInfo("Deleting buffers for files that must be re-read");
				foreach (ILogFileInfo ILogFileInfo in readNewILogFileInfoList)
				{
					DeleteBuffersForInfo(ILogFileInfo, true);
					//this.ILogFileInfoList.Remove(ILogFileInfo);
				}
				Logger.logInfo("Deleting buffers for the watched file");
				DeleteBuffersForInfo(this.watchedILogFileInfo, true);
				int startLine = LineCount - 1;
				Logger.logInfo("Re-Reading files");
				foreach (ILogFileInfo ILogFileInfo in readNewILogFileInfoList)
				{
					//ILogFileInfo.OpenFile();
					ReadToBufferList(ILogFileInfo, 0, this.LineCount);
					//this.ILogFileInfoList.Add(ILogFileInfo);
					newFileInfoList.Add(ILogFileInfo);
				}
				//this.watchedILogFileInfo = this.ILogFileInfoList[this.ILogFileInfoList.Count - 1];
				this.logFileInfoList = newFileInfoList;
				this.watchedILogFileInfo = GetLogFileInfo(this.watchedILogFileInfo.FullName);
				this.logFileInfoList.Add(this.watchedILogFileInfo);
				Logger.logInfo("Reading watched file");
				ReadToBufferList(watchedILogFileInfo, 0, this.LineCount);
			}
			Logger.logInfo("ShiftBuffers() end. offset=" + offset);
			ReleaseBufferListWriterLock();
			return offset;
		}

		private void ReplaceBufferInfos(ILogFileInfo oldLogFileInfo, ILogFileInfo newLogFileInfo)
		{
			Logger.logDebug("ReplaceBufferInfos() " + oldLogFileInfo.FullName + " -> " + newLogFileInfo.FullName);
			AcquireBufferListReaderLock();
			foreach (LogBuffer buffer in this.bufferList)
			{
				if (buffer.FileInfo == oldLogFileInfo)
				{
					Logger.logDebug("Buffer with startLine=" + buffer.StartLine +
									", lineCount=" + buffer.LineCount + ", filePos=" + buffer.StartPos +
									", size=" + buffer.Size + " gets new filename " + newLogFileInfo.FullName);
					buffer.FileInfo = newLogFileInfo;
				}
			}
			ReleaseBufferListReaderLock();
		}

		private LogBuffer DeleteBuffersForInfo(ILogFileInfo ILogFileInfo, bool matchNamesOnly)
		{
			Logger.logInfo("Deleting buffers for file " + ILogFileInfo.FullName);
			LogBuffer lastRemovedBuffer = null;
			IList<LogBuffer> deleteList = new List<LogBuffer>();
			AcquireBufferListWriterLock();
			this.lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			if (matchNamesOnly)
			{
				foreach (LogBuffer buffer in this.bufferList)
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
				foreach (LogBuffer buffer in this.bufferList)
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
			this.lruCacheDictLock.ReleaseWriterLock();
			ReleaseBufferListWriterLock();
			if (lastRemovedBuffer == null)
				Logger.logInfo("lastRemovedBuffer is null");
			else
				Logger.logInfo("lastRemovedBuffer: startLine=" + lastRemovedBuffer.StartLine);
			return lastRemovedBuffer;
		}

		/// <summary>
		/// The caller must have writer locks for lruCache and buffer list!
		/// </summary>
		/// <param name="buffer"></param>
		private void RemoveFromBufferList(LogBuffer buffer)
		{
			Util.AssertTrue(this.lruCacheDictLock.IsWriterLockHeld, "No writer lock for lru cache");
			Util.AssertTrue(this.bufferListLock.IsWriterLockHeld, "No writer lock for buffer list");
			this.lruCacheDict.Remove(buffer.StartLine);
			this.bufferList.Remove(buffer);
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
				Logger.logWarn("IOException: " + fe.ToString());
				this.isDeleted = true;
				this.LineCount = 0;
				this.currFileSize = 0;
				OnFileNotFound(); // notify LogWindow
				return;
			}
			try
			{
				reader = GetLogStreamReader(fileStream, this.EncodingOptions, this.UseNewReader);
				reader.Position = filePos;
				this.fileLength = logFileInfo.Length;
				String line;
				int lineNum = startLine;
				LogBuffer logBuffer;
				AcquireBufferListReaderLock();
				if (this.bufferList.Count == 0)
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
					logBuffer = this.bufferList[this.bufferList.Count - 1];
					//if (logBuffer.FileInfo != ILogFileInfo)
					if (!logBuffer.FileInfo.FullName.Equals(logFileInfo.FullName))
					{
						logBuffer = new LogBuffer(logFileInfo, this.MAX_LINES_PER_BUFFER);
						logBuffer.StartLine = startLine;
						logBuffer.StartPos = filePos;
						LockCookie cookie = UpgradeBufferListLockToWriter();
						AddBufferToList(logBuffer);
						DowngradeBufferListLockFromWriter(ref cookie);
						#if DEBUG
						//Logger.logDebug("ReadToBufferList(): new buffer created because new ILogFileInfo");
						#endif
					}
					this.disposeLock.AcquireReaderLock(Timeout.Infinite);
					if (logBuffer.IsDisposed)
					{
						LockCookie cookie = this.disposeLock.UpgradeToWriterLock(Timeout.Infinite);
						ReReadBuffer(logBuffer);
						this.disposeLock.DowngradeFromWriterLock(ref cookie);
					}
					this.disposeLock.ReleaseReaderLock();
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
					if (this.shouldStop)
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
					if (lineCount > this.MAX_LINES_PER_BUFFER && reader.IsBufferComplete)
					{
						OnLoadFile(new LoadFileEventArgs(logFileInfo.FullName, filePos, false, logFileInfo.Length, false));
						#if DEBUG
						//Logger.logDebug("ReadToBufferList(): new buffer created. lineCount: " + lineCount + ", lineNum:" + lineNum + ", text: " + line);
						#endif
						//logBuffer.Size = filePos - logBuffer.StartPos;
						Monitor.Exit(logBuffer);
						logBuffer = new LogBuffer(logFileInfo, this.MAX_LINES_PER_BUFFER);
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
				this.isLineCountDirty = true;
				this.currFileSize = reader.Position;
				this.CurrentEncoding = reader.Encoding; // Reader may have detected another encoding
				if (!this.shouldStop)
				{
					OnLoadFile(new LoadFileEventArgs(logFileInfo.FullName, filePos, true, this.fileLength, false));
					// Fire "Ready" Event
				}
				GC.KeepAlive(fileStream);
			}
			catch (IOException ioex)
			{
				Logger.logWarn(ioex.GetType().Name + ": " + ioex.Message);
			}
			finally
			{
				fileStream.Close();
			}
		}

		private void AddBufferToList(LogBuffer logBuffer)
		{
			#if DEBUG
			Logger.logDebug("AddBufferToList(): " + logBuffer.StartLine + "/" + logBuffer.LineCount + "/" + logBuffer.FileInfo.FullName);
			#endif
			this.bufferList.Add(logBuffer);
			//UpdateLru(logBuffer);
			UpdateLruCache(logBuffer);
		}

		private void UpdateLruCache(LogBuffer logBuffer)
		{
			LogBufferCacheEntry cacheEntry;
			this.lruCacheDictLock.AcquireReaderLock(Timeout.Infinite);
			if (this.lruCacheDict.TryGetValue(logBuffer.StartLine, out cacheEntry))
			{
				cacheEntry.Touch();
			}
			else
			{
				LockCookie cookie = this.lruCacheDictLock.UpgradeToWriterLock(Timeout.Infinite);
				if (!this.lruCacheDict.TryGetValue(logBuffer.StartLine, out cacheEntry))  // #536: re-test, because multiple threads may have been waiting for writer lock
				{
					cacheEntry = new LogBufferCacheEntry();
					cacheEntry.LogBuffer = logBuffer;
					try
					{
						this.lruCacheDict.Add(logBuffer.StartLine, cacheEntry);
					}
					catch (ArgumentException e)
					{
						#if DEBUG
						// there seems to be a bug with double added key
						Logger.logError("Error in LRU cache: " + e.Message);
						Logger.logInfo("Added buffer:");
						DumpBufferInfos(logBuffer);
						LogBufferCacheEntry exisingEntry;
						if (this.lruCacheDict.TryGetValue(logBuffer.StartLine, out exisingEntry))
						{
							Logger.logInfo("Existing buffer: ");
							DumpBufferInfos(exisingEntry.LogBuffer);
						}
						else
						{
							Logger.logWarn("Ooops? Cannot find the already existing entry in LRU.");
						}
						#endif
						this.lruCacheDictLock.ReleaseLock();
						throw e;
					}
				}
				this.lruCacheDictLock.DowngradeFromWriterLock(ref cookie);
			}
			this.lruCacheDictLock.ReleaseReaderLock();
		}

		/// <summary>
		/// Sets a new start line in the given buffer and updates the LRU cache, if the buffer
		/// is present in the cache. The caller must have write lock for 'lruCacheDictLock';
		/// </summary>
		/// <param name="logBuffer"></param>
		/// <param name="newLineNum"></param>
		private void SetNewStartLineForBuffer(LogBuffer logBuffer, int newLineNum)
		{
			Util.AssertTrue(this.lruCacheDictLock.IsWriterLockHeld, "No writer lock for lru cache");
			if (this.lruCacheDict.ContainsKey(logBuffer.StartLine))
			{
				this.lruCacheDict.Remove(logBuffer.StartLine);
				logBuffer.StartLine = newLineNum;
				LogBufferCacheEntry cacheEntry = new LogBufferCacheEntry();
				cacheEntry.LogBuffer = logBuffer;
				this.lruCacheDict.Add(logBuffer.StartLine, cacheEntry);
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
			this.lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			int diff = 0;
			if (this.lruCacheDict.Count - (MAX_BUFFERS + threshold) > 0)
			{
				diff = this.lruCacheDict.Count - MAX_BUFFERS;
				#if DEBUG
				if (diff > 0)
				{
					Logger.logInfo("Removing " + diff + " entries from LRU cache for " + Util.GetNameFromPath(this.fileName));
				}
				#endif
				SortedList<long, int> useSorterList = new SortedList<long, int>();
				// sort by usage counter
				foreach (LogBufferCacheEntry entry in this.lruCacheDict.Values)
				{
					if (!useSorterList.ContainsKey(entry.LastUseTimeStamp))
					{
						useSorterList.Add(entry.LastUseTimeStamp, entry.LogBuffer.StartLine);
					}
				}
				// remove first <diff> entries (least usage)
				this.disposeLock.AcquireWriterLock(Timeout.Infinite);
				for (int i = 0; i < diff; ++i)
				{
					if (i >= useSorterList.Count)
						break;
					int startLine = useSorterList.Values[i];
					LogBufferCacheEntry entry = this.lruCacheDict[startLine];
					this.lruCacheDict.Remove(startLine);
					entry.LogBuffer.DisposeContent();
				}
				this.disposeLock.ReleaseWriterLock();
			}
			this.lruCacheDictLock.ReleaseWriterLock();
			#if DEBUG
			if (diff > 0)
			{
				long endTime = Environment.TickCount;
				Logger.logInfo("Garbage collector time: " + (endTime - startTime) + " ms.");
			}
			#endif
		}

		private void GarbageCollectorThreadProc()
		{
			while (!this.shouldStop)
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
		//      lock (this.monitor)
		//      {
		//        int index;
		//        if (this.lruDict.TryGetValue(logBuffer.StartLine, out index))
		//        {
		//          RemoveBufferFromLru(logBuffer, index);
		//          AddBufferToLru(logBuffer);
		//        }
		//        else
		//        {
		//          if (this.bufferLru.Count > MAX_BUFFERS - 1)
		//          {
		//            LogBuffer looser = this.bufferLru[0];
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
		//  lock (this.monitor)
		//  {
		//    if (this.lruDict.TryGetValue(buffer.StartLine, out index))
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
		//  lock (this.monitor)
		//  {
		//    this.bufferLru.RemoveAt(index);
		//    this.lruDict.Remove(buffer.StartLine);
		//    // adjust indizes, they have changed because of the remove 
		//    for (int i = index; i < this.bufferLru.Count; ++i)
		//    {
		//      this.lruDict[this.bufferLru[i].StartLine] = this.lruDict[this.bufferLru[i].StartLine] - 1;
		//    }
		//  }
		//}

		//private void AddBufferToLru(LogBuffer logBuffer)
		//{
		//  lock (this.monitor)
		//  {
		//    this.bufferLru.Add(logBuffer);
		//    int newIndex = this.bufferLru.Count - 1;
		//    this.lruDict[logBuffer.StartLine] = newIndex;
		//  }
		//}

		private void ClearLru()
		{
			//lock (this.monitor)
			//{
			//  foreach (LogBuffer buffer in this.bufferLru)
			//  {
			//    buffer.DisposeContent();
			//  }
			//  this.bufferLru.Clear();
			//  this.lruDict.Clear();
			//}
			Logger.logInfo("Clearing LRU cache.");
			this.lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			this.disposeLock.AcquireWriterLock(Timeout.Infinite);
			foreach (LogBufferCacheEntry entry in this.lruCacheDict.Values)
			{
				entry.LogBuffer.DisposeContent();
			}
			this.lruCacheDict.Clear();
			this.disposeLock.ReleaseWriterLock();
			this.lruCacheDictLock.ReleaseWriterLock();
			Logger.logInfo("Clearing done.");
		}

		private void ReReadBuffer(LogBuffer logBuffer)
		{
			#if DEBUG
			Logger.logInfo("re-reading buffer: " + logBuffer.StartLine + "/" + logBuffer.LineCount + "/" + logBuffer.FileInfo.FullName);
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
					ILogStreamReader reader = GetLogStreamReader(fileStream, this.EncodingOptions, this.UseNewReader);
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
						Logger.logWarn("LineCount in buffer differs after re-reading. old=" + maxLinesCount + ", new=" +
									   logBuffer.LineCount);
					}
					if (dropCount - logBuffer.PrevBuffersDroppedLinesSum != logBuffer.DroppedLinesCount)
					{
						Logger.logWarn("DroppedLinesCount in buffer differs after re-reading. old=" + logBuffer.DroppedLinesCount +
									   ", new=" + dropCount);
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
			//if (lineNum == this.lastReturnedLineNumForBuffer)
			//{
			//  return this.lastReturnedBuffer;
			//}

			//int startIndex = lineNum / LogBuffer.MAX_LINES;  // doesn't work anymore since XML buffer may contain more lines than MAX_LINES
			int startIndex = 0;
			int count = this.bufferList.Count;
			for (int i = startIndex; i < count; ++i)
			{
				logBuffer = this.bufferList[i];
				if (lineNum >= logBuffer.StartLine && lineNum < logBuffer.StartLine + logBuffer.LineCount)
				{
					//UpdateLru(logBuffer);
					UpdateLruCache(logBuffer);
					//this.lastReturnedLineNumForBuffer = lineNum;
					//this.lastReturnedBuffer = logBuffer;
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

			if (!this.isFastFailOnGetLogLine)
			{
				IAsyncResult asyncResult = logLineFx.BeginInvoke(lineNum, null, null);
				if (asyncResult.AsyncWaitHandle.WaitOne(WAIT_TIME, false))
				{
					result = logLineFx.EndInvoke(asyncResult);
					this.isFastFailOnGetLogLine = false;
				}
				else
				{
					logLineFx.EndInvoke(asyncResult);    // must be called according to MSDN docs... :(
					this.isFastFailOnGetLogLine = true;
					#if DEBUG
					Logger.logDebug("No result after " + WAIT_TIME + "ms. Returning <null>.");
					#endif
				}
			}
			else
			{
				Logger.logDebug("Fast failing GetLogLine()");
				if (!this.isFailModeCheckCallPending)
				{
					this.isFailModeCheckCallPending = true;
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
			this.isFailModeCheckCallPending = false;
			GetLogLineFx logLineFx = (GetLogLineFx)res.AsyncState;
			string line = logLineFx.EndInvoke(res);
			if (line != null)
			{
				Logger.logDebug("'isFastFailOnGetLogLine' flag was reset");
				this.isFastFailOnGetLogLine = false;
			}
			Logger.logDebug("'isLogLineCallPending' flag was reset.");
		}

		public string GetLogLineInternal(int lineNum)
		{
			if (this.isDeleted)
			{
				#if DEBUG
				Logger.logDebug("Returning null for line " + lineNum + " because file is deleted.");
				#endif
				// fast fail if dead file was detected. Prevents repeated lags in GUI thread caused by callbacks from control (e.g. repaint)
				return null;
			}

			AcquireBufferListReaderLock();
			LogBuffer logBuffer = getBufferForLine(lineNum);
			if (logBuffer == null)
			{
				ReleaseBufferListReaderLock();
				Logger.logError("Cannot find buffer for line " + lineNum + ", file: " + this.fileName + (this.IsMultiFile ? " (MultiFile)" : ""));
				return null;
			}
			// disposeLock prevents that the garbage collector is disposing just in the moment we use the buffer
			string line = null;
			this.disposeLock.AcquireReaderLock(Timeout.Infinite);
			if (logBuffer.IsDisposed)
			{
				LockCookie cookie = this.disposeLock.UpgradeToWriterLock(Timeout.Infinite);
				lock (logBuffer.FileInfo)
				{
					ReReadBuffer(logBuffer);
				}
				this.disposeLock.DowngradeFromWriterLock(ref cookie);
			}
			line = logBuffer.GetLineOfBlock(lineNum - logBuffer.StartLine);
			this.disposeLock.ReleaseReaderLock();
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
				int index = this.bufferList.IndexOf(logBuffer);
				if (index != -1)
				{
					for (int i = index; i < this.bufferList.Count; ++i)
					{
						if (this.bufferList[i].FileInfo != logBuffer.FileInfo)
						{
							result = this.bufferList[i].StartLine;
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
				int index = this.bufferList.IndexOf(logBuffer);
				if (index != -1)
				{
					for (int i = index; i >= 0; --i)
					{
						if (this.bufferList[i].FileInfo != logBuffer.FileInfo)
						{
							result = this.bufferList[i].StartLine + this.bufferList[i].LineCount;
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
			int index = this.bufferList.IndexOf(logBuffer);
			if (index == -1)
			{
				ReleaseBufferListReaderLock();
				return null;
			}
			LogBuffer resultBuffer = logBuffer;
			while (true)
			{
				index--;
				if (index < 0 || this.bufferList[index].FileInfo != info)
					break;
				resultBuffer = this.bufferList[index];
			}
			ReleaseBufferListReaderLock();
			return resultBuffer;
		}

		public void startMonitoring()
		{
			Logger.logInfo("startMonitoring()");
			this.monitorThread = new Thread(new ThreadStart(this.MonitorThreadProc));
			this.monitorThread.IsBackground = true;
			this.shouldStop = false;
			this.monitorThread.Start();
		}

		public void stopMonitoring()
		{
			Logger.logInfo("stopMonitoring()");
			this.shouldStop = true;

			System.Threading.Thread.Sleep(this.watchedILogFileInfo.PollInterval); // leave time for the threads to stop by themselves

			if (this.monitorThread != null)
			{
				if (monitorThread.IsAlive) // if thread has not finished, abort it
				{
					this.monitorThread.Interrupt();
					this.monitorThread.Abort();
					this.monitorThread.Join();
				}
			}
			if (this.garbageCollectorThread != null)
			{
				if (this.garbageCollectorThread.IsAlive) // if thread has not finished, abort it
				{
					this.garbageCollectorThread.Interrupt();
					this.garbageCollectorThread.Abort();
					this.garbageCollectorThread.Join();
				}
			}
			//this.loadThread = null;
			this.monitorThread = null;
			this.garbageCollectorThread = null; // preventive call
			CloseFiles();
		}

		/// <summary>
		/// calls stopMonitoring() in a background thread and returns to the caller immediately. 
		/// This is useful for a fast responding GUI (e.g. when closing a file tab)
		/// </summary>
		public void StopMonitoringAsync()
		{
			Thread stopperThread = new Thread(new ThreadStart(this.stopMonitoring));
			stopperThread.IsBackground = true;
			stopperThread.Start();
		}

		/// <summary>
		/// Deletes all buffer lines and disposes their content. Use only when the LogfileReader
		/// is about to be closed!
		/// </summary>
		public void DeleteAllContent()
		{
			if (this.contentDeleted)
			{
				Logger.logDebug("Buffers for " + Util.GetNameFromPath(this.fileName) + " already deleted.");
				return;
			}
			Logger.logInfo("Deleting all log buffers for " + Util.GetNameFromPath(this.fileName) + ". Used mem: " + GC.GetTotalMemory(true).ToString("N0"));
			AcquireBufferListWriterLock();
			this.lruCacheDictLock.AcquireWriterLock(Timeout.Infinite);
			this.disposeLock.AcquireWriterLock(Timeout.Infinite);

			foreach (LogBuffer logBuffer in this.bufferList)
			{
				if (!logBuffer.IsDisposed)
				{
					logBuffer.DisposeContent();
				}
			}
			this.lruCacheDict.Clear();
			this.bufferList.Clear();

			this.disposeLock.ReleaseWriterLock();
			this.lruCacheDictLock.ReleaseWriterLock();
			ReleaseBufferListWriterLock();
			GC.Collect();
			this.contentDeleted = true;
			Logger.logInfo("Deleting complete. Used mem: " + GC.GetTotalMemory(true).ToString("N0"));
		}

		private void MonitorThreadProc()
		{
			Thread.CurrentThread.Name = "MonitorThread";
			//IFileSystemPlugin fs = PluginRegistry.GetInstance().FindFileSystemForUri(this.watchedILogFileInfo.FullName);
			Logger.logInfo("MonitorThreadProc() for file " + this.watchedILogFileInfo.FullName);

			long oldSize = 0;
			try
			{
				OnLoadingStarted(new LoadFileEventArgs(this.fileName, 0, false, 0, false));
				ReadFiles();
				if (!this.isDeleted)
				{
					oldSize = this.fileLength;
					OnLoadingFinished();
				}
			}
			catch (Exception e)
			{
				Logger.logError(e.Message);
			}

			while (!this.shouldStop)
			{
				try
				{
					int pollInterval = this.watchedILogFileInfo.PollInterval;
					//#if DEBUG
					//          if (Logger.IsDebug)
					//          {
					//            Logger.logDebug("Poll interval for " + this.fileName + ": " + pollInterval);
					//          }
					//#endif
					Thread.Sleep(pollInterval);
				}
				catch (Exception e)
				{
					Logger.logError(e.Message);
				}
				if (shouldStop)
					return;
				try
				{
					if (this.watchedILogFileInfo.FileHasChanged())
					{
						this.fileLength = this.watchedILogFileInfo.Length;
						if (this.fileLength == -1)
						{
							MonitoredFileNotFound();
						}
						else
						{
							oldSize = this.fileLength;
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
			if (!this.isDeleted)
			{
				Logger.logDebug("File not FileNotFoundException catched. Switching to 'deleted' mode.");
				this.isDeleted = true;
				oldSize = this.fileLength = -1;
				this.currFileSize = 0;
				OnFileNotFound(); // notify LogWindow
			}
			#if DEBUG
			else
			{
				Logger.logDebug("File not FileNotFoundException catched. Already in deleted mode.");
			}
			#endif
		}

		private void LoaderThreadProc()
		{
			if (!this.isDeleted)
			{
				FireChangeEvent();
			}
			OnLoadingFinished();
		}

		private void FileChanged()
		{
			if (this.isDeleted)
			{
				OnRespawned();
				// prevent size update events. The window should reload the complete file.
				this.currFileSize = this.fileLength;
			}
			long newSize = this.fileLength;
			//if (this.currFileSize != newSize)
			{
				Logger.logInfo("file size changed. new size=" + newSize + ", file: " + this.fileName);
				FireChangeEvent();
			}
		}

		private void FireChangeEvent()
		{
			LogEventArgs args = new LogEventArgs();
			args.PrevFileSize = this.currFileSize;
			args.PrevLineCount = this.LineCount;
			long newSize = this.fileLength;
			if (newSize < this.currFileSize || this.isDeleted)
			{
				Logger.logInfo("File was created anew: new size=" + newSize + ", oldSize=" + this.currFileSize);
				// Fire "New File" event
				this.currFileSize = 0;
				this.LineCount = 0;
				try
				{
					if (!IsMultiFile)
					{
						// ReloadBufferList();  // removed because reloading is triggered by owning LogWindow
						// Trigger "new file" handling (reload)
						OnLoadFile(new LoadFileEventArgs(this.fileName, 0, true, this.fileLength, true));

						if (this.isDeleted)
						{
							args.FileSize = newSize;
							args.LineCount = this.LineCount;
							if (args.PrevLineCount != args.LineCount && !this.shouldStop)
								OnFileSizeChanged(args);
						}
						this.isDeleted = false;
					}
					else
					{
						int offset = ShiftBuffers();
						//this.currFileSize = newSize;    // removed because ShiftBuffers() calls ReadToBuffer() which will set the actual read size
						args.FileSize = newSize;
						args.LineCount = this.LineCount;
						args.IsRollover = true;
						args.RolloverOffset = offset;
						this.isDeleted = false;
						if (!this.shouldStop)
							OnFileSizeChanged(args);
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
				ReadToBufferList(this.watchedILogFileInfo, this.currFileSize > 0 ? this.currFileSize : this.currFileSize, this.LineCount);
				args.FileSize = newSize;
				args.LineCount = this.LineCount;
				//if (args.PrevLineCount != args.LineCount && !this.shouldStop)
				OnFileSizeChanged(args);
			}
		}

		#region Events

		public delegate void FileSizeChangedEventHandler(object sender, LogEventArgs e);

		public event FileSizeChangedEventHandler FileSizeChanged;

		protected virtual void OnFileSizeChanged(LogEventArgs e)
		{
			if (FileSizeChanged != null)
				FileSizeChanged(this, e);
		}

		public delegate void BlockLoadedEventHandler(object sender, LoadFileEventArgs e);

		public event BlockLoadedEventHandler LoadFile;

		protected virtual void OnLoadFile(LoadFileEventArgs e)
		{
			if (LoadFile != null)
				LoadFile(this, e);
		}

		public delegate void LoadingStartedEventHandler(object sender, LoadFileEventArgs e);

		public event LoadingStartedEventHandler LoadingStarted;

		protected virtual void OnLoadingStarted(LoadFileEventArgs e)
		{
			if (LoadingStarted != null)
				LoadingStarted(this, e);
		}

		public delegate void FinishedLoadingEventHandler(object sender, EventArgs e);

		public event FinishedLoadingEventHandler LoadingFinished;

		protected virtual void OnLoadingFinished()
		{
			if (LoadingFinished != null)
				LoadingFinished(this, new EventArgs());
		}

		public delegate void FileNotFoundEventHandler(object sender, EventArgs e);

		public event FileNotFoundEventHandler FileNotFound;

		protected virtual void OnFileNotFound()
		{
			if (FileNotFound != null)
				FileNotFound(this, new EventArgs());
		}

		public delegate void FileRespawnedEventHandler(object sender, EventArgs e);

		public event FileRespawnedEventHandler Respawned;

		protected virtual void OnRespawned()
		{
			Logger.logInfo("OnRespawned()");
			if (Respawned != null)
				Respawned(this, new EventArgs());
		}

		#endregion

		public int LineCount
		{
			get
			{
				if (this.isLineCountDirty)
				{
					this.currLineCount = 0;
					AcquireBufferListReaderLock();
					foreach (LogBuffer buffer in this.bufferList)
					{
						this.currLineCount += buffer.LineCount;
					}
					ReleaseBufferListReaderLock();
					this.isLineCountDirty = false;
				}
				return this.currLineCount;
			}
			set
			{
				this.currLineCount = value;
			}
		}

		public bool IsMultiFile
		{
			get
			{
				return this.isMultiFile;
			}
		}

		/// <summary>
		/// Explicit change the encoding.
		/// </summary>
		/// <param name="encoding"></param>
		public void ChangeEncoding(Encoding encoding)
		{
			this.CurrentEncoding = encoding;
			this.EncodingOptions.Encoding = encoding;
			ResetBufferCache();
			ClearLru();
		}

		public Encoding CurrentEncoding { get; private set; }

		public long FileSize
		{
			get
			{
				return this.currFileSize;
			}
		}

		public bool IsXmlMode
		{
			get
			{
				return this.isXmlMode;
			}
			set
			{
				this.isXmlMode = value;
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
				return this.preProcessColumnizer;
			}
			set
			{
				this.preProcessColumnizer = value;
			}
		}

		public EncodingOptions EncodingOptions
		{
			get
			{
				return encodingOptions;
			}
			set
			{
				{
					encodingOptions = new EncodingOptions();
					encodingOptions.DefaultEncoding = value.DefaultEncoding;
					encodingOptions.Encoding = value.Encoding;
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
				this.bufferListLock.AcquireReaderLock(10000);
				#if DEBUG && TRACE_LOCKS
		StackTrace st = new StackTrace(true);
		StackFrame callerFrame = st.GetFrame(2);
		this.bufferListLockInfo = "Read lock from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
				#endif
			}
			catch (ApplicationException)
			{
				Logger.logWarn("Reader lock wait for bufferList timed out. Now trying infinite.");
				#if DEBUG && TRACE_LOCKS
		Logger.logInfo(this.bufferListLockInfo);
				#endif
				this.bufferListLock.AcquireReaderLock(Timeout.Infinite);
			}
		}

		private void ReleaseBufferListReaderLock()
		{
			this.bufferListLock.ReleaseReaderLock();
		}

		private void AcquireBufferListWriterLock()
		{
			try
			{
				this.bufferListLock.AcquireWriterLock(10000);
				#if DEBUG && TRACE_LOCKS
		StackTrace st = new StackTrace(true);
		StackFrame callerFrame = st.GetFrame(1);
		this.bufferListLockInfo = "Write lock from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
		callerFrame.GetFileName();
				#endif
			}
			catch (ApplicationException)
			{
				Logger.logWarn("Writer lock wait for bufferList timed out. Now trying infinite.");
				#if DEBUG && TRACE_LOCKS
		Logger.logInfo(this.bufferListLockInfo);
				#endif
				this.bufferListLock.AcquireWriterLock(Timeout.Infinite);
			}
		}

		private void ReleaseBufferListWriterLock()
		{
			this.bufferListLock.ReleaseWriterLock();
		}

		private LockCookie UpgradeBufferListLockToWriter()
		{
			try
			{
				LockCookie cookie = this.bufferListLock.UpgradeToWriterLock(10000);
				#if DEBUG && TRACE_LOCKS
		StackTrace st = new StackTrace(true);
		StackFrame callerFrame = st.GetFrame(2);
		this.bufferListLockInfo += ", upgraded to writer from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
				#endif
				return cookie;
			}
			catch (ApplicationException)
			{
				Logger.logWarn("Writer lock update wait for bufferList timed out. Now trying infinite.");
				#if DEBUG && TRACE_LOCKS
		Logger.logInfo(this.bufferListLockInfo);
				#endif
				return this.bufferListLock.UpgradeToWriterLock(Timeout.Infinite);
			}
		}

		private void DowngradeBufferListLockFromWriter(ref LockCookie cookie)
		{
			this.bufferListLock.DowngradeFromWriterLock(ref cookie);
			#if DEBUG && TRACE_LOCKS
	  StackTrace st = new StackTrace(true);
	  StackFrame callerFrame = st.GetFrame(2);
	  this.bufferListLockInfo += ", downgraded to reader from " + callerFrame.GetMethod().DeclaringType.Name + "." + callerFrame.GetMethod().Name + "() " + callerFrame.GetFileLineNumber();
			#endif
		}

		/// <summary>
		/// For unit tests only.
		/// </summary>
		/// <returns></returns>
		public IList<ILogFileInfo> GetLogFileInfoList()
		{
			return this.logFileInfoList;
		}

		/// <summary>
		/// For unit tests only 
		/// </summary>
		/// <returns></returns>
		public IList<LogBuffer> GetBufferList()
		{
			return this.bufferList;
		}

		#region Debug output
		
		#if DEBUG
		
		internal void LogBufferInfoForLine(int lineNum)
		{
			AcquireBufferListReaderLock();
			LogBuffer buffer = getBufferForLine(lineNum);
			if (buffer == null)
			{
				ReleaseBufferListReaderLock();
				Logger.logError("Cannot find buffer for line " + lineNum + ", file: " + this.fileName + (this.IsMultiFile ? " (MultiFile)" : ""));
				return;
			}
			Logger.logInfo("-----------------------------------");
			this.disposeLock.AcquireReaderLock(Timeout.Infinite);
			Logger.logInfo("Buffer info for line " + lineNum);
			DumpBufferInfos(buffer);
			Logger.logInfo("File pos for current line: " + buffer.GetFilePosForLineOfBlock(lineNum - buffer.StartLine));
			this.disposeLock.ReleaseReaderLock();
			Logger.logInfo("-----------------------------------");
			ReleaseBufferListReaderLock();
		}
		
		#endif
		
		#if DEBUG
		private void DumpBufferInfos(LogBuffer buffer)
		{
			Logger.logInfo("StartLine: " + buffer.StartLine);
			Logger.logInfo("LineCount: " + buffer.LineCount);
			Logger.logInfo("StartPos: " + buffer.StartPos);
			Logger.logInfo("Size: " + buffer.Size);
			Logger.logInfo("Disposed: " + (buffer.IsDisposed ? "yes" : "no"));
			Logger.logInfo("DisposeCount: " + buffer.DisposeCount);
			Logger.logInfo("File: " + buffer.FileInfo.FullName);
		}
		
		#endif
		
		#if DEBUG
		internal void LogBufferDiagnostic()
		{
			Logger.logInfo("-------- Buffer diagnostics -------");
			this.lruCacheDictLock.AcquireReaderLock(Timeout.Infinite);
			int cacheCount = this.lruCacheDict.Count;
			Logger.logInfo("LRU entries: " + cacheCount);
			this.lruCacheDictLock.ReleaseReaderLock();
			
			AcquireBufferListReaderLock();
			Logger.logInfo("File: " + this.fileName);
			Logger.logInfo("Buffer count: " + this.bufferList.Count);
			Logger.logInfo("Disposed buffers: " + (this.bufferList.Count - cacheCount));
			int lineNum = 0;
			long disposeSum = 0;
			long maxDispose = 0;
			long minDispose = Int32.MaxValue;
			for (int i = 0; i < this.bufferList.Count; ++i)
			{
				LogBuffer buffer = this.bufferList[i];
				this.disposeLock.AcquireReaderLock(Timeout.Infinite);
				if (buffer.StartLine != lineNum)
				{
					Logger.logError("Start line of buffer is: " + buffer.StartLine + ", expected: " + lineNum);
					Logger.logInfo("Info of buffer follows:");
					DumpBufferInfos(buffer);
				}
				lineNum += buffer.LineCount;
				disposeSum += buffer.DisposeCount;
				maxDispose = Math.Max(maxDispose, buffer.DisposeCount);
				minDispose = Math.Min(minDispose, buffer.DisposeCount);
				this.disposeLock.ReleaseReaderLock();
			}
			ReleaseBufferListReaderLock();
			Logger.logInfo("Dispose count sum is: " + disposeSum);
			Logger.logInfo("Min dispose count is: " + minDispose);
			Logger.logInfo("Max dispose count is: " + maxDispose);
			Logger.logInfo("-----------------------------------");
		}
		#endif
	
		#endregion
	}
}