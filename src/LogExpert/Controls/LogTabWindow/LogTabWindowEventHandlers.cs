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
            this.firstBookmarkWindowShow = false;
        }

        private void LogTabWindow_Load(object sender, EventArgs e)
        {
            ApplySettings(ConfigManager.Settings, SettingsFlags.All);
            if (ConfigManager.Settings.isMaximized)
            {
                Rectangle tmpBounds = this.Bounds;
                if (ConfigManager.Settings.appBoundsFullscreen != null)
                {
                    this.Bounds = ConfigManager.Settings.appBoundsFullscreen;
                }
                this.WindowState = FormWindowState.Maximized;
                this.Bounds = ConfigManager.Settings.appBounds;
            }
            else
            {
                if (ConfigManager.Settings.appBounds != null && ConfigManager.Settings.appBounds.Right > 0)
                {
                    this.Bounds = ConfigManager.Settings.appBounds;
                }
            }

            if (ConfigManager.Settings.preferences.openLastFiles && this.startupFileNames == null)
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
            if (this.startupFileNames != null)
            {
                LoadFiles(this.startupFileNames, false);
            }
            this.ledThread = new Thread(new ThreadStart(this.LedThreadProc));
            this.ledThread.IsBackground = true;
            this.ledThread.Start();

            this.statusLineThread = new Thread(new ThreadStart(this.StatusLineThreadFunc));
            this.statusLineThread.IsBackground = true;
            this.statusLineThread.Start();

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
                this.shouldStop = true;
                this.statusLineEventHandle.Set();
                this.statusLineEventWakeupHandle.Set();
                this.ledThread.Join();
                this.statusLineThread.Join();

                IList<LogWindow> deleteLogWindowList = new List<LogWindow>();
                ConfigManager.Settings.alwaysOnTop =
                    this.TopMost && ConfigManager.Settings.preferences.allowOnlyOneInstance;
                SaveLastOpenFilesList();
                foreach (LogWindow logWindow in this.logWindowList)
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
                if (this.LogExpertProxy != null)
                {
                    this.LogExpertProxy.WindowClosed(this);
                }
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
            if (sender == this.CurrentLogWindow)
            {
                ChangeCurrentLogWindow(null);
            }
            RemoveLogWindow(logWindow);
            logWindow.Tag = null;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void selectFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow == null)
            {
                return;
            }
            this.CurrentLogWindow.ColumnizerCallbackObject.LineNum = this.CurrentLogWindow.GetCurrentLineNum();
            FilterSelectorForm form = new FilterSelectorForm(PluginRegistry.GetInstance().RegisteredColumnizers,
                this.CurrentLogWindow.CurrentColumnizer,
                this.CurrentLogWindow.ColumnizerCallbackObject);
            form.Owner = this;
            form.TopMost = this.TopMost;
            DialogResult res = form.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (form.ApplyToAll)
                {
                    lock (this.logWindowList)
                    {
                        foreach (LogWindow logWindow in this.logWindowList)
                        {
                            if (!logWindow.CurrentColumnizer.GetType().Equals(form.SelectedColumnizer.GetType()))
                            {
                                //logWindow.SetColumnizer(form.SelectedColumnizer);
                                SetColumnizerFx fx = new SetColumnizerFx(logWindow.ForceColumnizer);
                                logWindow.Invoke(fx, new object[] {form.SelectedColumnizer});
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
                    if (!this.CurrentLogWindow.CurrentColumnizer.GetType().Equals(form.SelectedColumnizer.GetType()))
                    {
                        SetColumnizerFx fx = new SetColumnizerFx(this.CurrentLogWindow.ForceColumnizer);
                        this.CurrentLogWindow.Invoke(fx, new object[] {form.SelectedColumnizer});
                        setColumnizerHistoryEntry(this.CurrentLogWindow.FileName, form.SelectedColumnizer);
                    }
                    if (form.IsConfigPressed)
                    {
                        lock (this.logWindowList)
                        {
                            foreach (LogWindow logWindow in this.logWindowList)
                            {
                                if (logWindow.CurrentColumnizer.GetType().Equals(form.SelectedColumnizer.GetType()))
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
            if (this.CurrentLogWindow == null)
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
                    this.CurrentLogWindow.GotoLine(line);
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
            if (!this.skipEvents && this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.SetTimeshiftValue(this.timeshiftMenuTextBox.Text);
                this.timeshiftMenuTextBox.Enabled = this.timeshiftToolStripMenuItem.Checked;
                this.CurrentLogWindow.TimeshiftEnabled(this.timeshiftToolStripMenuItem.Checked,
                    this.timeshiftMenuTextBox.Text);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.TopMost = this.TopMost;
            aboutBox.ShowDialog();
        }

        private void filterToggleButton_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ToggleFilterPanel();
            }
        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ToggleFilterPanel();
            }
        }

        private void multiFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMultiFile();
            toolStripMenuItem1.HideDropDown();
        }

        private void GuiStateUpdate(object sender, GuiStateArgs e)
        {
            this.BeginInvoke(new GuiStateUpdateWorkerDelegate(GuiStateUpdateWorker), new object[] {e});
        }

        private void ColumnizerChanged(object sender, ColumnizerEventArgs e)
        {
            if (this.bookmarkWindow != null)
            {
                this.bookmarkWindow.SetColumnizer(e.Columnizer);
            }
        }

        private void BookmarkAdded(object sender, EventArgs e)
        {
            this.bookmarkWindow.UpdateView();
        }

        private void BookmarkTextChanged(object sender, BookmarkEventArgs e)
        {
            this.bookmarkWindow.BookmarkTextChanged(e.Bookmark);
        }

        private void BookmarkRemoved(object sender, EventArgs e)
        {
            this.bookmarkWindow.UpdateView();
        }

        private void ProgressBarUpdate(object sender, ProgressEventArgs e)
        {
            this.Invoke(new ProgressBarEventFx(ProgressBarUpdateWorker), new object[] {e});
        }

        private void StatusLineEvent(object sender, StatusLineEventArgs e)
        {
            lock (this.statusLineLock)
            {
                this.lastStatusLineEvent = e;
                this.statusLineEventHandle.Set();
                this.statusLineEventWakeupHandle.Set();
            }
        }

        private void followTailCheckBox_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.FollowTailChanged(this.followTailCheckBox.Checked, false);
            }
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
                if (this.CurrentLogWindow != null)
                {
                    this.CurrentLogWindow.Close();
                }
            }
            else if (e.KeyCode == Keys.Tab && e.Control)
            {
                SwitchTab(e.Shift);
            }
            else
            {
                if (this.CurrentLogWindow != null)
                {
                    this.CurrentLogWindow.LogWindow_KeyDown(sender, e);
                }
            }
        }

        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.Close();
            }
        }

        private void cellSelectModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.SetCellSelectionMode(this.cellSelectModeToolStripMenuItem.Checked);
            }
        }

        private void copyMarkedLinesIntoNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.CopyMarkedLinesToTab();
            }
        }

        private void timeshiftMenuTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.CurrentLogWindow == null)
            {
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                this.CurrentLogWindow.SetTimeshiftValue(this.timeshiftMenuTextBox.Text);
            }
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TopMost = this.alwaysOnTopToolStripMenuItem.Checked;
        }

        private void FileSizeChanged(object sender, LogEventArgs e)
        {
            if (sender.GetType().IsAssignableFrom(typeof(LogWindow)))
            {
                int diff = e.LineCount - e.PrevLineCount;
                if (diff < 0)
                {
                    diff = DIFF_MAX;
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
                    if (this.CurrentLogWindow != null &&
                        this.CurrentLogWindow != sender || data.tailState != 0)
                    {
                        data.dirty = true;
                    }
                    Icon icon = GetIcon(diff, data);
                    this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] {(LogWindow) sender, icon});
                }
            }
        }

        private void logWindow_FileNotFound(object sender, EventArgs e)
        {
            this.Invoke(new FileNotFoundDelegate(FileNotFound), new object[] {sender});
        }

        private void logWindow_FileRespawned(object sender, EventArgs e)
        {
            this.Invoke(new FileRespawnedDelegate(FileRespawned), new object[] {sender});
        }

        private void logWindow_FilterListChanged(object sender, FilterListChangedEventArgs e)
        {
            lock (this.logWindowList)
            {
                foreach (LogWindow logWindow in this.logWindowList)
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
            ConfigManager.Settings.hilightGroupList = this.HilightGroupList;
            ConfigManager.Save(SettingsFlags.HighlightSettings);
        }

        private void TailFollowed(object sender, EventArgs e)
        {
            if (this.dockPanel.ActiveContent == null)
            {
                return;
            }
            if (sender.GetType().IsAssignableFrom(typeof(LogWindow)))
            {
                if (this.dockPanel.ActiveContent == sender)
                {
                    LogWindowData data = ((LogWindow) sender).Tag as LogWindowData;
                    data.dirty = false;
                    Icon icon = GetIcon(data.diffSum, data);
                    this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] {(LogWindow) sender, icon});
                }
            }
        }

        private void logWindow_SyncModeChanged(object sender, SyncModeEventArgs e)
        {
            if (!this.Disposing)
            {
                LogWindowData data = ((LogWindow) sender).Tag as LogWindowData;
                data.syncMode = e.IsTimeSynced ? 1 : 0;
                Icon icon = GetIcon(data.diffSum, data);
                this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] {(LogWindow) sender, icon});
            }
            else
            {
                _logger.Warn("Received SyncModeChanged event while disposing. Event ignored.");
            }
        }

        private void toggleBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ToggleBookmark();
            }
        }

        private void jumpToNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.JumpNextBookmark();
            }
        }

        private void jumpToPrevToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.JumpPrevBookmark();
            }
        }

        private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ChangeEncoding(Encoding.ASCII);
            }
        }

        private void aNSIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ChangeEncoding(Encoding.Default);
            }
        }

        private void uTF8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ChangeEncoding(new UTF8Encoding(false));
            }
        }

        private void uTF16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ChangeEncoding(Encoding.Unicode);
            }
        }

        private void iSO88591ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ChangeEncoding(Encoding.GetEncoding("iso-8859-1"));
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                LogWindowData data = this.CurrentLogWindow.Tag as LogWindowData;
                Icon icon = GetIcon(0, data);
                this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] {this.CurrentLogWindow, icon});
                this.CurrentLogWindow.Reload();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettings(0);
        }

        private void dateTimeDragControl_ValueDragged(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                //this.CurrentLogWindow.ScrollToTimestamp(this.dateTimeDragControl.DateTime);
            }
        }

        private void dateTimeDragControl_ValueChanged(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ScrollToTimestamp(this.dateTimeDragControl.DateTime, true, true);
            }
        }

        private void LogTabWindow_Deactivate(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.AppFocusLost();
            }
        }

        private void LogTabWindow_Activated(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.AppFocusGained();
            }
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
            if (this.bookmarkWindow.Visible)
            {
                this.bookmarkWindow.Hide();
            }
            else
            {
                // strange: on very first Show() now bookmarks are displayed. after a hide it will work.
                if (this.firstBookmarkWindowShow)
                {
                    this.bookmarkWindow.Show(this.dockPanel);
                    this.bookmarkWindow.Hide();
                }

                this.bookmarkWindow.Show(this.dockPanel);
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
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ToggleFilterPanel();
            }
        }

        private void toolStripButtonBookmark_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ToggleBookmark();
            }
        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.JumpPrevBookmark();
            }
        }

        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.JumpNextBookmark();
            }
        }

        private void showHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "LogExpert.chm");
        }

        private void hideLineColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigManager.Settings.hideLineColumn = this.hideLineColumnToolStripMenuItem.Checked;
            lock (this.logWindowList)
            {
                foreach (LogWindow logWin in this.logWindowList)
                {
                    logWin.ShowLineColumn(!ConfigManager.Settings.hideLineColumn);
                }
            }
            this.bookmarkWindow.LineColumnVisible = ConfigManager.Settings.hideLineColumn;
        }

        // ==================================================================
        // Tab context menu stuff
        // ==================================================================

        private void closeThisTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (this.dockPanel.ActiveContent as LogWindow).Close();
        }

        private void closeOtherTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<Form> closeList = new List<Form>();
            lock (this.logWindowList)
            {
                foreach (DockContent content in this.dockPanel.Contents)
                {
                    if (content != this.dockPanel.ActiveContent && content is LogWindow)
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
            LogWindow logWindow = this.dockPanel.ActiveContent as LogWindow;

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
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.wasMaximized = this.WindowState == FormWindowState.Maximized;
            }
        }

        private void patternStatisticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.PatternStatistic();
            }
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

                lock (this.logWindowList)
                {
                    foreach (DockContent content in this.dockPanel.Contents)
                    {
                        LogWindow logWindow = content as LogWindow;
                        if (logWindow != null)
                        {
                            string persistenceFileName = logWindow.SavePersistenceData(true);
                            if (persistenceFileName != null)
                            {
                                fileNames.Add(persistenceFileName);
                            }
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
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ShowBookmarkBubbles = this.toolStripButtonBubbles.Checked;
            }
        }

        private void copyPathToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogWindow logWindow = this.dockPanel.ActiveContent as LogWindow;
            Clipboard.SetText(logWindow.Title);
        }

        private void findInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogWindow logWindow = this.dockPanel.ActiveContent as LogWindow;

            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = "/e,/select," + logWindow.Title;
            explorer.StartInfo.UseShellExecute = false;
            explorer.Start();
        }

        private void exportBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ExportBookmarkList();
            }
        }

        private void importBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ImportBookmarkList();
            }
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
            if (this.LogExpertProxy != null)
            {
                this.NotifySettingsChanged(null, e.Flags);
            }
        }

        private void dumpLogBufferInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.DumpBufferInfo();
            }
#endif
        }

        private void dumpBufferDiagnosticToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.DumpBufferDiagnostic();
            }
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
            ExceptionFx fx = new ExceptionFx(throwExceptionFx);
            fx.BeginInvoke(null, null);
        }

        private void throwExceptionbackgroundThreadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(throwExceptionThreadFx));
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
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.RefreshAllGrids();
            }
        }

        private void multifileMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.ChangeMultifileMask();
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ToggleMultiFile();
        }

        private void lockInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lockInstanceToolStripMenuItem.Checked)
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
            this.lockInstanceToolStripMenuItem.Enabled = !ConfigManager.Settings.preferences.allowOnlyOneInstance;
            this.lockInstanceToolStripMenuItem.Checked = StaticData.CurrentLockedMainWindow == this;
        }

        private void toolStripMenuItem1_DropDownOpening(object sender, EventArgs e)
        {
            this.newFromClipboardToolStripMenuItem.Enabled = Clipboard.ContainsText();
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
            if (this.CurrentLogWindow != null && !skipEvents)
            {
                this.CurrentLogWindow.ToggleColumnFinder(this.columnFinderToolStripMenuItem.Checked, true);
            }
        }

        private void dockPanel_ActiveContentChanged(object sender, EventArgs e)
        {
            if (this.dockPanel.ActiveContent is LogWindow)
            {
                this.CurrentLogWindow = this.dockPanel.ActiveContent as LogWindow;
                this.CurrentLogWindow.LogWindowActivated();
                ConnectToolWindows(this.CurrentLogWindow);
            }
        }

        private void tabRenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentLogWindow != null)
            {
                TabRenameDlg dlg = new TabRenameDlg();
                dlg.TabName = this.CurrentLogWindow.Text;
                if (DialogResult.OK == dlg.ShowDialog())
                {
                    this.CurrentLogWindow.Text = dlg.TabName;
                }
                dlg.Dispose();
            }
        }

        #endregion
    }
}