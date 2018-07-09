using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LogExpert.Dialogs;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
    public partial class LogTabWindow
    {
        #region Public methods

        public LogWindow AddTempFileTab(string fileName, string title)
        {
            return AddFileTab(fileName, true, title, false, null);
        }

        public LogWindow AddFilterTab(FilterPipe pipe, string title, ILogLineColumnizer preProcessColumnizer)
        {
            LogWindow logWin = AddFileTab(pipe.FileName, true, title, false, preProcessColumnizer);
            if (pipe.FilterParams.searchText.Length > 0)
            {
                ToolTip tip = new ToolTip(components);
                tip.SetToolTip(logWin,
                    "Filter: \"" + pipe.FilterParams.searchText + "\"" +
                    (pipe.FilterParams.isInvert ? " (Invert match)" : "") +
                    (pipe.FilterParams.columnRestrict ? "\nColumn restrict" : "")
                );
                tip.AutomaticDelay = 10;
                tip.AutoPopDelay = 5000;
                LogWindowData data = logWin.Tag as LogWindowData;
                data.toolTip = tip;
            }

            return logWin;
        }

        public LogWindow AddFileTabDeferred(string givenFileName, bool isTempFile, string title,
            bool forcePersistenceLoading,
            ILogLineColumnizer preProcessColumnizer)
        {
            return AddFileTab(givenFileName, isTempFile, title, forcePersistenceLoading,
                preProcessColumnizer, true);
        }

        public LogWindow AddFileTab(string givenFileName, bool isTempFile, string title,
            bool forcePersistenceLoading,
            ILogLineColumnizer preProcessColumnizer)
        {
            return AddFileTab(givenFileName, isTempFile, title, forcePersistenceLoading,
                preProcessColumnizer, false);
        }

        public LogWindow AddFileTab(string givenFileName, bool isTempFile, string title,
            bool forcePersistenceLoading,
            ILogLineColumnizer preProcessColumnizer, bool doNotAddToDockPanel)
        {
            string logFileName = FindFilenameForSettings(givenFileName);
            LogWindow win = FindWindowForFile(logFileName);
            if (win != null)
            {
                if (!isTempFile)
                {
                    AddToFileHistory(givenFileName);
                }

                SelectTab(win);
                return win;
            }

            EncodingOptions encodingOptions = new EncodingOptions();
            FillDefaultEncodingFromSettings(encodingOptions);
            LogWindow logWindow =
                new LogWindow(this, logFileName, isTempFile, forcePersistenceLoading);

            logWindow.GivenFileName = givenFileName;

            if (preProcessColumnizer != null)
            {
                logWindow.ForceColumnizerForLoading(preProcessColumnizer);
            }

            if (isTempFile)
            {
                logWindow.TempTitleName = title;
                encodingOptions.Encoding = new UnicodeEncoding(false, false);
            }

            AddLogWindow(logWindow, title, doNotAddToDockPanel);
            if (!isTempFile)
            {
                AddToFileHistory(givenFileName);
            }

            LogWindowData data = logWindow.Tag as LogWindowData;
            data.color = defaultTabColor;
            setTabColor(logWindow, defaultTabColor);
            //data.tabPage.BorderColor = this.defaultTabBorderColor;
            if (!isTempFile)
            {
                foreach (ColorEntry colorEntry in ConfigManager.Settings.fileColors)
                {
                    if (colorEntry.fileName.ToLower().Equals(logFileName.ToLower()))
                    {
                        data.color = colorEntry.color;
                        setTabColor(logWindow, colorEntry.color);
                        break;
                    }
                }
            }

            if (!isTempFile)
            {
                SetTooltipText(logWindow, logFileName);
            }

            if (givenFileName.EndsWith(".lxp"))
            {
                logWindow.ForcedPersistenceFileName = givenFileName;
            }

            // this.BeginInvoke(new LoadFileDelegate(logWindow.LoadFile), new object[] { logFileName, encoding });
            LoadFileDelegate loadFileFx = new LoadFileDelegate(logWindow.LoadFile);
            loadFileFx.BeginInvoke(logFileName, encodingOptions, null, null);
            return logWindow;
        }

        public LogWindow AddMultiFileTab(string[] fileNames)
        {
            if (fileNames.Length < 1)
            {
                return null;
            }

            LogWindow logWindow = new LogWindow(this, fileNames[fileNames.Length - 1], false, false);
            AddLogWindow(logWindow, fileNames[fileNames.Length - 1], false);
            multiFileToolStripMenuItem.Checked = true;
            multiFileEnabledStripMenuItem.Checked = true;
            EncodingOptions encodingOptions = new EncodingOptions();
            FillDefaultEncodingFromSettings(encodingOptions);
            BeginInvoke(new LoadMultiFilesDelegate(logWindow.LoadFilesAsMulti),
                new object[] {fileNames, encodingOptions});
            AddToFileHistory(fileNames[0]);
            return logWindow;
        }

        public void LoadFiles(string[] fileNames)
        {
            Invoke(new AddFileTabsDelegate(AddFileTabs), new object[] {fileNames});
        }

        public void OpenSearchDialog()
        {
            if (CurrentLogWindow == null)
            {
                return;
            }

            SearchDialog dlg = new SearchDialog();
            AddOwnedForm(dlg);
            dlg.TopMost = TopMost;
            SearchParams.historyList = ConfigManager.Settings.searchHistoryList;
            dlg.SearchParams = SearchParams;
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK && dlg.SearchParams != null && !string.IsNullOrWhiteSpace(dlg.SearchParams.searchText))
            {
                SearchParams = dlg.SearchParams;
                SearchParams.isFindNext = false;
                CurrentLogWindow.StartSearch();
            }
        }

        public ILogLineColumnizer GetColumnizerHistoryEntry(string fileName)
        {
            ColumnizerHistoryEntry entry = findColumnizerHistoryEntry(fileName);
            if (entry != null)
            {
                foreach (ILogLineColumnizer columnizer in PluginRegistry.GetInstance().RegisteredColumnizers)
                {
                    if (columnizer.GetName().Equals(entry.columnizerName))
                    {
                        return columnizer;
                    }
                }

                ConfigManager.Settings.columnizerHistoryList.Remove(entry); // no valid name -> remove entry
            }

            return null;
        }

        public void SwitchTab(bool shiftPressed)
        {
            int index = dockPanel.Contents.IndexOf(dockPanel.ActiveContent);
            if (shiftPressed)
            {
                index--;
                if (index < 0)
                {
                    index = dockPanel.Contents.Count - 1;
                }

                if (index < 0)
                {
                    return;
                }
            }
            else
            {
                index++;
                if (index >= dockPanel.Contents.Count)
                {
                    index = 0;
                }
            }

            if (index < dockPanel.Contents.Count)
            {
                (dockPanel.Contents[index] as DockContent).Activate();
            }
        }

        public void ScrollAllTabsToTimestamp(DateTime timestamp, LogWindow senderWindow)
        {
            lock (logWindowList)
            {
                foreach (LogWindow logWindow in logWindowList)
                {
                    if (logWindow != senderWindow)
                    {
                        if (logWindow.ScrollToTimestamp(timestamp, false, false))
                        {
                            ShowLedPeak(logWindow);
                        }
                    }
                }
            }
        }

        public ILogLineColumnizer FindColumnizerByFileMask(string fileName)
        {
            foreach (ColumnizerMaskEntry entry in ConfigManager.Settings.preferences.columnizerMaskList)
            {
                if (entry.mask != null)
                {
                    try
                    {
                        if (Regex.IsMatch(fileName, entry.mask))
                        {
                            ILogLineColumnizer columnizer = Util.FindColumnizerByName(entry.columnizerName,
                                PluginRegistry.GetInstance().RegisteredColumnizers);
                            return columnizer;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        _logger.Error(e, "RegEx-error while finding columnizer: ");
                        // occurs on invalid regex patterns
                    }
                }
            }

            return null;
        }

        public HilightGroup FindHighlightGroupByFileMask(string fileName)
        {
            foreach (HighlightMaskEntry entry in ConfigManager.Settings.preferences.highlightMaskList)
            {
                if (entry.mask != null)
                {
                    try
                    {
                        if (Regex.IsMatch(fileName, entry.mask))
                        {
                            HilightGroup group = FindHighlightGroup(entry.highlightGroupName);
                            return group;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        _logger.Error(e, "RegEx-error while finding columnizer: ");
                        // occurs on invalid regex patterns
                    }
                }
            }

            return null;
        }

        public void SelectTab(LogWindow logWindow)
        {
            logWindow.Activate();
        }

        public void SetForeground()
        {
            SetForegroundWindow(Handle);
            if (WindowState == FormWindowState.Minimized)
            {
                if (wasMaximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    WindowState = FormWindowState.Normal;
                }
            }
        }

        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        // called from LogWindow when follow tail was changed
        public void FollowTailChanged(LogWindow logWindow, bool isEnabled, bool offByTrigger)
        {
            LogWindowData data = logWindow.Tag as LogWindowData;
            if (data == null)
            {
                return;
            }

            if (isEnabled)
            {
                data.tailState = 0;
            }
            else
            {
                data.tailState = offByTrigger ? 2 : 1;
            }

            if (Preferences.showTailState)
            {
                Icon icon = GetIcon(data.diffSum, data);
                BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] {logWindow, icon});
            }
        }

        public void NotifySettingsChanged(object cookie, SettingsFlags flags)
        {
            if (cookie != this)
            {
                NotifyWindowsForChangedPrefs(flags);
            }
        }

        public IList<WindowFileEntry> GetListOfOpenFiles()
        {
            IList<WindowFileEntry> list = new List<WindowFileEntry>();
            lock (logWindowList)
            {
                foreach (LogWindow logWindow in logWindowList)
                {
                    list.Add(new WindowFileEntry(logWindow));
                }
            }

            return list;
        }

        #endregion
    }
}