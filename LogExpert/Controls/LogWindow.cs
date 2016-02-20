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
using System.Linq;

namespace LogExpert
{
	public partial class LogWindow : Controls.BaseLogWindow, ILogPaintContext, ILogView, ILogWindowSearch
	{
		#region Const

		private const int MAX_HISTORY = 30;
		private const int MAX_COLUMNIZER_HISTORY = 40;
		private const int SPREAD_MAX = 99;
		private const int PROGRESS_BAR_MODULO = 1000;
		private const int FILTER_ADCANCED_SPLITTER_DISTANCE = 54;

		#endregion Const

		#region Fields

		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private Classes.FuzzyBlockDetection _fuzzyBlockDetection = new Classes.FuzzyBlockDetection();

		private List<HilightEntry> _tempHilightEntryList = new List<HilightEntry>();
		private readonly Object _tempHilightEntryListLock = new Object();
		private HilightGroup _currentHighlightGroup = new HilightGroup();
		private readonly Object _currentHighlightGroupLock = new Object();
		private FilterParams _filterParams = new FilterParams();
		private SearchParams _currentSearchParams = null;
		private List<int> _filterResultList = new List<int>();
		private List<int> _lastFilterLinesList = new List<int>();
		private List<int> _filterHitList = new List<int>();
		private readonly Object _bookmarkLock = new Object();

		private int _filterPipeNameCounter = 0;
		private readonly Dictionary<Control, bool> _freezeStateMap = new Dictionary<Control, bool>();

		private EventWaitHandle _filterUpdateEvent = new ManualResetEvent(false);

		private DelayedTrigger _selectionChangedTrigger = new DelayedTrigger(200);

		private IList<BackgroundProcessCancelHandler> _cancelHandlerList = new List<BackgroundProcessCancelHandler>();

		private bool _waitingForClose = false;
		private bool _isSearching = false;

		private bool _showAdvanced = false;
		private bool _isErrorShowing = false;
		private bool _isTimestampDisplaySyncing = false;
		private bool _shouldTimestampDisplaySyncingCancel = false;
		private bool _noSelectionUpdates = false;
		private bool _shouldCallTimeSync = false;

		private int _lineHeight = 0;
		private int _selectedCol = 0;    // set by context menu event for column headers only

		private readonly Thread _timeshiftSyncThread = null;
		private readonly EventWaitHandle _timeshiftSyncWakeupEvent = new ManualResetEvent(false);
		private readonly EventWaitHandle _timeshiftSyncTimerEvent = new ManualResetEvent(false);
		private int _timeshiftSyncLine = 0;

		private PatternWindow _patternWindow;
		private PatternArgs _patternArgs = new PatternArgs();

		private Image _advancedButtonImage;
		private Image _searchButtonImage;

		private Image _panelOpenButtonImage;
		private Image _panelCloseButtonImage;

		private Object _timeSyncListLock = new Object();

		private Font _normalFont;
		private Font _fontBold;
		private Font _fontMonospaced;

		private Action<int, bool> _selectLineAction;

		private Action _filterSearch;

		#endregion Fields

		#region cTor

		public LogWindow(LogTabWindow parent, string fileName, bool isTempFile, bool forcePersistenceLoading) :
			base()
		{
			_filterSearch = new Action(FilterSearch);

			BookmarkColor = Color.FromArgb(165, 200, 225);
			TempTitleName = "";
			SuspendLayout();
			_selectLineAction = new Action<int, bool>(SelectLine);

			InitializeComponent();

			columnNamesLabel.Text = ""; // no filtering on columns by default

			_parentLogTabWin = parent;
			IsTempFile = isTempFile;
			ColumnizerCallbackObject = new ColumnizerCallback(this);

			FileName = fileName;
			ForcePersistenceLoading = forcePersistenceLoading;

			dataGridView.CellValueNeeded += DataGridView_CellValueNeeded;
			dataGridView.CellPainting += DataGridView_CellPainting;

			filterGridView.CellValueNeeded += FilterGridView_CellValueNeeded;
			filterGridView.CellPainting += FilterGridView_CellPainting;

			Closing += LogWindow_Closing;
			Disposed += LogWindow_Disposed;

			_timeSpreadCalc = new TimeSpreadCalculator(this);
			timeSpreadingControl1.TimeSpreadCalc = _timeSpreadCalc;
			timeSpreadingControl1.LineSelected += TimeSpreadingControl1_LineSelected;
			tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Absolute;
			tableLayoutPanel1.ColumnStyles[1].Width = 20;
			tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Percent;
			tableLayoutPanel1.ColumnStyles[0].Width = 100;

			_parentLogTabWin.HighlightSettingsChanged += Parent_HighlightSettingsChanged;

			SetColumnizer(PluginRegistry.GetInstance().RegisteredColumnizers[0]);

			_patternArgs.maxMisses = 5;
			_patternArgs.minWeight = 1;
			_patternArgs.maxDiffInBlock = 5;
			_patternArgs.fuzzy = 5;

			_filterParams = new FilterParams();
			foreach (string item in ConfigManager.Settings.filterHistoryList)
			{
				filterComboBox.Items.Add(item);
			}
			filterRegexCheckBox.Checked = _filterParams.isRegex;
			filterCaseSensitiveCheckBox.Checked = _filterParams.isCaseSensitive;
			filterTailCheckBox.Checked = _filterParams.isFilterTail;

			splitContainer1.Panel2Collapsed = true;
			advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;

			_timeshiftSyncThread = new Thread(SyncTimestampDisplayWorker);
			_timeshiftSyncThread.IsBackground = true;
			_timeshiftSyncThread.Start();

			_advancedButtonImage = advancedButton.Image;
			_searchButtonImage = filterSearchButton.Image;
			filterSearchButton.Image = null;

			dataGridView.EditModeMenuStrip = editModeContextMenuStrip;
			markEditModeToolStripMenuItem.Enabled = true;

			_panelOpenButtonImage = new Bitmap(GetType(), "Resources.PanelOpen.gif");
			_panelCloseButtonImage = new Bitmap(GetType(), "Resources.PanelClose.gif");

			Settings settings = ConfigManager.Settings;
			if (settings.appBounds != null && settings.appBounds.Right > 0)
			{
				Bounds = settings.appBounds;
			}

			_waitingForClose = false;
			dataGridView.Enabled = false;
			dataGridView.ColumnDividerDoubleClick += DataGridView_ColumnDividerDoubleClick;
			ShowAdvancedFilterPanel(false);
			filterKnobControl1.MinValue = 0;
			filterKnobControl1.MaxValue = SPREAD_MAX;
			filterKnobControl1.ValueChanged += FilterKnobControl1_CheckForFilterDirty;
			filterKnobControl2.MinValue = 0;
			filterKnobControl2.MaxValue = SPREAD_MAX;
			filterKnobControl2.ValueChanged += FilterKnobControl1_CheckForFilterDirty;
			fuzzyKnobControl.MinValue = 0;
			fuzzyKnobControl.MaxValue = 10;
			ToggleHighlightPanel(false); // hidden

			BookmarkProvider.BookmarkAdded += BookmarkProvider_BookmarkChanged;
			BookmarkProvider.BookmarkRemoved += BookmarkProvider_BookmarkChanged;

			ResumeLayout();

			_statusLineTrigger.Signal += StatusLineTrigger_Signal;
			_selectionChangedTrigger.Signal += SelectionChangedTrigger_Signal;

			PreferencesChanged(_parentLogTabWin.Preferences, true, SettingsFlags.GuiOrColors);
		}

		#endregion cTor

		#region Properties

		public Color BookmarkColor { get; set; }

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

		public bool ForcePersistenceLoading { get; set; }

		// file name of given file used for loading (maybe logfile or lxp)
		public string GivenFileName { get; set; }

		public TimeSyncList TimeSyncList { get; private set; }

		public bool IsTimeSynced
		{
			get
			{
				return TimeSyncList != null;
			}
		}

		internal FilterPipe FilterPipe { get; set; }

		public bool IsAdvancedOptionActive
		{
			get
			{
				return (rangeCheckBox.Checked ||
						fuzzyKnobControl.Value > 0 ||
						filterKnobControl1.Value > 0 ||
						filterKnobControl2.Value > 0 ||
						invertFilterCheckBox.Checked ||
						columnRestrictCheckBox.Checked);
			}
		}

		public bool ShowBookmarkBubbles
		{
			get
			{
				return _guiStateArgs.ShowBookmarkBubbles;
			}
			set
			{
				_guiStateArgs.ShowBookmarkBubbles = dataGridView.PaintWithOverlays = value;
				dataGridView.Refresh();
			}
		}

		protected override int CurrentDataGridLine
		{
			get
			{
				if (dataGridView.CurrentCellAddress == null)
				{
					return -1;
				}
				return dataGridView.CurrentCellAddress.Y;
			}
		}

		protected override int CurrentFilterGridLine
		{
			get
			{
				if (filterGridView.CurrentCellAddress == null)
				{
					return -1;
				}
				return filterGridView.CurrentCellAddress.Y;
			}
		}

		#endregion Properties

		#region Public Methods

		public override PersistenceData GetPersistenceData()
		{
			PersistenceData persistenceData = base.GetPersistenceData();

			persistenceData.CurrentLine = CurrentDataGridLine;
			persistenceData.FirstDisplayedLine = dataGridView.FirstDisplayedScrollingRowIndex;
			persistenceData.FilterVisible = !splitContainer1.Panel2Collapsed;
			persistenceData.FilterAdvanced = !advancedFilterSplitContainer.Panel1Collapsed;
			persistenceData.FilterPosition = splitContainer1.SplitterDistance;
			persistenceData.TabName = Text;
			_filterParams.isFilterTail = filterTailCheckBox.Checked; // this option doesnt need a press on 'search'
			if (Preferences.saveFilters)
			{
				List<FilterParams> filterList = new List<FilterParams>();
				filterList.Add(_filterParams);
				persistenceData.FilterParamsList = filterList;

				foreach (FilterPipe filterPipe in _filterPipeList)
				{
					FilterTabData data = new FilterTabData();
					data.persistenceData = filterPipe.OwnLogWindow.GetPersistenceData();
					data.filterParams = filterPipe.FilterParams;
					persistenceData.FilterTabDataList.Add(data);
				}
			}
			if (_currentHighlightGroup != null)
			{
				persistenceData.HighlightGroupName = _currentHighlightGroup.GroupName;
			}
			if (_fileNames != null && IsMultiFile)
			{
				persistenceData.MultiFileNames.AddRange(_fileNames);
			}
			//persistenceData.showBookmarkCommentColumn = bookmarkWindow.ShowBookmarkCommentColumn;
			persistenceData.FilterSaveListVisible = !highlightSplitContainer.Panel2Collapsed;
			return persistenceData;
		}

		public void WaitForLoadingFinished()
		{
			_externaLoadingFinishedEvent.WaitOne();
		}

		public void CloseLogWindow()
		{
			StopTimespreadThread();
			StopTimestampSyncThread();
			StopLogEventWorkerThread();
			_statusLineTrigger.Stop();
			_selectionChangedTrigger.Stop();
			_shouldCancel = true;
			if (CurrentLogFileReader != null)
			{
				UnRegisterLogFileReaderEvents();
				CurrentLogFileReader.StopMonitoringAsync();
			}
			if (_isLoading)
			{
				_waitingForClose = true;
			}
			if (IsTempFile)
			{
				_logger.Info("Deleting temp file " + FileName);
				try
				{
					File.Delete(FileName);
				}
				catch (IOException e)
				{
					_logger.Error(e, "Error while deleting temp file " + FileName);
				}
			}
			if (FilterPipe != null)
			{
				FilterPipe.CloseAndDisconnect();
			}
			DisconnectFilterPipes();
		}

		public void ForceColumnizerForLoading(ILogLineColumnizer columnizer)
		{
			_forcedColumnizerForLoading = Util.CloneColumnizer(columnizer);
		}

		public void ColumnizerConfigChanged()
		{
			SetColumnizerInternal(CurrentColumnizer);
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
			markerColumn.HeaderCell.ContextMenuStrip = columnContextMenuStrip;
			gridView.Columns.Add(markerColumn);

			DataGridViewTextBoxColumn lineNumberColumn = new DataGridViewTextBoxColumn();
			lineNumberColumn.HeaderText = "Line";
			lineNumberColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
			lineNumberColumn.Resizable = DataGridViewTriState.NotSet;
			lineNumberColumn.DividerWidth = 1;
			lineNumberColumn.ReadOnly = true;
			lineNumberColumn.HeaderCell.ContextMenuStrip = columnContextMenuStrip;
			gridView.Columns.Add(lineNumberColumn);

			foreach (string colName in columnizer.GetColumnNames())
			{
				DataGridViewColumn titleColumn = new LogTextColumn();
				titleColumn.HeaderText = colName;
				titleColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
				titleColumn.Resizable = DataGridViewTriState.NotSet;
				titleColumn.DividerWidth = 1;
				titleColumn.HeaderCell.ContextMenuStrip = columnContextMenuStrip;
				gridView.Columns.Add(titleColumn);
			}

			columnNamesLabel.Text = CalculateColumnNames(_filterParams);

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
				return (rowIndex + 1).ToString();   // line number
			}
			if (columnIndex != 0)   // marker column
			{
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
						}
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex);
					//nothing
				}
			}
			return "";
		}

		/**
		 * Returns the first HilightEntry that matches the given line
		 */

		public HilightEntry FindHilightEntry(string line, bool noWordMatches)
		{
			// first check the temp entries
			lock (_tempHilightEntryListLock)
			{
				foreach (HilightEntry entry in _tempHilightEntryList)
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

			lock (_currentHighlightGroupLock)
			{
				foreach (HilightEntry entry in _currentHighlightGroup.HilightEntryList)
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
				lock (_currentHighlightGroupLock)
				{
					GetHighlightEntryMatches(line, _currentHighlightGroup.HilightEntryList, resultList);
				}
				lock (_tempHilightEntryList)
				{
					GetHighlightEntryMatches(line, _tempHilightEntryList, resultList);
				}
			}
			return resultList;
		}

		public void GotoLine(int line)
		{
			if (line >= 0)
			{
				if (line < dataGridView.RowCount)
				{
					SelectLine(line, false);
				}
				else
				{
					SelectLine(dataGridView.RowCount - 1, false);
				}
				dataGridView.Focus();
			}
		}

		public void StartSearch()
		{
			_guiStateArgs.MenuEnabled = false;
			GuiStateUpdate(this, _guiStateArgs);
			SearchParams searchParams = _parentLogTabWin.SearchParams;
			if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
			{
				searchParams.currentLine = CurrentDataGridLine + 1;
			}
			else
			{
				searchParams.currentLine = CurrentDataGridLine - 1;
			}

			_currentSearchParams = searchParams;    // remember for async "not found" messages

			_isSearching = true;
			_shouldCancel = false;

			StartProgressBar(dataGridView.RowCount, "Searching... Press ESC to cancel.");

			Func<SearchParams, int> searchFx = new Func<SearchParams, int>(Search);
			searchFx.BeginInvoke(searchParams, SearchComplete, null);

			RemoveAllSearchHighlightEntries();
			AddSearchHitHighlightEntry(searchParams);
		}

		public override void FollowTailChanged(bool isChecked, bool byTrigger)
		{
			_guiStateArgs.FollowTail = isChecked;

			if (_guiStateArgs.FollowTail && CurrentLogFileReader != null)
			{
				if (dataGridView.RowCount >= CurrentLogFileReader.LineCount && CurrentLogFileReader.LineCount > 0)
				{
					dataGridView.FirstDisplayedScrollingRowIndex = CurrentLogFileReader.LineCount - 1;
				}
			}
			BeginInvoke(new MethodInvoker(dataGridView.Refresh));
			//dataGridView.Refresh();
			_parentLogTabWin.FollowTailChanged(this, isChecked, byTrigger);
			SendGuiStateUpdate();
		}

		public void SelectLogLine(int line)
		{
			Invoke(_selectLineAction, line, true);
		}

		public void SelectAndEnsureVisible(int line, bool triggerSyncCall)
		{
			try
			{
				SelectLine(line, triggerSyncCall, false);

				if (line < dataGridView.FirstDisplayedScrollingRowIndex ||
					line > dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(false))
				{
					dataGridView.FirstDisplayedScrollingRowIndex = line;
					for (int i = 0;
						 i < 8 && dataGridView.FirstDisplayedScrollingRowIndex > 0 &&
						 line < dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(false);
						 ++i)
					{
						dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.FirstDisplayedScrollingRowIndex - 1;
					}
					if (line >= dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(false))
					{
						dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.FirstDisplayedScrollingRowIndex + 1;
					}
				}
				dataGridView.CurrentCell = dataGridView.Rows[line].Cells[0];
			}
			catch (Exception e)
			{
				// In rare situations there seems to be an invalid argument exceptions (or something like this). Concrete location isn't visible in stack
				// trace because use of Invoke(). So catch it, and log (better than crashing the app).
				_logger.Error(e);
			}
		}

		public void AddBookmarkOverlays()
		{
			const int OVERSCAN = 20;
			int firstLine = dataGridView.FirstDisplayedScrollingRowIndex;
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

			for (int i = firstLine; i < dataGridView.RowCount; ++i)
			{
				if (!dataGridView.Rows[i].Displayed && i > dataGridView.FirstDisplayedScrollingRowIndex)
				{
					if (oversizeCount-- < 0)
					{
						break;
					}
				}
				if (BookmarkProvider.IsBookmarkAtLine(i))
				{
					Bookmark bookmark = BookmarkProvider.GetBookmarkForLine(i);
					if (bookmark.Text.Length > 0)
					{
						BookmarkOverlay overlay = bookmark.Overlay;
						overlay.Bookmark = bookmark;

						Rectangle r;
						if (dataGridView.Rows[i].Displayed)
						{
							r = dataGridView.GetCellDisplayRectangle(0, i, false);
						}
						else
						{
							r = dataGridView.GetCellDisplayRectangle(0, dataGridView.FirstDisplayedScrollingRowIndex, false);
							int heightSum = 0;
							if (dataGridView.FirstDisplayedScrollingRowIndex < i)
							{
								for (int rn = dataGridView.FirstDisplayedScrollingRowIndex + 1; rn < i; ++rn)
								{
									heightSum += GetRowHeight(rn);
								}
								r.Offset(0, r.Height + heightSum);
							}
							else
							{
								for (int rn = dataGridView.FirstDisplayedScrollingRowIndex + 1; rn > i; --rn)
								{
									heightSum += GetRowHeight(rn);
								}
								r.Offset(0, -(r.Height + heightSum));
							}
						}

						if (_logger.IsDebugEnabled)
						{
							_logger.Debug("AddBookmarkOverlay() r.Location=" + r.Location.X + ", width=" + r.Width + ", scroll_offset=" + this.dataGridView.HorizontalScrollingOffset);
						}
						overlay.Position = r.Location - new Size(dataGridView.HorizontalScrollingOffset, 0);
						overlay.Position = overlay.Position + new Size(10, r.Height / 2);
						dataGridView.AddOverlay(overlay);
					}
				}
			}
		}

		public void ToggleBookmark()
		{
			int lineNum;

			if (filterGridView.Focused)
			{
				if (CurrentFilterGridLine == -1)
				{
					return;
				}
				lineNum = _filterResultList[CurrentFilterGridLine];
			}
			else
			{
				if (CurrentDataGridLine == -1)
				{
					return;
				}
				lineNum = CurrentDataGridLine;
			}

			ToggleBookmark(lineNum);
		}

		public void SetBookmarkFromTrigger(int lineNum, string comment)
		{
			lock (_bookmarkLock)
			{
				string line = CurrentLogFileReader.GetLogLine(lineNum);
				if (line == null)
				{
					return;
				}
				ParamParser paramParser = new ParamParser(comment);
				try
				{
					comment = paramParser.ReplaceParams(line, lineNum, FileName);
				}
				catch (ArgumentException ex)
				{
					_logger.Error(ex);
					// occurs on invalid regex
				}
				if (BookmarkProvider.IsBookmarkAtLine(lineNum))
				{
					BookmarkProvider.RemoveBookmarkForLine(lineNum);
				}
				BookmarkProvider.AddBookmark(new Bookmark(lineNum, comment));
			}
		}

		public void JumpToNextBookmark(bool isForward)
		{
			int currentBookMarkCount = BookmarkProvider.Bookmarks.Count;
			if (currentBookMarkCount > 0)
			{
				int bookmarkIndex = 0;

				bookmarkIndex = FindNextBookmarkIndex(CurrentDataGridLine, isForward);

				bookmarkIndex = currentBookMarkCount.SanitizeIndex(bookmarkIndex);

				if (filterGridView.Focused)
				{
					int startIndex = bookmarkIndex;
					bool wrapped = false;

					//Search for a bookmarked and visible line
					while (true)
					{
						int bookMarkedLine = BookmarkProvider.Bookmarks[bookmarkIndex].LineNum;
						if (_filterResultList.Contains(bookMarkedLine))
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
					int lineNum = BookmarkProvider.Bookmarks[bookmarkIndex].LineNum;
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
					if (BookmarkProvider.IsBookmarkAtLine(lineNum) && BookmarkProvider.GetBookmarkForLine(lineNum).Text.Length > 0)
					{
						bookmarksPresent = true;
						break;
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
			BookmarkProvider.RemoveBookmarksForLines(lineNumList);
		}

		public void LogWindowActivated()
		{
			if (_guiStateArgs.FollowTail && !_isDeadFile)
			{
				OnTailFollowed(new EventArgs());
			}
			if (Preferences.timestampControl)
			{
				SetTimestampLimits();
				SyncTimestampDisplay();
			}
			dataGridView.Focus();

			SendGuiStateUpdate();
			SendStatusLineUpdate();
			SendProgressBarUpdate();
		}

		public void SetCellSelectionMode(bool isCellMode)
		{
			if (isCellMode)
			{
				dataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
			}
			else
			{
				dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			}
			_guiStateArgs.CellSelectMode = isCellMode;
		}

		public void TimeshiftEnabled(bool isEnabled, string shiftValue)
		{
			_guiStateArgs.TimeshiftEnabled = isEnabled;
			SetTimestampLimits();
			SetTimeshiftValue(shiftValue);
		}

		public void SetTimeshiftValue(string value)
		{
			_guiStateArgs.TimeshiftText = value;
			if (CurrentColumnizer.IsTimeshiftImplemented())
			{
				try
				{
					if (_guiStateArgs.TimeshiftEnabled)
					{
						try
						{
							string text = _guiStateArgs.TimeshiftText;
							if (text.StartsWith("+"))
							{
								text = text.Substring(1);
							}
							TimeSpan timeSpan = TimeSpan.Parse(text);
							int diff = (int)(timeSpan.Ticks / TimeSpan.TicksPerMillisecond);
							CurrentColumnizer.SetTimeOffset(diff);
						}
						catch (Exception ex)
						{
							_logger.Error(ex);
							CurrentColumnizer.SetTimeOffset(0);
						}
					}
					else
						CurrentColumnizer.SetTimeOffset(0);
					RefreshAllGrids();
					if (CurrentColumnizer.IsTimeshiftImplemented())
					{
						SetTimestampLimits();
						SyncTimestampDisplay();
					}
				}
				catch (FormatException ex)
				{
					_logger.Error(ex);
				}
			}
		}

		public void CopyMarkedLinesToTab()
		{
			if (dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect)
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in dataGridView.SelectedRows)
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
				WritePipeToTab(pipe, lineNumList, Text + "->C", null);
			}
			else
			{
				string fileName = Path.GetTempFileName();
				FileStream fStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				StreamWriter writer = new StreamWriter(fStream, Encoding.Unicode);

				DataObject data = dataGridView.GetClipboardContent();
				string text = data.GetText(TextDataFormat.Text);
				writer.Write(text);

				writer.Close();
				string title = Util.GetNameFromPath(FileName) + "->Clip";
				_parentLogTabWin.AddTempFileTab(fileName, title);
			}
		}

		/// <summary>
		/// Change the file encoding. May force a reload if byte count ot preamble lenght differs from previous used encoding.
		/// </summary>
		/// <param name="encoding"></param>
		public void ChangeEncoding(Encoding encoding)
		{
			CurrentLogFileReader.ChangeEncoding(encoding);
			EncodingOptions.Encoding = encoding;
			if (_guiStateArgs.CurrentEncoding.IsSingleByte != encoding.IsSingleByte ||
				_guiStateArgs.CurrentEncoding.GetPreamble().Length != encoding.GetPreamble().Length)
			{
				Reload();
			}
			else
			{
				dataGridView.Refresh();
				SendGuiStateUpdate();
			}
			_guiStateArgs.CurrentEncoding = CurrentLogFileReader.CurrentEncoding;
		}

		public void Reload()
		{
			SavePersistenceData(false);

			_reloadMemento = new ReloadMemento();
			_reloadMemento.CurrentLine = CurrentDataGridLine;
			_reloadMemento.FirstDisplayedLine = dataGridView.FirstDisplayedScrollingRowIndex;
			_forcedColumnizerForLoading = CurrentColumnizer;

			if (_fileNames == null || !IsMultiFile)
			{
				LoadFile(FileName, EncodingOptions);
			}
			else
			{
				LoadFilesAsMulti(_fileNames, EncodingOptions);
			}
		}

		public void PreferencesChanged(Preferences newPreferences, bool isLoadTime, SettingsFlags flags)
		{
			if ((flags & SettingsFlags.GuiOrColors) == SettingsFlags.GuiOrColors)
			{
				_normalFont = new Font(new FontFamily(newPreferences.fontName), newPreferences.fontSize);
				_fontBold = new Font(NormalFont, FontStyle.Bold);
				_fontMonospaced = new Font("Courier New", Preferences.fontSize, FontStyle.Bold);

				dataGridView.DefaultCellStyle.Font = NormalFont;
				filterGridView.DefaultCellStyle.Font = NormalFont;
				_lineHeight = NormalFont.Height + 4;
				dataGridView.RowTemplate.Height = NormalFont.Height + 4;

				ShowBookmarkBubbles = Preferences.showBubbles;

				ApplyDataGridViewPrefs(dataGridView, newPreferences);
				ApplyDataGridViewPrefs(filterGridView, newPreferences);

				if (Preferences.timestampControl)
				{
					SetTimestampLimits();
					SyncTimestampDisplay();
				}
				if (isLoadTime)
				{
					filterTailCheckBox.Checked = Preferences.filterTail;
					syncFilterCheckBox.Checked = Preferences.filterSync;
				}

				_timeSpreadCalc.TimeMode = Preferences.timeSpreadTimeMode;
				timeSpreadingControl1.ForeColor = Preferences.timeSpreadColor;
				timeSpreadingControl1.ReverseAlpha = Preferences.reverseAlpha;
				if (CurrentColumnizer.IsTimeshiftImplemented())
				{
					timeSpreadingControl1.Invoke(new MethodInvoker(timeSpreadingControl1.Refresh));
					ShowTimeSpread(Preferences.showTimeSpread);
				}
				ToggleColumnFinder(Preferences.showColumnFinder, false);
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
			if (InvokeRequired)
			{
				BeginInvoke(new Func<DateTime, bool, bool, bool>(ScrollToTimestampWorker), new object[] { timestamp, roundToSeconds, triggerSyncCall });
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
			if (!CurrentColumnizer.IsTimeshiftImplemented() || dataGridView.RowCount == 0)
			{
				return false;
			}

			int currentLine = CurrentDataGridLine;
			if (currentLine < 0 || currentLine >= dataGridView.RowCount)
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
			int foundLine = FindTimestampLine_Internal(lineNum, 0, dataGridView.RowCount - 1, timestamp, roundToSeconds);
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
			_logger.Debug("FindTimestampLine_Internal(): timestamp=" + timestamp + ", lineNum=" + lineNum + ", rangeStart=" + rangeStart + ", rangeEnd=" + rangeEnd);
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
			lock (_currentColumnizerLock)
			{
				if (!CurrentColumnizer.IsTimeshiftImplemented())
				{
					return DateTime.MinValue;
				}
				_logger.Debug("GetTimestampForLine(" + lineNum + ") enter");
				DateTime timeStamp = DateTime.MinValue;
				bool lookBack = false;
				if (lineNum >= 0 && lineNum < dataGridView.RowCount)
				{
					while (timeStamp.CompareTo(DateTime.MinValue) == 0 && lineNum >= 0)
					{
						if (_isTimestampDisplaySyncing && _shouldTimestampDisplaySyncingCancel)
						{
							return DateTime.MinValue;
						}
						lookBack = true;
						string logLine = CurrentLogFileReader.GetLogLine(lineNum);
						if (logLine == null)
						{
							return DateTime.MinValue;
						}
						ColumnizerCallbackObject.LineNum = lineNum;
						timeStamp = CurrentColumnizer.GetTimestamp(ColumnizerCallbackObject, logLine);
						if (roundToSeconds)
						{
							timeStamp = timeStamp.Subtract(TimeSpan.FromMilliseconds(timeStamp.Millisecond));
						}
						lineNum--;
					}
				}
				if (lookBack)
					lineNum++;
				_logger.Debug("GetTimestampForLine() leave with lineNum=" + lineNum);
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
			lock (_currentColumnizerLock)
			{
				if (!CurrentColumnizer.IsTimeshiftImplemented())
				{
					return DateTime.MinValue;
				}

				DateTime timeStamp = DateTime.MinValue;
				bool lookFwd = false;
				if (lineNum >= 0 && lineNum < dataGridView.RowCount)
				{
					while (timeStamp.CompareTo(DateTime.MinValue) == 0 && lineNum < dataGridView.RowCount)
					{
						lookFwd = true;
						string logLine = CurrentLogFileReader.GetLogLine(lineNum);
						if (logLine == null)
						{
							timeStamp = DateTime.MinValue;
							break;
						}
						timeStamp = CurrentColumnizer.GetTimestamp(ColumnizerCallbackObject, logLine);
						if (roundToSeconds)
						{
							timeStamp = timeStamp.Subtract(TimeSpan.FromMilliseconds(timeStamp.Millisecond));
						}
						lineNum++;
					}
				}
				if (lookFwd)
				{
					lineNum--;
				}
				return timeStamp;
			}
		}

		public void AppFocusLost()
		{
			InvalidateCurrentRow(dataGridView);
		}

		public void AppFocusGained()
		{
			InvalidateCurrentRow(dataGridView);
		}

		public string GetCurrentLine()
		{
			if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.Index != -1)
			{
				return CurrentLogFileReader.GetLogLine(dataGridView.CurrentRow.Index);
			}
			return null;
		}

		public string GetLine(int lineNum)
		{
			if (lineNum < 0 || lineNum >= CurrentLogFileReader.LineCount)
			{
				return null;
			}
			return CurrentLogFileReader.GetLogLine(lineNum);
		}

		public int GetCurrentLineNum()
		{
			if (dataGridView.CurrentRow == null)
			{
				return -1;
			}
			return dataGridView.CurrentRow.Index;
		}

		public int GetRealLineNum()
		{
			int lineNum = GetCurrentLineNum();
			if (lineNum == -1)
			{
				return -1;
			}
			return CurrentLogFileReader.GetRealLineNumForVirtualLineNum(lineNum);
		}

		public string GetCurrentFileName()
		{
			if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.Index != -1)
			{
				return CurrentLogFileReader.GetLogFileNameForLine(dataGridView.CurrentRow.Index);
			}
			return null;
		}

		public ILogFileInfo GetCurrentFileInfo()
		{
			if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.Index != -1)
			{
				return CurrentLogFileReader.GetLogFileInfoForLine(dataGridView.CurrentRow.Index);
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
			return CurrentLogFileReader.GetLogFileNameForLine(lineNum);
		}

		public void ShowLineColumn(bool show)
		{
			dataGridView.Columns[1].Visible = show;
			filterGridView.Columns[1].Visible = show;
		}

		public void PatternStatistic()
		{
			InitPatternWindow();
		}

		public void PatternStatisticSelectRange(PatternArgs patternArgs)
		{
			if (dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect)
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in dataGridView.SelectedRows)
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
				if (CurrentDataGridLine != -1)
				{
					patternArgs.startLine = CurrentDataGridLine;
				}
				else
				{
					patternArgs.startLine = 0;
				}
				patternArgs.endLine = dataGridView.RowCount - 1;
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
			dlg.FileName = Path.GetFileNameWithoutExtension(FileName);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					BookmarkProvider.ExportBookmarkList(FileName, dlg.FileName);
				}
				catch (IOException e)
				{
					_logger.Error(e);
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
			dlg.Filter = "CSV file (*.csv)|*.csv|Bookmark file (*.bmk)|*.bmk";
			dlg.FilterIndex = 1;
			dlg.FileName = Path.GetFileNameWithoutExtension(FileName);

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					BookmarkProvider.ImportBookmarkList(FileName, dlg.FileName);

					RefreshAllGrids();
				}
				catch (IOException e)
				{
					HandleError(e, string.Format("Error while importing bookmark list: {0}", e.Message));
				}
			}
		}

		public void HandleChangedFilterList()
		{
			Invoke(new MethodInvoker(HandleChangedFilterListWorker));
		}

		public void HandleChangedFilterListWorker()
		{
			int index = filterListBox.SelectedIndex;
			filterListBox.Items.Clear();
			foreach (FilterParams filterParam in ConfigManager.Settings.filterList)
			{
				filterListBox.Items.Add(filterParam);
			}
			filterListBox.Refresh();
			if (index >= 0 && index < filterListBox.Items.Count)
			{
				filterListBox.SelectedIndex = index;
			}
			filterOnLoadCheckBox.Checked = Preferences.isFilterOnLoad;
			hideFilterListOnLoadCheckBox.Checked = Preferences.isAutoHideFilterList;
		}

		public override void SetCurrentHighlightGroup(string groupName)
		{
			_guiStateArgs.HighlightGroupName = groupName;
			lock (_currentHighlightGroupLock)
			{
				_currentHighlightGroup = _parentLogTabWin.FindHighlightGroup(groupName);
				if (_currentHighlightGroup == null)
				{
					if (_parentLogTabWin.HilightGroupList.Count > 0)
					{
						_currentHighlightGroup = _parentLogTabWin.HilightGroupList[0];
					}
					else
					{
						_currentHighlightGroup = new HilightGroup();
					}
				}
				_guiStateArgs.HighlightGroupName = _currentHighlightGroup.GroupName;
			}
			SendGuiStateUpdate();
			BeginInvoke(new MethodInvoker(RefreshAllGrids));
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
				if (IsTimeSynced)
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
			_logger.Info("Syncing window for " + Util.GetNameFromPath(FileName) + " to " + Util.GetNameFromPath(master.FileName));
			lock (_timeSyncListLock)
			{
				if (IsTimeSynced && master.TimeSyncList != TimeSyncList)  // already synced but master has different sync list
				{
					FreeFromTimeSync();
				}
				TimeSyncList = master.TimeSyncList;
				TimeSyncList.AddWindow(this);
				ScrollToTimestamp(TimeSyncList.CurrentTimestamp, false, false);
			}
			OnSyncModeChanged();
		}

		public void FreeFromTimeSync()
		{
			lock (_timeSyncListLock)
			{
				if (TimeSyncList != null)
				{
					_logger.Info("De-Syncing window for " + Util.GetNameFromPath(FileName));
					TimeSyncList.WindowRemoved -= TimeSyncList_WindowRemoved;
					TimeSyncList.RemoveWindow(this);
					TimeSyncList = null;
				}
			}
			OnSyncModeChanged();
		}

		public IBookmarkData BookmarkData
		{
			get
			{
				return BookmarkProvider;
			}
		}

		public void AddTempFileTab(string fileName, string title)
		{
			_parentLogTabWin.AddTempFileTab(fileName, title);
		}

		/// <summary>
		/// Used to create a new tab and pipe the given content into it.
		/// </summary>
		/// <param name="lineEntryList"></param>
		public void WritePipeTab(IList<LineEntry> lineEntryList, string title)
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
			Invoke(new Action<FilterPipe, string, PersistenceData>(WriteFilterToTabFinished), new object[] { pipe, title, null });
		}

		#endregion Public Methods

		#region Events

		private void LogWindow_Disposed(object sender, EventArgs e)
		{
			_waitingForClose = true;
			_parentLogTabWin.HighlightSettingsChanged -= Parent_HighlightSettingsChanged;
			if (CurrentLogFileReader != null)
			{
				CurrentLogFileReader.DeleteAllContent();
			}
			FreeFromTimeSync();
		}

		private void UpdateProgress(LoadFileEventArgs e)
		{
			try
			{
				if (e.ReadPos >= e.FileSize)
				{
					//_logger.logWarn("UpdateProgress(): ReadPos (" + e.ReadPos + ") is greater than file size (" + e.FileSize + "). Aborting Update");
					return;
				}

				_statusEventArgs.FileSize = e.ReadPos;
				_progressEventArgs.MaxValue = (int)e.FileSize;
				_progressEventArgs.Value = (int)e.ReadPos;
				SendProgressBarUpdate();
				SendStatusLineUpdate();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "UpdateProgress():");
			}
		}

		#region DataGridView Events

		private void DataGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
		{
			e.Handled = true;
			AutoResizeColumns(dataGridView);
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
			if (!CurrentColumnizer.IsTimeshiftImplemented())
			{
				return;
			}
			string line = CurrentLogFileReader.GetLogLine(e.RowIndex);
			int offset = CurrentColumnizer.GetTimeOffset();
			CurrentColumnizer.SetTimeOffset(0);
			ColumnizerCallbackObject.LineNum = e.RowIndex;
			string[] cols = CurrentColumnizer.SplitLine(ColumnizerCallbackObject, line);
			CurrentColumnizer.SetTimeOffset(offset);
			if (cols.Length <= e.ColumnIndex - 2)
			{
				return;
			}

			string oldValue = cols[e.ColumnIndex - 2];
			string newValue = (string)e.Value;
			CurrentColumnizer.PushValue(ColumnizerCallbackObject, e.ColumnIndex - 2, newValue, oldValue);
			dataGridView.Refresh();
			TimeSpan timeSpan = new TimeSpan(CurrentColumnizer.GetTimeOffset() * TimeSpan.TicksPerMillisecond);
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
			if (dataGridView.CurrentRow != null)
			{
				_statusEventArgs.CurrentLineNum = dataGridView.CurrentRow.Index + 1;
				SendStatusLineUpdate();
				if (syncFilterCheckBox.Checked)
				{
					SyncFilterGridPos();
				}

				if (CurrentColumnizer.IsTimeshiftImplemented() && Preferences.timestampControl)
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
			if (ShowBookmarkBubbles)
			{
				AddBookmarkOverlays();
			}
		}

		private void DataGridView_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
			{
				if (dataGridView.DisplayedRowCount(false) +
					dataGridView.FirstDisplayedScrollingRowIndex >=
					dataGridView.RowCount
				)
				{
					if (!_guiStateArgs.FollowTail)
					{
						FollowTailChanged(true, false);
					}
					OnTailFollowed(new EventArgs());
				}
				else
				{
					if (_guiStateArgs.FollowTail)
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
				filterGridView.Focus();
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.Control)
			{
				//parentLogTabWin.SwitchTab(e.Shift);
			}
			_shouldCallTimeSync = true;
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
			if (dataGridView.CurrentCell != null)
			{
				dataGridView.BeginEdit(false);
			}
		}

		private void DataGridView_InvalidateCurrentRow(object sender, EventArgs e)
		{
			InvalidateCurrentRow(dataGridView);
		}

		private void DataGridView_Resize(object sender, EventArgs e)
		{
			if (CurrentLogFileReader != null && dataGridView.RowCount > 0 &&
				_guiStateArgs.FollowTail)
			{
				dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.RowCount - 1;
			}
		}

		private void DataGridView_SelectionChanged(object sender, EventArgs e)
		{
			UpdateSelectionDisplay();
		}

		private void DataGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
		{
			if (e.RowIndex > 0 && e.RowIndex < dataGridView.RowCount &&
				!dataGridView.Rows[e.RowIndex].Selected)
			{
				SelectLine(e.RowIndex, false);
			}
			if (e.ContextMenuStrip == columnContextMenuStrip)
			{
				_selectedCol = e.ColumnIndex;
			}
		}

		private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			_shouldCallTimeSync = true;
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

		#endregion DataGridView Events

		#region FilterGridView Events

		private void FilterGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || _filterResultList.Count <= e.RowIndex)
			{
				e.Handled = false;
				return;
			}

			int lineNum = _filterResultList[e.RowIndex];
			string line = CurrentLogFileReader.GetLogLineWithWait(lineNum);

			if (line != null)
			{
				DataGridView gridView = (DataGridView)sender;
				HilightEntry entry = FindFirstNoWordMatchHilightEntry(line);

				PaintHelper.CellPaintFilter(this, gridView, e, lineNum, line, entry);
			}
		}

		private void FilterGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || _filterResultList.Count <= e.RowIndex)
			{
				e.Value = "";
				return;
			}

			int lineNum = _filterResultList[e.RowIndex];
			e.Value = GetCellValue(lineNum, e.ColumnIndex);
		}

		private void FilterGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
		{
			e.Height = _lineHeight;
		}

		private void FilterGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
		{
			e.Handled = true;
			Action<DataGridView> fx = AutoResizeColumns;
			BeginInvoke(fx, new object[] { filterGridView });
		}

		private void FilterGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 0)
			{
				ToggleBookmark();
				return;
			}

			if (filterGridView.CurrentRow != null && e.RowIndex >= 0)
			{
				int lineNum = _filterResultList[filterGridView.CurrentRow.Index];
				SelectAndEnsureVisible(lineNum, true);
			}
		}

		private void FilterGridView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (CurrentFilterGridLine >= 0 && CurrentFilterGridLine < _filterResultList.Count)
				{
					int lineNum = _filterResultList[CurrentFilterGridLine];
					SelectLine(lineNum, false);
					e.Handled = true;
				}
			}
			if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.None)
			{
				dataGridView.Focus();
				e.Handled = true;
			}
		}

		private void FilterGridView_InvalidateCurrentRow(object sender, EventArgs e)
		{
			InvalidateCurrentRow(filterGridView);
		}

		private void FilterGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
		{
			if (e.ContextMenuStrip == columnContextMenuStrip)
			{
				_selectedCol = e.ColumnIndex;
			}
		}

		#endregion FilterGridView Events

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

		#endregion EditControl Events

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
			filterRangeComboBox.Enabled = rangeCheckBox.Checked;
			CheckForFilterDirty();
		}

		private void SyncFilterCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (syncFilterCheckBox.Checked)
			{
				SyncFilterGridPos();
			}
		}

		private void SelectionChangedTrigger_Signal(object sender, EventArgs e)
		{
			_logger.Debug("Selection changed trigger");
			int selCount = dataGridView.SelectedRows.Count;
			if (selCount > 1)
			{
				StatusLineText(selCount + " selected lines");
			}
			else
			{
				if (IsMultiFile)
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

		private void Pipe_Disconnected(FilterPipe sender)
		{
			lock (_filterPipeList)
			{
				_filterPipeList.Remove(sender);
				if (_filterPipeList.Count == 0)
				{
					// reset naming counter to 0 if no more open filter tabs for this source window
					_filterPipeNameCounter = 0;
				}
			}
		}

		private void AdvancedButton_Click(object sender, EventArgs e)
		{
			_showAdvanced = !_showAdvanced;
			ShowAdvancedFilterPanel(_showAdvanced);
		}

		private void DataGridContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			int lineNum = -1;
			if (dataGridView.CurrentRow != null)
			{
				lineNum = dataGridView.CurrentRow.Index;
			}
			if (lineNum == -1)
			{
				return;
			}
			int refLineNum = lineNum;

			copyToTabToolStripMenuItem.Enabled = dataGridView.SelectedCells.Count > 0;
			scrollAllTabsToTimestampToolStripMenuItem.Enabled = CurrentColumnizer.IsTimeshiftImplemented() &&
																GetTimestampForLine(ref refLineNum, false) != DateTime.MinValue;
			locateLineInOriginalFileToolStripMenuItem.Enabled = IsTempFile &&
																FilterPipe != null &&
																FilterPipe.GetOriginalLineNum(lineNum) != -1;
			markEditModeToolStripMenuItem.Enabled = !dataGridView.CurrentCell.ReadOnly;

			// Remove all "old" plugin entries
			int index = dataGridContextMenuStrip.Items.IndexOf(pluginSeparator);
			if (index > 0)
			{
				for (int i = index + 1; i < dataGridContextMenuStrip.Items.Count;)
				{
					dataGridContextMenuStrip.Items.RemoveAt(i);
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
					ContextMenuPluginEventArgs evArgs = new ContextMenuPluginEventArgs(entry, lines, CurrentColumnizer, callback);
					EventHandler ev = new EventHandler(HandlePluginContextMenu);
					string menuText = entry.GetMenuText(lines, CurrentColumnizer, callback);
					if (menuText != null)
					{
						bool disabled = menuText.StartsWith("_");
						if (disabled)
						{
							menuText = menuText.Substring(1);
						}
						ToolStripItem item = dataGridContextMenuStrip.Items.Add(menuText, null, ev);
						item.Tag = evArgs;
						item.Enabled = !disabled;
						isAdded = true;
					}
				}
			}
			pluginSeparator.Visible = isAdded;

			// enable/disable Temp Highlight item
			tempHighlightsToolStripMenuItem.Enabled = _tempHilightEntryList.Count > 0;

			markCurrentFilterRangeToolStripMenuItem.Enabled = filterRangeComboBox.Text != null && filterRangeComboBox.Text.Length > 0;

			if (CurrentColumnizer.IsTimeshiftImplemented())
			{
				IList<WindowFileEntry> list = _parentLogTabWin.GetListOfOpenFiles();
				syncTimestampsToToolStripMenuItem.Enabled = true;
				syncTimestampsToToolStripMenuItem.DropDownItems.Clear();
				EventHandler ev = new EventHandler(HandleSyncContextMenu);
				Font italicFont = new Font(syncTimestampsToToolStripMenuItem.Font.FontFamily, syncTimestampsToToolStripMenuItem.Font.Size, FontStyle.Italic);
				foreach (WindowFileEntry fileEntry in list)
				{
					if (fileEntry.LogWindow != this)
					{
						ToolStripMenuItem item = syncTimestampsToToolStripMenuItem.DropDownItems.Add(fileEntry.Title, null, ev) as ToolStripMenuItem;
						item.Tag = fileEntry;
						item.Checked = TimeSyncList != null && TimeSyncList.Contains(fileEntry.LogWindow);
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
				syncTimestampsToToolStripMenuItem.Enabled = false;
			}
			freeThisWindowFromTimeSyncToolStripMenuItem.Enabled = TimeSyncList != null && TimeSyncList.Count > 1;
		}

		private void Copy_Click(object sender, EventArgs e)
		{
			CopyMarkedLinesToClipboard();
		}

		private void ScrollAllTabsToTimestampToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentColumnizer.IsTimeshiftImplemented())
			{
				int currentLine = CurrentDataGridLine;
				if (currentLine > 0 && currentLine < dataGridView.RowCount)
				{
					int lineNum = currentLine;
					DateTime timeStamp = GetTimestampForLine(ref lineNum, false);
					if (timeStamp.Equals(DateTime.MinValue))  // means: invalid
					{
						return;
					}
					_parentLogTabWin.ScrollAllTabsToTimestamp(timeStamp, this);
				}
			}
		}

		private void LocateLineInOriginalFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dataGridView.CurrentRow != null && FilterPipe != null)
			{
				int lineNum = FilterPipe.GetOriginalLineNum(dataGridView.CurrentRow.Index);
				if (lineNum != -1)
				{
					FilterPipe.LogWindow.SelectLine(lineNum, false);
					_parentLogTabWin.SelectTab(FilterPipe.LogWindow);
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

		private void BookmarkWindow_BookmarkCommentChanged(object sender, EventArgs e)
		{
			dataGridView.Refresh();
		}

		private void ColumnRestrictCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			columnButton.Enabled = columnRestrictCheckBox.Checked;
			if (columnRestrictCheckBox.Checked) // disable when nothing to filter
			{
				columnNamesLabel.Visible = true;
				_filterParams.columnRestrict = true;
				columnNamesLabel.Text = CalculateColumnNames(_filterParams);
			}
			else
			{
				columnNamesLabel.Visible = false;
			}
			CheckForFilterDirty();
		}

		private void ColumnButton_Click(object sender, EventArgs e)
		{
			_filterParams.currentColumnizer = _currentColumnizer;
			FilterColumnChooser chooser = new FilterColumnChooser(_filterParams);
			if (chooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				columnNamesLabel.Text = CalculateColumnNames(_filterParams);

				filterSearchButton.Image = _searchButtonImage;
				saveFilterButton.Enabled = false;
			}
		}

		private void ColumnContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			Control ctl = columnContextMenuStrip.SourceControl;
			DataGridView gridView = ctl as DataGridView;
			bool frozen = false;
			if (_freezeStateMap.ContainsKey(ctl))
			{
				frozen = _freezeStateMap[ctl];
			}
			freezeLeftColumnsUntilHereToolStripMenuItem.Checked = frozen;
			if (frozen)
			{
				freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Frozen";
			}
			else
			{
				if (ctl is DataGridView)
				{
					freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Freeze left columns until here (" +
																	   gridView.Columns[_selectedCol].HeaderText + ")";
				}
			}
			DataGridViewColumn col = gridView.Columns[_selectedCol];
			moveLeftToolStripMenuItem.Enabled = (col != null && col.DisplayIndex > 0);
			moveRightToolStripMenuItem.Enabled = (col != null && col.DisplayIndex < gridView.Columns.Count - 1);

			if (gridView.Columns.Count - 1 > _selectedCol)
			{
				DataGridViewColumn colRight = gridView.Columns.GetNextColumn(col, DataGridViewElementStates.None,
					DataGridViewElementStates.None);
				moveRightToolStripMenuItem.Enabled = (colRight != null && colRight.Frozen == col.Frozen);
			}
			if (_selectedCol > 0)
			{
				DataGridViewColumn colLeft = gridView.Columns.GetPreviousColumn(col, DataGridViewElementStates.None,
					DataGridViewElementStates.None);

				moveLeftToolStripMenuItem.Enabled = (colLeft != null && colLeft.Frozen == col.Frozen);
			}
			DataGridViewColumn colLast = gridView.Columns[gridView.Columns.Count - 1];
			moveToLastColumnToolStripMenuItem.Enabled = (colLast != null && colLast.Frozen == col.Frozen);

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
			Control ctl = columnContextMenuStrip.SourceControl;
			bool frozen = false;
			if (_freezeStateMap.ContainsKey(ctl))
			{
				frozen = _freezeStateMap[ctl];
			}
			frozen = !frozen;
			_freezeStateMap[ctl] = frozen;

			if (ctl is DataGridView)
			{
				DataGridView gridView = ctl as DataGridView;
				ApplyFrozenState(gridView);
			}
		}

		private void MoveToLastColumnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[_selectedCol];
			if (col != null)
			{
				col.DisplayIndex = gridView.Columns.Count - 1;
			}
		}

		private void MoveLeftToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[_selectedCol];
			if (col != null && col.DisplayIndex > 0)
			{
				col.DisplayIndex = col.DisplayIndex - 1;
			}
		}

		private void MoveRightToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[_selectedCol];
			if (col != null && col.DisplayIndex < gridView.Columns.Count - 1)
			{
				col.DisplayIndex = col.DisplayIndex + 1;
			}
		}

		private void HideColumnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
			DataGridViewColumn col = gridView.Columns[_selectedCol];
			col.Visible = false;
		}

		private void RestoreColumnsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
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
			advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;
		}

		private void MarkFilterHitsInLogViewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SearchParams p = new SearchParams();
			p.searchText = _filterParams.searchText;
			p.isRegex = _filterParams.isRegex;
			p.isCaseSensitive = _filterParams.isCaseSensitive;
			AddSearchHitHighlightEntry(p);
		}

		private void StatusLineTrigger_Signal(object sender, EventArgs e)
		{
			OnStatusLine(_statusEventArgs);
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
				dataGridView.Focus();
			}
		}

		private void ColumnComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
			{
				columnComboBox.DroppedDown = true;
			}
			if (e.KeyCode == Keys.Enter)
			{
				e.IsInputKey = true;
			}
		}

		private void BookmarkProvider_BookmarkChanged()
		{
			if (!_isLoading)
			{
				RefreshAllGrids();
			}
		}

		private void BookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddBookmarkAndEditComment();
		}

		private void HighlightSelectionInLogFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl =
					dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
				HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow,
					false, true, false, false, false, false, null, false);
				lock (_tempHilightEntryListLock)
				{
					_tempHilightEntryList.Add(he);
				}
				dataGridView.CancelEdit();
				dataGridView.EndEdit();
				RefreshAllGrids();
			}
		}

		private void CopyToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl =
					dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
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
			lock (_tempHilightEntryListLock)
			{
				lock (_currentHighlightGroupLock)
				{
					_currentHighlightGroup.HilightEntryList.AddRange(_tempHilightEntryList);
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
			if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl =
					dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
				splitContainer1.Panel2Collapsed = false;
				ResetFilterControls();
				FilterSearch(ctl.SelectedText);
			}
		}

		private void SetSelectedTextAsBookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl ctl = dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
				int lineNum = CurrentDataGridLine;
				AddBookmarkComment(ctl.SelectedText, lineNum);
			}
		}

		private void FilterRegexCheckBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				RegexHelperDialog dlg = new RegexHelperDialog();
				dlg.Owner = this;
				dlg.CaseSensitive = filterCaseSensitiveCheckBox.Checked;
				dlg.Pattern = filterComboBox.Text;
				DialogResult res = dlg.ShowDialog();
				if (res == DialogResult.OK)
				{
					filterCaseSensitiveCheckBox.Checked = dlg.CaseSensitive;
					filterComboBox.Text = dlg.Pattern;
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
			BeginInvoke(new Action<HighlightEventArgs>(HighlightDoneEventWorker), new object[] { e });
		}

		/// <summary>
		/// Highlights the done event worker.
		/// </summary>
		/// <param name="e">The <see cref="LogExpert.HighlightEventArgs"/> instance containing the event data.</param>
		private void HighlightDoneEventWorker(HighlightEventArgs e)
		{
			if (dataGridView.FirstDisplayedScrollingRowIndex > e.StartLine &&
				dataGridView.FirstDisplayedScrollingRowIndex < e.StartLine + e.Count ||
				dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(true) >
				e.StartLine &&
				dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(true) < e.StartLine + e.Count)
			{
				BeginInvoke(new MethodInvoker(RefreshAllGrids));
			}
		}

		private void ToggleHighlightPanelButton_Click(object sender, EventArgs e)
		{
			ToggleHighlightPanel(highlightSplitContainer.Panel2Collapsed);
		}

		private void SaveFilterButton_Click(object sender, EventArgs e)
		{
			FilterParams newParams = _filterParams.CreateCopy();
			newParams.color = Color.FromKnownColor(KnownColor.Black);
			ConfigManager.Settings.filterList.Add(newParams);
			OnFilterListChanged(this);
		}

		private void DeleteFilterButton_Click(object sender, EventArgs e)
		{
			int index = filterListBox.SelectedIndex;
			if (index >= 0)
			{
				FilterParams filterParams = (FilterParams)filterListBox.Items[index];
				ConfigManager.Settings.filterList.Remove(filterParams);
				OnFilterListChanged(this);
				if (filterListBox.Items.Count > 0)
				{
					filterListBox.SelectedIndex = filterListBox.Items.Count - 1;
				}
			}
		}

		private void FilterUpButton_Click(object sender, EventArgs e)
		{
			int i = filterListBox.SelectedIndex;
			if (i > 0)
			{
				FilterParams filterParams = (FilterParams)filterListBox.Items[i];
				ConfigManager.Settings.filterList.RemoveAt(i);
				i--;
				ConfigManager.Settings.filterList.Insert(i, filterParams);
				OnFilterListChanged(this);
				filterListBox.SelectedIndex = i;
			}
		}

		private void FilterDownButton_Click(object sender, EventArgs e)
		{
			int i = filterListBox.SelectedIndex;
			if (i < filterListBox.Items.Count - 1)
			{
				FilterParams filterParams = (FilterParams)filterListBox.Items[i];
				ConfigManager.Settings.filterList.RemoveAt(i);
				i++;
				ConfigManager.Settings.filterList.Insert(i, filterParams);
				OnFilterListChanged(this);
				filterListBox.SelectedIndex = i;
			}
		}

		private void FilterListBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (filterListBox.SelectedIndex >= 0)
			{
				FilterParams filterParams = (FilterParams)filterListBox.Items[filterListBox.SelectedIndex];
				FilterParams newParams = filterParams.CreateCopy();
				//newParams.historyList = ConfigManager.Settings.filterHistoryList;
				_filterParams = newParams;
				ReInitFilterParams(_filterParams);
				ApplyFilterParams();
				CheckForAdvancedButtonDirty();
				CheckForFilterDirty();
				filterSearchButton.Image = _searchButtonImage;
				saveFilterButton.Enabled = false;
				if (hideFilterListOnLoadCheckBox.Checked)
				{
					ToggleHighlightPanel(false);
				}
				if (filterOnLoadCheckBox.Checked)
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
				FilterParams filterParams = (FilterParams)filterListBox.Items[e.Index];
				Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

				Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? new SolidBrush(filterListBox.BackColor) : new SolidBrush(filterParams.color);

				e.Graphics.DrawString(filterParams.searchText, e.Font, brush,
					new PointF(rectangle.Left, rectangle.Top));
				e.DrawFocusRectangle();
				brush.Dispose();
			}
		}

		// Color for filter list entry
		private void ColorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int i = filterListBox.SelectedIndex;
			if (i < filterListBox.Items.Count && i >= 0)
			{
				FilterParams filterParams = (FilterParams)filterListBox.Items[i];
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
			fuzzyKnobControl.Enabled = !filterRegexCheckBox.Checked;
			fuzzyLabel.Enabled = !filterRegexCheckBox.Checked;
			CheckForFilterDirty();
		}

		private void SetBookmarksOnSelectedLinesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetBoomarksForSelectedFilterLines();
		}

		private void Parent_HighlightSettingsChanged(object sender, EventArgs e)
		{
			string groupName = _guiStateArgs.HighlightGroupName;
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

		private void FilterToTabToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FilterToTab();
		}

		private void TimeSyncList_WindowRemoved(object sender, EventArgs e)
		{
			TimeSyncList syncList = sender as TimeSyncList;
			lock (_timeSyncListLock)
			{
				if (syncList.Count == 0 || syncList.Count == 1 && syncList.Contains(this))
				{
					if (syncList == TimeSyncList)
					{
						TimeSyncList = null;
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

				if (TimeSyncList != null && TimeSyncList.Contains(entry.LogWindow))
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

		#endregion Events

		#region Private Methods

		private void SetTimestampLimits()
		{
			if (!CurrentColumnizer.IsTimeshiftImplemented())
			{
				return;
			}

			int line = 0;
			_guiStateArgs.MinTimestamp = GetTimestampForLineForward(ref line, true);
			line = dataGridView.RowCount - 1;
			_guiStateArgs.MaxTimestamp = GetTimestampForLine(ref line, true);
			SendGuiStateUpdate();
		}

		protected override void LogFileLoadFile(LoadFileEventArgs e)
		{
			if (e.NewFile)
			{
				_logger.Info("File created anew.");

				// File was new created (e.g. rollover)
				_isDeadFile = false;
				UnRegisterLogFileReaderEvents();
				dataGridView.CurrentCellChanged -= DataGridView_CurrentCellChanged;
				BeginInvoke(new Action(ReloadNewFile));
				_logger.Debug("Reloading invoked.");
			}
			else if (_isLoading)
			{
				BeginInvoke(new Action<LoadFileEventArgs>(UpdateProgress), e);
			}

			base.LogFileLoadFile(e);
		}

		protected override void LogFileLoadFinished()
		{
			_logger.Info("Finished loading.");
			_isLoading = false;
			_isDeadFile = false;
			if (!_waitingForClose)
			{
				LoadingFinished();
				LoadPersistenceData();
				SetGuiAfterLoading();
				_loadingFinishedEvent.Set();
				_externaLoadingFinishedEvent.Set();
				_timeSpreadCalc.SetLineCount(CurrentLogFileReader.LineCount);

				if (_reloadMemento != null)
				{
					PositionAfterReload(_reloadMemento);
				}
				if (filterTailCheckBox.Checked)
				{
					_logger.Info("Refreshing filter view because of reload.");
					FilterSearch();
				}

				HandleChangedFilterList();
			}
			_reloadMemento = null;

			base.LogFileLoadFinished();
		}

		protected override void SetColumnizer(ILogLineColumnizer columnizer)
		{
			int timeDiff = 0;
			if (CurrentColumnizer != null && CurrentColumnizer.IsTimeshiftImplemented())
			{
				timeDiff = CurrentColumnizer.GetTimeOffset();
			}

			SetColumnizerInternal(columnizer);

			if (CurrentColumnizer.IsTimeshiftImplemented())
			{
				CurrentColumnizer.SetTimeOffset(timeDiff);
			}
		}

		private void StartProgressBar(int maxValue, string statusMessage)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action<int, string>(StartProgressBar), maxValue, statusMessage);
				return;
			}

			StatusLineText(statusMessage);

			_progressEventArgs.MinValue = 0;
			_progressEventArgs.MaxValue = maxValue;
			_progressEventArgs.Value = 0;
			_progressEventArgs.Visible = true;
			SendProgressBarUpdate();
		}

		private void SetColumnizerInternal(ILogLineColumnizer columnizer)
		{
			_logger.Info("SetColumnizerInternal(): " + columnizer.GetName());

			ILogLineColumnizer oldColumnizer = CurrentColumnizer;
			bool oldColumnizerIsXmlType = CurrentColumnizer is ILogLineXmlColumnizer;
			bool oldColumnizerIsPreProcess = CurrentColumnizer is IPreProcessColumnizer;
			bool mustReload = false;

			// Check if the filtered columns disappeared, if so must refresh the UI
			if (_filterParams.columnRestrict)
			{
				string[] newColumns = columnizer != null ? columnizer.GetColumnNames() : new string[0];
				bool colChanged = false;
				if (dataGridView.ColumnCount - 2 == newColumns.Length) // two first columns are 'marker' and 'line number'
				{
					for (int i = 0; i < newColumns.Length; i++)
					{
						if (dataGridView.Columns[i].HeaderText != newColumns[i])
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
					columnNamesLabel.Text = CalculateColumnNames(_filterParams);
				}
			}

			Type oldColType = _filterParams.currentColumnizer != null ? _filterParams.currentColumnizer.GetType() : null;
			Type newColType = columnizer != null ? columnizer.GetType() : null;
			if (oldColType != newColType && _filterParams.columnRestrict && _filterParams.isFilterTail)
			{
				_filterParams.columnList.Clear();
			}
			if (CurrentColumnizer == null || !CurrentColumnizer.GetType().Equals(columnizer.GetType()))
			{
				CurrentColumnizer = columnizer;
				_freezeStateMap.Clear();
				if (CurrentLogFileReader != null)
				{
					IPreProcessColumnizer preprocessColumnizer = CurrentColumnizer as IPreProcessColumnizer;
					if (preprocessColumnizer != null)
					{
						CurrentLogFileReader.PreProcessColumnizer = preprocessColumnizer;
					}
					else
					{
						CurrentLogFileReader.PreProcessColumnizer = null;
					}
				}
				// always reload when choosing XML columnizers
				if (CurrentLogFileReader != null && CurrentColumnizer is ILogLineXmlColumnizer)
				{
					//forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
					mustReload = true;
				}
				// Reload when choosing no XML columnizer but previous columnizer was XML
				if (CurrentLogFileReader != null && !(CurrentColumnizer is ILogLineXmlColumnizer) && oldColumnizerIsXmlType)
				{
					CurrentLogFileReader.IsXmlMode = false;
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

			if (CurrentLogFileReader != null)
			{
				dataGridView.RowCount = CurrentLogFileReader.LineCount;
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
				columnComboBox.Items.Add(columnName);
			}
			columnComboBox.SelectedIndex = 0;

			OnColumnizerChanged(CurrentColumnizer);
		}

		protected override void UnRegisterLogFileReaderEvents()
		{
			if (CurrentLogFileReader != null)
			{
				CurrentLogFileReader.FileSizeChanged -= FileSizeChangedHandler;
			}
			base.UnRegisterLogFileReaderEvents();
		}

		protected override void LoadPersistenceOptions(PersistenceData persistenceData)
		{
			base.LoadPersistenceOptions(persistenceData);

			splitContainer1.SplitterDistance = persistenceData.FilterPosition;
			splitContainer1.Panel2Collapsed = !persistenceData.FilterVisible;
			ToggleHighlightPanel(persistenceData.FilterSaveListVisible);
			ShowAdvancedFilterPanel(persistenceData.FilterAdvanced);
		}

		private void SetDefaultsFromPrefs()
		{
			filterTailCheckBox.Checked = Preferences.filterTail;
			syncFilterCheckBox.Checked = Preferences.filterSync;
			FollowTailChanged(Preferences.followTail, false);
			_multifileOptions = ObjectClone.Clone<MultifileOptions>(Preferences.multifileOptions);
		}

		private void LoadPersistenceData()
		{
			if (InvokeRequired)
			{
				Invoke(new MethodInvoker(LoadPersistenceData));
				return;
			}

			if (!Preferences.saveSessions && !ForcePersistenceLoading && ForcedPersistenceFileName == null)
			{
				SetDefaultsFromPrefs();
				return;
			}

			if (IsTempFile)
			{
				SetDefaultsFromPrefs();
				return;
			}

			ForcePersistenceLoading = false;  // force only 1 time (while session load)

			try
			{
				PersistenceData persistenceData;
				if (ForcedPersistenceFileName == null)
				{
					persistenceData = Persister.LoadPersistenceData(FileName, Preferences);
				}
				else
				{
					persistenceData = Persister.LoadPersistenceDataFromFixedFile(ForcedPersistenceFileName);
				}

				if (persistenceData.LineCount > CurrentLogFileReader.LineCount)
				{
					// outdated persistence data (logfile rollover)
					// MessageBox.Show(this, "Persistence data for " + FileName + " is outdated. It was discarded.", "Log Expert");
					_logger.Info("Persistence data for " + FileName + " is outdated. It was discarded.");
					LoadPersistenceOptions();
					return;
				}
				BookmarkProvider.BookmarkList = new SortedList<int, Bookmark>();

				foreach (var item in persistenceData.BookmarkList)
				{
					BookmarkProvider.BookmarkList.Add(item.LineNum, item);
				}

				_rowHeightList = new SortedList<int, RowHeightEntry>();

				foreach (var item in persistenceData.RowHeightList)
				{
					_rowHeightList.Add(item.LineNum, item);
				}

				try
				{
					if (persistenceData.CurrentLine >= 0 && persistenceData.CurrentLine < dataGridView.RowCount)
					{
						SelectLine(persistenceData.CurrentLine, false);
					}
					else
					{
						if (CurrentLogFileReader.LineCount > 0)
						{
							dataGridView.FirstDisplayedScrollingRowIndex = CurrentLogFileReader.LineCount - 1;
							SelectLine(CurrentLogFileReader.LineCount - 1, false);
						}
					}
					if (persistenceData.FirstDisplayedLine >= 0 && persistenceData.FirstDisplayedLine < dataGridView.RowCount)
					{
						dataGridView.FirstDisplayedScrollingRowIndex = persistenceData.FirstDisplayedLine;
					}
					if (persistenceData.FollowTail)
					{
						FollowTailChanged(persistenceData.FollowTail, false);
					}
				}
				catch (ArgumentOutOfRangeException exc)
				{
					_logger.Error(exc);
					// FirstDisplayedScrollingRowIndex errechnet manchmal falsche Scroll-Ranges???
				}

				if (Preferences.saveFilters)
				{
					RestoreFilters(persistenceData);
				}
			}
			catch (IOException ex)
			{
				SetDefaultsFromPrefs();
				_logger.Error(ex, "Error loading bookmarks: ");
			}
		}

		private void RestoreFilters(PersistenceData persistenceData)
		{
			if (persistenceData.FilterParamsList.Count > 0)
			{
				_filterParams = persistenceData.FilterParamsList[0];
				ReInitFilterParams(_filterParams);
			}
			ApplyFilterParams();  // re-loaded filter settingss
			BeginInvoke(new MethodInvoker(FilterSearch));
			try
			{
				splitContainer1.SplitterDistance = persistenceData.FilterPosition;
				splitContainer1.Panel2Collapsed = !persistenceData.FilterVisible;
			}
			catch (InvalidOperationException e)
			{
				_logger.Error(e, "Error setting splitter distance:");
			}
			ShowAdvancedFilterPanel(persistenceData.FilterAdvanced);
			if (_filterPipeList.Count == 0)     // don't restore if it's only a reload
			{
				RestoreFilterTabs(persistenceData);
			}
		}

		private void RestoreFilterTabs(PersistenceData persistenceData)
		{
			foreach (FilterTabData data in persistenceData.FilterTabDataList)
			{
				FilterParams persistFilterParams = data.filterParams;
				ReInitFilterParams(persistFilterParams);
				List<int> filterResultList = new List<int>();
				List<int> filterHitList = new List<int>();
				Filter(persistFilterParams, filterResultList, _lastFilterLinesList, filterHitList);
				FilterPipe pipe = new FilterPipe(persistFilterParams.CreateCopy(), this);
				WritePipeToTab(pipe, filterResultList, data.persistenceData.TabName, data.persistenceData);
			}
		}

		private void ReInitFilterParams(FilterParams filterParams)
		{
			filterParams.searchText = filterParams.searchText;   // init "lowerSearchText"
			filterParams.rangeSearchText = filterParams.rangeSearchText;   // init "lowerRangesearchText"
			filterParams.currentColumnizer = CurrentColumnizer;
			if (filterParams.isRegex)
			{
				try
				{
					filterParams.CreateRegex();
				}
				catch (ArgumentException ex)
				{
					_logger.Error(ex);
					StatusLineError("Invalid regular expression");
				}
			}
		}

		protected override void EnterLoadFileStatus()
		{
			base.EnterLoadFileStatus();

			dataGridView.ClearSelection();
			dataGridView.RowCount = 0;
		}

		private void PositionAfterReload(ReloadMemento reloadMemento)
		{
			if (_reloadMemento.CurrentLine < dataGridView.RowCount && _reloadMemento.CurrentLine >= 0)
			{
				dataGridView.CurrentCell = dataGridView.Rows[_reloadMemento.CurrentLine].Cells[0];
			}
			if (_reloadMemento.FirstDisplayedLine < dataGridView.RowCount && _reloadMemento.FirstDisplayedLine >= 0)
			{
				dataGridView.FirstDisplayedScrollingRowIndex = _reloadMemento.FirstDisplayedLine;
			}
		}

		protected override void LogfileDead()
		{
			dataGridView.Enabled = false;
			dataGridView.RowCount = 0;

			base.LogfileDead();

			ClearFilterList();

			StatusLineText("File not found");
		}

		protected override void LogfileRespawned()
		{
			base.LogfileRespawned();

			dataGridView.Enabled = true;
			StatusLineText("");
			OnFileRespawned();
			Reload();
		}

		private void SetGuiAfterLoading()
		{
			if (Text.Length == 0)
			{
				if (IsTempFile)
				{
					Text = TempTitleName;
				}
				else
				{
					Text = Util.GetNameFromPath(FileName);
				}
			}
			ShowBookmarkBubbles = Preferences.showBubbles;

			ILogLineColumnizer columnizer;
			if (_forcedColumnizerForLoading != null)
			{
				columnizer = _forcedColumnizerForLoading;
				_forcedColumnizerForLoading = null;
			}
			else
			{
				columnizer = FindColumnizer();
				if (columnizer != null)
				{
					if (_reloadMemento == null)
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

			Invoke(new Action<ILogLineColumnizer>(SetColumnizer), new object[] { columnizer });
			dataGridView.Enabled = true;
			DisplayCurrentFileOnStatusline();
			_guiStateArgs.MultiFileEnabled = !IsTempFile;
			_guiStateArgs.MenuEnabled = true;
			_guiStateArgs.CurrentEncoding = CurrentLogFileReader.CurrentEncoding;
			SendGuiStateUpdate();

			if (CurrentColumnizer.IsTimeshiftImplemented())
			{
				if (Preferences.timestampControl)
				{
					SetTimestampLimits();
					SyncTimestampDisplay();
				}
				Settings settings = ConfigManager.Settings;
				ShowLineColumn(!settings.hideLineColumn);
			}
			ShowTimeSpread(Preferences.showTimeSpread && CurrentColumnizer.IsTimeshiftImplemented());
			locateLineInOriginalFileToolStripMenuItem.Enabled = FilterPipe != null;
		}

		protected override void ReloadFinishedThreadFx()
		{
			_logger.Info("Waiting for loading to be complete.");
			_loadingFinishedEvent.WaitOne();
			_logger.Info("Refreshing filter view because of reload.");
			Invoke(_filterSearch);
			LoadFilterPipes();
			OnFileReloadFinished();
		}

		private void LoadingFinished()
		{
			_logger.Info("File loading complete.");
			StatusLineText("");
			CurrentLogFileReader.FileSizeChanged += FileSizeChangedHandler;
			_isLoading = false;
			_shouldCancel = false;
			dataGridView.SuspendLayout();
			dataGridView.RowCount = CurrentLogFileReader.LineCount;
			dataGridView.CurrentCellChanged += new EventHandler(DataGridView_CurrentCellChanged);
			dataGridView.Enabled = true;
			dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
			dataGridView.ResumeLayout();
			_progressEventArgs.Visible = false;
			_progressEventArgs.Value = _progressEventArgs.MaxValue;
			SendProgressBarUpdate();

			_guiStateArgs.FollowTail = true;
			SendGuiStateUpdate();
			_statusEventArgs.LineCount = CurrentLogFileReader.LineCount;
			_statusEventArgs.FileSize = CurrentLogFileReader.FileSize;
			SendStatusLineUpdate();

			PreferencesChanged(_parentLogTabWin.Preferences, true, SettingsFlags.All);
		}

		private void FileSizeChangedHandler(object sender, LogEventArgs e)
		{
			_logger.Info("Got FileSizeChanged event. prevLines:" + e.PrevLineCount + ", curr lines: " + e.LineCount);

			lock (_logEventArgsList)
			{
				_logEventArgsList.Add(e);
				_logEventArgsEvent.Set();
			}
		}

		protected override void UpdateGrid(LogEventArgs e)
		{
			int oldRowCount = dataGridView.RowCount;
			int firstDisplayedLine = dataGridView.FirstDisplayedScrollingRowIndex;

			try
			{
				if (dataGridView.RowCount > e.LineCount)
				{
					int currentLineNum = CurrentDataGridLine;
					dataGridView.RowCount = 0;
					dataGridView.RowCount = e.LineCount;
					if (!_guiStateArgs.FollowTail)
					{
						if (currentLineNum >= dataGridView.RowCount)
						{
							currentLineNum = dataGridView.RowCount - 1;
						}
						dataGridView.CurrentCell = dataGridView.Rows[currentLineNum].Cells[0];
					}
				}
				else
				{
					dataGridView.RowCount = e.LineCount;
				}
				_logger.Debug("UpdateGrid(): new RowCount=" + this.dataGridView.RowCount);
				if (e.IsRollover)
				{
					// Multifile rollover
					// keep selection and view range, if no follow tail mode
					if (!_guiStateArgs.FollowTail)
					{
						int currentLineNum = CurrentDataGridLine;
						currentLineNum -= e.RolloverOffset;
						if (currentLineNum < 0)
						{
							currentLineNum = 0;
						}
						_logger.Debug("UpdateGrid(): Rollover=true, Rollover offset=" + e.RolloverOffset + ", currLineNum was " + this.CurrentDataGridLine + ", new currLineNum=" + currentLineNum);
						firstDisplayedLine -= e.RolloverOffset;
						if (firstDisplayedLine < 0)
						{
							firstDisplayedLine = 0;
						}
						dataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedLine;
						dataGridView.CurrentCell = dataGridView.Rows[currentLineNum].Cells[0];
						dataGridView.Rows[currentLineNum].Selected = true;
					}
				}
				_statusEventArgs.LineCount = e.LineCount;
				StatusLineFileSize(e.FileSize);

				if (!_isLoading)
				{
					if (oldRowCount == 0)
					{
						AdjustMinimumGridWith();
					}
				}
				if (_guiStateArgs.FollowTail && dataGridView.RowCount > 0)
				{
					dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.RowCount - 1;
					OnTailFollowed(new EventArgs());
				}
				if (Preferences.timestampControl && !_isLoading)
				{
					SetTimestampLimits();
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Fehler bei UpdateGrid(): ");
			}
		}

		protected override void CheckFilterAndHighlight(LogEventArgs e)
		{
			bool noLed = true;
			bool suppressLed = false;
			bool setBookmark = false;
			bool stopTail = false;
			string bookmarkComment = null;
			if (filterTailCheckBox.Checked || _filterPipeList.Count > 0)
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
					string line = CurrentLogFileReader.GetLogLine(i);
					if (line == null)
					{
						return;
					}
					if (filterTailCheckBox.Checked)
					{
						callback.LineNum = i;
						if (Classes.DamerauLevenshtein.TestFilterCondition(_filterParams, line, callback))
						{
							filterLineAdded = true;
							AddFilterLine(i, false, _filterParams, _filterResultList, _lastFilterLinesList, _filterHitList);
						}
					}
					ProcessFilterPipes(i);

					IList<HilightEntry> matchingList = FindMatchingHilightEntries(line);
					LaunchHighlightPlugins(matchingList, i);
					GetHilightActions(matchingList, out suppressLed, out stopTail, out setBookmark, out bookmarkComment);
					if (setBookmark)
					{
						Action<int, string> fx = new Action<int, string>(SetBookmarkFromTrigger);
						fx.BeginInvoke(i, bookmarkComment, null, null);
					}
					if (stopTail && _guiStateArgs.FollowTail)
					{
						bool wasFollow = _guiStateArgs.FollowTail;
						FollowTailChanged(false, true);
						if (firstStopTail && wasFollow)
						{
							Invoke(new Action<int, bool>(SelectAndEnsureVisible), new object[] { i, false });
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
					string line = CurrentLogFileReader.GetLogLine(i);
					if (line != null)
					{
						IList<HilightEntry> matchingList = FindMatchingHilightEntries(line);
						LaunchHighlightPlugins(matchingList, i);
						GetHilightActions(matchingList, out suppressLed, out stopTail, out setBookmark, out bookmarkComment);
						if (setBookmark)
						{
							Action<int, string> fx = new Action<int, string>(SetBookmarkFromTrigger);
							fx.BeginInvoke(i, bookmarkComment, null, null);
						}
						if (stopTail && _guiStateArgs.FollowTail)
						{
							bool wasFollow = _guiStateArgs.FollowTail;
							FollowTailChanged(false, true);
							if (firstStopTail && wasFollow)
							{
								Invoke(new Action<int, bool>(SelectAndEnsureVisible), new object[] { i, false });
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
						Action<string, string, ILogExpertCallback, ILogLineColumnizer> fx = new Action<string, string, ILogExpertCallback, ILogLineColumnizer>(plugin.Execute);
						fx.BeginInvoke(entry.SearchText, entry.ActionEntry.actionParam, callback, CurrentColumnizer, null, null);
					}
				}
			}
		}

		private void AutoResizeColumns(DataGridView gridView)
		{
			try
			{
				gridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
				if (gridView.Columns.Count > 1 && Preferences.setLastColumnWidth &&
					gridView.Columns[gridView.Columns.Count - 1].Width < Preferences.lastColumnWidth
				)
				{
					// It seems that using 'MinimumWidth' instead of 'Width' prevents the DataGridView's NullReferenceExceptions
					//gridView.Columns[gridView.Columns.Count - 1].Width = Preferences.lastColumnWidth;
					gridView.Columns[gridView.Columns.Count - 1].MinimumWidth = Preferences.lastColumnWidth;
				}
			}
			catch (NullReferenceException e)
			{
				// See https://connect.microsoft.com/VisualStudio/feedback/details/366943/autoresizecolumns-in-datagridview-throws-nullreferenceexception
				// There are some rare situations with null ref exceptions when resizing columns and on filter finished
				// So catch them here. Better than crashing.
				_logger.Error(e, "Error while resizing columns:");
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
				lock (_currentHighlightGroupLock)
				{
					foreach (HilightEntry entry in _currentHighlightGroup.HilightEntryList)
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
			_timeSpreadCalc.Stop();
		}

		private void StopTimestampSyncThread()
		{
			_shouldTimestampDisplaySyncingCancel = true;
			_timeshiftSyncWakeupEvent.Set();
			_timeshiftSyncThread.Abort();
			_timeshiftSyncThread.Join();
		}

		private void SyncTimestampDisplay()
		{
			if (CurrentColumnizer.IsTimeshiftImplemented())
			{
				if (dataGridView.CurrentRow != null)
				{
					SyncTimestampDisplay(dataGridView.CurrentRow.Index);
				}
			}
		}

		private void SyncTimestampDisplay(int lineNum)
		{
			_timeshiftSyncLine = lineNum;
			_timeshiftSyncTimerEvent.Set();
			_timeshiftSyncWakeupEvent.Set();
		}

		private void SyncTimestampDisplayWorker()
		{
			const int WAIT_TIME = 500;
			Thread.CurrentThread.Name = "SyncTimestampDisplayWorker";
			_shouldTimestampDisplaySyncingCancel = false;
			_isTimestampDisplaySyncing = true;

			while (!_shouldTimestampDisplaySyncingCancel)
			{
				_timeshiftSyncWakeupEvent.WaitOne();
				if (_shouldTimestampDisplaySyncingCancel)
				{
					return;
				}
				_timeshiftSyncWakeupEvent.Reset();

				while (!_shouldTimestampDisplaySyncingCancel)
				{
					bool signaled = _timeshiftSyncTimerEvent.WaitOne(WAIT_TIME, true);
					_timeshiftSyncTimerEvent.Reset();
					if (!signaled)
					{
						break;
					}
				}
				// timeout with no new Trigger -> update display
				int lineNum = _timeshiftSyncLine;
				if (lineNum >= 0 && lineNum < dataGridView.RowCount)
				{
					int refLine = lineNum;
					DateTime timeStamp = GetTimestampForLine(ref refLine, true);
					if (!timeStamp.Equals(DateTime.MinValue) && !_shouldTimestampDisplaySyncingCancel)
					{
						_guiStateArgs.Timestamp = timeStamp;
						SendGuiStateUpdate();
						if (_shouldCallTimeSync)
						{
							refLine = lineNum;
							DateTime exactTimeStamp = GetTimestampForLine(ref refLine, false);
							SyncOtherWindows(exactTimeStamp);
							_shouldCallTimeSync = false;
						}
					}
				}
				// show time difference between 2 selected lines
				if (dataGridView.SelectedRows.Count == 2)
				{
					int row1 = dataGridView.SelectedRows[0].Index;
					int row2 = dataGridView.SelectedRows[1].Index;
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
					if (!IsMultiFile && dataGridView.SelectedRows.Count == 1)
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
				if (_filterResultList.Count > 0)
				{
					int index = _filterResultList.BinarySearch(dataGridView.CurrentRow.Index);
					if (index < 0)
					{
						index = ~index;
						if (index > 0)
							--index;
					}
					if (filterGridView.Rows.Count > 0)  // exception no rows
					{
						filterGridView.CurrentCell = filterGridView.Rows[index].Cells[0];
					}
					else
					{
						filterGridView.CurrentCell = null;
					}
				}
			}
			catch (Exception e)
			{
				_logger.Error(e, "SyncFilterGridPos(): ");
			}
		}

		private void StatusLineFileSize(long size)
		{
			_statusEventArgs.FileSize = size;
			SendStatusLineUpdate();
		}

		private void SearchComplete(IAsyncResult result)
		{
			if (Disposing)
				return;
			try
			{
				Invoke(new MethodInvoker(ResetProgressBar));
				AsyncResult ar = (AsyncResult)result;
				Func<SearchParams, int> fx = (Func<SearchParams, int>)ar.AsyncDelegate;
				int line = fx.EndInvoke(result);
				_guiStateArgs.MenuEnabled = true;
				GuiStateUpdate(this, _guiStateArgs);
				if (line == -1)
				{
					return;
				}
				dataGridView.Invoke(new Action<int, bool>(SelectLine), new object[] { line, true });
			}
			catch (Exception e) // in the case the windows is already destroyed
			{
				_logger.Error(e);
			}
		}

		private void ResetProgressBar()
		{
			_progressEventArgs.Value = _progressEventArgs.MaxValue;
			_progressEventArgs.Visible = false;
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
				_shouldCallTimeSync = triggerSyncCall;
				bool wasCancelled = _shouldCancel;
				_shouldCancel = false;
				_isSearching = false;
				StatusLineText("");
				_guiStateArgs.MenuEnabled = true;
				if (wasCancelled)
				{
					return;
				}
				if (line == -1)
				{
					MessageBox.Show(this, "Not found:", "Search result");   // Hmm... is that experimental code from early days?
					return;
				}
				dataGridView.Rows[line].Selected = true;
				if (shouldScroll)
				{
					dataGridView.CurrentCell = dataGridView.Rows[line].Cells[0];
					dataGridView.Focus();
				}
			}
			catch (IndexOutOfRangeException e)
			{
				// Occures sometimes (but cannot reproduce)
				_logger.Error(e, "Error while selecting line: ");
			}
		}

		private void SelectAndScrollToLine(int line)
		{
			SelectLine(line, false);
			dataGridView.FirstDisplayedScrollingRowIndex = line;
		}

		public void LogWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (_isErrorShowing)
			{
				RemoveStatusLineError();
			}
			if (e.KeyCode == Keys.F3)
			{
				if (_parentLogTabWin.SearchParams == null ||
					_parentLogTabWin.SearchParams.searchText == null ||
					_parentLogTabWin.SearchParams.searchText.Length == 0)
				{
					return;
				}
				_parentLogTabWin.SearchParams.isFindNext = true;
				_parentLogTabWin.SearchParams.isShiftF3Pressed = ((e.Modifiers & Keys.Shift) == Keys.Shift);
				StartSearch();
			}
			if (e.KeyCode == Keys.Escape)
			{
				if (_isSearching)
				{
					_shouldCancel = true;
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
				int newLine = CurrentLogFileReader.GetNextMultiFileLine(CurrentDataGridLine);
				if (newLine != -1)
				{
					SelectLine(newLine, false);
				}
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
			{
				int newLine = CurrentLogFileReader.GetPrevMultiFileLine(CurrentDataGridLine);
				if (newLine != -1)
				{
					SelectLine(newLine - 1, false);
				}
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Enter && dataGridView.Focused)
			{
				ChangeRowHeight(e.Shift);
				e.Handled = true;
			}
			if (e.KeyCode == Keys.Back && dataGridView.Focused)
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
			if (!dataGridView.CurrentCell.ReadOnly)
			{
				dataGridView.BeginEdit(false);
				if (dataGridView.EditingControl != null)
				{
					if (dataGridView.EditingControl.GetType().IsAssignableFrom(typeof(LogCellEditingControl)))
					{
						DataGridViewTextBoxEditingControl editControl = dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
						editControl.KeyDown += EditControl_UpdateEditColumnDisplay;
						editControl.KeyPress += new KeyPressEventHandler(EditControl_KeyPress);
						editControl.KeyUp += new KeyEventHandler(EditControl_UpdateEditColumnDisplay);
						editControl.Click += new EventHandler(EditControl_Click);
						dataGridView.CellEndEdit += new DataGridViewCellEventHandler(DataGridView_CellEndEdit);
						editControl.SelectionStart = 0;
					}
				}
			}
		}

		private void UpdateEditColumnDisplay(DataGridViewTextBoxEditingControl editControl)
		{
			// prevents key events after edit mode has ended
			if (dataGridView.EditingControl != null)
			{
				int pos = editControl.SelectionStart + editControl.SelectionLength;
				StatusLineText("   " + pos);
				_logger.Debug("SelStart: " + editControl.SelectionStart + ", SelLen: " + editControl.SelectionLength);
			}
		}

		private void SelectPrevHighlightLine()
		{
			int lineNum = CurrentDataGridLine;
			while (lineNum > 0)
			{
				lineNum--;
				string line = CurrentLogFileReader.GetLogLine(lineNum);
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
			int lineNum = CurrentDataGridLine;
			while (lineNum < CurrentLogFileReader.LineCount)
			{
				lineNum++;
				string line = CurrentLogFileReader.GetLogLine(lineNum);
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
				return BookmarkProvider.FindNextBookmarkIndex(lineNum);
			}
			else
			{
				return BookmarkProvider.FindPrevBookmarkIndex(lineNum);
			}
		}

		/**
		 * Shift bookmarks after a logfile rollover
		 */

		private void ShiftBookmarks(int offset)
		{
			BookmarkProvider.ShiftBookmarks(offset);
		}

		private void LoadFilterPipes()
		{
			lock (_filterPipeList)
			{
				foreach (FilterPipe pipe in _filterPipeList)
				{
					pipe.RecreateTempFile();
				}
			}
			if (_filterPipeList.Count > 0)
			{
				for (int i = 0; i < dataGridView.RowCount; ++i)
				{
					ProcessFilterPipes(i);
				}
			}
		}

		private void DisconnectFilterPipes()
		{
			lock (_filterPipeList)
			{
				foreach (FilterPipe pipe in _filterPipeList)
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
			filterComboBox.Text = _filterParams.searchText;
			filterCaseSensitiveCheckBox.Checked = _filterParams.isCaseSensitive;
			filterRegexCheckBox.Checked = _filterParams.isRegex;
			filterTailCheckBox.Checked = _filterParams.isFilterTail;
			invertFilterCheckBox.Checked = _filterParams.isInvert;
			filterKnobControl1.Value = _filterParams.spreadBefore;
			filterKnobControl2.Value = _filterParams.spreadBehind;
			rangeCheckBox.Checked = _filterParams.isRangeSearch;
			columnRestrictCheckBox.Checked = _filterParams.columnRestrict;
			fuzzyKnobControl.Value = _filterParams.fuzzyValue;
			filterRangeComboBox.Text = _filterParams.rangeSearchText;
		}

		private void ResetFilterControls()
		{
			filterComboBox.Text = "";
			filterCaseSensitiveCheckBox.Checked = false;
			filterRegexCheckBox.Checked = false;
			invertFilterCheckBox.Checked = false;
			filterKnobControl1.Value = 0;
			filterKnobControl2.Value = 0;
			rangeCheckBox.Checked = false;
			columnRestrictCheckBox.Checked = false;
			fuzzyKnobControl.Value = 0;
			filterRangeComboBox.Text = "";
		}

		private void FilterSearch()
		{
			if (filterComboBox.Text.Length == 0)
			{
				_filterParams.searchText = "";
				_filterParams.lowerSearchText = "";
				_filterParams.isRangeSearch = false;
				ClearFilterList();
				filterSearchButton.Image = null;
				ResetFilterControls();
				saveFilterButton.Enabled = false;
				return;
			}
			FilterSearch(filterComboBox.Text);
		}

		private void FilterSearch(string text)
		{
			FireCancelHandlers();   // make sure that there's no other filter running (maybe from filter restore)

			_filterParams.searchText = text;
			_filterParams.lowerSearchText = text.ToLower();
			ConfigManager.Settings.filterHistoryList.Remove(text);
			ConfigManager.Settings.filterHistoryList.Insert(0, text);
			if (ConfigManager.Settings.filterHistoryList.Count > MAX_HISTORY)
			{
				ConfigManager.Settings.filterHistoryList.RemoveAt(filterComboBox.Items.Count - 1);
			}
			filterComboBox.Items.Clear();
			foreach (string item in ConfigManager.Settings.filterHistoryList)
			{
				filterComboBox.Items.Add(item);
			}
			filterComboBox.Text = text;

			_filterParams.isRangeSearch = rangeCheckBox.Checked;
			_filterParams.rangeSearchText = filterRangeComboBox.Text;
			if (_filterParams.isRangeSearch)
			{
				ConfigManager.Settings.filterRangeHistoryList.Remove(filterRangeComboBox.Text);
				ConfigManager.Settings.filterRangeHistoryList.Insert(0, filterRangeComboBox.Text);
				if (ConfigManager.Settings.filterRangeHistoryList.Count > MAX_HISTORY)
				{
					ConfigManager.Settings.filterRangeHistoryList.RemoveAt(filterRangeComboBox.Items.Count - 1);
				}

				filterRangeComboBox.Items.Clear();
				foreach (string item in ConfigManager.Settings.filterRangeHistoryList)
				{
					filterRangeComboBox.Items.Add(item);
				}
			}
			ConfigManager.Save(SettingsFlags.FilterHistory);

			_filterParams.isCaseSensitive = filterCaseSensitiveCheckBox.Checked;
			_filterParams.isRegex = filterRegexCheckBox.Checked;
			_filterParams.isFilterTail = filterTailCheckBox.Checked;
			_filterParams.isInvert = invertFilterCheckBox.Checked;
			if (_filterParams.isRegex)
			{
				try
				{
					_filterParams.CreateRegex();
				}
				catch (ArgumentException ex)
				{
					_logger.Error(ex);
					StatusLineError("Invalid regular expression");
					return;
				}
			}
			_filterParams.fuzzyValue = fuzzyKnobControl.Value;
			_filterParams.spreadBefore = filterKnobControl1.Value;
			_filterParams.spreadBehind = filterKnobControl2.Value;
			_filterParams.columnRestrict = columnRestrictCheckBox.Checked;

			//ConfigManager.SaveFilterParams(filterParams);
			ConfigManager.Settings.filterParams = _filterParams;  // wozu eigentlich? sinnlos seit MDI?

			_shouldCancel = false;
			_isSearching = true;
			filterSearchButton.Enabled = false;
			ClearFilterList();

			StartProgressBar(dataGridView.RowCount, "Filtering... Press ESC to cancel");

			Settings settings = ConfigManager.Settings;
			Action<FilterParams, List<int>, List<int>, List<int>> fx;
			fx = settings.preferences.multiThreadFilter ? new Action<FilterParams, List<int>, List<int>, List<int>>(MultiThreadedFilter) : new Action<FilterParams, List<int>, List<int>, List<int>>(Filter);
			fx.BeginInvoke(_filterParams, _filterResultList, _lastFilterLinesList, _filterHitList, FilterComplete, null);
			CheckForFilterDirty();
		}

		private void MultiThreadedFilter(FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterLinesList, List<int> filterHitList)
		{
			ColumnizerCallback callback = new ColumnizerCallback(this);
			FilterStarter fs = new FilterStarter(callback, Environment.ProcessorCount + 2);
			fs.FilterHitList = _filterHitList;
			fs.FilterResultLines = _filterResultList;
			fs.LastFilterLinesList = _lastFilterLinesList;
			BackgroundProcessCancelHandler cancelHandler = new FilterCancelHandler(fs);
			RegisterCancelHandler(cancelHandler);
			long startTime = Environment.TickCount;

			fs.DoFilter(filterParams, 0, CurrentLogFileReader.LineCount, FilterProgressCallback);

			long endTime = Environment.TickCount;
#if DEBUG
			_logger.Info("Multi threaded filter duration: " + ((endTime - startTime)) + " ms.");
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
					string line = CurrentLogFileReader.GetLogLine(lineNum);
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
					if (_shouldCancel)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Exception while filtering. Please report to developer:");
				MessageBox.Show(null, "Exception while filtering. Please report to developer: \n\n" + ex + "\n\n" + ex.StackTrace, "LogExpert");
			}
			long endTime = Environment.TickCount;
#if DEBUG
			_logger.Info("Single threaded filter duration: " + ((endTime - startTime)) + " ms.");
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
				if (lineNum + i < CurrentLogFileReader.LineCount)
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
			lock (_filterResultList)
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

		private void TriggerFilterLineGuiUpdate()
		{
			Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
		}

		private void AddFilterLineGuiUpdate()
		{
			try
			{
				lock (_filterResultList)
				{
					filterCountLabel.Text = "" + _filterResultList.Count;
					if (filterGridView.RowCount > _filterResultList.Count)
					{
						filterGridView.RowCount = 0;  // helps to prevent hang ?
					}
					filterGridView.RowCount = _filterResultList.Count;
					if (filterGridView.RowCount > 0)
					{
						filterGridView.FirstDisplayedScrollingRowIndex = filterGridView.RowCount - 1;
					}
					if (filterGridView.RowCount == 1)
					{
						// after a file reload adjusted column sizes anew when the first line arrives
						//filterGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
						AutoResizeColumns(filterGridView);
					}
				}
			}
			catch (Exception e)
			{
				_logger.Error(e, "AddFilterLineGuiUpdate(): ");
			}
		}

		protected override void UpdateProgressBar(int value)
		{
			_progressEventArgs.Value = value;
			if (value > _progressEventArgs.MaxValue)
			{
				// can occur if new lines will be added while filtering
				_progressEventArgs.MaxValue = value;
			}
			SendProgressBarUpdate();
		}

		private void FilterComplete(IAsyncResult result)
		{
			if (!IsDisposed && !_waitingForClose && !Disposing)
			{
				Invoke(new MethodInvoker(ResetStatusAfterFilter));
			}
		}

		private void ResetStatusAfterFilter()
		{
			try
			{
				_isSearching = false;
				_progressEventArgs.Value = _progressEventArgs.MaxValue;
				_progressEventArgs.Visible = false;
				SendProgressBarUpdate();
				filterGridView.RowCount = _filterResultList.Count;
				AutoResizeColumns(filterGridView);
				filterCountLabel.Text = "" + _filterResultList.Count;
				if (filterGridView.RowCount > 0)
				{
					filterGridView.Focus();
				}
				filterSearchButton.Enabled = true;
			}
			catch (NullReferenceException e)
			{
				// See https://connect.microsoft.com/VisualStudio/feedback/details/366943/autoresizecolumns-in-datagridview-throws-nullreferenceexception
				// There are some rare situations with null ref exceptions when resizing columns and on filter finished
				// So catch them here. Better than crashing.
				_logger.Error(e);
			}
		}

		protected override void ClearFilterList()
		{
			try
			{
				lock (_filterResultList)
				{
					filterGridView.SuspendLayout();
					filterGridView.RowCount = 0;
					filterCountLabel.Text = "0";
					_filterResultList = new List<int>();
					_lastFilterLinesList = new List<int>();
					_filterHitList = new List<int>();
					filterGridView.ResumeLayout();
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Wieder dieser sporadische Fehler: ");
				MessageBox.Show(null, ex.StackTrace, "Wieder dieser sporadische Fehler:");
			}
		}

		/**
		 * Shift filter list line entries after a logfile rollover
		 */

		private void ShiftFilterLines(int offset)
		{
			List<int> newFilterList = new List<int>();
			lock (_filterResultList)
			{
				foreach (int lineNum in _filterResultList)
				{
					int line = lineNum - offset;
					if (line >= 0)
					{
						newFilterList.Add(line);
					}
				}
				_filterResultList = newFilterList;
			}

			newFilterList = new List<int>();
			foreach (int lineNum in _filterHitList)
			{
				int line = lineNum - offset;
				if (line >= 0)
				{
					newFilterList.Add(line);
				}
			}
			_filterHitList = newFilterList;

			int count = SPREAD_MAX;
			if (_filterResultList.Count < SPREAD_MAX)
			{
				count = _filterResultList.Count;
			}
			_lastFilterLinesList = _filterResultList.GetRange(_filterResultList.Count - count, count);

			TriggerFilterLineGuiUpdate();
		}

		private void CheckForFilterDirty()
		{
			if (IsFilterSearchDirty(_filterParams))
			{
				filterSearchButton.Image = _searchButtonImage;
				saveFilterButton.Enabled = false;
			}
			else
			{
				filterSearchButton.Image = null;
				saveFilterButton.Enabled = true;
			}
		}

		private bool IsFilterSearchDirty(FilterParams filterParams)
		{
			if (!filterParams.searchText.Equals(filterComboBox.Text))
			{
				return true;
			}
			if (filterParams.isRangeSearch != rangeCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.isRangeSearch && !filterParams.rangeSearchText.Equals(filterRangeComboBox.Text))
			{
				return true;
			}
			if (filterParams.isRegex != filterRegexCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.isInvert != invertFilterCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.spreadBefore != filterKnobControl1.Value)
			{
				return true;
			}
			if (filterParams.spreadBehind != filterKnobControl2.Value)
			{
				return true;
			}
			if (filterParams.fuzzyValue != fuzzyKnobControl.Value)
			{
				return true;
			}
			if (filterParams.columnRestrict != columnRestrictCheckBox.Checked)
			{
				return true;
			}
			if (filterParams.isCaseSensitive != filterCaseSensitiveCheckBox.Checked)
			{
				return true;
			}
			return false;
		}

		private void AdjustMinimumGridWith()
		{
			if (dataGridView.Columns.Count > 1)
			{
				AutoResizeColumns(dataGridView);

				int width = dataGridView.Columns.GetColumnsWidth(DataGridViewElementStates.Visible);
				int diff = dataGridView.Width - width;
				if (diff > 0)
				{
					diff -= dataGridView.RowHeadersWidth / 2;
					dataGridView.Columns[dataGridView.Columns.Count - 1].Width =
																				dataGridView.Columns[dataGridView.Columns.Count - 1].Width + diff;
					filterGridView.Columns[filterGridView.Columns.Count - 1].Width =
																					filterGridView.Columns[filterGridView.Columns.Count - 1].Width + diff;
				}
			}
		}

		public void ToggleFilterPanel()
		{
			splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
			if (!splitContainer1.Panel2Collapsed)
			{
				filterComboBox.Focus();
			}
			else
			{
				dataGridView.Focus();
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
			InvalidateCurrentRow(dataGridView);
			InvalidateCurrentRow(filterGridView);
		}

		private void DisplayCurrentFileOnStatusline()
		{
			if (CurrentLogFileReader.IsMultiFile)
			{
				try
				{
					if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.Index > -1)
					{
						string fileName =
							CurrentLogFileReader.GetLogFileNameForLine(dataGridView.CurrentRow.Index);
						if (fileName != null)
						{
							StatusLineText(Util.GetNameFromPath(fileName));
						}
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex);
					// TODO: handle this concurrent situation better:
					// dataGridView.CurrentRow may be null even if checked before.
					// This can happen when MultiFile shift deselects the current row because there
					// are less lines after rollover than before.
					// access to dataGridView-Rows should be locked
				}
			}
		}

		private void UpdateSelectionDisplay()
		{
			if (_noSelectionUpdates)
			{
				return;
			}
			_selectionChangedTrigger.Trigger();
		}

		private void UpdateFilterHistoryFromSettings()
		{
			ConfigManager.Settings.filterHistoryList = ConfigManager.Settings.filterHistoryList;
			filterComboBox.Items.Clear();
			foreach (string item in ConfigManager.Settings.filterHistoryList)
			{
				filterComboBox.Items.Add(item);
			}

			filterRangeComboBox.Items.Clear();
			foreach (string item in ConfigManager.Settings.filterRangeHistoryList)
			{
				filterRangeComboBox.Items.Add(item);
			}
		}

		private void StatusLineText(string text)
		{
			_statusEventArgs.StatusText = text;
			SendStatusLineUpdate();
		}

		private void StatusLineTextImmediate(string text)
		{
			_statusEventArgs.StatusText = text;
			_statusLineTrigger.TriggerImmediate();
		}

		protected override void StatusLineError(string text)
		{
			StatusLineText(text);
			_isErrorShowing = true;
		}

		private void RemoveStatusLineError()
		{
			StatusLineText("");
			_isErrorShowing = false;
		}

		private void SendGuiStateUpdate()
		{
			OnGuiState(_guiStateArgs);
		}

		private void ShowAdvancedFilterPanel(bool show)
		{
			if (show)
			{
				advancedButton.Text = "Hide advanced...";
				advancedButton.Image = null;
			}
			else
			{
				advancedButton.Text = "Show advanced...";
				CheckForAdvancedButtonDirty();
			}

			advancedFilterSplitContainer.Panel1Collapsed = !show;
			advancedFilterSplitContainer.SplitterDistance = 54;
			_showAdvanced = show;
		}

		private void CheckForAdvancedButtonDirty()
		{
			if (IsAdvancedOptionActive && !_showAdvanced)
			{
				advancedButton.Image = _advancedButtonImage;
			}
			else
			{
				advancedButton.Image = null;
			}
		}

		private void FilterToTab()
		{
			filterSearchButton.Enabled = false;
			MethodInvoker invoker = new MethodInvoker(WriteFilterToTab);
			invoker.BeginInvoke(null, null);
		}

		private void WriteFilterToTab()
		{
			FilterPipe pipe = new FilterPipe(_filterParams.CreateCopy(), this);
			lock (_filterResultList)
			{
				string namePrefix = "->F";
				string title;
				if (IsTempFile)
				{
					title = TempTitleName + namePrefix + ++_filterPipeNameCounter;
				}
				else
				{
					title = Util.GetNameFromPath(FileName) + namePrefix + ++_filterPipeNameCounter;
				}

				WritePipeToTab(pipe, _filterResultList, title, null);
			}
		}

		private void WritePipeToTab(FilterPipe pipe, IList<int> lineNumberList, string name, PersistenceData persistenceData)
		{
			_logger.Info("WritePipeToTab(): " + lineNumberList.Count + " lines.");
			_guiStateArgs.MenuEnabled = false;
			SendGuiStateUpdate();

			StartProgressBar(lineNumberList.Count, "Writing to temp file... Press ESC to cancel.");

			_isSearching = true;
			_shouldCancel = false;

			lock (_filterPipeList)
			{
				_filterPipeList.Add(pipe);
			}
			pipe.Closed += new FilterPipe.ClosedEventHandler(Pipe_Disconnected);
			int count = 0;
			pipe.OpenFile();
			LogExpertCallback callback = new LogExpertCallback(this);
			foreach (int i in lineNumberList)
			{
				if (_shouldCancel)
				{
					break;
				}
				string line = CurrentLogFileReader.GetLogLine(i);
				if (CurrentColumnizer is ILogLineXmlColumnizer)
				{
					callback.LineNum = i;
					line = (CurrentColumnizer as ILogLineXmlColumnizer).GetLineTextForClipboard(line, callback);
				}
				pipe.WriteToPipe(line, i);
				if (++count % PROGRESS_BAR_MODULO == 0)
				{
					_progressEventArgs.Value = count;
					Invoke(new MethodInvoker(SendProgressBarUpdate));
				}
			}
			pipe.CloseFile();
			_logger.Info("WritePipeToTab(): finished");
			Invoke(new Action<FilterPipe, string, PersistenceData>(WriteFilterToTabFinished), new object[] { pipe, name, persistenceData });
		}

		private void WriteFilterToTabFinished(FilterPipe pipe, string name, PersistenceData persistenceData)
		{
			_isSearching = false;
			if (!_shouldCancel)
			{
				string title = name;
				ILogLineColumnizer preProcessColumnizer = null;
				if (!(CurrentColumnizer is ILogLineXmlColumnizer))
				{
					preProcessColumnizer = CurrentColumnizer;
				}
				LogWindow newWin = _parentLogTabWin.AddFilterTab(pipe, title, preProcessColumnizer);
				newWin.FilterPipe = pipe;
				pipe.OwnLogWindow = newWin;
				if (persistenceData != null)
				{
					Action<LogWindow, PersistenceData> fx = new Action<LogWindow, PersistenceData>(FilterRestore);
					fx.BeginInvoke(newWin, persistenceData, null, null);
				}
			}
			_progressEventArgs.Value = _progressEventArgs.MaxValue;
			_progressEventArgs.Visible = false;
			SendProgressBarUpdate();
			_guiStateArgs.MenuEnabled = true;
			SendGuiStateUpdate();
			StatusLineText("");
			filterSearchButton.Enabled = true;
		}

		private void FilterRestore(LogWindow newWin, PersistenceData persistenceData)
		{
			newWin.WaitForLoadingFinished();
			ILogLineColumnizer columnizer = Util.FindColumnizerByName(persistenceData.ColumnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
			if (columnizer != null)
			{
				Action<ILogLineColumnizer> fx = new Action<ILogLineColumnizer>(newWin.ForceColumnizer);
				newWin.Invoke(fx, new object[] { columnizer });
			}
			else
			{
				_logger.Warn("FilterRestore(): Columnizer " + persistenceData.ColumnizerName + " not found");
			}
			newWin.BeginInvoke(new Action<PersistenceData>(newWin.RestoreFilters), new object[] { persistenceData });
		}

		private void ProcessFilterPipes(int lineNum)
		{
			string searchLine = CurrentLogFileReader.GetLogLine(lineNum);
			if (searchLine == null)
			{
				return;
			}

			ColumnizerCallback callback = new ColumnizerCallback(this);
			callback.LineNum = lineNum;
			IList<FilterPipe> deleteList = new List<FilterPipe>();
			lock (_filterPipeList)
			{
				foreach (FilterPipe pipe in _filterPipeList)
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

							string textLine = CurrentLogFileReader.GetLogLine(line);
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
				_filterPipeList.Remove(pipe);
			}
		}

		private void CopyMarkedLinesToClipboard()
		{
			if (_guiStateArgs.CellSelectMode)
			{
				DataObject data = dataGridView.GetClipboardContent();
				Clipboard.SetDataObject(data);
			}
			else
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in dataGridView.SelectedRows)
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
					string line = CurrentLogFileReader.GetLogLine(lineNum);
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
			if (dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect)
			{
				List<int> lineNumList = new List<int>();
				foreach (DataGridViewRow row in dataGridView.SelectedRows)
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

		private void BookmarkComment(Bookmark bookmark)
		{
			BookmarkCommentDlg dlg = new BookmarkCommentDlg();
			dlg.Comment = bookmark.Text;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				BookmarkProvider.UpdateBookmarkText(bookmark, dlg.Comment);
				dataGridView.Refresh();
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
					if (colIndex < dataGridView.Columns.Count - 2)
					{
						if (names.Length > 0)
						{
							names += ", ";
						}
						names += dataGridView.Columns[2 + colIndex].HeaderText; // skip first two columns: marker + line number
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
				col.Frozen = _freezeStateMap.ContainsKey(gridView) && _freezeStateMap[gridView];
				if (col.Index == _selectedCol)
				{
					break;
				}
			}
		}

		private void ShowTimeSpread(bool show)
		{
			if (show)
			{
				tableLayoutPanel1.ColumnStyles[1].Width = 16;
			}
			else
			{
				tableLayoutPanel1.ColumnStyles[1].Width = 0;
			}
			_timeSpreadCalc.Enabled = show;
		}

#if DEBUG

		internal void DumpBufferInfo()
		{
			int currentLineNum = CurrentDataGridLine;
			CurrentLogFileReader.LogBufferInfoForLine(currentLineNum);
		}

		internal void DumpBufferDiagnostic()
		{
			CurrentLogFileReader.LogBufferDiagnostic();
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
			lock (_tempHilightEntryListLock)
			{
				_tempHilightEntryList.Add(he);
			}
			RefreshAllGrids();
		}

		private void RemoveAllSearchHighlightEntries()
		{
			lock (_tempHilightEntryListLock)
			{
				List<HilightEntry> newList = new List<HilightEntry>();
				foreach (HilightEntry he in _tempHilightEntryList)
				{
					if (!he.IsSearchHit)
					{
						newList.Add(he);
					}
				}
				_tempHilightEntryList = newList;
			}
			RefreshAllGrids();
		}

		internal void ChangeMultifileMask()
		{
			MultiFileMaskDialog dlg = new MultiFileMaskDialog(this, FileName);
			dlg.Owner = this;
			dlg.MaxDays = _multifileOptions.MaxDayTry;
			dlg.FileNamePattern = _multifileOptions.FormatPattern;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				_multifileOptions.FormatPattern = dlg.FileNamePattern;
				_multifileOptions.MaxDayTry = dlg.MaxDays;
				if (IsMultiFile)
				{
					Reload();
				}
			}
		}

		internal void ToggleColumnFinder(bool show, bool setFocus)
		{
			_guiStateArgs.ColumnFinderVisible = show;
			if (show)
			{
				columnComboBox.AutoCompleteMode = AutoCompleteMode.Suggest;
				columnComboBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
				columnComboBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
				columnComboBox.AutoCompleteCustomSource.AddRange(CurrentColumnizer.GetColumnNames());
				if (setFocus)
				{
					columnComboBox.Focus();
				}
			}
			else
			{
				dataGridView.Focus();
			}
			tableLayoutPanel1.RowStyles[0].Height = show ? 28 : 0;
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
			string colName = columnComboBox.SelectedItem as string;
			DataGridViewColumn col = GetColumnByName(dataGridView, colName);
			if (col != null && !col.Frozen)
			{
				dataGridView.FirstDisplayedScrollingColumnIndex = col.Index;
				int currentLine = CurrentDataGridLine;
				if (currentLine >= 0)
				{
					dataGridView.CurrentCell =
						dataGridView.Rows[CurrentDataGridLine].Cells[col.Index];
				}
			}
		}

		private void InitPatternWindow()
		{
			_patternWindow = new PatternWindow(this);
			_patternWindow.SetColumnizer(CurrentColumnizer);
			_patternWindow.SetFont(Preferences.fontName, Preferences.fontSize);
			_patternWindow.Fuzzy = _patternArgs.fuzzy;
			_patternWindow.MaxDiff = _patternArgs.maxDiffInBlock;
			_patternWindow.MaxMisses = _patternArgs.maxMisses;
			_patternWindow.Weight = _patternArgs.minWeight;
		}

		private void ChangeRowHeight(bool decrease)
		{
			int rowNum = CurrentDataGridLine;
			if (rowNum < 0 || rowNum >= dataGridView.RowCount)
			{
				return;
			}
			if (decrease)
			{
				if (!_rowHeightList.ContainsKey(rowNum))
				{
					return;
				}
				else
				{
					RowHeightEntry entry = _rowHeightList[rowNum];
					entry.Height = entry.Height - _lineHeight;
					if (entry.Height <= _lineHeight)
					{
						_rowHeightList.Remove(rowNum);
					}
				}
			}
			else
			{
				RowHeightEntry entry;
				if (!_rowHeightList.ContainsKey(rowNum))
				{
					entry = new RowHeightEntry();
					entry.LineNum = rowNum;
					entry.Height = _lineHeight;
					_rowHeightList[rowNum] = entry;
				}
				else
				{
					entry = _rowHeightList[rowNum];
				}
				entry.Height = entry.Height + _lineHeight;
			}
			dataGridView.UpdateRowHeightInfo(rowNum, false);
			if (rowNum == dataGridView.RowCount - 1 && _guiStateArgs.FollowTail)
			{
				dataGridView.FirstDisplayedScrollingRowIndex = rowNum;
			}
			dataGridView.Refresh();
		}

		private int GetRowHeight(int rowNum)
		{
			if (_rowHeightList.ContainsKey(rowNum))
			{
				return _rowHeightList[rowNum].Height;
			}
			else
			{
				return _lineHeight;
			}
		}

		private void AddBookmarkAtLineSilently(int lineNum)
		{
			if (!BookmarkProvider.IsBookmarkAtLine(lineNum))
			{
				BookmarkProvider.AddBookmark(new Bookmark(lineNum));
			}
		}

		private void AddBookmarkAndEditComment()
		{
			int lineNum = CurrentDataGridLine;
			if (!BookmarkProvider.IsBookmarkAtLine(lineNum))
			{
				ToggleBookmark();
			}
			BookmarkComment(BookmarkProvider.GetBookmarkForLine(lineNum));
		}

		private void MarkCurrentFilterRange()
		{
			_filterParams.rangeSearchText = filterRangeComboBox.Text;
			ColumnizerCallback callback = new ColumnizerCallback(this);
			RangeFinder rangeFinder = new RangeFinder(_filterParams, callback);
			Range range = rangeFinder.FindRange(CurrentDataGridLine);
			if (range != null)
			{
				SetCellSelectionMode(false);
				_noSelectionUpdates = true;
				for (int i = range.StartLine; i <= range.EndLine; ++i)
				{
					dataGridView.Rows[i].Selected = true;
				}
				_noSelectionUpdates = false;
				UpdateSelectionDisplay();
			}
		}

		private void RemoveTempHighlights()
		{
			lock (_tempHilightEntryListLock)
			{
				_tempHilightEntryList.Clear();
			}
			RefreshAllGrids();
		}

		private void ToggleHighlightPanel(bool open)
		{
			highlightSplitContainer.Panel2Collapsed = !open;
			toggleHighlightPanelButton.Image = (open ? _panelCloseButtonImage : _panelOpenButtonImage);
		}

		private void SetBoomarksForSelectedFilterLines()
		{
			lock (_filterResultList)
			{
				foreach (DataGridViewRow row in filterGridView.SelectedRows)
				{
					int lineNum = _filterResultList[row.Index];
					AddBookmarkAtLineSilently(lineNum);
				}
			}
			RefreshAllGrids();
		}

		private void HandleChangedFilterOnLoadSetting()
		{
			_parentLogTabWin.Preferences.isFilterOnLoad = filterOnLoadCheckBox.Checked;
			_parentLogTabWin.Preferences.isAutoHideFilterList = hideFilterListOnLoadCheckBox.Checked;
			OnFilterListChanged(this);
		}

		private void RegisterCancelHandler(BackgroundProcessCancelHandler handler)
		{
			lock (_cancelHandlerList)
			{
				_cancelHandlerList.Add(handler);
			}
		}

		private void DeRegisterCancelHandler(BackgroundProcessCancelHandler handler)
		{
			lock (_cancelHandlerList)
			{
				_cancelHandlerList.Remove(handler);
			}
		}

		private void FireCancelHandlers()
		{
			lock (_cancelHandlerList)
			{
				foreach (BackgroundProcessCancelHandler handler in _cancelHandlerList)
				{
					handler.EscapePressed();
				}
			}
		}

		private void SyncOtherWindows(DateTime timestamp)
		{
			lock (_timeSyncListLock)
			{
				if (TimeSyncList != null)
				{
					TimeSyncList.NavigateToTimestamp(timestamp, this);
				}
			}
		}

		private void AddSlaveToTimesync(LogWindow slave)
		{
			lock (_timeSyncListLock)
			{
				if (TimeSyncList == null)
				{
					if (slave.TimeSyncList == null)
					{
						TimeSyncList = new TimeSyncList();
						TimeSyncList.AddWindow(this);
					}
					else
					{
						TimeSyncList = slave.TimeSyncList;
					}
					int currentLineNum = CurrentDataGridLine;
					int refLine = currentLineNum;
					DateTime timeStamp = GetTimestampForLine(ref refLine, true);
					if (!timeStamp.Equals(DateTime.MinValue) && !_shouldTimestampDisplaySyncingCancel)
					{
						TimeSyncList.CurrentTimestamp = timeStamp;
					}
					TimeSyncList.WindowRemoved += TimeSyncList_WindowRemoved;
				}
			}
			slave.AddToTimeSync(this);
			OnSyncModeChanged();
		}

		private void FreeSlaveFromTimesync(LogWindow slave)
		{
			slave.FreeFromTimeSync();
		}

		protected override string GetPersistString()
		{
			return "LogWindow#" + FileName;
		}

		#endregion Private Methods

		#region Events

		private void LogWindow_Closing(object sender, CancelEventArgs e)
		{
			if (Preferences.askForClose)
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

		#endregion Events

		#region Events there delegates an methods

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

		public delegate void FilterListChangedEventHandler(object sender, FilterListChangedEventArgs e);

		public event FilterListChangedEventHandler FilterListChanged;

		protected void OnFilterListChanged(LogWindow source)
		{
			if (FilterListChanged != null)
			{
				FilterListChanged(this, new FilterListChangedEventArgs(source));
			}
		}

		public delegate void CurrentHighlightGroupChangedEventHandler(object sender, CurrentHighlightGroupChangedEventArgs e);

		public event CurrentHighlightGroupChangedEventHandler CurrentHighlightGroupChanged;

		protected void OnCurrentHighlightListChanged()
		{
			if (CurrentHighlightGroupChanged != null)
			{
				CurrentHighlightGroupChanged(this, new CurrentHighlightGroupChangedEventArgs(this, _currentHighlightGroup));
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
				SyncModeChanged(this, new SyncModeEventArgs(IsTimeSynced));
			}
		}

		#endregion Events there delegates an methods

		#region Internals

		internal override void RefreshAllGrids()
		{
			dataGridView.Refresh();
			filterGridView.Refresh();
		}

		#endregion Internals

		#region ILogPaintContext Member

		public string GetLogLine(int lineNum)
		{
			return CurrentLogFileReader.GetLogLine(lineNum);
		}

		public Bookmark GetBookmarkForLine(int lineNum)
		{
			return BookmarkProvider.GetBookmarkForLine(lineNum);
		}

		public Font MonospacedFont
		{
			get
			{
				return _fontMonospaced;
			}
		}

		public Font NormalFont
		{
			get
			{
				return _normalFont;
			}
		}

		public Font BoldFont
		{
			get
			{
				return _fontBold;
			}
		}

		#endregion ILogPaintContext Member

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

		public int LineCount
		{
			get
			{
				return CurrentLogFileReader.LineCount;
			}
		}

		public LogWindow CurrentLogWindows
		{
			get
			{
				return this;
			}
		}

		#endregion ILogWindowSerach Members

		#region ILogView Member

		public void RefreshLogView()
		{
			RefreshAllGrids();
		}

		#endregion ILogView Member

		#region Nested Classes

		// =================== ILogLineColumnizerCallback ============================

		public class LogExpertCallback : ColumnizerCallback, ILogExpertCallback
		{
			public LogExpertCallback(LogWindow logWindow)
				: base(logWindow)
			{
			}

			#region ILogExpertCallback Member

			public void AddTempFileTab(string fileName, string title)
			{
				_logWindow.AddTempFileTab(fileName, title);
			}

			public void AddPipedTab(IList<LineEntry> lineEntryList, string title)
			{
				_logWindow.WritePipeTab(lineEntryList, title);
			}

			public string GetTabTitle()
			{
				return _logWindow.Text;
			}

			#endregion ILogExpertCallback Member
		}

		#endregion Nested Classes
	}
}