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
    public partial class LogWindow
    {
        #region Private Methods

        private void RegisterLogFileReaderEvents()
        {
            logFileReader.LoadFile += logFileReader_LoadFile;
            logFileReader.LoadingFinished += logFileReader_FinishedLoading;
            logFileReader.LoadingStarted += logFileReader_LoadingStarted;
            logFileReader.FileNotFound += logFileReader_FileNotFound;
            logFileReader.Respawned += logFileReader_Respawned;
            // FileSizeChanged is not registered here because it's registered after loading has finished
        }

        private void UnRegisterLogFileReaderEvents()
        {
            if (logFileReader != null)
            {
                logFileReader.LoadFile -= logFileReader_LoadFile;
                logFileReader.LoadingFinished -= logFileReader_FinishedLoading;
                logFileReader.LoadingStarted -= logFileReader_LoadingStarted;
                logFileReader.FileNotFound -= logFileReader_FileNotFound;
                logFileReader.Respawned -= logFileReader_Respawned;
                logFileReader.FileSizeChanged -= FileSizeChangedHandler;
            }
        }

        private bool LoadPersistenceOptions()
        {
            if (InvokeRequired)
            {
                return (bool) Invoke(new BoolReturnDelegate(LoadPersistenceOptions));
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
                    persistenceData =
                        Persister.LoadPersistenceDataOptionsOnlyFromFixedFile(ForcedPersistenceFileName);
                }

                if (persistenceData == null)
                {
                    _logger.Info("No persistence data for {0} found.", FileName);
                    return false;
                }

                IsMultiFile = persistenceData.multiFile;
                multifileOptions = new MultifileOptions();
                multifileOptions.FormatPattern = persistenceData.multiFilePattern;
                multifileOptions.MaxDayTry = persistenceData.multiFileMaxDays;
                if (multifileOptions.FormatPattern == null || multifileOptions.FormatPattern.Length == 0)
                {
                    multifileOptions = ObjectClone.Clone<MultifileOptions>(Preferences.multifileOptions);
                }

                splitContainer1.SplitterDistance = persistenceData.filterPosition;
                splitContainer1.Panel2Collapsed = !persistenceData.filterVisible;
                ToggleHighlightPanel(persistenceData.filterSaveListVisible);
                ShowAdvancedFilterPanel(persistenceData.filterAdvanced);
                if (reloadMemento == null)
                {
                    PreselectColumnizer(persistenceData.columnizerName);
                }

                FollowTailChanged(persistenceData.followTail, false);
                if (persistenceData.tabName != null)
                {
                    Text = persistenceData.tabName;
                }

                AdjustHighlightSplitterWidth();
                SetCurrentHighlightGroup(persistenceData.highlightGroupName);
                if (persistenceData.multiFileNames.Count > 0)
                {
                    _logger.Info("Detected MultiFile name list in persistence options");
                    fileNames = new string[persistenceData.multiFileNames.Count];
                    persistenceData.multiFileNames.CopyTo(fileNames);
                }
                else
                {
                    fileNames = null;
                }

                //this.bookmarkWindow.ShowBookmarkCommentColumn = persistenceData.showBookmarkCommentColumn;
                SetExplicitEncoding(persistenceData.encoding);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading persistence data: ");
                return false;
            }
        }

        private void SetDefaultsFromPrefs()
        {
            filterTailCheckBox.Checked = Preferences.filterTail;
            syncFilterCheckBox.Checked = Preferences.filterSync;
            FollowTailChanged(Preferences.followTail, false);
            multifileOptions = ObjectClone.Clone<MultifileOptions>(Preferences.multifileOptions);
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

            ForcePersistenceLoading = false; // force only 1 time (while session load)

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

                if (persistenceData.lineCount > logFileReader.LineCount)
                {
                    // outdated persistence data (logfile rollover)
                    // MessageBox.Show(this, "Persistence data for " + this.FileName + " is outdated. It was discarded.", "Log Expert");
                    _logger.Info("Persistence data for {0} is outdated. It was discarded.", FileName);
                    LoadPersistenceOptions();
                    return;
                }

                bookmarkProvider.BookmarkList = persistenceData.bookmarkList;
                rowHeightList = persistenceData.rowHeightList;
                try
                {
                    if (persistenceData.currentLine >= 0 && persistenceData.currentLine < dataGridView.RowCount)
                    {
                        SelectLine(persistenceData.currentLine, false, true);
                    }
                    else
                    {
                        if (logFileReader.LineCount > 0)
                        {
                            dataGridView.FirstDisplayedScrollingRowIndex = logFileReader.LineCount - 1;
                            SelectLine(logFileReader.LineCount - 1, false, true);
                        }
                    }

                    if (persistenceData.firstDisplayedLine >= 0 &&
                        persistenceData.firstDisplayedLine < dataGridView.RowCount)
                    {
                        dataGridView.FirstDisplayedScrollingRowIndex = persistenceData.firstDisplayedLine;
                    }

                    if (persistenceData.followTail)
                    {
                        FollowTailChanged(persistenceData.followTail, false);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
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
            if (persistenceData.filterParamsList.Count > 0)
            {
                filterParams = persistenceData.filterParamsList[0];
                ReInitFilterParams(filterParams);
            }

            ApplyFilterParams(); // re-loaded filter settingss
            BeginInvoke(new MethodInvoker(FilterSearch));
            try
            {
                splitContainer1.SplitterDistance = persistenceData.filterPosition;
                splitContainer1.Panel2Collapsed = !persistenceData.filterVisible;
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e, "Error setting splitter distance: ");
            }

            ShowAdvancedFilterPanel(persistenceData.filterAdvanced);
            if (filterPipeList.Count == 0) // don't restore if it's only a reload
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
                List<int> lastFilterResultList = new List<int>();
                List<int> filterHitList = new List<int>();
                Filter(persistFilterParams, filterResultList, lastFilterLinesList, filterHitList);
                FilterPipe pipe = new FilterPipe(persistFilterParams.CreateCopy(), this);
                WritePipeToTab(pipe, filterResultList, data.persistenceData.tabName, data.persistenceData);
            }
        }

        private void ReInitFilterParams(FilterParams filterParams)
        {
            filterParams.searchText = filterParams.searchText; // init "lowerSearchText"
            filterParams.rangeSearchText = filterParams.rangeSearchText; // init "lowerRangesearchText"
            filterParams.currentColumnizer = CurrentColumnizer;
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
            _logger.Debug("EnterLoadFileStatus begin");

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(EnterLoadFileStatus));
                return;
            }

            statusEventArgs.StatusText = "Loading file...";
            statusEventArgs.LineCount = 0;
            statusEventArgs.FileSize = 0;
            SendStatusLineUpdate();

            progressEventArgs.MinValue = 0;
            progressEventArgs.MaxValue = 0;
            progressEventArgs.Value = 0;
            progressEventArgs.Visible = true;
            SendProgressBarUpdate();

            isLoading = true;
            shouldCancel = true;
            ClearFilterList();
            ClearBookmarkList();
            dataGridView.ClearSelection();
            dataGridView.RowCount = 0;
            _logger.Debug("EnterLoadFileStatus end");
        }

        private void PositionAfterReload(ReloadMemento reloadMemento)
        {
            if (this.reloadMemento.currentLine < dataGridView.RowCount && this.reloadMemento.currentLine >= 0)
            {
                dataGridView.CurrentCell = dataGridView.Rows[this.reloadMemento.currentLine].Cells[0];
            }

            if (this.reloadMemento.firstDisplayedLine < dataGridView.RowCount &&
                this.reloadMemento.firstDisplayedLine >= 0)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = this.reloadMemento.firstDisplayedLine;
            }
        }

        private void LogfileDead()
        {
            _logger.Info("File not found.");
            isDeadFile = true;

            //this.logFileReader.FileSizeChanged -= this.FileSizeChangedHandler;
            //if (this.logFileReader != null)
            //  this.logFileReader.stopMonitoring();

            dataGridView.Enabled = false;
            dataGridView.RowCount = 0;
            progressEventArgs.Visible = false;
            progressEventArgs.Value = progressEventArgs.MaxValue;
            SendProgressBarUpdate();
            statusEventArgs.FileSize = 0;
            statusEventArgs.LineCount = 0;
            statusEventArgs.CurrentLineNum = 0;
            SendStatusLineUpdate();
            shouldCancel = true;
            ClearFilterList();
            ClearBookmarkList();

            StatusLineText("File not found");
            OnFileNotFound(new EventArgs());
        }

        private void LogfileRespawned()
        {
            _logger.Info("LogfileDead(): Reloading file because it has been respawned.");
            isDeadFile = false;
            dataGridView.Enabled = true;
            StatusLineText("");
            OnFileRespawned(new EventArgs());
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
            //if (this.forcedColumnizer == null)
            {
                ILogLineColumnizer columnizer;
                if (forcedColumnizerForLoading != null)
                {
                    columnizer = forcedColumnizerForLoading;
                    forcedColumnizerForLoading = null;
                }
                else
                {
                    columnizer = FindColumnizer();
                    if (columnizer != null)
                    {
                        if (reloadMemento == null)
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

                Invoke(new SetColumnizerFx(SetColumnizer), new object[] {columnizer});
            }
            dataGridView.Enabled = true;
            DisplayCurrentFileOnStatusline();
            //this.guiStateArgs.FollowTail = this.Preferences.followTail;
            guiStateArgs.MultiFileEnabled = !IsTempFile;
            guiStateArgs.MenuEnabled = true;
            guiStateArgs.CurrentEncoding = logFileReader.CurrentEncoding;
            SendGuiStateUpdate();
            //if (this.dataGridView.RowCount > 0)
            //  SelectLine(this.dataGridView.RowCount - 1);
            //if (this.dataGridView.Columns.Count > 1)
            //{
            //  this.dataGridView.Columns[this.dataGridView.Columns.Count-1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //  this.dataGridView.Columns[this.dataGridView.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
            //  AdjustMinimumGridWith();
            //}
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

        private ILogLineColumnizer FindColumnizer()
        {
            ILogLineColumnizer columnizer = null;
            if (Preferences.maskPrio)
            {
                columnizer = parentLogTabWin.FindColumnizerByFileMask(Util.GetNameFromPath(FileName));
                if (columnizer == null)
                {
                    columnizer = parentLogTabWin.GetColumnizerHistoryEntry(FileName);
                }
            }
            else
            {
                columnizer = parentLogTabWin.GetColumnizerHistoryEntry(FileName);
                if (columnizer == null)
                {
                    columnizer = parentLogTabWin.FindColumnizerByFileMask(Util.GetNameFromPath(FileName));
                }
            }

            return columnizer;
        }

        private void ReloadNewFile()
        {
            // prevent "overloads". May occur on very fast rollovers (next rollover before the file is reloaded)
            lock (reloadLock)
            {
                reloadOverloadCounter++;
                _logger.Info("ReloadNewFile(): counter = {0}", reloadOverloadCounter);
                if (reloadOverloadCounter <= 1)
                {
                    SavePersistenceData(false);
                    loadingFinishedEvent.Reset();
                    externaLoadingFinishedEvent.Reset();
                    Thread reloadFinishedThread = new Thread(new ThreadStart(ReloadFinishedThreadFx));
                    reloadFinishedThread.IsBackground = true;
                    reloadFinishedThread.Start();
                    LoadFile(FileName, EncodingOptions);

                    ClearBookmarkList();
                    SavePersistenceData(false);

                    //if (this.filterTailCheckBox.Checked)
                    //{
                    //  _logger.logDebug("Waiting for loading to be complete.");
                    //  loadingFinishedEvent.WaitOne();
                    //  _logger.logDebug("Refreshing filter view because of reload.");
                    //  FilterSearch();
                    //}
                    //LoadFilterPipes();
                }
                else
                {
                    _logger.Debug("Preventing reload because of recursive calls.");
                }

                reloadOverloadCounter--;
            }
        }

        private void ReloadFinishedThreadFx()
        {
            _logger.Info("Waiting for loading to be complete.");
            loadingFinishedEvent.WaitOne();
            _logger.Info("Refreshing filter view because of reload.");
            Invoke(new MethodInvoker(FilterSearch));
            LoadFilterPipes();
        }

        private void UpdateProgress(LoadFileEventArgs e)
        {
            try
            {
                if (e.ReadPos >= e.FileSize)
                {
                    //_logger.Warn("UpdateProgress(): ReadPos (" + e.ReadPos + ") is greater than file size (" + e.FileSize + "). Aborting Update");
                    return;
                }

                statusEventArgs.FileSize = e.ReadPos;
                //this.progressEventArgs.Visible = true;
                progressEventArgs.MaxValue = (int) e.FileSize;
                progressEventArgs.Value = (int) e.ReadPos;
                SendProgressBarUpdate();
                SendStatusLineUpdate();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UpdateProgress(): ");
            }
        }

        private void LoadingStarted(LoadFileEventArgs e)
        {
            try
            {
                statusEventArgs.FileSize = e.ReadPos;
                statusEventArgs.StatusText = "Loading " + Util.GetNameFromPath(e.FileName);
                progressEventArgs.Visible = true;
                progressEventArgs.MaxValue = (int) e.FileSize;
                progressEventArgs.Value = (int) e.ReadPos;
                SendProgressBarUpdate();
                SendStatusLineUpdate();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "LoadingStarted(): ");
            }
        }

        private void LoadingFinished()
        {
            _logger.Info("File loading complete.");

            if (currentColumnizer is AutoColumnizer)
            {
                currentColumnizer = ((AutoColumnizer)currentColumnizer).FindColumnizer(FileName, logFileReader);
            }

            StatusLineText("");
            logFileReader.FileSizeChanged += FileSizeChangedHandler;
            isLoading = false;
            shouldCancel = false;
            dataGridView.SuspendLayout();
            dataGridView.RowCount = logFileReader.LineCount;
            dataGridView.CurrentCellChanged += new EventHandler(dataGridView_CurrentCellChanged);
            dataGridView.Enabled = true;
            dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            dataGridView.ResumeLayout();
            progressEventArgs.Visible = false;
            progressEventArgs.Value = progressEventArgs.MaxValue;
            SendProgressBarUpdate();
            //if (this.logFileReader.LineCount > 0)
            //{
            //  this.dataGridView.FirstDisplayedScrollingRowIndex = this.logFileReader.LineCount - 1;
            //  SelectLine(this.logFileReader.LineCount - 1);
            //}
            guiStateArgs.FollowTail = true;
            SendGuiStateUpdate();
            statusEventArgs.LineCount = logFileReader.LineCount;
            statusEventArgs.FileSize = logFileReader.FileSize;
            SendStatusLineUpdate();

            PreferencesChanged(parentLogTabWin.Preferences, true, SettingsFlags.All);
            //LoadPersistenceData();
        }

        private void LogEventWorker()
        {
            Thread.CurrentThread.Name = "LogEventWorker";
            while (true)
            {
                _logger.Debug("Waiting for signal");
                logEventArgsEvent.WaitOne();
                _logger.Debug("Wakeup signal received.");
                while (true)
                {
                    LogEventArgs e;
                    int lastLineCount = 0;
                    lock (logEventArgsList)
                    {
                        _logger.Info("{0} events in queue", logEventArgsList.Count);
                        if (logEventArgsList.Count == 0)
                        {
                            logEventArgsEvent.Reset();
                            break;
                        }

                        e = logEventArgsList[0];
                        logEventArgsList.RemoveAt(0);
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
                            _logger.Error("Line count of event is: {0}, should be greater than last line count: {1}", e.LineCount, lastLineCount);
                        }
                    }

                    UpdateGridCallback callback = new UpdateGridCallback(UpdateGrid);
                    Invoke(callback, new object[] {e});
                    CheckFilterAndHighlight(e);
                    timeSpreadCalc.SetLineCount(e.LineCount);
                }
            }
        }

        private void StopLogEventWorkerThread()
        {
            logEventArgsEvent.Set();
            logEventHandlerThread.Abort();
            logEventHandlerThread.Join();
        }

        private void OnFileSizeChanged(LogEventArgs e)
        {
            if (FileSizeChanged != null)
            {
                FileSizeChanged(this, e);
            }
        }

        private void UpdateGrid(LogEventArgs e)
        {
            int oldRowCount = dataGridView.RowCount;
            int firstDisplayedLine = dataGridView.FirstDisplayedScrollingRowIndex;

            if (dataGridView.CurrentCellAddress.Y >= e.LineCount)
            {
                //this.dataGridView.Rows[this.dataGridView.CurrentCellAddress.Y].Selected = false;
                //this.dataGridView.CurrentCell = this.dataGridView.Rows[0].Cells[0];
            }

            try
            {
                if (dataGridView.RowCount > e.LineCount)
                {
                    int currentLineNum = dataGridView.CurrentCellAddress.Y;
                    dataGridView.RowCount = 0;
                    dataGridView.RowCount = e.LineCount;
                    if (!guiStateArgs.FollowTail)
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

                _logger.Debug("UpdateGrid(): new RowCount={0}", dataGridView.RowCount);
                if (e.IsRollover)
                {
                    // Multifile rollover
                    // keep selection and view range, if no follow tail mode
                    if (!guiStateArgs.FollowTail)
                    {
                        int currentLineNum = dataGridView.CurrentCellAddress.Y;
                        currentLineNum -= e.RolloverOffset;
                        if (currentLineNum < 0)
                        {
                            currentLineNum = 0;
                        }

                        _logger.Debug("UpdateGrid(): Rollover=true, Rollover offset={0}, currLineNum was {1}, new currLineNum={2}", e.RolloverOffset, dataGridView.CurrentCellAddress.Y, currentLineNum);
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

                statusEventArgs.LineCount = e.LineCount;
                statusLineFileSize(e.FileSize);

                if (!isLoading)
                {
                    if (oldRowCount == 0)
                    {
                        AdjustMinimumGridWith();
                    }

                    //CheckFilterAndHighlight(e);
                }

                if (guiStateArgs.FollowTail && dataGridView.RowCount > 0)
                {
                    dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.RowCount - 1;
                    OnTailFollowed(new EventArgs());
                }

                if (Preferences.timestampControl && !isLoading)
                {
                    SetTimestampLimits();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Fehler bei UpdateGrid(): ");
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
            if (filterTailCheckBox.Checked || filterPipeList.Count > 0)
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
                    ILogLine line = logFileReader.GetLogLine(i);
                    if (line == null)
                    {
                        return;
                    }

                    if (filterTailCheckBox.Checked)
                    {
                        callback.LineNum = i;
                        if (Util.TestFilterCondition(filterParams, line, callback))
                        {
                            //AddFilterLineFx addFx = new AddFilterLineFx(AddFilterLine);
                            //this.Invoke(addFx, new object[] { i, true });
                            filterLineAdded = true;
                            AddFilterLine(i, false, filterParams, filterResultList, lastFilterLinesList,
                                filterHitList);
                        }
                    }

                    //ProcessFilterPipeFx pipeFx = new ProcessFilterPipeFx(ProcessFilterPipes);
                    //pipeFx.BeginInvoke(i, null, null);
                    ProcessFilterPipes(i);

                    IList<HilightEntry> matchingList = FindMatchingHilightEntries(line);
                    LaunchHighlightPlugins(matchingList, i);
                    GetHilightActions(matchingList, out suppressLed, out stopTail, out setBookmark,
                        out bookmarkComment);
                    if (setBookmark)
                    {
                        SetBookmarkFx fx = new SetBookmarkFx(SetBookmarkFromTrigger);
                        fx.BeginInvoke(i, bookmarkComment, null, null);
                    }

                    if (stopTail && guiStateArgs.FollowTail)
                    {
                        bool wasFollow = guiStateArgs.FollowTail;
                        FollowTailChanged(false, true);
                        if (firstStopTail && wasFollow)
                        {
                            Invoke(new SelectLineFx(SelectAndEnsureVisible), new object[] {i, false});
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
                    ILogLine line = logFileReader.GetLogLine(i);
                    if (line != null)
                    {
                        IList<HilightEntry> matchingList = FindMatchingHilightEntries(line);
                        LaunchHighlightPlugins(matchingList, i);
                        GetHilightActions(matchingList, out suppressLed, out stopTail, out setBookmark,
                            out bookmarkComment);
                        if (setBookmark)
                        {
                            SetBookmarkFx fx = new SetBookmarkFx(SetBookmarkFromTrigger);
                            fx.BeginInvoke(i, bookmarkComment, null, null);
                        }

                        if (stopTail && guiStateArgs.FollowTail)
                        {
                            bool wasFollow = guiStateArgs.FollowTail;
                            FollowTailChanged(false, true);
                            if (firstStopTail && wasFollow)
                            {
                                Invoke(new SelectLineFx(SelectAndEnsureVisible), new object[] {i, false});
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
                    IKeywordAction plugin =
                        PluginRegistry.GetInstance().FindKeywordActionPluginByName(entry.ActionEntry.pluginName);
                    if (plugin != null)
                    {
                        ActionPluginExecuteFx fx = new ActionPluginExecuteFx(plugin.Execute);
                        fx.BeginInvoke(entry.SearchText, entry.ActionEntry.actionParam, callback,
                            CurrentColumnizer, null, null);
                    }
                }
            }
        }

        private void PreSelectColumnizer(ILogLineColumnizer columnizer)
        {
            if (columnizer != null)
            {
                CurrentColumnizer = forcedColumnizerForLoading = columnizer;
            }
            else
            {
                CurrentColumnizer = forcedColumnizerForLoading =
                    PluginRegistry.GetInstance().RegisteredColumnizers[0];
            }
        }

        private void SetColumnizer(ILogLineColumnizer columnizer)
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

        private void SetColumnizerInternal(ILogLineColumnizer columnizer)
        {
            _logger.Info("SetColumnizerInternal(): {0}", columnizer.GetName());

            if (columnizer is AutoColumnizer)
            {
                columnizer = ((AutoColumnizer)columnizer).FindColumnizer(FileName, logFileReader);
            }


            ILogLineColumnizer oldColumnizer = CurrentColumnizer;
            bool oldColumnizerIsXmlType = CurrentColumnizer is ILogLineXmlColumnizer;
            bool oldColumnizerIsPreProcess = CurrentColumnizer is IPreProcessColumnizer;
            bool mustReload = false;

            // Check if the filtered columns disappeared, if so must refresh the UI
            if (filterParams.columnRestrict)
            {
                string[] newColumns = columnizer != null ? columnizer.GetColumnNames() : new string[0];
                bool colChanged = false;
                if (dataGridView.ColumnCount - 2 == newColumns.Length
                ) // two first columns are 'marker' and 'line number'
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
                    columnNamesLabel.Text = CalculateColumnNames(filterParams);
                }
            }

            Type oldColType = filterParams.currentColumnizer != null
                ? filterParams.currentColumnizer.GetType()
                : null;
            Type newColType = columnizer != null ? columnizer.GetType() : null;
            if (oldColType != newColType && filterParams.columnRestrict && filterParams.isFilterTail)
            {
                filterParams.columnList.Clear();
            }

            if (CurrentColumnizer == null || !CurrentColumnizer.GetType().Equals(columnizer.GetType()))
            {
                CurrentColumnizer = columnizer;
                freezeStateMap.Clear();
                if (logFileReader != null)
                {
                    if (CurrentColumnizer is IPreProcessColumnizer)
                    {
                        logFileReader.PreProcessColumnizer = (IPreProcessColumnizer) CurrentColumnizer;
                    }
                    else
                    {
                        logFileReader.PreProcessColumnizer = null;
                    }
                }

                // always reload when choosing XML columnizers
                if (logFileReader != null && CurrentColumnizer is ILogLineXmlColumnizer)
                {
                    //forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
                    mustReload = true;
                }

                // Reload when choosing no XML columnizer but previous columnizer was XML
                if (logFileReader != null && !(CurrentColumnizer is ILogLineXmlColumnizer) &&
                    oldColumnizerIsXmlType)
                {
                    logFileReader.IsXmlMode = false;
                    //forcedColumnizer = currentColumnizer; // prevent Columnizer selection on SetGuiAfterReload()
                    mustReload = true;
                }

                // Reload when previous columnizer was PreProcess and current is not, and vice versa.
                // When the current columnizer is a preProcess columnizer, reload in every case.
                if (CurrentColumnizer is IPreProcessColumnizer != oldColumnizerIsPreProcess ||
                    CurrentColumnizer is IPreProcessColumnizer
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

            if (oldColumnizer is IInitColumnizer)
            {
                (oldColumnizer as IInitColumnizer).DeSelected(new ColumnizerCallback(this));
            }

            if (columnizer is IInitColumnizer)
            {
                (columnizer as IInitColumnizer).Selected(new ColumnizerCallback(this));
            }

            SetColumnizer(columnizer, dataGridView);
            SetColumnizer(columnizer, filterGridView);
            if (patternWindow != null)
            {
                patternWindow.SetColumnizer(columnizer);
            }

            guiStateArgs.TimeshiftPossible = columnizer.IsTimeshiftImplemented();
            SendGuiStateUpdate();

            if (logFileReader != null)
            {
                dataGridView.RowCount = logFileReader.LineCount;
            }

            if (filterResultList != null)
            {
                filterGridView.RowCount = filterResultList.Count;
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
            foreach (string columnName in columnizer.GetColumnNames())
            {
                columnComboBox.Items.Add(columnName);
            }

            columnComboBox.SelectedIndex = 0;

            OnColumnizerChanged(CurrentColumnizer);
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
                    //gridView.Columns[gridView.Columns.Count - 1].Width = this.Preferences.lastColumnWidth;
                    gridView.Columns[gridView.Columns.Count - 1].MinimumWidth = Preferences.lastColumnWidth;
                }
            }
            catch (NullReferenceException e)
            {
                // See https://connect.microsoft.com/VisualStudio/feedback/details/366943/autoresizecolumns-in-datagridview-throws-nullreferenceexception
                // There are some rare situations with null ref exceptions when resizing columns and on filter finished
                // So catch them here. Better than crashing.
                _logger.Error(e, "Error while resizing columns: ");
            }
        }

        private void PaintCell(DataGridViewCellPaintingEventArgs e, DataGridView gridView, bool noBackgroundFill,
            HilightEntry groundEntry)
        {
            PaintHighlightedCell(e, gridView, noBackgroundFill, groundEntry);
        }

        private void PaintHighlightedCell(DataGridViewCellPaintingEventArgs e, DataGridView gridView,
            bool noBackgroundFill,
            HilightEntry groundEntry)
        {
            IColumn column = e.Value as IColumn;

            if (column == null)
            {
                column = Column.EmptyColumn;
            }

            IList<HilightMatchEntry> matchList = FindHilightMatches(column);
            // too many entries per line seem to cause problems with the GDI
            while (matchList.Count > 50)
            {
                matchList.RemoveAt(50);
            }

            HilightMatchEntry hme = new HilightMatchEntry();
            hme.StartPos = 0;
            hme.Length = column.DisplayValue.Length;
            hme.HilightEntry = new HilightEntry(column.DisplayValue,
                groundEntry != null
                    ? groundEntry.ForegroundColor
                    : Color.FromKnownColor(KnownColor.Black),
                groundEntry != null ? groundEntry.BackgroundColor : Color.Empty,
                false);

            if (groundEntry != null)
            {
                hme.HilightEntry.IsBold = groundEntry.IsBold;
            }

            matchList = MergeHighlightMatchEntries(matchList, hme);

            int leftPad = e.CellStyle.Padding.Left;
            RectangleF rect = new RectangleF(e.CellBounds.Left + leftPad, e.CellBounds.Top, e.CellBounds.Width,
                e.CellBounds.Height);
            Rectangle borderWidths = PaintHelper.BorderWidths(e.AdvancedBorderStyle);
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
                Font font = matchEntry != null && matchEntry.HilightEntry.IsBold ? BoldFont : NormalFont;
                Brush bgBrush = matchEntry.HilightEntry.BackgroundColor != Color.Empty
                    ? new SolidBrush(matchEntry.HilightEntry.BackgroundColor)
                    : null;
                string matchWord = column.DisplayValue.Substring(matchEntry.StartPos, matchEntry.Length);
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

        /// <summary>
        /// Builds a list of HilightMatchEntry objects. A HilightMatchEntry spans over a region that is painted with the same foreground and
        /// background colors.
        /// All regions which don't match a word-mode entry will be painted with the colors of a default entry (groundEntry). This is either the
        /// first matching non-word-mode highlight entry or a black-on-white default (if no matching entry was found).
        /// </summary>
        /// <param name="matchList">List of all highlight matches for the current cell</param>
        /// <param name="groundEntry">The entry that is used as the default.</param>
        /// <returns>List of HilightMatchEntry objects. The list spans over the whole cell and contains color infos for every substring.</returns>
        private IList<HilightMatchEntry> MergeHighlightMatchEntries(IList<HilightMatchEntry> matchList,
            HilightMatchEntry groundEntry)
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

        /**
       * Returns the first HilightEntry that matches the given line
       */

        private HilightEntry FindHilightEntry(ITextValue line)
        {
            return FindHilightEntry(line, false);
        }

        private HilightEntry FindFirstNoWordMatchHilightEntry(ITextValue line)
        {
            return FindHilightEntry(line, true);
        }

        private bool CheckHighlightEntryMatch(HilightEntry entry, ITextValue column)
        {
            if (entry.IsRegEx)
            {
                //Regex rex = new Regex(entry.SearchText, entry.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                if (entry.Regex.IsMatch(column.Text))
                {
                    return true;
                }
            }
            else
            {
                if (entry.IsCaseSensitive)
                {
                    if (column.Text.Contains(entry.SearchText))
                    {
                        return true;
                    }
                }
                else
                {
                    if (column.Text.ToLower().Contains(entry.SearchText.ToLower()))
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

        private IList<HilightEntry> FindMatchingHilightEntries(ITextValue line)
        {
            IList<HilightEntry> resultList = new List<HilightEntry>();
            if (line != null)
            {
                lock (currentHighlightGroupLock)
                {
                    foreach (HilightEntry entry in currentHighlightGroup.HilightEntryList)
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

        private void GetHighlightEntryMatches(ITextValue line, IList<HilightEntry> hilightEntryList,
            IList<HilightMatchEntry> resultList)
        {
            foreach (HilightEntry entry in hilightEntryList)
            {
                if (entry.IsWordMatch)
                {
                    MatchCollection matches = entry.Regex.Matches(line.Text);
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
                        me.Length = line.Text.Length;
                        resultList.Add(me);
                    }
                }
            }
        }

        private void GetHilightActions(IList<HilightEntry> matchingList, out bool noLed, out bool stopTail,
            out bool setBookmark, out string bookmarkComment)
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
                {
                    stopTail = true;
                }
            }

            bookmarkComment.TrimEnd(new char[] {'\r', '\n'});
        }

        private void StopTimespreadThread()
        {
            timeSpreadCalc.Stop();
        }

        private void StopTimestampSyncThread()
        {
            shouldTimestampDisplaySyncingCancel = true;
            timeshiftSyncWakeupEvent.Set();
            timeshiftSyncThread.Abort();
            timeshiftSyncThread.Join();
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
            timeshiftSyncLine = lineNum;
            timeshiftSyncTimerEvent.Set();
            timeshiftSyncWakeupEvent.Set();
        }

        private void SyncTimestampDisplayWorker()
        {
            const int WAIT_TIME = 500;
            Thread.CurrentThread.Name = "SyncTimestampDisplayWorker";
            shouldTimestampDisplaySyncingCancel = false;
            isTimestampDisplaySyncing = true;

            while (!shouldTimestampDisplaySyncingCancel)
            {
                timeshiftSyncWakeupEvent.WaitOne();
                if (shouldTimestampDisplaySyncingCancel)
                {
                    return;
                }

                timeshiftSyncWakeupEvent.Reset();

                while (!shouldTimestampDisplaySyncingCancel)
                {
                    bool signaled = timeshiftSyncTimerEvent.WaitOne(WAIT_TIME, true);
                    timeshiftSyncTimerEvent.Reset();
                    if (!signaled)
                    {
                        break;
                    }
                }

                // timeout with no new Trigger -> update display
                int lineNum = timeshiftSyncLine;
                if (lineNum >= 0 && lineNum < dataGridView.RowCount)
                {
                    int refLine = lineNum;
                    DateTime timeStamp = GetTimestampForLine(ref refLine, true);
                    if (!timeStamp.Equals(DateTime.MinValue) && !shouldTimestampDisplaySyncingCancel)
                    {
                        guiStateArgs.Timestamp = timeStamp;
                        SendGuiStateUpdate();
                        if (shouldCallTimeSync)
                        {
                            refLine = lineNum;
                            DateTime exactTimeStamp = GetTimestampForLine(ref refLine, false);
                            SyncOtherWindows(exactTimeStamp);
                            shouldCallTimeSync = false;
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
                    //TimeSpan span = TimeSpan.FromTicks(timeStamp2.Ticks - timeStamp1.Ticks);
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
                if (filterResultList.Count > 0)
                {
                    int index = filterResultList.BinarySearch(dataGridView.CurrentRow.Index);
                    if (index < 0)
                    {
                        index = ~index;
                        if (index > 0)
                        {
                            --index;
                        }
                    }

                    if (filterGridView.Rows.Count > 0) // exception no rows
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

        private void statusLineFileSize(long size)
        {
            statusEventArgs.FileSize = size;
            SendStatusLineUpdate();
        }

        private int Search(SearchParams searchParams)
        {
            UpdateProgressBarFx progressFx = new UpdateProgressBarFx(UpdateProgressBar);
            if (searchParams.searchText == null)
            {
                return -1;
            }

            int lineNum = searchParams.isFromTop && !searchParams.isFindNext ? 0 : searchParams.currentLine;
            string lowerSearchText = searchParams.searchText.ToLower();
            int count = 0;
            bool hasWrapped = false;
            while (true)
            {
                if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
                {
                    if (lineNum >= logFileReader.LineCount)
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
                        lineNum = logFileReader.LineCount - 1;
                        hasWrapped = true;
                        StatusLineError("Started from end of file");
                    }
                }

                ILogLine line = logFileReader.GetLogLine(lineNum);
                if (line == null)
                {
                    return -1;
                }

                if (searchParams.isRegex)
                {
                    Regex rex = new Regex(searchParams.searchText,
                        searchParams.isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    if (rex.IsMatch(line.FullLine))
                    {
                        return lineNum;
                    }
                }
                else
                {
                    if (!searchParams.isCaseSensitive)
                    {
                        if (line.FullLine.ToLower().Contains(lowerSearchText))
                        {
                            return lineNum;
                        }
                    }
                    else
                    {
                        if (line.FullLine.Contains(searchParams.searchText))
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

                if (shouldCancel)
                {
                    return -1;
                }

                if (++count % PROGRESS_BAR_MODULO == 0)
                {
                    try
                    {
                        if (!Disposing)
                        {
                            Invoke(progressFx, new object[] {count});
                        }
                    }
                    catch (ObjectDisposedException ex) // can occur when closing the app while searching
                    {
                        _logger.Warn(ex);
                    }
                }
            }
        }

        private void SearchComplete(IAsyncResult result)
        {
            if (Disposing)
            {
                return;
            }

            try
            {
                Invoke(new MethodInvoker(ResetProgressBar));
                AsyncResult ar = (AsyncResult) result;
                SearchFx fx = (SearchFx) ar.AsyncDelegate;
                int line = fx.EndInvoke(result);
                guiStateArgs.MenuEnabled = true;
                GuiStateUpdate(this, guiStateArgs);
                if (line == -1)
                {
                    return;
                }

                dataGridView.Invoke(new SelectLineFx((line1, triggerSyncCall) => SelectLine(line1, triggerSyncCall, true)), new object[] {line, true});
            }
            catch (Exception ex) // in the case the windows is already destroyed
            {
                _logger.Warn(ex);
            }
        }

        private void ResetProgressBar()
        {
            progressEventArgs.Value = progressEventArgs.MaxValue;
            progressEventArgs.Visible = false;
            SendProgressBarUpdate();
        }

        private void SelectLine(int line, bool triggerSyncCall, bool shouldScroll)
        {
            try
            {
                shouldCallTimeSync = triggerSyncCall;
                bool wasCancelled = shouldCancel;
                shouldCancel = false;
                isSearching = false;
                StatusLineText("");
                guiStateArgs.MenuEnabled = true;
                if (wasCancelled)
                {
                    return;
                }

                if (line == -1)
                {
                    MessageBox.Show(this, "Not found:",
                        "Search result"); // Hmm... is that experimental code from early days?
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

        private void StartEditMode()
        {
            if (!dataGridView.CurrentCell.ReadOnly)
            {
                dataGridView.BeginEdit(false);
                if (dataGridView.EditingControl != null)
                {
                    LogCellEditingControl editControl = dataGridView.EditingControl as LogCellEditingControl;

                    if (editControl != null)
                    {
                        editControl.KeyDown += new KeyEventHandler(editControl_KeyDown);
                        editControl.KeyPress += new KeyPressEventHandler(editControl_KeyPress);
                        editControl.KeyUp += new KeyEventHandler(editControl_KeyUp);
                        editControl.Click += new EventHandler(editControl_Click);
                        dataGridView.CellEndEdit += new DataGridViewCellEventHandler(dataGridView_CellEndEdit);
                        editControl.SelectionStart = 0;
                    }
                    else
                    {
                        _logger.Warn("Edit control in logWindow was null");
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
                _logger.Debug("SelStart: {0}, SelLen: {1}", editControl.SelectionStart, editControl.SelectionLength);
            }
        }

        private void SelectPrevHighlightLine()
        {
            int lineNum = dataGridView.CurrentCellAddress.Y;
            while (lineNum > 0)
            {
                lineNum--;
                ILogLine line = logFileReader.GetLogLine(lineNum);
                if (line != null)
                {
                    HilightEntry entry = FindHilightEntry(line);
                    if (entry != null)
                    {
                        SelectLine(lineNum, false, true);
                        break;
                    }
                }
            }
        }

        private void SelectNextHighlightLine()
        {
            int lineNum = dataGridView.CurrentCellAddress.Y;
            while (lineNum < logFileReader.LineCount)
            {
                lineNum++;
                ILogLine line = logFileReader.GetLogLine(lineNum);
                if (line != null)
                {
                    HilightEntry entry = FindHilightEntry(line);
                    if (entry != null)
                    {
                        SelectLine(lineNum, false, true);
                        break;
                    }
                }
            }
        }

        private int FindNextBookmarkIndex(int lineNum)
        {
            if (lineNum >= dataGridView.RowCount)
            {
                lineNum = 0;
            }
            else
            {
                lineNum++;
            }

            return bookmarkProvider.FindNextBookmarkIndex(lineNum);
        }

        private int FindPrevBookmarkIndex(int lineNum)
        {
            if (lineNum <= 0)
            {
                lineNum = dataGridView.RowCount - 1;
            }
            else
            {
                lineNum--;
            }

            return bookmarkProvider.FindPrevBookmarkIndex(lineNum);
        }

        /**
       * Shift bookmarks after a logfile rollover
       */

        private void ShiftBookmarks(int offset)
        {
            bookmarkProvider.ShiftBookmarks(offset);
            OnBookmarkRemoved();
        }

        private void ShiftRowHeightList(int offset)
        {
            SortedList<int, RowHeightEntry> newList = new SortedList<int, RowHeightEntry>();
            foreach (RowHeightEntry entry in rowHeightList.Values)
            {
                int line = entry.LineNum - offset;
                if (line >= 0)
                {
                    entry.LineNum = line;
                    newList.Add(line, entry);
                }
            }

            rowHeightList = newList;
        }

        private void ShiftFilterPipes(int offset)
        {
            lock (filterPipeList)
            {
                foreach (FilterPipe pipe in filterPipeList)
                {
                    pipe.ShiftLineNums(offset);
                }
            }
        }

        private void LoadFilterPipes()
        {
            lock (filterPipeList)
            {
                foreach (FilterPipe pipe in filterPipeList)
                {
                    pipe.RecreateTempFile();
                }
            }

            if (filterPipeList.Count > 0)
            {
                for (int i = 0; i < dataGridView.RowCount; ++i)
                {
                    ProcessFilterPipes(i);
                }
            }
        }

        private void DisconnectFilterPipes()
        {
            lock (filterPipeList)
            {
                foreach (FilterPipe pipe in filterPipeList)
                {
                    pipe.ClearLineList();
                }
            }
        }

        private void ApplyFilterParams()
        {
            filterComboBox.Text = filterParams.searchText;
            filterCaseSensitiveCheckBox.Checked = filterParams.isCaseSensitive;
            filterRegexCheckBox.Checked = filterParams.isRegex;
            filterTailCheckBox.Checked = filterParams.isFilterTail;
            invertFilterCheckBox.Checked = filterParams.isInvert;
            filterKnobControl1.Value = filterParams.spreadBefore;
            filterKnobControl2.Value = filterParams.spreadBehind;
            rangeCheckBox.Checked = filterParams.isRangeSearch;
            columnRestrictCheckBox.Checked = filterParams.columnRestrict;
            fuzzyKnobControl.Value = filterParams.fuzzyValue;
            filterRangeComboBox.Text = filterParams.rangeSearchText;
        }

        private void ResetFilterControls()
        {
            filterComboBox.Text = "";
            filterCaseSensitiveCheckBox.Checked = false;
            filterRegexCheckBox.Checked = false;
            //this.filterTailCheckBox.Checked = this.Preferences.filterTail;
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
                filterParams.searchText = "";
                filterParams.lowerSearchText = "";
                filterParams.isRangeSearch = false;
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
            FireCancelHandlers(); // make sure that there's no other filter running (maybe from filter restore)

            filterParams.searchText = text;
            filterParams.lowerSearchText = text.ToLower();
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

            filterParams.isRangeSearch = rangeCheckBox.Checked;
            filterParams.rangeSearchText = filterRangeComboBox.Text;
            if (filterParams.isRangeSearch)
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

            filterParams.isCaseSensitive = filterCaseSensitiveCheckBox.Checked;
            filterParams.isRegex = filterRegexCheckBox.Checked;
            filterParams.isFilterTail = filterTailCheckBox.Checked;
            filterParams.isInvert = invertFilterCheckBox.Checked;
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

            filterParams.fuzzyValue = fuzzyKnobControl.Value;
            filterParams.spreadBefore = filterKnobControl1.Value;
            filterParams.spreadBehind = filterKnobControl2.Value;
            filterParams.columnRestrict = columnRestrictCheckBox.Checked;

            //ConfigManager.SaveFilterParams(this.filterParams);
            ConfigManager.Settings.filterParams = filterParams; // wozu eigentlich? sinnlos seit MDI?

            shouldCancel = false;
            isSearching = true;
            StatusLineText("Filtering... Press ESC to cancel");
            filterSearchButton.Enabled = false;
            ClearFilterList();

            progressEventArgs.MinValue = 0;
            progressEventArgs.MaxValue = dataGridView.RowCount;
            progressEventArgs.Value = 0;
            progressEventArgs.Visible = true;
            SendProgressBarUpdate();

            Settings settings = ConfigManager.Settings;
            FilterFx fx;
            fx = settings.preferences.multiThreadFilter ? new FilterFx(MultiThreadedFilter) : new FilterFx(Filter);
            fx.BeginInvoke(filterParams, filterResultList, lastFilterLinesList, filterHitList,
                FilterComplete, null);
            CheckForFilterDirty();
        }

        private void MultiThreadedFilter(FilterParams filterParams, List<int> filterResultLines,
            List<int> lastFilterLinesList, List<int> filterHitList)
        {
            ColumnizerCallback callback = new ColumnizerCallback(this);
            FilterStarter fs = new FilterStarter(callback, Environment.ProcessorCount + 2);
            fs.FilterHitList = this.filterHitList;
            fs.FilterResultLines = filterResultList;
            fs.LastFilterLinesList = this.lastFilterLinesList;
            BackgroundProcessCancelHandler cancelHandler = new FilterCancelHandler(fs);
            RegisterCancelHandler(cancelHandler);
            long startTime = Environment.TickCount;

            fs.DoFilter(filterParams, 0, logFileReader.LineCount, FilterProgressCallback);

            long endTime = Environment.TickCount;

            _logger.Debug("Multi threaded filter duration: {0} ms.", endTime - startTime);

            DeRegisterCancelHandler(cancelHandler);
            StatusLineText("Filter duration: " + (endTime - startTime) + " ms.");
        }

        private void FilterProgressCallback(int lineCount)
        {
            UpdateProgressBar(lineCount);
        }

        private void Filter(FilterParams filterParams, List<int> filterResultLines, List<int> lastFilterLinesList,
            List<int> filterHitList)
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
                    ILogLine line = logFileReader.GetLogLine(lineNum);
                    if (line == null)
                    {
                        break;
                    }

                    callback.LineNum = lineNum;
                    if (Util.TestFilterCondition(filterParams, line, callback))
                    {
                        AddFilterLine(lineNum, false, filterParams, filterResultLines, lastFilterLinesList,
                            filterHitList);
                    }

                    lineNum++;
                    if (lineNum % PROGRESS_BAR_MODULO == 0)
                    {
                        UpdateProgressBar(lineNum);
                    }

                    if (shouldCancel)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while filtering. Please report to developer: ");
                MessageBox.Show(null,
                    "Exception while filtering. Please report to developer: \n\n" + ex + "\n\n" + ex.StackTrace,
                    "LogExpert");
            }

            long endTime = Environment.TickCount;

            _logger.Info("Single threaded filter duration: {0} ms.", endTime - startTime);

            StatusLineText("Filter duration: " + (endTime - startTime) + " ms.");
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
                if (lineNum + i < logFileReader.LineCount)
                {
                    if (!resultList.Contains(lineNum + i) && !checkList.Contains(lineNum + i))
                    {
                        resultList.Add(lineNum + i);
                    }
                }
            }

            return resultList;
        }

        private void AddFilterLine(int lineNum, bool immediate, FilterParams filterParams, List<int> filterResultLines,
            List<int> lastFilterLinesList, List<int> filterHitList)
        {
            int count;
            lock (filterResultList)
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

        private void TriggerFilterLineGuiUpdate()
        {
            //lock (this.filterUpdateThread)
            //{
            //  this.filterEventCount++;
            //  this.filterUpdateEvent.Set();
            //}
            Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
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

        //    //_logger.logDebug("FilterUpdateWorker: Waiting for signal");
        //    //bool signaled = this.filterUpdateEvent.WaitOne(1000, false);

        //    //if (!signaled)
        //    //{
        //    //  lock (this.filterUpdateThread)
        //    //  {
        //    //    if (this.filterEventCount > 0)
        //    //    {
        //    //      this.filterEventCount = 0;
        //    //      _logger.logDebug("FilterUpdateWorker: Invoking GUI update because of wait timeout");
        //    //      this.Invoke(new MethodInvoker(AddFilterLineGuiUpdate));
        //    //    }
        //    //  }
        //    //}
        //    //else
        //    //{
        //    //  _logger.logDebug("FilterUpdateWorker: Wakeup signal received.");
        //    //  lock (this.filterUpdateThread)
        //    //  {
        //    //    _logger.logDebug("FilterUpdateWorker: event count: " + this.filterEventCount);
        //    //    if (this.filterEventCount > 100)
        //    //    {
        //    //      this.filterEventCount = 0;
        //    //      _logger.logDebug("FilterUpdateWorker: Invoking GUI update because of event count");
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
                lock (filterResultList)
                {
                    filterCountLabel.Text = "" + filterResultList.Count;
                    if (filterGridView.RowCount > filterResultList.Count)
                    {
                        filterGridView.RowCount = 0; // helps to prevent hang ?
                    }

                    filterGridView.RowCount = filterResultList.Count;
                    if (filterGridView.RowCount > 0)
                    {
                        filterGridView.FirstDisplayedScrollingRowIndex = filterGridView.RowCount - 1;
                    }

                    if (filterGridView.RowCount == 1)
                    {
                        // after a file reload adjusted column sizes anew when the first line arrives
                        //this.filterGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                        AutoResizeColumns(filterGridView);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "AddFilterLineGuiUpdate(): ");
            }
        }

        private void UpdateProgressBar(int value)
        {
            progressEventArgs.Value = value;
            if (value > progressEventArgs.MaxValue)
            {
                // can occur if new lines will be added while filtering
                progressEventArgs.MaxValue = value;
            }

            SendProgressBarUpdate();
        }

        private void FilterComplete(IAsyncResult result)
        {
            if (!IsDisposed && !waitingForClose && !Disposing)
            {
                Invoke(new MethodInvoker(ResetStatusAfterFilter));
            }
        }

        private void ResetStatusAfterFilter()
        {
            try
            {
                //StatusLineText("");
                isSearching = false;
                progressEventArgs.Value = progressEventArgs.MaxValue;
                progressEventArgs.Visible = false;
                SendProgressBarUpdate();
                filterGridView.RowCount = filterResultList.Count;
                //this.filterGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                AutoResizeColumns(filterGridView);
                filterCountLabel.Text = "" + filterResultList.Count;
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
                _logger.Error(e, "Error: ");
            }
        }

        private void ClearFilterList()
        {
            try
            {
                //this.shouldCancel = true;
                lock (filterResultList)
                {
                    filterGridView.SuspendLayout();
                    filterGridView.RowCount = 0;
                    filterCountLabel.Text = "0";
                    filterResultList = new List<int>();
                    lastFilterLinesList = new List<int>();
                    filterHitList = new List<int>();
                    //this.filterGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                    filterGridView.ResumeLayout();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Wieder dieser sporadische Fehler: ");

                MessageBox.Show(null, ex.StackTrace, "Wieder dieser sporadische Fehler:");
            }
        }

        private void ClearBookmarkList()
        {
            bookmarkProvider.ClearAllBookmarks();
        }

        /**
       * Shift filter list line entries after a logfile rollover
       */

        private void ShiftFilterLines(int offset)
        {
            List<int> newFilterList = new List<int>();
            lock (filterResultList)
            {
                foreach (int lineNum in filterResultList)
                {
                    int line = lineNum - offset;
                    if (line >= 0)
                    {
                        newFilterList.Add(line);
                    }
                }

                filterResultList = newFilterList;
            }

            newFilterList = new List<int>();
            foreach (int lineNum in filterHitList)
            {
                int line = lineNum - offset;
                if (line >= 0)
                {
                    newFilterList.Add(line);
                }
            }

            filterHitList = newFilterList;

            int count = SPREAD_MAX;
            if (filterResultList.Count < SPREAD_MAX)
            {
                count = filterResultList.Count;
            }

            lastFilterLinesList = filterResultList.GetRange(filterResultList.Count - count, count);

            //this.filterGridView.RowCount = this.filterResultList.Count;
            //this.filterCountLabel.Text = "" + this.filterResultList.Count;
            //this.BeginInvoke(new MethodInvoker(this.filterGridView.Refresh));
            //this.BeginInvoke(new MethodInvoker(AddFilterLineGuiUpdate));
            TriggerFilterLineGuiUpdate();
        }

        private void CheckForFilterDirty()
        {
            if (IsFilterSearchDirty(filterParams))
            {
                filterSearchButton.Image = searchButtonImage;
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
                //this.dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
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
            if (logFileReader.IsMultiFile)
            {
                try
                {
                    if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.Index > -1)
                    {
                        string fileName =
                            logFileReader.GetLogFileNameForLine(dataGridView.CurrentRow.Index);
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
            if (noSelectionUpdates)
            {
                return;
            }

            selectionChangedTrigger.Trigger();
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
            statusEventArgs.StatusText = text;
            SendStatusLineUpdate();
        }

        private void StatusLineError(string text)
        {
            StatusLineText(text);
            isErrorShowing = true;
        }

        private void RemoveStatusLineError()
        {
            StatusLineText("");
            isErrorShowing = false;
        }

        private void SendGuiStateUpdate()
        {
            OnGuiState(guiStateArgs);
        }

        private void SendProgressBarUpdate()
        {
            OnProgressBarUpdate(progressEventArgs);
        }

        private void SendStatusLineUpdate()
        {
            //OnStatusLine(this.statusEventArgs);
            statusLineTrigger.Trigger();
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
            showAdvanced = show;
        }

        private void CheckForAdvancedButtonDirty()
        {
            if (IsAdvancedOptionActive() && !showAdvanced)
            {
                advancedButton.Image = advancedButtonImage;
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
            FilterPipe pipe = new FilterPipe(filterParams.CreateCopy(), this);
            lock (filterResultList)
            {
                string namePrefix = "->F";
                string title;
                if (IsTempFile)
                {
                    title = TempTitleName + namePrefix + ++filterPipeNameCounter;
                }
                else
                {
                    title = Util.GetNameFromPath(FileName) + namePrefix + ++filterPipeNameCounter;
                }

                WritePipeToTab(pipe, filterResultList, title, null);
            }
        }

        private void WritePipeToTab(FilterPipe pipe, IList<int> lineNumberList, string name,
            PersistenceData persistenceData)
        {
            _logger.Info("WritePipeToTab(): {0} lines.", lineNumberList.Count);
            StatusLineText("Writing to temp file... Press ESC to cancel.");
            guiStateArgs.MenuEnabled = false;
            SendGuiStateUpdate();
            progressEventArgs.MinValue = 0;
            progressEventArgs.MaxValue = lineNumberList.Count;
            progressEventArgs.Value = 0;
            progressEventArgs.Visible = true;
            Invoke(new MethodInvoker(SendProgressBarUpdate));
            isSearching = true;
            shouldCancel = false;

            lock (filterPipeList)
            {
                filterPipeList.Add(pipe);
            }

            pipe.Closed += new FilterPipe.ClosedEventHandler(pipe_Disconnected);
            int count = 0;
            pipe.OpenFile();
            LogExpertCallback callback = new LogExpertCallback(this);
            foreach (int i in lineNumberList)
            {
                if (shouldCancel)
                {
                    break;
                }

                ILogLine line = logFileReader.GetLogLine(i);
                if (CurrentColumnizer is ILogLineXmlColumnizer)
                {
                    callback.LineNum = i;
                    line = (CurrentColumnizer as ILogLineXmlColumnizer).GetLineTextForClipboard(line,
                        callback);
                }

                pipe.WriteToPipe(line, i);
                if (++count % PROGRESS_BAR_MODULO == 0)
                {
                    progressEventArgs.Value = count;
                    Invoke(new MethodInvoker(SendProgressBarUpdate));
                }
            }

            pipe.CloseFile();
            _logger.Info("WritePipeToTab(): finished");
            Invoke(new WriteFilterToTabFinishedFx(WriteFilterToTabFinished),
                new object[] {pipe, name, persistenceData});
        }

        private void WriteFilterToTabFinished(FilterPipe pipe, string name, PersistenceData persistenceData)
        {
            isSearching = false;
            if (!shouldCancel)
            {
                string title = name;
                ILogLineColumnizer preProcessColumnizer = null;
                if (!(CurrentColumnizer is ILogLineXmlColumnizer))
                {
                    preProcessColumnizer = CurrentColumnizer;
                }

                LogWindow newWin = parentLogTabWin.AddFilterTab(pipe, title,
                    preProcessColumnizer);
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

            progressEventArgs.Value = progressEventArgs.MaxValue;
            progressEventArgs.Visible = false;
            SendProgressBarUpdate();
            guiStateArgs.MenuEnabled = true;
            SendGuiStateUpdate();
            StatusLineText("");
            filterSearchButton.Enabled = true;
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
            Invoke(new WriteFilterToTabFinishedFx(WriteFilterToTabFinished), new object[] {pipe, title, null});
        }

        private void FilterRestore(LogWindow newWin, PersistenceData persistenceData)
        {
            newWin.WaitForLoadingFinished();
            ILogLineColumnizer columnizer = Util.FindColumnizerByName(persistenceData.columnizerName,
                PluginRegistry.GetInstance().RegisteredColumnizers);
            if (columnizer != null)
            {
                SetColumnizerFx fx = new SetColumnizerFx(newWin.ForceColumnizer);
                newWin.Invoke(fx, new object[] {columnizer});
            }
            else
            {
                _logger.Warn("FilterRestore(): Columnizer {0} not found", persistenceData.columnizerName);
            }

            newWin.BeginInvoke(new RestoreFiltersFx(newWin.RestoreFilters), new object[] {persistenceData});
        }

        private void ProcessFilterPipes(int lineNum)
        {
            ILogLine searchLine = logFileReader.GetLogLine(lineNum);
            if (searchLine == null)
            {
                return;
            }

            ColumnizerCallback callback = new ColumnizerCallback(this);
            callback.LineNum = lineNum;
            IList<FilterPipe> deleteList = new List<FilterPipe>();
            lock (filterPipeList)
            {
                foreach (FilterPipe pipe in filterPipeList)
                {
                    if (pipe.IsStopped)
                    {
                        continue;
                    }

                    long startTime = Environment.TickCount;
                    if (Util.TestFilterCondition(pipe.FilterParams, searchLine, callback))
                    {
                        IList<int> filterResult =
                            GetAdditionalFilterResults(pipe.FilterParams, lineNum, pipe.LastLinesHistoryList);
                        pipe.OpenFile();
                        foreach (int line in filterResult)
                        {
                            pipe.LastLinesHistoryList.Add(line);
                            if (pipe.LastLinesHistoryList.Count > SPREAD_MAX * 2)
                            {
                                pipe.LastLinesHistoryList.RemoveAt(0);
                            }

                            ILogLine textLine = logFileReader.GetLogLine(line);
                            bool fileOk = pipe.WriteToPipe(textLine, line);
                            if (!fileOk)
                            {
                                deleteList.Add(pipe);
                            }
                        }

                        pipe.CloseFile();
                    }

                    long endTime = Environment.TickCount;
                    //_logger.logDebug("ProcessFilterPipes(" + lineNum + ") duration: " + ((endTime - startTime)));
                }
            }

            foreach (FilterPipe pipe in deleteList)
            {
                filterPipeList.Remove(pipe);
            }
        }

        private void CopyMarkedLinesToClipboard()
        {
            if (guiStateArgs.CellSelectMode)
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

                var xmlColumnizer = currentColumnizer as ILogLineXmlColumnizer;

                foreach (int lineNum in lineNumList)
                {
                    ILogLine line = logFileReader.GetLogLine(lineNum);
                    if (xmlColumnizer != null)
                    {
                        callback.LineNum = lineNum;
                        line = xmlColumnizer.GetLineTextForClipboard(line, callback);
                    }

                    clipText.AppendLine(line.FullLine);
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
            EncodingOptions.Encoding = encoding;
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

                    dataGridView.Columns[dataGridView.Columns.Count - 1].MinimumWidth = 5; // default
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

        /* ========================================================================
       * Timestamp stuff
       * =======================================================================*/

        private void SetTimestampLimits()
        {
            if (!CurrentColumnizer.IsTimeshiftImplemented())
            {
                return;
            }

            int line = 0;
            guiStateArgs.MinTimestamp = GetTimestampForLineForward(ref line, true);
            line = dataGridView.RowCount - 1;
            guiStateArgs.MaxTimestamp = GetTimestampForLine(ref line, true);
            SendGuiStateUpdate();
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
                dataGridView.Refresh();
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
                    if (colIndex < dataGridView.Columns.Count - 2)
                    {
                        if (names.Length > 0)
                        {
                            names += ", ";
                        }

                        names += dataGridView.Columns[2 + colIndex]
                            .HeaderText; // skip first two columns: marker + line number
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
                col.Frozen = freezeStateMap.ContainsKey(gridView) && freezeStateMap[gridView];
                bool sel = col.HeaderCell.Selected;
                if (col.Index == selectedCol)
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

            timeSpreadCalc.Enabled = show;
        }

        private void AddTempFileTab(string fileName, string title)
        {
            parentLogTabWin.AddTempFileTab(fileName, title);
        }

        private void InitPatternWindow()
        {
            //PatternStatistic(this.patternArgs);
            patternWindow = new PatternWindow(this);
            patternWindow.SetColumnizer(CurrentColumnizer);
            //this.patternWindow.SetBlockList(blockList);
            patternWindow.SetFont(Preferences.fontName, Preferences.fontSize);
            patternWindow.Fuzzy = patternArgs.fuzzy;
            patternWindow.MaxDiff = patternArgs.maxDiffInBlock;
            patternWindow.MaxMisses = patternArgs.maxMisses;
            patternWindow.Weight = patternArgs.minWeight;
            //this.patternWindow.Show();
        }

        private void TestStatistic(PatternArgs patternArgs)
        {
            int beginLine = patternArgs.startLine;
            _logger.Info("TestStatistics() called with start line {0}", beginLine);

            this.patternArgs = patternArgs;

            int num = beginLine + 1; //this.dataGridView.RowCount;

            progressEventArgs.MinValue = 0;
            progressEventArgs.MaxValue = dataGridView.RowCount;
            progressEventArgs.Value = beginLine;
            progressEventArgs.Visible = true;
            SendProgressBarUpdate();

            PrepareDict();
            ResetCache(num);

            Dictionary<int, int> processedLinesDict = new Dictionary<int, int>();
            List<PatternBlock> blockList = new List<PatternBlock>();
            int blockId = 0;
            isSearching = true;
            shouldCancel = false;
            int searchLine = -1;
            for (int i = beginLine; i < num && !shouldCancel; ++i)
            {
                if (processedLinesDict.ContainsKey(i))
                {
                    continue;
                }

                PatternBlock block;
                int maxBlockLen = patternArgs.endLine - patternArgs.startLine;
                //int searchLine = i + 1;
                _logger.Debug("TestStatistic(): i={0} searchLine={1}", i, searchLine);
                //bool firstBlock = true;
                searchLine++;
                UpdateProgressBar(searchLine);
                while (!shouldCancel &&
                       (block =
                           DetectBlock(i, searchLine, maxBlockLen, this.patternArgs.maxDiffInBlock,
                               this.patternArgs.maxMisses,
                               processedLinesDict)) != null)
                {
                    _logger.Debug("Found block: {0}", block);
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

            isSearching = false;
            progressEventArgs.MinValue = 0;
            progressEventArgs.MaxValue = 0;
            progressEventArgs.Value = 0;
            progressEventArgs.Visible = false;
            SendProgressBarUpdate();
            //if (this.patternWindow.IsDisposed)
            //{
            //  this.Invoke(new MethodInvoker(CreatePatternWindow));
            //}
            patternWindow.SetBlockList(blockList, this.patternArgs);
            _logger.Info("TestStatistics() ended");
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

        //Well keep this for the moment because there is some other commented code which calls this one
        private PatternBlock FindExistingBlock(PatternBlock block, List<PatternBlock> blockList)
        {
            foreach (PatternBlock searchBlock in blockList)
            {
                if ((block.startLine > searchBlock.startLine &&
                     block.startLine < searchBlock.endLine
                     ||
                     block.endLine > searchBlock.startLine &&
                     block.endLine < searchBlock.endLine) && block.startLine != searchBlock.startLine &&
                    block.endLine != searchBlock.endLine
                )
                {
                    return searchBlock;
                }
            }

            return null;
        }

        private PatternBlock DetectBlock(int startNum, int startLineToSearch, int maxBlockLen, int maxDiffInBlock,
            int maxMisses, Dictionary<int, int> processedLinesDict)
        {
            int targetLine = FindSimilarLine(startNum, startLineToSearch, processedLinesDict);
            if (targetLine == -1)
            {
                return null;
            }

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

            while (!shouldCancel)
            {
                srcLine++;
                len++;
                //if (srcLine >= block.targetStart)
                //  break;  // prevent to search in the own block
                if (maxBlockLen > 0 && len > maxBlockLen)
                {
                    break;
                }

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

        private void PrepareDict()
        {
            lineHashList.Clear();
            Regex regex = new Regex("\\d");
            Regex regex2 = new Regex("\\S");

            int num = logFileReader.LineCount;
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
            int value = lineHashList[srcLine];

            int num = lineHashList.Count;
            for (int i = startLine; i < num; ++i)
            {
                if (Math.Abs(lineHashList[i] - value) < 3)
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
            int threshold = patternArgs.fuzzy;

            bool prepared = false;
            Regex regex = null;
            Regex regex2 = null;
            string msgToFind = null;
            CultureInfo culture = CultureInfo.CurrentCulture;

            int num = logFileReader.LineCount;
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
            ILogLine line = logFileReader.GetLogLine(i);
            ILogLineColumnizer columnizer = CurrentColumnizer;
            ColumnizerCallback callback = new ColumnizerCallback(this);
            IColumnizedLogLine cols = columnizer.SplitLine(callback, line);
            return cols.ColumnValues.Last().FullValue;
        }

        private void ChangeRowHeight(bool decrease)
        {
            int rowNum = dataGridView.CurrentCellAddress.Y;
            if (rowNum < 0 || rowNum >= dataGridView.RowCount)
            {
                return;
            }

            if (decrease)
            {
                if (!rowHeightList.ContainsKey(rowNum))
                {
                    return;
                }
                else
                {
                    RowHeightEntry entry = rowHeightList[rowNum];
                    entry.Height = entry.Height - lineHeight;
                    if (entry.Height <= lineHeight)
                    {
                        rowHeightList.Remove(rowNum);
                    }
                }
            }
            else
            {
                RowHeightEntry entry;
                if (!rowHeightList.ContainsKey(rowNum))
                {
                    entry = new RowHeightEntry();
                    entry.LineNum = rowNum;
                    entry.Height = lineHeight;
                    rowHeightList[rowNum] = entry;
                }
                else
                {
                    entry = rowHeightList[rowNum];
                }

                entry.Height = entry.Height + lineHeight;
            }

            dataGridView.UpdateRowHeightInfo(rowNum, false);
            if (rowNum == dataGridView.RowCount - 1 && guiStateArgs.FollowTail)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = rowNum;
            }

            dataGridView.Refresh();
        }

        private int GetRowHeight(int rowNum)
        {
            if (rowHeightList.ContainsKey(rowNum))
            {
                return rowHeightList[rowNum].Height;
            }
            else
            {
                return lineHeight;
            }
        }

        private void AddBookmarkAtLineSilently(int lineNum)
        {
            if (!bookmarkProvider.IsBookmarkAtLine(lineNum))
            {
                bookmarkProvider.AddBookmark(new Bookmark(lineNum));
            }
        }

        private void AddBookmarkAndEditComment()
        {
            int lineNum = dataGridView.CurrentCellAddress.Y;
            if (!bookmarkProvider.IsBookmarkAtLine(lineNum))
            {
                ToggleBookmark();
            }

            BookmarkComment(bookmarkProvider.GetBookmarkForLine(lineNum));
        }

        private void AddBookmarkComment(string text)
        {
            int lineNum = dataGridView.CurrentCellAddress.Y;
            Bookmark bookmark;
            if (!bookmarkProvider.IsBookmarkAtLine(lineNum))
            {
                bookmarkProvider.AddBookmark(bookmark = new Bookmark(lineNum));
            }
            else
            {
                bookmark = bookmarkProvider.GetBookmarkForLine(lineNum);
            }

            bookmark.Text = bookmark.Text + text;
            dataGridView.Refresh();
            filterGridView.Refresh();
            OnBookmarkTextChanged(bookmark);
        }

        private void markCurrentFilterRange()
        {
            filterParams.rangeSearchText = filterRangeComboBox.Text;
            ColumnizerCallback callback = new ColumnizerCallback(this);
            RangeFinder rangeFinder = new RangeFinder(filterParams, callback);
            Range range = rangeFinder.FindRange(dataGridView.CurrentCellAddress.Y);
            if (range != null)
            {
                SetCellSelectionMode(false);
                noSelectionUpdates = true;
                for (int i = range.StartLine; i <= range.EndLine; ++i)
                {
                    dataGridView.Rows[i].Selected = true;
                }

                noSelectionUpdates = false;
                UpdateSelectionDisplay();
            }
        }

        private void RemoveTempHighlights()
        {
            lock (tempHilightEntryListLock)
            {
                tempHilightEntryList.Clear();
            }

            RefreshAllGrids();
        }

        /// <summary>
        /// Highlights the done event worker.
        /// </summary>
        /// <param name="e">The <see cref="LogExpert.HighlightEventArgs"/> instance containing the event data.</param>
        private void HighlightDoneEventWorker(HighlightEventArgs e)
        {
            if (dataGridView.FirstDisplayedScrollingRowIndex > e.StartLine
                && dataGridView.FirstDisplayedScrollingRowIndex < e.StartLine + e.Count
                ||
                dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(true)
                > e.StartLine
                &&
                dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(true) <
                e.StartLine + e.Count)
            {
                BeginInvoke(new MethodInvoker(RefreshAllGrids));
            }
        }

        private void ToggleHighlightPanel(bool open)
        {
            highlightSplitContainer.Panel2Collapsed = !open;
            toggleHighlightPanelButton.Image = open ? panelCloseButtonImage : panelOpenButtonImage;
        }

        private void SetBoomarksForSelectedFilterLines()
        {
            lock (filterResultList)
            {
                foreach (DataGridViewRow row in filterGridView.SelectedRows)
                {
                    int lineNum = filterResultList[row.Index];
                    AddBookmarkAtLineSilently(lineNum);
                }
            }

            dataGridView.Refresh();
            filterGridView.Refresh();
            OnBookmarkAdded();
        }

        private void SetDefaultHighlightGroup()
        {
            HilightGroup group = parentLogTabWin.FindHighlightGroupByFileMask(FileName);
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
            parentLogTabWin.Preferences.isFilterOnLoad = filterOnLoadCheckBox.Checked;
            parentLogTabWin.Preferences.isAutoHideFilterList = hideFilterListOnLoadCheckBox.Checked;
            OnFilterListChanged(this);
        }

        private void FireCancelHandlers()
        {
            lock (cancelHandlerList)
            {
                foreach (BackgroundProcessCancelHandler handler in cancelHandlerList)
                {
                    handler.EscapePressed();
                }
            }
        }

        private void SyncOtherWindows(DateTime timestamp)
        {
            lock (timeSyncListLock)
            {
                if (TimeSyncList != null)
                {
                    TimeSyncList.NavigateToTimestamp(timestamp, this);
                }
            }
        }

        private void AddSlaveToTimesync(LogWindow slave)
        {
            lock (timeSyncListLock)
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

                    int currentLineNum = dataGridView.CurrentCellAddress.Y;
                    int refLine = currentLineNum;
                    DateTime timeStamp = GetTimestampForLine(ref refLine, true);
                    if (!timeStamp.Equals(DateTime.MinValue) && !shouldTimestampDisplaySyncingCancel)
                    {
                        TimeSyncList.CurrentTimestamp = timeStamp;
                    }

                    TimeSyncList.WindowRemoved += timeSyncList_WindowRemoved;
                }
            }

            slave.AddToTimeSync(this);
            OnSyncModeChanged();
        }

        private void FreeSlaveFromTimesync(LogWindow slave)
        {
            slave.FreeFromTimeSync();
        }

        private void OnSyncModeChanged()
        {
            if (SyncModeChanged != null)
            {
                SyncModeChanged(this, new SyncModeEventArgs(IsTimeSynced));
            }
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
            lock (tempHilightEntryListLock)
            {
                tempHilightEntryList.Add(he);
            }

            RefreshAllGrids();
        }

        private void RemoveAllSearchHighlightEntries()
        {
            lock (tempHilightEntryListLock)
            {
                List<HilightEntry> newList = new List<HilightEntry>();
                foreach (HilightEntry he in tempHilightEntryList)
                {
                    if (!he.IsSearchHit)
                    {
                        newList.Add(he);
                    }
                }

                tempHilightEntryList = newList;
            }

            RefreshAllGrids();
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
                int currentLine = dataGridView.CurrentCellAddress.Y;
                if (currentLine >= 0)
                {
                    dataGridView.CurrentCell =
                        dataGridView.Rows[dataGridView.CurrentCellAddress.Y].Cells[col.Index];
                }
            }
        }

        #endregion
    }
}