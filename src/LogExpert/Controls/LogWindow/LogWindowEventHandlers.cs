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
        #region Events handler

        private void LogWindow_Disposed(object sender, EventArgs e)
        {
            waitingForClose = true;
            parentLogTabWin.HighlightSettingsChanged -= parent_HighlightSettingsChanged;
            if (logFileReader != null)
            {
                logFileReader.DeleteAllContent();
            }

            FreeFromTimeSync();
        }

        private void logFileReader_LoadingStarted(object sender, LoadFileEventArgs e)
        {
            Invoke(new LoadingStartedFx(LoadingStarted), new object[] {e});
        }

        private void logFileReader_FinishedLoading(object sender, EventArgs e)
        {
            //Thread.CurrentThread.Name = "FinishedLoading event thread";
            _logger.Info("Finished loading.");
            isLoading = false;
            isDeadFile = false;
            if (!waitingForClose)
            {
                Invoke(new MethodInvoker(LoadingFinished));
                Invoke(new MethodInvoker(LoadPersistenceData));
                Invoke(new MethodInvoker(SetGuiAfterLoading));
                loadingFinishedEvent.Set();
                externaLoadingFinishedEvent.Set();
                timeSpreadCalc.SetLineCount(logFileReader.LineCount);

                if (reloadMemento != null)
                {
                    Invoke(new PositionAfterReloadFx(PositionAfterReload), new object[] {reloadMemento});
                }

                if (filterTailCheckBox.Checked)
                {
                    _logger.Info("Refreshing filter view because of reload.");
                    Invoke(new MethodInvoker(FilterSearch)); // call on proper thread
                }

                HandleChangedFilterList();
            }

            reloadMemento = null;
        }

        private void logFileReader_FileNotFound(object sender, EventArgs e)
        {
            if (!IsDisposed && !Disposing)
            {
                _logger.Info("Handling file not found event.");
                isDeadFile = true;
                BeginInvoke(new MethodInvoker(LogfileDead));
            }
        }

        private void logFileReader_Respawned(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(LogfileRespawned));
        }

        private void LogWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Preferences.askForClose)
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

        private void dataGridView_ColumnDividerDoubleClick(object sender,
            DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            e.Handled = true;
            AutoResizeColumns(dataGridView);
        }

        /**
       * Event handler for the Load event from LogfileReader
       */

        private void logFileReader_LoadFile(object sender, LoadFileEventArgs e)
        {
            if (e.NewFile)
            {
                _logger.Info("File created anew.");

                // File was new created (e.g. rollover)
                isDeadFile = false;
                UnRegisterLogFileReaderEvents();
                dataGridView.CurrentCellChanged -= new EventHandler(dataGridView_CurrentCellChanged);
                MethodInvoker invoker = new MethodInvoker(ReloadNewFile);
                BeginInvoke(invoker);
                //Thread loadThread = new Thread(new ThreadStart(ReloadNewFile));
                //loadThread.Start();
                _logger.Debug("Reloading invoked.");
                return;
            }

            if (!isLoading)
            {
                return;
            }

            UpdateProgressCallback callback = new UpdateProgressCallback(UpdateProgress);
            BeginInvoke(callback, new object[] {e});
        }

        private void FileSizeChangedHandler(object sender, LogEventArgs e)
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
            lock (logEventArgsList)
            {
                logEventArgsList.Add(e);
                logEventArgsEvent.Set();
            }
        }

        private void dataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            int startCount = CurrentColumnizer?.GetColumnCount() ?? 0;

            e.Value = GetCellValue(e.RowIndex, e.ColumnIndex);

            // The new column could be find dynamically.
            // Only support add new columns for now.
            // TODO: Support reload all columns?
            if (CurrentColumnizer != null && CurrentColumnizer.GetColumnCount() > startCount)
            {
                for (int i = startCount; i < CurrentColumnizer.GetColumnCount(); i++)
                {
                    var colName = CurrentColumnizer.GetColumnNames()[i];
                    DataGridViewColumn titleColumn = new LogTextColumn();
                    titleColumn.HeaderText = colName;
                    titleColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                    titleColumn.Resizable = DataGridViewTriState.NotSet;
                    titleColumn.DividerWidth = 1;
                    dataGridView.Columns.Add(titleColumn);
                }
            }
        }

        private void dataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            if (!CurrentColumnizer.IsTimeshiftImplemented())
            {
                return;
            }

            ILogLine line = logFileReader.GetLogLine(e.RowIndex);
            int offset = CurrentColumnizer.GetTimeOffset();
            CurrentColumnizer.SetTimeOffset(0);
            ColumnizerCallbackObject.LineNum = e.RowIndex;
            IColumnizedLogLine cols = CurrentColumnizer.SplitLine(ColumnizerCallbackObject, line);
            CurrentColumnizer.SetTimeOffset(offset);
            if (cols.ColumnValues.Length <= e.ColumnIndex - 2)
            {
                return;
            }

            string oldValue = cols.ColumnValues[e.ColumnIndex - 2].FullValue;
            string newValue = (string) e.Value;
            //string oldValue = (string) this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
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

        private void dataGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
        {
            e.Height = GetRowHeight(e.RowIndex);
        }

        private void dataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                statusEventArgs.CurrentLineNum = dataGridView.CurrentRow.Index + 1;
                SendStatusLineUpdate();
                if (syncFilterCheckBox.Checked)
                {
                    SyncFilterGridPos();
                }

                if (CurrentColumnizer.IsTimeshiftImplemented() && Preferences.timestampControl)
                {
                    SyncTimestampDisplay();
                }

                //MethodInvoker invoker = new MethodInvoker(DisplayCurrentFileOnStatusline);
                //invoker.BeginInvoke(null, null);
            }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            StatusLineText("");
        }

        private void editControl_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl) sender);
        }

        private void editControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl) sender);
        }

        private void editControl_Click(object sender, EventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl) sender);
        }

        private void editControl_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl) sender);
        }

        private void dataGridView_Paint(object sender, PaintEventArgs e)
        {
            if (ShowBookmarkBubbles)
            {
                AddBookmarkOverlays();
            }
        }

        // ======================================================================================
        // Filter Grid stuff
        // ======================================================================================

        private void filterSearchButton_Click(object sender, EventArgs e)
        {
            FilterSearch();
        }

        private void filterGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView gridView = (DataGridView) sender;

            if (e.RowIndex < 0 || e.ColumnIndex < 0 || filterResultList.Count <= e.RowIndex)
            {
                e.Handled = false;
                return;
            }

            int lineNum = filterResultList[e.RowIndex];
            ILogLine line = logFileReader.GetLogLineWithWait(lineNum);
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
                    PaintCell(e, filterGridView, false, entry);
                }

                if (e.ColumnIndex == 0)
                {
                    if (bookmarkProvider.IsBookmarkAtLine(lineNum))
                    {
                        Rectangle r = new Rectangle(e.CellBounds.Left + 2, e.CellBounds.Top + 2, 6, 6);
                        r = e.CellBounds;
                        r.Inflate(-2, -2);
                        Brush brush = new SolidBrush(BookmarkColor);
                        e.Graphics.FillRectangle(brush, r);
                        brush.Dispose();
                        Bookmark bookmark = bookmarkProvider.GetBookmarkForLine(lineNum);
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
                }

                e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                e.Handled = true;
            }
        }

        private void filterGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || filterResultList.Count <= e.RowIndex)
            {
                e.Value = "";
                return;
            }

            int lineNum = filterResultList[e.RowIndex];
            e.Value = GetCellValue(lineNum, e.ColumnIndex);
        }

        private void filterGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
        {
            e.Height = lineHeight;
        }

        private void filterComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FilterSearch();
            }
        }

        private void filterGridView_ColumnDividerDoubleClick(object sender,
            DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            e.Handled = true;
            AutoResizeColumnsFx fx = AutoResizeColumns;
            BeginInvoke(fx, new object[] {filterGridView});
        }

        private void filterGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                ToggleBookmark();
                return;
            }

            if (filterGridView.CurrentRow != null && e.RowIndex >= 0)
            {
                int lineNum = filterResultList[filterGridView.CurrentRow.Index];
                SelectAndEnsureVisible(lineNum, true);
            }
        }

        private void rangeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            filterRangeComboBox.Enabled = rangeCheckBox.Checked;
            CheckForFilterDirty();
        }

        private void dataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                if (dataGridView.DisplayedRowCount(false) +
                    dataGridView.FirstDisplayedScrollingRowIndex
                    >= dataGridView.RowCount
                )
                {
                    //this.guiStateArgs.FollowTail = true;
                    if (!guiStateArgs.FollowTail)
                    {
                        FollowTailChanged(true, false);
                    }

                    OnTailFollowed(new EventArgs());
                }
                else
                {
                    //this.guiStateArgs.FollowTail = false;
                    if (guiStateArgs.FollowTail)
                    {
                        FollowTailChanged(false, false);
                    }
                }

                SendGuiStateUpdate();
            }
        }

        private void filterGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (filterGridView.CurrentCellAddress.Y >= 0 &&
                    filterGridView.CurrentCellAddress.Y < filterResultList.Count)
                {
                    int lineNum = filterResultList[filterGridView.CurrentCellAddress.Y];
                    SelectLine(lineNum, false, true);
                    e.Handled = true;
                }
            }

            if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.None)
            {
                dataGridView.Focus();
                e.Handled = true;
            }
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.None)
            {
                filterGridView.Focus();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.Control)
            {
                //this.parentLogTabWin.SwitchTab(e.Shift);
            }

            shouldCallTimeSync = true;
        }

        private void dataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && e.Control)
            {
                e.IsInputKey = true;
            }
        }

        private void dataGridView_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView.CurrentCell != null)
            {
                dataGridView.BeginEdit(false);
            }
        }

        private void syncFilterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (syncFilterCheckBox.Checked)
            {
                SyncFilterGridPos();
            }
        }

        private void dataGridView_Leave(object sender, EventArgs e)
        {
            InvalidateCurrentRow(dataGridView);
        }

        private void dataGridView_Enter(object sender, EventArgs e)
        {
            InvalidateCurrentRow(dataGridView);
        }

        private void filterGridView_Enter(object sender, EventArgs e)
        {
            InvalidateCurrentRow(filterGridView);
        }

        private void filterGridView_Leave(object sender, EventArgs e)
        {
            InvalidateCurrentRow(filterGridView);
        }

        private void dataGridView_Resize(object sender, EventArgs e)
        {
            if (logFileReader != null && dataGridView.RowCount > 0
                                      && guiStateArgs.FollowTail)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.RowCount - 1;
            }
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectionDisplay();
        }

        private void selectionChangedTrigger_Signal(object sender, EventArgs e)
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
                        MethodInvoker invoker = new MethodInvoker(DisplayCurrentFileOnStatusline);
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

        private void filterKnobControl1_ValueChanged(object sender, EventArgs e)
        {
            CheckForFilterDirty();
        }

        private void filterToTabButton_Click(object sender, EventArgs e)
        {
            FilterToTab();
        }

        private void pipe_Disconnected(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(FilterPipe))
            {
                lock (filterPipeList)
                {
                    filterPipeList.Remove((FilterPipe) sender);
                    if (filterPipeList.Count == 0)
                    {
                        // reset naming counter to 0 if no more open filter tabs for this source window
                        filterPipeNameCounter = 0;
                    }
                }
            }
        }

        private void advancedButton_Click(object sender, EventArgs e)
        {
            showAdvanced = !showAdvanced;
            ShowAdvancedFilterPanel(showAdvanced);
        }

        /*========================================================================
       * Context menu stuff
       *========================================================================*/

        private void dataGridContextMenuStrip_Opening(object sender, CancelEventArgs e)
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
                //string line = this.logFileReader.GetLogLine(lineNum);
                IList<int> lines = GetSelectedContent();
                foreach (IContextMenuEntry entry in PluginRegistry.GetInstance().RegisteredContextMenuPlugins)
                {
                    LogExpertCallback callback = new LogExpertCallback(this);
                    ContextMenuPluginEventArgs evArgs = new ContextMenuPluginEventArgs(entry, lines,
                        CurrentColumnizer,
                        callback);
                    EventHandler ev = new EventHandler(HandlePluginContextMenu);
                    //MenuItem item = this.dataGridView.ContextMenu.MenuItems.Add(entry.GetMenuText(line, this.CurrentColumnizer, callback), ev);
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
            tempHighlightsToolStripMenuItem.Enabled = tempHilightEntryList.Count > 0;

            markCurrentFilterRangeToolStripMenuItem.Enabled = filterRangeComboBox.Text != null &&
                                                              filterRangeComboBox.Text.Length > 0;

            if (CurrentColumnizer.IsTimeshiftImplemented())
            {
                IList<WindowFileEntry> list = parentLogTabWin.GetListOfOpenFiles();
                syncTimestampsToToolStripMenuItem.Enabled = true;
                syncTimestampsToToolStripMenuItem.DropDownItems.Clear();
                EventHandler ev = new EventHandler(HandleSyncContextMenu);
                Font italicFont = new Font(syncTimestampsToToolStripMenuItem.Font.FontFamily,
                    syncTimestampsToToolStripMenuItem.Font.Size, FontStyle.Italic);
                foreach (WindowFileEntry fileEntry in list)
                {
                    if (fileEntry.LogWindow != this)
                    {
                        ToolStripMenuItem item =
                            syncTimestampsToToolStripMenuItem.DropDownItems.Add(fileEntry.Title, null, ev) as
                                ToolStripMenuItem;
                        item.Tag = fileEntry;
                        item.Checked = TimeSyncList != null && TimeSyncList.Contains(fileEntry.LogWindow);
                        if (fileEntry.LogWindow.TimeSyncList != null &&
                            !fileEntry.LogWindow.TimeSyncList.Contains(this))
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
                    //AddSlaveToTimesync(entry.LogWindow);
                    AddOtherWindowToTimesync(entry.LogWindow);
                }
            }
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

                    parentLogTabWin.ScrollAllTabsToTimestamp(timeStamp, this);
                }
            }
        }

        private void locateLineInOriginalFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null && FilterPipe != null)
            {
                int lineNum = FilterPipe.GetOriginalLineNum(dataGridView.CurrentRow.Index);
                if (lineNum != -1)
                {
                    FilterPipe.LogWindow.SelectLine(lineNum, false, true);
                    parentLogTabWin.SelectTab(FilterPipe.LogWindow);
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

        // ======================= Bookmark list ====================================

        private void columnRestrictCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            columnButton.Enabled = columnRestrictCheckBox.Checked;
            if (columnRestrictCheckBox.Checked) // disable when nothing to filter
            {
                columnNamesLabel.Visible = true;
                filterParams.columnRestrict = true;
                columnNamesLabel.Text = CalculateColumnNames(filterParams);
            }
            else
            {
                columnNamesLabel.Visible = false;
            }

            CheckForFilterDirty();
        }

        private void columnButton_Click(object sender, EventArgs e)
        {
            filterParams.currentColumnizer = currentColumnizer;
            FilterColumnChooser chooser = new FilterColumnChooser(filterParams);
            if (chooser.ShowDialog() == DialogResult.OK)
            {
                columnNamesLabel.Text = CalculateColumnNames(filterParams);

                //CheckForFilterDirty(); //!!!GBro: Indicate to redo the search if search columns were changed
                filterSearchButton.Image = searchButtonImage;
                saveFilterButton.Enabled = false;
            }
        }

        // =======================================================================================
        // Column header context menu stuff
        // =======================================================================================

        private void dataGridView_CellContextMenuStripNeeded(object sender,
            DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.RowIndex > 0 && e.RowIndex < dataGridView.RowCount
                               && !dataGridView.Rows[e.RowIndex].Selected)
            {
                SelectLine(e.RowIndex, false, true);
            }

            if (e.ContextMenuStrip == columnContextMenuStrip)
            {
                selectedCol = e.ColumnIndex;
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

        private void filterGridView_CellContextMenuStripNeeded(object sender,
            DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.ContextMenuStrip == columnContextMenuStrip)
            {
                selectedCol = e.ColumnIndex;
            }
        }

        private void columnContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            Control ctl = columnContextMenuStrip.SourceControl;
            DataGridView gridView = ctl as DataGridView;
            bool frozen = false;
            if (freezeStateMap.ContainsKey(ctl))
            {
                frozen = freezeStateMap[ctl];
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
                                                                       gridView.Columns[selectedCol]
                                                                           .HeaderText + ")";
                }
            }

            DataGridViewColumn col = gridView.Columns[selectedCol];
            moveLeftToolStripMenuItem.Enabled = col != null && col.DisplayIndex > 0;
            moveRightToolStripMenuItem.Enabled = col != null && col.DisplayIndex < gridView.Columns.Count - 1;

            if (gridView.Columns.Count - 1 > selectedCol)
            {
                //        DataGridViewColumn colRight = gridView.Columns[this.selectedCol + 1];
                DataGridViewColumn colRight = gridView.Columns.GetNextColumn(col, DataGridViewElementStates.None,
                    DataGridViewElementStates.None);
                moveRightToolStripMenuItem.Enabled = colRight != null && colRight.Frozen == col.Frozen;
            }

            if (selectedCol > 0)
            {
                //DataGridViewColumn colLeft = gridView.Columns[this.selectedCol - 1];
                DataGridViewColumn colLeft = gridView.Columns.GetPreviousColumn(col, DataGridViewElementStates.None,
                    DataGridViewElementStates.None);

                moveLeftToolStripMenuItem.Enabled = colLeft != null && colLeft.Frozen == col.Frozen;
            }

            DataGridViewColumn colLast = gridView.Columns[gridView.Columns.Count - 1];
            moveToLastColumnToolStripMenuItem.Enabled = colLast != null && colLast.Frozen == col.Frozen;

            // Fill context menu with column names 
            //
            EventHandler ev = new EventHandler(HandleColumnItemContextMenu);
            allColumnsToolStripMenuItem.DropDownItems.Clear();
            foreach (DataGridViewColumn column in gridView.Columns)
            {
                if (column.HeaderText.Length > 0)
                {
                    ToolStripMenuItem item =
                        allColumnsToolStripMenuItem.DropDownItems.Add(column.HeaderText, null, ev) as ToolStripMenuItem;
                    item.Tag = column;
                    item.Enabled = !column.Frozen;
                }
            }
        }

        private void HandleColumnItemContextMenu(object sender, EventArgs args)
        {
            if (sender is ToolStripItem)
            {
                DataGridViewColumn column = (sender as ToolStripItem).Tag as DataGridViewColumn;
                column.Visible = true;
                column.DataGridView.FirstDisplayedScrollingColumnIndex = column.Index;
            }
        }

        private void freezeLeftColumnsUntilHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control ctl = columnContextMenuStrip.SourceControl;
            bool frozen = false;
            if (freezeStateMap.ContainsKey(ctl))
            {
                frozen = freezeStateMap[ctl];
            }

            frozen = !frozen;
            freezeStateMap[ctl] = frozen;

            DataGridViewColumn senderCol = sender as DataGridViewColumn;
            if (ctl is DataGridView)
            {
                DataGridView gridView = ctl as DataGridView;
                ApplyFrozenState(gridView);
            }
        }

        private void moveToLastColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[selectedCol];
            if (col != null)
            {
                col.DisplayIndex = gridView.Columns.Count - 1;
            }
        }

        private void moveLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[selectedCol];
            if (col != null && col.DisplayIndex > 0)
            {
                col.DisplayIndex = col.DisplayIndex - 1;
            }
        }

        private void moveRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[selectedCol];
            if (col != null && col.DisplayIndex < gridView.Columns.Count - 1)
            {
                col.DisplayIndex = col.DisplayIndex + 1;
            }
        }

        private void hideColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            DataGridViewColumn col = gridView.Columns[selectedCol];
            col.Visible = false;
        }

        private void restoreColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView gridView = columnContextMenuStrip.SourceControl as DataGridView;
            foreach (DataGridViewColumn col in gridView.Columns)
            {
                col.Visible = true;
            }
        }

        private void timeSpreadingControl1_LineSelected(object sender, SelectLineEventArgs e)
        {
            SelectLine(e.Line, false, true);
        }

        private void bookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBookmarkAndEditComment();
        }

        private void highlightSelectionInLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl ctl =
                    dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow,
                    false, true, false, false, false, false, null, false);
                lock (tempHilightEntryListLock)
                {
                    tempHilightEntryList.Add(he);
                }

                dataGridView.CancelEdit();
                dataGridView.EndEdit();
                RefreshAllGrids();
            }
        }

        private void highlightSelectionInLogFilewordModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl ctl =
                    dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                HilightEntry he = new HilightEntry(ctl.SelectedText, Color.Red, Color.Yellow,
                    false, true, false, false, false, false, null, true);
                lock (tempHilightEntryListLock)
                {
                    tempHilightEntryList.Add(he);
                }

                dataGridView.CancelEdit();
                dataGridView.EndEdit();
                RefreshAllGrids();
            }
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl ctl =
                    dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
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
            lock (tempHilightEntryListLock)
            {
                lock (currentHighlightGroupLock)
                {
                    currentHighlightGroup.HilightEntryList.AddRange(tempHilightEntryList);
                    RemoveTempHighlights();
                    OnCurrentHighlightListChanged();
                }
            }
        }

        private void markCurrentFilterRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            markCurrentFilterRange();
        }

        private void filterForSelectionToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void setSelectedTextAsBookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl)
            {
                DataGridViewTextBoxEditingControl ctl =
                    dataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                AddBookmarkComment(ctl.SelectedText);
            }
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            shouldCallTimeSync = true;
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
        private void highlightThread_HighlightDoneEvent(object sender, HighlightEventArgs e)
        {
            BeginInvoke(new HighlightEventFx(HighlightDoneEventWorker), new object[] {e});
        }

        private void toggleHighlightPanelButton_Click(object sender, EventArgs e)
        {
            ToggleHighlightPanel(highlightSplitContainer.Panel2Collapsed);
        }

        private void saveFilterButton_Click(object sender, EventArgs e)
        {
            FilterParams newParams = filterParams.CreateCopy();
            newParams.color = Color.FromKnownColor(KnownColor.Black);
            ConfigManager.Settings.filterList.Add(newParams);
            OnFilterListChanged(this);
        }

        private void deleteFilterButton_Click(object sender, EventArgs e)
        {
            int index = filterListBox.SelectedIndex;
            if (index >= 0)
            {
                FilterParams filterParams = (FilterParams) filterListBox.Items[index];
                ConfigManager.Settings.filterList.Remove(filterParams);
                OnFilterListChanged(this);
                if (filterListBox.Items.Count > 0)
                {
                    filterListBox.SelectedIndex = filterListBox.Items.Count - 1;
                }
            }
        }

        private void filterUpButton_Click(object sender, EventArgs e)
        {
            int i = filterListBox.SelectedIndex;
            if (i > 0)
            {
                FilterParams filterParams = (FilterParams) filterListBox.Items[i];
                ConfigManager.Settings.filterList.RemoveAt(i);
                i--;
                ConfigManager.Settings.filterList.Insert(i, filterParams);
                OnFilterListChanged(this);
                filterListBox.SelectedIndex = i;
            }
        }

        private void filterDownButton_Click(object sender, EventArgs e)
        {
            int i = filterListBox.SelectedIndex;
            if (i < filterListBox.Items.Count - 1)
            {
                FilterParams filterParams = (FilterParams) filterListBox.Items[i];
                ConfigManager.Settings.filterList.RemoveAt(i);
                i++;
                ConfigManager.Settings.filterList.Insert(i, filterParams);
                OnFilterListChanged(this);
                filterListBox.SelectedIndex = i;
            }
        }

        private void filterListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (filterListBox.SelectedIndex >= 0)
            {
                FilterParams filterParams = (FilterParams) filterListBox.Items[filterListBox.SelectedIndex];
                FilterParams newParams = filterParams.CreateCopy();
                //newParams.historyList = ConfigManager.Settings.filterHistoryList;
                this.filterParams = newParams;
                ReInitFilterParams(this.filterParams);
                ApplyFilterParams();
                CheckForAdvancedButtonDirty();
                CheckForFilterDirty();
                filterSearchButton.Image = searchButtonImage;
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

        private void filterListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                FilterParams filterParams = (FilterParams) filterListBox.Items[e.Index];
                Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

                Brush brush = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? new SolidBrush(filterListBox.BackColor)
                    : new SolidBrush(filterParams.color);

                e.Graphics.DrawString(filterParams.searchText, e.Font, brush,
                    new PointF(rectangle.Left, rectangle.Top));
                e.DrawFocusRectangle();
                brush.Dispose();
            }
        }

        // Color for filter list entry
        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = filterListBox.SelectedIndex;
            if (i < filterListBox.Items.Count && i >= 0)
            {
                FilterParams filterParams = (FilterParams) filterListBox.Items[i];
                ColorDialog dlg = new ColorDialog();
                dlg.CustomColors = new int[] {filterParams.color.ToArgb()};
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
            fuzzyKnobControl.Enabled = !filterRegexCheckBox.Checked;
            fuzzyLabel.Enabled = !filterRegexCheckBox.Checked;
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

        private void parent_HighlightSettingsChanged(object sender, EventArgs e)
        {
            string groupName = guiStateArgs.HighlightGroupName;
            SetCurrentHighlightGroup(groupName);
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

        private void filterToTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterToTab();
        }

        private void timeSyncList_WindowRemoved(object sender, EventArgs e)
        {
            TimeSyncList syncList = sender as TimeSyncList;
            lock (timeSyncListLock)
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

        private void freeThisWindowFromTimeSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FreeFromTimeSync();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;
        }

        private void markFilterHitsInLogViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchParams p = new SearchParams();
            p.searchText = filterParams.searchText;
            p.isRegex = filterParams.isRegex;
            p.isCaseSensitive = filterParams.isCaseSensitive;
            AddSearchHitHighlightEntry(p);
        }

        private void statusLineTrigger_Signal(object sender, EventArgs e)
        {
            OnStatusLine(statusEventArgs);
        }

        private void columnComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SelectColumn();
        }

        private void columnComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectColumn();
                dataGridView.Focus();
            }
        }

        private void columnComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
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

        private void bookmarkProvider_BookmarkRemoved(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                dataGridView.Refresh();
                filterGridView.Refresh();
            }
        }

        private void bookmarkProvider_BookmarkAdded(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                dataGridView.Refresh();
                filterGridView.Refresh();
            }
        }

        private void bookmarkProvider_AllBookmarksRemoved(object sender, EventArgs e)
        {
            // nothing
        }

        private void LogWindow_Leave(object sender, EventArgs e)
        {
            InvalidateCurrentRow();
        }

        private void LogWindow_Enter(object sender, EventArgs e)
        {
            InvalidateCurrentRow();
        }

        private void dataGridView_RowUnshared(object sender, DataGridViewRowEventArgs e)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.Trace("Row unshared line {0}", e.Row.Cells[1].Value);
            }
        }

        #endregion

        protected void OnProgressBarUpdate(ProgressEventArgs e)
        {
            ProgressBarEventHandler handler = ProgressBarUpdate;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnStatusLine(StatusLineEventArgs e)
        {
            StatusLineEventHandler handler = StatusLineEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnGuiState(GuiStateArgs e)
        {
            GuiStateEventHandler handler = GuiStateUpdate;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnTailFollowed(EventArgs e)
        {
            if (TailFollowed != null)
            {
                TailFollowed(this, e);
            }
        }

        protected void OnFileNotFound(EventArgs e)
        {
            if (FileNotFound != null)
            {
                FileNotFound(this, e);
            }
        }

        protected void OnFileRespawned(EventArgs e)
        {
            if (FileRespawned != null)
            {
                FileRespawned(this, e);
            }
        }

        protected void OnFilterListChanged(LogWindow source)
        {
            if (FilterListChanged != null)
            {
                FilterListChanged(this, new FilterListChangedEventArgs(source));
            }
        }

        protected void OnCurrentHighlightListChanged()
        {
            if (CurrentHighlightGroupChanged != null)
            {
                CurrentHighlightGroupChanged(this,
                    new CurrentHighlightGroupChangedEventArgs(this, currentHighlightGroup));
            }
        }

        protected void OnBookmarkAdded()
        {
            if (BookmarkAdded != null)
            {
                BookmarkAdded(this, new EventArgs());
            }
        }

        protected void OnBookmarkRemoved()
        {
            if (BookmarkRemoved != null)
            {
                BookmarkRemoved(this, new EventArgs());
            }
        }

        protected void OnBookmarkTextChanged(Bookmark bookmark)
        {
            if (BookmarkTextChanged != null)
            {
                BookmarkTextChanged(this, new BookmarkEventArgs(bookmark));
            }
        }

        protected void OnColumnizerChanged(ILogLineColumnizer columnizer)
        {
            if (ColumnizerChanged != null)
            {
                ColumnizerChanged(this, new ColumnizerEventArgs(columnizer));
            }
        }

        protected void RegisterCancelHandler(BackgroundProcessCancelHandler handler)
        {
            lock (cancelHandlerList)
            {
                cancelHandlerList.Add(handler);
            }
        }

        protected void DeRegisterCancelHandler(BackgroundProcessCancelHandler handler)
        {
            lock (cancelHandlerList)
            {
                cancelHandlerList.Remove(handler);
            }
        }
    }
}