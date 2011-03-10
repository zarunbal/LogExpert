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
using System.Diagnostics;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;

namespace LogExpert
{
  public partial class LogTabWindow : Form
  {
    class LogWindowData
    {
      public int diffSum;
      public bool dirty;
      public MdiTabControl.TabPage tabPage;
      public Color color = Color.FromKnownColor(KnownColor.Gray);
      public int tailState = 0; // tailState: 0,1,2 = on/off/off by Trigger
      public ToolTip toolTip;
      public int syncMode; // 0 = off, 1 = timeSynced
    }

    private const int MAX_HISTORY = 30;
    private const int MAX_COLUMNIZER_HISTORY = 40;
    private const int MAX_COLOR_HISTORY = 40;
    private const int DIFF_MAX = 100;
    private const int MAX_FILE_HISTORY = 10;

    private List<HilightGroup> hilightGroupList = new List<HilightGroup>();
    //private List<FilterParams> filterList = new List<FilterParams>();

    private ILogExpertProxy logExpertProxy;

    delegate void UpdateGridCallback(LogEventArgs e);
    delegate void UpdateProgressCallback(LoadFileEventArgs e);
    delegate int SearchFx(SearchParams searchParams);
    delegate void SelectLineFx(int line);
    delegate void FilterFx(FilterParams filterParams);
    delegate void AddFilterLineFx(int lineNum, bool immediate);
    delegate void UpdateProgressBarFx(int lineNum);
    delegate void SetColumnizerFx(ILogLineColumnizer columnizer);
    delegate void StatusLineEventFx(StatusLineEventArgs e);
    delegate void ProgressBarEventFx(ProgressEventArgs e);
    delegate void GuiStateUpdateWorkerDelegate(GuiStateArgs e);
    delegate void SetTabIconDelegate(MdiTabControl.TabPage tabPage, Icon icon);
    delegate void HandleTabDoubleClick(object sender);

    //bool waitingForClose = false;
    bool skipEvents = false;
    bool shouldStop = false;
    bool wasMaximized = false;

    string [] startupFileNames;

    LogWindow currentLogWindow = null;
    IList<LogWindow> logWindowList = new List<LogWindow>();

    Rectangle[] leds = new Rectangle[5];
    Brush[] ledBrushes = new Brush[5];
    Icon[,,,] ledIcons = new Icon[6, 2, 4, 2];
    Icon deadIcon;
    StringFormat tabStringFormat = new StringFormat();
    Brush offLedBrush;
    Brush dirtyLedBrush;
    Brush[] tailLedBrush = new Brush[3];
    Brush syncLedBrush;
    Thread ledThread;

    //Settings settings;
    SearchParams searchParams = new SearchParams();

    Color defaultTabColor = Color.FromArgb(255, 192, 192, 192);
    //Color defaultTabBorderColor = Color.FromArgb(255, 255, 140, 5);

    int instanceNumber = 0;
    bool showInstanceNumbers = false;

    StatusLineEventArgs lastStatusLineEvent = null;
    Object statusLineLock = new Object();
    readonly EventWaitHandle statusLineEventHandle = new AutoResetEvent(false);
    readonly EventWaitHandle statusLineEventWakeupHandle = new ManualResetEvent(false);
    Thread statusLineThread;


    public LogTabWindow(string [] fileNames, int instanceNumber, bool showInstanceNumbers)
    {
      InitializeComponent();
      this.startupFileNames = fileNames;
      this.instanceNumber = instanceNumber;
      this.showInstanceNumbers = showInstanceNumbers;

      this.Load += LogTabWindow_Load;

      ConfigManager.Instance.ConfigChanged += ConfigChanged;
      this.hilightGroupList = ConfigManager.Settings.hilightGroupList;

      Rectangle led = new Rectangle(0, 0, 8, 2);
      for (int i = 0; i < leds.Length; ++i)
      {
        this.leds[i] = led;
        led.Offset(0, led.Height + 1);
      }
      this.ledBrushes[0] = new SolidBrush(Color.FromArgb(255, 220, 0, 0));
      this.ledBrushes[1] = new SolidBrush(Color.FromArgb(255, 220, 220, 0));
      this.ledBrushes[2] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
      this.ledBrushes[3] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
      this.ledBrushes[4] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
      this.offLedBrush = new SolidBrush(Color.FromArgb(255, 160, 160, 160));
      this.dirtyLedBrush = new SolidBrush(Color.FromArgb(255, 220, 0, 00));
      this.tailLedBrush[0] = new SolidBrush(Color.FromArgb(255, 50, 100, 250));
      this.tailLedBrush[1] = new SolidBrush(Color.FromArgb(255, 160, 160, 160));
      this.tailLedBrush[2] = new SolidBrush(Color.FromArgb(255, 220, 220, 0));
      this.syncLedBrush = new SolidBrush(Color.FromArgb(255, 250, 145, 30));
      CreateIcons();
      tabStringFormat.LineAlignment = StringAlignment.Center;
      tabStringFormat.Alignment = StringAlignment.Near;

      ToolStripControlHost host = new ToolStripControlHost(this.followTailCheckBox);
      host.Padding = new Padding(20, 0, 0, 0);
      host.BackColor = Color.FromKnownColor(KnownColor.Transparent);
      int index = this.toolStrip4.Items.IndexOfKey("toolStripButtonTail");
      if (index != -1)
      {
        this.toolStrip4.Items.RemoveAt(index);
        this.toolStrip4.Items.Insert(index, host);
      }

      this.dateTimeDragControl.Visible = false;
      this.loadProgessBar.Visible = false;

      // get a reference to the current assembly
      Assembly a = Assembly.GetExecutingAssembly();

      // get a list of resource names from the manifest
      string[] resNames = a.GetManifestResourceNames();

      Bitmap bmp = new Bitmap(GetType(), "Resources.delete-page-red.gif");
      this.deadIcon = System.Drawing.Icon.FromHandle(bmp.GetHicon());
      bmp.Dispose();
      this.Closing += LogTabWindow_Closing;
    }

    void LogTabWindow_Load(object sender, EventArgs e)
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
            AddFileTab(name, false, null, null, false, null);
          }
        }
      }
      if (this.startupFileNames != null)
      {
        LoadFiles(this.startupFileNames, false);
      }
      this.ledThread = new Thread(new ThreadStart(this.LedThreadProc));
      this.ledThread.Start();

      this.statusLineThread = new Thread(new ThreadStart(this.StatusLineThreadFunc));
      this.statusLineThread.Start();

      FillHighlightComboBox();
      FillToolLauncherBar();
#if !DEBUG
      debugToolStripMenuItem.Visible = false;
#endif

    }


    void LogTabWindow_Closing(object sender, CancelEventArgs e)
    {
      try
      {
        this.shouldStop = true;
        this.statusLineEventHandle.Set();
        this.statusLineEventWakeupHandle.Set();
        this.ledThread.Join();
        this.statusLineThread.Join();

        IList<LogWindow> deleteLogWindowList = new List<LogWindow>();
        ConfigManager.Settings.alwaysOnTop = this.TopMost && ConfigManager.Settings.preferences.allowOnlyOneInstance;
        SaveLastOpenFilesList();
        foreach (LogWindow logWindow in this.logWindowList)
        {
          deleteLogWindowList.Add(logWindow);
        }
        foreach (LogWindow logWindow in deleteLogWindowList)
        {
          RemoveAndDisposeLogWindow(logWindow, true);
        }

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

    private void SaveLastOpenFilesList()
    {
      ConfigManager.Settings.lastOpenFilesList.Clear();
      foreach (MdiTabControl.TabPage tabPage in this.tabControl1.TabPages)
      {
        LogWindow logWin = tabPage.Form as LogWindow;
        if (!logWin.IsTempFile)
        {
          ConfigManager.Settings.lastOpenFilesList.Add(logWin.GivenFileName);
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


    public LogWindow AddTempFileTab(string fileName, string title)
    {
      return AddFileTab(fileName, true, title, null, false, null);
    }

    public LogWindow AddFilterTab(FilterPipe pipe, string title, LogWindow.LoadingFinishedFx loadingFinishedFx, ILogLineColumnizer preProcessColumnizer)
    {
      LogWindow logWin = AddFileTab(pipe.FileName, true, title, loadingFinishedFx, false, preProcessColumnizer);
      if (pipe.FilterParams.searchText.Length > 0)
      {
        ToolTip tip = new ToolTip(this.components);
        tip.SetToolTip(((LogWindowData)logWin.Tag).tabPage,
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


    public LogWindow AddFileTab(string givenFileName, bool isTempFile, string title, LogWindow.LoadingFinishedFx loadingFinishedFx, bool forcePersistenceLoading, ILogLineColumnizer preProcessColumnizer)
    {
      string logFileName = FindFilenameForSettings(givenFileName);
      LogWindow win = FindWindowForFile(logFileName);
      if (win != null)
      {
        if (!isTempFile)
          AddToFileHistory(givenFileName);
        SelectTab(win);
        return win;
      }

      EncodingOptions encodingOptions = new EncodingOptions();
      FillDefaultEncodingFromSettings(encodingOptions);
      LogWindow logWindow = new LogWindow(this, logFileName, isTempFile, loadingFinishedFx, forcePersistenceLoading);

      logWindow.GivenFileName = givenFileName;

      if (preProcessColumnizer != null)
      {
        logWindow.ForceColumnizerForLoading(preProcessColumnizer);
      }

      if (isTempFile) {
        logWindow.TempTitleName = title;
        encodingOptions.Encoding = new UnicodeEncoding(false, false);
      }
      AddLogWindow(logWindow, title);
      if (!isTempFile)
        AddToFileHistory(givenFileName);

      LogWindowData data = logWindow.Tag as LogWindowData;
      data.color = this.defaultTabColor;
      setTabColor(data.tabPage, this.defaultTabColor);
      //data.tabPage.BorderColor = this.defaultTabBorderColor;
      if (!isTempFile)
      {
        foreach (ColorEntry colorEntry in ConfigManager.Settings.fileColors)
        {
          if (colorEntry.fileName.ToLower().Equals(logFileName.ToLower()))
          {
            data.color = colorEntry.color;
            setTabColor(data.tabPage, colorEntry.color);
            break;
          }
        }
      }
      if (!isTempFile)
      {
        ToolTip tip = new ToolTip(this.components);
        tip.SetToolTip(data.tabPage, logFileName);
        tip.AutomaticDelay = 40;
        tip.AutoPopDelay = 5000;
        //tip.InitialDelay = 40;
        //tip.ReshowDelay = 40;
        //tip.ShowAlways = true;
        data.toolTip = tip;
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

    private void FillDefaultEncodingFromSettings(EncodingOptions encodingOptions)
    {
      if (ConfigManager.Settings.preferences.defaultEncoding != null)
      {
        try
        {
          encodingOptions.DefaultEncoding = Encoding.GetEncoding(ConfigManager.Settings.preferences.defaultEncoding);
        }
        catch (ArgumentException)
        {
          Logger.logWarn("Encoding " + ConfigManager.Settings.preferences.defaultEncoding + " is not a valid encoding");
          encodingOptions.DefaultEncoding = null;
        }
      }
    }

    public LogWindow AddMultiFileTab(string [] fileNames)
    {
      if (fileNames.Length < 1)
        return null;
      LogWindow logWindow = new LogWindow(this, fileNames[fileNames.Length-1], false, null, false);
      AddLogWindow(logWindow, fileNames[fileNames.Length - 1]);
      this.multiFileToolStripMenuItem.Checked = true;
      this.BeginInvoke(new LoadMultiFilesDelegate(logWindow.LoadFilesAsMulti), new object[] { fileNames, Encoding.Default });
      AddToFileHistory(fileNames[0]);
      return logWindow;
    }

    delegate void LoadFileDelegate(string fileName, EncodingOptions encodingOptions);
    delegate void LoadMultiFilesDelegate(string [] fileName, EncodingOptions encodingOptions);

    delegate void AddFileTabsDelegate(string[] fileNames);

    public void LoadFiles(string[] fileNames)
    {
      this.Invoke(new AddFileTabsDelegate(AddFileTabs), new object[] { fileNames });
    }

    private void AddFileTabs(string[] fileNames)
    {
      foreach (string fileName in fileNames)
      {
        if (fileName != null && fileName.Length > 0)
        {
          if (fileName.EndsWith(".lxj"))
          {
            LoadProject(fileName); 
          }
          else
          {
            AddFileTab(fileName, false, null, null, false, null);
          }
        }
      }
    }

    private void AddLogWindow(LogWindow logWindow, string title)
    {
      //logWindow.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);
      logWindow.Dock = DockStyle.Fill;

      MdiTabControl.TabPage tabPage = this.tabControl1.TabPages.Add(logWindow);

      logWindow.Location = new Point(30, 30);
      Size size = tabPage.Size;
      logWindow.Size = size;
      this.tabControl1.TabPages.set_IndexOf(tabPage, this.tabControl1.TabPages.Count - 1);
      tabPage.Name = title;
      tabPage.ContextMenuStrip = this.tabContextMenuStrip;
      tabPage.MouseClick += tabPage_MouseClick;
      tabPage.TabDoubleClick += tabPage_TabDoubleClick;
      LogWindowData data = new LogWindowData();
      data.diffSum = 0;
      data.tabPage = tabPage;
      logWindow.Tag = data;
      lock (this.logWindowList)
      {
        this.logWindowList.Add(logWindow);
      }
      Icon icon = this.ledIcons[0, 0, 0, 0];
      this.Invoke(new SetTabIconDelegate(SetTabIcon), new object[] { tabPage, icon });
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
      data.tabPage.MouseClick -= tabPage_MouseClick;
      data.tabPage.TabDoubleClick -= tabPage_TabDoubleClick;
      data.tabPage.ContextMenuStrip = null;
      data.tabPage = null;
    }


    class LowercaseStringComparer : IComparer<string>
    {
      public int Compare(string x, string y)
      {
        return x.ToLower().CompareTo(y.ToLower());
      }
    };

    private void AddToFileHistory(string fileName)
    {
      Predicate<string> findName = delegate(string s)
      {
        return s.ToLower().Equals(fileName.ToLower());
      };
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
      foreach (var file in ConfigManager.Settings.fileHistoryList)
      {
        ToolStripItem item = new ToolStripMenuItem(file);
        strip.Items.Add(item);
      }
      strip.ItemClicked += new ToolStripItemClickedEventHandler(history_ItemClicked);
      strip.MouseUp += new MouseEventHandler(strip_MouseUp);
      this.lastUsedToolStripMenuItem.DropDown = strip;
    }


    void strip_MouseUp(object sender, MouseEventArgs e)
    {
      if (sender is ToolStripDropDown)
      {
        AddFileTab(((ToolStripDropDown)sender).Text, false, null, null, false, null);
      }
    }

    void history_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      if (e.ClickedItem.Text != null && e.ClickedItem.Text.Length > 0)
      {
        AddFileTab(e.ClickedItem.Text, false, null, null, false, null);
      }
    }

    void logWindow_Disposed(object sender, EventArgs e)
    {
      LogWindow logWindow = sender as LogWindow;
      if (sender == this.currentLogWindow)
      {
        ChangeCurrentLogWindow(null);
      }
      RemoveLogWindow(logWindow);
      logWindow.Tag = null;
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
      if (this.currentLogWindow == logWindow)
      {
        ChangeCurrentLogWindow(null);
      }
      lock (this.logWindowList)
      {
        this.logWindowList.Remove(logWindow);
      }
      logWindow.Close(dontAsk);
    }


    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }


    private void selectFilterToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow == null)
        return;
      this.CurrentLogWindow.ColumnizerCallbackObject.LineNum = this.CurrentLogWindow.GetCurrentLineNum();
      FilterSelectorForm form = new FilterSelectorForm(PluginRegistry.GetInstance().RegisteredColumnizers, this.CurrentLogWindow.CurrentColumnizer, this.CurrentLogWindow.ColumnizerCallbackObject);
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
                logWindow.Invoke(fx, new object[] { form.SelectedColumnizer });
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
            this.CurrentLogWindow.Invoke(fx, new object[] { form.SelectedColumnizer });
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
        return;
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


    private void ShowHighlightSettingsDialog()
    {
      HilightDialog dlg = new HilightDialog();
      dlg.KeywordActionList = PluginRegistry.GetInstance().RegisteredKeywordActions;
      dlg.Owner = this;
      dlg.TopMost = this.TopMost;
      dlg.HilightGroupList = this.hilightGroupList;
      dlg.PreSelectedGroupName = this.highlightGroupsComboBox.Text;
      DialogResult res = dlg.ShowDialog();
      if (res == DialogResult.OK)
      {
        this.hilightGroupList = dlg.HilightGroupList;
        FillHighlightComboBox();
        ConfigManager.Settings.hilightGroupList = this.hilightGroupList;
        ConfigManager.Save(SettingsFlags.HighlightSettings);
        OnHighlightSettingsChanged();
      }
    }


    private void FillHighlightComboBox()
    {
      string currentGroupName = this.highlightGroupsComboBox.Text;
      this.highlightGroupsComboBox.Items.Clear();
      foreach (HilightGroup group in this.hilightGroupList)
      {
        this.highlightGroupsComboBox.Items.Add(group.GroupName);
        if (group.GroupName.Equals(currentGroupName))
        {
          this.highlightGroupsComboBox.Text = group.GroupName;
        }
      }
    }


    private void searchToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenSearchDialog();
    }


    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenFileDialog();
    }



    public void OpenSearchDialog()
    {
      if (this.CurrentLogWindow == null)
        return;
      SearchDialog dlg = new SearchDialog();
      this.AddOwnedForm(dlg);
      dlg.TopMost = this.TopMost;
      this.searchParams.historyList = ConfigManager.Settings.searchHistoryList;
      dlg.SearchParams = this.searchParams;
      DialogResult res = dlg.ShowDialog();
      if (res == DialogResult.OK)
      {
        this.searchParams = dlg.SearchParams;
        this.searchParams.isFindNext = false;
        this.CurrentLogWindow.StartSearch();
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
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
          }
          catch (SecurityException e)
          {
            Logger.logWarn("Insufficient rights for GetFolderPath(): " + e.Message);
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


    private void LogWindow_DragDrop(object sender, DragEventArgs e)
    {
#if DEBUG
      string[] formats = e.Data.GetFormats();
      string s = "Dropped formats:  ";
      foreach (string format in formats)
      {
        s += format; s+= " , ";
      }
      s = s.Substring(0, s.Length - 3);
      Logger.logInfo(s);
#endif

      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        object o = e.Data.GetData(DataFormats.FileDrop);
        if (o is string[])
        {
          LoadFiles(((string[])o), (e.KeyState & 4) == 4);  // (shift pressed?)
          e.Effect = DragDropEffects.Copy;
        }
      }
    }

    private void LoadFiles(string[] names, bool invertLogic)
    {
      if (names.Length == 1)
      {
        if (names[0].EndsWith(".lxj"))
        {
          LoadProject(names[0]);
          return;
        }
        else
        {
          AddFileTab(names[0], false, null, null, false, null);
          return;
        }
      }

      MultiFileOption option = ConfigManager.Settings.preferences.multiFileOption;
      if (option == MultiFileOption.Ask)
      {
        MultiLoadRequestDialog dlg = new MultiLoadRequestDialog();
        DialogResult res = dlg.ShowDialog();
        if (res == DialogResult.Yes)
          option = MultiFileOption.SingleFiles;
        else if (res == DialogResult.No)
          option = MultiFileOption.MultiFile;
        else
          return;
      }
      else
      {
        if (invertLogic)
        {
          if (option == MultiFileOption.SingleFiles)
            option = MultiFileOption.MultiFile;
          else
            option = MultiFileOption.SingleFiles;
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


    private void LogTabWindow_DragEnter(object sender, DragEventArgs e)
    {
#if DEBUG
      string[] formats = e.Data.GetFormats();
      string s = "Dragging something over LogExpert. Formats:  ";
      foreach (string format in formats)
      {
        s += format; s += " , ";
      }
      s = s.Substring(0, s.Length - 3);
      Logger.logInfo(s);
#endif
    }

    private void LogWindow_DragOver(object sender, DragEventArgs e)
    {

      if (!e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      else
      {
        e.Effect = DragDropEffects.Copy;
      }
    }


    private void timeshiftToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
    {
      if (!this.skipEvents && this.CurrentLogWindow != null)
      {
        this.CurrentLogWindow.SetTimeshiftValue(this.timeshiftMenuTextBox.Text);
        this.timeshiftMenuTextBox.Enabled = this.timeshiftToolStripMenuItem.Checked;
        this.CurrentLogWindow.TimeshiftEnabled(this.timeshiftToolStripMenuItem.Checked, this.timeshiftMenuTextBox.Text);
      }
    }



    private void setColumnizerHistoryEntry(string fileName, ILogLineColumnizer columnizer)
    {
      ColumnizerHistoryEntry entry = findColumnizerHistoryEntry(fileName);
      if (entry != null)
        ConfigManager.Settings.columnizerHistoryList.Remove(entry);
      ConfigManager.Settings.columnizerHistoryList.Add(new ColumnizerHistoryEntry(fileName, columnizer.GetName()));
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
        ConfigManager.Settings.columnizerHistoryList.Remove(entry);   // no valid name -> remove entry
      }
      return null;
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
        this.CurrentLogWindow.ToggleFilterPanel();
    }

    private void filterToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ToggleFilterPanel();
    }


    private void multiFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToggleMultiFile();
      toolStripMenuItem1.HideDropDown();
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

    public LogWindow CurrentLogWindow
    {
      get { return currentLogWindow; }
      set { ChangeCurrentLogWindow(value); }
    }


    public SearchParams SearchParams
    {
      get { return this.searchParams; }
    }

    public Preferences Preferences
    {
      get { return ConfigManager.Settings.preferences; }
    }

    public List<HilightGroup> HilightGroupList
    {
      get { return this.hilightGroupList; }
    }


    private void ChangeCurrentLogWindow(LogWindow newLogWindow)
    {
      LogWindow oldLogWindow = this.currentLogWindow;
      this.currentLogWindow = newLogWindow;
      string titleName = this.showInstanceNumbers ? "LogExpert #" + this.instanceNumber : "LogExpert";

      if (newLogWindow != null)
      {
        newLogWindow.StatusLineEvent += StatusLineEvent;
        newLogWindow.ProgressBarUpdate += ProgressBarUpdate;
        newLogWindow.GuiStateUpdate += GuiStateUpdate;
        if (newLogWindow.IsTempFile)
          this.Text = titleName + " - " + newLogWindow.TempTitleName;
        else
          this.Text = titleName + " - " + newLogWindow.FileName;
        this.multiFileToolStripMenuItem.Checked = this.CurrentLogWindow.IsMultiFile;
        this.multiFileToolStripMenuItem.Enabled = true;
        this.cellSelectModeToolStripMenuItem.Checked = true;
        this.cellSelectModeToolStripMenuItem.Enabled = true;
        this.closeFileToolStripMenuItem.Enabled = true;
        this.searchToolStripMenuItem.Enabled = true;
        this.filterToolStripMenuItem.Enabled = true;
        this.goToLineToolStripMenuItem.Enabled = true;
      }
      else
      {
        this.Text = titleName;
        this.multiFileToolStripMenuItem.Checked = false;
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

      if (oldLogWindow != null)
      {
        oldLogWindow.StatusLineEvent -= StatusLineEvent;
        oldLogWindow.ProgressBarUpdate -= ProgressBarUpdate;
        oldLogWindow.GuiStateUpdate -= GuiStateUpdate;
      }

      
    }

    void GuiStateUpdate(object sender, GuiStateArgs e)
    {
      this.BeginInvoke(new GuiStateUpdateWorkerDelegate(GuiStateUpdateWorker), new object[] { e });
    }


    void GuiStateUpdateWorker(GuiStateArgs e)
    {
      skipEvents = true;
      this.followTailCheckBox.Checked = e.FollowTail;
      this.menuStrip1.Enabled = e.MenuEnabled;
      timeshiftToolStripMenuItem.Enabled = e.TimeshiftPossible;
      timeshiftToolStripMenuItem.Checked = e.TimeshiftEnabled;
      this.timeshiftMenuTextBox.Text = e.TimeshiftText;
      this.timeshiftMenuTextBox.Enabled = e.TimeshiftEnabled;
      this.multiFileToolStripMenuItem.Enabled = e.MultiFileEnabled;  // disabled for temp files
      this.multiFileToolStripMenuItem.Checked = e.IsMultiFileActive;
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
      
      skipEvents = false;
    }


    void ProgressBarUpdate(object sender, ProgressEventArgs e)
    {
      this.Invoke(new ProgressBarEventFx(ProgressBarUpdateWorker), new object[] { e });
    }

    void ProgressBarUpdateWorker(ProgressEventArgs e)
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


    void StatusLineEvent(object sender, StatusLineEventArgs e)
    {
      lock (this.statusLineLock)
      {
        this.lastStatusLineEvent = e;
        this.statusLineEventHandle.Set();
        this.statusLineEventWakeupHandle.Set();
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
              this.BeginInvoke(new StatusLineEventFx(StatusLineEventWorker), new object[] { e });
            }
            catch (ObjectDisposedException)
            { }
          }
        }
      }
    }

    void StatusLineEventWorker(StatusLineEventArgs e)
    {
      //Logger.logDebug("StatusLineEvent: text = " + e.StatusText);
      this.statusLabel.Text = e.StatusText;
      this.linesLabel.Text = "" + e.LineCount + " lines";
      this.sizeLabel.Text = Util.GetFileSizeAsText(e.FileSize);
      this.currentLineLabel.Text = "" + e.CurrentLineNum;
      this.statusStrip1.Refresh();
    }

    private void followTailCheckBox_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.FollowTailChanged(this.followTailCheckBox.Checked, false);
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
          this.currentLogWindow.Close();
        }
      }
      else if (e.KeyCode == Keys.Tab && e.Control)
      {
        SwitchTab(e.Shift);
      }
      else
      {
        if (this.CurrentLogWindow != null)
          this.CurrentLogWindow.LogWindow_KeyDown(sender, e);
      }
    }

    private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.currentLogWindow != null)
        this.currentLogWindow.Close();
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


    public void SwitchTab(bool shiftPressed)
    {
      int index = this.tabControl1.TabPages.SelectedIndex();
      if (shiftPressed)
      {
        index--;
        if (index < 0)
          index = this.tabControl1.TabPages.Count - 1;
        if (index < 0)
          return;
      }
      else
      {
        index++;
        if (index >= this.tabControl1.TabPages.Count)
          index = 0;
      }
      this.tabControl1.TabPages[index].Select();

      //int index = this.tabControl.SelectedIndex;
      //if (shiftPressed)
      //{
      //  index--;
      //  if (index < 0)
      //  {
      //    index = this.tabControl.TabCount - 1;
      //    if (index < 0)
      //      index = 0;
      //  }
      //  this.tabControl.SelectedIndex = index;
      //}
      //else
      //{
      //  index++;
      //  if (index > this.tabControl.TabCount - 1)
      //  {
      //    index = 0;
      //  }
      //  this.tabControl.SelectedIndex = index;
      //}
    }

    private void timeshiftMenuTextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (this.CurrentLogWindow == null)
        return;
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



    private void tabControl1_TabPageChanged(object sender, EventArgs e)
    {
      MdiTabControl.TabPage currentTab = (MdiTabControl.TabPage)this.tabControl1.TabPages.SelectedTab();
      if (currentTab != null)
      {
        this.CurrentLogWindow = (LogWindow)currentTab.Form;
        //currentTab.IsActiveTab = true;
        //this.CurrentLogWindow = currentTab.LogWindow;
        this.CurrentLogWindow.LogWindowActivated();
      }

    }

    // tailState: 0,1,2 = on/off/off by Trigger
    // syncMode: 0 = normal (no), 1 = time synced 
    private Icon CreateLedIcon(int level, bool dirty, int tailState, int syncMode)
    {
      Rectangle iconRect = this.leds[0];
      iconRect.Height = this.leds[this.leds.Length - 1].Bottom;
      iconRect.Width = iconRect.Right + 6;
      Bitmap bmp = new Bitmap(iconRect.Width, iconRect.Height);
      Graphics gfx = Graphics.FromImage(bmp);

      for (int i = 0; i < this.leds.Length; ++i)
      {
        Rectangle ledRect = this.leds[i];
        if (level >= this.leds.Length - i)
          gfx.FillRectangle(this.ledBrushes[i], ledRect);
        else
          gfx.FillRectangle(this.offLedBrush, ledRect);
      }

      Rectangle lastLed = this.leds[this.leds.Length - 1];
      Rectangle dirtyLed = new Rectangle(lastLed.Right + 2, lastLed.Bottom - 4, 4, 4);
      Rectangle tailLed = new Rectangle(dirtyLed.Location, dirtyLed.Size);
      tailLed.Offset(0, -5);
      Rectangle syncLed = new Rectangle(tailLed.Location, dirtyLed.Size);
      syncLed.Offset(0, -5);

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
      for (int syncMode = 0; syncMode <= 1; syncMode++)   // LED indicating time synced tabs
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

    void FileSizeChanged(object sender, LogEventArgs e)
    {
      if (sender.GetType().IsAssignableFrom(typeof(LogWindow)))
      {
        int diff = e.LineCount - e.PrevLineCount;
        if (diff < 0)
        {
          diff = DIFF_MAX;
          return;
        }
        LogWindowData data = ((LogWindow)sender).Tag as LogWindowData;
        if (data != null)
        {
          lock (data)
          {
            data.diffSum = data.diffSum + diff;
            if (data.diffSum > DIFF_MAX)
              data.diffSum = DIFF_MAX;
          }
          if (this.tabControl1.TabPages.SelectedTab() != null && 
              this.tabControl1.TabPages.SelectedTab().Form != sender
              || data.tailState != 0)
          {
            data.dirty = true;
          }
          Icon icon = GetIcon(diff, data);
          this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
        }
      }
    }


    void logWindow_FileNotFound(object sender, EventArgs e)
    {
      this.Invoke(new FileNotFoundDelegate(FileNotFound), new object[] { sender });
    }

    delegate void FileNotFoundDelegate(LogWindow logWin);
    private void FileNotFound(LogWindow logWin)
    {
      LogWindowData data = logWin.Tag as LogWindowData;
      this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, this.deadIcon });
      this.dateTimeDragControl.Visible = false;
    }


    void logWindow_FileRespawned(object sender, EventArgs e)
    {
      this.Invoke(new FileRespawnedDelegate(FileRespawned), new object[] { sender });
    }

    delegate void FileRespawnedDelegate(LogWindow logWin);
    private void FileRespawned(LogWindow logWin)
    {
      LogWindowData data = logWin.Tag as LogWindowData;
      Icon icon = GetIcon(0, data);
      this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
    }

    void logWindow_FilterListChanged(object sender, FilterListChangedEventArgs e)
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


    void logWindow_CurrentHighlightGroupChanged(object sender, CurrentHighlightGroupChangedEventArgs e)
    {
      OnHighlightSettingsChanged();
      ConfigManager.Settings.hilightGroupList = this.HilightGroupList;
      ConfigManager.Save(SettingsFlags.HighlightSettings);
    }


    private void ShowLedPeak(LogWindow logWin)
    {
      LogWindowData data = logWin.Tag as LogWindowData;
      lock (data)
      {
        data.diffSum = DIFF_MAX;
      }
      Icon icon = GetIcon(data.diffSum, data);
      this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
    }


    void TailFollowed(object sender, EventArgs e)
    {
      if (this.tabControl1.TabPages.SelectedTab() == null)
        return;
      if (sender.GetType().IsAssignableFrom(typeof(LogWindow)))
      {
        if (this.tabControl1.TabPages.SelectedTab().Form == sender)
        {
          LogWindowData data = ((LogWindow)sender).Tag as LogWindowData;
          data.dirty = false;
          Icon icon = GetIcon(data.diffSum, data);
          this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
        }
      }
    }

    void logWindow_SyncModeChanged(object sender, SyncModeEventArgs e)
    {
      if (!this.Disposing)
      {
        LogWindowData data = ((LogWindow)sender).Tag as LogWindowData;
        data.syncMode = e.IsTimeSynced ? 1 : 0;
        Icon icon = GetIcon(data.diffSum, data);
        this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
      }
      else
      {
        Logger.logWarn("Received SyncModeChanged event while disposing. Event ignored.");
      }
    }


    private int GetLevelFromDiff(int diff)
    {
      if (diff > 60)
        diff = 60;
      int level = diff / 10;
      if (diff > 0 && level == 0)
        level = 2;
      else if (level == 0)
        level = 1;
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
            LogWindowData data = logWindow.Tag as LogWindowData;
            if (data.diffSum > 0)
            {
              data.diffSum -= 10;
              if (data.diffSum < 0)
              {
                data.diffSum = 0;
              }
              Icon icon = GetIcon(data.diffSum, data);
              this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
            }
          }
        }
      }
    }

    private void SetTabIcon(MdiTabControl.TabPage tabPage, Icon icon)
    {
      if (tabPage.Form != null)
      {
        tabPage.Icon = icon;
      }
    }

    private Icon GetIcon(int diff, LogWindowData data)
    {
      Icon icon = this.ledIcons[GetLevelFromDiff(diff), data.dirty ? 1 : 0, this.Preferences.showTailState ? data.tailState : 3, data.syncMode];
      return icon;
    }

    public void ScrollAllTabsToTimestamp(DateTime timestamp, LogWindow senderWindow)
    {
      lock (this.logWindowList)
      {
        foreach(LogWindow logWindow in this.logWindowList)
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

    private void toggleBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ToggleBookmark();
    }

    private void jumpToNextToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.JumpNextBookmark();
    }

    private void jumpToPrevToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.JumpPrevBookmark();
    }

    private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ChangeEncoding(Encoding.ASCII);
    }

    private void aNSIToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ChangeEncoding(Encoding.Default);
    }

    private void uTF8ToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ChangeEncoding(new UTF8Encoding(false));
    }

    private void uTF16ToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ChangeEncoding(Encoding.Unicode);
    }

    private void iSO88591ToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ChangeEncoding(Encoding.GetEncoding("iso-8859-1"));
    }


    private void RefreshEncodingMenuBar(Encoding encoding)
    {
      this.aSCIIToolStripMenuItem.Checked = false;
      this.aNSIToolStripMenuItem.Checked = false;
      this.uTF8ToolStripMenuItem.Checked = false;
      this.uTF16ToolStripMenuItem.Checked = false;
      this.iSO88591ToolStripMenuItem.Checked = false;
      if (encoding == null)
        return;
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

    private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
      {
        LogWindowData data = this.currentLogWindow.Tag as LogWindowData;
        Icon icon = GetIcon(0, data);
        this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
        this.CurrentLogWindow.Reload();
      }
    }

    private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenSettings(0);
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
      Logger.logInfo("The preferences have changed");
      ApplySettings(ConfigManager.Settings, flags);

      lock (this.logWindowList)
      {
        foreach (LogWindow logWindow in this.logWindowList)
        {
          logWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, flags);
        }
      }
      this.hilightGroupList = ConfigManager.Settings.hilightGroupList;
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
    }

    private void SetTabIcons(Preferences preferences)
    {
      this.tailLedBrush[0] = new SolidBrush(preferences.showTailColor);
      CreateIcons();
      lock (this.logWindowList)
      {
        foreach (LogWindow logWindow in this.logWindowList)
        {
          LogWindowData data = logWindow.Tag as LogWindowData;
          Icon icon = GetIcon(data.diffSum, data);
          this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
        }
      }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);

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

    private void ToolButtonClick(ToolEntry toolEntry)
    {
      if (toolEntry.cmd == null || toolEntry.cmd.Length == 0)
      {
        OpenSettings(2);
        return;
      }
      if (this.CurrentLogWindow != null)
      {
        string line = this.CurrentLogWindow.GetCurrentLine();
        string fileName = this.CurrentLogWindow.GetCurrentFileName();
        if (line != null && fileName != null)
        {
          ArgParser parser = new ArgParser(toolEntry.args);
          string argLine = parser.BuildArgs(line, this.CurrentLogWindow.GetRealLineNum() + 1, fileName, this);
          if (argLine != null)
          {
            StartTool(toolEntry.cmd, argLine, toolEntry.sysout, toolEntry.columnizerName, toolEntry.workingDir);
          }
        }
      }
    }

    private void StartTool(string cmd, string args, bool sysoutPipe, string columnizerName, string workingDir)
    {
      if (cmd == null || cmd.Length == 0)
        return;
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
        ILogLineColumnizer columnizer = Util.FindColumnizerByName(columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
        if (columnizer == null)
          columnizer = PluginRegistry.GetInstance().RegisteredColumnizers[0];

        Logger.logInfo("Starting external tool with sysout redirection: " + cmd + " " + args);
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        //process.OutputDataReceived += pipe.DataReceivedEventHandler;
        try
        {
          process.Start();
        }
        catch (Win32Exception e)
        {
          Logger.logError(e.Message);
          MessageBox.Show(e.Message);
          return;
        }
        SysoutPipe pipe = new SysoutPipe(process.StandardOutput);
        LogWindow logWin = AddTempFileTab(pipe.FileName, this.CurrentLogWindow.IsTempFile ? this.CurrentLogWindow.TempTitleName : Util.GetNameFromPath(this.CurrentLogWindow.FileName) + "->E");
        logWin.ForceColumnizer(columnizer);
        process.Exited += pipe.ProcessExitedEventHandler;
        //process.BeginOutputReadLine();
      }
      else
      {
        Logger.logInfo("Starting external tool: " + cmd + " " + args);
        try
        {
          startInfo.UseShellExecute = false;
          process.Start();
        }
        catch (Exception e)
        {
          Logger.logError(e.Message);
          MessageBox.Show(e.Message);
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
              ILogLineColumnizer columnizer = Util.FindColumnizerByName(entry.columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
              return columnizer;
            }
          }
          catch (ArgumentException e)
          {
            Logger.logError("RegEx-error while finding columnizer: " + e.Message);
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
          catch (ArgumentException)
          {
            // occurs on invalid regex patterns
          }
        }
      }
      return null;
    }


    public void SelectTab(LogWindow logWindow)
    {
      if (logWindow.Tag != null)
      {
        ((LogWindowData)logWindow.Tag).tabPage.Select();
      }

    }

    private void showBookmarkListToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
      {
        this.CurrentLogWindow.ToggleBookmarkView();
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
        this.CurrentLogWindow.ToggleFilterPanel();
    }

    private void toolStripButtonBookmark_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.ToggleBookmark();
    }

    private void toolStripButtonUp_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.JumpPrevBookmark();
    }

    private void toolStripButtonDown_Click(object sender, EventArgs e)
    {
      if (this.CurrentLogWindow != null)
        this.CurrentLogWindow.JumpNextBookmark();
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
    }

    // ==================================================================
    // Tab context menu stuff
    // ==================================================================

    private void closeThisTabToolStripMenuItem_Click(object sender, EventArgs e)
    {
      MdiTabControl.TabPage tabPage = this.tabContextMenuStrip.SourceControl as MdiTabControl.TabPage;
      ((Form)tabPage.Form).Close();
    }

    private void closeOtherTabsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      MdiTabControl.TabPage tabPage = this.tabContextMenuStrip.SourceControl as MdiTabControl.TabPage;
      Form thisForm = ((Form)tabPage.Form);
      IList<Form> closeList = new List<Form>();
      lock (this.logWindowList)
      {
        foreach (LogWindow logWin in this.logWindowList)
        {
          if (logWin != thisForm)
          {
            closeList.Add(logWin);
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
      IList<Form> closeList = new List<Form>();
      lock (this.logWindowList)
      {
        foreach (LogWindow logWin in this.logWindowList)
        {
          closeList.Add(logWin);
        }
      }
      foreach (Form form in closeList)
      {
        form.Close();
      }
    }

    void tabPage_MouseClick(object sender, MouseEventArgs e)
    {
      MdiTabControl.TabPage tabPage = sender as MdiTabControl.TabPage;
      if (e.Button == MouseButtons.Middle)
      {
        ((Form)tabPage.Form).Close();
      }
      else
      {
        LogWindowData data = ((Form) tabPage.Form).Tag as LogWindowData;
        ToolTip tip = new ToolTip(this.components);
        if (data.toolTip != null)
        {
          tip.SetToolTip(tabPage, data.toolTip.GetToolTip(tabPage));
          tip.AutomaticDelay = data.toolTip.AutomaticDelay;
          tip.AutoPopDelay = data.toolTip.AutoPopDelay;
          data.toolTip = tip;
        }
      }
    }

    private void tabColorToolStripMenuItem_Click(object sender, EventArgs e)
    {
      MdiTabControl.TabPage tabPage = this.tabContextMenuStrip.SourceControl as MdiTabControl.TabPage;
      LogWindow logWindow = ((LogWindow)tabPage.Form);

      LogWindowData data = logWindow.Tag as LogWindowData;
      if (data == null)
        return;

      ColorDialog dlg = new ColorDialog();
      dlg.Color = data.color;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        data.color = dlg.Color;
        setTabColor(tabPage, data.color);

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

    private void setTabColor(MdiTabControl.TabPage tabPage, Color color)
    {
      tabPage.BackLowColor = color;
      tabPage.BackLowColorDisabled = Color.FromArgb(255,
        Math.Max(0, color.R - 50),
        Math.Max(0, color.G - 50),
        Math.Max(0, color.B - 50)
        );
      //tabPage.BackHighColor = Color.FromArgb(255,
      //  Math.Min(255, color.R + 70),
      //  Math.Min(255, color.G + 70),
      //  Math.Min(255, color.B + 70)
      //  );
      //tabPage.BackHighColorDisabled = Color.FromArgb(255, 190, 190, 190);
    }


    public void SetForeground()
    {
      SetForegroundWindow(this.Handle);
      if (this.WindowState == FormWindowState.Minimized)
      {
        if (this.wasMaximized)
        {
          this.WindowState = FormWindowState.Maximized;
        }
        else
        {
          this.WindowState = FormWindowState.Normal;
        }
      }
    }

    [DllImport("User32.dll")]
    public static extern Int32 SetForegroundWindow(IntPtr hWnd);


    // called from LogWindow when follow tail was changed
    public void FollowTailChanged(LogWindow logWindow, bool isEnabled, bool offByTrigger)
    {
      LogWindowData data = logWindow.Tag as LogWindowData;
      if (data == null)
        return;
      if (isEnabled)
      {
        data.tailState = 0;
      }
      else
      {
        data.tailState = offByTrigger ? 2 : 1;
      }
      if (this.Preferences.showTailState)
      {
        Icon icon = GetIcon(data.diffSum, data);
        this.BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { data.tabPage, icon });
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
          foreach (MdiTabControl.TabPage tabPage in this.tabControl1.TabPages)
          {
            LogWindow logWindow = tabPage.Form as LogWindow;
            string persistenceFileName = logWindow.SavePersistenceData(true);
            if (persistenceFileName != null)
            {
              fileNames.Add(persistenceFileName);
            }
          }
        }
        ProjectData projectData = new ProjectData();
        projectData.memberList = fileNames;
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
        LoadProject(projectFileName);
      }
    }

    private void LoadProject(string projectFileName)
    {
      ProjectData projectData = ProjectPersister.LoadProjectData(projectFileName);
      if (projectData != null)
      {
        foreach (string fileName in projectData.memberList)
        {
          AddFileTab(fileName, false, null, null, true, null);
        }
      }
    }

    void tabPage_TabDoubleClick(object sender, EventArgs e)
    {
      handleTabDoubleClick(sender);
    }

    private void handleTabDoubleClick(object sender)
    {
      MdiTabControl.TabPage tabPage = sender as MdiTabControl.TabPage;
      TextBox textBox = new TextBox();
      textBox.Text = (tabPage.Form as LogWindow).Text;
      textBox.KeyPress += delegate(object keySender, KeyPressEventArgs keyEvent)
        {
          TextBox box = keySender as TextBox;
          switch (keyEvent.KeyChar)
          {
            case (char)13:
              (tabPage.Form as LogWindow).Text = box.Text;
              box.Hide();
              tabPage.Controls.Remove(box);
              break;
            case (char)27:
              box.Hide();
              tabPage.Controls.Remove(box);
              break;
          }
        };
      textBox.LostFocus += delegate(object keySender2, EventArgs keyEvent2)
         {
           TextBox box = keySender2 as TextBox;
           box.Hide();
           tabPage.Controls.Remove(box);
         };
      textBox.Parent = tabPage;
      Size size = new Size(tabPage.Size.Width, tabPage.Size.Height);
      size.Height = size.Height - 2;
      size.Width = size.Width - (15 + 26);
      textBox.Size = size;
      //Point location = new Point(tabPage.Location.X, tabPage.Location.Y);
      Point location = new Point(0, 0);
      location.Offset(19, 1);
      textBox.Location = location;
      tabPage.Controls.Add(textBox);
      textBox.Parent = tabPage;
      textBox.BringToFront();
      textBox.Show();
      textBox.Focus();
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
      MdiTabControl.TabPage tabPage = this.tabContextMenuStrip.SourceControl as MdiTabControl.TabPage;
      LogWindow logWindow = ((LogWindow)tabPage.Form);
      Clipboard.SetText(logWindow.Title);
    }

    private void findInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
    {
      MdiTabControl.TabPage tabPage = this.tabContextMenuStrip.SourceControl as MdiTabControl.TabPage;
      LogWindow logWindow = ((LogWindow)tabPage.Form);

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


    public delegate void HighlightSettingsChangedEventHandler(object sender, EventArgs e);
    public event HighlightSettingsChangedEventHandler HighlightSettingsChanged;
    protected void OnHighlightSettingsChanged()
    {
      if (HighlightSettingsChanged != null)
      {
        HighlightSettingsChanged(this, new EventArgs());
      }
    }


    internal HilightGroup FindHighlightGroup(string groupName)
    {
      lock (this.hilightGroupList)
      {
        foreach (HilightGroup group in this.hilightGroupList)
        {
          if (group.GroupName.Equals(groupName))
          {
            return group;
          }
        }
        return null;
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

    //public Settings Settings
    //{
    //  get { return ConfigManager.Settings; }
    //}

    public ILogExpertProxy LogExpertProxy
    {
      get { return this.logExpertProxy; }
      set { this.logExpertProxy = value; }
    }


    public void NotifySettingsChanged(Object cookie, SettingsFlags flags)
    {
      if (cookie != this)
      {
        NotifyWindowsForChangedPrefs(flags);
      }
    }

    void ConfigChanged(object sender, ConfigChangedEventArgs e)
    {
      if (this.LogExpertProxy != null)
      {
        this.NotifySettingsChanged(null, e.Flags);
      }
    }

    public IList<WindowFileEntry> GetListOfOpenFiles()
    {
      IList<WindowFileEntry> list = new List<WindowFileEntry>();
      lock (this.logWindowList)
      {
        foreach (LogWindow logWindow in this.logWindowList)
        {
          list.Add(new WindowFileEntry(logWindow));
        }
      }
      return list;
    }

    private void FillToolLauncherBar()
    {
      char[] labels = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
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

    private void runGC()
    {
      Logger.logInfo("Running GC. Used mem before: " + GC.GetTotalMemory(false).ToString("N0"));
      GC.Collect();
      Logger.logInfo("GC done.    Used mem after:  " + GC.GetTotalMemory(true).ToString("N0"));
    }

    private void dumpGCInfo()
    {
      Logger.logInfo("-------- GC info -----------");
      Logger.logInfo("Used mem: " + GC.GetTotalMemory(false).ToString("N0"));
      for (int i = 0; i < GC.MaxGeneration; ++i )
      {
        Logger.logInfo("Generation " + i + " collect count: " + GC.CollectionCount(i));
      }
      Logger.logInfo("----------------------------");
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

    private delegate void ExceptionFx();

    private void throwExceptionbackgroundThToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ExceptionFx fx = new ExceptionFx(throwExceptionFx);
      fx.BeginInvoke(null, null);
    }

    private void throwExceptionFx()
    {
      throw new Exception("This is a test exception thrown by an async delegate");
    }

    private void throwExceptionbackgroundThreadToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Thread thread = new Thread(new ThreadStart(throwExceptionThreadFx));
      thread.Start();
    }

    private void throwExceptionThreadFx()
    {
      throw new Exception("This is a test exception thrown by a background thread");
    }

    private void warnToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Logger.GetLogger().LogLevel = Logger.Level.WARN;
    }

    private void infoToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Logger.GetLogger().LogLevel = Logger.Level.INFO;
    }

    private void debugToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      Logger.GetLogger().LogLevel = Logger.Level.DEBUG;
    }

    private void loglevelToolStripMenuItem_Click(object sender, EventArgs e)
    {

    }

    private void loglevelToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
    {
      warnToolStripMenuItem.Checked = Logger.GetLogger().LogLevel == Logger.Level.WARN;
      infoToolStripMenuItem.Checked = Logger.GetLogger().LogLevel == Logger.Level.INFO;
      debugToolStripMenuItem1.Checked = Logger.GetLogger().LogLevel == Logger.Level.DEBUG;

    }

    private void disableWordHighlightModeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DebugOptions.disableWordHighlight = disableWordHighlightModeToolStripMenuItem.Checked;
      if (this.currentLogWindow != null)
      {
        this.currentLogWindow.RefreshAllGrids();
      }
    }

    private void multifileMaskToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.currentLogWindow != null)
      {
        this.currentLogWindow.ChangeMultifileMask();
      }

    }

    private void toolStripMenuItem3_Click(object sender, EventArgs e)
    {
      ToggleMultiFile();
    }


    //private void openSftpToolStripMenuItem_Click(object sender, EventArgs e)
    //{
    //  JSch jsch = new JSch();
    //  Session session = jsch.getSession("hagen", "localhost");
    //  UserInfo userInfo = new MyUserInfo("test1");
    //  session.setUserInfo(userInfo);
    //  session.connect();
    //  Channel channel = session.openChannel("sftp");
    //  channel.connect();
    //  ChannelSftp sftpChannel = (ChannelSftp)channel;

    //  ILogFileInfo fileInfo = new SftpLogFileInfo(sftpChannel, "/workspaces/test/dlltest/Debug/lanoffline.log");
      
    //}

   


  }


}
