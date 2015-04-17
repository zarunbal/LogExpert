using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.Controls
{
	//Zarunbal: only for refactoring and cleanup
	public abstract class BaseLogWindow : DockContent
	{
		#region Const
		
		private const int PROGRESS_BAR_MODULO = 1000;
		
		#endregion
		
		#region Fields

		protected ILogLineColumnizer _forcedColumnizerForLoading;
		
		protected ILogLineColumnizer _currentColumnizer;
		protected readonly Object _currentColumnizerLock = new Object();
		
		private readonly Thread _logEventHandlerThread = null;
		private readonly EventWaitHandle _logEventArgsEvent = new ManualResetEvent(false);
		private readonly List<LogEventArgs> _logEventArgsList = new List<LogEventArgs>();

		protected readonly GuiStateArgs _guiStateArgs = new GuiStateArgs();

		protected SortedList<int, RowHeightEntry> _rowHeightList = new SortedList<int, RowHeightEntry>();

		protected readonly IList<FilterPipe> _filterPipeList = new List<FilterPipe>();

		protected TimeSpreadCalculator _timeSpreadCalc;
		
		protected readonly StatusLineEventArgs _statusEventArgs = new StatusLineEventArgs();
		protected readonly ProgressEventArgs _progressEventArgs = new ProgressEventArgs();

		protected DelayedTrigger _statusLineTrigger = new DelayedTrigger(200);
		
		protected bool _shouldCancel = false;
		protected bool _isLoading;
		protected bool _isLoadError = false;
		
		protected MultifileOptions _multifileOptions = new MultifileOptions();
		protected ReloadMemento _reloadMemento;
		protected string[] _fileNames;
		
		protected LogTabWindow _parentLogTabWin;
		protected ColumnCache _columnCache = new ColumnCache();

		#endregion

		#region cTor
		
		public BaseLogWindow()
		{
			_logEventHandlerThread = new Thread(new ThreadStart(LogEventWorker));
			_logEventHandlerThread.IsBackground = true;
			_logEventHandlerThread.Start();
		}

		#endregion

		#region Events

		public BookmarkDataProvider BookmarkProvider { get; private set; }

		public event Action<ProgressEventArgs> ProgressBarUpdate;
		
		protected void OnProgressBarUpdate(ProgressEventArgs e)
		{
			if (ProgressBarUpdate != null)
			{
				ProgressBarUpdate(e);
			}
		}

		#endregion

		#region Properties

		public LogfileReader CurrentLogFileReader { get; protected set; }

		public string FileName { get; protected set; }

		protected EncodingOptions EncodingOptions { get; set; }
		
		//TODO Zarunbal: think about to return directly _guiStateArgs
		public bool IsMultiFile
		{
			get
			{
				return _guiStateArgs.IsMultiFileActive;
			}
			set
			{
				_guiStateArgs.IsMultiFileActive = value;
			}
		}

		public bool IsTempFile { get; protected set; }

		public string ForcedPersistenceFileName { get; set; }
		
		public Preferences Preferences
		{
			get
			{
				return ConfigManager.Settings.preferences;
			}
		}
		
		public ILogLineColumnizer CurrentColumnizer
		{
			get
			{
				return _currentColumnizer;
			}
			set
			{
				lock (_currentColumnizerLock)
				{
					_currentColumnizer = value;
					Logger.logDebug("Setting columnizer " + _currentColumnizer != null ? _currentColumnizer.GetName() : "<none>");
				}
			}
		}

		#endregion

		#region Public Methods
		
		public void PreselectColumnizer(string columnizerName)
		{
			ILogLineColumnizer columnizer = Util.FindColumnizerByName(columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
			PreSelectColumnizer(Util.CloneColumnizer(columnizer));
		}
		
		public void Close(bool dontAsk)
		{
			Preferences.askForClose = !dontAsk;
			Close();
		}
		
		public void LoadFile(string fileName, EncodingOptions encodingOptions)
		{
			LoadFileInternal(fileName, encodingOptions);
		}
		
		public void LoadFilesAsMulti(string[] fileNames, EncodingOptions encodingOptions)
		{
			LoadFilesAsMultiInternal(fileNames, encodingOptions);
		}

		#endregion

		#region Methods
		
		private void LogEventWorker()
		{
			Thread.CurrentThread.Name = "LogEventWorker";
			while (true)
			{
				Logger.logDebug("Waiting for signal");
				_logEventArgsEvent.WaitOne();
				Logger.logDebug("Wakeup signal received.");
				while (true)
				{
					LogEventArgs e;
					int lastLineCount = 0;
					lock (_logEventArgsList)
					{
						Logger.logInfo(string.Format("{0} events in queue", _logEventArgsList.Count));
						if (_logEventArgsList.Count == 0)
						{
							_logEventArgsEvent.Reset();
							break;
						}
						e = _logEventArgsList[0];
						_logEventArgsList.RemoveAt(0);
					}
					if (e.IsRollover)
					{
						BookmarkProvider.ShiftBookmarks(e.RolloverOffset);
						ShiftRowHeightList(e.RolloverOffset);
						ShiftFilterPipes(e.RolloverOffset);
						lastLineCount = 0;
					}
					else
					{
						if (e.LineCount < lastLineCount)
						{
							Logger.logError("Line count of event is: " + e.LineCount + ", should be greater than last line count: " + lastLineCount);
						}
					}
					Action<LogEventArgs> callback = new Action<LogEventArgs>(UpdateGrid);
					Invoke(callback, new object[] { e });
					CheckFilterAndHighlight(e);
					_timeSpreadCalc.SetLineCount(e.LineCount);
				}
			}
		}

		#region Load File
		
		private void LoadFileInternal(string fileName, EncodingOptions encodingOptions)
		{
			EnterLoadFileStatus();
			
			if (fileName != null)
			{
				FileName = fileName;
				EncodingOptions = encodingOptions;
				
				if (CurrentLogFileReader != null)
				{
					CurrentLogFileReader.StopMonitoringAsync();
					UnRegisterLogFileReaderEvents();
				}
				if (!LoadPersistenceOptions())
				{
					if (!IsTempFile)
					{
						ILogLineColumnizer columnizer = FindColumnizer();
						if (columnizer != null)
						{
							if (_reloadMemento == null)
							{
								columnizer = Util.CloneColumnizer(columnizer);
							}
						}
						PreSelectColumnizer(columnizer);
					}
					SetDefaultHighlightGroup();
				}
				
				// this may be set after loading persistence data
				if (_fileNames != null && IsMultiFile)
				{
					LoadFilesAsMulti(_fileNames, EncodingOptions);
					return;
				}
				_columnCache = new ColumnCache();
				try
				{
					CurrentLogFileReader = new LogfileReader(
						fileName,
						EncodingOptions,
						IsMultiFile,
						Preferences.bufferCount,
						Preferences.linesPerBuffer,
						_multifileOptions);
					CurrentLogFileReader.UseNewReader = !Preferences.useLegacyReader;
				}
				catch (LogFileException lfe)
				{
					MessageBox.Show("Cannot load file\n" + lfe.Message, "LogExpert");
					BeginInvoke(new Action<bool>(Close), new object[] { true });
					_isLoadError = true;
					return;
				}

				ILogLineXmlColumnizer xmlColumnizer = CurrentColumnizer as ILogLineXmlColumnizer;
				
				if (xmlColumnizer != null)
				{
					CurrentLogFileReader.IsXmlMode = true;
					CurrentLogFileReader.XmlLogConfig = xmlColumnizer.GetXmlLogConfiguration();
				}
				
				if (_forcedColumnizerForLoading != null)
				{
					CurrentColumnizer = _forcedColumnizerForLoading;
				}

				IPreProcessColumnizer preProcessColumnizer = CurrentColumnizer as IPreProcessColumnizer;
				
				if (CurrentColumnizer is IPreProcessColumnizer)
				{
					CurrentLogFileReader.PreProcessColumnizer = preProcessColumnizer;
				}
				else
				{
					CurrentLogFileReader.PreProcessColumnizer = null;
				}
				
				RegisterLogFileReaderEvents();
				Logger.logInfo("Loading logfile: " + fileName);
				CurrentLogFileReader.startMonitoring();
			}
		}
		
		private void LoadFilesAsMultiInternal(string[] fileNames, EncodingOptions encodingOptions)
		{
			Logger.logInfo("Loading given files as MultiFile:");

			EnterLoadFileStatus();
			
			foreach (string name in fileNames)
			{
				Logger.logInfo("File: " + name);
			}
			
			if (CurrentLogFileReader != null)
			{
				CurrentLogFileReader.stopMonitoring();
				UnRegisterLogFileReaderEvents();
			}
			
			EncodingOptions = encodingOptions;
			_columnCache = new ColumnCache();
				
			CurrentLogFileReader = new LogfileReader(
				fileNames,
				EncodingOptions,
				Preferences.bufferCount,
				Preferences.linesPerBuffer,
				_multifileOptions);
			
			CurrentLogFileReader.UseNewReader = !Preferences.useLegacyReader;
			RegisterLogFileReaderEvents();
			CurrentLogFileReader.startMonitoring();
			FileName = fileNames[fileNames.Length - 1];
			_fileNames = fileNames;
			IsMultiFile = true;
		}
		
		protected virtual void EnterLoadFileStatus()
		{
			Logger.logDebug("EnterLoadFileStatus begin");
			
			if (InvokeRequired)
			{
				Invoke(new MethodInvoker(EnterLoadFileStatus));
				return;
			}
			_statusEventArgs.StatusText = "Loading file...";
			_statusEventArgs.LineCount = 0;
			_statusEventArgs.FileSize = 0;
			SendStatusLineUpdate();
			
			_progressEventArgs.MinValue = 0;
			_progressEventArgs.MaxValue = 0;
			_progressEventArgs.Value = 0;
			_progressEventArgs.Visible = true;
			SendProgressBarUpdate();
			
			_isLoading = true;
			_shouldCancel = true;
			ClearFilterList();
			BookmarkProvider.ClearAllBookmarks();
		
			Logger.logDebug("EnterLoadFileStatus end");
		}

		#endregion
		
		protected void ShiftRowHeightList(int offset)
		{
			SortedList<int, RowHeightEntry> newList = new SortedList<int, RowHeightEntry>();
			foreach (RowHeightEntry entry in _rowHeightList.Values)
			{
				int line = entry.LineNum - offset;
				if (line >= 0)
				{
					entry.LineNum = line;
					newList.Add(line, entry);
				}
			}
			_rowHeightList = newList;
		}
		
		protected void ShiftFilterPipes(int offset)
		{
			lock (_filterPipeList)
			{
				foreach (FilterPipe pipe in _filterPipeList)
				{
					pipe.ShiftLineNums(offset);
				}
			}
		}
		
		protected void SendStatusLineUpdate()
		{
			_statusLineTrigger.Trigger();
		}
		
		protected void SendProgressBarUpdate()
		{
			OnProgressBarUpdate(_progressEventArgs);
		}
		
		protected virtual bool LoadPersistenceOptions()
		{
			if (InvokeRequired)
			{
				return (bool)Invoke(new Func<bool>(LoadPersistenceOptions));
			}
			
			if (!Preferences.saveSessions && ForcedPersistenceFileName == null)
			{
				return false;
			}
			
			try
			{
				PersistenceData persistenceData;
				if (ForcedPersistenceFileName == null)
				{
					persistenceData = Persister.LoadPersistenceDataOptionsOnly(FileName, Preferences);
				}
				else
				{
					persistenceData = Persister.LoadPersistenceDataOptionsOnlyFromFixedFile(ForcedPersistenceFileName);
				}
				
				if (persistenceData == null)
				{
					Logger.logInfo("No persistence data for " + FileName + " found.");
					return false;
				}

				LoadPersistenceOptions(persistenceData);
			
				return true;
			}
			catch (Exception ex)
			{
				Logger.logError("Error loading persistence data: " + ex.Message);
				return false;
			}
		}
		
		protected virtual void LoadPersistenceOptions(PersistenceData persistenceData)
		{
			IsMultiFile = persistenceData.multiFile;
			_multifileOptions = new MultifileOptions();
			_multifileOptions.FormatPattern = persistenceData.multiFilePattern;
			_multifileOptions.MaxDayTry = persistenceData.multiFileMaxDays;
			if (_multifileOptions.FormatPattern == null || _multifileOptions.FormatPattern.Length == 0)
			{
				_multifileOptions = ObjectClone.Clone<MultifileOptions>(Preferences.multifileOptions);
			}
			
			if (_reloadMemento == null)
			{
				PreselectColumnizer(persistenceData.columnizerName);
			}
			FollowTailChanged(persistenceData.followTail, false);
			if (persistenceData.tabName != null)
			{
				Text = persistenceData.tabName;
			}
			SetCurrentHighlightGroup(persistenceData.highlightGroupName);
			if (persistenceData.multiFileNames.Count > 0)
			{
				Logger.logInfo("Detected MultiFile name list in persistence options");
				_fileNames = new string[persistenceData.multiFileNames.Count];
				persistenceData.multiFileNames.CopyTo(_fileNames);
			}
			else
			{
				_fileNames = null;
			}
			SetExplicitEncoding(persistenceData.encoding);
		}
		
		protected void PreSelectColumnizer(ILogLineColumnizer columnizer)
		{
			if (columnizer != null)
			{
				CurrentColumnizer = _forcedColumnizerForLoading = columnizer;
			}
			else
			{
				CurrentColumnizer = _forcedColumnizerForLoading = PluginRegistry.GetInstance().RegisteredColumnizers[0];
			}
		}
		
		/// <summary>
		/// Set an Encoding which shall be used when loading a file. Used before a file is loaded.
		/// </summary>
		/// <param name="encoding"></param>
		protected void SetExplicitEncoding(Encoding encoding)
		{
			EncodingOptions.Encoding = encoding;
		}
		
		protected ILogLineColumnizer FindColumnizer()
		{
			ILogLineColumnizer columnizer = null;
			string path = Util.GetNameFromPath(FileName);
			
			if (Preferences.maskPrio)
			{
				columnizer = _parentLogTabWin.FindColumnizerByFileMask(path);
				if (columnizer == null)
				{
					columnizer = _parentLogTabWin.GetColumnizerHistoryEntry(FileName);
				}
			}
			else
			{
				columnizer = _parentLogTabWin.GetColumnizerHistoryEntry(FileName);
				if (columnizer == null)
				{
					columnizer = _parentLogTabWin.FindColumnizerByFileMask(path);
				}
			}
			return columnizer;
		}
		
		protected void SetDefaultHighlightGroup()
		{
			HilightGroup group = _parentLogTabWin.FindHighlightGroupByFileMask(FileName);
			if (group != null)
			{
				SetCurrentHighlightGroup(group.GroupName);
			}
			else
			{
				SetCurrentHighlightGroup("[Default]");
			}
		}
		
		protected int Search(SearchParams searchParams)
		{
			if (searchParams.searchText == null)
			{
				return -1;
			}

			Action<int> progressFx = new Action<int>(UpdateProgressBar);
			
			int lineNum = (searchParams.isFromTop && !searchParams.isFindNext) ? 0 : searchParams.currentLine;
			string lowerSearchText = searchParams.searchText.ToLower();
			int count = 0;
			bool hasWrapped = false;
			Regex regex = null;
			string regexPattern = null;
			while (true)
			{
				if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
				{
					if (lineNum >= CurrentLogFileReader.LineCount)
					{
						if (hasWrapped)
						{
							StatusLineError("Not found: " + searchParams.searchText);
							return -1;
						}
						lineNum = 0;
						count = 0;
						hasWrapped = true;
						StatusLineError("Started from beginning of file");
					}
				}
				else
				{
					if (lineNum < 0)
					{
						if (hasWrapped)
						{
							StatusLineError("Not found: " + searchParams.searchText);
							return -1;
						}
						count = 0;
						lineNum = CurrentLogFileReader.LineCount - 1;
						hasWrapped = true;
						StatusLineError("Started from end of file");
					}
				}

				string line = CurrentLogFileReader.GetLogLine(lineNum);
				
				if (line == null)
				{
					return -1;
				}
				
				if (searchParams.isRegex)
				{
					if (regex == null && string.IsNullOrEmpty(regexPattern) && object.ReferenceEquals(searchParams.searchText, regexPattern))
					{
						regex = new Regex(searchParams.searchText, searchParams.isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					}
					if (regex.IsMatch(line))
					{
						return lineNum;
					}
				}
				else
				{
					if (!searchParams.isCaseSensitive)
					{
						if (line.ToLower().Contains(lowerSearchText))
						{
							return lineNum;
						}
					}
					else
					{
						if (line.Contains(searchParams.searchText))
						{
							return lineNum;
						}
					}
				}
				
				if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
				{
					lineNum++;
				}
				else
				{
					lineNum--;
				}
				
				if (_shouldCancel)
				{
					return -1;
				}
				
				if (++count % PROGRESS_BAR_MODULO == 0)
				{
					try
					{
						if (!Disposing)
						{
							Invoke(progressFx, new object[] { count });
						}
					}
					catch (ObjectDisposedException)  // can occur when closing the app while searching
					{
					}
				}
			}
		}

		#endregion

		#region Abstract methods

		protected abstract void UpdateGrid(LogEventArgs e);

		protected abstract void CheckFilterAndHighlight(LogEventArgs e);

		protected abstract void ClearFilterList();

		protected abstract void UnRegisterLogFileReaderEvents();

		protected abstract void RegisterLogFileReaderEvents();

		public abstract void FollowTailChanged(bool isChecked, bool byTrigger);

		public abstract void SetCurrentHighlightGroup(string groupName);

		protected abstract void UpdateProgressBar(int value);
		
		protected abstract void StatusLineError(string text);
		
		#endregion
	}
}