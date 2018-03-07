namespace SftpFileSystem
{
  partial class ConfigDialog
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
      this.pkCheckBox = new System.Windows.Forms.CheckBox();
      this.keyFileButton = new System.Windows.Forms.Button();
      this.puttyKeyRadioButton = new System.Windows.Forms.RadioButton();
      this.sshKeyRadioButton = new System.Windows.Forms.RadioButton();
      this.keyTypeGroupBox = new System.Windows.Forms.GroupBox();
      this.fileLabel = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.keyTypeGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // pkCheckBox
      // 
      this.pkCheckBox.AutoSize = true;
      this.pkCheckBox.Location = new System.Drawing.Point(12, 12);
      this.pkCheckBox.Name = "pkCheckBox";
      this.pkCheckBox.Size = new System.Drawing.Size(78, 17);
      this.pkCheckBox.TabIndex = 0;
      this.pkCheckBox.Text = "Use keyfile";
      this.pkCheckBox.UseVisualStyleBackColor = true;
      this.pkCheckBox.CheckStateChanged += new System.EventHandler(this.pkCheckBox_CheckStateChanged);
      this.pkCheckBox.CheckedChanged += new System.EventHandler(this.pkCheckBox_CheckedChanged);
      // 
      // keyFileButton
      // 
      this.keyFileButton.Location = new System.Drawing.Point(12, 35);
      this.keyFileButton.Name = "keyFileButton";
      this.keyFileButton.Size = new System.Drawing.Size(75, 23);
      this.keyFileButton.TabIndex = 1;
      this.keyFileButton.Text = "Select file...";
      this.keyFileButton.UseVisualStyleBackColor = true;
      this.keyFileButton.Click += new System.EventHandler(this.keyFileButton_Click);
      // 
      // puttyKeyRadioButton
      // 
      this.puttyKeyRadioButton.AutoSize = true;
      this.puttyKeyRadioButton.Location = new System.Drawing.Point(6, 19);
      this.puttyKeyRadioButton.Name = "puttyKeyRadioButton";
      this.puttyKeyRadioButton.Size = new System.Drawing.Size(104, 17);
      this.puttyKeyRadioButton.TabIndex = 2;
      this.puttyKeyRadioButton.TabStop = true;
      this.puttyKeyRadioButton.Text = "Putty private key";
      this.puttyKeyRadioButton.UseVisualStyleBackColor = true;
      this.puttyKeyRadioButton.CheckedChanged += new System.EventHandler(this.puttyKeyRadioButton_CheckedChanged);
      // 
      // sshKeyRadioButton
      // 
      this.sshKeyRadioButton.AutoSize = true;
      this.sshKeyRadioButton.Location = new System.Drawing.Point(6, 42);
      this.sshKeyRadioButton.Name = "sshKeyRadioButton";
      this.sshKeyRadioButton.Size = new System.Drawing.Size(131, 17);
      this.sshKeyRadioButton.TabIndex = 3;
      this.sshKeyRadioButton.TabStop = true;
      this.sshKeyRadioButton.Text = "Open SSH private key";
      this.sshKeyRadioButton.UseVisualStyleBackColor = true;
      this.sshKeyRadioButton.CheckedChanged += new System.EventHandler(this.sshKeyRadioButton_CheckedChanged);
      // 
      // keyTypeGroupBox
      // 
      this.keyTypeGroupBox.Controls.Add(this.puttyKeyRadioButton);
      this.keyTypeGroupBox.Controls.Add(this.sshKeyRadioButton);
      this.keyTypeGroupBox.Location = new System.Drawing.Point(131, 12);
      this.keyTypeGroupBox.Name = "keyTypeGroupBox";
      this.keyTypeGroupBox.Size = new System.Drawing.Size(149, 74);
      this.keyTypeGroupBox.TabIndex = 4;
      this.keyTypeGroupBox.TabStop = false;
      this.keyTypeGroupBox.Text = "Key type";
      // 
      // fileLabel
      // 
      this.fileLabel.Location = new System.Drawing.Point(12, 96);
      this.fileLabel.Name = "fileLabel";
      this.fileLabel.Size = new System.Drawing.Size(332, 20);
      this.fileLabel.TabIndex = 5;
      this.fileLabel.Text = "label1";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(9, 140);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(335, 32);
      this.label1.TabIndex = 6;
      this.label1.Text = "Key will be loaded once on first usage. Password will be asked then.";
      // 
      // ConfigDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(362, 171);
      this.ControlBox = false;
      this.Controls.Add(this.label1);
      this.Controls.Add(this.fileLabel);
      this.Controls.Add(this.keyTypeGroupBox);
      this.Controls.Add(this.keyFileButton);
      this.Controls.Add(this.pkCheckBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "ConfigDialog";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "ConfigDialog";
      this.Load += new System.EventHandler(this.ConfigDialog_Load);
      this.keyTypeGroupBox.ResumeLayout(false);
      this.keyTypeGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox pkCheckBox;
    private System.Windows.Forms.Button keyFileButton;
    private System.Windows.Forms.RadioButton puttyKeyRadioButton;
    private System.Windows.Forms.RadioButton sshKeyRadioButton;
    private System.Windows.Forms.GroupBox keyTypeGroupBox;
    private System.Windows.Forms.Label fileLabel;
    private System.Windows.Forms.Label label1;
  }
}