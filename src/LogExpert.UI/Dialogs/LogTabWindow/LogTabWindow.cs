using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

using ColumnizerLib;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Classes.Persister;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;
using LogExpert.Core.EventArguments;
using LogExpert.Core.Interface;
using LogExpert.Dialogs;
using LogExpert.Entities;
using LogExpert.PluginRegistry.FileSystem;
using LogExpert.UI.Dialogs;
using LogExpert.UI.Entities;
using LogExpert.UI.Extensions;
using LogExpert.UI.Extensions.LogWindow;

using NLog;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.UI.Controls.LogTabWindow;

// Data shared over all LogTabWindow instances
//TODO: Can we get rid of this class?
[SupportedOSPlatform("windows")]
internal partial class LogTabWindow : Form, ILogTabWindow
{
    #region Fields

    private const int MAX_COLUMNIZER_HISTORY = 40;
    private const int MAX_COLOR_HISTORY = 40;
    private const int DIFF_MAX = 100;
    private const int MAX_FILE_HISTORY = 10;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly Icon _deadIcon;

    private readonly Color _defaultTabColor = Color.FromArgb(255, 192, 192, 192);
    private readonly Brush _dirtyLedBrush;

    private readonly int _instanceNumber;
    private readonly Brush[] _ledBrushes = new Brush[5];
    private readonly Icon[,,,] _ledIcons = new Icon[6, 2, 4, 2];

    private readonly Rectangle[] _leds = new Rectangle[5];

    private readonly IList<LogWindow.LogWindow> _logWindowList = [];
    private readonly Brush _offLedBrush;
    private readonly bool _showInstanceNumbers;

    private readonly string[] _startupFileNames;

    private readonly EventWaitHandle _statusLineEventHandle = new AutoResetEvent(false);
    private readonly EventWaitHandle _statusLineEventWakeupHandle = new ManualResetEvent(false);
    private readonly Brush _syncLedBrush;

    [SupportedOSPlatform("windows")]
    private readonly StringFormat _tabStringFormat = new();

    private readonly Brush[] _tailLedBrush = new Brush[3];

    private BookmarkWindow _bookmarkWindow;

    private LogWindow.LogWindow _currentLogWindow;
    private bool _firstBookmarkWindowShow = true;

    private Thread _ledThread;

    //Settings settings;

    private bool _shouldStop;

    private bool _skipEvents;

    private bool _wasMaximized;

    #endregion

    #region cTor

    [SupportedOSPlatform("windows")]
    public LogTabWindow (string[] fileNames, int instanceNumber, bool showInstanceNumbers, IConfigManager configManager)
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ConfigureDockPanel();

        ApplyTextResources();

        ConfigManager = configManager;

        //Fix MainMenu and externalToolsToolStrip.Location, if the location has been changed in the designer
        mainMenuStrip.Location = new Point(0, 0);
        externalToolsToolStrip.Location = new Point(0, 54);

        _startupFileNames = fileNames;
        _instanceNumber = instanceNumber;
        _showInstanceNumbers = showInstanceNumbers;

        Load += OnLogTabWindowLoad;

        ConfigManager.ConfigChanged += OnConfigChanged;
        HighlightGroupList = configManager.Settings.Preferences.HighlightGroupList;

        Rectangle led = new(0, 0, 8, 2);

        for (var i = 0; i < _leds.Length; ++i)
        {
            _leds[i] = led;
            led.Offset(0, led.Height + 0);
        }

        var grayAlpha = 50;

        _ledBrushes[0] = new SolidBrush(Color.FromArgb(255, 220, 0, 0));
        _ledBrushes[1] = new SolidBrush(Color.FromArgb(255, 220, 220, 0));
        _ledBrushes[2] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
        _ledBrushes[3] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
        _ledBrushes[4] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));

        _offLedBrush = new SolidBrush(Color.FromArgb(grayAlpha, 160, 160, 160));

        _dirtyLedBrush = new SolidBrush(Color.FromArgb(255, 220, 0, 00));

        _tailLedBrush[0] = new SolidBrush(Color.FromArgb(255, 50, 100, 250)); // Follow tail: blue-ish
        _tailLedBrush[1] = new SolidBrush(Color.FromArgb(grayAlpha, 160, 160, 160)); // Don't follow tail: gray
        _tailLedBrush[2] = new SolidBrush(Color.FromArgb(255, 220, 220, 0)); // Stop follow tail (trigger): yellow-ish

        _syncLedBrush = new SolidBrush(Color.FromArgb(255, 250, 145, 30));

        CreateIcons();

        _tabStringFormat.LineAlignment = StringAlignment.Center;
        _tabStringFormat.Alignment = StringAlignment.Near;

        ToolStripControlHost host = new(checkBoxFollowTail)
        {
            Padding = new Padding(20, 0, 0, 0),
            BackColor = Color.FromKnownColor(KnownColor.Transparent)
        };

        var index = buttonToolStrip.Items.IndexOfKey("toolStripButtonTail");

        encodingASCIIToolStripMenuItem.Text = Encoding.ASCII.HeaderName;
        encodingANSIToolStripMenuItem.Text = Encoding.Default.HeaderName;
        encodingISO88591toolStripMenuItem.Text = Encoding.GetEncoding("iso-8859-1").HeaderName;
        encodingUTF8toolStripMenuItem.Text = Encoding.UTF8.HeaderName;
        encodingUTF16toolStripMenuItem.Text = Encoding.Unicode.HeaderName;

        if (index != -1)
        {
            buttonToolStrip.Items.RemoveAt(index);
            buttonToolStrip.Items.Insert(index, host);
        }

        dragControlDateTime.Visible = false;
        loadProgessBar.Visible = false;

        using var bmp = Resources.Deceased;
        _deadIcon = Icon.FromHandle(bmp.GetHicon());

        FormClosing += OnLogTabWindowFormClosing;

        InitToolWindows();
    }

    #endregion

    #region Delegates

    private delegate void AddFileTabsDelegate (string[] fileNames);

    private delegate void ExceptionFx ();

    private delegate void FileNotFoundDelegate (LogWindow.LogWindow logWin);

    private delegate void FileRespawnedDelegate (LogWindow.LogWindow logWin);

    public delegate void HighlightSettingsChangedEventHandler (object sender, EventArgs e);

    private delegate void LoadMultiFilesDelegate (string[] fileName, EncodingOptions encodingOptions);

    private delegate void SetColumnizerFx (ILogLineMemoryColumnizer columnizer);

    private delegate void SetTabIconDelegate (LogWindow.LogWindow logWindow, Icon icon);

    #endregion

    #region Events

    public event HighlightSettingsChangedEventHandler HighlightSettingsChanged;

    #endregion

    #region Properties

    [SupportedOSPlatform("windows")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public LogWindow.LogWindow CurrentLogWindow
    {
        get => _currentLogWindow;
        set => ChangeCurrentLogWindow(value);
    }

    public SearchParams SearchParams { get; private set; } = new SearchParams();

    public Preferences Preferences => ConfigManager.Settings.Preferences;

    public List<HighlightGroup> HighlightGroupList { get; private set; } = [];

    //public Settings Settings
    //{
    //  get { return ConfigManager.Settings; }
    //}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public ILogExpertProxy LogExpertProxy { get; set; }

    public IConfigManager ConfigManager { get; }

    #endregion

    #region Internals

    internal HighlightGroup FindHighlightGroup (string groupName)
    {
        lock (HighlightGroupList)
        {
            foreach (var group in HighlightGroupList)
            {
                if (group.GroupName.Equals(groupName, StringComparison.Ordinal))
                {
                    return group;
                }
            }

            return null;
        }
    }

    #endregion

    private class LogWindowData
    {
        #region Fields

        // public MdiTabControl.TabPage tabPage;

        public Color Color { get; set; } = Color.FromKnownColor(KnownColor.Gray);

        public int DiffSum { get; set; }

        public bool Dirty { get; set; }

        // tailState:
        /// <summary>
        /// 0 = on<br></br>
        /// 1 = off<br></br>
        /// 2 = off by Trigger<br></br>
        /// </summary>
        public int TailState { get; set; }

        public ToolTip ToolTip { get; set; }

        /// <summary>
        /// 0 = off<br></br>
        /// 1 = timeSynced
        /// </summary>
        public int SyncMode { get; set; }

        #endregion
    }

    #region Public methods

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddTempFileTab (string fileName, string title)
    {
        return AddFileTab(fileName, true, title, false, null);
    }

    private void ConfigureDockPanel ()
    {
        var autoHideStripSkin1 = new AutoHideStripSkin();
        var dockPanelGradient1 = new DockPanelGradient();
        var tabGradient1 = new TabGradient();
        var dockPaneStripSkin1 = new DockPaneStripSkin();
        var dockPaneStripGradient1 = new DockPaneStripGradient();
        var tabGradient2 = new TabGradient();
        var dockPanelGradient2 = new DockPanelGradient();
        var tabGradient3 = new TabGradient();
        var dockPaneStripToolWindowGradient1 = new DockPaneStripToolWindowGradient();
        var tabGradient4 = new TabGradient();
        var tabGradient5 = new TabGradient();
        var dockPanelGradient3 = new DockPanelGradient();
        var tabGradient6 = new TabGradient();
        var tabGradient7 = new TabGradient();

        dockPanelGradient1.EndColor = SystemColors.Control;
        dockPanelGradient1.StartColor = SystemColors.Control;
        autoHideStripSkin1.DockStripGradient = dockPanelGradient1;
        tabGradient1.EndColor = SystemColors.Control;
        tabGradient1.StartColor = SystemColors.Control;
        tabGradient1.TextColor = SystemColors.ControlText;
        autoHideStripSkin1.TabGradient = tabGradient1;
        autoHideStripSkin1.TextFont = new Font("Segoe UI", 9F);
        tabGradient2.EndColor = SystemColors.Control;
        tabGradient2.StartColor = SystemColors.Control;
        tabGradient2.TextColor = SystemColors.ControlText;
        dockPaneStripGradient1.ActiveTabGradient = tabGradient2;
        dockPanelGradient2.EndColor = SystemColors.Control;
        dockPanelGradient2.StartColor = SystemColors.Control;
        dockPaneStripGradient1.DockStripGradient = dockPanelGradient2;
        tabGradient3.EndColor = SystemColors.ControlLight;
        tabGradient3.StartColor = SystemColors.ControlLight;
        tabGradient3.TextColor = SystemColors.ControlText;
        dockPaneStripGradient1.InactiveTabGradient = tabGradient3;
        dockPaneStripSkin1.DocumentGradient = dockPaneStripGradient1;
        dockPaneStripSkin1.TextFont = new Font("Segoe UI", 9F);
        tabGradient4.EndColor = SystemColors.ActiveCaption;
        tabGradient4.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
        tabGradient4.StartColor = SystemColors.GradientActiveCaption;
        tabGradient4.TextColor = SystemColors.ActiveCaptionText;
        dockPaneStripToolWindowGradient1.ActiveCaptionGradient = tabGradient4;
        tabGradient5.EndColor = SystemColors.Control;
        tabGradient5.StartColor = SystemColors.Control;
        tabGradient5.TextColor = SystemColors.ControlText;
        dockPaneStripToolWindowGradient1.ActiveTabGradient = tabGradient5;
        dockPanelGradient3.EndColor = SystemColors.ControlLight;
        dockPanelGradient3.StartColor = SystemColors.ControlLight;
        dockPaneStripToolWindowGradient1.DockStripGradient = dockPanelGradient3;
        tabGradient6.EndColor = SystemColors.InactiveCaption;
        tabGradient6.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
        tabGradient6.StartColor = SystemColors.GradientInactiveCaption;
        tabGradient6.TextColor = SystemColors.InactiveCaptionText;
        dockPaneStripToolWindowGradient1.InactiveCaptionGradient = tabGradient6;
        tabGradient7.EndColor = Color.Transparent;
        tabGradient7.StartColor = Color.Transparent;
        tabGradient7.TextColor = SystemColors.Control;
        dockPaneStripToolWindowGradient1.InactiveTabGradient = tabGradient7;
        dockPaneStripSkin1.ToolWindowGradient = dockPaneStripToolWindowGradient1;
        dockPanel.Theme = new VS2015LightTheme();
        dockPanel.Theme.Skin.DockPaneStripSkin = dockPaneStripSkin1;
        dockPanel.Theme.Skin.AutoHideStripSkin = autoHideStripSkin1;
        dockPanel.ActiveAutoHideContent = null;
        dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
    }

    private void ApplyTextResources ()
    {

        mainMenuStrip.Text = Resources.LogTabWindow_UI_MenuStrip_MainMenu;
        Text = Resources.LogExpert_Common_UI_Title_LogExpert;
        checkBoxHost.AccessibleName = Resources.LogTabWindow_UI_CheckBox_ToolTip_checkBoxHost;

        ApplyStatusStripResources();
        ApplyContextMenuResources();
        ApplyToolStripResources();
        ApplyTabContextMenuResources();

        ApplyToolTips();
    }

    private void ApplyTabContextMenuResources ()
    {
        closeThisTabToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_closeThisTabToolStripMenuItem;
        closeOtherTabsToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_closeOtherTabsToolStripMenuItem;
        closeAllTabsToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_closeAllTabsToolStripMenuItem;
        tabColorToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_tabColorToolStripMenuItem;
        tabRenameToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_tabRenameToolStripMenuItem;
        copyPathToClipboardToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_copyPathToClipboardToolStripMenuItem;
        findInExplorerToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_findInExplorerToolStripMenuItem;
        truncateFileToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_truncateFileToolStripMenuItem;
    }

    private void ApplyToolStripResources ()
    {
        checkBoxHost.Text = Resources.LogTabWindow_UI_CheckBox_Host;
        toolStripContainer.Text = Resources.LogTabWindow_UI_ToolStripContainer_toolStripContainer;
        toolStripButtonOpen.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonOpen;
        toolStripButtonSearch.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonSearch;
        toolStripButtonFilter.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonFilter;
        toolStripButtonBookmark.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonBookmark;
        toolStripButtonUp.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonUp;
        toolStripButtonDown.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonDown;
        toolStripButtonBubbles.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonBubbles;
        toolStripButtonTail.Text = Resources.LogTabWindow_UI_ToolStripButton_toolStripButtonTail;
        checkBoxFollowTail.Text = Resources.LogTabWindow_UI_CheckBox_checkBoxFollowTail;
        pluginTrustManagementToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_Text_PluginTrustManagement;
    }

    private void ApplyContextMenuResources ()
    {
        //File menu
        fileToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_fileToolStripMenuItem;
        openToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_openToolStripMenuItem;
        openURIToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_openURIToolStripMenuItem;
        closeFileToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_closeFileToolStripMenuItem;
        reloadToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_reloadToolStripMenuItem;
        newFromClipboardToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_newFromClipboardToolStripMenuItem;
        multiFileToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_multiFileToolStripMenuItem;
        multiFileEnabledStripMenuItem.Text = Resources.LogTabWindow_UI_StripMenuItem_multiFileEnabledStripMenuItem;
        multifileMaskToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_multifileMaskToolStripMenuItem;
        loadProjectToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_loadProjectToolStripMenuItem;
        saveProjectToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_saveProjectToolStripMenuItem;
        exportBookmarksToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_exportBookmarksToolStripMenuItem;
        lastUsedToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_lastUsedToolStripMenuItem;
        exitToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_exitToolStripMenuItem;

        //View/Navigate menu
        viewNavigateToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_viewNavigateToolStripMenuItem;
        goToLineToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_goToLineToolStripMenuItem;
        searchToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_searchToolStripMenuItem;
        filterToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_filterToolStripMenuItem;
        bookmarksToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_bookmarksToolStripMenuItem;
        toggleBookmarkToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_toggleBookmarkToolStripMenuItem;
        jumpToNextToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_jumpToNextToolStripMenuItem;
        jumpToPrevToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_jumpToPrevToolStripMenuItem;
        showBookmarkListToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_showBookmarkListToolStripMenuItem;
        columnFinderToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_columnFinderToolStripMenuItem;
        encodingToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_encodingToolStripMenuItem;
        encodingASCIIToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_encodingASCIIToolStripMenuItem;
        encodingANSIToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_encodingANSIToolStripMenuItem;
        encodingISO88591toolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_encodingISO88591toolStripMenuItem;
        encodingUTF8toolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_encodingUTF8toolStripMenuItem;
        encodingUTF16toolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_encodingUTF16toolStripMenuItem;
        timeshiftToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_timeshiftToolStripMenuItem;
        timeshiftToolStripTextBox.Text = Resources.LogTabWindow_UI_ToolStripTextBox_timeshiftToolStripTextBox;
        copyMarkedLinesIntoNewTabToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_copyMarkedLinesIntoNewTabToolStripMenuItem;

        //Options menu
        optionToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_optionToolStripMenuItem;
        columnizerToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_columnizerToolStripMenuItem;
        hilightingToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_hilightingToolStripMenuItem;
        settingsToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_settingsToolStripMenuItem;
        cellSelectModeToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_cellSelectModeToolStripMenuItem;
        alwaysOnTopToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_alwaysOnTopToolStripMenuItem;
        hideLineColumnToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_hideLineColumnToolStripMenuItem;
        lockInstanceToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_lockInstanceToolStripMenuItem;

        //Tools Menu
        toolsToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_toolsToolStripMenuItem;
        configureToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_configureToolStripMenuItem;

        //Help Menu
        helpToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_helpToolStripMenuItem;
        showHelpToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_showHelpToolStripMenuItem;
        aboutToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_aboutToolStripMenuItem;

        //Debug Menu
        debugToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_debugToolStripMenuItem;
        dumpLogBufferInfoToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_dumpLogBufferInfoToolStripMenuItem;
        dumpBufferDiagnosticToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_dumpBufferDiagnosticToolStripMenuItem;
        runGCToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_runGCToolStripMenuItem;
        gCInfoToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_gCInfoToolStripMenuItem;
        throwExceptionGUIThreadToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_throwExceptionGUIThreadToolStripMenuItem;
        throwExceptionbackgroundThToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_throwExceptionbackgroundThToolStripMenuItem;
        throwExceptionBackgroundThreadToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_throwExceptionBackgroundThreadToolStripMenuItem;
        loglevelToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_loglevelToolStripMenuItem;
        warnLogLevelToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_warnToolStripMenuItem;
        infoLogLevelToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_infoToolStripMenuItem;
        debugLogLevelToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_debugLogLevelToolStripMenuItem;
        disableWordHighlightModeToolStripMenuItem.Text = Resources.LogTabWindow_UI_ToolStripMenuItem_disableWordHighlightModeToolStripMenuItem;
    }

    private void ApplyStatusStripResources ()
    {
        labelLines.Text = Resources.LogTabWindow_UI_Label_labelLines;
        labelSize.Text = Resources.LogTabWindow_UI_Label_labelSize;
        labelCurrentLine.Text = Resources.LogTabWindow_UI_Label_labelCurrentLine;
        labelStatus.Text = Resources.LogTabWindow_UI_Label_labelStatus;
    }

    private void ApplyToolTips ()
    {
        //TODO use ToolTip class instead of ToolTipText
        pluginTrustManagementToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_PluginTrustManagement;
        timeshiftToolStripTextBox.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_timeshiftToolStripTextBox;
        openURIToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_openURIToolStripMenuItem;
        newFromClipboardToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_newFromClipboardToolStripMenuItem;
        multiFileToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_multiFileToolStripMenuItem;
        loadProjectToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_loadProjectToolStripMenuItem;
        saveProjectToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_saveProjectToolStripMenuItem;
        timeshiftToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_timeshiftToolStripMenuItem;
        copyMarkedLinesIntoNewTabToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_copyMarkedLinesIntoNewTabToolStripMenuItem;
        columnizerToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_columnizerToolStripMenuItem;
        cellSelectModeToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_cellSelectModeToolStripMenuItem;
        lockInstanceToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_lockInstanceToolStripMenuItem;
        toolsToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_toolsToolStripMenuItem;
        toolStripButtonSearch.ToolTipText = Resources.LogTabWindow_UI_ToolStripButton_ToolTip_toolStripButtonSearch;
        toolStripButtonOpen.ToolTipText = Resources.LogTabWindow_UI_ToolStripButton_ToolTip_toolStripButtonOpen;
        toolStripButtonDown.ToolTipText = Resources.LogTabWindow_UI_ToolStripButton_ToolTip_toolStripButtonDown;
        toolStripButtonUp.ToolTipText = Resources.LogTabWindow_UI_ToolStripButton_ToolTip_toolStripButtonUp;
        toolStripButtonBookmark.ToolTipText = Resources.LogTabWindow_UI_ToolStripButton_ToolTip_toolStripButtonBookmark;
        toolStripButtonFilter.ToolTipText = Resources.LogTabWindow_UI_ToolStripButton_ToolTip_toolStripButtonFilter;
        highlightGroupsToolStripComboBox.ToolTipText = Resources.LogTabWindow_UI_ToolStripComboBox_ToolTip_highlightGroupsToolStripComboBox;
        tabRenameToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_tabRenameToolStripMenuItem;
        closeAllTabsToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_closeAllTabsToolStripMenuItem;
        closeOtherTabsToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_closeOtherTabsToolStripMenuItem;
        tabColorToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_tabColorToolStripMenuItem;
        findInExplorerToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_findInExplorerToolStripMenuItem;
        copyPathToClipboardToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_copyPathToClipboardToolStripMenuItem;
        truncateFileToolStripMenuItem.ToolTipText = Resources.LogTabWindow_UI_ToolStripMenuItem_ToolTip_truncateFileToolStripMenuItem;
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddFilterTab (FilterPipe pipe, string title, ILogLineMemoryColumnizer preProcessColumnizer)
    {
        var logWin = AddFileTab(pipe.FileName, true, title, false, preProcessColumnizer);
        if (pipe.FilterParams.SearchText.Length > 0)
        {
            ToolTip tip = new(components);

            //Resources.LogTabWindow_UI_LogWindow_ToolTip_Filter
            var isInvertText = pipe.FilterParams.IsInvert ? Resources.LogTabWindow_UI_LogWindow_ToolTip_InvertMatch : string.Empty;
            var isColumnRestrictText = pipe.FilterParams.ColumnRestrict ? Resources.LogTabWindow_UI_LogWindow_Tooltip_ColumnRestrict : string.Empty;
            tip.SetToolTip(logWin, string.Format(CultureInfo.InvariantCulture, Resources.LogTabWindow_UI_LogWindow_ToolTip_Filter, pipe.FilterParams.SearchText, isInvertText, isColumnRestrictText));
            tip.AutomaticDelay = 10;
            tip.AutoPopDelay = 5000;
            var data = logWin.Tag as LogWindowData;
            data.ToolTip = tip;
        }

        return logWin;
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddFileTabDeferred (string givenFileName, bool isTempFile, string title, bool forcePersistenceLoading, ILogLineMemoryColumnizer preProcessColumnizer)
    {
        return AddFileTab(givenFileName, isTempFile, title, forcePersistenceLoading, preProcessColumnizer, true);
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddFileTab (string givenFileName, bool isTempFile, string title, bool forcePersistenceLoading, ILogLineMemoryColumnizer preProcessColumnizer, bool doNotAddToDockPanel = false)
    {
        var logFileName = FindFilenameForSettings(givenFileName);
        var win = FindWindowForFile(logFileName);
        if (win != null)
        {
            if (!isTempFile)
            {
                AddToFileHistory(givenFileName);
            }

            SelectTab(win);
            return win;
        }

        EncodingOptions encodingOptions = new();
        FillDefaultEncodingFromSettings(encodingOptions);
        LogWindow.LogWindow logWindow = new(this, logFileName, isTempFile, forcePersistenceLoading, ConfigManager)
        {
            GivenFileName = givenFileName
        };

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

        var data = logWindow.Tag as LogWindowData;
        data.Color = _defaultTabColor;
        //TODO SetTabColor and the Coloring must be reimplemented with a different UI Framework
        //SetTabColor(logWindow, _defaultTabColor);
        //data.tabPage.BorderColor = this.defaultTabBorderColor;
        //if (!isTempFile)
        //{
        //    foreach (var colorEntry in ConfigManager.Settings.FileColors)
        //    {
        //        if (colorEntry.FileName.ToUpperInvariant().Equals(logFileName.ToUpperInvariant(), StringComparison.Ordinal))
        //        {
        //            data.Color = colorEntry.Color;
        //            //SetTabColor(logWindow, colorEntry.Color);
        //            break;
        //        }
        //    }
        //}

        if (!isTempFile)
        {
            SetTooltipText(logWindow, logFileName);
        }

        if (givenFileName.EndsWith(".lxp", StringComparison.Ordinal))
        {
            logWindow.ForcedPersistenceFileName = givenFileName;
        }

        // this.BeginInvoke(new LoadFileDelegate(logWindow.LoadFile), new object[] { logFileName, encoding });
        _ = Task.Run(() => logWindow.LoadFile(logFileName, encodingOptions));
        return logWindow;
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddMultiFileTab (string[] fileNames)
    {
        if (fileNames.Length < 1)
        {
            return null;
        }

        LogWindow.LogWindow logWindow = new(this, fileNames[^1], false, false, ConfigManager);
        AddLogWindow(logWindow, fileNames[^1], false);
        multiFileToolStripMenuItem.Checked = true;
        multiFileEnabledStripMenuItem.Checked = true;
        EncodingOptions encodingOptions = new();
        FillDefaultEncodingFromSettings(encodingOptions);
        _ = BeginInvoke(new LoadMultiFilesDelegate(logWindow.LoadFilesAsMulti), fileNames, encodingOptions);
        AddToFileHistory(fileNames[0]);
        return logWindow;
    }

    [SupportedOSPlatform("windows")]
    public void LoadFiles (string[] fileNames)
    {
        _ = Invoke(new AddFileTabsDelegate(AddFileTabs), [fileNames]);
    }

    [SupportedOSPlatform("windows")]
    public void OpenSearchDialog ()
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        SearchDialog dlg = new();
        AddOwnedForm(dlg);
        dlg.TopMost = TopMost;
        SearchParams.HistoryList = ConfigManager.Settings.SearchHistoryList;
        dlg.SearchParams = SearchParams;
        var res = dlg.ShowDialog();
        if (res == DialogResult.OK && dlg.SearchParams != null && !string.IsNullOrWhiteSpace(dlg.SearchParams.SearchText))
        {
            SearchParams = dlg.SearchParams;
            SearchParams.IsFindNext = false;
            CurrentLogWindow.StartSearch();
        }
    }

    public ILogLineMemoryColumnizer GetColumnizerHistoryEntry (string fileName)
    {
        var entry = FindColumnizerHistoryEntry(fileName);
        if (entry != null)
        {
            foreach (var columnizer in PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers)
            {
                if (columnizer.GetName().Equals(entry.ColumnizerName, StringComparison.Ordinal))
                {
                    return columnizer;
                }
            }

            _ = ConfigManager.Settings.ColumnizerHistoryList.Remove(entry); // no valid name -> remove entry
        }

        return null;
    }

    public void SwitchTab (bool shiftPressed)
    {
        var index = dockPanel.Contents.IndexOf(dockPanel.ActiveContent);
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

    public void ScrollAllTabsToTimestamp (DateTime timestamp, LogWindow.LogWindow senderWindow)
    {
        lock (_logWindowList)
        {
            foreach (var logWindow in _logWindowList)
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

    public ILogLineMemoryColumnizer FindColumnizerByFileMask (string fileName)
    {
        foreach (var entry in ConfigManager.Settings.Preferences.ColumnizerMaskList)
        {
            if (entry.Mask != null)
            {
                try
                {
                    if (Regex.IsMatch(fileName, entry.Mask))
                    {
                        var columnizer = ColumnizerPicker.FindMemorColumnizerByName(entry.ColumnizerName, PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers);
                        return columnizer;
                    }
                }
                catch (ArgumentException e)
                {
                    _logger.Error($"RegEx-error while finding columnizer: {e}");
                }
            }
        }

        return null;
    }

    public HighlightGroup FindHighlightGroupByFileMask (string fileName)
    {
        foreach (var entry in ConfigManager.Settings.Preferences.HighlightMaskList)
        {
            if (entry.Mask != null)
            {
                try
                {
                    if (Regex.IsMatch(fileName, entry.Mask))
                    {
                        var group = FindHighlightGroup(entry.HighlightGroupName);
                        return group;
                    }
                }
                catch (ArgumentException e)
                {
                    _logger.Error($"RegEx-error while finding columnizer: {e}");
                }
            }
        }

        return null;
    }

    public void SelectTab (ILogWindow logWindow)
    {
        logWindow.Activate();
    }

    [SupportedOSPlatform("windows")]
    public void SetForeground ()
    {
        _ = Vanara.PInvoke.User32.SetForegroundWindow(Handle);
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = _wasMaximized
                ? FormWindowState.Maximized
                : FormWindowState.Normal;
        }
    }

    // called from LogWindow when follow tail was changed
    [SupportedOSPlatform("windows")]
    public void FollowTailChanged (LogWindow.LogWindow logWindow, bool isEnabled, bool offByTrigger)
    {
        if (logWindow.Tag is not LogWindowData data)
        {
            return;
        }

        data.TailState = isEnabled
            ? 0
            : offByTrigger
                ? 2
                : 1;

        if (Preferences.ShowTailState)
        {
            var icon = GetLedIcon(data.DiffSum, data);
            _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
        }
    }

    [SupportedOSPlatform("windows")]
    public void NotifySettingsChanged (object sender, SettingsFlags flags)
    {
        if (sender != this)
        {
            NotifyWindowsForChangedPrefs(flags);
        }
    }

    public IList<WindowFileEntry> GetListOfOpenFiles ()
    {
        IList<WindowFileEntry> list = [];
        lock (_logWindowList)
        {
            foreach (var logWindow in _logWindowList)
            {
                list.Add(new WindowFileEntry(logWindow));
            }
        }

        return list;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates a temp file with the text content of the clipboard and opens the temp file in a new tab.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private void PasteFromClipboard ()
    {
        if (Clipboard.ContainsText())
        {
            var text = Clipboard.GetText();
            var fileName = Path.GetTempFileName();

            using (FileStream fStream = new(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (StreamWriter writer = new(fStream, Encoding.Unicode))
            {
                writer.Write(text);
                writer.Close();
            }

            var title = Resources.LogTabWindow_UI_LogWindow_Title_Text_From_Clipboard;
            var logWindow = AddTempFileTab(fileName, title);
            if (logWindow.Tag is LogWindowData)
            {
                SetTooltipText(logWindow, string.Format(CultureInfo.InvariantCulture, Resources.LogTabWindow_UI_LogWindow_Title_ToolTip_PastedOn, DateTime.Now));
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void InitToolWindows ()
    {
        InitBookmarkWindow();
    }

    [SupportedOSPlatform("windows")]
    private void DestroyToolWindows ()
    {
        DestroyBookmarkWindow();
    }

    [SupportedOSPlatform("windows")]
    private void InitBookmarkWindow ()
    {
        _bookmarkWindow = new BookmarkWindow
        {
            HideOnClose = true,
            ShowHint = DockState.DockBottom
        };

        var setLastColumnWidth = ConfigManager.Settings.Preferences.SetLastColumnWidth;
        var lastColumnWidth = ConfigManager.Settings.Preferences.LastColumnWidth;
        var fontName = ConfigManager.Settings.Preferences.FontName;
        var fontSize = ConfigManager.Settings.Preferences.FontSize;

        _bookmarkWindow.PreferencesChanged(fontName, fontSize, setLastColumnWidth, lastColumnWidth, SettingsFlags.All);
        _bookmarkWindow.VisibleChanged += OnBookmarkWindowVisibleChanged;
        _firstBookmarkWindowShow = true;
    }

    [SupportedOSPlatform("windows")]
    private void DestroyBookmarkWindow ()
    {
        _bookmarkWindow.HideOnClose = false;
        _bookmarkWindow.Close();
    }

    private void SaveLastOpenFilesList ()
    {
        ConfigManager.Settings.LastOpenFilesList.Clear();
        foreach (DockContent content in dockPanel.Contents)
        {
            if (content is LogWindow.LogWindow logWin)
            {
                if (!logWin.IsTempFile)
                {
                    ConfigManager.Settings.LastOpenFilesList.Add(logWin.GivenFileName);
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void SaveWindowPosition ()
    {
        SuspendLayout();
        if (WindowState == FormWindowState.Normal)
        {
            ConfigManager.Settings.AppBounds = Bounds;
            ConfigManager.Settings.IsMaximized = false;
        }
        else
        {
            ConfigManager.Settings.AppBoundsFullscreen = Bounds;
            ConfigManager.Settings.IsMaximized = true;
            WindowState = FormWindowState.Normal;
            ConfigManager.Settings.AppBounds = Bounds;
        }

        ResumeLayout();
    }

    private static void SetTooltipText (LogWindow.LogWindow logWindow, string logFileName)
    {
        logWindow.ToolTipText = logFileName;
    }

    private void FillDefaultEncodingFromSettings (EncodingOptions encodingOptions)
    {
        if (ConfigManager.Settings.Preferences.DefaultEncoding != null)
        {
            try
            {
                encodingOptions.DefaultEncoding = Encoding.GetEncoding(ConfigManager.Settings.Preferences.DefaultEncoding);
            }
            catch (ArgumentException)
            {
                //ConfigManager.Settings.Preferences.DefaultEncoding
                _logger.Warn($"### FillDefaultEncodingFromSettings: Encoding {ConfigManager.Settings.Preferences.DefaultEncoding} is not a valid encoding");
                encodingOptions.DefaultEncoding = null;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void AddFileTabs (string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.EndsWith(".lxj", StringComparison.OrdinalIgnoreCase))
                {
                    LoadProject(fileName, false);
                }
                else
                {
                    _ = AddFileTab(fileName, false, null, false, null);
                }
            }
        }

        Activate();
    }

    [SupportedOSPlatform("windows")]
    private void AddLogWindow (LogWindow.LogWindow logWindow, string title, bool doNotAddToPanel)
    {
        logWindow.CloseButton = true;
        logWindow.TabPageContextMenuStrip = tabContextMenuStrip;
        SetTooltipText(logWindow, title);
        logWindow.DockAreas = DockAreas.Document | DockAreas.Float;

        if (!doNotAddToPanel)
        {
            logWindow.Show(dockPanel);
        }

        LogWindowData data = new()
        {
            DiffSum = 0
        };

        logWindow.Tag = data;

        lock (_logWindowList)
        {
            _logWindowList.Add(logWindow);
        }

        logWindow.FileSizeChanged += OnFileSizeChanged;
        logWindow.TailFollowed += OnTailFollowed;
        logWindow.Disposed += OnLogWindowDisposed;
        logWindow.FileNotFound += OnLogWindowFileNotFound;
        logWindow.FileRespawned += OnLogWindowFileRespawned;
        logWindow.FilterListChanged += OnLogWindowFilterListChanged;
        logWindow.CurrentHighlightGroupChanged += OnLogWindowCurrentHighlightGroupChanged;
        logWindow.SyncModeChanged += OnLogWindowSyncModeChanged;

        logWindow.Visible = true;
    }

    [SupportedOSPlatform("windows")]
    private void DisconnectEventHandlers (LogWindow.LogWindow logWindow)
    {
        logWindow.FileSizeChanged -= OnFileSizeChanged;
        logWindow.TailFollowed -= OnTailFollowed;
        logWindow.Disposed -= OnLogWindowDisposed;
        logWindow.FileNotFound -= OnLogWindowFileNotFound;
        logWindow.FileRespawned -= OnLogWindowFileRespawned;
        logWindow.FilterListChanged -= OnLogWindowFilterListChanged;
        logWindow.CurrentHighlightGroupChanged -= OnLogWindowCurrentHighlightGroupChanged;
        logWindow.SyncModeChanged -= OnLogWindowSyncModeChanged;

        var data = logWindow.Tag as LogWindowData;
        //data.tabPage.MouseClick -= tabPage_MouseClick;
        //data.tabPage.TabDoubleClick -= tabPage_TabDoubleClick;
        //data.tabPage.ContextMenuStrip = null;
        //data.tabPage = null;
    }

    [SupportedOSPlatform("windows")]
    private void AddToFileHistory (string fileName)
    {
        bool findName (string s) => s.ToUpperInvariant().Equals(fileName.ToUpperInvariant(), StringComparison.Ordinal);

        var index = ConfigManager.Settings.FileHistoryList.FindIndex(findName);

        if (index != -1)
        {
            ConfigManager.Settings.FileHistoryList.RemoveAt(index);
        }

        ConfigManager.Settings.FileHistoryList.Insert(0, fileName);

        while (ConfigManager.Settings.FileHistoryList.Count > MAX_FILE_HISTORY)
        {
            ConfigManager.Settings.FileHistoryList.RemoveAt(ConfigManager.Settings.FileHistoryList.Count - 1);
        }

        ConfigManager.Save(SettingsFlags.FileHistory);

        FillHistoryMenu();
    }

    [SupportedOSPlatform("windows")]
    private LogWindow.LogWindow FindWindowForFile (string fileName)
    {
        lock (_logWindowList)
        {
            foreach (var logWindow in _logWindowList)
            {
                if (logWindow.FileName.ToUpperInvariant().Equals(fileName.ToUpperInvariant(), StringComparison.Ordinal))
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
    private static string FindFilenameForSettings (string fileName)
    {
        if (fileName.EndsWith(".lxp", StringComparison.OrdinalIgnoreCase))
        {
            var persistenceData = Persister.Load(fileName);
            if (persistenceData == null)
            {
                return fileName;
            }

            if (!string.IsNullOrEmpty(persistenceData.FileName))
            {
                var fs = PluginRegistry.PluginRegistry.Instance.FindFileSystemForUri(persistenceData.FileName);
                if (fs != null && !fs.GetType().Equals(typeof(LocalFileSystem)))
                {
                    return persistenceData.FileName;
                }

                // On relative paths the URI check (and therefore the file system plugin check) will fail.
                // So fs == null and fs == LocalFileSystem are handled here like normal files.
                if (Path.IsPathRooted(persistenceData.FileName))
                {
                    return persistenceData.FileName;
                }

                // handle relative paths in .lxp files
                var dir = Path.GetDirectoryName(fileName);
                return Path.Join(dir, persistenceData.FileName);
            }
        }

        return fileName;
    }

    [SupportedOSPlatform("windows")]
    private void FillHistoryMenu ()
    {
        ToolStripDropDown strip = new ToolStripDropDownMenu();

        foreach (var file in ConfigManager.Settings.FileHistoryList)
        {
            ToolStripItem item = new ToolStripMenuItem(file);
            _ = strip.Items.Add(item);

        }

        strip.ItemClicked += OnHistoryItemClicked;
        strip.MouseUp += OnStripMouseUp;
        lastUsedToolStripMenuItem.DropDown = strip;
    }

    [SupportedOSPlatform("windows")]
    private void RemoveLogWindow (LogWindow.LogWindow logWindow)
    {
        lock (_logWindowList)
        {
            _ = _logWindowList.Remove(logWindow);
        }

        DisconnectEventHandlers(logWindow);
    }

    [SupportedOSPlatform("windows")]
    private void RemoveAndDisposeLogWindow (LogWindow.LogWindow logWindow, bool dontAsk)
    {
        if (CurrentLogWindow == logWindow)
        {
            ChangeCurrentLogWindow(null);
        }

        lock (_logWindowList)
        {
            _ = _logWindowList.Remove(logWindow);
        }

        logWindow.Close(dontAsk);
    }

    [SupportedOSPlatform("windows")]
    private void ShowHighlightSettingsDialog ()
    {
        HighlightDialog dlg = new(ConfigManager)
        {
            KeywordActionList = PluginRegistry.PluginRegistry.Instance.RegisteredKeywordActions,
            Owner = this,
            TopMost = TopMost,
            HighlightGroupList = HighlightGroupList,
            PreSelectedGroupName = highlightGroupsToolStripComboBox.Text
        };

        var res = dlg.ShowDialog();

        if (res == DialogResult.OK)
        {
            HighlightGroupList = dlg.HighlightGroupList;
            FillHighlightComboBox();
            ConfigManager.Settings.Preferences.HighlightGroupList = HighlightGroupList;
            ConfigManager.Save(SettingsFlags.HighlightSettings);
            OnHighlightSettingsChanged();
        }
    }

    [SupportedOSPlatform("windows")]
    private void FillHighlightComboBox ()
    {
        var currentGroupName = highlightGroupsToolStripComboBox.Text;
        highlightGroupsToolStripComboBox.Items.Clear();
        foreach (var group in HighlightGroupList)
        {
            _ = highlightGroupsToolStripComboBox.Items.Add(group.GroupName);
            if (group.GroupName.Equals(currentGroupName, StringComparison.Ordinal))
            {
                highlightGroupsToolStripComboBox.Text = group.GroupName;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OpenFileDialog ()
    {
        OpenFileDialog openFileDialog = new();

        if (CurrentLogWindow != null)
        {
            FileInfo info = new(CurrentLogWindow.FileName);
            openFileDialog.InitialDirectory = info.DirectoryName;
        }
        else
        {
            if (!string.IsNullOrEmpty(ConfigManager.Settings.LastDirectory))
            {
                openFileDialog.InitialDirectory = ConfigManager.Settings.LastDirectory;
            }
            else
            {
                try
                {
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                catch (SecurityException e)
                {
                    _logger.Warn(string.Format(CultureInfo.InvariantCulture, Resources.LogExpert_Common_Error_InsufficientRights_For_Parameter_ErrorMessage, nameof(Environment.GetFolderPath), e));
                    // no initial directory if insufficient rights
                }
            }
        }

        openFileDialog.Multiselect = true;

        if (DialogResult.OK == openFileDialog.ShowDialog(this))
        {
            FileInfo info = new(openFileDialog.FileName);
            if (info.Directory.Exists)
            {
                ConfigManager.Settings.LastDirectory = info.DirectoryName;
                ConfigManager.Save(SettingsFlags.FileHistory);
            }

            if (info.Exists)
            {
                LoadFiles(openFileDialog.FileNames, false);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void LoadFiles (string[] names, bool invertLogic)
    {
        Array.Sort(names);

        if (names.Length == 1)
        {
            if (names[0].EndsWith(".lxj", StringComparison.OrdinalIgnoreCase))
            {
                LoadProject(names[0], true);
                return;
            }

            _ = AddFileTab(names[0], false, null, false, null);
            return;
        }

        var option = ConfigManager.Settings.Preferences.MultiFileOption;
        if (option == MultiFileOption.Ask)
        {
            MultiLoadRequestDialog dlg = new();
            var res = dlg.ShowDialog();

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
                option = option == MultiFileOption.SingleFiles
                    ? MultiFileOption.MultiFile
                    : MultiFileOption.SingleFiles;
            }
        }

        if (option == MultiFileOption.SingleFiles)
        {
            AddFileTabs(names);
        }
        else
        {
            _ = AddMultiFileTab(names);
        }
    }

    private void SetColumnizerHistoryEntry (string fileName, ILogLineMemoryColumnizer columnizer)
    {
        var entry = FindColumnizerHistoryEntry(fileName);
        if (entry != null)
        {
            _ = ConfigManager.Settings.ColumnizerHistoryList.Remove(entry);

        }

        ConfigManager.Settings.ColumnizerHistoryList.Add(new ColumnizerHistoryEntry(fileName, columnizer.GetName()));

        if (ConfigManager.Settings.ColumnizerHistoryList.Count > MAX_COLUMNIZER_HISTORY)
        {
            ConfigManager.Settings.ColumnizerHistoryList.RemoveAt(0);
        }
    }

    private ColumnizerHistoryEntry FindColumnizerHistoryEntry (string fileName)
    {
        foreach (var entry in ConfigManager.Settings.ColumnizerHistoryList)
        {
            if (entry.FileName.Equals(fileName, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    [SupportedOSPlatform("windows")]
    private void ToggleMultiFile ()
    {
        if (CurrentLogWindow != null)
        {
            CurrentLogWindow.SwitchMultiFile(!CurrentLogWindow.IsMultiFile);
            multiFileToolStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
            multiFileEnabledStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
        }
    }

    [SupportedOSPlatform("windows")]
    private void ChangeCurrentLogWindow (LogWindow.LogWindow newLogWindow)
    {
        if (newLogWindow == _currentLogWindow)
        {
            return; // do nothing if wishing to set the same window
        }

        var oldLogWindow = _currentLogWindow;
        _currentLogWindow = newLogWindow;
        var titleName = _showInstanceNumbers ? "LogExpert #" + _instanceNumber : "LogExpert";

        if (oldLogWindow != null)
        {
            oldLogWindow.StatusLineEvent -= OnStatusLineEvent;
            oldLogWindow.ProgressBarUpdate -= OnProgressBarUpdate;
            oldLogWindow.GuiStateUpdate -= OnGuiStateUpdate;
            oldLogWindow.ColumnizerChanged -= OnColumnizerChanged;
            oldLogWindow.BookmarkAdded -= OnBookmarkAdded;
            oldLogWindow.BookmarkRemoved -= OnBookmarkRemoved;
            oldLogWindow.BookmarkTextChanged -= OnBookmarkTextChanged;
            DisconnectToolWindows();
        }

        if (newLogWindow != null)
        {
            newLogWindow.StatusLineEvent += OnStatusLineEvent;
            newLogWindow.ProgressBarUpdate += OnProgressBarUpdate;
            newLogWindow.GuiStateUpdate += OnGuiStateUpdate;
            newLogWindow.ColumnizerChanged += OnColumnizerChanged;
            newLogWindow.BookmarkAdded += OnBookmarkAdded;
            newLogWindow.BookmarkRemoved += OnBookmarkRemoved;
            newLogWindow.BookmarkTextChanged += OnBookmarkTextChanged;

            Text = newLogWindow.IsTempFile
                ? titleName + @" - " + newLogWindow.TempTitleName
                : titleName + @" - " + newLogWindow.FileName;

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
            checkBoxFollowTail.Checked = false;
            mainMenuStrip.Enabled = true;
            timeshiftToolStripMenuItem.Enabled = false;
            timeshiftToolStripMenuItem.Checked = false;
            timeshiftToolStripTextBox.Text = string.Empty;
            timeshiftToolStripTextBox.Enabled = false;
            multiFileToolStripMenuItem.Enabled = false;
            cellSelectModeToolStripMenuItem.Checked = false;
            cellSelectModeToolStripMenuItem.Enabled = false;
            closeFileToolStripMenuItem.Enabled = false;
            searchToolStripMenuItem.Enabled = false;
            filterToolStripMenuItem.Enabled = false;
            goToLineToolStripMenuItem.Enabled = false;
            dragControlDateTime.Visible = false;
        }
    }

    private void ConnectToolWindows (LogWindow.LogWindow logWindow)
    {
        ConnectBookmarkWindow(logWindow);
    }

    private void ConnectBookmarkWindow (LogWindow.LogWindow logWindow)
    {
        FileViewContext ctx = new(logWindow, logWindow);
        _bookmarkWindow.SetBookmarkData(logWindow.BookmarkData);
        _bookmarkWindow.SetCurrentFile(ctx);
    }

    private void DisconnectToolWindows ()
    {
        DisconnectBookmarkWindow();
    }

    private void DisconnectBookmarkWindow ()
    {
        _bookmarkWindow.SetBookmarkData(null);
        _bookmarkWindow.SetCurrentFile(null);
    }

    [SupportedOSPlatform("windows")]
    private void GuiStateUpdateWorker (GuiStateEventArgs e)
    {
        _skipEvents = true;
        checkBoxFollowTail.Checked = e.FollowTail;
        mainMenuStrip.Enabled = e.MenuEnabled;
        timeshiftToolStripMenuItem.Enabled = e.TimeshiftPossible;
        timeshiftToolStripMenuItem.Checked = e.TimeshiftEnabled;
        timeshiftToolStripTextBox.Text = e.TimeshiftText;
        timeshiftToolStripTextBox.Enabled = e.TimeshiftEnabled;
        multiFileToolStripMenuItem.Enabled = e.MultiFileEnabled; // disabled for temp files
        multiFileToolStripMenuItem.Checked = e.IsMultiFileActive;
        multiFileEnabledStripMenuItem.Checked = e.IsMultiFileActive;
        cellSelectModeToolStripMenuItem.Checked = e.CellSelectMode;

        RefreshEncodingMenuBar(e.CurrentEncoding);

        if (e.TimeshiftPossible && ConfigManager.Settings.Preferences.TimestampControl)
        {
            dragControlDateTime.MinDateTime = e.MinTimestamp;
            dragControlDateTime.MaxDateTime = e.MaxTimestamp;
            dragControlDateTime.DateTime = e.Timestamp;
            dragControlDateTime.Visible = true;
            dragControlDateTime.Enabled = true;
            dragControlDateTime.Refresh();
        }
        else
        {
            dragControlDateTime.Visible = false;
            dragControlDateTime.Enabled = false;
        }

        toolStripButtonBubbles.Checked = e.ShowBookmarkBubbles;
        highlightGroupsToolStripComboBox.Text = e.HighlightGroupName;
        columnFinderToolStripMenuItem.Checked = e.ColumnFinderVisible;

        _skipEvents = false;
    }

    [SupportedOSPlatform("windows")]
    private void ProgressBarUpdateWorker (ProgressEventArgs e)
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
                _logger.Error(string.Format(CultureInfo.InvariantCulture, Resources.LogExpert_Common_Error_5Parameters_ErrorDuring0Value1Min2Max3Visible45, e.Value, e.MinValue, e.MaxValue, e.Visible, ex));
            }

            _ = Invoke(new MethodInvoker(statusStrip.Refresh));
        }
    }

    [SupportedOSPlatform("windows")]
    //TODO Crossthread Exception when a log file has been filtered to a new tab!
    private void StatusLineEventWorker (StatusLineEventArgs e)
    {
        if (e != null)
        {
            //_logger.logDebug("StatusLineEvent: text = " + e.StatusText);
            labelStatus.Text = e.StatusText;
            labelStatus.Size = TextRenderer.MeasureText(labelStatus.Text, labelStatus.Font);
            labelLines.Text = $"{e.LineCount} {Resources.LogTabWindow_StatusLineText_lowerCase_Lines}";
            labelLines.Size = TextRenderer.MeasureText(labelLines.Text, labelLines.Font);
            labelSize.Text = Util.GetFileSizeAsText(e.FileSize);
            labelSize.Size = TextRenderer.MeasureText(labelSize.Text, labelSize.Font);
            labelCurrentLine.Text = $"{Resources.LogTabWindow_StatusLineText_UpperCase_Lines} {e.CurrentLineNum}";
            labelCurrentLine.Size = TextRenderer.MeasureText(labelCurrentLine.Text, labelCurrentLine.Font);

            if (statusStrip.InvokeRequired)
            {
                _ = statusStrip.BeginInvoke(new MethodInvoker(statusStrip.Refresh));
            }
            else
            {
                statusStrip.Refresh();
            }
        }
    }

    // tailState: 0,1,2 = on/off/off by Trigger
    // syncMode: 0 = normal (no), 1 = time synced
    [SupportedOSPlatform("windows")]
    private Icon CreateLedIcon (int level, bool dirty, int tailState, int syncMode)
    {
        var iconRect = _leds[0];
        iconRect.Height = 16; // (DockPanel's damn hardcoded height) // this.leds[this.leds.Length - 1].Bottom;
        iconRect.Width = iconRect.Right + 6;
        Bitmap bmp = new(iconRect.Width, iconRect.Height);
        var gfx = Graphics.FromImage(bmp);

        var offsetFromTop = 4;

        for (var i = 0; i < _leds.Length; ++i)
        {
            var ledRect = _leds[i];
            ledRect.Offset(0, offsetFromTop);

            if (level >= _leds.Length - i)
            {
                gfx.FillRectangle(_ledBrushes[i], ledRect);
            }
            else
            {
                gfx.FillRectangle(_offLedBrush, ledRect);
            }
        }

        var ledSize = 3;
        var ledGap = 1;
        var lastLed = _leds[^1];
        Rectangle dirtyLed = new(lastLed.Right + 2, lastLed.Bottom - ledSize, ledSize, ledSize);
        Rectangle tailLed = new(dirtyLed.Location, dirtyLed.Size);
        tailLed.Offset(0, -(ledSize + ledGap));
        Rectangle syncLed = new(tailLed.Location, dirtyLed.Size);
        syncLed.Offset(0, -(ledSize + ledGap));

        syncLed.Offset(0, offsetFromTop);
        tailLed.Offset(0, offsetFromTop);
        dirtyLed.Offset(0, offsetFromTop);

        if (dirty)
        {
            gfx.FillRectangle(_dirtyLedBrush, dirtyLed);
        }
        else
        {
            gfx.FillRectangle(_offLedBrush, dirtyLed);
        }

        // tailMode 4 means: don't show
        if (tailState < 3)
        {
            gfx.FillRectangle(_tailLedBrush[tailState], tailLed);
        }

        if (syncMode == 1)
        {
            gfx.FillRectangle(_syncLedBrush, syncLed);
        }
        //else
        //{
        //  gfx.FillRectangle(this.offLedBrush, syncLed);
        //}

        // see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=345656
        // GetHicon() creates an unmanaged handle which must be destroyed. The Clone() workaround creates
        // a managed copy of icon. then the unmanaged win32 handle is destroyed
        var iconHandle = bmp.GetHicon();
        var icon = Icon.FromHandle(iconHandle).Clone() as Icon;
        _ = Vanara.PInvoke.User32.DestroyIcon(iconHandle);

        gfx.Dispose();
        bmp.Dispose();
        return icon;
    }

    [SupportedOSPlatform("windows")]
    private void CreateIcons ()
    {
        for (var syncMode = 0; syncMode <= 1; syncMode++) // LED indicating time synced tabs
        {
            for (var tailMode = 0; tailMode < 4; tailMode++)
            {
                for (var i = 0; i < 6; ++i)
                {
                    _ledIcons[i, 0, tailMode, syncMode] = CreateLedIcon(i, false, tailMode, syncMode);
                }

                for (var i = 0; i < 6; ++i)
                {
                    _ledIcons[i, 1, tailMode, syncMode] = CreateLedIcon(i, true, tailMode, syncMode);
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void FileNotFound (LogWindow.LogWindow logWin)
    {
        var data = logWin.Tag as LogWindowData;
        _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, _deadIcon);
        dragControlDateTime.Visible = false;
    }

    [SupportedOSPlatform("windows")]
    private void FileRespawned (LogWindow.LogWindow logWin)
    {
        var data = logWin.Tag as LogWindowData;
        var icon = GetLedIcon(0, data);
        _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, icon);
    }

    [SupportedOSPlatform("windows")]
    private void ShowLedPeak (LogWindow.LogWindow logWin)
    {
        var data = logWin.Tag as LogWindowData;
        lock (data)
        {
            data.DiffSum = DIFF_MAX;
        }

        var icon = GetLedIcon(data.DiffSum, data);
        _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, icon);
    }

    private static int GetLevelFromDiff (int diff)
    {
        if (diff > 60)
        {
            diff = 60;
        }

        var level = diff / 10;
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

    [SupportedOSPlatform("windows")]
    //TODO Task based
    private void LedThreadProc ()
    {
        Thread.CurrentThread.Name = "LED Thread";
        while (!_shouldStop)
        {
            try
            {
                Thread.Sleep(200);
            }
            catch
            {
                return;
            }

            lock (_logWindowList)
            {
                foreach (var logWindow in _logWindowList)
                {
                    var data = logWindow.Tag as LogWindowData;
                    if (data.DiffSum > 0)
                    {
                        data.DiffSum -= 10;
                        if (data.DiffSum < 0)
                        {
                            data.DiffSum = 0;
                        }

                        var icon = GetLedIcon(data.DiffSum, data);
                        _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
                    }
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void SetTabIcon (LogWindow.LogWindow logWindow, Icon icon)
    {
        if (logWindow != null)
        {
            logWindow.Icon = icon;
            logWindow.DockHandler.Pane?.TabStripControl.Invalidate(false);
        }
    }

    private Icon GetLedIcon (int diff, LogWindowData data)
    {
        var icon =
            _ledIcons[
                GetLevelFromDiff(diff), data.Dirty ? 1 : 0, Preferences.ShowTailState ? data.TailState : 3,
                data.SyncMode
            ];
        return icon;
    }

    [SupportedOSPlatform("windows")]
    private void RefreshEncodingMenuBar (Encoding encoding)
    {
        encodingASCIIToolStripMenuItem.Checked = false;
        encodingANSIToolStripMenuItem.Checked = false;
        encodingUTF8toolStripMenuItem.Checked = false;
        encodingUTF16toolStripMenuItem.Checked = false;
        encodingISO88591toolStripMenuItem.Checked = false;

        if (encoding == null)
        {
            return;
        }

        if (encoding is ASCIIEncoding)
        {
            encodingASCIIToolStripMenuItem.Checked = true;
        }
        else if (encoding.Equals(Encoding.Default))
        {
            encodingANSIToolStripMenuItem.Checked = true;
        }
        else if (encoding is UTF8Encoding)
        {
            encodingUTF8toolStripMenuItem.Checked = true;
        }
        else if (encoding is UnicodeEncoding)
        {
            encodingUTF16toolStripMenuItem.Checked = true;
        }
        else if (encoding.Equals(Encoding.GetEncoding("iso-8859-1")))
        {
            encodingISO88591toolStripMenuItem.Checked = true;
        }

        encodingANSIToolStripMenuItem.Text = Encoding.Default.HeaderName;
    }

    [SupportedOSPlatform("windows")]
    private void OpenSettings (int tabToOpen)
    {
        SettingsDialog dlg = new(ConfigManager.Settings.Preferences, this, tabToOpen, ConfigManager)
        {
            TopMost = TopMost
        };

        if (DialogResult.OK == dlg.ShowDialog())
        {
            ConfigManager.Settings.Preferences = dlg.Preferences;
            ConfigManager.Save(SettingsFlags.Settings);
            NotifyWindowsForChangedPrefs(SettingsFlags.Settings);
        }
    }

    [SupportedOSPlatform("windows")]
    private void NotifyWindowsForChangedPrefs (SettingsFlags flags)
    {
        ApplySettings(ConfigManager.Settings, flags);

        var setLastColumnWidth = ConfigManager.Settings.Preferences.SetLastColumnWidth;
        var lastColumnWidth = ConfigManager.Settings.Preferences.LastColumnWidth;
        var fontName = ConfigManager.Settings.Preferences.FontName;
        var fontSize = ConfigManager.Settings.Preferences.FontSize;

        lock (_logWindowList)
        {
            foreach (var logWindow in _logWindowList)
            {
                logWindow.PreferencesChanged(fontName, fontSize, setLastColumnWidth, lastColumnWidth, false, flags);
            }
        }

        _bookmarkWindow.PreferencesChanged(fontName, fontSize, setLastColumnWidth, lastColumnWidth, flags);

        HighlightGroupList = ConfigManager.Settings.Preferences.HighlightGroupList;
        if ((flags & SettingsFlags.HighlightSettings) == SettingsFlags.HighlightSettings)
        {
            OnHighlightSettingsChanged();
        }
    }

    [SupportedOSPlatform("windows")]
    private void ApplySettings (Settings settings, SettingsFlags flags)
    {
        if ((flags & SettingsFlags.WindowPosition) == SettingsFlags.WindowPosition)
        {
            TopMost = alwaysOnTopToolStripMenuItem.Checked = settings.AlwaysOnTop;
            dragControlDateTime.DragOrientation = settings.Preferences.TimestampControlDragOrientation;
            hideLineColumnToolStripMenuItem.Checked = settings.HideLineColumn;
        }

        if ((flags & SettingsFlags.FileHistory) == SettingsFlags.FileHistory)
        {
            FillHistoryMenu();
        }

        if ((flags & SettingsFlags.GuiOrColors) == SettingsFlags.GuiOrColors)
        {
            SetTabIcons(settings.Preferences);
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

    [SupportedOSPlatform("windows")]
    private void SetTabIcons (Preferences preferences)
    {
        _tailLedBrush[0] = new SolidBrush(preferences.ShowTailColor);
        CreateIcons();
        lock (_logWindowList)
        {
            foreach (var logWindow in _logWindowList)
            {
                var data = logWindow.Tag as LogWindowData;
                var icon = GetLedIcon(data.DiffSum, data);
                _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private static void SetToolIcon (ToolEntry entry, ToolStripItem item)
    {
        var icon = NativeMethods.LoadIconFromExe(entry.IconFile, entry.IconIndex);

        if (icon != null)
        {
            item.Image = icon.ToBitmap();

            item.DisplayStyle = item is ToolStripMenuItem
                ? ToolStripItemDisplayStyle.ImageAndText
                : ToolStripItemDisplayStyle.Image;

            _ = Vanara.PInvoke.User32.DestroyIcon(icon.Handle);
            icon.Dispose();
        }

        if (!string.IsNullOrEmpty(entry.Cmd))
        {
            item.ToolTipText = entry.Name;
        }
    }

    [SupportedOSPlatform("windows")]
    private void ToolButtonClick (ToolEntry toolEntry)
    {
        if (string.IsNullOrEmpty(toolEntry.Cmd))
        {
            //TODO TabIndex => To Enum
            OpenSettings(2);
            return;
        }

        if (CurrentLogWindow != null)
        {
            var line = CurrentLogWindow.GetCurrentLine();
            var info = CurrentLogWindow.GetCurrentFileInfo();
            if (line != null && info != null)
            {
                ArgParser parser = new(toolEntry.Args);
                var argLine = parser.BuildArgs(line, CurrentLogWindow.GetRealLineNum() + 1, info, this);
                if (argLine != null)
                {
                    StartTool(toolEntry.Cmd, argLine, toolEntry.Sysout, toolEntry.ColumnizerName, toolEntry.WorkingDir);
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void StartTool (string cmd, string args, bool sysoutPipe, string columnizerName, string workingDir)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            return;
        }

        Process process = new();
        ProcessStartInfo startInfo = new(cmd, args);
        if (!string.IsNullOrEmpty(workingDir))
        {
            startInfo.WorkingDirectory = workingDir;
        }

        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;

        if (sysoutPipe)
        {
            var columnizer = ColumnizerPicker.DecideMemoryColumnizerByName(columnizerName, PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers);

            //_logger.Info($"Starting external tool with sysout redirection: {cmd} {args}"));
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            //process.OutputDataReceived += pipe.DataReceivedEventHandler;
            try
            {
                _ = process.Start();
            }
            catch (Exception e) when (e is Win32Exception or
                                            InvalidOperationException or
                                            ObjectDisposedException or
                                            PlatformNotSupportedException)
            {
                _logger.Error(e);
                _ = MessageBox.Show(e.Message, Resources.LogExpert_Common_UI_Title_LogExpert);
                return;
            }

            SysoutPipe pipe = new(process.StandardOutput);

            var logWin = AddTempFileTab(pipe.FileName,
                CurrentLogWindow.IsTempFile
                    ? CurrentLogWindow.TempTitleName
                    : $"{Util.GetNameFromPath(CurrentLogWindow.FileName)}{Resources.LogTabWindow_UI_LogWindow_Title_ExternalStartTool_Suffix}");
            logWin.ForceColumnizer(columnizer);

            process.Exited += pipe.ProcessExitedEventHandler;
            //process.BeginOutputReadLine();
        }
        else
        {
            try
            {
                startInfo.UseShellExecute = false;
                _ = process.Start();
            }
            catch (Exception e) when (e is Win32Exception or
                                            InvalidOperationException or
                                            ObjectDisposedException or
                                            PlatformNotSupportedException)
            {
                _logger.Error(e);
                _ = MessageBox.Show(e.Message, Resources.LogExpert_Common_UI_Title_LogExpert);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void CloseAllTabs ()
    {
        IList<Form> closeList = [];
        lock (_logWindowList)
        {
            foreach (var content in dockPanel.Contents.Cast<DockContent>())
            {
                if (content is LogWindow.LogWindow window)
                {
                    closeList.Add(window);
                }
            }
        }

        foreach (var form in closeList)
        {
            form.Close();
        }
    }

    //TODO Reimplementation needs a new UI Framework since, DockpanelSuite has no easy way to change TabColor
    //private static void SetTabColor (LogWindow.LogWindow logWindow, Color color)
    //{
    //    //tabPage.BackLowColor = color;
    //    //tabPage.BackLowColorDisabled = Color.FromArgb(255,
    //    //  Math.Max(0, color.R - 50),
    //    //  Math.Max(0, color.G - 50),
    //    //  Math.Max(0, color.B - 50)
    //    //  );
    //}

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("windows")]
    private void LoadProject (string projectFileName, bool restoreLayout)
    {
        try
        {
            _logger.Info($"Loading project from {projectFileName}");

            // Load project with validation
            var loadResult = ProjectPersister.LoadProjectData(projectFileName, PluginRegistry.PluginRegistry.Instance);

            // Check if project data was loaded
            if (loadResult?.ProjectData == null)
            {
                _ = MessageBox.Show(
                    Resources.LoadProject_UI_Message_Error_FileMaybeCorruptedOrInaccessible,
                    Resources.LoadProject_UI_Message_Error_Title_ProjectLoadFailed,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var projectData = loadResult.ProjectData;
            var hasLayoutData = projectData.TabLayoutXml != null;

            // Handle missing files or layout options
            if (loadResult.RequiresUserIntervention)
            {
                // If NO valid files AND NO alternatives, always cancel
                if (loadResult.RequiresUserIntervention && !loadResult.HasValidFiles && loadResult.ValidationResult.PossibleAlternatives.Count == 0)
                {
                    _ = MessageBox.Show(
                        Resources.LoadProject_UI_Message_Message_FilesForSessionCouldNotBeFound,
                        Resources.LoadProject_UI_Message_Error_Title_SessionLoadFailed,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Show enhanced dialog with browsing capability and layout options
                var dialogResult = MissingFilesDialog.ShowDialog(
                    loadResult.ValidationResult,
                    hasLayoutData,
                    out var selectedAlternatives);

                if (dialogResult == MissingFilesDialogResult.Cancel)
                {
                    return;
                }

                // Handle layout-related results
                switch (dialogResult)
                {
                    case MissingFilesDialogResult.CloseTabsAndRestoreLayout:
                        CloseAllTabs();
                        break;
                    case MissingFilesDialogResult.OpenInNewWindow:
                        LogExpertProxy.NewWindow([.. projectData.FileNames]);
                        return;
                    case MissingFilesDialogResult.IgnoreLayout:
                        hasLayoutData = false;
                        break;
                }

                // Apply selected alternatives
                if (selectedAlternatives.Count > 0)
                {
                    _logger.Info($"User selected {selectedAlternatives.Count} alternative paths");

                    // Replace original paths with selected alternatives in project data
                    for (int i = 0; i < projectData.FileNames.Count; i++)
                    {
                        var originalPath = projectData.FileNames[i];
                        if (selectedAlternatives.TryGetValue(originalPath, out string value))
                        {
                            projectData.FileNames[i] = value;
                            _logger.Info($"Replaced {Path.GetFileName(originalPath)} with {Path.GetFileName(value)}");
                        }
                    }

                    // Update session file if user requested
                    if (dialogResult == MissingFilesDialogResult.LoadAndUpdateSession)
                    {
                        ProjectPersister.SaveProjectData(projectFileName, projectData);

                        _ = MessageBox.Show(
                            Resources.LoadProject_UI_Message_Error_Message_UpdateSessionFile,
                            Resources.LoadProject_UI_Message_Error_Title_UpdateSessionFile,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }

                // Load only valid files (original or replaced with alternatives)
                if (loadResult.RequiresUserIntervention)
                {
                    _logger.Info($"Loading {loadResult.ValidationResult.ValidFiles.Count} valid files");

                    // Filter project data to only include valid files (considering alternatives)
                    var filesToLoad = new List<string>();
                    foreach (var fileName in projectData.FileNames)
                    {
                        // Check if this file exists (either original or alternative)
                        try
                        {
                            var fs = PluginRegistry.PluginRegistry.Instance.FindFileSystemForUri(fileName);
                            if (fs != null)
                            {
                                var fileInfo = fs.GetLogfileInfo(fileName);
                                if (fileInfo != null)
                                {
                                    filesToLoad.Add(fileName);
                                }
                            }
                        }
                        catch (Exception ex) when (ex is FileNotFoundException or
                                                         DirectoryNotFoundException or
                                                         UnauthorizedAccessException or
                                                         IOException or
                                                         UriFormatException or
                                                         ArgumentException or
                                                         ArgumentNullException)
                        {
                            // File doesn't exist or can't be accessed, skip it
                            _logger.Warn($"Skipping inaccessible file: {fileName}");
                        }
                    }

                    projectData.FileNames = filesToLoad;
                }
            }
            else
            {
                // All files valid - proceed normally
                _logger.Info($"All {projectData.FileNames.Count} files found, loading project");
            }

            foreach (var fileName in projectData.FileNames)
            {
                _ = hasLayoutData
                    ? AddFileTabDeferred(fileName, false, null, true, null)
                    : AddFileTab(fileName, false, null, true, null);
            }

            // Restore layout only if we loaded at least one file
            if (hasLayoutData && restoreLayout && _logWindowList.Count > 0)
            {
                _logger.Info("Restoring layout");
                // Re-creating tool (non-document) windows is needed because the DockPanel control would throw strange errors
                DestroyToolWindows();
                InitToolWindows();
                RestoreLayout(projectData.TabLayoutXml);
            }
            else if (_logWindowList.Count == 0)
            {
                _logger.Warn("No files loaded, skipping layout restoration");
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show(
                $"Error loading project: {ex.Message}",
                Resources.LogExpert_Common_UI_Title_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    [SupportedOSPlatform("windows")]
    private void ApplySelectedHighlightGroup ()
    {
        var groupName = highlightGroupsToolStripComboBox.Text;
        CurrentLogWindow?.SetCurrentHighlightGroup(groupName);
    }

    [SupportedOSPlatform("windows")]
    private void FillToolLauncherBar ()
    {
        char[] labels =
        [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z'
        ];
        toolsToolStripMenuItem.DropDownItems.Clear();
        _ = toolsToolStripMenuItem.DropDownItems.Add(configureToolStripMenuItem);
        _ = toolsToolStripMenuItem.DropDownItems.Add(configureToolStripSeparator);
        externalToolsToolStrip.Items.Clear();
        var num = 0;
        externalToolsToolStrip.SuspendLayout();
        foreach (var tool in Preferences.ToolEntries)
        {
            if (tool.IsFavourite)
            {
                ToolStripButton button = new("" + labels[num % 26])
                {
                    Alignment = ToolStripItemAlignment.Left,
                    Tag = tool
                };

                SetToolIcon(tool, button);
                _ = externalToolsToolStrip.Items.Add(button);
            }

            num++;
            ToolStripMenuItem menuItem = new(tool.Name)
            {
                Tag = tool
            };

            SetToolIcon(tool, menuItem);
            _ = toolsToolStripMenuItem.DropDownItems.Add(menuItem);
        }

        externalToolsToolStrip.ResumeLayout();

        externalToolsToolStrip.Visible = num > 0; // do not show bar if no tool uses it
    }

    private static void RunGC ()
    {
        _logger.Info($"Running GC. Used mem before: {GC.GetTotalMemory(false):N0}");
        GC.Collect();
        _logger.Info($"GC done.    Used mem after:  {GC.GetTotalMemory(true):N0}");
    }

    private static void DumpGCInfo ()
    {
        _logger.Info($"-------- GC info -----------\r\nUsed mem: {GC.GetTotalMemory(false):N0}");
        for (var i = 0; i < GC.MaxGeneration; ++i)
        {
            _logger.Info($"Generation {i} collect count: {GC.CollectionCount(i)}");
        }

        _logger.Info(CultureInfo.InvariantCulture, "----------------------------");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "For Debug Purposes")]
    private void ThrowExceptionFx ()
    {
        throw new Exception(Resources.LogTabWindow_ThrowTestException_ThisIsATestExceptionThrownByAnAsyncDelegate);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "For Debug Purposes")]
    private void ThrowExceptionThreadFx ()
    {
        throw new Exception(Resources.LogTabWindow_ThrowTestExceptionThread_ThisIsATestExceptionThrownByABackgroundThread);
    }

    private string SaveLayout ()
    {
        using MemoryStream memStream = new(2000);
        using StreamReader r = new(memStream);
        dockPanel.SaveAsXml(memStream, Encoding.UTF8, true);

        _ = memStream.Seek(0, SeekOrigin.Begin);
        var resultXml = r.ReadToEnd();

        r.Close();

        return resultXml;
    }

    [SupportedOSPlatform("windows")]
    private void RestoreLayout (string layoutXml)
    {
        using MemoryStream memStream = new(2000);
        using StreamWriter w = new(memStream);
        w.Write(layoutXml);
        w.Flush();

        _ = memStream.Seek(0, SeekOrigin.Begin);

        dockPanel.LoadFromXml(memStream, DeserializeDockContent, true);
    }

    [SupportedOSPlatform("windows")]
    private IDockContent DeserializeDockContent (string persistString)
    {
        if (persistString.Equals(WindowTypes.BookmarkWindow.ToString(), StringComparison.Ordinal))
        {
            return _bookmarkWindow;
        }

        if (persistString.StartsWith(WindowTypes.LogWindow.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            var fileName = persistString[(WindowTypes.LogWindow.ToString().Length + 1)..];
            var win = FindWindowForFile(fileName);
            if (win != null)
            {
                return win;
            }

            //_logger.Warn("Layout data contains non-existing LogWindow for {fileName}"));
        }

        return null;
    }

    private void OnHighlightSettingsChanged ()
    {
        HighlightSettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Events handler

    private void OnBookmarkWindowVisibleChanged (object sender, EventArgs e)
    {
        _firstBookmarkWindowShow = false;
    }

    private void OnLogTabWindowLoad (object sender, EventArgs e)
    {
        ApplySettings(ConfigManager.Settings, SettingsFlags.All);
        if (ConfigManager.Settings.IsMaximized)
        {
            Bounds = ConfigManager.Settings.AppBoundsFullscreen;
            WindowState = FormWindowState.Maximized;
            Bounds = ConfigManager.Settings.AppBounds;
        }
        else
        {
            if (ConfigManager.Settings.AppBounds.Right > 0)
            {
                Bounds = ConfigManager.Settings.AppBounds;
            }
        }

        if (ConfigManager.Settings.Preferences.OpenLastFiles && _startupFileNames == null)
        {
            var tmpList = ObjectClone.Clone(ConfigManager.Settings.LastOpenFilesList);

            foreach (var name in tmpList)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    AddFileTab(name, false, null, false, null);
                }
            }
        }

        if (_startupFileNames != null)
        {
            LoadFiles(_startupFileNames, false);
        }

        _ledThread = new Thread(LedThreadProc)
        {
            IsBackground = true
        };
        _ledThread.Start();

        FillHighlightComboBox();
        FillToolLauncherBar();

        //TODO Change to Debug and true
#if !DEBUG
        debugToolStripMenuItem.Visible = false;
#endif
    }

    private void OnLogTabWindowFormClosing (object sender, CancelEventArgs e)
    {
        try
        {
            _shouldStop = true;
            _ = _statusLineEventHandle.Set();
            _ = _statusLineEventWakeupHandle.Set();
            _ledThread.Join();

            IList<LogWindow.LogWindow> deleteLogWindowList = [];
            ConfigManager.Settings.AlwaysOnTop = TopMost && ConfigManager.Settings.Preferences.AllowOnlyOneInstance;
            SaveLastOpenFilesList();

            foreach (var logWindow in _logWindowList.ToArray())
            {
                RemoveAndDisposeLogWindow(logWindow, true);
            }

            DestroyBookmarkWindow();

            ConfigManager.ConfigChanged -= OnConfigChanged;

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

    private void OnStripMouseUp (object sender, MouseEventArgs e)
    {
        if (sender is ToolStripDropDown dropDown)
        {
            _ = AddFileTab(dropDown.Text, false, null, false, null);
        }
    }

    private void OnHistoryItemClicked (object sender, ToolStripItemClickedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.ClickedItem.Text))
        {
            _ = AddFileTab(e.ClickedItem.Text, false, null, false, null);
        }
    }

    private void OnLogWindowDisposed (object sender, EventArgs e)
    {
        var logWindow = sender as LogWindow.LogWindow;

        if (sender == CurrentLogWindow)
        {
            ChangeCurrentLogWindow(null);
        }

        RemoveLogWindow(logWindow);

        logWindow.Tag = null;
    }

    private void OnExitToolStripMenuItemClick (object sender, EventArgs e)
    {
        Close();
    }

    private void OnSelectFilterToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        CurrentLogWindow.ColumnizerCallbackObject.LineNum = CurrentLogWindow.CurrentLineNum;
        FilterSelectorForm form = new(PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers, CurrentLogWindow.CurrentColumnizer, CurrentLogWindow.ColumnizerCallbackObject, ConfigManager)
        {
            Owner = this,
            TopMost = TopMost
        };
        var res = form.ShowDialog();

        if (res == DialogResult.OK)
        {
            if (form.ApplyToAll)
            {
                lock (_logWindowList)
                {
                    foreach (var logWindow in _logWindowList)
                    {
                        if (logWindow.CurrentColumnizer.GetType() != form.SelectedColumnizer.GetType())
                        {
                            //logWindow.SetColumnizer(form.SelectedColumnizer);
                            SetColumnizerFx fx = logWindow.ForceColumnizer;
                            _ = logWindow.Invoke(fx, form.SelectedColumnizer);
                            SetColumnizerHistoryEntry(logWindow.FileName, form.SelectedColumnizer);
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
                    _ = CurrentLogWindow.Invoke(fx, form.SelectedColumnizer);
                    SetColumnizerHistoryEntry(CurrentLogWindow.FileName, form.SelectedColumnizer);
                }

                if (form.IsConfigPressed)
                {
                    lock (_logWindowList)
                    {
                        foreach (var logWindow in _logWindowList)
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

    private void OnGoToLineToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        GotoLineDialog dlg = new(this);
        var res = dlg.ShowDialog();
        if (res == DialogResult.OK)
        {
            var line = dlg.Line - 1;
            if (line >= 0)
            {
                CurrentLogWindow.GotoLine(line);
            }
        }
    }

    private void OnHighlightingToolStripMenuItemClick (object sender, EventArgs e)
    {
        ShowHighlightSettingsDialog();
    }

    private void OnSearchToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenSearchDialog();
    }

    private void OnOpenToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenFileDialog();
    }

    private void OnLogTabWindowDragEnter (object sender, DragEventArgs e)
    {
#if DEBUG
        var formats = e.Data.GetFormats();
        var s = "Dragging something over LogExpert. Formats: ";
        foreach (var format in formats)
        {
            s += format;
            s += " , ";
        }

        s = s[..^3];
        _logger.Info(s);
#endif
    }

    private void OnLogWindowDragOver (object sender, DragEventArgs e)
    {
        e.Effect = !e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.None
            : DragDropEffects.Copy;
    }

    private void OnLogWindowDragDrop (object sender, DragEventArgs e)
    {
#if DEBUG
        var formats = e.Data.GetFormats();
        var s = "Dropped formats: ";
        foreach (var format in formats)
        {
            s += format;
            s += " , ";
        }

        s = s[..^3];
        _logger.Debug(s);
#endif

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var o = e.Data.GetData(DataFormats.FileDrop);
            if (o is string[] names)
            {
                // (shift pressed) https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.drageventargs.keystate
                LoadFiles(names, (e.KeyState & 4) == 4);
                e.Effect = DragDropEffects.Copy;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnTimeShiftToolStripMenuItemCheckStateChanged (object sender, EventArgs e)
    {
        if (!_skipEvents && CurrentLogWindow != null)
        {
            CurrentLogWindow.SetTimeshiftValue(timeshiftToolStripTextBox.Text);
            timeshiftToolStripTextBox.Enabled = timeshiftToolStripMenuItem.Checked;
            CurrentLogWindow.TimeshiftEnabled(timeshiftToolStripMenuItem.Checked,
                timeshiftToolStripTextBox.Text);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnAboutToolStripMenuItemClick (object sender, EventArgs e)
    {
        AboutBox aboutBox = new()
        {
            TopMost = TopMost
        };

        _ = aboutBox.ShowDialog();
    }

    private void OnFilterToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleFilterPanel();
    }

    [SupportedOSPlatform("windows")]
    private void OnMultiFileToolStripMenuItemClick (object sender, EventArgs e)
    {
        ToggleMultiFile();
        fileToolStripMenuItem.HideDropDown();
    }

    [SupportedOSPlatform("windows")]
    private void OnGuiStateUpdate (object sender, GuiStateEventArgs e)
    {
        _ = BeginInvoke(GuiStateUpdateWorker, e);
    }

    private void OnColumnizerChanged (object sender, ColumnizerEventArgs e)
    {
        _bookmarkWindow?.SetColumnizer(e.Columnizer);
    }

    private void OnBookmarkAdded (object sender, EventArgs e)
    {
        _bookmarkWindow.UpdateView();
    }

    private void OnBookmarkTextChanged (object sender, BookmarkEventArgs e)
    {
        _bookmarkWindow.BookmarkTextChanged(e.Bookmark);
    }

    private void OnBookmarkRemoved (object sender, EventArgs e)
    {
        _bookmarkWindow.UpdateView();
    }

    private void OnProgressBarUpdate (object sender, ProgressEventArgs e)
    {
        _ = Invoke(ProgressBarUpdateWorker, e);
    }

    private void OnStatusLineEvent (object sender, StatusLineEventArgs e)
    {
        StatusLineEventWorker(e);
    }

    private void OnFollowTailCheckBoxClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.FollowTailChanged(checkBoxFollowTail.Checked, false);
    }

    private void OnLogTabWindowKeyDown (object sender, KeyEventArgs e)
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
            CurrentLogWindow?.OnLogWindowKeyDown(sender, e);
        }
    }

    private void OnCloseFileToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.Close();
    }

    [SupportedOSPlatform("windows")]
    private void OnCellSelectModeToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.SetCellSelectionMode(cellSelectModeToolStripMenuItem.Checked);
    }

    private void OnCopyMarkedLinesIntoNewTabToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.CopyMarkedLinesToTab();
    }

    private void OnTimeShiftMenuTextBoxKeyDown (object sender, KeyEventArgs e)
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            CurrentLogWindow.SetTimeshiftValue(timeshiftToolStripTextBox.Text);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnAlwaysOnTopToolStripMenuItemClick (object sender, EventArgs e)
    {
        TopMost = alwaysOnTopToolStripMenuItem.Checked;
    }

    private void OnFileSizeChanged (object sender, LogEventArgs e)
    {
        if (sender.GetType().IsAssignableFrom(typeof(LogWindow.LogWindow)))
        {
            var diff = e.LineCount - e.PrevLineCount;
            if (diff < 0)
            {
                return;
            }

            if (((LogWindow.LogWindow)sender).Tag is LogWindowData data)
            {
                lock (data)
                {
                    data.DiffSum += diff;
                    if (data.DiffSum > DIFF_MAX)
                    {
                        data.DiffSum = DIFF_MAX;
                    }
                }

                //if (this.dockPanel.ActiveContent != null &&
                //    this.dockPanel.ActiveContent != sender || data.tailState != 0)
                if (CurrentLogWindow != null && CurrentLogWindow != sender || data.TailState != 0)
                {
                    data.Dirty = true;
                }

                var icon = GetLedIcon(diff, data);
                _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow.LogWindow)sender, icon);
            }
        }
    }

    private void OnLogWindowFileNotFound (object sender, EventArgs e)
    {
        _ = Invoke(new FileNotFoundDelegate(FileNotFound), sender);
    }

    private void OnLogWindowFileRespawned (object sender, EventArgs e)
    {
        _ = Invoke(new FileRespawnedDelegate(FileRespawned), sender);
    }

    private void OnLogWindowFilterListChanged (object sender, FilterListChangedEventArgs e)
    {
        lock (_logWindowList)
        {
            foreach (var logWindow in _logWindowList)
            {
                if (logWindow != e.LogWindow)
                {
                    logWindow.HandleChangedFilterList();
                }
            }
        }

        ConfigManager.Save(SettingsFlags.FilterList);
    }

    private void OnLogWindowCurrentHighlightGroupChanged (object sender, CurrentHighlightGroupChangedEventArgs e)
    {
        OnHighlightSettingsChanged();
        ConfigManager.Settings.Preferences.HighlightGroupList = HighlightGroupList;
        ConfigManager.Save(SettingsFlags.HighlightSettings);
    }

    private void OnTailFollowed (object sender, EventArgs e)
    {
        if (dockPanel.ActiveContent == null)
        {
            return;
        }

        if (sender.GetType().IsAssignableFrom(typeof(LogWindow.LogWindow)))
        {
            if (dockPanel.ActiveContent == sender)
            {
                var data = ((LogWindow.LogWindow)sender).Tag as LogWindowData;
                data.Dirty = false;
                var icon = GetLedIcon(data.DiffSum, data);
                _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow.LogWindow)sender, icon);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLogWindowSyncModeChanged (object sender, SyncModeEventArgs e)
    {
        if (!Disposing)
        {
            var data = ((LogWindow.LogWindow)sender).Tag as LogWindowData;
            data.SyncMode = e.IsTimeSynced ? 1 : 0;
            var icon = GetLedIcon(data.DiffSum, data);
            _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow.LogWindow)sender, icon);
        }
        //else
        //{
        //    _logger.Warn("Received SyncModeChanged event while disposing. Event ignored.");
        //}
    }

    [SupportedOSPlatform("windows")]
    private void OnToggleBookmarkToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnJumpToNextToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpNextBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnJumpToPrevToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpPrevBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnASCIIToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.ASCII);
    }

    [SupportedOSPlatform("windows")]
    private void OnANSIToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.Default);
    }

    [SupportedOSPlatform("windows")]
    private void OnUTF8ToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(new UTF8Encoding(false));
    }

    [SupportedOSPlatform("windows")]
    private void OnUTF16ToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.Unicode);
    }

    [SupportedOSPlatform("windows")]
    private void OnISO88591ToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.GetEncoding("iso-8859-1"));
    }

    [SupportedOSPlatform("windows")]
    private void OnReloadToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null)
        {
            var data = CurrentLogWindow.Tag as LogWindowData;
            var icon = GetLedIcon(0, data);
            _ = BeginInvoke(new SetTabIconDelegate(SetTabIcon), CurrentLogWindow, icon);
            CurrentLogWindow.Reload();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnSettingsToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenSettings(0);
    }

    [SupportedOSPlatform("windows")]
    private void OnPluginTrustToolStripMenuItemClick (object sender, EventArgs e)
    {
        using var dialog = new PluginTrustDialog(this);
        var result = dialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            var restartPrompt = MessageBox.Show(
                Resources.LogTabWindow_UI_Message_PluginTrustConfigurationUpdate,
                Resources.LogTabWindow_UI_Title_RestartRecommended,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (restartPrompt == DialogResult.Yes)
            {
                Application.Restart();
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDateTimeDragControlValueDragged (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null)
        {
            //this.CurrentLogWindow.ScrollToTimestamp(this.dateTimeDragControl.DateTime);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDateTimeDragControlValueChanged (object sender, EventArgs e)
    {
        _ = CurrentLogWindow?.ScrollToTimestamp(dragControlDateTime.DateTime, true, true);
    }

    [SupportedOSPlatform("windows")]
    private void OnLogTabWindowDeactivate (object sender, EventArgs e)
    {
        CurrentLogWindow?.AppFocusLost();
    }

    [SupportedOSPlatform("windows")]
    private void OnLogTabWindowActivated (object sender, EventArgs e)
    {
        LogExpertProxy?.NotifyWindowActivated(this);
        CurrentLogWindow?.AppFocusGained();
    }

    [SupportedOSPlatform("windows")]
    private void OnShowBookmarkListToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (_bookmarkWindow.Visible)
        {
            _bookmarkWindow.Hide();
        }
        else
        {
            // strange: on very first Show() now bookmarks are displayed. after a hide it will work.
            if (_firstBookmarkWindowShow)
            {
                _bookmarkWindow.Show(dockPanel);
                _bookmarkWindow.Hide();
            }

            _bookmarkWindow.Show(dockPanel);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonOpenClick (object sender, EventArgs e)
    {
        OpenFileDialog();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonSearchClick (object sender, EventArgs e)
    {
        OpenSearchDialog();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonFilterClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleFilterPanel();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonBookmarkClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonUpClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpPrevBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonDownClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpNextBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnShowHelpToolStripMenuItemClick (object sender, EventArgs e)
    {
        Help.ShowHelp(this, Resources.LogTabWindow_HelpFile);
    }

    private void OnHideLineColumnToolStripMenuItemClick (object sender, EventArgs e)
    {
        ConfigManager.Settings.HideLineColumn = hideLineColumnToolStripMenuItem.Checked;

        lock (_logWindowList)
        {
            foreach (var logWin in _logWindowList)
            {
                logWin.ShowLineColumn(!ConfigManager.Settings.HideLineColumn);
            }
        }

        _bookmarkWindow.LineColumnVisible = ConfigManager.Settings.HideLineColumn;
    }

    // ==================================================================
    // Tab context menu stuff
    // ==================================================================

    [SupportedOSPlatform("windows")]
    private void OnCloseThisTabToolStripMenuItemClick (object sender, EventArgs e)
    {
        (dockPanel.ActiveContent as LogWindow.LogWindow).Close();
    }

    [SupportedOSPlatform("windows")]
    private void OnCloseOtherTabsToolStripMenuItemClick (object sender, EventArgs e)
    {
        var closeList = dockPanel.Contents
                .OfType<LogWindow.LogWindow>()
                .Where(content => content != dockPanel.ActiveContent)
                .ToList();

        foreach (var logWindow in closeList)
        {
            logWindow.Close();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnCloseAllTabsToolStripMenuItemClick (object sender, EventArgs e)
    {
        CloseAllTabs();
    }

    [SupportedOSPlatform("windows")]
    private void OnTabColorToolStripMenuItemClick (object sender, EventArgs e)
    {
        //Todo TabColoring must be reimplemented with a different UI Framework
        //var logWindow = dockPanel.ActiveContent as LogWindow.LogWindow;

        //if (logWindow.Tag is not LogWindowData data)
        //{
        //    return;
        //}

        //ColorDialog dlg = new()
        //{
        //    Color = data.Color
        //};

        //if (dlg.ShowDialog() == DialogResult.OK)
        //{
        //    data.Color = dlg.Color;
        //    //SetTabColor(logWindow, data.Color);
        //}

        //List<ColorEntry> delList = [];

        //foreach (var entry in ConfigManager.Settings.FileColors)
        //{
        //    if (entry.FileName.Equals(logWindow.FileName, StringComparison.Ordinal))
        //    {
        //        delList.Add(entry);
        //    }
        //}

        //foreach (var entry in delList)
        //{
        //    _ = ConfigManager.Settings.FileColors.Remove(entry);
        //}

        //ConfigManager.Settings.FileColors.Add(new ColorEntry(logWindow.FileName, dlg.Color));

        //while (ConfigManager.Settings.FileColors.Count > MAX_COLOR_HISTORY)
        //{
        //    ConfigManager.Settings.FileColors.RemoveAt(0);
        //}
    }

    [SupportedOSPlatform("windows")]
    private void OnLogTabWindowSizeChanged (object sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized)
        {
            _wasMaximized = WindowState == FormWindowState.Maximized;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnSaveProjectToolStripMenuItemClick (object sender, EventArgs e)
    {
        SaveFileDialog dlg = new()
        {
            DefaultExt = "lxj",
            Filter = string.Format(CultureInfo.InvariantCulture, Resources.LogTabWindow_UI_Project_Session_Default_Filter, "(*.lxj)|*.lxj")
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var fileName = dlg.FileName;
            List<string> fileNames = [];

            lock (_logWindowList)
            {
                foreach (var logWindow in dockPanel.Contents.OfType<LogWindow.LogWindow>())
                {
                    var persistenceFileName = logWindow?.SavePersistenceDataAndReturnFileName(true);
                    if (persistenceFileName != null)
                    {
                        fileNames.Add(persistenceFileName);
                    }
                }
            }

            ProjectData projectData = new()
            {
                FileNames = fileNames,
                TabLayoutXml = SaveLayout()
            };

            ProjectPersister.SaveProjectData(fileName, projectData);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLoadProjectToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenFileDialog dlg = new()
        {
            DefaultExt = "lxj",
            Filter = string.Format(CultureInfo.InvariantCulture, Resources.LogTabWindow_UI_Project_Session_Default_Filter, "(*.lxj)|*.lxj")
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var projectFileName = dlg.FileName;
            LoadProject(projectFileName, true);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonBubblesClick (object sender, EventArgs e)
    {
        _ = CurrentLogWindow?.ShowBookmarkBubbles = toolStripButtonBubbles.Checked;
    }

    [SupportedOSPlatform("windows")]
    private void OnCopyPathToClipboardToolStripMenuItemClick (object sender, EventArgs e)
    {
        var logWindow = dockPanel.ActiveContent as LogWindow.LogWindow;
        Clipboard.SetText(logWindow.Title);
    }

    private void OnFindInExplorerToolStripMenuItemClick (object sender, EventArgs e)
    {
        var logWindow = dockPanel.ActiveContent as LogWindow.LogWindow;

        Process explorer = new();
        explorer.StartInfo.FileName = "explorer.exe";
        explorer.StartInfo.Arguments = "/e,/select," + logWindow.Title;
        explorer.StartInfo.UseShellExecute = false;
        _ = explorer.Start();
    }

    private void TruncateFileToolStripMenuItem_Click (object sender, EventArgs e)
    {
        CurrentLogWindow?.TryToTruncate();
    }

    private void OnExportBookmarksToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ExportBookmarkList();
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightGroupsComboBoxDropDownClosed (object sender, EventArgs e)
    {
        ApplySelectedHighlightGroup();
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightGroupsComboBoxSelectedIndexChanged (object sender, EventArgs e)
    {
        ApplySelectedHighlightGroup();
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightGroupsComboBoxMouseUp (object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            ShowHighlightSettingsDialog();
        }
    }

    private void OnConfigChanged (object sender, ConfigChangedEventArgs e)
    {
        if (LogExpertProxy != null)
        {
            NotifySettingsChanged(null, e.Flags);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDumpLogBufferInfoToolStripMenuItemClick (object sender, EventArgs e)
    {
#if DEBUG
        CurrentLogWindow?.DumpBufferInfo();
#endif
    }

    [SupportedOSPlatform("windows")]
    private void OnDumpBufferDiagnosticToolStripMenuItemClick (object sender, EventArgs e)
    {
#if DEBUG
        CurrentLogWindow?.DumpBufferDiagnostic();
#endif
    }

    private void OnRunGCToolStripMenuItemClick (object sender, EventArgs e)
    {
        RunGC();
    }

    private void OnGCInfoToolStripMenuItemClick (object sender, EventArgs e)
    {
        DumpGCInfo();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolsToolStripMenuItemDropDownItemClicked (object sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem.Tag is ToolEntry tag)
        {
            ToolButtonClick(tag);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnExternalToolsToolStripItemClicked (object sender, ToolStripItemClickedEventArgs e)
    {
        ToolButtonClick(e.ClickedItem.Tag as ToolEntry);
    }

    [SupportedOSPlatform("windows")]
    private void OnConfigureToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenSettings(2);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "For Debug Purposes")]
    private void OnThrowExceptionGUIThreadToolStripMenuItemClick (object sender, EventArgs e)
    {
        throw new Exception(Resources.LogTabWindow_OnThrowTestExceptionGUIThread_ThisIsATestExceptionThrownByTheGUIThread);
    }

    private void OnThrowExceptionBackgroundThToolStripMenuItemClick (object sender, EventArgs e)
    {
        ExceptionFx fx = ThrowExceptionFx;
        _ = fx.BeginInvoke(null, null);
    }

    private void OnThrowExceptionBackgroundThreadToolStripMenuItemClick (object sender, EventArgs e)
    {
        Thread thread = new(ThrowExceptionThreadFx)
        {
            IsBackground = true
        };

        thread.Start();
    }

    private void OnWarnToolStripMenuItemClick (object sender, EventArgs e)
    {
        //_logger.GetLogger().LogLevel = _logger.Level.WARN;
    }

    private void OnInfoToolStripMenuItemClick (object sender, EventArgs e)
    {
        //_logger.Get_logger().LogLevel = _logger.Level.INFO;
    }

    private void OnDebugLogLevelToolStripMenuItemClick (object sender, EventArgs e)
    {
        //_logger.Get_logger().LogLevel = _logger.Level.DEBUG;
    }

    private void OnLogLevelToolStripMenuItemClick (object sender, EventArgs e)
    {
    }

    private void OnLogLevelToolStripMenuItemDropDownOpening (object sender, EventArgs e)
    {
        //warnToolStripMenuItem.Checked = _logger.Get_logger().LogLevel == _logger.Level.WARN;
        //infoToolStripMenuItem.Checked = _logger.Get_logger().LogLevel == _logger.Level.INFO;
        //debugToolStripMenuItem1.Checked = _logger.Get_logger().LogLevel == _logger.Level.DEBUG;
    }

    [SupportedOSPlatform("windows")]
    private void OnDisableWordHighlightModeToolStripMenuItemClick (object sender, EventArgs e)
    {
        DebugOptions.DisableWordHighlight = disableWordHighlightModeToolStripMenuItem.Checked;
        CurrentLogWindow?.RefreshAllGrids();
    }

    [SupportedOSPlatform("windows")]
    private void OnMultiFileMaskToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeMultifileMask();
    }

    [SupportedOSPlatform("windows")]
    private void OnMultiFileEnabledStripMenuItemClick (object sender, EventArgs e)
    {
        ToggleMultiFile();
    }

    [SupportedOSPlatform("windows")]
    private void OnLockInstanceToolStripMenuItemClick (object sender, EventArgs e)
    {
        AbstractLogTabWindow.StaticData.CurrentLockedMainWindow = lockInstanceToolStripMenuItem.Checked ? null : this;
    }

    [SupportedOSPlatform("windows")]
    private void OnOptionToolStripMenuItemDropDownOpening (object sender, EventArgs e)
    {
        lockInstanceToolStripMenuItem.Enabled = !ConfigManager.Settings.Preferences.AllowOnlyOneInstance;
        lockInstanceToolStripMenuItem.Checked = AbstractLogTabWindow.StaticData.CurrentLockedMainWindow == this;
    }

    [SupportedOSPlatform("windows")]
    private void OnFileToolStripMenuItemDropDownOpening (object sender, EventArgs e)
    {
        newFromClipboardToolStripMenuItem.Enabled = Clipboard.ContainsText();
    }

    [SupportedOSPlatform("windows")]
    private void OnNewFromClipboardToolStripMenuItemClick (object sender, EventArgs e)
    {
        PasteFromClipboard();
    }

    [SupportedOSPlatform("windows")]
    private void OnOpenURIToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenUriDialog dlg = new()
        {
            UriHistory = ConfigManager.Settings.UriHistoryList
        };

        if (DialogResult.OK == dlg.ShowDialog())
        {
            if (dlg.Uri.Trim().Length > 0)
            {
                ConfigManager.Settings.UriHistoryList = dlg.UriHistory;
                ConfigManager.Save(SettingsFlags.FileHistory);
                LoadFiles([dlg.Uri], false);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnColumnFinderToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null && !_skipEvents)
        {
            CurrentLogWindow.ToggleColumnFinder(columnFinderToolStripMenuItem.Checked, true);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDockPanelActiveContentChanged (object sender, EventArgs e)
    {
        if (dockPanel.ActiveContent is LogWindow.LogWindow window)
        {
            CurrentLogWindow = window;
            CurrentLogWindow.LogWindowActivated();
            ConnectToolWindows(CurrentLogWindow);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnTabRenameToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null)
        {
            TabRenameDialog dlg = new()
            {
                TabName = CurrentLogWindow.Text
            };

            if (DialogResult.OK == dlg.ShowDialog())
            {
                CurrentLogWindow.Text = dlg.TabName;
            }

            dlg.Dispose();
        }
    }

    #endregion
}
