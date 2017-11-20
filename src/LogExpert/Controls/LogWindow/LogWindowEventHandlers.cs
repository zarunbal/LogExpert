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
            this.waitingForClose = true;
            this.parentLogTabWin.HighlightSettingsChanged -= parent_HighlightSettingsChanged;
            if (this.logFileReader != null)
            {
                this.logFileReader.DeleteAllContent();
            }
            FreeFromTimeSync();
        }

        private void logFileReader_LoadingStarted(object sender, LoadFileEventArgs e)
        {
            this.Invoke(new LoadingStartedFx(LoadingStarted), new object[] {e});
        }

        private void logFileReader_FinishedLoading(object sender, EventArgs e)
        {
            //Thread.CurrentThread.Name = "FinishedLoading event thread";
            _logger.Info("Finished loading.");
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
                    this.Invoke(new PositionAfterReloadFx(this.PositionAfterReload), new object[] {this.reloadMemento});
                }
                if (this.filterTailCheckBox.Checked)
                {
                    _logger.Info("Refreshing filter view because of reload.");
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
                _logger.Info("Handling file not found event.");
                this.isDeadFile = true;
                this.BeginInvoke(new MethodInvoker(LogfileDead));
            }
        }

        private void logFileReader_Respawned(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(LogfileRespawned));
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

        private void dataGridView_ColumnDividerDoubleClick(object sender,
            DataGridViewColumnDividerDoubleClickEventArgs e)
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
                _logger.Info("File created anew.");

                // File was new created (e.g. rollover)
                this.isDeadFile = false;
                UnRegisterLogFileReaderEvents();
                this.dataGridView.CurrentCellChanged -= new EventHandler(dataGridView_CurrentCellChanged);
                MethodInvoker invoker = new MethodInvoker(ReloadNewFile);
                this.BeginInvoke(invoker);
                //Thread loadThread = new Thread(new ThreadStart(ReloadNewFile));
                //loadThread.Start();
                _logger.Debug("Reloading invoked.");
                return;
            }

            if (!this.isLoading)
            {
                return;
            }
            UpdateProgressCallback callback = new UpdateProgressCallback(UpdateProgress);
            this.BeginInvoke(callback, new object[] {e});
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
            lock (this.logEventArgsList)
            {
                this.logEventArgsList.Add(e);
                this.logEventArgsEvent.Set();
            }
        }

        private void dataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            e.Value = GetCellValue(e.RowIndex, e.ColumnIndex);
        }

        private void dataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            if (!this.CurrentColumnizer.IsTimeshiftImplemented())
            {
                return;
            }
            ILogLine line = this.logFileReader.GetLogLine(e.RowIndex);
            int offset = this.CurrentColumnizer.GetTimeOffset();
            this.CurrentColumnizer.SetTimeOffset(0);
            this.ColumnizerCallbackObject.LineNum = e.RowIndex;
            IColumnizedLogLine cols = this.CurrentColumnizer.SplitLine(this.ColumnizerCallbackObject, line);
            this.CurrentColumnizer.SetTimeOffset(offset);
            if (cols.ColumnValues.Length <= e.ColumnIndex - 2)
            {
                return;
            }

            string oldValue = cols.ColumnValues[e.ColumnIndex - 2].FullValue;
            string newValue = (string) e.Value;
            //string oldValue = (string) this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
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

        private void dataGridView_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
        {
            e.Height = GetRowHeight(e.RowIndex);
        }

        private void dataGridView_CurrentCellChanged(object sender, EventArgs e)
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
            if (this.ShowBookmarkBubbles)
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

            if (e.RowIndex < 0 || e.ColumnIndex < 0 || this.filterResultList.Count <= e.RowIndex)
            {
                e.Handled = false;
                return;
            }
            int lineNum = this.filterResultList[e.RowIndex];
            ILogLine line = this.logFileReader.GetLogLineWithWait(lineNum);
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
            this.BeginInvoke(fx, new object[] {this.filterGridView});
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

        private void rangeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.filterRangeComboBox.Enabled = this.rangeCheckBox.Checked;
            CheckForFilterDirty();
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
                    {
                        FollowTailChanged(true, false);
                    }
                    OnTailFollowed(new EventArgs());
                }
                else
                {
                    //this.guiStateArgs.FollowTail = false;
                    if (this.guiStateArgs.FollowTail)
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
                if (this.filterGridView.CurrentCellAddress.Y >= 0 &&
                    this.filterGridView.CurrentCellAddress.Y < this.filterResultList.Count)
                {
                    int lineNum = this.filterResultList[this.filterGridView.CurrentCellAddress.Y];
                    SelectLine(lineNum, false, true);
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
            if (e.KeyCode == Keys.Tab && e.Control)
            {
                e.IsInputKey = true;
            }
        }

        private void dataGridView_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGridView.CurrentCell != null)
            {
                this.dataGridView.BeginEdit(false);
            }
        }

        private void syncFilterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.syncFilterCheckBox.Checked)
            {
                SyncFilterGridPos();
            }
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

        private void selectionChangedTrigger_Signal(object sender, EventArgs e)
        {
            _logger.Debug("Selection changed trigger");
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
                lock (this.filterPipeList)
                {
                    this.filterPipeList.Remove((FilterPipe) sender);
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

        /*========================================================================
       * Context menu stuff
       *========================================================================*/

        private void dataGridContextMenuStrip_Opening(object sender, CancelEventArgs e)
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
            this.scrollAllTabsToTimestampToolStripMenuItem.Enabled = this.CurrentColumnizer.IsTimeshiftImplemented()
                                                                     &&
                                                                     GetTimestampForLine(ref refLineNum, false) !=
                                                                     DateTime.MinValue;
            this.locateLineInOriginalFileToolStripMenuItem.Enabled = this.IsTempFile &&
                                                                     this.FilterPipe != null &&
                                                                     this.FilterPipe.GetOriginalLineNum(lineNum) != -1;
            this.markEditModeToolStripMenuItem.Enabled = !this.dataGridView.CurrentCell.ReadOnly;

            // Remove all "old" plugin entries
            int index = this.dataGridContextMenuStrip.Items.IndexOf(this.pluginSeparator);
            if (index > 0)
            {
                for (int i = index + 1; i < this.dataGridContextMenuStrip.Items.Count;)
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
                    ContextMenuPluginEventArgs evArgs = new ContextMenuPluginEventArgs(entry, lines,
                        this.CurrentColumnizer,
                        callback);
                    EventHandler ev = new EventHandler(HandlePluginContextMenu);
                    //MenuItem item = this.dataGridView.ContextMenu.MenuItems.Add(entry.GetMenuText(line, this.CurrentColumnizer, callback), ev);
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
            this.tempHighlightsToolStripMenuItem.Enabled = this.tempHilightEntryList.Count > 0;

            this.markCurrentFilterRangeToolStripMenuItem.Enabled = this.filterRangeComboBox.Text != null &&
                                                                   this.filterRangeComboBox.Text.Length > 0;

            if (this.CurrentColumnizer.IsTimeshiftImplemented())
            {
                IList<WindowFileEntry> list = this.parentLogTabWin.GetListOfOpenFiles();
                this.syncTimestampsToToolStripMenuItem.Enabled = true;
                this.syncTimestampsToToolStripMenuItem.DropDownItems.Clear();
                EventHandler ev = new EventHandler(HandleSyncContextMenu);
                Font italicFont = new Font(syncTimestampsToToolStripMenuItem.Font.FontFamily,
                    this.syncTimestampsToToolStripMenuItem.Font.Size, FontStyle.Italic);
                foreach (WindowFileEntry fileEntry in list)
                {
                    if (fileEntry.LogWindow != this)
                    {
                        ToolStripMenuItem item =
                            this.syncTimestampsToToolStripMenuItem.DropDownItems.Add(fileEntry.Title, null, ev) as
                                ToolStripMenuItem;
                        item.Tag = fileEntry;
                        item.Checked = this.TimeSyncList != null && this.TimeSyncList.Contains(fileEntry.LogWindow);
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
                this.syncTimestampsToToolStripMenuItem.Enabled = false;
            }
            this.freeThisWindowFromTimeSyncToolStripMenuItem.Enabled = this.TimeSyncList != null &&
                                                                       this.TimeSyncList.Count > 1;
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

                if (this.TimeSyncList != null && this.TimeSyncList.Contains(entry.LogWindow))
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
            if (this.CurrentColumnizer.IsTimeshiftImplemented())
            {
                int currentLine = this.dataGridView.CurrentCellAddress.Y;
                if (currentLine > 0 && currentLine < this.dataGridView.RowCount)
                {
                    int lineNum = currentLine;
                    DateTime timeStamp = GetTimestampForLine(ref lineNum, false);
                    if (timeStamp.Equals(DateTime.MinValue)) // means: invalid
                    {
                        return;
                    }
                    this.parentLogTabWin.ScrollAllTabsToTimestamp(timeStamp, this);
                }
            }
        }

        private void locateLineInOriginalFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dataGridView.CurrentRow != null && this.FilterPipe != null)
            {
                int lineNum = this.FilterPipe.GetOriginalLineNum(this.dataGridView.CurrentRow.Index);
                if (lineNum != -1)
                {
                    this.FilterPipe.LogWindow.SelectLine(lineNum, false, true);
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

        // ======================= Bookmark list ====================================

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

        // =======================================================================================
        // Column header context menu stuff
        // =======================================================================================

        private void dataGridView_CellContextMenuStripNeeded(object sender,
            DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.RowIndex > 0 && e.RowIndex < this.dataGridView.RowCount
                && !this.dataGridView.Rows[e.RowIndex].Selected)
            {
                SelectLine(e.RowIndex, false, true);
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

        private void filterGridView_CellContextMenuStripNeeded(object sender,
            DataGridViewCellContextMenuStripNeededEventArgs e)
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
                                                                            gridView.Columns[this.selectedCol]
                                                                                .HeaderText + ")";
                }
            }
            DataGridViewColumn col = gridView.Columns[this.selectedCol];
            this.moveLeftToolStripMenuItem.Enabled = col != null && col.DisplayIndex > 0;
            this.moveRightToolStripMenuItem.Enabled = col != null && col.DisplayIndex < gridView.Columns.Count - 1;

            if (gridView.Columns.Count - 1 > this.selectedCol)
            {
                //        DataGridViewColumn colRight = gridView.Columns[this.selectedCol + 1];
                DataGridViewColumn colRight = gridView.Columns.GetNextColumn(col, DataGridViewElementStates.None,
                    DataGridViewElementStates.None);
                this.moveRightToolStripMenuItem.Enabled = colRight != null && colRight.Frozen == col.Frozen;
            }
            if (this.selectedCol > 0)
            {
                //DataGridViewColumn colLeft = gridView.Columns[this.selectedCol - 1];
                DataGridViewColumn colLeft = gridView.Columns.GetPreviousColumn(col, DataGridViewElementStates.None,
                    DataGridViewElementStates.None);

                this.moveLeftToolStripMenuItem.Enabled = colLeft != null && colLeft.Frozen == col.Frozen;
            }
            DataGridViewColumn colLast = gridView.Columns[gridView.Columns.Count - 1];
            this.moveToLastColumnToolStripMenuItem.Enabled = colLast != null && colLast.Frozen == col.Frozen;

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
            SelectLine(e.Line, false, true);
        }

        private void bookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBookmarkAndEditComment();
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
        private void highlightThread_HighlightDoneEvent(object sender, HighlightEventArgs e)
        {
            this.BeginInvoke(new HighlightEventFx(HighlightDoneEventWorker), new object[] {e});
        }

        private void toggleHighlightPanelButton_Click(object sender, EventArgs e)
        {
            ToggleHighlightPanel(this.highlightSplitContainer.Panel2Collapsed);
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
                FilterParams filterParams = (FilterParams) this.filterListBox.Items[index];
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
                FilterParams filterParams = (FilterParams) this.filterListBox.Items[i];
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
                FilterParams filterParams = (FilterParams) this.filterListBox.Items[i];
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
                FilterParams filterParams = (FilterParams) this.filterListBox.Items[this.filterListBox.SelectedIndex];
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
                FilterParams filterParams = (FilterParams) this.filterListBox.Items[e.Index];
                Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

                Brush brush = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? new SolidBrush(this.filterListBox.BackColor)
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
            int i = this.filterListBox.SelectedIndex;
            if (i < this.filterListBox.Items.Count && i >= 0)
            {
                FilterParams filterParams = (FilterParams) this.filterListBox.Items[i];
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

        private void parent_HighlightSettingsChanged(object sender, EventArgs e)
        {
            string groupName = this.guiStateArgs.HighlightGroupName;
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
            lock (this.timeSyncListLock)
            {
                if (syncList.Count == 0 || syncList.Count == 1 && syncList.Contains(this))
                {
                    if (syncList == this.TimeSyncList)
                    {
                        this.TimeSyncList = null;
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
            this.advancedFilterSplitContainer.SplitterDistance = FILTER_ADCANCED_SPLITTER_DISTANCE;
        }

        private void markFilterHitsInLogViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchParams p = new SearchParams();
            p.searchText = this.filterParams.searchText;
            p.isRegex = this.filterParams.isRegex;
            p.isCaseSensitive = this.filterParams.isCaseSensitive;
            AddSearchHitHighlightEntry(p);
        }

        private void statusLineTrigger_Signal(object sender, EventArgs e)
        {
            OnStatusLine(this.statusEventArgs);
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

        private void LogWindow_Leave(object sender, EventArgs e)
        {
            InvalidateCurrentRow();
        }

        private void LogWindow_Enter(object sender, EventArgs e)
        {
            InvalidateCurrentRow();
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
            if (this.FilterListChanged != null)
            {
                this.FilterListChanged(this, new FilterListChangedEventArgs(source));
            }
        }

        protected void OnCurrentHighlightListChanged()
        {
            if (this.CurrentHighlightGroupChanged != null)
            {
                this.CurrentHighlightGroupChanged(this,
                    new CurrentHighlightGroupChangedEventArgs(this, this.currentHighlightGroup));
            }
        }

        protected void OnFileReloadFinished()
        {
            if (FileReloadFinished != null)
            {
                FileReloadFinished(this, new EventArgs());
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

        protected void OnAllBookmarksRemoved()
        {
            if (AllBookmarksRemoved != null)
            {
                AllBookmarksRemoved(this, new EventArgs());
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
    }
}