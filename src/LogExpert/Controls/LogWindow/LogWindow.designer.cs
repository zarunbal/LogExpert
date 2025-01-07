using LogExpert.Dialogs;
using LogExpert.Properties;

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogWindow));
            splitContainerLogWindow = new System.Windows.Forms.SplitContainer();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            columnFinderPanel = new System.Windows.Forms.Panel();
            columnComboBox = new System.Windows.Forms.ComboBox();
            lblColumnName = new System.Windows.Forms.Label();
            dataGridView = new BufferedDataGridView();
            dataGridContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            scrollAllTabsToTimestampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            syncTimestampsToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            freeThisWindowFromTimeSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            locateLineInOriginalFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toggleBoomarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bookmarkCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            markEditModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tempHighlightsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            makePermanentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            markCurrentFilterRangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pluginSeparator = new System.Windows.Forms.ToolStripSeparator();
            timeSpreadingControl = new TimeSpreadingControl();
            advancedBackPanel = new System.Windows.Forms.Panel();
            advancedFilterSplitContainer = new System.Windows.Forms.SplitContainer();
            pnlProFilter = new System.Windows.Forms.Panel();
            columnButton = new System.Windows.Forms.Button();
            columnRestrictCheckBox = new System.Windows.Forms.CheckBox();
            rangeCheckBox = new System.Windows.Forms.CheckBox();
            filterRangeComboBox = new System.Windows.Forms.ComboBox();
            columnNamesLabel = new System.Windows.Forms.Label();
            fuzzyLabel = new System.Windows.Forms.Label();
            fuzzyKnobControl = new KnobControl();
            invertFilterCheckBox = new System.Windows.Forms.CheckBox();
            pnlProFilterLabel = new System.Windows.Forms.Panel();
            lblBackSpread = new System.Windows.Forms.Label();
            filterKnobBackSpread = new KnobControl();
            lblForeSpread = new System.Windows.Forms.Label();
            filterKnobForeSpread = new KnobControl();
            btnFilterToTab = new System.Windows.Forms.Button();
            panelBackgroundAdvancedFilterSplitContainer = new System.Windows.Forms.Panel();
            btnToggleHighlightPanel = new System.Windows.Forms.Button();
            highlightSplitContainer = new System.Windows.Forms.SplitContainer();
            filterGridView = new BufferedDataGridView();
            filterContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            setBookmarksOnSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            filterToTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            markFilterHitsInLogViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            highlightSplitContainerBackPanel = new System.Windows.Forms.Panel();
            hideFilterListOnLoadCheckBox = new System.Windows.Forms.CheckBox();
            filterDownButton = new System.Windows.Forms.Button();
            filterUpButton = new System.Windows.Forms.Button();
            filterOnLoadCheckBox = new System.Windows.Forms.CheckBox();
            saveFilterButton = new System.Windows.Forms.Button();
            deleteFilterButton = new System.Windows.Forms.Button();
            filterListBox = new System.Windows.Forms.ListBox();
            filterListContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pnlFilterInput = new System.Windows.Forms.Panel();
            filterSplitContainer = new System.Windows.Forms.SplitContainer();
            filterComboBox = new System.Windows.Forms.ComboBox();
            lblTextFilter = new System.Windows.Forms.Label();
            advancedButton = new System.Windows.Forms.Button();
            syncFilterCheckBox = new System.Windows.Forms.CheckBox();
            lblFilterCount = new System.Windows.Forms.Label();
            filterTailCheckBox = new System.Windows.Forms.CheckBox();
            filterRegexCheckBox = new System.Windows.Forms.CheckBox();
            filterCaseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            filterSearchButton = new System.Windows.Forms.Button();
            bookmarkContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            deleteBookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            columnContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            freezeLeftColumnsUntilHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            moveToLastColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            hideColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            restoreColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            allColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editModeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            editModecopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            highlightSelectionInLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            highlightSelectionInLogFilewordModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            filterForSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            setSelectedTextAsBookmarkCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolTip = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainerLogWindow).BeginInit();
            splitContainerLogWindow.Panel1.SuspendLayout();
            splitContainerLogWindow.Panel2.SuspendLayout();
            splitContainerLogWindow.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            columnFinderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            dataGridContextMenuStrip.SuspendLayout();
            advancedBackPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)advancedFilterSplitContainer).BeginInit();
            advancedFilterSplitContainer.Panel1.SuspendLayout();
            advancedFilterSplitContainer.Panel2.SuspendLayout();
            advancedFilterSplitContainer.SuspendLayout();
            pnlProFilter.SuspendLayout();
            panelBackgroundAdvancedFilterSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)highlightSplitContainer).BeginInit();
            highlightSplitContainer.Panel1.SuspendLayout();
            highlightSplitContainer.Panel2.SuspendLayout();
            highlightSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)filterGridView).BeginInit();
            filterContextMenuStrip.SuspendLayout();
            highlightSplitContainerBackPanel.SuspendLayout();
            filterListContextMenuStrip.SuspendLayout();
            pnlFilterInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)filterSplitContainer).BeginInit();
            filterSplitContainer.Panel1.SuspendLayout();
            filterSplitContainer.Panel2.SuspendLayout();
            filterSplitContainer.SuspendLayout();
            bookmarkContextMenuStrip.SuspendLayout();
            columnContextMenuStrip.SuspendLayout();
            editModeContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerLogWindow
            // 
            splitContainerLogWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            splitContainerLogWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerLogWindow.Location = new Point(0, 0);
            splitContainerLogWindow.Margin = new System.Windows.Forms.Padding(0);
            splitContainerLogWindow.Name = "splitContainerLogWindow";
            splitContainerLogWindow.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLogWindow.Panel1
            // 
            splitContainerLogWindow.Panel1.Controls.Add(tableLayoutPanel1);
            splitContainerLogWindow.Panel1MinSize = 50;
            // 
            // splitContainerLogWindow.Panel2
            // 
            splitContainerLogWindow.Panel2.Controls.Add(advancedBackPanel);
            splitContainerLogWindow.Panel2.Controls.Add(pnlFilterInput);
            splitContainerLogWindow.Panel2MinSize = 50;
            splitContainerLogWindow.Size = new Size(1145, 920);
            splitContainerLogWindow.SplitterDistance = 405;
            splitContainerLogWindow.TabIndex = 9;
            splitContainerLogWindow.SplitterMoved += OnSplitContainerSplitterMoved;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 181F));
            tableLayoutPanel1.Controls.Add(columnFinderPanel, 0, 0);
            tableLayoutPanel1.Controls.Add(dataGridView, 0, 1);
            tableLayoutPanel1.Controls.Add(timeSpreadingControl, 1, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.ForeColor = SystemColors.ControlText;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1143, 403);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // columnFinderPanel
            // 
            columnFinderPanel.Controls.Add(columnComboBox);
            columnFinderPanel.Controls.Add(lblColumnName);
            columnFinderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            columnFinderPanel.Location = new Point(4, 4);
            columnFinderPanel.Name = "columnFinderPanel";
            columnFinderPanel.Size = new Size(841, 22);
            columnFinderPanel.TabIndex = 2;
            // 
            // columnComboBox
            // 
            columnComboBox.FormattingEnabled = true;
            columnComboBox.Location = new Point(125, 1);
            columnComboBox.MaxDropDownItems = 15;
            columnComboBox.Name = "columnComboBox";
            columnComboBox.Size = new Size(181, 28);
            columnComboBox.TabIndex = 1;
            helpToolTip.SetToolTip(columnComboBox, "Select column to scroll to");
            columnComboBox.SelectionChangeCommitted += OnColumnComboBoxSelectionChangeCommitted;
            columnComboBox.KeyDown += OnColumnComboBoxKeyDown;
            columnComboBox.PreviewKeyDown += OnColumnComboBoxPreviewKeyDown;
            // 
            // lblColumnName
            // 
            lblColumnName.AutoSize = true;
            lblColumnName.Location = new Point(8, 4);
            lblColumnName.Name = "lblColumnName";
            lblColumnName.Size = new Size(117, 20);
            lblColumnName.TabIndex = 0;
            lblColumnName.Text = "Column name:";
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToOrderColumns = true;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.BackgroundColor = SystemColors.Window;
            dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.ContextMenuStrip = dataGridContextMenuStrip;
            dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dataGridView.EditModeMenuStrip = null;
            dataGridView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            dataGridView.Location = new Point(1, 30);
            dataGridView.Margin = new System.Windows.Forms.Padding(0);
            dataGridView.Name = "dataGridView";
            dataGridView.PaintWithOverlays = false;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowHeadersWidth = 62;
            dataGridView.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            dataGridView.RowTemplate.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            dataGridView.RowTemplate.Height = 15;
            dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView.ShowCellErrors = false;
            dataGridView.ShowCellToolTips = false;
            dataGridView.ShowEditingIcon = false;
            dataGridView.ShowRowErrors = false;
            dataGridView.Size = new Size(847, 372);
            dataGridView.TabIndex = 0;
            dataGridView.VirtualMode = true;
            dataGridView.OverlayDoubleClicked += OnDataGridViewOverlayDoubleClicked;
            dataGridView.CellClick += OnDataGridViewCellClick;
            dataGridView.CellContentDoubleClick += OnDataGridViewCellContentDoubleClick;
            dataGridView.CellContextMenuStripNeeded += OnDataGridViewCellContextMenuStripNeeded;
            dataGridView.CellDoubleClick += OnDataGridViewCellDoubleClick;
            dataGridView.CellValuePushed += OnDataGridViewCellValuePushed;
            dataGridView.RowHeightInfoNeeded += OnDataGridViewRowHeightInfoNeeded;
            dataGridView.RowUnshared += OnDataGridViewRowUnshared;
            dataGridView.Scroll += OnDataGridViewScroll;
            dataGridView.SelectionChanged += OnDataGridViewSelectionChanged;
            dataGridView.Paint += OnDataGridViewPaint;
            dataGridView.Enter += OnDataGridViewEnter;
            dataGridView.KeyDown += OnDataGridViewKeyDown;
            dataGridView.Leave += OnDataGridViewLeave;
            dataGridView.PreviewKeyDown += OnDataGridViewPreviewKeyDown;
            dataGridView.Resize += OnDataGridViewResize;
            // 
            // dataGridContextMenuStrip
            // 
            dataGridContextMenuStrip.ImageScalingSize = new Size(24, 24);
            dataGridContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyToolStripMenuItem, copyToTabToolStripMenuItem, toolStripSeparator1, scrollAllTabsToTimestampToolStripMenuItem, syncTimestampsToToolStripMenuItem, freeThisWindowFromTimeSyncToolStripMenuItem, locateLineInOriginalFileToolStripMenuItem, toolStripSeparator2, toggleBoomarkToolStripMenuItem, bookmarkCommentToolStripMenuItem, toolStripSeparator4, markEditModeToolStripMenuItem, tempHighlightsToolStripMenuItem, markCurrentFilterRangeToolStripMenuItem, pluginSeparator });
            dataGridContextMenuStrip.Name = "dataGridContextMenuStrip";
            dataGridContextMenuStrip.Size = new Size(398, 380);
            dataGridContextMenuStrip.Opening += OnDataGridContextMenuStripOpening;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            copyToolStripMenuItem.Size = new Size(397, 32);
            copyToolStripMenuItem.Text = "Copy to clipboard";
            copyToolStripMenuItem.Click += OnCopyToolStripMenuItemClick;
            // 
            // copyToTabToolStripMenuItem
            // 
            copyToTabToolStripMenuItem.Name = "copyToTabToolStripMenuItem";
            copyToTabToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T;
            copyToTabToolStripMenuItem.Size = new Size(397, 32);
            copyToTabToolStripMenuItem.Text = "Copy to new tab";
            copyToTabToolStripMenuItem.ToolTipText = "Copy marked lines into a new tab window";
            copyToTabToolStripMenuItem.Click += OnCopyToTabToolStripMenuItemClick;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(394, 6);
            // 
            // scrollAllTabsToTimestampToolStripMenuItem
            // 
            scrollAllTabsToTimestampToolStripMenuItem.Name = "scrollAllTabsToTimestampToolStripMenuItem";
            scrollAllTabsToTimestampToolStripMenuItem.Size = new Size(397, 32);
            scrollAllTabsToTimestampToolStripMenuItem.Text = "Scroll all tabs to current timestamp";
            scrollAllTabsToTimestampToolStripMenuItem.ToolTipText = "Scolls all open tabs to the selected timestamp, if possible";
            scrollAllTabsToTimestampToolStripMenuItem.Click += OnScrollAllTabsToTimestampToolStripMenuItemClick;
            // 
            // syncTimestampsToToolStripMenuItem
            // 
            syncTimestampsToToolStripMenuItem.Name = "syncTimestampsToToolStripMenuItem";
            syncTimestampsToToolStripMenuItem.Size = new Size(397, 32);
            syncTimestampsToToolStripMenuItem.Text = "Time synced files";
            // 
            // freeThisWindowFromTimeSyncToolStripMenuItem
            // 
            freeThisWindowFromTimeSyncToolStripMenuItem.Name = "freeThisWindowFromTimeSyncToolStripMenuItem";
            freeThisWindowFromTimeSyncToolStripMenuItem.Size = new Size(397, 32);
            freeThisWindowFromTimeSyncToolStripMenuItem.Text = "Free this window from time sync";
            freeThisWindowFromTimeSyncToolStripMenuItem.Click += OnFreeThisWindowFromTimeSyncToolStripMenuItemClick;
            // 
            // locateLineInOriginalFileToolStripMenuItem
            // 
            locateLineInOriginalFileToolStripMenuItem.Name = "locateLineInOriginalFileToolStripMenuItem";
            locateLineInOriginalFileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L;
            locateLineInOriginalFileToolStripMenuItem.Size = new Size(397, 32);
            locateLineInOriginalFileToolStripMenuItem.Text = "Locate filtered line in original file";
            locateLineInOriginalFileToolStripMenuItem.Click += OnLocateLineInOriginalFileToolStripMenuItemClick;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(394, 6);
            // 
            // toggleBoomarkToolStripMenuItem
            // 
            toggleBoomarkToolStripMenuItem.Name = "toggleBoomarkToolStripMenuItem";
            toggleBoomarkToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2;
            toggleBoomarkToolStripMenuItem.Size = new Size(397, 32);
            toggleBoomarkToolStripMenuItem.Text = "Toggle Boomark";
            toggleBoomarkToolStripMenuItem.Click += OnToggleBoomarkToolStripMenuItemClick;
            // 
            // bookmarkCommentToolStripMenuItem
            // 
            bookmarkCommentToolStripMenuItem.Name = "bookmarkCommentToolStripMenuItem";
            bookmarkCommentToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F2;
            bookmarkCommentToolStripMenuItem.Size = new Size(397, 32);
            bookmarkCommentToolStripMenuItem.Text = "Bookmark comment...";
            bookmarkCommentToolStripMenuItem.ToolTipText = "Edit the comment for a bookmark";
            bookmarkCommentToolStripMenuItem.Click += OnBookmarkCommentToolStripMenuItemClick;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(394, 6);
            // 
            // markEditModeToolStripMenuItem
            // 
            markEditModeToolStripMenuItem.Name = "markEditModeToolStripMenuItem";
            markEditModeToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            markEditModeToolStripMenuItem.Size = new Size(397, 32);
            markEditModeToolStripMenuItem.Text = "Mark/Edit-Mode";
            markEditModeToolStripMenuItem.Click += OnMarkEditModeToolStripMenuItemClick;
            // 
            // tempHighlightsToolStripMenuItem
            // 
            tempHighlightsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { removeAllToolStripMenuItem, makePermanentToolStripMenuItem });
            tempHighlightsToolStripMenuItem.Name = "tempHighlightsToolStripMenuItem";
            tempHighlightsToolStripMenuItem.Size = new Size(397, 32);
            tempHighlightsToolStripMenuItem.Text = "Temp Highlights";
            // 
            // removeAllToolStripMenuItem
            // 
            removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            removeAllToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.H;
            removeAllToolStripMenuItem.Size = new Size(312, 34);
            removeAllToolStripMenuItem.Text = "Remove all";
            removeAllToolStripMenuItem.Click += OnRemoveAllToolStripMenuItemClick;
            // 
            // makePermanentToolStripMenuItem
            // 
            makePermanentToolStripMenuItem.Name = "makePermanentToolStripMenuItem";
            makePermanentToolStripMenuItem.Size = new Size(312, 34);
            makePermanentToolStripMenuItem.Text = "Make all permanent";
            makePermanentToolStripMenuItem.Click += OnMakePermanentToolStripMenuItemClick;
            // 
            // markCurrentFilterRangeToolStripMenuItem
            // 
            markCurrentFilterRangeToolStripMenuItem.Name = "markCurrentFilterRangeToolStripMenuItem";
            markCurrentFilterRangeToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R;
            markCurrentFilterRangeToolStripMenuItem.Size = new Size(397, 32);
            markCurrentFilterRangeToolStripMenuItem.Text = "Mark current filter range";
            markCurrentFilterRangeToolStripMenuItem.Click += OnMarkCurrentFilterRangeToolStripMenuItemClick;
            // 
            // pluginSeparator
            // 
            pluginSeparator.Name = "pluginSeparator";
            pluginSeparator.Size = new Size(394, 6);
            // 
            // timeSpreadingControl
            // 
            timeSpreadingControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            timeSpreadingControl.Font = new Font("Verdana", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            timeSpreadingControl.ForeColor = Color.Teal;
            timeSpreadingControl.Location = new Point(1125, 30);
            timeSpreadingControl.Margin = new System.Windows.Forms.Padding(2, 0, 1, 0);
            timeSpreadingControl.Name = "timeSpreadingControl";
            timeSpreadingControl.ReverseAlpha = false;
            timeSpreadingControl.Size = new Size(16, 372);
            timeSpreadingControl.TabIndex = 1;
            // 
            // advancedBackPanel
            // 
            advancedBackPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            advancedBackPanel.Controls.Add(advancedFilterSplitContainer);
            advancedBackPanel.Location = new Point(3, 35);
            advancedBackPanel.Name = "advancedBackPanel";
            advancedBackPanel.Size = new Size(1138, 471);
            advancedBackPanel.TabIndex = 3;
            // 
            // advancedFilterSplitContainer
            // 
            advancedFilterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            advancedFilterSplitContainer.IsSplitterFixed = true;
            advancedFilterSplitContainer.Location = new Point(0, 0);
            advancedFilterSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            advancedFilterSplitContainer.Name = "advancedFilterSplitContainer";
            advancedFilterSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // advancedFilterSplitContainer.Panel1
            // 
            advancedFilterSplitContainer.Panel1.Controls.Add(pnlProFilter);
            advancedFilterSplitContainer.Panel1MinSize = 100;
            // 
            // advancedFilterSplitContainer.Panel2
            // 
            advancedFilterSplitContainer.Panel2.Controls.Add(panelBackgroundAdvancedFilterSplitContainer);
            advancedFilterSplitContainer.Panel2MinSize = 50;
            advancedFilterSplitContainer.Size = new Size(1138, 471);
            advancedFilterSplitContainer.SplitterDistance = 105;
            advancedFilterSplitContainer.SplitterWidth = 2;
            advancedFilterSplitContainer.TabIndex = 2;
            // 
            // pnlProFilter
            // 
            pnlProFilter.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlProFilter.BackColor = Color.FromArgb(227, 227, 227);
            pnlProFilter.Controls.Add(columnButton);
            pnlProFilter.Controls.Add(columnRestrictCheckBox);
            pnlProFilter.Controls.Add(rangeCheckBox);
            pnlProFilter.Controls.Add(filterRangeComboBox);
            pnlProFilter.Controls.Add(columnNamesLabel);
            pnlProFilter.Controls.Add(fuzzyLabel);
            pnlProFilter.Controls.Add(fuzzyKnobControl);
            pnlProFilter.Controls.Add(invertFilterCheckBox);
            pnlProFilter.Controls.Add(pnlProFilterLabel);
            pnlProFilter.Controls.Add(lblBackSpread);
            pnlProFilter.Controls.Add(filterKnobBackSpread);
            pnlProFilter.Controls.Add(lblForeSpread);
            pnlProFilter.Controls.Add(filterKnobForeSpread);
            pnlProFilter.Controls.Add(btnFilterToTab);
            pnlProFilter.Location = new Point(0, 3);
            pnlProFilter.Name = "pnlProFilter";
            pnlProFilter.Size = new Size(1135, 100);
            pnlProFilter.TabIndex = 0;
            // 
            // columnButton
            // 
            columnButton.Enabled = false;
            columnButton.Location = new Point(750, 30);
            columnButton.Name = "columnButton";
            columnButton.Size = new Size(71, 23);
            columnButton.TabIndex = 15;
            columnButton.Text = "Columns...";
            helpToolTip.SetToolTip(columnButton, "Choose columns for 'Column restrict'");
            columnButton.UseVisualStyleBackColor = true;
            columnButton.Click += OnColumnButtonClick;
            // 
            // columnRestrictCheckBox
            // 
            columnRestrictCheckBox.AutoSize = true;
            columnRestrictCheckBox.Location = new Point(594, 38);
            columnRestrictCheckBox.Name = "columnRestrictCheckBox";
            columnRestrictCheckBox.Size = new Size(150, 24);
            columnRestrictCheckBox.TabIndex = 14;
            columnRestrictCheckBox.Text = "Column restrict";
            helpToolTip.SetToolTip(columnRestrictCheckBox, "Restrict search to columns");
            columnRestrictCheckBox.UseVisualStyleBackColor = true;
            columnRestrictCheckBox.CheckedChanged += OnColumnRestrictCheckBoxCheckedChanged;
            // 
            // rangeCheckBox
            // 
            rangeCheckBox.AutoSize = true;
            rangeCheckBox.Location = new Point(73, 38);
            rangeCheckBox.Name = "rangeCheckBox";
            rangeCheckBox.Size = new Size(139, 24);
            rangeCheckBox.TabIndex = 13;
            rangeCheckBox.Text = "Range search";
            helpToolTip.SetToolTip(rangeCheckBox, "Enable a special search mode which filters all content between the 2 given search terms.");
            rangeCheckBox.UseVisualStyleBackColor = true;
            rangeCheckBox.CheckedChanged += OnRangeCheckBoxCheckedChanged;
            // 
            // filterRangeComboBox
            // 
            filterRangeComboBox.Enabled = false;
            filterRangeComboBox.FormattingEnabled = true;
            filterRangeComboBox.Location = new Point(73, 11);
            filterRangeComboBox.Name = "filterRangeComboBox";
            filterRangeComboBox.Size = new Size(207, 28);
            filterRangeComboBox.TabIndex = 12;
            helpToolTip.SetToolTip(filterRangeComboBox, "2nd search string ('end string') when using the range search");
            filterRangeComboBox.TextChanged += OnFilterRangeComboBoxTextChanged;
            // 
            // columnNamesLabel
            // 
            columnNamesLabel.AutoSize = true;
            columnNamesLabel.Location = new Point(827, 35);
            columnNamesLabel.Name = "columnNamesLabel";
            columnNamesLabel.Size = new Size(118, 20);
            columnNamesLabel.TabIndex = 11;
            columnNamesLabel.Text = "column names";
            // 
            // fuzzyLabel
            // 
            fuzzyLabel.AutoSize = true;
            fuzzyLabel.Location = new Point(502, 38);
            fuzzyLabel.Name = "fuzzyLabel";
            fuzzyLabel.Size = new Size(90, 20);
            fuzzyLabel.TabIndex = 11;
            fuzzyLabel.Text = "Fuzzyness";
            // 
            // fuzzyKnobControl
            // 
            fuzzyKnobControl.DragSensitivity = 6;
            fuzzyKnobControl.Font = new Font("Verdana", 6F, FontStyle.Regular, GraphicsUnit.Point, 0);
            fuzzyKnobControl.Location = new Point(521, 7);
            fuzzyKnobControl.Margin = new System.Windows.Forms.Padding(2);
            fuzzyKnobControl.MaxValue = 0;
            fuzzyKnobControl.MinValue = 0;
            fuzzyKnobControl.Name = "fuzzyKnobControl";
            fuzzyKnobControl.Size = new Size(17, 29);
            fuzzyKnobControl.TabIndex = 10;
            helpToolTip.SetToolTip(fuzzyKnobControl, "Fuzzy search level (0 = fuzzy off)");
            fuzzyKnobControl.Value = 0;
            fuzzyKnobControl.ValueChanged += OnFuzzyKnobControlValueChanged;
            // 
            // invertFilterCheckBox
            // 
            invertFilterCheckBox.AutoSize = true;
            invertFilterCheckBox.Location = new Point(594, 7);
            invertFilterCheckBox.Name = "invertFilterCheckBox";
            invertFilterCheckBox.Size = new Size(127, 24);
            invertFilterCheckBox.TabIndex = 8;
            invertFilterCheckBox.Text = "Invert Match";
            helpToolTip.SetToolTip(invertFilterCheckBox, "Invert the search result");
            invertFilterCheckBox.UseVisualStyleBackColor = true;
            invertFilterCheckBox.CheckedChanged += OnInvertFilterCheckBoxCheckedChanged;
            // 
            // pnlProFilterLabel
            // 
            pnlProFilterLabel.BackgroundImage = Resources.Pro_Filter;
            pnlProFilterLabel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pnlProFilterLabel.Location = new Point(5, 7);
            pnlProFilterLabel.Name = "pnlProFilterLabel";
            pnlProFilterLabel.Size = new Size(60, 44);
            pnlProFilterLabel.TabIndex = 7;
            // 
            // lblBackSpread
            // 
            lblBackSpread.AutoSize = true;
            lblBackSpread.Location = new Point(287, 38);
            lblBackSpread.Name = "lblBackSpread";
            lblBackSpread.Size = new Size(110, 20);
            lblBackSpread.TabIndex = 6;
            lblBackSpread.Text = "Back Spread ";
            // 
            // filterKnobBackSpread
            // 
            filterKnobBackSpread.DragSensitivity = 3;
            filterKnobBackSpread.Font = new Font("Verdana", 6F, FontStyle.Regular, GraphicsUnit.Point, 0);
            filterKnobBackSpread.Location = new Point(313, 7);
            filterKnobBackSpread.Margin = new System.Windows.Forms.Padding(2);
            filterKnobBackSpread.MaxValue = 0;
            filterKnobBackSpread.MinValue = 0;
            filterKnobBackSpread.Name = "filterKnobBackSpread";
            filterKnobBackSpread.Size = new Size(17, 29);
            filterKnobBackSpread.TabIndex = 5;
            helpToolTip.SetToolTip(filterKnobBackSpread, "Add preceding lines to search result (Drag up/down, press Shift for finer pitch)");
            filterKnobBackSpread.Value = 0;
            // 
            // lblForeSpread
            // 
            lblForeSpread.AutoSize = true;
            lblForeSpread.Location = new Point(397, 38);
            lblForeSpread.Name = "lblForeSpread";
            lblForeSpread.Size = new Size(101, 20);
            lblForeSpread.TabIndex = 2;
            lblForeSpread.Text = "Fore Spread";
            // 
            // filterKnobForeSpread
            // 
            filterKnobForeSpread.DragSensitivity = 3;
            filterKnobForeSpread.Font = new Font("Verdana", 6F, FontStyle.Regular, GraphicsUnit.Point, 0);
            filterKnobForeSpread.Location = new Point(420, 7);
            filterKnobForeSpread.Margin = new System.Windows.Forms.Padding(2);
            filterKnobForeSpread.MaxValue = 0;
            filterKnobForeSpread.MinValue = 0;
            filterKnobForeSpread.Name = "filterKnobForeSpread";
            filterKnobForeSpread.Size = new Size(17, 29);
            filterKnobForeSpread.TabIndex = 1;
            helpToolTip.SetToolTip(filterKnobForeSpread, "Add following lines to search result (Drag up/down, press Shift for finer pitch)");
            filterKnobForeSpread.Value = 0;
            // 
            // btnFilterToTab
            // 
            btnFilterToTab.Location = new Point(750, 3);
            btnFilterToTab.Name = "btnFilterToTab";
            btnFilterToTab.Size = new Size(71, 23);
            btnFilterToTab.TabIndex = 0;
            btnFilterToTab.Text = "Filter to Tab";
            helpToolTip.SetToolTip(btnFilterToTab, "Launch a new tab with filtered content");
            btnFilterToTab.UseVisualStyleBackColor = true;
            btnFilterToTab.Click += OnFilterToTabButtonClick;
            // 
            // panelBackgroundAdvancedFilterSplitContainer
            // 
            panelBackgroundAdvancedFilterSplitContainer.Controls.Add(btnToggleHighlightPanel);
            panelBackgroundAdvancedFilterSplitContainer.Controls.Add(highlightSplitContainer);
            panelBackgroundAdvancedFilterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            panelBackgroundAdvancedFilterSplitContainer.Location = new Point(0, 0);
            panelBackgroundAdvancedFilterSplitContainer.Name = "panelBackgroundAdvancedFilterSplitContainer";
            panelBackgroundAdvancedFilterSplitContainer.Size = new Size(1138, 364);
            panelBackgroundAdvancedFilterSplitContainer.TabIndex = 7;
            // 
            // btnToggleHighlightPanel
            // 
            btnToggleHighlightPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnToggleHighlightPanel.Image = Resources.PanelOpen;
            btnToggleHighlightPanel.Location = new Point(1115, 1);
            btnToggleHighlightPanel.Name = "btnToggleHighlightPanel";
            btnToggleHighlightPanel.Size = new Size(20, 21);
            btnToggleHighlightPanel.TabIndex = 6;
            helpToolTip.SetToolTip(btnToggleHighlightPanel, "Open or close a list with saved filters");
            btnToggleHighlightPanel.UseVisualStyleBackColor = true;
            btnToggleHighlightPanel.Click += OnToggleHighlightPanelButtonClick;
            // 
            // highlightSplitContainer
            // 
            highlightSplitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            highlightSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            highlightSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            highlightSplitContainer.IsSplitterFixed = true;
            highlightSplitContainer.Location = new Point(0, 3);
            highlightSplitContainer.Name = "highlightSplitContainer";
            // 
            // highlightSplitContainer.Panel1
            // 
            highlightSplitContainer.Panel1.Controls.Add(filterGridView);
            // 
            // highlightSplitContainer.Panel2
            // 
            highlightSplitContainer.Panel2.Controls.Add(highlightSplitContainerBackPanel);
            highlightSplitContainer.Panel2MinSize = 30;
            highlightSplitContainer.Size = new Size(1112, 361);
            highlightSplitContainer.SplitterDistance = 743;
            highlightSplitContainer.TabIndex = 2;
            // 
            // filterGridView
            // 
            filterGridView.AllowUserToAddRows = false;
            filterGridView.AllowUserToDeleteRows = false;
            filterGridView.AllowUserToOrderColumns = true;
            filterGridView.AllowUserToResizeRows = false;
            filterGridView.BackgroundColor = SystemColors.Window;
            filterGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            filterGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            filterGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            filterGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            filterGridView.ContextMenuStrip = filterContextMenuStrip;
            filterGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            filterGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            filterGridView.EditModeMenuStrip = null;
            filterGridView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            filterGridView.Location = new Point(0, 0);
            filterGridView.Margin = new System.Windows.Forms.Padding(0);
            filterGridView.Name = "filterGridView";
            filterGridView.PaintWithOverlays = false;
            filterGridView.ReadOnly = true;
            filterGridView.RowHeadersVisible = false;
            filterGridView.RowHeadersWidth = 62;
            filterGridView.RowTemplate.Height = 15;
            filterGridView.RowTemplate.ReadOnly = true;
            filterGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            filterGridView.ShowCellErrors = false;
            filterGridView.ShowCellToolTips = false;
            filterGridView.ShowEditingIcon = false;
            filterGridView.ShowRowErrors = false;
            filterGridView.Size = new Size(741, 359);
            filterGridView.TabIndex = 1;
            filterGridView.VirtualMode = true;
            filterGridView.CellContextMenuStripNeeded += OnFilterGridViewCellContextMenuStripNeeded;
            filterGridView.CellDoubleClick += OnFilterGridViewCellDoubleClick;
            filterGridView.ColumnDividerDoubleClick += OnFilterGridViewColumnDividerDoubleClick;
            filterGridView.RowHeightInfoNeeded += OnFilterGridViewRowHeightInfoNeeded;
            filterGridView.Enter += OnFilterGridViewEnter;
            filterGridView.KeyDown += OnFilterGridViewKeyDown;
            filterGridView.Leave += OnFilterGridViewLeave;
            // 
            // filterContextMenuStrip
            // 
            filterContextMenuStrip.ImageScalingSize = new Size(24, 24);
            filterContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { setBookmarksOnSelectedLinesToolStripMenuItem, filterToTabToolStripMenuItem, markFilterHitsInLogViewToolStripMenuItem });
            filterContextMenuStrip.Name = "filterContextMenuStrip";
            filterContextMenuStrip.Size = new Size(340, 100);
            // 
            // setBookmarksOnSelectedLinesToolStripMenuItem
            // 
            setBookmarksOnSelectedLinesToolStripMenuItem.Name = "setBookmarksOnSelectedLinesToolStripMenuItem";
            setBookmarksOnSelectedLinesToolStripMenuItem.Size = new Size(339, 32);
            setBookmarksOnSelectedLinesToolStripMenuItem.Text = "Set bookmarks on selected lines";
            setBookmarksOnSelectedLinesToolStripMenuItem.Click += OnSetBookmarksOnSelectedLinesToolStripMenuItemClick;
            // 
            // filterToTabToolStripMenuItem
            // 
            filterToTabToolStripMenuItem.Name = "filterToTabToolStripMenuItem";
            filterToTabToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.T;
            filterToTabToolStripMenuItem.Size = new Size(339, 32);
            filterToTabToolStripMenuItem.Text = "Filter to new tab";
            filterToTabToolStripMenuItem.Click += OnFilterToTabToolStripMenuItemClick;
            // 
            // markFilterHitsInLogViewToolStripMenuItem
            // 
            markFilterHitsInLogViewToolStripMenuItem.Name = "markFilterHitsInLogViewToolStripMenuItem";
            markFilterHitsInLogViewToolStripMenuItem.Size = new Size(339, 32);
            markFilterHitsInLogViewToolStripMenuItem.Text = "Mark filter hits in log view";
            markFilterHitsInLogViewToolStripMenuItem.Click += OnMarkFilterHitsInLogViewToolStripMenuItemClick;
            // 
            // highlightSplitContainerBackPanel
            // 
            highlightSplitContainerBackPanel.Controls.Add(hideFilterListOnLoadCheckBox);
            highlightSplitContainerBackPanel.Controls.Add(filterDownButton);
            highlightSplitContainerBackPanel.Controls.Add(filterUpButton);
            highlightSplitContainerBackPanel.Controls.Add(filterOnLoadCheckBox);
            highlightSplitContainerBackPanel.Controls.Add(saveFilterButton);
            highlightSplitContainerBackPanel.Controls.Add(deleteFilterButton);
            highlightSplitContainerBackPanel.Controls.Add(filterListBox);
            highlightSplitContainerBackPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            highlightSplitContainerBackPanel.Location = new Point(0, 0);
            highlightSplitContainerBackPanel.Name = "highlightSplitContainerBackPanel";
            highlightSplitContainerBackPanel.Size = new Size(363, 359);
            highlightSplitContainerBackPanel.TabIndex = 1;
            // 
            // hideFilterListOnLoadCheckBox
            // 
            hideFilterListOnLoadCheckBox.AutoSize = true;
            hideFilterListOnLoadCheckBox.Location = new Point(287, 134);
            hideFilterListOnLoadCheckBox.Name = "hideFilterListOnLoadCheckBox";
            hideFilterListOnLoadCheckBox.Size = new Size(105, 24);
            hideFilterListOnLoadCheckBox.TabIndex = 20;
            hideFilterListOnLoadCheckBox.Text = "Auto hide";
            helpToolTip.SetToolTip(hideFilterListOnLoadCheckBox, "Hides the filter list after loading a filter");
            hideFilterListOnLoadCheckBox.UseVisualStyleBackColor = true;
            hideFilterListOnLoadCheckBox.MouseClick += OnHideFilterListOnLoadCheckBoxMouseClick;
            // 
            // filterDownButton
            // 
            filterDownButton.Image = Resources.ArrowDown;
            filterDownButton.Location = new Point(325, 70);
            filterDownButton.Name = "filterDownButton";
            filterDownButton.SizeChanged += new EventHandler(this.OnButtonSizeChanged);
            filterDownButton.Size = new Size(35, 24);
            filterDownButton.TabIndex = 19;
            helpToolTip.SetToolTip(filterDownButton, "Move the selected entry down in the list");
            filterDownButton.UseVisualStyleBackColor = true;
            filterDownButton.Click += OnFilterDownButtonClick;
            // 
            // filterUpButton
            // 
            filterUpButton.Image = Resources.ArrowUp;
            filterUpButton.Location = new Point(287, 70);
            filterUpButton.Name = "filterUpButton";
            filterUpButton.Size = new Size(35, 24);
            filterUpButton.SizeChanged += new EventHandler(this.OnButtonSizeChanged);
            filterUpButton.TabIndex = 18;
            helpToolTip.SetToolTip(filterUpButton, "Move the selected entry up in the list");
            filterUpButton.UseVisualStyleBackColor = true;
            filterUpButton.Click += OnFilterUpButtonClick;
            // 
            // filterOnLoadCheckBox
            // 
            filterOnLoadCheckBox.AutoSize = true;
            filterOnLoadCheckBox.Location = new Point(287, 110);
            filterOnLoadCheckBox.Name = "filterOnLoadCheckBox";
            filterOnLoadCheckBox.Size = new Size(108, 24);
            filterOnLoadCheckBox.TabIndex = 17;
            filterOnLoadCheckBox.Text = "Auto start";
            helpToolTip.SetToolTip(filterOnLoadCheckBox, "Start immediate filtering after loading a saved filter");
            filterOnLoadCheckBox.UseVisualStyleBackColor = true;
            filterOnLoadCheckBox.KeyPress += OnFilterOnLoadCheckBoxKeyPress;
            filterOnLoadCheckBox.MouseClick += OnFilterOnLoadCheckBoxMouseClick;
            // 
            // saveFilterButton
            // 
            saveFilterButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            saveFilterButton.Location = new Point(287, 11);
            saveFilterButton.Name = "saveFilterButton";
            saveFilterButton.Size = new Size(73, 23);
            saveFilterButton.TabIndex = 16;
            saveFilterButton.Text = "Save filter";
            saveFilterButton.UseVisualStyleBackColor = true;
            saveFilterButton.Click += OnSaveFilterButtonClick;
            // 
            // deleteFilterButton
            // 
            deleteFilterButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            deleteFilterButton.Location = new Point(287, 40);
            deleteFilterButton.Name = "deleteFilterButton";
            deleteFilterButton.Size = new Size(73, 23);
            deleteFilterButton.TabIndex = 3;
            deleteFilterButton.Text = "Delete";
            deleteFilterButton.UseVisualStyleBackColor = true;
            deleteFilterButton.Click += OnDeleteFilterButtonClick;
            // 
            // filterListBox
            // 
            filterListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            filterListBox.ContextMenuStrip = filterListContextMenuStrip;
            filterListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            filterListBox.Font = new Font("Courier New", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            filterListBox.FormattingEnabled = true;
            filterListBox.IntegralHeight = false;
            filterListBox.ItemHeight = 25;
            filterListBox.Location = new Point(3, 3);
            filterListBox.Name = "filterListBox";
            filterListBox.Size = new Size(278, 353);
            filterListBox.TabIndex = 0;
            helpToolTip.SetToolTip(filterListBox, "Doubleclick to load a saved filter");
            filterListBox.DrawItem += OnFilterListBoxDrawItem;
            filterListBox.MouseDoubleClick += OnFilterListBoxMouseDoubleClick;
            // 
            // filterListContextMenuStrip
            // 
            filterListContextMenuStrip.ImageScalingSize = new Size(24, 24);
            filterListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { colorToolStripMenuItem });
            filterListContextMenuStrip.Name = "filterListContextMenuStrip";
            filterListContextMenuStrip.Size = new Size(140, 36);
            // 
            // colorToolStripMenuItem
            // 
            colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            colorToolStripMenuItem.Size = new Size(139, 32);
            colorToolStripMenuItem.Text = "Color...";
            colorToolStripMenuItem.Click += OnColorToolStripMenuItemClick;
            // 
            // pnlFilterInput
            // 
            pnlFilterInput.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlFilterInput.Controls.Add(filterSplitContainer);
            pnlFilterInput.Location = new Point(3, 2);
            pnlFilterInput.Name = "pnlFilterInput";
            pnlFilterInput.Size = new Size(1138, 32);
            pnlFilterInput.TabIndex = 0;
            // 
            // filterSplitContainer
            // 
            filterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            filterSplitContainer.Location = new Point(0, 0);
            filterSplitContainer.Name = "filterSplitContainer";
            // 
            // filterSplitContainer.Panel1
            // 
            filterSplitContainer.Panel1.Controls.Add(filterComboBox);
            filterSplitContainer.Panel1.Controls.Add(lblTextFilter);
            filterSplitContainer.Panel1MinSize = 200;
            // 
            // filterSplitContainer.Panel2
            // 
            filterSplitContainer.Panel2.Controls.Add(advancedButton);
            filterSplitContainer.Panel2.Controls.Add(syncFilterCheckBox);
            filterSplitContainer.Panel2.Controls.Add(lblFilterCount);
            filterSplitContainer.Panel2.Controls.Add(filterTailCheckBox);
            filterSplitContainer.Panel2.Controls.Add(filterRegexCheckBox);
            filterSplitContainer.Panel2.Controls.Add(filterCaseSensitiveCheckBox);
            filterSplitContainer.Panel2.Controls.Add(filterSearchButton);
            filterSplitContainer.Panel2MinSize = 550;
            filterSplitContainer.Size = new Size(1138, 32);
            filterSplitContainer.SplitterDistance = 318;
            filterSplitContainer.TabIndex = 11;
            filterSplitContainer.MouseDoubleClick += OnFilterSplitContainerMouseDoubleClick;
            filterSplitContainer.MouseDown += OnFilterSplitContainerMouseDown;
            filterSplitContainer.MouseMove += OnFilterSplitContainerMouseMove;
            filterSplitContainer.MouseUp += OnFilterSplitContainerMouseUp;
            // 
            // filterComboBox
            // 
            filterComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            filterComboBox.Font = new Font("Courier New", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            filterComboBox.FormattingEnabled = true;
            filterComboBox.Location = new Point(89, 5);
            filterComboBox.Name = "filterComboBox";
            filterComboBox.Size = new Size(226, 35);
            filterComboBox.TabIndex = 4;
            helpToolTip.SetToolTip(filterComboBox, "Search string for the filter");
            filterComboBox.TextChanged += OnFilterComboBoxTextChanged;
            filterComboBox.KeyDown += OnFilterComboBoxKeyDown;
            // 
            // lblTextFilter
            // 
            lblTextFilter.AutoSize = true;
            lblTextFilter.Location = new Point(5, 5);
            lblTextFilter.Name = "lblTextFilter";
            lblTextFilter.Size = new Size(84, 20);
            lblTextFilter.TabIndex = 3;
            lblTextFilter.Text = "Text &filter:";
            // 
            // advancedButton
            // 
            advancedButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            advancedButton.Image = Resources.AdvancedIcon2;
            advancedButton.ImageAlign = ContentAlignment.BottomRight;
            advancedButton.Location = new Point(495, 5);
            advancedButton.Name = "advancedButton";
            advancedButton.Size = new Size(110, 21);
            advancedButton.TabIndex = 17;
            advancedButton.Text = "Show advanced...";
            helpToolTip.SetToolTip(advancedButton, "Togge the advanced filter options panel");
            advancedButton.UseVisualStyleBackColor = true;
            advancedButton.Click += OnAdvancedButtonClick;
            // 
            // syncFilterCheckBox
            // 
            syncFilterCheckBox.AutoSize = true;
            syncFilterCheckBox.Location = new Point(423, 5);
            syncFilterCheckBox.Name = "syncFilterCheckBox";
            syncFilterCheckBox.Size = new Size(72, 24);
            syncFilterCheckBox.TabIndex = 16;
            syncFilterCheckBox.Text = "Sync";
            helpToolTip.SetToolTip(syncFilterCheckBox, "Sync the current selected line in the filter view to the selection in the log file view");
            syncFilterCheckBox.UseVisualStyleBackColor = true;
            syncFilterCheckBox.CheckedChanged += OnSyncFilterCheckBoxCheckedChanged;
            // 
            // lblFilterCount
            // 
            lblFilterCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lblFilterCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lblFilterCount.Location = new Point(742, -23);
            lblFilterCount.Name = "lblFilterCount";
            lblFilterCount.Size = new Size(71, 21);
            lblFilterCount.TabIndex = 15;
            lblFilterCount.Text = "0";
            lblFilterCount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // filterTailCheckBox
            // 
            filterTailCheckBox.AutoSize = true;
            filterTailCheckBox.Location = new Point(323, 5);
            filterTailCheckBox.Name = "filterTailCheckBox";
            filterTailCheckBox.Size = new Size(100, 24);
            filterTailCheckBox.TabIndex = 14;
            filterTailCheckBox.Text = "Filter tail";
            helpToolTip.SetToolTip(filterTailCheckBox, "Filter tailed file content (keeps filter view up to date on file changes)");
            filterTailCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterRegexCheckBox
            // 
            filterRegexCheckBox.AutoSize = true;
            filterRegexCheckBox.Location = new Point(239, 5);
            filterRegexCheckBox.Name = "filterRegexCheckBox";
            filterRegexCheckBox.Size = new Size(82, 24);
            filterRegexCheckBox.TabIndex = 13;
            filterRegexCheckBox.Text = "Regex";
            helpToolTip.SetToolTip(filterRegexCheckBox, "Use regular expressions. (right-click for RegEx helper window)");
            filterRegexCheckBox.UseVisualStyleBackColor = true;
            filterRegexCheckBox.CheckedChanged += OnFilterRegexCheckBoxCheckedChanged;
            filterRegexCheckBox.MouseUp += OnFilterRegexCheckBoxMouseUp;
            // 
            // filterCaseSensitiveCheckBox
            // 
            filterCaseSensitiveCheckBox.AutoSize = true;
            filterCaseSensitiveCheckBox.Location = new Point(93, 5);
            filterCaseSensitiveCheckBox.Name = "filterCaseSensitiveCheckBox";
            filterCaseSensitiveCheckBox.Size = new Size(145, 24);
            filterCaseSensitiveCheckBox.TabIndex = 12;
            filterCaseSensitiveCheckBox.Text = "Case sensitive";
            helpToolTip.SetToolTip(filterCaseSensitiveCheckBox, "Makes the filter case sensitive");
            filterCaseSensitiveCheckBox.UseVisualStyleBackColor = true;
            filterCaseSensitiveCheckBox.CheckedChanged += OnFilterCaseSensitiveCheckBoxCheckedChanged;
            // 
            // filterSearchButton
            // 
            filterSearchButton.Image = Resources.AdvancedIcon2;
            filterSearchButton.ImageAlign = ContentAlignment.BottomRight;
            filterSearchButton.Location = new Point(3, 5);
            filterSearchButton.Name = "filterSearchButton";
            filterSearchButton.Size = new Size(72, 21);
            filterSearchButton.TabIndex = 11;
            filterSearchButton.Text = "Search";
            helpToolTip.SetToolTip(filterSearchButton, "Start the filter search");
            filterSearchButton.UseVisualStyleBackColor = true;
            filterSearchButton.Click += OnFilterSearchButtonClick;
            // 
            // bookmarkContextMenuStrip
            // 
            bookmarkContextMenuStrip.ImageScalingSize = new Size(24, 24);
            bookmarkContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { deleteBookmarksToolStripMenuItem });
            bookmarkContextMenuStrip.Name = "bookmarkContextMenuStrip";
            bookmarkContextMenuStrip.Size = new Size(73, 28);
            // 
            // deleteBookmarksToolStripMenuItem
            // 
            deleteBookmarksToolStripMenuItem.Name = "deleteBookmarksToolStripMenuItem";
            deleteBookmarksToolStripMenuItem.Size = new Size(72, 24);
            // 
            // columnContextMenuStrip
            // 
            columnContextMenuStrip.ImageScalingSize = new Size(24, 24);
            columnContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { freezeLeftColumnsUntilHereToolStripMenuItem, toolStripSeparator3, moveToLastColumnToolStripMenuItem, moveLeftToolStripMenuItem, moveRightToolStripMenuItem, toolStripSeparator5, hideColumnToolStripMenuItem, restoreColumnsToolStripMenuItem, toolStripSeparator6, allColumnsToolStripMenuItem });
            columnContextMenuStrip.Name = "columnContextMenuStrip";
            columnContextMenuStrip.Size = new Size(315, 246);
            columnContextMenuStrip.Opening += OnColumnContextMenuStripOpening;
            // 
            // freezeLeftColumnsUntilHereToolStripMenuItem
            // 
            freezeLeftColumnsUntilHereToolStripMenuItem.Name = "freezeLeftColumnsUntilHereToolStripMenuItem";
            freezeLeftColumnsUntilHereToolStripMenuItem.Size = new Size(314, 32);
            freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Freeze left columns until here";
            freezeLeftColumnsUntilHereToolStripMenuItem.Click += OnFreezeLeftColumnsUntilHereToolStripMenuItemClick;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(311, 6);
            // 
            // moveToLastColumnToolStripMenuItem
            // 
            moveToLastColumnToolStripMenuItem.Name = "moveToLastColumnToolStripMenuItem";
            moveToLastColumnToolStripMenuItem.Size = new Size(314, 32);
            moveToLastColumnToolStripMenuItem.Text = "Move to last column";
            moveToLastColumnToolStripMenuItem.ToolTipText = "Move this column to the last position";
            moveToLastColumnToolStripMenuItem.Click += OnMoveToLastColumnToolStripMenuItemClick;
            // 
            // moveLeftToolStripMenuItem
            // 
            moveLeftToolStripMenuItem.Name = "moveLeftToolStripMenuItem";
            moveLeftToolStripMenuItem.Size = new Size(314, 32);
            moveLeftToolStripMenuItem.Text = "Move left";
            moveLeftToolStripMenuItem.Click += OnMoveLeftToolStripMenuItemClick;
            // 
            // moveRightToolStripMenuItem
            // 
            moveRightToolStripMenuItem.Name = "moveRightToolStripMenuItem";
            moveRightToolStripMenuItem.Size = new Size(314, 32);
            moveRightToolStripMenuItem.Text = "Move right";
            moveRightToolStripMenuItem.Click += OnMoveRightToolStripMenuItemClick;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(311, 6);
            // 
            // hideColumnToolStripMenuItem
            // 
            hideColumnToolStripMenuItem.Name = "hideColumnToolStripMenuItem";
            hideColumnToolStripMenuItem.Size = new Size(314, 32);
            hideColumnToolStripMenuItem.Text = "Hide column";
            hideColumnToolStripMenuItem.ToolTipText = "Hide this column";
            hideColumnToolStripMenuItem.Click += OnHideColumnToolStripMenuItemClick;
            // 
            // restoreColumnsToolStripMenuItem
            // 
            restoreColumnsToolStripMenuItem.Name = "restoreColumnsToolStripMenuItem";
            restoreColumnsToolStripMenuItem.Size = new Size(314, 32);
            restoreColumnsToolStripMenuItem.Text = "Restore columns";
            restoreColumnsToolStripMenuItem.Click += OnRestoreColumnsToolStripMenuItemClick;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(311, 6);
            // 
            // allColumnsToolStripMenuItem
            // 
            allColumnsToolStripMenuItem.Name = "allColumnsToolStripMenuItem";
            allColumnsToolStripMenuItem.Size = new Size(314, 32);
            allColumnsToolStripMenuItem.Text = "Scroll to column...";
            // 
            // editModeContextMenuStrip
            // 
            editModeContextMenuStrip.ImageScalingSize = new Size(24, 24);
            editModeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { editModecopyToolStripMenuItem, highlightSelectionInLogFileToolStripMenuItem, highlightSelectionInLogFilewordModeToolStripMenuItem, filterForSelectionToolStripMenuItem, setSelectedTextAsBookmarkCommentToolStripMenuItem });
            editModeContextMenuStrip.Name = "editModeContextMenuStrip";
            editModeContextMenuStrip.Size = new Size(486, 164);
            // 
            // editModecopyToolStripMenuItem
            // 
            editModecopyToolStripMenuItem.Name = "editModecopyToolStripMenuItem";
            editModecopyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            editModecopyToolStripMenuItem.Size = new Size(485, 32);
            editModecopyToolStripMenuItem.Text = "Copy";
            editModecopyToolStripMenuItem.Click += OnEditModeCopyToolStripMenuItemClick;
            // 
            // highlightSelectionInLogFileToolStripMenuItem
            // 
            highlightSelectionInLogFileToolStripMenuItem.Name = "highlightSelectionInLogFileToolStripMenuItem";
            highlightSelectionInLogFileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H;
            highlightSelectionInLogFileToolStripMenuItem.Size = new Size(485, 32);
            highlightSelectionInLogFileToolStripMenuItem.Text = "Highlight selection in log file (full line)";
            highlightSelectionInLogFileToolStripMenuItem.Click += OnHighlightSelectionInLogFileToolStripMenuItemClick;
            // 
            // highlightSelectionInLogFilewordModeToolStripMenuItem
            // 
            highlightSelectionInLogFilewordModeToolStripMenuItem.Name = "highlightSelectionInLogFilewordModeToolStripMenuItem";
            highlightSelectionInLogFilewordModeToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W;
            highlightSelectionInLogFilewordModeToolStripMenuItem.Size = new Size(485, 32);
            highlightSelectionInLogFilewordModeToolStripMenuItem.Text = "Highlight selection in log file (word mode)";
            highlightSelectionInLogFilewordModeToolStripMenuItem.Click += OnHighlightSelectionInLogFilewordModeToolStripMenuItemClick;
            // 
            // filterForSelectionToolStripMenuItem
            // 
            filterForSelectionToolStripMenuItem.Name = "filterForSelectionToolStripMenuItem";
            filterForSelectionToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            filterForSelectionToolStripMenuItem.Size = new Size(485, 32);
            filterForSelectionToolStripMenuItem.Text = "Filter for selection";
            filterForSelectionToolStripMenuItem.Click += OnFilterForSelectionToolStripMenuItemClick;
            // 
            // setSelectedTextAsBookmarkCommentToolStripMenuItem
            // 
            setSelectedTextAsBookmarkCommentToolStripMenuItem.Name = "setSelectedTextAsBookmarkCommentToolStripMenuItem";
            setSelectedTextAsBookmarkCommentToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B;
            setSelectedTextAsBookmarkCommentToolStripMenuItem.Size = new Size(485, 32);
            setSelectedTextAsBookmarkCommentToolStripMenuItem.Text = "Set selected text as bookmark comment";
            setSelectedTextAsBookmarkCommentToolStripMenuItem.Click += OnSetSelectedTextAsBookmarkCommentToolStripMenuItemClick;
            // 
            // LogWindow
            // 
            BackColor = SystemColors.Control;
            ClientSize = new Size(1145, 920);
            ControlBox = false;
            Controls.Add(splitContainerLogWindow);
            Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(0);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LogWindow";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            SizeChanged += OnLogWindowSizeChanged;
            Enter += OnLogWindowEnter;
            KeyDown += OnLogWindowKeyDown;
            Leave += OnLogWindowLeave;
            splitContainerLogWindow.Panel1.ResumeLayout(false);
            splitContainerLogWindow.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerLogWindow).EndInit();
            splitContainerLogWindow.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            columnFinderPanel.ResumeLayout(false);
            columnFinderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            dataGridContextMenuStrip.ResumeLayout(false);
            advancedBackPanel.ResumeLayout(false);
            advancedFilterSplitContainer.Panel1.ResumeLayout(false);
            advancedFilterSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)advancedFilterSplitContainer).EndInit();
            advancedFilterSplitContainer.ResumeLayout(false);
            pnlProFilter.ResumeLayout(false);
            pnlProFilter.PerformLayout();
            panelBackgroundAdvancedFilterSplitContainer.ResumeLayout(false);
            highlightSplitContainer.Panel1.ResumeLayout(false);
            highlightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)highlightSplitContainer).EndInit();
            highlightSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)filterGridView).EndInit();
            filterContextMenuStrip.ResumeLayout(false);
            highlightSplitContainerBackPanel.ResumeLayout(false);
            highlightSplitContainerBackPanel.PerformLayout();
            filterListContextMenuStrip.ResumeLayout(false);
            pnlFilterInput.ResumeLayout(false);
            filterSplitContainer.Panel1.ResumeLayout(false);
            filterSplitContainer.Panel1.PerformLayout();
            filterSplitContainer.Panel2.ResumeLayout(false);
            filterSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)filterSplitContainer).EndInit();
            filterSplitContainer.ResumeLayout(false);
            bookmarkContextMenuStrip.ResumeLayout(false);
            columnContextMenuStrip.ResumeLayout(false);
            editModeContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.Panel panelBackgroundAdvancedFilterSplitContainer;
    }
}
