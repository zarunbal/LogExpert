using LogExpert.Dialogs;

namespace LogExpert.Controls.LogWindow
{
	partial class LogWindow
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogWindow));
            this.splitContainerLogWindow = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.columnFinderPanel = new System.Windows.Forms.Panel();
            this.columnComboBox = new System.Windows.Forms.ComboBox();
            this.lblColumnName = new System.Windows.Forms.Label();
            this.dataGridView = new LogExpert.Dialogs.BufferedDataGridView();
            this.dataGridContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.scrollAllTabsToTimestampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncTimestampsToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.freeThisWindowFromTimeSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.locateLineInOriginalFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleBoomarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarkCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.markEditModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tempHighlightsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makePermanentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markCurrentFilterRangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.timeSpreadingControl = new LogExpert.Dialogs.TimeSpreadingControl();
            this.advancedBackPanel = new System.Windows.Forms.Panel();
            this.advancedFilterSplitContainer = new System.Windows.Forms.SplitContainer();
            this.pnlProFilter = new System.Windows.Forms.Panel();
            this.columnButton = new System.Windows.Forms.Button();
            this.columnRestrictCheckBox = new System.Windows.Forms.CheckBox();
            this.rangeCheckBox = new System.Windows.Forms.CheckBox();
            this.filterRangeComboBox = new System.Windows.Forms.ComboBox();
            this.columnNamesLabel = new System.Windows.Forms.Label();
            this.fuzzyLabel = new System.Windows.Forms.Label();
            this.fuzzyKnobControl = new KnobControl();
            this.invertFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.pnlProFilterLabel = new System.Windows.Forms.Panel();
            this.lblBackSpread = new System.Windows.Forms.Label();
            this.filterKnobBackSpread = new KnobControl();
            this.lblForeSpread = new System.Windows.Forms.Label();
            this.filterKnobForeSpread = new KnobControl();
            this.btnFilterToTab = new System.Windows.Forms.Button();
            this.btnToggleHighlightPanel = new System.Windows.Forms.Button();
            this.highlightSplitContainer = new System.Windows.Forms.SplitContainer();
            this.filterGridView = new LogExpert.Dialogs.BufferedDataGridView();
            this.filterContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setBookmarksOnSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markFilterHitsInLogViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightSplitContainerBackPanel = new System.Windows.Forms.Panel();
            this.hideFilterListOnLoadCheckBox = new System.Windows.Forms.CheckBox();
            this.filterDownButton = new System.Windows.Forms.Button();
            this.filterUpButton = new System.Windows.Forms.Button();
            this.filterOnLoadCheckBox = new System.Windows.Forms.CheckBox();
            this.saveFilterButton = new System.Windows.Forms.Button();
            this.deleteFilterButton = new System.Windows.Forms.Button();
            this.filterListBox = new System.Windows.Forms.ListBox();
            this.filterListContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlFilterInput = new System.Windows.Forms.Panel();
            this.filterSplitContainer = new System.Windows.Forms.SplitContainer();
            this.lblTextFilter = new System.Windows.Forms.Label();
            this.filterComboBox = new System.Windows.Forms.ComboBox();
            this.advancedButton = new System.Windows.Forms.Button();
            this.syncFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.lblFilterCount = new System.Windows.Forms.Label();
            this.filterTailCheckBox = new System.Windows.Forms.CheckBox();
            this.filterRegexCheckBox = new System.Windows.Forms.CheckBox();
            this.filterCaseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            this.filterSearchButton = new System.Windows.Forms.Button();
            this.bookmarkContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteBookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.freezeLeftColumnsUntilHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.moveToLastColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.hideColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.allColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editModeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editModecopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightSelectionInLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightSelectionInLogFilewordModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterForSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setSelectedTextAsBookmarkCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLogWindow)).BeginInit();
            this.splitContainerLogWindow.Panel1.SuspendLayout();
            this.splitContainerLogWindow.Panel2.SuspendLayout();
            this.splitContainerLogWindow.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.columnFinderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.dataGridContextMenuStrip.SuspendLayout();
            this.advancedBackPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.advancedFilterSplitContainer)).BeginInit();
            this.advancedFilterSplitContainer.Panel1.SuspendLayout();
            this.advancedFilterSplitContainer.Panel2.SuspendLayout();
            this.advancedFilterSplitContainer.SuspendLayout();
            this.pnlProFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.highlightSplitContainer)).BeginInit();
            this.highlightSplitContainer.Panel1.SuspendLayout();
            this.highlightSplitContainer.Panel2.SuspendLayout();
            this.highlightSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterGridView)).BeginInit();
            this.filterContextMenuStrip.SuspendLayout();
            this.highlightSplitContainerBackPanel.SuspendLayout();
            this.filterListContextMenuStrip.SuspendLayout();
            this.pnlFilterInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterSplitContainer)).BeginInit();
            this.filterSplitContainer.Panel1.SuspendLayout();
            this.filterSplitContainer.Panel2.SuspendLayout();
            this.filterSplitContainer.SuspendLayout();
            this.bookmarkContextMenuStrip.SuspendLayout();
            this.columnContextMenuStrip.SuspendLayout();
            this.editModeContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerLogWindow
            // 
            this.splitContainerLogWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerLogWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLogWindow.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLogWindow.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerLogWindow.Name = "splitContainerLogWindow";
            this.splitContainerLogWindow.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLogWindow.Panel1
            // 
            this.splitContainerLogWindow.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainerLogWindow.Panel1MinSize = 50;
            // 
            // splitContainerLogWindow.Panel2
            // 
            this.splitContainerLogWindow.Panel2.Controls.Add(this.advancedBackPanel);
            this.splitContainerLogWindow.Panel2.Controls.Add(this.pnlFilterInput);
            this.splitContainerLogWindow.Panel2MinSize = 50;
            this.splitContainerLogWindow.Size = new System.Drawing.Size(1014, 656);
            this.splitContainerLogWindow.SplitterDistance = 364;
            this.splitContainerLogWindow.TabIndex = 9;
            this.splitContainerLogWindow.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.OnSplitContainerSplitterMoved);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 181F));
            this.tableLayoutPanel1.Controls.Add(this.columnFinderPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.timeSpreadingControl, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1012, 362);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // columnFinderPanel
            // 
            this.columnFinderPanel.Controls.Add(this.columnComboBox);
            this.columnFinderPanel.Controls.Add(this.lblColumnName);
            this.columnFinderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.columnFinderPanel.Location = new System.Drawing.Point(4, 4);
            this.columnFinderPanel.Name = "columnFinderPanel";
            this.columnFinderPanel.Size = new System.Drawing.Size(841, 22);
            this.columnFinderPanel.TabIndex = 2;
            // 
            // columnComboBox
            // 
            this.columnComboBox.FormattingEnabled = true;
            this.columnComboBox.Location = new System.Drawing.Point(88, 1);
            this.columnComboBox.MaxDropDownItems = 15;
            this.columnComboBox.Name = "columnComboBox";
            this.columnComboBox.Size = new System.Drawing.Size(181, 21);
            this.columnComboBox.TabIndex = 1;
            this.helpToolTip.SetToolTip(this.columnComboBox, "Select column to scroll to");
            this.columnComboBox.SelectionChangeCommitted += new System.EventHandler(this.OnColumnComboBoxSelectionChangeCommitted);
            this.columnComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnColumnComboBoxKeyDown);
            this.columnComboBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.OnColumnComboBoxPreviewKeyDown);
            // 
            // lblColumnName
            // 
            this.lblColumnName.AutoSize = true;
            this.lblColumnName.Location = new System.Drawing.Point(8, 4);
            this.lblColumnName.Name = "lblColumnName";
            this.lblColumnName.Size = new System.Drawing.Size(74, 13);
            this.lblColumnName.TabIndex = 0;
            this.lblColumnName.Text = "Column name:";
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToOrderColumns = true;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.ContextMenuStrip = this.dataGridContextMenuStrip;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView.EditModeMenuStrip = null;
            this.dataGridView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.dataGridView.Location = new System.Drawing.Point(1, 30);
            this.dataGridView.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.PaintWithOverlays = false;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.dataGridView.RowTemplate.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.dataGridView.RowTemplate.Height = 15;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.ShowCellErrors = false;
            this.dataGridView.ShowCellToolTips = false;
            this.dataGridView.ShowEditingIcon = false;
            this.dataGridView.ShowRowErrors = false;
            this.dataGridView.Size = new System.Drawing.Size(847, 331);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.VirtualMode = true;
            this.dataGridView.OverlayDoubleClicked += new LogExpert.Dialogs.BufferedDataGridView.OverlayDoubleClickedEventHandler(this.OnDataGridViewOverlayDoubleClicked);
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnDataGridViewCellClick);
            this.dataGridView.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnDataGridViewCellContentDoubleClick);
            this.dataGridView.CellContextMenuStripNeeded += new System.Windows.Forms.DataGridViewCellContextMenuStripNeededEventHandler(this.OnDataGridViewCellContextMenuStripNeeded);
            this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnDataGridViewCellDoubleClick);
            this.dataGridView.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.OnDataGridViewCellValuePushed);
            this.dataGridView.RowHeightInfoNeeded += new System.Windows.Forms.DataGridViewRowHeightInfoNeededEventHandler(this.OnDataGridViewRowHeightInfoNeeded);
            this.dataGridView.RowUnshared += new System.Windows.Forms.DataGridViewRowEventHandler(this.OnDataGridViewRowUnshared);
            this.dataGridView.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnDataGridViewScroll);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.OnDataGridViewSelectionChanged);
            this.dataGridView.Paint += new System.Windows.Forms.PaintEventHandler(this.OnDataGridViewPaint);
            this.dataGridView.Enter += new System.EventHandler(this.OnDataGridViewEnter);
            this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnDataGridViewKeyDown);
            this.dataGridView.Leave += new System.EventHandler(this.OnDataGridViewLeave);
            this.dataGridView.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.OnDataGridViewPreviewKeyDown);
            this.dataGridView.Resize += new System.EventHandler(this.OnDataGridViewResize);
            // 
            // dataGridContextMenuStrip
            // 
            this.dataGridContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.copyToTabToolStripMenuItem,
            this.toolStripSeparator1,
            this.scrollAllTabsToTimestampToolStripMenuItem,
            this.syncTimestampsToToolStripMenuItem,
            this.freeThisWindowFromTimeSyncToolStripMenuItem,
            this.locateLineInOriginalFileToolStripMenuItem,
            this.toolStripSeparator2,
            this.toggleBoomarkToolStripMenuItem,
            this.bookmarkCommentToolStripMenuItem,
            this.toolStripSeparator4,
            this.markEditModeToolStripMenuItem,
            this.tempHighlightsToolStripMenuItem,
            this.markCurrentFilterRangeToolStripMenuItem,
            this.pluginSeparator});
            this.dataGridContextMenuStrip.Name = "dataGridContextMenuStrip";
            this.dataGridContextMenuStrip.Size = new System.Drawing.Size(287, 270);
            this.dataGridContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnDataGridContextMenuStripOpening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.copyToolStripMenuItem.Text = "Copy to clipboard";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.OnCopyToolStripMenuItemClick);
            // 
            // copyToTabToolStripMenuItem
            // 
            this.copyToTabToolStripMenuItem.Name = "copyToTabToolStripMenuItem";
            this.copyToTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.copyToTabToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.copyToTabToolStripMenuItem.Text = "Copy to new tab";
            this.copyToTabToolStripMenuItem.ToolTipText = "Copy marked lines into a new tab window";
            this.copyToTabToolStripMenuItem.Click += new System.EventHandler(this.OnCopyToTabToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(283, 6);
            // 
            // scrollAllTabsToTimestampToolStripMenuItem
            // 
            this.scrollAllTabsToTimestampToolStripMenuItem.Name = "scrollAllTabsToTimestampToolStripMenuItem";
            this.scrollAllTabsToTimestampToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.scrollAllTabsToTimestampToolStripMenuItem.Text = "Scroll all tabs to current timestamp";
            this.scrollAllTabsToTimestampToolStripMenuItem.ToolTipText = "Scolls all open tabs to the selected timestamp, if possible";
            this.scrollAllTabsToTimestampToolStripMenuItem.Click += new System.EventHandler(this.OnScrollAllTabsToTimestampToolStripMenuItemClick);
            // 
            // syncTimestampsToToolStripMenuItem
            // 
            this.syncTimestampsToToolStripMenuItem.Name = "syncTimestampsToToolStripMenuItem";
            this.syncTimestampsToToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.syncTimestampsToToolStripMenuItem.Text = "Time synced files";
            // 
            // freeThisWindowFromTimeSyncToolStripMenuItem
            // 
            this.freeThisWindowFromTimeSyncToolStripMenuItem.Name = "freeThisWindowFromTimeSyncToolStripMenuItem";
            this.freeThisWindowFromTimeSyncToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.freeThisWindowFromTimeSyncToolStripMenuItem.Text = "Free this window from time sync";
            this.freeThisWindowFromTimeSyncToolStripMenuItem.Click += new System.EventHandler(this.OnFreeThisWindowFromTimeSyncToolStripMenuItemClick);
            // 
            // locateLineInOriginalFileToolStripMenuItem
            // 
            this.locateLineInOriginalFileToolStripMenuItem.Name = "locateLineInOriginalFileToolStripMenuItem";
            this.locateLineInOriginalFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.locateLineInOriginalFileToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.locateLineInOriginalFileToolStripMenuItem.Text = "Locate filtered line in original file";
            this.locateLineInOriginalFileToolStripMenuItem.Click += new System.EventHandler(this.OnLocateLineInOriginalFileToolStripMenuItemClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(283, 6);
            // 
            // toggleBoomarkToolStripMenuItem
            // 
            this.toggleBoomarkToolStripMenuItem.Name = "toggleBoomarkToolStripMenuItem";
            this.toggleBoomarkToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
            this.toggleBoomarkToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.toggleBoomarkToolStripMenuItem.Text = "Toggle Boomark";
            this.toggleBoomarkToolStripMenuItem.Click += new System.EventHandler(this.OnToggleBoomarkToolStripMenuItemClick);
            // 
            // bookmarkCommentToolStripMenuItem
            // 
            this.bookmarkCommentToolStripMenuItem.Name = "bookmarkCommentToolStripMenuItem";
            this.bookmarkCommentToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F2)));
            this.bookmarkCommentToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.bookmarkCommentToolStripMenuItem.Text = "Bookmark comment...";
            this.bookmarkCommentToolStripMenuItem.ToolTipText = "Edit the comment for a bookmark";
            this.bookmarkCommentToolStripMenuItem.Click += new System.EventHandler(this.OnBookmarkCommentToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(283, 6);
            // 
            // markEditModeToolStripMenuItem
            // 
            this.markEditModeToolStripMenuItem.Name = "markEditModeToolStripMenuItem";
            this.markEditModeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.markEditModeToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.markEditModeToolStripMenuItem.Text = "Mark/Edit-Mode";
            this.markEditModeToolStripMenuItem.Click += new System.EventHandler(this.OnMarkEditModeToolStripMenuItemClick);
            // 
            // tempHighlightsToolStripMenuItem
            // 
            this.tempHighlightsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeAllToolStripMenuItem,
            this.makePermanentToolStripMenuItem});
            this.tempHighlightsToolStripMenuItem.Name = "tempHighlightsToolStripMenuItem";
            this.tempHighlightsToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.tempHighlightsToolStripMenuItem.Text = "Temp Highlights";
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.H)));
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.removeAllToolStripMenuItem.Text = "Remove all";
            this.removeAllToolStripMenuItem.Click += new System.EventHandler(this.OnRemoveAllToolStripMenuItemClick);
            // 
            // makePermanentToolStripMenuItem
            // 
            this.makePermanentToolStripMenuItem.Name = "makePermanentToolStripMenuItem";
            this.makePermanentToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.makePermanentToolStripMenuItem.Text = "Make all permanent";
            this.makePermanentToolStripMenuItem.Click += new System.EventHandler(this.OnMakePermanentToolStripMenuItemClick);
            // 
            // markCurrentFilterRangeToolStripMenuItem
            // 
            this.markCurrentFilterRangeToolStripMenuItem.Name = "markCurrentFilterRangeToolStripMenuItem";
            this.markCurrentFilterRangeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.markCurrentFilterRangeToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.markCurrentFilterRangeToolStripMenuItem.Text = "Mark current filter range";
            this.markCurrentFilterRangeToolStripMenuItem.Click += new System.EventHandler(this.OnMarkCurrentFilterRangeToolStripMenuItemClick);
            // 
            // pluginSeparator
            // 
            this.pluginSeparator.Name = "pluginSeparator";
            this.pluginSeparator.Size = new System.Drawing.Size(283, 6);
            // 
            // timeSpreadingControl
            // 
            this.timeSpreadingControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timeSpreadingControl.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeSpreadingControl.ForeColor = System.Drawing.Color.Teal;
            this.timeSpreadingControl.Location = new System.Drawing.Point(1013, 30);
            this.timeSpreadingControl.Margin = new System.Windows.Forms.Padding(2, 0, 1, 0);
            this.timeSpreadingControl.Name = "timeSpreadingControl";
            this.timeSpreadingControl.ReverseAlpha = false;
            this.timeSpreadingControl.Size = new System.Drawing.Size(16, 331);
            this.timeSpreadingControl.TabIndex = 1;
            // 
            // advancedBackPanel
            // 
            this.advancedBackPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedBackPanel.Controls.Add(this.advancedFilterSplitContainer);
            this.advancedBackPanel.Location = new System.Drawing.Point(3, 35);
            this.advancedBackPanel.Name = "advancedBackPanel";
            this.advancedBackPanel.Size = new System.Drawing.Size(1007, 248);
            this.advancedBackPanel.TabIndex = 3;
            // 
            // advancedFilterSplitContainer
            // 
            this.advancedFilterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advancedFilterSplitContainer.IsSplitterFixed = true;
            this.advancedFilterSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.advancedFilterSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.advancedFilterSplitContainer.Name = "advancedFilterSplitContainer";
            this.advancedFilterSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // advancedFilterSplitContainer.Panel1
            // 
            this.advancedFilterSplitContainer.Panel1.Controls.Add(this.pnlProFilter);
            this.advancedFilterSplitContainer.Panel1MinSize = 54;
            // 
            // advancedFilterSplitContainer.Panel2
            // 
            this.advancedFilterSplitContainer.Panel2.Controls.Add(this.btnToggleHighlightPanel);
            this.advancedFilterSplitContainer.Panel2.Controls.Add(this.highlightSplitContainer);
            this.advancedFilterSplitContainer.Panel2MinSize = 50;
            this.advancedFilterSplitContainer.Size = new System.Drawing.Size(1007, 248);
            this.advancedFilterSplitContainer.SplitterDistance = 73;
            this.advancedFilterSplitContainer.SplitterWidth = 2;
            this.advancedFilterSplitContainer.TabIndex = 2;
            // 
            // pnlProFilter
            // 
            this.pnlProFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlProFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.pnlProFilter.Controls.Add(this.columnButton);
            this.pnlProFilter.Controls.Add(this.columnRestrictCheckBox);
            this.pnlProFilter.Controls.Add(this.rangeCheckBox);
            this.pnlProFilter.Controls.Add(this.filterRangeComboBox);
            this.pnlProFilter.Controls.Add(this.columnNamesLabel);
            this.pnlProFilter.Controls.Add(this.fuzzyLabel);
            this.pnlProFilter.Controls.Add(this.fuzzyKnobControl);
            this.pnlProFilter.Controls.Add(this.invertFilterCheckBox);
            this.pnlProFilter.Controls.Add(this.pnlProFilterLabel);
            this.pnlProFilter.Controls.Add(this.lblBackSpread);
            this.pnlProFilter.Controls.Add(this.filterKnobBackSpread);
            this.pnlProFilter.Controls.Add(this.lblForeSpread);
            this.pnlProFilter.Controls.Add(this.filterKnobForeSpread);
            this.pnlProFilter.Controls.Add(this.btnFilterToTab);
            this.pnlProFilter.Location = new System.Drawing.Point(0, 3);
            this.pnlProFilter.Name = "pnlProFilter";
            this.pnlProFilter.Size = new System.Drawing.Size(1004, 69);
            this.pnlProFilter.TabIndex = 0;
            // 
            // columnButton
            // 
            this.columnButton.Enabled = false;
            this.columnButton.Location = new System.Drawing.Point(655, 30);
            this.columnButton.Name = "columnButton";
            this.columnButton.Size = new System.Drawing.Size(71, 23);
            this.columnButton.TabIndex = 15;
            this.columnButton.Text = "Columns...";
            this.helpToolTip.SetToolTip(this.columnButton, "Choose columns for \'Column restrict\'");
            this.columnButton.UseVisualStyleBackColor = true;
            this.columnButton.Click += new System.EventHandler(this.OncolumnButtonClick);
            // 
            // columnRestrictCheckBox
            // 
            this.columnRestrictCheckBox.AutoSize = true;
            this.columnRestrictCheckBox.Location = new System.Drawing.Point(527, 37);
            this.columnRestrictCheckBox.Name = "columnRestrictCheckBox";
            this.columnRestrictCheckBox.Size = new System.Drawing.Size(95, 17);
            this.columnRestrictCheckBox.TabIndex = 14;
            this.columnRestrictCheckBox.Text = "Column restrict";
            this.helpToolTip.SetToolTip(this.columnRestrictCheckBox, "Restrict search to columns");
            this.columnRestrictCheckBox.UseVisualStyleBackColor = true;
            this.columnRestrictCheckBox.CheckedChanged += new System.EventHandler(this.OnColumnRestrictCheckBoxCheckedChanged);
            // 
            // rangeCheckBox
            // 
            this.rangeCheckBox.AutoSize = true;
            this.rangeCheckBox.Location = new System.Drawing.Point(75, 36);
            this.rangeCheckBox.Name = "rangeCheckBox";
            this.rangeCheckBox.Size = new System.Drawing.Size(93, 17);
            this.rangeCheckBox.TabIndex = 13;
            this.rangeCheckBox.Text = "Range search";
            this.helpToolTip.SetToolTip(this.rangeCheckBox, "Enable a special search mode which filters all content between the 2 given search" +
        " terms.");
            this.rangeCheckBox.UseVisualStyleBackColor = true;
            this.rangeCheckBox.CheckedChanged += new System.EventHandler(this.OnRangeCheckBoxCheckedChanged);
            // 
            // filterRangeComboBox
            // 
            this.filterRangeComboBox.Enabled = false;
            this.filterRangeComboBox.FormattingEnabled = true;
            this.filterRangeComboBox.Location = new System.Drawing.Point(73, 11);
            this.filterRangeComboBox.Name = "filterRangeComboBox";
            this.filterRangeComboBox.Size = new System.Drawing.Size(207, 21);
            this.filterRangeComboBox.TabIndex = 12;
            this.helpToolTip.SetToolTip(this.filterRangeComboBox, "2nd search string (\'end string\') when using the range search");
            this.filterRangeComboBox.TextChanged += new System.EventHandler(this.OnFilterRangeComboBoxTextChanged);
            // 
            // columnNamesLabel
            // 
            this.columnNamesLabel.AutoSize = true;
            this.columnNamesLabel.Location = new System.Drawing.Point(732, 35);
            this.columnNamesLabel.Name = "columnNamesLabel";
            this.columnNamesLabel.Size = new System.Drawing.Size(75, 13);
            this.columnNamesLabel.TabIndex = 11;
            this.columnNamesLabel.Text = "column names";
            // 
            // fuzzyLabel
            // 
            this.fuzzyLabel.AutoSize = true;
            this.fuzzyLabel.Location = new System.Drawing.Point(435, 38);
            this.fuzzyLabel.Name = "fuzzyLabel";
            this.fuzzyLabel.Size = new System.Drawing.Size(56, 13);
            this.fuzzyLabel.TabIndex = 11;
            this.fuzzyLabel.Text = "Fuzzyness";
            // 
            // fuzzyKnobControl
            // 
            this.fuzzyKnobControl.DragSensitivity = 6;
            this.fuzzyKnobControl.Font = new System.Drawing.Font("Verdana", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fuzzyKnobControl.Location = new System.Drawing.Point(454, 7);
            this.fuzzyKnobControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.fuzzyKnobControl.MaxValue = 0;
            this.fuzzyKnobControl.MinValue = 0;
            this.fuzzyKnobControl.Name = "fuzzyKnobControl";
            this.fuzzyKnobControl.Size = new System.Drawing.Size(17, 29);
            this.fuzzyKnobControl.TabIndex = 10;
            this.helpToolTip.SetToolTip(this.fuzzyKnobControl, "Fuzzy search level (0 = fuzzy off)");
            this.fuzzyKnobControl.Value = 0;
            this.fuzzyKnobControl.ValueChanged += new KnobControl.ValueChangedEventHandler(this.OnFuzzyKnobControlValueChanged);
            // 
            // invertFilterCheckBox
            // 
            this.invertFilterCheckBox.AutoSize = true;
            this.invertFilterCheckBox.Location = new System.Drawing.Point(527, 13);
            this.invertFilterCheckBox.Name = "invertFilterCheckBox";
            this.invertFilterCheckBox.Size = new System.Drawing.Size(86, 17);
            this.invertFilterCheckBox.TabIndex = 8;
            this.invertFilterCheckBox.Text = "Invert Match";
            this.helpToolTip.SetToolTip(this.invertFilterCheckBox, "Invert the search result");
            this.invertFilterCheckBox.UseVisualStyleBackColor = true;
            this.invertFilterCheckBox.CheckedChanged += new System.EventHandler(this.OnInvertFilterCheckBoxCheckedChanged);
            // 
            // pnlProFilterLabel
            // 
            this.pnlProFilterLabel.BackgroundImage = global::LogExpert.Properties.Resources.Pro_Filter;
            this.pnlProFilterLabel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pnlProFilterLabel.Location = new System.Drawing.Point(5, 7);
            this.pnlProFilterLabel.Name = "pnlProFilterLabel";
            this.pnlProFilterLabel.Size = new System.Drawing.Size(60, 44);
            this.pnlProFilterLabel.TabIndex = 7;
            // 
            // lblBackSpread
            // 
            this.lblBackSpread.AutoSize = true;
            this.lblBackSpread.Location = new System.Drawing.Point(273, 38);
            this.lblBackSpread.Name = "lblBackSpread";
            this.lblBackSpread.Size = new System.Drawing.Size(72, 13);
            this.lblBackSpread.TabIndex = 6;
            this.lblBackSpread.Text = "Back Spread ";
            // 
            // filterKnobBackSpread
            // 
            this.filterKnobBackSpread.DragSensitivity = 3;
            this.filterKnobBackSpread.Font = new System.Drawing.Font("Verdana", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filterKnobBackSpread.Location = new System.Drawing.Point(299, 7);
            this.filterKnobBackSpread.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.filterKnobBackSpread.MaxValue = 0;
            this.filterKnobBackSpread.MinValue = 0;
            this.filterKnobBackSpread.Name = "filterKnobBackSpread";
            this.filterKnobBackSpread.Size = new System.Drawing.Size(17, 29);
            this.filterKnobBackSpread.TabIndex = 5;
            this.helpToolTip.SetToolTip(this.filterKnobBackSpread, "Add preceding lines to search result (Drag up/down, press Shift for finer pitch)");
            this.filterKnobBackSpread.Value = 0;
            // 
            // lblForeSpread
            // 
            this.lblForeSpread.AutoSize = true;
            this.lblForeSpread.Location = new System.Drawing.Point(342, 38);
            this.lblForeSpread.Name = "lblForeSpread";
            this.lblForeSpread.Size = new System.Drawing.Size(65, 13);
            this.lblForeSpread.TabIndex = 2;
            this.lblForeSpread.Text = "Fore Spread";
            // 
            // filterKnobForeSpread
            // 
            this.filterKnobForeSpread.DragSensitivity = 3;
            this.filterKnobForeSpread.Font = new System.Drawing.Font("Verdana", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filterKnobForeSpread.Location = new System.Drawing.Point(365, 7);
            this.filterKnobForeSpread.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.filterKnobForeSpread.MaxValue = 0;
            this.filterKnobForeSpread.MinValue = 0;
            this.filterKnobForeSpread.Name = "filterKnobForeSpread";
            this.filterKnobForeSpread.Size = new System.Drawing.Size(17, 29);
            this.filterKnobForeSpread.TabIndex = 1;
            this.helpToolTip.SetToolTip(this.filterKnobForeSpread, "Add following lines to search result (Drag up/down, press Shift for finer pitch)");
            this.filterKnobForeSpread.Value = 0;
            // 
            // btnFilterToTab
            // 
            this.btnFilterToTab.Location = new System.Drawing.Point(655, 3);
            this.btnFilterToTab.Name = "btnFilterToTab";
            this.btnFilterToTab.Size = new System.Drawing.Size(71, 23);
            this.btnFilterToTab.TabIndex = 0;
            this.btnFilterToTab.Text = "Filter to Tab";
            this.helpToolTip.SetToolTip(this.btnFilterToTab, "Launch a new tab with filtered content");
            this.btnFilterToTab.UseVisualStyleBackColor = true;
            this.btnFilterToTab.Click += new System.EventHandler(this.OnFilterToTabButtonClick);
            // 
            // btnToggleHighlightPanel
            // 
            this.btnToggleHighlightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleHighlightPanel.Image = global::LogExpert.Properties.Resources.PanelOpen;
            this.btnToggleHighlightPanel.Location = new System.Drawing.Point(984, 1);
            this.btnToggleHighlightPanel.Name = "btnToggleHighlightPanel";
            this.btnToggleHighlightPanel.Size = new System.Drawing.Size(20, 21);
            this.btnToggleHighlightPanel.TabIndex = 6;
            this.helpToolTip.SetToolTip(this.btnToggleHighlightPanel, "Open or close a list with saved filters");
            this.btnToggleHighlightPanel.UseVisualStyleBackColor = true;
            this.btnToggleHighlightPanel.Click += new System.EventHandler(this.OnToggleHighlightPanelButtonClick);
            // 
            // highlightSplitContainer
            // 
            this.highlightSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.highlightSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.highlightSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.highlightSplitContainer.IsSplitterFixed = true;
            this.highlightSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.highlightSplitContainer.Name = "highlightSplitContainer";
            // 
            // highlightSplitContainer.Panel1
            // 
            this.highlightSplitContainer.Panel1.Controls.Add(this.filterGridView);
            // 
            // highlightSplitContainer.Panel2
            // 
            this.highlightSplitContainer.Panel2.Controls.Add(this.highlightSplitContainerBackPanel);
            this.highlightSplitContainer.Panel2MinSize = 30;
            this.highlightSplitContainer.Size = new System.Drawing.Size(981, 175);
            this.highlightSplitContainer.SplitterDistance = 612;
            this.highlightSplitContainer.TabIndex = 2;
            // 
            // filterGridView
            // 
            this.filterGridView.AllowUserToAddRows = false;
            this.filterGridView.AllowUserToDeleteRows = false;
            this.filterGridView.AllowUserToOrderColumns = true;
            this.filterGridView.AllowUserToResizeRows = false;
            this.filterGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.filterGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.filterGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.filterGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.filterGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.filterGridView.ContextMenuStrip = this.filterContextMenuStrip;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.filterGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.filterGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.filterGridView.EditModeMenuStrip = null;
            this.filterGridView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.filterGridView.Location = new System.Drawing.Point(0, 0);
            this.filterGridView.Margin = new System.Windows.Forms.Padding(0);
            this.filterGridView.Name = "filterGridView";
            this.filterGridView.PaintWithOverlays = false;
            this.filterGridView.ReadOnly = true;
            this.filterGridView.RowHeadersVisible = false;
            this.filterGridView.RowTemplate.Height = 15;
            this.filterGridView.RowTemplate.ReadOnly = true;
            this.filterGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.filterGridView.ShowCellErrors = false;
            this.filterGridView.ShowCellToolTips = false;
            this.filterGridView.ShowEditingIcon = false;
            this.filterGridView.ShowRowErrors = false;
            this.filterGridView.Size = new System.Drawing.Size(610, 173);
            this.filterGridView.TabIndex = 1;
            this.filterGridView.VirtualMode = true;
            this.filterGridView.CellContextMenuStripNeeded += new System.Windows.Forms.DataGridViewCellContextMenuStripNeededEventHandler(this.OnFilterGridViewCellContextMenuStripNeeded);
            this.filterGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnFilterGridViewCellDoubleClick);
            this.filterGridView.ColumnDividerDoubleClick += new System.Windows.Forms.DataGridViewColumnDividerDoubleClickEventHandler(this.OnFilterGridViewColumnDividerDoubleClick);
            this.filterGridView.RowHeightInfoNeeded += new System.Windows.Forms.DataGridViewRowHeightInfoNeededEventHandler(this.OnFilterGridViewRowHeightInfoNeeded);
            this.filterGridView.Enter += new System.EventHandler(this.OnFilterGridViewEnter);
            this.filterGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnFilterGridViewKeyDown);
            this.filterGridView.Leave += new System.EventHandler(this.OnFilterGridViewLeave);
            // 
            // filterContextMenuStrip
            // 
            this.filterContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setBookmarksOnSelectedLinesToolStripMenuItem,
            this.filterToTabToolStripMenuItem,
            this.markFilterHitsInLogViewToolStripMenuItem});
            this.filterContextMenuStrip.Name = "filterContextMenuStrip";
            this.filterContextMenuStrip.Size = new System.Drawing.Size(243, 70);
            // 
            // setBookmarksOnSelectedLinesToolStripMenuItem
            // 
            this.setBookmarksOnSelectedLinesToolStripMenuItem.Name = "setBookmarksOnSelectedLinesToolStripMenuItem";
            this.setBookmarksOnSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.setBookmarksOnSelectedLinesToolStripMenuItem.Text = "Set bookmarks on selected lines";
            this.setBookmarksOnSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.OnSetBookmarksOnSelectedLinesToolStripMenuItemClick);
            // 
            // filterToTabToolStripMenuItem
            // 
            this.filterToTabToolStripMenuItem.Name = "filterToTabToolStripMenuItem";
            this.filterToTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.T)));
            this.filterToTabToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.filterToTabToolStripMenuItem.Text = "Filter to new tab";
            this.filterToTabToolStripMenuItem.Click += new System.EventHandler(this.OnFilterToTabToolStripMenuItemClick);
            // 
            // markFilterHitsInLogViewToolStripMenuItem
            // 
            this.markFilterHitsInLogViewToolStripMenuItem.Name = "markFilterHitsInLogViewToolStripMenuItem";
            this.markFilterHitsInLogViewToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.markFilterHitsInLogViewToolStripMenuItem.Text = "Mark filter hits in log view";
            this.markFilterHitsInLogViewToolStripMenuItem.Click += new System.EventHandler(this.OnMarkFilterHitsInLogViewToolStripMenuItemClick);
            // 
            // highlightSplitContainerBackPanel
            // 
            this.highlightSplitContainerBackPanel.Controls.Add(this.hideFilterListOnLoadCheckBox);
            this.highlightSplitContainerBackPanel.Controls.Add(this.filterDownButton);
            this.highlightSplitContainerBackPanel.Controls.Add(this.filterUpButton);
            this.highlightSplitContainerBackPanel.Controls.Add(this.filterOnLoadCheckBox);
            this.highlightSplitContainerBackPanel.Controls.Add(this.saveFilterButton);
            this.highlightSplitContainerBackPanel.Controls.Add(this.deleteFilterButton);
            this.highlightSplitContainerBackPanel.Controls.Add(this.filterListBox);
            this.highlightSplitContainerBackPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.highlightSplitContainerBackPanel.Location = new System.Drawing.Point(0, 0);
            this.highlightSplitContainerBackPanel.Name = "highlightSplitContainerBackPanel";
            this.highlightSplitContainerBackPanel.Size = new System.Drawing.Size(363, 173);
            this.highlightSplitContainerBackPanel.TabIndex = 1;
            // 
            // hideFilterListOnLoadCheckBox
            // 
            this.hideFilterListOnLoadCheckBox.AutoSize = true;
            this.hideFilterListOnLoadCheckBox.Location = new System.Drawing.Point(287, 134);
            this.hideFilterListOnLoadCheckBox.Name = "hideFilterListOnLoadCheckBox";
            this.hideFilterListOnLoadCheckBox.Size = new System.Drawing.Size(71, 17);
            this.hideFilterListOnLoadCheckBox.TabIndex = 20;
            this.hideFilterListOnLoadCheckBox.Text = "Auto hide";
            this.helpToolTip.SetToolTip(this.hideFilterListOnLoadCheckBox, "Hides the filter list after loading a filter");
            this.hideFilterListOnLoadCheckBox.UseVisualStyleBackColor = true;
            this.hideFilterListOnLoadCheckBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnHideFilterListOnLoadCheckBoxMouseClick);
            // 
            // filterDownButton
            // 
            this.filterDownButton.Image = global::LogExpert.Properties.Resources.ArrowDown;
            this.filterDownButton.Location = new System.Drawing.Point(325, 70);
            this.filterDownButton.Name = "filterDownButton";
            this.filterDownButton.Size = new System.Drawing.Size(35, 23);
            this.filterDownButton.TabIndex = 19;
            this.helpToolTip.SetToolTip(this.filterDownButton, "Move the selected entry down in the list");
            this.filterDownButton.UseVisualStyleBackColor = true;
            this.filterDownButton.Click += new System.EventHandler(this.OnFilterDownButtonClick);
            // 
            // filterUpButton
            // 
            this.filterUpButton.Image = global::LogExpert.Properties.Resources.ArrowUp;
            this.filterUpButton.Location = new System.Drawing.Point(287, 70);
            this.filterUpButton.Name = "filterUpButton";
            this.filterUpButton.Size = new System.Drawing.Size(35, 23);
            this.filterUpButton.TabIndex = 18;
            this.helpToolTip.SetToolTip(this.filterUpButton, "Move the selected entry up in the list");
            this.filterUpButton.UseVisualStyleBackColor = true;
            this.filterUpButton.Click += new System.EventHandler(this.OnFilterUpButtonClick);
            // 
            // filterOnLoadCheckBox
            // 
            this.filterOnLoadCheckBox.AutoSize = true;
            this.filterOnLoadCheckBox.Location = new System.Drawing.Point(287, 110);
            this.filterOnLoadCheckBox.Name = "filterOnLoadCheckBox";
            this.filterOnLoadCheckBox.Size = new System.Drawing.Size(71, 17);
            this.filterOnLoadCheckBox.TabIndex = 17;
            this.filterOnLoadCheckBox.Text = "Auto start";
            this.helpToolTip.SetToolTip(this.filterOnLoadCheckBox, "Start immediate filtering after loading a saved filter");
            this.filterOnLoadCheckBox.UseVisualStyleBackColor = true;
            this.filterOnLoadCheckBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnFilterOnLoadCheckBoxKeyPress);
            this.filterOnLoadCheckBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFilterOnLoadCheckBoxMouseClick);
            // 
            // saveFilterButton
            // 
            this.saveFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.saveFilterButton.Location = new System.Drawing.Point(287, 11);
            this.saveFilterButton.Name = "saveFilterButton";
            this.saveFilterButton.Size = new System.Drawing.Size(73, 23);
            this.saveFilterButton.TabIndex = 16;
            this.saveFilterButton.Text = "Save filter";
            this.saveFilterButton.UseVisualStyleBackColor = true;
            this.saveFilterButton.Click += new System.EventHandler(this.OnSaveFilterButtonClick);
            // 
            // deleteFilterButton
            // 
            this.deleteFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.deleteFilterButton.Location = new System.Drawing.Point(287, 40);
            this.deleteFilterButton.Name = "deleteFilterButton";
            this.deleteFilterButton.Size = new System.Drawing.Size(73, 23);
            this.deleteFilterButton.TabIndex = 3;
            this.deleteFilterButton.Text = "Delete";
            this.deleteFilterButton.UseVisualStyleBackColor = true;
            this.deleteFilterButton.Click += new System.EventHandler(this.OnDeleteFilterButtonClick);
            // 
            // filterListBox
            // 
            this.filterListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterListBox.ContextMenuStrip = this.filterListContextMenuStrip;
            this.filterListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.filterListBox.FormattingEnabled = true;
            this.filterListBox.IntegralHeight = false;
            this.filterListBox.Location = new System.Drawing.Point(3, 3);
            this.filterListBox.Name = "filterListBox";
            this.filterListBox.Size = new System.Drawing.Size(278, 168);
            this.filterListBox.TabIndex = 0;
            this.helpToolTip.SetToolTip(this.filterListBox, "Doubleclick to load a saved filter");
            this.filterListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.OnFilterListBoxDrawItem);
            this.filterListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnFilterListBoxMouseDoubleClick);
            // 
            // filterListContextMenuStrip
            // 
            this.filterListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorToolStripMenuItem});
            this.filterListContextMenuStrip.Name = "filterListContextMenuStrip";
            this.filterListContextMenuStrip.Size = new System.Drawing.Size(113, 26);
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.colorToolStripMenuItem.Text = "Color...";
            this.colorToolStripMenuItem.Click += new System.EventHandler(this.OnColorToolStripMenuItemClick);
            // 
            // pnlFilterInput
            // 
            this.pnlFilterInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlFilterInput.Controls.Add(this.filterSplitContainer);
            this.pnlFilterInput.Location = new System.Drawing.Point(3, 2);
            this.pnlFilterInput.Name = "pnlFilterInput";
            this.pnlFilterInput.Size = new System.Drawing.Size(1007, 32);
            this.pnlFilterInput.TabIndex = 0;
            // 
            // filterSplitContainer
            // 
            this.filterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.filterSplitContainer.Name = "filterSplitContainer";
            // 
            // filterSplitContainer.Panel1
            // 
            this.filterSplitContainer.Panel1.Controls.Add(this.lblTextFilter);
            this.filterSplitContainer.Panel1.Controls.Add(this.filterComboBox);
            this.filterSplitContainer.Panel1MinSize = 200;
            // 
            // filterSplitContainer.Panel2
            // 
            this.filterSplitContainer.Panel2.Controls.Add(this.advancedButton);
            this.filterSplitContainer.Panel2.Controls.Add(this.syncFilterCheckBox);
            this.filterSplitContainer.Panel2.Controls.Add(this.lblFilterCount);
            this.filterSplitContainer.Panel2.Controls.Add(this.filterTailCheckBox);
            this.filterSplitContainer.Panel2.Controls.Add(this.filterRegexCheckBox);
            this.filterSplitContainer.Panel2.Controls.Add(this.filterCaseSensitiveCheckBox);
            this.filterSplitContainer.Panel2.Controls.Add(this.filterSearchButton);
            this.filterSplitContainer.Panel2MinSize = 550;
            this.filterSplitContainer.Size = new System.Drawing.Size(1007, 32);
            this.filterSplitContainer.SplitterDistance = 282;
            this.filterSplitContainer.TabIndex = 11;
            this.filterSplitContainer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnFilterSplitContainerMouseDoubleClick);
            this.filterSplitContainer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnFilterSplitContainerMouseDown);
            this.filterSplitContainer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnFilterSplitContainerMouseMove);
            this.filterSplitContainer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnFilterSplitContainerMouseUp);
            // 
            // lblTextFilter
            // 
            this.lblTextFilter.AutoSize = true;
            this.lblTextFilter.Location = new System.Drawing.Point(5, 9);
            this.lblTextFilter.Name = "lblTextFilter";
            this.lblTextFilter.Size = new System.Drawing.Size(53, 13);
            this.lblTextFilter.TabIndex = 3;
            this.lblTextFilter.Text = "Text &filter:";
            // 
            // filterComboBox
            // 
            this.filterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterComboBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filterComboBox.FormattingEnabled = true;
            this.filterComboBox.Location = new System.Drawing.Point(73, 5);
            this.filterComboBox.Name = "filterComboBox";
            this.filterComboBox.Size = new System.Drawing.Size(206, 22);
            this.filterComboBox.TabIndex = 4;
            this.helpToolTip.SetToolTip(this.filterComboBox, "Search string for the filter");
            this.filterComboBox.TextChanged += new System.EventHandler(this.OnFilterComboBoxTextChanged);
            this.filterComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnFilterComboBoxKeyDown);
            // 
            // advancedButton
            // 
            this.advancedButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.advancedButton.Image = global::LogExpert.Properties.Resources.AdvancedIcon2;
            this.advancedButton.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.advancedButton.Location = new System.Drawing.Point(363, 5);
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(110, 21);
            this.advancedButton.TabIndex = 17;
            this.advancedButton.Text = "Show advanced...";
            this.helpToolTip.SetToolTip(this.advancedButton, "Togge the advanced filter options panel");
            this.advancedButton.UseVisualStyleBackColor = true;
            this.advancedButton.Click += new System.EventHandler(this.OnAdvancedButtonClick);
            // 
            // syncFilterCheckBox
            // 
            this.syncFilterCheckBox.AutoSize = true;
            this.syncFilterCheckBox.Location = new System.Drawing.Point(306, 7);
            this.syncFilterCheckBox.Name = "syncFilterCheckBox";
            this.syncFilterCheckBox.Size = new System.Drawing.Size(50, 17);
            this.syncFilterCheckBox.TabIndex = 16;
            this.syncFilterCheckBox.Text = "Sync";
            this.helpToolTip.SetToolTip(this.syncFilterCheckBox, "Sync the current selected line in the filter view to the selection in the log fil" +
        "e view");
            this.syncFilterCheckBox.UseVisualStyleBackColor = true;
            this.syncFilterCheckBox.CheckedChanged += new System.EventHandler(this.OnSyncFilterCheckBoxCheckedChanged);
            // 
            // lblFilterCount
            // 
            this.lblFilterCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFilterCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblFilterCount.Location = new System.Drawing.Point(647, 5);
            this.lblFilterCount.Name = "lblFilterCount";
            this.lblFilterCount.Size = new System.Drawing.Size(71, 21);
            this.lblFilterCount.TabIndex = 15;
            this.lblFilterCount.Text = "0";
            this.lblFilterCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // filterTailCheckBox
            // 
            this.filterTailCheckBox.AutoSize = true;
            this.filterTailCheckBox.Location = new System.Drawing.Point(235, 7);
            this.filterTailCheckBox.Name = "filterTailCheckBox";
            this.filterTailCheckBox.Size = new System.Drawing.Size(64, 17);
            this.filterTailCheckBox.TabIndex = 14;
            this.filterTailCheckBox.Text = "Filter tail";
            this.helpToolTip.SetToolTip(this.filterTailCheckBox, "Filter tailed file content (keeps filter view up to date on file changes)");
            this.filterTailCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterRegexCheckBox
            // 
            this.filterRegexCheckBox.AutoSize = true;
            this.filterRegexCheckBox.Location = new System.Drawing.Point(172, 7);
            this.filterRegexCheckBox.Name = "filterRegexCheckBox";
            this.filterRegexCheckBox.Size = new System.Drawing.Size(57, 17);
            this.filterRegexCheckBox.TabIndex = 13;
            this.filterRegexCheckBox.Text = "Regex";
            this.helpToolTip.SetToolTip(this.filterRegexCheckBox, "Use regular expressions. (right-click for RegEx helper window)");
            this.filterRegexCheckBox.UseVisualStyleBackColor = true;
            this.filterRegexCheckBox.CheckedChanged += new System.EventHandler(this.OnFilterRegexCheckBoxCheckedChanged);
            this.filterRegexCheckBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnFilterRegexCheckBoxMouseUp);
            // 
            // filterCaseSensitiveCheckBox
            // 
            this.filterCaseSensitiveCheckBox.AutoSize = true;
            this.filterCaseSensitiveCheckBox.Location = new System.Drawing.Point(72, 7);
            this.filterCaseSensitiveCheckBox.Name = "filterCaseSensitiveCheckBox";
            this.filterCaseSensitiveCheckBox.Size = new System.Drawing.Size(94, 17);
            this.filterCaseSensitiveCheckBox.TabIndex = 12;
            this.filterCaseSensitiveCheckBox.Text = "Case sensitive";
            this.helpToolTip.SetToolTip(this.filterCaseSensitiveCheckBox, "Makes the filter case sensitive");
            this.filterCaseSensitiveCheckBox.UseVisualStyleBackColor = true;
            this.filterCaseSensitiveCheckBox.CheckedChanged += new System.EventHandler(this.OnFilterCaseSensitiveCheckBoxCheckedChanged);
            // 
            // filterSearchButton
            // 
            this.filterSearchButton.Image = global::LogExpert.Properties.Resources.AdvancedIcon2;
            this.filterSearchButton.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.filterSearchButton.Location = new System.Drawing.Point(3, 5);
            this.filterSearchButton.Name = "filterSearchButton";
            this.filterSearchButton.Size = new System.Drawing.Size(50, 21);
            this.filterSearchButton.TabIndex = 11;
            this.filterSearchButton.Text = "Search";
            this.helpToolTip.SetToolTip(this.filterSearchButton, "Start the filter search");
            this.filterSearchButton.UseVisualStyleBackColor = true;
            this.filterSearchButton.Click += new System.EventHandler(this.OnFilterSearchButtonClick);
            // 
            // bookmarkContextMenuStrip
            // 
            this.bookmarkContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteBookmarksToolStripMenuItem});
            this.bookmarkContextMenuStrip.Name = "bookmarkContextMenuStrip";
            this.bookmarkContextMenuStrip.Size = new System.Drawing.Size(68, 26);
            // 
            // deleteBookmarksToolStripMenuItem
            // 
            this.deleteBookmarksToolStripMenuItem.Name = "deleteBookmarksToolStripMenuItem";
            this.deleteBookmarksToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            // 
            // columnContextMenuStrip
            // 
            this.columnContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.freezeLeftColumnsUntilHereToolStripMenuItem,
            this.toolStripSeparator3,
            this.moveToLastColumnToolStripMenuItem,
            this.moveLeftToolStripMenuItem,
            this.moveRightToolStripMenuItem,
            this.toolStripSeparator5,
            this.hideColumnToolStripMenuItem,
            this.restoreColumnsToolStripMenuItem,
            this.toolStripSeparator6,
            this.allColumnsToolStripMenuItem});
            this.columnContextMenuStrip.Name = "columnContextMenuStrip";
            this.columnContextMenuStrip.Size = new System.Drawing.Size(230, 176);
            this.columnContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnColumnContextMenuStripOpening);
            // 
            // freezeLeftColumnsUntilHereToolStripMenuItem
            // 
            this.freezeLeftColumnsUntilHereToolStripMenuItem.Name = "freezeLeftColumnsUntilHereToolStripMenuItem";
            this.freezeLeftColumnsUntilHereToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Freeze left columns until here";
            this.freezeLeftColumnsUntilHereToolStripMenuItem.Click += new System.EventHandler(this.OnFreezeLeftColumnsUntilHereToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(226, 6);
            // 
            // moveToLastColumnToolStripMenuItem
            // 
            this.moveToLastColumnToolStripMenuItem.Name = "moveToLastColumnToolStripMenuItem";
            this.moveToLastColumnToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.moveToLastColumnToolStripMenuItem.Text = "Move to last column";
            this.moveToLastColumnToolStripMenuItem.ToolTipText = "Move this column to the last position";
            this.moveToLastColumnToolStripMenuItem.Click += new System.EventHandler(this.OnMoveToLastColumnToolStripMenuItemClick);
            // 
            // moveLeftToolStripMenuItem
            // 
            this.moveLeftToolStripMenuItem.Name = "moveLeftToolStripMenuItem";
            this.moveLeftToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.moveLeftToolStripMenuItem.Text = "Move left";
            this.moveLeftToolStripMenuItem.Click += new System.EventHandler(this.OnMoveLeftToolStripMenuItemClick);
            // 
            // moveRightToolStripMenuItem
            // 
            this.moveRightToolStripMenuItem.Name = "moveRightToolStripMenuItem";
            this.moveRightToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.moveRightToolStripMenuItem.Text = "Move right";
            this.moveRightToolStripMenuItem.Click += new System.EventHandler(this.OnMoveRightToolStripMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(226, 6);
            // 
            // hideColumnToolStripMenuItem
            // 
            this.hideColumnToolStripMenuItem.Name = "hideColumnToolStripMenuItem";
            this.hideColumnToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.hideColumnToolStripMenuItem.Text = "Hide column";
            this.hideColumnToolStripMenuItem.ToolTipText = "Hide this column";
            this.hideColumnToolStripMenuItem.Click += new System.EventHandler(this.OnHideColumnToolStripMenuItemClick);
            // 
            // restoreColumnsToolStripMenuItem
            // 
            this.restoreColumnsToolStripMenuItem.Name = "restoreColumnsToolStripMenuItem";
            this.restoreColumnsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.restoreColumnsToolStripMenuItem.Text = "Restore columns";
            this.restoreColumnsToolStripMenuItem.Click += new System.EventHandler(this.OnRestoreColumnsToolStripMenuItemClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(226, 6);
            // 
            // allColumnsToolStripMenuItem
            // 
            this.allColumnsToolStripMenuItem.Name = "allColumnsToolStripMenuItem";
            this.allColumnsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.allColumnsToolStripMenuItem.Text = "Scroll to column...";
            // 
            // editModeContextMenuStrip
            // 
            this.editModeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editModecopyToolStripMenuItem,
            this.highlightSelectionInLogFileToolStripMenuItem,
            this.highlightSelectionInLogFilewordModeToolStripMenuItem,
            this.filterForSelectionToolStripMenuItem,
            this.setSelectedTextAsBookmarkCommentToolStripMenuItem});
            this.editModeContextMenuStrip.Name = "editModeContextMenuStrip";
            this.editModeContextMenuStrip.Size = new System.Drawing.Size(344, 114);
            // 
            // editModecopyToolStripMenuItem
            // 
            this.editModecopyToolStripMenuItem.Name = "editModecopyToolStripMenuItem";
            this.editModecopyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.editModecopyToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.editModecopyToolStripMenuItem.Text = "Copy";
            this.editModecopyToolStripMenuItem.Click += new System.EventHandler(this.OnEditModeCopyToolStripMenuItemClick);
            // 
            // highlightSelectionInLogFileToolStripMenuItem
            // 
            this.highlightSelectionInLogFileToolStripMenuItem.Name = "highlightSelectionInLogFileToolStripMenuItem";
            this.highlightSelectionInLogFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.highlightSelectionInLogFileToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.highlightSelectionInLogFileToolStripMenuItem.Text = "Highlight selection in log file (full line)";
            this.highlightSelectionInLogFileToolStripMenuItem.Click += new System.EventHandler(this.OnHighlightSelectionInLogFileToolStripMenuItemClick);
            // 
            // highlightSelectionInLogFilewordModeToolStripMenuItem
            // 
            this.highlightSelectionInLogFilewordModeToolStripMenuItem.Name = "highlightSelectionInLogFilewordModeToolStripMenuItem";
            this.highlightSelectionInLogFilewordModeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.highlightSelectionInLogFilewordModeToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.highlightSelectionInLogFilewordModeToolStripMenuItem.Text = "Highlight selection in log file (word mode)";
            this.highlightSelectionInLogFilewordModeToolStripMenuItem.Click += new System.EventHandler(this.OnHighlightSelectionInLogFilewordModeToolStripMenuItemClick);
            // 
            // filterForSelectionToolStripMenuItem
            // 
            this.filterForSelectionToolStripMenuItem.Name = "filterForSelectionToolStripMenuItem";
            this.filterForSelectionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.filterForSelectionToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.filterForSelectionToolStripMenuItem.Text = "Filter for selection";
            this.filterForSelectionToolStripMenuItem.Click += new System.EventHandler(this.OnFilterForSelectionToolStripMenuItemClick);
            // 
            // setSelectedTextAsBookmarkCommentToolStripMenuItem
            // 
            this.setSelectedTextAsBookmarkCommentToolStripMenuItem.Name = "setSelectedTextAsBookmarkCommentToolStripMenuItem";
            this.setSelectedTextAsBookmarkCommentToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.setSelectedTextAsBookmarkCommentToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.setSelectedTextAsBookmarkCommentToolStripMenuItem.Text = "Set selected text as bookmark comment";
            this.setSelectedTextAsBookmarkCommentToolStripMenuItem.Click += new System.EventHandler(this.OnSetSelectedTextAsBookmarkCommentToolStripMenuItemClick);
            // 
            // LogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1014, 656);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainerLogWindow);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LogWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.SizeChanged += new System.EventHandler(this.OnLogWindowSizeChanged);
            this.Enter += new System.EventHandler(this.OnLogWindowEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnLogWindowKeyDown);
            this.Leave += new System.EventHandler(this.OnLogWindowLeave);
            this.splitContainerLogWindow.Panel1.ResumeLayout(false);
            this.splitContainerLogWindow.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLogWindow)).EndInit();
            this.splitContainerLogWindow.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.columnFinderPanel.ResumeLayout(false);
            this.columnFinderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.dataGridContextMenuStrip.ResumeLayout(false);
            this.advancedBackPanel.ResumeLayout(false);
            this.advancedFilterSplitContainer.Panel1.ResumeLayout(false);
            this.advancedFilterSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.advancedFilterSplitContainer)).EndInit();
            this.advancedFilterSplitContainer.ResumeLayout(false);
            this.pnlProFilter.ResumeLayout(false);
            this.pnlProFilter.PerformLayout();
            this.highlightSplitContainer.Panel1.ResumeLayout(false);
            this.highlightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.highlightSplitContainer)).EndInit();
            this.highlightSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.filterGridView)).EndInit();
            this.filterContextMenuStrip.ResumeLayout(false);
            this.highlightSplitContainerBackPanel.ResumeLayout(false);
            this.highlightSplitContainerBackPanel.PerformLayout();
            this.filterListContextMenuStrip.ResumeLayout(false);
            this.pnlFilterInput.ResumeLayout(false);
            this.filterSplitContainer.Panel1.ResumeLayout(false);
            this.filterSplitContainer.Panel1.PerformLayout();
            this.filterSplitContainer.Panel2.ResumeLayout(false);
            this.filterSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterSplitContainer)).EndInit();
            this.filterSplitContainer.ResumeLayout(false);
            this.bookmarkContextMenuStrip.ResumeLayout(false);
            this.columnContextMenuStrip.ResumeLayout(false);
            this.editModeContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainerLogWindow;
		private System.Windows.Forms.Panel pnlFilterInput;
		private BufferedDataGridView dataGridView;
		private BufferedDataGridView filterGridView;
		private System.Windows.Forms.SplitContainer advancedFilterSplitContainer;
		private System.Windows.Forms.Panel pnlProFilter;
		private System.Windows.Forms.Button btnFilterToTab;
		private KnobControl filterKnobForeSpread;
		private System.Windows.Forms.Label lblForeSpread;
		private KnobControl filterKnobBackSpread;
		private System.Windows.Forms.Label lblBackSpread;
		private System.Windows.Forms.Panel pnlProFilterLabel;
		private System.Windows.Forms.CheckBox invertFilterCheckBox;
		private System.Windows.Forms.Label fuzzyLabel;
		private KnobControl fuzzyKnobControl;
		private System.Windows.Forms.CheckBox rangeCheckBox;
		private System.Windows.Forms.ComboBox filterRangeComboBox;
		private System.Windows.Forms.ContextMenuStrip dataGridContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem copyToTabToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scrollAllTabsToTimestampToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem locateLineInOriginalFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toggleBoomarkToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem markEditModeToolStripMenuItem;
		//private BufferedDataGridView boomarkDataGridView;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ContextMenuStrip bookmarkContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem deleteBookmarksToolStripMenuItem;
		private System.Windows.Forms.CheckBox columnRestrictCheckBox;
		private System.Windows.Forms.Button columnButton;
		private System.Windows.Forms.ContextMenuStrip columnContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem freezeLeftColumnsUntilHereToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveToLastColumnToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem moveLeftToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveRightToolStripMenuItem;
		private TimeSpreadingControl timeSpreadingControl;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ToolStripSeparator pluginSeparator;
		private System.Windows.Forms.ToolStripMenuItem bookmarkCommentToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ContextMenuStrip editModeContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem highlightSelectionInLogFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editModecopyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem tempHighlightsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem removeAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem makePermanentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem filterForSelectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem setSelectedTextAsBookmarkCommentToolStripMenuItem;
		private System.Windows.Forms.ToolTip helpToolTip;
		private System.Windows.Forms.SplitContainer highlightSplitContainer;
		private System.Windows.Forms.Button btnToggleHighlightPanel;
		private System.Windows.Forms.Panel highlightSplitContainerBackPanel;
		private System.Windows.Forms.Button saveFilterButton;
		private System.Windows.Forms.Button deleteFilterButton;
		private System.Windows.Forms.ListBox filterListBox;
		private System.Windows.Forms.ContextMenuStrip filterContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem setBookmarksOnSelectedLinesToolStripMenuItem;
		private System.Windows.Forms.CheckBox filterOnLoadCheckBox;
		private System.Windows.Forms.ToolStripMenuItem markCurrentFilterRangeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem syncTimestampsToToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem freeThisWindowFromTimeSyncToolStripMenuItem;
		private System.Windows.Forms.Button filterDownButton;
		private System.Windows.Forms.Button filterUpButton;
		private System.Windows.Forms.ContextMenuStrip filterListContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem filterToTabToolStripMenuItem;
		private System.Windows.Forms.CheckBox hideFilterListOnLoadCheckBox;
		private System.Windows.Forms.Panel advancedBackPanel;
		private System.Windows.Forms.ToolStripMenuItem markFilterHitsInLogViewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem highlightSelectionInLogFilewordModeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem hideColumnToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem restoreColumnsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem allColumnsToolStripMenuItem;
		private System.Windows.Forms.Label columnNamesLabel;
		private System.Windows.Forms.Panel columnFinderPanel;
		private System.Windows.Forms.ComboBox columnComboBox;
		private System.Windows.Forms.Label lblColumnName;
        private System.Windows.Forms.SplitContainer filterSplitContainer;
        private System.Windows.Forms.Label lblTextFilter;
        private System.Windows.Forms.ComboBox filterComboBox;
        private System.Windows.Forms.Button advancedButton;
        private System.Windows.Forms.CheckBox syncFilterCheckBox;
        private System.Windows.Forms.Label lblFilterCount;
        private System.Windows.Forms.CheckBox filterTailCheckBox;
        private System.Windows.Forms.CheckBox filterRegexCheckBox;
        private System.Windows.Forms.CheckBox filterCaseSensitiveCheckBox;
        private System.Windows.Forms.Button filterSearchButton;
    }
}
