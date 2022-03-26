using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LogExpert.Dialogs;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
    public partial class LogTabWindow
    {
        #region Events handler

        private void bookmarkWindow_VisibleChanged(object sender, EventArgs e)
        {
            firstBookmarkWindowShow = false;
        }

        private void LogTabWindow_Load(object sender, EventArgs e)
        {
            ApplySettings(ConfigManager.Settings, SettingsFlags.All);
            if (ConfigManager.Settings.isMaximized)
            {
                if (ConfigManager.Settings.appBoundsFullscreen != null)
                {
                    Bounds = ConfigManager.Settings.appBoundsFullscreen;
                }
                WindowState = FormWindowState.Maximized;
                Bounds = ConfigManager.Settings.appBounds;
            }
            else
            {
                if (ConfigManager.Settings.appBounds != null && ConfigManager.Settings.appBounds.Right > 0)
                {
                    Bounds = ConfigManager.Settings.appBounds;
                }
            }

            if (ConfigManager.Settings.preferences.openLastFiles && startupFileNames == null)
            {
                List<string> tmpList = ObjectClone.Clone<List<string>>(ConfigManager.Settings.lastOpenFilesList);
                foreach (string name in tmpList)
                {
                    if (name != null && name.Length > 0)
                    {
                        AddFileTab(name, false, null, false, null);
                    }
                }
            }
            if (startupFileNames != null)
            {
                LoadFiles(startupFileNames, false);
            }
            ledThread = new Thread(LedThreadProc);
            ledThread.IsBackground = true;
            ledThread.Start();

            statusLineThread = new Thread(StatusLineThreadFunc);
            statusLineThread.IsBackground = true;
            statusLineThread.Start();

            FillHighlightComboBox();
            FillToolLauncherBar();
#if !DEBUG
            debugToolStripMenuItem.Visible = false;
#endif
        }

        private void LogTabWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                shouldStop = true;
                statusLineEventHandle.Set();
                statusLineEventWakeupHandle.Set();
                ledThread.Join();
                statusLineThread.Join();

                IList<LogWindow> deleteLogWindowList = new List<LogWindow>();
                ConfigManager.Settings.alwaysOnTop = TopMost && ConfigManager.Settings.preferences.allowOnlyOneInstance;
                SaveLastOpenFilesList();

                foreach (LogWindow logWindow in logWindowList)
                {
                    deleteLogWindowList.Add(logWindow);
                }

                foreach (LogWindow logWindow in deleteLogWindowList)
                {
                    RemoveAndDisposeLogWindow(logWindow, true);
                }

                DestroyBookmarkWindow();

                ConfigManager.Instance.ConfigChanged -= ConfigChanged;

                SaveWindowPosition();
                ConfigManager.Save(SettingsFlags.WindowPosition | SettingsFlags.FileHistory);
            }
            catch (Exception)
            {
                // ignore error (can occur then multipe instances are closed simultaneously or if the
                // window was not constructed completely because of errors)
            }
            finally
            {
                LogExpertProxy?.WindowClosed(this);
            }
        }

        private void strip_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is ToolStripDropDown)
            {
                AddFileTab(((ToolStripDropDown) sender).Text, false, null, false, null);
            }
        }

        private void history_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text != null && e.ClickedItem.Text.Length > 0)
            {
                AddFileTab(e.ClickedItem.Text, false, null, false, null);
            }
        }

        private void logWindow_Disposed(object sender, EventArgs e)
        {
            LogWindow logWindow = sender as LogWindow;
            if (sender == CurrentLogWindow)
            {
                ChangeCurrentLogWindow(null);
            }
            RemoveLogWindow(logWindow);
            logWindow.Tag = null;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void selectFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentLogWindow == null)
            {
                return;
            }

            CurrentLogWindow.ColumnizerCallbackObject.LineNum = CurrentLogWindow.GetCurrentLineNum();
            FilterSelectorForm form = new FilterSelectorForm(PluginRegistry.GetInstance().RegisteredColumnizers, CurrentLogWindow.CurrentColumnizer, CurrentLogWindow.ColumnizerCallbackObject);
            form.Owner = this;
            form.TopMost = TopMost;
            DialogResult res = form.ShowDialog();
            
            if (res == DialogResult.OK)
            {
                if (form.ApplyToAll)
                {
                    lock (logWindowList)
                    {
                        foreach (LogWindow logWindow in logWindowList)
                        {
                            if (logWindow.CurrentColumnizer.GetType() != form.SelectedColumnizer.GetType())
                            {
                                //logWindow.SetColumnizer(form.SelectedColumnizer);
                                SetColumnizerFx fx = logWindow.ForceColumnizer;
                                logWindow.Invoke(fx, form.SelectedColumnizer);
                                setColumnizerHistoryEntry(logWindow.FileName, form.SelectedColumnizer);
                            }
                            else
                            {
                                if (form.IsConfigPressed)
                                {
                                    logWindow.ColumnizerConfigChanged();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (CurrentLogWindow.CurrentColumnizer.GetType() != form.SelectedColumnizer.GetType())
                    {
                        SetColumnizerFx fx = CurrentLogWindow.ForceColumnizer;
                        CurrentLogWindow.Invoke(fx, form.SelectedColumnizer);
                        setColumnizerHistoryEntry(CurrentLogWindow.FileName, form.SelectedColumnizer);
                    }

                    if (form.IsConfigPressed)
                    {
                        lock (logWindowList)
                        {
                            foreach (LogWindow logWindow in logWindowList)
                            {
                                if (logWindow.CurrentColumnizer.GetType() == form.SelectedColumnizer.GetType())
                                {
                                    logWindow.ColumnizerConfigChanged();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void goToLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentLogWindow == null)
            {
                return;
            }
            GotoLineDialog dlg = new GotoLineDialog(this);
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                int line = dlg.Line - 1;
                if (line >= 0)
                {
                    CurrentLogWindow.GotoLine(line);
                }
            }
        }

        private void hilightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowHighlightSettingsDialog();
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSearchDialog();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog();
        }

        private void LogTabWindow_DragEnter(object sender, DragEventArgs e)
        {
#if DEBUG
            string[] formats = e.Data.GetFormats();
            string s = "Dragging something over LogExpert. Formats:  ";
            foreach (string format in formats)
            {
                s += format;
                s += " , ";
            }
            s = s.Substring(0, s.Length - 3);
            _logger.Info(s);
#endif
        }

        private void LogWindow_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void LogWindow_DragDrop(object sender, DragEventArgs e)
        {
#if DEBUG
            string[] formats = e.Data.GetFormats();
            string s = "Dropped formats:  ";
            foreach (string format in formats)
            {
                s += format;
                s += " , ";
            }
            s = s.Substring(0, s.Length - 3);
            _logger.Debug(s);
#endif

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                object o = e.Data.GetData(DataFormats.FileDrop);
                string[] names = o as string[];
                if (names != null)
                {
                    LoadFiles(names, (e.KeyState & 4) == 4); // (shift pressed?)
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void timeshiftToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (!skipEvents && CurrentLogWindow != null)
            {
                CurrentLogWindow.SetTimeshiftValue(timeshiftMenuTextBox.Text);
                timeshiftMenuTextBox.Enabled = timeshiftToolStripMenuItem.Checked;
                CurrentLogWindow.TimeshiftEnabled(timeshiftToolStripMenuItem.Checked,
                    timeshiftMenuTextBox.Text);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.TopMost = TopMost;
            aboutBox.ShowDialog();
        }

        private void filterToggleButton_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ToggleFilterPanel();
        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ToggleFilterPanel();
        }

        private void multiFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMultiFile();
            toolStripMenuItem1.HideDropDown();
        }

        private void GuiStateUpdate(object sender, GuiStateArgs e)
        {
            BeginInvoke(new GuiStateUpdateWorkerDelegate(GuiStateUpdateWorker), e);
        }

        private void ColumnizerChanged(object sender, ColumnizerEventArgs e)
        {
            bookmarkWindow?.SetColumnizer(e.Columnizer);
        }

        private void BookmarkAdded(object sender, EventArgs e)
        {
            bookmarkWindow.UpdateView();
        }

        private void BookmarkTextChanged(object sender, BookmarkEventArgs e)
        {
            bookmarkWindow.BookmarkTextChanged(e.Bookmark);
        }

        private void BookmarkRemoved(object sender, EventArgs e)
        {
            bookmarkWindow.UpdateView();
        }

        private void ProgressBarUpdate(object sender, ProgressEventArgs e)
        {
            Invoke(new ProgressBarEventFx(ProgressBarUpdateWorker), e);
        }

        private void StatusLineEvent(object sender, StatusLineEventArgs e)
        {
            lock (statusLineLock)
            {
                lastStatusLineEvent = e;
                statusLineEventHandle.Set();
                statusLineEventWakeupHandle.Set();
            }
        }

        private void followTailCheckBox_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.FollowTailChanged(followTailCheckBox.Checked, false);
        }

        private void toolStripOpenButton_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog();
        }

        private void toolStripSearchButton_Click_1(object sender, EventArgs e)
        {
            OpenSearchDialog();
        }

        private void LogTabWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && e.Control)
            {
                CurrentLogWindow?.Close();
            }
            else if (e.KeyCode == Keys.Tab && e.Control)
            {
                SwitchTab(e.Shift);
            }
            else
            {
                CurrentLogWindow?.LogWindow_KeyDown(sender, e);
            }
        }

        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.Close();
        }

        private void cellSelectModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.SetCellSelectionMode(cellSelectModeToolStripMenuItem.Checked);
        }

        private void copyMarkedLinesIntoNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.CopyMarkedLinesToTab();
        }

        private void timeshiftMenuTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentLogWindow == null)
            {
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                CurrentLogWindow.SetTimeshiftValue(timeshiftMenuTextBox.Text);
            }
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost = alwaysOnTopToolStripMenuItem.Checked;
        }

        private void FileSizeChanged(object sender, LogEventArgs e)
        {
            if (sender.GetType().IsAssignableFrom(typeof(LogWindow)))
            {
                int diff = e.LineCount - e.PrevLineCount;
                if (diff < 0)
                {
                    return;
                }
                LogWindowData data = ((LogWindow) sender).Tag as LogWindowData;
                if (data != null)
                {
                    lock (data)
                    {
                        data.diffSum = data.diffSum + diff;
                        if (data.diffSum > DIFF_MAX)
                        {
                            data.diffSum = DIFF_MAX;
                        }
                    }

                    //if (this.dockPanel.ActiveContent != null &&
                    //    this.dockPanel.ActiveContent != sender || data.tailState != 0)
                    if (CurrentLogWindow != null &&
                        CurrentLogWindow != sender || data.tailState != 0)
                    {
                        data.dirty = true;
                    }
                    Icon icon = GetIcon(diff, data);
                    BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow) sender, icon);
                }
            }
        }

        private void logWindow_FileNotFound(object sender, EventArgs e)
        {
            Invoke(new FileNotFoundDelegate(FileNotFound), sender);
        }

        private void logWindow_FileRespawned(object sender, EventArgs e)
        {
            Invoke(new FileRespawnedDelegate(FileRespawned), sender);
        }

        private void logWindow_FilterListChanged(object sender, FilterListChangedEventArgs e)
        {
            lock (logWindowList)
            {
                foreach (LogWindow logWindow in logWindowList)
                {
                    if (logWindow != e.LogWindow)
                    {
                        logWindow.HandleChangedFilterList();
                    }
                }
            }
            ConfigManager.Save(SettingsFlags.FilterList);
        }

        private void logWindow_CurrentHighlightGroupChanged(object sender, CurrentHighlightGroupChangedEventArgs e)
        {
            OnHighlightSettingsChanged();
            ConfigManager.Settings.hilightGroupList = HilightGroupList;
            ConfigManager.Save(SettingsFlags.HighlightSettings);
        }

        private void TailFollowed(object sender, EventArgs e)
        {
            if (dockPanel.ActiveContent == null)
            {
                return;
            }
            if (sender.GetType().IsAssignableFrom(typeof(LogWindow)))
            {
                if (dockPanel.ActiveContent == sender)
                {
                    LogWindowData data = ((LogWindow) sender).Tag as LogWindowData;
                    data.dirty = false;
                    Icon icon = GetIcon(data.diffSum, data);
                    BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow) sender, icon);
                }
            }
        }

        private void logWindow_SyncModeChanged(object sender, SyncModeEventArgs e)
        {
            if (!Disposing)
            {
                LogWindowData data = ((LogWindow) sender).Tag as LogWindowData;
                data.syncMode = e.IsTimeSynced ? 1 : 0;
                Icon icon = GetIcon(data.diffSum, data);
                BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow) sender, icon);
            }
            else
            {
                _logger.Warn("Received SyncModeChanged event while disposing. Event ignored.");
            }
        }

        private void toggleBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ToggleBookmark();
        }

        private void jumpToNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.JumpNextBookmark();
        }

        private void jumpToPrevToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.JumpPrevBookmark();
        }

        private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ChangeEncoding(Encoding.ASCII);
        }

        private void aNSIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ChangeEncoding(Encoding.Default);
        }

        private void uTF8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ChangeEncoding(new UTF8Encoding(false));
        }

        private void uTF16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ChangeEncoding(Encoding.Unicode);
        }

        private void iSO88591ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ChangeEncoding(Encoding.GetEncoding("iso-8859-1"));
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentLogWindow != null)
            {
                LogWindowData data = CurrentLogWindow.Tag as LogWindowData;
                Icon icon = GetIcon(0, data);
                BeginInvoke(new SetTabIconDelegate(SetTabIcon), CurrentLogWindow, icon);
                CurrentLogWindow.Reload();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettings(0);
        }

        private void dateTimeDragControl_ValueDragged(object sender, EventArgs e)
        {
            if (CurrentLogWindow != null)
            {
                //this.CurrentLogWindow.ScrollToTimestamp(this.dateTimeDragControl.DateTime);
            }
        }

        private void dateTimeDragControl_ValueChanged(object sender, EventArgs e)
        {
            CurrentLogWindow?.ScrollToTimestamp(dateTimeDragControl.DateTime, true, true);
        }

        private void LogTabWindow_Deactivate(object sender, EventArgs e)
        {
            CurrentLogWindow?.AppFocusLost();
        }

        private void LogTabWindow_Activated(object sender, EventArgs e)
        {
            CurrentLogWindow?.AppFocusGained();
        }

        private void toolStripButtonA_Click(object sender, EventArgs e)
        {
            ToolButtonClick(ConfigManager.Settings.preferences.toolEntries[0]);
        }

        private void toolStripButtonB_Click(object sender, EventArgs e)
        {
            ToolButtonClick(ConfigManager.Settings.preferences.toolEntries[1]);
        }

        private void toolStripButtonC_Click(object sender, EventArgs e)
        {
            ToolButtonClick(ConfigManager.Settings.preferences.toolEntries[2]);
        }

        private void showBookmarkListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bookmarkWindow.Visible)
            {
                bookmarkWindow.Hide();
            }
            else
            {
                // strange: on very first Show() now bookmarks are displayed. after a hide it will work.
                if (firstBookmarkWindowShow)
                {
                    bookmarkWindow.Show(dockPanel);
                    bookmarkWindow.Hide();
                }

                bookmarkWindow.Show(dockPanel);
            }
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog();
        }

        private void toolStripButtonSearch_Click(object sender, EventArgs e)
        {
            OpenSearchDialog();
        }

        private void toolStripButtonFilter_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ToggleFilterPanel();
        }

        private void toolStripButtonBookmark_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ToggleBookmark();
        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.JumpPrevBookmark();
        }

        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.JumpNextBookmark();
        }

        private void showHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "LogExpert.chm");
        }

        private void hideLineColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigManager.Settings.hideLineColumn = hideLineColumnToolStripMenuItem.Checked;
            lock (logWindowList)
            {
                foreach (LogWindow logWin in logWindowList)
                {
                    logWin.ShowLineColumn(!ConfigManager.Settings.hideLineColumn);
                }
            }
            bookmarkWindow.LineColumnVisible = ConfigManager.Settings.hideLineColumn;
        }

        // ==================================================================
        // Tab context menu stuff
        // ==================================================================

        private void closeThisTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (dockPanel.ActiveContent as LogWindow).Close();
        }

        private void closeOtherTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<Form> closeList = new List<Form>();
            lock (logWindowList)
            {
                foreach (DockContent content in dockPanel.Contents)
                {
                    if (content != dockPanel.ActiveContent && content is LogWindow)
                    {
                        closeList.Add(content as Form);
                    }
                }
            }
            foreach (Form form in closeList)
            {
                form.Close();
            }
        }

        private void closeAllTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseAllTabs();
        }

        private void tabColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogWindow logWindow = dockPanel.ActiveContent as LogWindow;

            LogWindowData data = logWindow.Tag as LogWindowData;
            if (data == null)
            {
                return;
            }

            ColorDialog dlg = new ColorDialog();
            dlg.Color = data.color;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                data.color = dlg.Color;
                setTabColor(logWindow, data.color);
            }
            List<ColorEntry> delList = new List<ColorEntry>();
            foreach (ColorEntry entry in ConfigManager.Settings.fileColors)
            {
                if (entry.fileName.ToLower().Equals(logWindow.FileName.ToLower()))
                {
                    delList.Add(entry);
                }
            }
            foreach (ColorEntry entry in delList)
            {
                ConfigManager.Settings.fileColors.Remove(entry);
            }
            ConfigManager.Settings.fileColors.Add(new ColorEntry(logWindow.FileName, dlg.Color));
            while (ConfigManager.Settings.fileColors.Count > MAX_COLOR_HISTORY)
            {
                ConfigManager.Settings.fileColors.RemoveAt(0);
            }
        }

        private void LogTabWindow_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                wasMaximized = WindowState == FormWindowState.Maximized;
            }
        }

        private void patternStatisticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.PatternStatistic();
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = "lxj";
            dlg.Filter = "LogExpert session (*.lxj)|*.lxj";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string fileName = dlg.FileName;
                List<string> fileNames = new List<string>();

                lock (logWindowList)
                {
                    foreach (DockContent content in dockPanel.Contents)
                    {
                        LogWindow logWindow = content as LogWindow;
                        string persistenceFileName = logWindow?.SavePersistenceData(true);
                        if (persistenceFileName != null)
                        {
                            fileNames.Add(persistenceFileName);
                        }
                    }
                }
                ProjectData projectData = new ProjectData();
                projectData.memberList = fileNames;
                projectData.tabLayoutXml = SaveLayout();
                ProjectPersister.SaveProjectData(fileName, projectData);
            }
        }

        private void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "lxj";
            dlg.Filter = "LogExpert sessions (*.lxj)|*.lxj";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string projectFileName = dlg.FileName;
                LoadProject(projectFileName, true);
            }
        }

        private void toolStripButtonBubbles_Click(object sender, EventArgs e)
        {
            if (CurrentLogWindow != null)
            {
                CurrentLogWindow.ShowBookmarkBubbles = toolStripButtonBubbles.Checked;
            }
        }

        private void copyPathToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogWindow logWindow = dockPanel.ActiveContent as LogWindow;
            Clipboard.SetText(logWindow.Title);
        }

        private void findInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogWindow logWindow = dockPanel.ActiveContent as LogWindow;

            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = "/e,/select," + logWindow.Title;
            explorer.StartInfo.UseShellExecute = false;
            explorer.Start();
        }

        private void exportBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ExportBookmarkList();
        }

        private void importBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ImportBookmarkList();
        }

        private void highlightGroupsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ApplySelectedHighlightGroup();
        }

        private void highlightGroupsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySelectedHighlightGroup();
        }

        private void highlightGroupsComboBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowHighlightSettingsDialog();
            }
        }

        private void ConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            if (LogExpertProxy != null)
            {
                NotifySettingsChanged(null, e.Flags);
            }
        }

        private void dumpLogBufferInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            CurrentLogWindow?.DumpBufferInfo();
#endif
        }

        private void dumpBufferDiagnosticToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            CurrentLogWindow?.DumpBufferDiagnostic();
#endif
        }

        private void runGCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runGC();
        }

        private void gCInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dumpGCInfo();
        }

        private void toolsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag is ToolEntry)
            {
                ToolButtonClick(e.ClickedItem.Tag as ToolEntry);
            }
        }

        private void externalToolsToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolButtonClick(e.ClickedItem.Tag as ToolEntry);
        }

        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettings(2);
        }

        private void throwExceptionGUIThreadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new Exception("This is a test exception thrown by the GUI thread");
        }

        private void throwExceptionbackgroundThToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExceptionFx fx = throwExceptionFx;
            fx.BeginInvoke(null, null);
        }

        private void throwExceptionbackgroundThreadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(throwExceptionThreadFx);
            thread.IsBackground = true;
            thread.Start();
        }

        private void warnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_logger.GetLogger().LogLevel = _logger.Level.WARN;
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_logger.Get_logger().LogLevel = _logger.Level.INFO;
        }

        private void debugToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //_logger.Get_logger().LogLevel = _logger.Level.DEBUG;
        }

        private void loglevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void loglevelToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            //warnToolStripMenuItem.Checked = _logger.Get_logger().LogLevel == _logger.Level.WARN;
            //infoToolStripMenuItem.Checked = _logger.Get_logger().LogLevel == _logger.Level.INFO;
            //debugToolStripMenuItem1.Checked = _logger.Get_logger().LogLevel == _logger.Level.DEBUG;
        }

        private void disableWordHighlightModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugOptions.disableWordHighlight = disableWordHighlightModeToolStripMenuItem.Checked;
            CurrentLogWindow?.RefreshAllGrids();
        }

        private void multifileMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLogWindow?.ChangeMultifileMask();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ToggleMultiFile();
        }

        private void lockInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lockInstanceToolStripMenuItem.Checked)
            {
                StaticData.CurrentLockedMainWindow = null;
            }
            else
            {
                StaticData.CurrentLockedMainWindow = this;
            }
        }

        private void optionToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            lockInstanceToolStripMenuItem.Enabled = !ConfigManager.Settings.preferences.allowOnlyOneInstance;
            lockInstanceToolStripMenuItem.Checked = StaticData.CurrentLockedMainWindow == this;
        }

        private void toolStripMenuItem1_DropDownOpening(object sender, EventArgs e)
        {
            newFromClipboardToolStripMenuItem.Enabled = Clipboard.ContainsText();
        }

        private void newFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard();
        }

        private void openURIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenUriDialog dlg = new OpenUriDialog();
            dlg.UriHistory = ConfigManager.Settings.uriHistoryList;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                if (dlg.Uri.Trim().Length > 0)
                {
                    ConfigManager.Settings.uriHistoryList = dlg.UriHistory;
                    ConfigManager.Save(SettingsFlags.FileHistory);
                    LoadFiles(new string[] {dlg.Uri}, false);
                }
            }
        }

        private void columnFinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentLogWindow != null && !skipEvents)
            {
                CurrentLogWindow.ToggleColumnFinder(columnFinderToolStripMenuItem.Checked, true);
            }
        }

        private void dockPanel_ActiveContentChanged(object sender, EventArgs e)
        {
            if (dockPanel.ActiveContent is LogWindow)
            {
                CurrentLogWindow = dockPanel.ActiveContent as LogWindow;
                CurrentLogWindow.LogWindowActivated();
                ConnectToolWindows(CurrentLogWindow);
            }
        }

        private void tabRenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentLogWindow != null)
            {
                TabRenameDlg dlg = new TabRenameDlg();
                dlg.TabName = CurrentLogWindow.Text;
                if (DialogResult.OK == dlg.ShowDialog())
                {
                    CurrentLogWindow.Text = dlg.TabName;
                }
                dlg.Dispose();
            }
        }

        #endregion
    }
}