namespace LogExpert.Dialogs
{
  partial class SettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.tabControlSettings = new System.Windows.Forms.TabControl();
            this.tabPageViewSettings = new System.Windows.Forms.TabPage();
            this.upDownMaximumFilterEntriesDisplayed = new System.Windows.Forms.NumericUpDown();
            this.labelMaximumFilterEntriesDisplayed = new System.Windows.Forms.Label();
            this.upDownMaximumFilterEntries = new System.Windows.Forms.NumericUpDown();
            this.labelMaximumFilterEntries = new System.Windows.Forms.Label();
            this.labelDefaultEncoding = new System.Windows.Forms.Label();
            this.encodingComboBox = new System.Windows.Forms.ComboBox();
            this.groupBoxMisc = new System.Windows.Forms.GroupBox();
            this.cpDownColumnWidth = new System.Windows.Forms.NumericUpDown();
            this.checkBoxColumnSize = new System.Windows.Forms.CheckBox();
            this.buttonTailColor = new System.Windows.Forms.Button();
            this.checkBoxTailState = new System.Windows.Forms.CheckBox();
            this.checkBoxOpenLastFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxSingleInstance = new System.Windows.Forms.CheckBox();
            this.checkBoxAskCloseTabs = new System.Windows.Forms.CheckBox();
            this.groupBoxDefaults = new System.Windows.Forms.GroupBox();
            this.checkBoxFollowTail = new System.Windows.Forms.CheckBox();
            this.checkBoxColumnFinder = new System.Windows.Forms.CheckBox();
            this.checkBoxSyncFilter = new System.Windows.Forms.CheckBox();
            this.checkBoxFilterTail = new System.Windows.Forms.CheckBox();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.buttonChangeFont = new System.Windows.Forms.Button();
            this.labelFont = new System.Windows.Forms.Label();
            this.tabPageTimeStampFeatures = new System.Windows.Forms.TabPage();
            this.groupBoxTimeSpreadDisplay = new System.Windows.Forms.GroupBox();
            this.groupBoxDisplayMode = new System.Windows.Forms.GroupBox();
            this.radioButtonLineView = new System.Windows.Forms.RadioButton();
            this.radioButtonTimeView = new System.Windows.Forms.RadioButton();
            this.checkBoxReverseAlpha = new System.Windows.Forms.CheckBox();
            this.buttonTimespreadColor = new System.Windows.Forms.Button();
            this.checkBoxTimeSpread = new System.Windows.Forms.CheckBox();
            this.groupBoxTimeStampNavigationControl = new System.Windows.Forms.GroupBox();
            this.checkBoxTimestamp = new System.Windows.Forms.CheckBox();
            this.groupBoxMouseDragDefaults = new System.Windows.Forms.GroupBox();
            this.radioButtonVerticalMouseDragInverted = new System.Windows.Forms.RadioButton();
            this.radioButtonHorizMouseDrag = new System.Windows.Forms.RadioButton();
            this.radioButtonVerticalMouseDrag = new System.Windows.Forms.RadioButton();
            this.tabPageExternalTools = new System.Windows.Forms.TabPage();
            this.labelToolsDescription = new System.Windows.Forms.Label();
            this.buttonToolDelete = new System.Windows.Forms.Button();
            this.buttonToolAdd = new System.Windows.Forms.Button();
            this.buttonToolDown = new System.Windows.Forms.Button();
            this.buttonToolUp = new System.Windows.Forms.Button();
            this.listBoxTools = new System.Windows.Forms.CheckedListBox();
            this.groupBoxToolSettings = new System.Windows.Forms.GroupBox();
            this.labelWorkingDir = new System.Windows.Forms.Label();
            this.buttonWorkingDir = new System.Windows.Forms.Button();
            this.textBoxWorkingDir = new System.Windows.Forms.TextBox();
            this.buttonIcon = new System.Windows.Forms.Button();
            this.labelToolName = new System.Windows.Forms.Label();
            this.labelToolColumnizerForOutput = new System.Windows.Forms.Label();
            this.comboBoxColumnizer = new System.Windows.Forms.ComboBox();
            this.textBoxToolName = new System.Windows.Forms.TextBox();
            this.checkBoxSysout = new System.Windows.Forms.CheckBox();
            this.buttonArguments = new System.Windows.Forms.Button();
            this.labelTool = new System.Windows.Forms.Label();
            this.buttonTool = new System.Windows.Forms.Button();
            this.textBoxTool = new System.Windows.Forms.TextBox();
            this.labelArguments = new System.Windows.Forms.Label();
            this.textBoxArguments = new System.Windows.Forms.TextBox();
            this.tabPageColumnizers = new System.Windows.Forms.TabPage();
            this.checkBoxAutoPick = new System.Windows.Forms.CheckBox();
            this.checkBoxMaskPrio = new System.Windows.Forms.CheckBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.dataGridViewColumnizer = new System.Windows.Forms.DataGridView();
            this.columnFileMask = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnColumnizer = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tabPageHighlightMask = new System.Windows.Forms.TabPage();
            this.dataGridViewHighlightMask = new System.Windows.Forms.DataGridView();
            this.columnFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnHighlightGroup = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tabPageMultiFile = new System.Windows.Forms.TabPage();
            this.groupBoxDefaultFileNamePattern = new System.Windows.Forms.GroupBox();
            this.labelMaxDays = new System.Windows.Forms.Label();
            this.labelPattern = new System.Windows.Forms.Label();
            this.upDownMultifileDays = new System.Windows.Forms.NumericUpDown();
            this.textBoxMultifilePattern = new System.Windows.Forms.TextBox();
            this.labelHintMultiFile = new System.Windows.Forms.Label();
            this.labelNoteMultiFile = new System.Windows.Forms.Label();
            this.groupBoxWhenOpeningMultiFile = new System.Windows.Forms.GroupBox();
            this.radioButtonAskWhatToDo = new System.Windows.Forms.RadioButton();
            this.radioButtonTreatAllFilesAsOneMultifile = new System.Windows.Forms.RadioButton();
            this.radioButtonLoadEveryFileIntoSeperatedTab = new System.Windows.Forms.RadioButton();
            this.tabPagePlugins = new System.Windows.Forms.TabPage();
            this.groupBoxPlugins = new System.Windows.Forms.GroupBox();
            this.listBoxPlugin = new System.Windows.Forms.ListBox();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.panelPlugin = new System.Windows.Forms.Panel();
            this.buttonConfigPlugin = new System.Windows.Forms.Button();
            this.tabPageSessions = new System.Windows.Forms.TabPage();
            this.checkBoxSaveFilter = new System.Windows.Forms.CheckBox();
            this.groupBoxPersistantFileLocation = new System.Windows.Forms.GroupBox();
            this.checkBoxPortableMode = new System.Windows.Forms.CheckBox();
            this.labelSessionSaveOwnDir = new System.Windows.Forms.Label();
            this.buttonSessionSaveDir = new System.Windows.Forms.Button();
            this.radioButtonSessionSaveOwn = new System.Windows.Forms.RadioButton();
            this.radioButtonsessionSaveDocuments = new System.Windows.Forms.RadioButton();
            this.radioButtonSessionSameDir = new System.Windows.Forms.RadioButton();
            this.checkBoxSaveSessions = new System.Windows.Forms.CheckBox();
            this.tabPageMemory = new System.Windows.Forms.TabPage();
            this.groupBoxCPUAndStuff = new System.Windows.Forms.GroupBox();
            this.checkBoxLegacyReader = new System.Windows.Forms.CheckBox();
            this.checkBoxMultiThread = new System.Windows.Forms.CheckBox();
            this.labelFilePollingInterval = new System.Windows.Forms.Label();
            this.upDownPollingInterval = new System.Windows.Forms.NumericUpDown();
            this.groupBoxLineBufferUsage = new System.Windows.Forms.GroupBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelNumberOfBlocks = new System.Windows.Forms.Label();
            this.upDownLinesPerBlock = new System.Windows.Forms.NumericUpDown();
            this.upDownBlockCount = new System.Windows.Forms.NumericUpDown();
            this.labelLinesPerBlock = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControlSettings.SuspendLayout();
            this.tabPageViewSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMaximumFilterEntriesDisplayed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMaximumFilterEntries)).BeginInit();
            this.groupBoxMisc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cpDownColumnWidth)).BeginInit();
            this.groupBoxDefaults.SuspendLayout();
            this.groupBoxFont.SuspendLayout();
            this.tabPageTimeStampFeatures.SuspendLayout();
            this.groupBoxTimeSpreadDisplay.SuspendLayout();
            this.groupBoxDisplayMode.SuspendLayout();
            this.groupBoxTimeStampNavigationControl.SuspendLayout();
            this.groupBoxMouseDragDefaults.SuspendLayout();
            this.tabPageExternalTools.SuspendLayout();
            this.groupBoxToolSettings.SuspendLayout();
            this.tabPageColumnizers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewColumnizer)).BeginInit();
            this.tabPageHighlightMask.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewHighlightMask)).BeginInit();
            this.tabPageMultiFile.SuspendLayout();
            this.groupBoxDefaultFileNamePattern.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMultifileDays)).BeginInit();
            this.groupBoxWhenOpeningMultiFile.SuspendLayout();
            this.tabPagePlugins.SuspendLayout();
            this.groupBoxPlugins.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.panelPlugin.SuspendLayout();
            this.tabPageSessions.SuspendLayout();
            this.groupBoxPersistantFileLocation.SuspendLayout();
            this.tabPageMemory.SuspendLayout();
            this.groupBoxCPUAndStuff.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownPollingInterval)).BeginInit();
            this.groupBoxLineBufferUsage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownLinesPerBlock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownBlockCount)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlSettings
            // 
            this.tabControlSettings.Controls.Add(this.tabPageViewSettings);
            this.tabControlSettings.Controls.Add(this.tabPageTimeStampFeatures);
            this.tabControlSettings.Controls.Add(this.tabPageExternalTools);
            this.tabControlSettings.Controls.Add(this.tabPageColumnizers);
            this.tabControlSettings.Controls.Add(this.tabPageHighlightMask);
            this.tabControlSettings.Controls.Add(this.tabPageMultiFile);
            this.tabControlSettings.Controls.Add(this.tabPagePlugins);
            this.tabControlSettings.Controls.Add(this.tabPageSessions);
            this.tabControlSettings.Controls.Add(this.tabPageMemory);
            this.tabControlSettings.Location = new System.Drawing.Point(1, 2);
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(633, 304);
            this.tabControlSettings.TabIndex = 0;
            // 
            // tabPageViewSettings
            // 
            this.tabPageViewSettings.Controls.Add(this.upDownMaximumFilterEntriesDisplayed);
            this.tabPageViewSettings.Controls.Add(this.labelMaximumFilterEntriesDisplayed);
            this.tabPageViewSettings.Controls.Add(this.upDownMaximumFilterEntries);
            this.tabPageViewSettings.Controls.Add(this.labelMaximumFilterEntries);
            this.tabPageViewSettings.Controls.Add(this.labelDefaultEncoding);
            this.tabPageViewSettings.Controls.Add(this.encodingComboBox);
            this.tabPageViewSettings.Controls.Add(this.groupBoxMisc);
            this.tabPageViewSettings.Controls.Add(this.groupBoxDefaults);
            this.tabPageViewSettings.Controls.Add(this.groupBoxFont);
            this.tabPageViewSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPageViewSettings.Name = "tabPageViewSettings";
            this.tabPageViewSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageViewSettings.Size = new System.Drawing.Size(625, 278);
            this.tabPageViewSettings.TabIndex = 0;
            this.tabPageViewSettings.Text = "View settings";
            this.tabPageViewSettings.UseVisualStyleBackColor = true;
            // 
            // upDownMaximumFilterEntriesDisplayed
            // 
            this.upDownMaximumFilterEntriesDisplayed.Location = new System.Drawing.Point(508, 74);
            this.upDownMaximumFilterEntriesDisplayed.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.upDownMaximumFilterEntriesDisplayed.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.upDownMaximumFilterEntriesDisplayed.Name = "upDownMaximumFilterEntriesDisplayed";
            this.upDownMaximumFilterEntriesDisplayed.Size = new System.Drawing.Size(53, 20);
            this.upDownMaximumFilterEntriesDisplayed.TabIndex = 13;
            this.upDownMaximumFilterEntriesDisplayed.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // labelMaximumFilterEntriesDisplayed
            // 
            this.labelMaximumFilterEntriesDisplayed.AutoSize = true;
            this.labelMaximumFilterEntriesDisplayed.Location = new System.Drawing.Point(308, 76);
            this.labelMaximumFilterEntriesDisplayed.Name = "labelMaximumFilterEntriesDisplayed";
            this.labelMaximumFilterEntriesDisplayed.Size = new System.Drawing.Size(154, 13);
            this.labelMaximumFilterEntriesDisplayed.TabIndex = 12;
            this.labelMaximumFilterEntriesDisplayed.Text = "Maximum filter entries displayed";
            // 
            // upDownMaximumFilterEntries
            // 
            this.upDownMaximumFilterEntries.Location = new System.Drawing.Point(508, 46);
            this.upDownMaximumFilterEntries.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.upDownMaximumFilterEntries.Name = "upDownMaximumFilterEntries";
            this.upDownMaximumFilterEntries.Size = new System.Drawing.Size(53, 20);
            this.upDownMaximumFilterEntries.TabIndex = 11;
            this.upDownMaximumFilterEntries.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // labelMaximumFilterEntries
            // 
            this.labelMaximumFilterEntries.AutoSize = true;
            this.labelMaximumFilterEntries.Location = new System.Drawing.Point(308, 48);
            this.labelMaximumFilterEntries.Name = "labelMaximumFilterEntries";
            this.labelMaximumFilterEntries.Size = new System.Drawing.Size(107, 13);
            this.labelMaximumFilterEntries.TabIndex = 10;
            this.labelMaximumFilterEntries.Text = "Maximum filter entries";
            // 
            // labelDefaultEncoding
            // 
            this.labelDefaultEncoding.AutoSize = true;
            this.labelDefaultEncoding.Location = new System.Drawing.Point(308, 22);
            this.labelDefaultEncoding.Name = "labelDefaultEncoding";
            this.labelDefaultEncoding.Size = new System.Drawing.Size(88, 13);
            this.labelDefaultEncoding.TabIndex = 9;
            this.labelDefaultEncoding.Text = "Default encoding";
            // 
            // encodingComboBox
            // 
            this.encodingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encodingComboBox.FormattingEnabled = true;
            this.encodingComboBox.Location = new System.Drawing.Point(459, 19);
            this.encodingComboBox.Name = "encodingComboBox";
            this.encodingComboBox.Size = new System.Drawing.Size(102, 21);
            this.encodingComboBox.TabIndex = 8;
            this.toolTip.SetToolTip(this.encodingComboBox, "Encoding to be used when no BOM header and no persistence data is available.");
            // 
            // groupBoxMisc
            // 
            this.groupBoxMisc.Controls.Add(this.cpDownColumnWidth);
            this.groupBoxMisc.Controls.Add(this.checkBoxColumnSize);
            this.groupBoxMisc.Controls.Add(this.buttonTailColor);
            this.groupBoxMisc.Controls.Add(this.checkBoxTailState);
            this.groupBoxMisc.Controls.Add(this.checkBoxOpenLastFiles);
            this.groupBoxMisc.Controls.Add(this.checkBoxSingleInstance);
            this.groupBoxMisc.Controls.Add(this.checkBoxAskCloseTabs);
            this.groupBoxMisc.Location = new System.Drawing.Point(305, 111);
            this.groupBoxMisc.Name = "groupBoxMisc";
            this.groupBoxMisc.Size = new System.Drawing.Size(256, 147);
            this.groupBoxMisc.TabIndex = 7;
            this.groupBoxMisc.TabStop = false;
            this.groupBoxMisc.Text = "Misc";
            // 
            // cpDownColumnWidth
            // 
            this.cpDownColumnWidth.Location = new System.Drawing.Point(154, 113);
            this.cpDownColumnWidth.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.cpDownColumnWidth.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.cpDownColumnWidth.Name = "cpDownColumnWidth";
            this.cpDownColumnWidth.Size = new System.Drawing.Size(56, 20);
            this.cpDownColumnWidth.TabIndex = 6;
            this.cpDownColumnWidth.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // checkBoxColumnSize
            // 
            this.checkBoxColumnSize.AutoSize = true;
            this.checkBoxColumnSize.Location = new System.Drawing.Point(6, 115);
            this.checkBoxColumnSize.Name = "checkBoxColumnSize";
            this.checkBoxColumnSize.Size = new System.Drawing.Size(126, 17);
            this.checkBoxColumnSize.TabIndex = 5;
            this.checkBoxColumnSize.Text = "Set last column width";
            this.checkBoxColumnSize.UseVisualStyleBackColor = true;
            this.checkBoxColumnSize.CheckedChanged += new System.EventHandler(this.columnSizeCheckBox_CheckedChanged);
            // 
            // buttonTailColor
            // 
            this.buttonTailColor.Location = new System.Drawing.Point(154, 87);
            this.buttonTailColor.Name = "buttonTailColor";
            this.buttonTailColor.Size = new System.Drawing.Size(56, 21);
            this.buttonTailColor.TabIndex = 4;
            this.buttonTailColor.Text = "Color...";
            this.buttonTailColor.UseVisualStyleBackColor = true;
            this.buttonTailColor.Click += new System.EventHandler(this.tailColorButton_Click);
            // 
            // checkBoxTailState
            // 
            this.checkBoxTailState.AutoSize = true;
            this.checkBoxTailState.Location = new System.Drawing.Point(6, 91);
            this.checkBoxTailState.Name = "checkBoxTailState";
            this.checkBoxTailState.Size = new System.Drawing.Size(133, 17);
            this.checkBoxTailState.TabIndex = 3;
            this.checkBoxTailState.Text = "Show tail state on tabs";
            this.checkBoxTailState.UseVisualStyleBackColor = true;
            // 
            // checkBoxOpenLastFiles
            // 
            this.checkBoxOpenLastFiles.AutoSize = true;
            this.checkBoxOpenLastFiles.Location = new System.Drawing.Point(6, 67);
            this.checkBoxOpenLastFiles.Name = "checkBoxOpenLastFiles";
            this.checkBoxOpenLastFiles.Size = new System.Drawing.Size(133, 17);
            this.checkBoxOpenLastFiles.TabIndex = 2;
            this.checkBoxOpenLastFiles.Text = "Re-open last used files";
            this.checkBoxOpenLastFiles.UseVisualStyleBackColor = true;
            // 
            // checkBoxSingleInstance
            // 
            this.checkBoxSingleInstance.AutoSize = true;
            this.checkBoxSingleInstance.Location = new System.Drawing.Point(6, 43);
            this.checkBoxSingleInstance.Name = "checkBoxSingleInstance";
            this.checkBoxSingleInstance.Size = new System.Drawing.Size(121, 17);
            this.checkBoxSingleInstance.TabIndex = 1;
            this.checkBoxSingleInstance.Text = "Allow only 1 window";
            this.checkBoxSingleInstance.UseVisualStyleBackColor = true;
            // 
            // checkBoxAskCloseTabs
            // 
            this.checkBoxAskCloseTabs.AutoSize = true;
            this.checkBoxAskCloseTabs.Location = new System.Drawing.Point(6, 19);
            this.checkBoxAskCloseTabs.Name = "checkBoxAskCloseTabs";
            this.checkBoxAskCloseTabs.Size = new System.Drawing.Size(136, 17);
            this.checkBoxAskCloseTabs.TabIndex = 0;
            this.checkBoxAskCloseTabs.Text = "Ask before closing tabs";
            this.checkBoxAskCloseTabs.UseVisualStyleBackColor = true;
            // 
            // groupBoxDefaults
            // 
            this.groupBoxDefaults.Controls.Add(this.checkBoxFollowTail);
            this.groupBoxDefaults.Controls.Add(this.checkBoxColumnFinder);
            this.groupBoxDefaults.Controls.Add(this.checkBoxSyncFilter);
            this.groupBoxDefaults.Controls.Add(this.checkBoxFilterTail);
            this.groupBoxDefaults.Location = new System.Drawing.Point(7, 111);
            this.groupBoxDefaults.Name = "groupBoxDefaults";
            this.groupBoxDefaults.Size = new System.Drawing.Size(274, 147);
            this.groupBoxDefaults.TabIndex = 6;
            this.groupBoxDefaults.TabStop = false;
            this.groupBoxDefaults.Text = "Defaults";
            // 
            // checkBoxFollowTail
            // 
            this.checkBoxFollowTail.AutoSize = true;
            this.checkBoxFollowTail.Location = new System.Drawing.Point(6, 19);
            this.checkBoxFollowTail.Name = "checkBoxFollowTail";
            this.checkBoxFollowTail.Size = new System.Drawing.Size(113, 17);
            this.checkBoxFollowTail.TabIndex = 3;
            this.checkBoxFollowTail.Text = "Follow tail enabled";
            this.checkBoxFollowTail.UseVisualStyleBackColor = true;
            // 
            // checkBoxColumnFinder
            // 
            this.checkBoxColumnFinder.AutoSize = true;
            this.checkBoxColumnFinder.Location = new System.Drawing.Point(6, 91);
            this.checkBoxColumnFinder.Name = "checkBoxColumnFinder";
            this.checkBoxColumnFinder.Size = new System.Drawing.Size(119, 17);
            this.checkBoxColumnFinder.TabIndex = 5;
            this.checkBoxColumnFinder.Text = "Show column finder";
            this.checkBoxColumnFinder.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyncFilter
            // 
            this.checkBoxSyncFilter.AutoSize = true;
            this.checkBoxSyncFilter.Location = new System.Drawing.Point(6, 67);
            this.checkBoxSyncFilter.Name = "checkBoxSyncFilter";
            this.checkBoxSyncFilter.Size = new System.Drawing.Size(128, 17);
            this.checkBoxSyncFilter.TabIndex = 5;
            this.checkBoxSyncFilter.Text = "Sync filter list enabled";
            this.checkBoxSyncFilter.UseVisualStyleBackColor = true;
            // 
            // checkBoxFilterTail
            // 
            this.checkBoxFilterTail.AutoSize = true;
            this.checkBoxFilterTail.Location = new System.Drawing.Point(6, 43);
            this.checkBoxFilterTail.Name = "checkBoxFilterTail";
            this.checkBoxFilterTail.Size = new System.Drawing.Size(105, 17);
            this.checkBoxFilterTail.TabIndex = 4;
            this.checkBoxFilterTail.Text = "Filter tail enabled";
            this.checkBoxFilterTail.UseVisualStyleBackColor = true;
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Controls.Add(this.buttonChangeFont);
            this.groupBoxFont.Controls.Add(this.labelFont);
            this.groupBoxFont.Location = new System.Drawing.Point(7, 6);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(272, 83);
            this.groupBoxFont.TabIndex = 1;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // buttonChangeFont
            // 
            this.buttonChangeFont.Location = new System.Drawing.Point(6, 50);
            this.buttonChangeFont.Name = "buttonChangeFont";
            this.buttonChangeFont.Size = new System.Drawing.Size(75, 23);
            this.buttonChangeFont.TabIndex = 1;
            this.buttonChangeFont.Text = "Change...";
            this.buttonChangeFont.UseVisualStyleBackColor = true;
            this.buttonChangeFont.Click += new System.EventHandler(this.changeFontButton_Click);
            // 
            // labelFont
            // 
            this.labelFont.Location = new System.Drawing.Point(6, 16);
            this.labelFont.Name = "labelFont";
            this.labelFont.Size = new System.Drawing.Size(208, 31);
            this.labelFont.TabIndex = 0;
            this.labelFont.Text = "Font";
            this.labelFont.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabPageTimeStampFeatures
            // 
            this.tabPageTimeStampFeatures.Controls.Add(this.groupBoxTimeSpreadDisplay);
            this.tabPageTimeStampFeatures.Controls.Add(this.groupBoxTimeStampNavigationControl);
            this.tabPageTimeStampFeatures.Location = new System.Drawing.Point(4, 22);
            this.tabPageTimeStampFeatures.Name = "tabPageTimeStampFeatures";
            this.tabPageTimeStampFeatures.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTimeStampFeatures.Size = new System.Drawing.Size(625, 278);
            this.tabPageTimeStampFeatures.TabIndex = 1;
            this.tabPageTimeStampFeatures.Text = "Timestamp features";
            this.tabPageTimeStampFeatures.UseVisualStyleBackColor = true;
            // 
            // groupBoxTimeSpreadDisplay
            // 
            this.groupBoxTimeSpreadDisplay.Controls.Add(this.groupBoxDisplayMode);
            this.groupBoxTimeSpreadDisplay.Controls.Add(this.checkBoxReverseAlpha);
            this.groupBoxTimeSpreadDisplay.Controls.Add(this.buttonTimespreadColor);
            this.groupBoxTimeSpreadDisplay.Controls.Add(this.checkBoxTimeSpread);
            this.groupBoxTimeSpreadDisplay.Location = new System.Drawing.Point(327, 16);
            this.groupBoxTimeSpreadDisplay.Name = "groupBoxTimeSpreadDisplay";
            this.groupBoxTimeSpreadDisplay.Size = new System.Drawing.Size(200, 160);
            this.groupBoxTimeSpreadDisplay.TabIndex = 8;
            this.groupBoxTimeSpreadDisplay.TabStop = false;
            this.groupBoxTimeSpreadDisplay.Text = "Time spread display";
            // 
            // groupBoxDisplayMode
            // 
            this.groupBoxDisplayMode.Controls.Add(this.radioButtonLineView);
            this.groupBoxDisplayMode.Controls.Add(this.radioButtonTimeView);
            this.groupBoxDisplayMode.Location = new System.Drawing.Point(15, 71);
            this.groupBoxDisplayMode.Name = "groupBoxDisplayMode";
            this.groupBoxDisplayMode.Size = new System.Drawing.Size(125, 77);
            this.groupBoxDisplayMode.TabIndex = 11;
            this.groupBoxDisplayMode.TabStop = false;
            this.groupBoxDisplayMode.Text = "Display mode";
            // 
            // radioButtonLineView
            // 
            this.radioButtonLineView.AutoSize = true;
            this.radioButtonLineView.Location = new System.Drawing.Point(6, 42);
            this.radioButtonLineView.Name = "radioButtonLineView";
            this.radioButtonLineView.Size = new System.Drawing.Size(70, 17);
            this.radioButtonLineView.TabIndex = 9;
            this.radioButtonLineView.TabStop = true;
            this.radioButtonLineView.Text = "Line view";
            this.radioButtonLineView.UseVisualStyleBackColor = true;
            // 
            // radioButtonTimeView
            // 
            this.radioButtonTimeView.AutoSize = true;
            this.radioButtonTimeView.Location = new System.Drawing.Point(6, 19);
            this.radioButtonTimeView.Name = "radioButtonTimeView";
            this.radioButtonTimeView.Size = new System.Drawing.Size(73, 17);
            this.radioButtonTimeView.TabIndex = 10;
            this.radioButtonTimeView.TabStop = true;
            this.radioButtonTimeView.Text = "Time view";
            this.radioButtonTimeView.UseVisualStyleBackColor = true;
            // 
            // checkBoxReverseAlpha
            // 
            this.checkBoxReverseAlpha.AutoSize = true;
            this.checkBoxReverseAlpha.Location = new System.Drawing.Point(15, 48);
            this.checkBoxReverseAlpha.Name = "checkBoxReverseAlpha";
            this.checkBoxReverseAlpha.Size = new System.Drawing.Size(95, 17);
            this.checkBoxReverseAlpha.TabIndex = 8;
            this.checkBoxReverseAlpha.Text = "Reverse alpha";
            this.checkBoxReverseAlpha.UseVisualStyleBackColor = true;
            // 
            // buttonTimespreadColor
            // 
            this.buttonTimespreadColor.Location = new System.Drawing.Point(138, 21);
            this.buttonTimespreadColor.Name = "buttonTimespreadColor";
            this.buttonTimespreadColor.Size = new System.Drawing.Size(56, 21);
            this.buttonTimespreadColor.TabIndex = 7;
            this.buttonTimespreadColor.Text = "Color...";
            this.buttonTimespreadColor.UseVisualStyleBackColor = true;
            this.buttonTimespreadColor.Click += new System.EventHandler(this.timespreadColorButton_Click);
            // 
            // checkBoxTimeSpread
            // 
            this.checkBoxTimeSpread.AutoSize = true;
            this.checkBoxTimeSpread.Location = new System.Drawing.Point(15, 24);
            this.checkBoxTimeSpread.Name = "checkBoxTimeSpread";
            this.checkBoxTimeSpread.Size = new System.Drawing.Size(110, 17);
            this.checkBoxTimeSpread.TabIndex = 6;
            this.checkBoxTimeSpread.Text = "Show time spread";
            this.checkBoxTimeSpread.UseVisualStyleBackColor = true;
            // 
            // groupBoxTimeStampNavigationControl
            // 
            this.groupBoxTimeStampNavigationControl.Controls.Add(this.checkBoxTimestamp);
            this.groupBoxTimeStampNavigationControl.Controls.Add(this.groupBoxMouseDragDefaults);
            this.groupBoxTimeStampNavigationControl.Location = new System.Drawing.Point(7, 16);
            this.groupBoxTimeStampNavigationControl.Name = "groupBoxTimeStampNavigationControl";
            this.groupBoxTimeStampNavigationControl.Size = new System.Drawing.Size(300, 160);
            this.groupBoxTimeStampNavigationControl.TabIndex = 7;
            this.groupBoxTimeStampNavigationControl.TabStop = false;
            this.groupBoxTimeStampNavigationControl.Text = "Timestamp navigation control";
            // 
            // checkBoxTimestamp
            // 
            this.checkBoxTimestamp.AutoSize = true;
            this.checkBoxTimestamp.Location = new System.Drawing.Point(18, 24);
            this.checkBoxTimestamp.Name = "checkBoxTimestamp";
            this.checkBoxTimestamp.Size = new System.Drawing.Size(266, 17);
            this.checkBoxTimestamp.TabIndex = 3;
            this.checkBoxTimestamp.Text = "Show timestamp control, if supported by columnizer";
            this.checkBoxTimestamp.UseVisualStyleBackColor = true;
            // 
            // groupBoxMouseDragDefaults
            // 
            this.groupBoxMouseDragDefaults.Controls.Add(this.radioButtonVerticalMouseDragInverted);
            this.groupBoxMouseDragDefaults.Controls.Add(this.radioButtonHorizMouseDrag);
            this.groupBoxMouseDragDefaults.Controls.Add(this.radioButtonVerticalMouseDrag);
            this.groupBoxMouseDragDefaults.Location = new System.Drawing.Point(18, 52);
            this.groupBoxMouseDragDefaults.Name = "groupBoxMouseDragDefaults";
            this.groupBoxMouseDragDefaults.Size = new System.Drawing.Size(124, 96);
            this.groupBoxMouseDragDefaults.TabIndex = 5;
            this.groupBoxMouseDragDefaults.TabStop = false;
            this.groupBoxMouseDragDefaults.Text = "Mouse Drag Default";
            // 
            // radioButtonVerticalMouseDragInverted
            // 
            this.radioButtonVerticalMouseDragInverted.AutoSize = true;
            this.radioButtonVerticalMouseDragInverted.Location = new System.Drawing.Point(6, 66);
            this.radioButtonVerticalMouseDragInverted.Name = "radioButtonVerticalMouseDragInverted";
            this.radioButtonVerticalMouseDragInverted.Size = new System.Drawing.Size(102, 17);
            this.radioButtonVerticalMouseDragInverted.TabIndex = 6;
            this.radioButtonVerticalMouseDragInverted.TabStop = true;
            this.radioButtonVerticalMouseDragInverted.Text = "Vertical Inverted";
            this.radioButtonVerticalMouseDragInverted.UseVisualStyleBackColor = true;
            // 
            // radioButtonHorizMouseDrag
            // 
            this.radioButtonHorizMouseDrag.AutoSize = true;
            this.radioButtonHorizMouseDrag.Location = new System.Drawing.Point(6, 19);
            this.radioButtonHorizMouseDrag.Name = "radioButtonHorizMouseDrag";
            this.radioButtonHorizMouseDrag.Size = new System.Drawing.Size(72, 17);
            this.radioButtonHorizMouseDrag.TabIndex = 5;
            this.radioButtonHorizMouseDrag.TabStop = true;
            this.radioButtonHorizMouseDrag.Text = "Horizontal";
            this.radioButtonHorizMouseDrag.UseVisualStyleBackColor = true;
            // 
            // radioButtonVerticalMouseDrag
            // 
            this.radioButtonVerticalMouseDrag.AutoSize = true;
            this.radioButtonVerticalMouseDrag.Location = new System.Drawing.Point(6, 42);
            this.radioButtonVerticalMouseDrag.Name = "radioButtonVerticalMouseDrag";
            this.radioButtonVerticalMouseDrag.Size = new System.Drawing.Size(60, 17);
            this.radioButtonVerticalMouseDrag.TabIndex = 4;
            this.radioButtonVerticalMouseDrag.TabStop = true;
            this.radioButtonVerticalMouseDrag.Text = "Vertical";
            this.radioButtonVerticalMouseDrag.UseVisualStyleBackColor = true;
            // 
            // tabPageExternalTools
            // 
            this.tabPageExternalTools.Controls.Add(this.labelToolsDescription);
            this.tabPageExternalTools.Controls.Add(this.buttonToolDelete);
            this.tabPageExternalTools.Controls.Add(this.buttonToolAdd);
            this.tabPageExternalTools.Controls.Add(this.buttonToolDown);
            this.tabPageExternalTools.Controls.Add(this.buttonToolUp);
            this.tabPageExternalTools.Controls.Add(this.listBoxTools);
            this.tabPageExternalTools.Controls.Add(this.groupBoxToolSettings);
            this.tabPageExternalTools.Location = new System.Drawing.Point(4, 22);
            this.tabPageExternalTools.Name = "tabPageExternalTools";
            this.tabPageExternalTools.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageExternalTools.Size = new System.Drawing.Size(625, 278);
            this.tabPageExternalTools.TabIndex = 2;
            this.tabPageExternalTools.Text = "External Tools";
            this.tabPageExternalTools.UseVisualStyleBackColor = true;
            // 
            // labelToolsDescription
            // 
            this.labelToolsDescription.Location = new System.Drawing.Point(364, 66);
            this.labelToolsDescription.Name = "labelToolsDescription";
            this.labelToolsDescription.Size = new System.Drawing.Size(251, 52);
            this.labelToolsDescription.TabIndex = 6;
            this.labelToolsDescription.Text = "You can configure as many tools as you want. \r\nChecked tools will appear in the i" +
    "con bar. All other tools are available in the tools menu.";
            // 
            // buttonToolDelete
            // 
            this.buttonToolDelete.Location = new System.Drawing.Point(367, 9);
            this.buttonToolDelete.Name = "buttonToolDelete";
            this.buttonToolDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonToolDelete.TabIndex = 2;
            this.buttonToolDelete.Text = "Remove";
            this.buttonToolDelete.UseVisualStyleBackColor = true;
            this.buttonToolDelete.Click += new System.EventHandler(this.toolDeleteButton_Click);
            // 
            // buttonToolAdd
            // 
            this.buttonToolAdd.Location = new System.Drawing.Point(286, 9);
            this.buttonToolAdd.Name = "buttonToolAdd";
            this.buttonToolAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonToolAdd.TabIndex = 1;
            this.buttonToolAdd.Text = "Add new";
            this.buttonToolAdd.UseVisualStyleBackColor = true;
            this.buttonToolAdd.Click += new System.EventHandler(this.toolAddButton_Click);
            // 
            // buttonToolDown
            // 
            this.buttonToolDown.Location = new System.Drawing.Point(286, 95);
            this.buttonToolDown.Name = "buttonToolDown";
            this.buttonToolDown.Size = new System.Drawing.Size(43, 23);
            this.buttonToolDown.TabIndex = 4;
            this.buttonToolDown.Text = "Down";
            this.buttonToolDown.UseVisualStyleBackColor = true;
            this.buttonToolDown.Click += new System.EventHandler(this.toolDownButton_Click);
            // 
            // buttonToolUp
            // 
            this.buttonToolUp.Location = new System.Drawing.Point(286, 66);
            this.buttonToolUp.Name = "buttonToolUp";
            this.buttonToolUp.Size = new System.Drawing.Size(43, 23);
            this.buttonToolUp.TabIndex = 3;
            this.buttonToolUp.Text = "Up";
            this.buttonToolUp.UseVisualStyleBackColor = true;
            this.buttonToolUp.Click += new System.EventHandler(this.toolUpButton_Click);
            // 
            // listBoxTools
            // 
            this.listBoxTools.FormattingEnabled = true;
            this.listBoxTools.Location = new System.Drawing.Point(7, 9);
            this.listBoxTools.Name = "listBoxTools";
            this.listBoxTools.Size = new System.Drawing.Size(272, 109);
            this.listBoxTools.TabIndex = 0;
            this.listBoxTools.SelectedIndexChanged += new System.EventHandler(this.toolListBox_SelectedIndexChanged);
            // 
            // groupBoxToolSettings
            // 
            this.groupBoxToolSettings.Controls.Add(this.labelWorkingDir);
            this.groupBoxToolSettings.Controls.Add(this.buttonWorkingDir);
            this.groupBoxToolSettings.Controls.Add(this.textBoxWorkingDir);
            this.groupBoxToolSettings.Controls.Add(this.buttonIcon);
            this.groupBoxToolSettings.Controls.Add(this.labelToolName);
            this.groupBoxToolSettings.Controls.Add(this.labelToolColumnizerForOutput);
            this.groupBoxToolSettings.Controls.Add(this.comboBoxColumnizer);
            this.groupBoxToolSettings.Controls.Add(this.textBoxToolName);
            this.groupBoxToolSettings.Controls.Add(this.checkBoxSysout);
            this.groupBoxToolSettings.Controls.Add(this.buttonArguments);
            this.groupBoxToolSettings.Controls.Add(this.labelTool);
            this.groupBoxToolSettings.Controls.Add(this.buttonTool);
            this.groupBoxToolSettings.Controls.Add(this.textBoxTool);
            this.groupBoxToolSettings.Controls.Add(this.labelArguments);
            this.groupBoxToolSettings.Controls.Add(this.textBoxArguments);
            this.groupBoxToolSettings.Location = new System.Drawing.Point(7, 124);
            this.groupBoxToolSettings.Name = "groupBoxToolSettings";
            this.groupBoxToolSettings.Size = new System.Drawing.Size(608, 148);
            this.groupBoxToolSettings.TabIndex = 0;
            this.groupBoxToolSettings.TabStop = false;
            this.groupBoxToolSettings.Text = "Tool settings";
            // 
            // labelWorkingDir
            // 
            this.labelWorkingDir.AutoSize = true;
            this.labelWorkingDir.Location = new System.Drawing.Point(316, 56);
            this.labelWorkingDir.Name = "labelWorkingDir";
            this.labelWorkingDir.Size = new System.Drawing.Size(64, 13);
            this.labelWorkingDir.TabIndex = 11;
            this.labelWorkingDir.Text = "Working dir:";
            // 
            // buttonWorkingDir
            // 
            this.buttonWorkingDir.Location = new System.Drawing.Point(571, 52);
            this.buttonWorkingDir.Name = "buttonWorkingDir";
            this.buttonWorkingDir.Size = new System.Drawing.Size(30, 20);
            this.buttonWorkingDir.TabIndex = 10;
            this.buttonWorkingDir.Text = "...";
            this.buttonWorkingDir.UseVisualStyleBackColor = true;
            this.buttonWorkingDir.Click += new System.EventHandler(this.workingDirButton_Click);
            // 
            // textBoxWorkingDir
            // 
            this.textBoxWorkingDir.Location = new System.Drawing.Point(384, 53);
            this.textBoxWorkingDir.Name = "textBoxWorkingDir";
            this.textBoxWorkingDir.Size = new System.Drawing.Size(181, 20);
            this.textBoxWorkingDir.TabIndex = 9;
            // 
            // buttonIcon
            // 
            this.buttonIcon.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonIcon.Location = new System.Drawing.Point(279, 17);
            this.buttonIcon.Name = "buttonIcon";
            this.buttonIcon.Size = new System.Drawing.Size(75, 23);
            this.buttonIcon.TabIndex = 1;
            this.buttonIcon.Text = "   Icon...";
            this.buttonIcon.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonIcon.UseVisualStyleBackColor = true;
            this.buttonIcon.Click += new System.EventHandler(this.iconButton_Click);
            // 
            // labelToolName
            // 
            this.labelToolName.AutoSize = true;
            this.labelToolName.Location = new System.Drawing.Point(6, 22);
            this.labelToolName.Name = "labelToolName";
            this.labelToolName.Size = new System.Drawing.Size(38, 13);
            this.labelToolName.TabIndex = 8;
            this.labelToolName.Text = "Name:";
            // 
            // labelToolColumnizerForOutput
            // 
            this.labelToolColumnizerForOutput.AutoSize = true;
            this.labelToolColumnizerForOutput.Location = new System.Drawing.Point(269, 120);
            this.labelToolColumnizerForOutput.Name = "labelToolColumnizerForOutput";
            this.labelToolColumnizerForOutput.Size = new System.Drawing.Size(109, 13);
            this.labelToolColumnizerForOutput.TabIndex = 6;
            this.labelToolColumnizerForOutput.Text = "Columnizer for output:";
            // 
            // comboBoxColumnizer
            // 
            this.comboBoxColumnizer.FormattingEnabled = true;
            this.comboBoxColumnizer.Location = new System.Drawing.Point(384, 117);
            this.comboBoxColumnizer.Name = "comboBoxColumnizer";
            this.comboBoxColumnizer.Size = new System.Drawing.Size(181, 21);
            this.comboBoxColumnizer.TabIndex = 7;
            // 
            // textBoxToolName
            // 
            this.textBoxToolName.Location = new System.Drawing.Point(72, 19);
            this.textBoxToolName.Name = "textBoxToolName";
            this.textBoxToolName.Size = new System.Drawing.Size(200, 20);
            this.textBoxToolName.TabIndex = 0;
            // 
            // checkBoxSysout
            // 
            this.checkBoxSysout.AutoSize = true;
            this.checkBoxSysout.Location = new System.Drawing.Point(72, 119);
            this.checkBoxSysout.Name = "checkBoxSysout";
            this.checkBoxSysout.Size = new System.Drawing.Size(110, 17);
            this.checkBoxSysout.TabIndex = 6;
            this.checkBoxSysout.Text = "Pipe sysout to tab";
            this.checkBoxSysout.UseVisualStyleBackColor = true;
            this.checkBoxSysout.CheckedChanged += new System.EventHandler(this.sysoutCheckBoxA_CheckedChanged);
            // 
            // buttonArguments
            // 
            this.buttonArguments.Location = new System.Drawing.Point(571, 83);
            this.buttonArguments.Name = "buttonArguments";
            this.buttonArguments.Size = new System.Drawing.Size(31, 21);
            this.buttonArguments.TabIndex = 5;
            this.buttonArguments.Text = "...";
            this.buttonArguments.UseVisualStyleBackColor = true;
            this.buttonArguments.Click += new System.EventHandler(this.argButtonA_Click);
            // 
            // labelTool
            // 
            this.labelTool.AutoSize = true;
            this.labelTool.Location = new System.Drawing.Point(6, 56);
            this.labelTool.Name = "labelTool";
            this.labelTool.Size = new System.Drawing.Size(49, 13);
            this.labelTool.TabIndex = 4;
            this.labelTool.Text = "Program:";
            // 
            // buttonTool
            // 
            this.buttonTool.Location = new System.Drawing.Point(279, 51);
            this.buttonTool.Name = "buttonTool";
            this.buttonTool.Size = new System.Drawing.Size(30, 20);
            this.buttonTool.TabIndex = 3;
            this.buttonTool.Text = "...";
            this.buttonTool.UseVisualStyleBackColor = true;
            this.buttonTool.Click += new System.EventHandler(this.toolButtonA_Click);
            // 
            // textBoxTool
            // 
            this.textBoxTool.Location = new System.Drawing.Point(72, 52);
            this.textBoxTool.Name = "textBoxTool";
            this.textBoxTool.Size = new System.Drawing.Size(200, 20);
            this.textBoxTool.TabIndex = 2;
            // 
            // labelArguments
            // 
            this.labelArguments.AutoSize = true;
            this.labelArguments.Location = new System.Drawing.Point(6, 87);
            this.labelArguments.Name = "labelArguments";
            this.labelArguments.Size = new System.Drawing.Size(60, 13);
            this.labelArguments.TabIndex = 1;
            this.labelArguments.Text = "Arguments:";
            // 
            // textBoxArguments
            // 
            this.textBoxArguments.Location = new System.Drawing.Point(72, 84);
            this.textBoxArguments.Name = "textBoxArguments";
            this.textBoxArguments.Size = new System.Drawing.Size(493, 20);
            this.textBoxArguments.TabIndex = 4;
            // 
            // tabPageColumnizers
            // 
            this.tabPageColumnizers.Controls.Add(this.checkBoxAutoPick);
            this.tabPageColumnizers.Controls.Add(this.checkBoxMaskPrio);
            this.tabPageColumnizers.Controls.Add(this.buttonDelete);
            this.tabPageColumnizers.Controls.Add(this.dataGridViewColumnizer);
            this.tabPageColumnizers.Location = new System.Drawing.Point(4, 22);
            this.tabPageColumnizers.Name = "tabPageColumnizers";
            this.tabPageColumnizers.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageColumnizers.Size = new System.Drawing.Size(625, 278);
            this.tabPageColumnizers.TabIndex = 3;
            this.tabPageColumnizers.Text = "Columnizers";
            this.tabPageColumnizers.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoPick
            // 
            this.checkBoxAutoPick.AutoSize = true;
            this.checkBoxAutoPick.Checked = true;
            this.checkBoxAutoPick.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoPick.Location = new System.Drawing.Point(353, 251);
            this.checkBoxAutoPick.Name = "checkBoxAutoPick";
            this.checkBoxAutoPick.Size = new System.Drawing.Size(170, 17);
            this.checkBoxAutoPick.TabIndex = 5;
            this.checkBoxAutoPick.Text = "Automatically pick for new files";
            this.checkBoxAutoPick.UseVisualStyleBackColor = true;
            // 
            // checkBoxMaskPrio
            // 
            this.checkBoxMaskPrio.AutoSize = true;
            this.checkBoxMaskPrio.Location = new System.Drawing.Point(142, 252);
            this.checkBoxMaskPrio.Name = "checkBoxMaskPrio";
            this.checkBoxMaskPrio.Size = new System.Drawing.Size(171, 17);
            this.checkBoxMaskPrio.TabIndex = 4;
            this.checkBoxMaskPrio.Text = "Mask has priority before history";
            this.checkBoxMaskPrio.UseVisualStyleBackColor = true;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(8, 247);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 3;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // dataGridViewColumnizer
            // 
            this.dataGridViewColumnizer.AllowUserToResizeRows = false;
            this.dataGridViewColumnizer.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewColumnizer.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dataGridViewColumnizer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewColumnizer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnFileMask,
            this.columnColumnizer});
            this.dataGridViewColumnizer.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridViewColumnizer.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewColumnizer.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewColumnizer.Name = "dataGridViewColumnizer";
            this.dataGridViewColumnizer.Size = new System.Drawing.Size(619, 237);
            this.dataGridViewColumnizer.TabIndex = 2;
            this.dataGridViewColumnizer.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.columnizerDataGridView_RowsAdded);
            // 
            // columnFileMask
            // 
            this.columnFileMask.HeaderText = "File name mask (RegEx)";
            this.columnFileMask.MinimumWidth = 40;
            this.columnFileMask.Name = "columnFileMask";
            // 
            // columnColumnizer
            // 
            this.columnColumnizer.HeaderText = "Columnizer";
            this.columnColumnizer.MinimumWidth = 230;
            this.columnColumnizer.Name = "columnColumnizer";
            // 
            // tabPageHighlightMask
            // 
            this.tabPageHighlightMask.Controls.Add(this.dataGridViewHighlightMask);
            this.tabPageHighlightMask.Location = new System.Drawing.Point(4, 22);
            this.tabPageHighlightMask.Name = "tabPageHighlightMask";
            this.tabPageHighlightMask.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHighlightMask.Size = new System.Drawing.Size(625, 278);
            this.tabPageHighlightMask.TabIndex = 8;
            this.tabPageHighlightMask.Text = "Highlight";
            this.tabPageHighlightMask.UseVisualStyleBackColor = true;
            // 
            // dataGridViewHighlightMask
            // 
            this.dataGridViewHighlightMask.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewHighlightMask.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewHighlightMask.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnFileName,
            this.columnHighlightGroup});
            this.dataGridViewHighlightMask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewHighlightMask.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewHighlightMask.Name = "dataGridViewHighlightMask";
            this.dataGridViewHighlightMask.Size = new System.Drawing.Size(619, 272);
            this.dataGridViewHighlightMask.TabIndex = 0;
            // 
            // columnFileName
            // 
            this.columnFileName.HeaderText = "File name mask (RegEx)";
            this.columnFileName.MinimumWidth = 40;
            this.columnFileName.Name = "columnFileName";
            // 
            // columnHighlightGroup
            // 
            this.columnHighlightGroup.HeaderText = "Highlight group";
            this.columnHighlightGroup.MinimumWidth = 50;
            this.columnHighlightGroup.Name = "columnHighlightGroup";
            // 
            // tabPageMultiFile
            // 
            this.tabPageMultiFile.Controls.Add(this.groupBoxDefaultFileNamePattern);
            this.tabPageMultiFile.Controls.Add(this.labelHintMultiFile);
            this.tabPageMultiFile.Controls.Add(this.labelNoteMultiFile);
            this.tabPageMultiFile.Controls.Add(this.groupBoxWhenOpeningMultiFile);
            this.tabPageMultiFile.Location = new System.Drawing.Point(4, 22);
            this.tabPageMultiFile.Name = "tabPageMultiFile";
            this.tabPageMultiFile.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMultiFile.Size = new System.Drawing.Size(625, 278);
            this.tabPageMultiFile.TabIndex = 4;
            this.tabPageMultiFile.Text = "MultiFile";
            this.tabPageMultiFile.UseVisualStyleBackColor = true;
            // 
            // groupBoxDefaultFileNamePattern
            // 
            this.groupBoxDefaultFileNamePattern.Controls.Add(this.labelMaxDays);
            this.groupBoxDefaultFileNamePattern.Controls.Add(this.labelPattern);
            this.groupBoxDefaultFileNamePattern.Controls.Add(this.upDownMultifileDays);
            this.groupBoxDefaultFileNamePattern.Controls.Add(this.textBoxMultifilePattern);
            this.groupBoxDefaultFileNamePattern.Location = new System.Drawing.Point(243, 18);
            this.groupBoxDefaultFileNamePattern.Name = "groupBoxDefaultFileNamePattern";
            this.groupBoxDefaultFileNamePattern.Size = new System.Drawing.Size(291, 100);
            this.groupBoxDefaultFileNamePattern.TabIndex = 3;
            this.groupBoxDefaultFileNamePattern.TabStop = false;
            this.groupBoxDefaultFileNamePattern.Text = "Default filename pattern";
            // 
            // labelMaxDays
            // 
            this.labelMaxDays.AutoSize = true;
            this.labelMaxDays.Location = new System.Drawing.Point(7, 49);
            this.labelMaxDays.Name = "labelMaxDays";
            this.labelMaxDays.Size = new System.Drawing.Size(55, 13);
            this.labelMaxDays.TabIndex = 3;
            this.labelMaxDays.Text = "Max days:";
            // 
            // labelPattern
            // 
            this.labelPattern.AutoSize = true;
            this.labelPattern.Location = new System.Drawing.Point(7, 24);
            this.labelPattern.Name = "labelPattern";
            this.labelPattern.Size = new System.Drawing.Size(44, 13);
            this.labelPattern.TabIndex = 2;
            this.labelPattern.Text = "Pattern:";
            // 
            // upDownMultifileDays
            // 
            this.upDownMultifileDays.Location = new System.Drawing.Point(68, 47);
            this.upDownMultifileDays.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.upDownMultifileDays.Name = "upDownMultifileDays";
            this.helpProvider.SetShowHelp(this.upDownMultifileDays, false);
            this.upDownMultifileDays.Size = new System.Drawing.Size(61, 20);
            this.upDownMultifileDays.TabIndex = 1;
            this.upDownMultifileDays.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // textBoxMultifilePattern
            // 
            this.textBoxMultifilePattern.Location = new System.Drawing.Point(68, 21);
            this.textBoxMultifilePattern.Name = "textBoxMultifilePattern";
            this.textBoxMultifilePattern.Size = new System.Drawing.Size(187, 20);
            this.textBoxMultifilePattern.TabIndex = 0;
            this.textBoxMultifilePattern.TextChanged += new System.EventHandler(this.multifilePattern_TextChanged);
            // 
            // labelHintMultiFile
            // 
            this.labelHintMultiFile.Location = new System.Drawing.Point(4, 132);
            this.labelHintMultiFile.Name = "labelHintMultiFile";
            this.labelHintMultiFile.Size = new System.Drawing.Size(203, 72);
            this.labelHintMultiFile.TabIndex = 2;
            this.labelHintMultiFile.Text = "Hint: Pressing the Shift key while dropping files onto LogExpert will switch the " +
    "behaviour from single to multi and vice versa.";
            // 
            // labelNoteMultiFile
            // 
            this.labelNoteMultiFile.Location = new System.Drawing.Point(4, 204);
            this.labelNoteMultiFile.Name = "labelNoteMultiFile";
            this.labelNoteMultiFile.Size = new System.Drawing.Size(470, 53);
            this.labelNoteMultiFile.TabIndex = 1;
            this.labelNoteMultiFile.Text = resources.GetString("labelNoteMultiFile.Text");
            // 
            // groupBoxWhenOpeningMultiFile
            // 
            this.groupBoxWhenOpeningMultiFile.Controls.Add(this.radioButtonAskWhatToDo);
            this.groupBoxWhenOpeningMultiFile.Controls.Add(this.radioButtonTreatAllFilesAsOneMultifile);
            this.groupBoxWhenOpeningMultiFile.Controls.Add(this.radioButtonLoadEveryFileIntoSeperatedTab);
            this.groupBoxWhenOpeningMultiFile.Location = new System.Drawing.Point(7, 18);
            this.groupBoxWhenOpeningMultiFile.Name = "groupBoxWhenOpeningMultiFile";
            this.groupBoxWhenOpeningMultiFile.Size = new System.Drawing.Size(200, 100);
            this.groupBoxWhenOpeningMultiFile.TabIndex = 0;
            this.groupBoxWhenOpeningMultiFile.TabStop = false;
            this.groupBoxWhenOpeningMultiFile.Text = "When opening multiple files...";
            // 
            // radioButtonAskWhatToDo
            // 
            this.radioButtonAskWhatToDo.AutoSize = true;
            this.radioButtonAskWhatToDo.Location = new System.Drawing.Point(7, 68);
            this.radioButtonAskWhatToDo.Name = "radioButtonAskWhatToDo";
            this.radioButtonAskWhatToDo.Size = new System.Drawing.Size(96, 17);
            this.radioButtonAskWhatToDo.TabIndex = 2;
            this.radioButtonAskWhatToDo.TabStop = true;
            this.radioButtonAskWhatToDo.Text = "Ask what to do";
            this.radioButtonAskWhatToDo.UseVisualStyleBackColor = true;
            // 
            // radioButtonTreatAllFilesAsOneMultifile
            // 
            this.radioButtonTreatAllFilesAsOneMultifile.AutoSize = true;
            this.radioButtonTreatAllFilesAsOneMultifile.Location = new System.Drawing.Point(7, 44);
            this.radioButtonTreatAllFilesAsOneMultifile.Name = "radioButtonTreatAllFilesAsOneMultifile";
            this.radioButtonTreatAllFilesAsOneMultifile.Size = new System.Drawing.Size(164, 17);
            this.radioButtonTreatAllFilesAsOneMultifile.TabIndex = 1;
            this.radioButtonTreatAllFilesAsOneMultifile.TabStop = true;
            this.radioButtonTreatAllFilesAsOneMultifile.Text = "Treat all files as one \'MultiFile\'";
            this.radioButtonTreatAllFilesAsOneMultifile.UseVisualStyleBackColor = true;
            // 
            // radioButtonLoadEveryFileIntoSeperatedTab
            // 
            this.radioButtonLoadEveryFileIntoSeperatedTab.AutoSize = true;
            this.radioButtonLoadEveryFileIntoSeperatedTab.Location = new System.Drawing.Point(7, 20);
            this.radioButtonLoadEveryFileIntoSeperatedTab.Name = "radioButtonLoadEveryFileIntoSeperatedTab";
            this.radioButtonLoadEveryFileIntoSeperatedTab.Size = new System.Drawing.Size(185, 17);
            this.radioButtonLoadEveryFileIntoSeperatedTab.TabIndex = 0;
            this.radioButtonLoadEveryFileIntoSeperatedTab.TabStop = true;
            this.radioButtonLoadEveryFileIntoSeperatedTab.Text = "Load every file into a separate tab";
            this.radioButtonLoadEveryFileIntoSeperatedTab.UseVisualStyleBackColor = true;
            // 
            // tabPagePlugins
            // 
            this.tabPagePlugins.Controls.Add(this.groupBoxPlugins);
            this.tabPagePlugins.Controls.Add(this.groupBoxSettings);
            this.tabPagePlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPagePlugins.Name = "tabPagePlugins";
            this.tabPagePlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePlugins.Size = new System.Drawing.Size(625, 278);
            this.tabPagePlugins.TabIndex = 5;
            this.tabPagePlugins.Text = "Plugins";
            this.tabPagePlugins.UseVisualStyleBackColor = true;
            // 
            // groupBoxPlugins
            // 
            this.groupBoxPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPlugins.Controls.Add(this.listBoxPlugin);
            this.groupBoxPlugins.Location = new System.Drawing.Point(7, 15);
            this.groupBoxPlugins.Name = "groupBoxPlugins";
            this.groupBoxPlugins.Size = new System.Drawing.Size(228, 257);
            this.groupBoxPlugins.TabIndex = 3;
            this.groupBoxPlugins.TabStop = false;
            this.groupBoxPlugins.Text = "Plugins";
            // 
            // listBoxPlugin
            // 
            this.listBoxPlugin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPlugin.DisplayMember = "Text";
            this.listBoxPlugin.FormattingEnabled = true;
            this.listBoxPlugin.Location = new System.Drawing.Point(6, 19);
            this.listBoxPlugin.Name = "listBoxPlugin";
            this.listBoxPlugin.Size = new System.Drawing.Size(216, 225);
            this.listBoxPlugin.TabIndex = 0;
            this.listBoxPlugin.ValueMember = "Text";
            this.listBoxPlugin.SelectedIndexChanged += new System.EventHandler(this.pluginListBox_SelectedIndexChanged);
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSettings.Controls.Add(this.panelPlugin);
            this.groupBoxSettings.Location = new System.Drawing.Point(241, 15);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(378, 257);
            this.groupBoxSettings.TabIndex = 2;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Settings";
            // 
            // panelPlugin
            // 
            this.panelPlugin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPlugin.AutoScroll = true;
            this.panelPlugin.Controls.Add(this.buttonConfigPlugin);
            this.panelPlugin.Location = new System.Drawing.Point(6, 19);
            this.panelPlugin.Name = "panelPlugin";
            this.panelPlugin.Size = new System.Drawing.Size(366, 232);
            this.panelPlugin.TabIndex = 1;
            // 
            // buttonConfigPlugin
            // 
            this.buttonConfigPlugin.Location = new System.Drawing.Point(109, 106);
            this.buttonConfigPlugin.Name = "buttonConfigPlugin";
            this.buttonConfigPlugin.Size = new System.Drawing.Size(113, 23);
            this.buttonConfigPlugin.TabIndex = 0;
            this.buttonConfigPlugin.Text = "Configure...";
            this.buttonConfigPlugin.UseVisualStyleBackColor = true;
            this.buttonConfigPlugin.Click += new System.EventHandler(this.configPluginButton_Click);
            // 
            // tabPageSessions
            // 
            this.tabPageSessions.Controls.Add(this.checkBoxSaveFilter);
            this.tabPageSessions.Controls.Add(this.groupBoxPersistantFileLocation);
            this.tabPageSessions.Controls.Add(this.checkBoxSaveSessions);
            this.tabPageSessions.Location = new System.Drawing.Point(4, 22);
            this.tabPageSessions.Name = "tabPageSessions";
            this.tabPageSessions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSessions.Size = new System.Drawing.Size(625, 278);
            this.tabPageSessions.TabIndex = 6;
            this.tabPageSessions.Text = "Persistence";
            this.tabPageSessions.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveFilter
            // 
            this.checkBoxSaveFilter.AutoSize = true;
            this.checkBoxSaveFilter.Location = new System.Drawing.Point(23, 48);
            this.checkBoxSaveFilter.Name = "checkBoxSaveFilter";
            this.checkBoxSaveFilter.Size = new System.Drawing.Size(198, 17);
            this.checkBoxSaveFilter.TabIndex = 2;
            this.checkBoxSaveFilter.Text = " Save and restore filter and filter tabs";
            this.checkBoxSaveFilter.UseVisualStyleBackColor = true;
            // 
            // groupBoxPersistantFileLocation
            // 
            this.groupBoxPersistantFileLocation.Controls.Add(this.checkBoxPortableMode);
            this.groupBoxPersistantFileLocation.Controls.Add(this.labelSessionSaveOwnDir);
            this.groupBoxPersistantFileLocation.Controls.Add(this.buttonSessionSaveDir);
            this.groupBoxPersistantFileLocation.Controls.Add(this.radioButtonSessionSaveOwn);
            this.groupBoxPersistantFileLocation.Controls.Add(this.radioButtonsessionSaveDocuments);
            this.groupBoxPersistantFileLocation.Controls.Add(this.radioButtonSessionSameDir);
            this.groupBoxPersistantFileLocation.Location = new System.Drawing.Point(23, 87);
            this.groupBoxPersistantFileLocation.Name = "groupBoxPersistantFileLocation";
            this.groupBoxPersistantFileLocation.Size = new System.Drawing.Size(274, 137);
            this.groupBoxPersistantFileLocation.TabIndex = 1;
            this.groupBoxPersistantFileLocation.TabStop = false;
            this.groupBoxPersistantFileLocation.Text = "Persistence file location";
            // 
            // checkBoxPortableMode
            // 
            this.checkBoxPortableMode.AutoSize = true;
            this.checkBoxPortableMode.Location = new System.Drawing.Point(7, 112);
            this.checkBoxPortableMode.Name = "checkBoxPortableMode";
            this.checkBoxPortableMode.Size = new System.Drawing.Size(137, 17);
            this.checkBoxPortableMode.TabIndex = 3;
            this.checkBoxPortableMode.Text = "Activate Portable Mode";
            this.toolTip.SetToolTip(this.checkBoxPortableMode, "If this mode is activated, the save file will be loaded from the Executable Locat" +
        "ion");
            this.checkBoxPortableMode.UseVisualStyleBackColor = true;
            this.checkBoxPortableMode.CheckedChanged += new System.EventHandler(this.OnPortableModeCheckedChanged);
            // 
            // labelSessionSaveOwnDir
            // 
            this.labelSessionSaveOwnDir.Location = new System.Drawing.Point(26, 92);
            this.labelSessionSaveOwnDir.Name = "labelSessionSaveOwnDir";
            this.labelSessionSaveOwnDir.Size = new System.Drawing.Size(168, 20);
            this.labelSessionSaveOwnDir.TabIndex = 4;
            this.labelSessionSaveOwnDir.Text = "sessionSaveOwnDirLabel";
            // 
            // buttonSessionSaveDir
            // 
            this.buttonSessionSaveDir.Location = new System.Drawing.Point(238, 66);
            this.buttonSessionSaveDir.Name = "buttonSessionSaveDir";
            this.buttonSessionSaveDir.Size = new System.Drawing.Size(30, 20);
            this.buttonSessionSaveDir.TabIndex = 3;
            this.buttonSessionSaveDir.Text = "...";
            this.buttonSessionSaveDir.UseVisualStyleBackColor = true;
            this.buttonSessionSaveDir.Click += new System.EventHandler(this.sessionSaveDirButton_Click);
            // 
            // radioButtonSessionSaveOwn
            // 
            this.radioButtonSessionSaveOwn.AutoSize = true;
            this.radioButtonSessionSaveOwn.Location = new System.Drawing.Point(7, 68);
            this.radioButtonSessionSaveOwn.Name = "radioButtonSessionSaveOwn";
            this.radioButtonSessionSaveOwn.Size = new System.Drawing.Size(90, 17);
            this.radioButtonSessionSaveOwn.TabIndex = 2;
            this.radioButtonSessionSaveOwn.TabStop = true;
            this.radioButtonSessionSaveOwn.Text = "Own directory";
            this.radioButtonSessionSaveOwn.UseVisualStyleBackColor = true;
            // 
            // radioButtonsessionSaveDocuments
            // 
            this.radioButtonsessionSaveDocuments.AutoSize = true;
            this.radioButtonsessionSaveDocuments.Location = new System.Drawing.Point(7, 44);
            this.radioButtonsessionSaveDocuments.Name = "radioButtonsessionSaveDocuments";
            this.radioButtonsessionSaveDocuments.Size = new System.Drawing.Size(146, 17);
            this.radioButtonsessionSaveDocuments.TabIndex = 1;
            this.radioButtonsessionSaveDocuments.TabStop = true;
            this.radioButtonsessionSaveDocuments.Text = "MyDocuments/LogExpert";
            this.radioButtonsessionSaveDocuments.UseVisualStyleBackColor = true;
            // 
            // radioButtonSessionSameDir
            // 
            this.radioButtonSessionSameDir.AutoSize = true;
            this.radioButtonSessionSameDir.Location = new System.Drawing.Point(7, 20);
            this.radioButtonSessionSameDir.Name = "radioButtonSessionSameDir";
            this.radioButtonSessionSameDir.Size = new System.Drawing.Size(142, 17);
            this.radioButtonSessionSameDir.TabIndex = 0;
            this.radioButtonSessionSameDir.TabStop = true;
            this.radioButtonSessionSameDir.Text = "Same directory as log file";
            this.radioButtonSessionSameDir.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveSessions
            // 
            this.checkBoxSaveSessions.AutoSize = true;
            this.checkBoxSaveSessions.Location = new System.Drawing.Point(23, 25);
            this.checkBoxSaveSessions.Name = "checkBoxSaveSessions";
            this.checkBoxSaveSessions.Size = new System.Drawing.Size(217, 17);
            this.checkBoxSaveSessions.TabIndex = 0;
            this.checkBoxSaveSessions.Text = "Automatically save persistence files (.lxp)";
            this.checkBoxSaveSessions.UseVisualStyleBackColor = true;
            // 
            // tabPageMemory
            // 
            this.tabPageMemory.Controls.Add(this.groupBoxCPUAndStuff);
            this.tabPageMemory.Controls.Add(this.groupBoxLineBufferUsage);
            this.tabPageMemory.Location = new System.Drawing.Point(4, 22);
            this.tabPageMemory.Name = "tabPageMemory";
            this.tabPageMemory.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMemory.Size = new System.Drawing.Size(625, 278);
            this.tabPageMemory.TabIndex = 7;
            this.tabPageMemory.Text = "Memory/CPU";
            this.tabPageMemory.UseVisualStyleBackColor = true;
            // 
            // groupBoxCPUAndStuff
            // 
            this.groupBoxCPUAndStuff.Controls.Add(this.checkBoxLegacyReader);
            this.groupBoxCPUAndStuff.Controls.Add(this.checkBoxMultiThread);
            this.groupBoxCPUAndStuff.Controls.Add(this.labelFilePollingInterval);
            this.groupBoxCPUAndStuff.Controls.Add(this.upDownPollingInterval);
            this.groupBoxCPUAndStuff.Location = new System.Drawing.Point(272, 19);
            this.groupBoxCPUAndStuff.Name = "groupBoxCPUAndStuff";
            this.groupBoxCPUAndStuff.Size = new System.Drawing.Size(200, 128);
            this.groupBoxCPUAndStuff.TabIndex = 8;
            this.groupBoxCPUAndStuff.TabStop = false;
            this.groupBoxCPUAndStuff.Text = "CPU and stuff";
            // 
            // checkBoxLegacyReader
            // 
            this.checkBoxLegacyReader.AutoSize = true;
            this.checkBoxLegacyReader.Location = new System.Drawing.Point(9, 90);
            this.checkBoxLegacyReader.Name = "checkBoxLegacyReader";
            this.checkBoxLegacyReader.Size = new System.Drawing.Size(167, 17);
            this.checkBoxLegacyReader.TabIndex = 9;
            this.checkBoxLegacyReader.Text = "Use legacy file reader (slower)";
            this.toolTip.SetToolTip(this.checkBoxLegacyReader, "Slower but more compatible with strange linefeeds and encodings");
            this.checkBoxLegacyReader.UseVisualStyleBackColor = true;
            // 
            // checkBoxMultiThread
            // 
            this.checkBoxMultiThread.AutoSize = true;
            this.checkBoxMultiThread.Location = new System.Drawing.Point(9, 67);
            this.checkBoxMultiThread.Name = "checkBoxMultiThread";
            this.checkBoxMultiThread.Size = new System.Drawing.Size(115, 17);
            this.checkBoxMultiThread.TabIndex = 5;
            this.checkBoxMultiThread.Text = "Multi threaded filter";
            this.checkBoxMultiThread.UseVisualStyleBackColor = true;
            // 
            // labelFilePollingInterval
            // 
            this.labelFilePollingInterval.AutoSize = true;
            this.labelFilePollingInterval.Location = new System.Drawing.Point(6, 34);
            this.labelFilePollingInterval.Name = "labelFilePollingInterval";
            this.labelFilePollingInterval.Size = new System.Drawing.Size(118, 13);
            this.labelFilePollingInterval.TabIndex = 7;
            this.labelFilePollingInterval.Text = "File polling interval (ms):";
            // 
            // upDownPollingInterval
            // 
            this.upDownPollingInterval.Location = new System.Drawing.Point(127, 32);
            this.upDownPollingInterval.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.upDownPollingInterval.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.upDownPollingInterval.Name = "upDownPollingInterval";
            this.upDownPollingInterval.Size = new System.Drawing.Size(57, 20);
            this.upDownPollingInterval.TabIndex = 6;
            this.upDownPollingInterval.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // groupBoxLineBufferUsage
            // 
            this.groupBoxLineBufferUsage.Controls.Add(this.labelInfo);
            this.groupBoxLineBufferUsage.Controls.Add(this.labelNumberOfBlocks);
            this.groupBoxLineBufferUsage.Controls.Add(this.upDownLinesPerBlock);
            this.groupBoxLineBufferUsage.Controls.Add(this.upDownBlockCount);
            this.groupBoxLineBufferUsage.Controls.Add(this.labelLinesPerBlock);
            this.groupBoxLineBufferUsage.Location = new System.Drawing.Point(7, 19);
            this.groupBoxLineBufferUsage.Name = "groupBoxLineBufferUsage";
            this.groupBoxLineBufferUsage.Size = new System.Drawing.Size(217, 128);
            this.groupBoxLineBufferUsage.TabIndex = 4;
            this.groupBoxLineBufferUsage.TabStop = false;
            this.groupBoxLineBufferUsage.Text = "Line buffer usage";
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(6, 94);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(197, 13);
            this.labelInfo.TabIndex = 4;
            this.labelInfo.Text = "Changes will take effect on next file load";
            // 
            // labelNumberOfBlocks
            // 
            this.labelNumberOfBlocks.AutoSize = true;
            this.labelNumberOfBlocks.Location = new System.Drawing.Point(6, 34);
            this.labelNumberOfBlocks.Name = "labelNumberOfBlocks";
            this.labelNumberOfBlocks.Size = new System.Drawing.Size(90, 13);
            this.labelNumberOfBlocks.TabIndex = 1;
            this.labelNumberOfBlocks.Text = "Number of blocks";
            // 
            // upDownLinesPerBlock
            // 
            this.upDownLinesPerBlock.Location = new System.Drawing.Point(140, 66);
            this.upDownLinesPerBlock.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.upDownLinesPerBlock.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.upDownLinesPerBlock.Name = "upDownLinesPerBlock";
            this.upDownLinesPerBlock.Size = new System.Drawing.Size(63, 20);
            this.upDownLinesPerBlock.TabIndex = 3;
            this.upDownLinesPerBlock.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.upDownLinesPerBlock.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // upDownBlockCount
            // 
            this.upDownBlockCount.Location = new System.Drawing.Point(140, 32);
            this.upDownBlockCount.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.upDownBlockCount.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.upDownBlockCount.Name = "upDownBlockCount";
            this.upDownBlockCount.Size = new System.Drawing.Size(63, 20);
            this.upDownBlockCount.TabIndex = 0;
            this.upDownBlockCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // labelLinesPerBlock
            // 
            this.labelLinesPerBlock.AutoSize = true;
            this.labelLinesPerBlock.Location = new System.Drawing.Point(6, 68);
            this.labelLinesPerBlock.Name = "labelLinesPerBlock";
            this.labelLinesPerBlock.Size = new System.Drawing.Size(63, 13);
            this.labelLinesPerBlock.TabIndex = 2;
            this.labelLinesPerBlock.Text = "Lines/block";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(545, 331);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(464, 331);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.okButton_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "LogExpert.chm";
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(13, 331);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 2;
            this.buttonExport.Text = "Export...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(95, 331);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 23);
            this.buttonImport.TabIndex = 3;
            this.buttonImport.Text = "Import...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.importButton_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "File name mask (RegEx)";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 40;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 99;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "File name mask (RegEx)";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 40;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Width = 259;
            // 
            // SettingsDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(637, 366);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tabControlSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.helpProvider.SetHelpKeyword(this, "Settings.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.helpProvider.SetShowHelp(this, true);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.tabControlSettings.ResumeLayout(false);
            this.tabPageViewSettings.ResumeLayout(false);
            this.tabPageViewSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMaximumFilterEntriesDisplayed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMaximumFilterEntries)).EndInit();
            this.groupBoxMisc.ResumeLayout(false);
            this.groupBoxMisc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cpDownColumnWidth)).EndInit();
            this.groupBoxDefaults.ResumeLayout(false);
            this.groupBoxDefaults.PerformLayout();
            this.groupBoxFont.ResumeLayout(false);
            this.tabPageTimeStampFeatures.ResumeLayout(false);
            this.groupBoxTimeSpreadDisplay.ResumeLayout(false);
            this.groupBoxTimeSpreadDisplay.PerformLayout();
            this.groupBoxDisplayMode.ResumeLayout(false);
            this.groupBoxDisplayMode.PerformLayout();
            this.groupBoxTimeStampNavigationControl.ResumeLayout(false);
            this.groupBoxTimeStampNavigationControl.PerformLayout();
            this.groupBoxMouseDragDefaults.ResumeLayout(false);
            this.groupBoxMouseDragDefaults.PerformLayout();
            this.tabPageExternalTools.ResumeLayout(false);
            this.groupBoxToolSettings.ResumeLayout(false);
            this.groupBoxToolSettings.PerformLayout();
            this.tabPageColumnizers.ResumeLayout(false);
            this.tabPageColumnizers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewColumnizer)).EndInit();
            this.tabPageHighlightMask.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewHighlightMask)).EndInit();
            this.tabPageMultiFile.ResumeLayout(false);
            this.groupBoxDefaultFileNamePattern.ResumeLayout(false);
            this.groupBoxDefaultFileNamePattern.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownMultifileDays)).EndInit();
            this.groupBoxWhenOpeningMultiFile.ResumeLayout(false);
            this.groupBoxWhenOpeningMultiFile.PerformLayout();
            this.tabPagePlugins.ResumeLayout(false);
            this.groupBoxPlugins.ResumeLayout(false);
            this.groupBoxSettings.ResumeLayout(false);
            this.panelPlugin.ResumeLayout(false);
            this.tabPageSessions.ResumeLayout(false);
            this.tabPageSessions.PerformLayout();
            this.groupBoxPersistantFileLocation.ResumeLayout(false);
            this.groupBoxPersistantFileLocation.PerformLayout();
            this.tabPageMemory.ResumeLayout(false);
            this.groupBoxCPUAndStuff.ResumeLayout(false);
            this.groupBoxCPUAndStuff.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownPollingInterval)).EndInit();
            this.groupBoxLineBufferUsage.ResumeLayout(false);
            this.groupBoxLineBufferUsage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownLinesPerBlock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownBlockCount)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControlSettings;
    private System.Windows.Forms.TabPage tabPageViewSettings;
    private System.Windows.Forms.Label labelFont;
    private System.Windows.Forms.TabPage tabPageTimeStampFeatures;
    private System.Windows.Forms.GroupBox groupBoxFont;
    private System.Windows.Forms.Button buttonChangeFont;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.CheckBox checkBoxFilterTail;
    private System.Windows.Forms.CheckBox checkBoxFollowTail;
    private System.Windows.Forms.CheckBox checkBoxSyncFilter;
    private System.Windows.Forms.GroupBox groupBoxDefaults;
    private System.Windows.Forms.CheckBox checkBoxTimestamp;
    private System.Windows.Forms.GroupBox groupBoxMouseDragDefaults;
    private System.Windows.Forms.RadioButton radioButtonHorizMouseDrag;
    private System.Windows.Forms.RadioButton radioButtonVerticalMouseDrag;
    private System.Windows.Forms.RadioButton radioButtonVerticalMouseDragInverted;
    private System.Windows.Forms.TabPage tabPageExternalTools;
    private System.Windows.Forms.GroupBox groupBoxToolSettings;
    private System.Windows.Forms.TextBox textBoxArguments;
    private System.Windows.Forms.Button buttonTool;
    private System.Windows.Forms.TextBox textBoxTool;
    private System.Windows.Forms.Label labelArguments;
    private System.Windows.Forms.Label labelTool;
    private System.Windows.Forms.Button buttonArguments;
    private System.Windows.Forms.TabPage tabPageColumnizers;
    private System.Windows.Forms.DataGridView dataGridViewColumnizer;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private System.Windows.Forms.Button buttonDelete;
    private System.Windows.Forms.DataGridViewTextBoxColumn columnFileMask;
    private System.Windows.Forms.DataGridViewComboBoxColumn columnColumnizer;
    private System.Windows.Forms.CheckBox checkBoxSysout;
    private System.Windows.Forms.CheckBox checkBoxMaskPrio;
    private System.Windows.Forms.GroupBox groupBoxMisc;
    private System.Windows.Forms.CheckBox checkBoxAskCloseTabs;
    private System.Windows.Forms.TabPage tabPageMultiFile;
    private System.Windows.Forms.GroupBox groupBoxWhenOpeningMultiFile;
    private System.Windows.Forms.RadioButton radioButtonAskWhatToDo;
    private System.Windows.Forms.RadioButton radioButtonTreatAllFilesAsOneMultifile;
    private System.Windows.Forms.RadioButton radioButtonLoadEveryFileIntoSeperatedTab;
    private System.Windows.Forms.Label labelNoteMultiFile;
    private System.Windows.Forms.Label labelHintMultiFile;
    private System.Windows.Forms.HelpProvider helpProvider;
    private System.Windows.Forms.CheckBox checkBoxSingleInstance;
    private System.Windows.Forms.ComboBox comboBoxColumnizer;
    private System.Windows.Forms.CheckBox checkBoxOpenLastFiles;
    private System.Windows.Forms.CheckBox checkBoxTailState;
    private System.Windows.Forms.Button buttonTailColor;
    private System.Windows.Forms.NumericUpDown cpDownColumnWidth;
    private System.Windows.Forms.CheckBox checkBoxColumnSize;
    private System.Windows.Forms.CheckBox checkBoxTimeSpread;
    private System.Windows.Forms.GroupBox groupBoxTimeSpreadDisplay;
    private System.Windows.Forms.GroupBox groupBoxTimeStampNavigationControl;
    private System.Windows.Forms.Button buttonTimespreadColor;
    private System.Windows.Forms.CheckBox checkBoxReverseAlpha;
    private System.Windows.Forms.GroupBox groupBoxDisplayMode;
    private System.Windows.Forms.RadioButton radioButtonLineView;
    private System.Windows.Forms.RadioButton radioButtonTimeView;
    private System.Windows.Forms.TabPage tabPagePlugins;
    private System.Windows.Forms.ListBox listBoxPlugin;
    private System.Windows.Forms.Panel panelPlugin;
    private System.Windows.Forms.GroupBox groupBoxPlugins;
    private System.Windows.Forms.GroupBox groupBoxSettings;
    private System.Windows.Forms.Button buttonConfigPlugin;
    private System.Windows.Forms.TabPage tabPageSessions;
    private System.Windows.Forms.GroupBox groupBoxPersistantFileLocation;
    private System.Windows.Forms.RadioButton radioButtonsessionSaveDocuments;
    private System.Windows.Forms.RadioButton radioButtonSessionSameDir;
    private System.Windows.Forms.CheckBox checkBoxSaveSessions;
    private System.Windows.Forms.RadioButton radioButtonSessionSaveOwn;
    private System.Windows.Forms.Label labelSessionSaveOwnDir;
    private System.Windows.Forms.Button buttonSessionSaveDir;
    private System.Windows.Forms.CheckBox checkBoxSaveFilter;
    private System.Windows.Forms.TabPage tabPageMemory;
    private System.Windows.Forms.Label labelNumberOfBlocks;
    private System.Windows.Forms.NumericUpDown upDownBlockCount;
    private System.Windows.Forms.NumericUpDown upDownLinesPerBlock;
    private System.Windows.Forms.Label labelLinesPerBlock;
    private System.Windows.Forms.GroupBox groupBoxLineBufferUsage;
    private System.Windows.Forms.Label labelInfo;
    private System.Windows.Forms.TabPage tabPageHighlightMask;
    private System.Windows.Forms.DataGridView dataGridViewHighlightMask;
    private System.Windows.Forms.DataGridViewTextBoxColumn columnFileName;
    private System.Windows.Forms.DataGridViewComboBoxColumn columnHighlightGroup;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private System.Windows.Forms.CheckBox checkBoxMultiThread;
    private System.Windows.Forms.Label labelFilePollingInterval;
    private System.Windows.Forms.NumericUpDown upDownPollingInterval;
    private System.Windows.Forms.GroupBox groupBoxCPUAndStuff;
    private System.Windows.Forms.Label labelToolColumnizerForOutput;
    private System.Windows.Forms.Label labelToolName;
    private System.Windows.Forms.CheckedListBox listBoxTools;
    private System.Windows.Forms.TextBox textBoxToolName;
    private System.Windows.Forms.Button buttonToolDelete;
    private System.Windows.Forms.Button buttonToolAdd;
    private System.Windows.Forms.Button buttonToolDown;
    private System.Windows.Forms.Button buttonToolUp;
    private System.Windows.Forms.Label labelToolsDescription;
    private System.Windows.Forms.Button buttonIcon;
    private System.Windows.Forms.Label labelWorkingDir;
    private System.Windows.Forms.Button buttonWorkingDir;
    private System.Windows.Forms.TextBox textBoxWorkingDir;
    private System.Windows.Forms.GroupBox groupBoxDefaultFileNamePattern;
    private System.Windows.Forms.Label labelMaxDays;
    private System.Windows.Forms.Label labelPattern;
    private System.Windows.Forms.NumericUpDown upDownMultifileDays;
    private System.Windows.Forms.TextBox textBoxMultifilePattern;
    private System.Windows.Forms.Label labelDefaultEncoding;
    private System.Windows.Forms.ComboBox encodingComboBox;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.CheckBox checkBoxColumnFinder;
    private System.Windows.Forms.Button buttonExport;
    private System.Windows.Forms.Button buttonImport;
    private System.Windows.Forms.CheckBox checkBoxLegacyReader;
        private System.Windows.Forms.Label labelMaximumFilterEntries;
        private System.Windows.Forms.NumericUpDown upDownMaximumFilterEntries;
        private System.Windows.Forms.NumericUpDown upDownMaximumFilterEntriesDisplayed;
        private System.Windows.Forms.Label labelMaximumFilterEntriesDisplayed;
        private System.Windows.Forms.CheckBox checkBoxAutoPick;
        private System.Windows.Forms.CheckBox checkBoxPortableMode;
    }
}
