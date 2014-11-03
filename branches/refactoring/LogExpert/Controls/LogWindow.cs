using LogExpert.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LogExpert.Dialogs;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Collections;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
	public partial class LogWindow : DockContent, ILogPaintContext, ILogView, ILogWindowSearch
	{
		#region Const

		private const int MAX_HISTORY = 30;
		private const int MAX_COLUMNIZER_HISTORY = 40;
		private const int SPREAD_MAX = 99;
		private const int PROGRESS_BAR_MODULO = 1000;
		private const int FILTER_ADCANCED_SPLITTER_DISTANCE = 54;

		#endregion

		#region Fields

		private Classes.FuzzyBlockDetection _fuzzyBlockDetection = new Classes.FuzzyBlockDetection();

		private LogfileReader _logFileReader;
		private ILogLineColumnizer _currentColumnizer;
		private readonly Object _currentColumnizerLock = new Object();
		private ILogLineColumnizer _forcedColumnizer;
		private ILogLineColumnizer _forcedColumnizerForLoading;
		private List<HilightEntry> _tempHilightEntryList = new List<HilightEntry>();
		private Object _tempHilightEntryListLock = new Object();
		private HilightGroup _currentHighlightGroup = new HilightGroup();
		private Object _currentHighlightGroupLock = new Object();
		private FilterParams _filterParams = new FilterParams();
		private SearchParams _currentSearchParams = null;
		private List<int> _filterResultList = new List<int>();
		private List<int> _lastFilterLinesList = new List<int>();
		private List<int> _filterHitList = new List<int>();
		private readonly BookmarkDataProvider _bookmarkProvider = new BookmarkDataProvider();
		private Object _bookmarkLock = new Object();

		private readonly IList<FilterPipe> _filterPipeList = new List<FilterPipe>();
		private int _filterPipeNameCounter = 0;
		private readonly Dictionary<Control, bool> _freezeStateMap = new Dictionary<Control, bool>();
		private SortedList<int, RowHeightEntry> _rowHeightList = new SortedList<int, RowHeightEntry>();

		private readonly List<LogEventArgs> _logEventArgsList = new List<LogEventArgs>();
		private readonly EventWaitHandle _logEventArgsEvent = new ManualResetEvent(false);
		private readonly Thread _logEventHandlerThread = null;

		private EventWaitHandle _filterUpdateEvent = new ManualResetEvent(false);

		private DelayedTrigger _statusLineTrigger = new DelayedTrigger(200);
		private DelayedTrigger _selectionChangedTrigger = new DelayedTrigger(200);

		private IList<BackgroundProcessCancelHandler> _cancelHandlerList = new List<BackgroundProcessCancelHandler>();

		private readonly EventWaitHandle _loadingFinishedEvent = new ManualResetEvent(false);
		private readonly EventWaitHandle _externaLoadingFinishedEvent = new ManualResetEvent(false); // used for external wait fx WaitForLoadFinished()

		#region Delegates

		private delegate void UpdateGridCallback(LogEventArgs e);

		private delegate void UpdateProgressCallback(LoadFileEventArgs e);

		private delegate void LoadingStartedFx(LoadFileEventArgs e);

		private delegate int SearchFx(SearchParams searchParams);

		private delegate void SelectLineFx(int line, bool triggerSyncCall);

		private delegate void FilterFx(FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterResultLines, List<int> filterHitList);

		private delegate void AddFilterLineGuiUpdateFx();

		private delegate void UpdateProgressBarFx(int lineNum);

		private delegate void SetColumnizerFx(ILogLineColumnizer columnizer);

		private delegate void ProcessFilterPipeFx(int lineNum);

		private delegate void WriteFilterToTabFinishedFx(FilterPipe pipe, string namePrefix, PersistenceData persistenceData);

		private delegate void TimestampSyncFx(int lineNum);

		private delegate void SetBookmarkFx(int lineNum, string comment);

		private delegate void UpdateBookmarkViewFx();

		private delegate void FunctionWith1IntParam(int arg);

		private delegate void FunctionWith1BoolParam(bool arg);

		private delegate void ActionPluginExecuteFx(string keyword, string param, ILogExpertCallback callback, ILogLineColumnizer columnizer);

		private delegate void HighlightEventFx(HighlightEventArgs e);

		private delegate void PositionAfterReloadFx(ReloadMemento reloadMemento);

		public delegate void LoadingFinishedFx(LogWindow newWin);    // used for filterTab restore

		public delegate void FilterRestoreFx(LogWindow newWin, PersistenceData persistenceData);

		public delegate void RestoreFiltersFx(PersistenceData persistenceData);

		public delegate void HideRowFx(int lineNum, bool show);

		public delegate bool ScrollToTimestampFx(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall);

		private delegate void AutoResizeColumnsFx(DataGridView gridView);

		private delegate bool BoolReturnDelegate();

		#endregion

		private bool _waitingForClose = false;
		private bool _isLoading = false;
		private bool _isSearching = false;
		private bool _shouldCancel = false;
		private bool _isMultiFile = false;
		private bool _isTempFile = false;
		private bool _showAdvanced = false;
		private bool _isErrorShowing = false;
		private bool _isTimestampDisplaySyncing = false;
		private bool _shouldTimestampDisplaySyncingCancel = false;
		private bool _isDeadFile = false;
		private bool _noSelectionUpdates = false;
		private bool _shouldCallTimeSync = false;
		private bool _isLoadError = false;

		private int _lineHeight = 0;
		private int _reloadOverloadCounter = 0;
		private readonly Object _reloadLock = new Object();
		private int _selectedCol = 0;    // set by context menu event for column headers only

		private readonly ProgressEventArgs _progressEventArgs = new ProgressEventArgs();
		private readonly GuiStateArgs _guiStateArgs = new GuiStateArgs();
		private readonly StatusLineEventArgs _statusEventArgs = new StatusLineEventArgs();

		private readonly Thread _timeshiftSyncThread = null;
		private readonly EventWaitHandle _timeshiftSyncWakeupEvent = new ManualResetEvent(false);
		private readonly EventWaitHandle _timeshiftSyncTimerEvent = new ManualResetEvent(false);
		private int _timeshiftSyncLine = 0;

		private string _fileNameField;
		private string[] _fileNames;
		private readonly LogTabWindow _parentLogTabWin;
		private MultifileOptions _multifileOptions = new MultifileOptions();

		private readonly TimeSpreadCalculator _timeSpreadCalc;
		private PatternWindow _patternWindow;
		private PatternArgs _patternArgs = new PatternArgs();

		private readonly LoadingFinishedFx _loadingFinishedFx;

		private Image _advancedButtonImage;
		private Image _searchButtonImage;

		private Image _panelOpenButtonImage;
		private Image _panelCloseButtonImage;

		private TimeSyncList _timeSyncList = null;
		private Object _timeSyncListLock = new Object();

		private Font _normalFont;
		private Font _fontBold;
		private Font _fontMonospaced;

		private ReloadMemento _reloadMemento;
		private ColumnCache _columnCache = new ColumnCache();

		#endregion

		#region cTor

		public LogWindow(LogTabWindow parent, string fileName, bool isTempFile, LoadingFinishedFx loadingFinishedFx, bool forcePersistenceLoading)
		{
			BookmarkColor = Color.FromArgb(165, 200, 225);
			TempTitleName = "";
			this.SuspendLayout();

			InitializeComponent();

			this.columnNamesLabel.Text = ""; // no filtering on columns by default

			this._parentLogTabWin = parent;
			this._isTempFile = isTempFile;
			this._loadingFinishedFx = loadingFinishedFx;
			ColumnizerCallbackObject = new ColumnizerCallback(this);

			this._fileNameField = fileName;
			this.ForcePersistenceLoading = forcePersistenceLoading;

			this.dataGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(DataGridView_CellValueNeeded);
			this.dataGridView.CellPainting += new DataGridViewCellPaintingEventHandler(DataGridView_CellPainting);

			this.filterGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(FilterGridView_CellValueNeeded);
			this.filterGridView.CellPainting += new DataGridViewCellPaintingEventHandler(FilterGridView_CellPainting);

			this.Closing += new CancelEventHandler(LogWindow_Closing);
			this.Disposed += new EventHandler(LogWindow_Disposed);

			this._timeSpreadCalc = new TimeSpreadCalculator(this);
			this.timeSpreadingControl1.TimeSpreadCalc = this._timeSpreadCalc;
			this.timeSpreadingControl1.LineSelected += new TimeSpreadingControl.LineSelectedEventHandler(TimeSpreadingControl1_LineSelected);
			this.tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Absolute;
			this.tableLayoutPanel1.ColumnStyles[1].Width = 20;
			this.tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Percent;
			this.tableLayoutPanel1.ColumnStyles[0].Width = 100;

			this._parentLogTabWin.HighlightSettingsChanged += Parent_HighlightSettingsChanged;

			SetColumnizer(PluginRegistry.GetInstance().RegisteredColumnizers[0]);

			_patternArgs.maxMisses = 5;
			_patternArgs.minWeight = 1;
			_patternArgs.maxDiffInBlock = 5;
			_patternArgs.fuzzy = 5;

			this._filterParams = new FilterParams();
			foreach (string item in ConfigManager.Settings.filterHistoryList)
			{
				this.filterComboBox.Items.Add(item);
			}
			this.filterRegexCheckBox.Checked = this._filterParams.isRegex;
			this.filterCaseSensitiveCheckBox.Checked = this._filterParams.isCaseSensitive;
			this.filterTailCheckBox.Checked = this._filterParams.isFilterTail;

			this.splitContainer1.Panel2Collapsed = true;
			this.advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;

			this._timeshiftSyncThread = new Thread(new ThreadStart(this.SyncTimestampDisplayWorker));
			this._timeshiftSyncThread.IsBackground = true;
			this._timeshiftSyncThread.Start();

			this._logEventHandlerThread = new Thread(new ThreadStart(this.LogEventWorker));
			this._logEventHandlerThread.IsBackground = true;
			this._logEventHandlerThread.Start();

			this._advancedButtonImage = this.advancedButton.Image;
			this._searchButtonImage = this.filterSearchButton.Image;
			this.filterSearchButton.Image = null;

			this.dataGridView.EditModeMenuStrip = this.editModeContextMenuStrip;
			this.markEditModeToolStripMenuItem.Enabled = true;

			this._panelOpenButtonImage = new Bitmap(GetType(), "Resources.PanelOpen.gif");
			this._panelCloseButtonImage = new Bitmap(GetType(), "Resources.PanelClose.gif");

			Settings settings = ConfigManager.Settings;
			if (settings.appBounds != null && settings.appBounds.Right > 0)
			{
				this.Bounds = settings.appBounds;
			}

			this._waitingForClose = false;
			this.dataGridView.Enabled = false;
			this.dataGridView.ColumnDividerDoubleClick += new DataGridViewColumnDividerDoubleClickEventHandler(DataGridView_ColumnDividerDoubleClick);
			ShowAdvancedFilterPanel(false);
			this.filterKnobControl1.MinValue = 0;
			this.filterKnobControl1.MaxValue = SPREAD_MAX;
			this.filterKnobControl1.ValueChanged += new KnobControl.ValueChangedEventHandler(FilterKnobControl1_CheckForFilterDirty);
			this.filterKnobControl2.MinValue = 0;
			this.filterKnobControl2.MaxValue = SPREAD_MAX;
			this.filterKnobControl2.ValueChanged += new KnobControl.ValueChangedEventHandler(FilterKnobControl1_CheckForFilterDirty);
			this.fuzzyKnobControl.MinValue = 0;
			this.fuzzyKnobControl.MaxValue = 10;
			AdjustHighlightSplitterWidth();
			ToggleHighlightPanel(false); // hidden

			_bookmarkProvider.BookmarkAdded += new BookmarkDataProvider.BookmarkAddedEventHandler(BookmarkProvider_BookmarkAdded);
			_bookmarkProvider.BookmarkRemoved += new BookmarkDataProvider.BookmarkRemovedEventHandler(BookmarkProvider_BookmarkRemoved);

			this.ResumeLayout();

			this._statusLineTrigger.Signal += new DelayedTrigger.SignalEventHandler(StatusLineTrigger_Signal);
			this._selectionChangedTrigger.Signal += new DelayedTrigger.SignalEventHandler(SelectionChangedTrigger_Signal);

			PreferencesChanged(this._parentLogTabWin.Preferences, true, SettingsFlags.GuiOrColors);
		}

		#endregion

		#region Properties

		public Color BookmarkColor { get; set; }

		public string FileName
		{
			get
			{
				return this._fileNameField;
			}
		}

		// unused?
		public string SessionFileName { get; set; }

		//TODO Zarunbal: think about to return directly _guiStateArgs
		public bool IsMultiFile
		{
			get
			{
				return _isMultiFile;
			}
			set
			{
				this._guiStateArgs.IsMultiFileActive = this._isMultiFile = value;
			}
		}

		public bool IsTempFile
		{
			get
			{
				return _isTempFile;
			}
		}

		public string TempTitleName { get; set; }

		public string Title
		{
			get
			{
				if (IsTempFile)
				{
					return TempTitleName;
				}
				else
				{
					return FileName;
				}
			}
		}

		public ColumnizerCallback ColumnizerCallbackObject { get; private set; }

		public bool ForcePersistenceLoading { get; set; }

		public string ForcedPersistenceFileName { get; set; }

		public Preferences Preferences
		{
			get
			{
				return ConfigManager.Settings.preferences;
			}
		}
		// file name of given file used for loading (maybe logfile or lxp)
		public string GivenFileName { get; set; }

		public TimeSyncList TimeSyncList
		{
			get
			{
				return this._timeSyncList;
			}
		}

		public bool IsTimeSynced
		{
			get
			{
				return this._timeSyncList != null;
			}
		}

		protected EncodingOptions EncodingOptions { get; set; }

		internal FilterPipe FilterPipe { get; set; }

		#endregion

		#region Public Methods

		public void LoadFile(string fileName, EncodingOptions encodingOptions)
		{
			EnterLoadFileStatus();

			if (fileName != null)
			{
				this._fileNameField = fileName;
				this.EncodingOptions = encodingOptions;

				if (this._logFileReader != null)
				{
					this._logFileReader.StopMonitoringAsync();
					UnRegisterLogFileReaderEvents();
				}
				if (!LoadPersistenceOptions())
				{
					if (!this.IsTempFile)
					{
						ILogLineColumnizer columnizer = FindColumnizer();
						if (columnizer != null)
						{
							if (this._reloadMemento == null)
							{
								columnizer = Util.CloneColumnizer(columnizer);
							}
						}
						PreSelectColumnizer(columnizer);
					}
					SetDefaultHighlightGroup();
				}

				// this may be set after loading persistence data
				if (this._fileNames != null && this.IsMultiFile)
				{
					LoadFilesAsMulti(this._fileNames, this.EncodingOptions);
					return;
				}
				this._columnCache = new ColumnCache();
				try
				{
					this._logFileReader = new LogfileReader(fileName, this.EncodingOptions, this.IsMultiFile,
						this.Preferences.bufferCount, this.Preferences.linesPerBuffer,
						this._multifileOptions);
					this._logFileReader.UseNewReader = !this.Preferences.useLegacyReader;
				}
				catch (LogFileException lfe)
				{
					MessageBox.Show("Cannot load file\n" + lfe.Message, "LogExpert");
					this.BeginInvoke(new FunctionWith1BoolParam(Close), new object[] { true });
					this._isLoadError = true;
					return;
				}

				if (this.CurrentColumnizer is ILogLineXmlColumnizer)
				{
					this._logFileReader.IsXmlMode = true;
					this._logFileReader.XmlLogConfig = (this.CurrentColumnizer as ILogLineXmlColumnizer).GetXmlLogConfiguration();
				}
				if (this._forcedColumnizerForLoading != null)
				{
					this.CurrentColumnizer = this._forcedColumnizerForLoading;
				}
				if (this.CurrentColumnizer is IPreProcessColumnizer)
				{
					this._logFileReader.PreProcessColumnizer = (IPreProcessColumnizer)this.CurrentColumnizer;
				}
				else
				{
					this._logFileReader.PreProcessColumnizer = null;
				}
				RegisterLogFileReaderEvents();
				Logger.logInfo("Loading logfile: " + fileName);
				this._logFileReader.startMonitoring();
			}
		}

		public void LoadFilesAsMulti(string[] fileNames, EncodingOptions encodingOptions)
		{
			Logger.logInfo("Loading given files as MultiFile:");

			EnterLoadFileStatus();

			foreach (string name in fileNames)
			{
				Logger.logInfo("File: " + name);
			}
			if (this._logFileReader != null)
			{
				this._logFileReader.stopMonitoring();
				UnRegisterLogFileReaderEvents();
			}
			this.EncodingOptions = encodingOptions;
			this._columnCache = new ColumnCache();
			this._logFileReader = new LogfileReader(fileNames, this.EncodingOptions, this.Preferences.bufferCount,
				this.Preferences.linesPerBuffer, this._multifileOptions);
			this._logFileReader.UseNewReader = !this.Preferences.useLegacyReader;
			RegisterLogFileReaderEvents();
			this._logFileReader.startMonitoring();
			this._fileNameField = fileNames[fileNames.Length - 1];
			this._fileNames = fileNames;
			this.IsMultiFile = true;
		}

		public string SavePersistenceData(bool force)
		{
			if (!force)
			{
				if (!this.Preferences.saveSessions)
				{
					return null;
				}
			}

			if (this._isTempFile || this._isLoadError)
			{
				return null;
			}

			try
			{
				PersistenceData persistenceData = GetPersistenceData();
				if (this.ForcedPersistenceFileName == null)
				{
					return Persister.SavePersistenceData(this.FileName, persistenceData, this.Preferences);
				}
				else
				{
					return Persister.SavePersistenceDataWithFixedName(this.ForcedPersistenceFileName, persistenceData);
				}
			}
			catch (IOException ex)
			{
				Logger.logError("Error saving persistence: " + ex.Message);
			}
			catch (Exception e)
			{
				MessageBox.Show("Unexpected error while saving persistence: " + e.Message);
			}
			return null;
		}

		public PersistenceData GetPersistenceData()
		{
			PersistenceData persistenceData = new PersistenceData();
			persistenceData.bookmarkList = this._bookmarkProvider.BookmarkList;
			persistenceData.rowHeightList = this._rowHeightList;
			persistenceData.multiFile = this.IsMultiFile;
			persistenceData.multiFilePattern = this._multifileOptions.FormatPattern;
			persistenceData.multiFileMaxDays = this._multifileOptions.MaxDayTry;
			persistenceData.currentLine = this.dataGridView.CurrentCellAddress.Y;
			persistenceData.firstDisplayedLine = this.dataGridView.FirstDisplayedScrollingRowIndex;
			persistenceData.filterVisible = !this.splitContainer1.Panel2Collapsed;
			persistenceData.filterAdvanced = !this.advancedFilterSplitContainer.Panel1Collapsed;
			persistenceData.filterPosition = this.splitContainer1.SplitterDistance;
			persistenceData.followTail = this._guiStateArgs.FollowTail;
			persistenceData.fileName = this.FileName;
			persistenceData.tabName = this.Text;
			persistenceData.sessionFileName = this.SessionFileName;
			persistenceData.columnizerName = this.CurrentColumnizer.GetName();
			persistenceData.lineCount = this._logFileReader.LineCount;
			this._filterParams.isFilterTail = this.filterTailCheckBox.Checked; // this option doesnt need a press on 'search'
			if (this.Preferences.saveFilters)
			{
				List<FilterParams> filterList = new List<FilterParams>();
				filterList.Add(this._filterParams);
				persistenceData.filterParamsList = filterList;

				foreach (FilterPipe filterPipe in this._filterPipeList)
				{
					FilterTabData data = new FilterTabData();
					data.persistenceData = filterPipe.OwnLogWindow.GetPersistenceData();
					data.filterParams = filterPipe.FilterParams;
					persistenceData.filterTabDataList.Add(data);
				}
			}
			if (this._currentHighlightGroup != null)
			{
				persistenceData.highlightGroupName = this._currentHighlightGroup.GroupName;
			}
			if (this._fileNames != null && this.IsMultiFile)
			{
				persistenceData.multiFileNames.AddRange(this._fileNames);
			}
			//persistenceData.showBookmarkCommentColumn = this.bookmarkWindow.ShowBookmarkCommentColumn;
			persistenceData.filterSaveListVisible = !this.highlightSplitContainer.Panel2Collapsed;
			persistenceData.encoding = this._logFileReader.CurrentEncoding;
			return persistenceData;
		}

		public void Close(bool dontAsk)
		{
			this.Preferences.askForClose = !dontAsk;
			Close();
		}

		public void WaitForLoadingFinished()
		{
			this._externaLoadingFinishedEvent.WaitOne();
		}

		public void CloseLogWindow()
		{
			StopTimespreadThread();
			StopTimestampSyncThread();
			StopLogEventWorkerThread();
			this._statusLineTrigger.Stop();
			this._selectionChangedTrigger.Stop();
			this._shouldCancel = true;
			if (this._logFileReader != null)
			{
				UnRegisterLogFileReaderEvents();
				this._logFileReader.StopMonitoringAsync();
			}
			if (this._isLoading)
			{
				this._waitingForClose = true;
			}
			if (this.IsTempFile)
			{
				Logger.logInfo("Deleting temp file " + this.FileName);
				try
				{
					File.Delete(this.FileName);
				}
				catch (IOException e)
				{
					Logger.logError("Error while deleting temp file " + this.FileName + ": " + e.ToString());
				}
			}
			if (this.FilterPipe != null)
			{
				this.FilterPipe.CloseAndDisconnect();
			}
			DisconnectFilterPipes();
		}

		public void ForceColumnizer(ILogLineColumnizer columnizer)
		{
			this._forcedColumnizer = Util.CloneColumnizer(columnizer);
			SetColumnizer(this._forcedColumnizer);
		}

		public void ForceColumnizerForLoading(ILogLineColumnizer columnizer)
		{
			this._forcedColumnizerForLoading = Util.CloneColumnizer(columnizer);
		}

		public void PreselectColumnizer(string columnizerName)
		{
			ILogLineColumnizer columnizer = Util.FindColumnizerByName(columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
			PreSelectColumnizer(Util.CloneColumnizer(columnizer));
		}

		private void PreSelectColumnizer(ILogLineColumnizer columnizer)
		{
			if (columnizer != null)
			{
				this.CurrentColumnizer = this._forcedColumnizerForLoading = columnizer;
			}
			else
			{
				this.CurrentColumnizer = this._forcedColumnizerForLoading = PluginRegistry.GetInstance().RegisteredColumnizers[0];
			}
		}

		public void ColumnizerConfigChanged()
		{
			SetColumnizerInternal(this.CurrentColumnizer);
		}

		private void SetColumnizer(ILogLineColumnizer columnizer)
		{
			int timeDiff = 0;
			if (this.CurrentColumnizer != null && this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				timeDiff = this.CurrentColumnizer.GetTimeOffset();
			}

			SetColumnizerInternal(columnizer);

			if (this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				this.CurrentColumnizer.SetTimeOffset(timeDiff);
			}
		}

		private void SetColumnizerInternal(ILogLineColumnizer columnizer)
		{
			Logger.logInfo("SetColumnizerInternal(): " + columnizer.GetName());

			ILogLineColumnizer oldColumnizer = this.CurrentColumnizer;
			bool oldColumnizerIsXmlType = this.CurrentColumnizer is ILogLineXmlColumnizer;
			bool oldColumnizerIsPreProcess = this.CurrentColumnizer is IPreProcessColumnizer;
			bool mustReload = false;

			// Check if the filtered columns disappeared, if so must refresh the UI
			if (this._filterParams.columnRestrict)
			{
				string[] newColumns = columnizer != null ? columnizer.GetColumnNames() : new string[0];
				bool colChanged = false;
				if (this.dataGridView.ColumnCount - 2 == newColumns.Length) // two first columns are 'marker' and 'line number'
				{
					for (int i = 0; i < newColumns.Length; i++)
					{
						if (this.dataGridView.Columns[i].HeaderText != newColumns[i])
						{
							colChanged = true;
							break; // one change is sufficient
						}
					}
				}
				else
				{
					colChanged = true;
				}

				if (colChanged)
				{
					// Update UI
					this.columnNamesLabel.Text = CalculateColumnNames(this._filterParams);
				}
			}

			Type oldColType = _filterParams.currentColumnizer != null ? this._filterParams.currentColumnizer.GetType() : null;
			Type newColType = columnizer != null ? columnizer.GetType() : null;
			if (oldColType != newColType && _filterParams.columnRestrict && this._filterParams.isFilterTail)
			{
				this._filterParams.columnList.Clear();
			}
			if (this.CurrentColumnizer == null || !this.CurrentColumnizer.GetType().Equals(columnizer.GetType()))
			{
				this.CurrentColumnizer = columnizer;
				this._freezeStateMap.Clear();
				if (this._logFileReader != null)
				{
					IPreProcessColumnizer preprocessColumnizer = CurrentColumnizer as IPreProcessColumnizer;
					if (preprocessColumnizer != null)
					{
						_logFileReader.PreProcessColumnizer = preprocessColumnizer;
					}
					else
					{
						_logFileReader.PreProcessColumnizer = null;
					}
				}
				// always reload when choosing XML columnizers
				if (_logFileReader != null && CurrentColumnizer is ILogLineXmlColumnizer)
				{
					//forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
					mustReload = true;
				}
				// Reload when choosing no XML columnizer but previous columnizer was XML
				if (_logFileReader != null && !(CurrentColumnizer is ILogLineXmlColumnizer) && oldColumnizerIsXmlType)
				{
					this._logFileReader.IsXmlMode = false;
					//forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
					mustReload = true;
				}
				// Reload when previous columnizer was PreProcess and current is not, and vice versa.
				// When the current columnizer is a preProcess columnizer, reload in every case.
				bool isCurrentColumnizerIPreProcessColumnizer = CurrentColumnizer is IPreProcessColumnizer;
				if ((isCurrentColumnizerIPreProcessColumnizer != oldColumnizerIsPreProcess) ||
					isCurrentColumnizerIPreProcessColumnizer
				)
				{
					//forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
					mustReload = true;
				}
			}
			else
			{
				CurrentColumnizer = columnizer;
			}

			IInitColumnizer initColumnizer = oldColumnizer as IInitColumnizer;

			if (initColumnizer != null)
			{
				initColumnizer.DeSelected(new ColumnizerCallback(this));
			}
			initColumnizer = columnizer as IInitColumnizer;
			if (initColumnizer != null)
			{
				initColumnizer.Selected(new ColumnizerCallback(this));
			}

			SetColumnizer(columnizer, dataGridView);
			SetColumnizer(columnizer, filterGridView);
			if (_patternWindow != null)
			{
				_patternWindow.SetColumnizer(columnizer);
			}

			_guiStateArgs.TimeshiftPossible = columnizer.IsTimeshiftImplemented();
			SendGuiStateUpdate();

			if (_logFileReader != null)
			{
				dataGridView.RowCount = _logFileReader.LineCount;
			}
			if (_filterResultList != null)
			{
				filterGridView.RowCount = _filterResultList.Count;
			}
			if (mustReload)
			{
				Reload();
			}
			else
			{
				if (CurrentColumnizer.IsTimeshiftImplemented())
				{
					SetTimestampLimits();
					SyncTimestampDisplay();
				}
				Settings settings = ConfigManager.Settings;
				ShowLineColumn(!settings.hideLineColumn);
				ShowTimeSpread(Preferences.showTimeSpread && columnizer.IsTimeshiftImplemented());
			}

			if (!columnizer.IsTimeshiftImplemented() && IsTimeSynced)
			{
				FreeFromTimeSync();
			}

			columnComboBox.Items.Clear();
			foreach (String columnName in columnizer.GetColumnNames())
			{
				this.columnComboBox.Items.Add(columnName);
			}
			this.columnComboBox.SelectedIndex = 0;

			OnColumnizerChanged(this.CurrentColumnizer);
		}

		public void SetColumnizer(ILogLineColumnizer columnizer, DataGridView gridView)
		{
			int rowCount = gridView.RowCount;
			int currLine = gridView.CurrentCellAddress.Y;
			int currFirstLine = gridView.FirstDisplayedScrollingRowIndex;

			gridView.Columns.Clear();

			DataGridViewTextBoxColumn markerColumn = new DataGridViewTextBoxColumn();
			markerColumn.HeaderText = "";
			markerColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
			markerColumn.Resizable = DataGridViewTriState.False;
			markerColumn.DividerWidth = 1;
			markerColumn.ReadOnly = true;
			markerColumn.HeaderCell.ContextMenuStrip = this.columnContextMenuStrip;
			gridView.Columns.Add(markerColumn);

			DataGridViewTextBoxColumn lineNumberColumn = new DataGridViewTextBoxColumn();
			lineNumberColumn.HeaderText = "Line";
			lineNumberColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
			lineNumberColumn.Resizable = DataGridViewTriState.NotSet;
			lineNumberColumn.DividerWidth = 1;
			lineNumberColumn.ReadOnly = true;
			lineNumberColumn.HeaderCell.ContextMenuStrip = this.columnContextMenuStrip;
			gridView.Columns.Add(lineNumberColumn);

			foreach (string colName in columnizer.GetColumnNames())
			{
				DataGridViewColumn titleColumn = new LogTextColumn();
				titleColumn.HeaderText = colName;
				titleColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
				titleColumn.Resizable = DataGridViewTriState.NotSet;
				titleColumn.DividerWidth = 1;
				titleColumn.HeaderCell.ContextMenuStrip = this.columnContextMenuStrip;
				gridView.Columns.Add(titleColumn);
			}

			this.columnNamesLabel.Text = CalculateColumnNames(this._filterParams);

			gridView.RowCount = rowCount;
			if (currLine != -1)
			{
				gridView.CurrentCell = gridView.Rows[currLine].Cells[0];
			}
			if (currFirstLine != -1)
			{
				gridView.FirstDisplayedScrollingRowIndex = currFirstLine;
			}
			gridView.Refresh();
			AutoResizeColumns(gridView);
			ApplyFrozenState(gridView);
		}

		public string GetCellValue(int rowIndex, int columnIndex)
		{
			if (columnIndex == 1)
			{
				return "" + (rowIndex + 1);   // line number
			}
			if (columnIndex == 0)   // marker column
			{
				return "";
			}

			try
			{
				string[] cols = GetColumnsForLine(rowIndex);
				if (cols != null)
				{
					if (columnIndex <= cols.Length + 1)
					{
						string value = cols[columnIndex - 2];
						if (value != null)
						{
							value = value.Replace("\t", "  ");
						}
						return value;
					}
					else
					{
						if (columnIndex == 2)
						{
							return cols[cols.Length - 1].Replace("\t", "  ");
						}
						else
							return "";
					}
				}
			}
			catch (Exception)
			{
				return "";
			}
			return "";
		}

		/**
		 * Returns the first HilightEntry that matches the given line
		 */
		public HilightEntry FindHilightEntry(string line, bool noWordMatches)
		{
			// first check the temp entries
			lock (this._tempHilightEntryListLock)
			{
				foreach (HilightEntry entry in this._tempHilightEntryList)
				{
					if (noWordMatches && entry.IsWordMatch)
					{
						continue;
					}
					if (CheckHighlightEntryMatch(entry, line))
					{
						return entry;
					}
				}
			}

			lock (this._currentHighlightGroupLock)
			{
				foreach (HilightEntry entry in this._currentHighlightGroup.HilightEntryList)
				{
					if (noWordMatches && entry.IsWordMatch)
					{
						continue;
					}
					if (CheckHighlightEntryMatch(entry, line))
					{
						return entry;
					}
				}
				return null;
			}
		}

		public IList<HilightMatchEntry> FindHilightMatches(string line)
		{
			IList<HilightMatchEntry> resultList = new List<HilightMatchEntry>();
			if (line != null)
			{
				lock (this._currentHighlightGroupLock)
				{
					GetHighlightEntryMatches(line, this._currentHighlightGroup.HilightEntryList, resultList);
				}
				lock (this._tempHilightEntryList)
				{
					GetHighlightEntryMatches(line, this._tempHilightEntryList, resultList);
				}
			}
			return resultList;
		}

		public void GotoLine(int line)
		{
			if (line >= 0)
			{
				if (line < this.dataGridView.RowCount)
				{
					SelectLine(line, false);
				}
				else
				{
					SelectLine(this.dataGridView.RowCount - 1, false);
				}
				this.dataGridView.Focus();
			}
		}

		public void StartSearch()
		{
			this._guiStateArgs.MenuEnabled = false;
			GuiStateUpdate(this, this._guiStateArgs);
			SearchParams searchParams = this._parentLogTabWin.SearchParams;
			if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
			{
				searchParams.currentLine = this.dataGridView.CurrentCellAddress.Y + 1;
			}
			else
			{
				searchParams.currentLine = this.dataGridView.CurrentCellAddress.Y - 1;
			}

			this._currentSearchParams = searchParams;    // remember for async "not found" messages

			this._isSearching = true;
			this._shouldCancel = false;
			StatusLineText("Searching... Press ESC to cancel.");

			this._progressEventArgs.MinValue = 0;
			this._progressEventArgs.MaxValue = this.dataGridView.RowCount;
			this._progressEventArgs.Value = 0;
			this._progressEventArgs.Visible = true;
			SendProgressBarUpdate();

			SearchFx searchFx = new SearchFx(Search);
			searchFx.BeginInvoke(searchParams, SearchComplete, null);

			RemoveAllSearchHighlightEntries();
			AddSearchHitHighlightEntry(searchParams);
		}

		public void FollowTailChanged(bool isChecked, bool byTrigger)
		{
			this._guiStateArgs.FollowTail = isChecked;

			if (this._guiStateArgs.FollowTail && _logFileReader != null)
			{
				if (this.dataGridView.RowCount >= this._logFileReader.LineCount && this._logFileReader.LineCount > 0)
				{
					this.dataGridView.FirstDisplayedScrollingRowIndex = this._logFileReader.LineCount - 1;
				}
			}
			this.BeginInvoke(new MethodInvoker(this.dataGridView.Refresh));
			//this.dataGridView.Refresh();
			this._parentLogTabWin.FollowTailChanged(this, isChecked, byTrigger);
			SendGuiStateUpdate();
		}

		public void SelectLogLine(int line)
		{
			this.Invoke(new SelectLineFx(this.SelectLine), new object[] { line, true });
		}

		public void SelectAndEnsureVisible(int line, bool triggerSyncCall)
		{
			try
			{
				SelectLine(line, triggerSyncCall, false);

				if (line < this.dataGridView.FirstDisplayedScrollingRowIndex ||
					line > this.dataGridView.FirstDisplayedScrollingRowIndex + this.dataGridView.DisplayedRowCount(false))
				{
					this.dataGridView.FirstDisplayedScrollingRowIndex = line;
					for (int i = 0;
						 i < 8 && this.dataGridView.FirstDisplayedScrollingRowIndex > 0 &&
						 line < this.dataGridView.FirstDisplayedScrollingRowIndex + this.dataGridView.DisplayedRowCount(false);
						 ++i)
					{
						this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.FirstDisplayedScrollingRowIndex - 1;
					}
					if (line >= this.dataGridView.FirstDisplayedScrollingRowIndex + this.dataGridView.DisplayedRowCount(false))
					{
						this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.FirstDisplayedScrollingRowIndex + 1;
					}
				}
				this.dataGridView.CurrentCell = this.dataGridView.Rows[line].Cells[0];
			}
			catch (Exception e)
			{
				// In rare situations there seems to be an invalid argument exceptions (or something like this). Concrete location isn't visible in stack
				// trace because use of Invoke(). So catch it, and log (better than crashing the app).
				Logger.logError(e.ToString());
			}
		}

		public void AddBookmarkOverlays()
		{
			const int OVERSCAN = 20;
			int firstLine = this.dataGridView.FirstDisplayedScrollingRowIndex;
			if (firstLine < 0)
			{
				return;
			}

			firstLine -= OVERSCAN;
			if (firstLine < 0)
			{
				firstLine = 0;
			}

			int oversizeCount = OVERSCAN;

			for (int i = firstLine; i < this.dataGridView.RowCount; ++i)
			{
				if (!this.dataGridView.Rows[i].Displayed && i > this.dataGridView.FirstDisplayedScrollingRowIndex)
				{
					if (oversizeCount-- < 0)
					{
						break;
					}
				}
				if (this._bookmarkProvider.IsBookmarkAtLine(i))
				{
					Bookmark bookmark = this._bookmarkProvider.GetBookmarkForLine(i);
					if (bookmark.Text.Length > 0)
					{
						BookmarkOverlay overlay = bookmark.Overlay;
						overlay.Bookmark = bookmark;

						Rectangle r;
						if (this.dataGridView.Rows[i].Displayed)
						{
							r = this.dataGridView.GetCellDisplayRectangle(0, i, false);
						}
						else
						{
							r = this.dataGridView.GetCellDisplayRectangle(0, this.dataGridView.FirstDisplayedScrollingRowIndex, false);
							int heightSum = 0;
							if (this.dataGridView.FirstDisplayedScrollingRowIndex < i)
							{
								for (int rn = this.dataGridView.FirstDisplayedScrollingRowIndex + 1; rn < i; ++rn)
								{
									heightSum += GetRowHeight(rn);
								}
								r.Offset(0, r.Height + heightSum);
							}
							else
							{
								for (int rn = this.dataGridView.FirstDisplayedScrollingRowIndex + 1; rn > i; --rn)
								{
									heightSum += GetRowHeight(rn);
								}
								r.Offset(0, -(r.Height + heightSum));
							}
						}
						if (Logger.IsDebug)
						{
							Logger.logDebug("AddBookmarkOverlay() r.Location=" + r.Location.X + ", width=" + r.Width + ", scroll_offset=" + this.dataGridView.HorizontalScrollingOffset);
						}
						overlay.Position = r.Location - new Size(this.dataGridView.HorizontalScrollingOffset, 0);
						overlay.Position = overlay.Position + new Size(10, r.Height / 2);
						this.dataGridView.AddOverlay(overlay);
					}
				}
			}
		}

		public bool ShowBookmarkBubbles
		{
			get
			{
				return this._guiStateArgs.ShowBookmarkBubbles;
			}
			set
			{
				this._guiStateArgs.ShowBookmarkBubbles = this.dataGridView.PaintWithOverlays = value;
				this.dataGridView.Refresh();
			}
		}

		public void ToggleBookmark()
		{
			DataGridView gridView;
			int lineNum;

			if (this.filterGridView.Focused)
			{
				gridView = this.filterGridView;
				if (gridView.CurrentCellAddress == null || gridView.CurrentCellAddress.Y == -1)
				{
					return;
				}
				lineNum = this._filterResultList[gridView.CurrentCellAddress.Y];
			}
			else
			{
				gridView = this.dataGridView;
				if (gridView.CurrentCellAddress == null || gridView.CurrentCellAddress.Y == -1)
				{
					return;
				}
				lineNum = this.dataGridView.CurrentCellAddress.Y;
			}

			ToggleBookmark(lineNum);
		}

		public void ToggleBookmark(int lineNum)
		{
			if (this._bookmarkProvider.IsBookmarkAtLine(lineNum))
			{
				Bookmark bookmark = this._bookmarkProvider.GetBookmarkForLine(lineNum);
				if (bookmark.Text != null && bookmark.Text.Length > 0)
				{
					if (DialogResult.No == MessageBox.Show("There's a comment attached to the bookmark. Really remove the bookmark?", "LogExpert", MessageBoxButtons.YesNo))
					{
						return;
					}
				}
				this._bookmarkProvider.RemoveBookmarkForLine(lineNum);
			}
			else
			{
				this._bookmarkProvider.AddBookmark(new Bookmark(lineNum));
			}
			this.dataGridView.Refresh();
			this.filterGridView.Refresh();
			OnBookmarkAdded();
		}

		public void SetBookmarkFromTrigger(int lineNum, string comment)
		{
			lock (this._bookmarkLock)
			{
				string line = this._logFileReader.GetLogLine(lineNum);
				if (line == null)
				{
					return;
				}
				ParamParser paramParser = new ParamParser(comment);
				try
				{
					comment = paramParser.ReplaceParams(line, lineNum, this.FileName);
				}
				catch (ArgumentException)
				{
					// occurs on invalid regex 
				}
				if (this._bookmarkProvider.IsBookmarkAtLine(lineNum))
				{
					this._bookmarkProvider.RemoveBookmarkForLine(lineNum);
				}
				this._bookmarkProvider.AddBookmark(new Bookmark(lineNum, comment));
				OnBookmarkAdded();
			}
		}

		public void JumpToNextBookmark(bool isForward)
		{
			int currentBookMarkCount = _bookmarkProvider.Bookmarks.Count;
			if (currentBookMarkCount > 0)
			{
				int bookmarkIndex = 0;

				bookmarkIndex = FindNextBookmarkIndex(this.dataGridView.CurrentCellAddress.Y, isForward);

				bookmarkIndex = currentBookMarkCount.SanitizeIndex(bookmarkIndex);

				if (this.filterGridView.Focused)
				{
					int startIndex = bookmarkIndex;
					bool wrapped = false;

					//Search for a bookmarked and visible line
					while (true)
					{
						int bookMarkedLine = _bookmarkProvider.Bookmarks[bookmarkIndex].LineNum;
						if (this._filterResultList.Contains(bookMarkedLine))
						{
							//Bookmarked Line is in the filtered list display it
							int filterLine = _filterResultList.IndexOf(bookMarkedLine);
							filterGridView.Rows[filterLine].Selected = true;
							filterGridView.CurrentCell = filterGridView.Rows[filterLine].Cells[0];
							break;
						}

						//Bookmarked line is not visible with the current filter, search for another
						bookmarkIndex = currentBookMarkCount.GetNextIndex(bookmarkIndex, isForward, out wrapped);

						if (wrapped &&
							((isForward && bookmarkIndex >= startIndex) ||
							 (!isForward && bookmarkIndex <= startIndex)))
						{
							//We checked already this index, break out of the loop
							break;
						}
					}
				}
				else
				{
					int lineNum = _bookmarkProvider.Bookmarks[bookmarkIndex].LineNum;
					SelectLine(lineNum, false);
				}
			}
		}

		public void DeleteBookmarks(List<int> lineNumList)
		{
			bool bookmarksPresent = false;
			foreach (int lineNum in lineNumList)
			{
				if (lineNum != -1)
				{
					if (this._bookmarkProvider.IsBookmarkAtLine(lineNum) && this._bookmarkProvider.GetBookmarkForLine(lineNum).Text.Length > 0)
					{
						bookmarksPresent = true;
					}
				}
			}
			if (bookmarksPresent)
			{
				if (MessageBox.Show("There are some comments in the bookmarks. Really remove bookmarks?", "LogExpert", MessageBoxButtons.YesNo) == DialogResult.No)
				{
					return;
				}
			}
			_bookmarkProvider.RemoveBookmarksForLines(lineNumList);
			OnBookmarkRemoved();
		}

		public void LogWindowActivated()
		{
			if (this._guiStateArgs.FollowTail && !this._isDeadFile)
			{
				OnTailFollowed(new EventArgs());
			}
			if (this.Preferences.timestampControl)
			{
				SetTimestampLimits();
				SyncTimestampDisplay();
			}
			this.dataGridView.Focus();

			SendGuiStateUpdate();
			SendStatusLineUpdate();
			SendProgressBarUpdate();
		}

		public void SetCellSelectionMode(bool isCellMode)
		{
			if (isCellMode)
			{
				this.dataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
			}
			else
			{
				this.dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			}
			this._guiStateArgs.CellSelectMode = isCellMode;
		}

		public void TimeshiftEnabled(bool isEnabled, string shiftValue)
		{
			this._guiStateArgs.TimeshiftEnabled = isEnabled;
			SetTimestampLimits();
			SetTimeshiftValue(shiftValue);
		}

		public void SetTimeshiftValue(string value)
		{
			this._guiStateArgs.TimeshiftText = value;
			if (this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				try
				{
					if (this._guiStateArgs.TimeshiftEnabled)
					{
						try
						{
							string text = this._guiStateArgs.TimeshiftText;
							if (text.StartsWith("+"))
							{
								text = text.Substring(1);
							}
							TimeSpan timeSpan = TimeSpan.Parse(text);
							int diff = (int)(timeSpan.Ticks / TimeSpan.TicksPerMillisecond);
							this.CurrentColumnizer.SetTimeOffset(diff);
						}
						catch (Exception)
						{
							this.CurrentColumnizer.SetTimeOffset(0);
						}
					}
					else
						this.CurrentColumnizer.SetTimeOffset(0);
					this.dataGridView.Refresh();
					this.filterGridView.Refresh();
					if (this.CurrentColumnizer.IsTimeshiftImplemented())
					{
						SetTimestampLimits();
						SyncTimestampDisplay();
					}
				}
				catch (FormatException ex)
				{
					Logger.logError(ex.StackTrace);
				}
			}
		}

		public void CopyMarkedLinesToTab()
		{
			if (this.dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect)
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in this.dataGridView.SelectedRows)
				{
					if (row.Index != -1)
					{
						lineNumList.Add(row.Index);
					}
				}
				lineNumList.Sort();
				// create dummy FilterPipe for connecting line numbers to original window
				// setting IsStopped to true prevents further filter processing
				FilterPipe pipe = new FilterPipe(new FilterParams(), this);
				pipe.IsStopped = true;
				WritePipeToTab(pipe, lineNumList, this.Text + "->C", null);
			}
			else
			{
				string fileName = Path.GetTempFileName();
				FileStream fStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				StreamWriter writer = new StreamWriter(fStream, Encoding.Unicode);

				DataObject data = this.dataGridView.GetClipboardContent();
				string text = data.GetText(TextDataFormat.Text);
				writer.Write(text);

				writer.Close();
				string title = Util.GetNameFromPath(this.FileName) + "->Clip";
				this._parentLogTabWin.AddTempFileTab(fileName, title);
			}
		}

		/// <summary>
		/// Change the file encoding. May force a reload if byte count ot preamble lenght differs from previous used encoding.
		/// </summary>
		/// <param name="encoding"></param>
		public void ChangeEncoding(Encoding encoding)
		{
			this._logFileReader.ChangeEncoding(encoding);
			this.EncodingOptions.Encoding = encoding;
			if (this._guiStateArgs.CurrentEncoding.IsSingleByte != encoding.IsSingleByte ||
				this._guiStateArgs.CurrentEncoding.GetPreamble().Length != encoding.GetPreamble().Length)
			{
				Reload();
			}
			else
			{
				this.dataGridView.Refresh();
				SendGuiStateUpdate();
			}
			this._guiStateArgs.CurrentEncoding = this._logFileReader.CurrentEncoding;
		}

		public void Reload()
		{
			SavePersistenceData(false);

			this._reloadMemento = new ReloadMemento();
			this._reloadMemento.currentLine = this.dataGridView.CurrentCellAddress.Y;
			this._reloadMemento.firstDisplayedLine = this.dataGridView.FirstDisplayedScrollingRowIndex;
			this._forcedColumnizerForLoading = this.CurrentColumnizer;

			if (this._fileNames == null || !this.IsMultiFile)
			{
				LoadFile(this.FileName, this.EncodingOptions);
			}
			else
			{
				LoadFilesAsMulti(this._fileNames, this.EncodingOptions);
			}
		}

		public void PreferencesChanged(Preferences newPreferences, bool isLoadTime, SettingsFlags flags)
		{
			if ((flags & SettingsFlags.GuiOrColors) == SettingsFlags.GuiOrColors)
			{
				_normalFont = new Font(new FontFamily(newPreferences.fontName), newPreferences.fontSize);
				this._fontBold = new Font(this.NormalFont, FontStyle.Bold);
				this._fontMonospaced = new Font("Courier New", this.Preferences.fontSize, FontStyle.Bold);

				this.dataGridView.DefaultCellStyle.Font = NormalFont;
				this.filterGridView.DefaultCellStyle.Font = NormalFont;
				this._lineHeight = NormalFont.Height + 4;
				this.dataGridView.RowTemplate.Height = NormalFont.Height + 4;

				this.ShowBookmarkBubbles = this.Preferences.showBubbles;

				ApplyDataGridViewPrefs(this.dataGridView, newPreferences);
				ApplyDataGridViewPrefs(this.filterGridView, newPreferences);

				if (this.Preferences.timestampControl)
				{
					SetTimestampLimits();
					SyncTimestampDisplay();
				}
				if (isLoadTime)
				{
					this.filterTailCheckBox.Checked = this.Preferences.filterTail;
					this.syncFilterCheckBox.Checked = this.Preferences.filterSync;
				}

				this._timeSpreadCalc.TimeMode = this.Preferences.timeSpreadTimeMode;
				this.timeSpreadingControl1.ForeColor = this.Preferences.timeSpreadColor;
				this.timeSpreadingControl1.ReverseAlpha = this.Preferences.reverseAlpha;
				if (this.CurrentColumnizer.IsTimeshiftImplemented())
				{
					this.timeSpreadingControl1.Invoke(new MethodInvoker(this.timeSpreadingControl1.Refresh));
					ShowTimeSpread(this.Preferences.showTimeSpread);
				}
				ToggleColumnFinder(this.Preferences.showColumnFinder, false);
			}

			if ((flags & SettingsFlags.FilterList) == SettingsFlags.FilterList)
			{
				HandleChangedFilterList();
			}

			if ((flags & SettingsFlags.FilterHistory) == SettingsFlags.FilterHistory)
			{
				UpdateFilterHistoryFromSettings();
			}
		}

		public bool ScrollToTimestamp(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new ScrollToTimestampFx(this.ScrollToTimestampWorker), new object[] { timestamp, roundToSeconds, triggerSyncCall });
				return true;
			}
			else
			{
				return ScrollToTimestampWorker(timestamp, roundToSeconds, triggerSyncCall);
			}
		}

		public bool ScrollToTimestampWorker(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall)
		{
			bool hasScrolled = false;
			if (!this.CurrentColumnizer.IsTimeshiftImplemented() || this.dataGridView.RowCount == 0)
			{
				return false;
			}

			int currentLine = this.dataGridView.CurrentCellAddress.Y;
			if (currentLine < 0 || currentLine >= this.dataGridView.RowCount)
			{
				currentLine = 0;
			}
			int foundLine = FindTimestampLine(currentLine, timestamp, roundToSeconds);
			if (foundLine >= 0)
			{
				SelectAndEnsureVisible(foundLine, triggerSyncCall);
				hasScrolled = true;
			}
			return hasScrolled;
		}

		public int FindTimestampLine(int lineNum, DateTime timestamp, bool roundToSeconds)
		{
			int foundLine = FindTimestampLine_Internal(lineNum, 0, this.dataGridView.RowCount - 1, timestamp, roundToSeconds);
			if (foundLine >= 0)
			{
				// go backwards to the first occurence of the hit
				DateTime foundTimestamp = GetTimestampForLine(ref foundLine, roundToSeconds);
				while (foundTimestamp.CompareTo(timestamp) == 0 && foundLine >= 0)
				{
					foundLine--;
					foundTimestamp = GetTimestampForLine(ref foundLine, roundToSeconds);
				}
				if (foundLine < 0)
				{
					return 0;
				}
				else
				{
					foundLine++;
					GetTimestampForLineForward(ref foundLine, roundToSeconds); // fwd to next valid timestamp
					return foundLine;
				}
			}
			return -foundLine;
		}

		public int FindTimestampLine_Internal(int lineNum, int rangeStart, int rangeEnd, DateTime timestamp, bool roundToSeconds)
		{
			Logger.logDebug("FindTimestampLine_Internal(): timestamp=" + timestamp + ", lineNum=" + lineNum + ", rangeStart=" + rangeStart + ", rangeEnd=" + rangeEnd);
			int refLine = lineNum;
			DateTime currentTimestamp = GetTimestampForLine(ref refLine, roundToSeconds);
			if (currentTimestamp.CompareTo(timestamp) == 0)
			{
				return lineNum;
			}
			if (timestamp < currentTimestamp)
			{
				rangeEnd = lineNum;
			}
			else
			{
				rangeStart = lineNum;
			}

			if (rangeEnd - rangeStart <= 0)
			{
				return -lineNum;
			}

			lineNum = (rangeEnd - rangeStart) / 2 + rangeStart;
			// prevent endless loop
			if (rangeEnd - rangeStart < 2)
			{
				currentTimestamp = GetTimestampForLine(ref rangeStart, roundToSeconds);
				if (currentTimestamp.CompareTo(timestamp) == 0)
				{
					return rangeStart;
				}
				currentTimestamp = GetTimestampForLine(ref rangeEnd, roundToSeconds);
				if (currentTimestamp.CompareTo(timestamp) == 0)
				{
					return rangeEnd;
				}
				return -lineNum;
			}

			return FindTimestampLine_Internal(lineNum, rangeStart, rangeEnd, timestamp, roundToSeconds);
		}

		/**
		 * Get the timestamp for the given line number. If the line
		 * has no timestamp, the previous line will be checked until a
		 * timestamp is found.
		 */
		public DateTime GetTimestampForLine(ref int lineNum, bool roundToSeconds)
		{
			lock (this._currentColumnizerLock)
			{
				if (!this.CurrentColumnizer.IsTimeshiftImplemented())
				{
					return DateTime.MinValue;
				}
				Logger.logDebug("GetTimestampForLine(" + lineNum + ") enter");
				DateTime timeStamp = DateTime.MinValue;
				bool lookBack = false;
				if (lineNum >= 0 && lineNum < this.dataGridView.RowCount)
				{
					while (timeStamp.CompareTo(DateTime.MinValue) == 0 && lineNum >= 0)
					{
						if (this._isTimestampDisplaySyncing && this._shouldTimestampDisplaySyncingCancel)
						{
							return DateTime.MinValue;
						}
						lookBack = true;
						string logLine = this._logFileReader.GetLogLine(lineNum);
						if (logLine == null)
						{
							return DateTime.MinValue;
						}
						this.ColumnizerCallbackObject.LineNum = lineNum;
						timeStamp = this.CurrentColumnizer.GetTimestamp(this.ColumnizerCallbackObject, logLine);
						if (roundToSeconds)
						{
							timeStamp = timeStamp.Subtract(TimeSpan.FromMilliseconds(timeStamp.Millisecond));
						}
						lineNum--;
					}
				}
				if (lookBack)
					lineNum++;
				Logger.logDebug("GetTimestampForLine() leave with lineNum=" + lineNum);
				return timeStamp;
			}
		}

		/**
		 * Get the timestamp for the given line number. If the line
		 * has no timestamp, the next line will be checked until a
		 * timestamp is found.
		 */
		public DateTime GetTimestampForLineForward(ref int lineNum, bool roundToSeconds)
		{
			lock (this._currentColumnizerLock)
			{
				if (!this.CurrentColumnizer.IsTimeshiftImplemented())
				{
					return DateTime.MinValue;
				}

				DateTime timeStamp = DateTime.MinValue;
				bool lookFwd = false;
				if (lineNum >= 0 && lineNum < this.dataGridView.RowCount)
				{
					while (timeStamp.CompareTo(DateTime.MinValue) == 0 && lineNum < this.dataGridView.RowCount)
					{
						lookFwd = true;
						string logLine = this._logFileReader.GetLogLine(lineNum);
						if (logLine == null)
						{
							timeStamp = DateTime.MinValue;
							break;
						}
						timeStamp = this.CurrentColumnizer.GetTimestamp(this.ColumnizerCallbackObject, logLine);
						if (roundToSeconds)
						{
							timeStamp = timeStamp.Subtract(TimeSpan.FromMilliseconds(timeStamp.Millisecond));
						}
						lineNum++;
					}
				}
				if (lookFwd)
					lineNum--;
				return timeStamp;
			}
		}

		public void AppFocusLost()
		{
			InvalidateCurrentRow(this.dataGridView);
		}

		public void AppFocusGained()
		{
			InvalidateCurrentRow(this.dataGridView);
		}

		public string GetCurrentLine()
		{
			if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index != -1)
			{
				return this._logFileReader.GetLogLine(this.dataGridView.CurrentRow.Index);
			}
			return null;
		}

		public string GetLine(int lineNum)
		{
			if (lineNum < 0 || lineNum >= this._logFileReader.LineCount)
			{
				return null;
			}
			return this._logFileReader.GetLogLine(lineNum);
		}

		public int GetCurrentLineNum()
		{
			if (this.dataGridView.CurrentRow == null)
			{
				return -1;
			}
			return this.dataGridView.CurrentRow.Index;
		}

		public int GetRealLineNum()
		{
			int lineNum = this.GetCurrentLineNum();
			if (lineNum == -1)
			{
				return -1;
			}
			return this._logFileReader.GetRealLineNumForVirtualLineNum(lineNum);
		}

		public string GetCurrentFileName()
		{
			if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index != -1)
			{
				return this._logFileReader.GetLogFileNameForLine(this.dataGridView.CurrentRow.Index);
			}
			return null;
		}

		public ILogFileInfo GetCurrentFileInfo()
		{
			if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index != -1)
			{
				return this._logFileReader.GetLogFileInfoForLine(this.dataGridView.CurrentRow.Index);
			}
			return null;
		}

		/// <summary>
		/// zero-based
		/// </summary>
		/// <param name="lineNum"></param>
		/// <returns></returns>
		public string GetCurrentFileName(int lineNum)
		{
			return this._logFileReader.GetLogFileNameForLine(lineNum);
		}

		public void ShowLineColumn(bool show)
		{
			this.dataGridView.Columns[1].Visible = show;
			this.filterGridView.Columns[1].Visible = show;
		}

		public void PatternStatistic()
		{
			InitPatternWindow();
		}

		public void PatternStatisticSelectRange(PatternArgs patternArgs)
		{
			if (this.dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect)
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in this.dataGridView.SelectedRows)
				{
					if (row.Index != -1)
					{
						lineNumList.Add(row.Index);
					}
				}
				lineNumList.Sort();
				patternArgs.startLine = lineNumList[0];
				patternArgs.endLine = lineNumList[lineNumList.Count - 1];
			}
			else
			{
				if (this.dataGridView.CurrentCellAddress.Y != -1)
				{
					patternArgs.startLine = this.dataGridView.CurrentCellAddress.Y;
				}
				else
				{
					patternArgs.startLine = 0;
				}
				patternArgs.endLine = this.dataGridView.RowCount - 1;
			}
		}

		public void PatternStatistic(PatternArgs patternArgs)
		{
			Action<PatternArgs, Interfaces.ILogWindowSearch> fx = new Action<PatternArgs, Interfaces.ILogWindowSearch>(_fuzzyBlockDetection.TestStatistic);
			fx.BeginInvoke(patternArgs, this, null, null);
		}

		public void ExportBookmarkList()
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Title = "Choose a file to save bookmarks into";
			dlg.AddExtension = true;
			dlg.DefaultExt = "csv";
			dlg.Filter = "CSV file (*.csv)|*.csv|Bookmark file (*.bmk)|*.bmk";
			dlg.FilterIndex = 1;
			dlg.FileName = Path.GetFileNameWithoutExtension(this.FileName);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					BookmarkExporter.ExportBookmarkList(this._bookmarkProvider.BookmarkList, this.FileName, dlg.FileName);
				}
				catch (IOException e)
				{
					MessageBox.Show("Error while exporting bookmark list: " + e.Message, "LogExpert");
				}
			}
		}

		public void ImportBookmarkList()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Choose a file to load bookmarks from";
			dlg.AddExtension = true;
			dlg.DefaultExt = "csv";
			dlg.DefaultExt = "csv";
			dlg.Filter = "CSV file (*.csv)|*.csv|Bookmark file (*.bmk)|*.bmk";
			dlg.FilterIndex = 1;
			dlg.FileName = Path.GetFileNameWithoutExtension(this.FileName);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					// add to the existing bookmarks
					var newBookmarks = new SortedList<int, Bookmark>();
					BookmarkExporter.ImportBookmarkList(this.FileName, dlg.FileName, newBookmarks);

					// Add (or replace) to existing bookmark list
					bool bookmarkAdded = false;
					foreach (var b in newBookmarks.Values)
					{
						if (!this._bookmarkProvider.BookmarkList.ContainsKey(b.LineNum))
						{
							this._bookmarkProvider.BookmarkList.Add(b.LineNum, b);
							bookmarkAdded = true; // refresh the list only once at the end
						}
						else
						{
							var existingBookmark = this._bookmarkProvider.BookmarkList[b.LineNum];
							existingBookmark.Text = b.Text; // replace existing bookmark for that line, preserving the overlay
							OnBookmarkTextChanged(b);
						}
					}

					// Refresh the lists
					if (bookmarkAdded)
					{
						OnBookmarkAdded();
					}
					this.dataGridView.Refresh();
					this.filterGridView.Refresh();
				}
				catch (IOException e)
				{
					MessageBox.Show("Error while importing bookmark list: " + e.Message, "LogExpert");
				}
			}
		}

		public bool IsAdvancedOptionActive()
		{
			return (this.rangeCheckBox.Checked ||
					this.fuzzyKnobControl.Value > 0 ||
					this.filterKnobControl1.Value > 0 ||
					this.filterKnobControl2.Value > 0 ||
					this.invertFilterCheckBox.Checked ||
					this.columnRestrictCheckBox.Checked);
		}

		public void HandleChangedFilterList()
		{
			this.Invoke(new MethodInvoker(HandleChangedFilterListWorker));
		}

		public void HandleChangedFilterListWorker()
		{
			int index = this.filterListBox.SelectedIndex;
			this.filterListBox.Items.Clear();
			foreach (FilterParams filterParam in ConfigManager.Settings.filterList)
			{
				this.filterListBox.Items.Add(filterParam);
			}
			this.filterListBox.Refresh();
			if (index >= 0 && index < this.filterListBox.Items.Count)
			{
				this.filterListBox.SelectedIndex = index;
			}
			this.filterOnLoadCheckBox.Checked = this.Preferences.isFilterOnLoad;
			this.hideFilterListOnLoadCheckBox.Checked = this.Preferences.isAutoHideFilterList;
		}

		public void SetCurrentHighlightGroup(string groupName)
		{
			this._guiStateArgs.HighlightGroupName = groupName;
			lock (this._currentHighlightGroupLock)
			{
				this._currentHighlightGroup = this._parentLogTabWin.FindHighlightGroup(groupName);
				if (this._currentHighlightGroup == null)
				{
					if (this._parentLogTabWin.HilightGroupList.Count > 0)
					{
						this._currentHighlightGroup = this._parentLogTabWin.HilightGroupList[0];
					}
					else
					{
						this._currentHighlightGroup = new HilightGroup();
					}
				}
				this._guiStateArgs.HighlightGroupName = this._currentHighlightGroup.GroupName;
			}
			SendGuiStateUpdate();
			this.BeginInvoke(new MethodInvoker(RefreshAllGrids));
		}

		public void SwitchMultiFile(bool enabled)
		{
			IsMultiFile = enabled;
			Reload();
		}

		public void AddOtherWindowToTimesync(LogWindow other)
		{
			if (other.IsTimeSynced)
			{
				if (this.IsTimeSynced)
				{
					other.FreeFromTimeSync();
					AddSlaveToTimesync(other);
				}
				else
				{
					AddToTimeSync(other);
				}
			}
			else
			{
				AddSlaveToTimesync(other);
			}
		}

		public void AddToTimeSync(LogWindow master)
		{
			Logger.logInfo("Syncing window for " + Util.GetNameFromPath(this.FileName) + " to " + Util.GetNameFromPath(master.FileName));
			lock (this._timeSyncListLock)
			{
				if (this.IsTimeSynced && master.TimeSyncList != this._timeSyncList)  // already synced but master has different sync list
				{
					FreeFromTimeSync();
				}
				this._timeSyncList = master.TimeSyncList;
				this._timeSyncList.AddWindow(this);
				this.ScrollToTimestamp(this._timeSyncList.CurrentTimestamp, false, false);
			}
			OnSyncModeChanged();
		}

		public void FreeFromTimeSync()
		{
			lock (this._timeSyncListLock)
			{
				if (this.TimeSyncList != null)
				{
					Logger.logInfo("De-Syncing window for " + Util.GetNameFromPath(this.FileName));
					this._timeSyncList.WindowRemoved -= TimeSyncList_WindowRemoved;
					this.TimeSyncList.RemoveWindow(this);
					this._timeSyncList = null;
				}
			}
			OnSyncModeChanged();
		}

		public IBookmarkData BookmarkData
		{
			get
			{
				return this._bookmarkProvider;
			}
		}

		#endregion

		#region Events

		private void LogWindow_Disposed(object sender, EventArgs e)
		{
			this._waitingForClose = true;
			this._parentLogTabWin.HighlightSettingsChanged -= Parent_HighlightSettingsChanged;
			if (this._logFileReader != null)
			{
				this._logFileReader.DeleteAllContent();
			}
			FreeFromTimeSync();
		}

		#region LogFileReader Events

		private void LogFileReader_LoadingStarted(object sender, LoadFileEventArgs e)
		{
			this.Invoke(new LoadingStartedFx(LoadingStarted), new object[] { e });
		}

		private void LogFileReader_FinishedLoading(object sender, EventArgs e)
		{
			Logger.logInfo("Finished loading.");
			this._isLoading = false;
			this._isDeadFile = false;
			if (!this._waitingForClose)
			{
				this.Invoke(new MethodInvoker(LoadingFinished));
				this.Invoke(new MethodInvoker(LoadPersistenceData));
				this.Invoke(new MethodInvoker(SetGuiAfterLoading));
				this._loadingFinishedEvent.Set();
				this._externaLoadingFinishedEvent.Set();
				this._timeSpreadCalc.SetLineCount(this._logFileReader.LineCount);
				if (this._loadingFinishedFx != null)
				{
					this._loadingFinishedFx(this);
				}

				if (this._reloadMemento != null)
				{
					this.Invoke(new PositionAfterReloadFx(this.PositionAfterReload), new object[] { this._reloadMemento });
				}
				if (this.filterTailCheckBox.Checked)
				{
					Logger.logInfo("Refreshing filter view because of reload.");
					this.Invoke(new MethodInvoker(FilterSearch)); // call on proper thread
				}

				HandleChangedFilterList();
			}
			this._reloadMemento = null;
		}

		private void LogFileReader_FileNotFound(object sender, EventArgs e)
		{
			if (!this.IsDisposed && !this.Disposing)
			{
				Logger.logInfo("Handling file not found event.");
				this._isDeadFile = true;
				this.BeginInvoke(new MethodInvoker(LogfileDead));
			}
		}

		private void LogFileReader_Respawned(object sender, EventArgs e)
		{
			this.BeginInvoke(new MethodInvoker(LogfileRespawned));
		}

		/**
		 * Event handler for the Load event from LogfileReader
		 */
		private void LogFileReader_LoadFile(object sender, LoadFileEventArgs e)
		{
			if (e.NewFile)
			{
				Logger.logInfo("File created anew.");

				// File was new created (e.g. rollover)
				this._isDeadFile = false;
				UnRegisterLogFileReaderEvents();
				this.dataGridView.CurrentCellChanged -= new EventHandler(DataGridView_CurrentCellChanged);
				MethodInvoker invoker = new MethodInvoker(ReloadNewFile);
				this.BeginInvoke(invoker);
				Logger.logDebug("Reloading invoked.");
				return;
			}

			if (!this._isLoading)
			{
				return;
			}
			UpdateProgressCallback callback = new UpdateProgressCallback(UpdateProgress);
			this.BeginInvoke(callback, new object[] { e });
		}

		#endregion

		private void UpdateProgress(LoadFileEventArgs e)
		{
			try
			{
				if (e.ReadPos >= e.FileSize)
				{
					//Logger.logWarn("UpdateProgress(): ReadPos (" + e.ReadPos + ") is greater than file size (" + e.FileSize + "). Aborting Update");
					return;
				}

				this._statusEventArgs.FileSize = e.ReadPos;
				this._progressEventArgs.MaxValue = (int)e.FileSize;
				this._progressEventArgs.Value = (int)e.ReadPos;
				SendProgressBarUpdate();
				SendStatusLineUpdate();
			}
			catch (Exception ex)
			{
				Logger.logError("UpdateProgress(): \n" + ex + "\n" + ex.StackTrace);
			}
		}

		private void LoadingStarted(LoadFileEventArgs e)
		{
			try
			{
				this._statusEventArgs.FileSize = e.ReadPos;
				this._statusEventArgs.StatusText = "Loading " + Util.GetNameFromPath(e.FileName);
				this._progressEventArgs.Visible = true;
				this._progressEventArgs.MaxValue = (int)e.FileSize;
				this._progressEventArgs.Value = (int)e.ReadPos;
				SendProgressBarUpdate();
				SendStatusLineUpdate();
			}
			catch (Exception ex)
			{
				Logger.logError("LoadingStarted(): " + ex + "\n" + ex.StackTrace);
			}
		}

		#region DataGridView Events

		private void DataGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
		{
			e.Handled = true;
			AutoResizeColumns(this.dataGridView);
		}

		private void DataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			e.Value = GetCellValue(e.RowIndex, e.ColumnIndex);
		}

		private void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			DataGridView gridView = (DataGridView)sender;
			PaintHelper.CellPainting(this, gridView, e.RowIndex, e);
		}

		private void DataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			if (!this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				return;
			}
			string line = this._logFileReader.GetLogLine(e.RowIndex);
			int offset = this.CurrentColumnizer.GetTimeOffset();
			this.CurrentColumnizer.SetTimeOffset(0);
			this.ColumnizerCallbackObject.LineNum = e.RowIndex;
			string[] cols = this.CurrentColumnizer.SplitLine(this.ColumnizerCallbackObject, line);
			this.CurrentColumnizer.SetTimeOffset(offset);
			if (cols.Length <= e.ColumnIndex - 2)
			{
				return;
			}

			string oldValue = cols[e.ColumnIndex - 2];
			string newValue = (string)e.Value;
			this.CurrentColumnizer.PushValue(this.ColumnizerCallbackObject, e.ColumnIndex - 2, newValue, oldValue);
			this.dataGridView.Refresh();
			TimeSpan timeSpan = new TimeSpan(this.CurrentColumnizer.GetTimeOffset() * TimeSpan.TicksPerMillisecond);
			string span = timeSpan.ToString();
			int index = span.LastIndexOf('.');
			if (index > 0)
			{
				span = span.Substring(0, index + 4);
			}
			SetTimeshiftValue(span);
			SendGuiStateUpdate();
		}

		private void DataGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
		{
			e.Height = GetRowHeight(e.RowIndex);
		}

		private void DataGridView_CurrentCellChanged(object sender, EventArgs e)
		{
			if (this.dataGridView.CurrentRow != null)
			{
				this._statusEventArgs.CurrentLineNum = this.dataGridView.CurrentRow.Index + 1;
				SendStatusLineUpdate();
				if (this.syncFilterCheckBox.Checked)
				{
					SyncFilterGridPos();
				}

				if (this.CurrentColumnizer.IsTimeshiftImplemented() && this.Preferences.timestampControl)
				{
					SyncTimestampDisplay();
				}
			}
		}

		private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			StatusLineText("");
		}

		private void DataGridView_Paint(object sender, PaintEventArgs e)
		{
			if (this.ShowBookmarkBubbles)
			{
				AddBookmarkOverlays();
			}
		}

		private void DataGridView_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
			{
				if (this.dataGridView.DisplayedRowCount(false) +
					this.dataGridView.FirstDisplayedScrollingRowIndex >=
					this.dataGridView.RowCount
				)
				{
					if (!this._guiStateArgs.FollowTail)
					{
						FollowTailChanged(true, false);
					}
					OnTailFollowed(new EventArgs());
				}
				else
				{
					if (this._guiStateArgs.FollowTail)
					{
						FollowTailChanged(false, false);
					}
				}
				SendGuiStateUpdate();
			}
		}

		private void DataGridView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.None)
			{
				this.filterGridView.Focus();
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.Control)
			{
				//this.parentLogTabWin.SwitchTab(e.Shift);
			}
			this._shouldCallTimeSync = true;
		}

		private void DataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if ((e.KeyCode == Keys.Tab) && e.Control)
			{
				e.IsInputKey = true;
			}
		}

		private void DataGridView_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (this.dataGridView.CurrentCell != null)
			{
				this.dataGridView.BeginEdit(false);
			}
		}

		private void DataGridView_InvalidateCurrentRow(object sender, EventArgs e)
		{
			InvalidateCurrentRow(this.dataGridView);
		}

		private void DataGridView_Resize(object sender, EventArgs e)
		{
			if (this._logFileReader != null && this.dataGridView.RowCount > 0 &&
				this._guiStateArgs.FollowTail)
			{
				this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.RowCount - 1;
			}
		}

		private void DataGridView_SelectionChanged(object sender, EventArgs e)
		{
			UpdateSelectionDisplay();
		}

		private void DataGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
		{
			if (e.RowIndex > 0 && e.RowIndex < this.dataGridView.RowCount &&
				!this.dataGridView.Rows[e.RowIndex].Selected)
			{
				SelectLine(e.RowIndex, false);
			}
			if (e.ContextMenuStrip == this.columnContextMenuStrip)
			{
				this._selectedCol = e.ColumnIndex;
			}
		}

		private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			this._shouldCallTimeSync = true;
		}

		private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 0)
			{
				ToggleBookmark();
			}
		}

		private void DataGridView_OverlayDoubleClicked(object sender, OverlayEventArgs e)
		{
			BookmarkComment(e.BookmarkOverlay.Bookmark);
		}

		#endregion

		#region FilterGridView Events

		private void FilterGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || this._filterResultList.Count <= e.RowIndex)
			{
				e.Handled = false;
				return;
			}

			int lineNum = this._filterResultList[e.RowIndex];
			string line = this._logFileReader.GetLogLineWithWait(lineNum);

			if (line != null)
			{
				DataGridView gridView = (DataGridView)sender;
				HilightEntry entry = FindFirstNoWordMatchHilightEntry(line);

				PaintHelper.CellPaintFilter(this, gridView, e, lineNum, line, entry);
			}
		}

		private void FilterGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || this._filterResultList.Count <= e.RowIndex)
			{
				e.Value = "";
				return;
			}

			int lineNum = this._filterResultList[e.RowIndex];
			e.Value = GetCellValue(lineNum, e.ColumnIndex);
		}

		private void FilterGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
		{
			e.Height = this._lineHeight;
		}

		private void FilterGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
		{
			e.Handled = true;
			AutoResizeColumnsFx fx = AutoResizeColumns;
			this.BeginInvoke(fx, new object[] { this.filterGridView });
		}

		private void FilterGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 0)
			{
				ToggleBookmark();
				return;
			}

			if (this.filterGridView.CurrentRow != null && e.RowIndex >= 0)
			{
				int lineNum = this._filterResultList[this.filterGridView.CurrentRow.Index];
				SelectAndEnsureVisible(lineNum, true);
			}
		}

		private void FilterGridView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (this.filterGridView.CurrentCellAddress.Y >= 0 && this.filterGridView.CurrentCellAddress.Y < this._filterResultList.Count)
				{
					int lineNum = this._filterResultList[this.filterGridView.CurrentCellAddress.Y];
					SelectLine(lineNum, false);
					e.Handled = true;
				}
			}
			if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.None)
			{
				this.dataGridView.Focus();
				e.Handled = true;
			}
		}

		private void FilterGridView_InvalidateCurrentRow(object sender, EventArgs e)
		{
			InvalidateCurrentRow(this.filterGridView);
		}

		private void FilterGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
		{
			if (e.ContextMenuStrip == this.columnContextMenuStrip)
			{
				this._selectedCol = e.ColumnIndex;
			}
		}

		#endregion

		#region EditControl Events

		private void EditControl_UpdateEditColumnDisplay(object sender, KeyEventArgs e)
		{
			UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
		}

		private void EditControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
		}

		private void EditControl_Click(object sender, EventArgs e)
		{
			UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
		}

		#endregion

		private void FilterSearchButton_Click(object sender, EventArgs e)
		{
			FilterSearch();
		}

		private void FilterComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				FilterSearch();
			}
		}

		private void RangeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.filterRangeComboBox.Enabled = this.rangeCheckBox.Checked;
			CheckForFilterDirty();
		}

		private void SyncFilterCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this.syncFilterCheckBox.Checked)
			{
				SyncFilterGridPos();
			}
		}

		private void SelectionChangedTrigger_Signal(object sender, EventArgs e)
		{
			Logger.logDebug("Selection changed trigger");
			int selCount = this.dataGridView.SelectedRows.Count;
			if (selCount > 1)
			{
				StatusLineText(selCount + " selected lines");
			}
			else
			{
				if (this.IsMultiFile)
				{
					MethodInvoker invoker = new MethodInvoker(DisplayCurrentFileOnStatusline);
					invoker.BeginInvoke(null, null);
				}
				else
				{
					StatusLineText("");
				}
			}
		}

		private void FilterKnobControl1_CheckForFilterDirty(object sender, EventArgs e)
		{
			CheckForFilterDirty();
		}

		private void FilterToTabButton_Click(object sender, EventArgs e)
		{
			FilterToTab();
		}

		private void Pipe_Disconnected(object sender, EventArgs e)
		{
			if (sender.GetType() == typeof(FilterPipe))
			{
				lock (this._filterPipeList)
				{
					this._filterPipeList.Remove((FilterPipe)sender);
					if (this._filterPipeList.Count == 0)
					{
						// reset naming counter to 0 if no more open filter tabs for this source window
						this._filterPipeNameCounter = 0;
					}
				}
			}
		}

		private void AdvancedButton_Click(object sender, EventArgs e)
		{
			this._showAdvanced = !this._showAdvanced;
			ShowAdvancedFilterPanel(this._showAdvanced);
		}

		private void SetTimestampLimits()
		{
			if (!this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				return;
			}

			int line = 0;
			this._guiStateArgs.MinTimestamp = GetTimestampForLineForward(ref line, true);
			line = this.dataGridView.RowCount - 1;
			this._guiStateArgs.MaxTimestamp = GetTimestampForLine(ref line, true);
			SendGuiStateUpdate();
		}

		private void DataGridContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			int lineNum = -1;
			if (this.dataGridView.CurrentRow != null)
			{
				lineNum = this.dataGridView.CurrentRow.Index;
			}
			if (lineNum == -1)
			{
				return;
			}
			int refLineNum = lineNum;

			this.copyToTabToolStripMenuItem.Enabled = this.dataGridView.SelectedCells.Count > 0;
			this.scrollAllTabsToTimestampToolStripMenuItem.Enabled = this.CurrentColumnizer.IsTimeshiftImplemented() &&
																	 GetTimestampForLine(ref refLineNum, false) != DateTime.MinValue;
			this.locateLineInOriginalFileToolStripMenuItem.Enabled = this.IsTempFile &&
																	 this.FilterPipe != null &&
																	 this.FilterPipe.GetOriginalLineNum(lineNum) != -1;
			this.markEditModeToolStripMenuItem.Enabled = !this.dataGridView.CurrentCell.ReadOnly;

			// Remove all "old" plugin entries
			int index = this.dataGridContextMenuStrip.Items.IndexOf(this.pluginSeparator);
			if (index > 0)
			{
				for (int i = index + 1; i < this.dataGridContextMenuStrip.Items.Count; )
				{
					this.dataGridContextMenuStrip.Items.RemoveAt(i);
				}
			}

			// Add plugin entries
			bool isAdded = false;
			if (PluginRegistry.GetInstance().RegisteredContextMenuPlugins.Count > 0)
			{
				IList<int> lines = GetSelectedContent();
				foreach (IContextMenuEntry entry in PluginRegistry.GetInstance().RegisteredContextMenuPlugins)
				{
					LogExpertCallback callback = new LogExpertCallback(this);
					ContextMenuPluginEventArgs evArgs = new ContextMenuPluginEventArgs(entry, lines, this.CurrentColumnizer, callback);
					EventHandler ev = new EventHandler(HandlePluginContextMenu);
					string menuText = entry.GetMenuText(lines, this.CurrentColumnizer, callback);
					if (menuText != null)
					{
						bool disabled = menuText.StartsWith("_");
						if (disabled)
						{
							menuText = menuText.Substring(1);
						}
						ToolStripItem item = this.dataGridContextMenuStrip.Items.Add(menuText, null, ev);
						item.Tag = evArgs;
						item.Enabled = !disabled;
						isAdded = true;
					}
				}
			}
			this.pluginSeparator.Visible = isAdded;

			// enable/disable Temp Highlight item
			this.tempHighlightsToolStripMenuItem.Enabled = this._tempHilightEntryList.Count > 0;

			this.markCurrentFilterRangeToolStripMenuItem.Enabled = this.filterRangeComboBox.Text != null && this.filterRangeComboBox.Text.Length > 0;

			if (this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				IList<WindowFileEntry> list = this._parentLogTabWin.GetListOfOpenFiles();
				this.syncTimestampsToToolStripMenuItem.Enabled = true;
				this.syncTimestampsToToolStripMenuItem.DropDownItems.Clear();
				EventHandler ev = new EventHandler(HandleSyncContextMenu);
				Font italicFont = new Font(syncTimestampsToToolStripMenuItem.Font.FontFamily, this.syncTimestampsToToolStripMenuItem.Font.Size, FontStyle.Italic);
				foreach (WindowFileEntry fileEntry in list)
				{
					if (fileEntry.LogWindow != this)
					{
						ToolStripMenuItem item = this.syncTimestampsToToolStripMenuItem.DropDownItems.Add(fileEntry.Title, null, ev) as ToolStripMenuItem;
						item.Tag = fileEntry;
						item.Checked = this._timeSyncList != null && this._timeSyncList.Contains(fileEntry.LogWindow);
						if (fileEntry.LogWindow.TimeSyncList != null && !fileEntry.LogWindow.TimeSyncList.Contains(this))
						{
							item.Font = italicFont;
							item.ForeColor = Color.Blue;
						}
						item.Enabled = fileEntry.LogWindow.CurrentColumnizer.IsTimeshiftImplemented();
					}
				}
			}
			else
			{
				this.syncTimestampsToToolStripMenuItem.Enabled = false;
			}
			this.freeThisWindowFromTimeSyncToolStripMenuItem.Enabled = this._timeSyncList != null && this._timeSyncList.Count > 1;
		}

		private void Copy_Click(object sender, EventArgs e)
		{
			CopyMarkedLinesToClipboard();
		}

		private void ScrollAllTabsToTimestampToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				int currentLine = this.dataGridView.CurrentCellAddress.Y;
				if (currentLine > 0 && currentLine < this.dataGridView.RowCount)
				{
					int lineNum = currentLine;
					DateTime timeStamp = GetTimestampForLine(ref lineNum, false);
					if (timeStamp.Equals(DateTime.MinValue))  // means: invalid
					{
						return;
					}
					this._parentLogTabWin.ScrollAllTabsToTimestamp(timeStamp, this);
				}
			}
		}

		private void LocateLineInOriginalFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView.CurrentRow != null && this.FilterPipe != null)
			{
				int lineNum = this.FilterPipe.GetOriginalLineNum(this.dataGridView.CurrentRow.Index);
				if (lineNum != -1)
				{
					this.FilterPipe.LogWindow.SelectLine(lineNum, false);
					this._parentLogTabWin.SelectTab(this.FilterPipe.LogWindow);
				}
			}
		}

		private void ToggleBoomarkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToggleBookmark();
		}

		private void MarkEditModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StartEditMode();
		}

		private void LogWindow_SizeChanged(object sender, EventArgs e)
		{
			AdjustHighlightSplitterWidth();
		}

		private void BookmarkWindow_BookmarkCommentChanged(object sender, EventArgs e)
		{
			this.dataGridView.Refresh();
		}

		private void ColumnRestrictCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.columnButton.Enabled = this.columnRestrictCheckBox.Checked;
			if (this.columnRestrictCheckBox.Checked) // disable when nothing to filter
			{
				this.columnNamesLabel.Visible = true;
				this._filterParams.columnRestrict = true;
				this.columnNamesLabel.Text = CalculateColumnNames(this._filterParams);
			}
			else
			{
				this.columnNamesLabel.Visible = false;
			}
			CheckForFilterDirty();
		}

		private void ColumnButton_Click(object sender, EventArgs e)
		{
			_filterParams.currentColumnizer = this._currentColumnizer;
			FilterColumnChooser chooser = new FilterColumnChooser(this._filterParams);
			if (chooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				columnNamesLabel.Text = CalculateColumnNames(this._filterParams);

				this.filterSearchButton.Image = this._searchButtonImage;
				this.saveFilterButton.Enabled = false;
			}
		}

		private void ColumnContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			Control ctl = this.columnContextMenuStrip.SourceControl;
			DataGridView gridView = ctl as DataGridView;
			bool frozen = false;
			if (_freezeStateMap.ContainsKey(ctl))
			{
				frozen = this._freezeStateMap[ctl];
			}
			this.freezeLeftColumnsUntilHereToolStripMenuItem.Checked = frozen;
			if (frozen)
			{
				this.freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Frozen";
			}
			else
			{
				if (ctl is DataGridView)
				{
					this.freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Freeze left columns until here (" +
																			gridView.Columns[this._selectedCol].HeaderText + ")";
				}
			}
			DataGridViewColumn col = gridView.Columns[this._selectedCol];
			this.moveLeftToolStripMenuItem.Enabled = (col != null && col.DisplayIndex > 0);
			this.moveRightToolStripMenuItem.Enabled = (col != null && col.DisplayIndex < gridView.Columns.Count - 1);

			if (gridView.Columns.Count - 1 > this._selectedCol)
			{
				DataGridViewColumn colRight = gridView.Columns.GetNextColumn(col, DataGridViewElementStates.None,
					DataGridViewElementStates.None);
				this.moveRightToolStripMenuItem.Enabled = (colRight != null && colRight.Frozen == col.Frozen);
			}
			if (this._selectedCol > 0)
			{
				DataGridViewColumn colLeft = gridView.Columns.GetPreviousColumn(col, DataGridViewElementStates.None,
					DataGridViewElementStates.None);

				this.moveLeftToolStripMenuItem.Enabled = (colLeft != null && colLeft.Frozen == col.Frozen);
			}
			DataGridViewColumn colLast = gridView.Columns[gridView.Columns.Count - 1];
			this.moveToLastColumnToolStripMenuItem.Enabled = (colLast != null && colLast.Frozen == col.Frozen);

			// Fill context menu with column names 
			//
			EventHandler ev = new EventHandler(HandleColumnItemContextMenu);
			allColumnsToolStripMenuItem.DropDownItems.Clear();
			foreach (DataGridViewColumn column in gridView.Columns)
			{
				if (column.HeaderText.Length > 0)
				{
					ToolStripMenuItem item = allColumnsToolStripMenuItem.DropDownItems.Add(column.HeaderText, null, ev) as ToolStripMenuItem;
					item.Tag = column;
					item.Enabled = !column.Frozen;
				}
			}
		}

		private void FreezeLeftColumnsUntilHereToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Control ctl = this.columnContextMenuStrip.SourceControl;
			bool frozen = false;
			if (_freezeStateMap.ContainsKey(ctl))
			{
				frozen = this._freezeStateMap[ctl];
			}
			frozen = !frozen;
			this._freezeStateMap[ctl] = frozen;

			if (ctl is DataGridView)
			{
				DataGridView gridView = ctl as DataGridView;
				ApplyFrozenState(gridView);
			}
		}

		private void MoveToLastColumnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[this._selectedCol];
			if (col != null)
			{
				col.DisplayIndex = gridView.Columns.Count - 1;
			}
		}

		private void MoveLeftToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[this._selectedCol];
			if (col != null && col.DisplayIndex > 0)
			{
				col.DisplayIndex = col.DisplayIndex - 1;
			}
		}

		private void MoveRightToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[this._selectedCol];
			if (col != null && col.DisplayIndex < gridView.Columns.Count - 1)
			{
				col.DisplayIndex = col.DisplayIndex + 1;
			}
		}

		private void HideColumnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[this._selectedCol];
			col.Visible = false;
		}

		private void RestoreColumnsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
			foreach (DataGridViewColumn col in gridView.Columns)
			{
				col.Visible = true;
			}
		}

		private void TimeSpreadingControl1_LineSelected(object sender, SelectLineEventArgs e)
		{
			SelectLine(e.Line, false);
		}

		private void SplitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			this.advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;
		}

		private void MarkFilterHitsInLogViewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SearchParams p = new SearchParams();
			p.searchText = this._filterParams.searchText;
			p.isRegex = this._filterParams.isRegex;
			p.isCaseSensitive = this._filterParams.isCaseSensitive;
			AddSearchHitHighlightEntry(p);
		}

		private void StatusLineTrigger_Signal(object sender, EventArgs e)
		{
			OnStatusLine(this._statusEventArgs);
		}

		private void ColumnComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			SelectColumn();
		}

		private void ColumnComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				SelectColumn();
				this.dataGridView.Focus();
			}
		}

		private void ColumnComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
			{
				this.columnComboBox.DroppedDown = true;
			}
			if (e.KeyCode == Keys.Enter)
			{
				e.IsInputKey = true;
			}
		}

		private void BookmarkProvider_BookmarkRemoved(object sender, EventArgs e)
		{
			if (!this._isLoading)
			{
				this.dataGridView.Refresh();
				this.filterGridView.Refresh();
			}
		}

		private void BookmarkProvider_BookmarkAdded(object sender, EventArgs e)
		{
			if (!this._isLoading)
			{
				this.dataGridView.Refresh();
				this.filterGridView.Refresh();
			}
		}

		private void BookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddBookmarkAndEditComment();
		}

		private void HighlightSelectionInLogFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl =
					this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
				HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow,
					false, true, false, false, false, false, null, false);
				lock (this._tempHilightEntryListLock)
				{
					this._tempHilightEntryList.Add(he);
				}
				this.dataGridView.CancelEdit();
				this.dataGridView.EndEdit();
				RefreshAllGrids();
			}
		}

		private void CopyToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl =
					this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
				if (!string.IsNullOrEmpty(ctl.SelectedText))
				{
					Clipboard.SetText(ctl.SelectedText);
				}
			}
		}

		private void RemoveAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RemoveTempHighlights();
		}

		private void MakePermanentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			lock (this._tempHilightEntryListLock)
			{
				lock (this._currentHighlightGroupLock)
				{
					this._currentHighlightGroup.HilightEntryList.AddRange(this._tempHilightEntryList);
					RemoveTempHighlights();
					OnCurrentHighlightListChanged();
				}
			}
		}

		private void MarkCurrentFilterRangeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MarkCurrentFilterRange();
		}

		private void FilterForSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl =
					this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
				this.splitContainer1.Panel2Collapsed = false;
				ResetFilterControls();
				FilterSearch(ctl.SelectedText);
			}
		}

		private void SetSelectedTextAsBookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl =
					this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
				AddBookmarkComment(ctl.SelectedText);
			}
		}

		private void FilterRegexCheckBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				RegexHelperDialog dlg = new RegexHelperDialog();
				dlg.Owner = this;
				dlg.CaseSensitive = this.filterCaseSensitiveCheckBox.Checked;
				dlg.Pattern = this.filterComboBox.Text;
				DialogResult res = dlg.ShowDialog();
				if (res == DialogResult.OK)
				{
					this.filterCaseSensitiveCheckBox.Checked = dlg.CaseSensitive;
					this.filterComboBox.Text = dlg.Pattern;
				}
			}
		}

		// ================= Filter-Highlight stuff ===============================

		/// <summary>
		/// Event handler for the HighlightEvent generated by the HighlightThread
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HighlightThread_HighlightDoneEvent(object sender, HighlightEventArgs e)
		{
			this.BeginInvoke(new HighlightEventFx(HighlightDoneEventWorker), new object[] { e });
		}

		/// <summary>
		/// Highlights the done event worker.
		/// </summary>
		/// <param name="e">The <see cref="LogExpert.HighlightEventArgs"/> instance containing the event data.</param>
		private void HighlightDoneEventWorker(HighlightEventArgs e)
		{
			if (this.dataGridView.FirstDisplayedScrollingRowIndex > e.StartLine &&
				this.dataGridView.FirstDisplayedScrollingRowIndex < e.StartLine + e.Count ||
				this.dataGridView.FirstDisplayedScrollingRowIndex + this.dataGridView.DisplayedRowCount(true) >
				e.StartLine &&
				this.dataGridView.FirstDisplayedScrollingRowIndex + this.dataGridView.DisplayedRowCount(true) < e.StartLine + e.Count)
			{
				this.BeginInvoke(new MethodInvoker(this.RefreshAllGrids));
			}
		}

		private void ToggleHighlightPanelButton_Click(object sender, EventArgs e)
		{
			ToggleHighlightPanel(this.highlightSplitContainer.Panel2Collapsed);
		}

		private void SaveFilterButton_Click(object sender, EventArgs e)
		{
			FilterParams newParams = this._filterParams.CreateCopy();
			newParams.color = Color.FromKnownColor(KnownColor.Black);
			ConfigManager.Settings.filterList.Add(newParams);
			OnFilterListChanged(this);
		}

		private void DeleteFilterButton_Click(object sender, EventArgs e)
		{
			int index = this.filterListBox.SelectedIndex;
			if (index >= 0)
			{
				FilterParams filterParams = (FilterParams)this.filterListBox.Items[index];
				ConfigManager.Settings.filterList.Remove(filterParams);
				OnFilterListChanged(this);
				if (this.filterListBox.Items.Count > 0)
				{
					this.filterListBox.SelectedIndex = this.filterListBox.Items.Count - 1;
				}
			}
		}

		private void FilterUpButton_Click(object sender, EventArgs e)
		{
			int i = this.filterListBox.SelectedIndex;
			if (i > 0)
			{
				FilterParams filterParams = (FilterParams)this.filterListBox.Items[i];
				ConfigManager.Settings.filterList.RemoveAt(i);
				i--;
				ConfigManager.Settings.filterList.Insert(i, filterParams);
				OnFilterListChanged(this);
				this.filterListBox.SelectedIndex = i;
			}
		}

		private void FilterDownButton_Click(object sender, EventArgs e)
		{
			int i = this.filterListBox.SelectedIndex;
			if (i < this.filterListBox.Items.Count - 1)
			{
				FilterParams filterParams = (FilterParams)this.filterListBox.Items[i];
				ConfigManager.Settings.filterList.RemoveAt(i);
				i++;
				ConfigManager.Settings.filterList.Insert(i, filterParams);
				OnFilterListChanged(this);
				this.filterListBox.SelectedIndex = i;
			}
		}

		private void FilterListBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.filterListBox.SelectedIndex >= 0)
			{
				FilterParams filterParams = (FilterParams)this.filterListBox.Items[this.filterListBox.SelectedIndex];
				FilterParams newParams = filterParams.CreateCopy();
				//newParams.historyList = ConfigManager.Settings.filterHistoryList;
				this._filterParams = newParams;
				ReInitFilterParams(this._filterParams);
				ApplyFilterParams();
				CheckForAdvancedButtonDirty();
				CheckForFilterDirty();
				this.filterSearchButton.Image = this._searchButtonImage;
				this.saveFilterButton.Enabled = false;
				if (this.hideFilterListOnLoadCheckBox.Checked)
				{
					ToggleHighlightPanel(false);
				}
				if (this.filterOnLoadCheckBox.Checked)
				{
					FilterSearch();
				}
			}
		}

		private void FilterListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index >= 0)
			{
				FilterParams filterParams = (FilterParams)this.filterListBox.Items[e.Index];
				Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

				Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? new SolidBrush(this.filterListBox.BackColor) : new SolidBrush(filterParams.color);

				e.Graphics.DrawString(filterParams.searchText, e.Font, brush,
					new PointF(rectangle.Left, rectangle.Top));
				e.DrawFocusRectangle();
				brush.Dispose();
			}
		}

		// Color for filter list entry
		private void ColorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int i = this.filterListBox.SelectedIndex;
			if (i < this.filterListBox.Items.Count && i >= 0)
			{
				FilterParams filterParams = (FilterParams)this.filterListBox.Items[i];
				ColorDialog dlg = new ColorDialog();
				dlg.CustomColors = new int[] { filterParams.color.ToArgb() };
				dlg.Color = filterParams.color;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					filterParams.color = dlg.Color;
					filterListBox.Refresh();
				}
			}
		}

		private void FilterChanges_CheckForDirty(object sender, EventArgs e)
		{
			CheckForFilterDirty();
		}

		private void FilterRegexCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.fuzzyKnobControl.Enabled = !this.filterRegexCheckBox.Checked;
			this.fuzzyLabel.Enabled = !this.filterRegexCheckBox.Checked;
			CheckForFilterDirty();
		}

		private void SetBookmarksOnSelectedLinesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetBoomarksForSelectedFilterLines();
		}

		private void Parent_HighlightSettingsChanged(object sender, EventArgs e)
		{
			string groupName = this._guiStateArgs.HighlightGroupName;
			SetCurrentHighlightGroup(groupName);
		}

		private void FilterOnLoadCheckBox_MouseClick(object sender, MouseEventArgs e)
		{
			HandleChangedFilterOnLoadSetting();
		}

		private void FilterOnLoadCheckBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleChangedFilterOnLoadSetting();
		}

		private void HideFilterListOnLoadCheckBox_MouseClick(object sender, MouseEventArgs e)
		{
			HandleChangedFilterOnLoadSetting();
		}

		private void FilterToTabToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FilterToTab();
		}

		private void TimeSyncList_WindowRemoved(object sender, EventArgs e)
		{
			TimeSyncList syncList = sender as TimeSyncList;
			lock (this._timeSyncListLock)
			{
				if (syncList.Count == 0 || syncList.Count == 1 && syncList.Contains(this))
				{
					if (syncList == this._timeSyncList)
					{
						this._timeSyncList = null;
						OnSyncModeChanged();
					}
				}
			}
		}

		private void FreeThisWindowFromTimeSyncToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FreeFromTimeSync();
		}

		private void LogWindow_InvalidateCurrentRow(object sender, EventArgs e)
		{
			InvalidateCurrentRow();
		}

		private void HandlePluginContextMenu(object sender, EventArgs args)
		{
			if (sender is ToolStripItem)
			{
				ContextMenuPluginEventArgs menuArgs = (sender as ToolStripItem).Tag as ContextMenuPluginEventArgs;
				menuArgs.Entry.MenuSelected(menuArgs.LogLines, menuArgs.Columnizer, menuArgs.Callback);
			}
		}

		private void HandleSyncContextMenu(object sender, EventArgs args)
		{
			if (sender is ToolStripItem)
			{
				WindowFileEntry entry = (sender as ToolStripItem).Tag as WindowFileEntry;

				if (this._timeSyncList != null && this._timeSyncList.Contains(entry.LogWindow))
				{
					FreeSlaveFromTimesync(entry.LogWindow);
				}
				else
				{
					AddOtherWindowToTimesync(entry.LogWindow);
				}
			}
		}

		private void HandleColumnItemContextMenu(object sender, EventArgs args)
		{
			if (sender is ToolStripItem)
			{
				DataGridViewColumn column = ((sender as ToolStripItem).Tag as DataGridViewColumn);
				column.Visible = true;
				column.DataGridView.FirstDisplayedScrollingColumnIndex = column.Index;
			}
		}

		#endregion

		#region Private Methods

		private void RegisterLogFileReaderEvents()
		{
			this._logFileReader.LoadFile += LogFileReader_LoadFile;
			this._logFileReader.LoadingFinished += LogFileReader_FinishedLoading;
			this._logFileReader.LoadingStarted += LogFileReader_LoadingStarted;
			this._logFileReader.FileNotFound += LogFileReader_FileNotFound;
			this._logFileReader.Respawned += LogFileReader_Respawned;
			// FileSizeChanged is not registered here because it's registered after loading has finished
		}

		private void UnRegisterLogFileReaderEvents()
		{
			if (this._logFileReader != null)
			{
				this._logFileReader.LoadFile -= LogFileReader_LoadFile;
				this._logFileReader.LoadingFinished -= LogFileReader_FinishedLoading;
				this._logFileReader.LoadingStarted -= LogFileReader_LoadingStarted;
				this._logFileReader.FileNotFound -= LogFileReader_FileNotFound;
				this._logFileReader.Respawned -= LogFileReader_Respawned;
				this._logFileReader.FileSizeChanged -= this.FileSizeChangedHandler;
			}
		}

		private bool LoadPersistenceOptions()
		{
			if (this.InvokeRequired)
			{
				return (bool)this.Invoke(new BoolReturnDelegate(LoadPersistenceOptions));
			}

			if (!this.Preferences.saveSessions && this.ForcedPersistenceFileName == null)
			{
				return false;
			}

			try
			{
				PersistenceData persistenceData;
				if (this.ForcedPersistenceFileName == null)
				{
					persistenceData = Persister.LoadPersistenceDataOptionsOnly(this.FileName, this.Preferences);
				}
				else
				{
					persistenceData = Persister.LoadPersistenceDataOptionsOnlyFromFixedFile(this.ForcedPersistenceFileName);
				}

				if (persistenceData == null)
				{
					Logger.logInfo("No persistence data for " + this.FileName + " found.");
					return false;
				}

				this.IsMultiFile = persistenceData.multiFile;
				this._multifileOptions = new MultifileOptions();
				this._multifileOptions.FormatPattern = persistenceData.multiFilePattern;
				this._multifileOptions.MaxDayTry = persistenceData.multiFileMaxDays;
				if (this._multifileOptions.FormatPattern == null || this._multifileOptions.FormatPattern.Length == 0)
				{
					this._multifileOptions = ObjectClone.Clone<MultifileOptions>(this.Preferences.multifileOptions);
				}

				this.splitContainer1.SplitterDistance = persistenceData.filterPosition;
				this.splitContainer1.Panel2Collapsed = !persistenceData.filterVisible;
				ToggleHighlightPanel(persistenceData.filterSaveListVisible);
				ShowAdvancedFilterPanel(persistenceData.filterAdvanced);
				if (_reloadMemento == null)
				{
					PreselectColumnizer(persistenceData.columnizerName);
				}
				this.FollowTailChanged(persistenceData.followTail, false);
				if (persistenceData.tabName != null)
				{
					this.Text = persistenceData.tabName;
				}
				AdjustHighlightSplitterWidth();
				SetCurrentHighlightGroup(persistenceData.highlightGroupName);
				if (persistenceData.multiFileNames.Count > 0)
				{
					Logger.logInfo("Detected MultiFile name list in persistence options");
					this._fileNames = new string[persistenceData.multiFileNames.Count];
					persistenceData.multiFileNames.CopyTo(this._fileNames);
				}
				else
				{
					this._fileNames = null;
				}
				SetExplicitEncoding(persistenceData.encoding);
				return true;
			}
			catch (Exception ex)
			{
				Logger.logError("Error loading persistence data: " + ex.Message);
				return false;
			}
		}

		private void SetDefaultsFromPrefs()
		{
			this.filterTailCheckBox.Checked = this.Preferences.filterTail;
			this.syncFilterCheckBox.Checked = this.Preferences.filterSync;
			this.FollowTailChanged(this.Preferences.followTail, false);
			this._multifileOptions = ObjectClone.Clone<MultifileOptions>(this.Preferences.multifileOptions);
		}

		private void LoadPersistenceData()
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new MethodInvoker(LoadPersistenceData));
				return;
			}

			if (!this.Preferences.saveSessions && !ForcePersistenceLoading && this.ForcedPersistenceFileName == null)
			{
				SetDefaultsFromPrefs();
				return;
			}

			if (this._isTempFile)
			{
				SetDefaultsFromPrefs();
				return;
			}

			ForcePersistenceLoading = false;  // force only 1 time (while session load)

			try
			{
				PersistenceData persistenceData;
				if (this.ForcedPersistenceFileName == null)
				{
					persistenceData = Persister.LoadPersistenceData(this.FileName, this.Preferences);
				}
				else
				{
					persistenceData = Persister.LoadPersistenceDataFromFixedFile(this.ForcedPersistenceFileName);
				}

				if (persistenceData.lineCount > this._logFileReader.LineCount)
				{
					// outdated persistence data (logfile rollover)
					// MessageBox.Show(this, "Persistence data for " + this.FileName + " is outdated. It was discarded.", "Log Expert");
					Logger.logInfo("Persistence data for " + this.FileName + " is outdated. It was discarded.");
					LoadPersistenceOptions();
					return;
				}
				this._bookmarkProvider.BookmarkList = persistenceData.bookmarkList;
				this._rowHeightList = persistenceData.rowHeightList;
				try
				{
					if (persistenceData.currentLine >= 0 && persistenceData.currentLine < this.dataGridView.RowCount)
					{
						SelectLine(persistenceData.currentLine, false);
					}
					else
					{
						if (this._logFileReader.LineCount > 0)
						{
							this.dataGridView.FirstDisplayedScrollingRowIndex = this._logFileReader.LineCount - 1;
							SelectLine(this._logFileReader.LineCount - 1, false);
						}
					}
					if (persistenceData.firstDisplayedLine >= 0 && persistenceData.firstDisplayedLine < this.dataGridView.RowCount)
					{
						this.dataGridView.FirstDisplayedScrollingRowIndex = persistenceData.firstDisplayedLine;
					}
					if (persistenceData.followTail)
					{
						this.FollowTailChanged(persistenceData.followTail, false);
					}
				}
				catch (ArgumentOutOfRangeException)
				{
					// FirstDisplayedScrollingRowIndex errechnet manchmal falsche Scroll-Ranges???
				}

				if (this.Preferences.saveFilters)
				{
					RestoreFilters(persistenceData);
				}
			}
			catch (IOException ex)
			{
				SetDefaultsFromPrefs();
				Logger.logError("Error loading bookmarks: " + ex.Message);
			}
		}

		private void RestoreFilters(PersistenceData persistenceData)
		{
			if (persistenceData.filterParamsList.Count > 0)
			{
				this._filterParams = persistenceData.filterParamsList[0];
				ReInitFilterParams(this._filterParams);
			}
			ApplyFilterParams();  // re-loaded filter settingss
			this.BeginInvoke(new MethodInvoker(FilterSearch));
			try
			{
				this.splitContainer1.SplitterDistance = persistenceData.filterPosition;
				this.splitContainer1.Panel2Collapsed = !persistenceData.filterVisible;
			}
			catch (InvalidOperationException e)
			{
				Logger.logError("Error setting splitter distance: " + e.Message);
			}
			ShowAdvancedFilterPanel(persistenceData.filterAdvanced);
			if (this._filterPipeList.Count == 0)     // don't restore if it's only a reload
			{
				RestoreFilterTabs(persistenceData);
			}
		}

		private void RestoreFilterTabs(PersistenceData persistenceData)
		{
			foreach (FilterTabData data in persistenceData.filterTabDataList)
			{
				FilterParams persistFilterParams = data.filterParams;
				ReInitFilterParams(persistFilterParams);
				List<int> filterResultList = new List<int>();
				List<int> filterHitList = new List<int>();
				Filter(persistFilterParams, filterResultList, _lastFilterLinesList, filterHitList);
				FilterPipe pipe = new FilterPipe(persistFilterParams.CreateCopy(), this);
				WritePipeToTab(pipe, filterResultList, data.persistenceData.tabName, data.persistenceData);
			}
		}

		private void ReInitFilterParams(FilterParams filterParams)
		{
			filterParams.searchText = filterParams.searchText;   // init "lowerSearchText"
			filterParams.rangeSearchText = filterParams.rangeSearchText;   // init "lowerRangesearchText"
			filterParams.currentColumnizer = this.CurrentColumnizer;
			if (filterParams.isRegex)
			{
				try
				{
					filterParams.CreateRegex();
				}
				catch (ArgumentException)
				{
					StatusLineError("Invalid regular expression");
					return;
				}
			}
		}

		private void EnterLoadFileStatus()
		{
			Logger.logDebug("EnterLoadFileStatus begin");

			if (this.InvokeRequired)
			{
				this.Invoke(new MethodInvoker(EnterLoadFileStatus));
				return;
			}
			this._statusEventArgs.StatusText = "Loading file...";
			this._statusEventArgs.LineCount = 0;
			this._statusEventArgs.FileSize = 0;
			SendStatusLineUpdate();

			this._progressEventArgs.MinValue = 0;
			this._progressEventArgs.MaxValue = 0;
			this._progressEventArgs.Value = 0;
			this._progressEventArgs.Visible = true;
			SendProgressBarUpdate();

			this._isLoading = true;
			this._shouldCancel = true;
			ClearFilterList();
			ClearBookmarkList();
			this.dataGridView.ClearSelection();
			this.dataGridView.RowCount = 0;
			Logger.logDebug("EnterLoadFileStatus end");
		}

		private void PositionAfterReload(ReloadMemento reloadMemento)
		{
			if (this._reloadMemento.currentLine < this.dataGridView.RowCount && this._reloadMemento.currentLine >= 0)
			{
				this.dataGridView.CurrentCell = this.dataGridView.Rows[this._reloadMemento.currentLine].Cells[0];
			}
			if (this._reloadMemento.firstDisplayedLine < this.dataGridView.RowCount && this._reloadMemento.firstDisplayedLine >= 0)
			{
				this.dataGridView.FirstDisplayedScrollingRowIndex = this._reloadMemento.firstDisplayedLine;
			}
		}

		private void LogfileDead()
		{
			Logger.logInfo("File not found.");
			this._isDeadFile = true;

			this.dataGridView.Enabled = false;
			this.dataGridView.RowCount = 0;
			this._progressEventArgs.Visible = false;
			this._progressEventArgs.Value = this._progressEventArgs.MaxValue;
			SendProgressBarUpdate();
			this._statusEventArgs.FileSize = 0;
			this._statusEventArgs.LineCount = 0;
			this._statusEventArgs.CurrentLineNum = 0;
			SendStatusLineUpdate();
			this._shouldCancel = true;
			ClearFilterList();
			ClearBookmarkList();

			StatusLineText("File not found");
			OnFileNotFound(new EventArgs());
		}

		private void LogfileRespawned()
		{
			Logger.logInfo("LogfileDead(): Reloading file because it has been respawned.");
			this._isDeadFile = false;
			this.dataGridView.Enabled = true;
			StatusLineText("");
			OnFileRespawned(new EventArgs());
			Reload();
		}

		private void SetGuiAfterLoading()
		{
			if (this.Text.Length == 0)
			{
				if (this._isTempFile)
				{
					this.Text = TempTitleName;
				}
				else
				{
					this.Text = Util.GetNameFromPath(this.FileName);
				}
			}
			this.ShowBookmarkBubbles = this.Preferences.showBubbles;

			ILogLineColumnizer columnizer;
			if (this._forcedColumnizerForLoading != null)
			{
				columnizer = this._forcedColumnizerForLoading;
				this._forcedColumnizerForLoading = null;
			}
			else
			{
				columnizer = FindColumnizer();
				if (columnizer != null)
				{
					if (this._reloadMemento == null)
					{
						columnizer = Util.CloneColumnizer(columnizer);
					}
				}
				else
				{
					// Default Columnizers
					columnizer = Util.CloneColumnizer(PluginRegistry.GetInstance().RegisteredColumnizers[0]);
				}
			}

			this.Invoke(new SetColumnizerFx(this.SetColumnizer), new object[] { columnizer });
			this.dataGridView.Enabled = true;
			DisplayCurrentFileOnStatusline();
			this._guiStateArgs.MultiFileEnabled = !this.IsTempFile;
			this._guiStateArgs.MenuEnabled = true;
			this._guiStateArgs.CurrentEncoding = this._logFileReader.CurrentEncoding;
			SendGuiStateUpdate();

			if (this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				if (this.Preferences.timestampControl)
				{
					SetTimestampLimits();
					SyncTimestampDisplay();
				}
				Settings settings = ConfigManager.Settings;
				ShowLineColumn(!settings.hideLineColumn);
			}
			ShowTimeSpread(this.Preferences.showTimeSpread && this.CurrentColumnizer.IsTimeshiftImplemented());
			this.locateLineInOriginalFileToolStripMenuItem.Enabled = this.FilterPipe != null;
		}

		private ILogLineColumnizer FindColumnizer()
		{
			ILogLineColumnizer columnizer = null;
			if (this.Preferences.maskPrio)
			{
				columnizer = this._parentLogTabWin.FindColumnizerByFileMask(Util.GetNameFromPath(this.FileName));
				if (columnizer == null)
				{
					columnizer = this._parentLogTabWin.GetColumnizerHistoryEntry(this.FileName);
				}
			}
			else
			{
				columnizer = this._parentLogTabWin.GetColumnizerHistoryEntry(this.FileName);
				if (columnizer == null)
				{
					columnizer = this._parentLogTabWin.FindColumnizerByFileMask(Util.GetNameFromPath(this.FileName));
				}
			}
			return columnizer;
		}

		private void LogWindow_Closing(object sender, CancelEventArgs e)
		{
			if (this.Preferences.askForClose)
			{
				if (MessageBox.Show("Sure to close?", "LogExpert", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
					DialogResult.No)
				{
					e.Cancel = true;
					return;
				}
			}
			SavePersistenceData(false);
			CloseLogWindow();
		}

		private void ReloadNewFile()
		{
			// prevent "overloads". May occur on very fast rollovers (next rollover before the file is reloaded)
			lock (this._reloadLock)
			{
				this._reloadOverloadCounter++;
				Logger.logInfo("ReloadNewFile(): counter = " + this._reloadOverloadCounter);
				if (this._reloadOverloadCounter <= 1)
				{
					SavePersistenceData(false);
					_loadingFinishedEvent.Reset();
					_externaLoadingFinishedEvent.Reset();
					Thread reloadFinishedThread = new Thread(new ThreadStart(ReloadFinishedThreadFx));
					reloadFinishedThread.IsBackground = true;
					reloadFinishedThread.Start();
					LoadFile(this.FileName, this.EncodingOptions);

					ClearBookmarkList();
					SavePersistenceData(false);
				}
				else
				{
					Logger.logDebug("Preventing reload because of recursive calls.");
				}
				this._reloadOverloadCounter--;
			}
		}

		private void ReloadFinishedThreadFx()
		{
			Logger.logInfo("Waiting for loading to be complete.");
			this._loadingFinishedEvent.WaitOne();
			Logger.logInfo("Refreshing filter view because of reload.");
			this.Invoke(new MethodInvoker(FilterSearch));
			LoadFilterPipes();
			OnFileReloadFinished();
		}

		private void LoadingFinished()
		{
			Logger.logInfo("File loading complete.");
			StatusLineText("");
			this._logFileReader.FileSizeChanged += this.FileSizeChangedHandler;
			this._isLoading = false;
			this._shouldCancel = false;
			this.dataGridView.SuspendLayout();
			this.dataGridView.RowCount = this._logFileReader.LineCount;
			this.dataGridView.CurrentCellChanged += new EventHandler(DataGridView_CurrentCellChanged);
			this.dataGridView.Enabled = true;
			this.dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
			this.dataGridView.ResumeLayout();
			this._progressEventArgs.Visible = false;
			this._progressEventArgs.Value = this._progressEventArgs.MaxValue;
			SendProgressBarUpdate();

			this._guiStateArgs.FollowTail = true;
			SendGuiStateUpdate();
			this._statusEventArgs.LineCount = this._logFileReader.LineCount;
			this._statusEventArgs.FileSize = this._logFileReader.FileSize;
			SendStatusLineUpdate();

			PreferencesChanged(this._parentLogTabWin.Preferences, true, SettingsFlags.All);
		}

		private void FileSizeChangedHandler(object sender, LogEventArgs e)
		{
			Logger.logInfo("Got FileSizeChanged event. prevLines:" + e.PrevLineCount + ", curr lines: " + e.LineCount);

			lock (this._logEventArgsList)
			{
				this._logEventArgsList.Add(e);
				this._logEventArgsEvent.Set();
			}
		}

		private void LogEventWorker()
		{
			Thread.CurrentThread.Name = "LogEventWorker";
			while (true)
			{
				Logger.logDebug("Waiting for signal");
				this._logEventArgsEvent.WaitOne();
				Logger.logDebug("Wakeup signal received.");
				while (true)
				{
					LogEventArgs e;
					int lastLineCount = 0;
					lock (this._logEventArgsList)
					{
						Logger.logInfo("" + this._logEventArgsList.Count + " events in queue");
						if (this._logEventArgsList.Count == 0)
						{
							this._logEventArgsEvent.Reset();
							break;
						}
						e = this._logEventArgsList[0];
						this._logEventArgsList.RemoveAt(0);
					}
					if (e.IsRollover)
					{
						ShiftBookmarks(e.RolloverOffset);
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
					UpdateGridCallback callback = new UpdateGridCallback(UpdateGrid);
					this.Invoke(callback, new object[] { e });
					CheckFilterAndHighlight(e);
					this._timeSpreadCalc.SetLineCount(e.LineCount);
				}
			}
		}

		private void StopLogEventWorkerThread()
		{
			this._logEventArgsEvent.Set();
			this._logEventHandlerThread.Abort();
			this._logEventHandlerThread.Join();
		}

		private void UpdateGrid(LogEventArgs e)
		{
			int oldRowCount = this.dataGridView.RowCount;
			int firstDisplayedLine = this.dataGridView.FirstDisplayedScrollingRowIndex;

			try
			{
				if (this.dataGridView.RowCount > e.LineCount)
				{
					int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
					this.dataGridView.RowCount = 0;
					this.dataGridView.RowCount = e.LineCount;
					if (!this._guiStateArgs.FollowTail)
					{
						if (currentLineNum >= this.dataGridView.RowCount)
						{
							currentLineNum = this.dataGridView.RowCount - 1;
						}
						this.dataGridView.CurrentCell = this.dataGridView.Rows[currentLineNum].Cells[0];
					}
				}
				else
				{
					this.dataGridView.RowCount = e.LineCount;
				}
				Logger.logDebug("UpdateGrid(): new RowCount=" + this.dataGridView.RowCount);
				if (e.IsRollover)
				{
					// Multifile rollover
					// keep selection and view range, if no follow tail mode
					if (!this._guiStateArgs.FollowTail)
					{
						int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
						currentLineNum -= e.RolloverOffset;
						if (currentLineNum < 0)
						{
							currentLineNum = 0;
						}
						Logger.logDebug("UpdateGrid(): Rollover=true, Rollover offset=" + e.RolloverOffset + ", currLineNum was " + this.dataGridView.CurrentCellAddress.Y + ", new currLineNum=" + currentLineNum);
						firstDisplayedLine -= e.RolloverOffset;
						if (firstDisplayedLine < 0)
						{
							firstDisplayedLine = 0;
						}
						this.dataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedLine;
						this.dataGridView.CurrentCell = this.dataGridView.Rows[currentLineNum].Cells[0];
						this.dataGridView.Rows[currentLineNum].Selected = true;
					}
				}
				this._statusEventArgs.LineCount = e.LineCount;
				StatusLineFileSize(e.FileSize);

				if (!this._isLoading)
				{
					if (oldRowCount == 0)
					{
						AdjustMinimumGridWith();
					}
				}
				if (this._guiStateArgs.FollowTail && this.dataGridView.RowCount > 0)
				{
					this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.RowCount - 1;
					OnTailFollowed(new EventArgs());
				}
				if (this.Preferences.timestampControl && !this._isLoading)
				{
					SetTimestampLimits();
				}
			}
			catch (Exception ex)
			{
				Logger.logError("Fehler bei UpdateGrid(): " + ex + "\n" + ex.StackTrace);
			}
		}

		private void CheckFilterAndHighlight(LogEventArgs e)
		{
			bool noLed = true;
			bool suppressLed = false;
			bool setBookmark = false;
			bool stopTail = false;
			string bookmarkComment = null;
			if (this.filterTailCheckBox.Checked || this._filterPipeList.Count > 0)
			{
				int filterStart = e.PrevLineCount;
				if (e.IsRollover)
				{
					ShiftFilterLines(e.RolloverOffset);
					filterStart -= e.RolloverOffset;
				}
				bool firstStopTail = true;
				ColumnizerCallback callback = new ColumnizerCallback(this);
				bool filterLineAdded = false;
				for (int i = filterStart; i < e.LineCount; ++i)
				{
					string line = this._logFileReader.GetLogLine(i);
					if (line == null)
					{
						return;
					}
					if (this.filterTailCheckBox.Checked)
					{
						callback.LineNum = i;
						if (Classes.DamerauLevenshtein.TestFilterCondition(this._filterParams, line, callback))
						{
							filterLineAdded = true;
							AddFilterLine(i, false, this._filterParams, this._filterResultList, this._lastFilterLinesList, this._filterHitList);
						}
					}
					ProcessFilterPipes(i);

					IList<HilightEntry> matchingList = FindMatchingHilightEntries(line);
					LaunchHighlightPlugins(matchingList, i);
					GetHilightActions(matchingList, out suppressLed, out stopTail, out setBookmark, out bookmarkComment);
					if (setBookmark)
					{
						SetBookmarkFx fx = new SetBookmarkFx(this.SetBookmarkFromTrigger);
						fx.BeginInvoke(i, bookmarkComment, null, null);
					}
					if (stopTail && this._guiStateArgs.FollowTail)
					{
						bool wasFollow = this._guiStateArgs.FollowTail;
						FollowTailChanged(false, true);
						if (firstStopTail && wasFollow)
						{
							this.Invoke(new SelectLineFx(this.SelectAndEnsureVisible), new object[] { i, false });
							firstStopTail = false;
						}
					}
					if (!suppressLed)
					{
						noLed = false;
					}
				}
				if (filterLineAdded)
				{
					TriggerFilterLineGuiUpdate();
				}
			}
			else
			{
				bool firstStopTail = true;
				int startLine = e.PrevLineCount;
				if (e.IsRollover)
				{
					ShiftFilterLines(e.RolloverOffset);
					startLine -= e.RolloverOffset;
				}
				for (int i = startLine; i < e.LineCount; ++i)
				{
					string line = this._logFileReader.GetLogLine(i);
					if (line != null)
					{
						IList<HilightEntry> matchingList = FindMatchingHilightEntries(line);
						LaunchHighlightPlugins(matchingList, i);
						GetHilightActions(matchingList, out suppressLed, out stopTail, out setBookmark, out bookmarkComment);
						if (setBookmark)
						{
							SetBookmarkFx fx = new SetBookmarkFx(this.SetBookmarkFromTrigger);
							fx.BeginInvoke(i, bookmarkComment, null, null);
						}
						if (stopTail && this._guiStateArgs.FollowTail)
						{
							bool wasFollow = this._guiStateArgs.FollowTail;
							FollowTailChanged(false, true);
							if (firstStopTail && wasFollow)
							{
								this.Invoke(new SelectLineFx(this.SelectAndEnsureVisible), new object[] { i, false });
								firstStopTail = false;
							}
						}
						if (!suppressLed)
						{
							noLed = false;
						}
					}
				}
			}
			if (!noLed)
			{
				OnFileSizeChanged(e);
			}
		}

		private void LaunchHighlightPlugins(IList<HilightEntry> matchingList, int lineNum)
		{
			LogExpertCallback callback = new LogExpertCallback(this);
			callback.LineNum = lineNum;
			foreach (HilightEntry entry in matchingList)
			{
				if (entry.IsActionEntry && entry.ActionEntry.pluginName != null)
				{
					IKeywordAction plugin = PluginRegistry.GetInstance().FindKeywordActionPluginByName(entry.ActionEntry.pluginName);
					if (plugin != null)
					{
						ActionPluginExecuteFx fx = new ActionPluginExecuteFx(plugin.Execute);
						fx.BeginInvoke(entry.SearchText, entry.ActionEntry.actionParam, callback, this.CurrentColumnizer, null, null);
					}
				}
			}
		}

		private void AutoResizeColumns(DataGridView gridView)
		{
			try
			{
				gridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
				if (gridView.Columns.Count > 1 && this.Preferences.setLastColumnWidth &&
					gridView.Columns[gridView.Columns.Count - 1].Width < this.Preferences.lastColumnWidth
				)
				{
					// It seems that using 'MinimumWidth' instead of 'Width' prevents the DataGridView's NullReferenceExceptions
					//gridView.Columns[gridView.Columns.Count - 1].Width = this.Preferences.lastColumnWidth;
					gridView.Columns[gridView.Columns.Count - 1].MinimumWidth = this.Preferences.lastColumnWidth;
				}
			}
			catch (NullReferenceException e)
			{
				// See https://connect.microsoft.com/VisualStudio/feedback/details/366943/autoresizecolumns-in-datagridview-throws-nullreferenceexception
				// There are some rare situations with null ref exceptions when resizing columns and on filter finished
				// So catch them here. Better than crashing.
				Logger.logError("Error while resizing columns: " + e.Message);
				Logger.logError(e.StackTrace);
			}
		}

		/// <summary>
		/// Builds a list of HilightMatchEntry objects. A HilightMatchEntry spans over a region that is painted with the same foreground and 
		/// background colors.
		/// All regions which don't match a word-mode entry will be painted with the colors of a default entry (groundEntry). This is either the 
		/// first matching non-word-mode highlight entry or a black-on-white default (if no matching entry was found).
		/// </summary>
		/// <param name="matchList">List of all highlight matches for the current cell</param>
		/// <param name="groundEntry">The entry that is used as the default.</param>
		/// <returns>List of HilightMatchEntry objects. The list spans over the whole cell and contains color infos for every substring.</returns>
		private IList<HilightMatchEntry> MergeHighlightMatchEntries(IList<HilightMatchEntry> matchList, HilightMatchEntry groundEntry)
		{
			// Fill an area with lenth of whole text with a default hilight entry
			HilightEntry[] entryArray = new HilightEntry[groundEntry.Length];
			for (int i = 0; i < entryArray.Length; ++i)
			{
				entryArray[i] = groundEntry.HilightEntry;
			}

			// "overpaint" with all matching word match enries
			// Non-word-mode matches will not overpaint because they use the groundEntry
			foreach (HilightMatchEntry me in matchList)
			{
				int endPos = me.StartPos + me.Length;
				for (int i = me.StartPos; i < endPos; ++i)
				{
					if (me.HilightEntry.IsWordMatch)
					{
						entryArray[i] = me.HilightEntry;
					}
				}
			}

			// collect areas with same hilight entry and build new highlight match entries for it
			IList<HilightMatchEntry> mergedList = new List<HilightMatchEntry>();
			if (entryArray.Length > 0)
			{
				HilightEntry currentEntry = entryArray[0];
				int lastStartPos = 0;
				int pos = 0;
				for (; pos < entryArray.Length; ++pos)
				{
					if (entryArray[pos] != currentEntry)
					{
						HilightMatchEntry me = new HilightMatchEntry();
						me.StartPos = lastStartPos;
						me.Length = pos - lastStartPos;
						me.HilightEntry = currentEntry;
						mergedList.Add(me);
						currentEntry = entryArray[pos];
						lastStartPos = pos;
					}
				}
				HilightMatchEntry me2 = new HilightMatchEntry();
				me2.StartPos = lastStartPos;
				me2.Length = pos - lastStartPos;
				me2.HilightEntry = currentEntry;
				mergedList.Add(me2);
			}
			return mergedList;
		}

		/**
		 * Returns the first HilightEntry that matches the given line
		 */
		private HilightEntry FindHilightEntry(string line)
		{
			return FindHilightEntry(line, false);
		}

		private HilightEntry FindFirstNoWordMatchHilightEntry(string line)
		{
			return FindHilightEntry(line, true);
		}

		private bool CheckHighlightEntryMatch(HilightEntry entry, string line)
		{
			if (entry.IsRegEx)
			{
				if (entry.Regex.IsMatch(line))
				{
					return true;
				}
			}
			else
			{
				if (entry.IsCaseSensitive)
				{
					if (line.Contains(entry.SearchText))
					{
						return true;
					}
				}
				else
				{
					if (line.ToLower().Contains(entry.SearchText.ToLower()))
					{
						return true;
					}
				}
			}
			return false;
		}

		/**
		 * Returns all HilightEntry entries which matches the given line
		 */
		private IList<HilightEntry> FindMatchingHilightEntries(string line)
		{
			IList<HilightEntry> resultList = new List<HilightEntry>();
			if (line != null)
			{
				lock (this._currentHighlightGroupLock)
				{
					foreach (HilightEntry entry in this._currentHighlightGroup.HilightEntryList)
					{
						if (CheckHighlightEntryMatch(entry, line))
						{
							resultList.Add(entry);
						}
					}
				}
			}
			return resultList;
		}

		private void GetHighlightEntryMatches(string line, IList<HilightEntry> hilightEntryList, IList<HilightMatchEntry> resultList)
		{
			foreach (HilightEntry entry in hilightEntryList)
			{
				if (entry.IsWordMatch)
				{
					MatchCollection matches = entry.Regex.Matches(line);
					foreach (Match match in matches)
					{
						HilightMatchEntry me = new HilightMatchEntry();
						me.HilightEntry = entry;
						me.StartPos = match.Index;
						me.Length = match.Length;
						resultList.Add(me);
					}
				}
				else
				{
					if (CheckHighlightEntryMatch(entry, line))
					{
						HilightMatchEntry me = new HilightMatchEntry();
						me.HilightEntry = entry;
						me.StartPos = 0;
						me.Length = line.Length;
						resultList.Add(me);
					}
				}
			}
		}

		private void GetHilightActions(IList<HilightEntry> matchingList, out bool noLed, out bool stopTail, out bool setBookmark, out string bookmarkComment)
		{
			noLed = stopTail = setBookmark = false;
			bookmarkComment = "";

			foreach (HilightEntry entry in matchingList)
			{
				if (entry.IsLedSwitch)
				{
					noLed = true;
				}
				if (entry.IsSetBookmark)
				{
					setBookmark = true;
					if (entry.BookmarkComment != null && entry.BookmarkComment.Length > 0)
					{
						bookmarkComment += entry.BookmarkComment + "\r\n";
					}
				}
				if (entry.IsStopTail)
					stopTail = true;
			}
			bookmarkComment.TrimEnd(new char[] { '\r', '\n' });
		}

		private void StopTimespreadThread()
		{
			this._timeSpreadCalc.Stop();
		}

		private void StopTimestampSyncThread()
		{
			this._shouldTimestampDisplaySyncingCancel = true;
			this._timeshiftSyncWakeupEvent.Set();
			this._timeshiftSyncThread.Abort();
			this._timeshiftSyncThread.Join();
		}

		private void SyncTimestampDisplay()
		{
			if (this.CurrentColumnizer.IsTimeshiftImplemented())
			{
				if (this.dataGridView.CurrentRow != null)
				{
					SyncTimestampDisplay(this.dataGridView.CurrentRow.Index);
				}
			}
		}

		private void SyncTimestampDisplay(int lineNum)
		{
			this._timeshiftSyncLine = lineNum;
			this._timeshiftSyncTimerEvent.Set();
			this._timeshiftSyncWakeupEvent.Set();
		}

		private void SyncTimestampDisplayWorker()
		{
			const int WAIT_TIME = 500;
			Thread.CurrentThread.Name = "SyncTimestampDisplayWorker";
			this._shouldTimestampDisplaySyncingCancel = false;
			this._isTimestampDisplaySyncing = true;

			while (!this._shouldTimestampDisplaySyncingCancel)
			{
				this._timeshiftSyncWakeupEvent.WaitOne();
				if (this._shouldTimestampDisplaySyncingCancel)
				{
					return;
				}
				this._timeshiftSyncWakeupEvent.Reset();

				while (!this._shouldTimestampDisplaySyncingCancel)
				{
					bool signaled = this._timeshiftSyncTimerEvent.WaitOne(WAIT_TIME, true);
					this._timeshiftSyncTimerEvent.Reset();
					if (!signaled)
					{
						break;
					}
				}
				// timeout with no new Trigger -> update display
				int lineNum = this._timeshiftSyncLine;
				if (lineNum >= 0 && lineNum < this.dataGridView.RowCount)
				{
					int refLine = lineNum;
					DateTime timeStamp = GetTimestampForLine(ref refLine, true);
					if (!timeStamp.Equals(DateTime.MinValue) && !this._shouldTimestampDisplaySyncingCancel)
					{
						this._guiStateArgs.Timestamp = timeStamp;
						SendGuiStateUpdate();
						if (this._shouldCallTimeSync)
						{
							refLine = lineNum;
							DateTime exactTimeStamp = GetTimestampForLine(ref refLine, false);
							SyncOtherWindows(exactTimeStamp);
							this._shouldCallTimeSync = false;
						}
					}
				}
				// show time difference between 2 selected lines
				if (this.dataGridView.SelectedRows.Count == 2)
				{
					int row1 = this.dataGridView.SelectedRows[0].Index;
					int row2 = this.dataGridView.SelectedRows[1].Index;
					if (row1 > row2)
					{
						int tmp = row1;
						row1 = row2;
						row2 = tmp;
					}
					int refLine = row1;
					DateTime timeStamp1 = GetTimestampForLine(ref refLine, false);
					refLine = row2;
					DateTime timeStamp2 = GetTimestampForLine(ref refLine, false);
					DateTime diff;
					if (timeStamp1.Ticks > timeStamp2.Ticks)
					{
						diff = new DateTime(timeStamp1.Ticks - timeStamp2.Ticks);
					}
					else
					{
						diff = new DateTime(timeStamp2.Ticks - timeStamp1.Ticks);
					}
					StatusLineText("Time diff is " + diff.ToString("HH:mm:ss.fff"));
				}
				else
				{
					if (!this.IsMultiFile && this.dataGridView.SelectedRows.Count == 1)
					{
						StatusLineText("");
					}
				}
			}
		}

		private void SyncFilterGridPos()
		{
			try
			{
				if (this._filterResultList.Count > 0)
				{
					int index = this._filterResultList.BinarySearch(this.dataGridView.CurrentRow.Index);
					if (index < 0)
					{
						index = ~index;
						if (index > 0)
							--index;
					}
					if (this.filterGridView.Rows.Count > 0)	// exception no rows
					{
						this.filterGridView.CurrentCell = this.filterGridView.Rows[index].Cells[0];
					}
					else
					{
						this.filterGridView.CurrentCell = null;
					}
				}
			}
			catch (Exception e)
			{
				Logger.logError("SyncFilterGridPos(): " + e.Message);
			}
		}

		private void StatusLineFileSize(long size)
		{
			this._statusEventArgs.FileSize = size;
			SendStatusLineUpdate();
		}

		private int Search(SearchParams searchParams)
		{
			UpdateProgressBarFx progressFx = new UpdateProgressBarFx(UpdateProgressBar);
			if (searchParams.searchText == null)
			{
				return -1;
			}
			int lineNum = (searchParams.isFromTop && !searchParams.isFindNext) ? 0 : searchParams.currentLine;
			string lowerSearchText = searchParams.searchText.ToLower();
			int count = 0;
			bool hasWrapped = false;
			while (true)
			{
				if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
				{
					if (lineNum >= this._logFileReader.LineCount)
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
						lineNum = this._logFileReader.LineCount - 1;
						hasWrapped = true;
						StatusLineError("Started from end of file");
					}
				}

				string line = this._logFileReader.GetLogLine(lineNum);
				if (line == null)
				{
					return -1;
				}

				if (searchParams.isRegex)
				{
					Regex rex = new Regex(searchParams.searchText, searchParams.isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
					if (rex.IsMatch(line))
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
				if (this._shouldCancel)
				{
					return -1;
				}
				if (++count % PROGRESS_BAR_MODULO == 0)
				{
					try
					{
						if (!this.Disposing)
							this.Invoke(progressFx, new object[] { count });
					}
					catch (ObjectDisposedException)  // can occur when closing the app while searching
					{
					}
				}
			}
		}

		private void SearchComplete(IAsyncResult result)
		{
			if (this.Disposing)
				return;
			try
			{
				this.Invoke(new MethodInvoker(ResetProgressBar));
				AsyncResult ar = (AsyncResult)result;
				SearchFx fx = (SearchFx)ar.AsyncDelegate;
				int line = fx.EndInvoke(result);
				this._guiStateArgs.MenuEnabled = true;
				GuiStateUpdate(this, this._guiStateArgs);
				if (line == -1)
				{
					return;
				}
				this.dataGridView.Invoke(new SelectLineFx(SelectLine), new object[] { line, true });
			}
			catch (Exception) // in the case the windows is already destroyed
			{
			}
		}

		private void ResetProgressBar()
		{
			this._progressEventArgs.Value = this._progressEventArgs.MaxValue;
			this._progressEventArgs.Visible = false;
			SendProgressBarUpdate();
		}

		private void SelectLine(int line, bool triggerSyncCall)
		{
			SelectLine(line, triggerSyncCall, true);
		}

		private void SelectLine(int line, bool triggerSyncCall, bool shouldScroll)
		{
			try
			{
				this._shouldCallTimeSync = triggerSyncCall;
				bool wasCancelled = this._shouldCancel;
				this._shouldCancel = false;
				this._isSearching = false;
				StatusLineText("");
				this._guiStateArgs.MenuEnabled = true;
				if (wasCancelled)
				{
					return;
				}
				if (line == -1)
				{
					MessageBox.Show(this, "Not found:", "Search result");   // Hmm... is that experimental code from early days?  
					return;
				}
				this.dataGridView.Rows[line].Selected = true;
				if (shouldScroll)
				{
					this.dataGridView.CurrentCell = this.dataGridView.Rows[line].Cells[0];
					this.dataGridView.Focus();
				}
			}
			catch (IndexOutOfRangeException e)
			{
				// Occures sometimes (but cannot reproduce)
				Logger.logError("Error while selecting line: " + e.ToString());
			}
		}

		private void SelectAndScrollToLine(int line)
		{
			SelectLine(line, false);
			this.dataGridView.FirstDisplayedScrollingRowIndex = line;
		}

		public void LogWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (this._isErrorShowing)
			{
				RemoveStatusLineError();
			}
			if (e.KeyCode == Keys.F3)
			{
				if (this._parentLogTabWin.SearchParams == null ||
					this._parentLogTabWin.SearchParams.searchText == null ||
					this._parentLogTabWin.SearchParams.searchText.Length == 0)
				{
					return;
				}
				this._parentLogTabWin.SearchParams.isFindNext = true;
				this._parentLogTabWin.SearchParams.isShiftF3Pressed = ((e.Modifiers & Keys.Shift) == Keys.Shift);
				StartSearch();
			}
			if (e.KeyCode == Keys.Escape)
			{
				if (this._isSearching)
				{
					this._shouldCancel = true;
				}
				FireCancelHandlers();
				RemoveAllSearchHighlightEntries();
			}
			if (e.KeyCode == Keys.E && (e.Modifiers & Keys.Control) == Keys.Control)
			{
				StartEditMode();
			}
			if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
			{
				int newLine = this._logFileReader.GetNextMultiFileLine(this.dataGridView.CurrentCellAddress.Y);
				if (newLine != -1)
				{
					SelectLine(newLine, false);
				}
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
			{
				int newLine = this._logFileReader.GetPrevMultiFileLine(this.dataGridView.CurrentCellAddress.Y);
				if (newLine != -1)
				{
					SelectLine(newLine - 1, false);
				}
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Enter && this.dataGridView.Focused)
			{
				ChangeRowHeight(e.Shift);
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Back && this.dataGridView.Focused)
			{
				ChangeRowHeight(true);
				e.Handled = true;
			}
			if (e.KeyCode == Keys.PageUp && e.Modifiers == Keys.Alt)
			{
				SelectPrevHighlightLine();
				e.Handled = true;
			}
			if (e.KeyCode == Keys.PageDown && e.Modifiers == Keys.Alt)
			{
				SelectNextHighlightLine();
				e.Handled = true;
			}
			if (e.KeyCode == Keys.T && (e.Modifiers & Keys.Control) == Keys.Control && (e.Modifiers & Keys.Shift) == Keys.Shift)
			{
				FilterToTab();
			}
		}

		private void StartEditMode()
		{
			if (!this.dataGridView.CurrentCell.ReadOnly)
			{
				this.dataGridView.BeginEdit(false);
				if (this.dataGridView.EditingControl != null)
				{
					if (this.dataGridView.EditingControl.GetType().IsAssignableFrom(typeof(LogCellEditingControl)))
					{
						DataGridViewTextBoxEditingControl editControl = this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
						editControl.KeyDown += EditControl_UpdateEditColumnDisplay;
						editControl.KeyPress += new KeyPressEventHandler(EditControl_KeyPress);
						editControl.KeyUp += new KeyEventHandler(EditControl_UpdateEditColumnDisplay);
						editControl.Click += new EventHandler(EditControl_Click);
						this.dataGridView.CellEndEdit += new DataGridViewCellEventHandler(DataGridView_CellEndEdit);
						editControl.SelectionStart = 0;
					}
				}
			}
		}

		private void UpdateEditColumnDisplay(DataGridViewTextBoxEditingControl editControl)
		{
			// prevents key events after edit mode has ended
			if (this.dataGridView.EditingControl != null)
			{
				int pos = editControl.SelectionStart + editControl.SelectionLength;
				StatusLineText("   " + pos);
				Logger.logDebug("SelStart: " + editControl.SelectionStart + ", SelLen: " + editControl.SelectionLength);
			}
		}

		private void SelectPrevHighlightLine()
		{
			int lineNum = this.dataGridView.CurrentCellAddress.Y;
			while (lineNum > 0)
			{
				lineNum--;
				string line = this._logFileReader.GetLogLine(lineNum);
				if (line != null)
				{
					HilightEntry entry = FindHilightEntry(line);
					if (entry != null)
					{
						SelectLine(lineNum, false);
						break;
					}
				}
			}
		}

		private void SelectNextHighlightLine()
		{
			int lineNum = this.dataGridView.CurrentCellAddress.Y;
			while (lineNum < this._logFileReader.LineCount)
			{
				lineNum++;
				string line = _logFileReader.GetLogLine(lineNum);
				if (line != null)
				{
					HilightEntry entry = FindHilightEntry(line);
					if (entry != null)
					{
						SelectLine(lineNum, false);
						break;
					}
				}
			}
		}

		private int FindNextBookmarkIndex(int lineNum, bool isForward)
		{
			lineNum = dataGridView.RowCount.GetNextIndex(lineNum, isForward);

			if (isForward)
			{
				return this._bookmarkProvider.FindNextBookmarkIndex(lineNum);
			}
			else
			{
				return _bookmarkProvider.FindPrevBookmarkIndex(lineNum);
			}
		}

		/**
		 * Shift bookmarks after a logfile rollover
		 */
		private void ShiftBookmarks(int offset)
		{
			this._bookmarkProvider.ShiftBookmarks(offset);
			OnBookmarkRemoved();
		}

		private void ShiftRowHeightList(int offset)
		{
			SortedList<int, RowHeightEntry> newList = new SortedList<int, RowHeightEntry>();
			foreach (RowHeightEntry entry in this._rowHeightList.Values)
			{
				int line = entry.LineNum - offset;
				if (line >= 0)
				{
					entry.LineNum = line;
					newList.Add(line, entry);
				}
			}
			this._rowHeightList = newList;
		}

		private void ShiftFilterPipes(int offset)
		{
			lock (this._filterPipeList)
			{
				foreach (FilterPipe pipe in this._filterPipeList)
				{
					pipe.ShiftLineNums(offset);
				}
			}
		}

		private void LoadFilterPipes()
		{
			lock (this._filterPipeList)
			{
				foreach (FilterPipe pipe in this._filterPipeList)
				{
					pipe.RecreateTempFile();
				}
			}
			if (this._filterPipeList.Count > 0)
			{
				for (int i = 0; i < this.dataGridView.RowCount; ++i)
				{
					ProcessFilterPipes(i);
				}
			}
		}

		private void DisconnectFilterPipes()
		{
			lock (this._filterPipeList)
			{
				foreach (FilterPipe pipe in this._filterPipeList)
				{
					pipe.ClearLineList();
				}
			}
		}

		// ======================================================================================
		// Filter Grid stuff
		// ======================================================================================

		private void ApplyFilterParams()
		{
			this.filterComboBox.Text = this._filterParams.searchText;
			this.filterCaseSensitiveCheckBox.Checked = this._filterParams.isCaseSensitive;
			this.filterRegexCheckBox.Checked = this._filterParams.isRegex;
			this.filterTailCheckBox.Checked = this._filterParams.isFilterTail;
			this.invertFilterCheckBox.Checked = this._filterParams.isInvert;
			this.filterKnobControl1.Value = this._filterParams.spreadBefore;
			this.filterKnobControl2.Value = this._filterParams.spreadBehind;
			this.rangeCheckBox.Checked = this._filterParams.isRangeSearch;
			this.columnRestrictCheckBox.Checked = this._filterParams.columnRestrict;
			this.fuzzyKnobControl.Value = this._filterParams.fuzzyValue;
			this.filterRangeComboBox.Text = this._filterParams.rangeSearchText;
		}

		private void ResetFilterControls()
		{
			this.filterComboBox.Text = "";
			this.filterCaseSensitiveCheckBox.Checked = false;
			this.filterRegexCheckBox.Checked = false;
			this.invertFilterCheckBox.Checked = false;
			this.filterKnobControl1.Value = 0;
			this.filterKnobControl2.Value = 0;
			this.rangeCheckBox.Checked = false;
			this.columnRestrictCheckBox.Checked = false;
			this.fuzzyKnobControl.Value = 0;
			this.filterRangeComboBox.Text = "";
		}

		private void FilterSearch()
		{
			if (this.filterComboBox.Text.Length == 0)
			{
				this._filterParams.searchText = "";
				this._filterParams.lowerSearchText = "";
				this._filterParams.isRangeSearch = false;
				ClearFilterList();
				this.filterSearchButton.Image = null;
				ResetFilterControls();
				this.saveFilterButton.Enabled = false;
				return;
			}
			FilterSearch(this.filterComboBox.Text);
		}

		private void FilterSearch(string text)
		{
			FireCancelHandlers();   // make sure that there's no other filter running (maybe from filter restore)

			this._filterParams.searchText = text;
			this._filterParams.lowerSearchText = text.ToLower();
			ConfigManager.Settings.filterHistoryList.Remove(text);
			ConfigManager.Settings.filterHistoryList.Insert(0, text);
			if (ConfigManager.Settings.filterHistoryList.Count > MAX_HISTORY)
			{
				ConfigManager.Settings.filterHistoryList.RemoveAt(this.filterComboBox.Items.Count - 1);
			}
			this.filterComboBox.Items.Clear();
			foreach (string item in ConfigManager.Settings.filterHistoryList)
			{
				this.filterComboBox.Items.Add(item);
			}
			this.filterComboBox.Text = text;

			this._filterParams.isRangeSearch = this.rangeCheckBox.Checked;
			this._filterParams.rangeSearchText = this.filterRangeComboBox.Text;
			if (this._filterParams.isRangeSearch)
			{
				ConfigManager.Settings.filterRangeHistoryList.Remove(this.filterRangeComboBox.Text);
				ConfigManager.Settings.filterRangeHistoryList.Insert(0, this.filterRangeComboBox.Text);
				if (ConfigManager.Settings.filterRangeHistoryList.Count > MAX_HISTORY)
				{
					ConfigManager.Settings.filterRangeHistoryList.RemoveAt(this.filterRangeComboBox.Items.Count - 1);
				}

				this.filterRangeComboBox.Items.Clear();
				foreach (string item in ConfigManager.Settings.filterRangeHistoryList)
				{
					this.filterRangeComboBox.Items.Add(item);
				}
			}
			ConfigManager.Save(SettingsFlags.FilterHistory);

			this._filterParams.isCaseSensitive = this.filterCaseSensitiveCheckBox.Checked;
			this._filterParams.isRegex = this.filterRegexCheckBox.Checked;
			this._filterParams.isFilterTail = this.filterTailCheckBox.Checked;
			this._filterParams.isInvert = this.invertFilterCheckBox.Checked;
			if (this._filterParams.isRegex)
			{
				try
				{
					this._filterParams.CreateRegex();
				}
				catch (ArgumentException)
				{
					StatusLineError("Invalid regular expression");
					return;
				}
			}
			this._filterParams.fuzzyValue = this.fuzzyKnobControl.Value;
			this._filterParams.spreadBefore = this.filterKnobControl1.Value;
			this._filterParams.spreadBehind = this.filterKnobControl2.Value;
			this._filterParams.columnRestrict = this.columnRestrictCheckBox.Checked;

			//ConfigManager.SaveFilterParams(this.filterParams);
			ConfigManager.Settings.filterParams = this._filterParams;  // wozu eigentlich? sinnlos seit MDI?

			this._shouldCancel = false;
			this._isSearching = true;
			StatusLineText("Filtering... Press ESC to cancel");
			this.filterSearchButton.Enabled = false;
			ClearFilterList();

			this._progressEventArgs.MinValue = 0;
			this._progressEventArgs.MaxValue = this.dataGridView.RowCount;
			this._progressEventArgs.Value = 0;
			this._progressEventArgs.Visible = true;
			SendProgressBarUpdate();

			Settings settings = ConfigManager.Settings;
			FilterFx fx;
			fx = settings.preferences.multiThreadFilter ? new FilterFx(MultiThreadedFilter) : new FilterFx(Filter);
			fx.BeginInvoke(this._filterParams, this._filterResultList, this._lastFilterLinesList, this._filterHitList, FilterComplete, null);
			CheckForFilterDirty();
		}

		private void MultiThreadedFilter(FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterLinesList, List<int> filterHitList)
		{
			ColumnizerCallback callback = new ColumnizerCallback(this);
			FilterStarter fs = new FilterStarter(callback, Environment.ProcessorCount + 2);
			fs.FilterHitList = this._filterHitList;
			fs.FilterResultLines = this._filterResultList;
			fs.LastFilterLinesList = this._lastFilterLinesList;
			BackgroundProcessCancelHandler cancelHandler = new FilterCancelHandler(fs);
			RegisterCancelHandler(cancelHandler);
			long startTime = Environment.TickCount;

			fs.DoFilter(filterParams, 0, this._logFileReader.LineCount, FilterProgressCallback);

			long endTime = Environment.TickCount;
#if DEBUG
			Logger.logInfo("Multi threaded filter duration: " + ((endTime - startTime)) + " ms.");
#endif
			DeRegisterCancelHandler(cancelHandler);
			StatusLineText("Filter duration: " + ((endTime - startTime)) + " ms.");
		}

		private void FilterProgressCallback(int lineCount)
		{
			UpdateProgressBar(lineCount);
		}

		private void Filter(FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterLinesList, List<int> filterHitList)
		{
			long startTime = Environment.TickCount;
			try
			{
				filterParams.Reset();
				int lineNum = 0;
				ColumnizerCallback callback = new ColumnizerCallback(this);
				while (true)
				{
					string line = this._logFileReader.GetLogLine(lineNum);
					if (line == null)
					{
						break;
					}
					callback.LineNum = lineNum;
					if (Classes.DamerauLevenshtein.TestFilterCondition(filterParams, line, callback))
					{
						AddFilterLine(lineNum, false, filterParams, filterResultLines, lastFilterLinesList, filterHitList);
					}
					lineNum++;
					if (lineNum % PROGRESS_BAR_MODULO == 0)
					{
						UpdateProgressBar(lineNum);
					}
					if (this._shouldCancel)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.logError("Exception while filtering. Please report to developer: \n\n" + ex + "\n\n" + ex.StackTrace);
				MessageBox.Show(null, "Exception while filtering. Please report to developer: \n\n" + ex + "\n\n" + ex.StackTrace, "LogExpert");
			}
			long endTime = Environment.TickCount;
#if DEBUG
			Logger.logInfo("Single threaded filter duration: " + ((endTime - startTime)) + " ms.");
#endif
			StatusLineText("Filter duration: " + ((endTime - startTime)) + " ms.");
		}

		/// <summary>
		///  Returns a list with 'additional filter results'. This is the given line number 
		///  and (if back spread and/or fore spread is enabled) some additional lines.
		///  This function doesn't check the filter condition! 
		/// </summary>
		/// <param name="filterParams"></param>
		/// <param name="lineNum"></param>
		/// <param name="checkList"></param>
		/// <returns></returns>
		private IList<int> GetAdditionalFilterResults(FilterParams filterParams, int lineNum, IList<int> checkList)
		{
			IList<int> resultList = new List<int>();

			if (filterParams.spreadBefore == 0 && filterParams.spreadBehind == 0)
			{
				resultList.Add(lineNum);
				return resultList;
			}

			// back spread
			for (int i = filterParams.spreadBefore; i > 0; --i)
			{
				if (lineNum - i > 0)
				{
					if (!resultList.Contains(lineNum - i) && !checkList.Contains(lineNum - i))
					{
						resultList.Add(lineNum - i);
					}
				}
			}
			// direct filter hit
			if (!resultList.Contains(lineNum) && !checkList.Contains(lineNum))
			{
				resultList.Add(lineNum);
			}
			// after spread
			for (int i = 1; i <= filterParams.spreadBehind; ++i)
			{
				if (lineNum + i < this._logFileReader.LineCount)
				{
					if (!resultList.Contains(lineNum + i) && !checkList.Contains(lineNum + i))
					{
						resultList.Add(lineNum + i);
					}
				}
			}
			return resultList;
		}

		private void AddFilterLine(int lineNum, bool immediate, FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterLinesList, List<int> filterHitList)
		{
			lock (this._filterResultList)
			{
				filterHitList.Add(lineNum);
				IList<int> filterResult = GetAdditionalFilterResults(filterParams, lineNum, lastFilterLinesList);
				filterResultLines.AddRange(filterResult);
				lastFilterLinesList.AddRange(filterResult);
				if (lastFilterLinesList.Count > SPREAD_MAX * 2)
				{
					lastFilterLinesList.RemoveRange(0, lastFilterLinesList.Count - SPREAD_MAX * 2);
				}
			}
			if (immediate)
			{
				TriggerFilterLineGuiUpdate();
			}
		}

		private void UpdateFilterCountLabel(int count)
		{
			this.filterCountLabel.Text = "" + this._filterResultList.Count;
		}

		private void TriggerFilterLineGuiUpdate()
		{
			this.Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
		}

		private void AddFilterLineGuiUpdate()
		{
			try
			{
				lock (this._filterResultList)
				{
					this.filterCountLabel.Text = "" + this._filterResultList.Count;
					if (this.filterGridView.RowCount > this._filterResultList.Count)
					{
						this.filterGridView.RowCount = 0;  // helps to prevent hang ?
					}
					this.filterGridView.RowCount = this._filterResultList.Count;
					if (this.filterGridView.RowCount > 0)
					{
						this.filterGridView.FirstDisplayedScrollingRowIndex = this.filterGridView.RowCount - 1;
					}
					if (this.filterGridView.RowCount == 1)
					{
						// after a file reload adjusted column sizes anew when the first line arrives
						//this.filterGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
						AutoResizeColumns(this.filterGridView);
					}
				}
			}
			catch (Exception e)
			{
				Logger.logError("AddFilterLineGuiUpdate(): " + e.Message);
			}
		}

		private void UpdateProgressBar(int value)
		{
			this._progressEventArgs.Value = value;
			if (value > this._progressEventArgs.MaxValue)
			{
				// can occur if new lines will be added while filtering
				this._progressEventArgs.MaxValue = value;
			}
			SendProgressBarUpdate();
		}

		private void FilterComplete(IAsyncResult result)
		{
			if (!this.IsDisposed && !this._waitingForClose && !this.Disposing)
			{
				this.Invoke(new MethodInvoker(ResetStatusAfterFilter));
			}
		}

		private void ResetStatusAfterFilter()
		{
			try
			{
				this._isSearching = false;
				this._progressEventArgs.Value = this._progressEventArgs.MaxValue;
				this._progressEventArgs.Visible = false;
				SendProgressBarUpdate();
				this.filterGridView.RowCount = this._filterResultList.Count;
				AutoResizeColumns(this.filterGridView);
				this.filterCountLabel.Text = "" + this._filterResultList.Count;
				if (filterGridView.RowCount > 0)
				{
					filterGridView.Focus();
				}
				this.filterSearchButton.Enabled = true;
			}
			catch (NullReferenceException e)
			{
				// See https://connect.microsoft.com/VisualStudio/feedback/details/366943/autoresizecolumns-in-datagridview-throws-nullreferenceexception
				// There are some rare situations with null ref exceptions when resizing columns and on filter finished
				// So catch them here. Better than crashing.
				Logger.logError("Error: " + e.Message);
				Logger.logError(e.StackTrace);
			}
		}

		private void ClearFilterList()
		{
			try
			{
				lock (this._filterResultList)
				{
					this.filterGridView.SuspendLayout();
					this.filterGridView.RowCount = 0;
					this.filterCountLabel.Text = "0";
					this._filterResultList = new List<int>();
					this._lastFilterLinesList = new List<int>();
					this._filterHitList = new List<int>();
					this.filterGridView.ResumeLayout();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(null, ex.StackTrace, "Wieder dieser sporadische Fehler:");
				Logger.logError("Wieder dieser sporadische Fehler: " + ex + "\n" + ex.StackTrace);
			}
		}

		private void ClearBookmarkList()
		{
			this._bookmarkProvider.ClearAllBookmarks();
			OnAllBookmarksRemoved();
		}

		/**
		 * Shift filter list line entries after a logfile rollover
		 */
		private void ShiftFilterLines(int offset)
		{
			List<int> newFilterList = new List<int>();
			lock (this._filterResultList)
			{
				foreach (int lineNum in this._filterResultList)
				{
					int line = lineNum - offset;
					if (line >= 0)
					{
						newFilterList.Add(line);
					}
				}
				this._filterResultList = newFilterList;
			}

			newFilterList = new List<int>();
			foreach (int lineNum in this._filterHitList)
			{
				int line = lineNum - offset;
				if (line >= 0)
				{
					newFilterList.Add(line);
				}
			}
			this._filterHitList = newFilterList;

			int count = SPREAD_MAX;
			if (this._filterResultList.Count < SPREAD_MAX)
			{
				count = this._filterResultList.Count;
			}
			this._lastFilterLinesList = this._filterResultList.GetRange(this._filterResultList.Count - count, count);

			TriggerFilterLineGuiUpdate();
		}

		private void CheckForFilterDirty()
		{
			if (IsFilterSearchDirty(this._filterParams))
			{
				this.filterSearchButton.Image = this._searchButtonImage;
				this.saveFilterButton.Enabled = false;
			}
			else
			{
				this.filterSearchButton.Image = null;
				this.saveFilterButton.Enabled = true;
			}
		}

		private bool IsFilterSearchDirty(FilterParams filterParams)
		{
			if (!filterParams.searchText.Equals(this.filterComboBox.Text))
			{
				return true;
			}
			if (filterParams.isRangeSearch != this.rangeCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.isRangeSearch && !filterParams.rangeSearchText.Equals(this.filterRangeComboBox.Text))
			{
				return true;
			}
			if (filterParams.isRegex != this.filterRegexCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.isInvert != this.invertFilterCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.spreadBefore != this.filterKnobControl1.Value)
			{
				return true;
			}
			if (filterParams.spreadBehind != this.filterKnobControl2.Value)
			{
				return true;
			}
			if (filterParams.fuzzyValue != this.fuzzyKnobControl.Value)
			{
				return true;
			}
			if (filterParams.columnRestrict != this.columnRestrictCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.isCaseSensitive != this.filterCaseSensitiveCheckBox.Checked)
			{
				return true;
			}
			return false;
		}

		private void AdjustMinimumGridWith()
		{
			if (this.dataGridView.Columns.Count > 1)
			{
				AutoResizeColumns(this.dataGridView);

				int width = this.dataGridView.Columns.GetColumnsWidth(DataGridViewElementStates.Visible);
				int diff = this.dataGridView.Width - width;
				if (diff > 0)
				{
					diff -= this.dataGridView.RowHeadersWidth / 2;
					this.dataGridView.Columns[this.dataGridView.Columns.Count - 1].Width =
																						  this.dataGridView.Columns[this.dataGridView.Columns.Count - 1].Width + diff;
					this.filterGridView.Columns[this.filterGridView.Columns.Count - 1].Width =
																							  this.filterGridView.Columns[this.filterGridView.Columns.Count - 1].Width + diff;
				}
			}
		}

		public void ToggleFilterPanel()
		{
			this.splitContainer1.Panel2Collapsed = !this.splitContainer1.Panel2Collapsed;
			if (!this.splitContainer1.Panel2Collapsed)
			{
				this.filterComboBox.Focus();
			}
			else
			{
				this.dataGridView.Focus();
			}
		}

		private void InvalidateCurrentRow(DataGridView gridView)
		{
			if (gridView.CurrentCellAddress.Y > -1)
			{
				gridView.InvalidateRow(gridView.CurrentCellAddress.Y);
			}
		}

		private void InvalidateCurrentRow()
		{
			InvalidateCurrentRow(this.dataGridView);
			InvalidateCurrentRow(this.filterGridView);
		}

		private void DisplayCurrentFileOnStatusline()
		{
			if (this._logFileReader.IsMultiFile)
			{
				try
				{
					if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index > -1)
					{
						string fileName =
							this._logFileReader.GetLogFileNameForLine(this.dataGridView.CurrentRow.Index);
						if (fileName != null)
						{
							StatusLineText(Util.GetNameFromPath(fileName));
						}
					}
				}
				catch (Exception)
				{
					// TODO: handle this concurrent situation better:
					// this.dataGridView.CurrentRow may be null even if checked before.
					// This can happen when MultiFile shift deselects the current row because there 
					// are less lines after rollover than before.
					// access to dataGridView-Rows should be locked 
				}
			}
		}

		private void UpdateSelectionDisplay()
		{
			if (this._noSelectionUpdates)
			{
				return;
			}
			this._selectionChangedTrigger.Trigger();
		}

		private void UpdateFilterHistoryFromSettings()
		{
			ConfigManager.Settings.filterHistoryList = ConfigManager.Settings.filterHistoryList;
			this.filterComboBox.Items.Clear();
			foreach (string item in ConfigManager.Settings.filterHistoryList)
			{
				this.filterComboBox.Items.Add(item);
			}

			this.filterRangeComboBox.Items.Clear();
			foreach (string item in ConfigManager.Settings.filterRangeHistoryList)
			{
				this.filterRangeComboBox.Items.Add(item);
			}
		}

		private void StatusLineText(string text)
		{
			this._statusEventArgs.StatusText = text;
			SendStatusLineUpdate();
		}

		private void StatusLineTextImmediate(string text)
		{
			this._statusEventArgs.StatusText = text;
			this._statusLineTrigger.TriggerImmediate();
		}

		private void StatusLineError(string text)
		{
			StatusLineText(text);
			this._isErrorShowing = true;
		}

		private void RemoveStatusLineError()
		{
			StatusLineText("");
			this._isErrorShowing = false;
		}

		private void SendGuiStateUpdate()
		{
			OnGuiState(this._guiStateArgs);
		}

		private void SendProgressBarUpdate()
		{
			OnProgressBarUpdate(this._progressEventArgs);
		}

		private void SendStatusLineUpdate()
		{
			this._statusLineTrigger.Trigger();
		}

		private void ShowAdvancedFilterPanel(bool show)
		{
			if (show)
			{
				this.advancedButton.Text = "Hide advanced...";
				this.advancedButton.Image = null;
			}
			else
			{
				this.advancedButton.Text = "Show advanced...";
				CheckForAdvancedButtonDirty();
			}

			this.advancedFilterSplitContainer.Panel1Collapsed = !show;
			this.advancedFilterSplitContainer.SplitterDistance = 54;
			this._showAdvanced = show;
		}

		private void CheckForAdvancedButtonDirty()
		{
			if (this.IsAdvancedOptionActive() && !this._showAdvanced)
			{
				this.advancedButton.Image = this._advancedButtonImage;
			}
			else
			{
				this.advancedButton.Image = null;
			}
		}

		private void FilterToTab()
		{
			this.filterSearchButton.Enabled = false;
			MethodInvoker invoker = new MethodInvoker(WriteFilterToTab);
			invoker.BeginInvoke(null, null);
		}

		private void WriteFilterToTab()
		{
			FilterPipe pipe = new FilterPipe(this._filterParams.CreateCopy(), this);
			lock (this._filterResultList)
			{
				string namePrefix = "->F";
				string title;
				if (this.IsTempFile)
				{
					title = this.TempTitleName + namePrefix + ++this._filterPipeNameCounter;
				}
				else
				{
					title = Util.GetNameFromPath(this.FileName) + namePrefix + ++this._filterPipeNameCounter;
				}

				WritePipeToTab(pipe, this._filterResultList, title, null);
			}
		}

		private void WritePipeToTab(FilterPipe pipe, IList<int> lineNumberList, string name, PersistenceData persistenceData)
		{
			Logger.logInfo("WritePipeToTab(): " + lineNumberList.Count + " lines.");
			StatusLineText("Writing to temp file... Press ESC to cancel.");
			this._guiStateArgs.MenuEnabled = false;
			SendGuiStateUpdate();
			this._progressEventArgs.MinValue = 0;
			this._progressEventArgs.MaxValue = lineNumberList.Count;
			this._progressEventArgs.Value = 0;
			this._progressEventArgs.Visible = true;
			this.Invoke(new MethodInvoker(SendProgressBarUpdate));
			this._isSearching = true;
			this._shouldCancel = false;

			lock (this._filterPipeList)
			{
				this._filterPipeList.Add(pipe);
			}
			pipe.Closed += new FilterPipe.ClosedEventHandler(Pipe_Disconnected);
			int count = 0;
			pipe.OpenFile();
			LogExpertCallback callback = new LogExpertCallback(this);
			foreach (int i in lineNumberList)
			{
				if (this._shouldCancel)
				{
					break;
				}
				string line = this._logFileReader.GetLogLine(i);
				if (this.CurrentColumnizer is ILogLineXmlColumnizer)
				{
					callback.LineNum = i;
					line = (this.CurrentColumnizer as ILogLineXmlColumnizer).GetLineTextForClipboard(line, callback);
				}
				pipe.WriteToPipe(line, i);
				if (++count % PROGRESS_BAR_MODULO == 0)
				{
					this._progressEventArgs.Value = count;
					this.Invoke(new MethodInvoker(SendProgressBarUpdate));
				}
			}
			pipe.CloseFile();
			Logger.logInfo("WritePipeToTab(): finished");
			this.Invoke(new WriteFilterToTabFinishedFx(WriteFilterToTabFinished), new object[] { pipe, name, persistenceData });
		}

		private void WriteFilterToTabFinished(FilterPipe pipe, string name, PersistenceData persistenceData)
		{
			this._isSearching = false;
			if (!this._shouldCancel)
			{
				string title = name;
				ILogLineColumnizer preProcessColumnizer = null;
				if (!(this.CurrentColumnizer is ILogLineXmlColumnizer))
				{
					preProcessColumnizer = this.CurrentColumnizer;
				}
				LogWindow newWin = this._parentLogTabWin.AddFilterTab(pipe, title, new LoadingFinishedFx(LoadingFinishedFunc), preProcessColumnizer);
				newWin.FilterPipe = pipe;
				pipe.OwnLogWindow = newWin;
				if (persistenceData != null)
				{
					FilterRestoreFx fx = new FilterRestoreFx(FilterRestore);
					fx.BeginInvoke(newWin, persistenceData, null, null);
				}
			}
			this._progressEventArgs.Value = _progressEventArgs.MaxValue;
			this._progressEventArgs.Visible = false;
			SendProgressBarUpdate();
			this._guiStateArgs.MenuEnabled = true;
			SendGuiStateUpdate();
			StatusLineText("");
			this.filterSearchButton.Enabled = true;
		}

		/// <summary>
		/// Used to create a new tab and pipe the given content into it.
		/// </summary>
		/// <param name="lineEntryList"></param>
		private void WritePipeTab(IList<LineEntry> lineEntryList, string title)
		{
			FilterPipe pipe = new FilterPipe(new FilterParams(), this);
			pipe.IsStopped = true;
			pipe.Closed += new FilterPipe.ClosedEventHandler(Pipe_Disconnected);
			pipe.OpenFile();
			foreach (LineEntry entry in lineEntryList)
			{
				pipe.WriteToPipe(entry.logLine, entry.lineNum);
			}
			pipe.CloseFile();
			this.Invoke(new WriteFilterToTabFinishedFx(WriteFilterToTabFinished), new object[] { pipe, title, null });
		}

		private void FilterRestore(LogWindow newWin, PersistenceData persistenceData)
		{
			newWin.WaitForLoadingFinished();
			ILogLineColumnizer columnizer = Util.FindColumnizerByName(persistenceData.columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
			if (columnizer != null)
			{
				SetColumnizerFx fx = new SetColumnizerFx(newWin.ForceColumnizer);
				newWin.Invoke(fx, new object[] { columnizer });
			}
			else
			{
				Logger.logWarn("FilterRestore(): Columnizer " + persistenceData.columnizerName + " not found");
			}
			newWin.BeginInvoke(new RestoreFiltersFx(newWin.RestoreFilters), new object[] { persistenceData });
		}

		private void LoadingFinishedFunc(LogWindow newWin)
		{
			//if (newWin.forcedColumnizerForLoading != null)
			//{
			//  SetColumnizerFx fx = new SetColumnizerFx(newWin.ForceColumnizer);
			//  newWin.Invoke(fx, new object[] { newWin.forcedColumnizerForLoading });
			//  newWin.forcedColumnizerForLoading = null;
			//}
		}

		private void ProcessFilterPipes(int lineNum)
		{
			string searchLine = this._logFileReader.GetLogLine(lineNum);
			if (searchLine == null)
			{
				return;
			}

			ColumnizerCallback callback = new ColumnizerCallback(this);
			callback.LineNum = lineNum;
			IList<FilterPipe> deleteList = new List<FilterPipe>();
			lock (this._filterPipeList)
			{
				foreach (FilterPipe pipe in this._filterPipeList)
				{
					if (pipe.IsStopped)
					{
						continue;
					}
					if (Classes.DamerauLevenshtein.TestFilterCondition(pipe.FilterParams, searchLine, callback))
					{
						IList<int> filterResult = GetAdditionalFilterResults(pipe.FilterParams, lineNum, pipe.LastLinesHistoryList);
						pipe.OpenFile();
						foreach (int line in filterResult)
						{
							pipe.LastLinesHistoryList.Add(line);
							if (pipe.LastLinesHistoryList.Count > SPREAD_MAX * 2)
							{
								pipe.LastLinesHistoryList.RemoveAt(0);
							}

							string textLine = this._logFileReader.GetLogLine(line);
							bool fileOk = pipe.WriteToPipe(textLine, line);
							if (!fileOk)
							{
								deleteList.Add(pipe);
							}
						}
						pipe.CloseFile();
					}
				}
			}
			foreach (FilterPipe pipe in deleteList)
			{
				this._filterPipeList.Remove(pipe);
			}
		}

		private void CopyMarkedLinesToClipboard()
		{
			if (this._guiStateArgs.CellSelectMode)
			{
				DataObject data = this.dataGridView.GetClipboardContent();
				Clipboard.SetDataObject(data);
			}
			else
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in this.dataGridView.SelectedRows)
				{
					if (row.Index != -1)
					{
						lineNumList.Add(row.Index);
					}
				}
				lineNumList.Sort();
				StringBuilder clipText = new StringBuilder();
				LogExpertCallback callback = new LogExpertCallback(this);
				foreach (int lineNum in lineNumList)
				{
					string line = this._logFileReader.GetLogLine(lineNum);
					if (CurrentColumnizer is ILogLineXmlColumnizer)
					{
						callback.LineNum = lineNum;
						line = (CurrentColumnizer as ILogLineXmlColumnizer).GetLineTextForClipboard(line, callback);
					}
					clipText.AppendLine(line);
				}
				Clipboard.SetText(clipText.ToString());
			}
		}

		/// <summary>
		/// Set an Encoding which shall be used when loading a file. Used before a file is loaded.
		/// </summary>
		/// <param name="encoding"></param>
		private void SetExplicitEncoding(Encoding encoding)
		{
			this.EncodingOptions.Encoding = encoding;
		}

		private void ApplyDataGridViewPrefs(DataGridView dataGridView, Preferences prefs)
		{
			if (dataGridView.Columns.Count > 1)
			{
				if (prefs.setLastColumnWidth)
				{
					dataGridView.Columns[dataGridView.Columns.Count - 1].MinimumWidth = prefs.lastColumnWidth;
				}
				else
				{
					// Workaround for a .NET bug which brings the DataGridView into an unstable state (causing lots of NullReferenceExceptions). 
					dataGridView.FirstDisplayedScrollingColumnIndex = 0;

					dataGridView.Columns[dataGridView.Columns.Count - 1].MinimumWidth = 5;  // default
				}
			}
			if (dataGridView.RowCount > 0)
			{
				dataGridView.UpdateRowHeightInfo(0, true);
			}
			dataGridView.Invalidate();
			dataGridView.Refresh();
			AutoResizeColumns(dataGridView);
		}

		private IList<int> GetSelectedContent()
		{
			if (this.dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect)
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in this.dataGridView.SelectedRows)
				{
					if (row.Index != -1)
					{
						lineNumList.Add(row.Index);
					}
				}
				lineNumList.Sort();
				return lineNumList;
			}
			return new List<int>();
		}

		private void AdjustHighlightSplitterWidth()
		{
			//int size = this.editHighlightsSplitContainer.Panel2Collapsed ? 600 : 660;
			//int distance = this.highlightSplitContainer.Width - size;
			//if (distance < 10)
			//  distance = 10;
			//this.highlightSplitContainer.SplitterDistance = distance;
		}

		private void BookmarkComment(Bookmark bookmark)
		{
			BookmarkCommentDlg dlg = new BookmarkCommentDlg();
			dlg.Comment = bookmark.Text;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				bookmark.Text = dlg.Comment;
				this.dataGridView.Refresh();
				OnBookmarkTextChanged(bookmark);
			}
		}

		/// <summary>
		/// Indicates which columns we are filtering on
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		private string CalculateColumnNames(FilterParams filter)
		{
			string names = string.Empty;

			if (filter.columnRestrict)
			{
				foreach (int colIndex in filter.columnList)
				{
					if (colIndex < this.dataGridView.Columns.Count - 2)
					{
						if (names.Length > 0)
						{
							names += ", ";
						}
						names += this.dataGridView.Columns[2 + colIndex].HeaderText; // skip first two columns: marker + line number
					}
				}
			}

			return names;
		}

		private void ApplyFrozenState(DataGridView gridView)
		{
			SortedDictionary<int, DataGridViewColumn> dict = new SortedDictionary<int, DataGridViewColumn>();
			foreach (DataGridViewColumn col in gridView.Columns)
			{
				dict.Add(col.DisplayIndex, col);
			}
			foreach (DataGridViewColumn col in dict.Values)
			{
				col.Frozen = this._freezeStateMap.ContainsKey(gridView) && this._freezeStateMap[gridView];
				if (col.Index == this._selectedCol)
				{
					break;
				}
			}
		}

		private void ShowTimeSpread(bool show)
		{
			if (show)
			{
				this.tableLayoutPanel1.ColumnStyles[1].Width = 16;
			}
			else
			{
				this.tableLayoutPanel1.ColumnStyles[1].Width = 0;
			}
			this._timeSpreadCalc.Enabled = show;
		}

		private void AddTempFileTab(string fileName, string title)
		{
			this._parentLogTabWin.AddTempFileTab(fileName, title);
		}

#if DEBUG
		internal void DumpBufferInfo()
		{
			int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
			this._logFileReader.LogBufferInfoForLine(currentLineNum);
		}

		internal void DumpBufferDiagnostic()
		{
			this._logFileReader.LogBufferDiagnostic();
		}

#endif

		private void AddSearchHitHighlightEntry(SearchParams para)
		{
			HilightEntry he = new HilightEntry(para.searchText,
				Color.Red, Color.Yellow,
				para.isRegex,
				para.isCaseSensitive,
				false,
				false,
				false,
				false,
				null,
				true);
			he.IsSearchHit = true;
			lock (this._tempHilightEntryListLock)
			{
				this._tempHilightEntryList.Add(he);
			}
			RefreshAllGrids();
		}

		private void RemoveAllSearchHighlightEntries()
		{
			lock (this._tempHilightEntryListLock)
			{
				List<HilightEntry> newList = new List<HilightEntry>();
				foreach (HilightEntry he in this._tempHilightEntryList)
				{
					if (!he.IsSearchHit)
					{
						newList.Add(he);
					}
				}
				this._tempHilightEntryList = newList;
			}
			RefreshAllGrids();
		}

		internal void ChangeMultifileMask()
		{
			MultiFileMaskDialog dlg = new MultiFileMaskDialog(this, this.FileName);
			dlg.Owner = this;
			dlg.MaxDays = this._multifileOptions.MaxDayTry;
			dlg.FileNamePattern = this._multifileOptions.FormatPattern;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				this._multifileOptions.FormatPattern = dlg.FileNamePattern;
				this._multifileOptions.MaxDayTry = dlg.MaxDays;
				if (this.IsMultiFile)
				{
					Reload();
				}
			}
		}

		internal void ToggleColumnFinder(bool show, bool setFocus)
		{
			this._guiStateArgs.ColumnFinderVisible = show;
			if (show)
			{
				this.columnComboBox.AutoCompleteMode = AutoCompleteMode.Suggest;
				this.columnComboBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
				this.columnComboBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
				this.columnComboBox.AutoCompleteCustomSource.AddRange(this.CurrentColumnizer.GetColumnNames());
				if (setFocus)
				{
					this.columnComboBox.Focus();
				}
			}
			else
			{
				this.dataGridView.Focus();
			}
			this.tableLayoutPanel1.RowStyles[0].Height = show ? 28 : 0;
		}

		private DataGridViewColumn GetColumnByName(DataGridView dataGridView, string name)
		{
			foreach (DataGridViewColumn col in dataGridView.Columns)
			{
				if (col.HeaderText.Equals(name))
				{
					return col;
				}
			}
			return null;
		}

		private void SelectColumn()
		{
			string colName = this.columnComboBox.SelectedItem as string;
			DataGridViewColumn col = GetColumnByName(this.dataGridView, colName);
			if (col != null && !col.Frozen)
			{
				this.dataGridView.FirstDisplayedScrollingColumnIndex = col.Index;
				int currentLine = this.dataGridView.CurrentCellAddress.Y;
				if (currentLine >= 0)
				{
					this.dataGridView.CurrentCell =
						this.dataGridView.Rows[this.dataGridView.CurrentCellAddress.Y].Cells[col.Index];
				}
			}
		}

		private void InitPatternWindow()
		{
			this._patternWindow = new PatternWindow(this);
			this._patternWindow.SetColumnizer(this.CurrentColumnizer);
			this._patternWindow.SetFont(this.Preferences.fontName, this.Preferences.fontSize);
			this._patternWindow.Fuzzy = this._patternArgs.fuzzy;
			this._patternWindow.MaxDiff = this._patternArgs.maxDiffInBlock;
			this._patternWindow.MaxMisses = this._patternArgs.maxMisses;
			this._patternWindow.Weight = this._patternArgs.minWeight;
		}

		private void UpdateBookmarkGui()
		{
			this.dataGridView.Refresh();
			this.filterGridView.Refresh();
		}

		private void ChangeRowHeight(bool decrease)
		{
			int rowNum = this.dataGridView.CurrentCellAddress.Y;
			if (rowNum < 0 || rowNum >= this.dataGridView.RowCount)
			{
				return;
			}
			if (decrease)
			{
				if (!this._rowHeightList.ContainsKey(rowNum))
				{
					return;
				}
				else
				{
					RowHeightEntry entry = this._rowHeightList[rowNum];
					entry.Height = entry.Height - this._lineHeight;
					if (entry.Height <= this._lineHeight)
					{
						this._rowHeightList.Remove(rowNum);
					}
				}
			}
			else
			{
				RowHeightEntry entry;
				if (!this._rowHeightList.ContainsKey(rowNum))
				{
					entry = new RowHeightEntry();
					entry.LineNum = rowNum;
					entry.Height = this._lineHeight;
					this._rowHeightList[rowNum] = entry;
				}
				else
				{
					entry = this._rowHeightList[rowNum];
				}
				entry.Height = entry.Height + this._lineHeight;
			}
			this.dataGridView.UpdateRowHeightInfo(rowNum, false);
			if (rowNum == this.dataGridView.RowCount - 1 && this._guiStateArgs.FollowTail)
			{
				this.dataGridView.FirstDisplayedScrollingRowIndex = rowNum;
			}
			this.dataGridView.Refresh();
		}

		private int GetRowHeight(int rowNum)
		{
			if (this._rowHeightList.ContainsKey(rowNum))
			{
				return this._rowHeightList[rowNum].Height;
			}
			else
			{
				return this._lineHeight;
			}
		}

		private void AddBookmarkAtLineSilently(int lineNum)
		{
			if (!this._bookmarkProvider.IsBookmarkAtLine(lineNum))
			{
				this._bookmarkProvider.AddBookmark(new Bookmark(lineNum));
			}
		}

		private void AddBookmarkAndEditComment()
		{
			int lineNum = this.dataGridView.CurrentCellAddress.Y;
			if (!this._bookmarkProvider.IsBookmarkAtLine(lineNum))
			{
				ToggleBookmark();
			}
			BookmarkComment(this._bookmarkProvider.GetBookmarkForLine(lineNum));
		}

		private void AddBookmarkComment(string text)
		{
			int lineNum = this.dataGridView.CurrentCellAddress.Y;
			Bookmark bookmark;
			if (!this._bookmarkProvider.IsBookmarkAtLine(lineNum))
			{
				this._bookmarkProvider.AddBookmark(bookmark = new Bookmark(lineNum));
			}
			else
			{
				bookmark = this._bookmarkProvider.GetBookmarkForLine(lineNum);
			}
			bookmark.Text = bookmark.Text + text;
			this.dataGridView.Refresh();
			this.filterGridView.Refresh();
			OnBookmarkTextChanged(bookmark);
		}

		private void MarkCurrentFilterRange()
		{
			this._filterParams.rangeSearchText = this.filterRangeComboBox.Text;
			ColumnizerCallback callback = new ColumnizerCallback(this);
			RangeFinder rangeFinder = new RangeFinder(this._filterParams, callback);
			Range range = rangeFinder.FindRange(this.dataGridView.CurrentCellAddress.Y);
			if (range != null)
			{
				SetCellSelectionMode(false);
				this._noSelectionUpdates = true;
				for (int i = range.StartLine; i <= range.EndLine; ++i)
				{
					this.dataGridView.Rows[i].Selected = true;
				}
				this._noSelectionUpdates = false;
				UpdateSelectionDisplay();
			}
		}

		private void RemoveTempHighlights()
		{
			lock (this._tempHilightEntryListLock)
			{
				this._tempHilightEntryList.Clear();
			}
			RefreshAllGrids();
		}

		private void ToggleHighlightPanel(bool open)
		{
			this.highlightSplitContainer.Panel2Collapsed = !open;
			this.toggleHighlightPanelButton.Image = (open ? this._panelCloseButtonImage : this._panelOpenButtonImage);
		}

		private void SetBoomarksForSelectedFilterLines()
		{
			lock (this._filterResultList)
			{
				foreach (DataGridViewRow row in this.filterGridView.SelectedRows)
				{
					int lineNum = this._filterResultList[row.Index];
					AddBookmarkAtLineSilently(lineNum);
				}
			}
			this.dataGridView.Refresh();
			this.filterGridView.Refresh();
			OnBookmarkAdded();
		}

		private void SetDefaultHighlightGroup()
		{
			HilightGroup group = this._parentLogTabWin.FindHighlightGroupByFileMask(this.FileName);
			if (group != null)
			{
				SetCurrentHighlightGroup(group.GroupName);
			}
			else
			{
				SetCurrentHighlightGroup("[Default]");
			}
		}

		private void HandleChangedFilterOnLoadSetting()
		{
			this._parentLogTabWin.Preferences.isFilterOnLoad = this.filterOnLoadCheckBox.Checked;
			this._parentLogTabWin.Preferences.isAutoHideFilterList = this.hideFilterListOnLoadCheckBox.Checked;
			OnFilterListChanged(this);
		}

		private void RegisterCancelHandler(BackgroundProcessCancelHandler handler)
		{
			lock (this._cancelHandlerList)
			{
				this._cancelHandlerList.Add(handler);
			}
		}

		private void DeRegisterCancelHandler(BackgroundProcessCancelHandler handler)
		{
			lock (this._cancelHandlerList)
			{
				this._cancelHandlerList.Remove(handler);
			}
		}

		private void FireCancelHandlers()
		{
			lock (this._cancelHandlerList)
			{
				foreach (BackgroundProcessCancelHandler handler in this._cancelHandlerList)
				{
					handler.EscapePressed();
				}
			}
		}

		private void SyncOtherWindows(DateTime timestamp)
		{
			lock (this._timeSyncListLock)
			{
				if (this._timeSyncList != null)
				{
					this._timeSyncList.NavigateToTimestamp(timestamp, this);
				}
			}
		}

		private void AddSlaveToTimesync(LogWindow slave)
		{
			lock (this._timeSyncListLock)
			{
				if (this._timeSyncList == null)
				{
					if (slave.TimeSyncList == null)
					{
						this._timeSyncList = new TimeSyncList();
						this._timeSyncList.AddWindow(this);
					}
					else
					{
						this._timeSyncList = slave.TimeSyncList;
					}
					int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
					int refLine = currentLineNum;
					DateTime timeStamp = GetTimestampForLine(ref refLine, true);
					if (!timeStamp.Equals(DateTime.MinValue) && !this._shouldTimestampDisplaySyncingCancel)
					{
						this._timeSyncList.CurrentTimestamp = timeStamp;
					}
					this._timeSyncList.WindowRemoved += TimeSyncList_WindowRemoved;
				}
			}
			slave.AddToTimeSync(this);
			OnSyncModeChanged();
		}

		private void FreeSlaveFromTimesync(LogWindow slave)
		{
			slave.FreeFromTimeSync();
		}

		private string[] GetColumnsForLine(int lineNumber)
		{
			return this._columnCache.GetColumnsForLine(this._logFileReader, lineNumber, this.CurrentColumnizer, this.ColumnizerCallbackObject);
		}

		protected override string GetPersistString()
		{
			return "LogWindow#" + FileName;
		}

		#endregion

		#region Events there delegates an methods

		public delegate void FileSizeChangedEventHandler(object sender, LogEventArgs e);

		public event FileSizeChangedEventHandler FileSizeChanged;

		private void OnFileSizeChanged(LogEventArgs e)
		{
			if (FileSizeChanged != null)
			{
				FileSizeChanged(this, e);
			}
		}

		public delegate void ProgressBarEventHandler(object sender, ProgressEventArgs e);

		public event ProgressBarEventHandler ProgressBarUpdate;

		protected void OnProgressBarUpdate(ProgressEventArgs e)
		{
			ProgressBarEventHandler handler = ProgressBarUpdate;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public delegate void StatusLineEventHandler(object sender, StatusLineEventArgs e);

		public event StatusLineEventHandler StatusLineEvent;

		protected void OnStatusLine(StatusLineEventArgs e)
		{
			StatusLineEventHandler handler = StatusLineEvent;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public delegate void GuiStateEventHandler(object sender, GuiStateArgs e);

		public event GuiStateEventHandler GuiStateUpdate;

		protected void OnGuiState(GuiStateArgs e)
		{
			GuiStateEventHandler handler = GuiStateUpdate;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public delegate void TailFollowedEventHandler(object sender, EventArgs e);

		public event TailFollowedEventHandler TailFollowed;

		protected void OnTailFollowed(EventArgs e)
		{
			if (TailFollowed != null)
			{
				TailFollowed(this, e);
			}
		}

		public delegate void FileNotFoundEventHandler(object sender, EventArgs e);

		public event FileNotFoundEventHandler FileNotFound;

		protected void OnFileNotFound(EventArgs e)
		{
			if (FileNotFound != null)
			{
				FileNotFound(this, e);
			}
		}

		public delegate void FileRespawnedEventHandler(object sender, EventArgs e);

		public event FileRespawnedEventHandler FileRespawned;

		protected void OnFileRespawned(EventArgs e)
		{
			if (FileRespawned != null)
			{
				FileRespawned(this, e);
			}
		}

		public delegate void FilterListChangedEventHandler(object sender, FilterListChangedEventArgs e);

		public event FilterListChangedEventHandler FilterListChanged;

		protected void OnFilterListChanged(LogWindow source)
		{
			if (this.FilterListChanged != null)
			{
				this.FilterListChanged(this, new FilterListChangedEventArgs(source));
			}
		}

		public delegate void CurrentHighlightGroupChangedEventHandler(object sender, CurrentHighlightGroupChangedEventArgs e);

		public event CurrentHighlightGroupChangedEventHandler CurrentHighlightGroupChanged;

		protected void OnCurrentHighlightListChanged()
		{
			if (this.CurrentHighlightGroupChanged != null)
			{
				this.CurrentHighlightGroupChanged(this, new CurrentHighlightGroupChangedEventArgs(this, this._currentHighlightGroup));
			}
		}

		public delegate void FileReloadFinishedEventHandler(object sender, EventArgs e);

		public event FileReloadFinishedEventHandler FileReloadFinished;

		protected void OnFileReloadFinished()
		{
			if (FileReloadFinished != null)
			{
				FileReloadFinished(this, new EventArgs());
			}
		}

		public delegate void BookmarkAddedEventHandler(object sender, EventArgs e);

		public event BookmarkAddedEventHandler BookmarkAdded;

		protected void OnBookmarkAdded()
		{
			if (BookmarkAdded != null)
			{
				BookmarkAdded(this, new EventArgs());
			}
		}

		public delegate void BookmarkRemovedEventHandler(object sender, EventArgs e);

		public event BookmarkRemovedEventHandler BookmarkRemoved;

		protected void OnBookmarkRemoved()
		{
			if (BookmarkRemoved != null)
			{
				BookmarkRemoved(this, new EventArgs());
			}
		}

		public delegate void AllBookmarksRemovedEventHandler(object sender, EventArgs e);

		public event AllBookmarksRemovedEventHandler AllBookmarksRemoved;

		protected void OnAllBookmarksRemoved()
		{
			if (AllBookmarksRemoved != null)
			{
				AllBookmarksRemoved(this, new EventArgs());
			}
		}

		public delegate void BookmarkTextChangedEventHandler(object sender, BookmarkEventArgs e);

		public event BookmarkTextChangedEventHandler BookmarkTextChanged;

		protected void OnBookmarkTextChanged(Bookmark bookmark)
		{
			if (BookmarkTextChanged != null)
			{
				BookmarkTextChanged(this, new BookmarkEventArgs(bookmark));
			}
		}

		public delegate void ColumnizerChangedEventHandler(object sender, ColumnizerEventArgs e);

		public event ColumnizerChangedEventHandler ColumnizerChanged;

		private void OnColumnizerChanged(ILogLineColumnizer columnizer)
		{
			if (ColumnizerChanged != null)
			{
				ColumnizerChanged(this, new ColumnizerEventArgs(columnizer));
			}
		}

		public delegate void SyncModeChangedEventHandler(object sender, SyncModeEventArgs e);

		public event SyncModeChangedEventHandler SyncModeChanged;

		private void OnSyncModeChanged()
		{
			if (SyncModeChanged != null)
			{
				SyncModeChanged(this, new SyncModeEventArgs(this.IsTimeSynced));
			}
		}

		#endregion

		#region Internals

		internal void RefreshAllGrids()
		{
			this.dataGridView.Refresh();
			this.filterGridView.Refresh();
		}

		#endregion

		#region ILogPaintContext Member

		public string GetLogLine(int lineNum)
		{
			return this._logFileReader.GetLogLine(lineNum);
		}

		public Bookmark GetBookmarkForLine(int lineNum)
		{
			return this._bookmarkProvider.GetBookmarkForLine(lineNum);
		}

		public Font MonospacedFont
		{
			get
			{
				return this._fontMonospaced;
			}
		}

		public Font NormalFont
		{
			get
			{
				return this._normalFont;
			}
		}

		public Font BoldFont
		{
			get
			{
				return this._fontBold;
			}
		}

		#endregion

		#region ILogWindowSerach Members
		public PatternArgs PatternArgs
		{
			get
			{
				return _patternArgs;
			}
			set
			{
				_patternArgs = value;
			}
		}

		public ProgressEventArgs ProgressEventArgs
		{
			get
			{
				return _progressEventArgs;
			}
		}

		public bool IsSearching
		{
			get
			{
				return _isSearching;
			}
			set
			{
				_isSearching = value;
			}
		}

		public bool ShouldCancel
		{
			get
			{
				return _shouldCancel;
			}
			set
			{
				_shouldCancel = value;
			}
		}

		void ILogWindowSearch.SendProgressBarUpdate()
		{
			SendProgressBarUpdate();
		}

		void ILogWindowSearch.UpdateProgressBar(int value)
		{
			UpdateProgressBar(value);
		}

		public PatternWindow PatternWindow
		{
			get
			{
				return _patternWindow;
			}
		}

		public BufferedDataGridView DataGridView
		{
			get
			{
				return dataGridView;
			}
		}

		public ILogLineColumnizer CurrentColumnizer
		{
			get
			{
				return this._currentColumnizer;
			}
			set
			{
				lock (this._currentColumnizerLock)
				{
					this._currentColumnizer = value;
					Logger.logDebug("Setting columnizer " + this._currentColumnizer != null ? this._currentColumnizer.GetName() : "<none>");
				}
			}
		}

		public int LineCount
		{
			get
			{
				return _logFileReader.LineCount;
			}
		}

		public LogWindow CurrentLogWindows
		{
			get
			{
				return this;
			}
		}
		#endregion


		#region ILogView Member

		public void RefreshLogView()
		{
			this.RefreshAllGrids();
		}

		#endregion

		#region Nested Classes

		// =================== ILogLineColumnizerCallback ============================

		public class ColumnizerCallback : ILogLineColumnizerCallback
		{
			protected LogWindow logWindow;

			public int LineNum { get; set; }

			public ColumnizerCallback(LogWindow logWindow)
			{
				this.logWindow = logWindow;
			}

			private ColumnizerCallback(ColumnizerCallback original)
			{
				this.logWindow = original.logWindow;
				this.LineNum = original.LineNum;
			}

			public ColumnizerCallback createCopy()
			{
				return new ColumnizerCallback(this);
			}

			public int GetLineNum()
			{
				return LineNum;
			}

			public string GetFileName()
			{
				return this.logWindow.GetCurrentFileName(LineNum);
			}

			public string GetLogLine(int lineNum)
			{
				return this.logWindow.GetLine(lineNum);
			}

			public int GetLineCount()
			{
				return this.logWindow._logFileReader.LineCount;
			}
		}

		public class LogExpertCallback : ColumnizerCallback, ILogExpertCallback
		{
			public LogExpertCallback(LogWindow logWindow)
				: base(logWindow)
			{
			}

			#region ILogExpertCallback Member

			public void AddTempFileTab(string fileName, string title)
			{
				this.logWindow.AddTempFileTab(fileName, title);
			}

			public void AddPipedTab(IList<LineEntry> lineEntryList, string title)
			{
				this.logWindow.WritePipeTab(lineEntryList, title);
			}

			public string GetTabTitle()
			{
				return this.logWindow.Text;
			}

			#endregion
		}

		#endregion
	}
}