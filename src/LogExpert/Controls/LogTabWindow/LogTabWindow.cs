using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LogExpert.Config;
using LogExpert.Dialogs;
using LogExpert.Entities;
using LogExpert.Entities.EventArgs;
using LogExpert.Interface;
using NLog;
//using System.Linq;

namespace LogExpert.Controls.LogTabWindow
{
    public partial class LogTabWindow : Form
    {
        #region Fields

        private const int MAX_COLUMNIZER_HISTORY = 40;
        private const int MAX_COLOR_HISTORY = 40;
        private const int DIFF_MAX = 100;
        private const int MAX_FILE_HISTORY = 10;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly Icon _deadIcon;

        private readonly Color _defaultTabColor = Color.FromArgb(255, 192, 192, 192);
        private readonly Brush _dirtyLedBrush;

        private readonly int _instanceNumber;
        private readonly Brush[] _ledBrushes = new Brush[5];
        private readonly Icon[,,,] _ledIcons = new Icon[6, 2, 4, 2];

        private readonly Rectangle[] _leds = new Rectangle[5];

        private readonly IList<LogWindow.LogWindow> _logWindowList = new List<LogWindow.LogWindow>();
        private readonly Brush _offLedBrush;
        private readonly bool _showInstanceNumbers;

        private readonly string[] _startupFileNames;

        private readonly EventWaitHandle _statusLineEventHandle = new AutoResetEvent(false);
        private readonly EventWaitHandle _statusLineEventWakeupHandle = new ManualResetEvent(false);
        private readonly object _statusLineLock = new object();
        private readonly Brush _syncLedBrush;
        private readonly StringFormat _tabStringFormat = new StringFormat();
        private readonly Brush[] _tailLedBrush = new Brush[3];

        private BookmarkWindow _bookmarkWindow;

        private LogWindow.LogWindow _currentLogWindow;
        private bool _firstBookmarkWindowShow = true;

        private StatusLineEventArgs _lastStatusLineEvent;

        private Thread _ledThread;

        //Settings settings;

        private bool _shouldStop;

        private bool _skipEvents;

        private Thread _statusLineThread;
        private bool _wasMaximized;

        #endregion

        #region cTor

        public LogTabWindow(string[] fileNames, int instanceNumber, bool showInstanceNumbers)
        {
            InitializeComponent();
            _startupFileNames = fileNames;
            this._instanceNumber = instanceNumber;
            this._showInstanceNumbers = showInstanceNumbers;

            Load += OnLogTabWindowLoad;

            ConfigManager.Instance.ConfigChanged += ConfigChanged;
            HilightGroupList = ConfigManager.Settings.hilightGroupList;

            Rectangle led = new Rectangle(0, 0, 8, 2);
            
            for (int i = 0; i < _leds.Length; ++i)
            {
                _leds[i] = led;
                led.Offset(0, led.Height + 0);
            }

            int grayAlpha = 50;

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

            ToolStripControlHost host = new ToolStripControlHost(checkBoxFollowTail);
            
            host.Padding = new Padding(20, 0, 0, 0);
            host.BackColor = Color.FromKnownColor(KnownColor.Transparent);
            
            int index = buttonToolStrip.Items.IndexOfKey("toolStripButtonTail");

            toolStripEncodingASCIIItem.Text = Encoding.ASCII.HeaderName;
            toolStripEncodingANSIItem.Text = Encoding.Default.HeaderName;
            toolStripEncodingISO88591Item.Text = Encoding.GetEncoding("iso-8859-1").HeaderName;
            toolStripEncodingUTF8Item.Text = Encoding.UTF8.HeaderName;
            toolStripEncodingUTF16Item.Text = Encoding.Unicode.HeaderName;

            if (index != -1)
            {
                buttonToolStrip.Items.RemoveAt(index);
                buttonToolStrip.Items.Insert(index, host);
            }

            dragControlDateTime.Visible = false;
            loadProgessBar.Visible = false;

            // get a reference to the current assembly
            Assembly a = Assembly.GetExecutingAssembly();

            // get a list of resource names from the manifest
            string[] resNames = a.GetManifestResourceNames();

            Bitmap bmp = Properties.Resources.delete_page_red;
            _deadIcon = Icon.FromHandle(bmp.GetHicon());
            bmp.Dispose();
            Closing += OnLogTabWindowClosing;

            InitToolWindows();
        }

        #endregion

        #region Delegates

        private delegate void AddFileTabsDelegate(string[] fileNames);

        private delegate void ExceptionFx();

        private delegate void FileNotFoundDelegate(LogWindow.LogWindow logWin);

        private delegate void FileRespawnedDelegate(LogWindow.LogWindow logWin);

        private delegate void GuiStateUpdateWorkerDelegate(GuiStateArgs e);

        public delegate void HighlightSettingsChangedEventHandler(object sender, EventArgs e);

        private delegate void LoadFileDelegate(string fileName, EncodingOptions encodingOptions);

        private delegate void LoadMultiFilesDelegate(string[] fileName, EncodingOptions encodingOptions);

        private delegate void ProgressBarEventFx(ProgressEventArgs e);

        private delegate void SetColumnizerFx(ILogLineColumnizer columnizer);

        private delegate void SetTabIconDelegate(LogWindow.LogWindow logWindow, Icon icon);

        private delegate void StatusLineEventFx(StatusLineEventArgs e);

        #endregion

        #region Events

        public event HighlightSettingsChangedEventHandler HighlightSettingsChanged;

        #endregion

        #region Properties

        public LogWindow.LogWindow CurrentLogWindow
        {
            get => _currentLogWindow;
            set => ChangeCurrentLogWindow(value);
        }

        public SearchParams SearchParams { get; private set; } = new SearchParams();

        public Preferences Preferences => ConfigManager.Settings.preferences;

        public List<HilightGroup> HilightGroupList { get; private set; } = new List<HilightGroup>();

        //public Settings Settings
        //{
        //  get { return ConfigManager.Settings; }
        //}

        public ILogExpertProxy LogExpertProxy { get; set; }

        internal static StaticLogTabWindowData StaticData { get; set; } = new StaticLogTabWindowData();

        #endregion

        #region Internals

        internal HilightGroup FindHighlightGroup(string groupName)
        {
            lock (HilightGroupList)
            {
                foreach (HilightGroup group in HilightGroupList)
                {
                    if (group.GroupName.Equals(groupName))
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
            public Color color = Color.FromKnownColor(KnownColor.Gray);

            public int diffSum;
            public bool dirty;
            public int syncMode; // 0 = off, 1 = timeSynced
            public int tailState; // tailState: 0,1,2 = on/off/off by Trigger
            public ToolTip toolTip;

            #endregion
        }

        // Data shared over all LogTabWindow instances
        internal class StaticLogTabWindowData
        {
            #region Properties

            public LogTabWindow CurrentLockedMainWindow { get; set; }

            #endregion
        }
    }
}