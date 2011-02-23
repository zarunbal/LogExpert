using LogExpert.Dialogs;
using System.Windows.Forms;
using System.Drawing;
namespace LogExpert
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogTabWindow));
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.linesLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.sizeLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.currentLineLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.loadProgessBar = new System.Windows.Forms.ToolStripProgressBar();
      this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
      this.goToLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toggleBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.jumpToNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.jumpToPrevToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.showBookmarkListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.encodingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.aSCIIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.aNSIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.iSO88591ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.uTF8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.uTF16ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
      this.timeshiftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.timeshiftMenuTextBox = new System.Windows.Forms.ToolStripTextBox();
      this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      this.copyMarkedLinesIntoNewTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.showHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
      this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.columnizerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.hilightingToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
      this.cellSelectModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.hideLineColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.dumpLogBufferInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.dumpBufferDiagnosticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.runGCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.gCInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
      this.throwExceptionGUIThreadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.throwExceptionbackgroundThToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.throwExceptionbackgroundThreadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
      this.loglevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.warnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.debugToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.disableWordHighlightModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.configureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.configureToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
      this.host = new System.Windows.Forms.CheckBox();
      this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
      this.tabControl1 = new MdiTabControl.TabControl();
      this.toolStrip4 = new System.Windows.Forms.ToolStrip();
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
      this.highlightGroupsComboBox = new System.Windows.Forms.ToolStripComboBox();
      this.externalToolsToolStrip = new System.Windows.Forms.ToolStrip();
      this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
      this.followTailCheckBox = new System.Windows.Forms.CheckBox();
      this.tabContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.closeThisTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeOtherTabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeAllTabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
      this.tabColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
      this.copyPathToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.findInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.dateTimeDragControl = new LogExpert.Dialogs.DateTimeDragControl();
      this.statusStrip1.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.toolStripContainer1.ContentPanel.SuspendLayout();
      this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
      this.toolStripContainer1.SuspendLayout();
      this.toolStrip4.SuspendLayout();
      this.tabContextMenuStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.linesLabel,
            this.sizeLabel,
            this.currentLineLabel,
            this.loadProgessBar,
            this.statusLabel});
      this.statusStrip1.Location = new System.Drawing.Point(0, 505);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(962, 24);
      this.statusStrip1.TabIndex = 5;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // linesLabel
      // 
      this.linesLabel.AutoSize = false;
      this.linesLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                  | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                  | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
      this.linesLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
      this.linesLabel.Name = "linesLabel";
      this.linesLabel.Size = new System.Drawing.Size(90, 19);
      this.linesLabel.Text = "0";
      // 
      // sizeLabel
      // 
      this.sizeLabel.AutoSize = false;
      this.sizeLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                  | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                  | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
      this.sizeLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
      this.sizeLabel.Name = "sizeLabel";
      this.sizeLabel.Size = new System.Drawing.Size(90, 19);
      this.sizeLabel.Text = "0";
      // 
      // currentLineLabel
      // 
      this.currentLineLabel.AutoSize = false;
      this.currentLineLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                  | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                  | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
      this.currentLineLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
      this.currentLineLabel.Name = "currentLineLabel";
      this.currentLineLabel.Size = new System.Drawing.Size(90, 19);
      this.currentLineLabel.Text = "L:";
      // 
      // loadProgessBar
      // 
      this.loadProgessBar.Name = "loadProgessBar";
      this.loadProgessBar.Size = new System.Drawing.Size(50, 18);
      // 
      // statusLabel
      // 
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(39, 19);
      this.statusLabel.Text = "Ready";
      // 
      // menuStrip1
      // 
      this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
      this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.helpToolStripMenuItem,
            this.optionToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.toolsToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(962, 24);
      this.menuStrip1.TabIndex = 6;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeFileToolStripMenuItem,
            this.reloadToolStripMenuItem,
            this.toolStripSeparator8,
            this.multiFileToolStripMenuItem,
            this.toolStripSeparator7,
            this.loadProjectToolStripMenuItem,
            this.saveProjectToolStripMenuItem,
            this.exportBookmarksToolStripMenuItem,
            this.toolStripSeparator14,
            this.lastUsedToolStripMenuItem,
            this.exitToolStripMenuItem});
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
      this.toolStripMenuItem1.Text = "File";
      // 
      // openToolStripMenuItem
      // 
      this.openToolStripMenuItem.Name = "openToolStripMenuItem";
      this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.openToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.openToolStripMenuItem.Text = "Open...";
      this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
      // 
      // closeFileToolStripMenuItem
      // 
      this.closeFileToolStripMenuItem.Name = "closeFileToolStripMenuItem";
      this.closeFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
      this.closeFileToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.closeFileToolStripMenuItem.Text = "Close File";
      this.closeFileToolStripMenuItem.Click += new System.EventHandler(this.closeFileToolStripMenuItem_Click);
      // 
      // reloadToolStripMenuItem
      // 
      this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
      this.reloadToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
      this.reloadToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.reloadToolStripMenuItem.Text = "Reload";
      this.reloadToolStripMenuItem.Click += new System.EventHandler(this.reloadToolStripMenuItem_Click);
      // 
      // toolStripSeparator8
      // 
      this.toolStripSeparator8.Name = "toolStripSeparator8";
      this.toolStripSeparator8.Size = new System.Drawing.Size(175, 6);
      // 
      // multiFileToolStripMenuItem
      // 
      this.multiFileToolStripMenuItem.CheckOnClick = true;
      this.multiFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.multiFileEnabledStripMenuItem,
            this.multifileMaskToolStripMenuItem});
      this.multiFileToolStripMenuItem.Name = "multiFileToolStripMenuItem";
      this.multiFileToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.multiFileToolStripMenuItem.Text = "MultiFile";
      this.multiFileToolStripMenuItem.ToolTipText = "Treat multiple files as one large file (e.g. data.log, data.log.1, data.log.2,..." +
          ")";
      this.multiFileToolStripMenuItem.Click += new System.EventHandler(this.multiFileToolStripMenuItem_Click);
      // 
      // multiFileEnabledStripMenuItem
      // 
      this.multiFileEnabledStripMenuItem.CheckOnClick = true;
      this.multiFileEnabledStripMenuItem.Name = "multiFileEnabledStripMenuItem";
      this.multiFileEnabledStripMenuItem.Size = new System.Drawing.Size(165, 22);
      this.multiFileEnabledStripMenuItem.Text = "Enable MultiFile";
      this.multiFileEnabledStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
      // 
      // multifileMaskToolStripMenuItem
      // 
      this.multifileMaskToolStripMenuItem.Name = "multifileMaskToolStripMenuItem";
      this.multifileMaskToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
      this.multifileMaskToolStripMenuItem.Text = "File name mask...";
      this.multifileMaskToolStripMenuItem.Click += new System.EventHandler(this.multifileMaskToolStripMenuItem_Click);
      // 
      // toolStripSeparator7
      // 
      this.toolStripSeparator7.Name = "toolStripSeparator7";
      this.toolStripSeparator7.Size = new System.Drawing.Size(175, 6);
      // 
      // loadProjectToolStripMenuItem
      // 
      this.loadProjectToolStripMenuItem.Name = "loadProjectToolStripMenuItem";
      this.loadProjectToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.loadProjectToolStripMenuItem.Text = "Load session...";
      this.loadProjectToolStripMenuItem.ToolTipText = "Load a saved session (list of log files)";
      this.loadProjectToolStripMenuItem.Click += new System.EventHandler(this.loadProjectToolStripMenuItem_Click);
      // 
      // saveProjectToolStripMenuItem
      // 
      this.saveProjectToolStripMenuItem.Name = "saveProjectToolStripMenuItem";
      this.saveProjectToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.saveProjectToolStripMenuItem.Text = "Save session...";
      this.saveProjectToolStripMenuItem.ToolTipText = "Save a session (all open tabs)";
      this.saveProjectToolStripMenuItem.Click += new System.EventHandler(this.saveProjectToolStripMenuItem_Click);
      // 
      // exportBookmarksToolStripMenuItem
      // 
      this.exportBookmarksToolStripMenuItem.Name = "exportBookmarksToolStripMenuItem";
      this.exportBookmarksToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.exportBookmarksToolStripMenuItem.Text = "Export bookmarks...";
      this.exportBookmarksToolStripMenuItem.ToolTipText = "Write a list of bookmarks and their comments to a CSV file";
      this.exportBookmarksToolStripMenuItem.Click += new System.EventHandler(this.exportBookmarksToolStripMenuItem_Click);
      // 
      // toolStripSeparator14
      // 
      this.toolStripSeparator14.Name = "toolStripSeparator14";
      this.toolStripSeparator14.Size = new System.Drawing.Size(175, 6);
      // 
      // lastUsedToolStripMenuItem
      // 
      this.lastUsedToolStripMenuItem.Name = "lastUsedToolStripMenuItem";
      this.lastUsedToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.lastUsedToolStripMenuItem.Text = "Last used";
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
      this.exitToolStripMenuItem.Text = "Exit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goToLineToolStripMenuItem,
            this.searchToolStripMenuItem,
            this.filterToolStripMenuItem,
            this.bookmarksToolStripMenuItem,
            this.toolStripSeparator2,
            this.encodingToolStripMenuItem,
            this.toolStripSeparator4,
            this.timeshiftToolStripMenuItem,
            this.timeshiftMenuTextBox,
            this.toolStripSeparator3,
            this.copyMarkedLinesIntoNewTabToolStripMenuItem});
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(96, 20);
      this.toolStripMenuItem2.Text = "View/Navigate";
      // 
      // goToLineToolStripMenuItem
      // 
      this.goToLineToolStripMenuItem.Name = "goToLineToolStripMenuItem";
      this.goToLineToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
      this.goToLineToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
      this.goToLineToolStripMenuItem.Text = "Go to line...";
      this.goToLineToolStripMenuItem.Click += new System.EventHandler(this.goToLineToolStripMenuItem_Click);
      // 
      // searchToolStripMenuItem
      // 
      this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
      this.searchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
      this.searchToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
      this.searchToolStripMenuItem.Text = "Search...";
      this.searchToolStripMenuItem.Click += new System.EventHandler(this.searchToolStripMenuItem_Click);
      // 
      // filterToolStripMenuItem
      // 
      this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
      this.filterToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
      this.filterToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
      this.filterToolStripMenuItem.Text = "Filter";
      this.filterToolStripMenuItem.Click += new System.EventHandler(this.filterToolStripMenuItem_Click);
      // 
      // bookmarksToolStripMenuItem
      // 
      this.bookmarksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleBookmarkToolStripMenuItem,
            this.jumpToNextToolStripMenuItem,
            this.jumpToPrevToolStripMenuItem,
            this.showBookmarkListToolStripMenuItem});
      this.bookmarksToolStripMenuItem.Name = "bookmarksToolStripMenuItem";
      this.bookmarksToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
      this.bookmarksToolStripMenuItem.Text = "Bookmarks";
      // 
      // toggleBookmarkToolStripMenuItem
      // 
      this.toggleBookmarkToolStripMenuItem.Name = "toggleBookmarkToolStripMenuItem";
      this.toggleBookmarkToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
      this.toggleBookmarkToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
      this.toggleBookmarkToolStripMenuItem.Text = "Toggle Bookmark";
      this.toggleBookmarkToolStripMenuItem.Click += new System.EventHandler(this.toggleBookmarkToolStripMenuItem_Click);
      // 
      // jumpToNextToolStripMenuItem
      // 
      this.jumpToNextToolStripMenuItem.Name = "jumpToNextToolStripMenuItem";
      this.jumpToNextToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
      this.jumpToNextToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
      this.jumpToNextToolStripMenuItem.Text = "Jump to next";
      this.jumpToNextToolStripMenuItem.Click += new System.EventHandler(this.jumpToNextToolStripMenuItem_Click);
      // 
      // jumpToPrevToolStripMenuItem
      // 
      this.jumpToPrevToolStripMenuItem.Name = "jumpToPrevToolStripMenuItem";
      this.jumpToPrevToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F2)));
      this.jumpToPrevToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
      this.jumpToPrevToolStripMenuItem.Text = "Jump to prev";
      this.jumpToPrevToolStripMenuItem.Click += new System.EventHandler(this.jumpToPrevToolStripMenuItem_Click);
      // 
      // showBookmarkListToolStripMenuItem
      // 
      this.showBookmarkListToolStripMenuItem.Name = "showBookmarkListToolStripMenuItem";
      this.showBookmarkListToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
      this.showBookmarkListToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
      this.showBookmarkListToolStripMenuItem.Text = "Bookmark list";
      this.showBookmarkListToolStripMenuItem.Click += new System.EventHandler(this.showBookmarkListToolStripMenuItem_Click);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(179, 6);
      // 
      // encodingToolStripMenuItem
      // 
      this.encodingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aSCIIToolStripMenuItem,
            this.aNSIToolStripMenuItem,
            this.iSO88591ToolStripMenuItem,
            this.uTF8ToolStripMenuItem,
            this.uTF16ToolStripMenuItem});
      this.encodingToolStripMenuItem.Name = "encodingToolStripMenuItem";
      this.encodingToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
      this.encodingToolStripMenuItem.Text = "Encoding";
      // 
      // aSCIIToolStripMenuItem
      // 
      this.aSCIIToolStripMenuItem.Name = "aSCIIToolStripMenuItem";
      this.aSCIIToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
      this.aSCIIToolStripMenuItem.Tag = "";
      this.aSCIIToolStripMenuItem.Text = "ASCII";
      this.aSCIIToolStripMenuItem.Click += new System.EventHandler(this.aSCIIToolStripMenuItem_Click);
      // 
      // aNSIToolStripMenuItem
      // 
      this.aNSIToolStripMenuItem.Name = "aNSIToolStripMenuItem";
      this.aNSIToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
      this.aNSIToolStripMenuItem.Tag = "";
      this.aNSIToolStripMenuItem.Text = "ANSI";
      this.aNSIToolStripMenuItem.Click += new System.EventHandler(this.aNSIToolStripMenuItem_Click);
      // 
      // iSO88591ToolStripMenuItem
      // 
      this.iSO88591ToolStripMenuItem.Name = "iSO88591ToolStripMenuItem";
      this.iSO88591ToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
      this.iSO88591ToolStripMenuItem.Text = "ISO-8859-1";
      this.iSO88591ToolStripMenuItem.Click += new System.EventHandler(this.iSO88591ToolStripMenuItem_Click);
      // 
      // uTF8ToolStripMenuItem
      // 
      this.uTF8ToolStripMenuItem.Name = "uTF8ToolStripMenuItem";
      this.uTF8ToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
      this.uTF8ToolStripMenuItem.Text = "UTF8";
      this.uTF8ToolStripMenuItem.Click += new System.EventHandler(this.uTF8ToolStripMenuItem_Click);
      // 
      // uTF16ToolStripMenuItem
      // 
      this.uTF16ToolStripMenuItem.Name = "uTF16ToolStripMenuItem";
      this.uTF16ToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
      this.uTF16ToolStripMenuItem.Text = "Unicode";
      this.uTF16ToolStripMenuItem.Click += new System.EventHandler(this.uTF16ToolStripMenuItem_Click);
      // 
      // toolStripSeparator4
      // 
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      this.toolStripSeparator4.Size = new System.Drawing.Size(179, 6);
      // 
      // timeshiftToolStripMenuItem
      // 
      this.timeshiftToolStripMenuItem.CheckOnClick = true;
      this.timeshiftToolStripMenuItem.Name = "timeshiftToolStripMenuItem";
      this.timeshiftToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
      this.timeshiftToolStripMenuItem.Text = "Timeshift";
      this.timeshiftToolStripMenuItem.ToolTipText = "If supported by the columnizer, you can set an offset to the displayed log time";
      this.timeshiftToolStripMenuItem.CheckStateChanged += new System.EventHandler(this.timeshiftToolStripMenuItem_CheckStateChanged);
      // 
      // timeshiftMenuTextBox
      // 
      this.timeshiftMenuTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.timeshiftMenuTextBox.Enabled = false;
      this.timeshiftMenuTextBox.Name = "timeshiftMenuTextBox";
      this.timeshiftMenuTextBox.Size = new System.Drawing.Size(100, 23);
      this.timeshiftMenuTextBox.Text = "+00:00:00.000";
      this.timeshiftMenuTextBox.ToolTipText = "Time offset (hh:mm:ss.fff)";
      this.timeshiftMenuTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.timeshiftMenuTextBox_KeyDown);
      // 
      // toolStripSeparator3
      // 
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
      // 
      // copyMarkedLinesIntoNewTabToolStripMenuItem
      // 
      this.copyMarkedLinesIntoNewTabToolStripMenuItem.Name = "copyMarkedLinesIntoNewTabToolStripMenuItem";
      this.copyMarkedLinesIntoNewTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
      this.copyMarkedLinesIntoNewTabToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
      this.copyMarkedLinesIntoNewTabToolStripMenuItem.Text = "Copy to Tab";
      this.copyMarkedLinesIntoNewTabToolStripMenuItem.ToolTipText = "Copies all selected lines into a new tab page";
      this.copyMarkedLinesIntoNewTabToolStripMenuItem.Click += new System.EventHandler(this.copyMarkedLinesIntoNewTabToolStripMenuItem_Click);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showHelpToolStripMenuItem,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.helpToolStripMenuItem.Text = "Help";
      // 
      // showHelpToolStripMenuItem
      // 
      this.showHelpToolStripMenuItem.Name = "showHelpToolStripMenuItem";
      this.showHelpToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
      this.showHelpToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.showHelpToolStripMenuItem.Text = "Show help";
      this.showHelpToolStripMenuItem.Click += new System.EventHandler(this.showHelpToolStripMenuItem_Click);
      // 
      // toolStripSeparator5
      // 
      this.toolStripSeparator5.Name = "toolStripSeparator5";
      this.toolStripSeparator5.Size = new System.Drawing.Size(145, 6);
      // 
      // aboutToolStripMenuItem
      // 
      this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      this.aboutToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
      this.aboutToolStripMenuItem.Text = "About";
      this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
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
            this.hideLineColumnToolStripMenuItem});
      this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
      this.optionToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
      this.optionToolStripMenuItem.Text = "Options";
      // 
      // columnizerToolStripMenuItem
      // 
      this.columnizerToolStripMenuItem.Name = "columnizerToolStripMenuItem";
      this.columnizerToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.columnizerToolStripMenuItem.Text = "Columnizer...";
      this.columnizerToolStripMenuItem.ToolTipText = "Splits various kinds of logfiles into fixed columns";
      this.columnizerToolStripMenuItem.Click += new System.EventHandler(this.selectFilterToolStripMenuItem_Click);
      // 
      // hilightingToolStripMenuItem1
      // 
      this.hilightingToolStripMenuItem1.Name = "hilightingToolStripMenuItem1";
      this.hilightingToolStripMenuItem1.Size = new System.Drawing.Size(202, 22);
      this.hilightingToolStripMenuItem1.Text = "Hilighting and triggers...";
      this.hilightingToolStripMenuItem1.Click += new System.EventHandler(this.hilightingToolStripMenuItem_Click);
      // 
      // settingsToolStripMenuItem
      // 
      this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
      this.settingsToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.settingsToolStripMenuItem.Text = "Settings...";
      this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
      // 
      // toolStripSeparator6
      // 
      this.toolStripSeparator6.Name = "toolStripSeparator6";
      this.toolStripSeparator6.Size = new System.Drawing.Size(199, 6);
      // 
      // cellSelectModeToolStripMenuItem
      // 
      this.cellSelectModeToolStripMenuItem.CheckOnClick = true;
      this.cellSelectModeToolStripMenuItem.Name = "cellSelectModeToolStripMenuItem";
      this.cellSelectModeToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.cellSelectModeToolStripMenuItem.Text = "Cell select mode";
      this.cellSelectModeToolStripMenuItem.ToolTipText = "Switches between foll row selection and single cell selection mode";
      this.cellSelectModeToolStripMenuItem.Click += new System.EventHandler(this.cellSelectModeToolStripMenuItem_Click);
      // 
      // alwaysOnTopToolStripMenuItem
      // 
      this.alwaysOnTopToolStripMenuItem.CheckOnClick = true;
      this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
      this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.alwaysOnTopToolStripMenuItem.Text = "Always on top";
      this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
      // 
      // hideLineColumnToolStripMenuItem
      // 
      this.hideLineColumnToolStripMenuItem.CheckOnClick = true;
      this.hideLineColumnToolStripMenuItem.Name = "hideLineColumnToolStripMenuItem";
      this.hideLineColumnToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.hideLineColumnToolStripMenuItem.Text = "Hide line column";
      this.hideLineColumnToolStripMenuItem.Click += new System.EventHandler(this.hideLineColumnToolStripMenuItem_Click);
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
            this.throwExceptionbackgroundThreadToolStripMenuItem,
            this.toolStripSeparator12,
            this.loglevelToolStripMenuItem,
            this.disableWordHighlightModeToolStripMenuItem});
      this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
      this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
      this.debugToolStripMenuItem.Text = "Debug";
      // 
      // dumpLogBufferInfoToolStripMenuItem
      // 
      this.dumpLogBufferInfoToolStripMenuItem.Name = "dumpLogBufferInfoToolStripMenuItem";
      this.dumpLogBufferInfoToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.dumpLogBufferInfoToolStripMenuItem.Text = "Dump LogBuffer info";
      this.dumpLogBufferInfoToolStripMenuItem.Click += new System.EventHandler(this.dumpLogBufferInfoToolStripMenuItem_Click);
      // 
      // dumpBufferDiagnosticToolStripMenuItem
      // 
      this.dumpBufferDiagnosticToolStripMenuItem.Name = "dumpBufferDiagnosticToolStripMenuItem";
      this.dumpBufferDiagnosticToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.dumpBufferDiagnosticToolStripMenuItem.Text = "Dump buffer diagnostic";
      this.dumpBufferDiagnosticToolStripMenuItem.Click += new System.EventHandler(this.dumpBufferDiagnosticToolStripMenuItem_Click);
      // 
      // runGCToolStripMenuItem
      // 
      this.runGCToolStripMenuItem.Name = "runGCToolStripMenuItem";
      this.runGCToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.runGCToolStripMenuItem.Text = "Run GC";
      this.runGCToolStripMenuItem.Click += new System.EventHandler(this.runGCToolStripMenuItem_Click);
      // 
      // gCInfoToolStripMenuItem
      // 
      this.gCInfoToolStripMenuItem.Name = "gCInfoToolStripMenuItem";
      this.gCInfoToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.gCInfoToolStripMenuItem.Text = "Dump GC info";
      this.gCInfoToolStripMenuItem.Click += new System.EventHandler(this.gCInfoToolStripMenuItem_Click);
      // 
      // toolStripSeparator18
      // 
      this.toolStripSeparator18.Name = "toolStripSeparator18";
      this.toolStripSeparator18.Size = new System.Drawing.Size(271, 6);
      // 
      // throwExceptionGUIThreadToolStripMenuItem
      // 
      this.throwExceptionGUIThreadToolStripMenuItem.Name = "throwExceptionGUIThreadToolStripMenuItem";
      this.throwExceptionGUIThreadToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.throwExceptionGUIThreadToolStripMenuItem.Text = "Throw exception (GUI Thread)";
      this.throwExceptionGUIThreadToolStripMenuItem.Click += new System.EventHandler(this.throwExceptionGUIThreadToolStripMenuItem_Click);
      // 
      // throwExceptionbackgroundThToolStripMenuItem
      // 
      this.throwExceptionbackgroundThToolStripMenuItem.Name = "throwExceptionbackgroundThToolStripMenuItem";
      this.throwExceptionbackgroundThToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.throwExceptionbackgroundThToolStripMenuItem.Text = "Throw exception (Async delegate)";
      this.throwExceptionbackgroundThToolStripMenuItem.Click += new System.EventHandler(this.throwExceptionbackgroundThToolStripMenuItem_Click);
      // 
      // throwExceptionbackgroundThreadToolStripMenuItem
      // 
      this.throwExceptionbackgroundThreadToolStripMenuItem.Name = "throwExceptionbackgroundThreadToolStripMenuItem";
      this.throwExceptionbackgroundThreadToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.throwExceptionbackgroundThreadToolStripMenuItem.Text = "Throw exception (background thread)";
      this.throwExceptionbackgroundThreadToolStripMenuItem.Click += new System.EventHandler(this.throwExceptionbackgroundThreadToolStripMenuItem_Click);
      // 
      // toolStripSeparator12
      // 
      this.toolStripSeparator12.Name = "toolStripSeparator12";
      this.toolStripSeparator12.Size = new System.Drawing.Size(271, 6);
      // 
      // loglevelToolStripMenuItem
      // 
      this.loglevelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.warnToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.debugToolStripMenuItem1});
      this.loglevelToolStripMenuItem.Name = "loglevelToolStripMenuItem";
      this.loglevelToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.loglevelToolStripMenuItem.Text = "Loglevel";
      this.loglevelToolStripMenuItem.DropDownOpening += new System.EventHandler(this.loglevelToolStripMenuItem_DropDownOpening);
      this.loglevelToolStripMenuItem.Click += new System.EventHandler(this.loglevelToolStripMenuItem_Click);
      // 
      // warnToolStripMenuItem
      // 
      this.warnToolStripMenuItem.Name = "warnToolStripMenuItem";
      this.warnToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
      this.warnToolStripMenuItem.Text = "Warn";
      this.warnToolStripMenuItem.Click += new System.EventHandler(this.warnToolStripMenuItem_Click);
      // 
      // infoToolStripMenuItem
      // 
      this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
      this.infoToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
      this.infoToolStripMenuItem.Text = "Info";
      this.infoToolStripMenuItem.Click += new System.EventHandler(this.infoToolStripMenuItem_Click);
      // 
      // debugToolStripMenuItem1
      // 
      this.debugToolStripMenuItem1.Name = "debugToolStripMenuItem1";
      this.debugToolStripMenuItem1.Size = new System.Drawing.Size(109, 22);
      this.debugToolStripMenuItem1.Text = "Debug";
      this.debugToolStripMenuItem1.Click += new System.EventHandler(this.debugToolStripMenuItem1_Click);
      // 
      // disableWordHighlightModeToolStripMenuItem
      // 
      this.disableWordHighlightModeToolStripMenuItem.CheckOnClick = true;
      this.disableWordHighlightModeToolStripMenuItem.Name = "disableWordHighlightModeToolStripMenuItem";
      this.disableWordHighlightModeToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
      this.disableWordHighlightModeToolStripMenuItem.Text = "Disable word highlight mode";
      this.disableWordHighlightModeToolStripMenuItem.Click += new System.EventHandler(this.disableWordHighlightModeToolStripMenuItem_Click);
      // 
      // toolsToolStripMenuItem
      // 
      this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configureToolStripMenuItem,
            this.configureToolStripSeparator});
      this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
      this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
      this.toolsToolStripMenuItem.Text = "Tools";
      this.toolsToolStripMenuItem.ToolTipText = "Launch external tools (configure in the settings)";
      this.toolsToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolsToolStripMenuItem_DropDownItemClicked);
      // 
      // configureToolStripMenuItem
      // 
      this.configureToolStripMenuItem.Name = "configureToolStripMenuItem";
      this.configureToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
      this.configureToolStripMenuItem.Text = "Configure...";
      this.configureToolStripMenuItem.Click += new System.EventHandler(this.configureToolStripMenuItem_Click);
      // 
      // configureToolStripSeparator
      // 
      this.configureToolStripSeparator.Name = "configureToolStripSeparator";
      this.configureToolStripSeparator.Size = new System.Drawing.Size(133, 6);
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
      // toolStripContainer1
      // 
      // 
      // toolStripContainer1.BottomToolStripPanel
      // 
      this.toolStripContainer1.BottomToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
      this.toolStripContainer1.BottomToolStripPanelVisible = false;
      // 
      // toolStripContainer1.ContentPanel
      // 
      this.toolStripContainer1.ContentPanel.Controls.Add(this.tabControl1);
      this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
      this.toolStripContainer1.ContentPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
      this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(962, 456);
      this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      // 
      // toolStripContainer1.LeftToolStripPanel
      // 
      this.toolStripContainer1.LeftToolStripPanel.Enabled = false;
      this.toolStripContainer1.LeftToolStripPanelVisible = false;
      this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
      this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(0);
      this.toolStripContainer1.Name = "toolStripContainer1";
      // 
      // toolStripContainer1.RightToolStripPanel
      // 
      this.toolStripContainer1.RightToolStripPanel.Enabled = false;
      this.toolStripContainer1.RightToolStripPanelVisible = false;
      this.toolStripContainer1.Size = new System.Drawing.Size(962, 505);
      this.toolStripContainer1.TabIndex = 13;
      this.toolStripContainer1.Text = "toolStripContainer1";
      // 
      // toolStripContainer1.TopToolStripPanel
      // 
      this.toolStripContainer1.TopToolStripPanel.BackColor = System.Drawing.SystemColors.ControlLight;
      this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
      this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.externalToolsToolStrip);
      this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip4);
      // 
      // tabControl1
      // 
      this.tabControl1.Alignment = MdiTabControl.TabControl.TabAlignment.Top;
      this.tabControl1.CloseButtonVisible = true;
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
      this.tabControl1.MenuRenderer = null;
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.Size = new System.Drawing.Size(962, 456);
      this.tabControl1.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
      this.tabControl1.TabBorderEnhanceWeight = MdiTabControl.TabControl.Weight.Strong;
      this.tabControl1.TabCloseButtonImage = null;
      this.tabControl1.TabCloseButtonImageDisabled = null;
      this.tabControl1.TabCloseButtonImageHot = null;
      this.tabControl1.TabCloseButtonSize = new System.Drawing.Size(14, 14);
      this.tabControl1.TabHeight = 21;
      this.tabControl1.TabIconSize = new System.Drawing.Size(14, 14);
      this.tabControl1.TabIndex = 13;
      this.tabControl1.TabOffset = 0;
      this.tabControl1.TabPadLeft = 4;
      this.tabControl1.TabPadRight = 4;
      this.tabControl1.TabsDirection = MdiTabControl.TabControl.FlowDirection.LeftToRight;
      this.tabControl1.TabPageChanged += new MdiTabControl.TabControl.TabPageChangedEventHandler(this.tabControl1_TabPageChanged);
      // 
      // toolStrip4
      // 
      this.toolStrip4.BackColor = System.Drawing.SystemColors.ControlLight;
      this.toolStrip4.Dock = System.Windows.Forms.DockStyle.None;
      this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
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
            this.highlightGroupsComboBox});
      this.toolStrip4.Location = new System.Drawing.Point(3, 24);
      this.toolStrip4.Name = "toolStrip4";
      this.toolStrip4.Size = new System.Drawing.Size(382, 25);
      this.toolStrip4.TabIndex = 7;
      // 
      // toolStripButtonOpen
      // 
      this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonOpen.Image = global::LogExpert.Properties.Resources.folder_blue;
      this.toolStripButtonOpen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonOpen.Name = "toolStripButtonOpen";
      this.toolStripButtonOpen.Size = new System.Drawing.Size(23, 22);
      this.toolStripButtonOpen.Text = "toolStripButton1";
      this.toolStripButtonOpen.ToolTipText = "Open file";
      this.toolStripButtonOpen.Click += new System.EventHandler(this.toolStripButtonOpen_Click);
      // 
      // toolStripSeparator10
      // 
      this.toolStripSeparator10.Name = "toolStripSeparator10";
      this.toolStripSeparator10.Size = new System.Drawing.Size(6, 25);
      // 
      // toolStripButtonSearch
      // 
      this.toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonSearch.Image = global::LogExpert.Properties.Resources.search_icon_blue;
      this.toolStripButtonSearch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.toolStripButtonSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonSearch.Name = "toolStripButtonSearch";
      this.toolStripButtonSearch.Size = new System.Drawing.Size(23, 22);
      this.toolStripButtonSearch.Text = "toolStripButtonSearch";
      this.toolStripButtonSearch.ToolTipText = "Search";
      this.toolStripButtonSearch.Click += new System.EventHandler(this.toolStripButtonSearch_Click);
      // 
      // toolStripButtonFilter
      // 
      this.toolStripButtonFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonFilter.Image = global::LogExpert.Properties.Resources.search_folder_blue;
      this.toolStripButtonFilter.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.toolStripButtonFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonFilter.Name = "toolStripButtonFilter";
      this.toolStripButtonFilter.Size = new System.Drawing.Size(23, 22);
      this.toolStripButtonFilter.Text = "toolStripButton1";
      this.toolStripButtonFilter.ToolTipText = "Filter window";
      this.toolStripButtonFilter.Click += new System.EventHandler(this.toolStripButtonFilter_Click);
      // 
      // toolStripSeparator11
      // 
      this.toolStripSeparator11.Name = "toolStripSeparator11";
      this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
      // 
      // toolStripButtonBookmark
      // 
      this.toolStripButtonBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonBookmark.Image = global::LogExpert.Properties.Resources.check_blue;
      this.toolStripButtonBookmark.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.toolStripButtonBookmark.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonBookmark.Name = "toolStripButtonBookmark";
      this.toolStripButtonBookmark.Size = new System.Drawing.Size(23, 22);
      this.toolStripButtonBookmark.Text = "toolStripButton1";
      this.toolStripButtonBookmark.ToolTipText = "Toggle bookmark";
      this.toolStripButtonBookmark.Click += new System.EventHandler(this.toolStripButtonBookmark_Click);
      // 
      // toolStripButtonUp
      // 
      this.toolStripButtonUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonUp.Image = global::LogExpert.Properties.Resources.up_blue;
      this.toolStripButtonUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.toolStripButtonUp.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonUp.Name = "toolStripButtonUp";
      this.toolStripButtonUp.Size = new System.Drawing.Size(23, 22);
      this.toolStripButtonUp.Text = "toolStripButton1";
      this.toolStripButtonUp.ToolTipText = "Go to previous bookmark";
      this.toolStripButtonUp.Click += new System.EventHandler(this.toolStripButtonUp_Click);
      // 
      // toolStripButtonDown
      // 
      this.toolStripButtonDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonDown.Image = global::LogExpert.Properties.Resources.down_blue;
      this.toolStripButtonDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.toolStripButtonDown.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonDown.Name = "toolStripButtonDown";
      this.toolStripButtonDown.Size = new System.Drawing.Size(23, 22);
      this.toolStripButtonDown.Text = "toolStripButton1";
      this.toolStripButtonDown.ToolTipText = "Go to next bookmark";
      this.toolStripButtonDown.Click += new System.EventHandler(this.toolStripButtonDown_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
      // 
      // toolStripButtonBubbles
      // 
      this.toolStripButtonBubbles.CheckOnClick = true;
      this.toolStripButtonBubbles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonBubbles.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonBubbles.Image")));
      this.toolStripButtonBubbles.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.toolStripButtonBubbles.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonBubbles.Name = "toolStripButtonBubbles";
      this.toolStripButtonBubbles.Size = new System.Drawing.Size(23, 22);
      this.toolStripButtonBubbles.Text = "Show bookmark bubbles";
      this.toolStripButtonBubbles.Click += new System.EventHandler(this.toolStripButtonBubbles_Click);
      // 
      // toolStripSeparator15
      // 
      this.toolStripSeparator15.Name = "toolStripSeparator15";
      this.toolStripSeparator15.Size = new System.Drawing.Size(6, 25);
      // 
      // toolStripButtonTail
      // 
      this.toolStripButtonTail.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripButtonTail.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonTail.Image")));
      this.toolStripButtonTail.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonTail.Name = "toolStripButtonTail";
      this.toolStripButtonTail.Size = new System.Drawing.Size(27, 22);
      this.toolStripButtonTail.Text = "tail";
      // 
      // toolStripSeparator17
      // 
      this.toolStripSeparator17.Name = "toolStripSeparator17";
      this.toolStripSeparator17.Size = new System.Drawing.Size(6, 25);
      // 
      // highlightGroupsComboBox
      // 
      this.highlightGroupsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.highlightGroupsComboBox.DropDownWidth = 250;
      this.highlightGroupsComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
      this.highlightGroupsComboBox.Name = "highlightGroupsComboBox";
      this.highlightGroupsComboBox.Size = new System.Drawing.Size(150, 25);
      this.highlightGroupsComboBox.ToolTipText = "Select the current highlight settings for the log file (right-click to open highl" +
          "ight settings)";
      this.highlightGroupsComboBox.SelectedIndexChanged += new System.EventHandler(this.highlightGroupsComboBox_SelectedIndexChanged);
      this.highlightGroupsComboBox.DropDownClosed += new System.EventHandler(this.highlightGroupsComboBox_DropDownClosed);
      this.highlightGroupsComboBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.highlightGroupsComboBox_MouseUp);
      // 
      // externalToolsToolStrip
      // 
      this.externalToolsToolStrip.BackColor = System.Drawing.SystemColors.ControlLight;
      this.externalToolsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
      this.externalToolsToolStrip.Location = new System.Drawing.Point(390, 24);
      this.externalToolsToolStrip.Name = "externalToolsToolStrip";
      this.externalToolsToolStrip.Size = new System.Drawing.Size(43, 25);
      this.externalToolsToolStrip.TabIndex = 8;
      this.externalToolsToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.externalToolsToolStrip_ItemClicked);
      // 
      // toolStripSeparator9
      // 
      this.toolStripSeparator9.Name = "toolStripSeparator9";
      this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
      // 
      // followTailCheckBox
      // 
      this.followTailCheckBox.AutoSize = true;
      this.followTailCheckBox.Location = new System.Drawing.Point(397, 512);
      this.followTailCheckBox.Name = "followTailCheckBox";
      this.followTailCheckBox.Size = new System.Drawing.Size(72, 17);
      this.followTailCheckBox.TabIndex = 14;
      this.followTailCheckBox.Text = "Follow tail";
      this.followTailCheckBox.UseVisualStyleBackColor = true;
      this.followTailCheckBox.Click += new System.EventHandler(this.followTailCheckBox_Click);
      // 
      // tabContextMenuStrip
      // 
      this.tabContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeThisTabToolStripMenuItem,
            this.closeOtherTabsToolStripMenuItem,
            this.closeAllTabsToolStripMenuItem,
            this.toolStripSeparator13,
            this.tabColorToolStripMenuItem,
            this.toolStripSeparator16,
            this.copyPathToClipboardToolStripMenuItem,
            this.findInExplorerToolStripMenuItem});
      this.tabContextMenuStrip.Name = "tabContextMenuStrip";
      this.tabContextMenuStrip.Size = new System.Drawing.Size(197, 148);
      // 
      // closeThisTabToolStripMenuItem
      // 
      this.closeThisTabToolStripMenuItem.Name = "closeThisTabToolStripMenuItem";
      this.closeThisTabToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
      this.closeThisTabToolStripMenuItem.Text = "Close this tab";
      this.closeThisTabToolStripMenuItem.Click += new System.EventHandler(this.closeThisTabToolStripMenuItem_Click);
      // 
      // closeOtherTabsToolStripMenuItem
      // 
      this.closeOtherTabsToolStripMenuItem.Name = "closeOtherTabsToolStripMenuItem";
      this.closeOtherTabsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
      this.closeOtherTabsToolStripMenuItem.Text = "Close other tabs";
      this.closeOtherTabsToolStripMenuItem.Click += new System.EventHandler(this.closeOtherTabsToolStripMenuItem_Click);
      // 
      // closeAllTabsToolStripMenuItem
      // 
      this.closeAllTabsToolStripMenuItem.Name = "closeAllTabsToolStripMenuItem";
      this.closeAllTabsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
      this.closeAllTabsToolStripMenuItem.Text = "Close all tabs";
      this.closeAllTabsToolStripMenuItem.Click += new System.EventHandler(this.closeAllTabsToolStripMenuItem_Click);
      // 
      // toolStripSeparator13
      // 
      this.toolStripSeparator13.Name = "toolStripSeparator13";
      this.toolStripSeparator13.Size = new System.Drawing.Size(193, 6);
      // 
      // tabColorToolStripMenuItem
      // 
      this.tabColorToolStripMenuItem.Name = "tabColorToolStripMenuItem";
      this.tabColorToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
      this.tabColorToolStripMenuItem.Text = "Tab color...";
      this.tabColorToolStripMenuItem.Click += new System.EventHandler(this.tabColorToolStripMenuItem_Click);
      // 
      // toolStripSeparator16
      // 
      this.toolStripSeparator16.Name = "toolStripSeparator16";
      this.toolStripSeparator16.Size = new System.Drawing.Size(193, 6);
      // 
      // copyPathToClipboardToolStripMenuItem
      // 
      this.copyPathToClipboardToolStripMenuItem.Name = "copyPathToClipboardToolStripMenuItem";
      this.copyPathToClipboardToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
      this.copyPathToClipboardToolStripMenuItem.Text = "Copy path to clipboard";
      this.copyPathToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyPathToClipboardToolStripMenuItem_Click);
      // 
      // findInExplorerToolStripMenuItem
      // 
      this.findInExplorerToolStripMenuItem.Name = "findInExplorerToolStripMenuItem";
      this.findInExplorerToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
      this.findInExplorerToolStripMenuItem.Text = "Find in Explorer";
      this.findInExplorerToolStripMenuItem.Click += new System.EventHandler(this.findInExplorerToolStripMenuItem_Click);
      // 
      // dateTimeDragControl
      // 
      this.dateTimeDragControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.dateTimeDragControl.BackColor = System.Drawing.SystemColors.Control;
      this.dateTimeDragControl.DateTime = new System.DateTime(((long)(0)));
      this.dateTimeDragControl.DragOrientation = LogExpert.Dialogs.DateTimeDragControl.DragOrientations.Vertical;
      this.dateTimeDragControl.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.dateTimeDragControl.HoverColor = System.Drawing.Color.LightGray;
      this.dateTimeDragControl.Location = new System.Drawing.Point(611, 507);
      this.dateTimeDragControl.MaxDateTime = new System.DateTime(9999, 12, 31, 23, 59, 59, 999);
      this.dateTimeDragControl.MinDateTime = new System.DateTime(((long)(0)));
      this.dateTimeDragControl.Name = "dateTimeDragControl";
      this.dateTimeDragControl.Size = new System.Drawing.Size(188, 20);
      this.dateTimeDragControl.TabIndex = 14;
      this.dateTimeDragControl.ValueDragged += new LogExpert.Dialogs.DateTimeDragControl.ValueDraggedEventHandler(this.dateTimeDragControl_ValueDragged);
      this.dateTimeDragControl.ValueChanged += new LogExpert.Dialogs.DateTimeDragControl.ValueChangedEventHandler(this.dateTimeDragControl_ValueChanged);
      // 
      // LogTabWindow
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(962, 529);
      this.Controls.Add(this.followTailCheckBox);
      this.Controls.Add(this.dateTimeDragControl);
      this.Controls.Add(this.toolStripContainer1);
      this.Controls.Add(this.statusStrip1);
      this.DoubleBuffered = true;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "LogTabWindow";
      this.Text = "LogExpert";
      this.Deactivate += new System.EventHandler(this.LogTabWindow_Deactivate);
      this.SizeChanged += new System.EventHandler(this.LogTabWindow_SizeChanged);
      this.Activated += new System.EventHandler(this.LogTabWindow_Activated);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.LogWindow_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.LogTabWindow_DragEnter);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LogTabWindow_KeyDown);
      this.DragOver += new System.Windows.Forms.DragEventHandler(this.LogWindow_DragOver);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.toolStripContainer1.ContentPanel.ResumeLayout(false);
      this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
      this.toolStripContainer1.TopToolStripPanel.PerformLayout();
      this.toolStripContainer1.ResumeLayout(false);
      this.toolStripContainer1.PerformLayout();
      this.toolStrip4.ResumeLayout(false);
      this.toolStrip4.PerformLayout();
      this.tabContextMenuStrip.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel linesLabel;
    private System.Windows.Forms.ToolStripStatusLabel sizeLabel;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    private System.Windows.Forms.ToolStripStatusLabel currentLineLabel;
    private System.Windows.Forms.ToolStripProgressBar loadProgessBar;
    private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripContainer toolStripContainer1;
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
    private MdiTabControl.TabControl tabControl1;
    private System.Windows.Forms.ToolStripMenuItem bookmarksToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toggleBookmarkToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem jumpToNextToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem jumpToPrevToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem encodingToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aSCIIToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aNSIToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem uTF8ToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem uTF16ToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    private System.Windows.Forms.ToolStripMenuItem reloadToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem columnizerToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
    private DateTimeDragControl dateTimeDragControl;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
    private System.Windows.Forms.ToolStripMenuItem showBookmarkListToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
    private System.Windows.Forms.ToolStrip toolStrip4;
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
    private CheckBox followTailCheckBox;
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
    private ToolStripComboBox highlightGroupsComboBox;
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
    private ToolStripMenuItem throwExceptionbackgroundThreadToolStripMenuItem;
    private ToolStripMenuItem loglevelToolStripMenuItem;
    private ToolStripMenuItem warnToolStripMenuItem;
    private ToolStripMenuItem infoToolStripMenuItem;
    private ToolStripMenuItem debugToolStripMenuItem1;
    private ToolStripSeparator toolStripSeparator12;
    private ToolStripSeparator toolStripSeparator18;
    private ToolStripMenuItem disableWordHighlightModeToolStripMenuItem;
    private ToolStripMenuItem multifileMaskToolStripMenuItem;
    private ToolStripMenuItem multiFileEnabledStripMenuItem;
    private ToolStripMenuItem iSO88591ToolStripMenuItem;


  }
}

