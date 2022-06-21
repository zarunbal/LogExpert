using System.Windows.Forms;
using LogExpert.Dialogs;

namespace LogExpert.Controls.LogTabWindow
{
	partial class LogTabWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
            WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin1 = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient1 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin dockPaneStripSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient dockPaneStripGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient2 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient3 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient dockPaneStripToolWindowGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient4 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient5 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient3 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient6 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient7 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogTabWindow));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.labelLines = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelCurrentLine = new System.Windows.Forms.ToolStripStatusLabel();
            this.loadProgessBar = new System.Windows.Forms.ToolStripProgressBar();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openURIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFromClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.multiFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiFileEnabledStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multifileMaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.loadProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.lastUsedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewNavigateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.jumpToNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.jumpToPrevToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBookmarkListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnFinderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripEncodingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripEncodingASCIIItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripEncodingANSIItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripEncodingISO88591Item = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripEncodingUTF8Item = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripEncodingUTF16Item = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.timeshiftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeshiftMenuTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.copyMarkedLinesIntoNewTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnizerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hilightingToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.cellSelectModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideLineColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.lockInstanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpLogBufferInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpBufferDiagnosticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runGCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gCInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.throwExceptionGUIThreadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.throwExceptionbackgroundThToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.throwExceptionBackgroundThreadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.loglevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.warnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.disableWordHighlightModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.host = new System.Windows.Forms.CheckBox();
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.buttonToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFilter = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonBookmark = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonBubbles = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonTail = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.groupsComboBoxHighlightGroups = new System.Windows.Forms.ToolStripComboBox();
            this.externalToolsToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.checkBoxFollowTail = new System.Windows.Forms.CheckBox();
            this.tabContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeThisTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeOtherTabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllTabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.tabColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabRenameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.copyPathToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dragControlDateTime = new LogExpert.Dialogs.DateTimeDragControl();
            this.statusStrip.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.buttonToolStrip.SuspendLayout();
            this.tabContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.AutoSize = false;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelLines,
            this.labelSize,
            this.labelCurrentLine,
            this.loadProgessBar,
            this.labelStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 779);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip.Size = new System.Drawing.Size(1443, 35);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip1";
            // 
            // labelLines
            // 
            this.labelLines.AutoSize = false;
            this.labelLines.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.labelLines.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.labelLines.Name = "labelLines";
            this.labelLines.Size = new System.Drawing.Size(90, 28);
            this.labelLines.Text = "0";
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = false;
            this.labelSize.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.labelSize.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(90, 28);
            this.labelSize.Text = "0";
            // 
            // labelCurrentLine
            // 
            this.labelCurrentLine.AutoSize = false;
            this.labelCurrentLine.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.labelCurrentLine.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.labelCurrentLine.Name = "labelCurrentLine";
            this.labelCurrentLine.Size = new System.Drawing.Size(90, 28);
            this.labelCurrentLine.Text = "L:";
            // 
            // loadProgessBar
            // 
            this.loadProgessBar.Name = "loadProgessBar";
            this.loadProgessBar.Size = new System.Drawing.Size(75, 27);
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(60, 28);
            this.labelStatus.Text = "Ready";
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.AllowMerge = false;
            this.mainMenuStrip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.mainMenuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewNavigateToolStripMenuItem,
            this.optionToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.mainMenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 34);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(1443, 33);
            this.mainMenuStrip.TabIndex = 6;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.openURIToolStripMenuItem,
            this.closeFileToolStripMenuItem,
            this.reloadToolStripMenuItem,
            this.newFromClipboardToolStripMenuItem,
            this.toolStripSeparator8,
            this.multiFileToolStripMenuItem,
            this.toolStripSeparator7,
            this.loadProjectToolStripMenuItem,
            this.saveProjectToolStripMenuItem,
            this.exportBookmarksToolStripMenuItem,
            this.toolStripSeparator14,
            this.lastUsedToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.OnFileToolStripMenuItemDropDownOpening);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::LogExpert.Properties.Resources.folder_blue;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OnOpenToolStripMenuItemClick);
            // 
            // openURIToolStripMenuItem
            // 
            this.openURIToolStripMenuItem.Name = "openURIToolStripMenuItem";
            this.openURIToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.openURIToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.openURIToolStripMenuItem.Text = "Open URL...";
            this.openURIToolStripMenuItem.ToolTipText = "Opens a file by entering a URL which is supported by a file system plugin";
            this.openURIToolStripMenuItem.Click += new System.EventHandler(this.OnOpenURIToolStripMenuItemClick);
            // 
            // closeFileToolStripMenuItem
            // 
            this.closeFileToolStripMenuItem.Name = "closeFileToolStripMenuItem";
            this.closeFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
            this.closeFileToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.closeFileToolStripMenuItem.Text = "Close File";
            this.closeFileToolStripMenuItem.Click += new System.EventHandler(this.OnCloseFileToolStripMenuItemClick);
            // 
            // reloadToolStripMenuItem
            // 
            this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            this.reloadToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.reloadToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.reloadToolStripMenuItem.Text = "Reload";
            this.reloadToolStripMenuItem.Click += new System.EventHandler(this.OnReloadToolStripMenuItemClick);
            // 
            // newFromClipboardToolStripMenuItem
            // 
            this.newFromClipboardToolStripMenuItem.Name = "newFromClipboardToolStripMenuItem";
            this.newFromClipboardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newFromClipboardToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.newFromClipboardToolStripMenuItem.Text = "New tab from clipboard";
            this.newFromClipboardToolStripMenuItem.ToolTipText = "Creates a new tab with content from clipboard";
            this.newFromClipboardToolStripMenuItem.Click += new System.EventHandler(this.OnNewFromClipboardToolStripMenuItemClick);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(365, 6);
            // 
            // multiFileToolStripMenuItem
            // 
            this.multiFileToolStripMenuItem.CheckOnClick = true;
            this.multiFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.multiFileEnabledStripMenuItem,
            this.multifileMaskToolStripMenuItem});
            this.multiFileToolStripMenuItem.Name = "multiFileToolStripMenuItem";
            this.multiFileToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.multiFileToolStripMenuItem.Text = "MultiFile";
            this.multiFileToolStripMenuItem.ToolTipText = "Treat multiple files as one large file (e.g. data.log, data.log.1, data.log.2,..." +
    ")";
            this.multiFileToolStripMenuItem.Click += new System.EventHandler(this.OnMultiFileToolStripMenuItemClick);
            // 
            // multiFileEnabledStripMenuItem
            // 
            this.multiFileEnabledStripMenuItem.CheckOnClick = true;
            this.multiFileEnabledStripMenuItem.Name = "multiFileEnabledStripMenuItem";
            this.multiFileEnabledStripMenuItem.Size = new System.Drawing.Size(248, 34);
            this.multiFileEnabledStripMenuItem.Text = "Enable MultiFile";
            this.multiFileEnabledStripMenuItem.Click += new System.EventHandler(this.OnMultiFileEnabledStripMenuItemClick);
            // 
            // multifileMaskToolStripMenuItem
            // 
            this.multifileMaskToolStripMenuItem.Name = "multifileMaskToolStripMenuItem";
            this.multifileMaskToolStripMenuItem.Size = new System.Drawing.Size(248, 34);
            this.multifileMaskToolStripMenuItem.Text = "File name mask...";
            this.multifileMaskToolStripMenuItem.Click += new System.EventHandler(this.OnMultiFileMaskToolStripMenuItemClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(365, 6);
            // 
            // loadProjectToolStripMenuItem
            // 
            this.loadProjectToolStripMenuItem.Name = "loadProjectToolStripMenuItem";
            this.loadProjectToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.loadProjectToolStripMenuItem.Text = "Load session...";
            this.loadProjectToolStripMenuItem.ToolTipText = "Load a saved session (list of log files)";
            this.loadProjectToolStripMenuItem.Click += new System.EventHandler(this.OnLoadProjectToolStripMenuItemClick);
            // 
            // saveProjectToolStripMenuItem
            // 
            this.saveProjectToolStripMenuItem.Name = "saveProjectToolStripMenuItem";
            this.saveProjectToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.saveProjectToolStripMenuItem.Text = "Save session...";
            this.saveProjectToolStripMenuItem.ToolTipText = "Save a session (all open tabs)";
            this.saveProjectToolStripMenuItem.Click += new System.EventHandler(this.OnSaveProjectToolStripMenuItemClick);
            // 
            // exportBookmarksToolStripMenuItem
            // 
            this.exportBookmarksToolStripMenuItem.Name = "exportBookmarksToolStripMenuItem";
            this.exportBookmarksToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.exportBookmarksToolStripMenuItem.Text = "Export bookmarks...";
            this.exportBookmarksToolStripMenuItem.ToolTipText = "Write a list of bookmarks and their comments to a CSV file";
            this.exportBookmarksToolStripMenuItem.Click += new System.EventHandler(this.OnExportBookmarksToolStripMenuItemClick);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(365, 6);
            // 
            // lastUsedToolStripMenuItem
            // 
            this.lastUsedToolStripMenuItem.Name = "lastUsedToolStripMenuItem";
            this.lastUsedToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.lastUsedToolStripMenuItem.Text = "Last used";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(368, 34);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnExitToolStripMenuItemClick);
            // 
            // viewNavigateToolStripMenuItem
            // 
            this.viewNavigateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goToLineToolStripMenuItem,
            this.searchToolStripMenuItem,
            this.filterToolStripMenuItem,
            this.bookmarksToolStripMenuItem,
            this.columnFinderToolStripMenuItem,
            this.toolStripSeparator2,
            this.toolStripEncodingMenuItem,
            this.toolStripSeparator4,
            this.timeshiftToolStripMenuItem,
            this.timeshiftMenuTextBox,
            this.toolStripSeparator3,
            this.copyMarkedLinesIntoNewTabToolStripMenuItem});
            this.viewNavigateToolStripMenuItem.Name = "viewNavigateToolStripMenuItem";
            this.viewNavigateToolStripMenuItem.Size = new System.Drawing.Size(142, 29);
            this.viewNavigateToolStripMenuItem.Text = "View/Navigate";
            // 
            // goToLineToolStripMenuItem
            // 
            this.goToLineToolStripMenuItem.Name = "goToLineToolStripMenuItem";
            this.goToLineToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.goToLineToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.goToLineToolStripMenuItem.Text = "Go to line...";
            this.goToLineToolStripMenuItem.Click += new System.EventHandler(this.OnGoToLineToolStripMenuItemClick);
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.searchToolStripMenuItem.Text = "Search...";
            this.searchToolStripMenuItem.Click += new System.EventHandler(this.OnSearchToolStripMenuItemClick);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Image = global::LogExpert.Properties.Resources.search_blue;
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.filterToolStripMenuItem.Text = "Filter";
            this.filterToolStripMenuItem.Click += new System.EventHandler(this.OnFilterToolStripMenuItemClick);
            // 
            // bookmarksToolStripMenuItem
            // 
            this.bookmarksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleBookmarkToolStripMenuItem,
            this.jumpToNextToolStripMenuItem,
            this.jumpToPrevToolStripMenuItem,
            this.showBookmarkListToolStripMenuItem});
            this.bookmarksToolStripMenuItem.Name = "bookmarksToolStripMenuItem";
            this.bookmarksToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.bookmarksToolStripMenuItem.Text = "Bookmarks";
            // 
            // toggleBookmarkToolStripMenuItem
            // 
            this.toggleBookmarkToolStripMenuItem.Image = global::LogExpert.Properties.Resources.check_blue;
            this.toggleBookmarkToolStripMenuItem.Name = "toggleBookmarkToolStripMenuItem";
            this.toggleBookmarkToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
            this.toggleBookmarkToolStripMenuItem.Size = new System.Drawing.Size(323, 34);
            this.toggleBookmarkToolStripMenuItem.Text = "Toggle Bookmark";
            this.toggleBookmarkToolStripMenuItem.Click += new System.EventHandler(this.OnToggleBookmarkToolStripMenuItemClick);
            // 
            // jumpToNextToolStripMenuItem
            // 
            this.jumpToNextToolStripMenuItem.Image = global::LogExpert.Properties.Resources.down_blue;
            this.jumpToNextToolStripMenuItem.Name = "jumpToNextToolStripMenuItem";
            this.jumpToNextToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.jumpToNextToolStripMenuItem.Size = new System.Drawing.Size(323, 34);
            this.jumpToNextToolStripMenuItem.Text = "Jump to next";
            this.jumpToNextToolStripMenuItem.Click += new System.EventHandler(this.OnJumpToNextToolStripMenuItemClick);
            // 
            // jumpToPrevToolStripMenuItem
            // 
            this.jumpToPrevToolStripMenuItem.Image = global::LogExpert.Properties.Resources.up_blue;
            this.jumpToPrevToolStripMenuItem.Name = "jumpToPrevToolStripMenuItem";
            this.jumpToPrevToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F2)));
            this.jumpToPrevToolStripMenuItem.Size = new System.Drawing.Size(323, 34);
            this.jumpToPrevToolStripMenuItem.Text = "Jump to prev";
            this.jumpToPrevToolStripMenuItem.Click += new System.EventHandler(this.OnJumpToPrevToolStripMenuItemClick);
            // 
            // showBookmarkListToolStripMenuItem
            // 
            this.showBookmarkListToolStripMenuItem.Name = "showBookmarkListToolStripMenuItem";
            this.showBookmarkListToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.showBookmarkListToolStripMenuItem.Size = new System.Drawing.Size(323, 34);
            this.showBookmarkListToolStripMenuItem.Text = "Bookmark list";
            this.showBookmarkListToolStripMenuItem.Click += new System.EventHandler(this.OnShowBookmarkListToolStripMenuItemClick);
            // 
            // columnFinderToolStripMenuItem
            // 
            this.columnFinderToolStripMenuItem.CheckOnClick = true;
            this.columnFinderToolStripMenuItem.Name = "columnFinderToolStripMenuItem";
            this.columnFinderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.columnFinderToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.columnFinderToolStripMenuItem.Text = "Column finder";
            this.columnFinderToolStripMenuItem.Click += new System.EventHandler(this.OnColumnFinderToolStripMenuItemClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(267, 6);
            // 
            // toolStripEncodingMenuItem
            // 
            this.toolStripEncodingMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripEncodingASCIIItem,
            this.toolStripEncodingANSIItem,
            this.toolStripEncodingISO88591Item,
            this.toolStripEncodingUTF8Item,
            this.toolStripEncodingUTF16Item});
            this.toolStripEncodingMenuItem.Name = "toolStripEncodingMenuItem";
            this.toolStripEncodingMenuItem.Size = new System.Drawing.Size(270, 34);
            this.toolStripEncodingMenuItem.Text = "Encoding";
            // 
            // toolStripEncodingASCIIItem
            // 
            this.toolStripEncodingASCIIItem.Name = "toolStripEncodingASCIIItem";
            this.toolStripEncodingASCIIItem.Size = new System.Drawing.Size(207, 34);
            this.toolStripEncodingASCIIItem.Tag = "";
            this.toolStripEncodingASCIIItem.Text = "ASCII";
            this.toolStripEncodingASCIIItem.Click += new System.EventHandler(this.OnASCIIToolStripMenuItemClick);
            // 
            // toolStripEncodingANSIItem
            // 
            this.toolStripEncodingANSIItem.Name = "toolStripEncodingANSIItem";
            this.toolStripEncodingANSIItem.Size = new System.Drawing.Size(207, 34);
            this.toolStripEncodingANSIItem.Tag = "";
            this.toolStripEncodingANSIItem.Text = "ANSI";
            this.toolStripEncodingANSIItem.Click += new System.EventHandler(this.OnANSIToolStripMenuItemClick);
            // 
            // toolStripEncodingISO88591Item
            // 
            this.toolStripEncodingISO88591Item.Name = "toolStripEncodingISO88591Item";
            this.toolStripEncodingISO88591Item.Size = new System.Drawing.Size(207, 34);
            this.toolStripEncodingISO88591Item.Text = "ISO-8859-1";
            this.toolStripEncodingISO88591Item.Click += new System.EventHandler(this.OnISO88591ToolStripMenuItemClick);
            // 
            // toolStripEncodingUTF8Item
            // 
            this.toolStripEncodingUTF8Item.Name = "toolStripEncodingUTF8Item";
            this.toolStripEncodingUTF8Item.Size = new System.Drawing.Size(207, 34);
            this.toolStripEncodingUTF8Item.Text = "UTF8";
            this.toolStripEncodingUTF8Item.Click += new System.EventHandler(this.OnUTF8ToolStripMenuItemClick);
            // 
            // toolStripEncodingUTF16Item
            // 
            this.toolStripEncodingUTF16Item.Name = "toolStripEncodingUTF16Item";
            this.toolStripEncodingUTF16Item.Size = new System.Drawing.Size(207, 34);
            this.toolStripEncodingUTF16Item.Text = "Unicode";
            this.toolStripEncodingUTF16Item.Click += new System.EventHandler(this.OnUTF16ToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(267, 6);
            // 
            // timeshiftToolStripMenuItem
            // 
            this.timeshiftToolStripMenuItem.CheckOnClick = true;
            this.timeshiftToolStripMenuItem.Name = "timeshiftToolStripMenuItem";
            this.timeshiftToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.timeshiftToolStripMenuItem.Text = "Timeshift";
            this.timeshiftToolStripMenuItem.ToolTipText = "If supported by the columnizer, you can set an offset to the displayed log time";
            this.timeshiftToolStripMenuItem.CheckStateChanged += new System.EventHandler(this.OnTimeShiftToolStripMenuItemCheckStateChanged);
            // 
            // timeshiftMenuTextBox
            // 
            this.timeshiftMenuTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.timeshiftMenuTextBox.Enabled = false;
            this.timeshiftMenuTextBox.Name = "timeshiftMenuTextBox";
            this.timeshiftMenuTextBox.Size = new System.Drawing.Size(100, 31);
            this.timeshiftMenuTextBox.Text = "+00:00:00.000";
            this.timeshiftMenuTextBox.ToolTipText = "Time offset (hh:mm:ss.fff)";
            this.timeshiftMenuTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnTimeShiftMenuTextBoxKeyDown);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(267, 6);
            // 
            // copyMarkedLinesIntoNewTabToolStripMenuItem
            // 
            this.copyMarkedLinesIntoNewTabToolStripMenuItem.Name = "copyMarkedLinesIntoNewTabToolStripMenuItem";
            this.copyMarkedLinesIntoNewTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.copyMarkedLinesIntoNewTabToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.copyMarkedLinesIntoNewTabToolStripMenuItem.Text = "Copy to Tab";
            this.copyMarkedLinesIntoNewTabToolStripMenuItem.ToolTipText = "Copies all selected lines into a new tab page";
            this.copyMarkedLinesIntoNewTabToolStripMenuItem.Click += new System.EventHandler(this.OnCopyMarkedLinesIntoNewTabToolStripMenuItemClick);
            // 
            // optionToolStripMenuItem
            // 
            this.optionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.columnizerToolStripMenuItem,
            this.hilightingToolStripMenuItem1,
            this.settingsToolStripMenuItem,
            this.toolStripSeparator6,
            this.cellSelectModeToolStripMenuItem,
            this.alwaysOnTopToolStripMenuItem,
            this.hideLineColumnToolStripMenuItem,
            this.toolStripSeparator19,
            this.lockInstanceToolStripMenuItem});
            this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            this.optionToolStripMenuItem.Size = new System.Drawing.Size(92, 29);
            this.optionToolStripMenuItem.Text = "Options";
            this.optionToolStripMenuItem.DropDownOpening += new System.EventHandler(this.OnOptionToolStripMenuItemDropDownOpening);
            // 
            // columnizerToolStripMenuItem
            // 
            this.columnizerToolStripMenuItem.Name = "columnizerToolStripMenuItem";
            this.columnizerToolStripMenuItem.Size = new System.Drawing.Size(325, 34);
            this.columnizerToolStripMenuItem.Text = "Columnizer...";
            this.columnizerToolStripMenuItem.ToolTipText = "Splits various kinds of logfiles into fixed columns";
            this.columnizerToolStripMenuItem.Click += new System.EventHandler(this.OnSelectFilterToolStripMenuItemClick);
            // 
            // hilightingToolStripMenuItem1
            // 
            this.hilightingToolStripMenuItem1.Name = "hilightingToolStripMenuItem1";
            this.hilightingToolStripMenuItem1.Size = new System.Drawing.Size(325, 34);
            this.hilightingToolStripMenuItem1.Text = "Highlighting and triggers...";
            this.hilightingToolStripMenuItem1.Click += new System.EventHandler(this.OnHighlightingToolStripMenuItemClick);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(325, 34);
            this.settingsToolStripMenuItem.Text = "Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.OnSettingsToolStripMenuItemClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(322, 6);
            // 
            // cellSelectModeToolStripMenuItem
            // 
            this.cellSelectModeToolStripMenuItem.CheckOnClick = true;
            this.cellSelectModeToolStripMenuItem.Name = "cellSelectModeToolStripMenuItem";
            this.cellSelectModeToolStripMenuItem.Size = new System.Drawing.Size(325, 34);
            this.cellSelectModeToolStripMenuItem.Text = "Cell select mode";
            this.cellSelectModeToolStripMenuItem.ToolTipText = "Switches between foll row selection and single cell selection mode";
            this.cellSelectModeToolStripMenuItem.Click += new System.EventHandler(this.OnCellSelectModeToolStripMenuItemClick);
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.CheckOnClick = true;
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(325, 34);
            this.alwaysOnTopToolStripMenuItem.Text = "Always on top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.OnAlwaysOnTopToolStripMenuItemClick);
            // 
            // hideLineColumnToolStripMenuItem
            // 
            this.hideLineColumnToolStripMenuItem.CheckOnClick = true;
            this.hideLineColumnToolStripMenuItem.Name = "hideLineColumnToolStripMenuItem";
            this.hideLineColumnToolStripMenuItem.Size = new System.Drawing.Size(325, 34);
            this.hideLineColumnToolStripMenuItem.Text = "Hide line column";
            this.hideLineColumnToolStripMenuItem.Click += new System.EventHandler(this.OnHideLineColumnToolStripMenuItemClick);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(322, 6);
            // 
            // lockInstanceToolStripMenuItem
            // 
            this.lockInstanceToolStripMenuItem.Name = "lockInstanceToolStripMenuItem";
            this.lockInstanceToolStripMenuItem.Size = new System.Drawing.Size(325, 34);
            this.lockInstanceToolStripMenuItem.Text = "Lock instance";
            this.lockInstanceToolStripMenuItem.ToolTipText = "When enabled all new launched LogExpert instances will redirect to this window";
            this.lockInstanceToolStripMenuItem.Click += new System.EventHandler(this.OnLockInstanceToolStripMenuItemClick);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configureToolStripMenuItem,
            this.configureToolStripSeparator});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(69, 29);
            this.toolsToolStripMenuItem.Text = "Tools";
            this.toolsToolStripMenuItem.ToolTipText = "Launch external tools (configure in the settings)";
            this.toolsToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.OnToolsToolStripMenuItemDropDownItemClicked);
            // 
            // configureToolStripMenuItem
            // 
            this.configureToolStripMenuItem.Name = "configureToolStripMenuItem";
            this.configureToolStripMenuItem.Size = new System.Drawing.Size(204, 34);
            this.configureToolStripMenuItem.Text = "Configure...";
            this.configureToolStripMenuItem.Click += new System.EventHandler(this.OnConfigureToolStripMenuItemClick);
            // 
            // configureToolStripSeparator
            // 
            this.configureToolStripSeparator.Name = "configureToolStripSeparator";
            this.configureToolStripSeparator.Size = new System.Drawing.Size(201, 6);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showHelpToolStripMenuItem,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // showHelpToolStripMenuItem
            // 
            this.showHelpToolStripMenuItem.Name = "showHelpToolStripMenuItem";
            this.showHelpToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.showHelpToolStripMenuItem.Size = new System.Drawing.Size(228, 34);
            this.showHelpToolStripMenuItem.Text = "Show help";
            this.showHelpToolStripMenuItem.Click += new System.EventHandler(this.OnShowHelpToolStripMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(225, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(228, 34);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.OnAboutToolStripMenuItemClick);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dumpLogBufferInfoToolStripMenuItem,
            this.dumpBufferDiagnosticToolStripMenuItem,
            this.runGCToolStripMenuItem,
            this.gCInfoToolStripMenuItem,
            this.toolStripSeparator18,
            this.throwExceptionGUIThreadToolStripMenuItem,
            this.throwExceptionbackgroundThToolStripMenuItem,
            this.throwExceptionBackgroundThreadToolStripMenuItem,
            this.toolStripSeparator12,
            this.loglevelToolStripMenuItem,
            this.disableWordHighlightModeToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(82, 29);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // dumpLogBufferInfoToolStripMenuItem
            // 
            this.dumpLogBufferInfoToolStripMenuItem.Name = "dumpLogBufferInfoToolStripMenuItem";
            this.dumpLogBufferInfoToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.dumpLogBufferInfoToolStripMenuItem.Text = "Dump LogBuffer info";
            this.dumpLogBufferInfoToolStripMenuItem.Click += new System.EventHandler(this.OnDumpLogBufferInfoToolStripMenuItemClick);
            // 
            // dumpBufferDiagnosticToolStripMenuItem
            // 
            this.dumpBufferDiagnosticToolStripMenuItem.Name = "dumpBufferDiagnosticToolStripMenuItem";
            this.dumpBufferDiagnosticToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.dumpBufferDiagnosticToolStripMenuItem.Text = "Dump buffer diagnostic";
            this.dumpBufferDiagnosticToolStripMenuItem.Click += new System.EventHandler(this.OnDumpBufferDiagnosticToolStripMenuItemClick);
            // 
            // runGCToolStripMenuItem
            // 
            this.runGCToolStripMenuItem.Name = "runGCToolStripMenuItem";
            this.runGCToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.runGCToolStripMenuItem.Text = "Run GC";
            this.runGCToolStripMenuItem.Click += new System.EventHandler(this.OnRunGCToolStripMenuItemClick);
            // 
            // gCInfoToolStripMenuItem
            // 
            this.gCInfoToolStripMenuItem.Name = "gCInfoToolStripMenuItem";
            this.gCInfoToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.gCInfoToolStripMenuItem.Text = "Dump GC info";
            this.gCInfoToolStripMenuItem.Click += new System.EventHandler(this.OnGCInfoToolStripMenuItemClick);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(408, 6);
            // 
            // throwExceptionGUIThreadToolStripMenuItem
            // 
            this.throwExceptionGUIThreadToolStripMenuItem.Name = "throwExceptionGUIThreadToolStripMenuItem";
            this.throwExceptionGUIThreadToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.throwExceptionGUIThreadToolStripMenuItem.Text = "Throw exception (GUI Thread)";
            this.throwExceptionGUIThreadToolStripMenuItem.Click += new System.EventHandler(this.OnThrowExceptionGUIThreadToolStripMenuItemClick);
            // 
            // throwExceptionbackgroundThToolStripMenuItem
            // 
            this.throwExceptionbackgroundThToolStripMenuItem.Name = "throwExceptionbackgroundThToolStripMenuItem";
            this.throwExceptionbackgroundThToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.throwExceptionbackgroundThToolStripMenuItem.Text = "Throw exception (Async delegate)";
            this.throwExceptionbackgroundThToolStripMenuItem.Click += new System.EventHandler(this.OnThrowExceptionBackgroundThToolStripMenuItemClick);
            // 
            // throwExceptionBackgroundThreadToolStripMenuItem
            // 
            this.throwExceptionBackgroundThreadToolStripMenuItem.Name = "throwExceptionBackgroundThreadToolStripMenuItem";
            this.throwExceptionBackgroundThreadToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.throwExceptionBackgroundThreadToolStripMenuItem.Text = "Throw exception (background thread)";
            this.throwExceptionBackgroundThreadToolStripMenuItem.Click += new System.EventHandler(this.OnThrowExceptionBackgroundThreadToolStripMenuItemClick);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(408, 6);
            // 
            // loglevelToolStripMenuItem
            // 
            this.loglevelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.warnToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.debugToolStripMenuItem1});
            this.loglevelToolStripMenuItem.Name = "loglevelToolStripMenuItem";
            this.loglevelToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.loglevelToolStripMenuItem.Text = "Loglevel";
            this.loglevelToolStripMenuItem.DropDownOpening += new System.EventHandler(this.OnLogLevelToolStripMenuItemDropDownOpening);
            this.loglevelToolStripMenuItem.Click += new System.EventHandler(this.OnLogLevelToolStripMenuItemClick);
            // 
            // warnToolStripMenuItem
            // 
            this.warnToolStripMenuItem.Name = "warnToolStripMenuItem";
            this.warnToolStripMenuItem.Size = new System.Drawing.Size(168, 34);
            this.warnToolStripMenuItem.Text = "Warn";
            this.warnToolStripMenuItem.Click += new System.EventHandler(this.OnWarnToolStripMenuItemClick);
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(168, 34);
            this.infoToolStripMenuItem.Text = "Info";
            this.infoToolStripMenuItem.Click += new System.EventHandler(this.OnInfoToolStripMenuItemClick);
            // 
            // debugToolStripMenuItem1
            // 
            this.debugToolStripMenuItem1.Name = "debugToolStripMenuItem1";
            this.debugToolStripMenuItem1.Size = new System.Drawing.Size(168, 34);
            this.debugToolStripMenuItem1.Text = "Debug";
            this.debugToolStripMenuItem1.Click += new System.EventHandler(this.OnDebugToolStripMenuItemClick);
            // 
            // disableWordHighlightModeToolStripMenuItem
            // 
            this.disableWordHighlightModeToolStripMenuItem.CheckOnClick = true;
            this.disableWordHighlightModeToolStripMenuItem.Name = "disableWordHighlightModeToolStripMenuItem";
            this.disableWordHighlightModeToolStripMenuItem.Size = new System.Drawing.Size(411, 34);
            this.disableWordHighlightModeToolStripMenuItem.Text = "Disable word highlight mode";
            this.disableWordHighlightModeToolStripMenuItem.Click += new System.EventHandler(this.OnDisableWordHighlightModeToolStripMenuItemClick);
            // 
            // host
            // 
            this.host.AccessibleName = "host";
            this.host.AutoSize = true;
            this.host.BackColor = System.Drawing.Color.Transparent;
            this.host.Location = new System.Drawing.Point(9, 1);
            this.host.Name = "host";
            this.host.Size = new System.Drawing.Size(80, 22);
            this.host.TabIndex = 7;
            this.host.Text = "Follow tail";
            this.host.UseVisualStyleBackColor = false;
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.BottomToolStripPanel
            // 
            this.toolStripContainer.BottomToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer.ContentPanel
            // 
            this.toolStripContainer.ContentPanel.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripContainer.ContentPanel.Controls.Add(this.dockPanel);
            this.toolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(1443, 712);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // toolStripContainer.LeftToolStripPanel
            // 
            this.toolStripContainer.LeftToolStripPanel.Enabled = false;
            this.toolStripContainer.LeftToolStripPanelVisible = false;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripContainer.Name = "toolStripContainer";
            // 
            // toolStripContainer.RightToolStripPanel
            // 
            this.toolStripContainer.RightToolStripPanel.Enabled = false;
            this.toolStripContainer.RightToolStripPanelVisible = false;
            this.toolStripContainer.Size = new System.Drawing.Size(1443, 779);
            this.toolStripContainer.TabIndex = 13;
            this.toolStripContainer.Text = "toolStripContainer1";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            this.toolStripContainer.TopToolStripPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.buttonToolStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.mainMenuStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.externalToolsToolStrip);
            // 
            // dockPanel
            // 
            this.dockPanel.ActiveAutoHideContent = null;
            this.dockPanel.BackColor = System.Drawing.SystemColors.Control;
            this.dockPanel.DefaultFloatWindowSize = new System.Drawing.Size(600, 400);
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.DockBackColor = System.Drawing.SystemColors.Control;
            this.dockPanel.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow;
            this.dockPanel.Location = new System.Drawing.Point(0, 0);
            this.dockPanel.Margin = new System.Windows.Forms.Padding(0);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.ShowDocumentIcon = true;
            this.dockPanel.Size = new System.Drawing.Size(1443, 712);
            dockPanelGradient1.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient1.StartColor = System.Drawing.SystemColors.ControlLight;
            autoHideStripSkin1.DockStripGradient = dockPanelGradient1;
            tabGradient1.EndColor = System.Drawing.SystemColors.Control;
            tabGradient1.StartColor = System.Drawing.SystemColors.Control;
            tabGradient1.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            autoHideStripSkin1.TabGradient = tabGradient1;
            autoHideStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            dockPanelSkin1.AutoHideStripSkin = autoHideStripSkin1;
            tabGradient2.EndColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient2.StartColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient2.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripGradient1.ActiveTabGradient = tabGradient2;
            dockPanelGradient2.EndColor = System.Drawing.SystemColors.Control;
            dockPanelGradient2.StartColor = System.Drawing.SystemColors.Control;
            dockPaneStripGradient1.DockStripGradient = dockPanelGradient2;
            tabGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
            tabGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
            tabGradient3.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripGradient1.InactiveTabGradient = tabGradient3;
            dockPaneStripSkin1.DocumentGradient = dockPaneStripGradient1;
            dockPaneStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            tabGradient4.EndColor = System.Drawing.SystemColors.ActiveCaption;
            tabGradient4.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient4.StartColor = System.Drawing.SystemColors.GradientActiveCaption;
            tabGradient4.TextColor = System.Drawing.SystemColors.ActiveCaptionText;
            dockPaneStripToolWindowGradient1.ActiveCaptionGradient = tabGradient4;
            tabGradient5.EndColor = System.Drawing.SystemColors.Control;
            tabGradient5.StartColor = System.Drawing.SystemColors.Control;
            tabGradient5.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripToolWindowGradient1.ActiveTabGradient = tabGradient5;
            dockPanelGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
            dockPaneStripToolWindowGradient1.DockStripGradient = dockPanelGradient3;
            tabGradient6.EndColor = System.Drawing.SystemColors.InactiveCaption;
            tabGradient6.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient6.StartColor = System.Drawing.SystemColors.GradientInactiveCaption;
            tabGradient6.TextColor = System.Drawing.SystemColors.InactiveCaptionText;
            dockPaneStripToolWindowGradient1.InactiveCaptionGradient = tabGradient6;
            tabGradient7.EndColor = System.Drawing.Color.Transparent;
            tabGradient7.StartColor = System.Drawing.Color.Transparent;
            tabGradient7.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            dockPaneStripToolWindowGradient1.InactiveTabGradient = tabGradient7;
            dockPaneStripSkin1.ToolWindowGradient = dockPaneStripToolWindowGradient1;
            dockPanelSkin1.DockPaneStripSkin = dockPaneStripSkin1;
            this.dockPanel.Skin = dockPanelSkin1;
            this.dockPanel.TabIndex = 14;
            this.dockPanel.ActiveContentChanged += new System.EventHandler(this.OnDockPanelActiveContentChanged);
            // 
            // buttonToolStrip
            // 
            this.buttonToolStrip.AllowMerge = false;
            this.buttonToolStrip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.buttonToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.buttonToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.buttonToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOpen,
            this.toolStripSeparator10,
            this.toolStripButtonSearch,
            this.toolStripButtonFilter,
            this.toolStripSeparator11,
            this.toolStripButtonBookmark,
            this.toolStripButtonUp,
            this.toolStripButtonDown,
            this.toolStripSeparator1,
            this.toolStripButtonBubbles,
            this.toolStripSeparator15,
            this.toolStripButtonTail,
            this.toolStripSeparator17,
            this.groupsComboBoxHighlightGroups});
            this.buttonToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.buttonToolStrip.Location = new System.Drawing.Point(4, 0);
            this.buttonToolStrip.Name = "buttonToolStrip";
            this.buttonToolStrip.Size = new System.Drawing.Size(463, 34);
            this.buttonToolStrip.TabIndex = 7;
            // 
            // toolStripButtonOpen
            // 
            this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpen.Image = global::LogExpert.Properties.Resources.folder_blue;
            this.toolStripButtonOpen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new System.Drawing.Size(34, 18);
            this.toolStripButtonOpen.Text = "toolStripButton1";
            this.toolStripButtonOpen.ToolTipText = "Open file";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.OnToolStripButtonOpenClick);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonSearch
            // 
            this.toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSearch.Image = global::LogExpert.Properties.Resources.search_icon_blue;
            this.toolStripButtonSearch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSearch.Name = "toolStripButtonSearch";
            this.toolStripButtonSearch.Size = new System.Drawing.Size(34, 18);
            this.toolStripButtonSearch.Text = "toolStripButtonSearch";
            this.toolStripButtonSearch.ToolTipText = "Search";
            this.toolStripButtonSearch.Click += new System.EventHandler(this.OnToolStripButtonSearchClick);
            // 
            // toolStripButtonFilter
            // 
            this.toolStripButtonFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFilter.Image = global::LogExpert.Properties.Resources.search_folder_blue;
            this.toolStripButtonFilter.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFilter.Name = "toolStripButtonFilter";
            this.toolStripButtonFilter.Size = new System.Drawing.Size(34, 18);
            this.toolStripButtonFilter.Text = "toolStripButton1";
            this.toolStripButtonFilter.ToolTipText = "Filter window";
            this.toolStripButtonFilter.Click += new System.EventHandler(this.OnToolStripButtonFilterClick);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonBookmark
            // 
            this.toolStripButtonBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonBookmark.Image = global::LogExpert.Properties.Resources.check_blue;
            this.toolStripButtonBookmark.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonBookmark.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBookmark.Name = "toolStripButtonBookmark";
            this.toolStripButtonBookmark.Size = new System.Drawing.Size(34, 18);
            this.toolStripButtonBookmark.Text = "toolStripButton1";
            this.toolStripButtonBookmark.ToolTipText = "Toggle bookmark";
            this.toolStripButtonBookmark.Click += new System.EventHandler(this.OnToolStripButtonBookmarkClick);
            // 
            // toolStripButtonUp
            // 
            this.toolStripButtonUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonUp.Image = global::LogExpert.Properties.Resources.up_blue;
            this.toolStripButtonUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonUp.Name = "toolStripButtonUp";
            this.toolStripButtonUp.Size = new System.Drawing.Size(34, 18);
            this.toolStripButtonUp.Text = "toolStripButton1";
            this.toolStripButtonUp.ToolTipText = "Go to previous bookmark";
            this.toolStripButtonUp.Click += new System.EventHandler(this.OnToolStripButtonUpClick);
            // 
            // toolStripButtonDown
            // 
            this.toolStripButtonDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonDown.Image = global::LogExpert.Properties.Resources.down_blue;
            this.toolStripButtonDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonDown.Name = "toolStripButtonDown";
            this.toolStripButtonDown.Size = new System.Drawing.Size(34, 18);
            this.toolStripButtonDown.Text = "toolStripButton1";
            this.toolStripButtonDown.ToolTipText = "Go to next bookmark";
            this.toolStripButtonDown.Click += new System.EventHandler(this.OnToolStripButtonDownClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonBubbles
            // 
            this.toolStripButtonBubbles.CheckOnClick = true;
            this.toolStripButtonBubbles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonBubbles.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonBubbles.Image")));
            this.toolStripButtonBubbles.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolStripButtonBubbles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBubbles.Name = "toolStripButtonBubbles";
            this.toolStripButtonBubbles.Size = new System.Drawing.Size(34, 28);
            this.toolStripButtonBubbles.Text = "Show bookmark bubbles";
            this.toolStripButtonBubbles.Click += new System.EventHandler(this.OnToolStripButtonBubblesClick);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonTail
            // 
            this.toolStripButtonTail.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonTail.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonTail.Image")));
            this.toolStripButtonTail.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonTail.Name = "toolStripButtonTail";
            this.toolStripButtonTail.Size = new System.Drawing.Size(39, 29);
            this.toolStripButtonTail.Text = "tail";
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(6, 23);
            // 
            // groupsComboBoxHighlightGroups
            // 
            this.groupsComboBoxHighlightGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.groupsComboBoxHighlightGroups.DropDownWidth = 250;
            this.groupsComboBoxHighlightGroups.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.groupsComboBoxHighlightGroups.Name = "groupsComboBoxHighlightGroups";
            this.groupsComboBoxHighlightGroups.Size = new System.Drawing.Size(150, 33);
            this.groupsComboBoxHighlightGroups.ToolTipText = "Select the current highlight settings for the log file (right-click to open highl" +
    "ight settings)";
            this.groupsComboBoxHighlightGroups.DropDownClosed += new System.EventHandler(this.OnHighlightGroupsComboBoxDropDownClosed);
            this.groupsComboBoxHighlightGroups.SelectedIndexChanged += new System.EventHandler(this.OnHighlightGroupsComboBoxSelectedIndexChanged);
            this.groupsComboBoxHighlightGroups.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnHighlightGroupsComboBoxMouseUp);
            // 
            // externalToolsToolStrip
            // 
            this.externalToolsToolStrip.AllowMerge = false;
            this.externalToolsToolStrip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.externalToolsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.externalToolsToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.externalToolsToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.externalToolsToolStrip.Location = new System.Drawing.Point(148, 67);
            this.externalToolsToolStrip.Name = "externalToolsToolStrip";
            this.externalToolsToolStrip.Size = new System.Drawing.Size(2, 0);
            this.externalToolsToolStrip.TabIndex = 8;
            this.externalToolsToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.OnExternalToolsToolStripItemClicked);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
            // 
            // checkBoxFollowTail
            // 
            this.checkBoxFollowTail.AutoSize = true;
            this.checkBoxFollowTail.Location = new System.Drawing.Point(596, 788);
            this.checkBoxFollowTail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxFollowTail.Name = "checkBoxFollowTail";
            this.checkBoxFollowTail.Size = new System.Drawing.Size(104, 24);
            this.checkBoxFollowTail.TabIndex = 14;
            this.checkBoxFollowTail.Text = "Follow tail";
            this.checkBoxFollowTail.UseVisualStyleBackColor = true;
            this.checkBoxFollowTail.Click += new System.EventHandler(this.OnFollowTailCheckBoxClick);
            // 
            // tabContextMenuStrip
            // 
            this.tabContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tabContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeThisTabToolStripMenuItem,
            this.closeOtherTabsToolStripMenuItem,
            this.closeAllTabsToolStripMenuItem,
            this.toolStripSeparator13,
            this.tabColorToolStripMenuItem,
            this.tabRenameToolStripMenuItem,
            this.toolStripSeparator16,
            this.copyPathToClipboardToolStripMenuItem,
            this.findInExplorerToolStripMenuItem});
            this.tabContextMenuStrip.Name = "tabContextMenuStrip";
            this.tabContextMenuStrip.Size = new System.Drawing.Size(270, 240);
            // 
            // closeThisTabToolStripMenuItem
            // 
            this.closeThisTabToolStripMenuItem.Name = "closeThisTabToolStripMenuItem";
            this.closeThisTabToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.closeThisTabToolStripMenuItem.Text = "Close this tab";
            this.closeThisTabToolStripMenuItem.Click += new System.EventHandler(this.OnCloseThisTabToolStripMenuItemClick);
            // 
            // closeOtherTabsToolStripMenuItem
            // 
            this.closeOtherTabsToolStripMenuItem.Name = "closeOtherTabsToolStripMenuItem";
            this.closeOtherTabsToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.closeOtherTabsToolStripMenuItem.Text = "Close other tabs";
            this.closeOtherTabsToolStripMenuItem.ToolTipText = "Close all tabs except of this one";
            this.closeOtherTabsToolStripMenuItem.Click += new System.EventHandler(this.OnCloseOtherTabsToolStripMenuItemClick);
            // 
            // closeAllTabsToolStripMenuItem
            // 
            this.closeAllTabsToolStripMenuItem.Name = "closeAllTabsToolStripMenuItem";
            this.closeAllTabsToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.closeAllTabsToolStripMenuItem.Text = "Close all tabs";
            this.closeAllTabsToolStripMenuItem.ToolTipText = "Close all tabs";
            this.closeAllTabsToolStripMenuItem.Click += new System.EventHandler(this.OnCloseAllTabsToolStripMenuItemClick);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(266, 6);
            // 
            // tabColorToolStripMenuItem
            // 
            this.tabColorToolStripMenuItem.Name = "tabColorToolStripMenuItem";
            this.tabColorToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.tabColorToolStripMenuItem.Text = "Tab color...";
            this.tabColorToolStripMenuItem.ToolTipText = "Sets the tab color";
            this.tabColorToolStripMenuItem.Click += new System.EventHandler(this.OnTabColorToolStripMenuItemClick);
            // 
            // tabRenameToolStripMenuItem
            // 
            this.tabRenameToolStripMenuItem.Name = "tabRenameToolStripMenuItem";
            this.tabRenameToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.tabRenameToolStripMenuItem.Text = "Tab rename...";
            this.tabRenameToolStripMenuItem.ToolTipText = "Set the text which is shown on the tab";
            this.tabRenameToolStripMenuItem.Click += new System.EventHandler(this.OnTabRenameToolStripMenuItemClick);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(266, 6);
            // 
            // copyPathToClipboardToolStripMenuItem
            // 
            this.copyPathToClipboardToolStripMenuItem.Name = "copyPathToClipboardToolStripMenuItem";
            this.copyPathToClipboardToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.copyPathToClipboardToolStripMenuItem.Text = "Copy path to clipboard";
            this.copyPathToClipboardToolStripMenuItem.ToolTipText = "The complete file name (incl. path) is copied to clipboard";
            this.copyPathToClipboardToolStripMenuItem.Click += new System.EventHandler(this.OnCopyPathToClipboardToolStripMenuItemClick);
            // 
            // findInExplorerToolStripMenuItem
            // 
            this.findInExplorerToolStripMenuItem.Name = "findInExplorerToolStripMenuItem";
            this.findInExplorerToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.findInExplorerToolStripMenuItem.Text = "Find in Explorer";
            this.findInExplorerToolStripMenuItem.ToolTipText = "Opens an Explorer window and selects the log file";
            this.findInExplorerToolStripMenuItem.Click += new System.EventHandler(this.OnFindInExplorerToolStripMenuItemClick);
            // 
            // dragControlDateTime
            // 
            this.dragControlDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.dragControlDateTime.BackColor = System.Drawing.SystemColors.Control;
            this.dragControlDateTime.DateTime = new System.DateTime(((long)(0)));
            this.dragControlDateTime.DragOrientation = LogExpert.Dialogs.DateTimeDragControl.DragOrientations.Vertical;
            this.dragControlDateTime.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dragControlDateTime.HoverColor = System.Drawing.Color.LightGray;
            this.dragControlDateTime.Location = new System.Drawing.Point(916, 782);
            this.dragControlDateTime.Margin = new System.Windows.Forms.Padding(0);
            this.dragControlDateTime.MaxDateTime = new System.DateTime(9999, 12, 31, 23, 59, 59, 999);
            this.dragControlDateTime.MinDateTime = new System.DateTime(((long)(0)));
            this.dragControlDateTime.Name = "dragControlDateTime";
            this.dragControlDateTime.Size = new System.Drawing.Size(282, 31);
            this.dragControlDateTime.TabIndex = 14;
            this.dragControlDateTime.ValueChanged += new LogExpert.Dialogs.DateTimeDragControl.ValueChangedEventHandler(this.OnDateTimeDragControlValueChanged);
            this.dragControlDateTime.ValueDragged += new LogExpert.Dialogs.DateTimeDragControl.ValueDraggedEventHandler(this.OnDateTimeDragControlValueDragged);
            // 
            // LogTabWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1443, 814);
            this.Controls.Add(this.checkBoxFollowTail);
            this.Controls.Add(this.dragControlDateTime);
            this.Controls.Add(this.toolStripContainer);
            this.Controls.Add(this.statusStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.mainMenuStrip;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "LogTabWindow";
            this.Text = "LogExpert";
            this.Activated += new System.EventHandler(this.OnLogTabWindowActivated);
            this.Deactivate += new System.EventHandler(this.OnLogTabWindowDeactivate);
            this.SizeChanged += new System.EventHandler(this.OnLogTabWindowSizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnLogWindowDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnLogTabWindowDragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.OnLogWindowDragOver);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnLogTabWindowKeyDown);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.buttonToolStrip.ResumeLayout(false);
            this.buttonToolStrip.PerformLayout();
            this.tabContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel labelLines;
        private System.Windows.Forms.ToolStripStatusLabel labelSize;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewNavigateToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel labelCurrentLine;
        private System.Windows.Forms.ToolStripProgressBar loadProgessBar;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer toolStripContainer;
        private System.Windows.Forms.ToolStripMenuItem closeFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timeshiftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyMarkedLinesIntoNewTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hilightingToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cellSelectModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox timeshiftMenuTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bookmarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jumpToNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jumpToPrevToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripEncodingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripEncodingASCIIItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripEncodingANSIItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripEncodingUTF8Item;
        private System.Windows.Forms.ToolStripMenuItem toolStripEncodingUTF16Item;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem reloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem columnizerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private DateTimeDragControl dragControlDateTime;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem showBookmarkListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStrip buttonToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonFilter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripButton toolStripButtonBookmark;
        private System.Windows.Forms.ToolStripButton toolStripButtonUp;
        private System.Windows.Forms.ToolStripButton toolStripButtonDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private CheckBox host;
        private CheckBox checkBoxFollowTail;
        private ToolStripButton toolStripButtonTail;
        private ToolStripMenuItem showHelpToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem hideLineColumnToolStripMenuItem;
        private ToolStripMenuItem lastUsedToolStripMenuItem;
        private ContextMenuStrip tabContextMenuStrip;
        private ToolStripMenuItem closeThisTabToolStripMenuItem;
        private ToolStripMenuItem closeOtherTabsToolStripMenuItem;
        private ToolStripMenuItem closeAllTabsToolStripMenuItem;
        private ToolStripMenuItem tabColorToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem loadProjectToolStripMenuItem;
        private ToolStripMenuItem saveProjectToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripButton toolStripButtonBubbles;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem copyPathToClipboardToolStripMenuItem;
        private ToolStripMenuItem findInExplorerToolStripMenuItem;
        private ToolStripMenuItem exportBookmarksToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator17;
        private ToolStripComboBox groupsComboBoxHighlightGroups;
        private ToolStripMenuItem debugToolStripMenuItem;
        private ToolStripMenuItem dumpLogBufferInfoToolStripMenuItem;
        private ToolStripMenuItem dumpBufferDiagnosticToolStripMenuItem;
        private ToolStripMenuItem runGCToolStripMenuItem;
        private ToolStripMenuItem gCInfoToolStripMenuItem;
        private ToolStrip externalToolsToolStrip;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem configureToolStripMenuItem;
        private ToolStripSeparator configureToolStripSeparator;
        private ToolStripMenuItem throwExceptionGUIThreadToolStripMenuItem;
        private ToolStripMenuItem throwExceptionbackgroundThToolStripMenuItem;
        private ToolStripMenuItem throwExceptionBackgroundThreadToolStripMenuItem;
        private ToolStripMenuItem loglevelToolStripMenuItem;
        private ToolStripMenuItem warnToolStripMenuItem;
        private ToolStripMenuItem infoToolStripMenuItem;
        private ToolStripMenuItem debugToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem disableWordHighlightModeToolStripMenuItem;
        private ToolStripMenuItem multifileMaskToolStripMenuItem;
        private ToolStripMenuItem multiFileEnabledStripMenuItem;
        private ToolStripMenuItem toolStripEncodingISO88591Item;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripMenuItem lockInstanceToolStripMenuItem;
        private ToolStripMenuItem newFromClipboardToolStripMenuItem;
        private ToolStripMenuItem openURIToolStripMenuItem;
        private ToolStripMenuItem columnFinderToolStripMenuItem;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
        private ToolStripMenuItem tabRenameToolStripMenuItem;
    }
}

