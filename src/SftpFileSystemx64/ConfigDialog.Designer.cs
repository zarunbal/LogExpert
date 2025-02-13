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
      this.chkBoxPK = new System.Windows.Forms.CheckBox();
      this.keyFileButton = new System.Windows.Forms.Button();
      this.radioBtnPuttyKey = new System.Windows.Forms.RadioButton();
      this.radioBtnSSHKey = new System.Windows.Forms.RadioButton();
      this.keyTypeGroupBox = new System.Windows.Forms.GroupBox();
      this.lblFile = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.keyTypeGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // pkCheckBox
      // 
      this.chkBoxPK.AutoSize = true;
      this.chkBoxPK.Location = new System.Drawing.Point(12, 12);
      this.chkBoxPK.Name = "chkBoxPK";
      this.chkBoxPK.Size = new System.Drawing.Size(78, 17);
      this.chkBoxPK.TabIndex = 0;
      this.chkBoxPK.Text = "Use keyfile";
      this.chkBoxPK.UseVisualStyleBackColor = true;
      this.chkBoxPK.CheckStateChanged += new System.EventHandler(this.OnChkBoxPKCheckStateChanged);
      this.chkBoxPK.CheckedChanged += new System.EventHandler(this.OnChkBoxPKCheckedChanged);
      // 
      // keyFileButton
      // 
      this.keyFileButton.Location = new System.Drawing.Point(12, 35);
      this.keyFileButton.Name = "keyFileButton";
      this.keyFileButton.Size = new System.Drawing.Size(75, 23);
      this.keyFileButton.TabIndex = 1;
      this.keyFileButton.Text = "Select file...";
      this.keyFileButton.UseVisualStyleBackColor = true;
      this.keyFileButton.Click += new System.EventHandler(this.OnBtnKeyFileClick);
      // 
      // puttyKeyRadioButton
      // 
      this.radioBtnPuttyKey.AutoSize = true;
      this.radioBtnPuttyKey.Location = new System.Drawing.Point(6, 19);
      this.radioBtnPuttyKey.Name = "radioBtnPuttyKey";
      this.radioBtnPuttyKey.Size = new System.Drawing.Size(104, 17);
      this.radioBtnPuttyKey.TabIndex = 2;
      this.radioBtnPuttyKey.TabStop = true;
      this.radioBtnPuttyKey.Text = "Putty private key";
      this.radioBtnPuttyKey.UseVisualStyleBackColor = true;
      this.radioBtnPuttyKey.CheckedChanged += new System.EventHandler(this.OnRadioButtonPuttyKeyCheckedChanged);
      // 
      // sshKeyRadioButton
      // 
      this.radioBtnSSHKey.AutoSize = true;
      this.radioBtnSSHKey.Location = new System.Drawing.Point(6, 42);
      this.radioBtnSSHKey.Name = "radioBtnSSHKey";
      this.radioBtnSSHKey.Size = new System.Drawing.Size(131, 17);
      this.radioBtnSSHKey.TabIndex = 3;
      this.radioBtnSSHKey.TabStop = true;
      this.radioBtnSSHKey.Text = "Open SSH private key";
      this.radioBtnSSHKey.UseVisualStyleBackColor = true;
      this.radioBtnSSHKey.CheckedChanged += new System.EventHandler(this.OnRadioButtonSSHKeyCheckedChanged);
      // 
      // keyTypeGroupBox
      // 
      this.keyTypeGroupBox.Controls.Add(this.radioBtnPuttyKey);
      this.keyTypeGroupBox.Controls.Add(this.radioBtnSSHKey);
      this.keyTypeGroupBox.Location = new System.Drawing.Point(131, 12);
      this.keyTypeGroupBox.Name = "keyTypeGroupBox";
      this.keyTypeGroupBox.Size = new System.Drawing.Size(149, 74);
      this.keyTypeGroupBox.TabIndex = 4;
      this.keyTypeGroupBox.TabStop = false;
      this.keyTypeGroupBox.Text = "Key type";
      // 
      // fileLabel
      // 
      this.lblFile.Location = new System.Drawing.Point(12, 96);
      this.lblFile.Name = "lblFile";
      this.lblFile.Size = new System.Drawing.Size(332, 20);
      this.lblFile.TabIndex = 5;
      this.lblFile.Text = "label1";
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
      this.ClientSize = new System.Drawing.Size(362, 171);
      this.ControlBox = false;
      this.Controls.Add(this.label1);
      this.Controls.Add(this.lblFile);
      this.Controls.Add(this.keyTypeGroupBox);
      this.Controls.Add(this.keyFileButton);
      this.Controls.Add(this.chkBoxPK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "ConfigDialog";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "ConfigDialog";
      this.keyTypeGroupBox.ResumeLayout(false);
      this.keyTypeGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox chkBoxPK;
    private System.Windows.Forms.Button keyFileButton;
    private System.Windows.Forms.RadioButton radioBtnPuttyKey;
    private System.Windows.Forms.RadioButton radioBtnSSHKey;
    private System.Windows.Forms.GroupBox keyTypeGroupBox;
    private System.Windows.Forms.Label lblFile;
    private System.Windows.Forms.Label label1;
  }
}