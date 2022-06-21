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
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonMoveUp = new System.Windows.Forms.Button();
            this.buttonMoveDown = new System.Windows.Forms.Button();
            this.labelForgroundColor = new System.Windows.Forms.Label();
            this.labelBackgroundColor = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxSearchString = new System.Windows.Forms.TextBox();
            this.labelSearchString = new System.Windows.Forms.Label();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCustomForeColor = new System.Windows.Forms.Button();
            this.buttonCustomBackColor = new System.Windows.Forms.Button();
            this.checkBoxRegex = new System.Windows.Forms.CheckBox();
            this.checkBoxCaseSensitive = new System.Windows.Forms.CheckBox();
            this.checkBoxDontDirtyLed = new System.Windows.Forms.CheckBox();
            this.groupBoxLineMatchCriteria = new System.Windows.Forms.GroupBox();
            this.groupBoxColoring = new System.Windows.Forms.GroupBox();
            this.checkBoxNoBackground = new System.Windows.Forms.CheckBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.checkBoxWordMatch = new System.Windows.Forms.CheckBox();
            this.colorBoxForeground = new LogExpert.Dialogs.ColorComboBox();
            this.colorBoxBackground = new LogExpert.Dialogs.ColorComboBox();
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.buttonBookmarkComment = new System.Windows.Forms.Button();
            this.buttonPlugin = new System.Windows.Forms.Button();
            this.checkBoxPlugin = new System.Windows.Forms.CheckBox();
            this.checkBoxStopTail = new System.Windows.Forms.CheckBox();
            this.checkBoxBookmark = new System.Windows.Forms.CheckBox();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.buttonImportGroup = new System.Windows.Forms.Button();
            this.buttonMoveGroupDown = new System.Windows.Forms.Button();
            this.buttonMoveGroupUp = new System.Windows.Forms.Button();
            this.labelAssignNamesToGroups = new System.Windows.Forms.Label();
            this.buttonCopyGroup = new System.Windows.Forms.Button();
            this.buttonDeleteGroup = new System.Windows.Forms.Button();
            this.buttonNewGroup = new System.Windows.Forms.Button();
            this.comboBoxGroups = new System.Windows.Forms.ComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonExportGroup = new System.Windows.Forms.Button();
            this.groupBoxLineMatchCriteria.SuspendLayout();
            this.groupBoxColoring.SuspendLayout();
            this.groupBoxActions.SuspendLayout();
            this.groupBox4.SuspendLayout();
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
            this.listBoxHighlight.Size = new System.Drawing.Size(634, 264);
            this.listBoxHighlight.TabIndex = 0;
            this.listBoxHighlight.SelectedIndexChanged += new System.EventHandler(this.OnHighlightListBoxSelectedIndexChanged);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.Location = new System.Drawing.Point(676, 218);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(138, 35);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "&Add";
            this.toolTip.SetToolTip(this.buttonAdd, "Create a new hilight item from information below");
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.OnAddButtonClick);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.Location = new System.Drawing.Point(676, 263);
            this.buttonDelete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(138, 35);
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.Text = "D&elete";
            this.toolTip.SetToolTip(this.buttonDelete, "Delete the current hilight");
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.OnDeleteButtonClick);
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveUp.Location = new System.Drawing.Point(676, 160);
            this.buttonMoveUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonMoveUp.Name = "buttonMoveUp";
            this.buttonMoveUp.Size = new System.Drawing.Size(68, 35);
            this.buttonMoveUp.TabIndex = 3;
            this.buttonMoveUp.Text = "&Up";
            this.toolTip.SetToolTip(this.buttonMoveUp, "Move the current hilight one position up");
            this.buttonMoveUp.UseVisualStyleBackColor = true;
            this.buttonMoveUp.Click += new System.EventHandler(this.OnMoveUpButtonClick);
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveDown.Location = new System.Drawing.Point(747, 160);
            this.buttonMoveDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonMoveDown.Name = "buttonMoveDown";
            this.buttonMoveDown.Size = new System.Drawing.Size(68, 35);
            this.buttonMoveDown.TabIndex = 4;
            this.buttonMoveDown.Text = "&Down";
            this.toolTip.SetToolTip(this.buttonMoveDown, "Move the current hilight one position down");
            this.buttonMoveDown.UseVisualStyleBackColor = true;
            this.buttonMoveDown.Click += new System.EventHandler(this.OnMoveDownButtonClick);
            // 
            // labelForgroundColor
            // 
            this.labelForgroundColor.AutoSize = true;
            this.labelForgroundColor.Location = new System.Drawing.Point(9, 38);
            this.labelForgroundColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelForgroundColor.Name = "labelForgroundColor";
            this.labelForgroundColor.Size = new System.Drawing.Size(130, 20);
            this.labelForgroundColor.TabIndex = 6;
            this.labelForgroundColor.Text = "Foreground color";
            // 
            // labelBackgroundColor
            // 
            this.labelBackgroundColor.AutoSize = true;
            this.labelBackgroundColor.Location = new System.Drawing.Point(9, 115);
            this.labelBackgroundColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBackgroundColor.Name = "labelBackgroundColor";
            this.labelBackgroundColor.Size = new System.Drawing.Size(133, 20);
            this.labelBackgroundColor.TabIndex = 8;
            this.labelBackgroundColor.Text = "Background color";
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(596, 849);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(112, 35);
            this.buttonOk.TabIndex = 9;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.OnOkButtonClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(723, 849);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(112, 35);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxSearchString
            // 
            this.textBoxSearchString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearchString.Location = new System.Drawing.Point(9, 55);
            this.textBoxSearchString.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxSearchString.Name = "textBoxSearchString";
            this.textBoxSearchString.Size = new System.Drawing.Size(786, 26);
            this.textBoxSearchString.TabIndex = 11;
            this.textBoxSearchString.TextChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // labelSearchString
            // 
            this.labelSearchString.AutoSize = true;
            this.labelSearchString.Location = new System.Drawing.Point(9, 31);
            this.labelSearchString.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSearchString.Name = "labelSearchString";
            this.labelSearchString.Size = new System.Drawing.Size(107, 20);
            this.labelSearchString.TabIndex = 12;
            this.labelSearchString.Text = "Search string:";
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Enabled = false;
            this.buttonApply.Image = global::LogExpert.Properties.Resources.AdvancedIcon2;
            this.buttonApply.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.buttonApply.Location = new System.Drawing.Point(676, 308);
            this.buttonApply.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(138, 35);
            this.buttonApply.TabIndex = 13;
            this.buttonApply.Text = "A&pply";
            this.toolTip.SetToolTip(this.buttonApply, "Apply changes below to current hiligth");
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.OnApplyButtonClick);
            // 
            // buttonCustomForeColor
            // 
            this.buttonCustomForeColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCustomForeColor.Location = new System.Drawing.Point(394, 60);
            this.buttonCustomForeColor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonCustomForeColor.Name = "buttonCustomForeColor";
            this.buttonCustomForeColor.Size = new System.Drawing.Size(78, 35);
            this.buttonCustomForeColor.TabIndex = 14;
            this.buttonCustomForeColor.Text = "Custom";
            this.toolTip.SetToolTip(this.buttonCustomForeColor, "Pick a custom foreground color");
            this.buttonCustomForeColor.UseVisualStyleBackColor = true;
            this.buttonCustomForeColor.Click += new System.EventHandler(this.OnCustomForeColorButtonClick);
            // 
            // buttonCustomBackColor
            // 
            this.buttonCustomBackColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCustomBackColor.Location = new System.Drawing.Point(394, 137);
            this.buttonCustomBackColor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonCustomBackColor.Name = "buttonCustomBackColor";
            this.buttonCustomBackColor.Size = new System.Drawing.Size(78, 35);
            this.buttonCustomBackColor.TabIndex = 15;
            this.buttonCustomBackColor.Text = "Custom";
            this.toolTip.SetToolTip(this.buttonCustomBackColor, "Pick a custom background color");
            this.buttonCustomBackColor.UseVisualStyleBackColor = true;
            this.buttonCustomBackColor.Click += new System.EventHandler(this.OnCustomBackColorButtonClick);
            // 
            // checkBoxRegex
            // 
            this.checkBoxRegex.AutoSize = true;
            this.checkBoxRegex.Location = new System.Drawing.Point(180, 95);
            this.checkBoxRegex.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxRegex.Name = "checkBoxRegex";
            this.checkBoxRegex.Size = new System.Drawing.Size(83, 24);
            this.checkBoxRegex.TabIndex = 16;
            this.checkBoxRegex.Text = "RegEx";
            this.toolTip.SetToolTip(this.checkBoxRegex, "Whether the string is a regular expresion");
            this.checkBoxRegex.UseVisualStyleBackColor = true;
            this.checkBoxRegex.CheckedChanged += new System.EventHandler(this.ChangeToDirty);
            this.checkBoxRegex.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnRegexCheckBoxMouseUp);
            // 
            // checkBoxCaseSensitive
            // 
            this.checkBoxCaseSensitive.AutoSize = true;
            this.checkBoxCaseSensitive.Location = new System.Drawing.Point(14, 95);
            this.checkBoxCaseSensitive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxCaseSensitive.Name = "checkBoxCaseSensitive";
            this.checkBoxCaseSensitive.Size = new System.Drawing.Size(137, 24);
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
            this.checkBoxDontDirtyLed.Size = new System.Drawing.Size(157, 24);
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
            this.groupBoxLineMatchCriteria.Location = new System.Drawing.Point(18, 454);
            this.groupBoxLineMatchCriteria.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxLineMatchCriteria.Name = "groupBoxLineMatchCriteria";
            this.groupBoxLineMatchCriteria.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxLineMatchCriteria.Size = new System.Drawing.Size(818, 135);
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
            this.groupBoxColoring.Controls.Add(this.buttonCustomForeColor);
            this.groupBoxColoring.Controls.Add(this.buttonCustomBackColor);
            this.groupBoxColoring.Controls.Add(this.labelBackgroundColor);
            this.groupBoxColoring.Controls.Add(this.colorBoxBackground);
            this.groupBoxColoring.Location = new System.Drawing.Point(18, 598);
            this.groupBoxColoring.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxColoring.Name = "groupBoxColoring";
            this.groupBoxColoring.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxColoring.Size = new System.Drawing.Size(496, 286);
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
            this.checkBoxNoBackground.Size = new System.Drawing.Size(145, 24);
            this.checkBoxNoBackground.TabIndex = 18;
            this.checkBoxNoBackground.Text = "No Background";
            this.toolTip.SetToolTip(this.checkBoxNoBackground, "Don\'t set the background color");
            this.checkBoxNoBackground.UseVisualStyleBackColor = true;
            this.checkBoxNoBackground.CheckedChanged += new System.EventHandler(this.OnNoBackgroundCheckBoxCheckedChanged);
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Location = new System.Drawing.Point(9, 205);
            this.checkBoxBold.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(67, 24);
            this.checkBoxBold.TabIndex = 17;
            this.checkBoxBold.Text = "Bold";
            this.toolTip.SetToolTip(this.checkBoxBold, "Display the line in bold characters");
            this.checkBoxBold.UseVisualStyleBackColor = true;
            this.checkBoxBold.CheckedChanged += new System.EventHandler(this.OnBoldCheckBoxCheckedChanged);
            // 
            // checkBoxWordMatch
            // 
            this.checkBoxWordMatch.AutoSize = true;
            this.checkBoxWordMatch.Location = new System.Drawing.Point(9, 240);
            this.checkBoxWordMatch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxWordMatch.Name = "checkBoxWordMatch";
            this.checkBoxWordMatch.Size = new System.Drawing.Size(117, 24);
            this.checkBoxWordMatch.TabIndex = 16;
            this.checkBoxWordMatch.Text = "Word mode";
            this.toolTip.SetToolTip(this.checkBoxWordMatch, "Don\'t highlight the whole line but only the matching keywords");
            this.checkBoxWordMatch.UseVisualStyleBackColor = true;
            this.checkBoxWordMatch.CheckedChanged += new System.EventHandler(this.OnWordMatchCheckBoxCheckedChanged);
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
            System.Drawing.Color.Yellow});
            this.colorBoxForeground.Location = new System.Drawing.Point(8, 63);
            this.colorBoxForeground.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.colorBoxForeground.Name = "colorBoxForeground";
            this.colorBoxForeground.Size = new System.Drawing.Size(376, 27);
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
            System.Drawing.Color.Yellow});
            this.colorBoxBackground.Location = new System.Drawing.Point(9, 140);
            this.colorBoxBackground.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.colorBoxBackground.Name = "colorBoxBackground";
            this.colorBoxBackground.Size = new System.Drawing.Size(376, 27);
            this.colorBoxBackground.TabIndex = 7;
            this.colorBoxBackground.SelectedIndexChanged += new System.EventHandler(this.ChangeToDirty);
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxActions.Controls.Add(this.buttonBookmarkComment);
            this.groupBoxActions.Controls.Add(this.buttonPlugin);
            this.groupBoxActions.Controls.Add(this.checkBoxPlugin);
            this.groupBoxActions.Controls.Add(this.checkBoxStopTail);
            this.groupBoxActions.Controls.Add(this.checkBoxBookmark);
            this.groupBoxActions.Controls.Add(this.checkBoxDontDirtyLed);
            this.groupBoxActions.Location = new System.Drawing.Point(524, 598);
            this.groupBoxActions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxActions.Size = new System.Drawing.Size(312, 195);
            this.groupBoxActions.TabIndex = 21;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "Actions";
            // 
            // buttonBookmarkComment
            // 
            this.buttonBookmarkComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBookmarkComment.Location = new System.Drawing.Point(210, 69);
            this.buttonBookmarkComment.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonBookmarkComment.Name = "buttonBookmarkComment";
            this.buttonBookmarkComment.Size = new System.Drawing.Size(81, 31);
            this.buttonBookmarkComment.TabIndex = 23;
            this.buttonBookmarkComment.Text = "Text...";
            this.buttonBookmarkComment.UseVisualStyleBackColor = true;
            this.buttonBookmarkComment.Click += new System.EventHandler(this.OnBookmarkCommentButtonClick);
            // 
            // buttonPlugin
            // 
            this.buttonPlugin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlugin.Location = new System.Drawing.Point(210, 143);
            this.buttonPlugin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonPlugin.Name = "buttonPlugin";
            this.buttonPlugin.Size = new System.Drawing.Size(81, 31);
            this.buttonPlugin.TabIndex = 22;
            this.buttonPlugin.Text = "Select...";
            this.buttonPlugin.UseVisualStyleBackColor = true;
            this.buttonPlugin.Click += new System.EventHandler(this.OnPluginButtonClick);
            // 
            // checkBoxPlugin
            // 
            this.checkBoxPlugin.AutoSize = true;
            this.checkBoxPlugin.Location = new System.Drawing.Point(15, 148);
            this.checkBoxPlugin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxPlugin.Name = "checkBoxPlugin";
            this.checkBoxPlugin.Size = new System.Drawing.Size(78, 24);
            this.checkBoxPlugin.TabIndex = 21;
            this.checkBoxPlugin.Text = "Plugin";
            this.toolTip.SetToolTip(this.checkBoxPlugin, "When matching a line, call a keyword action plugin");
            this.checkBoxPlugin.UseVisualStyleBackColor = true;
            this.checkBoxPlugin.CheckedChanged += new System.EventHandler(this.OnPluginCheckBoxCheckedChanged);
            // 
            // checkBoxStopTail
            // 
            this.checkBoxStopTail.AutoSize = true;
            this.checkBoxStopTail.Location = new System.Drawing.Point(15, 111);
            this.checkBoxStopTail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxStopTail.Name = "checkBoxStopTail";
            this.checkBoxStopTail.Size = new System.Drawing.Size(146, 24);
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
            this.checkBoxBookmark.Size = new System.Drawing.Size(134, 24);
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
            this.groupBox4.Controls.Add(this.buttonImportGroup);
            this.groupBox4.Controls.Add(this.buttonExportGroup);
            this.groupBox4.Controls.Add(this.buttonMoveGroupDown);
            this.groupBox4.Controls.Add(this.buttonMoveGroupUp);
            this.groupBox4.Controls.Add(this.labelAssignNamesToGroups);
            this.groupBox4.Controls.Add(this.buttonCopyGroup);
            this.groupBox4.Controls.Add(this.buttonDeleteGroup);
            this.groupBox4.Controls.Add(this.buttonNewGroup);
            this.groupBox4.Controls.Add(this.comboBoxGroups);
            this.groupBox4.Location = new System.Drawing.Point(18, 5);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox4.Size = new System.Drawing.Size(818, 129);
            this.groupBox4.TabIndex = 22;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Groups";
            // 
            // buttonImportGroup
            // 
            this.buttonImportGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImportGroup.Location = new System.Drawing.Point(516, 29);
            this.buttonImportGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonImportGroup.Name = "buttonImportGroup";
            this.buttonImportGroup.Size = new System.Drawing.Size(68, 35);
            this.buttonImportGroup.TabIndex = 7;
            this.buttonImportGroup.Text = "Import";
            this.toolTip.SetToolTip(this.buttonImportGroup, "Import highlight groups");
            this.buttonImportGroup.UseVisualStyleBackColor = true;
            this.buttonImportGroup.Click += new System.EventHandler(this.OnImportGroupButtonClick);
            // 
            // buttonMoveGroupDown
            // 
            this.buttonMoveGroupDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveGroupDown.Location = new System.Drawing.Point(729, 75);
            this.buttonMoveGroupDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonMoveGroupDown.Name = "buttonMoveGroupDown";
            this.buttonMoveGroupDown.Size = new System.Drawing.Size(68, 35);
            this.buttonMoveGroupDown.TabIndex = 6;
            this.buttonMoveGroupDown.Text = "Down";
            this.toolTip.SetToolTip(this.buttonMoveGroupDown, "Move the current hilight group one position down");
            this.buttonMoveGroupDown.UseVisualStyleBackColor = true;
            this.buttonMoveGroupDown.Click += new System.EventHandler(this.OnGroupDownButtonClick);
            // 
            // buttonMoveGroupUp
            // 
            this.buttonMoveGroupUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveGroupUp.Location = new System.Drawing.Point(658, 75);
            this.buttonMoveGroupUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonMoveGroupUp.Name = "buttonMoveGroupUp";
            this.buttonMoveGroupUp.Size = new System.Drawing.Size(68, 35);
            this.buttonMoveGroupUp.TabIndex = 5;
            this.buttonMoveGroupUp.Text = "Up";
            this.toolTip.SetToolTip(this.buttonMoveGroupUp, "Move the current hilight group one position up");
            this.buttonMoveGroupUp.UseVisualStyleBackColor = true;
            this.buttonMoveGroupUp.Click += new System.EventHandler(this.OnGroupUpButtonClick);
            // 
            // labelAssignNamesToGroups
            // 
            this.labelAssignNamesToGroups.AutoSize = true;
            this.labelAssignNamesToGroups.Location = new System.Drawing.Point(9, 88);
            this.labelAssignNamesToGroups.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAssignNamesToGroups.Name = "labelAssignNamesToGroups";
            this.labelAssignNamesToGroups.Size = new System.Drawing.Size(372, 20);
            this.labelAssignNamesToGroups.TabIndex = 4;
            this.labelAssignNamesToGroups.Text = "You can assign groups to file names in the settings.";
            // 
            // buttonCopyGroup
            // 
            this.buttonCopyGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopyGroup.Location = new System.Drawing.Point(729, 29);
            this.buttonCopyGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonCopyGroup.Name = "buttonCopyGroup";
            this.buttonCopyGroup.Size = new System.Drawing.Size(68, 35);
            this.buttonCopyGroup.TabIndex = 3;
            this.buttonCopyGroup.Text = "Copy";
            this.toolTip.SetToolTip(this.buttonCopyGroup, "Copy the current hilight group into a new one");
            this.buttonCopyGroup.UseVisualStyleBackColor = true;
            this.buttonCopyGroup.Click += new System.EventHandler(this.OnCopyGroupButtonClick);
            // 
            // buttonDeleteGroup
            // 
            this.buttonDeleteGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeleteGroup.Location = new System.Drawing.Point(658, 29);
            this.buttonDeleteGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonDeleteGroup.Name = "buttonDeleteGroup";
            this.buttonDeleteGroup.Size = new System.Drawing.Size(68, 35);
            this.buttonDeleteGroup.TabIndex = 2;
            this.buttonDeleteGroup.Text = "Del";
            this.toolTip.SetToolTip(this.buttonDeleteGroup, "Delete the current hilight group");
            this.buttonDeleteGroup.UseVisualStyleBackColor = true;
            this.buttonDeleteGroup.Click += new System.EventHandler(this.OnDelGroupButtonClick);
            // 
            // buttonNewGroup
            // 
            this.buttonNewGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNewGroup.Location = new System.Drawing.Point(592, 29);
            this.buttonNewGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonNewGroup.Name = "buttonNewGroup";
            this.buttonNewGroup.Size = new System.Drawing.Size(63, 35);
            this.buttonNewGroup.TabIndex = 1;
            this.buttonNewGroup.Text = "New group";
            this.toolTip.SetToolTip(this.buttonNewGroup, "Create a new empty hilight group");
            this.buttonNewGroup.UseVisualStyleBackColor = true;
            this.buttonNewGroup.Click += new System.EventHandler(this.OnNewGroupButtonClick);
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
            this.comboBoxGroups.Size = new System.Drawing.Size(481, 27);
            this.comboBoxGroups.TabIndex = 0;
            this.toolTip.SetToolTip(this.comboBoxGroups, "Choose a group to create different highlight settings. Type in a name to change i" +
        "n the name of a group.");
            this.comboBoxGroups.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.OnGroupComboBoxDrawItem);
            this.comboBoxGroups.SelectionChangeCommitted += new System.EventHandler(this.OnGroupComboBoxSelectionChangeCommitted);
            this.comboBoxGroups.TextUpdate += new System.EventHandler(this.OnGroupComboBoxTextUpdate);
            // 
            // buttonExportGroup
            // 
            this.buttonExportGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExportGroup.Location = new System.Drawing.Point(516, 75);
            this.buttonExportGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonExportGroup.Name = "buttonExportGroup";
            this.buttonExportGroup.Size = new System.Drawing.Size(68, 35);
            this.buttonExportGroup.TabIndex = 8;
            this.buttonExportGroup.Text = "Export";
            this.toolTip.SetToolTip(this.buttonExportGroup, "Export highlight groups");
            this.buttonExportGroup.UseVisualStyleBackColor = true;
            this.buttonExportGroup.Click += new System.EventHandler(this.OnButtonExportGroupClick);
            // 
            // HighlightDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(855, 912);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBoxActions);
            this.Controls.Add(this.groupBoxColoring);
            this.Controls.Add(this.groupBoxLineMatchCriteria);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonMoveDown);
            this.Controls.Add(this.buttonMoveUp);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listBoxHighlight);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxHighlight;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonMoveUp;
        private System.Windows.Forms.Button buttonMoveDown;
        private ColorComboBox colorBoxForeground;
        private System.Windows.Forms.Label labelForgroundColor;
        private ColorComboBox colorBoxBackground;
        private System.Windows.Forms.Label labelBackgroundColor;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxSearchString;
        private System.Windows.Forms.Label labelSearchString;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCustomForeColor;
        private System.Windows.Forms.Button buttonCustomBackColor;
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
        private System.Windows.Forms.Button buttonPlugin;
        private System.Windows.Forms.Button buttonBookmarkComment;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox comboBoxGroups;
        private System.Windows.Forms.Button buttonDeleteGroup;
        private System.Windows.Forms.Button buttonNewGroup;
        private System.Windows.Forms.Button buttonCopyGroup;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label labelAssignNamesToGroups;
        private System.Windows.Forms.Button buttonMoveGroupUp;
        private System.Windows.Forms.Button buttonMoveGroupDown;
        private System.Windows.Forms.CheckBox checkBoxWordMatch;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.CheckBox checkBoxNoBackground;
        private System.Windows.Forms.Button buttonImportGroup;
        private System.Windows.Forms.Button buttonExportGroup;
    }
}