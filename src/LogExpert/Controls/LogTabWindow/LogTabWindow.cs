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
using NLog;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
    public partial class LogTabWindow : Form
    {
        #region Fields

        private const int MAX_COLUMNIZER_HISTORY = 40;
        private const int MAX_COLOR_HISTORY = 40;
        private const int DIFF_MAX = 100;
        private const int MAX_FILE_HISTORY = 10;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly Icon deadIcon;

        private readonly Color defaultTabColor = Color.FromArgb(255, 192, 192, 192);
        private readonly Brush dirtyLedBrush;

        private readonly int instanceNumber = 0;
        private readonly Brush[] ledBrushes = new Brush[5];
        private readonly Icon[,,,] ledIcons = new Icon[6, 2, 4, 2];

        private readonly Rectangle[] leds = new Rectangle[5];

        private readonly IList<LogWindow> logWindowList = new List<LogWindow>();
        private readonly Brush offLedBrush;
        private readonly bool showInstanceNumbers = false;

        private readonly string[] startupFileNames;

        private readonly EventWaitHandle statusLineEventHandle = new AutoResetEvent(false);
        private readonly EventWaitHandle statusLineEventWakeupHandle = new ManualResetEvent(false);
        private readonly object statusLineLock = new object();
        private readonly Brush syncLedBrush;
        private readonly StringFormat tabStringFormat = new StringFormat();
        private readonly Brush[] tailLedBrush = new Brush[3];

        private BookmarkWindow bookmarkWindow;

        private LogWindow currentLogWindow = null;
        private bool firstBookmarkWindowShow = true;

        private StatusLineEventArgs lastStatusLineEvent = null;

        private Thread ledThread;

        //Settings settings;

        private bool shouldStop = false;

        private bool skipEvents = false;

        private Thread statusLineThread;
        private bool wasMaximized = false;

        #endregion

        #region cTor

        public LogTabWindow(string[] fileNames, int instanceNumber, bool showInstanceNumbers)
        {
            InitializeComponent();
            this.startupFileNames = fileNames;
            this.instanceNumber = instanceNumber;
            this.showInstanceNumbers = showInstanceNumbers;

            this.Load += LogTabWindow_Load;

            ConfigManager.Instance.ConfigChanged += ConfigChanged;
            this.HilightGroupList = ConfigManager.Settings.hilightGroupList;

            Rectangle led = new Rectangle(0, 0, 8, 2);
            for (int i = 0; i < leds.Length; ++i)
            {
                this.leds[i] = led;
                led.Offset(0, led.Height + 0);
            }
            int grayAlpha = 50;
            this.ledBrushes[0] = new SolidBrush(Color.FromArgb(255, 220, 0, 0));
            this.ledBrushes[1] = new SolidBrush(Color.FromArgb(255, 220, 220, 0));
            this.ledBrushes[2] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
            this.ledBrushes[3] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
            this.ledBrushes[4] = new SolidBrush(Color.FromArgb(255, 0, 220, 0));
            this.offLedBrush = new SolidBrush(Color.FromArgb(grayAlpha, 160, 160, 160));
            this.dirtyLedBrush = new SolidBrush(Color.FromArgb(255, 220, 0, 00));
            this.tailLedBrush[0] = new SolidBrush(Color.FromArgb(255, 50, 100, 250)); // Follow tail: blue-ish
            this.tailLedBrush[1] = new SolidBrush(Color.FromArgb(grayAlpha, 160, 160, 160)); // Don't follow tail: gray
            this.tailLedBrush[2] =
                new SolidBrush(Color.FromArgb(255, 220, 220, 0)); // Stop follow tail (trigger): yellow-ish
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

            InitToolWindows();
        }

        #endregion

        #region Delegates

        private delegate void AddFileTabsDelegate(string[] fileNames);

        private delegate void ExceptionFx();

        private delegate void FileNotFoundDelegate(LogWindow logWin);

        private delegate void FileRespawnedDelegate(LogWindow logWin);

        private delegate void GuiStateUpdateWorkerDelegate(GuiStateArgs e);

        public delegate void HighlightSettingsChangedEventHandler(object sender, EventArgs e);

        private delegate void LoadFileDelegate(string fileName, EncodingOptions encodingOptions);

        private delegate void LoadMultiFilesDelegate(string[] fileName, EncodingOptions encodingOptions);

        private delegate void ProgressBarEventFx(ProgressEventArgs e);

        private delegate void SetColumnizerFx(ILogLineColumnizer columnizer);

        private delegate void SetTabIconDelegate(LogWindow logWindow, Icon icon);

        private delegate void StatusLineEventFx(StatusLineEventArgs e);

        #endregion

        #region Events

        public event HighlightSettingsChangedEventHandler HighlightSettingsChanged;

        #endregion

        #region Properties

        public LogWindow CurrentLogWindow
        {
            get { return this.currentLogWindow; }
            set { ChangeCurrentLogWindow(value); }
        }

        public SearchParams SearchParams { get; private set; } = new SearchParams();

        public Preferences Preferences
        {
            get { return ConfigManager.Settings.preferences; }
        }

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
            lock (this.HilightGroupList)
            {
                foreach (HilightGroup group in this.HilightGroupList)
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
            public int tailState = 0; // tailState: 0,1,2 = on/off/off by Trigger
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