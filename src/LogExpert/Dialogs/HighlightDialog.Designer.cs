using LogExpert.Properties;
using System.Drawing;

namespace LogExpert.Dialogs
{
    partial class HighlightDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HighlightDialog));
            this.listBoxHighlight = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.labelForgroundColor = new System.Windows.Forms.Label();
            this.labelBackgroundColor = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.textBoxSearchString = new System.Windows.Forms.TextBox();
            this.labelSearchString = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCustomForeColor = new System.Windows.Forms.Button();
            this.btnCustomBackColor = new System.Windows.Forms.Button();
            this.checkBoxRegex = new System.Windows.Forms.CheckBox();
            this.checkBoxCaseSensitive = new System.Windows.Forms.CheckBox();
            this.checkBoxDontDirtyLed = new System.Windows.Forms.CheckBox();
            this.groupBoxLineMatchCriteria = new System.Windows.Forms.GroupBox();
            this.groupBoxColoring = new System.Windows.Forms.GroupBox();
            this.checkBoxNoBackground = new System.Windows.Forms.CheckBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.checkBoxWordMatch = new System.Windows.Forms.CheckBox();
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.btnBookmarkComment = new System.Windows.Forms.Button();
            this.btnSelectPlugin = new System.Windows.Forms.Button();
            this.checkBoxPlugin = new System.Windows.Forms.CheckBox();
            this.checkBoxStopTail = new System.Windows.Forms.CheckBox();
            this.checkBoxBookmark = new System.Windows.Forms.CheckBox();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnImportGroup = new System.Windows.Forms.Button();
            this.btnExportGroup = new System.Windows.Forms.Button();
            this.btnMoveGroupDown = new System.Windows.Forms.Button();
            this.btnMoveGroupUp = new System.Windows.Forms.Button();
            this.labelAssignNamesToGroups = new System.Windows.Forms.Label();
            this.btnCopyGroup = new System.Windows.Forms.Button();
            this.btnDeleteGroup = new System.Windows.Forms.Button();
            this.btnNewGroup = new System.Windows.Forms.Button();
            this.comboBoxGroups = new System.Windows.Forms.ComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.colorBoxForeground = new LogExpert.Dialogs.ColorComboBox();
            this.colorBoxBackground = new LogExpert.Dialogs.ColorComboBox();
            this.groupBoxLineMatchCriteria.SuspendLayout();
            this.groupBoxColoring.SuspendLayout();
            this.groupBoxActions.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxHighlight
            // 
            this.listBoxHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxHighlight.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxHighlight.FormattingEnabled = true;
            this.listBoxHighlight.Location = new System.Drawing.Point(32, 160);
            this.listBoxHighlight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listBoxHighlight.Name = "listBoxHighlight";
            this.listBoxHighlight.Size = new System.Drawing.Size(423, 238);
            this.listBoxHighlight.TabIndex = 0;
            this.listBoxHighlight.SelectedIndexChanged += new System.EventHandler(this.OnListBoxHighlightSelectedIndexChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(465, 218);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(138, 35);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "&Add";
            this.toolTip.SetToolTip(this.btnAdd, "Create a new hilight item from information below");
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.OnAddButtonClick);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(465, 263);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(138, 35);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "D&elete";
            this.toolTip.SetToolTip(this.btnDelete, "Delete the current hilight");
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.OnDeleteButtonClick);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveUp.Location = new System.Drawing.Point(465, 160);
            this.btnMoveUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(68, 35);
            this.btnMoveUp.TabIndex = 3;
            this.btnMoveUp.Text = "&Up";
            this.toolTip.SetToolTip(this.btnMoveUp, "Move the current hilight one position up");
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.OnBtnMoveUpClick);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveDown.Location = new System.Drawing.Point(536, 160);
            this.btnMoveDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(68, 35);
            this.btnMoveDown.TabIndex = 4;
            this.btnMoveDown.Text = "&Down";
            this.toolTip.SetToolTip(this.btnMoveDown, "Move the current hilight one position down");
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.OnBtnMoveDownClick);
            // 
            // labelForgroundColor
            // 
            this.labelForgroundColor.AutoSize = true;
            this.labelForgroundColor.Location = new System.Drawing.Point(9, 38);
            this.labelForgroundColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelForgroundColor.Name = "labelForgroundColor";
            this.labelForgroundColor.Size = new System.Drawing.Size(87, 13);
            this.labelForgroundColor.TabIndex = 6;
            this.labelForgroundColor.Text = "Foreground color";
            // 
            // labelBackgroundColor
            // 
            this.labelBackgroundColor.AutoSize = true;
            this.labelBackgroundColor.Location = new System.Drawing.Point(9, 115);
            this.labelBackgroundColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBackgroundColor.Name = "labelBackgroundColor";
            this.labelBackgroundColor.Size = new System.Drawing.Size(91, 13);
            this.labelBackgroundColor.TabIndex = 8;
            this.labelBackgroundColor.Text = "Background color";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(385, 832);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(112, 35);
            this.btnOk.TabIndex = 9;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.OnBtnOkClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(512, 832);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 35);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxSearchString
            // 
            this.textBoxSearchString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearchString.Location = new System.Drawing.Point(9, 55);
            this.textBoxSearchString.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxSearchString.Name = "textBoxSearchString";
            this.textBoxSearchString.Size = new System.Drawing.Size(575, 20);
            this.textBoxSearchString.TabIndex = 11;
            this.textBoxSearchString.TextChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // labelSearchString
            // 
            this.labelSearchString.AutoSize = true;
            this.labelSearchString.Location = new System.Drawing.Point(9, 31);
            this.labelSearchString.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSearchString.Name = "labelSearchString";
            this.labelSearchString.Size = new System.Drawing.Size(72, 13);
            this.labelSearchString.TabIndex = 12;
            this.labelSearchString.Text = "Search string:";
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Enabled = false;
            this.btnApply.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnApply.Location = new System.Drawing.Point(465, 308);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(138, 35);
            this.btnApply.Image = new Bitmap(Resources.Check_circle, new Size(btnApply.Height, btnApply.Height));
            this.btnApply.TabIndex = 13;
            this.btnApply.Text = "A&pply";
            this.toolTip.SetToolTip(this.btnApply, "Apply changes below to current hiligth");
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.OnBtnApplyClick);
            // 
            // btnCustomForeColor
            // 
            this.btnCustomForeColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCustomForeColor.Location = new System.Drawing.Point(183, 60);
            this.btnCustomForeColor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCustomForeColor.Name = "btnCustomForeColor";
            this.btnCustomForeColor.Size = new System.Drawing.Size(78, 35);
            this.btnCustomForeColor.TabIndex = 14;
            this.btnCustomForeColor.Text = "Custom";
            this.toolTip.SetToolTip(this.btnCustomForeColor, "Pick a custom foreground color");
            this.btnCustomForeColor.UseVisualStyleBackColor = true;
            this.btnCustomForeColor.Click += new System.EventHandler(this.OnBtnCustomForeColorClick);
            // 
            // btnCustomBackColor
            // 
            this.btnCustomBackColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCustomBackColor.Location = new System.Drawing.Point(183, 137);
            this.btnCustomBackColor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCustomBackColor.Name = "btnCustomBackColor";
            this.btnCustomBackColor.Size = new System.Drawing.Size(78, 35);
            this.btnCustomBackColor.TabIndex = 15;
            this.btnCustomBackColor.Text = "Custom";
            this.toolTip.SetToolTip(this.btnCustomBackColor, "Pick a custom background color");
            this.btnCustomBackColor.UseVisualStyleBackColor = true;
            this.btnCustomBackColor.Click += new System.EventHandler(this.OnBtnCustomBackColorClick);
            // 
            // checkBoxRegex
            // 
            this.checkBoxRegex.AutoSize = true;
            this.checkBoxRegex.Location = new System.Drawing.Point(180, 95);
            this.checkBoxRegex.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxRegex.Name = "checkBoxRegex";
            this.checkBoxRegex.Size = new System.Drawing.Size(58, 17);
            this.checkBoxRegex.TabIndex = 16;
            this.checkBoxRegex.Text = "RegEx";
            this.toolTip.SetToolTip(this.checkBoxRegex, "Whether the string is a regular expresion");
            this.checkBoxRegex.UseVisualStyleBackColor = true;
            this.checkBoxRegex.CheckedChanged += new System.EventHandler(this.ChangeToDirty);
            this.checkBoxRegex.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnChkBoxRegexMouseUp);
            // 
            // checkBoxCaseSensitive
            // 
            this.checkBoxCaseSensitive.AutoSize = true;
            this.checkBoxCaseSensitive.Location = new System.Drawing.Point(14, 95);
            this.checkBoxCaseSensitive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxCaseSensitive.Name = "checkBoxCaseSensitive";
            this.checkBoxCaseSensitive.Size = new System.Drawing.Size(94, 17);
            this.checkBoxCaseSensitive.TabIndex = 17;
            this.checkBoxCaseSensitive.Text = "Case sensitive";
            this.toolTip.SetToolTip(this.checkBoxCaseSensitive, "Whether the string will match uppercases and lowercases");
            this.checkBoxCaseSensitive.UseVisualStyleBackColor = true;
            this.checkBoxCaseSensitive.CheckedChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // checkBoxDontDirtyLed
            // 
            this.checkBoxDontDirtyLed.AutoSize = true;
            this.checkBoxDontDirtyLed.Location = new System.Drawing.Point(15, 38);
            this.checkBoxDontDirtyLed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxDontDirtyLed.Name = "checkBoxDontDirtyLed";
            this.checkBoxDontDirtyLed.Size = new System.Drawing.Size(107, 17);
            this.checkBoxDontDirtyLed.TabIndex = 18;
            this.checkBoxDontDirtyLed.Text = "Don\'t lit dirty LED";
            this.toolTip.SetToolTip(this.checkBoxDontDirtyLed, "When matching a line, don\'t mark the page as \"dirty\"");
            this.checkBoxDontDirtyLed.UseVisualStyleBackColor = true;
            this.checkBoxDontDirtyLed.CheckedChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // groupBoxLineMatchCriteria
            // 
            this.groupBoxLineMatchCriteria.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLineMatchCriteria.Controls.Add(this.textBoxSearchString);
            this.groupBoxLineMatchCriteria.Controls.Add(this.labelSearchString);
            this.groupBoxLineMatchCriteria.Controls.Add(this.checkBoxRegex);
            this.groupBoxLineMatchCriteria.Controls.Add(this.checkBoxCaseSensitive);
            this.groupBoxLineMatchCriteria.Location = new System.Drawing.Point(18, 437);
            this.groupBoxLineMatchCriteria.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxLineMatchCriteria.Name = "groupBoxLineMatchCriteria";
            this.groupBoxLineMatchCriteria.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxLineMatchCriteria.Size = new System.Drawing.Size(607, 135);
            this.groupBoxLineMatchCriteria.TabIndex = 19;
            this.groupBoxLineMatchCriteria.TabStop = false;
            this.groupBoxLineMatchCriteria.Text = "Line match criteria";
            // 
            // groupBoxColoring
            // 
            this.groupBoxColoring.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxColoring.Controls.Add(this.checkBoxNoBackground);
            this.groupBoxColoring.Controls.Add(this.checkBoxBold);
            this.groupBoxColoring.Controls.Add(this.checkBoxWordMatch);
            this.groupBoxColoring.Controls.Add(this.labelForgroundColor);
            this.groupBoxColoring.Controls.Add(this.colorBoxForeground);
            this.groupBoxColoring.Controls.Add(this.btnCustomForeColor);
            this.groupBoxColoring.Controls.Add(this.btnCustomBackColor);
            this.groupBoxColoring.Controls.Add(this.labelBackgroundColor);
            this.groupBoxColoring.Controls.Add(this.colorBoxBackground);
            this.groupBoxColoring.Location = new System.Drawing.Point(18, 581);
            this.groupBoxColoring.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxColoring.Name = "groupBoxColoring";
            this.groupBoxColoring.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxColoring.Size = new System.Drawing.Size(285, 286);
            this.groupBoxColoring.TabIndex = 20;
            this.groupBoxColoring.TabStop = false;
            this.groupBoxColoring.Text = "Coloring";
            // 
            // checkBoxNoBackground
            // 
            this.checkBoxNoBackground.AutoSize = true;
            this.checkBoxNoBackground.Enabled = false;
            this.checkBoxNoBackground.Location = new System.Drawing.Point(141, 240);
            this.checkBoxNoBackground.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxNoBackground.Name = "checkBoxNoBackground";
            this.checkBoxNoBackground.Size = new System.Drawing.Size(101, 17);
            this.checkBoxNoBackground.TabIndex = 18;
            this.checkBoxNoBackground.Text = "No Background";
            this.toolTip.SetToolTip(this.checkBoxNoBackground, "Don\'t set the background color");
            this.checkBoxNoBackground.UseVisualStyleBackColor = true;
            this.checkBoxNoBackground.CheckedChanged += new System.EventHandler(this.OnChkBoxNoBackgroundCheckedChanged);
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Location = new System.Drawing.Point(9, 205);
            this.checkBoxBold.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(47, 17);
            this.checkBoxBold.TabIndex = 17;
            this.checkBoxBold.Text = "Bold";
            this.toolTip.SetToolTip(this.checkBoxBold, "Display the line in bold characters");
            this.checkBoxBold.UseVisualStyleBackColor = true;
            this.checkBoxBold.CheckedChanged += new System.EventHandler(this.OnChkBoxBoldCheckedChanged);
            // 
            // checkBoxWordMatch
            // 
            this.checkBoxWordMatch.AutoSize = true;
            this.checkBoxWordMatch.Location = new System.Drawing.Point(9, 240);
            this.checkBoxWordMatch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxWordMatch.Name = "checkBoxWordMatch";
            this.checkBoxWordMatch.Size = new System.Drawing.Size(81, 17);
            this.checkBoxWordMatch.TabIndex = 16;
            this.checkBoxWordMatch.Text = "Word mode";
            this.toolTip.SetToolTip(this.checkBoxWordMatch, "Don\'t highlight the whole line but only the matching keywords");
            this.checkBoxWordMatch.UseVisualStyleBackColor = true;
            this.checkBoxWordMatch.CheckedChanged += new System.EventHandler(this.OnChkBoxWordMatchCheckedChanged);
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxActions.Controls.Add(this.btnBookmarkComment);
            this.groupBoxActions.Controls.Add(this.btnSelectPlugin);
            this.groupBoxActions.Controls.Add(this.checkBoxPlugin);
            this.groupBoxActions.Controls.Add(this.checkBoxStopTail);
            this.groupBoxActions.Controls.Add(this.checkBoxBookmark);
            this.groupBoxActions.Controls.Add(this.checkBoxDontDirtyLed);
            this.groupBoxActions.Location = new System.Drawing.Point(313, 581);
            this.groupBoxActions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxActions.Size = new System.Drawing.Size(312, 195);
            this.groupBoxActions.TabIndex = 21;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "Actions";
            // 
            // btnBookmarkComment
            // 
            this.btnBookmarkComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBookmarkComment.Location = new System.Drawing.Point(210, 69);
            this.btnBookmarkComment.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnBookmarkComment.Name = "btnBookmarkComment";
            this.btnBookmarkComment.Size = new System.Drawing.Size(81, 31);
            this.btnBookmarkComment.TabIndex = 23;
            this.btnBookmarkComment.Text = "Text...";
            this.btnBookmarkComment.UseVisualStyleBackColor = true;
            this.btnBookmarkComment.Click += new System.EventHandler(this.OnBtnBookmarkCommentClick);
            // 
            // btnSelectPlugin
            // 
            this.btnSelectPlugin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectPlugin.Location = new System.Drawing.Point(210, 143);
            this.btnSelectPlugin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSelectPlugin.Name = "btnSelectPlugin";
            this.btnSelectPlugin.Size = new System.Drawing.Size(81, 31);
            this.btnSelectPlugin.TabIndex = 22;
            this.btnSelectPlugin.Text = "Select...";
            this.btnSelectPlugin.UseVisualStyleBackColor = true;
            this.btnSelectPlugin.Click += new System.EventHandler(this.OnPluginButtonClick);
            // 
            // checkBoxPlugin
            // 
            this.checkBoxPlugin.AutoSize = true;
            this.checkBoxPlugin.Location = new System.Drawing.Point(15, 148);
            this.checkBoxPlugin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxPlugin.Name = "checkBoxPlugin";
            this.checkBoxPlugin.Size = new System.Drawing.Size(55, 17);
            this.checkBoxPlugin.TabIndex = 21;
            this.checkBoxPlugin.Text = "Plugin";
            this.toolTip.SetToolTip(this.checkBoxPlugin, "When matching a line, call a keyword action plugin");
            this.checkBoxPlugin.UseVisualStyleBackColor = true;
            this.checkBoxPlugin.CheckedChanged += new System.EventHandler(this.OnChkBoxPluginCheckedChanged);
            // 
            // checkBoxStopTail
            // 
            this.checkBoxStopTail.AutoSize = true;
            this.checkBoxStopTail.Location = new System.Drawing.Point(15, 111);
            this.checkBoxStopTail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxStopTail.Name = "checkBoxStopTail";
            this.checkBoxStopTail.Size = new System.Drawing.Size(101, 17);
            this.checkBoxStopTail.TabIndex = 20;
            this.checkBoxStopTail.Text = "Stop Follow Tail";
            this.toolTip.SetToolTip(this.checkBoxStopTail, "When matching a line, stop automatic scrolling");
            this.checkBoxStopTail.UseVisualStyleBackColor = true;
            this.checkBoxStopTail.CheckedChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // checkBoxBookmark
            // 
            this.checkBoxBookmark.AutoSize = true;
            this.checkBoxBookmark.Location = new System.Drawing.Point(15, 74);
            this.checkBoxBookmark.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxBookmark.Name = "checkBoxBookmark";
            this.checkBoxBookmark.Size = new System.Drawing.Size(92, 17);
            this.checkBoxBookmark.TabIndex = 19;
            this.checkBoxBookmark.Text = "Set bookmark";
            this.toolTip.SetToolTip(this.checkBoxBookmark, "When matching a line, create a new bookmark for it");
            this.checkBoxBookmark.UseVisualStyleBackColor = true;
            this.checkBoxBookmark.CheckedChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "LogExpert.chm";
            this.helpProvider.Tag = "";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.btnImportGroup);
            this.groupBox4.Controls.Add(this.btnExportGroup);
            this.groupBox4.Controls.Add(this.btnMoveGroupDown);
            this.groupBox4.Controls.Add(this.btnMoveGroupUp);
            this.groupBox4.Controls.Add(this.labelAssignNamesToGroups);
            this.groupBox4.Controls.Add(this.btnCopyGroup);
            this.groupBox4.Controls.Add(this.btnDeleteGroup);
            this.groupBox4.Controls.Add(this.btnNewGroup);
            this.groupBox4.Controls.Add(this.comboBoxGroups);
            this.groupBox4.Location = new System.Drawing.Point(18, 5);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Size = new System.Drawing.Size(607, 129);
            this.groupBox4.TabIndex = 22;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Groups";
            // 
            // btnImportGroup
            // 
            this.btnImportGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImportGroup.Location = new System.Drawing.Point(305, 29);
            this.btnImportGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnImportGroup.Name = "btnImportGroup";
            this.btnImportGroup.Size = new System.Drawing.Size(68, 35);
            this.btnImportGroup.TabIndex = 7;
            this.btnImportGroup.Text = "Import";
            this.toolTip.SetToolTip(this.btnImportGroup, "Import highlight groups");
            this.btnImportGroup.UseVisualStyleBackColor = true;
            this.btnImportGroup.Click += new System.EventHandler(this.OnBtnImportGroupClick);
            // 
            // btnExportGroup
            // 
            this.btnExportGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportGroup.Location = new System.Drawing.Point(305, 75);
            this.btnExportGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnExportGroup.Name = "btnExportGroup";
            this.btnExportGroup.Size = new System.Drawing.Size(68, 35);
            this.btnExportGroup.TabIndex = 8;
            this.btnExportGroup.Text = "Export";
            this.toolTip.SetToolTip(this.btnExportGroup, "Export highlight groups");
            this.btnExportGroup.UseVisualStyleBackColor = true;
            this.btnExportGroup.Click += new System.EventHandler(this.OnBtnExportGroupClick);
            // 
            // btnMoveGroupDown
            // 
            this.btnMoveGroupDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveGroupDown.Location = new System.Drawing.Point(518, 75);
            this.btnMoveGroupDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMoveGroupDown.Name = "btnMoveGroupDown";
            this.btnMoveGroupDown.Size = new System.Drawing.Size(68, 35);
            this.btnMoveGroupDown.TabIndex = 6;
            this.btnMoveGroupDown.Text = "Down";
            this.toolTip.SetToolTip(this.btnMoveGroupDown, "Move the current hilight group one position down");
            this.btnMoveGroupDown.UseVisualStyleBackColor = true;
            this.btnMoveGroupDown.Click += new System.EventHandler(this.OnBtnGroupDownClick);
            // 
            // btnMoveGroupUp
            // 
            this.btnMoveGroupUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveGroupUp.Location = new System.Drawing.Point(447, 75);
            this.btnMoveGroupUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMoveGroupUp.Name = "btnMoveGroupUp";
            this.btnMoveGroupUp.Size = new System.Drawing.Size(68, 35);
            this.btnMoveGroupUp.TabIndex = 5;
            this.btnMoveGroupUp.Text = "Up";
            this.toolTip.SetToolTip(this.btnMoveGroupUp, "Move the current hilight group one position up");
            this.btnMoveGroupUp.UseVisualStyleBackColor = true;
            this.btnMoveGroupUp.Click += new System.EventHandler(this.OnBtnGroupUpClick);
            // 
            // labelAssignNamesToGroups
            // 
            this.labelAssignNamesToGroups.AutoSize = true;
            this.labelAssignNamesToGroups.Location = new System.Drawing.Point(9, 88);
            this.labelAssignNamesToGroups.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAssignNamesToGroups.Name = "labelAssignNamesToGroups";
            this.labelAssignNamesToGroups.Size = new System.Drawing.Size(248, 13);
            this.labelAssignNamesToGroups.TabIndex = 4;
            this.labelAssignNamesToGroups.Text = "You can assign groups to file names in the settings.";
            // 
            // btnCopyGroup
            // 
            this.btnCopyGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyGroup.Location = new System.Drawing.Point(518, 29);
            this.btnCopyGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCopyGroup.Name = "btnCopyGroup";
            this.btnCopyGroup.Size = new System.Drawing.Size(68, 35);
            this.btnCopyGroup.TabIndex = 3;
            this.btnCopyGroup.Text = "Copy";
            this.toolTip.SetToolTip(this.btnCopyGroup, "Copy the current hilight group into a new one");
            this.btnCopyGroup.UseVisualStyleBackColor = true;
            this.btnCopyGroup.Click += new System.EventHandler(this.OnBtnCopyGroupClick);
            // 
            // btnDeleteGroup
            // 
            this.btnDeleteGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteGroup.Location = new System.Drawing.Point(447, 29);
            this.btnDeleteGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDeleteGroup.Name = "btnDeleteGroup";
            this.btnDeleteGroup.Size = new System.Drawing.Size(68, 35);
            this.btnDeleteGroup.TabIndex = 2;
            this.btnDeleteGroup.Text = "Del";
            this.toolTip.SetToolTip(this.btnDeleteGroup, "Delete the current hilight group");
            this.btnDeleteGroup.UseVisualStyleBackColor = true;
            this.btnDeleteGroup.Click += new System.EventHandler(this.OnBtnDelGroupClick);
            // 
            // btnNewGroup
            // 
            this.btnNewGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewGroup.Location = new System.Drawing.Point(381, 29);
            this.btnNewGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnNewGroup.Name = "btnNewGroup";
            this.btnNewGroup.Size = new System.Drawing.Size(63, 35);
            this.btnNewGroup.TabIndex = 1;
            this.btnNewGroup.Text = "New group";
            this.toolTip.SetToolTip(this.btnNewGroup, "Create a new empty hilight group");
            this.btnNewGroup.UseVisualStyleBackColor = true;
            this.btnNewGroup.Click += new System.EventHandler(this.OnBtnNewGroupClick);
            // 
            // comboBoxGroups
            // 
            this.comboBoxGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxGroups.DisplayMember = "GroupName";
            this.comboBoxGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxGroups.Location = new System.Drawing.Point(14, 32);
            this.comboBoxGroups.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBoxGroups.Name = "comboBoxGroups";
            this.comboBoxGroups.Size = new System.Drawing.Size(270, 21);
            this.comboBoxGroups.TabIndex = 0;
            this.toolTip.SetToolTip(this.comboBoxGroups, "Choose a group to create different highlight settings. Type in a name to change i" +
        "n the name of a group.");
            this.comboBoxGroups.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.OnCmbBoxGroupDrawItem);
            this.comboBoxGroups.SelectionChangeCommitted += new System.EventHandler(this.OnCmbBoxGroupSelectionChangeCommitted);
            this.comboBoxGroups.TextUpdate += new System.EventHandler(this.OnCmbBoxGroupTextUpdate);
            // 
            // pnlBackground
            // 
            this.pnlBackground.AutoScroll = true;
            this.pnlBackground.Controls.Add(this.listBoxHighlight);
            this.pnlBackground.Controls.Add(this.btnAdd);
            this.pnlBackground.Controls.Add(this.btnDelete);
            this.pnlBackground.Controls.Add(this.btnMoveUp);
            this.pnlBackground.Controls.Add(this.btnMoveDown);
            this.pnlBackground.Controls.Add(this.btnOk);
            this.pnlBackground.Controls.Add(this.btnCancel);
            this.pnlBackground.Controls.Add(this.btnApply);
            this.pnlBackground.Controls.Add(this.groupBoxLineMatchCriteria);
            this.pnlBackground.Controls.Add(this.groupBoxColoring);
            this.pnlBackground.Controls.Add(this.groupBoxActions);
            this.pnlBackground.Controls.Add(this.groupBox4);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(644, 895);
            this.pnlBackground.TabIndex = 23;
            // 
            // colorBoxForeground
            // 
            this.colorBoxForeground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorBoxForeground.CustomColor = System.Drawing.Color.Black;
            this.colorBoxForeground.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorBoxForeground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorBoxForeground.FormattingEnabled = true;
            this.colorBoxForeground.Items.AddRange(new object[] {
            System.Drawing.Color.Black,
            System.Drawing.Color.Black,
            System.Drawing.Color.White,
            System.Drawing.Color.Gray,
            System.Drawing.Color.DarkGray,
            System.Drawing.Color.Blue,
            System.Drawing.Color.LightBlue,
            System.Drawing.Color.DarkBlue,
            System.Drawing.Color.Green,
            System.Drawing.Color.LightGreen,
            System.Drawing.Color.DarkGreen,
            System.Drawing.Color.Olive,
            System.Drawing.Color.Red,
            System.Drawing.Color.Pink,
            System.Drawing.Color.Purple,
            System.Drawing.Color.IndianRed,
            System.Drawing.Color.DarkCyan,
            System.Drawing.Color.Yellow,
            System.Drawing.Color.Black,
            System.Drawing.Color.Black,
            System.Drawing.Color.White,
            System.Drawing.Color.Gray,
            System.Drawing.Color.DarkGray,
            System.Drawing.Color.Blue,
            System.Drawing.Color.LightBlue,
            System.Drawing.Color.DarkBlue,
            System.Drawing.Color.Green,
            System.Drawing.Color.LightGreen,
            System.Drawing.Color.DarkGreen,
            System.Drawing.Color.Olive,
            System.Drawing.Color.Red,
            System.Drawing.Color.Pink,
            System.Drawing.Color.Purple,
            System.Drawing.Color.IndianRed,
            System.Drawing.Color.DarkCyan,
            System.Drawing.Color.Yellow,
            System.Drawing.Color.Black,
            System.Drawing.Color.Black,
            System.Drawing.Color.White,
            System.Drawing.Color.Gray,
            System.Drawing.Color.DarkGray,
            System.Drawing.Color.Blue,
            System.Drawing.Color.LightBlue,
            System.Drawing.Color.DarkBlue,
            System.Drawing.Color.Green,
            System.Drawing.Color.LightGreen,
            System.Drawing.Color.DarkGreen,
            System.Drawing.Color.Olive,
            System.Drawing.Color.Red,
            System.Drawing.Color.Pink,
            System.Drawing.Color.Purple,
            System.Drawing.Color.IndianRed,
            System.Drawing.Color.DarkCyan,
            System.Drawing.Color.Yellow});
            this.colorBoxForeground.Location = new System.Drawing.Point(8, 63);
            this.colorBoxForeground.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.colorBoxForeground.Name = "colorBoxForeground";
            this.colorBoxForeground.Size = new System.Drawing.Size(165, 21);
            this.colorBoxForeground.TabIndex = 5;
            this.colorBoxForeground.SelectedIndexChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // colorBoxBackground
            // 
            this.colorBoxBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorBoxBackground.CustomColor = System.Drawing.Color.Black;
            this.colorBoxBackground.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorBoxBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorBoxBackground.FormattingEnabled = true;
            this.colorBoxBackground.Items.AddRange(new object[] {
            System.Drawing.Color.Black,
            System.Drawing.Color.Black,
            System.Drawing.Color.White,
            System.Drawing.Color.Gray,
            System.Drawing.Color.DarkGray,
            System.Drawing.Color.Blue,
            System.Drawing.Color.LightBlue,
            System.Drawing.Color.DarkBlue,
            System.Drawing.Color.Green,
            System.Drawing.Color.LightGreen,
            System.Drawing.Color.DarkGreen,
            System.Drawing.Color.Olive,
            System.Drawing.Color.Red,
            System.Drawing.Color.Pink,
            System.Drawing.Color.Purple,
            System.Drawing.Color.IndianRed,
            System.Drawing.Color.DarkCyan,
            System.Drawing.Color.Yellow,
            System.Drawing.Color.Black,
            System.Drawing.Color.Black,
            System.Drawing.Color.White,
            System.Drawing.Color.Gray,
            System.Drawing.Color.DarkGray,
            System.Drawing.Color.Blue,
            System.Drawing.Color.LightBlue,
            System.Drawing.Color.DarkBlue,
            System.Drawing.Color.Green,
            System.Drawing.Color.LightGreen,
            System.Drawing.Color.DarkGreen,
            System.Drawing.Color.Olive,
            System.Drawing.Color.Red,
            System.Drawing.Color.Pink,
            System.Drawing.Color.Purple,
            System.Drawing.Color.IndianRed,
            System.Drawing.Color.DarkCyan,
            System.Drawing.Color.Yellow,
            System.Drawing.Color.Black,
            System.Drawing.Color.Black,
            System.Drawing.Color.White,
            System.Drawing.Color.Gray,
            System.Drawing.Color.DarkGray,
            System.Drawing.Color.Blue,
            System.Drawing.Color.LightBlue,
            System.Drawing.Color.DarkBlue,
            System.Drawing.Color.Green,
            System.Drawing.Color.LightGreen,
            System.Drawing.Color.DarkGreen,
            System.Drawing.Color.Olive,
            System.Drawing.Color.Red,
            System.Drawing.Color.Pink,
            System.Drawing.Color.Purple,
            System.Drawing.Color.IndianRed,
            System.Drawing.Color.DarkCyan,
            System.Drawing.Color.Yellow});
            this.colorBoxBackground.Location = new System.Drawing.Point(9, 140);
            this.colorBoxBackground.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.colorBoxBackground.Name = "colorBoxBackground";
            this.colorBoxBackground.Size = new System.Drawing.Size(165, 21);
            this.colorBoxBackground.TabIndex = 7;
            this.colorBoxBackground.SelectedIndexChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // HighlightDialog
            // 
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(644, 895);
            this.Controls.Add(this.pnlBackground);
            this.DoubleBuffered = true;
            this.helpProvider.SetHelpKeyword(this, "Highlighting.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(660, 934);
            this.Name = "HighlightDialog";
            this.helpProvider.SetShowHelp(this, true);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Highlighting and action triggers";
            this.Shown += new System.EventHandler(this.OnHighlightDialogShown);
            this.groupBoxLineMatchCriteria.ResumeLayout(false);
            this.groupBoxLineMatchCriteria.PerformLayout();
            this.groupBoxColoring.ResumeLayout(false);
            this.groupBoxColoring.PerformLayout();
            this.groupBoxActions.ResumeLayout(false);
            this.groupBoxActions.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.pnlBackground.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxHighlight;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private ColorComboBox colorBoxForeground;
        private System.Windows.Forms.Label labelForgroundColor;
        private ColorComboBox colorBoxBackground;
        private System.Windows.Forms.Label labelBackgroundColor;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox textBoxSearchString;
        private System.Windows.Forms.Label labelSearchString;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCustomForeColor;
        private System.Windows.Forms.Button btnCustomBackColor;
        private System.Windows.Forms.CheckBox checkBoxRegex;
        private System.Windows.Forms.CheckBox checkBoxCaseSensitive;
        private System.Windows.Forms.CheckBox checkBoxDontDirtyLed;
        private System.Windows.Forms.GroupBox groupBoxLineMatchCriteria;
        private System.Windows.Forms.GroupBox groupBoxColoring;
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.CheckBox checkBoxBookmark;
        private System.Windows.Forms.CheckBox checkBoxStopTail;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.CheckBox checkBoxPlugin;
        private System.Windows.Forms.Button btnSelectPlugin;
        private System.Windows.Forms.Button btnBookmarkComment;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox comboBoxGroups;
        private System.Windows.Forms.Button btnDeleteGroup;
        private System.Windows.Forms.Button btnNewGroup;
        private System.Windows.Forms.Button btnCopyGroup;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label labelAssignNamesToGroups;
        private System.Windows.Forms.Button btnMoveGroupUp;
        private System.Windows.Forms.Button btnMoveGroupDown;
        private System.Windows.Forms.CheckBox checkBoxWordMatch;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.CheckBox checkBoxNoBackground;
        private System.Windows.Forms.Button btnImportGroup;
        private System.Windows.Forms.Button btnExportGroup;
        private System.Windows.Forms.Panel pnlBackground;
    }
}