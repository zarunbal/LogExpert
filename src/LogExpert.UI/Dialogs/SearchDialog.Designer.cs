namespace LogExpert.Dialogs;

partial class SearchDialog
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
    private void InitializeComponent ()
    {
        buttonOk = new Button();
        buttonCancel = new Button();
        labelSearchFor = new Label();
        checkBoxCaseSensitive = new CheckBox();
        checkBoxRegex = new CheckBox();
        buttonRegexHelper = new Button();
        radioButtonFromTop = new RadioButton();
        radioButtonFromSelected = new RadioButton();
        groupBoxSearchStart = new GroupBox();
        groupBoxOptions = new GroupBox();
        groupBoxDirection = new GroupBox();
        radioButtonBackward = new RadioButton();
        radioButtonForward = new RadioButton();
        comboBoxSearchFor = new ComboBox();
        helpProvider1 = new HelpProvider();
        groupBoxSearchStart.SuspendLayout();
        groupBoxOptions.SuspendLayout();
        groupBoxDirection.SuspendLayout();
        SuspendLayout();
        // 
        // buttonOk
        // 
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonOk.DialogResult = DialogResult.OK;
        buttonOk.Location = new Point(317, 215);
        buttonOk.Name = "buttonOk";
        buttonOk.Size = new Size(75, 23);
        buttonOk.TabIndex = 5;
        buttonOk.Text = "OK";
        buttonOk.UseVisualStyleBackColor = true;
        buttonOk.Click += OnButtonOkClick;
        // 
        // buttonCancel
        // 
        buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonCancel.DialogResult = DialogResult.Cancel;
        buttonCancel.Location = new Point(398, 215);
        buttonCancel.Name = "buttonCancel";
        buttonCancel.Size = new Size(75, 23);
        buttonCancel.TabIndex = 6;
        buttonCancel.Text = "Cancel";
        buttonCancel.UseVisualStyleBackColor = true;
        buttonCancel.Click += OnButtonCancelClick;
        // 
        // labelSearchFor
        // 
        labelSearchFor.AutoSize = true;
        labelSearchFor.Location = new Point(10, 9);
        labelSearchFor.Name = "labelSearchFor";
        labelSearchFor.Size = new Size(63, 15);
        labelSearchFor.TabIndex = 0;
        labelSearchFor.Text = "&Search for:";
        // 
        // checkBoxCaseSensitive
        // 
        checkBoxCaseSensitive.AutoSize = true;
        checkBoxCaseSensitive.Location = new Point(6, 19);
        checkBoxCaseSensitive.Name = "checkBoxCaseSensitive";
        checkBoxCaseSensitive.Size = new Size(99, 19);
        checkBoxCaseSensitive.TabIndex = 4;
        checkBoxCaseSensitive.Text = "&Case sensitive";
        checkBoxCaseSensitive.UseVisualStyleBackColor = true;
        // 
        // checkBoxRegex
        // 
        checkBoxRegex.AutoSize = true;
        checkBoxRegex.Location = new Point(6, 43);
        checkBoxRegex.Name = "checkBoxRegex";
        checkBoxRegex.Size = new Size(125, 19);
        checkBoxRegex.TabIndex = 5;
        checkBoxRegex.Text = "&Regular expression";
        checkBoxRegex.UseVisualStyleBackColor = true;
        // 
        // buttonRegexHelper
        // 
        buttonRegexHelper.AutoSize = true;
        buttonRegexHelper.Location = new Point(6, 72);
        buttonRegexHelper.Name = "buttonRegexHelper";
        buttonRegexHelper.Size = new Size(89, 26);
        buttonRegexHelper.TabIndex = 3;
        buttonRegexHelper.Text = "Regex-&Helper";
        buttonRegexHelper.UseVisualStyleBackColor = true;
        buttonRegexHelper.Click += OnButtonRegexClick;
        // 
        // radioButtonFromTop
        // 
        radioButtonFromTop.AutoSize = true;
        radioButtonFromTop.Location = new Point(6, 19);
        radioButtonFromTop.Name = "radioButtonFromTop";
        radioButtonFromTop.Size = new Size(74, 19);
        radioButtonFromTop.TabIndex = 7;
        radioButtonFromTop.TabStop = true;
        radioButtonFromTop.Text = "From top";
        radioButtonFromTop.UseVisualStyleBackColor = true;
        // 
        // radioButtonFromSelected
        // 
        radioButtonFromSelected.AutoSize = true;
        radioButtonFromSelected.Location = new Point(6, 42);
        radioButtonFromSelected.Name = "radioButtonFromSelected";
        radioButtonFromSelected.Size = new Size(121, 19);
        radioButtonFromSelected.TabIndex = 8;
        radioButtonFromSelected.TabStop = true;
        radioButtonFromSelected.Text = "From selected line";
        radioButtonFromSelected.UseVisualStyleBackColor = true;
        // 
        // groupBoxSearchStart
        // 
        groupBoxSearchStart.Controls.Add(radioButtonFromTop);
        groupBoxSearchStart.Controls.Add(radioButtonFromSelected);
        groupBoxSearchStart.Location = new Point(12, 74);
        groupBoxSearchStart.Name = "groupBoxSearchStart";
        groupBoxSearchStart.Size = new Size(179, 79);
        groupBoxSearchStart.TabIndex = 1;
        groupBoxSearchStart.TabStop = false;
        groupBoxSearchStart.Text = "Search start";
        // 
        // groupBoxOptions
        // 
        groupBoxOptions.Controls.Add(checkBoxCaseSensitive);
        groupBoxOptions.Controls.Add(checkBoxRegex);
        groupBoxOptions.Controls.Add(buttonRegexHelper);
        groupBoxOptions.Location = new Point(198, 74);
        groupBoxOptions.Name = "groupBoxOptions";
        groupBoxOptions.Size = new Size(275, 104);
        groupBoxOptions.TabIndex = 2;
        groupBoxOptions.TabStop = false;
        groupBoxOptions.Text = "Options";
        // 
        // groupBoxDirection
        // 
        groupBoxDirection.Controls.Add(radioButtonBackward);
        groupBoxDirection.Controls.Add(radioButtonForward);
        groupBoxDirection.Location = new Point(13, 159);
        groupBoxDirection.Name = "groupBoxDirection";
        groupBoxDirection.Size = new Size(122, 79);
        groupBoxDirection.TabIndex = 4;
        groupBoxDirection.TabStop = false;
        groupBoxDirection.Text = "Direction";
        // 
        // radioButtonBackward
        // 
        radioButtonBackward.AutoSize = true;
        radioButtonBackward.Location = new Point(7, 44);
        radioButtonBackward.Name = "radioButtonBackward";
        radioButtonBackward.Size = new Size(76, 19);
        radioButtonBackward.TabIndex = 1;
        radioButtonBackward.TabStop = true;
        radioButtonBackward.Text = "Backward";
        radioButtonBackward.UseVisualStyleBackColor = true;
        // 
        // radioButtonForward
        // 
        radioButtonForward.AutoSize = true;
        radioButtonForward.Location = new Point(7, 20);
        radioButtonForward.Name = "radioButtonForward";
        radioButtonForward.Size = new Size(68, 19);
        radioButtonForward.TabIndex = 0;
        radioButtonForward.TabStop = true;
        radioButtonForward.Text = "Forward";
        radioButtonForward.UseVisualStyleBackColor = true;
        // 
        // comboBoxSearchFor
        // 
        comboBoxSearchFor.FormattingEnabled = true;
        comboBoxSearchFor.Location = new Point(13, 34);
        comboBoxSearchFor.Name = "comboBoxSearchFor";
        comboBoxSearchFor.Size = new Size(460, 23);
        comboBoxSearchFor.TabIndex = 0;
        // 
        // helpProvider1
        // 
        helpProvider1.HelpNamespace = "LogExpert.chm";
        // 
        // SearchDialog
        // 
        AcceptButton = buttonOk;
        CancelButton = buttonCancel;
        ClientSize = new Size(488, 250);
        Controls.Add(comboBoxSearchFor);
        Controls.Add(groupBoxDirection);
        Controls.Add(groupBoxOptions);
        Controls.Add(groupBoxSearchStart);
        Controls.Add(labelSearchFor);
        Controls.Add(buttonCancel);
        Controls.Add(buttonOk);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        helpProvider1.SetHelpKeyword(this, "Search and Navigation.htm");
        helpProvider1.SetHelpNavigator(this, HelpNavigator.Topic);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SearchDialog";
        helpProvider1.SetShowHelp(this, true);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Search";
        groupBoxSearchStart.ResumeLayout(false);
        groupBoxSearchStart.PerformLayout();
        groupBoxOptions.ResumeLayout(false);
        groupBoxOptions.PerformLayout();
        groupBoxDirection.ResumeLayout(false);
        groupBoxDirection.PerformLayout();
        ResumeLayout(false);
        PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonOk;
private System.Windows.Forms.Button buttonCancel;
private System.Windows.Forms.Label labelSearchFor;
private System.Windows.Forms.CheckBox checkBoxCaseSensitive;
private System.Windows.Forms.CheckBox checkBoxRegex;
private System.Windows.Forms.Button buttonRegexHelper;
private System.Windows.Forms.RadioButton radioButtonFromTop;
private System.Windows.Forms.RadioButton radioButtonFromSelected;
private System.Windows.Forms.GroupBox groupBoxSearchStart;
private System.Windows.Forms.GroupBox groupBoxOptions;
private System.Windows.Forms.GroupBox groupBoxDirection;
private System.Windows.Forms.RadioButton radioButtonBackward;
private System.Windows.Forms.RadioButton radioButtonForward;
private System.Windows.Forms.ComboBox comboBoxSearchFor;
private System.Windows.Forms.HelpProvider helpProvider1;
}