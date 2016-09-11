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
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert
{
	public partial class LogTabWindow : Form
	{
		#region Fields

		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private const int MAX_HISTORY = 30;
		private const int MAX_COLUMNIZER_HISTORY = 40;
		private const int MAX_COLOR_HISTORY = 40;
		private const int DIFF_MAX = 100;
		private const int MAX_FILE_HISTORY = 10;

		private static StaticLogTabWindowData staticData = new StaticLogTabWindowData();

		private List<HilightGroup> _hilightGroupList = new List<HilightGroup>();

		private bool _skipEvents = false;
		private bool _shouldStop = false;
		private bool _wasMaximized = false;

		private string[] _startupFileNames;

		private LogWindow _currentLogWindow = null;
		private IList<LogWindow> _logWindowList = new List<LogWindow>();

		private Rectangle[] _leds = new Rectangle[5];
		private Brush[] _ledBrushes = new Brush[5];
		private Icon[,,,] _ledIcons = new Icon[6, 2, 4, 2];
		private Icon _deadIcon;
		private StringFormat _tabStringFormat = new StringFormat();
		private Brush _offLedBrush;
		private Brush _dirtyLedBrush;
		private Brush[] _tailLedBrush = new Brush[3];
		private Brush _syncLedBrush;
		private Thread _ledThread;

		//Settings settings;
		private SearchParams _searchParams = new SearchParams();

		private Color _defaultTabColor = Color.FromArgb(255, 192, 192, 192);

		private int _instanceNumber = 0;
		private bool _showInstanceNumbers = false;

		private StatusLineEventArgs _lastStatusLineEvent = null;
		private Object _statusLineLock = new Object();
		private readonly EventWaitHandle _statusLineEventHandle = new AutoResetEvent(false);
		private readonly EventWaitHandle _statusLineEventWakeupHandle = new ManualResetEvent(false);
		private Thread _statusLineThread;

		private BookmarkWindow _bookmarkWindow;
		private bool _firstBookmarkWindowShow = true;

		#endregion Fields

		#region Delegates

		private delegate void SetColumnizerFx(ILogLineColumnizer columnizer);

		private delegate void StatusLineEventFx(StatusLineEventArgs e);

		private delegate void ProgressBarEventFx(ProgressEventArgs e);

		private delegate void GuiStateUpdateWorkerDelegate(GuiStateArgs e);

		private delegate void SetTabIconDelegate(LogWindow logWindow, Icon icon);

		public delegate void HighlightSettingsChangedEventHandler(object sender, EventArgs e);

		public event HighlightSettingsChangedEventHandler HighlightSettingsChanged;

		private delegate void ExceptionFx();

		private delegate void FileRespawnedDelegate(LogWindow logWin);

		private delegate void LoadFileDelegate(string fileName, EncodingOptions encodingOptions);

		private delegate void LoadMultiFilesDelegate(string[] fileName, EncodingOptions encodingOptions);

		private delegate void AddFileTabsDelegate(string[] fileNames);

		#endregion Delegates

		#region cTor

		public LogTabWindow(string[] fileNames, int instanceNumber, bool showInstanceNumbers)
		{
			InitializeComponent();
			_startupFileNames = fileNames;
			_instanceNumber = instanceNumber;
			_showInstanceNumbers = showInstanceNumbers;

			Load += LogTabWindow_Load;

			ConfigManager.Instance.ConfigChanged += ConfigChanged;
			_hilightGroupList = ConfigManager.Settings.hilightGroupList;

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

			using (Bitmap bmp = new Bitmap(GetType(), "Resources.delete-page-red.gif"))
			{
				_deadIcon = System.Drawing.Icon.FromHandle(bmp.GetHicon());
			}
			Closing += LogTabWindow_Closing;

			InitBookmarkWindow();
		}

		#endregion cTor

		#region Properties

		public LogWindow CurrentLogWindow
		{
			get
			{
				return _currentLogWindow;
			}
			set
			{
				ChangeCurrentLogWindow(value);
			}
		}

		public SearchParams SearchParams
		{
			get
			{
				return _searchParams;
			}
		}

		public Preferences Preferences
		{
			get
			{
				return ConfigManager.Settings.preferences;
			}
		}

		public List<HilightGroup> HilightGroupList
		{
			get
			{
				return _hilightGroupList;
			}
		}

		public ILogExpertProxy LogExpertProxy { get; set; }

		internal static StaticLogTabWindowData StaticData
		{
			get
			{
				return staticData;
			}
			set
			{
				staticData = value;
			}
		}

		#endregion Properties

		#region Public Methods

		public void OpenSearchDialog()
		{
			if (CurrentLogWindow == null)
			{
				return;
			}
			SearchDialog dlg = new SearchDialog();
			AddOwnedForm(dlg);
			dlg.TopMost = TopMost;
			_searchParams.historyList = ConfigManager.Settings.searchHistoryList;
			dlg.SearchParams = _searchParams;
			DialogResult res = dlg.ShowDialog();
			if (res == DialogResult.OK)
			{
				_searchParams = dlg.SearchParams;
				_searchParams.isFindNext = false;
				CurrentLogWindow.StartSearch();
			}
		}

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
									  (pipe.FilterParams.columnRestrict ? "\nColumn restrict" : ""));
				tip.AutomaticDelay = 10;
				tip.AutoPopDelay = 5000;
				LogWindowData data = logWin.Tag as LogWindowData;
				data.toolTip = tip;
			}
			return logWin;
		}

		public LogWindow AddFileTabDeferred(string givenFileName, bool isTempFile, string title, bool forcePersistenceLoading, ILogLineColumnizer preProcessColumnizer)
		{
			return AddFileTab(givenFileName, isTempFile, title, forcePersistenceLoading, preProcessColumnizer, true);
		}

		public LogWindow AddFileTab(string givenFileName, bool isTempFile, string title, bool forcePersistenceLoading, ILogLineColumnizer preProcessColumnizer)
		{
			return AddFileTab(givenFileName, isTempFile, title, forcePersistenceLoading, preProcessColumnizer, false);
		}

		public LogWindow AddFileTab(string givenFileName, bool isTempFile, string title, bool forcePersistenceLoading, ILogLineColumnizer preProcessColumnizer, bool doNotAddToDockPanel)
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
			LogWindow logWindow = new LogWindow(this, logFileName, isTempFile, forcePersistenceLoading);

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
			data.color = _defaultTabColor;

			if (!isTempFile)
			{
				foreach (ColorEntry colorEntry in ConfigManager.Settings.fileColors)
				{
					if (colorEntry.fileName.ToLower().Equals(logFileName.ToLower()))
					{
						data.color = colorEntry.color;
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

			LoadFileDelegate loadFileFx = new LoadFileDelegate(logWindow.LoadFile);
			loadFileFx.BeginInvoke(logFileName, encodingOptions, null, null);
			return logWindow;
		}

		public LogWindow AddMultiFileTab(string[] fileNames)
		{
			if (fileNames.Length < 1)
				return null;
			LogWindow logWindow = new LogWindow(this, fileNames[fileNames.Length - 1], false, false);
			AddLogWindow(logWindow, fileNames[fileNames.Length - 1], false);
			multiFileToolStripMenuItem.Checked = true;
			multiFileEnabledStripMenuItem.Checked = true;
			EncodingOptions encodingOptions = new EncodingOptions();
			FillDefaultEncodingFromSettings(encodingOptions);
			BeginInvoke(new LoadMultiFilesDelegate(logWindow.LoadFilesAsMulti), new object[] { fileNames, encodingOptions });
			AddToFileHistory(fileNames[0]);
			return logWindow;
		}

		public IList<WindowFileEntry> GetListOfOpenFiles()
		{
			IList<WindowFileEntry> list = new List<WindowFileEntry>();
			lock (_logWindowList)
			{
				foreach (LogWindow logWindow in _logWindowList)
				{
					list.Add(new WindowFileEntry(logWindow));
				}
			}
			return list;
		}

		public void NotifySettingsChanged(Object cookie, SettingsFlags flags)
		{
			if (cookie != this)
			{
				NotifyWindowsForChangedPrefs(flags);
			}
		}

		public void LoadFiles(string[] fileNames)
		{
			Invoke(new AddFileTabsDelegate(AddFileTabs), new object[] { fileNames });
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
			lock (_logWindowList)
			{
				foreach (LogWindow logWindow in _logWindowList)
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
							ILogLineColumnizer columnizer = Util.FindColumnizerByName(entry.columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
							return columnizer;
						}
					}
					catch (ArgumentException ex)
					{
						_logger.Error(ex, "RegEx-error while finding columnizer: ");
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
					catch (ArgumentException ex)
					{
						_logger.Error(ex);
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
			Win32.SetForegroundWindow(Handle);
			if (WindowState == FormWindowState.Minimized)
			{
				if (_wasMaximized)
				{
					WindowState = FormWindowState.Maximized;
				}
				else
				{
					WindowState = FormWindowState.Normal;
				}
			}
		}

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
			if (Preferences.showTailState)
			{
				Icon icon = GetIcon(data.diffSum, data);
				BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { logWindow, icon });
			}
		}

		/// <summary>
		/// Creates a temp file with the text content of the clipboard and opens the temp file in a new tab.
		/// </summary>
		private void PasteFromClipboard()
		{
			if (Clipboard.ContainsText())
			{
				string text = Clipboard.GetText();
				string fileName = Path.GetTempFileName();
				FileStream fStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				StreamWriter writer = new StreamWriter(fStream, Encoding.Unicode);
				writer.Write(text);
				writer.Close();
				string title = "Clipboard";
				LogWindow logWindow = AddTempFileTab(fileName, title);
				LogWindowData data = logWindow.Tag as LogWindowData;
				if (data != null)
				{
					SetTooltipText(logWindow, "Pasted on " + DateTime.Now.ToString());
				}
			}
		}

		#endregion Public Methods

		#region Private Methods

		private string SaveLayout()
		{
			using (MemoryStream memStream = new MemoryStream(2000))
			using (StreamReader r = new StreamReader(memStream))
			{
				dockPanel.SaveAsXml(memStream, Encoding.UTF8, true);

				memStream.Position = 0;

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

				memStream.Position = 0;

				dockPanel.LoadFromXml(memStream, DeserializeDockContent, true);
			}
		}

		private IDockContent DeserializeDockContent(string persistString)
		{
			if (persistString.Equals(WindowTypes.BookmarkWindow.ToString()))
			{
				return _bookmarkWindow;
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
					_logger.Warn("Layout data contains non-existing LogWindow for " + fileName);
				}
			}
			return null;
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
						openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					}
					catch (SecurityException e)
					{
						_logger.Warn("e,Insufficient rights for GetFolderPath(): ");
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

		private void DestroyToolWindows()
		{
			DestroyBookmarkWindow();
		}

		private void InitBookmarkWindow()
		{
			_bookmarkWindow = new BookmarkWindow();
			_bookmarkWindow.HideOnClose = true;
			_bookmarkWindow.ShowHint = DockState.DockBottom;
			_bookmarkWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, SettingsFlags.All);
			_bookmarkWindow.VisibleChanged += new EventHandler(bookmarkWindow_VisibleChanged);
			_firstBookmarkWindowShow = true;
		}

		private void DestroyBookmarkWindow()
		{
			_bookmarkWindow.HideOnClose = false;
			_bookmarkWindow.Close();
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
					encodingOptions.DefaultEncoding = Encoding.GetEncoding(ConfigManager.Settings.preferences.defaultEncoding);
				}
				catch (ArgumentException ex)
				{
					_logger.Warn(ex, "Encoding " + ConfigManager.Settings.preferences.defaultEncoding + " is not a valid encoding");
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
			lock (_logWindowList)
			{
				_logWindowList.Add(logWindow);
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
		}

		private void AddToFileHistory(string fileName)
		{
			Predicate<string> findName = delegate (string s)
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
			lock (_logWindowList)
			{
				foreach (LogWindow logWindow in _logWindowList)
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
			foreach (var file in ConfigManager.Settings.fileHistoryList)
			{
				ToolStripItem item = new ToolStripMenuItem(file);
				strip.Items.Add(item);
			}
			strip.ItemClicked += new ToolStripItemClickedEventHandler(history_ItemClicked);
			strip.MouseUp += new MouseEventHandler(strip_MouseUp);
			lastUsedToolStripMenuItem.DropDown = strip;
		}

		private void RemoveLogWindow(LogWindow logWindow)
		{
			lock (_logWindowList)
			{
				_logWindowList.Remove(logWindow);
			}
			DisconnectEventHandlers(logWindow);
		}

		private void RemoveAndDisposeLogWindow(LogWindow logWindow, bool dontAsk)
		{
			if (CurrentLogWindow == logWindow)
			{
				ChangeCurrentLogWindow(null);
			}
			lock (_logWindowList)
			{
				_logWindowList.Remove(logWindow);
			}
			logWindow.Close(dontAsk);
		}

		private void ShowHighlightSettingsDialog()
		{
			HilightDialog dlg = new HilightDialog();
			dlg.KeywordActionList = PluginRegistry.GetInstance().RegisteredKeywordActions;
			dlg.Owner = this;
			dlg.TopMost = TopMost;
			dlg.HilightGroupList = _hilightGroupList;
			dlg.PreSelectedGroupName = highlightGroupsComboBox.Text;
			DialogResult res = dlg.ShowDialog();
			if (res == DialogResult.OK)
			{
				_hilightGroupList = dlg.HilightGroupList;
				FillHighlightComboBox();
				ConfigManager.Settings.hilightGroupList = _hilightGroupList;
				ConfigManager.Save(SettingsFlags.HighlightSettings);
				OnHighlightSettingsChanged();
			}
		}

		private void FillHighlightComboBox()
		{
			string currentGroupName = highlightGroupsComboBox.Text;
			highlightGroupsComboBox.Items.Clear();
			foreach (HilightGroup group in _hilightGroupList)
			{
				highlightGroupsComboBox.Items.Add(group.GroupName);
				if (group.GroupName.Equals(currentGroupName))
				{
					highlightGroupsComboBox.Text = group.GroupName;
				}
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
			if (newLogWindow == _currentLogWindow)
			{
				return; // do nothing if wishing to set the same window
			}

			LogWindow oldLogWindow = _currentLogWindow;
			_currentLogWindow = newLogWindow;
			string titleName = _showInstanceNumbers ? "LogExpert #" + _instanceNumber : "LogExpert";

			if (oldLogWindow != null)
			{
				oldLogWindow.StatusLineEvent -= StatusLineEvent;
				oldLogWindow.ProgressBarUpdate -= ProgressBarUpdate;
				oldLogWindow.GuiStateUpdate -= GuiStateUpdate;
				oldLogWindow.ColumnizerChanged -= ColumnizerChanged;
				oldLogWindow.BookmarkProvider.BookmarkAdded -= BookmarkAdded;
				oldLogWindow.BookmarkProvider.BookmarkRemoved -= BookmarkRemoved;
				oldLogWindow.BookmarkProvider.BookmarkTextChanged -= BookmarkTextChanged;
				DisconnectToolWindows();
			}

			if (newLogWindow != null)
			{
				newLogWindow.StatusLineEvent += StatusLineEvent;
				newLogWindow.ProgressBarUpdate += ProgressBarUpdate;
				newLogWindow.GuiStateUpdate += GuiStateUpdate;
				newLogWindow.ColumnizerChanged += ColumnizerChanged;
				newLogWindow.BookmarkProvider.BookmarkAdded += BookmarkAdded;
				newLogWindow.BookmarkProvider.BookmarkRemoved += BookmarkRemoved;
				newLogWindow.BookmarkProvider.BookmarkTextChanged += BookmarkTextChanged;
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
			_bookmarkWindow.SetBookmarkData(logWindow.BookmarkData);
			_bookmarkWindow.SetCurrentFile(ctx);
		}

		private void DisconnectToolWindows()
		{
			DisconnectBookmarkWindow();
		}

		private void DisconnectBookmarkWindow()
		{
			_bookmarkWindow.SetBookmarkData(null);
			_bookmarkWindow.SetCurrentFile(null);
		}

		private void GuiStateUpdateWorker(GuiStateArgs e)
		{
			_skipEvents = true;
			followTailCheckBox.Checked = e.FollowTail;
			menuStrip1.Enabled = e.MenuEnabled;
			timeshiftToolStripMenuItem.Enabled = e.TimeshiftPossible;
			timeshiftToolStripMenuItem.Checked = e.TimeshiftEnabled;
			timeshiftMenuTextBox.Text = e.TimeshiftText;
			timeshiftMenuTextBox.Enabled = e.TimeshiftEnabled;
			multiFileToolStripMenuItem.Enabled = e.MultiFileEnabled;  // disabled for temp files
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

			_skipEvents = false;
		}

		// tailState: 0,1,2 = on/off/off by Trigger
		// syncMode: 0 = normal (no), 1 = time synced
		private Icon CreateLedIcon(int level, bool dirty, int tailState, int syncMode)
		{
			Rectangle iconRect = _leds[0];
			iconRect.Height = 16; // (DockPanel's damn hardcoded height) // _leds[_leds.Length - 1].Bottom;
			iconRect.Width = iconRect.Right + 6;

			using (Bitmap bmp = new Bitmap(iconRect.Width, iconRect.Height))
			using (Graphics gfx = Graphics.FromImage(bmp))
			{
				int offsetFromTop = 4;

				for (int i = 0; i < _leds.Length; ++i)
				{
					Rectangle ledRect = _leds[i];
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

				int ledSize = 3;
				int ledGap = 1;
				Rectangle lastLed = _leds[_leds.Length - 1];
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

				// see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=345656
				// GetHicon() creates an unmanaged handle which must be destroyed. The Clone() workaround creates
				// a managed copy of icon. then the unmanaged win32 handle is destroyed
				IntPtr iconHandle = bmp.GetHicon();
				Icon icon = System.Drawing.Icon.FromHandle(iconHandle).Clone() as Icon;
				Win32.DestroyIcon(iconHandle);

				return icon;
			}
		}

		private void StatusLineThreadFunc()
		{
			int timeSum = 0;
			int waitTime = 30;
			while (!_shouldStop)
			{
				_statusLineEventWakeupHandle.WaitOne();
				_statusLineEventWakeupHandle.Reset();
				if (!_shouldStop)
				{
					bool signaled = false;
					do
					{
						//_statusLineEventHandle.Reset();
						signaled = _statusLineEventHandle.WaitOne(waitTime, true);
						timeSum += waitTime;
					}
					while (signaled && timeSum < 900 && !_shouldStop);

					if (!_shouldStop)
					{
						timeSum = 0;
						try
						{
							StatusLineEventArgs e;
							lock (_statusLineLock)
							{
								e = _lastStatusLineEvent.Clone();
							}
							BeginInvoke(new StatusLineEventFx(StatusLineEventWorker), new object[] { e });
						}
						catch (ObjectDisposedException)
						{
						}
					}
				}
			}
		}

		private void CreateIcons()
		{
			for (int syncMode = 0; syncMode <= 1; syncMode++)   // LED indicating time synced tabs
			{
				for (int tailMode = 0; tailMode < 4; tailMode++)
				{
					for (int i = 0; i < 6; ++i)
					{
						_ledIcons[i, 0, tailMode, syncMode] = CreateLedIcon(i, false, tailMode, syncMode);
					}
					for (int i = 0; i < 6; ++i)
					{
						_ledIcons[i, 1, tailMode, syncMode] = CreateLedIcon(i, true, tailMode, syncMode);
					}
				}
			}
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
				LogWindowData data = ((LogWindow)sender).Tag as LogWindowData;
				if (data != null)
				{
					lock (data)
					{
						data.diffSum = data.diffSum + diff;
						if (data.diffSum > DIFF_MAX)
							data.diffSum = DIFF_MAX;
					}

					//if (dockPanel.ActiveContent != null &&
					//    dockPanel.ActiveContent != sender || data.tailState != 0)
					if (CurrentLogWindow != null &&
						CurrentLogWindow != sender || data.tailState != 0)
					{
						data.dirty = true;
					}
					Icon icon = GetIcon(diff, data);
					BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { (LogWindow)sender, icon });
				}
			}
		}

		private delegate void FileNotFoundDelegate(LogWindow logWin);

		private void FileNotFound(LogWindow logWin)
		{
			LogWindowData data = logWin.Tag as LogWindowData;
			BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { logWin, _deadIcon });
			dateTimeDragControl.Visible = false;
		}

		private void FileRespawned(LogWindow logWin)
		{
			LogWindowData data = logWin.Tag as LogWindowData;
			Icon icon = GetIcon(0, data);
			BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { logWin, icon });
		}

		private void ShowLedPeak(LogWindow logWin)
		{
			LogWindowData data = logWin.Tag as LogWindowData;
			lock (data)
			{
				data.diffSum = DIFF_MAX;
			}
			Icon icon = GetIcon(data.diffSum, data);
			BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { logWin, icon });
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
			while (!_shouldStop)
			{
				try
				{
					Thread.Sleep(200);
				}
				catch (Exception ex)
				{
					_logger.Error(ex);
					return;
				}
				lock (_logWindowList)
				{
					foreach (LogWindow logWindow in _logWindowList)
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
							BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { logWindow, icon });
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
			Icon icon = _ledIcons[GetLevelFromDiff(diff), data.dirty ? 1 : 0, Preferences.showTailState ? data.tailState : 3, data.syncMode];
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
				return;
			if (encoding is System.Text.ASCIIEncoding)
			{
				aSCIIToolStripMenuItem.Checked = true;
			}
			else if (encoding.Equals(Encoding.Default))
			{
				aNSIToolStripMenuItem.Checked = true;
			}
			else if (encoding is System.Text.UTF8Encoding)
			{
				uTF8ToolStripMenuItem.Checked = true;
			}
			else if (encoding is System.Text.UnicodeEncoding)
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

			lock (_logWindowList)
			{
				foreach (LogWindow logWindow in _logWindowList)
				{
					logWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, flags);
				}
			}
			_bookmarkWindow.PreferencesChanged(ConfigManager.Settings.preferences, false, flags);

			_hilightGroupList = ConfigManager.Settings.hilightGroupList;
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
			_tailLedBrush[0] = new SolidBrush(preferences.showTailColor);
			CreateIcons();
			lock (_logWindowList)
			{
				foreach (LogWindow logWindow in _logWindowList)
				{
					LogWindowData data = logWindow.Tag as LogWindowData;
					Icon icon = GetIcon(data.diffSum, data);
					BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { logWindow, icon });
				}
			}
		}

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
				Win32.DestroyIcon(icon.Handle);
				icon.Dispose();
			}
			if (entry.cmd != null && entry.cmd.Length > 0)
			{
				item.ToolTipText = entry.name;
			}
		}

		private void StartTool(string cmd, string args, bool sysoutPipe, string columnizerName, string workingDir)
		{
			if (cmd == null || cmd.Length == 0)
				return;
			Process process = new Process();
			ProcessStartInfo startInfo = new ProcessStartInfo(cmd, args);
			if (!string.IsNullOrEmpty(workingDir))
			{
				startInfo.WorkingDirectory = workingDir;
			}
			process.StartInfo = startInfo;
			process.EnableRaisingEvents = true;

			if (sysoutPipe)
			{
				ILogLineColumnizer columnizer = Util.FindColumnizerByName(columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
				if (columnizer == null)
				{
					columnizer = PluginRegistry.GetInstance().RegisteredColumnizers[0];
				}

				_logger.Info("Starting external tool with sysout redirection: " + cmd + " " + args);
				startInfo.UseShellExecute = false;
				startInfo.RedirectStandardOutput = true;
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
				LogWindow logWin = AddTempFileTab(pipe.FileName, CurrentLogWindow.IsTempFile ? CurrentLogWindow.TempTitleName : Util.GetNameFromPath(CurrentLogWindow.FileName) + "->E");
				logWin.ForceColumnizer(columnizer);
				process.Exited += pipe.ProcessExitedEventHandler;
			}
			else
			{
				_logger.Info("Starting external tool: " + cmd + " " + args);
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
			lock (_logWindowList)
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

		internal HilightGroup FindHighlightGroup(string groupName)
		{
			lock (_hilightGroupList)
			{
				foreach (HilightGroup group in _hilightGroupList)
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
			string groupName = highlightGroupsComboBox.Text;
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.SetCurrentHighlightGroup(groupName);
			}
		}

		private void FillToolLauncherBar()
		{
			char[] labels = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
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

			externalToolsToolStrip.Visible = (num > 0); // do not show bar if no tool uses it
		}

		protected void OnHighlightSettingsChanged()
		{
			if (HighlightSettingsChanged != null)
			{
				HighlightSettingsChanged(this, new EventArgs());
			}
		}

		private void RunGC()
		{
			_logger.Info("Running GC. Used mem before: " + GC.GetTotalMemory(false).ToString("N0"));
			GC.Collect();
			_logger.Info("GC done.    Used mem after:  " + GC.GetTotalMemory(true).ToString("N0"));
		}

		private void DumpGCInfo()
		{
			_logger.Info("-------- GC info -----------");
			_logger.Info("Used mem: " + GC.GetTotalMemory(false).ToString("N0"));
			for (int i = 0; i < GC.MaxGeneration; ++i)
			{
				_logger.Info("Generation " + i + " collect count: " + GC.CollectionCount(i));
			}
			_logger.Info("----------------------------");
		}

		private void ThrowExceptionFx()
		{
			throw new Exception("This is a test exception thrown by an async delegate");
		}

		private void LoadProject(string projectFileName, bool restoreLayout)
		{
			ProjectData projectData = ProjectPersister.LoadProjectData(projectFileName);
			bool hasLayoutData = projectData.tabLayoutXml != null;

			if (hasLayoutData && restoreLayout && _logWindowList.Count > 0)
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
							LogExpertProxy.NewWindow(new string[] { projectFileName });
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
					InitBookmarkWindow();
					RestoreLayout(projectData.tabLayoutXml);
				}
			}
		}

		#endregion Private Methods

		#region Events

		private void GuiStateUpdate(object sender, GuiStateArgs e)
		{
			BeginInvoke(new GuiStateUpdateWorkerDelegate(GuiStateUpdateWorker), new object[] { e });
		}

		private void LogTabWindow_Load(object sender, EventArgs e)
		{
			ApplySettings(ConfigManager.Settings, SettingsFlags.All);
			if (ConfigManager.Settings.isMaximized)
			{
				Rectangle tmpBounds = Bounds;
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

			if (ConfigManager.Settings.preferences.openLastFiles && _startupFileNames == null)
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
			if (_startupFileNames != null)
			{
				LoadFiles(_startupFileNames, false);
			}
			_ledThread = new Thread(new ThreadStart(LedThreadProc));
			_ledThread.IsBackground = true;
			_ledThread.Start();

			_statusLineThread = new Thread(new ThreadStart(StatusLineThreadFunc));
			_statusLineThread.IsBackground = true;
			_statusLineThread.Start();

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
				_shouldStop = true;
				_statusLineEventHandle.Set();
				_statusLineEventWakeupHandle.Set();
				_ledThread.Join();
				_statusLineThread.Join();

				IList<LogWindow> deleteLogWindowList = new List<LogWindow>();
				ConfigManager.Settings.alwaysOnTop = TopMost && ConfigManager.Settings.preferences.allowOnlyOneInstance;
				SaveLastOpenFilesList();
				foreach (LogWindow logWindow in _logWindowList)
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
			catch (Exception ex)
			{
				_logger.Error(ex);
				// ignore error (can occur then multipe instances are closed simultaneously or if the
				// window was not constructed completely because of errors)
			}
			finally
			{
				if (LogExpertProxy != null)
				{
					LogExpertProxy.WindowClosed(this);
				}
			}
		}

		private void bookmarkWindow_VisibleChanged(object sender, EventArgs e)
		{
			_firstBookmarkWindowShow = false;
		}

		private void strip_MouseUp(object sender, MouseEventArgs e)
		{
			if (sender is ToolStripDropDown)
			{
				AddFileTab(((ToolStripDropDown)sender).Text, false, null, false, null);
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
				return;
			CurrentLogWindow.ColumnizerCallbackObject.LineNum = CurrentLogWindow.GetCurrentLineNum();
			FilterSelectorForm form = new FilterSelectorForm(PluginRegistry.GetInstance().RegisteredColumnizers, CurrentLogWindow.CurrentColumnizer, CurrentLogWindow.ColumnizerCallbackObject);
			form.Owner = this;
			form.TopMost = TopMost;
			DialogResult res = form.ShowDialog();
			if (res == DialogResult.OK)
			{
				if (form.ApplyToAll)
				{
					lock (_logWindowList)
					{
						foreach (LogWindow logWindow in _logWindowList)
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
					if (!CurrentLogWindow.CurrentColumnizer.GetType().Equals(form.SelectedColumnizer.GetType()))
					{
						SetColumnizerFx fx = new SetColumnizerFx(CurrentLogWindow.ForceColumnizer);
						CurrentLogWindow.Invoke(fx, new object[] { form.SelectedColumnizer });
						setColumnizerHistoryEntry(CurrentLogWindow.FileName, form.SelectedColumnizer);
					}
					if (form.IsConfigPressed)
					{
						lock (_logWindowList)
						{
							foreach (LogWindow logWindow in _logWindowList)
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
			if (CurrentLogWindow == null)
				return;
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
			var list = e.Data.GetData(e.Data.GetFormats()[0]) as IEnumerable<ListViewItem>;
			var data = e.Data.GetData("Shell IDList Array");
			StreamReader r = new StreamReader(data as Stream);
			string line = r.ReadToEnd();

			string[] formats = e.Data.GetFormats();
			bool yeah = e.Data.GetDataPresent(formats[0], true);
			object obj = e.Data.GetData(formats[0], true);
			object obj1 = e.Data.GetData(formats[1]);
			object obj2 = e.Data.GetData(formats[2]);

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
			object test = e.Data.GetData(DataFormats.StringFormat);

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

		private void timeshiftToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
		{
			if (!_skipEvents && CurrentLogWindow != null)
			{
				CurrentLogWindow.SetTimeshiftValue(timeshiftMenuTextBox.Text);
				timeshiftMenuTextBox.Enabled = timeshiftToolStripMenuItem.Checked;
				CurrentLogWindow.TimeshiftEnabled(timeshiftToolStripMenuItem.Checked, timeshiftMenuTextBox.Text);
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

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox aboutBox = new AboutBox();
			aboutBox.TopMost = TopMost;
			aboutBox.ShowDialog();
		}

		private void filterToggleButton_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ToggleFilterPanel();
		}

		private void filterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ToggleFilterPanel();
		}

		private void multiFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToggleMultiFile();
			toolStripMenuItem1.HideDropDown();
		}

		private void ColumnizerChanged(object sender, ColumnizerEventArgs e)
		{
			if (_bookmarkWindow != null)
			{
				_bookmarkWindow.SetColumnizer(e.Columnizer);
			}
		}

		private void BookmarkAdded()
		{
			_bookmarkWindow.UpdateView();
		}

		private void BookmarkTextChanged(BookmarkEventArgs e)
		{
			_bookmarkWindow.BookmarkTextChanged(e.Bookmark);
		}

		private void BookmarkRemoved()
		{
			_bookmarkWindow.UpdateView();
		}

		private void ProgressBarUpdate(ProgressEventArgs e)
		{
			Invoke(new ProgressBarEventFx(ProgressBarUpdateWorker), new object[] { e });
		}

		private void ProgressBarUpdateWorker(ProgressEventArgs e)
		{
			if (e.Value <= e.MaxValue && e.Value >= e.MinValue)
			{
				loadProgessBar.Minimum = e.MinValue;
				loadProgessBar.Maximum = e.MaxValue;
				loadProgessBar.Value = e.Value;
				loadProgessBar.Visible = e.Visible;
				Invoke(new MethodInvoker(statusStrip1.Refresh));
			}
		}

		private void StatusLineEvent(object sender, StatusLineEventArgs e)
		{
			lock (_statusLineLock)
			{
				_lastStatusLineEvent = e;
				_statusLineEventHandle.Set();
				_statusLineEventWakeupHandle.Set();
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

		private void followTailCheckBox_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.FollowTailChanged(followTailCheckBox.Checked, false);
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
				if (CurrentLogWindow != null)
				{
					CurrentLogWindow.Close();
				}
			}
			else if (e.KeyCode == Keys.Tab && e.Control)
			{
				SwitchTab(e.Shift);
			}
			else
			{
				if (CurrentLogWindow != null)
					CurrentLogWindow.LogWindow_KeyDown(sender, e);
			}
		}

		private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.Close();
		}

		private void cellSelectModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.SetCellSelectionMode(cellSelectModeToolStripMenuItem.Checked);
			}
		}

		private void copyMarkedLinesIntoNewTabToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.CopyMarkedLinesToTab();
			}
		}

		private void timeshiftMenuTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (CurrentLogWindow == null)
				return;
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

		private void logWindow_FileNotFound(object sender)
		{
			Invoke(new FileNotFoundDelegate(FileNotFound), sender);
		}

		private void logWindow_FileRespawned(object sender)
		{
			Invoke(new FileRespawnedDelegate(FileRespawned), sender);
		}

		private void logWindow_FilterListChanged(object sender, FilterListChangedEventArgs e)
		{
			lock (_logWindowList)
			{
				foreach (LogWindow logWindow in _logWindowList)
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
				return;
			if (sender.GetType().IsAssignableFrom(typeof(LogWindow)))
			{
				if (dockPanel.ActiveContent == sender)
				{
					LogWindowData data = ((LogWindow)sender).Tag as LogWindowData;
					data.dirty = false;
					Icon icon = GetIcon(data.diffSum, data);
					BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { (LogWindow)sender, icon });
				}
			}
		}

		private void logWindow_SyncModeChanged(object sender, SyncModeEventArgs e)
		{
			if (!Disposing)
			{
				LogWindowData data = ((LogWindow)sender).Tag as LogWindowData;
				data.syncMode = e.IsTimeSynced ? 1 : 0;
				Icon icon = GetIcon(data.diffSum, data);
				BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { (LogWindow)sender, icon });
			}
			else
			{
				_logger.Warn("Received SyncModeChanged event while disposing. Event ignored.");
			}
		}

		private void toggleBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ToggleBookmark();
		}

		private void jumpToNextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.JumpToNextBookmark(true);
		}

		private void jumpToPrevToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.JumpToNextBookmark(false);
		}

		private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ChangeEncoding(Encoding.ASCII);
		}

		private void aNSIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ChangeEncoding(Encoding.Default);
		}

		private void uTF8ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ChangeEncoding(new UTF8Encoding(false));
		}

		private void uTF16ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ChangeEncoding(Encoding.Unicode);
		}

		private void iSO88591ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ChangeEncoding(Encoding.GetEncoding("iso-8859-1"));
		}

		private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				LogWindowData data = CurrentLogWindow.Tag as LogWindowData;
				Icon icon = GetIcon(0, data);
				BeginInvoke(new SetTabIconDelegate(SetTabIcon), new object[] { CurrentLogWindow, icon });
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
				//CurrentLogWindow.ScrollToTimestamp(dateTimeDragControl.DateTime);
			}
		}

		private void dateTimeDragControl_ValueChanged(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.ScrollToTimestamp(dateTimeDragControl.DateTime, true, true);
			}
		}

		private void LogTabWindow_Deactivate(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.AppFocusLost();
			}
		}

		private void LogTabWindow_Activated(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.AppFocusGained();
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
			if (CurrentLogWindow != null)
			{
				string line = CurrentLogWindow.GetCurrentLine();
				ILogFileInfo info = CurrentLogWindow.GetCurrentFileInfo();
				if (line != null && info != null)
				{
					ArgParser parser = new ArgParser(toolEntry.args);
					string argLine = parser.BuildArgs(line, CurrentLogWindow.GetRealLineNum() + 1, info, this);
					if (argLine != null)
					{
						StartTool(toolEntry.cmd, argLine, toolEntry.sysout, toolEntry.columnizerName, toolEntry.workingDir);
					}
				}
			}
		}

		private void showBookmarkListToolStripMenuItem_Click(object sender, EventArgs e)
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
			if (CurrentLogWindow != null)
				CurrentLogWindow.ToggleFilterPanel();
		}

		private void toolStripButtonBookmark_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.ToggleBookmark();
		}

		private void toolStripButtonUp_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.JumpToNextBookmark(false);
		}

		private void toolStripButtonDown_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
				CurrentLogWindow.JumpToNextBookmark(true);
		}

		private void showHelpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Help.ShowHelp(this, "LogExpert.chm");
		}

		private void hideLineColumnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ConfigManager.Settings.hideLineColumn = hideLineColumnToolStripMenuItem.Checked;
			lock (_logWindowList)
			{
				foreach (LogWindow logWin in _logWindowList)
				{
					logWin.ShowLineColumn(!ConfigManager.Settings.hideLineColumn);
				}
			}
			_bookmarkWindow.LineColumnVisible = ConfigManager.Settings.hideLineColumn;
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
			lock (_logWindowList)
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
				return;

			ColorDialog dlg = new ColorDialog();
			dlg.Color = data.color;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				data.color = dlg.Color;
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
				_wasMaximized = WindowState == FormWindowState.Maximized;
			}
		}

		private void patternStatisticToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.PatternStatistic();
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

				lock (_logWindowList)
				{
					foreach (DockContent content in dockPanel.Contents)
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
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.ExportBookmarkList();
			}
		}

		private void importBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.ImportBookmarkList();
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
			if (LogExpertProxy != null)
			{
				NotifySettingsChanged(null, e.Flags);
			}
		}

		private void dumpLogBufferInfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
#if DEBUG
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.DumpBufferInfo();
			}
#endif
		}

		private void dumpBufferDiagnosticToolStripMenuItem_Click(object sender, EventArgs e)
		{
#if DEBUG
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.DumpBufferDiagnostic();
			}
#endif
		}

		private void runGCToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunGC();
		}

		private void gCInfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DumpGCInfo();
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
			ExceptionFx fx = new ExceptionFx(ThrowExceptionFx);
			fx.BeginInvoke(null, null);
		}

		private void throwExceptionbackgroundThreadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Thread thread = new Thread(new ThreadStart(() =>
			{
				throw new Exception("This is a test exception thrown by a background thread");
			}));
			thread.IsBackground = true;
			thread.Start();
		}

		private void disableWordHighlightModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DebugOptions.disableWordHighlight = disableWordHighlightModeToolStripMenuItem.Checked;
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.RefreshAllGrids();
			}
		}

		private void multifileMaskToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null)
			{
				CurrentLogWindow.ChangeMultifileMask();
			}
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
					LoadFiles(new string[] { dlg.Uri }, false);
				}
			}
		}

		private void columnFinderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentLogWindow != null && !_skipEvents)
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

		#endregion Events

		#region Nested Classes

		private class LowercaseStringComparer : IComparer<string>
		{
			public int Compare(string x, string y)
			{
				return x.ToLower().CompareTo(y.ToLower());
			}
		}

		;

		private class LogWindowData
		{
			public int diffSum;
			public bool dirty;
			public Color color = Color.FromKnownColor(KnownColor.Gray);
			public int tailState = 0; // tailState: 0,1,2 = on/off/off by Trigger
			public ToolTip toolTip;
			public int syncMode; // 0 = off, 1 = timeSynced
		}

		// Data shared over all LogTabWindow instances
		internal class StaticLogTabWindowData
		{
			public LogTabWindow CurrentLockedMainWindow { get; set; }
		}

		#endregion Nested Classes
	}
}