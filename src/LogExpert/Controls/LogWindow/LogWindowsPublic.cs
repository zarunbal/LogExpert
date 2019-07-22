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
using WeifenLuo.WinFormsUI.Docking;
using LogExpert.Classes.Columnizer;

namespace LogExpert
{
    public partial class LogWindow
    {
        #region Public methods

        public void LoadFile(string fileName, EncodingOptions encodingOptions)
        {
            EnterLoadFileStatus();

            if (fileName != null)
            {
                this.FileName = fileName;
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
                                columnizer = ColumnizerPicker.CloneColumnizer(columnizer);
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
                    _logger.Error(lfe);
                    MessageBox.Show("Cannot load file\n" + lfe.Message, "LogExpert");
                    this.BeginInvoke(new FunctionWith1BoolParam(Close), new object[] {true});
                    this.isLoadError = true;
                    return;
                }

                ILogLineXmlColumnizer xmlColumnizer = this.CurrentColumnizer as ILogLineXmlColumnizer;
                if (xmlColumnizer != null)
                {
                    this.logFileReader.IsXmlMode = true;
                    this.logFileReader.XmlLogConfig =
                        xmlColumnizer.GetXmlLogConfiguration();
                }
                if (this.forcedColumnizerForLoading != null)
                {
                    this.CurrentColumnizer = this.forcedColumnizerForLoading;
                }
                IPreProcessColumnizer processColumnizer = this.CurrentColumnizer as IPreProcessColumnizer;
                if (processColumnizer != null)
                {
                    this.logFileReader.PreProcessColumnizer = processColumnizer;
                }
                else
                {
                    this.logFileReader.PreProcessColumnizer = null;
                }
                RegisterLogFileReaderEvents();
                _logger.Info("Loading logfile: {0}", fileName);
                this.logFileReader.startMonitoring();
            }
        }

        public void LoadFilesAsMulti(string[] fileNames, EncodingOptions encodingOptions)
        {
            _logger.Info("Loading given files as MultiFile:");

            EnterLoadFileStatus();

            foreach (string name in fileNames)
            {
                _logger.Info("File: {0}", name);
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
            this.FileName = fileNames[fileNames.Length - 1];
            this.fileNames = fileNames;
            this.IsMultiFile = true;
            //if (this.isTempFile)
            //  this.Text = this.tempTitleName;
            //else
            //  this.Text = Util.GetNameFromPath(this.FileName);
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

            if (this.IsTempFile || this.isLoadError)
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
                _logger.Error(ex, "Error saving persistence: ");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unexpected error while saving persistence: {e.Message}");
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
            this.filterParams.isFilterTail =
                this.filterTailCheckBox.Checked; // this option doesnt need a press on 'search'
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
                _logger.Info("Deleting temp file {0}", this.FileName);
                try
                {
                    File.Delete(this.FileName);
                }
                catch (IOException e)
                {
                    _logger.Error(e, "Error while deleting temp file {0}: {1}", this.FileName, e);
                }
            }
            if (this.FilterPipe != null)
            {
                this.FilterPipe.CloseAndDisconnect();
            }
            DisconnectFilterPipes();
        }

        public void WaitForLoadingFinished()
        {
            this.externaLoadingFinishedEvent.WaitOne();
        }

        public void ForceColumnizer(ILogLineColumnizer columnizer)
        {
            this.forcedColumnizer = ColumnizerPicker.CloneColumnizer(columnizer);
            SetColumnizer(this.forcedColumnizer);
        }

        public void ForceColumnizerForLoading(ILogLineColumnizer columnizer)
        {
            this.forcedColumnizerForLoading = ColumnizerPicker.CloneColumnizer(columnizer);
        }

        public void PreselectColumnizer(string columnizerName)
        {
            ILogLineColumnizer columnizer = ColumnizerPicker.FindColumnizerByName(columnizerName,
                PluginRegistry.GetInstance().RegisteredColumnizers);
            PreSelectColumnizer(ColumnizerPicker.CloneColumnizer(columnizer));
        }

        public void ColumnizerConfigChanged()
        {
            SetColumnizerInternal(this.CurrentColumnizer);
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

        public IColumn GetCellValue(int rowIndex, int columnIndex)
        {
            if (columnIndex == 1)
            {
                return new Column
                {
                    FullValue = (rowIndex + 1).ToString() // line number
                };
            }
            if (columnIndex == 0) // marker column
            {
                return Column.EmptyColumn;
            }

            try
            {
                IColumnizedLogLine cols = GetColumnsForLine(rowIndex);
                if (cols != null && cols.ColumnValues != null)
                {
                    if (columnIndex <= cols.ColumnValues.Length + 1)
                    {
                        IColumn value = cols.ColumnValues[columnIndex - 2];

                        if (value != null && value.DisplayValue != null)
                        {
                            return value;
                        }
                        return value;
                    }
                    else
                    {
                        if (columnIndex == 2)
                        {
                            return cols.ColumnValues[cols.ColumnValues.Length - 1];
                        }
                        else
                        {
                            return Column.EmptyColumn;
                            ;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return Column.EmptyColumn;
            }
            return Column.EmptyColumn;
        }

        public void CellPainting(DataGridView gridView, int rowIndex, DataGridViewCellPaintingEventArgs e)
        {
            if (rowIndex < 0 || e.ColumnIndex < 0)
            {
                e.Handled = false;
                return;
            }
            ILogLine line = this.logFileReader.GetLogLineWithWait(rowIndex);
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
                            e.Graphics.DrawString("i", font, brush2, new RectangleF(r.Left, r.Top, r.Width, r.Height),
                                format);
                            font.Dispose();
                            brush2.Dispose();
                        }
                    }
                }

                e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                e.Handled = true;
            }
        }

        public void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView gridView = (DataGridView) sender;
            CellPainting(gridView, e.RowIndex, e);
        }

        /**
       * Returns the first HilightEntry that matches the given line
       */

        public HilightEntry FindHilightEntry(ITextValue line, bool noWordMatches)
        {
            // first check the temp entries
            lock (this.tempHilightEntryListLock)
            {
                foreach (HilightEntry entry in this.tempHilightEntryList)
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

            lock (this.currentHighlightGroupLock)
            {
                foreach (HilightEntry entry in this.currentHighlightGroup.HilightEntryList)
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

        public IList<HilightMatchEntry> FindHilightMatches(ITextValue column)
        {
            IList<HilightMatchEntry> resultList = new List<HilightMatchEntry>();
            if (column != null)
            {
                lock (this.currentHighlightGroupLock)
                {
                    GetHighlightEntryMatches(column, this.currentHighlightGroup.HilightEntryList, resultList);
                }
                lock (this.tempHilightEntryList)
                {
                    GetHighlightEntryMatches(column, this.tempHilightEntryList, resultList);
                }
            }
            return resultList;
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
                {
                    SelectLine(line, false, true);
                }
                else
                {
                    SelectLine(this.dataGridView.RowCount - 1, false, true);
                }
                this.dataGridView.Focus();
            }
        }

        public void StartSearch()
        {
            this.guiStateArgs.MenuEnabled = false;
            GuiStateUpdate(this, this.guiStateArgs);
            SearchParams searchParams = this.parentLogTabWin.SearchParams;
            if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
            {
                searchParams.currentLine = this.dataGridView.CurrentCellAddress.Y + 1;
            }
            else
            {
                searchParams.currentLine = this.dataGridView.CurrentCellAddress.Y - 1;
            }

            this.currentSearchParams = searchParams; // remember for async "not found" messages

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

        public void SelectLogLine(int line)
        {
            this.Invoke(new SelectLineFx((line1, triggerSyncCall) => this.SelectLine(line1, triggerSyncCall, true)), new object[] {line, true});
        }

        public void SelectAndEnsureVisible(int line, bool triggerSyncCall)
        {
            try
            {
                SelectLine(line, triggerSyncCall, false);

                //if (!this.dataGridView.CurrentRow.Displayed)
                if (line < this.dataGridView.FirstDisplayedScrollingRowIndex ||
                    line > this.dataGridView.FirstDisplayedScrollingRowIndex +
                    this.dataGridView.DisplayedRowCount(false))
                {
                    this.dataGridView.FirstDisplayedScrollingRowIndex = line;
                    for (int i = 0;
                        i < 8 && this.dataGridView.FirstDisplayedScrollingRowIndex > 0 &&
                        line < this.dataGridView.FirstDisplayedScrollingRowIndex +
                        this.dataGridView.DisplayedRowCount(false);
                        ++i)
                    {
                        this.dataGridView.FirstDisplayedScrollingRowIndex =
                            this.dataGridView.FirstDisplayedScrollingRowIndex - 1;
                    }
                    if (line >= this.dataGridView.FirstDisplayedScrollingRowIndex +
                        this.dataGridView.DisplayedRowCount(false))
                    {
                        this.dataGridView.FirstDisplayedScrollingRowIndex =
                            this.dataGridView.FirstDisplayedScrollingRowIndex + 1;
                    }
                }
                this.dataGridView.CurrentCell = this.dataGridView.Rows[line].Cells[0];
            }
            catch (Exception e)
            {
                // In rare situations there seems to be an invalid argument exceptions (or something like this). Concrete location isn't visible in stack
                // trace because use of Invoke(). So catch it, and log (better than crashing the app).
                _logger.Error(e);
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
                {
                    return;
                }
                this.parentLogTabWin.SearchParams.isFindNext = true;
                this.parentLogTabWin.SearchParams.isShiftF3Pressed = (e.Modifiers & Keys.Shift) == Keys.Shift;
                StartSearch();
            }
            if (e.KeyCode == Keys.Escape)
            {
                if (this.isSearching)
                {
                    this.shouldCancel = true;
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
                int newLine = this.logFileReader.GetNextMultiFileLine(this.dataGridView.CurrentCellAddress.Y);
                if (newLine != -1)
                {
                    SelectLine(newLine, false, true);
                }
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                int newLine = this.logFileReader.GetPrevMultiFileLine(this.dataGridView.CurrentCellAddress.Y);
                if (newLine != -1)
                {
                    SelectLine(newLine - 1, false, true);
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
            if (e.KeyCode == Keys.T && (e.Modifiers & Keys.Control) == Keys.Control &&
                (e.Modifiers & Keys.Shift) == Keys.Shift)
            {
                FilterToTab();
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
                            r = this.dataGridView.GetCellDisplayRectangle(0,
                                this.dataGridView.FirstDisplayedScrollingRowIndex, false);
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
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.Debug("AddBookmarkOverlay() r.Location={0}, width={1}, scroll_offset={2}", r.Location.X, r.Width, this.dataGridView.HorizontalScrollingOffset);
                        }
                        overlay.Position = r.Location - new Size(this.dataGridView.HorizontalScrollingOffset, 0);
                        overlay.Position = overlay.Position + new Size(10, r.Height / 2);
                        this.dataGridView.AddOverlay(overlay);
                    }
                }
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
                    if (DialogResult.No ==
                        MessageBox.Show("There's a comment attached to the bookmark. Really remove the bookmark?",
                            "LogExpert", MessageBoxButtons.YesNo))
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
                ILogLine line = this.logFileReader.GetLogLine(lineNum);
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
                if (this.bookmarkProvider.IsBookmarkAtLine(lineNum))
                {
                    this.bookmarkProvider.RemoveBookmarkForLine(lineNum);
                }
                this.bookmarkProvider.AddBookmark(new Bookmark(lineNum, comment));
                OnBookmarkAdded();
            }
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
                        {
                            break;
                        }
                    }
                }
                else
                {
                    int index = FindNextBookmarkIndex(this.dataGridView.CurrentCellAddress.Y);
                    if (index > this.bookmarkProvider.Bookmarks.Count - 1)
                    {
                        index = 0;
                    }

                    int lineNum = this.bookmarkProvider.Bookmarks[index].LineNum;
                    SelectLine(lineNum, true, true);
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
                    {
                        index = this.bookmarkProvider.Bookmarks.Count - 1;
                    }
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
                        {
                            break;
                        }
                    }
                }
                else
                {
                    int index = FindPrevBookmarkIndex(this.dataGridView.CurrentCellAddress.Y);
                    if (index < 0)
                    {
                        index = this.bookmarkProvider.Bookmarks.Count - 1;
                    }

                    int lineNum = this.bookmarkProvider.Bookmarks[index].LineNum;
                    SelectLine(lineNum, false, true);
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
                    if (this.bookmarkProvider.IsBookmarkAtLine(lineNum) &&
                        this.bookmarkProvider.GetBookmarkForLine(lineNum).Text.Length > 0)
                    {
                        bookmarksPresent = true;
                    }
                }
            }
            if (bookmarksPresent)
            {
                if (
                    MessageBox.Show("There are some comments in the bookmarks. Really remove bookmarks?", "LogExpert",
                        MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }
            bookmarkProvider.RemoveBookmarksForLines(lineNumList);
            OnBookmarkRemoved();
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
                            int diff = (int) (timeSpan.Ticks / TimeSpan.TicksPerMillisecond);
                            this.CurrentColumnizer.SetTimeOffset(diff);
                        }
                        catch (Exception)
                        {
                            this.CurrentColumnizer.SetTimeOffset(0);
                        }
                    }
                    else
                    {
                        this.CurrentColumnizer.SetTimeOffset(0);
                    }
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
                    _logger.Error(ex);
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

        /// <summary>
        /// Change the file encoding. May force a reload if byte count ot preamble lenght differs from previous used encoding.
        /// </summary>
        /// <param name="encoding"></param>
        public void ChangeEncoding(Encoding encoding)
        {
            this.logFileReader.ChangeEncoding(encoding);
            this.EncodingOptions.Encoding = encoding;
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
            //  _logger.logInfo("Refreshing filter view because of reload.");
            //  FilterSearch();
            //}
        }

        public void PreferencesChanged(Preferences newPreferences, bool isLoadTime, SettingsFlags flags)
        {
            if ((flags & SettingsFlags.GuiOrColors) == SettingsFlags.GuiOrColors)
            {
                this.NormalFont = new Font(new FontFamily(newPreferences.fontName), newPreferences.fontSize);
                this.BoldFont = new Font(this.NormalFont, FontStyle.Bold);
                this.MonospacedFont = new Font("Courier New", this.Preferences.fontSize, FontStyle.Bold);

                int lineSpacing = NormalFont.FontFamily.GetLineSpacing(FontStyle.Regular);
                float lineSpacingPixel =
                    NormalFont.Size * lineSpacing / NormalFont.FontFamily.GetEmHeight(FontStyle.Regular);

                this.dataGridView.DefaultCellStyle.Font = NormalFont;
                this.filterGridView.DefaultCellStyle.Font = NormalFont;
                this.lineHeight = NormalFont.Height + 4;
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

        public bool ScrollToTimestamp(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ScrollToTimestampFx(this.ScrollToTimestampWorker),
                    new object[] {timestamp, roundToSeconds, triggerSyncCall});
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
            int foundLine =
                FindTimestampLine_Internal(lineNum, 0, this.dataGridView.RowCount - 1, timestamp, roundToSeconds);
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

        public int FindTimestampLine_Internal(int lineNum, int rangeStart, int rangeEnd, DateTime timestamp,
            bool roundToSeconds)
        {
            _logger.Debug("FindTimestampLine_Internal(): timestamp={0}, lineNum={1}, rangeStart={2}, rangeEnd={3}", timestamp, lineNum, rangeStart, rangeEnd);
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
                _logger.Debug("GetTimestampForLine({0}) enter", lineNum);
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
                        ILogLine logLine = this.logFileReader.GetLogLine(lineNum);
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
                {
                    lineNum++;
                }
                _logger.Debug("GetTimestampForLine() leave with lineNum={0}", lineNum);
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
                        ILogLine logLine = this.logFileReader.GetLogLine(lineNum);
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
                {
                    lineNum--;
                }
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

        public ILogLine GetCurrentLine()
        {
            if (this.dataGridView.CurrentRow != null && this.dataGridView.CurrentRow.Index != -1)
            {
                return this.logFileReader.GetLogLine(this.dataGridView.CurrentRow.Index);
            }
            return null;
        }

        public ILogLine GetLine(int lineNum)
        {
            if (lineNum < 0 || logFileReader == null || lineNum >= this.logFileReader.LineCount)
            {
                return null;
            }
            return this.logFileReader.GetLogLine(lineNum);
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
            return this.logFileReader.GetRealLineNumForVirtualLineNum(lineNum);
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

        // =============== end of bookmark stuff ===================================

        public void ShowLineColumn(bool show)
        {
            this.dataGridView.Columns[1].Visible = show;
            this.filterGridView.Columns[1].Visible = show;
        }

        // =================================================================
        // Pattern statistics
        // =================================================================

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
            PatternStatisticFx fx = new PatternStatisticFx(TestStatistic);
            fx.BeginInvoke(patternArgs, null, null);
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
                    BookmarkExporter.ExportBookmarkList(this.bookmarkProvider.BookmarkList, this.FileName,
                        dlg.FileName);
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
            dlg.DefaultExt = "csv";
            dlg.Filter = "CSV file (*.csv)|*.csv|Bookmark file (*.bmk)|*.bmk";
            dlg.FilterIndex = 1;
            dlg.FileName = Path.GetFileNameWithoutExtension(this.FileName);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // add to the existing bookmarks
                    SortedList<int, Bookmark> newBookmarks = new SortedList<int, Bookmark>();
                    BookmarkExporter.ImportBookmarkList(this.FileName, dlg.FileName, newBookmarks);

                    // Add (or replace) to existing bookmark list
                    bool bookmarkAdded = false;
                    foreach (Bookmark b in newBookmarks.Values)
                    {
                        if (!this.bookmarkProvider.BookmarkList.ContainsKey(b.LineNum))
                        {
                            this.bookmarkProvider.BookmarkList.Add(b.LineNum, b);
                            bookmarkAdded = true; // refresh the list only once at the end
                        }
                        else
                        {
                            Bookmark existingBookmark = this.bookmarkProvider.BookmarkList[b.LineNum];
                            existingBookmark.Text =
                                b.Text; // replace existing bookmark for that line, preserving the overlay
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
                    _logger.Error(e);
                    MessageBox.Show("Error while importing bookmark list: " + e.Message, "LogExpert");
                }
            }
        }

        public bool IsAdvancedOptionActive()
        {
            return this.rangeCheckBox.Checked ||
                   this.fuzzyKnobControl.Value > 0 ||
                   this.filterKnobControl1.Value > 0 ||
                   this.filterKnobControl2.Value > 0 ||
                   this.invertFilterCheckBox.Checked ||
                   this.columnRestrictCheckBox.Checked;
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
            _logger.Info("Syncing window for {0} to {1}", Util.GetNameFromPath(this.FileName), Util.GetNameFromPath(master.FileName));
            lock (this.timeSyncListLock)
            {
                if (this.IsTimeSynced && master.TimeSyncList != this.TimeSyncList)
                    // already synced but master has different sync list
                {
                    FreeFromTimeSync();
                }
                this.TimeSyncList = master.TimeSyncList;
                this.TimeSyncList.AddWindow(this);
                this.ScrollToTimestamp(this.TimeSyncList.CurrentTimestamp, false, false);
            }
            OnSyncModeChanged();
        }

        public void FreeFromTimeSync()
        {
            lock (this.timeSyncListLock)
            {
                if (this.TimeSyncList != null)
                {
                    _logger.Info("De-Syncing window for {0}", Util.GetNameFromPath(this.FileName));
                    this.TimeSyncList.WindowRemoved -= timeSyncList_WindowRemoved;
                    this.TimeSyncList.RemoveWindow(this);
                    this.TimeSyncList = null;
                }
            }
            OnSyncModeChanged();
        }

        public void RefreshLogView()
        {
            this.RefreshAllGrids();
        }

        #endregion
    }
}