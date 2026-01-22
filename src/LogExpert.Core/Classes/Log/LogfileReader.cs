using System.Globalization;
using System.Text;

using ColumnizerLib;

using LogExpert.Core.Classes.xml;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;
using LogExpert.Core.EventArguments;
using LogExpert.Core.Interface;

using NLog;

namespace LogExpert.Core.Classes.Log;

public partial class LogfileReader : IAutoLogLineMemoryColumnizerCallback, IDisposable
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly string _fileName;
    private readonly int _max_buffers;
    private readonly int _maxLinesPerBuffer;
    private readonly Lock _monitor = new();
    private readonly MultiFileOptions _multiFileOptions;
    private readonly IPluginRegistry _pluginRegistry;
    private readonly CancellationTokenSource _cts = new();
    private readonly ReaderType _readerType;
    private readonly int _maximumLineLength;

    private readonly ReaderWriterLockSlim _bufferListLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly ReaderWriterLockSlim _disposeLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly ReaderWriterLockSlim _lruCacheDictLock = new(LockRecursionPolicy.SupportsRecursion);

    private const int PROGRESS_UPDATE_INTERVAL_MS = 100;
    private const int WAIT_TIME = 1000;

    private IList<LogBuffer> _bufferList;

    private bool _contentDeleted;
    private DateTime _lastProgressUpdate = DateTime.MinValue;

    private long _fileLength;
    private Task _garbageCollectorTask;
    private Task _monitorTask;
    private bool _isDeleted;
    private bool _isFailModeCheckCallPending;
    private bool _isFastFailOnGetLogLine;
    private bool _isLineCountDirty = true;
    private IList<ILogFileInfo> _logFileInfoList = [];
    private Dictionary<int, LogBufferCacheEntry> _lruCacheDict;
    private bool _shouldStop;
    private bool _disposed;
    private ILogFileInfo _watchedILogFileInfo;

    #endregion

    #region cTor

    /// Public constructor for single file.
    public LogfileReader (string fileName, EncodingOptions encodingOptions, bool multiFile, int bufferCount, int linesPerBuffer, MultiFileOptions multiFileOptions, ReaderType readerType, IPluginRegistry pluginRegistry, int maximumLineLength)
    : this([fileName], encodingOptions, multiFile, bufferCount, linesPerBuffer, multiFileOptions, readerType, pluginRegistry, maximumLineLength)
    {
    }

    /// Public constructor for multiple files.
    public LogfileReader (string[] fileNames, EncodingOptions encodingOptions, int bufferCount, int linesPerBuffer, MultiFileOptions multiFileOptions, ReaderType readerType, IPluginRegistry pluginRegistry, int maximumLineLength)
        : this(fileNames, encodingOptions, true, bufferCount, linesPerBuffer, multiFileOptions, readerType, pluginRegistry, maximumLineLength)
    {
        // In this overload, we assume multiFile is always true.
    }

    // Single private constructor that contains the common initialization logic.
    private LogfileReader (string[] fileNames, EncodingOptions encodingOptions, bool multiFile, int bufferCount, int linesPerBuffer, MultiFileOptions multiFileOptions, ReaderType readerType, IPluginRegistry pluginRegistry, int maximumLineLength)
    {
        // Validate input: at least one file must be provided.
        if (fileNames == null || fileNames.Length < 1)
        {
            throw new ArgumentException(Resources.LogfileReader_Error_Message_MustProvideAtLeastOneFile, nameof(fileNames));
        }

        //Set default maximum line length if invalid value provided.
        if (maximumLineLength <= 0)
        {
            maximumLineLength = 500;
        }

        _maximumLineLength = maximumLineLength;
        _readerType = readerType;
        EncodingOptions = encodingOptions;
        _max_buffers = bufferCount;
        _maxLinesPerBuffer = linesPerBuffer;
        _multiFileOptions = multiFileOptions;
        _pluginRegistry = pluginRegistry;
        _disposed = false;

        InitLruBuffers();

        ILogFileInfo fileInfo = null;

        IsMultiFile = multiFile || fileNames.Length == 1;
        _fileName = fileNames[0];

        IEnumerable<string> names = IsMultiFile
            // For multi-file rollover mode: get rollover names.
            ? new RolloverFilenameHandler(GetLogFileInfo(_fileName), _multiFileOptions).GetNameList(_pluginRegistry)
            : [_fileName];

        foreach (var name in names)
        {
            fileInfo = AddFile(name);
        }

        if (IsMultiFile)
        {
            // Use the full name of the last file as _fileName.
            _fileName = fileInfo.FullName;
        }

        _watchedILogFileInfo = fileInfo;

        StartGCThread();
    }

    #endregion

    #region Events

    public event EventHandler<LogEventArgs> FileSizeChanged;
    public event EventHandler<LoadFileEventArgs> LoadFile;
    public event EventHandler<LoadFileEventArgs> LoadingStarted;
    public event EventHandler<EventArgs> LoadingFinished;
    public event EventHandler<EventArgs> FileNotFound;
    public event EventHandler<EventArgs> Respawned;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the total number of lines contained in all buffers.
    /// </summary>
    /// <remarks>The value is recalculated on demand if the underlying buffers have changed since the last
    /// access. Accessing this property is thread-safe.</remarks>
    public int LineCount
    {
        get
        {
            if (_isLineCountDirty)
            {
                field = 0;
                if (_bufferListLock.IsReadLockHeld || _bufferListLock.IsWriteLockHeld)
                {
                    foreach (var buffer in _bufferList)
                    {
                        field += buffer.LineCount;
                    }
                }
                else
                {
                    AcquireBufferListReaderLock();
                    foreach (var buffer in _bufferList)
                    {
                        field += buffer.LineCount;
                    }

                    ReleaseBufferListReaderLock();
                }

                _isLineCountDirty = false;
            }

            return field;
        }

        private set;
    }

    /// <summary>
    /// Gets a value indicating whether the current operation involves multiple files.
    /// </summary>
    public bool IsMultiFile { get; }

    /// <summary>
    /// Gets the character encoding currently used for reading or writing operations.
    /// </summary>
    public Encoding CurrentEncoding { get; private set; }

    /// <summary>
    /// Gets the size of the file, in bytes.
    /// </summary>
    public long FileSize { get; private set; }

    //TODO: Change to private field. No need for a property.
    /// <summary>
    /// Gets or sets a value indicating whether XML mode is enabled.
    /// </summary>
    public bool IsXmlMode { get; set; }

    //TODO: Change to private field. No need for a property.
    /// <summary>
    /// Gets or sets the XML log configuration used to control logging behavior and settings.
    /// </summary>
    public IXmlLogConfiguration XmlLogConfig { get; set; }

    /// <summary>
    /// Gets or sets the columnizer used to preprocess data before further processing.
    /// </summary>
    public IPreProcessColumnizerMemory PreProcessColumnizer { get; set; }

    /// <summary>
    /// Gets or sets the encoding options used for text processing operations.
    /// </summary>
    private EncodingOptions EncodingOptions
    {
        get;
        set
        {
            {
                field = new EncodingOptions
                {
                    DefaultEncoding = value.DefaultEncoding,
                    Encoding = value.Encoding
                };
            }
        }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Reads all log files and refreshes the internal buffer and related state to reflect the current contents of the
    /// files.
    /// Public for unit test reasons
    /// </summary>
    /// <remarks>This method resets file size and line count tracking, clears any cached data, and repopulates
    /// the buffer with the latest data from the log files. If an I/O error occurs while reading the files, the internal
    /// state is updated to indicate that the files are unavailable. After reading, a file size changed event is raised
    /// to notify listeners of the update.</remarks>
    //TODO: Make this private
    public void ReadFiles ()
    {
        _lastProgressUpdate = DateTime.MinValue;
        FileSize = 0;
        LineCount = 0;
        //this.lastReturnedLine = "";
        //this.lastReturnedLineNum = -1;
        //this.lastReturnedLineNumForBuffer = -1;
        _isDeleted = false;
        ClearLru();
        AcquireBufferListWriterLock();
        _bufferList.Clear();
        ReleaseBufferListWriterLock();
        try
        {
            foreach (var info in _logFileInfoList)
            {
                //info.OpenFile();
                ReadToBufferList(info, 0, LineCount);
            }

            if (_logFileInfoList.Count > 0)
            {
                var info = _logFileInfoList[_logFileInfoList.Count - 1];
                _fileLength = info.Length;
                _watchedILogFileInfo = info;
            }
        }
        catch (IOException e)
        {
            _logger.Warn(e, "IOException");
            _fileLength = 0;
            _isDeleted = true;
            LineCount = 0;
        }

        LogEventArgs args = new()
        {
            PrevFileSize = 0,
            PrevLineCount = 0,
            LineCount = LineCount,
            FileSize = FileSize
        };

        OnFileSizeChanged(args);
    }

    /// <summary>
    /// Synchronizes the internal buffer state with the current set of log files, updating or removing buffers as
    /// necessary to reflect file changes.
    /// Public for unit tests.
    /// </summary>
    /// <remarks>Call this method after external changes to the underlying log files, such as file rotation or
    /// deletion, to ensure the buffer accurately represents the current log file set. This method may remove, update,
    /// or re-read buffers to match the current files. Thread safety is ensured during the operation.</remarks>
    /// <returns>The total number of lines removed from the buffer as a result of deleted or replaced log files. Returns 0 if no
    /// lines were removed.</returns>
    //TODO: Make this private
    public int ShiftBuffers ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "ShiftBuffers() begin for {0}{1}", _fileName, IsMultiFile ? " (MultiFile)" : "");

        AcquireBufferListWriterLock();

        var offset = 0;
        _isLineCountDirty = true;

        lock (_monitor)
        {
            RolloverFilenameHandler rolloverHandler = new(_watchedILogFileInfo, _multiFileOptions);
            var fileNameList = rolloverHandler.GetNameList(_pluginRegistry);

            ResetBufferCache();

            IList<ILogFileInfo> lostILogFileInfoList = [];
            IList<ILogFileInfo> readNewILogFileInfoList = [];
            IList<ILogFileInfo> newFileInfoList = [];

            var enumerator = _logFileInfoList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var logFileInfo = enumerator.Current;
                var fileName = logFileInfo.FullName;
                _logger.Debug(CultureInfo.InvariantCulture, "Testing file {0}", fileName);
                var node = fileNameList.Find(fileName);
                if (node == null)
                {
                    _logger.Warn(CultureInfo.InvariantCulture, "File {0} not found", fileName);
                    continue;
                }

                if (node.Previous != null)
                {
                    fileName = node.Previous.Value;
                    var newILogFileInfo = GetLogFileInfo(fileName);
                    _logger.Debug(CultureInfo.InvariantCulture, "{0} exists\r\nOld size={1}, new size={2}", fileName, logFileInfo.OriginalLength, newILogFileInfo.Length);
                    // is the new file the same as the old buffer info?
                    if (newILogFileInfo.Length == logFileInfo.OriginalLength)
                    {
                        ReplaceBufferInfos(logFileInfo, newILogFileInfo);
                        newFileInfoList.Add(newILogFileInfo);
                    }
                    else
                    {
                        _logger.Debug(CultureInfo.InvariantCulture, "Buffer for {0} must be re-read.", fileName);
                        // not the same. so must read the rest of the list anew from the files
                        readNewILogFileInfoList.Add(newILogFileInfo);
                        while (enumerator.MoveNext())
                        {
                            fileName = enumerator.Current.FullName;
                            node = fileNameList.Find(fileName);
                            if (node == null)
                            {
                                _logger.Warn(CultureInfo.InvariantCulture, "File {0} not found", fileName);
                                continue;
                            }

                            if (node.Previous != null)
                            {
                                fileName = node.Previous.Value;
                                _logger.Debug(CultureInfo.InvariantCulture, "New name is {0}", fileName);
                                readNewILogFileInfoList.Add(GetLogFileInfo(fileName));
                            }
                            else
                            {
                                _logger.Warn(CultureInfo.InvariantCulture, "No previous file for {0} found", fileName);
                            }
                        }
                    }
                }
                else
                {
                    _logger.Info(CultureInfo.InvariantCulture, "{0} does not exist", fileName);
                    lostILogFileInfoList.Add(logFileInfo);
#if DEBUG // for better overview in logfile:
                    //ILogFileInfo newILogFileInfo = new ILogFileInfo(fileName);
                    //ReplaceBufferInfos(ILogFileInfo, newILogFileInfo);
#endif
                }
            }

            if (lostILogFileInfoList.Count > 0)
            {
                _logger.Info(CultureInfo.InvariantCulture, "Deleting buffers for lost files");

                AcquireLruCacheDictWriterLock();

                foreach (var logFileInfo in lostILogFileInfoList)
                {
                    //this.ILogFileInfoList.Remove(logFileInfo);
                    var lastBuffer = DeleteBuffersForInfo(logFileInfo, false);
                    if (lastBuffer != null)
                    {
                        offset += lastBuffer.StartLine + lastBuffer.LineCount;
                    }
                }

                _logger.Info(CultureInfo.InvariantCulture, "Adjusting StartLine values in {0} buffers by offset {1}", _bufferList.Count, offset);
                foreach (var buffer in _bufferList)
                {
                    SetNewStartLineForBuffer(buffer, buffer.StartLine - offset);
                }

                ReleaseLRUCacheDictWriterLock();
#if DEBUG
                if (_bufferList.Count > 0)
                {
                    _logger.Debug(CultureInfo.InvariantCulture, "First buffer now has StartLine {0}", _bufferList[0].StartLine);
                }
#endif
            }

            // Read anew all buffers following a buffer info that couldn't be matched with the corresponding existing file
            _logger.Info(CultureInfo.InvariantCulture, "Deleting buffers for files that must be re-read");

            AcquireLruCacheDictWriterLock();

            foreach (var iLogFileInfo in readNewILogFileInfoList)
            {
                DeleteBuffersForInfo(iLogFileInfo, true);
                //this.ILogFileInfoList.Remove(logFileInfo);
            }

            _logger.Info(CultureInfo.InvariantCulture, "Deleting buffers for the watched file");

            DeleteBuffersForInfo(_watchedILogFileInfo, true);
            ReleaseLRUCacheDictWriterLock();

            _logger.Info(CultureInfo.InvariantCulture, "Re-Reading files");

            foreach (var iLogFileInfo in readNewILogFileInfoList)
            {
                //logFileInfo.OpenFile();
                ReadToBufferList(iLogFileInfo, 0, LineCount);
                //this.ILogFileInfoList.Add(logFileInfo);
                newFileInfoList.Add(iLogFileInfo);
            }

            //this.watchedILogFileInfo = this.ILogFileInfoList[this.ILogFileInfoList.Count - 1];
            _logFileInfoList = newFileInfoList;
            _watchedILogFileInfo = GetLogFileInfo(_watchedILogFileInfo.FullName);
            _logFileInfoList.Add(_watchedILogFileInfo);
            _logger.Info(CultureInfo.InvariantCulture, "Reading watched file");

            ReadToBufferList(_watchedILogFileInfo, 0, LineCount);
        }

        _logger.Info(CultureInfo.InvariantCulture, "ShiftBuffers() end. offset={0}", offset);

        ReleaseBufferListWriterLock();

        return offset;
    }

    /// <summary>
    /// Acquires a read lock on the buffer list, waiting up to 10 seconds before forcing entry if the lock is not
    /// immediately available.
    /// </summary>
    /// <remarks>If the read lock cannot be acquired within 10 seconds, the method will forcibly enter the
    /// lock and log a warning. Callers should ensure that holding the read lock for extended periods does not block
    /// other operations.</remarks>
    private void AcquireBufferListReaderLock ()
    {
        if (!_bufferListLock.TryEnterReadLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Reader lock wait timed out, forcing entry");
            _bufferListLock.EnterReadLock();
        }
    }

    /// <summary>
    /// Releases the reader lock on the buffer list, allowing other threads to acquire write access.
    /// </summary>
    /// <remarks>Call this method after completing operations that require read access to the buffer list.
    /// Failing to release the reader lock may result in deadlocks or prevent other threads from obtaining write
    /// access.</remarks>
    private void ReleaseBufferListReaderLock ()
    {
        _bufferListLock.ExitReadLock();
    }

    /// <summary>
    /// Releases the writer lock on the buffer list, allowing other threads to acquire the lock.
    /// </summary>
    /// <remarks>Call this method after completing operations that required exclusive access to the buffer
    /// list. Failing to release the writer lock may result in deadlocks or reduced concurrency.</remarks>
    private void ReleaseBufferListWriterLock ()
    {
        _bufferListLock.ExitWriteLock();
    }

    /// <summary>
    /// Releases an upgradeable read lock held by the current thread on the associated lock object.
    /// </summary>
    /// <remarks>Call this method to exit an upgradeable read lock previously acquired on the underlying lock.
    /// Failing to release the lock may result in deadlocks or resource contention.</remarks>
    private void ReleaseDisposeUpgradeableReadLock ()
    {
        _disposeLock.ExitUpgradeableReadLock();
    }

    /// <summary>
    /// Acquires the writer lock for the buffer list, blocking the calling thread until the lock is obtained.
    /// </summary>
    /// <remarks>If the writer lock cannot be acquired within 10 seconds, a warning is logged and the method
    /// will continue to wait until the lock becomes available. This method should be used to ensure exclusive access to
    /// the buffer list when performing write operations.</remarks>
    private void AcquireBufferListWriterLock ()
    {
        if (!_bufferListLock.TryEnterWriteLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Writer lock wait timed out");
            _bufferListLock.EnterWriteLock();
        }
    }

    /// <summary>
    /// Retrieves the log line at the specified zero-based line number.
    /// </summary>
    /// <remarks>This method blocks until the log line is available. If the specified line number is out of
    /// range, an exception may be thrown.</remarks>
    /// <param name="lineNum">The zero-based index of the log line to retrieve. Must be greater than or equal to 0 and less than the total
    /// number of log lines.</param>
    /// <returns>An object representing the log line at the specified index.</returns>
    public ILogLine GetLogLine (int lineNum)
    {
        return GetLogLineInternal(lineNum).Result;
    }

    //TODO Make Task Based
    public ILogLineMemory GetLogLineMemory (int lineNum)
    {
        return GetLogLineMemoryInternal(lineNum).Result;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Constants always UpperCase")]
    public async Task<ILogLine> GetLogLineWithWait (int lineNum)
    {
        const int WAIT_TIME = 1000;

        ILogLine result = null;

        if (!_isFastFailOnGetLogLine)
        {
            var task = Task.Run(() => GetLogLineInternal(lineNum));
            if (task.Wait(WAIT_TIME))
            {
                result = task.Result;
                _isFastFailOnGetLogLine = false;
            }
            else
            {
                _isFastFailOnGetLogLine = true;
                _logger.Debug(CultureInfo.InvariantCulture, "No result after {0}ms. Returning <null>.", WAIT_TIME);
            }
        }
        else
        {
            _logger.Debug(CultureInfo.InvariantCulture, "Fast failing GetLogLine()");
            if (!_isFailModeCheckCallPending)
            {
                _isFailModeCheckCallPending = true;
                var logLine = await GetLogLineInternal(lineNum).ConfigureAwait(true);
                GetLineFinishedCallback(logLine);
            }
        }

        return result;
    }

    public async Task<ILogLineMemory> GetLogLineMemoryWithWait (int lineNum)
    {
        ILogLineMemory result = null;

        if (!_isFastFailOnGetLogLine)
        {
            var task = Task.Run(() => GetLogLineMemoryInternal(lineNum));
            if (task.Wait(WAIT_TIME))
            {
                result = await task.ConfigureAwait(false);
                _isFastFailOnGetLogLine = false;
            }
            else
            {
                _isFastFailOnGetLogLine = true;
                _logger.Debug(CultureInfo.InvariantCulture, "No result after {0}ms. Returning <null>.", WAIT_TIME);
            }
        }
        else
        {
            _logger.Debug(CultureInfo.InvariantCulture, "Fast failing GetLogLine()");
            if (!_isFailModeCheckCallPending)
            {
                _isFailModeCheckCallPending = true;
                var logLine = await GetLogLineMemoryInternal(lineNum).ConfigureAwait(true);
                GetLineMemoryFinishedCallback(logLine);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the file name of the actual file for the given line. Needed for MultiFile.
    /// </summary>
    /// <param name="lineNum"></param>
    /// <returns></returns>
    public string GetLogFileNameForLine (int lineNum)
    {
        var logBuffer = GetBufferForLine(lineNum);
        var fileName = logBuffer?.FileInfo.FullName;
        return fileName;
    }

    /// <summary>
    /// Returns the ILogFileInfo for the actual file for the given line. Needed for MultiFile.
    /// </summary>
    /// <param name="lineNum"></param>
    /// <returns></returns>
    public ILogFileInfo GetLogFileInfoForLine (int lineNum)
    {
        AcquireBufferListReaderLock();
        var logBuffer = GetBufferForLine(lineNum);
        var info = logBuffer?.FileInfo;
        ReleaseBufferListReaderLock();
        return info;
    }

    /// <summary>
    /// Returns the line number (starting from the given number) where the next multi file
    /// starts.
    /// </summary>
    /// <param name="lineNum"></param>
    /// <returns></returns>
    public int GetNextMultiFileLine (int lineNum)
    {
        var result = -1;
        AcquireBufferListReaderLock();
        var logBuffer = GetBufferForLine(lineNum);
        if (logBuffer != null)
        {
            var index = _bufferList.IndexOf(logBuffer);
            if (index != -1)
            {
                for (var i = index; i < _bufferList.Count; ++i)
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

    /// <summary>
    /// Finds the starting line number of the previous file segment before the specified line number across multiple
    /// files.
    /// </summary>
    /// <remarks>This method is useful when navigating through a collection of files represented as contiguous
    /// line segments. If the specified line number is within the first file segment, the method returns -1 to indicate
    /// that there is no previous file segment.</remarks>
    /// <param name="lineNum">The line number for which to locate the previous file segment. Must be a valid line number within the buffer.</param>
    /// <returns>The starting line number of the previous file segment if one exists; otherwise, -1.</returns>
    public int GetPrevMultiFileLine (int lineNum)
    {
        var result = -1;
        AcquireBufferListReaderLock();
        var logBuffer = GetBufferForLine(lineNum);
        if (logBuffer != null)
        {
            var index = _bufferList.IndexOf(logBuffer);
            if (index != -1)
            {
                for (var i = index; i >= 0; --i)
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
    public int GetRealLineNumForVirtualLineNum (int lineNum)
    {
        AcquireBufferListReaderLock();
        var logBuffer = GetBufferForLine(lineNum);
        var result = -1;
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

    /// <summary>
    /// Begins monitoring by starting the background monitoring process.
    /// </summary>
    /// <remarks>This method initiates monitoring if it is not already running. To stop monitoring, call the
    /// corresponding stop method if available. This method is not thread-safe; ensure that it is not called
    /// concurrently with other monitoring control methods.</remarks>
    public void StartMonitoring ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "startMonitoring()");
        _monitorTask = Task.Run(MonitorThreadProc, _cts.Token);
        _shouldStop = false;
    }

    /// <summary>
    /// Stops monitoring the log file and terminates any background monitoring or cleanup tasks.
    /// </summary>
    /// <remarks>Call this method to halt all ongoing monitoring activity and release associated resources.
    /// After calling this method, monitoring cannot be resumed without restarting the monitoring process.</remarks>
    public void StopMonitoring ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "stopMonitoring()");
        _shouldStop = true;

        Thread.Sleep(_watchedILogFileInfo.PollInterval); // leave time for the threads to stop by themselves

        if (_monitorTask != null)
        {
            if (_monitorTask.Status == TaskStatus.Running) // if thread has not finished, abort it
            {
                _cts.Cancel();
            }
        }

        if (!_garbageCollectorTask.IsCanceled)
        {
            if (_garbageCollectorTask.Status == TaskStatus.Running) // if thread has not finished, abort it
            {
                _cts.Cancel();
            }
        }

        //this.loadThread = null;
        //_monitorThread = null;
        //_garbageCollectorThread = null; // preventive call
        CloseFiles();
    }

    /// <summary>
    /// calls stopMonitoring() in a background thread and returns to the caller immediately.
    /// This is useful for a fast responding GUI (e.g. when closing a file tab)
    /// </summary>
    public void StopMonitoringAsync ()
    {
        var task = Task.Run(StopMonitoring);

        //Thread stopperThread = new(new ThreadStart(StopMonitoring))
        //{
        //    IsBackground = true
        //};
        //stopperThread.Start();
    }

    /// <summary>
    /// Deletes all buffer lines and disposes their content. Use only when the LogfileReader
    /// is about to be closed!
    /// </summary>
    public void DeleteAllContent ()
    {
        if (_contentDeleted)
        {
            _logger.Debug(CultureInfo.InvariantCulture, "Buffers for {0} already deleted.", Util.GetNameFromPath(_fileName));
            return;
        }

        _logger.Info(CultureInfo.InvariantCulture, "Deleting all log buffers for {0}. Used mem: {1:N0}", Util.GetNameFromPath(_fileName), GC.GetTotalMemory(true)); //TODO [Z] uh GC collect calls creepy
        AcquireBufferListWriterLock();
        AcquireLruCacheDictWriterLock();
        AcquireDisposeWriterLock();

        foreach (var logBuffer in _bufferList)
        {
            if (!logBuffer.IsDisposed)
            {
                logBuffer.DisposeContent();
            }
        }

        _lruCacheDict.Clear();
        _bufferList.Clear();

        ReleaseDisposeWriterLock();
        ReleaseLRUCacheDictWriterLock();
        ReleaseBufferListWriterLock();
        GC.Collect();
        _contentDeleted = true;
        _logger.Info(CultureInfo.InvariantCulture, "Deleting complete. Used mem: {0:N0}", GC.GetTotalMemory(true)); //TODO [Z] uh GC collect calls creepy
    }

    /// <summary>
    /// Explicit change the encoding.
    /// </summary>
    /// <param name="encoding"></param>
    public void ChangeEncoding (Encoding encoding)
    {
        CurrentEncoding = encoding;
        EncodingOptions.Encoding = encoding;
        ResetBufferCache();
        ClearLru();
    }

    /// <summary>
    /// For unit tests only.
    /// </summary>
    /// <returns></returns>
    public IList<ILogFileInfo> GetLogFileInfoList ()
    {
        return _logFileInfoList;
    }

    /// <summary>
    /// For unit tests only
    /// </summary>
    /// <returns></returns>
    public IList<LogBuffer> GetBufferList ()
    {
        return _bufferList;
    }

    #endregion

    #region Internals

#if DEBUG

    /// <summary>
    /// Logs detailed buffer information for the specified line number to the debug output.
    /// </summary>
    /// <remarks>This method is intended for debugging purposes and is only available in debug builds. It logs
    /// buffer details and file position information to assist with diagnostics.</remarks>
    /// <param name="lineNum">The zero-based line number for which buffer information is logged.</param>
    public void LogBufferInfoForLine (int lineNum)
    {
        AcquireBufferListReaderLock();
        var buffer = GetBufferForLine(lineNum);
        if (buffer == null)
        {
            ReleaseBufferListReaderLock();
            _logger.Error("Cannot find buffer for line {0}, file: {1}{2}", lineNum, _fileName, IsMultiFile ? " (MultiFile)" : "");
            return;
        }

        _logger.Info(CultureInfo.InvariantCulture, "-----------------------------------");
        AcquireDisposeReaderLock();
        _logger.Info(CultureInfo.InvariantCulture, "Buffer info for line {0}", lineNum);
        DumpBufferInfos(buffer);
        _logger.Info(CultureInfo.InvariantCulture, "File pos for current line: {0}", buffer.GetFilePosForLineOfBlock(lineNum - buffer.StartLine));
        ReleaseDisposeReaderLock();
        _logger.Info(CultureInfo.InvariantCulture, "-----------------------------------");
        ReleaseBufferListReaderLock();
    }

    /// <summary>
    /// Logs diagnostic information about the current state of the buffer and LRU cache for debugging purposes.
    /// </summary>
    /// <remarks>This method is intended for use in debug builds to assist with troubleshooting and analyzing
    /// buffer management. It outputs details such as the number of LRU cache entries, buffer counts, and dispose
    /// statistics to the logger. This method does not modify the state of the buffers or cache.</remarks>
    public void LogBufferDiagnostic ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "-------- Buffer diagnostics -------");
        AcquireLruCacheDictReaderLock();
        var cacheCount = _lruCacheDict.Count;
        _logger.Info(CultureInfo.InvariantCulture, "LRU entries: {0}", cacheCount);
        ReleaseLRUCacheDictReaderLock();

        AcquireBufferListReaderLock();
        _logger.Info(CultureInfo.InvariantCulture, "File: {0}\r\nBuffer count: {1}\r\nDisposed buffers: {2}", _fileName, _bufferList.Count, _bufferList.Count - cacheCount);
        var lineNum = 0;
        long disposeSum = 0;
        long maxDispose = 0;
        long minDispose = int.MaxValue;
        for (var i = 0; i < _bufferList.Count; ++i)
        {
            var buffer = _bufferList[i];
            AcquireDisposeReaderLock();
            if (buffer.StartLine != lineNum)
            {
                _logger.Error("Start line of buffer is: {0}, expected: {1}", buffer.StartLine, lineNum);
                _logger.Info(CultureInfo.InvariantCulture, "Info of buffer follows:");
                DumpBufferInfos(buffer);
            }

            lineNum += buffer.LineCount;
            disposeSum += buffer.DisposeCount;
            maxDispose = Math.Max(maxDispose, buffer.DisposeCount);
            minDispose = Math.Min(minDispose, buffer.DisposeCount);
            ReleaseDisposeReaderLock();
        }

        ReleaseBufferListReaderLock();
        _logger.Info(CultureInfo.InvariantCulture, "Dispose count sum is: {0}\r\nMin dispose count is: {1}\r\nMax dispose count is: {2}\r\n-----------------------------------", disposeSum, minDispose, maxDispose);
    }

#endif

    #endregion

    #region Private Methods

    /// <summary>
    /// Adds a log file to the collection and returns information about the added file.
    /// </summary>
    /// <param name="fileName">The path of the log file to add. Cannot be null or empty.</param>
    /// <returns>An object that provides information about the added log file.</returns>
    private ILogFileInfo AddFile (string fileName)
    {
        _logger.Info(CultureInfo.InvariantCulture, "Adding file to ILogFileInfoList: " + fileName);
        var info = GetLogFileInfo(fileName);
        _logFileInfoList.Add(info);
        return info;
    }

    /// <summary>
    /// Retrieves the log line at the specified line number, or returns null if the file has been deleted or the line
    /// cannot be found.
    /// </summary>
    /// <param name="lineNum">The zero-based line number of the log entry to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the log line at the specified line
    /// number, or null if the file is deleted or the line does not exist.</returns>
    private Task<ILogLine> GetLogLineInternal (int lineNum)
    {
        if (_isDeleted)
        {
            _logger.Debug(CultureInfo.InvariantCulture, "Returning null for line {0} because file is deleted.", lineNum);
            // fast fail if dead file was detected. Prevents repeated lags in GUI thread caused by callbacks from control (e.g. repaint)
            return null;
        }

        AcquireBufferListReaderLock();
        var logBuffer = GetBufferForLine(lineNum);
        if (logBuffer == null)
        {
            ReleaseBufferListReaderLock();
            _logger.Error("Cannot find buffer for line {0}, file: {1}{2}", lineNum, _fileName, IsMultiFile ? " (MultiFile)" : "");
            return null;
        }

        // disposeLock prevents that the garbage collector is disposing just in the moment we use the buffer
        AcquireDisposeLockUpgradableReadLock();
        if (logBuffer.IsDisposed)
        {
            UpgradeDisposeLockToWriterLock();
            lock (logBuffer.FileInfo)
            {
                ReReadBuffer(logBuffer);
            }

            DowngradeDisposeLockFromWriterLock();
        }

        var line = logBuffer.GetLineOfBlock(lineNum - logBuffer.StartLine);
        ReleaseDisposeUpgradeableReadLock();
        ReleaseBufferListReaderLock();

        return Task.FromResult(line);
    }

    private Task<ILogLineMemory> GetLogLineMemoryInternal (int lineNum)
    {
        if (_isDeleted)
        {
            _logger.Debug(CultureInfo.InvariantCulture, "Returning null for line {0} because file is deleted.", lineNum);
            // fast fail if dead file was detected. Prevents repeated lags in GUI thread caused by callbacks from control (e.g. repaint)
            return null;
        }

        AcquireBufferListReaderLock();
        var logBuffer = GetBufferForLine(lineNum);
        if (logBuffer == null)
        {
            ReleaseBufferListReaderLock();
            _logger.Error("Cannot find buffer for line {0}, file: {1}{2}", lineNum, _fileName, IsMultiFile ? " (MultiFile)" : "");
            return null;
        }
        // disposeLock prevents that the garbage collector is disposing just in the moment we use the buffer
        AcquireDisposeLockUpgradableReadLock();
        if (logBuffer.IsDisposed)
        {
            UpgradeDisposeLockToWriterLock();

            lock (logBuffer.FileInfo)
            {
                ReReadBuffer(logBuffer);
            }

            DowngradeDisposeLockFromWriterLock();
        }

        var line = logBuffer.GetLineMemoryOfBlock(lineNum - logBuffer.StartLine);
        ReleaseDisposeUpgradeableReadLock();
        ReleaseBufferListReaderLock();

        return Task.FromResult(line);
    }

    /// <summary>
    /// Initializes the internal data structures used for least recently used (LRU) buffer management.
    /// </summary>
    /// <remarks>Call this method to reset or prepare the LRU buffer cache before use. This method clears any
    /// existing buffer state and sets up the cache to track buffer usage according to the configured maximum buffer
    /// count.</remarks>
    private void InitLruBuffers ()
    {
        _bufferList = [];
        //_bufferLru = new List<LogBuffer>(_max_buffers + 1);
        //this.lruDict = new Dictionary<int, int>(this.MAX_BUFFERS + 1);  // key=startline, value = index in bufferLru
        _lruCacheDict = new Dictionary<int, LogBufferCacheEntry>(_max_buffers + 1);
    }

    /// <summary>
    /// Starts the background task responsible for performing garbage collection operations.
    /// </summary>
    /// <remarks>This method initiates the garbage collection process on a separate thread or task. It is
    /// intended for internal use to manage resource cleanup asynchronously. Calling this method multiple times without
    /// proper synchronization may result in multiple concurrent garbage collection tasks.</remarks>
    private void StartGCThread ()
    {
        _garbageCollectorTask = Task.Run(GarbageCollectorThreadProc, _cts.Token);
        //_garbageCollectorThread = new Thread(new ThreadStart(GarbageCollectorThreadProc));
        //_garbageCollectorThread.IsBackground = true;
        //_garbageCollectorThread.Start();
    }

    /// <summary>
    /// Resets the internal buffer cache, clearing any stored file size and line count information.
    /// </summary>
    /// <remarks>Call this method to reinitialize the buffer cache state, typically before reloading or
    /// reprocessing file data. After calling this method, any previously cached file size or line count values will be
    /// lost.</remarks>
    private void ResetBufferCache ()
    {
        FileSize = 0;
        LineCount = 0;
        //this.lastReturnedLine = "";
        //this.lastReturnedLineNum = -1;
        //this.lastReturnedLineNumForBuffer = -1;
    }

    /// <summary>
    /// Releases resources associated with open log files and resets related state information.
    /// </summary>
    private void CloseFiles ()
    {
        //foreach (ILogFileInfo info in this.ILogFileInfoList)
        //{
        //  info.CloseFile();
        //}
        FileSize = 0;
        LineCount = 0;
        //this.lastReturnedLine = "";
        //this.lastReturnedLineNum = -1;
        //this.lastReturnedLineNumForBuffer = -1;
    }

    /// <summary>
    /// Retrieves information about a log file specified by its file name or URI.
    /// </summary>
    /// <param name="fileNameOrUri">The file name or URI identifying the log file for which to retrieve information. Cannot be null or empty.</param>
    /// <returns>An object containing information about the specified log file.</returns>
    /// <exception cref="LogFileException">Thrown if no file system plugin is found for the specified file name or URI, or if the log file cannot be found.</exception>
    private ILogFileInfo GetLogFileInfo (string fileNameOrUri) //TODO: I changed to static
    {
        //TODO this must be fixed and should be given to the logfilereader not just called (https://github.com/LogExperts/LogExpert/issues/402)
        var fs = _pluginRegistry.FindFileSystemForUri(fileNameOrUri) ?? throw new LogFileException("No file system plugin found for " + fileNameOrUri);
        var logFileInfo = fs.GetLogfileInfo(fileNameOrUri);
        return logFileInfo ?? throw new LogFileException("Cannot find " + fileNameOrUri);
    }

    /// <summary>
    /// Replaces references to an existing log file information object with a new one in all managed buffers.
    /// </summary>
    /// <remarks>This method updates all buffer entries that reference the specified old log file information object,
    /// assigning them the new log file information object instead. Use this method when a log file has been renamed or its
    /// metadata has changed, and all associated buffers need to reference the updated information.</remarks>
    /// <param name="oldLogFileInfo">The log file information object to be replaced. Cannot be null.</param>
    /// <param name="newLogFileInfo">The new log file information object to use as a replacement. Cannot be null.</param>
    private void ReplaceBufferInfos (ILogFileInfo oldLogFileInfo, ILogFileInfo newLogFileInfo)
    {
        _logger.Debug(CultureInfo.InvariantCulture, "ReplaceBufferInfos() " + oldLogFileInfo.FullName + " -> " + newLogFileInfo.FullName);
        foreach (var buffer in _bufferList)
        {
            if (buffer.FileInfo == oldLogFileInfo)
            {
                _logger.Debug($"Buffer with startLine={buffer.StartLine}, lineCount={buffer.LineCount}, filePos={buffer.StartPos}, size={buffer.Size} gets new filename {newLogFileInfo.FullName}");
                buffer.FileInfo = newLogFileInfo;
            }
        }
    }

    /// <summary>
    /// Deletes all log buffers associated with the specified log file information and returns the last buffer that was
    /// removed.
    /// </summary>
    /// <remarks>If multiple buffers match the specified criteria, all are removed and the last one found is
    /// returned. If no buffers match, the method returns null.</remarks>
    /// <param name="iLogFileInfo">The log file information used to identify which buffers to delete. Cannot be null.</param>
    /// <param name="matchNamesOnly">true to match buffers by file name only; false to require an exact object match for the log file information.</param>
    /// <returns>The last LogBuffer instance that was removed; or null if no matching buffers were found.</returns>
    private LogBuffer DeleteBuffersForInfo (ILogFileInfo iLogFileInfo, bool matchNamesOnly)
    {
        _logger.Info($"Deleting buffers for file {iLogFileInfo.FullName}");
        LogBuffer lastRemovedBuffer = null;
        IList<LogBuffer> deleteList = [];

        if (matchNamesOnly)
        {
            foreach (var buffer in _bufferList)
            {
                if (buffer.FileInfo.FullName.Equals(iLogFileInfo.FullName, StringComparison.Ordinal))
                {
                    lastRemovedBuffer = buffer;
                    deleteList.Add(buffer);
                }
            }
        }
        else
        {
            foreach (var buffer in _bufferList)
            {
                if (buffer.FileInfo == iLogFileInfo)
                {
                    lastRemovedBuffer = buffer;
                    deleteList.Add(buffer);
                }
            }
        }

        foreach (var buffer in deleteList)
        {
            RemoveFromBufferList(buffer);
        }

        if (lastRemovedBuffer == null)
        {
            _logger.Info(CultureInfo.InvariantCulture, "lastRemovedBuffer is null");
        }
        else
        {
            _logger.Info(CultureInfo.InvariantCulture, "lastRemovedBuffer: startLine={0}", lastRemovedBuffer.StartLine);
        }

        return lastRemovedBuffer;
    }

    /// <summary>
    /// Removes the specified log buffer from the internal buffer list and associated cache.
    /// The caller must have _writer locks for lruCache and buffer list!
    /// </summary>
    /// <remarks>This method must be called only when the appropriate write locks for both the LRU cache and
    /// buffer list are held. Removing a buffer that is not present has no effect.</remarks>
    /// <param name="buffer">The log buffer to remove from the buffer list and cache. Must not be null.</param>
    private void RemoveFromBufferList (LogBuffer buffer)
    {
        Util.AssertTrue(_lruCacheDictLock.IsWriteLockHeld, "No _writer lock for lru cache");
        Util.AssertTrue(_bufferListLock.IsWriteLockHeld, "No _writer lock for buffer list");
        _ = _lruCacheDict.Remove(buffer.StartLine);
        _ = _bufferList.Remove(buffer);
    }

    /// <summary>
    /// Reads log lines from the specified log file starting at the given file position and line number, and populates
    /// the internal buffer list with the read data.
    /// </summary>
    /// <remarks>If the buffer list is empty or the log file changes, a new buffer is created. The method
    /// updates internal state such as file size, encoding, and line count, and may trigger events to notify about file
    /// loading progress or file not found conditions. This method is not thread-safe and should be called with
    /// appropriate synchronization if accessed concurrently.</remarks>
    /// <param name="logFileInfo">The log file information used to open and read the file. Must not be null.</param>
    /// <param name="filePos">The byte position in the file at which to begin reading.</param>
    /// <param name="startLine">The line number corresponding to the starting position in the file. Used to assign line numbers to buffered log
    /// lines.</param>
    private void ReadToBufferList (ILogFileInfo logFileInfo, long filePos, int startLine)
    {
        try
        {
            using var fileStream = logFileInfo.OpenStream();
            using var reader = GetLogStreamReader(fileStream, EncodingOptions);

            reader.Position = filePos;
            _fileLength = logFileInfo.Length;

            var lineNum = startLine;
            LogBuffer logBuffer;

            AcquireBufferListUpgradeableReadLock();

            try
            {
                if (_bufferList.Count == 0)
                {
                    logBuffer = new LogBuffer(logFileInfo, _maxLinesPerBuffer)
                    {
                        StartLine = startLine,
                        StartPos = filePos
                    };

                    UpgradeBufferlistLockToWriterLock();

                    try
                    {
                        AddBufferToList(logBuffer);
                    }
                    finally
                    {
                        DowngradeBufferListLockFromWriterLock();
                    }
                }
                else
                {
                    logBuffer = _bufferList[_bufferList.Count - 1];

                    if (!logBuffer.FileInfo.FullName.Equals(logFileInfo.FullName, StringComparison.Ordinal))
                    {
                        logBuffer = new LogBuffer(logFileInfo, _maxLinesPerBuffer)
                        {
                            StartLine = startLine,
                            StartPos = filePos
                        };

                        UpgradeBufferlistLockToWriterLock();

                        try
                        {
                            AddBufferToList(logBuffer);
                        }
                        finally
                        {
                            DowngradeBufferListLockFromWriterLock();
                        }
                    }

                    AcquireDisposeLockUpgradableReadLock();
                    if (logBuffer.IsDisposed)
                    {
                        UpgradeDisposeLockToWriterLock();
                        ReReadBuffer(logBuffer);
                        DowngradeDisposeLockFromWriterLock();
                    }

                    ReleaseDisposeUpgradeableReadLock();
                }
            }
            finally
            {
                ReleaseBufferListUpgradeableReadLock();
            }

            Monitor.Enter(logBuffer);
            try
            {
                var lineCount = logBuffer.LineCount;
                var droppedLines = logBuffer.PrevBuffersDroppedLinesSum;
                filePos = reader.Position;

                var (success, lineMemory, wasDropped) = ReadLineMemory(reader as ILogStreamReaderMemory, logBuffer.StartLine + logBuffer.LineCount, logBuffer.StartLine + logBuffer.LineCount + droppedLines);

                while (success)
                {
                    if (_shouldStop)
                    {
                        return;
                    }

                    if (wasDropped)
                    {
                        logBuffer.DroppedLinesCount += 1;
                        droppedLines++;
                        (success, lineMemory, wasDropped) = ReadLineMemory(reader as ILogStreamReaderMemory, logBuffer.StartLine + logBuffer.LineCount, logBuffer.StartLine + logBuffer.LineCount + droppedLines);
                        continue;
                    }

                    lineCount++;

                    if (lineCount > _maxLinesPerBuffer && reader.IsBufferComplete)
                    {
                        //Rate Limited Progrress
                        var now = DateTime.Now;
                        bool shouldFireLoadFileEvent = (now - _lastProgressUpdate).TotalMilliseconds >= PROGRESS_UPDATE_INTERVAL_MS;

                        if (shouldFireLoadFileEvent)
                        {
                            OnLoadFile(new LoadFileEventArgs(logFileInfo.FullName, filePos, false, logFileInfo.Length, false));
                            _lastProgressUpdate = now;
                        }

                        logBuffer.Size = filePos - logBuffer.StartPos;

                        Monitor.Exit(logBuffer);
                        try
                        {
                            var newBuffer = new LogBuffer(logFileInfo, _maxLinesPerBuffer)
                            {
                                StartLine = lineNum,
                                StartPos = filePos,
                                PrevBuffersDroppedLinesSum = droppedLines
                            };

                            AcquireBufferListWriterLock();

                            try
                            {
                                AddBufferToList(newBuffer);
                            }
                            finally
                            {
                                ReleaseBufferListWriterLock();
                            }

                            logBuffer = newBuffer;
                            Monitor.Enter(logBuffer);
                            lineCount = 1;
                        }
                        catch (Exception)
                        {
                            Monitor.Enter(logBuffer);
                            throw;
                        }
                    }

                    LogLine logLine = new(lineMemory, logBuffer.StartLine + logBuffer.LineCount);
                    logBuffer.AddLine(logLine, filePos);
                    filePos = reader.Position;
                    lineNum++;

                    (success, lineMemory, wasDropped) = ReadLineMemory(reader as ILogStreamReaderMemory, logBuffer.StartLine + logBuffer.LineCount, logBuffer.StartLine + logBuffer.LineCount + droppedLines);
                }

                logBuffer.Size = filePos - logBuffer.StartPos;
            }
            finally
            {
                Monitor.Exit(logBuffer);
            }

            _isLineCountDirty = true;
            FileSize = reader.Position;

            // Reader may have detected another encoding
            CurrentEncoding = reader.Encoding;

            if (!_shouldStop)
            {
                OnLoadFile(new LoadFileEventArgs(logFileInfo.FullName, filePos, true, _fileLength, false));
            }
        }
        catch (IOException ioex)
        {
            _logger.Warn(ioex, "IOException: ");
            _isDeleted = true;
            LineCount = 0;
            FileSize = 0;
            OnFileNotFound(); // notify LogWindow
        }
    }

    /// <summary>
    /// Adds the specified log buffer to the internal buffer list and updates its position in the least recently used
    /// (LRU) cache.
    /// </summary>
    /// <param name="logBuffer">The log buffer to add to the buffer list. Cannot be null.</param>
    private void AddBufferToList (LogBuffer logBuffer)
    {
#if DEBUG
        _logger.Debug(CultureInfo.InvariantCulture, "AddBufferToList(): {0}/{1}/{2}", logBuffer.StartLine, logBuffer.LineCount, logBuffer.FileInfo.FullName);
#endif
        _bufferList.Add(logBuffer);
        //UpdateLru(logBuffer);
        UpdateLruCache(logBuffer);
    }

    /// <summary>
    /// Updates the least recently used (LRU) cache with the specified log buffer, adding it if it does not already
    /// exist or marking it as recently used if it does.
    /// </summary>
    /// <remarks>If the specified log buffer is not already present in the cache, it is added. If it is
    /// present, its usage is updated to reflect recent access. This method is thread-safe and manages cache locks
    /// internally.</remarks>
    /// <param name="logBuffer">The log buffer to add to or update in the LRU cache. Cannot be null.</param>
    private void UpdateLruCache (LogBuffer logBuffer)
    {
        AcquireLRUCacheDictUpgradeableReadLock();
        try
        {
            if (_lruCacheDict.TryGetValue(logBuffer.StartLine, out var cacheEntry))
            {
                cacheEntry.Touch();
            }
            else
            {
                UpgradeLRUCacheDicLockToWriterLock();
                try
                {
                    if (!_lruCacheDict.TryGetValue(logBuffer.StartLine, out cacheEntry))
                    {
                        cacheEntry = new LogBufferCacheEntry
                        {
                            LogBuffer = logBuffer
                        };

                        try
                        {
                            _lruCacheDict.Add(logBuffer.StartLine, cacheEntry);
                        }
                        catch (ArgumentException e)
                        {
                            _logger.Error(e, "Error in LRU cache: " + e.Message);
#if DEBUG // there seems to be a bug with double added key

                            _logger.Info(CultureInfo.InvariantCulture, "Added buffer:");
                            DumpBufferInfos(logBuffer);
                            if (_lruCacheDict.TryGetValue(logBuffer.StartLine, out var existingEntry))
                            {
                                _logger.Info(CultureInfo.InvariantCulture, "Existing buffer: ");
                                DumpBufferInfos(existingEntry.LogBuffer);
                            }
                            else
                            {
                                _logger.Warn(CultureInfo.InvariantCulture, "Ooops? Cannot find the already existing entry in LRU.");
                            }
#endif
                            throw;
                        }
                    }
                }
                finally
                {
                    DowngradeLRUCacheLockFromWriterLock();
                }
            }
        }
        finally
        {
            ReleaseLRUCacheDictUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Sets a new start line in the given buffer and updates the LRU cache, if the buffer
    /// is present in the cache. The caller must have write lock for 'lruCacheDictLock';
    /// </summary>
    /// <param name="logBuffer"></param>
    /// <param name="newLineNum"></param>
    private void SetNewStartLineForBuffer (LogBuffer logBuffer, int newLineNum)
    {
        Util.AssertTrue(_lruCacheDictLock.IsWriteLockHeld, "No _writer lock for lru cache");
        if (_lruCacheDict.ContainsKey(logBuffer.StartLine))
        {
            _ = _lruCacheDict.Remove(logBuffer.StartLine);
            logBuffer.StartLine = newLineNum;
            LogBufferCacheEntry cacheEntry = new()
            {
                LogBuffer = logBuffer
            };
            _lruCacheDict.Add(logBuffer.StartLine, cacheEntry);
        }
        else
        {
            logBuffer.StartLine = newLineNum;
        }
    }

    /// <summary>
    /// Removes least recently used entries from the LRU cache to maintain the cache size within the configured limit.
    /// </summary>
    /// <remarks>This method is intended to be called when the LRU cache exceeds its maximum allowed size. It
    /// removes the least recently used entries to free up resources and ensure optimal cache performance. The method is
    /// not thread-safe and should be called only when appropriate locks are held to prevent concurrent
    /// modifications.</remarks>
    private void GarbageCollectLruCache ()
    {
#if DEBUG
        long startTime = Environment.TickCount;
#endif
        _logger.Debug(CultureInfo.InvariantCulture, "Starting garbage collection");
        var threshold = 10;
        AcquireLruCacheDictWriterLock();
        var diff = 0;
        if (_lruCacheDict.Count - (_max_buffers + threshold) > 0)
        {
            diff = _lruCacheDict.Count - _max_buffers;
#if DEBUG
            if (diff > 0)
            {
                _logger.Info(CultureInfo.InvariantCulture, "Removing {0} entries from LRU cache for {1}", diff, Util.GetNameFromPath(_fileName));
            }
#endif
            SortedList<long, int> useSorterList = [];
            // sort by usage counter
            foreach (var entry in _lruCacheDict.Values)
            {
                if (!useSorterList.ContainsKey(entry.LastUseTimeStamp))
                {
                    useSorterList.Add(entry.LastUseTimeStamp, entry.LogBuffer.StartLine);
                }
            }

            // remove first <diff> entries (least usage)
            AcquireDisposeWriterLock();
            for (var i = 0; i < diff; ++i)
            {
                if (i >= useSorterList.Count)
                {
                    break;
                }

                var startLine = useSorterList.Values[i];
                var entry = _lruCacheDict[startLine];
                _lruCacheDict.Remove(startLine);
                entry.LogBuffer.DisposeContent();
            }

            ReleaseDisposeWriterLock();
        }

        ReleaseLRUCacheDictWriterLock();
#if DEBUG
        if (diff > 0)
        {
            long endTime = Environment.TickCount;
            _logger.Info(CultureInfo.InvariantCulture, "Garbage collector time: " + (endTime - startTime) + " ms.");
        }
#endif
    }

    /// <summary>
    /// Executes the background thread procedure responsible for periodically triggering garbage collection of the least
    /// recently used (LRU) cache while the thread is active.
    /// </summary>
    /// <remarks>This method is intended to run on a dedicated background thread. It repeatedly waits for a
    /// fixed interval and then invokes cache cleanup, continuing until a stop signal is received. Exceptions during the
    /// sleep interval are caught and ignored to ensure the thread remains active.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Garbage collector Thread Process")]
    private void GarbageCollectorThreadProc ()
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

    /// <summary>
    /// Clears all entries from the least recently used (LRU) cache and releases associated resources.
    /// </summary>
    /// <remarks>Call this method to remove all items from the LRU cache and dispose of their contents. This
    /// operation is typically used to free memory or reset the cache state. The method is not thread-safe and should be
    /// called only when appropriate synchronization is ensured.</remarks>
    private void ClearLru ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "Clearing LRU cache.");
        AcquireLruCacheDictWriterLock();
        AcquireDisposeWriterLock();
        foreach (var entry in _lruCacheDict.Values)
        {
            entry.LogBuffer.DisposeContent();
        }

        _lruCacheDict.Clear();
        ReleaseDisposeWriterLock();
        ReleaseLRUCacheDictWriterLock();
        _logger.Info(CultureInfo.InvariantCulture, "Clearing done.");
    }

    /// <summary>
    /// Re-reads the contents of the specified log buffer from its associated file, updating its lines and dropped line
    /// count as necessary.
    /// </summary>
    /// <remarks>This method acquires a lock on the provided log buffer during the operation to ensure thread
    /// safety. If an I/O error occurs while accessing the file, the method logs a warning and returns without updating
    /// the buffer.</remarks>
    /// <param name="logBuffer">The log buffer to refresh with the latest data from its underlying file. Cannot be null.</param>
    private void ReReadBuffer (LogBuffer logBuffer)
    {
#if DEBUG
        _logger.Info(CultureInfo.InvariantCulture, "re-reading buffer: {0}/{1}/{2}", logBuffer.StartLine, logBuffer.LineCount, logBuffer.FileInfo.FullName);
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
                _logger.Warn(e);
                return;
            }

            try
            {
                var reader = GetLogStreamReader(fileStream, EncodingOptions);

                var filePos = logBuffer.StartPos;
                reader.Position = logBuffer.StartPos;
                var maxLinesCount = logBuffer.LineCount;
                var lineCount = 0;
                var dropCount = logBuffer.PrevBuffersDroppedLinesSum;
                logBuffer.ClearLines();

                var (success, lineMemory, wasDropped) = ReadLineMemory(reader as ILogStreamReaderMemory, logBuffer.StartLine + logBuffer.LineCount, logBuffer.StartLine + logBuffer.LineCount + dropCount);

                while (success)
                {
                    if (lineCount >= maxLinesCount)
                    {
                        break;
                    }

                    if (wasDropped)
                    {
                        dropCount++;
                        (success, lineMemory, wasDropped) = ReadLineMemory(reader as ILogStreamReaderMemory, logBuffer.StartLine + logBuffer.LineCount, logBuffer.StartLine + logBuffer.LineCount + dropCount);
                        continue;
                    }

                    LogLine logLine = new(lineMemory, logBuffer.StartLine + logBuffer.LineCount);

                    logBuffer.AddLine(logLine, filePos);
                    filePos = reader.Position;
                    lineCount++;

                    (success, lineMemory, wasDropped) = ReadLineMemory(reader as ILogStreamReaderMemory, logBuffer.StartLine + logBuffer.LineCount, logBuffer.StartLine + logBuffer.LineCount + dropCount);
                }

                if (maxLinesCount != logBuffer.LineCount)
                {
                    _logger.Warn(CultureInfo.InvariantCulture, "LineCount in buffer differs after re-reading. old={0}, new={1}", maxLinesCount, logBuffer.LineCount);
                }

                if (dropCount - logBuffer.PrevBuffersDroppedLinesSum != logBuffer.DroppedLinesCount)
                {
                    _logger.Warn(CultureInfo.InvariantCulture, "DroppedLinesCount in buffer differs after re-reading. old={0}, new={1}", logBuffer.DroppedLinesCount, dropCount);
                    logBuffer.DroppedLinesCount = dropCount - logBuffer.PrevBuffersDroppedLinesSum;
                }

                GC.KeepAlive(fileStream);
            }
            catch (IOException e)
            {
                _logger.Warn(e);
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

    /// <summary>
    /// Retrieves the log buffer that contains the specified line number.
    /// </summary>
    /// <param name="lineNum">The zero-based line number for which to retrieve the corresponding log buffer. Must be greater than or equal to
    /// zero.</param>
    /// <returns>The <see cref="LogBuffer"/> instance that contains the specified line number, or <see langword="null"/> if no
    /// such buffer exists.</returns>
    private LogBuffer GetBufferForLine (int lineNum)
    {
#if DEBUG
        long startTime = Environment.TickCount;
#endif
        LogBuffer logBuffer = null;
        AcquireBufferListReaderLock();

        var startIndex = 0;
        var count = _bufferList.Count;
        for (var i = startIndex; i < count; ++i)
        {
            logBuffer = _bufferList[i];
            if (lineNum >= logBuffer.StartLine && lineNum < logBuffer.StartLine + logBuffer.LineCount)
            {
                UpdateLruCache(logBuffer);
                break;
            }
        }
#if DEBUG
        long endTime = Environment.TickCount;
        //_logger.logDebug("getBufferForLine(" + lineNum + ") duration: " + ((endTime - startTime)) + " ms. Buffer start line: " + logBuffer.StartLine);
#endif
        ReleaseBufferListReaderLock();
        return logBuffer;
    }

    /// <summary>
    /// Handles the completion of a log line retrieval operation and updates internal state flags accordingly.
    /// </summary>
    /// <param name="line">The log line that was retrieved. Can be null if the operation did not return a line.</param>
    private void GetLineFinishedCallback (ILogLine line)
    {
        _isFailModeCheckCallPending = false;
        if (line != null)
        {
            _logger.Debug(CultureInfo.InvariantCulture, "'isFastFailOnGetLogLine' flag was reset");
            _isFastFailOnGetLogLine = false;
        }

        _logger.Debug(CultureInfo.InvariantCulture, "'isLogLineCallPending' flag was reset.");
    }

    private void GetLineMemoryFinishedCallback (ILogLineMemory line)
    {
        _isFailModeCheckCallPending = false;
        if (line != null)
        {
            _logger.Debug(CultureInfo.InvariantCulture, "'isFastFailOnGetLogLine' flag was reset");
            _isFastFailOnGetLogLine = false;
        }

        _logger.Debug(CultureInfo.InvariantCulture, "'isLogLineCallPending' flag was reset.");
    }

    /// <summary>
    /// Finds the first buffer in the buffer list that is associated with the same file as the specified log buffer,
    /// searching backwards from the given buffer.
    /// </summary>
    /// <remarks>This method searches backwards from the specified buffer in the buffer list to locate the
    /// earliest buffer associated with the same file. The search is inclusive of the starting buffer.</remarks>
    /// <param name="logBuffer">The log buffer from which to begin the search. Must not be null.</param>
    /// <returns>The first LogBuffer in the buffer list that is associated with the same file as the specified buffer, searching
    /// in reverse order from the given buffer. Returns null if the specified buffer is not found in the buffer list.</returns>
    private LogBuffer GetFirstBufferForFileByLogBuffer (LogBuffer logBuffer)
    {
        var info = logBuffer.FileInfo;
        AcquireBufferListReaderLock();
        var index = _bufferList.IndexOf(logBuffer);
        if (index == -1)
        {
            ReleaseBufferListReaderLock();
            return null;
        }

        var resultBuffer = logBuffer;
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

    /// <summary>
    /// Monitors the specified log file for changes and processes updates in a background thread.
    /// </summary>
    /// <remarks>This method is intended to be used as the entry point for a monitoring thread. It
    /// periodically checks the watched log file for changes, handles file not found scenarios, and triggers appropriate
    /// events when the file is updated or deleted. The method runs until a stop signal is received. Exceptions
    /// encountered during monitoring are logged but do not terminate the monitoring loop.</remarks>
    private void MonitorThreadProc ()
    {
        Thread.CurrentThread.Name = "MonitorThread";
        //IFileSystemPlugin fs = PluginRegistry.GetInstance().FindFileSystemForUri(this.watchedILogFileInfo.FullName);
        _logger.Info(CultureInfo.InvariantCulture, "MonitorThreadProc() for file {0}", _watchedILogFileInfo.FullName);

        long oldSize;
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
            _logger.Error(e);
        }

        while (!_shouldStop)
        {
            try
            {
                var pollInterval = _watchedILogFileInfo.PollInterval;
                //#if DEBUG
                //          if (_logger.IsDebug)
                //          {
                //            _logger.logDebug("Poll interval for " + this.fileName + ": " + pollInterval);
                //          }
                //#endif
                Thread.Sleep(pollInterval);
            }
            catch (Exception e)
            {
                _logger.Error(e);
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

    /// <summary>
    /// Handles the scenario when the monitored file is not found and updates the internal state to reflect that the
    /// file has been deleted.
    /// </summary>
    /// <remarks>This method should be called when a monitored file is determined to be missing, such as after
    /// a FileNotFoundException. It transitions the monitoring logic into a 'deleted' state and notifies any listeners
    /// of the file's absence. Subsequent calls have no effect if the file is already marked as deleted.</remarks>
    private void MonitoredFileNotFound ()
    {
        long oldSize;
        if (!_isDeleted)
        {
            _logger.Debug(CultureInfo.InvariantCulture, "File not FileNotFoundException catched. Switching to 'deleted' mode.");
            _isDeleted = true;
            oldSize = _fileLength = -1;
            FileSize = 0;
            OnFileNotFound(); // notify LogWindow
        }
#if DEBUG
        else
        {
            _logger.Debug(CultureInfo.InvariantCulture, "File not FileNotFoundException catched. Already in deleted mode.");
        }
#endif
    }

    /// <summary>
    /// Handles updates when the underlying file has changed, such as when it is modified or restored after deletion.
    /// </summary>
    /// <remarks>This method should be called when the file being monitored is detected to have changed. If
    /// the file was previously deleted and has been restored, the method triggers a respawn event and resets the file
    /// size. It also logs the change and notifies listeners of the update.</remarks>
    private void FileChanged ()
    {
        if (_isDeleted)
        {
            OnRespawned();
            // prevent size update events. The window should reload the complete file.
            FileSize = _fileLength;
        }

        var newSize = _fileLength;
        //if (this.currFileSize != newSize)
        {
            _logger.Info(CultureInfo.InvariantCulture, "file size changed. new size={0}, file: {1}", newSize, _fileName);
            FireChangeEvent();
        }
    }

    /// <summary>
    /// Raises a change event to notify listeners of updates to the monitored file, such as changes in file size, line
    /// count, or file rollover events.
    /// </summary>
    /// <remarks>This method should be called whenever the state of the monitored file may have changed,
    /// including when the file is recreated, deleted, or rolled over. It updates relevant event arguments and invokes
    /// event handlers as appropriate. Listeners can use the event data to respond to file changes, such as updating UI
    /// elements or processing new log entries.</remarks>
    private void FireChangeEvent ()
    {
        LogEventArgs args = new()
        {
            PrevFileSize = FileSize,
            PrevLineCount = LineCount
        };

        var newSize = _fileLength;
        if (newSize < FileSize || _isDeleted)
        {
            _logger.Info(CultureInfo.InvariantCulture, "File was created anew: new size={0}, oldSize={1}", newSize, FileSize);
            // Fire "New File" event
            FileSize = 0;
            LineCount = 0;
            try
            {
                if (IsMultiFile)
                {
                    var offset = ShiftBuffers();
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
                else
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
            }
            catch (FileNotFoundException e)
            {
                // trying anew in next poll intervall. So let currFileSize untouched.
                _logger.Warn(e);
            }
        }
        else
        {
            ReadToBufferList(_watchedILogFileInfo, FileSize, LineCount);
            args.FileSize = newSize;
            args.LineCount = LineCount;
            //if (args.PrevLineCount != args.LineCount && !this.shouldStop)
            OnFileSizeChanged(args);
        }
    }

    /// <summary>
    /// Creates an <see cref="ILogStreamReader"/> for reading log entries from the specified stream using the provided
    /// encoding options.
    /// </summary>
    /// <remarks>If XML mode is enabled, the returned reader splits and parses XML log blocks according to the
    /// current XML log configuration. The caller is responsible for disposing the returned reader when
    /// finished.</remarks>
    /// <param name="stream">The input stream containing the log data to be read. The stream must be readable and positioned at the start of
    /// the log content.</param>
    /// <param name="encodingOptions">The encoding options to use when interpreting the log data from the stream.</param>
    /// <returns>An <see cref="ILogStreamReader"/> instance for reading log entries from the specified stream. If XML mode is
    /// enabled, the reader parses XML log blocks; otherwise, it reads logs in the default format.</returns>
    private ILogStreamReader GetLogStreamReader (Stream stream, EncodingOptions encodingOptions)
    {
        var reader = CreateLogStreamReader(stream, encodingOptions);

        return IsXmlMode ? new XmlBlockSplitter(new XmlLogReader(reader), XmlLogConfig) : reader;
    }

    /// <summary>
    /// Creates an instance of an ILogStreamReader for reading log data from the specified stream using the provided
    /// encoding options.
    /// </summary>
    /// <param name="stream">The input stream containing the log data to be read. The stream must be readable and positioned at the start of
    /// the log data.</param>
    /// <param name="encodingOptions">The encoding options to use when interpreting the log data from the stream.</param>
    /// <returns>An ILogStreamReader instance configured to read from the specified stream with the given encoding options.</returns>
    private ILogStreamReader CreateLogStreamReader (Stream stream, EncodingOptions encodingOptions)
    {
        return _readerType switch
        {
            ReaderType.Legacy => new PositionAwareStreamReaderLegacy(stream, encodingOptions, _maximumLineLength),
            ReaderType.System => new PositionAwareStreamReaderSystem(stream, encodingOptions, _maximumLineLength),
            //Default will be System
            _ => new PositionAwareStreamReaderSystem(stream, encodingOptions, _maximumLineLength),
        };
    }

    /// <summary>
    /// Attempts to read a single line from the specified log stream reader and applies optional preprocessing.
    /// </summary>
    /// <remarks>If an IOException or NotSupportedException occurs during reading, the method logs a warning
    /// and treats the situation as end of stream. If a PreProcessColumnizer is set, the line is processed before being
    /// returned.</remarks>
    /// <param name="reader">The log stream reader from which to read the next line. Cannot be null.</param>
    /// <param name="lineNum">The logical line number to associate with the line being read. Used for preprocessing.</param>
    /// <param name="realLineNum">The actual line number in the underlying data source. Used for preprocessing.</param>
    /// <param name="outLine">When this method returns, contains the line that was read and optionally preprocessed, or null if the end of the
    /// stream is reached or an error occurs.</param>
    /// <returns>true if a line was successfully read and assigned to outLine; otherwise, false.</returns>
    private bool ReadLine (ILogStreamReader reader, int lineNum, int realLineNum, out string outLine)
    {
        string line = null;
        try
        {
            line = reader.ReadLine();
        }
        catch (IOException e)
        {
            _logger.Warn(e);
        }
        catch (NotSupportedException e)
        {
            // Bug#11: "Reading operations are not supported by the stream"
            // Currently not reproducible. Probably happens at an unlucky time interval (after opening the file)
            // when the file is being deleted (rolling)
            // This will be handled as EOF.
            _logger.Warn(e);
        }

        if (line == null) // EOF or catched Exception
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

    /// <summary>
    /// Attempts to read a single line from the specified log stream reader, returning both the line as a string and, if
    /// available, as a memory buffer without additional allocations.
    /// </summary>
    /// <remarks>If the reader implements memory-based access, this method avoids unnecessary string
    /// allocations by returning the line as a ReadOnlyMemory<char>. Otherwise, it falls back to reading the line as a
    /// string only. The returned memory buffer is only valid until the next read operation on the reader.</remarks>
    /// <param name="reader">The log stream reader from which to read the line. Must not be null.</param>
    /// <param name="lineNum">The zero-based logical line number to associate with the read operation. Used for preprocessing or context.</param>
    /// <param name="realLineNum">The zero-based physical line number in the underlying data source. Used for preprocessing or context.</param>
    /// <returns>A tuple containing a boolean indicating success, a read-only memory buffer containing the line if available, and
    /// the line as a string. If the reader supports memory-based access, the memory buffer is populated; otherwise, it
    /// is null.</returns>
    private (bool Success, ReadOnlyMemory<char> LineMemory, bool wasDropped) ReadLineMemory (ILogStreamReaderMemory reader, int lineNum, int realLineNum)
    {
        if (reader is null)
        {
            // Fallback to string-based reading if memory reader not available
            if (ReadLine(reader, lineNum, realLineNum, out var outLine))
            {
                return (true, outLine.AsMemory(), false);
            }

            return (false, ReadOnlyMemory<char>.Empty, false);
        }

        if (!reader.TryReadLine(out var lineMemory))
        {
            return (false, ReadOnlyMemory<char>.Empty, false);
        }

        var originalMemory = lineMemory;

        if (PreProcessColumnizer != null)
        {
            lineMemory = PreProcessColumnizer.PreProcessLine(lineMemory, lineNum, realLineNum);

            if (lineMemory.IsEmpty && !originalMemory.IsEmpty)
            {
                // Line was dropped by preprocessor
                return (true, ReadOnlyMemory<char>.Empty, true);
            }
        }

        return (true, lineMemory, false);

        //return (ReadLine(reader, lineNum, realLineNum, out var outLine), outLine.AsMemory());
    }

    /// <summary>
    /// Acquires an upgradeable read lock on the buffer list, waiting up to 10 seconds before blocking indefinitely if
    /// the lock is not immediately available.
    /// </summary>
    /// <remarks>This method ensures that the calling thread holds an upgradeable read lock on the buffer
    /// list. If the lock cannot be acquired within 10 seconds, a warning is logged and the method blocks until the lock
    /// becomes available. Use this method when a read lock is needed with the potential to upgrade to a write
    /// lock.</remarks>
    private void AcquireBufferListUpgradeableReadLock ()
    {
        if (!_bufferListLock.TryEnterUpgradeableReadLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Upgradeable read lock timed out");
            _bufferListLock.EnterUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Acquires an upgradeable read lock on the dispose lock, waiting up to 10 seconds before blocking indefinitely if
    /// the lock is not immediately available.
    /// </summary>
    /// <remarks>This method ensures that the current thread holds an upgradeable read lock on the dispose
    /// lock, allowing for potential escalation to a write lock if needed. If the lock cannot be acquired within 10
    /// seconds, a warning is logged and the method blocks until the lock becomes available.</remarks>
    private void AcquireDisposeLockUpgradableReadLock ()
    {
        if (!_disposeLock.TryEnterUpgradeableReadLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Upgradeable read lock timed out");
            _disposeLock.EnterUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Acquires an upgradeable read lock on the LRU cache dictionary, waiting up to 10 seconds before blocking
    /// indefinitely if the lock is not immediately available.
    /// </summary>
    /// <remarks>This method ensures that the calling thread holds an upgradeable read lock on the LRU cache
    /// dictionary, allowing for safe read access and the potential to upgrade to a write lock if necessary. If the lock
    /// cannot be acquired within 10 seconds, a warning is logged and the method blocks until the lock becomes
    /// available. This approach helps prevent deadlocks and provides diagnostic information in case of lock
    /// contention.</remarks>
    private void AcquireLRUCacheDictUpgradeableReadLock ()
    {
        if (!_lruCacheDictLock.TryEnterUpgradeableReadLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Upgradeable read lock timed out");
            _lruCacheDictLock.EnterUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Acquires a read lock on the LRU cache dictionary to ensure thread-safe read access.
    /// </summary>
    /// <remarks>If the read lock cannot be acquired within 10 seconds, a warning is logged and the method
    /// will block until the lock becomes available. Callers should ensure that this method is used in contexts where
    /// blocking is acceptable to avoid potential deadlocks or performance issues.</remarks>
    private void AcquireLruCacheDictReaderLock ()
    {
        if (!_lruCacheDictLock.TryEnterReadLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("LRU cache dict reader lock timed out");
            _lruCacheDictLock.EnterReadLock();
        }
    }

    /// <summary>
    /// Acquires a read lock on the dispose lock, blocking the calling thread until the lock is obtained.
    /// </summary>
    /// <remarks>If the read lock cannot be acquired within 10 seconds, a warning is logged and the method
    /// will block until the lock becomes available. This method is intended to ensure thread-safe access during
    /// disposal operations.</remarks>
    private void AcquireDisposeReaderLock ()
    {
        if (!_disposeLock.TryEnterReadLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Dispose reader lock timed out");
            _disposeLock.EnterReadLock();
        }
    }

    /// <summary>
    /// Releases the writer lock held on the LRU cache dictionary, allowing other threads to acquire the lock.
    /// </summary>
    /// <remarks>Call this method after completing operations that require exclusive access to the LRU cache
    /// dictionary. Failing to release the writer lock may result in deadlocks or reduced concurrency.</remarks>
    private void ReleaseLRUCacheDictWriterLock ()
    {
        _lruCacheDictLock.ExitWriteLock();
    }

    /// <summary>
    /// Releases the writer lock held for disposing resources.
    /// </summary>
    /// <remarks>Call this method to exit the write lock acquired for resource disposal. This should be used
    /// in conjunction with the corresponding method that acquires the writer lock to ensure proper synchronization
    /// during disposal operations.</remarks>
    private void ReleaseDisposeWriterLock ()
    {
        _disposeLock.ExitWriteLock();
    }

    /// <summary>
    /// Releases the read lock on the LRU cache dictionary to allow write access by other threads.
    /// </summary>
    /// <remarks>Call this method after completing operations that require read access to the LRU cache
    /// dictionary. Failing to release the lock may result in deadlocks or prevent other threads from acquiring write
    /// access.</remarks>
    private void ReleaseLRUCacheDictReaderLock ()
    {
        _lruCacheDictLock.ExitReadLock();
    }

    /// <summary>
    /// Releases a reader lock held for disposing resources, allowing other threads to acquire the lock as needed.
    /// </summary>
    /// <remarks>Call this method to release the read lock previously acquired for resource disposal
    /// operations. Failing to release the lock may result in deadlocks or prevent other threads from accessing the
    /// protected resource.</remarks>
    private void ReleaseDisposeReaderLock ()
    {
        _disposeLock.ExitReadLock();
    }

    /// <summary>
    /// Releases the upgradeable read lock held on the LRU cache dictionary.
    /// </summary>
    /// <remarks>Call this method to release the upgradeable read lock previously acquired on the LRU cache
    /// dictionary. Failing to release the lock may result in deadlocks or reduced concurrency. This method should be
    /// used in conjunction with the corresponding lock acquisition method to ensure proper synchronization.</remarks>
    private void ReleaseLRUCacheDictUpgradeableReadLock ()
    {
        _lruCacheDictLock.ExitUpgradeableReadLock();
    }

    /// <summary>
    /// Acquires the writer lock used to synchronize disposal operations, blocking the calling thread until the lock is
    /// obtained.
    /// </summary>
    /// <remarks>If the writer lock cannot be acquired within 10 seconds, a warning is logged and the method
    /// waits indefinitely until the lock becomes available. Callers should ensure that holding the lock for extended
    /// periods does not cause deadlocks or performance issues.</remarks>
    private void AcquireDisposeWriterLock ()
    {
        if (!_disposeLock.TryEnterWriteLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Dispose writer lock timed out");
            _disposeLock.EnterWriteLock();
        }
    }

    /// <summary>
    /// Acquires an exclusive writer lock on the LRU cache dictionary, blocking if the lock is not immediately
    /// available.
    /// </summary>
    /// <remarks>If the writer lock cannot be acquired within 10 seconds, a warning is logged and the method
    /// blocks until the lock becomes available. This method should be called before performing write operations on the
    /// LRU cache dictionary to ensure thread safety.</remarks>
    private void AcquireLruCacheDictWriterLock ()
    {
        if (!_lruCacheDictLock.TryEnterWriteLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("LRU cache dict writer lock timed out");
            _lruCacheDictLock.EnterWriteLock();
        }
    }

    /// <summary>
    /// Releases the upgradeable read lock on the buffer list, allowing other threads to acquire exclusive or read
    /// access.
    /// </summary>
    /// <remarks>Call this method after completing operations that required an upgradeable read lock on the
    /// buffer list. Failing to release the lock may result in deadlocks or reduced concurrency.</remarks>
    private void ReleaseBufferListUpgradeableReadLock ()
    {
        _bufferListLock.ExitUpgradeableReadLock();
    }

    /// <summary>
    /// Upgrades the buffer list lock from a reader lock to a writer lock, waiting up to 10 seconds before forcing the
    /// upgrade if necessary.
    /// </summary>
    /// <remarks>If the writer lock cannot be acquired within 10 seconds, the method logs a warning and then
    /// blocks until the writer lock is obtained. Call this method only when the current thread already holds a reader
    /// lock on the buffer list.</remarks>
    private void UpgradeBufferlistLockToWriterLock ()
    {
        if (!_bufferListLock.TryEnterWriteLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Writer lock upgrade timed out");
            _bufferListLock.EnterWriteLock();
        }
    }

    /// <summary>
    /// Upgrades the current dispose lock to a writer lock, blocking if necessary until the upgrade is successful.
    /// </summary>
    /// <remarks>This method attempts to upgrade the dispose lock to a writer lock with a timeout. If the
    /// upgrade cannot be completed within the timeout period, it logs a warning and blocks until the writer lock is
    /// acquired. Call this method when exclusive access is required for disposal or resource modification.</remarks>
    private void UpgradeDisposeLockToWriterLock ()
    {
        if (!_disposeLock.TryEnterWriteLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Writer lock upgrade timed out");
            _disposeLock.EnterWriteLock();
        }
    }

    /// <summary>
    /// Upgrades the lock on the LRU cache dictionary from a reader lock to a writer lock, waiting up to 10 seconds
    /// before forcing the upgrade.
    /// </summary>
    /// <remarks>If the writer lock cannot be acquired within 10 seconds, the method logs a warning and then
    /// blocks until the writer lock is available. Call this method only when it is necessary to perform write
    /// operations on the LRU cache dictionary after holding a reader lock.</remarks>
    private void UpgradeLRUCacheDicLockToWriterLock ()
    {
        if (!_lruCacheDictLock.TryEnterWriteLock(TimeSpan.FromSeconds(10)))
        {
            _logger.Warn("Writer lock upgrade timed out");
            _lruCacheDictLock.EnterWriteLock();
        }
    }

    /// <summary>
    /// Downgrades the buffer list lock from write mode to allow other threads to acquire read access.
    /// </summary>
    /// <remarks>Call this method after completing write operations to permit concurrent read access to the
    /// buffer list. The calling thread must hold the write lock before invoking this method.</remarks>
    private void DowngradeBufferListLockFromWriterLock ()
    {
        _bufferListLock.ExitWriteLock();
    }

    /// <summary>
    /// Downgrades the LRU cache lock from a writer lock, allowing other threads to acquire read access.
    /// </summary>
    /// <remarks>Call this method after completing operations that require exclusive write access to the LRU
    /// cache, to permit concurrent read operations. The caller must hold the writer lock before invoking this
    /// method.</remarks>
    private void DowngradeLRUCacheLockFromWriterLock ()
    {
        _lruCacheDictLock.ExitWriteLock();
    }

    /// <summary>
    /// Releases the writer lock on the dispose lock, downgrading from write access.
    /// </summary>
    /// <remarks>Call this method to release write access to the dispose lock when a downgrade is required,
    /// such as when transitioning from exclusive to shared access. This method should only be called when the current
    /// thread holds the writer lock.</remarks>
    private void DowngradeDisposeLockFromWriterLock ()
    {
        _disposeLock.ExitWriteLock();
    }

#if DEBUG
    /// <summary>
    /// Outputs detailed information about the specified log buffer to the trace logger for debugging purposes.
    /// </summary>
    /// <remarks>This method is only available in debug builds. It writes buffer details such as start line,
    /// line count, position, size, disposal state, and associated file to the trace log if trace logging is
    /// enabled.</remarks>
    /// <param name="buffer">The log buffer whose information will be written to the trace output. Cannot be null.</param>
    private static void DumpBufferInfos (LogBuffer buffer)
    {
        if (_logger.IsTraceEnabled)
        {
            _logger.Trace(CultureInfo.InvariantCulture, "StartLine: {0}\r\nLineCount: {1}\r\nStartPos: {2}\r\nSize: {3}\r\nDisposed: {4}\r\nDisposeCount: {5}\r\nFile: {6}",
                buffer.StartLine,
                buffer.LineCount,
                buffer.StartPos,
                buffer.Size,
                buffer.IsDisposed ? "yes" : "no",
                buffer.DisposeCount,
                buffer.FileInfo.FullName);
        }
    }

#endif

    #endregion

    #region IDisposable Support

    /// <summary>
    /// Releases all resources used by the current instance of the class.
    /// </summary>
    /// <remarks>Call this method when you are finished using the object to release unmanaged resources and
    /// perform other cleanup operations. After calling Dispose, the object should not be used.</remarks>
    public void Dispose ()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Suppress finalization (not needed but best practice)
    }

    /// <summary>
    /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <remarks>This method is called by public Dispose methods and can be overridden to release additional
    /// resources in derived classes. When disposing is true, both managed and unmanaged resources should be released.
    /// When disposing is false, only unmanaged resources should be released.</remarks>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose (bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DeleteAllContent();
                _cts.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizes an instance of the LogfileReader class, releasing unmanaged resources before the object is reclaimed
    /// by garbage collection.
    /// </summary>
    /// <remarks>This destructor is called automatically by the garbage collector when the object is no longer
    /// accessible. It ensures that any unmanaged resources are properly released if Dispose was not called
    /// explicitly.</remarks>
    //TODO: Seems that this can be deleted. Need to verify.
    ~LogfileReader ()
    {
        Dispose(false);
    }

    #endregion IDisposable Support

    #region Event Handlers

    /// <summary>
    /// Raises the FileSizeChanged event to notify subscribers when the size of the log file changes.
    /// </summary>
    /// <remarks>Derived classes can override this method to provide custom handling when the file size changes. This
    /// method is typically called after the file size has been updated.</remarks>
    /// <param name="e">An object that contains the event data associated with the file size change.</param>
    protected virtual void OnFileSizeChanged (LogEventArgs e)
    {
        FileSizeChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the LoadFile event to notify subscribers that a file load operation has occurred.
    /// </summary>
    /// <remarks>Override this method in a derived class to provide custom handling when a file is loaded.
    /// Calling the base implementation ensures that registered event handlers are invoked.</remarks>
    /// <param name="e">An object that contains the event data for the file load operation.</param>
    protected virtual void OnLoadFile (LoadFileEventArgs e)
    {
        LoadFile?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the LoadingStarted event to signal that a file loading operation has begun.
    /// </summary>
    /// <remarks>Derived classes can override this method to provide custom handling when a loading operation
    /// starts. This method is typically called to notify subscribers that loading has commenced.</remarks>
    /// <param name="e">An object that contains the event data associated with the loading operation.</param>
    protected virtual void OnLoadingStarted (LoadFileEventArgs e)
    {
        LoadingStarted?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the LoadingFinished event to signal that the loading process has completed.
    /// </summary>
    /// <remarks>Override this method in a derived class to provide custom logic when loading is finished.
    /// This method is typically called after all loading operations are complete to notify subscribers.</remarks>
    protected virtual void OnLoadingFinished ()
    {
        LoadingFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the event that signals a file was not found.
    /// </summary>
    /// <remarks>Override this method in a derived class to provide custom handling when a file is not found.
    /// This method invokes the associated event handlers, if any are subscribed.</remarks>
    protected virtual void OnFileNotFound ()
    {
        FileNotFound?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the Respawned event to notify subscribers that the object has respawned.
    /// </summary>
    /// <remarks>Override this method in a derived class to provide custom logic when the object respawns.
    /// Always call the base implementation to ensure that the Respawned event is raised.</remarks>
    protected virtual void OnRespawned ()
    {
        _logger.Info(CultureInfo.InvariantCulture, "OnRespawned()");
        Respawned?.Invoke(this, EventArgs.Empty);
    }

    #endregion Event Handlers
}
