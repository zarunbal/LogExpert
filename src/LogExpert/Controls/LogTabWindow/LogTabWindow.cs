using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using LogExpert.Dialogs;
using NLog;

// using System.Linq;
namespace LogExpert
{
    public partial class LogTabWindow : Form
    {
        #region Delegates

        public delegate void HighlightSettingsChangedEventHandler(object sender, EventArgs e);

        private delegate void AddFileTabsDelegate(string[] fileNames);

        private delegate void ExceptionFx();

        private delegate void FileNotFoundDelegate(LogWindow logWin);

        private delegate void FileRespawnedDelegate(LogWindow logWin);

        private delegate void GuiStateUpdateWorkerDelegate(GuiStateArgs e);

        private delegate void LoadFileDelegate(string fileName, EncodingOptions encodingOptions);

        private delegate void LoadMultiFilesDelegate(string[] fileName, EncodingOptions encodingOptions);

        private delegate void ProgressBarEventFx(ProgressEventArgs e);

        private delegate void SetColumnizerFx(ILogLineColumnizer columnizer);

        private delegate void SetTabIconDelegate(LogWindow logWindow, Icon icon);

        private delegate void StatusLineEventFx(StatusLineEventArgs e);

        #endregion

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Static/Constants

        private const int DIFF_MAX = 100;
        private const int MAX_COLOR_HISTORY = 40;

        private const int MAX_COLUMNIZER_HISTORY = 40;
        private const int MAX_FILE_HISTORY = 10;

        #endregion

        #region Private Fields

        private readonly Icon deadIcon;

        private readonly Color defaultTabColor = Color.FromArgb(255, 192, 192, 192);
        private readonly Brush dirtyLedBrush;

        private readonly int instanceNumber;
        private readonly Brush[] ledBrushes = new Brush[5];
        private readonly Icon[,,,] ledIcons = new Icon[6, 2, 4, 2];

        private readonly Rectangle[] leds = new Rectangle[5];

        private readonly IList<LogWindow> logWindowList = new List<LogWindow>();
        private readonly Brush offLedBrush;
        private readonly bool showInstanceNumbers;

        private readonly string[] startupFileNames;

        private readonly EventWaitHandle statusLineEventHandle = new AutoResetEvent(false);
        private readonly EventWaitHandle statusLineEventWakeupHandle = new ManualResetEvent(false);
        private readonly object statusLineLock = new object();
        private readonly Brush syncLedBrush;
        private readonly StringFormat tabStringFormat = new StringFormat();
        private readonly Brush[] tailLedBrush = new Brush[3];

        private BookmarkWindow bookmarkWindow;

        private LogWindow currentLogWindow;
        private bool firstBookmarkWindowShow = true;

        private StatusLineEventArgs lastStatusLineEvent;

        private Thread ledThread;

        // Settings settings;
        private bool shouldStop;

        private bool skipEvents;

        private Thread statusLineThread;
        private bool wasMaximized;

        #endregion

        #region Public Events

        public event HighlightSettingsChangedEventHandler HighlightSettingsChanged;

        #endregion

        #region Ctor

        public LogTabWindow(string[] fileNames, int instanceNumber, bool showInstanceNumbers)
        {
            InitializeComponent();
            startupFileNames = fileNames;
            this.instanceNumber = instanceNumber;
            this.showInstanceNumbers = showInstanceNumbers;

            Load += LogTabWindow_Load;

            ConfigManager.Instance.ConfigChanged += ConfigChanged;
            HilightGroupList = ConfigManager.Settings.hilightGroupList;

            Rectangle led = new Rectangle(0, 0, 8, 2);
            for (int i = 0; i < leds.Length; ++i)
            {
                leds[i] = led;
                led.Offset(0, led.Height + 0);
            }

            int grayAlpha = 50;
            ledBrushes[0] = new SolidBrush(Color.FromArgb(255, 220, 0, 0));
            ledBrushes[1] = new SolidBrush(Color.FromArgb(255, 220, 220, 0));
            ledBrushes[2] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
            ledBrushes[3] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
            ledBrushes[4] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
            offLedBrush = new SolidBrush(Color.FromArgb(grayAlpha, 160, 160, 160));
            dirtyLedBrush = new SolidBrush(Color.FromArgb(255, 220, 0, 00));
            tailLedBrush[0] = new SolidBrush(Color.FromArgb(255, 50, 100, 250)); // Follow tail: blue-ish
            tailLedBrush[1] = new SolidBrush(Color.FromArgb(grayAlpha, 160, 160, 160)); // Don't follow tail: gray
            tailLedBrush[2] =
                new SolidBrush(Color.FromArgb(255, 220, 220, 0)); // Stop follow tail (trigger): yellow-ish
            syncLedBrush = new SolidBrush(Color.FromArgb(255, 250, 145, 30));
            CreateIcons();
            tabStringFormat.LineAlignment = StringAlignment.Center;
            tabStringFormat.Alignment = StringAlignment.Near;

            ToolStripControlHost host = new ToolStripControlHost(followTailCheckBox);
            host.Padding = new Padding(20, 0, 0, 0);
            host.BackColor = Color.FromKnownColor(KnownColor.Transparent);
            int index = toolStrip4.Items.IndexOfKey("toolStripButtonTail");
            if (index != -1)
            {
                toolStrip4.Items.RemoveAt(index);
                toolStrip4.Items.Insert(index, host);
            }

            dateTimeDragControl.Visible = false;
            loadProgessBar.Visible = false;

            // get a reference to the current assembly
            Assembly a = Assembly.GetExecutingAssembly();

            // get a list of resource names from the manifest
            string[] resNames = a.GetManifestResourceNames();

            Bitmap bmp = new Bitmap(GetType(), "Resources.delete-page-red.gif");
            deadIcon = Icon.FromHandle(bmp.GetHicon());
            bmp.Dispose();
            Closing += LogTabWindow_Closing;

            InitToolWindows();
        }

        #endregion

        #region Properties / Indexers

        public LogWindow CurrentLogWindow
        {
            get => currentLogWindow;
            set => ChangeCurrentLogWindow(value);
        }

        public List<HilightGroup> HilightGroupList { get; private set; } = new List<HilightGroup>();

        // public Settings Settings
        // {
        // get { return ConfigManager.Settings; }
        // }
        public ILogExpertProxy LogExpertProxy { get; set; }

        public Preferences Preferences => ConfigManager.Settings.preferences;

        public SearchParams SearchParams { get; private set; } = new SearchParams();

        internal static StaticLogTabWindowData StaticData { get; set; } = new StaticLogTabWindowData();

        #endregion

        #region Nested type: LogWindowData

        private class LogWindowData
        {
            #region Private Fields

            // public MdiTabControl.TabPage tabPage;
            public Color color = Color.FromKnownColor(KnownColor.Gray);

            public int diffSum;
            public bool dirty;
            public int syncMode; // 0 = off, 1 = timeSynced
            public int tailState; // tailState: 0,1,2 = on/off/off by Trigger
            public ToolTip toolTip;

            #endregion
        }

        #endregion

        #region Nested type: StaticLogTabWindowData

        // Data shared over all LogTabWindow instances
        internal class StaticLogTabWindowData
        {
            #region Properties / Indexers

            public LogTabWindow CurrentLockedMainWindow { get; set; }

            #endregion
        }

        #endregion

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
    }
}
