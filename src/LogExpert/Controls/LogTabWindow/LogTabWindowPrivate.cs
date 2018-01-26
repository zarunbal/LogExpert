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
            this.bookmarkWindow = new BookmarkWindow();
            this.bookmarkWindow.HideOnClose = true;
            this.bookmarkWindow.ShowHint = DockState.DockBottom;
            this.bookmarkWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, SettingsFlags.All);
            this.bookmarkWindow.VisibleChanged += new EventHandler(bookmarkWindow_VisibleChanged);
            this.firstBookmarkWindowShow = true;
        }

        private void DestroyBookmarkWindow()
        {
            this.bookmarkWindow.HideOnClose = false;
            this.bookmarkWindow.Close();
        }

        private void SaveLastOpenFilesList()
        {
            ConfigManager.Settings.lastOpenFilesList.Clear();
            foreach (DockContent content in this.dockPanel.Contents)
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
            if (this.WindowState == FormWindowState.Normal)
            {
                ConfigManager.Settings.appBounds = this.Bounds;
                ConfigManager.Settings.isMaximized = false;
            }
            else
            {
                FormWindowState state = this.WindowState;
                ConfigManager.Settings.appBoundsFullscreen = this.Bounds;
                ConfigManager.Settings.isMaximized = true;
                this.WindowState = FormWindowState.Normal;
                ConfigManager.Settings.appBounds = this.Bounds;
                //this.WindowState = state;
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
            this.Activate();
        }

        private void AddLogWindow(LogWindow logWindow, string title, bool doNotAddToPanel)
        {
            logWindow.CloseButton = true;
            logWindow.TabPageContextMenuStrip = this.tabContextMenuStrip;
            SetTooltipText(logWindow, title);
            logWindow.DockAreas = DockAreas.Document | DockAreas.Float;

            if (!doNotAddToPanel)
            {
                logWindow.Show(this.dockPanel);
            }

            LogExpert.LogTabWindow.LogWindowData data = new LogExpert.LogTabWindow.LogWindowData();
            data.diffSum = 0;
            logWindow.Tag = data;
            lock (this.logWindowList)
            {
                this.logWindowList.Add(logWindow);
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

            LogExpert.LogTabWindow.LogWindowData data = logWindow.Tag as LogExpert.LogTabWindow.LogWindowData;
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
            lock (this.logWindowList)
            {
                foreach (LogWindow logWindow in this.logWindowList)
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
            strip.ItemClicked += new ToolStripItemClickedEventHandler(history_ItemClicked);
            strip.MouseUp += new MouseEventHandler(strip_MouseUp);
            this.lastUsedToolStripMenuItem.DropDown = strip;
        }

        private void RemoveLogWindow(LogWindow logWindow)
        {
            lock (this.logWindowList)
            {
                this.logWindowList.Remove(logWindow);
            }
            DisconnectEventHandlers(logWindow);
        }

        private void RemoveAndDisposeLogWindow(LogWindow logWindow, bool dontAsk)
        {
            if (this.CurrentLogWindow == logWindow)
            {
                ChangeCurrentLogWindow(null);
            }
            lock (this.logWindowList)
            {
                this.logWindowList.Remove(logWindow);
            }
            logWindow.Close(dontAsk);
        }

        private void ShowHighlightSettingsDialog()
        {
            HilightDialog dlg = new HilightDialog();
            dlg.KeywordActionList = PluginRegistry.GetInstance().RegisteredKeywordActions;
            dlg.Owner = this;
            dlg.TopMost = this.TopMost;
            dlg.HilightGroupList = this.HilightGroupList;
            dlg.PreSelectedGroupName = this.highlightGroupsComboBox.Text;
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                this.HilightGroupList = dlg.HilightGroupList;
                FillHighlightComboBox();
                ConfigManager.Settings.hilightGroupList = this.HilightGroupList;
                ConfigManager.Save(SettingsFlags.HighlightSettings);
                OnHighlightSettingsChanged();
            }
        }

        private void FillHighlightComboBox()
        {
            string currentGroupName = this.highlightGroupsComboBox.Text;
            this.highlightGroupsComboBox.Items.Clear();
            foreach (HilightGroup group in this.HilightGroupList)
            {
                this.highlightGroupsComboBox.Items.Add(group.GroupName);
                if (group.GroupName.Equals(currentGroupName))
                {
                    this.highlightGroupsComboBox.Text = group.GroupName;
                }
            }
        }

        private void OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (this.CurrentLogWindow != null)
            {
                FileInfo info = new FileInfo(this.CurrentLogWindow.FileName);
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
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.SwitchMultiFile(!this.CurrentLogWindow.IsMultiFile);
                this.multiFileToolStripMenuItem.Checked = this.CurrentLogWindow.IsMultiFile;
                this.multiFileEnabledStripMenuItem.Checked = this.CurrentLogWindow.IsMultiFile;
            }
        }

        private void ChangeCurrentLogWindow(LogWindow newLogWindow)
        {
            if (newLogWindow == this.currentLogWindow)
            {
                return; // do nothing if wishing to set the same window
            }

            LogWindow oldLogWindow = this.currentLogWindow;
            this.currentLogWindow = newLogWindow;
            string titleName = this.showInstanceNumbers ? "LogExpert #" + this.instanceNumber : "LogExpert";

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
                    this.Text = titleName + " - " + newLogWindow.TempTitleName;
                }
                else
                {
                    this.Text = titleName + " - " + newLogWindow.FileName;
                }
                this.multiFileToolStripMenuItem.Checked = this.CurrentLogWindow.IsMultiFile;
                this.multiFileToolStripMenuItem.Enabled = true;
                this.multiFileEnabledStripMenuItem.Checked = this.CurrentLogWindow.IsMultiFile;
                this.cellSelectModeToolStripMenuItem.Checked = true;
                this.cellSelectModeToolStripMenuItem.Enabled = true;
                this.closeFileToolStripMenuItem.Enabled = true;
                this.searchToolStripMenuItem.Enabled = true;
                this.filterToolStripMenuItem.Enabled = true;
                this.goToLineToolStripMenuItem.Enabled = true;
                //ConnectToolWindows(newLogWindow);
            }
            else
            {
                this.Text = titleName;
                this.multiFileToolStripMenuItem.Checked = false;
                this.multiFileEnabledStripMenuItem.Checked = false;
                this.followTailCheckBox.Checked = false;
                this.menuStrip1.Enabled = true;
                timeshiftToolStripMenuItem.Enabled = false;
                timeshiftToolStripMenuItem.Checked = false;
                this.timeshiftMenuTextBox.Text = "";
                this.timeshiftMenuTextBox.Enabled = false;
                this.multiFileToolStripMenuItem.Enabled = false;
                this.cellSelectModeToolStripMenuItem.Checked = false;
                this.cellSelectModeToolStripMenuItem.Enabled = false;
                this.closeFileToolStripMenuItem.Enabled = false;
                this.searchToolStripMenuItem.Enabled = false;
                this.filterToolStripMenuItem.Enabled = false;
                this.goToLineToolStripMenuItem.Enabled = false;
                this.dateTimeDragControl.Visible = false;
            }
        }

        private void ConnectToolWindows(LogWindow logWindow)
        {
            ConnectBookmarkWindow(logWindow);
        }

        private void ConnectBookmarkWindow(LogWindow logWindow)
        {
            FileViewContext ctx = new FileViewContext(logWindow, logWindow);
            this.bookmarkWindow.SetBookmarkData(logWindow.BookmarkData);
            this.bookmarkWindow.SetCurrentFile(ctx);
        }

        private void DisconnectToolWindows(LogWindow logWindow)
        {
            DisconnectBookmarkWindow(logWindow);
        }

        private void DisconnectBookmarkWindow(LogWindow logWindow)
        {
            this.bookmarkWindow.SetBookmarkData(null);
            this.bookmarkWindow.SetCurrentFile(null);
        }

        private void GuiStateUpdateWorker(GuiStateArgs e)
        {
            skipEvents = true;
            this.followTailCheckBox.Checked = e.FollowTail;
            this.menuStrip1.Enabled = e.MenuEnabled;
            timeshiftToolStripMenuItem.Enabled = e.TimeshiftPossible;
            timeshiftToolStripMenuItem.Checked = e.TimeshiftEnabled;
            this.timeshiftMenuTextBox.Text = e.TimeshiftText;
            this.timeshiftMenuTextBox.Enabled = e.TimeshiftEnabled;
            this.multiFileToolStripMenuItem.Enabled = e.MultiFileEnabled; // disabled for temp files
            this.multiFileToolStripMenuItem.Checked = e.IsMultiFileActive;
            this.multiFileEnabledStripMenuItem.Checked = e.IsMultiFileActive;
            this.cellSelectModeToolStripMenuItem.Checked = e.CellSelectMode;
            RefreshEncodingMenuBar(e.CurrentEncoding);
            if (e.TimeshiftPossible && ConfigManager.Settings.preferences.timestampControl)
            {
                this.dateTimeDragControl.MinDateTime = e.MinTimestamp;
                this.dateTimeDragControl.MaxDateTime = e.MaxTimestamp;
                this.dateTimeDragControl.DateTime = e.Timestamp;
                this.dateTimeDragControl.Visible = true;
                this.dateTimeDragControl.Enabled = true;
                this.dateTimeDragControl.Refresh();
            }
            else
            {
                this.dateTimeDragControl.Visible = false;
                this.dateTimeDragControl.Enabled = false;
            }
            this.toolStripButtonBubbles.Checked = e.ShowBookmarkBubbles;
            this.highlightGroupsComboBox.Text = e.HighlightGroupName;
            this.columnFinderToolStripMenuItem.Checked = e.ColumnFinderVisible;

            skipEvents = false;
        }

        private void ProgressBarUpdateWorker(ProgressEventArgs e)
        {
            if (e.Value <= e.MaxValue && e.Value >= e.MinValue)
            {
                this.loadProgessBar.Minimum = e.MinValue;
                this.loadProgessBar.Maximum = e.MaxValue;
                this.loadProgessBar.Value = e.Value;
                this.loadProgessBar.Visible = e.Visible;
                this.Invoke(new MethodInvoker(this.statusStrip1.Refresh));
            }
        }

        private void StatusLineThreadFunc()
        {
            int timeSum = 0;
            int waitTime = 30;
            while (!this.shouldStop)
            {
                this.statusLineEventWakeupHandle.WaitOne();
                this.statusLineEventWakeupHandle.Reset();
                if (!this.shouldStop)
                {
                    bool signaled = false;
                    do
                    {
                        //this.statusLineEventHandle.Reset();
                        signaled = this.statusLineEventHandle.WaitOne(waitTime, true);
                        timeSum += waitTime;
                    } while (signaled && timeSum < 900 && !this.shouldStop);

                    if (!this.shouldStop)
                    {
                        timeSum = 0;
                        try
                        {
                            StatusLineEventArgs e;
                            lock (this.statusLineLock)
                            {
                                e = this.lastStatusLineEvent.Clone();
                            }
                            this.BeginInvoke(new LogExpert.LogTabWindow.StatusLineEventFx(StatusLineEventWorker), new object[] {e});
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
            this.statusLabel.Text = e.StatusText;
            this.linesLabel.Text = "" + e.LineCount + " lines";
            this.sizeLabel.Text = Util.GetFileSizeAsText(e.FileSize);
            this.currentLineLabel.Text = "" + e.CurrentLineNum;
            this.statusStrip1.Refresh();
        }

        // tailState: 0,1,2 = on/off/off by Trigger
        // syncMode: 0 = normal (no), 1 = time synced 
        private Icon CreateLedIcon(int level, bool dirty, int tailState, int syncMode)
        {
            Rectangle iconRect = this.leds[0];
            iconRect.Height = 16; // (DockPanel's damn hardcoded height) // this.leds[this.leds.Length - 1].Bottom;
            iconRect.Width = iconRect.Right + 6;
            Bitmap bmp = new Bitmap(iconRect.Width, iconRect.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            int offsetFromTop = 4;

            for (int i = 0; i < this.leds.Length; ++i)
            {
                Rectangle ledRect = this.leds[i];
                ledRect.Offset(0, offsetFromTop);
                if (level >= this.leds.Length - i)
                {
                    gfx.FillRectangle(this.ledBrushes[i], ledRect);
                }
                else
                {
                    gfx.FillRectangle(this.offLedBrush, ledRect);
                }
            }

            int ledSize = 3;
            int ledGap = 1;
            Rectangle lastLed = this.leds[this.leds.Length - 1];
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
                gfx.FillRectangle(this.dirtyLedBrush, dirtyLed);
            }
            else
            {
                gfx.FillRectangle(this.offLedBrush, dirtyLed);
            }

            // tailMode 4 means: don't show
            if (tailState < 3)
            {
                gfx.FillRectangle(this.tailLedBrush[tailState], tailLed);
            }

            if (syncMode == 1)
            {
                gfx.FillRectangle(this.syncLedBrush, syncLed);
            }
            //else
            //{
            //  gfx.FillRectangle(this.offLedBrush, syncLed);
            //}

            // see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=345656
            // GetHicon() creates an unmanaged handle which must be destroyed. The Clone() workaround creates
            // a managed copy of icon. then the unmanaged win32 handle is destroyed
            IntPtr iconHandle = bmp.GetHicon();
            Icon icon = System.Drawing.Icon.FromHandle(iconHandle).Clone() as Icon;
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
                        this.ledIcons[i, 0, tailMode, syncMode] = CreateLedIcon(i, false, tailMode, syncMode);
                    }
                    for (int i = 0; i < 6; ++i)
                    {
                        this.ledIcons[i, 1, tailMode, syncMode] = CreateLedIcon(i, true, tailMode, syncMode);
                    }
                }
            }
        }

        private void FileNotFound(LogWindow logWin)
        {
            LogExpert.LogTabWindow.LogWindowData data = logWin.Tag as LogExpert.LogTabWindow.LogWindowData;
            this.BeginInvoke(new LogExpert.LogTabWindow.SetTabIconDelegate(SetTabIcon), new object[] {logWin, this.deadIcon});
            this.dateTimeDragControl.Visible = false;
        }

        private void FileRespawned(LogWindow logWin)
        {
            LogExpert.LogTabWindow.LogWindowData data = logWin.Tag as LogExpert.LogTabWindow.LogWindowData;
            Icon icon = GetIcon(0, data);
            this.BeginInvoke(new LogExpert.LogTabWindow.SetTabIconDelegate(SetTabIcon), new object[] {logWin, icon});
        }

        private void ShowLedPeak(LogWindow logWin)
        {
            LogExpert.LogTabWindow.LogWindowData data = logWin.Tag as LogExpert.LogTabWindow.LogWindowData;
            lock (data)
            {
                data.diffSum = DIFF_MAX;
            }
            Icon icon = GetIcon(data.diffSum, data);
            this.BeginInvoke(new LogExpert.LogTabWindow.SetTabIconDelegate(SetTabIcon), new object[] {logWin, icon});
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
            while (!this.shouldStop)
            {
                try
                {
                    Thread.Sleep(200);
                }
                catch (Exception)
                {
                    return;
                }
                lock (this.logWindowList)
                {
                    foreach (LogWindow logWindow in this.logWindowList)
                    {
                        LogExpert.LogTabWindow.LogWindowData data = logWindow.Tag as LogExpert.LogTabWindow.LogWindowData;
                        if (data.diffSum > 0)
                        {
                            data.diffSum -= 10;
                            if (data.diffSum < 0)
                            {
                                data.diffSum = 0;
                            }
                            Icon icon = GetIcon(data.diffSum, data);
                            this.BeginInvoke(new LogExpert.LogTabWindow.SetTabIconDelegate(SetTabIcon), new object[] {logWindow, icon});
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

        private Icon GetIcon(int diff, LogExpert.LogTabWindow.LogWindowData data)
        {
            Icon icon =
                this.ledIcons[
                    GetLevelFromDiff(diff), data.dirty ? 1 : 0, this.Preferences.showTailState ? data.tailState : 3,
                    data.syncMode
                ];
            return icon;
        }

        private void RefreshEncodingMenuBar(Encoding encoding)
        {
            this.aSCIIToolStripMenuItem.Checked = false;
            this.aNSIToolStripMenuItem.Checked = false;
            this.uTF8ToolStripMenuItem.Checked = false;
            this.uTF16ToolStripMenuItem.Checked = false;
            this.iSO88591ToolStripMenuItem.Checked = false;
            if (encoding == null)
            {
                return;
            }
            if (encoding is System.Text.ASCIIEncoding)
            {
                this.aSCIIToolStripMenuItem.Checked = true;
            }
            else if (encoding.Equals(Encoding.Default))
            {
                this.aNSIToolStripMenuItem.Checked = true;
            }
            else if (encoding is System.Text.UTF8Encoding)
            {
                this.uTF8ToolStripMenuItem.Checked = true;
            }
            else if (encoding is System.Text.UnicodeEncoding)
            {
                this.uTF16ToolStripMenuItem.Checked = true;
            }
            else if (encoding.Equals(Encoding.GetEncoding("iso-8859-1")))
            {
                this.iSO88591ToolStripMenuItem.Checked = true;
            }
            this.aNSIToolStripMenuItem.Text = Encoding.Default.HeaderName;
        }

        private void OpenSettings(int tabToOpen)
        {
            SettingsDialog dlg = new SettingsDialog(ConfigManager.Settings.preferences, this, tabToOpen);
            dlg.TopMost = this.TopMost;
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

            lock (this.logWindowList)
            {
                foreach (LogWindow logWindow in this.logWindowList)
                {
                    logWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, flags);
                }
            }
            this.bookmarkWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, flags);

            this.HilightGroupList = ConfigManager.Settings.hilightGroupList;
            if ((flags & SettingsFlags.HighlightSettings) == SettingsFlags.HighlightSettings)
            {
                OnHighlightSettingsChanged();
            }
        }

        private void ApplySettings(Settings settings, SettingsFlags flags)
        {
            if ((flags & SettingsFlags.WindowPosition) == SettingsFlags.WindowPosition)
            {
                this.TopMost = this.alwaysOnTopToolStripMenuItem.Checked = settings.alwaysOnTop;
                this.dateTimeDragControl.DragOrientation = settings.preferences.timestampControlDragOrientation;
                this.hideLineColumnToolStripMenuItem.Checked = settings.hideLineColumn;
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
            this.tailLedBrush[0] = new SolidBrush(preferences.showTailColor);
            CreateIcons();
            lock (this.logWindowList)
            {
                foreach (LogWindow logWindow in this.logWindowList)
                {
                    LogExpert.LogTabWindow.LogWindowData data = logWindow.Tag as LogExpert.LogTabWindow.LogWindowData;
                    Icon icon = GetIcon(data.diffSum, data);
                    this.BeginInvoke(new LogExpert.LogTabWindow.SetTabIconDelegate(SetTabIcon), new object[] {logWindow, icon});
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
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
            if (entry.cmd != null && entry.cmd.Length > 0)
            {
                item.ToolTipText = entry.name;
            }
        }

        private void ToolButtonClick(ToolEntry toolEntry)
        {
            if (toolEntry.cmd == null || toolEntry.cmd.Length == 0)
            {
                OpenSettings(2);
                return;
            }
            if (this.CurrentLogWindow != null)
            {
                ILogLine line = this.CurrentLogWindow.GetCurrentLine();
                ILogFileInfo info = this.CurrentLogWindow.GetCurrentFileInfo();
                if (line != null && info != null)
                {
                    ArgParser parser = new ArgParser(toolEntry.args);
                    string argLine = parser.BuildArgs(line, this.CurrentLogWindow.GetRealLineNum() + 1, info, this);
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
            if (cmd == null || cmd.Length == 0)
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
                    this.CurrentLogWindow.IsTempFile
                        ? this.CurrentLogWindow.TempTitleName
                        : Util.GetNameFromPath(this.CurrentLogWindow.FileName) + "->E");
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
            lock (this.logWindowList)
            {
                foreach (DockContent content in this.dockPanel.Contents)
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

            if (hasLayoutData && restoreLayout && this.logWindowList.Count > 0)
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
            string groupName = this.highlightGroupsComboBox.Text;
            if (this.CurrentLogWindow != null)
            {
                this.CurrentLogWindow.SetCurrentHighlightGroup(groupName);
            }
        }

        private void FillToolLauncherBar()
        {
            char[] labels = new char[]
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
                'U', 'V', 'W', 'X', 'Y', 'Z'
            };
            this.toolsToolStripMenuItem.DropDownItems.Clear();
            this.toolsToolStripMenuItem.DropDownItems.Add(this.configureToolStripMenuItem);
            this.toolsToolStripMenuItem.DropDownItems.Add(this.configureToolStripSeparator);
            this.externalToolsToolStrip.Items.Clear();
            int num = 0;
            this.externalToolsToolStrip.SuspendLayout();
            foreach (ToolEntry tool in this.Preferences.toolEntries)
            {
                if (tool.isFavourite)
                {
                    ToolStripButton button = new ToolStripButton("" + labels[num % 26]);
                    button.Tag = tool;
                    SetToolIcon(tool, button);
                    this.externalToolsToolStrip.Items.Add(button);
                }
                num++;
                ToolStripMenuItem menuItem = new ToolStripMenuItem(tool.name);
                menuItem.Tag = tool;
                SetToolIcon(tool, menuItem);
                this.toolsToolStripMenuItem.DropDownItems.Add(menuItem);
            }
            this.externalToolsToolStrip.ResumeLayout();

            this.externalToolsToolStrip.Visible = num > 0; // do not show bar if no tool uses it
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
            MemoryStream memStream = new MemoryStream(2000);
            this.dockPanel.SaveAsXml(memStream, Encoding.UTF8, true);
            memStream.Seek(0, SeekOrigin.Begin);
            StreamReader r = new StreamReader(memStream);
            string resultXml = r.ReadToEnd();
            r.Close();
            return resultXml;
        }

        private void RestoreLayout(string layoutXml)
        {
            MemoryStream memStream = new MemoryStream(2000);
            StreamWriter w = new StreamWriter(memStream);
            w.Write(layoutXml);
            w.Flush();
            memStream.Seek(0, SeekOrigin.Begin);
            this.dockPanel.LoadFromXml(memStream, this.DeserializeDockContent, true);
            w.Dispose();
        }

        private IDockContent DeserializeDockContent(string persistString)
        {
            if (persistString.Equals(WindowTypes.BookmarkWindow.ToString()))
            {
                return this.bookmarkWindow;
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