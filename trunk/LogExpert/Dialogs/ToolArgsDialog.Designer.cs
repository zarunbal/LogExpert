namespace LogExpert.Dialogs
{
  partial class ToolArgsDialog
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
      this.regexHelpButton = new System.Windows.Forms.Button();
      this.argsTextBox = new System.Windows.Forms.TextBox();
      this.testButton = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.helpLabel = new System.Windows.Forms.Label();
      this.button2 = new System.Windows.Forms.Button();
      this.testResultLabel = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(429, 292);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 1;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.button1_Click);
      // 
      // regexHelpButton
      // 
      this.regexHelpButton.Location = new System.Drawing.Point(429, 59);
      this.regexHelpButton.Name = "regexHelpButton";
      this.regexHelpButton.Size = new System.Drawing.Size(75, 21);
      this.regexHelpButton.TabIndex = 2;
      this.regexHelpButton.Text = "RegEx Help";
      this.regexHelpButton.UseVisualStyleBackColor = true;
      this.regexHelpButton.Click += new System.EventHandler(this.regexHelpButton_Click);
      // 
      // argsTextBox
      // 
      this.argsTextBox.Location = new System.Drawing.Point(12, 30);
      this.argsTextBox.Name = "argsTextBox";
      this.argsTextBox.Size = new System.Drawing.Size(395, 20);
      this.argsTextBox.TabIndex = 8;
      // 
      // testButton
      // 
      this.testButton.Location = new System.Drawing.Point(429, 30);
      this.testButton.Name = "testButton";
      this.testButton.Size = new System.Drawing.Size(75, 23);
      this.testButton.TabIndex = 9;
      this.testButton.Text = "Test";
      this.testButton.UseVisualStyleBackColor = true;
      this.testButton.Click += new System.EventHandler(this.testButton_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(103, 13);
      this.label1.TabIndex = 11;
      this.label1.Text = "Enter command line:";
      // 
      // helpLabel
      // 
      this.helpLabel.Location = new System.Drawing.Point(15, 113);
      this.helpLabel.Name = "helpLabel";
      this.helpLabel.Size = new System.Drawing.Size(392, 157);
      this.helpLabel.TabIndex = 12;
      this.helpLabel.Text = "label2";
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(429, 261);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 13;
      this.button2.Text = "Cancel";
      this.button2.UseVisualStyleBackColor = true;
      // 
      // testResultLabel
      // 
      this.testResultLabel.Location = new System.Drawing.Point(12, 56);
      this.testResultLabel.Multiline = true;
      this.testResultLabel.Name = "testResultLabel";
      this.testResultLabel.ReadOnly = true;
      this.testResultLabel.Size = new System.Drawing.Size(395, 48);
      this.testResultLabel.TabIndex = 14;
      // 
      // ToolArgsDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.button2;
      this.ClientSize = new System.Drawing.Size(516, 327);
      this.Controls.Add(this.testResultLabel);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.helpLabel);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.testButton);
      this.Controls.Add(this.argsTextBox);
      this.Controls.Add(this.regexHelpButton);
      this.Controls.Add(this.okButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ToolArgsDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Tool Arguments Help";
      this.Load += new System.EventHandler(this.ToolArgsDialog_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button regexHelpButton;
    private System.Windows.Forms.TextBox argsTextBox;
    private System.Windows.Forms.Button testButton;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label helpLabel;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.TextBox testResultLabel;
  }
}