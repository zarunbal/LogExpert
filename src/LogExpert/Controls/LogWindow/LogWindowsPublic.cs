﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using LogExpert.Classes;
using LogExpert.Classes.Bookmark;
using LogExpert.Classes.Columnizer;
using LogExpert.Classes.Filter;
using LogExpert.Classes.Highlight;
using LogExpert.Classes.Log;
using LogExpert.Classes.Persister;
using LogExpert.Config;
using LogExpert.Entities;
using LogExpert.Entities.EventArgs;

using FileLockFinder;


namespace LogExpert.Controls.LogWindow
{

    public partial class LogWindow
    {
        #region Public methods

        public void LoadFile(string fileName, EncodingOptions encodingOptions)
        {
            EnterLoadFileStatus();

            if (fileName != null)
            {
                FileName = fileName;
                EncodingOptions = encodingOptions;

                if (_logFileReader != null)
                {
                    _logFileReader.StopMonitoringAsync();
                    UnRegisterLogFileReaderEvents();
                }

                //
                // isUsingDefaultColumnizer is to enable automatically find the best columnizer.
                // When a new log file is opened, and no Columnizer can be chose by file mask,
                // this flag will enable find a columnizer automatically.
                // Current solution is not elegant.
                // Since the refactory will involving a lot of work, we can plan it in the future.
                // One possible solution is, using raw file stream to read the sample lines to help 
                // the ColumnizerPicker to determine the priority.
                //
                bool isUsingDefaultColumnizer = false;
                if (!LoadPersistenceOptions())
                {
                    if (!IsTempFile)
                    {
                        ILogLineColumnizer columnizer = FindColumnizer();
                        if (columnizer != null)
                        {
                            if (_reloadMemento == null)
                            {
                                columnizer = ColumnizerPicker.CloneColumnizer(columnizer);
                            }
                        }
                        else
                        {
                            isUsingDefaultColumnizer = true;
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
                    _logFileReader = new LogfileReader(fileName, EncodingOptions, IsMultiFile, Preferences.bufferCount, Preferences.linesPerBuffer, _multiFileOptions);
                    _logFileReader.UseNewReader = !Preferences.useLegacyReader;
                }
                catch (LogFileException lfe)
                {
                    _logger.Error(lfe);
                    MessageBox.Show("Cannot load file\n" + lfe.Message, "LogExpert");
                    BeginInvoke(new FunctionWith1BoolParam(Close), true);
                    _isLoadError = true;
                    return;
                }

                if (CurrentColumnizer is ILogLineXmlColumnizer xmlColumnizer)
                {
                    _logFileReader.IsXmlMode = true;
                    _logFileReader.XmlLogConfig = xmlColumnizer.GetXmlLogConfiguration();
                }

                if (_forcedColumnizerForLoading != null)
                {
                    CurrentColumnizer = _forcedColumnizerForLoading;
                }

                if (CurrentColumnizer is IPreProcessColumnizer processColumnizer)
                {
                    _logFileReader.PreProcessColumnizer = processColumnizer;
                }
                else
                {
                    _logFileReader.PreProcessColumnizer = null;
                }
                
                RegisterLogFileReaderEvents();
                _logger.Info($"Loading logfile: {fileName}");
                _logFileReader.startMonitoring();
                
                if (isUsingDefaultColumnizer)
                {
                    if (Preferences.autoPick)
                    {
                        ILogLineColumnizer newColumnizer = ColumnizerPicker.FindBetterColumnizer(FileName, _logFileReader, CurrentColumnizer);

                        if (newColumnizer != null)
                        {
                            _logger.Debug($"Picked new columnizer '{newColumnizer}'");

                            PreSelectColumnizer(newColumnizer);
                        }
                    }
                }
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

            if (_logFileReader != null)
            {
                _logFileReader.stopMonitoring();
                UnRegisterLogFileReaderEvents();
            }

            EncodingOptions = encodingOptions;
            _columnCache = new ColumnCache();
            
            _logFileReader = new LogfileReader(fileNames, EncodingOptions, Preferences.bufferCount, Preferences.linesPerBuffer, _multiFileOptions);
            _logFileReader.UseNewReader = !Preferences.useLegacyReader;
            RegisterLogFileReaderEvents();
            _logFileReader.startMonitoring();
            FileName = fileNames[fileNames.Length - 1];
            _fileNames = fileNames;
            IsMultiFile = true;
            //if (this.isTempFile)
            //  this.Text = this.tempTitleName;
            //else
            //  this.Text = Util.GetNameFromPath(this.FileName);
        }

        public string SavePersistenceData(bool force)
        {
            if (!force)
            {
                if (!Preferences.saveSessions)
                {
                    return null;
                }
            }

            if (IsTempFile || _isLoadError)
            {
                return null;
            }

            try
            {
                PersistenceData persistenceData = GetPersistenceData();

                if (ForcedPersistenceFileName == null)
                {
                    return Persister.SavePersistenceData(FileName, persistenceData, Preferences);
                }

                return Persister.SavePersistenceDataWithFixedName(ForcedPersistenceFileName, persistenceData);
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
            persistenceData.bookmarkList = _bookmarkProvider.BookmarkList;
            persistenceData.rowHeightList = _rowHeightList;
            persistenceData.multiFile = IsMultiFile;
            persistenceData.multiFilePattern = _multiFileOptions.FormatPattern;
            persistenceData.multiFileMaxDays = _multiFileOptions.MaxDayTry;
            persistenceData.currentLine = dataGridView.CurrentCellAddress.Y;
            persistenceData.firstDisplayedLine = dataGridView.FirstDisplayedScrollingRowIndex;
            persistenceData.filterVisible = !splitContainerLogWindow.Panel2Collapsed;
            persistenceData.filterAdvanced = !advancedFilterSplitContainer.Panel1Collapsed;
            persistenceData.filterPosition = splitContainerLogWindow.SplitterDistance;
            persistenceData.followTail = _guiStateArgs.FollowTail;
            persistenceData.fileName = FileName;
            persistenceData.tabName = Text;
            persistenceData.sessionFileName = SessionFileName;
            persistenceData.columnizerName = CurrentColumnizer.GetName();
            persistenceData.lineCount = _logFileReader.LineCount;
            _filterParams.isFilterTail = filterTailCheckBox.Checked; // this option doesnt need a press on 'search'
            
            if (Preferences.saveFilters)
            {
                List<FilterParams> filterList = new List<FilterParams>();
                filterList.Add(_filterParams);
                persistenceData.filterParamsList = filterList;

                foreach (FilterPipe filterPipe in _filterPipeList)
                {
                    FilterTabData data = new FilterTabData();
                    data.persistenceData = filterPipe.OwnLogWindow.GetPersistenceData();
                    data.filterParams = filterPipe.FilterParams;
                    persistenceData.filterTabDataList.Add(data);
                }
            }

            if (_currentHighlightGroup != null)
            {
                persistenceData.highlightGroupName = _currentHighlightGroup.GroupName;
            }

            if (_fileNames != null && IsMultiFile)
            {
                persistenceData.multiFileNames.AddRange(_fileNames);
            }

            //persistenceData.showBookmarkCommentColumn = this.bookmarkWindow.ShowBookmarkCommentColumn;
            persistenceData.filterSaveListVisible = !highlightSplitContainer.Panel2Collapsed;
            persistenceData.encoding = _logFileReader.CurrentEncoding;
            
            return persistenceData;
        }

        public void Close(bool dontAsk)
        {
            Preferences.askForClose = !dontAsk;
            Close();
        }

        public void CloseLogWindow()
        {
            StopTimespreadThread();
            StopTimestampSyncThread();
            StopLogEventWorkerThread();
            _statusLineTrigger.Stop();
            _selectionChangedTrigger.Stop();
            //StopFilterUpdateWorkerThread();
            _shouldCancel = true;

            if (_logFileReader != null)
            {
                UnRegisterLogFileReaderEvents();
                _logFileReader.StopMonitoringAsync();
                //this.logFileReader.DeleteAllContent();
            }

            if (_isLoading)
            {
                _waitingForClose = true;
            }

            if (IsTempFile)
            {
                _logger.Info("Deleting temp file {0}", FileName);

                try
                {
                    File.Delete(FileName);
                }
                catch (IOException e)
                {
                    _logger.Error(e, "Error while deleting temp file {0}: {1}", FileName, e);
                }
            }

            FilterPipe?.CloseAndDisconnect();
            DisconnectFilterPipes();
        }

        public void WaitForLoadingFinished()
        {
            _externaLoadingFinishedEvent.WaitOne();
        }

        public void ForceColumnizer(ILogLineColumnizer columnizer)
        {
            _forcedColumnizer = ColumnizerPicker.CloneColumnizer(columnizer);
            SetColumnizer(_forcedColumnizer);
        }

        public void ForceColumnizerForLoading(ILogLineColumnizer columnizer)
        {
            _forcedColumnizerForLoading = ColumnizerPicker.CloneColumnizer(columnizer);
        }

        public void PreselectColumnizer(string columnizerName)
        {
            ILogLineColumnizer columnizer = ColumnizerPicker.FindColumnizerByName(columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
            PreSelectColumnizer(ColumnizerPicker.CloneColumnizer(columnizer));
        }

        public void ColumnizerConfigChanged()
        {
            SetColumnizerInternal(CurrentColumnizer);
        }

        public void SetColumnizer(ILogLineColumnizer columnizer, DataGridView gridView)
        {
            PaintHelper.SetColumnizer(columnizer,gridView);

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

                    if (columnIndex == 2)
                    {
                        return cols.ColumnValues[cols.ColumnValues.Length - 1];
                    }

                    return Column.EmptyColumn;
                }
            }
            catch
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

            ILogLine line = _logFileReader.GetLogLineWithWait(rowIndex);

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
                    if (_bookmarkProvider.IsBookmarkAtLine(rowIndex))
                    {
                        Rectangle r; // = new Rectangle(e.CellBounds.Left + 2, e.CellBounds.Top + 2, 6, 6);
                        r = e.CellBounds;
                        r.Inflate(-2, -2);
                        Brush brush = new SolidBrush(BookmarkColor);
                        e.Graphics.FillRectangle(brush, r);
                        brush.Dispose();
                        Bookmark bookmark = _bookmarkProvider.GetBookmarkForLine(rowIndex);
                    
                        if (bookmark.Text.Length > 0)
                        {
                            StringFormat format = new StringFormat();
                            format.LineAlignment = StringAlignment.Center;
                            format.Alignment = StringAlignment.Center;
                            Brush brush2 = new SolidBrush(Color.FromArgb(255, 190, 100, 0));
                            Font font = new Font("Courier New", Preferences.fontSize, FontStyle.Bold);
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

        /// <summary>
        /// Returns the first HilightEntry that matches the given line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="noWordMatches"></param>
        /// <returns></returns>
        public HilightEntry FindHighlightEntry(ITextValue line, bool noWordMatches)
        {
            // first check the temp entries
            lock (_tempHighlightEntryListLock)
            {
                foreach (HilightEntry entry in _tempHighlightEntryList)
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

        public IList<HilightMatchEntry> FindHighlightMatches(ITextValue column)
        {
            IList<HilightMatchEntry> resultList = new List<HilightMatchEntry>();
            if (column != null)
            {
                lock (_currentHighlightGroupLock)
                {
                    GetHighlightEntryMatches(column, _currentHighlightGroup.HilightEntryList, resultList);
                }
                lock (_tempHighlightEntryList)
                {
                    GetHighlightEntryMatches(column, _tempHighlightEntryList, resultList);
                }
            }
            return resultList;
        }

        public void FollowTailChanged(bool isChecked, bool byTrigger)
        {
            _guiStateArgs.FollowTail = isChecked;

            if (_guiStateArgs.FollowTail && _logFileReader != null)
            {
                if (dataGridView.RowCount >= _logFileReader.LineCount && _logFileReader.LineCount > 0)
                {
                    dataGridView.FirstDisplayedScrollingRowIndex = _logFileReader.LineCount - 1;
                }
            }
            BeginInvoke(new MethodInvoker(dataGridView.Refresh));
            //this.dataGridView.Refresh();
            _parentLogTabWin.FollowTailChanged(this, isChecked, byTrigger);
            SendGuiStateUpdate();
        }

        public void GotoLine(int line)
        {
            if (line >= 0)
            {
                if (line < dataGridView.RowCount)
                {
                    SelectLine(line, false, true);
                }
                else
                {
                    SelectLine(dataGridView.RowCount - 1, false, true);
                }
                dataGridView.Focus();
            }
        }

        public void TryToTruncate()
        {
            try
            {
                if (LockFinder.CheckIfFileIsLocked(Title))
                {
                    var name = LockFinder.FindLockedProcessName(Title);
                    var status = string.Format("Truncate failed: file is locked by {0}", name);
                    StatusLineText(status);
                    return;
                }

                File.WriteAllText(Title, "");
            }catch(Exception E)
            {
                StatusLineText("Unexpected issue truncating file");
                throw E;
            }
        }

        public void StartSearch()
        {
            _guiStateArgs.MenuEnabled = false;
            GuiStateUpdate(this, _guiStateArgs);
            SearchParams searchParams = _parentLogTabWin.SearchParams;

            if ((searchParams.isForward || searchParams.isFindNext) && !searchParams.isShiftF3Pressed)
            {
                searchParams.currentLine = dataGridView.CurrentCellAddress.Y + 1;
            }
            else
            {
                searchParams.currentLine = dataGridView.CurrentCellAddress.Y - 1;
            }

            _currentSearchParams = searchParams; // remember for async "not found" messages

            _isSearching = true;
            _shouldCancel = false;
            StatusLineText("Searching... Press ESC to cancel.");

            _progressEventArgs.MinValue = 0;
            _progressEventArgs.MaxValue = dataGridView.RowCount;
            _progressEventArgs.Value = 0;
            _progressEventArgs.Visible = true;
            SendProgressBarUpdate();

            SearchFx searchFx = Search;
            searchFx.BeginInvoke(searchParams, SearchComplete, null);

            RemoveAllSearchHighlightEntries();
            AddSearchHitHighlightEntry(searchParams);
        }

        public void SelectLogLine(int line)
        {
            Invoke(new SelectLineFx((line1, triggerSyncCall) => SelectLine(line1, triggerSyncCall, true)), line, true);
        }

        public void SelectAndEnsureVisible(int line, bool triggerSyncCall)
        {
            try
            {
                SelectLine(line, triggerSyncCall, false);

                //if (!this.dataGridView.CurrentRow.Displayed)
                if (line < dataGridView.FirstDisplayedScrollingRowIndex || line > dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(false))
                {
                    dataGridView.FirstDisplayedScrollingRowIndex = line;
                    for (int i = 0; i < 8 && dataGridView.FirstDisplayedScrollingRowIndex > 0 && line < dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(false); ++i)
                    {
                        dataGridView.FirstDisplayedScrollingRowIndex -= 1;
                    }

                    if (line >= dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(false))
                    {
                        dataGridView.FirstDisplayedScrollingRowIndex += 1;
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

        public void OnLogWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (_isErrorShowing)
            {
                RemoveStatusLineError();
            }

            switch (e.KeyCode)
            {
                case Keys.F3 when _parentLogTabWin.SearchParams?.searchText == null || _parentLogTabWin.SearchParams.searchText.Length == 0:
                {
                    return;
                }
                case Keys.F3:
                {
                    _parentLogTabWin.SearchParams.isFindNext = true;
                    _parentLogTabWin.SearchParams.isShiftF3Pressed = (e.Modifiers & Keys.Shift) == Keys.Shift;
                    StartSearch();
                    break;
                }
                case Keys.Escape:
                {
                    if (_isSearching)
                    {
                        _shouldCancel = true;
                    }

                    FireCancelHandlers();
                    RemoveAllSearchHighlightEntries();
                    break;
                }
                case Keys.E when (e.Modifiers & Keys.Control) == Keys.Control:
                {
                    StartEditMode();
                    
                    break;
                }
                case Keys.Down when e.Modifiers == Keys.Alt:
                {
                    int newLine = _logFileReader.GetNextMultiFileLine(dataGridView.CurrentCellAddress.Y);
                    
                    if (newLine != -1)
                    {
                        SelectLine(newLine, false, true);
                    }
                    
                    e.Handled = true;
                    
                    break;
                }
                case Keys.Up when e.Modifiers == Keys.Alt:
                {
                    int newLine = _logFileReader.GetPrevMultiFileLine(dataGridView.CurrentCellAddress.Y);
                    
                    if (newLine != -1)
                    {
                        SelectLine(newLine - 1, false, true);
                    }
                    
                    e.Handled = true;
                    
                    break;
                }
                case Keys.Enter when dataGridView.Focused:
                {
                    ChangeRowHeight(e.Shift);
                    e.Handled = true;
                    
                    break;
                }
                case Keys.Back when dataGridView.Focused:
                {
                    ChangeRowHeight(true);
                    e.Handled = true;
                    
                    break;
                }
                case Keys.PageUp when e.Modifiers == Keys.Alt:
                {
                    SelectPrevHighlightLine();
                    e.Handled = true;
                    
                    break;
                }
                case Keys.PageDown when e.Modifiers == Keys.Alt:
                {
                    SelectNextHighlightLine();
                    e.Handled = true;
                    
                    break;
                }
                case Keys.T when (e.Modifiers & Keys.Control) == Keys.Control && (e.Modifiers & Keys.Shift) == Keys.Shift:
                {
                    FilterToTab();
                    break;
                }
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
                if (_bookmarkProvider.IsBookmarkAtLine(i))
                {
                    Bookmark bookmark = _bookmarkProvider.GetBookmarkForLine(i);
                    if (bookmark.Text.Length > 0)
                    {
                        //BookmarkOverlay overlay = new BookmarkOverlay();
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
                            //int count = i - this.dataGridView.FirstDisplayedScrollingRowIndex;
                            int heightSum = 0;
                            if (dataGridView.FirstDisplayedScrollingRowIndex < i)
                            {
                                for (int rn = dataGridView.FirstDisplayedScrollingRowIndex + 1; rn < i; ++rn)
                                {
                                    //Rectangle rr = this.dataGridView.GetCellDisplayRectangle(0, rn, false);
                                    //heightSum += rr.Height;
                                    heightSum += GetRowHeight(rn);
                                }
                                r.Offset(0, r.Height + heightSum);
                            }
                            else
                            {
                                for (int rn = dataGridView.FirstDisplayedScrollingRowIndex + 1; rn > i; --rn)
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
                            _logger.Debug("AddBookmarkOverlay() r.Location={0}, width={1}, scroll_offset={2}", r.Location.X, r.Width, dataGridView.HorizontalScrollingOffset);
                        }
                        overlay.Position = r.Location - new Size(dataGridView.HorizontalScrollingOffset, 0);
                        overlay.Position += new Size(10, r.Height / 2);
                        dataGridView.AddOverlay(overlay);
                    }
                }
            }
        }

        public void ToggleBookmark()
        {
            DataGridView gridView;
            int lineNum;

            if (filterGridView.Focused)
            {
                gridView = filterGridView;
                if (gridView.CurrentCellAddress == null || gridView.CurrentCellAddress.Y == -1)
                {
                    return;
                }

                lineNum = _filterResultList[gridView.CurrentCellAddress.Y];
            }
            else
            {
                gridView = dataGridView;
                if (gridView.CurrentCellAddress == null || gridView.CurrentCellAddress.Y == -1)
                {
                    return;
                }
                lineNum = dataGridView.CurrentCellAddress.Y;
            }

            ToggleBookmark(lineNum);
        }

        public void ToggleBookmark(int lineNum)
        {
            if (_bookmarkProvider.IsBookmarkAtLine(lineNum))
            {
                Bookmark bookmark = _bookmarkProvider.GetBookmarkForLine(lineNum);

                if (string.IsNullOrEmpty(bookmark.Text) == false)
                {
                    if (DialogResult.No == MessageBox.Show("There's a comment attached to the bookmark. Really remove the bookmark?", "LogExpert", MessageBoxButtons.YesNo))
                    {
                        return;
                    }
                }
                _bookmarkProvider.RemoveBookmarkForLine(lineNum);
            }
            else
            {
                _bookmarkProvider.AddBookmark(new Bookmark(lineNum));
            }
            dataGridView.Refresh();
            filterGridView.Refresh();
            OnBookmarkAdded();
        }

        public void SetBookmarkFromTrigger(int lineNum, string comment)
        {
            lock (_bookmarkLock)
            {
                ILogLine line = _logFileReader.GetLogLine(lineNum);
                if (line == null)
                {
                    return;
                }
                ParamParser paramParser = new ParamParser(comment);
                try
                {
                    comment = paramParser.ReplaceParams(line, lineNum, FileName);
                }
                catch (ArgumentException)
                {
                    // occurs on invalid regex
                }
                if (_bookmarkProvider.IsBookmarkAtLine(lineNum))
                {
                    _bookmarkProvider.RemoveBookmarkForLine(lineNum);
                }
                _bookmarkProvider.AddBookmark(new Bookmark(lineNum, comment));
                OnBookmarkAdded();
            }
        }

        public void JumpNextBookmark()
        {
            if (_bookmarkProvider.Bookmarks.Count > 0)
            {
                if (filterGridView.Focused)
                {
                    int index = FindNextBookmarkIndex(_filterResultList[filterGridView.CurrentCellAddress.Y]);
                    int startIndex = index;
                    bool wrapped = false;
                    while (true)
                    {
                        int lineNum = _bookmarkProvider.Bookmarks[index].LineNum;
                        if (_filterResultList.Contains(lineNum))
                        {
                            int filterLine = _filterResultList.IndexOf(lineNum);
                            filterGridView.Rows[filterLine].Selected = true;
                            filterGridView.CurrentCell = filterGridView.Rows[filterLine].Cells[0];
                            break;
                        }
                        index++;
                        if (index > _bookmarkProvider.Bookmarks.Count - 1)
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
                    int index = FindNextBookmarkIndex(dataGridView.CurrentCellAddress.Y);
                    if (index > _bookmarkProvider.Bookmarks.Count - 1)
                    {
                        index = 0;
                    }

                    int lineNum = _bookmarkProvider.Bookmarks[index].LineNum;
                    SelectLine(lineNum, true, true);
                }
            }
        }

        public void JumpPrevBookmark()
        {
            if (_bookmarkProvider.Bookmarks.Count > 0)
            {
                if (filterGridView.Focused)
                {
                    //int index = this.bookmarkList.BinarySearch(this.filterResultList[this.filterGridView.CurrentCellAddress.Y]);
                    //if (index < 0)
                    //  index = ~index;
                    //index--;
                    int index = FindPrevBookmarkIndex(_filterResultList[filterGridView.CurrentCellAddress.Y]);
                    if (index < 0)
                    {
                        index = _bookmarkProvider.Bookmarks.Count - 1;
                    }
                    int startIndex = index;
                    bool wrapped = false;
                    while (true)
                    {
                        int lineNum = _bookmarkProvider.Bookmarks[index].LineNum;
                        if (_filterResultList.Contains(lineNum))
                        {
                            int filterLine = _filterResultList.IndexOf(lineNum);
                            filterGridView.Rows[filterLine].Selected = true;
                            filterGridView.CurrentCell = filterGridView.Rows[filterLine].Cells[0];
                            break;
                        }
                        index--;
                        if (index < 0)
                        {
                            index = _bookmarkProvider.Bookmarks.Count - 1;
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
                    int index = FindPrevBookmarkIndex(dataGridView.CurrentCellAddress.Y);
                    if (index < 0)
                    {
                        index = _bookmarkProvider.Bookmarks.Count - 1;
                    }

                    int lineNum = _bookmarkProvider.Bookmarks[index].LineNum;
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
                    if (_bookmarkProvider.IsBookmarkAtLine(lineNum) &&
                        _bookmarkProvider.GetBookmarkForLine(lineNum).Text.Length > 0)
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
            _bookmarkProvider.RemoveBookmarksForLines(lineNumList);
            OnBookmarkRemoved();
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
                            int diff = (int) (timeSpan.Ticks / TimeSpan.TicksPerMillisecond);
                            CurrentColumnizer.SetTimeOffset(diff);
                        }
                        catch (Exception)
                        {
                            CurrentColumnizer.SetTimeOffset(0);
                        }
                    }
                    else
                    {
                        CurrentColumnizer.SetTimeOffset(0);
                    }
                    dataGridView.Refresh();
                    filterGridView.Refresh();
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

        public void ToggleFilterPanel()
        {
            splitContainerLogWindow.Panel2Collapsed = !splitContainerLogWindow.Panel2Collapsed;
            if (!splitContainerLogWindow.Panel2Collapsed)
            {
                filterComboBox.Focus();
            }
            else
            {
                dataGridView.Focus();
            }
        }

        public void LogWindowActivated()
        {
            if (_guiStateArgs.FollowTail && !_isDeadFile)
            {
                OnTailFollowed(EventArgs.Empty);
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
                //possible performance issue, see => https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/best-practices-for-scaling-the-windows-forms-datagridview-control?view=netframeworkdesktop-4.8#using-the-selected-cells-rows-and-columns-collections-efficiently
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
            _logFileReader.ChangeEncoding(encoding);
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
            _guiStateArgs.CurrentEncoding = _logFileReader.CurrentEncoding;
        }

        public void Reload()
        {
            SavePersistenceData(false);

            _reloadMemento = new ReloadMemento();
            _reloadMemento.currentLine = dataGridView.CurrentCellAddress.Y;
            _reloadMemento.firstDisplayedLine = dataGridView.FirstDisplayedScrollingRowIndex;
            _forcedColumnizerForLoading = CurrentColumnizer;

            if (_fileNames == null || !IsMultiFile)
            {
                LoadFile(FileName, EncodingOptions);
            }
            else
            {
                LoadFilesAsMulti(_fileNames, EncodingOptions);
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
                NormalFont = new Font(new FontFamily(newPreferences.fontName), newPreferences.fontSize);
                BoldFont = new Font(NormalFont, FontStyle.Bold);
                MonospacedFont = new Font("Courier New", Preferences.fontSize, FontStyle.Bold);

                int lineSpacing = NormalFont.FontFamily.GetLineSpacing(FontStyle.Regular);
                float lineSpacingPixel = NormalFont.Size * lineSpacing / NormalFont.FontFamily.GetEmHeight(FontStyle.Regular);

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
                    //this.FollowTailChanged(this.Preferences.followTail, false);
                }

                _timeSpreadCalc.TimeMode = Preferences.timeSpreadTimeMode;
                timeSpreadingControl.ForeColor = Preferences.timeSpreadColor;
                timeSpreadingControl.ReverseAlpha = Preferences.reverseAlpha;
                if (CurrentColumnizer.IsTimeshiftImplemented())
                {
                    timeSpreadingControl.Invoke(new MethodInvoker(timeSpreadingControl.Refresh));
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

                if(isLoadTime)
                {
                    AutoResizeFilterBox();
                }
            }
        }

        public bool ScrollToTimestamp(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ScrollToTimestampFx(ScrollToTimestampWorker), timestamp, roundToSeconds, triggerSyncCall);
                return true;
            }

            return ScrollToTimestampWorker(timestamp, roundToSeconds, triggerSyncCall);
        }

        public bool ScrollToTimestampWorker(DateTime timestamp, bool roundToSeconds, bool triggerSyncCall)
        {
            bool hasScrolled = false;
            if (!CurrentColumnizer.IsTimeshiftImplemented() || dataGridView.RowCount == 0)
            {
                return false;
            }

            //this.Cursor = Cursors.WaitCursor;
            int currentLine = dataGridView.CurrentCellAddress.Y;
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
            //this.Cursor = Cursors.Default;
            return hasScrolled;
        }

        public int FindTimestampLine(int lineNum, DateTime timestamp, bool roundToSeconds)
        {
            int foundLine =
                FindTimestampLine_Internal(lineNum, 0, dataGridView.RowCount - 1, timestamp, roundToSeconds);
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

                foundLine++;
                GetTimestampForLineForward(ref foundLine, roundToSeconds); // fwd to next valid timestamp
                return foundLine;
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
            lock (_currentColumnizerLock)
            {
                if (!CurrentColumnizer.IsTimeshiftImplemented())
                {
                    return DateTime.MinValue;
                }
                _logger.Debug("GetTimestampForLine({0}) enter", lineNum);
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
                        ILogLine logLine = _logFileReader.GetLogLine(lineNum);
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
                        ILogLine logLine = _logFileReader.GetLogLine(lineNum);
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

        public ILogLine GetCurrentLine()
        {
            if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.Index != -1)
            {
                return _logFileReader.GetLogLine(dataGridView.CurrentRow.Index);
            }
            return null;
        }

        public ILogLine GetLine(int lineNum)
        {
            if (lineNum < 0 || _logFileReader == null || lineNum >= _logFileReader.LineCount)
            {
                return null;
            }
            return _logFileReader.GetLogLine(lineNum);
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
            return _logFileReader.GetRealLineNumForVirtualLineNum(lineNum);
        }

        public ILogFileInfo GetCurrentFileInfo()
        {
            if (dataGridView.CurrentRow != null && dataGridView.CurrentRow.Index != -1)
            {
                return _logFileReader.GetLogFileInfoForLine(dataGridView.CurrentRow.Index);
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
            return _logFileReader.GetLogFileNameForLine(lineNum);
        }

        // =============== end of bookmark stuff ===================================

        public void ShowLineColumn(bool show)
        {
            dataGridView.Columns[1].Visible = show;
            filterGridView.Columns[1].Visible = show;
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
                if (dataGridView.CurrentCellAddress.Y != -1)
                {
                    patternArgs.startLine = dataGridView.CurrentCellAddress.Y;
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
            dlg.FileName = Path.GetFileNameWithoutExtension(FileName);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    BookmarkExporter.ExportBookmarkList(_bookmarkProvider.BookmarkList, FileName,
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
            dlg.FileName = Path.GetFileNameWithoutExtension(FileName);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // add to the existing bookmarks
                    SortedList<int, Bookmark> newBookmarks = new SortedList<int, Bookmark>();
                    BookmarkExporter.ImportBookmarkList(FileName, dlg.FileName, newBookmarks);

                    // Add (or replace) to existing bookmark list
                    bool bookmarkAdded = false;
                    foreach (Bookmark b in newBookmarks.Values)
                    {
                        if (!_bookmarkProvider.BookmarkList.ContainsKey(b.LineNum))
                        {
                            _bookmarkProvider.BookmarkList.Add(b.LineNum, b);
                            bookmarkAdded = true; // refresh the list only once at the end
                        }
                        else
                        {
                            Bookmark existingBookmark = _bookmarkProvider.BookmarkList[b.LineNum];
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
                    dataGridView.Refresh();
                    filterGridView.Refresh();
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
            return rangeCheckBox.Checked ||
                   fuzzyKnobControl.Value > 0 ||
                   filterKnobBackSpread.Value > 0 ||
                   filterKnobForeSpread.Value > 0 ||
                   invertFilterCheckBox.Checked ||
                   columnRestrictCheckBox.Checked;
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

        public void SetCurrentHighlightGroup(string groupName)
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
            _logger.Info("Syncing window for {0} to {1}", Util.GetNameFromPath(FileName), Util.GetNameFromPath(master.FileName));
            lock (_timeSyncListLock)
            {
                if (IsTimeSynced && master.TimeSyncList != TimeSyncList)
                    // already synced but master has different sync list
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
                    _logger.Info("De-Syncing window for {0}", Util.GetNameFromPath(FileName));
                    TimeSyncList.WindowRemoved -= OnTimeSyncListWindowRemoved;
                    TimeSyncList.RemoveWindow(this);
                    TimeSyncList = null;
                }
            }
            OnSyncModeChanged();
        }

        public void RefreshLogView()
        {
            RefreshAllGrids();
        }

        #endregion
    }
}