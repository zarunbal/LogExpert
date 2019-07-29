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
using LogExpert.Classes.Columnizer;

namespace LogExpert
{
    public partial class LogWindow : DockContent, ILogPaintContext, ILogView
    {
        #region Fields

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
        private readonly Thread logEventHandlerThread;
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

        private readonly Thread timeshiftSyncThread;
        private readonly EventWaitHandle timeshiftSyncTimerEvent = new ManualResetEvent(false);
        private readonly EventWaitHandle timeshiftSyncWakeupEvent = new ManualResetEvent(false);

        private readonly TimeSpreadCalculator timeSpreadCalc;

        private readonly object timeSyncListLock = new object();

        private ColumnCache columnCache = new ColumnCache();

        private ILogLineColumnizer currentColumnizer;

        //List<HilightEntry> currentHilightEntryList = new List<HilightEntry>();
        private HilightGroup currentHighlightGroup = new HilightGroup();

        private SearchParams currentSearchParams;

        private string[] fileNames;
        private List<int> filterHitList = new List<int>();
        private FilterParams filterParams = new FilterParams();
        private int filterPipeNameCounter = 0;
        private List<int> filterResultList = new List<int>();

        private EventWaitHandle filterUpdateEvent = new ManualResetEvent(false);

        private ILogLineColumnizer forcedColumnizer;
        private ILogLineColumnizer forcedColumnizerForLoading;
        private bool isDeadFile;
        private bool isErrorShowing;
        private bool isLoadError;
        private bool isLoading;
        private bool isMultiFile;
        private bool isSearching;
        private bool isTimestampDisplaySyncing;
        private List<int> lastFilterLinesList = new List<int>();

        private int lineHeight = 0;

        private LogfileReader logFileReader;
        private MultifileOptions multifileOptions = new MultifileOptions();
        private bool noSelectionUpdates;
        private PatternArgs patternArgs = new PatternArgs();
        private PatternWindow patternWindow;

        private ReloadMemento reloadMemento;
        private int reloadOverloadCounter = 0;
        private SortedList<int, RowHeightEntry> rowHeightList = new SortedList<int, RowHeightEntry>();
        private int selectedCol = 0; // set by context menu event for column headers only
        private bool shouldCallTimeSync;
        private bool shouldCancel;
        private bool shouldTimestampDisplaySyncingCancel;
        private bool showAdvanced;
        private List<HilightEntry> tempHilightEntryList = new List<HilightEntry>();
        private int timeshiftSyncLine = 0;

        private bool waitingForClose;

        #endregion

        #region cTor

        public LogWindow(LogTabWindow parent, string fileName, bool isTempFile,
            bool forcePersistenceLoading)
        {
            SuspendLayout();

            InitializeComponent();

            columnNamesLabel.Text = ""; // no filtering on columns by default

            parentLogTabWin = parent;
            IsTempFile = isTempFile;
            //Thread.CurrentThread.Name = "LogWindowThread";
            ColumnizerCallbackObject = new ColumnizerCallback(this);

            FileName = fileName;
            ForcePersistenceLoading = forcePersistenceLoading;

            dataGridView.CellValueNeeded += dataGridView_CellValueNeeded;
            dataGridView.CellPainting += dataGridView_CellPainting;

            filterGridView.CellValueNeeded += filterGridView_CellValueNeeded;
            filterGridView.CellPainting += filterGridView_CellPainting;

            Closing += LogWindow_Closing;
            Disposed += LogWindow_Disposed;
            Load += LogWindow_Load;

            timeSpreadCalc = new TimeSpreadCalculator(this);
            timeSpreadingControl1.TimeSpreadCalc = timeSpreadCalc;
            timeSpreadingControl1.LineSelected += timeSpreadingControl1_LineSelected;
            tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Absolute;
            tableLayoutPanel1.ColumnStyles[1].Width = 20;
            tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Percent;
            tableLayoutPanel1.ColumnStyles[0].Width = 100;

            parentLogTabWin.HighlightSettingsChanged += parent_HighlightSettingsChanged;
            SetColumnizer(null);

            patternArgs.maxMisses = 5;
            patternArgs.minWeight = 1;
            patternArgs.maxDiffInBlock = 5;
            patternArgs.fuzzy = 5;

            //InitPatternWindow();

            //this.toolwinTabControl.TabPages.Add(this.patternWindow);
            //this.toolwinTabControl.TabPages.Add(this.bookmarkWindow);

            filterParams = new FilterParams();
            foreach (string item in ConfigManager.Settings.filterHistoryList)
            {
                filterComboBox.Items.Add(item);
            }

            filterComboBox.DropDownHeight = filterComboBox.ItemHeight * ConfigManager.Settings.preferences.maximumFilterEntriesDisplayed;

            filterRegexCheckBox.Checked = filterParams.isRegex;
            filterCaseSensitiveCheckBox.Checked = filterParams.isCaseSensitive;
            filterTailCheckBox.Checked = filterParams.isFilterTail;

            splitContainer1.Panel2Collapsed = true;
            advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;

            timeshiftSyncThread = new Thread(SyncTimestampDisplayWorker);
            timeshiftSyncThread.IsBackground = true;
            timeshiftSyncThread.Start();

            logEventHandlerThread = new Thread(LogEventWorker);
            logEventHandlerThread.IsBackground = true;
            logEventHandlerThread.Start();

            //this.filterUpdateThread = new Thread(new ThreadStart(this.FilterUpdateWorker));
            //this.filterUpdateThread.Start();

            advancedButtonImage = advancedButton.Image;
            searchButtonImage = filterSearchButton.Image;
            filterSearchButton.Image = null;

            dataGridView.EditModeMenuStrip = editModeContextMenuStrip;
            markEditModeToolStripMenuItem.Enabled = true;

            panelOpenButtonImage = new Bitmap(GetType(), "Resources.PanelOpen.gif");
            panelCloseButtonImage = new Bitmap(GetType(), "Resources.PanelClose.gif");

            Settings settings = ConfigManager.Settings;
            if (settings.appBounds != null && settings.appBounds.Right > 0)
            {
                Bounds = settings.appBounds;
            }

            waitingForClose = false;
            dataGridView.Enabled = false;
            dataGridView.ColumnDividerDoubleClick += dataGridView_ColumnDividerDoubleClick;
            ShowAdvancedFilterPanel(false);
            filterKnobControl1.MinValue = 0;
            filterKnobControl1.MaxValue = SPREAD_MAX;
            filterKnobControl1.ValueChanged += filterKnobControl1_ValueChanged;
            filterKnobControl2.MinValue = 0;
            filterKnobControl2.MaxValue = SPREAD_MAX;
            filterKnobControl2.ValueChanged += filterKnobControl1_ValueChanged;
            fuzzyKnobControl.MinValue = 0;
            fuzzyKnobControl.MaxValue = 10;
            //PreferencesChanged(settings.preferences, true);
            AdjustHighlightSplitterWidth();
            ToggleHighlightPanel(false); // hidden

            bookmarkProvider.BookmarkAdded += bookmarkProvider_BookmarkAdded;
            bookmarkProvider.BookmarkRemoved += bookmarkProvider_BookmarkRemoved;
            bookmarkProvider.AllBookmarksRemoved += bookmarkProvider_AllBookmarksRemoved;

            ResumeLayout();

            statusLineTrigger.Signal += statusLineTrigger_Signal;
            selectionChangedTrigger.Signal += selectionChangedTrigger_Signal;
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
            get => currentColumnizer;
            set
            {
                lock (currentColumnizerLock)
                {
                    currentColumnizer = value;
                    _logger.Debug("Setting columnizer {0} ", currentColumnizer.GetName());
                }
            }
        }

        public bool ShowBookmarkBubbles
        {
            get => guiStateArgs.ShowBookmarkBubbles;
            set
            {
                guiStateArgs.ShowBookmarkBubbles = dataGridView.PaintWithOverlays = value;
                dataGridView.Refresh();
            }
        }

        public string FileName { get; private set; }

        public string SessionFileName { get; set; } = null;

        public bool IsMultiFile
        {
            get => isMultiFile;
            set => guiStateArgs.IsMultiFileActive = isMultiFile = value;
        }

        public bool IsTempFile { get; }

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

                return FileName;
            }
        }

        public ColumnizerCallback ColumnizerCallbackObject { get; }

        public bool ForcePersistenceLoading { get; set; }

        public string ForcedPersistenceFileName { get; set; } = null;

        public Preferences Preferences => ConfigManager.Settings.preferences;

        public string GivenFileName { get; set; } = null;

        public TimeSyncList TimeSyncList { get; private set; }

        public bool IsTimeSynced => TimeSyncList != null;

        protected EncodingOptions EncodingOptions { get; set; }

        public IBookmarkData BookmarkData => bookmarkProvider;

        public Font MonospacedFont { get; private set; }

        public Font NormalFont { get; private set; }

        public Font BoldFont { get; private set; }

        #endregion

        #region Public methods

        public ILogLine GetLogLine(int lineNum)
        {
            return logFileReader.GetLogLine(lineNum);
        }

        public Bookmark GetBookmarkForLine(int lineNum)
        {
            return bookmarkProvider.GetBookmarkForLine(lineNum);
        }

        #endregion

        #region Internals

        internal IColumnizedLogLine GetColumnsForLine(int lineNumber)
        {
            return columnCache.GetColumnsForLine(logFileReader, lineNumber, CurrentColumnizer,
                ColumnizerCallbackObject);

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
            dataGridView.Refresh();
            filterGridView.Refresh();
        }

        internal void ChangeMultifileMask()
        {
            MultiFileMaskDialog dlg = new MultiFileMaskDialog(this, FileName);
            dlg.Owner = this;
            dlg.MaxDays = multifileOptions.MaxDayTry;
            dlg.FileNamePattern = multifileOptions.FormatPattern;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                multifileOptions.FormatPattern = dlg.FileNamePattern;
                multifileOptions.MaxDayTry = dlg.MaxDays;
                if (IsMultiFile)
                {
                    Reload();
                }
            }
        }

        internal void ToggleColumnFinder(bool show, bool setFocus)
        {
            guiStateArgs.ColumnFinderVisible = show;
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

        private delegate void WriteFilterToTabFinishedFx(
            FilterPipe pipe, string namePrefix,
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

        public class ColumnizerCallback : ILogLineColumnizerCallback, IAutoLogLineColumnizerCallback
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
                logWindow = original.logWindow;
                LineNum = original.LineNum;
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
                return logWindow.GetCurrentFileName(LineNum);
            }

            public ILogLine GetLogLine(int lineNum)
            {
                return logWindow.GetLine(lineNum);
            }

            public IList<ILogLineColumnizer> GetRegisteredColumnizers()
            {
                return PluginRegistry.GetInstance().RegisteredColumnizers;
            }

            public int GetLineCount()
            {
                return logWindow.logFileReader.LineCount;
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
                logWindow.AddTempFileTab(fileName, title);
            }

            public void AddPipedTab(IList<LineEntry> lineEntryList, string title)
            {
                logWindow.WritePipeTab(lineEntryList, title);
            }

            public string GetTabTitle()
            {
                return logWindow.Text;
            }

            #endregion
        }

#if DEBUG
        internal void DumpBufferInfo()
        {
            int currentLineNum = dataGridView.CurrentCellAddress.Y;
            logFileReader.LogBufferInfoForLine(currentLineNum);
        }

        internal void DumpBufferDiagnostic()
        {
            logFileReader.LogBufferDiagnostic();
        }
#endif
    }
}