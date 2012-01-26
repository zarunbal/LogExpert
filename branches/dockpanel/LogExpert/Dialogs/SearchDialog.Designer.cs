namespace LogExpert.Dialogs
{
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
    private void InitializeComponent()
    {
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.caseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
      this.regexCheckBox = new System.Windows.Forms.CheckBox();
      this.regexHelperButton = new System.Windows.Forms.Button();
      this.fromTopRadioButton = new System.Windows.Forms.RadioButton();
      this.fromSelectedRadioButton = new System.Windows.Forms.RadioButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.backwardRadioButton = new System.Windows.Forms.RadioButton();
      this.forwardRadioButton = new System.Windows.Forms.RadioButton();
      this.searchComboBox = new System.Windows.Forms.ComboBox();
      this.helpProvider1 = new System.Windows.Forms.HelpProvider();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(221, 204);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 5;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(298, 204);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 6;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(59, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "&Search for:";
      // 
      // caseSensitiveCheckBox
      // 
      this.caseSensitiveCheckBox.AutoSize = true;
      this.caseSensitiveCheckBox.Location = new System.Drawing.Point(6, 19);
      this.caseSensitiveCheckBox.Name = "caseSensitiveCheckBox";
      this.caseSensitiveCheckBox.Size = new System.Drawing.Size(94, 17);
      this.caseSensitiveCheckBox.TabIndex = 4;
      this.caseSensitiveCheckBox.Text = "&Case sensitive";
      this.caseSensitiveCheckBox.UseVisualStyleBackColor = true;
      // 
      // regexCheckBox
      // 
      this.regexCheckBox.AutoSize = true;
      this.regexCheckBox.Location = new System.Drawing.Point(6, 43);
      this.regexCheckBox.Name = "regexCheckBox";
      this.regexCheckBox.Size = new System.Drawing.Size(116, 17);
      this.regexCheckBox.TabIndex = 5;
      this.regexCheckBox.Text = "&Regular expression";
      this.regexCheckBox.UseVisualStyleBackColor = true;
      // 
      // regexHelperButton
      // 
      this.regexHelperButton.Location = new System.Drawing.Point(124, 34);
      this.regexHelperButton.Name = "regexHelperButton";
      this.regexHelperButton.Size = new System.Drawing.Size(84, 26);
      this.regexHelperButton.TabIndex = 3;
      this.regexHelperButton.Text = "Regex-&Helper";
      this.regexHelperButton.UseVisualStyleBackColor = true;
      this.regexHelperButton.Click += new System.EventHandler(this.regexHelperButton_Click);
      // 
      // fromTopRadioButton
      // 
      this.fromTopRadioButton.AutoSize = true;
      this.fromTopRadioButton.Location = new System.Drawing.Point(6, 19);
      this.fromTopRadioButton.Name = "fromTopRadioButton";
      this.fromTopRadioButton.Size = new System.Drawing.Size(66, 17);
      this.fromTopRadioButton.TabIndex = 7;
      this.fromTopRadioButton.TabStop = true;
      this.fromTopRadioButton.Text = "From top";
      this.fromTopRadioButton.UseVisualStyleBackColor = true;
      // 
      // fromSelectedRadioButton
      // 
      this.fromSelectedRadioButton.AutoSize = true;
      this.fromSelectedRadioButton.Location = new System.Drawing.Point(6, 42);
      this.fromSelectedRadioButton.Name = "fromSelectedRadioButton";
      this.fromSelectedRadioButton.Size = new System.Drawing.Size(110, 17);
      this.fromSelectedRadioButton.TabIndex = 8;
      this.fromSelectedRadioButton.TabStop = true;
      this.fromSelectedRadioButton.Text = "From selected line";
      this.fromSelectedRadioButton.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.fromTopRadioButton);
      this.groupBox1.Controls.Add(this.fromSelectedRadioButton);
      this.groupBox1.Location = new System.Drawing.Point(13, 62);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(122, 79);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Search start";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.caseSensitiveCheckBox);
      this.groupBox2.Controls.Add(this.regexCheckBox);
      this.groupBox2.Controls.Add(this.regexHelperButton);
      this.groupBox2.Location = new System.Drawing.Point(158, 62);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(219, 79);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Options";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.backwardRadioButton);
      this.groupBox3.Controls.Add(this.forwardRadioButton);
      this.groupBox3.Location = new System.Drawing.Point(13, 148);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(122, 79);
      this.groupBox3.TabIndex = 4;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Direction";
      // 
      // backwardRadioButton
      // 
      this.backwardRadioButton.AutoSize = true;
      this.backwardRadioButton.Location = new System.Drawing.Point(7, 44);
      this.backwardRadioButton.Name = "backwardRadioButton";
      this.backwardRadioButton.Size = new System.Drawing.Size(73, 17);
      this.backwardRadioButton.TabIndex = 1;
      this.backwardRadioButton.TabStop = true;
      this.backwardRadioButton.Text = "Backward";
      this.backwardRadioButton.UseVisualStyleBackColor = true;
      // 
      // forwardRadioButton
      // 
      this.forwardRadioButton.AutoSize = true;
      this.forwardRadioButton.Location = new System.Drawing.Point(7, 20);
      this.forwardRadioButton.Name = "forwardRadioButton";
      this.forwardRadioButton.Size = new System.Drawing.Size(63, 17);
      this.forwardRadioButton.TabIndex = 0;
      this.forwardRadioButton.TabStop = true;
      this.forwardRadioButton.Text = "Forward";
      this.forwardRadioButton.UseVisualStyleBackColor = true;
      // 
      // searchComboBox
      // 
      this.searchComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.searchComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
      this.searchComboBox.FormattingEnabled = true;
      this.searchComboBox.Location = new System.Drawing.Point(13, 25);
      this.searchComboBox.Name = "searchComboBox";
      this.searchComboBox.Size = new System.Drawing.Size(364, 21);
      this.searchComboBox.TabIndex = 0;
      // 
      // helpProvider1
      // 
      this.helpProvider1.HelpNamespace = "LogExpert.chm";
      // 
      // SearchDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(385, 239);
      this.Controls.Add(this.searchComboBox);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.helpProvider1.SetHelpKeyword(this, "Search and Navigation.htm");
      this.helpProvider1.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SearchDialog";
      this.helpProvider1.SetShowHelp(this, true);
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Search";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox caseSensitiveCheckBox;
    private System.Windows.Forms.CheckBox regexCheckBox;
    private System.Windows.Forms.Button regexHelperButton;
    private System.Windows.Forms.RadioButton fromTopRadioButton;
    private System.Windows.Forms.RadioButton fromSelectedRadioButton;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.RadioButton backwardRadioButton;
    private System.Windows.Forms.RadioButton forwardRadioButton;
    private System.Windows.Forms.ComboBox searchComboBox;
    private System.Windows.Forms.HelpProvider helpProvider1;
  }
}