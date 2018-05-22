using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LogExpert.Dialogs;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
    public partial class LogTabWindow
    {
        #region Private Methods

        /// <summary>
        /// Creates a temp file with the text content of the clipboard and opens the temp file in a new tab.
        /// </summary>
        private void PasteFromClipboard()
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                string fileName = Path.GetTempFileName();

                using (FileStream fStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(fStream, Encoding.Unicode))
                {
                    writer.Write(text);
                    writer.Close();
                }

                string title = "Clipboard";
                LogWindow logWindow = AddTempFileTab(fileName, title);
                LogWindowData data = logWindow.Tag as LogWindowData;
                if (data != null)
                {
                    SetTooltipText(logWindow, "Pasted on " + DateTime.Now);
                }
            }
        }

        private void InitToolWindows()
        {
            InitBookmarkWindow();
        }

        private void DestroyToolWindows()
        {
            DestroyBookmarkWindow();
        }

        private void InitBookmarkWindow()
        {
            bookmarkWindow = new BookmarkWindow();
            bookmarkWindow.HideOnClose = true;
            bookmarkWindow.ShowHint = DockState.DockBottom;
            bookmarkWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, SettingsFlags.All);
            bookmarkWindow.VisibleChanged += bookmarkWindow_VisibleChanged;
            firstBookmarkWindowShow = true;
        }

        private void DestroyBookmarkWindow()
        {
            bookmarkWindow.HideOnClose = false;
            bookmarkWindow.Close();
        }

        private void SaveLastOpenFilesList()
        {
            ConfigManager.Settings.lastOpenFilesList.Clear();
            foreach (DockContent content in dockPanel.Contents)
            {
                if (content is LogWindow)
                {
                    LogWindow logWin = content as LogWindow;
                    if (!logWin.IsTempFile)
                    {
                        ConfigManager.Settings.lastOpenFilesList.Add(logWin.GivenFileName);
                    }
                }
            }
        }

        private void SaveWindowPosition()
        {
            SuspendLayout();
            if (WindowState == FormWindowState.Normal)
            {
                ConfigManager.Settings.appBounds = Bounds;
                ConfigManager.Settings.isMaximized = false;
            }
            else
            {
                ConfigManager.Settings.appBoundsFullscreen = Bounds;
                ConfigManager.Settings.isMaximized = true;
                WindowState = FormWindowState.Normal;
                ConfigManager.Settings.appBounds = Bounds;
            }

            ResumeLayout();
        }

        private void SetTooltipText(LogWindow logWindow, string logFileName)
        {
            logWindow.ToolTipText = logFileName;
        }

        private void FillDefaultEncodingFromSettings(EncodingOptions encodingOptions)
        {
            if (ConfigManager.Settings.preferences.defaultEncoding != null)
            {
                try
                {
                    encodingOptions.DefaultEncoding =
                        Encoding.GetEncoding(ConfigManager.Settings.preferences.defaultEncoding);
                }
                catch (ArgumentException)
                {
                    _logger.Warn("Encoding " + ConfigManager.Settings.preferences.defaultEncoding +
                                 " is not a valid encoding");
                    encodingOptions.DefaultEncoding = null;
                }
            }
        }

        private void AddFileTabs(string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (fileName != null && fileName.Length > 0)
                {
                    if (fileName.EndsWith(".lxj"))
                    {
                        LoadProject(fileName, false);
                    }
                    else
                    {
                        AddFileTab(fileName, false, null, false, null);
                    }
                }
            }

            Activate();
        }

        private void AddLogWindow(LogWindow logWindow, string title, bool doNotAddToPanel)
        {
            logWindow.CloseButton = true;
            logWindow.TabPageContextMenuStrip = tabContextMenuStrip;
            SetTooltipText(logWindow, title);
            logWindow.DockAreas = DockAreas.Document | DockAreas.Float;

            if (!doNotAddToPanel)
            {
                logWindow.Show(dockPanel);
            }

            LogWindowData data = new LogWindowData();
            data.diffSum = 0;
            logWindow.Tag = data;
            lock (logWindowList)
            {
                logWindowList.Add(logWindow);
            }

            logWindow.FileSizeChanged += FileSizeChanged;
            logWindow.TailFollowed += TailFollowed;
            logWindow.Disposed += logWindow_Disposed;
            logWindow.FileNotFound += logWindow_FileNotFound;
            logWindow.FileRespawned += logWindow_FileRespawned;
            logWindow.FilterListChanged += logWindow_FilterListChanged;
            logWindow.CurrentHighlightGroupChanged += logWindow_CurrentHighlightGroupChanged;
            logWindow.SyncModeChanged += logWindow_SyncModeChanged;

            logWindow.Visible = true;
        }

        private void DisconnectEventHandlers(LogWindow logWindow)
        {
            logWindow.FileSizeChanged -= FileSizeChanged;
            logWindow.TailFollowed -= TailFollowed;
            logWindow.Disposed -= logWindow_Disposed;
            logWindow.FileNotFound -= logWindow_FileNotFound;
            logWindow.FileRespawned -= logWindow_FileRespawned;
            logWindow.FilterListChanged -= logWindow_FilterListChanged;
            logWindow.CurrentHighlightGroupChanged -= logWindow_CurrentHighlightGroupChanged;
            logWindow.SyncModeChanged -= logWindow_SyncModeChanged;

            LogWindowData data = logWindow.Tag as LogWindowData;
            //data.tabPage.MouseClick -= tabPage_MouseClick;
            //data.tabPage.TabDoubleClick -= tabPage_TabDoubleClick;
            //data.tabPage.ContextMenuStrip = null;
            //data.tabPage = null;
        }

        private void AddToFileHistory(string fileName)
        {
            Predicate<string> findName = delegate(string s) { return s.ToLower().Equals(fileName.ToLower()); };
            int index = ConfigManager.Settings.fileHistoryList.FindIndex(findName);
            if (index != -1)
            {
                ConfigManager.Settings.fileHistoryList.RemoveAt(index);
            }

            ConfigManager.Settings.fileHistoryList.Insert(0, fileName);
            while (ConfigManager.Settings.fileHistoryList.Count > MAX_FILE_HISTORY)
            {
                ConfigManager.Settings.fileHistoryList.RemoveAt(ConfigManager.Settings.fileHistoryList.Count - 1);
            }

            ConfigManager.Save(SettingsFlags.FileHistory);

            FillHistoryMenu();
        }

        private LogWindow FindWindowForFile(string fileName)
        {
            lock (logWindowList)
            {
                foreach (LogWindow logWindow in logWindowList)
                {
                    if (logWindow.FileName.ToLower().Equals(fileName.ToLower()))
                    {
                        return logWindow;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the file name is a settings file. If so, the contained logfile name
        /// is returned. If not, the given file name is returned unchanged.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string FindFilenameForSettings(string fileName)
        {
            if (fileName.EndsWith(".lxp"))
            {
                PersistenceData persistenceData = Persister.LoadOptionsOnly(fileName);
                if (persistenceData == null)
                {
                    return fileName;
                }

                if (persistenceData.fileName != null && persistenceData.fileName.Length > 0)
                {
                    IFileSystemPlugin fs = PluginRegistry.GetInstance().FindFileSystemForUri(persistenceData.fileName);
                    if (fs != null && !fs.GetType().Equals(typeof(LocalFileSystem)))
                    {
                        return persistenceData.fileName;
                    }

                    // On relative paths the URI check (and therefore the file system plugin check) will fail.
                    // So fs == null and fs == LocalFileSystem are handled here like normal files.
                    if (Path.IsPathRooted(persistenceData.fileName))
                    {
                        return persistenceData.fileName;
                    }
                    else
                    {
                        // handle relative paths in .lxp files
                        string dir = Path.GetDirectoryName(fileName);
                        return Path.Combine(dir, persistenceData.fileName);
                    }
                }
            }

            return fileName;
        }

        private void FillHistoryMenu()
        {
            ToolStripDropDown strip = new ToolStripDropDownMenu();
            foreach (string file in ConfigManager.Settings.fileHistoryList)
            {
                ToolStripItem item = new ToolStripMenuItem(file);
                strip.Items.Add(item);
            }

            strip.ItemClicked += history_ItemClicked;
            strip.MouseUp += strip_MouseUp;
            lastUsedToolStripMenuItem.DropDown = strip;
        }

        private void RemoveLogWindow(LogWindow logWindow)
        {
            lock (logWindowList)
            {
                logWindowList.Remove(logWindow);
            }

            DisconnectEventHandlers(logWindow);
        }

        private void RemoveAndDisposeLogWindow(LogWindow logWindow, bool dontAsk)
        {
            if (CurrentLogWindow == logWindow)
            {
                ChangeCurrentLogWindow(null);
            }

            lock (logWindowList)
            {
                logWindowList.Remove(logWindow);
            }

            logWindow.Close(dontAsk);
        }

        private void ShowHighlightSettingsDialog()
        {
            HilightDialog dlg = new HilightDialog();
            dlg.KeywordActionList = PluginRegistry.GetInstance().RegisteredKeywordActions;
            dlg.Owner = this;
            dlg.TopMost = TopMost;
            dlg.HilightGroupList = HilightGroupList;
            dlg.PreSelectedGroupName = highlightGroupsComboBox.Text;
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                HilightGroupList = dlg.HilightGroupList;
                FillHighlightComboBox();
                ConfigManager.Settings.hilightGroupList = HilightGroupList;
                ConfigManager.Save(SettingsFlags.HighlightSettings);
                OnHighlightSettingsChanged();
            }
        }

        private void FillHighlightComboBox()
        {
            string currentGroupName = highlightGroupsComboBox.Text;
            highlightGroupsComboBox.Items.Clear();
            foreach (HilightGroup group in HilightGroupList)
            {
                highlightGroupsComboBox.Items.Add(group.GroupName);
                if (group.GroupName.Equals(currentGroupName))
                {
                    highlightGroupsComboBox.Text = group.GroupName;
                }
            }
        }

        private void OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (CurrentLogWindow != null)
            {
                FileInfo info = new FileInfo(CurrentLogWindow.FileName);
                openFileDialog.InitialDirectory = info.DirectoryName;
            }
            else
            {
                if (ConfigManager.Settings.lastDirectory != null && ConfigManager.Settings.lastDirectory.Length > 0)
                {
                    openFileDialog.InitialDirectory = ConfigManager.Settings.lastDirectory;
                }
                else
                {
                    try
                    {
                        openFileDialog.InitialDirectory =
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }
                    catch (SecurityException e)
                    {
                        _logger.Warn(e, "Insufficient rights for GetFolderPath(): ");
                        // no initial directory if insufficient rights
                    }
                }
            }

            openFileDialog.Multiselect = true;

            if (DialogResult.OK == openFileDialog.ShowDialog(this))
            {
                FileInfo info = new FileInfo(openFileDialog.FileName);
                if (info.Directory.Exists)
                {
                    ConfigManager.Settings.lastDirectory = info.DirectoryName;
                    ConfigManager.Save(SettingsFlags.FileHistory);
                }

                if (info.Exists)
                {
                    LoadFiles(openFileDialog.FileNames, false);
                }
            }
        }

        private void LoadFiles(string[] names, bool invertLogic)
        {
            if (names.Length == 1)
            {
                if (names[0].EndsWith(".lxj"))
                {
                    LoadProject(names[0], true);
                    return;
                }
                else
                {
                    AddFileTab(names[0], false, null, false, null);
                    return;
                }
            }

            MultiFileOption option = ConfigManager.Settings.preferences.multiFileOption;
            if (option == MultiFileOption.Ask)
            {
                MultiLoadRequestDialog dlg = new MultiLoadRequestDialog();
                DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.Yes)
                {
                    option = MultiFileOption.SingleFiles;
                }
                else if (res == DialogResult.No)
                {
                    option = MultiFileOption.MultiFile;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (invertLogic)
                {
                    if (option == MultiFileOption.SingleFiles)
                    {
                        option = MultiFileOption.MultiFile;
                    }
                    else
                    {
                        option = MultiFileOption.SingleFiles;
                    }
                }
            }

            if (option == MultiFileOption.SingleFiles)
            {
                AddFileTabs(names);
            }
            else
            {
                AddMultiFileTab(names);
            }
        }

        private void setColumnizerHistoryEntry(string fileName, ILogLineColumnizer columnizer)
        {
            ColumnizerHistoryEntry entry = findColumnizerHistoryEntry(fileName);
            if (entry != null)
            {
                ConfigManager.Settings.columnizerHistoryList.Remove(entry);
            }

            ConfigManager.Settings.columnizerHistoryList.Add(new ColumnizerHistoryEntry(fileName,
                columnizer.GetName()));
            if (ConfigManager.Settings.columnizerHistoryList.Count > MAX_COLUMNIZER_HISTORY)
            {
                ConfigManager.Settings.columnizerHistoryList.RemoveAt(0);
            }
        }

        private ColumnizerHistoryEntry findColumnizerHistoryEntry(string fileName)
        {
            foreach (ColumnizerHistoryEntry entry in ConfigManager.Settings.columnizerHistoryList)
            {
                if (entry.fileName.Equals(fileName))
                {
                    return entry;
                }
            }

            return null;
        }

        private void ToggleMultiFile()
        {
            if (CurrentLogWindow != null)
            {
                CurrentLogWindow.SwitchMultiFile(!CurrentLogWindow.IsMultiFile);
                multiFileToolStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
                multiFileEnabledStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
            }
        }

        private void ChangeCurrentLogWindow(LogWindow newLogWindow)
        {
            if (newLogWindow == currentLogWindow)
            {
                return; // do nothing if wishing to set the same window
            }

            LogWindow oldLogWindow = currentLogWindow;
            currentLogWindow = newLogWindow;
            string titleName = showInstanceNumbers ? "LogExpert #" + instanceNumber : "LogExpert";

            if (oldLogWindow != null)
            {
                oldLogWindow.StatusLineEvent -= StatusLineEvent;
                oldLogWindow.ProgressBarUpdate -= ProgressBarUpdate;
                oldLogWindow.GuiStateUpdate -= GuiStateUpdate;
                oldLogWindow.ColumnizerChanged -= ColumnizerChanged;
                oldLogWindow.BookmarkAdded -= BookmarkAdded;
                oldLogWindow.BookmarkRemoved -= BookmarkRemoved;
                oldLogWindow.BookmarkTextChanged -= BookmarkTextChanged;
                DisconnectToolWindows(oldLogWindow);
            }

            if (newLogWindow != null)
            {
                newLogWindow.StatusLineEvent += StatusLineEvent;
                newLogWindow.ProgressBarUpdate += ProgressBarUpdate;
                newLogWindow.GuiStateUpdate += GuiStateUpdate;
                newLogWindow.ColumnizerChanged += ColumnizerChanged;
                newLogWindow.BookmarkAdded += BookmarkAdded;
                newLogWindow.BookmarkRemoved += BookmarkRemoved;
                newLogWindow.BookmarkTextChanged += BookmarkTextChanged;
                if (newLogWindow.IsTempFile)
                {
                    Text = titleName + " - " + newLogWindow.TempTitleName;
                }
                else
                {
                    Text = titleName + " - " + newLogWindow.FileName;
                }

                multiFileToolStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
                multiFileToolStripMenuItem.Enabled = true;
                multiFileEnabledStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
                cellSelectModeToolStripMenuItem.Checked = true;
                cellSelectModeToolStripMenuItem.Enabled = true;
                closeFileToolStripMenuItem.Enabled = true;
                searchToolStripMenuItem.Enabled = true;
                filterToolStripMenuItem.Enabled = true;
                goToLineToolStripMenuItem.Enabled = true;
                //ConnectToolWindows(newLogWindow);
            }
            else
            {
                Text = titleName;
                multiFileToolStripMenuItem.Checked = false;
                multiFileEnabledStripMenuItem.Checked = false;
                followTailCheckBox.Checked = false;
                menuStrip1.Enabled = true;
                timeshiftToolStripMenuItem.Enabled = false;
                timeshiftToolStripMenuItem.Checked = false;
                timeshiftMenuTextBox.Text = "";
                timeshiftMenuTextBox.Enabled = false;
                multiFileToolStripMenuItem.Enabled = false;
                cellSelectModeToolStripMenuItem.Checked = false;
                cellSelectModeToolStripMenuItem.Enabled = false;
                closeFileToolStripMenuItem.Enabled = false;
                searchToolStripMenuItem.Enabled = false;
                filterToolStripMenuItem.Enabled = false;
                goToLineToolStripMenuItem.Enabled = false;
                dateTimeDragControl.Visible = false;
            }
        }

        private void ConnectToolWindows(LogWindow logWindow)
        {
            ConnectBookmarkWindow(logWindow);
        }

        private void ConnectBookmarkWindow(LogWindow logWindow)
        {
            FileViewContext ctx = new FileViewContext(logWindow, logWindow);
            bookmarkWindow.SetBookmarkData(logWindow.BookmarkData);
            bookmarkWindow.SetCurrentFile(ctx);
        }

        private void DisconnectToolWindows(LogWindow logWindow)
        {
            DisconnectBookmarkWindow(logWindow);
        }

        private void DisconnectBookmarkWindow(LogWindow logWindow)
        {
            bookmarkWindow.SetBookmarkData(null);
            bookmarkWindow.SetCurrentFile(null);
        }

        private void GuiStateUpdateWorker(GuiStateArgs e)
        {
            skipEvents = true;
            followTailCheckBox.Checked = e.FollowTail;
            menuStrip1.Enabled = e.MenuEnabled;
            timeshiftToolStripMenuItem.Enabled = e.TimeshiftPossible;
            timeshiftToolStripMenuItem.Checked = e.TimeshiftEnabled;
            timeshiftMenuTextBox.Text = e.TimeshiftText;
            timeshiftMenuTextBox.Enabled = e.TimeshiftEnabled;
            multiFileToolStripMenuItem.Enabled = e.MultiFileEnabled; // disabled for temp files
            multiFileToolStripMenuItem.Checked = e.IsMultiFileActive;
            multiFileEnabledStripMenuItem.Checked = e.IsMultiFileActive;
            cellSelectModeToolStripMenuItem.Checked = e.CellSelectMode;
            RefreshEncodingMenuBar(e.CurrentEncoding);
            if (e.TimeshiftPossible && ConfigManager.Settings.preferences.timestampControl)
            {
                dateTimeDragControl.MinDateTime = e.MinTimestamp;
                dateTimeDragControl.MaxDateTime = e.MaxTimestamp;
                dateTimeDragControl.DateTime = e.Timestamp;
                dateTimeDragControl.Visible = true;
                dateTimeDragControl.Enabled = true;
                dateTimeDragControl.Refresh();
            }
            else
            {
                dateTimeDragControl.Visible = false;
                dateTimeDragControl.Enabled = false;
            }

            toolStripButtonBubbles.Checked = e.ShowBookmarkBubbles;
            highlightGroupsComboBox.Text = e.HighlightGroupName;
            columnFinderToolStripMenuItem.Checked = e.ColumnFinderVisible;

            skipEvents = false;
        }

        private void ProgressBarUpdateWorker(ProgressEventArgs e)
        {
            if (e.Value <= e.MaxValue && e.Value >= e.MinValue)
            {
                try
                {
                    loadProgessBar.Minimum = e.MinValue;
                    loadProgessBar.Maximum = e.MaxValue;
                    loadProgessBar.Value = e.Value;
                    loadProgessBar.Visible = e.Visible;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error during ProgressBarUpdateWorker value {0}, min {1}, max {2}, visible {3}", e.Value, e.MinValue, e.MaxValue, e.Visible);
                }

                Invoke(new MethodInvoker(statusStrip1.Refresh));
            }
        }

        private void StatusLineThreadFunc()
        {
            int timeSum = 0;
            int waitTime = 30;
            while (!shouldStop)
            {
                statusLineEventWakeupHandle.WaitOne();
                statusLineEventWakeupHandle.Reset();
                if (!shouldStop)
                {
                    bool signaled = false;
                    do
                    {
                        //this.statusLineEventHandle.Reset();
                        signaled = statusLineEventHandle.WaitOne(waitTime, true);
                        timeSum += waitTime;
                    } while (signaled && timeSum < 900 && !shouldStop);

                    if (!shouldStop)
                    {
                        timeSum = 0;
                        try
                        {
                            StatusLineEventArgs e;
                            lock (statusLineLock)
                            {
                                e = lastStatusLineEvent.Clone();
                            }

                            BeginInvoke(new StatusLineEventFx(StatusLineEventWorker), e);
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }
            }
        }

        private void StatusLineEventWorker(StatusLineEventArgs e)
        {
            //_logger.logDebug("StatusLineEvent: text = " + e.StatusText);
            statusLabel.Text = e.StatusText;
            linesLabel.Text = "" + e.LineCount + " lines";
            sizeLabel.Text = Util.GetFileSizeAsText(e.FileSize);
            currentLineLabel.Text = "" + e.CurrentLineNum;
            statusStrip1.Refresh();
        }

        // tailState: 0,1,2 = on/off/off by Trigger
        // syncMode: 0 = normal (no), 1 = time synced 
        private Icon CreateLedIcon(int level, bool dirty, int tailState, int syncMode)
        {
            Rectangle iconRect = leds[0];
            iconRect.Height = 16; // (DockPanel's damn hardcoded height) // this.leds[this.leds.Length - 1].Bottom;
            iconRect.Width = iconRect.Right + 6;
            Bitmap bmp = new Bitmap(iconRect.Width, iconRect.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            int offsetFromTop = 4;

            for (int i = 0; i < leds.Length; ++i)
            {
                Rectangle ledRect = leds[i];
                ledRect.Offset(0, offsetFromTop);
                if (level >= leds.Length - i)
                {
                    gfx.FillRectangle(ledBrushes[i], ledRect);
                }
                else
                {
                    gfx.FillRectangle(offLedBrush, ledRect);
                }
            }

            int ledSize = 3;
            int ledGap = 1;
            Rectangle lastLed = leds[leds.Length - 1];
            Rectangle dirtyLed = new Rectangle(lastLed.Right + 2, lastLed.Bottom - ledSize, ledSize, ledSize);
            Rectangle tailLed = new Rectangle(dirtyLed.Location, dirtyLed.Size);
            tailLed.Offset(0, -(ledSize + ledGap));
            Rectangle syncLed = new Rectangle(tailLed.Location, dirtyLed.Size);
            syncLed.Offset(0, -(ledSize + ledGap));

            syncLed.Offset(0, offsetFromTop);
            tailLed.Offset(0, offsetFromTop);
            dirtyLed.Offset(0, offsetFromTop);

            if (dirty)
            {
                gfx.FillRectangle(dirtyLedBrush, dirtyLed);
            }
            else
            {
                gfx.FillRectangle(offLedBrush, dirtyLed);
            }

            // tailMode 4 means: don't show
            if (tailState < 3)
            {
                gfx.FillRectangle(tailLedBrush[tailState], tailLed);
            }

            if (syncMode == 1)
            {
                gfx.FillRectangle(syncLedBrush, syncLed);
            }
            //else
            //{
            //  gfx.FillRectangle(this.offLedBrush, syncLed);
            //}

            // see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=345656
            // GetHicon() creates an unmanaged handle which must be destroyed. The Clone() workaround creates
            // a managed copy of icon. then the unmanaged win32 handle is destroyed
            IntPtr iconHandle = bmp.GetHicon();
            Icon icon = Icon.FromHandle(iconHandle).Clone() as Icon;
            Win32.DestroyIcon(iconHandle);

            gfx.Dispose();
            bmp.Dispose();
            return icon;
        }

        private void CreateIcons()
        {
            for (int syncMode = 0; syncMode <= 1; syncMode++) // LED indicating time synced tabs
            {
                for (int tailMode = 0; tailMode < 4; tailMode++)
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        ledIcons[i, 0, tailMode, syncMode] = CreateLedIcon(i, false, tailMode, syncMode);
                    }

                    for (int i = 0; i < 6; ++i)
                    {
                        ledIcons[i, 1, tailMode, syncMode] = CreateLedIcon(i, true, tailMode, syncMode);
                    }
                }
            }
        }

        private void FileNotFound(LogWindow logWin)
        {
            LogWindowData data = logWin.Tag as LogWindowData;
            BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, deadIcon);
            dateTimeDragControl.Visible = false;
        }

        private void FileRespawned(LogWindow logWin)
        {
            LogWindowData data = logWin.Tag as LogWindowData;
            Icon icon = GetIcon(0, data);
            BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, icon);
        }

        private void ShowLedPeak(LogWindow logWin)
        {
            LogWindowData data = logWin.Tag as LogWindowData;
            lock (data)
            {
                data.diffSum = DIFF_MAX;
            }

            Icon icon = GetIcon(data.diffSum, data);
            BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, icon);
        }

        private int GetLevelFromDiff(int diff)
        {
            if (diff > 60)
            {
                diff = 60;
            }

            int level = diff / 10;
            if (diff > 0 && level == 0)
            {
                level = 2;
            }
            else if (level == 0)
            {
                level = 1;
            }

            return level - 1;
        }

        private void LedThreadProc()
        {
            Thread.CurrentThread.Name = "LED Thread";
            while (!shouldStop)
            {
                try
                {
                    Thread.Sleep(200);
                }
                catch (Exception)
                {
                    return;
                }

                lock (logWindowList)
                {
                    foreach (LogWindow logWindow in logWindowList)
                    {
                        LogWindowData data = logWindow.Tag as LogWindowData;
                        if (data.diffSum > 0)
                        {
                            data.diffSum -= 10;
                            if (data.diffSum < 0)
                            {
                                data.diffSum = 0;
                            }

                            Icon icon = GetIcon(data.diffSum, data);
                            BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
                        }
                    }
                }
            }
        }

        private void SetTabIcon(LogWindow logWindow, Icon icon)
        {
            if (logWindow != null)
            {
                logWindow.Icon = icon;
                if (logWindow.DockHandler.Pane != null)
                {
                    logWindow.DockHandler.Pane.TabStripControl.Invalidate(false);
                }
            }
        }

        private Icon GetIcon(int diff, LogWindowData data)
        {
            Icon icon =
                ledIcons[
                    GetLevelFromDiff(diff), data.dirty ? 1 : 0, Preferences.showTailState ? data.tailState : 3,
                    data.syncMode
                ];
            return icon;
        }

        private void RefreshEncodingMenuBar(Encoding encoding)
        {
            aSCIIToolStripMenuItem.Checked = false;
            aNSIToolStripMenuItem.Checked = false;
            uTF8ToolStripMenuItem.Checked = false;
            uTF16ToolStripMenuItem.Checked = false;
            iSO88591ToolStripMenuItem.Checked = false;
            if (encoding == null)
            {
                return;
            }

            if (encoding is ASCIIEncoding)
            {
                aSCIIToolStripMenuItem.Checked = true;
            }
            else if (encoding.Equals(Encoding.Default))
            {
                aNSIToolStripMenuItem.Checked = true;
            }
            else if (encoding is UTF8Encoding)
            {
                uTF8ToolStripMenuItem.Checked = true;
            }
            else if (encoding is UnicodeEncoding)
            {
                uTF16ToolStripMenuItem.Checked = true;
            }
            else if (encoding.Equals(Encoding.GetEncoding("iso-8859-1")))
            {
                iSO88591ToolStripMenuItem.Checked = true;
            }

            aNSIToolStripMenuItem.Text = Encoding.Default.HeaderName;
        }

        private void OpenSettings(int tabToOpen)
        {
            SettingsDialog dlg = new SettingsDialog(ConfigManager.Settings.preferences, this, tabToOpen);
            dlg.TopMost = TopMost;
            if (DialogResult.OK == dlg.ShowDialog())
            {
                ConfigManager.Settings.preferences = dlg.Preferences;
                ConfigManager.Save(SettingsFlags.Settings);
                NotifyWindowsForChangedPrefs(SettingsFlags.Settings);
            }
        }

        private void NotifyWindowsForChangedPrefs(SettingsFlags flags)
        {
            _logger.Info("The preferences have changed");
            ApplySettings(ConfigManager.Settings, flags);

            lock (logWindowList)
            {
                foreach (LogWindow logWindow in logWindowList)
                {
                    logWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, flags);
                }
            }

            bookmarkWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, flags);

            HilightGroupList = ConfigManager.Settings.hilightGroupList;
            if ((flags & SettingsFlags.HighlightSettings) == SettingsFlags.HighlightSettings)
            {
                OnHighlightSettingsChanged();
            }
        }

        private void ApplySettings(Settings settings, SettingsFlags flags)
        {
            if ((flags & SettingsFlags.WindowPosition) == SettingsFlags.WindowPosition)
            {
                TopMost = alwaysOnTopToolStripMenuItem.Checked = settings.alwaysOnTop;
                dateTimeDragControl.DragOrientation = settings.preferences.timestampControlDragOrientation;
                hideLineColumnToolStripMenuItem.Checked = settings.hideLineColumn;
            }

            if ((flags & SettingsFlags.FileHistory) == SettingsFlags.FileHistory)
            {
                FillHistoryMenu();
            }

            if ((flags & SettingsFlags.GuiOrColors) == SettingsFlags.GuiOrColors)
            {
                SetTabIcons(settings.preferences);
            }

            if ((flags & SettingsFlags.ToolSettings) == SettingsFlags.ToolSettings)
            {
                FillToolLauncherBar();
            }

            if ((flags & SettingsFlags.HighlightSettings) == SettingsFlags.HighlightSettings)
            {
                FillHighlightComboBox();
            }
        }

        private void SetTabIcons(Preferences preferences)
        {
            tailLedBrush[0] = new SolidBrush(preferences.showTailColor);
            CreateIcons();
            lock (logWindowList)
            {
                foreach (LogWindow logWindow in logWindowList)
                {
                    LogWindowData data = logWindow.Tag as LogWindowData;
                    Icon icon = GetIcon(data.diffSum, data);
                    BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private void SetToolIcon(ToolEntry entry, ToolStripItem item)
        {
            Icon icon = Win32.LoadIconFromExe(entry.iconFile, entry.iconIndex);
            if (icon != null)
            {
                item.Image = icon.ToBitmap();
                if (item is ToolStripMenuItem)
                {
                    item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                }
                else
                {
                    item.DisplayStyle = ToolStripItemDisplayStyle.Image;
                }

                DestroyIcon(icon.Handle);
                icon.Dispose();
            }

            if (!string.IsNullOrEmpty(entry.cmd))
            {
                item.ToolTipText = entry.name;
            }
        }

        private void ToolButtonClick(ToolEntry toolEntry)
        {
            if (string.IsNullOrEmpty(toolEntry.cmd))
            {
                OpenSettings(2);
                return;
            }

            if (CurrentLogWindow != null)
            {
                ILogLine line = CurrentLogWindow.GetCurrentLine();
                ILogFileInfo info = CurrentLogWindow.GetCurrentFileInfo();
                if (line != null && info != null)
                {
                    ArgParser parser = new ArgParser(toolEntry.args);
                    string argLine = parser.BuildArgs(line, CurrentLogWindow.GetRealLineNum() + 1, info, this);
                    if (argLine != null)
                    {
                        StartTool(toolEntry.cmd, argLine, toolEntry.sysout, toolEntry.columnizerName,
                            toolEntry.workingDir);
                    }
                }
            }
        }

        private void StartTool(string cmd, string args, bool sysoutPipe, string columnizerName, string workingDir)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                return;
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(cmd, args);
            if (!Util.IsNull(workingDir))
            {
                startInfo.WorkingDirectory = workingDir;
            }

            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            if (sysoutPipe)
            {
                ILogLineColumnizer columnizer = Util.FindColumnizerByName(columnizerName,
                    PluginRegistry.GetInstance().RegisteredColumnizers);
                if (columnizer == null)
                {
                    columnizer = PluginRegistry.GetInstance().RegisteredColumnizers[0];
                }

                _logger.Info("Starting external tool with sysout redirection: {0} {1}", cmd, args);
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                //process.OutputDataReceived += pipe.DataReceivedEventHandler;
                try
                {
                    process.Start();
                }
                catch (Win32Exception e)
                {
                    _logger.Error(e);
                    MessageBox.Show(e.Message);
                    return;
                }

                SysoutPipe pipe = new SysoutPipe(process.StandardOutput);
                LogWindow logWin = AddTempFileTab(pipe.FileName,
                    CurrentLogWindow.IsTempFile
                        ? CurrentLogWindow.TempTitleName
                        : Util.GetNameFromPath(CurrentLogWindow.FileName) + "->E");
                logWin.ForceColumnizer(columnizer);
                process.Exited += pipe.ProcessExitedEventHandler;
                //process.BeginOutputReadLine();
            }
            else
            {
                _logger.Info("Starting external tool: {0} {1}", cmd, args);
                try
                {
                    startInfo.UseShellExecute = false;
                    process.Start();
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    MessageBox.Show(e.Message);
                }
            }
        }

        private void CloseAllTabs()
        {
            IList<Form> closeList = new List<Form>();
            lock (logWindowList)
            {
                foreach (DockContent content in dockPanel.Contents)
                {
                    if (content is LogWindow)
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

        private void setTabColor(LogWindow logWindow, Color color)
        {
            //tabPage.BackLowColor = color;
            //tabPage.BackLowColorDisabled = Color.FromArgb(255,
            //  Math.Max(0, color.R - 50),
            //  Math.Max(0, color.G - 50),
            //  Math.Max(0, color.B - 50)
            //  );
        }

        private void LoadProject(string projectFileName, bool restoreLayout)
        {
            ProjectData projectData = ProjectPersister.LoadProjectData(projectFileName);
            bool hasLayoutData = projectData.tabLayoutXml != null;

            if (hasLayoutData && restoreLayout && logWindowList.Count > 0)
            {
                ProjectLoadDlg dlg = new ProjectLoadDlg();
                if (DialogResult.Cancel != dlg.ShowDialog())
                {
                    switch (dlg.ProjectLoadResult)
                    {
                        case ProjectLoadDlgResult.IgnoreLayout:
                            hasLayoutData = false;
                            break;
                        case ProjectLoadDlgResult.CloseTabs:
                            CloseAllTabs();
                            break;
                        case ProjectLoadDlgResult.NewWindow:
                            LogExpertProxy.NewWindow(new string[] {projectFileName});
                            return;
                    }
                }
            }

            if (projectData != null)
            {
                foreach (string fileName in projectData.memberList)
                {
                    if (hasLayoutData)
                    {
                        AddFileTabDeferred(fileName, false, null, true, null);
                    }
                    else
                    {
                        AddFileTab(fileName, false, null, true, null);
                    }
                }

                if (hasLayoutData && restoreLayout)
                {
                    // Re-creating tool (non-document) windows is needed because the DockPanel control would throw strange errors
                    DestroyToolWindows();
                    InitToolWindows();
                    RestoreLayout(projectData.tabLayoutXml);
                }
            }
        }

        private void ApplySelectedHighlightGroup()
        {
            string groupName = highlightGroupsComboBox.Text;
            if (CurrentLogWindow != null)
            {
                CurrentLogWindow.SetCurrentHighlightGroup(groupName);
            }
        }

        private void FillToolLauncherBar()
        {
            char[] labels = new char[]
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
                'U', 'V', 'W', 'X', 'Y', 'Z'
            };
            toolsToolStripMenuItem.DropDownItems.Clear();
            toolsToolStripMenuItem.DropDownItems.Add(configureToolStripMenuItem);
            toolsToolStripMenuItem.DropDownItems.Add(configureToolStripSeparator);
            externalToolsToolStrip.Items.Clear();
            int num = 0;
            externalToolsToolStrip.SuspendLayout();
            foreach (ToolEntry tool in Preferences.toolEntries)
            {
                if (tool.isFavourite)
                {
                    ToolStripButton button = new ToolStripButton("" + labels[num % 26]);
                    button.Tag = tool;
                    SetToolIcon(tool, button);
                    externalToolsToolStrip.Items.Add(button);
                }

                num++;
                ToolStripMenuItem menuItem = new ToolStripMenuItem(tool.name);
                menuItem.Tag = tool;
                SetToolIcon(tool, menuItem);
                toolsToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            externalToolsToolStrip.ResumeLayout();

            externalToolsToolStrip.Visible = num > 0; // do not show bar if no tool uses it
        }

        private void runGC()
        {
            _logger.Info("Running GC. Used mem before: {0:N0}", GC.GetTotalMemory(false));
            GC.Collect();
            _logger.Info("GC done.    Used mem after:  {0:N0}", GC.GetTotalMemory(true));
        }

        private void dumpGCInfo()
        {
            _logger.Info("-------- GC info -----------\r\nUsed mem: {0:N0}", GC.GetTotalMemory(false));
            for (int i = 0; i < GC.MaxGeneration; ++i)
            {
                _logger.Info("Generation {0} collect count: {1}", i, GC.CollectionCount(i));
            }

            _logger.Info("----------------------------");
        }

        private void throwExceptionFx()
        {
            throw new Exception("This is a test exception thrown by an async delegate");
        }

        private void throwExceptionThreadFx()
        {
            throw new Exception("This is a test exception thrown by a background thread");
        }

        private string SaveLayout()
        {
            ;
            using (MemoryStream memStream = new MemoryStream(2000))
            using (StreamReader r = new StreamReader(memStream))
            {
                dockPanel.SaveAsXml(memStream, Encoding.UTF8, true);

                memStream.Seek(0, SeekOrigin.Begin);
                string resultXml = r.ReadToEnd();

                r.Close();

                return resultXml;
            }
        }

        private void RestoreLayout(string layoutXml)
        {
            using (MemoryStream memStream = new MemoryStream(2000))
            using (StreamWriter w = new StreamWriter(memStream))
            {
                w.Write(layoutXml);
                w.Flush();

                memStream.Seek(0, SeekOrigin.Begin);

                dockPanel.LoadFromXml(memStream, DeserializeDockContent, true);
            }
        }

        private IDockContent DeserializeDockContent(string persistString)
        {
            if (persistString.Equals(WindowTypes.BookmarkWindow.ToString()))
            {
                return bookmarkWindow;
            }
            else if (persistString.StartsWith(WindowTypes.LogWindow.ToString()))
            {
                string fileName = persistString.Substring(WindowTypes.LogWindow.ToString().Length + 1);
                LogWindow win = FindWindowForFile(fileName);
                if (win != null)
                {
                    return win;
                }
                else
                {
                    _logger.Warn("Layout data contains non-existing LogWindow for {0}", fileName);
                }
            }

            return null;
        }

        private void OnHighlightSettingsChanged()
        {
            if (HighlightSettingsChanged != null)
            {
                HighlightSettingsChanged(this, new EventArgs());
            }
        }

        #endregion
    }
}