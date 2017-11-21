using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
//using System.Linq;
using System.Windows.Forms;
using LogExpert.Dialogs;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Linq;
using NLog;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
    public partial class LogWindow : DockContent, ILogPaintContext, ILogView
    {
        #region Fields

        private const int MAX_HISTORY = 30;
        private const int SPREAD_MAX = 99;
        private const int PROGRESS_BAR_MODULO = 1000;
        private const int FILTER_ADCANCED_SPLITTER_DISTANCE = 54;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly Image advancedButtonImage;

        private readonly object bookmarkLock = new object();
        private readonly BookmarkDataProvider bookmarkProvider = new BookmarkDataProvider();

        private readonly IList<BackgroundProcessCancelHandler> cancelHandlerList =
            new List<BackgroundProcessCancelHandler>();

        private readonly object currentColumnizerLock = new object();

        private readonly object currentHighlightGroupLock = new object();

        private readonly EventWaitHandle externaLoadingFinishedEvent = new ManualResetEvent(false);

        private readonly IList<FilterPipe> filterPipeList = new List<FilterPipe>();
        private readonly Dictionary<Control, bool> freezeStateMap = new Dictionary<Control, bool>();
        private readonly GuiStateArgs guiStateArgs = new GuiStateArgs();

        private readonly List<int> lineHashList = new List<int>();

        private readonly EventWaitHandle loadingFinishedEvent = new ManualResetEvent(false);

        private readonly EventWaitHandle logEventArgsEvent = new ManualResetEvent(false);

        private readonly List<LogEventArgs> logEventArgsList = new List<LogEventArgs>();
        private readonly Thread logEventHandlerThread = null;
        private readonly Image panelCloseButtonImage;

        private readonly Image panelOpenButtonImage;
        private readonly LogTabWindow parentLogTabWin;

        private readonly ProgressEventArgs progressEventArgs = new ProgressEventArgs();
        private readonly object reloadLock = new object();
        private readonly Image searchButtonImage;
        private readonly DelayedTrigger selectionChangedTrigger = new DelayedTrigger(200);
        private readonly StatusLineEventArgs statusEventArgs = new StatusLineEventArgs();

        private readonly DelayedTrigger statusLineTrigger = new DelayedTrigger(200);
        private readonly object tempHilightEntryListLock = new object();

        private readonly Thread timeshiftSyncThread = null;
        private readonly EventWaitHandle timeshiftSyncTimerEvent = new ManualResetEvent(false);
        private readonly EventWaitHandle timeshiftSyncWakeupEvent = new ManualResetEvent(false);

        private readonly TimeSpreadCalculator timeSpreadCalc;

        private readonly object timeSyncListLock = new object();

        private ColumnCache columnCache = new ColumnCache();

        private ILogLineColumnizer currentColumnizer;

        //List<HilightEntry> currentHilightEntryList = new List<HilightEntry>();
        private HilightGroup currentHighlightGroup = new HilightGroup();

        private SearchParams currentSearchParams = null;

        private string[] fileNames;
        private List<int> filterHitList = new List<int>();
        private FilterParams filterParams = new FilterParams();
        private int filterPipeNameCounter = 0;
        private List<int> filterResultList = new List<int>();

        private EventWaitHandle filterUpdateEvent = new ManualResetEvent(false);

        private ILogLineColumnizer forcedColumnizer;
        private ILogLineColumnizer forcedColumnizerForLoading;
        private bool isDeadFile = false;
        private bool isErrorShowing = false;
        private bool isLoadError = false;
        private bool isLoading = false;
        private bool isMultiFile = false;
        private bool isSearching = false;
        private bool isTimestampDisplaySyncing = false;
        private List<int> lastFilterLinesList = new List<int>();

        private int lineHeight = 0;

        private LogfileReader logFileReader;
        private MultifileOptions multifileOptions = new MultifileOptions();
        private bool noSelectionUpdates = false;
        private PatternArgs patternArgs = new PatternArgs();
        private PatternWindow patternWindow;

        private ReloadMemento reloadMemento;
        private int reloadOverloadCounter = 0;
        private SortedList<int, RowHeightEntry> rowHeightList = new SortedList<int, RowHeightEntry>();
        private int selectedCol = 0; // set by context menu event for column headers only
        private bool shouldCallTimeSync = false;
        private bool shouldCancel = false;
        private bool shouldTimestampDisplaySyncingCancel = false;
        private bool showAdvanced = false;
        private List<HilightEntry> tempHilightEntryList = new List<HilightEntry>();
        private int timeshiftSyncLine = 0;

        private bool waitingForClose = false;

        #endregion

        #region cTor

        public LogWindow(LogTabWindow parent, string fileName, bool isTempFile,
            bool forcePersistenceLoading)
        {
            this.SuspendLayout();

            InitializeComponent();

            this.columnNamesLabel.Text = ""; // no filtering on columns by default

            this.parentLogTabWin = parent;
            this.IsTempFile = isTempFile;
            //Thread.CurrentThread.Name = "LogWindowThread";
            ColumnizerCallbackObject = new ColumnizerCallback(this);

            this.FileName = fileName;
            this.ForcePersistenceLoading = forcePersistenceLoading;

            this.dataGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(dataGridView_CellValueNeeded);
            this.dataGridView.CellPainting += new DataGridViewCellPaintingEventHandler(dataGridView_CellPainting);

            this.filterGridView.CellValueNeeded +=
                new DataGridViewCellValueEventHandler(filterGridView_CellValueNeeded);
            this.filterGridView.CellPainting += new DataGridViewCellPaintingEventHandler(filterGridView_CellPainting);

            this.Closing += new CancelEventHandler(LogWindow_Closing);
            this.Disposed += new EventHandler(LogWindow_Disposed);

            this.timeSpreadCalc = new TimeSpreadCalculator(this);
            this.timeSpreadingControl1.TimeSpreadCalc = this.timeSpreadCalc;
            this.timeSpreadingControl1.LineSelected +=
                new TimeSpreadingControl.LineSelectedEventHandler(timeSpreadingControl1_LineSelected);
            this.tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Absolute;
            this.tableLayoutPanel1.ColumnStyles[1].Width = 20;
            this.tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Percent;
            this.tableLayoutPanel1.ColumnStyles[0].Width = 100;

            this.parentLogTabWin.HighlightSettingsChanged += parent_HighlightSettingsChanged;

            SetColumnizer(PluginRegistry.GetInstance().RegisteredColumnizers[0]);

            patternArgs.maxMisses = 5;
            patternArgs.minWeight = 1;
            patternArgs.maxDiffInBlock = 5;
            patternArgs.fuzzy = 5;

            //InitPatternWindow();

            //this.toolwinTabControl.TabPages.Add(this.patternWindow);
            //this.toolwinTabControl.TabPages.Add(this.bookmarkWindow);

            this.filterParams = new FilterParams();
            foreach (string item in ConfigManager.Settings.filterHistoryList)
            {
                this.filterComboBox.Items.Add(item);
            }
            this.filterRegexCheckBox.Checked = this.filterParams.isRegex;
            this.filterCaseSensitiveCheckBox.Checked = this.filterParams.isCaseSensitive;
            this.filterTailCheckBox.Checked = this.filterParams.isFilterTail;

            this.splitContainer1.Panel2Collapsed = true;
            this.advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;

            this.timeshiftSyncThread = new Thread(new ThreadStart(this.SyncTimestampDisplayWorker));
            this.timeshiftSyncThread.IsBackground = true;
            this.timeshiftSyncThread.Start();

            this.logEventHandlerThread = new Thread(new ThreadStart(this.LogEventWorker));
            this.logEventHandlerThread.IsBackground = true;
            this.logEventHandlerThread.Start();

            //this.filterUpdateThread = new Thread(new ThreadStart(this.FilterUpdateWorker));
            //this.filterUpdateThread.Start();

            this.advancedButtonImage = this.advancedButton.Image;
            this.searchButtonImage = this.filterSearchButton.Image;
            this.filterSearchButton.Image = null;

            this.dataGridView.EditModeMenuStrip = this.editModeContextMenuStrip;
            this.markEditModeToolStripMenuItem.Enabled = true;

            this.panelOpenButtonImage = new Bitmap(GetType(), "Resources.PanelOpen.gif");
            this.panelCloseButtonImage = new Bitmap(GetType(), "Resources.PanelClose.gif");

            Settings settings = ConfigManager.Settings;
            if (settings.appBounds != null && settings.appBounds.Right > 0)
            {
                this.Bounds = settings.appBounds;
            }

            this.waitingForClose = false;
            this.dataGridView.Enabled = false;
            this.dataGridView.ColumnDividerDoubleClick +=
                new DataGridViewColumnDividerDoubleClickEventHandler(dataGridView_ColumnDividerDoubleClick);
            ShowAdvancedFilterPanel(false);
            this.filterKnobControl1.MinValue = 0;
            this.filterKnobControl1.MaxValue = SPREAD_MAX;
            this.filterKnobControl1.ValueChanged +=
                new KnobControl.ValueChangedEventHandler(filterKnobControl1_ValueChanged);
            this.filterKnobControl2.MinValue = 0;
            this.filterKnobControl2.MaxValue = SPREAD_MAX;
            this.filterKnobControl2.ValueChanged +=
                new KnobControl.ValueChangedEventHandler(filterKnobControl1_ValueChanged);
            this.fuzzyKnobControl.MinValue = 0;
            this.fuzzyKnobControl.MaxValue = 10;
            //PreferencesChanged(settings.preferences, true);
            AdjustHighlightSplitterWidth();
            ToggleHighlightPanel(false); // hidden

            bookmarkProvider.BookmarkAdded +=
                new BookmarkDataProvider.BookmarkAddedEventHandler(bookmarkProvider_BookmarkAdded);
            bookmarkProvider.BookmarkRemoved +=
                new BookmarkDataProvider.BookmarkRemovedEventHandler(bookmarkProvider_BookmarkRemoved);
            bookmarkProvider.AllBookmarksRemoved +=
                new BookmarkDataProvider.AllBookmarksRemovedEventHandler(bookmarkProvider_AllBookmarksRemoved);

            this.ResumeLayout();

            this.statusLineTrigger.Signal += new DelayedTrigger.SignalEventHandler(statusLineTrigger_Signal);
            this.selectionChangedTrigger.Signal +=
                new DelayedTrigger.SignalEventHandler(selectionChangedTrigger_Signal);

            PreferencesChanged(this.parentLogTabWin.Preferences, true, SettingsFlags.GuiOrColors);
        }

        #endregion

        #region Delegates

        public delegate void BookmarkAddedEventHandler(object sender, EventArgs e);

        public delegate void BookmarkRemovedEventHandler(object sender, EventArgs e);

        public delegate void BookmarkTextChangedEventHandler(object sender, BookmarkEventArgs e);

        public delegate void ColumnizerChangedEventHandler(object sender, ColumnizerEventArgs e);

        public delegate void CurrentHighlightGroupChangedEventHandler(object sender,
            CurrentHighlightGroupChangedEventArgs e
        );

        public delegate void FileNotFoundEventHandler(object sender, EventArgs e);

        public delegate void FileRespawnedEventHandler(object sender, EventArgs e);

        public delegate void FileSizeChangedEventHandler(object sender, LogEventArgs e);

        public delegate void FilterListChangedEventHandler(object sender, FilterListChangedEventArgs e);

        // used for filterTab restore
        public delegate void FilterRestoreFx(LogWindow newWin, PersistenceData persistenceData);

        public delegate void GuiStateEventHandler(object sender, GuiStateArgs e);

        public delegate void ProgressBarEventHandler(object sender, ProgressEventArgs e);

        public delegate void RestoreFiltersFx(PersistenceData persistenceData);

        public delegate bool ScrollToTimestampFx(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall);

        public delegate void StatusLineEventHandler(object sender, StatusLineEventArgs e);

        public delegate void SyncModeChangedEventHandler(object sender, SyncModeEventArgs e);

        public delegate void TailFollowedEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event FileSizeChangedEventHandler FileSizeChanged;

        public event ProgressBarEventHandler ProgressBarUpdate;

        public event StatusLineEventHandler StatusLineEvent;

        public event GuiStateEventHandler GuiStateUpdate;

        public event TailFollowedEventHandler TailFollowed;

        public event FileNotFoundEventHandler FileNotFound;

        public event FileRespawnedEventHandler FileRespawned;

        public event FilterListChangedEventHandler FilterListChanged;

        public event CurrentHighlightGroupChangedEventHandler CurrentHighlightGroupChanged;

        public event BookmarkAddedEventHandler BookmarkAdded;

        public event BookmarkRemovedEventHandler BookmarkRemoved;

        public event BookmarkTextChangedEventHandler BookmarkTextChanged;

        public event ColumnizerChangedEventHandler ColumnizerChanged;

        public event SyncModeChangedEventHandler SyncModeChanged;

        #endregion

        #region Properties

        public Color BookmarkColor { get; set; } = Color.FromArgb(165, 200, 225);

        public ILogLineColumnizer CurrentColumnizer
        {
            get { return this.currentColumnizer; }
            set
            {
                lock (this.currentColumnizerLock)
                {
                    this.currentColumnizer = value;
                    _logger.Debug("Setting columnizer {0} ", currentColumnizer.GetName());
                }
            }
        }

        public bool ShowBookmarkBubbles
        {
            get { return this.guiStateArgs.ShowBookmarkBubbles; }
            set
            {
                this.guiStateArgs.ShowBookmarkBubbles = this.dataGridView.PaintWithOverlays = value;
                this.dataGridView.Refresh();
            }
        }

        public string FileName { get; private set; }

        public string SessionFileName { get; set; } = null;

        public bool IsMultiFile
        {
            get { return isMultiFile; }
            set { this.guiStateArgs.IsMultiFileActive = this.isMultiFile = value; }
        }

        public bool IsTempFile { get; } = false;

        public string TempTitleName { get; set; } = "";

        internal FilterPipe FilterPipe { get; set; } = null;

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

        public ColumnizerCallback ColumnizerCallbackObject { get; }

        public bool ForcePersistenceLoading { get; set; } = false;

        public string ForcedPersistenceFileName { get; set; } = null;

        public Preferences Preferences
        {
            get { return ConfigManager.Settings.preferences; }
        }

        public string GivenFileName { get; set; } = null;

        public TimeSyncList TimeSyncList { get; private set; } = null;

        public bool IsTimeSynced
        {
            get { return this.TimeSyncList != null; }
        }

        protected EncodingOptions EncodingOptions { get; set; }

        public IBookmarkData BookmarkData
        {
            get { return this.bookmarkProvider; }
        }

        public Font MonospacedFont { get; private set; }

        public Font NormalFont { get; private set; }

        public Font BoldFont { get; private set; }

        #endregion

        #region Public methods

        public ILogLine GetLogLine(int lineNum)
        {
            return this.logFileReader.GetLogLine(lineNum);
        }

        public Bookmark GetBookmarkForLine(int lineNum)
        {
            return this.bookmarkProvider.GetBookmarkForLine(lineNum);
        }

        #endregion

        #region Internals

        internal IColumnizedLogLine GetColumnsForLine(int lineNumber)
        {
            return this.columnCache.GetColumnsForLine(this.logFileReader, lineNumber, this.CurrentColumnizer,
                this.ColumnizerCallbackObject);

            //string line = this.logFileReader.GetLogLine(lineNumber);
            //if (line != null)
            //{
            //  string[] cols;
            //  this.columnizerCallback.LineNum = lineNumber;
            //  cols = this.CurrentColumnizer.SplitLine(this.columnizerCallback, line);
            //  return cols;
            //}
            //else
            //{
            //  return null;
            //}
        }

        internal void RefreshAllGrids()
        {
            this.dataGridView.Refresh();
            this.filterGridView.Refresh();
        }

        internal void ChangeMultifileMask()
        {
            MultiFileMaskDialog dlg = new MultiFileMaskDialog(this, this.FileName);
            dlg.Owner = this;
            dlg.MaxDays = this.multifileOptions.MaxDayTry;
            dlg.FileNamePattern = this.multifileOptions.FormatPattern;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.multifileOptions.FormatPattern = dlg.FileNamePattern;
                this.multifileOptions.MaxDayTry = dlg.MaxDays;
                if (this.IsMultiFile)
                {
                    Reload();
                }
            }
        }

        internal void ToggleColumnFinder(bool show, bool setFocus)
        {
            this.guiStateArgs.ColumnFinderVisible = show;
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

        #endregion

        #region Overrides

        protected override string GetPersistString()
        {
            return "LogWindow#" + FileName;
        }

        #endregion


        // used for external wait fx WaitForLoadFinished()

        private delegate void UpdateGridCallback(LogEventArgs e);

        private delegate void UpdateProgressCallback(LoadFileEventArgs e);

        private delegate void LoadingStartedFx(LoadFileEventArgs e);

        private delegate int SearchFx(SearchParams searchParams);

        private delegate void SelectLineFx(int line, bool triggerSyncCall);

        private delegate void FilterFx(
            FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterResultLines,
            List<int> filterHitList);

        private delegate void UpdateProgressBarFx(int lineNum);

        private delegate void SetColumnizerFx(ILogLineColumnizer columnizer);

        private delegate void WriteFilterToTabFinishedFx(FilterPipe pipe, string namePrefix,
            PersistenceData persistenceData
        );

        private delegate void SetBookmarkFx(int lineNum, string comment);

        private delegate void FunctionWith1BoolParam(bool arg);

        private delegate void PatternStatisticFx(PatternArgs patternArgs);

        private delegate void ActionPluginExecuteFx(
            string keyword, string param, ILogExpertCallback callback, ILogLineColumnizer columnizer);

        private delegate void HighlightEventFx(HighlightEventArgs e);

        private delegate void PositionAfterReloadFx(ReloadMemento reloadMemento);

        private delegate void AutoResizeColumnsFx(DataGridView gridView);

        private delegate bool BoolReturnDelegate();

        // =================== ILogLineColumnizerCallback ============================

        public class ColumnizerCallback : ILogLineColumnizerCallback
        {
            #region Fields

            protected LogWindow logWindow;

            #endregion

            #region cTor

            public ColumnizerCallback(LogWindow logWindow)
            {
                this.logWindow = logWindow;
            }

            private ColumnizerCallback(ColumnizerCallback original)
            {
                this.logWindow = original.logWindow;
                this.LineNum = original.LineNum;
            }

            #endregion

            #region Properties

            public int LineNum { get; set; }

            #endregion

            #region Public methods

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

            public ILogLine GetLogLine(int lineNum)
            {
                return this.logWindow.GetLine(lineNum);
            }

            public int GetLineCount()
            {
                return this.logWindow.logFileReader.LineCount;
            }

            #endregion
        }

        public class LogExpertCallback : ColumnizerCallback, ILogExpertCallback
        {
            #region cTor

            public LogExpertCallback(LogWindow logWindow)
                : base(logWindow)
            {
            }

            #endregion

            #region Public methods

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

#if DEBUG
        internal void DumpBufferInfo()
        {
            int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
            this.logFileReader.LogBufferInfoForLine(currentLineNum);
        }

        internal void DumpBufferDiagnostic()
        {
            this.logFileReader.LogBufferDiagnostic();
        }

#endif
    }
}