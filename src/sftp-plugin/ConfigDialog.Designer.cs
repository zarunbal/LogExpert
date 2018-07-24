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
            this.label2 = new System.Windows.Forms.Label();
            this.sshApiCombo = new System.Windows.Forms.ComboBox();
            this.chilkatKeyBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.keyTypeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // pkCheckBox
            // 
            this.pkCheckBox.AutoSize = true;
            this.pkCheckBox.Location = new System.Drawing.Point(12, 53);
            this.pkCheckBox.Name = "pkCheckBox";
            this.pkCheckBox.Size = new System.Drawing.Size(78, 17);
            this.pkCheckBox.TabIndex = 0;
            this.pkCheckBox.Text = "Use keyfile";
            this.pkCheckBox.UseVisualStyleBackColor = true;
            this.pkCheckBox.CheckedChanged += new System.EventHandler(this.pkCheckBox_CheckedChanged);
            this.pkCheckBox.CheckStateChanged += new System.EventHandler(this.pkCheckBox_CheckStateChanged);
            // 
            // keyFileButton
            // 
            this.keyFileButton.Location = new System.Drawing.Point(12, 76);
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
            this.keyTypeGroupBox.Location = new System.Drawing.Point(131, 53);
            this.keyTypeGroupBox.Name = "keyTypeGroupBox";
            this.keyTypeGroupBox.Size = new System.Drawing.Size(149, 74);
            this.keyTypeGroupBox.TabIndex = 4;
            this.keyTypeGroupBox.TabStop = false;
            this.keyTypeGroupBox.Text = "Key type";
            // 
            // fileLabel
            // 
            this.fileLabel.Location = new System.Drawing.Point(12, 134);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(332, 14);
            this.fileLabel.TabIndex = 5;
            this.fileLabel.Text = "label1";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 154);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(335, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Key will be loaded once on first usage. Password will be asked then.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "SSH Api :";
            // 
            // sshApiCombo
            // 
            this.sshApiCombo.FormattingEnabled = true;
            this.sshApiCombo.Location = new System.Drawing.Point(70, 4);
            this.sshApiCombo.Name = "sshApiCombo";
            this.sshApiCombo.Size = new System.Drawing.Size(141, 21);
            this.sshApiCombo.TabIndex = 8;
            this.sshApiCombo.SelectedIndexChanged += new System.EventHandler(this.sshApiCombo_SelectedIndexChanged);
            // 
            // chilkatKeyBox
            // 
            this.chilkatKeyBox.Location = new System.Drawing.Point(81, 29);
            this.chilkatKeyBox.Name = "chilkatKeyBox";
            this.chilkatKeyBox.Size = new System.Drawing.Size(210, 20);
            this.chilkatKeyBox.TabIndex = 9;
            this.chilkatKeyBox.Visible = false;
            this.chilkatKeyBox.TextChanged += new System.EventHandler(this.chilkatKeyBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Chilkat Key :";
            this.label3.Visible = false;
            // 
            // ConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 171);
            this.ControlBox = false;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chilkatKeyBox);
            this.Controls.Add(this.sshApiCombo);
            this.Controls.Add(this.label2);
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox sshApiCombo;
        private System.Windows.Forms.TextBox chilkatKeyBox;
        private System.Windows.Forms.Label label3;
    }
}