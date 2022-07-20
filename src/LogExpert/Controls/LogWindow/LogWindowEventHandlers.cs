#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LogExpert.Classes;
using LogExpert.Classes.Filter;
using LogExpert.Classes.Highlight;
using LogExpert.Classes.ILogLineColumnizerCallback;
using LogExpert.Config;
using LogExpert.Dialogs;
using LogExpert.Entities;
using LogExpert.Entities.EventArgs;
using LogExpert.Extensions;
using LogExpert.Interface;

#endregion

namespace LogExpert.Controls.LogWindow
{
    public partial class LogWindow
    {
        private void AutoResizeFilterBox()
        {
            filterSplitContainer.SplitterDistance = filterComboBox.Left + filterComboBox.GetMaxTextWidth();
        }

        #region Events handler

        protected void OnProgressBarUpdate(ProgressEventArgs e)
        {
            ProgressBarEventHandler handler = ProgressBarUpdate;
            handler?.Invoke(this, e);
        }

        protected void OnStatusLine(StatusLineEventArgs e)
        {
            StatusLineEventHandler handler = StatusLineEvent;
            handler?.Invoke(this, e);
        }

        protected void OnGuiState(GuiStateArgs e)
        {
            GuiStateEventHandler handler = GuiStateUpdate;
            handler?.Invoke(this, e);
        }

        protected void OnTailFollowed(EventArgs e)
        {
            TailFollowed?.Invoke(this, e);
        }

        protected void OnFileNotFound(EventArgs e)
        {
            FileNotFound?.Invoke(this, e);
        }

        protected void OnFileRespawned(EventArgs e)
        {
            FileRespawned?.Invoke(this, e);
        }

        protected void OnFilterListChanged(LogWindow source)
        {
            FilterListChanged?.Invoke(this, new FilterListChangedEventArgs(source));
        }

        protected void OnCurrentHighlightListChanged()
        {
            CurrentHighlightGroupChanged?.Invoke(this, new CurrentHighlightGroupChangedEventArgs(this, _currentHighlightGroup));
        }

        protected void OnBookmarkAdded()
        {
            BookmarkAdded?.Invoke(this, EventArgs.Empty);
        }

        protected void OnBookmarkRemoved()
        {
            BookmarkRemoved?.Invoke(this, EventArgs.Empty);
        }

        protected void OnBookmarkTextChanged(Bookmark bookmark)
        {
            BookmarkTextChanged?.Invoke(this, new BookmarkEventArgs(bookmark));
        }

        protected void OnColumnizerChanged(ILogLineColumnizer columnizer)
        {
            ColumnizerChanged?.Invoke(this, new ColumnizerEventArgs(columnizer));
        }

        protected void OnRegisterCancelHandler(BackgroundProcessCancelHandler handler)
        {
            lock (_cancelHandlerList)
            {
                _cancelHandlerList.Add(handler);
            }
        }

        protected void OnDeRegisterCancelHandler(BackgroundProcessCancelHandler handler)
        {
            lock (_cancelHandlerList)
            {
                _cancelHandlerList.Remove(handler);
            }
        }

        private void OnLogWindowLoad(object sender, EventArgs e)
        {
            PreferencesChanged(_parentLogTabWin.Preferences, true, SettingsFlags.GuiOrColors);
        }

        private void OnLogWindowDisposed(object sender, EventArgs e)
        {
            _waitingForClose = true;
            _parentLogTabWin.HighlightSettingsChanged -= OnParentHighlightSettingsChanged;
            _logFileReader?.DeleteAllContent();

            FreeFromTimeSync();
        }

        private void OnLogFileReaderLoadingStarted(object sender, LoadFileEventArgs e)
        {
            Invoke(new LoadingStartedFx(LoadingStarted), e);
        }

        private void OnLogFileReaderFinishedLoading(object sender, EventArgs e)
        {
            //Thread.CurrentThread.Name = "FinishedLoading event thread";
            _logger.Info("Finished loading.");
            _isLoading = false;
            _isDeadFile = false;
            if (!_waitingForClose)
            {
                Invoke(new MethodInvoker(LoadingFinished));
                Invoke(new MethodInvoker(LoadPersistenceData));
                Invoke(new MethodInvoker(SetGuiAfterLoading));
                _loadingFinishedEvent.Set();
                _externaLoadingFinishedEvent.Set();
                _timeSpreadCalc.SetLineCount(_logFileReader.LineCount);

                if (_reloadMemento != null) Invoke(new PositionAfterReloadFx(PositionAfterReload), _reloadMemento);

                if (filterTailCheckBox.Checked)
                {
                    _logger.Info("Refreshing filter view because of reload.");
                    Invoke(new MethodInvoker(FilterSearch)); // call on proper thread
                }

                HandleChangedFilterList();
            }

            _reloadMemento = null;
        }

        private void OnLogFileReaderFileNotFound(object sender, EventArgs e)
        {
            if (!IsDisposed && !Disposing)
            {
                _logger.Info("Handling file not found event.");
                _isDeadFile = true;
                BeginInvoke(new MethodInvoker(LogfileDead));
            }
        }

        private void OnLogFileReaderRespawned(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(LogfileRespawned));
        }

        private void OnLogWindowClosing(object sender, CancelEventArgs e)
        {
            if (Preferences.askForClose)
                if (MessageBox.Show("Sure to close?", "LogExpert", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

            SavePersistenceData(false);
            CloseLogWindow();
        }

        private void OnDataGridViewColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            e.Handled = true;
            AutoResizeColumns(dataGridView);
        }

        /**
       * Event handler for the Load event from LogfileReader
       */
        private void OnLogFileReaderLoadFile(object sender, LoadFileEventArgs e)
        {
            if (e.NewFile)
            {
                _logger.Info("File created anew.");

                // File was new created (e.g. rollover)
                _isDeadFile = false;
                UnRegisterLogFileReaderEvents();
                dataGridView.CurrentCellChanged -= OnDataGridViewCurrentCellChanged;
                MethodInvoker invoker = ReloadNewFile;
                BeginInvoke(invoker);
                //Thread loadThread = new Thread(new ThreadStart(ReloadNewFile));
                //loadThread.Start();
                _logger.Debug("Reloading invoked.");
                return;
            }

            if (!_isLoading) return;

            UpdateProgressCallback callback = UpdateProgress;
            BeginInvoke(callback, e);
        }

        private void OnFileSizeChanged(object sender, LogEventArgs e)
        {
            //OnFileSizeChanged(e);  // now done in UpdateGrid()
            _logger.Info("Got FileSizeChanged event. prevLines:{0}, curr lines: {1}", e.PrevLineCount, e.LineCount);

            // - now done in the thread that works on the event args list
            //if (e.IsRollover)
            //{
            //  ShiftBookmarks(e.RolloverOffset);
            //  ShiftFilterPipes(e.RolloverOffset);
            //}

            //UpdateGridCallback callback = new UpdateGridCallback(UpdateGrid);
            //this.BeginInvoke(callback, new object[] { e });
            lock (_logEventArgsList)
            {
                _logEventArgsList.Add(e);
                _logEventArgsEvent.Set();
            }
        }

        private void OnDataGridViewCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            int startCount = CurrentColumnizer?.GetColumnCount() ?? 0;

            e.Value = GetCellValue(e.RowIndex, e.ColumnIndex);

            // The new column could be find dynamically.
            // Only support add new columns for now.
            // TODO: Support reload all columns?
            if (CurrentColumnizer != null && CurrentColumnizer.GetColumnCount() > startCount)
                for (int i = startCount; i < CurrentColumnizer.GetColumnCount(); i++)
                {
                    var colName = CurrentColumnizer.GetColumnNames()[i];
                    dataGridView.Columns.Add(PaintHelper.CreateTitleColumn(colName));
                }
        }

        private void OnDataGridViewCellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            if (!CurrentColumnizer.IsTimeshiftImplemented()) return;

            ILogLine line = _logFileReader.GetLogLine(e.RowIndex);
            int offset = CurrentColumnizer.GetTimeOffset();
            CurrentColumnizer.SetTimeOffset(0);
            ColumnizerCallbackObject.LineNum = e.RowIndex;
            IColumnizedLogLine cols = CurrentColumnizer.SplitLine(ColumnizerCallbackObject, line);
            CurrentColumnizer.SetTimeOffset(offset);
            if (cols.ColumnValues.Length <= e.ColumnIndex - 2) return;

            string oldValue = cols.ColumnValues[e.ColumnIndex - 2].FullValue;
            string newValue = (string)e.Value;
            //string oldValue = (string) this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            CurrentColumnizer.PushValue(ColumnizerCallbackObject, e.ColumnIndex - 2, newValue, oldValue);
            dataGridView.Refresh();
            TimeSpan timeSpan = new TimeSpan(CurrentColumnizer.GetTimeOffset() * TimeSpan.TicksPerMillisecond);
            string span = timeSpan.ToString();
            int index = span.LastIndexOf('.');
            if (index > 0) span = span.Substring(0, index + 4);

            SetTimeshiftValue(span);
            SendGuiStateUpdate();
        }

        private void OnDataGridViewRowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
        {
            e.Height = GetRowHeight(e.RowIndex);
        }

        private void OnDataGridViewCurrentCellChanged(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                _statusEventArgs.CurrentLineNum = dataGridView.CurrentRow.Index + 1;
                SendStatusLineUpdate();
                if (syncFilterCheckBox.Checked) SyncFilterGridPos();

                if (CurrentColumnizer.IsTimeshiftImplemented() && Preferences.timestampControl) SyncTimestampDisplay();

                //MethodInvoker invoker = new MethodInvoker(DisplayCurrentFileOnStatusline);
                //invoker.BeginInvoke(null, null);
            }
        }

        private void OnDataGridViewCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            StatusLineText(string.Empty);
        }

        private void OnEditControlKeyUp(object sender, KeyEventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
        }

        private void OnEditControlKeyPress(object sender, KeyPressEventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
        }

        private void OnEditControlClick(object sender, EventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
        }

        private void OnEditControlKeyDown(object sender, KeyEventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
        }

        private void OnDataGridViewPaint(object sender, PaintEventArgs e)
        {
            if (ShowBookmarkBubbles)
            {
                AddBookmarkOverlays();
            }
        }

        // ======================================================================================
        // Filter Grid stuff
        // ======================================================================================

        private void OnFilterSearchButtonClick(object sender, EventArgs e)
        {
            FilterSearch();
        }

        private void OnFilterGridViewCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView gridView = (DataGridView)sender;

            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _filterResultList.Count <= e.RowIndex)
            {
                e.Handled = false;
                return;
            }

            int lineNum = _filterResultList[e.RowIndex];
            ILogLine line = _logFileReader.GetLogLineWithWait(lineNum);
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
                        if (entry != null) bgColor = entry.BackgroundColor;
                    }
                    else
                    {
                        if (entry != null) bgColor = entry.BackgroundColor;
                    }

                    e.CellStyle.BackColor = bgColor;
                    e.PaintBackground(e.ClipBounds, false);
                }

                if (DebugOptions.disableWordHighlight)
                    e.PaintContent(e.CellBounds);
                else
                    PaintCell(e, filterGridView, false, entry);

                if (e.ColumnIndex == 0)
                    if (_bookmarkProvider.IsBookmarkAtLine(lineNum))
                    {
                        Rectangle r = new Rectangle(e.CellBounds.Left + 2, e.CellBounds.Top + 2, 6, 6);
                        r = e.CellBounds;
                        r.Inflate(-2, -2);
                        Brush brush = new SolidBrush(BookmarkColor);
                        e.Graphics.FillRectangle(brush, r);
                        brush.Dispose();
                        Bookmark bookmark = _bookmarkProvider.GetBookmarkForLine(lineNum);
                        if (bookmark.Text.Length > 0)
                        {
                            StringFormat format = new StringFormat();
                            format.LineAlignment = StringAlignment.Center;
                            format.Alignment = StringAlignment.Center;
                            Brush brush2 = new SolidBrush(Color.FromArgb(255, 190, 100, 0));
                            Font font = new Font("Verdana", Preferences.fontSize, FontStyle.Bold);
                            e.Graphics.DrawString("!", font, brush2, new RectangleF(r.Left, r.Top, r.Width, r.Height),
                                format);
                            font.Dispose();
                            brush2.Dispose();
                        }
                    }

                e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                e.Handled = true;
            }
        }

        private void OnFilterGridViewCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _filterResultList.Count <= e.RowIndex)
            {
                e.Value = "";
                return;
            }

            int lineNum = _filterResultList[e.RowIndex];
            e.Value = GetCellValue(lineNum, e.ColumnIndex);
        }

        private void OnFilterGridViewRowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
        {
            e.Height = _lineHeight;
        }

        private void OnFilterComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) FilterSearch();
        }

        private void OnFilterGridViewColumnDividerDoubleClick(object sender,
            DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            e.Handled = true;
            AutoResizeColumnsFx fx = AutoResizeColumns;
            BeginInvoke(fx, filterGridView);
        }

        private void OnFilterGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
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

        private void OnRangeCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            filterRangeComboBox.Enabled = rangeCheckBox.Checked;
            CheckForFilterDirty();
        }

        private void OnDataGridViewScroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                if (dataGridView.DisplayedRowCount(false) + dataGridView.FirstDisplayedScrollingRowIndex >= dataGridView.RowCount)
                {
                    //this.guiStateArgs.FollowTail = true;
                    if (!_guiStateArgs.FollowTail) FollowTailChanged(true, false);

                    OnTailFollowed(EventArgs.Empty);
                }
                else
                {
                    //this.guiStateArgs.FollowTail = false;
                    if (_guiStateArgs.FollowTail) FollowTailChanged(false, false);
                }

                SendGuiStateUpdate();
            }
        }

        private void OnFilterGridViewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                {
                    if (filterGridView.CurrentCellAddress.Y >= 0 && filterGridView.CurrentCellAddress.Y < _filterResultList.Count)
                    {
                        int lineNum = _filterResultList[filterGridView.CurrentCellAddress.Y];
                        SelectLine(lineNum, false, true);
                        e.Handled = true;
                    }

                    break;
                }
                case Keys.Tab when e.Modifiers == Keys.None:
                    dataGridView.Focus();
                    e.Handled = true;
                    break;
            }
        }

        private void OnDataGridViewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Tab when e.Modifiers == Keys.None:
                {
                    filterGridView.Focus();
                    e.Handled = true;
                    break;
                }
            }

            //switch (e.KeyCode)
            //{
            //    case Keys.Tab when e.Modifiers == Keys.Control:
            //        //this.parentLogTabWin.SwitchTab(e.Shift);
            //        break;
            //}

            _shouldCallTimeSync = true;
        }

        private void OnDataGridViewPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && e.Control) e.IsInputKey = true;
        }

        private void OnDataGridViewCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView.CurrentCell != null) dataGridView.BeginEdit(false);
        }

        private void OnSyncFilterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (syncFilterCheckBox.Checked) SyncFilterGridPos();
        }

        private void OnDataGridViewLeave(object sender, EventArgs e)
        {
            InvalidateCurrentRow(dataGridView);
        }

        private void OnDataGridViewEnter(object sender, EventArgs e)
        {
            InvalidateCurrentRow(dataGridView);
        }

        private void OnFilterGridViewEnter(object sender, EventArgs e)
        {
            InvalidateCurrentRow(filterGridView);
        }

        private void OnFilterGridViewLeave(object sender, EventArgs e)
        {
            InvalidateCurrentRow(filterGridView);
        }

        private void OnDataGridViewResize(object sender, EventArgs e)
        {
            if (_logFileReader != null && dataGridView.RowCount > 0 && _guiStateArgs.FollowTail)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.RowCount - 1;
            }
        }

        private void OnDataGridViewSelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectionDisplay();
        }

        private void OnSelectionChangedTriggerSignal(object sender, EventArgs e)
        {
            int selCount = 0;
            try
            {
                _logger.Debug("Selection changed trigger");
                selCount = dataGridView.SelectedRows.Count;
                if (selCount > 1)
                {
                    StatusLineText(selCount + " selected lines");
                }
                else
                {
                    if (IsMultiFile)
                    {
                        MethodInvoker invoker = DisplayCurrentFileOnStatusline;
                        invoker.BeginInvoke(null, null);
                    }
                    else
                    {
                        StatusLineText("");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in selectionChangedTrigger_Signal selcount {0}", selCount);
            }
        }

        private void OnFilterKnobControlValueChanged(object sender, EventArgs e)
        {
            CheckForFilterDirty();
        }

        private void OnFilterToTabButtonClick(object sender, EventArgs e)
        {
            FilterToTab();
        }

        private void OnPipeDisconnected(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(FilterPipe))
            {
                lock (_filterPipeList)
                {
                    _filterPipeList.Remove((FilterPipe)sender);
                    if (_filterPipeList.Count == 0)
                        // reset naming counter to 0 if no more open filter tabs for this source window
                    {
                        _filterPipeNameCounter = 0;
                    }
                }
            }
        }

        private void OnAdvancedButtonClick(object sender, EventArgs e)
        {
            _showAdvanced = !_showAdvanced;
            ShowAdvancedFilterPanel(_showAdvanced);
        }

        private void OnFilterSplitContainerMouseDown(object sender, MouseEventArgs e)
        {
            ((SplitContainer)sender).IsSplitterFixed = true;
        }

        private void OnFilterSplitContainerMouseUp(object sender, MouseEventArgs e)
        {
            ((SplitContainer)sender).IsSplitterFixed = false;
        }

        private void OnFilterSplitContainerMouseMove(object sender, MouseEventArgs e)
        {
            SplitContainer splitContainer = (SplitContainer)sender;
            if (splitContainer.IsSplitterFixed)
            {
                if (e.Button.Equals(MouseButtons.Left))
                {
                    if (splitContainer.Orientation.Equals(Orientation.Vertical))
                    {
                        if (e.X > 0 && e.X < splitContainer.Width)
                        {
                            splitContainer.SplitterDistance = e.X;
                            splitContainer.Refresh();
                        }
                    }
                    else
                    {
                        if (e.Y > 0 && e.Y < splitContainer.Height)
                        {
                            splitContainer.SplitterDistance = e.Y;
                            splitContainer.Refresh();
                        }
                    }
                }
                else
                {
                    splitContainer.IsSplitterFixed = false;
                }
            }
        }

        private void OnFilterSplitContainerMouseDoubleClick(object sender, MouseEventArgs e)
        {
            AutoResizeFilterBox();
        }

        #region Context Menu

        private void OnDataGridContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            int lineNum = -1;
            if (dataGridView.CurrentRow != null) lineNum = dataGridView.CurrentRow.Index;

            if (lineNum == -1) return;

            int refLineNum = lineNum;

            copyToTabToolStripMenuItem.Enabled = dataGridView.SelectedCells.Count > 0;
            scrollAllTabsToTimestampToolStripMenuItem.Enabled = CurrentColumnizer.IsTimeshiftImplemented()
                                                                &&
                                                                GetTimestampForLine(ref refLineNum, false) !=
                                                                DateTime.MinValue;
            locateLineInOriginalFileToolStripMenuItem.Enabled = IsTempFile &&
                                                                FilterPipe != null &&
                                                                FilterPipe.GetOriginalLineNum(lineNum) != -1;
            markEditModeToolStripMenuItem.Enabled = !dataGridView.CurrentCell.ReadOnly;

            // Remove all "old" plugin entries
            int index = dataGridContextMenuStrip.Items.IndexOf(pluginSeparator);
            if (index > 0)
                for (int i = index + 1; i < dataGridContextMenuStrip.Items.Count;)
                    dataGridContextMenuStrip.Items.RemoveAt(i);

            // Add plugin entries
            bool isAdded = false;
            if (PluginRegistry.GetInstance().RegisteredContextMenuPlugins.Count > 0)
            {
                //string line = this.logFileReader.GetLogLine(lineNum);
                IList<int> lines = GetSelectedContent();
                foreach (IContextMenuEntry entry in PluginRegistry.GetInstance().RegisteredContextMenuPlugins)
                {
                    LogExpertCallback callback = new LogExpertCallback(this);
                    ContextMenuPluginEventArgs evArgs = new ContextMenuPluginEventArgs(entry, lines, CurrentColumnizer, callback);
                    EventHandler ev = OnHandlePluginContextMenu;
                    //MenuItem item = this.dataGridView.ContextMenu.MenuItems.Add(entry.GetMenuText(line, this.CurrentColumnizer, callback), ev);
                    string menuText = entry.GetMenuText(lines, CurrentColumnizer, callback);
                    if (menuText != null)
                    {
                        bool disabled = menuText.StartsWith("_");
                        if (disabled) menuText = menuText.Substring(1);

                        ToolStripItem item = dataGridContextMenuStrip.Items.Add(menuText, null, ev);
                        item.Tag = evArgs;
                        item.Enabled = !disabled;
                        isAdded = true;
                    }
                }
            }

            pluginSeparator.Visible = isAdded;

            // enable/disable Temp Highlight item
            tempHighlightsToolStripMenuItem.Enabled = _tempHighlightEntryList.Count > 0;

            markCurrentFilterRangeToolStripMenuItem.Enabled = string.IsNullOrEmpty(filterRangeComboBox.Text) == false;

            if (CurrentColumnizer.IsTimeshiftImplemented())
            {
                IList<WindowFileEntry> list = _parentLogTabWin.GetListOfOpenFiles();
                syncTimestampsToToolStripMenuItem.Enabled = true;
                syncTimestampsToToolStripMenuItem.DropDownItems.Clear();
                EventHandler ev = OnHandleSyncContextMenu;
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

            freeThisWindowFromTimeSyncToolStripMenuItem.Enabled = TimeSyncList != null &&
                                                                  TimeSyncList.Count > 1;
        }

        private void OnHandlePluginContextMenu(object sender, EventArgs args)
        {
            if (sender is ToolStripItem item)
            {
                ContextMenuPluginEventArgs menuArgs = item.Tag as ContextMenuPluginEventArgs;
                menuArgs.Entry.MenuSelected(menuArgs.LogLines, menuArgs.Columnizer, menuArgs.Callback);
            }
        }

        private void OnHandleSyncContextMenu(object sender, EventArgs args)
        {
            if (sender is ToolStripItem item)
            {
                WindowFileEntry entry = item.Tag as WindowFileEntry;

                if (TimeSyncList != null && TimeSyncList.Contains(entry.LogWindow))
                {
                    FreeSlaveFromTimesync(entry.LogWindow);
                }
                else
                    //AddSlaveToTimesync(entry.LogWindow);
                {
                    AddOtherWindowToTimesync(entry.LogWindow);
                }
            }
        }

        private void OnCopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            CopyMarkedLinesToClipboard();
        }

        private void OnCopyToTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            CopyMarkedLinesToTab();
        }

        private void OnScrollAllTabsToTimestampToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (CurrentColumnizer.IsTimeshiftImplemented())
            {
                int currentLine = dataGridView.CurrentCellAddress.Y;
                if (currentLine > 0 && currentLine < dataGridView.RowCount)
                {
                    int lineNum = currentLine;
                    DateTime timeStamp = GetTimestampForLine(ref lineNum, false);
                    if (timeStamp.Equals(DateTime.MinValue)) // means: invalid
                    {
                        return;
                    }

                    _parentLogTabWin.ScrollAllTabsToTimestamp(timeStamp, this);
                }
            }
        }

        private void OnLocateLineInOriginalFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null && FilterPipe != null)
            {
                int lineNum = FilterPipe.GetOriginalLineNum(dataGridView.CurrentRow.Index);
                if (lineNum != -1)
                {
                    FilterPipe.LogWindow.SelectLine(lineNum, false, true);
                    _parentLogTabWin.SelectTab(FilterPipe.LogWindow);
                }
            }
        }

        private void OnToggleBoomarkToolStripMenuItemClick(object sender, EventArgs e)
        {
            ToggleBookmark();
        }

        private void OnMarkEditModeToolStripMenuItemClick(object sender, EventArgs e)
        {
            StartEditMode();
        }

        private void OnLogWindowSizeChanged(object sender, EventArgs e)
        {
            //AdjustMinimumGridWith();
            AdjustHighlightSplitterWidth();
        }

        #region BookMarkList

        private void OnColumnRestrictCheckBoxCheckedChanged(object sender, EventArgs e)
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

        private void OncolumnButtonClick(object sender, EventArgs e)
        {
            _filterParams.currentColumnizer = _currentColumnizer;
            FilterColumnChooser chooser = new FilterColumnChooser(_filterParams);
            if (chooser.ShowDialog() == DialogResult.OK)
            {
                columnNamesLabel.Text = CalculateColumnNames(_filterParams);

                //CheckForFilterDirty(); //!!!GBro: Indicate to redo the search if search columns were changed
                filterSearchButton.Image = _searchButtonImage;
                saveFilterButton.Enabled = false;
            }
        }

        #endregion

        #region Column Header Context Menu

        private void OnDataGridViewCellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView.RowCount && !dataGridView.Rows[e.RowIndex].Selected)
            {
                SelectLine(e.RowIndex, false, true);
            }
            else if (e.RowIndex < 0)
            {
                e.ContextMenuStrip = columnContextMenuStrip;
            }

            if (e.ContextMenuStrip == columnContextMenuStrip)
            {
                _selectedCol = e.ColumnIndex;
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

        private void OnFilterGridViewCellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.ContextMenuStrip == columnContextMenuStrip) _selectedCol = e.ColumnIndex;
        }

        private void OnColumnContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            Control ctl = columnContextMenuStrip.SourceControl;
            DataGridView gridView = ctl as DataGridView;
            bool frozen = false;
            if (_freezeStateMap.ContainsKey(ctl)) frozen = _freezeStateMap[ctl];

            freezeLeftColumnsUntilHereToolStripMenuItem.Checked = frozen;

            if (frozen)
            {
                freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Frozen";
            }
            else
            {
                if (ctl is DataGridView)
                {
                    freezeLeftColumnsUntilHereToolStripMenuItem.Text = $"Freeze left columns until here ({gridView.Columns[_selectedCol].HeaderText})";
                }
            }


            DataGridViewColumn col = gridView.Columns[_selectedCol];
            moveLeftToolStripMenuItem.Enabled = col != null && col.DisplayIndex > 0;
            moveRightToolStripMenuItem.Enabled = col != null && col.DisplayIndex < gridView.Columns.Count - 1;

            if (gridView.Columns.Count - 1 > _selectedCol)
            {
                //        DataGridViewColumn colRight = gridView.Columns[this.selectedCol + 1];
                DataGridViewColumn colRight = gridView.Columns.GetNextColumn(col, DataGridViewElementStates.None, DataGridViewElementStates.None);
                moveRightToolStripMenuItem.Enabled = colRight != null && colRight.Frozen == col.Frozen;
            }

            if (_selectedCol > 0)
            {
                //DataGridViewColumn colLeft = gridView.Columns[this.selectedCol - 1];
                DataGridViewColumn colLeft = gridView.Columns.GetPreviousColumn(col, DataGridViewElementStates.None, DataGridViewElementStates.None);

                moveLeftToolStripMenuItem.Enabled = colLeft != null && colLeft.Frozen == col.Frozen;
            }

            DataGridViewColumn colLast = gridView.Columns[gridView.Columns.Count - 1];
            moveToLastColumnToolStripMenuItem.Enabled = colLast != null && colLast.Frozen == col.Frozen;

            // Fill context menu with column names 
            //
            EventHandler ev = OnHandleColumnItemContextMenu;
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

        private void OnHandleColumnItemContextMenu(object sender, EventArgs args)
        {
            if (sender is ToolStripItem item)
            {
                DataGridViewColumn column = item.Tag as DataGridViewColumn;
                column.Visible = true;
                column.DataGridView.FirstDisplayedScrollingColumnIndex = column.Index;
            }
        }

        private void OnFreezeLeftColumnsUntilHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            Control ctl = columnContextMenuStrip.SourceControl;
            bool frozen = false;

            if (_freezeStateMap.ContainsKey(ctl))
            {
                frozen = _freezeStateMap[ctl];
            }

            frozen = !frozen;
            _freezeStateMap[ctl] = frozen;

            DataGridViewColumn senderCol = sender as DataGridViewColumn;
            if (ctl is DataGridView gridView)
            {
                ApplyFrozenState(gridView);
            }
        }

        private void OnMoveToLastColumnToolStripMenuItemClick(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[_selectedCol];
            if (col != null) col.DisplayIndex = gridView.Columns.Count - 1;
        }

        private void OnMoveLeftToolStripMenuItemClick(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[_selectedCol];
            if (col != null && col.DisplayIndex > 0)
            {
                col.DisplayIndex -= 1;
            }
        }

        private void OnMoveRightToolStripMenuItemClick(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[_selectedCol];
            if (col != null && col.DisplayIndex < gridView.Columns.Count - 1) col.DisplayIndex = col.DisplayIndex + 1;
        }

        private void OnHideColumnToolStripMenuItemClick(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[_selectedCol];
            col.Visible = false;
        }

        private void OnRestoreColumnsToolStripMenuItemClick(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            foreach (DataGridViewColumn col in gridView.Columns) col.Visible = true;
        }

        private void OnTimeSpreadingControlLineSelected(object sender, SelectLineEventArgs e)
        {
            SelectLine(e.Line, false, true);
        }

        private void OnBookmarkCommentToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddBookmarkAndEditComment();
        }

        private void OnHighlightSelectionInLogFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
            {
                HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow, false, true, false, false, false, false, null, false);
                lock (_tempHighlightEntryListLock)
                {
                    _tempHighlightEntryList.Add(he);
                }

                dataGridView.CancelEdit();
                dataGridView.EndEdit();
                RefreshAllGrids();
            }
        }

        private void OnHighlightSelectionInLogFilewordModeToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
            {
                HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow, false, true, false, false, false, false, null, true);
                lock (_tempHighlightEntryListLock)
                {
                    _tempHighlightEntryList.Add(he);
                }

                dataGridView.CancelEdit();
                dataGridView.EndEdit();
                RefreshAllGrids();
            }
        }

        private void OnEditModeCopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
            {
                if (Util.IsNull(ctl.SelectedText) == false)
                {
                    Clipboard.SetText(ctl.SelectedText);
                }
            }
        }

        private void OnRemoveAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            RemoveTempHighlights();
        }

        private void OnMakePermanentToolStripMenuItemClick(object sender, EventArgs e)
        {
            lock (_tempHighlightEntryListLock)
            {
                lock (_currentHighlightGroupLock)
                {
                    _currentHighlightGroup.HilightEntryList.AddRange(_tempHighlightEntryList);
                    RemoveTempHighlights();
                    OnCurrentHighlightListChanged();
                }
            }
        }

        private void OnMarkCurrentFilterRangeToolStripMenuItemClick(object sender, EventArgs e)
        {
            MarkCurrentFilterRange();
        }

        private void OnFilterForSelectionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
            {
                splitContainerLogWindow.Panel2Collapsed = false;
                ResetFilterControls();
                FilterSearch(ctl.SelectedText);
            }
        }

        private void OnSetSelectedTextAsBookmarkCommentToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
            {
                AddBookmarkComment(ctl.SelectedText);
            }
        }

        private void OnDataGridViewCellClick(object sender, DataGridViewCellEventArgs e)
        {
            _shouldCallTimeSync = true;
        }

        private void OnDataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0) ToggleBookmark();
        }

        private void OnDataGridViewOverlayDoubleClicked(object sender, OverlayEventArgs e)
        {
            BookmarkComment(e.BookmarkOverlay.Bookmark);
        }

        private void OnFilterRegexCheckBoxMouseUp(object sender, MouseEventArgs e)
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

        #endregion

        #region Filter-Highlight

        private void OnToggleHighlightPanelButtonClick(object sender, EventArgs e)
        {
            ToggleHighlightPanel(highlightSplitContainer.Panel2Collapsed);
        }

        private void OnSaveFilterButtonClick(object sender, EventArgs e)
        {
            FilterParams newParams = _filterParams.CreateCopy();
            newParams.color = Color.FromKnownColor(KnownColor.Black);
            ConfigManager.Settings.filterList.Add(newParams);
            OnFilterListChanged(this);
        }

        private void OnDeleteFilterButtonClick(object sender, EventArgs e)
        {
            int index = filterListBox.SelectedIndex;
            if (index >= 0)
            {
                FilterParams filterParams = (FilterParams)filterListBox.Items[index];
                ConfigManager.Settings.filterList.Remove(filterParams);
                OnFilterListChanged(this);
                if (filterListBox.Items.Count > 0) filterListBox.SelectedIndex = filterListBox.Items.Count - 1;
            }
        }

        private void OnFilterUpButtonClick(object sender, EventArgs e)
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

        private void OnFilterDownButtonClick(object sender, EventArgs e)
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

        private void OnFilterListBoxMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (filterListBox.SelectedIndex >= 0)
            {
                FilterParams filterParams = (FilterParams)filterListBox.Items[filterListBox.SelectedIndex];
                FilterParams newParams = filterParams.CreateCopy();
                //newParams.historyList = ConfigManager.Settings.filterHistoryList;
                this._filterParams = newParams;
                ReInitFilterParams(this._filterParams);
                ApplyFilterParams();
                CheckForAdvancedButtonDirty();
                CheckForFilterDirty();
                filterSearchButton.Image = _searchButtonImage;
                saveFilterButton.Enabled = false;
                if (hideFilterListOnLoadCheckBox.Checked) ToggleHighlightPanel(false);

                if (filterOnLoadCheckBox.Checked) FilterSearch();
            }
        }

        private void OnFilterListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                FilterParams filterParams = (FilterParams)filterListBox.Items[e.Index];
                Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

                Brush brush;

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    brush = new SolidBrush(filterListBox.BackColor);
                }
                else
                {
                    brush = new SolidBrush(filterParams.color);
                }

                e.Graphics.DrawString(filterParams.searchText, e.Font, brush,
                    new PointF(rectangle.Left, rectangle.Top));
                e.DrawFocusRectangle();
                brush.Dispose();
            }
        }

        // Color for filter list entry
        private void OnColorToolStripMenuItemClick(object sender, EventArgs e)
        {
            int i = filterListBox.SelectedIndex;
            if (i < filterListBox.Items.Count && i >= 0)
            {
                FilterParams filterParams = (FilterParams)filterListBox.Items[i];
                ColorDialog dlg = new ColorDialog();
                dlg.CustomColors = new[] { filterParams.color.ToArgb() };
                dlg.Color = filterParams.color;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    filterParams.color = dlg.Color;
                    filterListBox.Refresh();
                }
            }
        }

        private void OnFilterCaseSensitiveCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            CheckForFilterDirty();
        }

        private void OnFilterRegexCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            fuzzyKnobControl.Enabled = !filterRegexCheckBox.Checked;
            fuzzyLabel.Enabled = !filterRegexCheckBox.Checked;
            CheckForFilterDirty();
        }

        private void OnInvertFilterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            CheckForFilterDirty();
        }

        private void OnFilterRangeComboBoxTextChanged(object sender, EventArgs e)
        {
            CheckForFilterDirty();
        }

        private void OnFuzzyKnobControlValueChanged(object sender, EventArgs e)
        {
            CheckForFilterDirty();
        }

        private void OnFilterComboBoxTextChanged(object sender, EventArgs e)
        {
            CheckForFilterDirty();
        }

        private void OnSetBookmarksOnSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            SetBookmarksForSelectedFilterLines();
        }

        private void OnParentHighlightSettingsChanged(object sender, EventArgs e)
        {
            string groupName = _guiStateArgs.HighlightGroupName;
            SetCurrentHighlightGroup(groupName);
        }

        private void OnFilterOnLoadCheckBoxMouseClick(object sender, MouseEventArgs e)
        {
            HandleChangedFilterOnLoadSetting();
        }

        private void OnFilterOnLoadCheckBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            HandleChangedFilterOnLoadSetting();
        }

        private void OnHideFilterListOnLoadCheckBoxMouseClick(object sender, MouseEventArgs e)
        {
            HandleChangedFilterOnLoadSetting();
        }

        private void OnFilterToTabToolStripMenuItemClick(object sender, EventArgs e)
        {
            FilterToTab();
        }

        private void OnTimeSyncListWindowRemoved(object sender, EventArgs e)
        {
            TimeSyncList syncList = sender as TimeSyncList;
            lock (_timeSyncListLock)
            {
                if (syncList.Count == 0 || syncList.Count == 1 && syncList.Contains(this))
                    if (syncList == TimeSyncList)
                    {
                        TimeSyncList = null;
                        OnSyncModeChanged();
                    }
            }
        }

        private void OnFreeThisWindowFromTimeSyncToolStripMenuItemClick(object sender, EventArgs e)
        {
            FreeFromTimeSync();
        }

        private void OnSplitContainerSplitterMoved(object sender, SplitterEventArgs e)
        {
            advancedFilterSplitContainer.SplitterDistance = FILTER_ADVANCED_SPLITTER_DISTANCE;
        }

        private void OnMarkFilterHitsInLogViewToolStripMenuItemClick(object sender, EventArgs e)
        {
            SearchParams p = new SearchParams();
            p.searchText = _filterParams.searchText;
            p.isRegex = _filterParams.isRegex;
            p.isCaseSensitive = _filterParams.isCaseSensitive;
            AddSearchHitHighlightEntry(p);
        }

        private void OnStatusLineTriggerSignal(object sender, EventArgs e)
        {
            OnStatusLine(_statusEventArgs);
        }

        private void OnColumnComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            SelectColumn();
        }

        private void OnColumnComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectColumn();
                dataGridView.Focus();
            }
        }

        private void OnColumnComboBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt) columnComboBox.DroppedDown = true;

            if (e.KeyCode == Keys.Enter) e.IsInputKey = true;
        }

        private void OnBookmarkProviderBookmarkRemoved(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                dataGridView.Refresh();
                filterGridView.Refresh();
            }
        }

        private void OnBookmarkProviderBookmarkAdded(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                dataGridView.Refresh();
                filterGridView.Refresh();
            }
        }

        private void OnBookmarkProviderAllBookmarksRemoved(object sender, EventArgs e)
        {
            // nothing
        }

        private void OnLogWindowLeave(object sender, EventArgs e)
        {
            InvalidateCurrentRow();
        }

        private void OnLogWindowEnter(object sender, EventArgs e)
        {
            InvalidateCurrentRow();
        }

        private void OnDataGridViewRowUnshared(object sender, DataGridViewRowEventArgs e)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.Trace($"Row unshared line {e.Row.Cells[1].Value}");
            }
        }

        #endregion

        #endregion

        #endregion
    }
}