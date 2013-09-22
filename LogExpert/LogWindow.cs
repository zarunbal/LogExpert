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
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
  public partial class LogWindow : DockContent, ILogPaintContext, ILogView
  {
    private const int MAX_HISTORY = 30;
    private const int MAX_COLUMNIZER_HISTORY = 40;
    private const int SPREAD_MAX = 99;
    private const int PROGRESS_BAR_MODULO = 1000;
    private const int FILTER_ADCANCED_SPLITTER_DISTANCE = 54;

    private Color bookmarkColor = Color.FromArgb(165, 200, 225);

    public Color BookmarkColor
    {
      get { return bookmarkColor; }
      set { bookmarkColor = value; }
    }

    LogfileReader logFileReader;
    private ILogLineColumnizer currentColumnizer;
    private readonly Object currentColumnizerLock = new Object();
    ILogLineColumnizer forcedColumnizer;
    ILogLineColumnizer forcedColumnizerForLoading;
    List<HilightEntry> tempHilightEntryList = new List<HilightEntry>();
    Object tempHilightEntryListLock = new Object();
    //List<HilightEntry> currentHilightEntryList = new List<HilightEntry>();
    HilightGroup currentHighlightGroup = new HilightGroup();
    private Object currentHighlightGroupLock = new Object();
    FilterParams filterParams = new FilterParams();
    SearchParams currentSearchParams = null;
    List<int> filterResultList = new List<int>();
    List<int> lastFilterLinesList = new List<int>();
    List<int> filterHitList = new List<int>();
    readonly private BookmarkDataProvider bookmarkProvider = new BookmarkDataProvider();
    private Object bookmarkLock = new Object();

    readonly IList<FilterPipe> filterPipeList = new List<FilterPipe>();
    FilterPipe filterPipe = null;
    int filterPipeNameCounter = 0;
    readonly ColumnizerCallback columnizerCallback;
    readonly Dictionary<Control, bool> freezeStateMap = new Dictionary<Control, bool>();
    SortedList<int, RowHeightEntry> rowHeightList = new SortedList<int, RowHeightEntry>();

    readonly List<LogEventArgs> logEventArgsList = new List<LogEventArgs>();
    readonly EventWaitHandle logEventArgsEvent = new ManualResetEvent(false);
    readonly Thread logEventHandlerThread = null;

    EventWaitHandle filterUpdateEvent = new ManualResetEvent(false);

    DelayedTrigger statusLineTrigger = new DelayedTrigger(200);
    DelayedTrigger selectionChangedTrigger = new DelayedTrigger(200);

    IList<BackgroundProcessCancelHandler> cancelHandlerList = new List<BackgroundProcessCancelHandler>();

    readonly EventWaitHandle loadingFinishedEvent = new ManualResetEvent(false);
    readonly EventWaitHandle externaLoadingFinishedEvent = new ManualResetEvent(false); // used for external wait fx WaitForLoadFinished()

    delegate void UpdateGridCallback(LogEventArgs e);
    delegate void UpdateProgressCallback(LoadFileEventArgs e);
    delegate void LoadingStartedFx(LoadFileEventArgs e);
    delegate int SearchFx(SearchParams searchParams);
    delegate void SelectLineFx(int line, bool triggerSyncCall);
    delegate void FilterFx(FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterResultLines, List<int> filterHitList);
    delegate void AddFilterLineGuiUpdateFx();
    delegate void UpdateProgressBarFx(int lineNum);
    delegate void SetColumnizerFx(ILogLineColumnizer columnizer);
    delegate void ProcessFilterPipeFx(int lineNum);
    delegate void WriteFilterToTabFinishedFx(FilterPipe pipe, string namePrefix, PersistenceData persistenceData);
    delegate void TimestampSyncFx(int lineNum);
    delegate void SetBookmarkFx(int lineNum, string comment);
    delegate void UpdateBookmarkViewFx();
    delegate void FunctionWith1IntParam(int arg);
    delegate void FunctionWith1BoolParam(bool arg);
    delegate void PatternStatisticFx(PatternArgs patternArgs);
    delegate void ActionPluginExecuteFx(string keyword, string param, ILogExpertCallback callback, ILogLineColumnizer columnizer);
    delegate void HighlightEventFx(HighlightEventArgs e);
    delegate void PositionAfterReloadFx(ReloadMemento reloadMemento);
    public delegate void LoadingFinishedFx(LogWindow newWin);    // used for filterTab restore
    public delegate void FilterRestoreFx(LogWindow newWin, PersistenceData persistenceData);
    public delegate void RestoreFiltersFx(PersistenceData persistenceData);
    public delegate void HideRowFx(int lineNum, bool show);
    public delegate bool ScrollToTimestampFx(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall);
    delegate void AutoResizeColumnsFx(DataGridView gridView);
    delegate bool BoolReturnDelegate();

    bool waitingForClose = false;
    bool isLoading = false;
    bool isSearching = false;
    bool shouldCancel = false;
    bool isMultiFile = false;
    bool isTempFile = false;
    bool showAdvanced = false;
    bool isErrorShowing = false;
    bool isTimestampDisplaySyncing = false;
    bool shouldTimestampDisplaySyncingCancel = false;
    bool isDeadFile = false;
    private bool forcePersistenceLoading = false;
    bool noSelectionUpdates = false;
    bool shouldCallTimeSync = false;
    bool isLoadError = false;

    int lineHeight = 0;
    int reloadOverloadCounter = 0;
    readonly Object reloadLock = new Object();
    int selectedCol = 0;    // set by context menu event for column headers only

    readonly ProgressEventArgs progressEventArgs = new ProgressEventArgs();
    readonly GuiStateArgs guiStateArgs = new GuiStateArgs();
    readonly StatusLineEventArgs statusEventArgs = new StatusLineEventArgs();

    readonly Thread timeshiftSyncThread = null;
    readonly EventWaitHandle timeshiftSyncWakeupEvent = new ManualResetEvent(false);
    readonly EventWaitHandle timeshiftSyncTimerEvent = new ManualResetEvent(false);
    int timeshiftSyncLine = 0;

    string fileNameField;
    string[] fileNames;
    readonly LogTabWindow parentLogTabWin;
    string tempTitleName = "";
    string sessionFileName = null;    // unused?
    private string forcedPersistenceFileName = null;
    private string givenFileName = null;      // file name of given file used for loading (maybe logfile or lxp)
    private EncodingOptions encodingOptions;
    private MultifileOptions multifileOptions = new MultifileOptions();

    readonly TimeSpreadCalculator timeSpreadCalc;
    PatternWindow patternWindow;
    PatternArgs patternArgs = new PatternArgs();

    readonly LoadingFinishedFx loadingFinishedFx;

    Image advancedButtonImage;
    Image searchButtonImage;

    Image panelOpenButtonImage;
    Image panelCloseButtonImage;

    private TimeSyncList timeSyncList = null;
    private Object timeSyncListLock = new Object();

    private Font font;
    private Font fontBold;
    private Font fontMonospaced;

    private ReloadMemento reloadMemento;
    private ColumnCache columnCache = new ColumnCache();

    public LogWindow(LogTabWindow parent, string fileName, bool isTempFile, LoadingFinishedFx loadingFinishedFx, bool forcePersistenceLoading)
    {
      this.SuspendLayout();

      InitializeComponent();

      this.columnNamesLabel.Text = ""; // no filtering on columns by default

      this.parentLogTabWin = parent;
      this.isTempFile = isTempFile;
      this.loadingFinishedFx = loadingFinishedFx;
      //Thread.CurrentThread.Name = "LogWindowThread";
      columnizerCallback = new ColumnizerCallback(this);


      this.fileNameField = fileName;
      this.ForcePersistenceLoading = forcePersistenceLoading;

      this.dataGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(dataGridView_CellValueNeeded);
      this.dataGridView.CellPainting += new DataGridViewCellPaintingEventHandler(dataGridView_CellPainting);

      this.filterGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(filterGridView_CellValueNeeded);
      this.filterGridView.CellPainting += new DataGridViewCellPaintingEventHandler(filterGridView_CellPainting);

      this.Closing += new CancelEventHandler(LogWindow_Closing);
      this.Disposed += new EventHandler(LogWindow_Disposed);

      this.timeSpreadCalc = new TimeSpreadCalculator(this);
      this.timeSpreadingControl1.TimeSpreadCalc = this.timeSpreadCalc;
      this.timeSpreadingControl1.LineSelected += new TimeSpreadingControl.LineSelectedEventHandler(timeSpreadingControl1_LineSelected);
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
      this.dataGridView.ColumnDividerDoubleClick += new DataGridViewColumnDividerDoubleClickEventHandler(dataGridView_ColumnDividerDoubleClick);
      ShowAdvancedFilterPanel(false);
      this.filterKnobControl1.MinValue = 0;
      this.filterKnobControl1.MaxValue = SPREAD_MAX;
      this.filterKnobControl1.ValueChanged += new KnobControl.ValueChangedEventHandler(filterKnobControl1_ValueChanged);
      this.filterKnobControl2.MinValue = 0;
      this.filterKnobControl2.MaxValue = SPREAD_MAX;
      this.filterKnobControl2.ValueChanged += new KnobControl.ValueChangedEventHandler(filterKnobControl2_ValueChanged);
      this.fuzzyKnobControl.MinValue = 0;
      this.fuzzyKnobControl.MaxValue = 10;
      //PreferencesChanged(settings.preferences, true);
      AdjustHighlightSplitterWidth();
      ToggleHighlightPanel(false); // hidden

      bookmarkProvider.BookmarkAdded += new BookmarkDataProvider.BookmarkAddedEventHandler(bookmarkProvider_BookmarkAdded);
      bookmarkProvider.BookmarkRemoved += new BookmarkDataProvider.BookmarkRemovedEventHandler(bookmarkProvider_BookmarkRemoved);
      bookmarkProvider.AllBookmarksRemoved += new BookmarkDataProvider.AllBookmarksRemovedEventHandler(bookmarkProvider_AllBookmarksRemoved);

      this.ResumeLayout();

      this.statusLineTrigger.Signal += new DelayedTrigger.SignalEventHandler(statusLineTrigger_Signal);
      this.selectionChangedTrigger.Signal += new DelayedTrigger.SignalEventHandler(selectionChangedTrigger_Signal);

      PreferencesChanged(this.parentLogTabWin.Preferences, true, SettingsFlags.GuiOrColors);
    }

    ~LogWindow()
    {
    }

    private void LogWindow_Disposed(object sender, EventArgs e)
    {
      this.waitingForClose = true;
      this.parentLogTabWin.HighlightSettingsChanged -= parent_HighlightSettingsChanged;
      if (this.logFileReader != null)
      {
        this.logFileReader.DeleteAllContent();
      }
      FreeFromTimeSync();
    }

    public void LoadFile(string fileName, EncodingOptions encodingOptions)
    {
#if DEBUG
      //MessageBox.Show("Pause vor LoadFile()");
#endif

      EnterLoadFileStatus();

      if (fileName != null)
      {
        this.fileNameField = fileName;
        this.EncodingOptions = encodingOptions;

        if (this.logFileReader != null)
        {
          this.logFileReader.StopMonitoringAsync();
          UnRegisterLogFileReaderEvents();
        }
        if (!LoadPersistenceOptions())
        {
          if (!this.IsTempFile)
          {
            ILogLineColumnizer columnizer = FindColumnizer();
            if (columnizer != null)
            {
              if (this.reloadMemento == null)
              {
                columnizer = Util.CloneColumnizer(columnizer);
              }
            }
            PreSelectColumnizer(columnizer);
          }
          SetDefaultHighlightGroup();
        }

        // this may be set after loading persistence data
        if (this.fileNames != null && this.IsMultiFile)
        {
          LoadFilesAsMulti(this.fileNames, this.EncodingOptions);
          return;
        }
        this.columnCache = new ColumnCache();
        try
        {
          this.logFileReader = new LogfileReader(fileName, this.EncodingOptions, this.IsMultiFile,
                               this.Preferences.bufferCount, this.Preferences.linesPerBuffer,
                               this.multifileOptions);
          this.logFileReader.UseNewReader = !this.Preferences.useLegacyReader;
        }
        catch (LogFileException lfe)
        {
          MessageBox.Show("Cannot load file\n" + lfe.Message, "LogExpert");
          this.BeginInvoke(new FunctionWith1BoolParam(Close), new object[] { true });
          this.isLoadError = true;
          return;
        }

        if (this.CurrentColumnizer is ILogLineXmlColumnizer)
        {
          this.logFileReader.IsXmlMode = true;
          this.logFileReader.XmlLogConfig = (this.CurrentColumnizer as ILogLineXmlColumnizer).GetXmlLogConfiguration();
        }
        if (this.forcedColumnizerForLoading != null)
        {
          this.CurrentColumnizer = this.forcedColumnizerForLoading;
        }
        if (this.CurrentColumnizer is IPreProcessColumnizer)
        {
          this.logFileReader.PreProcessColumnizer = (IPreProcessColumnizer)this.CurrentColumnizer;
        }
        else
        {
          this.logFileReader.PreProcessColumnizer = null;
        }
        RegisterLogFileReaderEvents();
        Logger.logInfo("Loading logfile: " + fileName);
        this.logFileReader.startMonitoring();
      }
    }

    private void RegisterLogFileReaderEvents()
    {
      this.logFileReader.LoadFile += logFileReader_LoadFile;
      this.logFileReader.LoadingFinished += logFileReader_FinishedLoading;
      this.logFileReader.LoadingStarted += logFileReader_LoadingStarted;
      this.logFileReader.FileNotFound += logFileReader_FileNotFound;
      this.logFileReader.Respawned += logFileReader_Respawned;
      // FileSizeChanged is not registered here because it's registered after loading has finished
    }

    private void UnRegisterLogFileReaderEvents()
    {
      if (this.logFileReader != null)
      {
        this.logFileReader.LoadFile -= logFileReader_LoadFile;
        this.logFileReader.LoadingFinished -= logFileReader_FinishedLoading;
        this.logFileReader.LoadingStarted -= logFileReader_LoadingStarted;
        this.logFileReader.FileNotFound -= logFileReader_FileNotFound;
        this.logFileReader.Respawned -= logFileReader_Respawned;
        this.logFileReader.FileSizeChanged -= this.FileSizeChangedHandler;
      }
    }

    private bool LoadPersistenceOptions()
    {
      if (this.InvokeRequired)
      {
        return (bool)this.Invoke(new BoolReturnDelegate(LoadPersistenceOptions));
      }

      if (!this.Preferences.saveSessions && this.ForcedPersistenceFileName == null)
        return false;

      try
      {
        PersistenceData persistenceData;
        if (this.ForcedPersistenceFileName == null)
          persistenceData = Persister.LoadPersistenceDataOptionsOnly(this.FileName, this.Preferences);
        else
          persistenceData = Persister.LoadPersistenceDataOptionsOnlyFromFixedFile(this.ForcedPersistenceFileName);

        if (persistenceData == null)
        {
          Logger.logInfo("No persistence data for " + this.FileName + " found.");
          return false;
        }

        this.IsMultiFile = persistenceData.multiFile;
        this.multifileOptions = new MultifileOptions();
        this.multifileOptions.FormatPattern = persistenceData.multiFilePattern;
        this.multifileOptions.MaxDayTry = persistenceData.multiFileMaxDays;
        if (this.multifileOptions.FormatPattern == null || this.multifileOptions.FormatPattern.Length == 0)
        {
          this.multifileOptions = ObjectClone.Clone<MultifileOptions>(this.Preferences.multifileOptions);
        }

        this.splitContainer1.SplitterDistance = persistenceData.filterPosition;
        this.splitContainer1.Panel2Collapsed = !persistenceData.filterVisible;
        ToggleHighlightPanel(persistenceData.filterSaveListVisible);
        ShowAdvancedFilterPanel(persistenceData.filterAdvanced);
        if (reloadMemento == null)
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
          this.fileNames = new string[persistenceData.multiFileNames.Count];
          persistenceData.multiFileNames.CopyTo(this.fileNames);
        }
        else
        {
          this.fileNames = null;
        }
        //this.bookmarkWindow.ShowBookmarkCommentColumn = persistenceData.showBookmarkCommentColumn;
        SetExplicitEncoding(persistenceData.encoding);
        return true;
      }
      catch (Exception ex)
      {
        Logger.logError("Error loading persistence data: " + ex.Message);
        return false;
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
      if (this.logFileReader != null)
      {
        this.logFileReader.stopMonitoring();
        UnRegisterLogFileReaderEvents();
      }
      this.EncodingOptions = encodingOptions;
      this.columnCache = new ColumnCache();
      this.logFileReader = new LogfileReader(fileNames, this.EncodingOptions, this.Preferences.bufferCount,
                           this.Preferences.linesPerBuffer, this.multifileOptions);
      this.logFileReader.UseNewReader = !this.Preferences.useLegacyReader;
      RegisterLogFileReaderEvents();
      this.logFileReader.startMonitoring();
      this.fileNameField = fileNames[fileNames.Length - 1];
      this.fileNames = fileNames;
      this.IsMultiFile = true;
      //if (this.isTempFile)
      //  this.Text = this.tempTitleName;
      //else
      //  this.Text = Util.GetNameFromPath(this.FileName);
    }

    private void SetDefaultsFromPrefs()
    {
      this.filterTailCheckBox.Checked = this.Preferences.filterTail;
      this.syncFilterCheckBox.Checked = this.Preferences.filterSync;
      this.FollowTailChanged(this.Preferences.followTail, false);
      this.multifileOptions = ObjectClone.Clone<MultifileOptions>(this.Preferences.multifileOptions);
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

      if (this.isTempFile)
      {
        SetDefaultsFromPrefs();
        return;
      }

      ForcePersistenceLoading = false;  // force only 1 time (while session load)

      try
      {
        PersistenceData persistenceData;
        if (this.ForcedPersistenceFileName == null)
          persistenceData = Persister.LoadPersistenceData(this.FileName, this.Preferences);
        else
          persistenceData = Persister.LoadPersistenceDataFromFixedFile(this.ForcedPersistenceFileName);

        if (persistenceData.lineCount > this.logFileReader.LineCount)
        {
          // outdated persistence data (logfile rollover)
          // MessageBox.Show(this, "Persistence data for " + this.FileName + " is outdated. It was discarded.", "Log Expert");
          Logger.logInfo("Persistence data for " + this.FileName + " is outdated. It was discarded.");
          LoadPersistenceOptions();
          return;
        }
        this.bookmarkProvider.BookmarkList = persistenceData.bookmarkList;
        this.rowHeightList = persistenceData.rowHeightList;
        try
        {
          if (persistenceData.currentLine >= 0 && persistenceData.currentLine < this.dataGridView.RowCount)
          {
            SelectLine(persistenceData.currentLine, false);
          }
          else
          {
            if (this.logFileReader.LineCount > 0)
            {
              this.dataGridView.FirstDisplayedScrollingRowIndex = this.logFileReader.LineCount - 1;
              SelectLine(this.logFileReader.LineCount - 1, false);
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
        this.filterParams = persistenceData.filterParamsList[0];
        ReInitFilterParams(this.filterParams);
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
      if (this.filterPipeList.Count == 0)     // don't restore if it's only a reload
      {
        RestoreFilterTabs(persistenceData);
      }
    }

    public string SavePersistenceData(bool force)
    {
      if (!force)
      {
        if (!this.Preferences.saveSessions)
          return null;
      }

      if (this.isTempFile || this.isLoadError)
        return null;

      try
      {
        PersistenceData persistenceData = GetPersistenceData();
        if (this.ForcedPersistenceFileName == null)
          return Persister.SavePersistenceData(this.FileName, persistenceData, this.Preferences);
        else
          return Persister.SavePersistenceDataWithFixedName(this.ForcedPersistenceFileName, persistenceData);
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
      persistenceData.bookmarkList = this.bookmarkProvider.BookmarkList;
      persistenceData.rowHeightList = this.rowHeightList;
      persistenceData.multiFile = this.IsMultiFile;
      persistenceData.multiFilePattern = this.multifileOptions.FormatPattern;
      persistenceData.multiFileMaxDays = this.multifileOptions.MaxDayTry;
      persistenceData.currentLine = this.dataGridView.CurrentCellAddress.Y;
      persistenceData.firstDisplayedLine = this.dataGridView.FirstDisplayedScrollingRowIndex;
      persistenceData.filterVisible = !this.splitContainer1.Panel2Collapsed;
      persistenceData.filterAdvanced = !this.advancedFilterSplitContainer.Panel1Collapsed;
      persistenceData.filterPosition = this.splitContainer1.SplitterDistance;
      persistenceData.followTail = this.guiStateArgs.FollowTail;
      persistenceData.fileName = this.FileName;
      persistenceData.tabName = this.Text;
      persistenceData.sessionFileName = this.SessionFileName;
      persistenceData.columnizerName = this.CurrentColumnizer.GetName();
      persistenceData.lineCount = this.logFileReader.LineCount;
      this.filterParams.isFilterTail = this.filterTailCheckBox.Checked; // this option doesnt need a press on 'search'
      if (this.Preferences.saveFilters)
      {
        List<FilterParams> filterList = new List<FilterParams>();
        filterList.Add(this.filterParams);
        persistenceData.filterParamsList = filterList;

        foreach (FilterPipe filterPipe in this.filterPipeList)
        {
          FilterTabData data = new FilterTabData();
          data.persistenceData = filterPipe.OwnLogWindow.GetPersistenceData();
          data.filterParams = filterPipe.FilterParams;
          persistenceData.filterTabDataList.Add(data);
        }
      }
      if (this.currentHighlightGroup != null)
      {
        persistenceData.highlightGroupName = this.currentHighlightGroup.GroupName;
      }
      if (this.fileNames != null && this.IsMultiFile)
      {
        persistenceData.multiFileNames.AddRange(this.fileNames);
      }
      //persistenceData.showBookmarkCommentColumn = this.bookmarkWindow.ShowBookmarkCommentColumn;
      persistenceData.filterSaveListVisible = !this.highlightSplitContainer.Panel2Collapsed;
      persistenceData.encoding = this.logFileReader.CurrentEncoding;
      return persistenceData;
    }

    private void RestoreFilterTabs(PersistenceData persistenceData)
    {
      foreach (FilterTabData data in persistenceData.filterTabDataList)
      {
        FilterParams persistFilterParams = data.filterParams;
        ReInitFilterParams(persistFilterParams);
        List<int> filterResultList = new List<int>();
        List<int> lastFilterResultList = new List<int>();
        List<int> filterHitList = new List<int>();
        Filter(persistFilterParams, filterResultList, lastFilterLinesList, filterHitList);
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
      this.statusEventArgs.StatusText = "Loading file...";
      this.statusEventArgs.LineCount = 0;
      this.statusEventArgs.FileSize = 0;
      SendStatusLineUpdate();

      this.progressEventArgs.MinValue = 0;
      this.progressEventArgs.MaxValue = 0;
      this.progressEventArgs.Value = 0;
      this.progressEventArgs.Visible = true;
      SendProgressBarUpdate();

      this.isLoading = true;
      this.shouldCancel = true;
      ClearFilterList();
      ClearBookmarkList();
      this.dataGridView.ClearSelection();
      this.dataGridView.RowCount = 0;
      Logger.logDebug("EnterLoadFileStatus end");
    }

    private void logFileReader_LoadingStarted(object sender, LoadFileEventArgs e)
    {
      this.Invoke(new LoadingStartedFx(LoadingStarted), new object[] { e });
    }

    private void logFileReader_FinishedLoading(object sender, EventArgs e)
    {
      //Thread.CurrentThread.Name = "FinishedLoading event thread";
      Logger.logInfo("Finished loading.");
      this.isLoading = false;
      this.isDeadFile = false;
      if (!this.waitingForClose)
      {
        this.Invoke(new MethodInvoker(LoadingFinished));
        this.Invoke(new MethodInvoker(LoadPersistenceData));
        this.Invoke(new MethodInvoker(SetGuiAfterLoading));
        this.loadingFinishedEvent.Set();
        this.externaLoadingFinishedEvent.Set();
        this.timeSpreadCalc.SetLineCount(this.logFileReader.LineCount);
        if (this.loadingFinishedFx != null)
        {
          this.loadingFinishedFx(this);
        }

        if (this.reloadMemento != null)
        {
          this.Invoke(new PositionAfterReloadFx(this.PositionAfterReload), new object[] { this.reloadMemento });
        }
        if (this.filterTailCheckBox.Checked)
        {
          Logger.logInfo("Refreshing filter view because of reload.");
          this.Invoke(new MethodInvoker(FilterSearch)); // call on proper thread
        }

        HandleChangedFilterList();
      }
      this.reloadMemento = null;
    }

    private void logFileReader_FileNotFound(object sender, EventArgs e)
    {
      if (!this.IsDisposed && !this.Disposing)
      {
        Logger.logInfo("Handling file not found event.");
        this.isDeadFile = true;
        this.BeginInvoke(new MethodInvoker(LogfileDead));
      }
    }

    private void logFileReader_Respawned(object sender, EventArgs e)
    {
      this.BeginInvoke(new MethodInvoker(LogfileRespawned));
    }

    private void PositionAfterReload(ReloadMemento reloadMemento)
    {
      if (this.reloadMemento.currentLine < this.dataGridView.RowCount && this.reloadMemento.currentLine >= 0)
        this.dataGridView.CurrentCell = this.dataGridView.Rows[this.reloadMemento.currentLine].Cells[0];
      if (this.reloadMemento.firstDisplayedLine < this.dataGridView.RowCount && this.reloadMemento.firstDisplayedLine >= 0)
        this.dataGridView.FirstDisplayedScrollingRowIndex = this.reloadMemento.firstDisplayedLine;
    }

    private void LogfileDead()
    {
      Logger.logInfo("File not found.");
      this.isDeadFile = true;

      //this.logFileReader.FileSizeChanged -= this.FileSizeChangedHandler;
      //if (this.logFileReader != null)
      //  this.logFileReader.stopMonitoring();

      this.dataGridView.Enabled = false;
      this.dataGridView.RowCount = 0;
      this.progressEventArgs.Visible = false;
      this.progressEventArgs.Value = this.progressEventArgs.MaxValue;
      SendProgressBarUpdate();
      this.statusEventArgs.FileSize = 0;
      this.statusEventArgs.LineCount = 0;
      this.statusEventArgs.CurrentLineNum = 0;
      SendStatusLineUpdate();
      this.shouldCancel = true;
      ClearFilterList();
      ClearBookmarkList();

      StatusLineText("File not found");
      OnFileNotFound(new EventArgs());
    }

    private void LogfileRespawned()
    {
      Logger.logInfo("LogfileDead(): Reloading file because it has been respawned.");
      this.isDeadFile = false;
      this.dataGridView.Enabled = true;
      StatusLineText("");
      OnFileRespawned(new EventArgs());
      Reload();
    }

    private void SetGuiAfterLoading()
    {
      if (this.Text.Length == 0)
      {
        if (this.isTempFile)
          this.Text = this.tempTitleName;
        else
          this.Text = Util.GetNameFromPath(this.FileName);
      }
      this.ShowBookmarkBubbles = this.Preferences.showBubbles;
      //if (this.forcedColumnizer == null)
      {
        ILogLineColumnizer columnizer;
        if (this.forcedColumnizerForLoading != null)
        {
          columnizer = this.forcedColumnizerForLoading;
          this.forcedColumnizerForLoading = null;
        }
        else
        {
          columnizer = FindColumnizer();
          if (columnizer != null)
          {
            if (this.reloadMemento == null)
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
      }
      this.dataGridView.Enabled = true;
      DisplayCurrentFileOnStatusline();
      //this.guiStateArgs.FollowTail = this.Preferences.followTail;
      this.guiStateArgs.MultiFileEnabled = !this.IsTempFile;
      this.guiStateArgs.MenuEnabled = true;
      this.guiStateArgs.CurrentEncoding = this.logFileReader.CurrentEncoding;
      SendGuiStateUpdate();
      //if (this.dataGridView.RowCount > 0)
      //  SelectLine(this.dataGridView.RowCount - 1);
      //if (this.dataGridView.Columns.Count > 1)
      //{
      //  this.dataGridView.Columns[this.dataGridView.Columns.Count-1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
      //  this.dataGridView.Columns[this.dataGridView.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
      //  AdjustMinimumGridWith();
      //}
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
        columnizer = this.parentLogTabWin.FindColumnizerByFileMask(Util.GetNameFromPath(this.FileName));
        if (columnizer == null)
        {
          columnizer = this.parentLogTabWin.GetColumnizerHistoryEntry(this.FileName);
        }
      }
      else
      {
        columnizer = this.parentLogTabWin.GetColumnizerHistoryEntry(this.FileName);
        if (columnizer == null)
        {
          columnizer = this.parentLogTabWin.FindColumnizerByFileMask(Util.GetNameFromPath(this.FileName));
        }
      }
      return columnizer;
    }

    private void LogWindow_Closing(object sender, CancelEventArgs e)
    {
      if (this.Preferences.askForClose)
      {
        if (MessageBox.Show("Sure to close?", "LogExpert", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
          == DialogResult.No)
        {
          e.Cancel = true;
          return;
        }
      }
      SavePersistenceData(false);
      CloseLogWindow();
    }

    public void Close(bool dontAsk)
    {
      this.Preferences.askForClose = !dontAsk;
      Close();
    }

    public void CloseLogWindow()
    {
      StopTimespreadThread();
      StopTimestampSyncThread();
      StopLogEventWorkerThread();
      this.statusLineTrigger.Stop();
      this.selectionChangedTrigger.Stop();
      //StopFilterUpdateWorkerThread();
      this.shouldCancel = true;
      if (this.logFileReader != null)
      {
        UnRegisterLogFileReaderEvents();
        this.logFileReader.StopMonitoringAsync();
        //this.logFileReader.DeleteAllContent();
      }
      if (this.isLoading)
      {
        this.waitingForClose = true;
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

    private void dataGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
    {
      e.Handled = true;
      AutoResizeColumns(this.dataGridView);
    }

    /**
     * Event handler for the Load event from LogfileReader
     */
    private void logFileReader_LoadFile(object sender, LoadFileEventArgs e)
    {
      if (e.NewFile)
      {
        Logger.logInfo("File created anew.");

        // File was new created (e.g. rollover)
        this.isDeadFile = false;
        UnRegisterLogFileReaderEvents();
        this.dataGridView.CurrentCellChanged -= new EventHandler(dataGridView_CurrentCellChanged);
        MethodInvoker invoker = new MethodInvoker(ReloadNewFile);
        this.BeginInvoke(invoker);
        //Thread loadThread = new Thread(new ThreadStart(ReloadNewFile));
        //loadThread.Start();
        Logger.logDebug("Reloading invoked.");
        return;
      }

      if (!this.isLoading)
        return;
      UpdateProgressCallback callback = new UpdateProgressCallback(UpdateProgress);
      this.BeginInvoke(callback, new object[] { e });
    }

    private void ReloadNewFile()
    {
      // prevent "overloads". May occur on very fast rollovers (next rollover before the file is reloaded)
      lock (this.reloadLock)
      {
        this.reloadOverloadCounter++;
        Logger.logInfo("ReloadNewFile(): counter = " + this.reloadOverloadCounter);
        if (this.reloadOverloadCounter <= 1)
        {
          SavePersistenceData(false);
          loadingFinishedEvent.Reset();
          externaLoadingFinishedEvent.Reset();
          Thread reloadFinishedThread = new Thread(new ThreadStart(ReloadFinishedThreadFx));
          reloadFinishedThread.IsBackground = true;
          reloadFinishedThread.Start();
          LoadFile(this.FileName, this.EncodingOptions);

          ClearBookmarkList();
          SavePersistenceData(false);

          //if (this.filterTailCheckBox.Checked)
          //{
          //  Logger.logDebug("Waiting for loading to be complete.");
          //  loadingFinishedEvent.WaitOne();
          //  Logger.logDebug("Refreshing filter view because of reload.");
          //  FilterSearch();
          //}
          //LoadFilterPipes();
        }
        else
        {
          Logger.logDebug("Preventing reload because of recursive calls.");
        }
        this.reloadOverloadCounter--;
      }
    }

    private void ReloadFinishedThreadFx()
    {
      Logger.logInfo("Waiting for loading to be complete.");
      this.loadingFinishedEvent.WaitOne();
      Logger.logInfo("Refreshing filter view because of reload.");
      this.Invoke(new MethodInvoker(FilterSearch));
      LoadFilterPipes();
      OnFileReloadFinished();
    }

    public void WaitForLoadingFinished()
    {
      this.externaLoadingFinishedEvent.WaitOne();
    }

    private void UpdateProgress(LoadFileEventArgs e)
    {
      try
      {
        if (e.ReadPos >= e.FileSize)
        {
          //Logger.logWarn("UpdateProgress(): ReadPos (" + e.ReadPos + ") is greater than file size (" + e.FileSize + "). Aborting Update");
          return;
        }

        this.statusEventArgs.FileSize = e.ReadPos;
        //this.progressEventArgs.Visible = true;
        this.progressEventArgs.MaxValue = (int)e.FileSize;
        this.progressEventArgs.Value = (int)e.ReadPos;
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
        this.statusEventArgs.FileSize = e.ReadPos;
        this.statusEventArgs.StatusText = "Loading " + Util.GetNameFromPath(e.FileName);
        this.progressEventArgs.Visible = true;
        this.progressEventArgs.MaxValue = (int)e.FileSize;
        this.progressEventArgs.Value = (int)e.ReadPos;
        SendProgressBarUpdate();
        SendStatusLineUpdate();
      }
      catch (Exception ex)
      {
        Logger.logError("LoadingStarted(): " + ex + "\n" + ex.StackTrace);
      }
    }

    private void LoadingFinished()
    {
      Logger.logInfo("File loading complete.");
      StatusLineText("");
      this.logFileReader.FileSizeChanged += this.FileSizeChangedHandler;
      this.isLoading = false;
      this.shouldCancel = false;
      this.dataGridView.SuspendLayout();
      this.dataGridView.RowCount = this.logFileReader.LineCount;
      this.dataGridView.CurrentCellChanged += new EventHandler(dataGridView_CurrentCellChanged);
      this.dataGridView.Enabled = true;
      this.dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
      this.dataGridView.ResumeLayout();
      this.progressEventArgs.Visible = false;
      this.progressEventArgs.Value = this.progressEventArgs.MaxValue;
      SendProgressBarUpdate();
      //if (this.logFileReader.LineCount > 0)
      //{
      //  this.dataGridView.FirstDisplayedScrollingRowIndex = this.logFileReader.LineCount - 1;
      //  SelectLine(this.logFileReader.LineCount - 1);
      //}
      this.guiStateArgs.FollowTail = true;
      SendGuiStateUpdate();
      this.statusEventArgs.LineCount = this.logFileReader.LineCount;
      this.statusEventArgs.FileSize = this.logFileReader.FileSize;
      SendStatusLineUpdate();

      PreferencesChanged(this.parentLogTabWin.Preferences, true, SettingsFlags.All);
      //LoadPersistenceData();
    }


    private void FileSizeChangedHandler(object sender, LogEventArgs e)
    {
      //OnFileSizeChanged(e);  // now done in UpdateGrid()
      Logger.logInfo("Got FileSizeChanged event. prevLines:" + e.PrevLineCount + ", curr lines: " + e.LineCount);

      // - now done in the thread that works on the event args list
      //if (e.IsRollover)
      //{
      //  ShiftBookmarks(e.RolloverOffset);
      //  ShiftFilterPipes(e.RolloverOffset);
      //}

      //UpdateGridCallback callback = new UpdateGridCallback(UpdateGrid);
      //this.BeginInvoke(callback, new object[] { e });
      lock (this.logEventArgsList)
      {
        this.logEventArgsList.Add(e);
        this.logEventArgsEvent.Set();
      }
    }

    private void LogEventWorker()
    {
      Thread.CurrentThread.Name = "LogEventWorker";
      while (true)
      {
        Logger.logDebug("Waiting for signal");
        this.logEventArgsEvent.WaitOne();
        Logger.logDebug("Wakeup signal received.");
        while (true)
        {
          LogEventArgs e;
          int lastLineCount = 0;
          lock (this.logEventArgsList)
          {
            Logger.logInfo("" + this.logEventArgsList.Count + " events in queue");
            if (this.logEventArgsList.Count == 0)
            {
              this.logEventArgsEvent.Reset();
              break;
            }
            e = this.logEventArgsList[0];
            this.logEventArgsList.RemoveAt(0);
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
          this.timeSpreadCalc.SetLineCount(e.LineCount);
        }
      }
    }

    private void StopLogEventWorkerThread()
    {
      this.logEventArgsEvent.Set();
      this.logEventHandlerThread.Abort();
      this.logEventHandlerThread.Join();
    }

    public delegate void FileSizeChangedEventHandler(object sender, LogEventArgs e);
    public event FileSizeChangedEventHandler FileSizeChanged;
    private void OnFileSizeChanged(LogEventArgs e)
    {
      if (FileSizeChanged != null)
        FileSizeChanged(this, e);
    }

    private void UpdateGrid(LogEventArgs e)
    {
      int oldRowCount = this.dataGridView.RowCount;
      int firstDisplayedLine = this.dataGridView.FirstDisplayedScrollingRowIndex;

      if (this.dataGridView.CurrentCellAddress.Y >= e.LineCount)
      {
        //this.dataGridView.Rows[this.dataGridView.CurrentCellAddress.Y].Selected = false;
        //this.dataGridView.CurrentCell = this.dataGridView.Rows[0].Cells[0];
      }
      try
      {
        if (this.dataGridView.RowCount > e.LineCount)
        {
          int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
          this.dataGridView.RowCount = 0;
          this.dataGridView.RowCount = e.LineCount;
          if (!this.guiStateArgs.FollowTail)
          {
            if (currentLineNum >= this.dataGridView.RowCount)
              currentLineNum = this.dataGridView.RowCount - 1;
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
          if (!this.guiStateArgs.FollowTail)
          {
            int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
            currentLineNum -= e.RolloverOffset;
            if (currentLineNum < 0)
              currentLineNum = 0;
            Logger.logDebug("UpdateGrid(): Rollover=true, Rollover offset=" + e.RolloverOffset + ", currLineNum was " + this.dataGridView.CurrentCellAddress.Y + ", new currLineNum=" + currentLineNum);
            firstDisplayedLine -= e.RolloverOffset;
            if (firstDisplayedLine < 0)
              firstDisplayedLine = 0;
            this.dataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedLine;
            this.dataGridView.CurrentCell = this.dataGridView.Rows[currentLineNum].Cells[0];
            this.dataGridView.Rows[currentLineNum].Selected = true;
          }
        }
        this.statusEventArgs.LineCount = e.LineCount;
        statusLineFileSize(e.FileSize);

        if (!this.isLoading)
        {
          if (oldRowCount == 0)
          {
            AdjustMinimumGridWith();
          }

          //CheckFilterAndHighlight(e);
        }
        if (this.guiStateArgs.FollowTail && this.dataGridView.RowCount > 0)
        {
          this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.RowCount - 1;
          OnTailFollowed(new EventArgs());
        }
        if (this.Preferences.timestampControl && !this.isLoading)
        {
          SetTimestampLimits();
        }

      }
      catch (Exception ex)
      {
        Logger.logError("Fehler bei UpdateGrid(): " + ex + "\n" + ex.StackTrace);
      }

      //this.dataGridView.Refresh();
      //this.dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private void CheckFilterAndHighlight(LogEventArgs e)
    {
      bool noLed = true;
      bool suppressLed = false;
      bool setBookmark = false;
      bool stopTail = false;
      string bookmarkComment = null;
      if (this.filterTailCheckBox.Checked || this.filterPipeList.Count > 0)
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
          string line = this.logFileReader.GetLogLine(i);
          if (line == null)
            return;
          if (this.filterTailCheckBox.Checked)
          {
            callback.LineNum = i;
            if (Util.TestFilterCondition(this.filterParams, line, callback))
            {
              //AddFilterLineFx addFx = new AddFilterLineFx(AddFilterLine);
              //this.Invoke(addFx, new object[] { i, true });
              filterLineAdded = true;
              AddFilterLine(i, false, this.filterParams, this.filterResultList, this.lastFilterLinesList, this.filterHitList);
            }
          }
          //ProcessFilterPipeFx pipeFx = new ProcessFilterPipeFx(ProcessFilterPipes);
          //pipeFx.BeginInvoke(i, null, null);
          ProcessFilterPipes(i);

          IList<HilightEntry> matchingList = FindMatchingHilightEntries(line);
          LaunchHighlightPlugins(matchingList, i);
          GetHilightActions(matchingList, out suppressLed, out stopTail, out setBookmark, out bookmarkComment);
          if (setBookmark)
          {
            SetBookmarkFx fx = new SetBookmarkFx(this.SetBookmarkFromTrigger);
            fx.BeginInvoke(i, bookmarkComment, null, null);
          }
          if (stopTail && this.guiStateArgs.FollowTail)
          {
            bool wasFollow = this.guiStateArgs.FollowTail;
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
          //AddFilterLineGuiUpdateFx addFx = new AddFilterLineGuiUpdateFx(AddFilterLineGuiUpdate);
          //this.Invoke(addFx);
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
          string line = this.logFileReader.GetLogLine(i);
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
            if (stopTail && this.guiStateArgs.FollowTail)
            {
              bool wasFollow = this.guiStateArgs.FollowTail;
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

    public ILogLineColumnizer CurrentColumnizer
    {
      get { return this.currentColumnizer; }
      set
      {
        lock (this.currentColumnizerLock)
        {
          this.currentColumnizer = value;
          Logger.logDebug("Setting columnizer " + this.currentColumnizer != null ? this.currentColumnizer.GetName() : "<none>");
        }
      }
    }

    public void ForceColumnizer(ILogLineColumnizer columnizer)
    {
      this.forcedColumnizer = Util.CloneColumnizer(columnizer);
      SetColumnizer(this.forcedColumnizer);
    }

    public void ForceColumnizerForLoading(ILogLineColumnizer columnizer)
    {
      this.forcedColumnizerForLoading = Util.CloneColumnizer(columnizer);
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
        this.CurrentColumnizer = this.forcedColumnizerForLoading = columnizer;
      }
      else
      {
        this.CurrentColumnizer = this.forcedColumnizerForLoading = PluginRegistry.GetInstance().RegisteredColumnizers[0];
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
      if (this.filterParams.columnRestrict)
      {
        string[] newColumns = columnizer != null ? columnizer.GetColumnNames() : new string[0];
        bool colChanged = false;
        if (this.dataGridView.ColumnCount - 2 == newColumns.Length) // two first columns are 'marker' and 'line number'
        {
          for (int i = 0; i < newColumns.Length; i++)
            if (this.dataGridView.Columns[i].HeaderText != newColumns[i])
            {
              colChanged = true;
              break; // one change is sufficient
            }
        }
        else
          colChanged = true;

        if (colChanged)
        {
          // Update UI
          this.columnNamesLabel.Text = CalculateColumnNames(this.filterParams);
        }
      }

      Type oldColType = this.filterParams.currentColumnizer != null ? this.filterParams.currentColumnizer.GetType() : null;
      Type newColType = columnizer != null ? columnizer.GetType() : null;
      if (oldColType != newColType && this.filterParams.columnRestrict && this.filterParams.isFilterTail)
      {
        this.filterParams.columnList.Clear();
      }
      if (this.CurrentColumnizer == null || !this.CurrentColumnizer.GetType().Equals(columnizer.GetType()))
      {
        this.CurrentColumnizer = columnizer;
        this.freezeStateMap.Clear();
        if (this.logFileReader != null)
        {
          if (this.CurrentColumnizer is IPreProcessColumnizer)
          {
            this.logFileReader.PreProcessColumnizer = (IPreProcessColumnizer)this.CurrentColumnizer;
          }
          else
          {
            this.logFileReader.PreProcessColumnizer = null;
          }
        }
        // always reload when choosing XML columnizers
        if (this.logFileReader != null && this.CurrentColumnizer is ILogLineXmlColumnizer)
        {
          //forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
          mustReload = true;
        }
        // Reload when choosing no XML columnizer but previous columnizer was XML
        if (this.logFileReader != null && !(this.CurrentColumnizer is ILogLineXmlColumnizer) && oldColumnizerIsXmlType)
        {
          this.logFileReader.IsXmlMode = false;
          //forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
          mustReload = true;
        }
        // Reload when previous columnizer was PreProcess and current is not, and vice versa.
        // When the current columnizer is a preProcess columnizer, reload in every case.
        if (((this.CurrentColumnizer is IPreProcessColumnizer) != oldColumnizerIsPreProcess) ||
          (this.CurrentColumnizer is IPreProcessColumnizer)
           )
        {
          //forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
          mustReload = true;
        }
      }
      else
      {
        this.CurrentColumnizer = columnizer;
      }

      if (oldColumnizer is IInitColumnizer)
      {
        (oldColumnizer as IInitColumnizer).DeSelected(new ColumnizerCallback(this));
      }
      if (columnizer is IInitColumnizer)
      {
        (columnizer as IInitColumnizer).Selected(new ColumnizerCallback(this));
      }

      SetColumnizer(columnizer, this.dataGridView);
      SetColumnizer(columnizer, this.filterGridView);
      if (this.patternWindow != null)
      {
        this.patternWindow.SetColumnizer(columnizer);
      }

      this.guiStateArgs.TimeshiftPossible = columnizer.IsTimeshiftImplemented();
      SendGuiStateUpdate();

      if (this.logFileReader != null)
        this.dataGridView.RowCount = this.logFileReader.LineCount;
      if (this.filterResultList != null)
        this.filterGridView.RowCount = this.filterResultList.Count;
      if (mustReload)
      {
        Reload();
      }
      else
      {
        if (this.CurrentColumnizer.IsTimeshiftImplemented())
        {
          SetTimestampLimits();
          SyncTimestampDisplay();
        }
        Settings settings = ConfigManager.Settings;
        ShowLineColumn(!settings.hideLineColumn);
        ShowTimeSpread(this.Preferences.showTimeSpread && columnizer.IsTimeshiftImplemented());
      }

      if (!columnizer.IsTimeshiftImplemented() && this.IsTimeSynced)
      {
        FreeFromTimeSync();
      }

      this.columnComboBox.Items.Clear();
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

      this.columnNamesLabel.Text = CalculateColumnNames(this.filterParams);

      //gridView.Columns[gridView.Columns.Count - 1].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
      //gridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

      gridView.RowCount = rowCount;
      if (currLine != -1)
        gridView.CurrentCell = gridView.Rows[currLine].Cells[0];
      if (currFirstLine != -1)
        gridView.FirstDisplayedScrollingRowIndex = currFirstLine;
      gridView.Refresh();
      AutoResizeColumns(gridView);
      ApplyFrozenState(gridView);
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

    internal string[] GetColumnsForLine(int lineNumber)
    {
      return this.columnCache.GetColumnsForLine(this.logFileReader, lineNumber, this.CurrentColumnizer, this.columnizerCallback);

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
              value = value.Replace("\t", "  ");
            return value;
          }
          else
          {
            if (columnIndex == 2)
              return cols[cols.Length - 1].Replace("\t", "  ");
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

    private void dataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
       e.Value = GetCellValue(e.RowIndex, e.ColumnIndex);
    }

    public void CellPainting(DataGridView gridView, int rowIndex, DataGridViewCellPaintingEventArgs e)
    {
      if (rowIndex < 0 || e.ColumnIndex < 0)
      {
        e.Handled = false;
        return;
      }
      string line = this.logFileReader.GetLogLineWithWait(rowIndex);
      if (line != null)
      {
        HilightEntry entry = FindFirstNoWordMatchHilightEntry(line);
        e.Graphics.SetClip(e.CellBounds);
        if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
        {
          Color backColor = e.CellStyle.SelectionBackColor;
          Brush brush;
          if (gridView.Focused)
          {
            brush = new SolidBrush(e.CellStyle.SelectionBackColor);
          }
          else
          {
            Color color = Color.FromArgb(255, 170, 170, 170);
            brush = new SolidBrush(color);
          }
          e.Graphics.FillRectangle(brush, e.CellBounds);
          brush.Dispose();
        }
        else
        {
          Color bgColor = Color.White;
          if (!DebugOptions.disableWordHighlight)
          {
            if (entry != null)
            {
              bgColor = entry.BackgroundColor;
            }
          }
          else
          {
            if (entry != null)
            {
              bgColor = entry.BackgroundColor;
            }
          }
          e.CellStyle.BackColor = bgColor;
          e.PaintBackground(e.ClipBounds, false);
        }

        if (DebugOptions.disableWordHighlight)
        {
          e.PaintContent(e.CellBounds);
        }
        else
        {
          PaintCell(e, gridView, false, entry);
        }

        if (e.ColumnIndex == 0)
        {
          if (this.bookmarkProvider.IsBookmarkAtLine(rowIndex))
          {
            Rectangle r; // = new Rectangle(e.CellBounds.Left + 2, e.CellBounds.Top + 2, 6, 6);
            r = e.CellBounds;
            r.Inflate(-2, -2);
            Brush brush = new SolidBrush(this.BookmarkColor);
            e.Graphics.FillRectangle(brush, r);
            brush.Dispose();
            Bookmark bookmark = this.bookmarkProvider.GetBookmarkForLine(rowIndex);
            if (bookmark.Text.Length > 0)
            {
              StringFormat format = new StringFormat();
              format.LineAlignment = StringAlignment.Center;
              format.Alignment = StringAlignment.Center;
              Brush brush2 = new SolidBrush(Color.FromArgb(255, 190, 100, 0));
              Font font = new Font("Courier New", this.Preferences.fontSize, FontStyle.Bold);
              e.Graphics.DrawString("i", font, brush2, new RectangleF(r.Left, r.Top, r.Width, r.Height), format);
              font.Dispose();
              brush2.Dispose();
            }
          }
        }

        e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
        e.Handled = true;
      }
    }

    private void PaintCell(DataGridViewCellPaintingEventArgs e, DataGridView gridView, bool noBackgroundFill, HilightEntry groundEntry)
    {
      PaintHighlightedCell(e, gridView, noBackgroundFill, groundEntry);
    }

    private void PaintHighlightedCell(DataGridViewCellPaintingEventArgs e, DataGridView gridView, bool noBackgroundFill, HilightEntry groundEntry)
    {
      object value = e.Value != null ? e.Value : "";
      IList<HilightMatchEntry> matchList = FindHilightMatches(value as string);
      // too many entries per line seem to cause problems with the GDI 
      while (matchList.Count > 50)
      {
        matchList.RemoveAt(50);
      }

      var hme = new HilightMatchEntry();
      hme.StartPos = 0;
      hme.Length = (value as string).Length;
      hme.HilightEntry = new HilightEntry((value as string),
        groundEntry != null ? groundEntry.ForegroundColor : Color.FromKnownColor(KnownColor.Black),
        groundEntry != null ? groundEntry.BackgroundColor : Color.Empty,
        false);
      matchList = MergeHighlightMatchEntries(matchList, hme);

      int leftPad = e.CellStyle.Padding.Left;
      RectangleF rect = new RectangleF(e.CellBounds.Left + leftPad, e.CellBounds.Top, e.CellBounds.Width, e.CellBounds.Height);
      Rectangle borderWidths = BorderWidths(e.AdvancedBorderStyle);
      Rectangle valBounds = e.CellBounds;
      valBounds.Offset(borderWidths.X, borderWidths.Y);
      valBounds.Width -= borderWidths.Right;
      valBounds.Height -= borderWidths.Bottom;
      if (e.CellStyle.Padding != Padding.Empty)
      {
        valBounds.Offset(e.CellStyle.Padding.Left, e.CellStyle.Padding.Top);
        valBounds.Width -= e.CellStyle.Padding.Horizontal;
        valBounds.Height -= e.CellStyle.Padding.Vertical;
      }


      TextFormatFlags flags =
        TextFormatFlags.Left
        | TextFormatFlags.SingleLine
        | TextFormatFlags.NoPrefix
        | TextFormatFlags.PreserveGraphicsClipping
        | TextFormatFlags.NoPadding
        | TextFormatFlags.VerticalCenter
        | TextFormatFlags.TextBoxControl
        ;

      //          | TextFormatFlags.VerticalCenter
      //          | TextFormatFlags.TextBoxControl
      //          TextFormatFlags.SingleLine


      //TextRenderer.DrawText(e.Graphics, e.Value as String, e.CellStyle.Font, valBounds, Color.FromKnownColor(KnownColor.Black), flags);

      Point wordPos = valBounds.Location;
      Size proposedSize = new Size(valBounds.Width, valBounds.Height);

      Rectangle r = gridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
      e.Graphics.SetClip(e.CellBounds);

      foreach (HilightMatchEntry matchEntry in matchList)
      {
        Font font = matchEntry != null && matchEntry.HilightEntry.IsBold ? this.fontBold : this.font;
        Brush bgBrush = matchEntry.HilightEntry.BackgroundColor != Color.Empty ? new SolidBrush(matchEntry.HilightEntry.BackgroundColor) : null;
        string matchWord = (value as string).Substring(matchEntry.StartPos, matchEntry.Length);
        Size wordSize = TextRenderer.MeasureText(e.Graphics, matchWord, font, proposedSize, flags);
        wordSize.Height = e.CellBounds.Height;
        Rectangle wordRect = new Rectangle(wordPos, wordSize);

        Color foreColor = matchEntry.HilightEntry.ForegroundColor;
        if ((e.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.Selected)
        {
          if (!noBackgroundFill && bgBrush != null && !matchEntry.HilightEntry.NoBackground)
          {
            e.Graphics.FillRectangle(bgBrush, wordRect);
          }
        }
        else
        {
          if (foreColor.Equals(Color.Black))
          {
            foreColor = Color.White;
          }
        }
        TextRenderer.DrawText(e.Graphics, matchWord, font, wordRect,
          foreColor, flags);

        wordPos.Offset(wordSize.Width, 0);
        if (bgBrush != null)
        {
          bgBrush.Dispose();
        }
      }
    }

    protected virtual Rectangle BorderWidths(DataGridViewAdvancedBorderStyle advancedBorderStyle)
    {
      Rectangle rect = new Rectangle();

      rect.X = (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1;
      if (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.OutsetDouble || advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.InsetDouble)
      {
        rect.X++;
      }

      rect.Y = (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1;
      if (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.OutsetDouble || advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.InsetDouble)
      {
        rect.Y++;
      }

      rect.Width = (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1;
      if (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.OutsetDouble || advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.InsetDouble)
      {
        rect.Width++;
      }

      rect.Height = (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1;
      if (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.OutsetDouble || advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.InsetDouble)
      {
        rect.Height++;
      }

      //rect.Width += this.owningColumn.DividerWidth;
      //rect.Height += this.owningRow.DividerHeight;

      return rect;
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
          else
          {
            //entryArray[i].ForegroundColor = me.HilightEntry.ForegroundColor;
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

    public void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      DataGridView gridView = (DataGridView)sender;
      CellPainting(gridView, e.RowIndex, e);
    }

    private void dataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
    {
      if (!this.CurrentColumnizer.IsTimeshiftImplemented())
        return;
      string line = this.logFileReader.GetLogLine(e.RowIndex);
      int offset = this.CurrentColumnizer.GetTimeOffset();
      this.CurrentColumnizer.SetTimeOffset(0);
      this.columnizerCallback.LineNum = e.RowIndex;
      string[] cols = this.CurrentColumnizer.SplitLine(this.columnizerCallback, line);
      this.CurrentColumnizer.SetTimeOffset(offset);
      if (cols.Length <= e.ColumnIndex - 2)
        return;

      string oldValue = cols[e.ColumnIndex - 2];
      string newValue = (string)e.Value;
      //string oldValue = (string) this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
      this.CurrentColumnizer.PushValue(this.columnizerCallback, e.ColumnIndex - 2, newValue, oldValue);
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

    private void dataGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
    {
      e.Height = GetRowHeight(e.RowIndex);
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


    /**
     * Returns the first HilightEntry that matches the given line
     */
    public HilightEntry FindHilightEntry(string line, bool noWordMatches)
    {
      // first check the temp entries
      lock (this.tempHilightEntryListLock)
      {
        foreach (HilightEntry entry in this.tempHilightEntryList)
        {
          if (noWordMatches && entry.IsWordMatch)
            continue;
          if (CheckHighlightEntryMatch(entry, line))
          {
            return entry;
          }
        }
      }

      lock (this.currentHighlightGroupLock)
      {
        foreach (HilightEntry entry in this.currentHighlightGroup.HilightEntryList)
        {
          if (noWordMatches && entry.IsWordMatch)
            continue;
          if (CheckHighlightEntryMatch(entry, line))
          {
            return entry;
          }
        }
        return null;
      }
    }

    private bool CheckHighlightEntryMatch(HilightEntry entry, string line)
    {
      if (entry.IsRegEx)
      {
        //Regex rex = new Regex(entry.SearchText, entry.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
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
        lock (this.currentHighlightGroupLock)
        {
          foreach (HilightEntry entry in this.currentHighlightGroup.HilightEntryList)
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

    public IList<HilightMatchEntry> FindHilightMatches(string line)
    {
      IList<HilightMatchEntry> resultList = new List<HilightMatchEntry>();
      if (line != null)
      {
        lock (this.currentHighlightGroupLock)
        {
          GetHighlightEntryMatches(line, this.currentHighlightGroup.HilightEntryList, resultList);
        }
        lock (this.tempHilightEntryList)
        {
          GetHighlightEntryMatches(line, this.tempHilightEntryList, resultList);
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
          noLed = true;
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

    void dataGridView_CurrentCellChanged(object sender, EventArgs e)
    {
      if (this.dataGridView.CurrentRow != null)
      {
        this.statusEventArgs.CurrentLineNum = this.dataGridView.CurrentRow.Index + 1;
        SendStatusLineUpdate();
        if (this.syncFilterCheckBox.Checked)
        {
          SyncFilterGridPos();
        }

        if (this.CurrentColumnizer.IsTimeshiftImplemented() && this.Preferences.timestampControl)
        {
          SyncTimestampDisplay();
        }

        //MethodInvoker invoker = new MethodInvoker(DisplayCurrentFileOnStatusline);
        //invoker.BeginInvoke(null, null);
      }
    }

    private void StopTimespreadThread()
    {
      this.timeSpreadCalc.Stop();
    }

    private void StopTimestampSyncThread()
    {
      this.shouldTimestampDisplaySyncingCancel = true;
      this.timeshiftSyncWakeupEvent.Set();
      this.timeshiftSyncThread.Abort();
      this.timeshiftSyncThread.Join();
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
      this.timeshiftSyncLine = lineNum;
      this.timeshiftSyncTimerEvent.Set();
      this.timeshiftSyncWakeupEvent.Set();
    }

    private void SyncTimestampDisplayWorker()
    {
      const int WAIT_TIME = 500;
      Thread.CurrentThread.Name = "SyncTimestampDisplayWorker";
      this.shouldTimestampDisplaySyncingCancel = false;
      this.isTimestampDisplaySyncing = true;

      while (!this.shouldTimestampDisplaySyncingCancel)
      {
        this.timeshiftSyncWakeupEvent.WaitOne();
        if (this.shouldTimestampDisplaySyncingCancel)
          return;
        this.timeshiftSyncWakeupEvent.Reset();

        while (!this.shouldTimestampDisplaySyncingCancel)
        {
          bool signaled = this.timeshiftSyncTimerEvent.WaitOne(WAIT_TIME, true);
          this.timeshiftSyncTimerEvent.Reset();
          if (!signaled)
          {
            break;
          }
        }
        // timeout with no new Trigger -> update display
        int lineNum = this.timeshiftSyncLine;
        if (lineNum >= 0 && lineNum < this.dataGridView.RowCount)
        {
          int refLine = lineNum;
          DateTime timeStamp = GetTimestampForLine(ref refLine, true);
          if (!timeStamp.Equals(DateTime.MinValue) && !this.shouldTimestampDisplaySyncingCancel)
          {
            this.guiStateArgs.Timestamp = timeStamp;
            SendGuiStateUpdate();
            if (this.shouldCallTimeSync)
            {
              refLine = lineNum;
              DateTime exactTimeStamp = GetTimestampForLine(ref refLine, false);
              SyncOtherWindows(exactTimeStamp);
              this.shouldCallTimeSync = false;
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
          //TimeSpan span = TimeSpan.FromTicks(timeStamp2.Ticks - timeStamp1.Ticks);
          DateTime diff;
          if (timeStamp1.Ticks > timeStamp2.Ticks)
            diff = new DateTime(timeStamp1.Ticks - timeStamp2.Ticks);
          else
            diff = new DateTime(timeStamp2.Ticks - timeStamp1.Ticks);
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
        if (this.filterResultList.Count > 0)
        {
          int index = this.filterResultList.BinarySearch(this.dataGridView.CurrentRow.Index);
          if (index < 0)
          {
            index = ~index;
            if (index > 0)
              --index;
          }
          if (this.filterGridView.Rows.Count > 0)	// exception no rows
            this.filterGridView.CurrentCell = this.filterGridView.Rows[index].Cells[0];
          else
            this.filterGridView.CurrentCell = null;
        }
      }
      catch (Exception e)
      {
        Logger.logError("SyncFilterGridPos(): " + e.Message);
      }
    }

    private void statusLineFileSize(long size)
    {
      this.statusEventArgs.FileSize = size;
      SendStatusLineUpdate();
    }

    public void FollowTailChanged(bool isChecked, bool byTrigger)
    {
      this.guiStateArgs.FollowTail = isChecked;

      if (this.guiStateArgs.FollowTail && logFileReader != null)
      {
        if (this.dataGridView.RowCount >= this.logFileReader.LineCount && this.logFileReader.LineCount > 0)
        {
          this.dataGridView.FirstDisplayedScrollingRowIndex = this.logFileReader.LineCount - 1;
        }
      }
      this.BeginInvoke(new MethodInvoker(this.dataGridView.Refresh));
      //this.dataGridView.Refresh();
      this.parentLogTabWin.FollowTailChanged(this, isChecked, byTrigger);
      SendGuiStateUpdate();
    }

    public void GotoLine(int line)
    {
      if (line >= 0)
      {
        if (line < this.dataGridView.RowCount)
          SelectLine(line, false);
        else
          SelectLine(this.dataGridView.RowCount - 1, false);
        this.dataGridView.Focus();
      }
    }

    public void StartSearch()
    {
      this.guiStateArgs.MenuEnabled = false;
      GuiStateUpdate(this, this.guiStateArgs);
      SearchParams searchParams = this.parentLogTabWin.SearchParams;
      if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
        searchParams.currentLine = this.dataGridView.CurrentCellAddress.Y + 1;
      else
        searchParams.currentLine = this.dataGridView.CurrentCellAddress.Y - 1;

      this.currentSearchParams = searchParams;    // remember for async "not found" messages

      this.isSearching = true;
      this.shouldCancel = false;
      StatusLineText("Searching... Press ESC to cancel.");

      this.progressEventArgs.MinValue = 0;
      this.progressEventArgs.MaxValue = this.dataGridView.RowCount;
      this.progressEventArgs.Value = 0;
      this.progressEventArgs.Visible = true;
      SendProgressBarUpdate();

      SearchFx searchFx = new SearchFx(Search);
      searchFx.BeginInvoke(searchParams, SearchComplete, null);

      RemoveAllSearchHighlightEntries();
      AddSearchHitHighlightEntry(searchParams);
    }

    private int Search(SearchParams searchParams)
    {
      UpdateProgressBarFx progressFx = new UpdateProgressBarFx(UpdateProgressBar);
      if (searchParams.searchText == null)
        return -1;
      int lineNum = (searchParams.isFromTop && !searchParams.isFindNext) ? 0 : searchParams.currentLine;
      string lowerSearchText = searchParams.searchText.ToLower();
      int count = 0;
      bool hasWrapped = false;
      while (true)
      {
        if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
        {
          if (lineNum >= this.logFileReader.LineCount)
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
            lineNum = this.logFileReader.LineCount - 1;
            hasWrapped = true;
            StatusLineError("Started from end of file");
          }
        }

        string line = this.logFileReader.GetLogLine(lineNum);
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
        if (this.shouldCancel)
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
          { }
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
        this.guiStateArgs.MenuEnabled = true;
        GuiStateUpdate(this, this.guiStateArgs);
        if (line == -1)
        {
          return;
        }
        this.dataGridView.Invoke(new SelectLineFx(SelectLine), new object[] { line, true });
      }
      catch (Exception) // in the case the windows is already destroyed
      { }
    }

    private void ResetProgressBar()
    {
      this.progressEventArgs.Value = this.progressEventArgs.MaxValue;
      this.progressEventArgs.Visible = false;
      SendProgressBarUpdate();
    }

    public void SelectLogLine(int line)
    {
      this.Invoke(new SelectLineFx(this.SelectLine), new object[] { line, true });
    }

    private void SelectLine(int line, bool triggerSyncCall)
    {
      try
      {
        this.shouldCallTimeSync = triggerSyncCall;
        bool wasCancelled = this.shouldCancel;
        this.shouldCancel = false;
        this.isSearching = false;
        StatusLineText("");
        this.guiStateArgs.MenuEnabled = true;
        if (wasCancelled)
          return;
        if (line == -1)
        {
          MessageBox.Show(this, "Not found:", "Search result");   // Hmm... is that experimental code from early days?  
          return;
        }
        this.dataGridView.Rows[line].Selected = true;
        this.dataGridView.CurrentCell = this.dataGridView.Rows[line].Cells[0];
        this.dataGridView.Focus();
      }
      catch (IndexOutOfRangeException e)
      {
        // Occures sometimes (but cannot reproduce)
        Logger.logError("Error while selecting line: " + e.ToString());
      }
    }

    private void SelectLine_NoScroll(int line, bool triggerSyncCall)
    {
      try
      {
        this.shouldCallTimeSync = triggerSyncCall;
        bool wasCancelled = this.shouldCancel;
        this.shouldCancel = false;
        this.isSearching = false;
        StatusLineText("");
        this.guiStateArgs.MenuEnabled = true;
        if (wasCancelled)
          return;
        if (line == -1)
        {
          MessageBox.Show(this, "Not found:", "Search result");  // Hmm... is that experimental code from early days?  
          return;
        }
        this.dataGridView.Rows[line].Selected = true;
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

    public void SelectAndEnsureVisible(int line, bool triggerSyncCall)
    {
      try
      {
        SelectLine_NoScroll(line, triggerSyncCall);

        //if (!this.dataGridView.CurrentRow.Displayed)
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

    public void LogWindow_KeyDown(object sender, KeyEventArgs e)
    {
      if (this.isErrorShowing)
      {
        RemoveStatusLineError();
      }
      if (e.KeyCode == Keys.F3)
      {
        if (this.parentLogTabWin.SearchParams == null
          || this.parentLogTabWin.SearchParams.searchText == null
          || this.parentLogTabWin.SearchParams.searchText.Length == 0)
          return;
        this.parentLogTabWin.SearchParams.isFindNext = true;
        this.parentLogTabWin.SearchParams.isShiftF3Pressed = ((e.Modifiers & Keys.Shift) == Keys.Shift);
        StartSearch();
      }
      if (e.KeyCode == Keys.Escape)
      {
        if (this.isSearching)
          this.shouldCancel = true;
        FireCancelHandlers();
        RemoveAllSearchHighlightEntries();
      }
      if (e.KeyCode == Keys.E && (e.Modifiers & Keys.Control) == Keys.Control)
      {
        StartEditMode();
      }
      if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
      {
        int newLine = this.logFileReader.GetNextMultiFileLine(this.dataGridView.CurrentCellAddress.Y);
        if (newLine != -1)
        {
          SelectLine(newLine, false);
        }
        e.Handled = true;
      }
      if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
      {
        int newLine = this.logFileReader.GetPrevMultiFileLine(this.dataGridView.CurrentCellAddress.Y);
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
            editControl.KeyDown += new KeyEventHandler(editControl_KeyDown);
            editControl.KeyPress += new KeyPressEventHandler(editControl_KeyPress);
            editControl.KeyUp += new KeyEventHandler(editControl_KeyUp);
            editControl.Click += new EventHandler(editControl_Click);
            this.dataGridView.CellEndEdit += new DataGridViewCellEventHandler(dataGridView_CellEndEdit);
            editControl.SelectionStart = 0;
          }
        }
      }
    }

    private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
      StatusLineText("");
    }

    private void editControl_KeyUp(object sender, KeyEventArgs e)
    {
      UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
    }

    private void editControl_KeyPress(object sender, KeyPressEventArgs e)
    {
      UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
    }

    private void editControl_Click(object sender, EventArgs e)
    {
      UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
    }

    private void editControl_KeyDown(object sender, KeyEventArgs e)
    {
      UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
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
        string line = this.logFileReader.GetLogLine(lineNum);
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
      while (lineNum < this.logFileReader.LineCount)
      {
        lineNum++;
        string line = this.logFileReader.GetLogLine(lineNum);
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

    public void AddBookmarkOverlays()
    {
      const int OVERSCAN = 20;
      int firstLine = this.dataGridView.FirstDisplayedScrollingRowIndex;
      if (firstLine < 0)
        return;

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
            break;
        }
        if (this.bookmarkProvider.IsBookmarkAtLine(i))
        {
          Bookmark bookmark = this.bookmarkProvider.GetBookmarkForLine(i);
          if (bookmark.Text.Length > 0)
          {
            //BookmarkOverlay overlay = new BookmarkOverlay();
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
              //int count = i - this.dataGridView.FirstDisplayedScrollingRowIndex;
              int heightSum = 0;
              if (this.dataGridView.FirstDisplayedScrollingRowIndex < i)
              {
                for (int rn = this.dataGridView.FirstDisplayedScrollingRowIndex + 1; rn < i; ++rn)
                {
                  //Rectangle rr = this.dataGridView.GetCellDisplayRectangle(0, rn, false);
                  //heightSum += rr.Height;
                  heightSum += GetRowHeight(rn);
                }
                r.Offset(0, r.Height + heightSum);
              }
              else
              {
                for (int rn = this.dataGridView.FirstDisplayedScrollingRowIndex + 1; rn > i; --rn)
                {
                  //Rectangle rr = this.dataGridView.GetCellDisplayRectangle(0, rn, false);
                  //heightSum += rr.Height;
                  heightSum += GetRowHeight(rn);
                }
                r.Offset(0, -(r.Height + heightSum));
              }
              //r.Offset(0, this.dataGridView.DisplayRectangle.Height);
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

    private void dataGridView_Paint(object sender, PaintEventArgs e)
    {
      if (this.ShowBookmarkBubbles)
      {
        AddBookmarkOverlays();
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
        lineNum = this.filterResultList[gridView.CurrentCellAddress.Y];
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
      if (this.bookmarkProvider.IsBookmarkAtLine(lineNum))
      {
        Bookmark bookmark = this.bookmarkProvider.GetBookmarkForLine(lineNum);
        if (bookmark.Text != null && bookmark.Text.Length > 0)
        {
          if (DialogResult.No == MessageBox.Show("There's a comment attached to the bookmark. Really remove the bookmark?", "LogExpert", MessageBoxButtons.YesNo))
          {
            return;
          }
        }
        this.bookmarkProvider.RemoveBookmarkForLine(lineNum);
      }
      else
      {
        this.bookmarkProvider.AddBookmark(new Bookmark(lineNum));
      }
      this.dataGridView.Refresh();
      this.filterGridView.Refresh();
      OnBookmarkAdded();
    }

    public void SetBookmarkFromTrigger(int lineNum, string comment)
    {
      lock (this.bookmarkLock)
      {
        string line = this.logFileReader.GetLogLine(lineNum);
        if (line == null)
          return;
        ParamParser paramParser = new ParamParser(comment);
        try
        {
          comment = paramParser.ReplaceParams(line, lineNum, this.FileName);
        }
        catch (ArgumentException)
        {
          // occurs on invalid regex 
        }
        if (this.bookmarkProvider.IsBookmarkAtLine(lineNum))
        {
          this.bookmarkProvider.RemoveBookmarkForLine(lineNum);
        }
        this.bookmarkProvider.AddBookmark(new Bookmark(lineNum, comment));
        OnBookmarkAdded();
      }
    }

    private int FindNextBookmarkIndex(int lineNum)
    {
      if (lineNum >= this.dataGridView.RowCount)
      {
        lineNum = 0;
      }
      else
      {
        lineNum++;
      }
      return this.bookmarkProvider.FindNextBookmarkIndex(lineNum);
    }

    private int FindPrevBookmarkIndex(int lineNum)
    {
      if (lineNum <= 0)
      {
        lineNum = this.dataGridView.RowCount - 1;
      }
      else
      {
        lineNum--;
      }
      return this.bookmarkProvider.FindPrevBookmarkIndex(lineNum);
    }

    public void JumpNextBookmark()
    {
      if (this.bookmarkProvider.Bookmarks.Count > 0)
      {
        if (this.filterGridView.Focused)
        {
          int index = FindNextBookmarkIndex(this.filterResultList[this.filterGridView.CurrentCellAddress.Y]);
          int startIndex = index;
          bool wrapped = false;
          while (true)
          {
            int lineNum = this.bookmarkProvider.Bookmarks[index].LineNum;
            if (this.filterResultList.Contains(lineNum))
            {
              int filterLine = this.filterResultList.IndexOf(lineNum);
              this.filterGridView.Rows[filterLine].Selected = true;
              this.filterGridView.CurrentCell = this.filterGridView.Rows[filterLine].Cells[0];
              break;
            }
            index++;
            if (index > this.bookmarkProvider.Bookmarks.Count - 1)
            {
              index = 0;
              wrapped = true;
            }
            if (index >= startIndex && wrapped)
              break;
          }
        }
        else
        {
          int index = FindNextBookmarkIndex(this.dataGridView.CurrentCellAddress.Y);
          if (index > this.bookmarkProvider.Bookmarks.Count - 1)
            index = 0;

          int lineNum = this.bookmarkProvider.Bookmarks[index].LineNum;
          SelectLine(lineNum, true);
        }
      }
    }

    public void JumpPrevBookmark()
    {
      if (this.bookmarkProvider.Bookmarks.Count > 0)
      {
        if (this.filterGridView.Focused)
        {
          //int index = this.bookmarkList.BinarySearch(this.filterResultList[this.filterGridView.CurrentCellAddress.Y]);
          //if (index < 0)
          //  index = ~index;
          //index--;
          int index = FindPrevBookmarkIndex(this.filterResultList[this.filterGridView.CurrentCellAddress.Y]);
          if (index < 0)
            index = this.bookmarkProvider.Bookmarks.Count - 1;
          int startIndex = index;
          bool wrapped = false;
          while (true)
          {
            int lineNum = this.bookmarkProvider.Bookmarks[index].LineNum;
            if (this.filterResultList.Contains(lineNum))
            {
              int filterLine = this.filterResultList.IndexOf(lineNum);
              this.filterGridView.Rows[filterLine].Selected = true;
              this.filterGridView.CurrentCell = this.filterGridView.Rows[filterLine].Cells[0];
              break;
            }
            index--;
            if (index < 0)
            {
              index = this.bookmarkProvider.Bookmarks.Count - 1;
              wrapped = true;
            }
            if (index <= startIndex && wrapped)
              break;
          }
        }
        else
        {
          int index = FindPrevBookmarkIndex(this.dataGridView.CurrentCellAddress.Y);
          if (index < 0)
            index = this.bookmarkProvider.Bookmarks.Count - 1;

          int lineNum = this.bookmarkProvider.Bookmarks[index].LineNum;
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
          if (this.bookmarkProvider.IsBookmarkAtLine(lineNum) && this.bookmarkProvider.GetBookmarkForLine(lineNum).Text.Length > 0)
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
      bookmarkProvider.RemoveBookmarksForLines(lineNumList);
      OnBookmarkRemoved();
    }


    /**
     * Shift bookmarks after a logfile rollover
     */
    private void ShiftBookmarks(int offset)
    {
      this.bookmarkProvider.ShiftBookmarks(offset);
      OnBookmarkRemoved();
    }

    private void ShiftRowHeightList(int offset)
    {
      SortedList<int, RowHeightEntry> newList = new SortedList<int, RowHeightEntry>();
      foreach (RowHeightEntry entry in this.rowHeightList.Values)
      {
        int line = entry.LineNum - offset;
        if (line >= 0)
        {
          entry.LineNum = line;
          newList.Add(line, entry);
        }
      }
      this.rowHeightList = newList;
    }

    private void ShiftFilterPipes(int offset)
    {
      lock (this.filterPipeList)
      {
        foreach (FilterPipe pipe in this.filterPipeList)
        {
          pipe.ShiftLineNums(offset);
        }
      }
    }

    private void LoadFilterPipes()
    {
      lock (this.filterPipeList)
      {
        foreach (FilterPipe pipe in this.filterPipeList)
        {
          pipe.RecreateTempFile();
        }
      }
      if (this.filterPipeList.Count > 0)
      {
        for (int i = 0; i < this.dataGridView.RowCount; ++i)
        {
          ProcessFilterPipes(i);
        }
      }
    }

    private void DisconnectFilterPipes()
    {
      lock (this.filterPipeList)
      {
        foreach (FilterPipe pipe in this.filterPipeList)
        {
          pipe.ClearLineList();
        }
      }
    }

    // ======================================================================================
    // Filter Grid stuff
    // ======================================================================================

    private void filterSearchButton_Click(object sender, EventArgs e)
    {
      FilterSearch();
    }

    private void ApplyFilterParams()
    {
      this.filterComboBox.Text = this.filterParams.searchText;
      this.filterCaseSensitiveCheckBox.Checked = this.filterParams.isCaseSensitive;
      this.filterRegexCheckBox.Checked = this.filterParams.isRegex;
      this.filterTailCheckBox.Checked = this.filterParams.isFilterTail;
      this.invertFilterCheckBox.Checked = this.filterParams.isInvert;
      this.filterKnobControl1.Value = this.filterParams.spreadBefore;
      this.filterKnobControl2.Value = this.filterParams.spreadBehind;
      this.rangeCheckBox.Checked = this.filterParams.isRangeSearch;
      this.columnRestrictCheckBox.Checked = this.filterParams.columnRestrict;
      this.fuzzyKnobControl.Value = this.filterParams.fuzzyValue;
      this.filterRangeComboBox.Text = this.filterParams.rangeSearchText;
    }

    private void ResetFilterControls()
    {
      this.filterComboBox.Text = "";
      this.filterCaseSensitiveCheckBox.Checked = false;
      this.filterRegexCheckBox.Checked = false;
      //this.filterTailCheckBox.Checked = this.Preferences.filterTail;
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
        this.filterParams.searchText = "";
        this.filterParams.lowerSearchText = "";
        this.filterParams.isRangeSearch = false;
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

      this.filterParams.searchText = text;
      this.filterParams.lowerSearchText = text.ToLower();
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

      this.filterParams.isRangeSearch = this.rangeCheckBox.Checked;
      this.filterParams.rangeSearchText = this.filterRangeComboBox.Text;
      if (this.filterParams.isRangeSearch)
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

      this.filterParams.isCaseSensitive = this.filterCaseSensitiveCheckBox.Checked;
      this.filterParams.isRegex = this.filterRegexCheckBox.Checked;
      this.filterParams.isFilterTail = this.filterTailCheckBox.Checked;
      this.filterParams.isInvert = this.invertFilterCheckBox.Checked;
      if (this.filterParams.isRegex)
      {
        try
        {
          this.filterParams.CreateRegex();
        }
        catch (ArgumentException)
        {
          StatusLineError("Invalid regular expression");
          return;
        }
      }
      this.filterParams.fuzzyValue = this.fuzzyKnobControl.Value;
      this.filterParams.spreadBefore = this.filterKnobControl1.Value;
      this.filterParams.spreadBehind = this.filterKnobControl2.Value;
      this.filterParams.columnRestrict = this.columnRestrictCheckBox.Checked;

      //ConfigManager.SaveFilterParams(this.filterParams);
      ConfigManager.Settings.filterParams = this.filterParams;  // wozu eigentlich? sinnlos seit MDI?

      this.shouldCancel = false;
      this.isSearching = true;
      StatusLineText("Filtering... Press ESC to cancel");
      this.filterSearchButton.Enabled = false;
      ClearFilterList();

      this.progressEventArgs.MinValue = 0;
      this.progressEventArgs.MaxValue = this.dataGridView.RowCount;
      this.progressEventArgs.Value = 0;
      this.progressEventArgs.Visible = true;
      SendProgressBarUpdate();

      Settings settings = ConfigManager.Settings;
      FilterFx fx;
      fx = settings.preferences.multiThreadFilter ? new FilterFx(MultiThreadedFilter) : new FilterFx(Filter);
      fx.BeginInvoke(this.filterParams, this.filterResultList, this.lastFilterLinesList, this.filterHitList, FilterComplete, null);
      CheckForFilterDirty();
    }

    private void MultiThreadedFilter(FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterLinesList, List<int> filterHitList)
    {
      ColumnizerCallback callback = new ColumnizerCallback(this);
      FilterStarter fs = new FilterStarter(callback, Environment.ProcessorCount + 2);
      fs.FilterHitList = this.filterHitList;
      fs.FilterResultLines = this.filterResultList;
      fs.LastFilterLinesList = this.lastFilterLinesList;
      BackgroundProcessCancelHandler cancelHandler = new FilterCancelHandler(fs);
      RegisterCancelHandler(cancelHandler);
      long startTime = Environment.TickCount;

      fs.DoFilter(filterParams, 0, this.logFileReader.LineCount, FilterProgressCallback);

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
        //AddFilterLineFx addFx = new AddFilterLineFx(AddFilterLine);
        ColumnizerCallback callback = new ColumnizerCallback(this);
        while (true)
        {
          string line = this.logFileReader.GetLogLine(lineNum);
          if (line == null)
          {
            break;
          }
          callback.LineNum = lineNum;
          if (Util.TestFilterCondition(filterParams, line, callback))
          {
            AddFilterLine(lineNum, false, filterParams, filterResultLines, lastFilterLinesList, filterHitList);
          }
          lineNum++;
          if (lineNum % PROGRESS_BAR_MODULO == 0)
          {
            UpdateProgressBar(lineNum);
          }
          if (this.shouldCancel)
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
      //string textLine = this.logFileReader.GetLogLine(lineNum);
      //ColumnizerCallback callback = new ColumnizerCallback(this);
      //callback.LineNum = lineNum;

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
            resultList.Add(lineNum - i);
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
        if (lineNum + i < this.logFileReader.LineCount)
        {
          if (!resultList.Contains(lineNum + i) && !checkList.Contains(lineNum + i))
            resultList.Add(lineNum + i);
        }
      }
      return resultList;
    }

    private void AddFilterLine(int lineNum, bool immediate, FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterLinesList, List<int> filterHitList)
    {
      int count;
      lock (this.filterResultList)
      {
        filterHitList.Add(lineNum);
        IList<int> filterResult = GetAdditionalFilterResults(filterParams, lineNum, lastFilterLinesList);
        filterResultLines.AddRange(filterResult);
        count = filterResultLines.Count;
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
      else if (lineNum % PROGRESS_BAR_MODULO == 0)
      {
        //FunctionWith1IntParam fx = new FunctionWith1IntParam(UpdateFilterCountLabel);
        //this.Invoke(fx, new object[] { count});
      }
    }

    private void UpdateFilterCountLabel(int count)
    {
      this.filterCountLabel.Text = "" + this.filterResultList.Count;
    }

    private void TriggerFilterLineGuiUpdate()
    {
      //lock (this.filterUpdateThread)
      //{
      //  this.filterEventCount++;
      //  this.filterUpdateEvent.Set();
      //}
      this.Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
    }

    //private void FilterUpdateWorker()
    //{
    //  Thread.CurrentThread.Name = "FilterUpdateWorker";
    //  while (true)
    //  {
    //    this.filterUpdateEvent.WaitOne();
    //    lock (this.filterUpdateThread)
    //    {
    //      this.Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
    //      this.filterUpdateEvent.Reset();
    //    }

    //    //Logger.logDebug("FilterUpdateWorker: Waiting for signal");
    //    //bool signaled = this.filterUpdateEvent.WaitOne(1000, false);

    //    //if (!signaled)
    //    //{
    //    //  lock (this.filterUpdateThread)
    //    //  {
    //    //    if (this.filterEventCount > 0)
    //    //    {
    //    //      this.filterEventCount = 0;
    //    //      Logger.logDebug("FilterUpdateWorker: Invoking GUI update because of wait timeout");
    //    //      this.Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
    //    //    }
    //    //  }
    //    //}
    //    //else
    //    //{
    //    //  Logger.logDebug("FilterUpdateWorker: Wakeup signal received.");
    //    //  lock (this.filterUpdateThread)
    //    //  {
    //    //    Logger.logDebug("FilterUpdateWorker: event count: " + this.filterEventCount);
    //    //    if (this.filterEventCount > 100)
    //    //    {
    //    //      this.filterEventCount = 0;
    //    //      Logger.logDebug("FilterUpdateWorker: Invoking GUI update because of event count");
    //    //      this.Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
    //    //    }
    //    //    this.filterUpdateEvent.Reset();
    //    //  }
    //    //}
    //  }
    //}

    //private void StopFilterUpdateWorkerThread()
    //{
    //  this.filterUpdateEvent.Set();
    //  this.filterUpdateThread.Abort();
    //  this.filterUpdateThread.Join();
    //}

    private void AddFilterLineGuiUpdate()
    {
      try
      {
        lock (this.filterResultList)
        {
          this.filterCountLabel.Text = "" + this.filterResultList.Count;
          if (this.filterGridView.RowCount > this.filterResultList.Count)
          {
            this.filterGridView.RowCount = 0;  // helps to prevent hang ?
          }
          this.filterGridView.RowCount = this.filterResultList.Count;
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
      this.progressEventArgs.Value = value;
      if (value > this.progressEventArgs.MaxValue)
      {
        // can occur if new lines will be added while filtering
        this.progressEventArgs.MaxValue = value;
      }
      SendProgressBarUpdate();
    }

    private void FilterComplete(IAsyncResult result)
    {
      if (!this.IsDisposed && !this.waitingForClose && !this.Disposing)
      {
        this.Invoke(new MethodInvoker(ResetStatusAfterFilter));
      }
    }

    private void ResetStatusAfterFilter()
    {
      try
      {
        //StatusLineText("");
        this.isSearching = false;
        this.progressEventArgs.Value = this.progressEventArgs.MaxValue;
        this.progressEventArgs.Visible = false;
        SendProgressBarUpdate();
        this.filterGridView.RowCount = this.filterResultList.Count;
        //this.filterGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        AutoResizeColumns(this.filterGridView);
        this.filterCountLabel.Text = "" + this.filterResultList.Count;
        if (filterGridView.RowCount > 0)
          filterGridView.Focus();
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

    private void filterGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      DataGridView gridView = (DataGridView)sender;

      if (e.RowIndex < 0 || e.ColumnIndex < 0 || this.filterResultList.Count <= e.RowIndex)
      {
        e.Handled = false;
        return;
      }
      int lineNum = this.filterResultList[e.RowIndex];
      string line = this.logFileReader.GetLogLineWithWait(lineNum);
      if (line != null)
      {
        HilightEntry entry = FindFirstNoWordMatchHilightEntry(line);
        e.Graphics.SetClip(e.CellBounds);
        if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
        {
          Color backColor = e.CellStyle.SelectionBackColor;
          Brush brush;
          if (gridView.Focused)
          {
            brush = new SolidBrush(e.CellStyle.SelectionBackColor);
          }
          else
          {
            Color color = Color.FromArgb(255, 170, 170, 170);
            brush = new SolidBrush(color);
          }
          e.Graphics.FillRectangle(brush, e.CellBounds);
          brush.Dispose();
        }
        else
        {
          Color bgColor = Color.White;
          // paint direct filter hits with different bg color
          //if (this.filterParams.SpreadEnabled && this.filterHitList.Contains(lineNum))
          //{
          //  bgColor = Color.FromArgb(255, 220, 220, 220);
          //}
          if (!DebugOptions.disableWordHighlight)
          {
            if (entry != null)
            {
              bgColor = entry.BackgroundColor;
            }
          }
          else
          {
            if (entry != null)
            {
              bgColor = entry.BackgroundColor;
            }
          }
          e.CellStyle.BackColor = bgColor;
          e.PaintBackground(e.ClipBounds, false);
        }

        if (DebugOptions.disableWordHighlight)
        {
          e.PaintContent(e.CellBounds);
        }
        else
        {
          PaintCell(e, this.filterGridView, false, entry);
        }

        if (e.ColumnIndex == 0)
        {
          if (this.bookmarkProvider.IsBookmarkAtLine(lineNum))
          {
            Rectangle r = new Rectangle(e.CellBounds.Left + 2, e.CellBounds.Top + 2, 6, 6);
            r = e.CellBounds;
            r.Inflate(-2, -2);
            Brush brush = new SolidBrush(this.BookmarkColor);
            e.Graphics.FillRectangle(brush, r);
            brush.Dispose();
            Bookmark bookmark = this.bookmarkProvider.GetBookmarkForLine(lineNum);
            if (bookmark.Text.Length > 0)
            {
              StringFormat format = new StringFormat();
              format.LineAlignment = StringAlignment.Center;
              format.Alignment = StringAlignment.Center;
              Brush brush2 = new SolidBrush(Color.FromArgb(255, 190, 100, 0));
              Font font = new Font("Verdana", this.Preferences.fontSize, FontStyle.Bold);
              e.Graphics.DrawString("!", font, brush2, new RectangleF(r.Left, r.Top, r.Width, r.Height), format);
              font.Dispose();
              brush2.Dispose();
            }
          }
        }

        e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
        e.Handled = true;
      }
    }

    private void filterGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
      if (e.RowIndex < 0 || e.ColumnIndex < 0 || this.filterResultList.Count <= e.RowIndex)
      {
        e.Value = "";
        return;
      }

      int lineNum = this.filterResultList[e.RowIndex];
      e.Value = GetCellValue(lineNum, e.ColumnIndex);
    }

    private void filterGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
    {
      e.Height = this.lineHeight;
    }

    private void ClearFilterList()
    {
      try
      {
        //this.shouldCancel = true;
        lock (this.filterResultList)
        {
          this.filterGridView.SuspendLayout();
          this.filterGridView.RowCount = 0;
          this.filterCountLabel.Text = "0";
          this.filterResultList = new List<int>();
          this.lastFilterLinesList = new List<int>();
          this.filterHitList = new List<int>();
          //this.filterGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
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
      this.bookmarkProvider.ClearAllBookmarks();
      OnAllBookmarksRemoved();
    }

    /**
     * Shift filter list line entries after a logfile rollover
     */
    private void ShiftFilterLines(int offset)
    {
      List<int> newFilterList = new List<int>();
      lock (this.filterResultList)
      {
        foreach (int lineNum in this.filterResultList)
        {
          int line = lineNum - offset;
          if (line >= 0)
          {
            newFilterList.Add(line);
          }
        }
        this.filterResultList = newFilterList;
      }

      newFilterList = new List<int>();
      foreach (int lineNum in this.filterHitList)
      {
        int line = lineNum - offset;
        if (line >= 0)
        {
          newFilterList.Add(line);
        }
      }
      this.filterHitList = newFilterList;

      int count = SPREAD_MAX;
      if (this.filterResultList.Count < SPREAD_MAX)
        count = this.filterResultList.Count;
      this.lastFilterLinesList = this.filterResultList.GetRange(this.filterResultList.Count - count, count);

      //this.filterGridView.RowCount = this.filterResultList.Count;
      //this.filterCountLabel.Text = "" + this.filterResultList.Count;
      //this.BeginInvoke(new MethodInvoker(this.filterGridView.Refresh));
      //this.BeginInvoke(new MethodInvoker(AddFilterLineGuiUpdate));
      TriggerFilterLineGuiUpdate();
    }

    private void filterComboBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        FilterSearch();
      }
    }

    private void filterGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
    {
      e.Handled = true;
      AutoResizeColumnsFx fx = AutoResizeColumns;
      this.BeginInvoke(fx, new object[] { this.filterGridView });
    }

    private void filterGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == 0)
      {
        ToggleBookmark();
        return;
      }

      if (this.filterGridView.CurrentRow != null && e.RowIndex >= 0)
      {
        int lineNum = this.filterResultList[this.filterGridView.CurrentRow.Index];
        SelectAndEnsureVisible(lineNum, true);
      }
    }

    private void filterGridView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      //if (this.filterGridView.CurrentRow != null)
      //{
      //  int lineNum = this.filterResultList[this.filterGridView.CurrentRow.Index];
      //  SelectLine(lineNum);
      //}
    }

    private void rangeCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      this.filterRangeComboBox.Enabled = this.rangeCheckBox.Checked;
      CheckForFilterDirty();
    }

    private void CheckForFilterDirty()
    {
      if (IsFilterSearchDirty(this.filterParams))
      {
        this.filterSearchButton.Image = this.searchButtonImage;
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
        return true;
      if (filterParams.isRangeSearch != this.rangeCheckBox.Checked)
        return true;
      if (filterParams.isRangeSearch && !filterParams.rangeSearchText.Equals(this.filterRangeComboBox.Text))
        return true;
      if (filterParams.isRegex != this.filterRegexCheckBox.Checked)
        return true;
      if (filterParams.isInvert != this.invertFilterCheckBox.Checked)
        return true;
      if (filterParams.spreadBefore != this.filterKnobControl1.Value)
        return true;
      if (filterParams.spreadBehind != this.filterKnobControl2.Value)
        return true;
      if (filterParams.fuzzyValue != this.fuzzyKnobControl.Value)
        return true;
      if (filterParams.columnRestrict != this.columnRestrictCheckBox.Checked)
        return true;
      if (filterParams.isCaseSensitive != this.filterCaseSensitiveCheckBox.Checked)
        return true;
      return false;
    }

    private void AdjustMinimumGridWith()
    {
      if (this.dataGridView.Columns.Count > 1)
      {
        //this.dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
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

    public void SetTimeshiftValue(string value)
    {
      this.guiStateArgs.TimeshiftText = value;
      if (this.CurrentColumnizer.IsTimeshiftImplemented())
      {
        try
        {
          if (this.guiStateArgs.TimeshiftEnabled)
          {
            try
            {
              string text = this.guiStateArgs.TimeshiftText;
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

    private void dataGridView_Scroll(object sender, ScrollEventArgs e)
    {
      if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
      {
        if (this.dataGridView.DisplayedRowCount(false) +
          this.dataGridView.FirstDisplayedScrollingRowIndex
          >= this.dataGridView.RowCount
          )
        {
          //this.guiStateArgs.FollowTail = true;
          if (!this.guiStateArgs.FollowTail)
            FollowTailChanged(true, false);
          OnTailFollowed(new EventArgs());
        }
        else
        {
          //this.guiStateArgs.FollowTail = false;
          if (this.guiStateArgs.FollowTail)
            FollowTailChanged(false, false);
        }
        SendGuiStateUpdate();
      }
    }

    private void filterGridView_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        if (this.filterGridView.CurrentCellAddress.Y >= 0 && this.filterGridView.CurrentCellAddress.Y < this.filterResultList.Count)
        {
          int lineNum = this.filterResultList[this.filterGridView.CurrentCellAddress.Y];
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

    private void dataGridView_KeyDown(object sender, KeyEventArgs e)
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
      this.shouldCallTimeSync = true;
    }

    private void dataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
      if ((e.KeyCode == Keys.Tab) && e.Control)
      {
        e.IsInputKey = true;
      }
    }

    private void dataGridView_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (this.dataGridView.CurrentCell != null)
        this.dataGridView.BeginEdit(false);
    }

    public void ToggleFilterPanel()
    {
      this.splitContainer1.Panel2Collapsed = !this.splitContainer1.Panel2Collapsed;
      if (!this.splitContainer1.Panel2Collapsed)
        this.filterComboBox.Focus();
      else
        this.dataGridView.Focus();
    }

    private void syncFilterCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (this.syncFilterCheckBox.Checked)
        SyncFilterGridPos();
    }

    private void dataGridView_Leave(object sender, EventArgs e)
    {
      InvalidateCurrentRow(this.dataGridView);
    }

    private void dataGridView_Enter(object sender, EventArgs e)
    {
      InvalidateCurrentRow(this.dataGridView);
    }

    private void filterGridView_Enter(object sender, EventArgs e)
    {
      InvalidateCurrentRow(this.filterGridView);
    }

    private void filterGridView_Leave(object sender, EventArgs e)
    {
      InvalidateCurrentRow(this.filterGridView);
    }

    private void InvalidateCurrentRow(DataGridView gridView)
    {
      if (gridView.CurrentCellAddress.Y > -1)
        gridView.InvalidateRow(gridView.CurrentCellAddress.Y);
    }

    private void InvalidateCurrentRow()
    {
      InvalidateCurrentRow(this.dataGridView);
      InvalidateCurrentRow(this.filterGridView);
    }

    private void DisplayCurrentFileOnStatusline()
    {
      if (this.logFileReader.IsMultiFile)
      {
        try
        {
          if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index > -1)
          {
            string fileName =
              this.logFileReader.GetLogFileNameForLine(this.dataGridView.CurrentRow.Index);
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

    private void dataGridView_Resize(object sender, EventArgs e)
    {
      if (this.logFileReader != null && this.dataGridView.RowCount > 0
         && this.guiStateArgs.FollowTail)
      {
        this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.RowCount - 1;
      }
    }

    private void dataGridView_SelectionChanged(object sender, EventArgs e)
    {
      UpdateSelectionDisplay();
    }

    private void UpdateSelectionDisplay()
    {
      if (this.noSelectionUpdates)
      {
        return;
      }
      this.selectionChangedTrigger.Trigger();
    }

    void selectionChangedTrigger_Signal(object sender, EventArgs e)
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

    public void LogWindowActivated()
    {
      if (this.guiStateArgs.FollowTail && !this.isDeadFile)
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
      this.statusEventArgs.StatusText = text;
      SendStatusLineUpdate();
    }

    private void StatusLineTextImmediate(string text)
    {
      this.statusEventArgs.StatusText = text;
      this.statusLineTrigger.TriggerImmediate();
    }

    private void StatusLineError(string text)
    {
      StatusLineText(text);
      this.isErrorShowing = true;
    }

    private void RemoveStatusLineError()
    {
      StatusLineText("");
      this.isErrorShowing = false;
    }

    private void SendGuiStateUpdate()
    {
      OnGuiState(this.guiStateArgs);
    }

    private void SendProgressBarUpdate()
    {
      OnProgressBarUpdate(this.progressEventArgs);
    }

    private void SendStatusLineUpdate()
    {
      //OnStatusLine(this.statusEventArgs);
      this.statusLineTrigger.Trigger();
    }

    public string FileName
    {
      get { return this.fileNameField; }
    }

    public string SessionFileName
    {
      get { return this.sessionFileName; }
      set { this.sessionFileName = value; }
    }

    public bool IsMultiFile
    {
      get { return isMultiFile; }
      set { this.guiStateArgs.IsMultiFileActive = this.isMultiFile = value; }
    }

    public bool IsTempFile
    {
      get { return isTempFile; }
    }

    public string TempTitleName
    {
      get { return tempTitleName; }
      set { tempTitleName = value; }
    }

    internal FilterPipe FilterPipe
    {
      get { return filterPipe; }
      set { filterPipe = value; }
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
      this.guiStateArgs.CellSelectMode = isCellMode;
    }

    public void TimeshiftEnabled(bool isEnabled, string shiftValue)
    {
      this.guiStateArgs.TimeshiftEnabled = isEnabled;
      SetTimestampLimits();
      SetTimeshiftValue(shiftValue);
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
        this.CurrentHighlightGroupChanged(this, new CurrentHighlightGroupChangedEventArgs(this, this.currentHighlightGroup));
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

    public delegate void BookmarkTextChangedEventHandler(object sender, EventArgs e);
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
    protected void OnColumnizerChanged(ILogLineColumnizer columnizer)
    {
      if (ColumnizerChanged != null)
      {
        ColumnizerChanged(this, new ColumnizerEventArgs(columnizer));
      }
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
      this.showAdvanced = show;
    }

    private void CheckForAdvancedButtonDirty()
    {
      if (this.IsAdvancedOptionActive() && !this.showAdvanced)
      {
        this.advancedButton.Image = this.advancedButtonImage;
      }
      else
      {
        this.advancedButton.Image = null;
      }
    }

    private void filterKnobControl1_ValueChanged(object sender, EventArgs e)
    {
      CheckForFilterDirty();
    }

    private void filterKnobControl2_ValueChanged(object sender, EventArgs e)
    {
      CheckForFilterDirty();
    }

    private void filterToTabButton_Click(object sender, EventArgs e)
    {
      FilterToTab();
    }

    private void FilterToTab()
    {
      this.filterSearchButton.Enabled = false;
      MethodInvoker invoker = new MethodInvoker(WriteFilterToTab);
      invoker.BeginInvoke(null, null);
    }

    private void WriteFilterToTab()
    {
      FilterPipe pipe = new FilterPipe(this.filterParams.CreateCopy(), this);
      lock (this.filterResultList)
      {
        string namePrefix = "->F";
        string title;
        if (this.IsTempFile)
          title = this.TempTitleName + namePrefix + ++this.filterPipeNameCounter;
        else
          title = Util.GetNameFromPath(this.FileName) + namePrefix + ++this.filterPipeNameCounter;

        WritePipeToTab(pipe, this.filterResultList, title, null);
      }
    }

    private void WritePipeToTab(FilterPipe pipe, IList<int> lineNumberList, string name, PersistenceData persistenceData)
    {
      Logger.logInfo("WritePipeToTab(): " + lineNumberList.Count + " lines.");
      StatusLineText("Writing to temp file... Press ESC to cancel.");
      this.guiStateArgs.MenuEnabled = false;
      SendGuiStateUpdate();
      this.progressEventArgs.MinValue = 0;
      this.progressEventArgs.MaxValue = lineNumberList.Count;
      this.progressEventArgs.Value = 0;
      this.progressEventArgs.Visible = true;
      this.Invoke(new MethodInvoker(SendProgressBarUpdate));
      this.isSearching = true;
      this.shouldCancel = false;

      lock (this.filterPipeList)
      {
        this.filterPipeList.Add(pipe);
      }
      pipe.Closed += new FilterPipe.ClosedEventHandler(pipe_Disconnected);
      int count = 0;
      pipe.OpenFile();
      LogExpertCallback callback = new LogExpertCallback(this);
      foreach (int i in lineNumberList)
      {
        if (this.shouldCancel)
        {
          break;
        }
        string line = this.logFileReader.GetLogLine(i);
        if (this.CurrentColumnizer is ILogLineXmlColumnizer)
        {
          callback.LineNum = i;
          line = (this.CurrentColumnizer as ILogLineXmlColumnizer).GetLineTextForClipboard(line, callback);
        }
        pipe.WriteToPipe(line, i);
        if (++count % PROGRESS_BAR_MODULO == 0)
        {
          this.progressEventArgs.Value = count;
          this.Invoke(new MethodInvoker(SendProgressBarUpdate));
        }
      }
      pipe.CloseFile();
      Logger.logInfo("WritePipeToTab(): finished");
      this.Invoke(new WriteFilterToTabFinishedFx(WriteFilterToTabFinished), new object[] { pipe, name, persistenceData });
    }

    private void WriteFilterToTabFinished(FilterPipe pipe, string name, PersistenceData persistenceData)
    {
      this.isSearching = false;
      if (!this.shouldCancel)
      {
        string title = name;
        ILogLineColumnizer preProcessColumnizer = null;
        if (!(this.CurrentColumnizer is ILogLineXmlColumnizer))
        {
          preProcessColumnizer = this.CurrentColumnizer;
        }
        LogWindow newWin = this.parentLogTabWin.AddFilterTab(pipe, title, new LoadingFinishedFx(LoadingFinishedFunc), preProcessColumnizer);
        newWin.FilterPipe = pipe;
        pipe.OwnLogWindow = newWin;
        if (persistenceData != null)
        {
          FilterRestoreFx fx = new FilterRestoreFx(FilterRestore);
          fx.BeginInvoke(newWin, persistenceData, null, null);
        }
        else
        {
          //// dont force XML columnizers because the Tab window isn't XML
          //if (!(this.CurrentColumnizer is ILogLineXmlColumnizer))
          //{
          //  newWin.ForceColumnizerForLoading(this.CurrentColumnizer);
          //}
        }
      }
      this.progressEventArgs.Value = progressEventArgs.MaxValue;
      this.progressEventArgs.Visible = false;
      SendProgressBarUpdate();
      this.guiStateArgs.MenuEnabled = true;
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
      pipe.Closed += new FilterPipe.ClosedEventHandler(pipe_Disconnected);
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
      string searchLine = this.logFileReader.GetLogLine(lineNum);
      if (searchLine == null)
        return;

      ColumnizerCallback callback = new ColumnizerCallback(this);
      callback.LineNum = lineNum;
      IList<FilterPipe> deleteList = new List<FilterPipe>();
      lock (this.filterPipeList)
      {
        foreach (FilterPipe pipe in this.filterPipeList)
        {
          if (pipe.IsStopped)
            continue;
          long startTime = Environment.TickCount;
          if (Util.TestFilterCondition(pipe.FilterParams, searchLine, callback))
          {
            IList<int> filterResult = GetAdditionalFilterResults(pipe.FilterParams, lineNum, pipe.LastLinesHistoryList);
            pipe.OpenFile();
            foreach (int line in filterResult)
            {
              pipe.LastLinesHistoryList.Add(line);
              if (pipe.LastLinesHistoryList.Count > SPREAD_MAX * 2)
                pipe.LastLinesHistoryList.RemoveAt(0);

              string textLine = this.logFileReader.GetLogLine(line);
              bool fileOk = pipe.WriteToPipe(textLine, line);
              if (!fileOk)
              {
                deleteList.Add(pipe);
              }
            }
            pipe.CloseFile();
          }
          long endTime = Environment.TickCount;
          //Logger.logDebug("ProcessFilterPipes(" + lineNum + ") duration: " + ((endTime - startTime)));
        }
      }
      foreach (FilterPipe pipe in deleteList)
      {
        this.filterPipeList.Remove(pipe);
      }
    }

    private void pipe_Disconnected(object sender, EventArgs e)
    {
      if (sender.GetType() == typeof(FilterPipe))
      {
        lock (this.filterPipeList)
        {
          this.filterPipeList.Remove((FilterPipe)sender);
          if (this.filterPipeList.Count == 0)
          {
            // reset naming counter to 0 if no more open filter tabs for this source window
            this.filterPipeNameCounter = 0;
          }
        }
      }
    }

    private void advancedButton_Click(object sender, EventArgs e)
    {
      this.showAdvanced = !this.showAdvanced;
      ShowAdvancedFilterPanel(this.showAdvanced);
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
        this.parentLogTabWin.AddTempFileTab(fileName, title);
      }
    }

    private void CopyMarkedLinesToClipboard()
    {
      if (this.guiStateArgs.CellSelectMode)
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
          string line = this.logFileReader.GetLogLine(lineNum);
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

    /// <summary>
    /// Change the file encoding. May force a reload if byte count ot preamble lenght differs from previous used encoding.
    /// </summary>
    /// <param name="encoding"></param>
    public void ChangeEncoding(Encoding encoding)
    {
      this.logFileReader.ChangeEncoding(encoding);
      this.encodingOptions.Encoding = encoding;
      if (this.guiStateArgs.CurrentEncoding.IsSingleByte != encoding.IsSingleByte ||
        this.guiStateArgs.CurrentEncoding.GetPreamble().Length != encoding.GetPreamble().Length)
      {
        Reload();
      }
      else
      {
        this.dataGridView.Refresh();
        SendGuiStateUpdate();
      }
      this.guiStateArgs.CurrentEncoding = this.logFileReader.CurrentEncoding;
    }

    public void Reload()
    {
      SavePersistenceData(false);

      this.reloadMemento = new ReloadMemento();
      this.reloadMemento.currentLine = this.dataGridView.CurrentCellAddress.Y;
      this.reloadMemento.firstDisplayedLine = this.dataGridView.FirstDisplayedScrollingRowIndex;
      this.forcedColumnizerForLoading = this.CurrentColumnizer;

      if (this.fileNames == null || !this.IsMultiFile)
      {
        LoadFile(this.FileName, this.EncodingOptions);
      }
      else
      {
        LoadFilesAsMulti(this.fileNames, this.EncodingOptions);
      }
      //if (currentLine < this.dataGridView.RowCount && currentLine >= 0)
      //  this.dataGridView.CurrentCell = this.dataGridView.Rows[currentLine].Cells[0];
      //if (firstDisplayedLine < this.dataGridView.RowCount && firstDisplayedLine >= 0)
      //  this.dataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedLine;

      //if (this.filterTailCheckBox.Checked)
      //{
      //  Logger.logInfo("Refreshing filter view because of reload.");
      //  FilterSearch();
      //}
    }

    public void PreferencesChanged(Preferences newPreferences, bool isLoadTime, SettingsFlags flags)
    {
      if ((flags & SettingsFlags.GuiOrColors) == SettingsFlags.GuiOrColors)
      {
        this.font = new Font(new FontFamily(newPreferences.fontName), newPreferences.fontSize);
        this.fontBold = new Font(this.font, FontStyle.Bold);
        this.fontMonospaced = new Font("Courier New", this.Preferences.fontSize, FontStyle.Bold);

        int lineSpacing = font.FontFamily.GetLineSpacing(FontStyle.Regular);
        float lineSpacingPixel = font.Size * lineSpacing / font.FontFamily.GetEmHeight(FontStyle.Regular);

        this.dataGridView.DefaultCellStyle.Font = font;
        this.filterGridView.DefaultCellStyle.Font = font;
        this.lineHeight = font.Height + 4;
        this.dataGridView.RowTemplate.Height = font.Height + 4;

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
          //this.FollowTailChanged(this.Preferences.followTail, false);
        }

        this.timeSpreadCalc.TimeMode = this.Preferences.timeSpreadTimeMode;
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

    /*========================================================================
     * Context menu stuff
     *========================================================================*/

    private void dataGridContextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
      int lineNum = -1;
      if (this.dataGridView.CurrentRow != null)
        lineNum = this.dataGridView.CurrentRow.Index;
      if (lineNum == -1)
        return;
      int refLineNum = lineNum;

      this.copyToTabToolStripMenuItem.Enabled = this.dataGridView.SelectedCells.Count > 0;
      this.scrollAllTabsToTimestampToolStripMenuItem.Enabled = this.CurrentColumnizer.IsTimeshiftImplemented()
        && GetTimestampForLine(ref refLineNum, false) != DateTime.MinValue;
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
        //string line = this.logFileReader.GetLogLine(lineNum);
        IList<int> lines = GetSelectedContent();
        foreach (IContextMenuEntry entry in PluginRegistry.GetInstance().RegisteredContextMenuPlugins)
        {
          LogExpertCallback callback = new LogExpertCallback(this);
          ContextMenuPluginEventArgs evArgs = new ContextMenuPluginEventArgs(entry, lines, this.CurrentColumnizer, callback);
          EventHandler ev = new EventHandler(HandlePluginContextMenu);
          //MenuItem item = this.dataGridView.ContextMenu.MenuItems.Add(entry.GetMenuText(line, this.CurrentColumnizer, callback), ev);
          string menuText = entry.GetMenuText(lines, this.CurrentColumnizer, callback);
          if (menuText != null)
          {
            bool disabled = menuText.StartsWith("_");
            if (disabled)
              menuText = menuText.Substring(1);
            ToolStripItem item = this.dataGridContextMenuStrip.Items.Add(menuText, null, ev);
            item.Tag = evArgs;
            item.Enabled = !disabled;
            isAdded = true;
          }
        }
      }
      this.pluginSeparator.Visible = isAdded;

      // enable/disable Temp Highlight item
      this.tempHighlightsToolStripMenuItem.Enabled = this.tempHilightEntryList.Count > 0;

      this.markCurrentFilterRangeToolStripMenuItem.Enabled = this.filterRangeComboBox.Text != null && this.filterRangeComboBox.Text.Length > 0;

      if (this.CurrentColumnizer.IsTimeshiftImplemented())
      {
        IList<WindowFileEntry> list = this.parentLogTabWin.GetListOfOpenFiles();
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
            item.Checked = this.timeSyncList != null && this.timeSyncList.Contains(fileEntry.LogWindow);
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
      this.freeThisWindowFromTimeSyncToolStripMenuItem.Enabled = this.timeSyncList != null && this.timeSyncList.Count > 1;
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

        if (this.timeSyncList != null && this.timeSyncList.Contains(entry.LogWindow))
        {
          FreeSlaveFromTimesync(entry.LogWindow);
        }
        else
        {
          //AddSlaveToTimesync(entry.LogWindow);
          AddOtherWindowToTimesync(entry.LogWindow);
        }
      }
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

    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      CopyMarkedLinesToClipboard();
    }

    private void copyToTabToolStripMenuItem_Click(object sender, EventArgs e)
    {
      CopyMarkedLinesToTab();
    }

    private void scrollAllTabsToTimestampToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentColumnizer.IsTimeshiftImplemented())
      {
        int currentLine = this.dataGridView.CurrentCellAddress.Y;
        if (currentLine > 0 && currentLine < this.dataGridView.RowCount)
        {
          int lineNum = currentLine;
          DateTime timeStamp = GetTimestampForLine(ref lineNum, false);
          if (timeStamp.Equals(DateTime.MinValue))  // means: invalid
            return;
          this.parentLogTabWin.ScrollAllTabsToTimestamp(timeStamp, this);
        }
      }
    }

    /* ========================================================================
     * Timestamp stuff
     * =======================================================================*/

    private void SetTimestampLimits()
    {
      if (!this.CurrentColumnizer.IsTimeshiftImplemented())
        return;

      int line = 0;
      this.guiStateArgs.MinTimestamp = GetTimestampForLineForward(ref line, true);
      line = this.dataGridView.RowCount - 1;
      this.guiStateArgs.MaxTimestamp = GetTimestampForLine(ref line, true);
      SendGuiStateUpdate();
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
        return false;

      //this.Cursor = Cursors.WaitCursor;
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
      //this.Cursor = Cursors.Default;
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
          return 0;
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
        //rangeStart = rangeStart;
        rangeEnd = lineNum;
      }
      else
      {
        rangeStart = lineNum;
        //rangeEnd = rangeEnd;
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
      lock (this.currentColumnizerLock)
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
            if (this.isTimestampDisplaySyncing && this.shouldTimestampDisplaySyncingCancel)
            {
              return DateTime.MinValue;
            }
            lookBack = true;
            string logLine = this.logFileReader.GetLogLine(lineNum);
            if (logLine == null)
              return DateTime.MinValue;
            this.columnizerCallback.LineNum = lineNum;
            timeStamp = this.CurrentColumnizer.GetTimestamp(this.columnizerCallback, logLine);
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
      lock (this.currentColumnizerLock)
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
            string logLine = this.logFileReader.GetLogLine(lineNum);
            if (logLine == null)
            {
              timeStamp = DateTime.MinValue;
              break;
            }
            timeStamp = this.CurrentColumnizer.GetTimestamp(this.columnizerCallback, logLine);
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
        return this.logFileReader.GetLogLine(this.dataGridView.CurrentRow.Index);
      }
      return null;
    }

    public string GetLine(int lineNum)
    {
      if (lineNum < 0 || lineNum >= this.logFileReader.LineCount)
        return null;
      return this.logFileReader.GetLogLine(lineNum);
    }

    public int GetCurrentLineNum()
    {
      if (this.dataGridView.CurrentRow == null)
        return -1;
      return this.dataGridView.CurrentRow.Index;
    }

    public int GetRealLineNum()
    {
      int lineNum = this.GetCurrentLineNum();
      if (lineNum == -1)
        return -1;
      return this.logFileReader.GetRealLineNumForVirtualLineNum(lineNum);
    }

    public string GetCurrentFileName()
    {
      if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index != -1)
      {
        return this.logFileReader.GetLogFileNameForLine(this.dataGridView.CurrentRow.Index);
      }
      return null;
    }

    public ILogFileInfo GetCurrentFileInfo()
    {
      if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index != -1)
      {
        return this.logFileReader.GetLogFileInfoForLine(this.dataGridView.CurrentRow.Index);
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
      return this.logFileReader.GetLogFileNameForLine(lineNum);
    }

    private void locateLineInOriginalFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.dataGridView.CurrentRow != null && this.FilterPipe != null)
      {
        int lineNum = this.FilterPipe.GetOriginalLineNum(this.dataGridView.CurrentRow.Index);
        if (lineNum != -1)
        {
          this.FilterPipe.LogWindow.SelectLine(lineNum, false);
          this.parentLogTabWin.SelectTab(this.FilterPipe.LogWindow);
        }
      }
    }

    private void toggleBoomarkToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToggleBookmark();
    }

    private void markEditModeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      StartEditMode();
    }

    private void LogWindow_SizeChanged(object sender, EventArgs e)
    {
      //AdjustMinimumGridWith();
      AdjustHighlightSplitterWidth();
    }

    private void AdjustHighlightSplitterWidth()
    {
      //int size = this.editHighlightsSplitContainer.Panel2Collapsed ? 600 : 660;
      //int distance = this.highlightSplitContainer.Width - size;
      //if (distance < 10)
      //  distance = 10;
      //this.highlightSplitContainer.SplitterDistance = distance;
    }

    // ======================= Bookmark list ====================================

    private void bookmarkWindow_BookmarkCommentChanged(object sender, EventArgs e)
    {
      this.dataGridView.Refresh();
      //this.bookmarkDirty = true;
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

    // =============== end of bookmark stuff ===================================

    public void ShowLineColumn(bool show)
    {
      this.dataGridView.Columns[1].Visible = show;
      this.filterGridView.Columns[1].Visible = show;
    }

    public string Title
    {
      get
      {
        if (IsTempFile)
          return TempTitleName;
        else
          return FileName;
      }
    }

    public ColumnizerCallback ColumnizerCallbackObject
    {
      get { return this.columnizerCallback; }
    }

    // =================== ILogLineColumnizerCallback ============================

    public class ColumnizerCallback : ILogLineColumnizerCallback
    {
      protected LogWindow logWindow;
      int lineNum;

      public int LineNum
      {
        get { return this.lineNum; }
        set { lineNum = value; }
      }


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
        return this.logWindow.logFileReader.LineCount;
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

    private void columnRestrictCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      this.columnButton.Enabled = this.columnRestrictCheckBox.Checked;
      if (this.columnRestrictCheckBox.Checked) // disable when nothing to filter
      {
        this.columnNamesLabel.Visible = true;
        this.filterParams.columnRestrict = true;
        this.columnNamesLabel.Text = CalculateColumnNames(this.filterParams);
      }
      else
      {
        this.columnNamesLabel.Visible = false;
      }
      CheckForFilterDirty();
    }

    private void columnButton_Click(object sender, EventArgs e)
    {
      filterParams.currentColumnizer = this.currentColumnizer;
      FilterColumnChooser chooser = new FilterColumnChooser(this.filterParams);
      if (chooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        columnNamesLabel.Text = CalculateColumnNames(this.filterParams);

        //CheckForFilterDirty(); //!!!GBro: Indicate to redo the search if search columns were changed
        this.filterSearchButton.Image = this.searchButtonImage;
        this.saveFilterButton.Enabled = false;
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
              names += ", ";
            names += this.dataGridView.Columns[2 + colIndex].HeaderText; // skip first two columns: marker + line number
          }
        }
      }

      return names;
    }

    // =======================================================================================
    // Column header context menu stuff
    // =======================================================================================

    private void dataGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
    {
      if (e.RowIndex > 0 && e.RowIndex < this.dataGridView.RowCount
        && !this.dataGridView.Rows[e.RowIndex].Selected)
      {
        SelectLine(e.RowIndex, false);
      }
      if (e.ContextMenuStrip == this.columnContextMenuStrip)
      {
        this.selectedCol = e.ColumnIndex;
      }
    }

    //private void boomarkDataGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
    //{
    //  if (e.RowIndex > 0 && e.RowIndex < this.boomarkDataGridView.RowCount
    //      && !this.boomarkDataGridView.Rows[e.RowIndex].Selected)
    //  {
    //    this.boomarkDataGridView.Rows[e.RowIndex].Selected = true;
    //    this.boomarkDataGridView.CurrentCell = this.boomarkDataGridView.Rows[e.RowIndex].Cells[0];
    //  }
    //  if (e.ContextMenuStrip == this.columnContextMenuStrip)
    //  {
    //    this.selectedCol = e.ColumnIndex;
    //  }
    //}

    private void filterGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
    {
      if (e.ContextMenuStrip == this.columnContextMenuStrip)
      {
        this.selectedCol = e.ColumnIndex;
      }
    }

    private void columnContextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
      Control ctl = this.columnContextMenuStrip.SourceControl;
      DataGridView gridView = ctl as DataGridView;
      bool frozen = false;
      if (freezeStateMap.ContainsKey(ctl))
      {
        frozen = this.freezeStateMap[ctl];
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
                                      gridView.Columns[this.selectedCol].HeaderText + ")";
        }
      }
      DataGridViewColumn col = gridView.Columns[this.selectedCol];
      this.moveLeftToolStripMenuItem.Enabled = (col != null && col.DisplayIndex > 0);
      this.moveRightToolStripMenuItem.Enabled = (col != null && col.DisplayIndex < gridView.Columns.Count - 1);

      if (gridView.Columns.Count - 1 > this.selectedCol)
      {
        //        DataGridViewColumn colRight = gridView.Columns[this.selectedCol + 1];
        DataGridViewColumn colRight = gridView.Columns.GetNextColumn(col, DataGridViewElementStates.None,
                                       DataGridViewElementStates.None);
        this.moveRightToolStripMenuItem.Enabled = (colRight != null && colRight.Frozen == col.Frozen);
      }
      if (this.selectedCol > 0)
      {
        //DataGridViewColumn colLeft = gridView.Columns[this.selectedCol - 1];
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

    private void HandleColumnItemContextMenu(object sender, EventArgs args)
    {
      if (sender is ToolStripItem)
      {
        DataGridViewColumn column = ((sender as ToolStripItem).Tag as DataGridViewColumn);
        column.Visible = true;
        column.DataGridView.FirstDisplayedScrollingColumnIndex = column.Index;
      }
    }

    private void freezeLeftColumnsUntilHereToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Control ctl = this.columnContextMenuStrip.SourceControl;
      bool frozen = false;
      if (freezeStateMap.ContainsKey(ctl))
      {
        frozen = this.freezeStateMap[ctl];
      }
      frozen = !frozen;
      this.freezeStateMap[ctl] = frozen;

      DataGridViewColumn senderCol = sender as DataGridViewColumn;
      if (ctl is DataGridView)
      {
        DataGridView gridView = ctl as DataGridView;
        ApplyFrozenState(gridView);
      }
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
        col.Frozen = this.freezeStateMap.ContainsKey(gridView) && this.freezeStateMap[gridView];
        bool sel = col.HeaderCell.Selected;
        if (col.Index == this.selectedCol)
        {
          break;
        }
      }
    }

    private void moveToLastColumnToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
      DataGridViewColumn col = gridView.Columns[this.selectedCol];
      if (col != null)
      {
        col.DisplayIndex = gridView.Columns.Count - 1;
      }
    }

    private void moveLeftToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
      DataGridViewColumn col = gridView.Columns[this.selectedCol];
      if (col != null && col.DisplayIndex > 0)
      {
        col.DisplayIndex = col.DisplayIndex - 1;
      }
    }

    private void moveRightToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
      DataGridViewColumn col = gridView.Columns[this.selectedCol];
      if (col != null && col.DisplayIndex < gridView.Columns.Count - 1)
      {
        col.DisplayIndex = col.DisplayIndex + 1;
      }
    }

    private void hideColumnToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
      DataGridViewColumn col = gridView.Columns[this.selectedCol];
      col.Visible = false;
    }

    private void restoreColumnsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DataGridView gridView = this.columnContextMenuStrip.SourceControl as DataGridView;
      foreach (DataGridViewColumn col in gridView.Columns)
      {
        col.Visible = true;
      }
    }

    private void timeSpreadingControl1_LineSelected(object sender, SelectLineEventArgs e)
    {
      SelectLine(e.Line, false);
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
      this.timeSpreadCalc.Enabled = show;
    }

    private void AddTempFileTab(string fileName, string title)
    {
      this.parentLogTabWin.AddTempFileTab(fileName, title);
    }

    // =================================================================
    // Pattern statistics
    // =================================================================

    public void PatternStatistic()
    {
      InitPatternWindow();
    }

    private void InitPatternWindow()
    {
      //PatternStatistic(this.patternArgs);
      this.patternWindow = new PatternWindow(this);
      this.patternWindow.SetColumnizer(this.CurrentColumnizer);
      //this.patternWindow.SetBlockList(blockList);
      this.patternWindow.SetFont(this.Preferences.fontName, this.Preferences.fontSize);
      this.patternWindow.Fuzzy = this.patternArgs.fuzzy;
      this.patternWindow.MaxDiff = this.patternArgs.maxDiffInBlock;
      this.patternWindow.MaxMisses = this.patternArgs.maxMisses;
      this.patternWindow.Weight = this.patternArgs.minWeight;
      //this.patternWindow.Show();
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
      PatternStatisticFx fx = new PatternStatisticFx(TestStatistic);
      fx.BeginInvoke(patternArgs, null, null);
    }

    private void TestStatistic(PatternArgs patternArgs)
    {
      int beginLine = patternArgs.startLine;
      Logger.logInfo("TestStatistics() called with start line " + beginLine);

      this.patternArgs = patternArgs;

      int num = beginLine + 1; //this.dataGridView.RowCount;

      this.progressEventArgs.MinValue = 0;
      this.progressEventArgs.MaxValue = this.dataGridView.RowCount;
      this.progressEventArgs.Value = beginLine;
      this.progressEventArgs.Visible = true;
      SendProgressBarUpdate();

      PrepareDict();
      ResetCache(num);

      Dictionary<int, int> processedLinesDict = new Dictionary<int, int>();
      List<PatternBlock> blockList = new List<PatternBlock>();
      int blockId = 0;
      this.isSearching = true;
      this.shouldCancel = false;
      int searchLine = -1;
      for (int i = beginLine; i < num && !this.shouldCancel; ++i)
      {

        if (processedLinesDict.ContainsKey(i))
        {
          continue;
        }

        PatternBlock block;
        int maxBlockLen = patternArgs.endLine - patternArgs.startLine;
        //int searchLine = i + 1;
        Logger.logDebug("TestStatistic(): i=" + i + " searchLine=" + searchLine);
        //bool firstBlock = true;
        searchLine++;
        UpdateProgressBar(searchLine);
        while (!this.shouldCancel && (block = DetectBlock(i, searchLine, maxBlockLen, this.patternArgs.maxDiffInBlock, this.patternArgs.maxMisses, processedLinesDict)) != null)
        {
          Logger.logDebug("Found block: " + block);
          if (block.weigth >= this.patternArgs.minWeight)
          {
            //PatternBlock existingBlock = FindExistingBlock(block, blockList);
            //if (existingBlock != null)
            //{
            //  if (block.weigth > existingBlock.weigth)
            //  {
            //    blockList.Remove(existingBlock);
            //    blockList.Add(block);
            //  }
            //}
            //else
            {
              blockList.Add(block);
              addBlockTargetLinesToDict(processedLinesDict, block);
            }
            block.blockId = blockId;
            //if (firstBlock)
            //{
            //  addBlockSrcLinesToDict(processedLinesDict, block);
            //}
            searchLine = block.targetEnd + 1;
          }
          else
          {
            searchLine = block.targetStart + 1;
          }
          UpdateProgressBar(searchLine);
        }
        blockId++;
      }
      this.isSearching = false;
      this.progressEventArgs.MinValue = 0;
      this.progressEventArgs.MaxValue = 0;
      this.progressEventArgs.Value = 0;
      this.progressEventArgs.Visible = false;
      SendProgressBarUpdate();
      //if (this.patternWindow.IsDisposed)
      //{
      //  this.Invoke(new MethodInvoker(CreatePatternWindow));
      //}
      this.patternWindow.SetBlockList(blockList, this.patternArgs);
      Logger.logInfo("TestStatistics() ended");
    }

    private void addBlockSrcLinesToDict(SortedDictionary<int, int> dict, PatternBlock block)
    {
      foreach (int lineNum in block.srcLines.Keys)
      {
        if (!dict.ContainsKey(lineNum))
        {
          dict.Add(lineNum, lineNum);
        }
      }
    }

    private void addBlockTargetLinesToDict(Dictionary<int, int> dict, PatternBlock block)
    {
      foreach (int lineNum in block.targetLines.Keys)
      {
        if (!dict.ContainsKey(lineNum))
        {
          dict.Add(lineNum, lineNum);
        }
      }
    }

    private PatternBlock FindExistingBlock(PatternBlock block, List<PatternBlock> blockList)
    {
      foreach (PatternBlock searchBlock in blockList)
      {
        if ((block.startLine > searchBlock.startLine &&
          block.startLine < searchBlock.endLine
          ||
          block.endLine > searchBlock.startLine &&
          block.endLine < searchBlock.endLine)
          &&
          (
          block.startLine != searchBlock.startLine &&
          block.endLine != searchBlock.endLine
          )
          )
        {
          return searchBlock;
        }
      }
      return null;
    }

    private PatternBlock DetectBlock(int startNum, int startLineToSearch, int maxBlockLen, int maxDiffInBlock, int maxMisses, Dictionary<int, int> processedLinesDict)
    {
      int targetLine = FindSimilarLine(startNum, startLineToSearch, processedLinesDict);
      if (targetLine == -1)
        return null;

      PatternBlock block = new PatternBlock();
      block.startLine = startNum;
      int srcLine = block.startLine;
      block.targetStart = targetLine;
      int srcMisses = 0;
      block.srcLines.Add(srcLine, srcLine);
      //block.targetLines.Add(targetLine, targetLine);
      int len = 0;
      QualityInfo qi = new QualityInfo();
      qi.quality = block.weigth;
      block.qualityInfoList[targetLine] = qi;

      while (!this.shouldCancel)
      {
        srcLine++;
        len++;
        //if (srcLine >= block.targetStart)
        //  break;  // prevent to search in the own block
        if (maxBlockLen > 0 && len > maxBlockLen)
          break;
        int nextTargetLine = FindSimilarLine(srcLine, targetLine + 1, processedLinesDict);
        if (nextTargetLine > -1 && nextTargetLine - targetLine - 1 <= maxDiffInBlock)
        {
          block.weigth += maxDiffInBlock - (nextTargetLine - targetLine - 1) + 1;
          block.endLine = srcLine;
          //block.targetLines.Add(nextTargetLine, nextTargetLine);
          block.srcLines.Add(srcLine, srcLine);
          if (nextTargetLine - targetLine > 1)
          {
            int tempWeight = block.weigth;
            for (int tl = targetLine + 1; tl < nextTargetLine; ++tl)
            {
              qi = new QualityInfo();
              qi.quality = --tempWeight;
              block.qualityInfoList[tl] = qi;
            }
          }
          targetLine = nextTargetLine;
          qi = new QualityInfo();
          qi.quality = block.weigth;
          block.qualityInfoList[targetLine] = qi;
        }
        else
        {
          srcMisses++;
          block.weigth--;
          targetLine++;
          qi = new QualityInfo();
          qi.quality = block.weigth;
          block.qualityInfoList[targetLine] = qi;
          if (srcMisses > maxMisses)
          {
            break;
          }
        }
      }
      block.targetEnd = targetLine;
      qi = new QualityInfo();
      qi.quality = block.weigth;
      block.qualityInfoList[targetLine] = qi;
      for (int k = block.targetStart; k <= block.targetEnd; ++k)
      {
        block.targetLines.Add(k, k);
      }
      return block;
    }

    List<int> lineHashList = new List<int>();

    private void PrepareDict()
    {
      this.lineHashList.Clear();
      Regex regex = new Regex("\\d");
      Regex regex2 = new Regex("\\S");

      int num = this.logFileReader.LineCount;
      for (int i = 0; i < num; ++i)
      {
        string msg = GetMsgForLine(i);
        if (msg != null)
        {
          msg = msg.ToLower();
          msg = regex.Replace(msg, "0");
          msg = regex2.Replace(msg, " ");
          char[] chars = msg.ToCharArray();
          int value = 0;
          int numOfE = 0;
          int numOfA = 0;
          int numOfI = 0;
          for (int j = 0; j < chars.Length; ++j)
          {
            value += chars[j];
            switch (chars[j])
            {
              case 'e':
                numOfE++;
                break;
              case 'a':
                numOfA++;
                break;
              case 'i':
                numOfI++;
                break;
            }
          }
          value += numOfE * 30;
          value += numOfA * 20;
          value += numOfI * 10;
          lineHashList.Add(value);
        }
      }
    }

    private int _FindSimilarLine(int srcLine, int startLine)
    {
      int value = this.lineHashList[srcLine];

      int num = this.lineHashList.Count;
      for (int i = startLine; i < num; ++i)
      {
        if (Math.Abs(this.lineHashList[i] - value) < 3)
        {
          return i;
        }
      }
      return -1;
    }

    // int[,] similarCache;

    private void ResetCache(int num)
    {
      //this.similarCache = new int[num, num];
      //for (int i = 0; i < num; ++i)
      //{
      //  for (int j = 0; j < num; j++)
      //  {
      //    this.similarCache[i, j] = -1;
      //  }
      //}
    }

    private int FindSimilarLine(int srcLine, int startLine, Dictionary<int, int> processedLinesDict)
    {
      int threshold = this.patternArgs.fuzzy;

      bool prepared = false;
      Regex regex = null;
      Regex regex2 = null;
      string msgToFind = null;
      CultureInfo culture = CultureInfo.CurrentCulture;

      int num = this.logFileReader.LineCount;
      for (int i = startLine; i < num; ++i)
      {
        if (processedLinesDict.ContainsKey(i))
        {
          continue;
        }
        //if (this.similarCache[srcLine, i] != -1)
        //{
        //  if (this.similarCache[srcLine, i] < threshold)
        //  {
        //    return i;
        //  }
        //}
        //else
        {
          if (!prepared)
          {
            msgToFind = GetMsgForLine(srcLine);
            regex = new Regex("\\d");
            regex2 = new Regex("\\W");
            msgToFind = msgToFind.ToLower(culture);
            msgToFind = regex.Replace(msgToFind, "0");
            msgToFind = regex2.Replace(msgToFind, " ");
            prepared = true;
          }
          string msg = GetMsgForLine(i);
          if (msg != null)
          {

            msg = regex.Replace(msg, "0");
            msg = regex2.Replace(msg, " ");
            int lenDiff = Math.Abs(msg.Length - msgToFind.Length);
            if (lenDiff > threshold)
            {
              //this.similarCache[srcLine, i] = lenDiff;
              continue;
            }
            msg = msg.ToLower(culture);
            int distance = Util.YetiLevenshtein(msgToFind, msg);
            //this.similarCache[srcLine, i] = distance;
            if (distance < threshold)
            {
              return i;
            }
          }
        }
      }
      return -1;
    }

    private string GetMsgForLine(int i)
    {
      string line = this.logFileReader.GetLogLine(i);
      ILogLineColumnizer columnizer = this.CurrentColumnizer;
      ColumnizerCallback callback = new ColumnizerCallback(this);
      string[] cols = columnizer.SplitLine(callback, line);
      return cols[columnizer.GetColumnCount() - 1];
    }

    public bool ForcePersistenceLoading
    {
      get { return this.forcePersistenceLoading; }
      set { this.forcePersistenceLoading = value; }
    }

    public string ForcedPersistenceFileName
    {
      get { return this.forcedPersistenceFileName; }
      set { this.forcedPersistenceFileName = value; }
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
        if (!this.rowHeightList.ContainsKey(rowNum))
        {
          return;
        }
        else
        {
          RowHeightEntry entry = this.rowHeightList[rowNum];
          entry.Height = entry.Height - this.lineHeight;
          if (entry.Height <= this.lineHeight)
          {
            this.rowHeightList.Remove(rowNum);
          }
        }
      }
      else
      {
        RowHeightEntry entry;
        if (!this.rowHeightList.ContainsKey(rowNum))
        {
          entry = new RowHeightEntry();
          entry.LineNum = rowNum;
          entry.Height = this.lineHeight;
          this.rowHeightList[rowNum] = entry;
        }
        else
        {
          entry = this.rowHeightList[rowNum];
        }
        entry.Height = entry.Height + this.lineHeight;
      }
      this.dataGridView.UpdateRowHeightInfo(rowNum, false);
      if (rowNum == this.dataGridView.RowCount - 1 && this.guiStateArgs.FollowTail)
      {
        this.dataGridView.FirstDisplayedScrollingRowIndex = rowNum;
      }
      this.dataGridView.Refresh();
    }

    private int GetRowHeight(int rowNum)
    {
      if (this.rowHeightList.ContainsKey(rowNum))
      {
        return this.rowHeightList[rowNum].Height;
      }
      else
      {
        return this.lineHeight;
      }
    }

    private void bookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
    {
      AddBookmarkAndEditComment();
    }

    private void AddBookmarkAtLineSilently(int lineNum)
    {
      if (!this.bookmarkProvider.IsBookmarkAtLine(lineNum))
      {
        this.bookmarkProvider.AddBookmark(new Bookmark(lineNum));
      }
    }

    private void AddBookmarkAndEditComment()
    {
      int lineNum = this.dataGridView.CurrentCellAddress.Y;
      if (!this.bookmarkProvider.IsBookmarkAtLine(lineNum))
      {
        ToggleBookmark();
      }
      BookmarkComment(this.bookmarkProvider.GetBookmarkForLine(lineNum));
    }

    private void AddBookmarkComment(string text)
    {
      int lineNum = this.dataGridView.CurrentCellAddress.Y;
      Bookmark bookmark;
      if (!this.bookmarkProvider.IsBookmarkAtLine(lineNum))
      {
        this.bookmarkProvider.AddBookmark(bookmark = new Bookmark(lineNum));
      }
      else
      {
        bookmark = this.bookmarkProvider.GetBookmarkForLine(lineNum);
      }
      bookmark.Text = bookmark.Text + text;
      this.dataGridView.Refresh();
      this.filterGridView.Refresh();
      OnBookmarkTextChanged(bookmark);
    }

    public void ExportBookmarkList()
    {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Title = "Choose a file to save bookmarks into";
      dlg.AddExtension = true;
      dlg.DefaultExt = "csv";
      dlg.Filter = "CSV file (*.csv)|*.csv";
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        try
        {
          BookmarkExporter.ExportBookmarkList(this.bookmarkProvider.BookmarkList, this.FileName, dlg.FileName);
        }
        catch (IOException e)
        {
          MessageBox.Show("Error while exporting bookmark list: " + e.Message, "LogExpert");
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

    private void highlightSelectionInLogFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
      {
        DataGridViewTextBoxEditingControl ctl =
          this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
        HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow,
                           false, true, false, false, false, false, null, false);
        lock (this.tempHilightEntryListLock)
        {
          this.tempHilightEntryList.Add(he);
        }
        this.dataGridView.CancelEdit();
        this.dataGridView.EndEdit();
        RefreshAllGrids();
      }
    }

    private void highlightSelectionInLogFilewordModeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
      {
        DataGridViewTextBoxEditingControl ctl =
          this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
        HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow,
                           false, true, false, false, false, false, null, true);
        lock (this.tempHilightEntryListLock)
        {
          this.tempHilightEntryList.Add(he);
        }
        this.dataGridView.CancelEdit();
        this.dataGridView.EndEdit();
        RefreshAllGrids();
      }
    }

    private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
      {
        DataGridViewTextBoxEditingControl ctl =
          this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
        if (!Util.IsNull(ctl.SelectedText))
        {
          Clipboard.SetText(ctl.SelectedText);
        }
      }
    }

    private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      RemoveTempHighlights();
    }

    private void makePermanentToolStripMenuItem_Click(object sender, EventArgs e)
    {
      lock (this.tempHilightEntryListLock)
      {
        lock (this.currentHighlightGroupLock)
        {
          this.currentHighlightGroup.HilightEntryList.AddRange(this.tempHilightEntryList);
          RemoveTempHighlights();
          OnCurrentHighlightListChanged();
        }
      }
    }

    private void markCurrentFilterRangeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      markCurrentFilterRange();
    }

    private void markCurrentFilterRange()
    {
      this.filterParams.rangeSearchText = this.filterRangeComboBox.Text;
      ColumnizerCallback callback = new ColumnizerCallback(this);
      RangeFinder rangeFinder = new RangeFinder(this.filterParams, callback);
      Range range = rangeFinder.FindRange(this.dataGridView.CurrentCellAddress.Y);
      if (range != null)
      {
        SetCellSelectionMode(false);
        this.noSelectionUpdates = true;
        for (int i = range.StartLine; i <= range.EndLine; ++i)
        {
          this.dataGridView.Rows[i].Selected = true;
        }
        this.noSelectionUpdates = false;
        UpdateSelectionDisplay();
      }
    }

    private void RemoveTempHighlights()
    {
      lock (this.tempHilightEntryListLock)
      {
        this.tempHilightEntryList.Clear();
      }
      RefreshAllGrids();
    }

    internal void RefreshAllGrids()
    {
      this.dataGridView.Refresh();
      this.filterGridView.Refresh();
    }

    private void filterForSelectionToolStripMenuItem_Click(object sender, EventArgs e)
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

    private void setSelectedTextAsBookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
      {
        DataGridViewTextBoxEditingControl ctl =
          this.dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
        AddBookmarkComment(ctl.SelectedText);
      }
    }

    private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
    {
      this.shouldCallTimeSync = true;
    }


    private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == 0)
      {
        ToggleBookmark();
      }
    }

    private void dataGridView_OverlayDoubleClicked(object sender, OverlayEventArgs e)
    {
      BookmarkComment(e.BookmarkOverlay.Bookmark);
    }

    private void filterRegexCheckBox_MouseUp(object sender, MouseEventArgs e)
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
    void highlightThread_HighlightDoneEvent(object sender, HighlightEventArgs e)
    {
      this.BeginInvoke(new HighlightEventFx(HighlightDoneEventWorker), new object[] { e });
    }

    /// <summary>
    /// Highlights the done event worker.
    /// </summary>
    /// <param name="e">The <see cref="LogExpert.HighlightEventArgs"/> instance containing the event data.</param>
    private void HighlightDoneEventWorker(HighlightEventArgs e)
    {
      if (this.dataGridView.FirstDisplayedScrollingRowIndex > e.StartLine
        && this.dataGridView.FirstDisplayedScrollingRowIndex < e.StartLine + e.Count
        ||
        this.dataGridView.FirstDisplayedScrollingRowIndex + this.dataGridView.DisplayedRowCount(true)
          > e.StartLine
        && this.dataGridView.FirstDisplayedScrollingRowIndex + this.dataGridView.DisplayedRowCount(true) < e.StartLine + e.Count)
      {
        this.BeginInvoke(new MethodInvoker(this.RefreshAllGrids));
      }
    }

    private void toggleHighlightPanelButton_Click(object sender, EventArgs e)
    {
      ToggleHighlightPanel(this.highlightSplitContainer.Panel2Collapsed);
    }

    private void ToggleHighlightPanel(bool open)
    {
      this.highlightSplitContainer.Panel2Collapsed = !open;
      this.toggleHighlightPanelButton.Image = (open ? this.panelCloseButtonImage : this.panelOpenButtonImage);
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

    private void saveFilterButton_Click(object sender, EventArgs e)
    {
      FilterParams newParams = this.filterParams.CreateCopy();
      newParams.color = Color.FromKnownColor(KnownColor.Black);
      ConfigManager.Settings.filterList.Add(newParams);
      OnFilterListChanged(this);
    }

    private void deleteFilterButton_Click(object sender, EventArgs e)
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

    private void filterUpButton_Click(object sender, EventArgs e)
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

    private void filterDownButton_Click(object sender, EventArgs e)
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

    private void filterListBox_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (this.filterListBox.SelectedIndex >= 0)
      {
        FilterParams filterParams = (FilterParams)this.filterListBox.Items[this.filterListBox.SelectedIndex];
        FilterParams newParams = filterParams.CreateCopy();
        //newParams.historyList = ConfigManager.Settings.filterHistoryList;
        this.filterParams = newParams;
        ReInitFilterParams(this.filterParams);
        ApplyFilterParams();
        CheckForAdvancedButtonDirty();
        CheckForFilterDirty();
        this.filterSearchButton.Image = this.searchButtonImage;
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

    private void filterListBox_DrawItem(object sender, DrawItemEventArgs e)
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
    private void colorToolStripMenuItem_Click(object sender, EventArgs e)
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

    private void filterCaseSensitiveCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      CheckForFilterDirty();
    }

    private void filterRegexCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      this.fuzzyKnobControl.Enabled = !this.filterRegexCheckBox.Checked;
      this.fuzzyLabel.Enabled = !this.filterRegexCheckBox.Checked;
      CheckForFilterDirty();
    }

    private void invertFilterCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      CheckForFilterDirty();
    }

    private void filterRangeComboBox_TextChanged(object sender, EventArgs e)
    {
      CheckForFilterDirty();
    }

    private void fuzzyKnobControl_ValueChanged(object sender, EventArgs e)
    {
      CheckForFilterDirty();
    }

    private void filterComboBox_TextChanged(object sender, EventArgs e)
    {
      CheckForFilterDirty();
    }

    private void setBookmarksOnSelectedLinesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetBoomarksForSelectedFilterLines();
    }

    private void SetBoomarksForSelectedFilterLines()
    {
      lock (this.filterResultList)
      {
        foreach (DataGridViewRow row in this.filterGridView.SelectedRows)
        {
          int lineNum = this.filterResultList[row.Index];
          AddBookmarkAtLineSilently(lineNum);
        }
      }
      this.dataGridView.Refresh();
      this.filterGridView.Refresh();
      OnBookmarkAdded();
    }

    void parent_HighlightSettingsChanged(object sender, EventArgs e)
    {
      string groupName = this.guiStateArgs.HighlightGroupName;
      SetCurrentHighlightGroup(groupName);
    }

    public void SetCurrentHighlightGroup(string groupName)
    {
      this.guiStateArgs.HighlightGroupName = groupName;
      lock (this.currentHighlightGroupLock)
      {
        this.currentHighlightGroup = this.parentLogTabWin.FindHighlightGroup(groupName);
        if (this.currentHighlightGroup == null)
        {
          if (this.parentLogTabWin.HilightGroupList.Count > 0)
          {
            this.currentHighlightGroup = this.parentLogTabWin.HilightGroupList[0];
          }
          else
          {
            this.currentHighlightGroup = new HilightGroup();
          }
        }
        this.guiStateArgs.HighlightGroupName = this.currentHighlightGroup.GroupName;
      }
      SendGuiStateUpdate();
      this.BeginInvoke(new MethodInvoker(RefreshAllGrids));
    }

    private void SetDefaultHighlightGroup()
    {
      HilightGroup group = this.parentLogTabWin.FindHighlightGroupByFileMask(this.FileName);
      if (group != null)
      {
        SetCurrentHighlightGroup(group.GroupName);
      }
      else
      {
        SetCurrentHighlightGroup("[Default]");
      }
    }

    private void filterOnLoadCheckBox_MouseClick(object sender, MouseEventArgs e)
    {
      HandleChangedFilterOnLoadSetting();
    }

    private void filterOnLoadCheckBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      HandleChangedFilterOnLoadSetting();
    }

    private void hideFilterListOnLoadCheckBox_MouseClick(object sender, MouseEventArgs e)
    {
      HandleChangedFilterOnLoadSetting();
    }

    private void HandleChangedFilterOnLoadSetting()
    {
      this.parentLogTabWin.Preferences.isFilterOnLoad = this.filterOnLoadCheckBox.Checked;
      this.parentLogTabWin.Preferences.isAutoHideFilterList = this.hideFilterListOnLoadCheckBox.Checked;
      OnFilterListChanged(this);
    }

    public Preferences Preferences
    {
      get { return ConfigManager.Settings.preferences; }
    }

    public string GivenFileName
    {
      get { return this.givenFileName; }
      set { this.givenFileName = value; }
    }

    public TimeSyncList TimeSyncList
    {
      get { return this.timeSyncList; }
    }

    protected void RegisterCancelHandler(BackgroundProcessCancelHandler handler)
    {
      lock (this.cancelHandlerList)
      {
        this.cancelHandlerList.Add(handler);
      }
    }

    protected void DeRegisterCancelHandler(BackgroundProcessCancelHandler handler)
    {
      lock (this.cancelHandlerList)
      {
        this.cancelHandlerList.Remove(handler);
      }
    }

    private void FireCancelHandlers()
    {
      lock (this.cancelHandlerList)
      {
        foreach (BackgroundProcessCancelHandler handler in this.cancelHandlerList)
        {
          handler.EscapePressed();
        }
      }
    }

    public void SwitchMultiFile(bool enabled)
    {
      IsMultiFile = enabled;
      Reload();
    }

    private void filterToTabToolStripMenuItem_Click(object sender, EventArgs e)
    {
      FilterToTab();
    }

    private void SyncOtherWindows(DateTime timestamp)
    {
      lock (this.timeSyncListLock)
      {
        if (this.timeSyncList != null)
        {
          this.timeSyncList.NavigateToTimestamp(timestamp, this);
        }
      }
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
      lock (this.timeSyncListLock)
      {
        if (this.IsTimeSynced && master.TimeSyncList != this.timeSyncList)  // already synced but master has different sync list
        {
          FreeFromTimeSync();
        }
        this.timeSyncList = master.TimeSyncList;
        this.timeSyncList.AddWindow(this);
        this.ScrollToTimestamp(this.timeSyncList.CurrentTimestamp, false, false);
      }
      OnSyncModeChanged();
    }

    public void FreeFromTimeSync()
    {
      lock (this.timeSyncListLock)
      {
        if (this.TimeSyncList != null)
        {
          Logger.logInfo("De-Syncing window for " + Util.GetNameFromPath(this.FileName));
          this.timeSyncList.WindowRemoved -= timeSyncList_WindowRemoved;
          this.TimeSyncList.RemoveWindow(this);
          this.timeSyncList = null;
        }
      }
      OnSyncModeChanged();
    }

    private void AddSlaveToTimesync(LogWindow slave)
    {
      lock (this.timeSyncListLock)
      {
        if (this.timeSyncList == null)
        {
          if (slave.TimeSyncList == null)
          {
            this.timeSyncList = new TimeSyncList();
            this.timeSyncList.AddWindow(this);
          }
          else
          {
            this.timeSyncList = slave.TimeSyncList;
          }
          int currentLineNum = this.dataGridView.CurrentCellAddress.Y;
          int refLine = currentLineNum;
          DateTime timeStamp = GetTimestampForLine(ref refLine, true);
          if (!timeStamp.Equals(DateTime.MinValue) && !this.shouldTimestampDisplaySyncingCancel)
          {
            this.timeSyncList.CurrentTimestamp = timeStamp;
          }
          this.timeSyncList.WindowRemoved += timeSyncList_WindowRemoved;
        }
      }
      slave.AddToTimeSync(this);
      OnSyncModeChanged();
    }

    private void FreeSlaveFromTimesync(LogWindow slave)
    {
      slave.FreeFromTimeSync();
    }

    void timeSyncList_WindowRemoved(object sender, EventArgs e)
    {
      TimeSyncList syncList = sender as TimeSyncList;
      lock (this.timeSyncListLock)
      {
        if (syncList.Count == 0 || syncList.Count == 1 && syncList.Contains(this))
        {
          if (syncList == this.timeSyncList)
          {
            this.timeSyncList = null;
            OnSyncModeChanged();
          }
        }
      }
    }

    private void freeThisWindowFromTimeSyncToolStripMenuItem_Click(object sender, EventArgs e)
    {
      FreeFromTimeSync();
    }

    public bool IsTimeSynced
    {
      get { return this.timeSyncList != null; }
    }

    protected EncodingOptions EncodingOptions
    {
      get { return encodingOptions; }
      set { encodingOptions = value; }
    }

    public delegate void SyncModeChangedEventHandler(object sender, SyncModeEventArgs e);
    public event SyncModeChangedEventHandler SyncModeChanged;
    private void OnSyncModeChanged()
    {
      if (SyncModeChanged != null)
        SyncModeChanged(this, new SyncModeEventArgs(this.IsTimeSynced));
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

    private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
    {
      this.advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;
    }

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
      lock (this.tempHilightEntryListLock)
      {
        this.tempHilightEntryList.Add(he);
      }
      RefreshAllGrids();
    }

    private void RemoveAllSearchHighlightEntries()
    {
      lock (this.tempHilightEntryListLock)
      {
        List<HilightEntry> newList = new List<HilightEntry>();
        foreach (HilightEntry he in this.tempHilightEntryList)
        {
          if (!he.IsSearchHit)
          {
            newList.Add(he);
          }
        }
        this.tempHilightEntryList = newList;
      }
      RefreshAllGrids();
    }

    private void markFilterHitsInLogViewToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SearchParams p = new SearchParams();
      p.searchText = this.filterParams.searchText;
      p.isRegex = this.filterParams.isRegex;
      p.isCaseSensitive = this.filterParams.isCaseSensitive;
      AddSearchHitHighlightEntry(p);
    }

    void statusLineTrigger_Signal(object sender, EventArgs e)
    {
      OnStatusLine(this.statusEventArgs);
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

    private void columnComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
      SelectColumn();
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

    private void columnComboBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        SelectColumn();
        this.dataGridView.Focus();
      }
    }

    private void columnComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
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

    private void bookmarkProvider_BookmarkRemoved(object sender, EventArgs e)
    {
      if (!this.isLoading)
      {
        this.dataGridView.Refresh();
        this.filterGridView.Refresh();
      }
    }

    private void bookmarkProvider_BookmarkAdded(object sender, EventArgs e)
    {
      if (!this.isLoading)
      {
        this.dataGridView.Refresh();
        this.filterGridView.Refresh();
      }
    }

    private void bookmarkProvider_AllBookmarksRemoved(object sender, EventArgs e)
    {
      // nothing
    }

    #region ILogPaintContext Member

    public string GetLogLine(int lineNum)
    {
      return this.logFileReader.GetLogLine(lineNum);
    }

    public Bookmark GetBookmarkForLine(int lineNum)
    {
      return this.bookmarkProvider.GetBookmarkForLine(lineNum);
    }

    public Font MonospacedFont
    {
      get { return this.fontMonospaced; }
    }

    public Font NormalFont
    {
      get { return this.font; }
    }

    public Font BoldFont
    {
      get { return this.fontBold; }
    }

    #endregion

    #region ILogView Member


    public void RefreshLogView()
    {
      this.RefreshAllGrids();
    }

    #endregion

    public IBookmarkData BookmarkData
    {
      get { return this.bookmarkProvider; }
    }

    protected override string GetPersistString()
    {
      return "LogWindow#" + FileName;
    }

    private void LogWindow_Leave(object sender, EventArgs e)
    {
      InvalidateCurrentRow();
    }

    private void LogWindow_Enter(object sender, EventArgs e)
    {
      InvalidateCurrentRow();
    }

  }
}
