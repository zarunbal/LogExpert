namespace SftpFileSystem
{
  partial class LoginDialog
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
      this.label1 = new System.Windows.Forms.Label();
      this.passwordLabel = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.serverNameLabel = new System.Windows.Forms.Label();
      this.userNameComboBox = new System.Windows.Forms.ComboBox();
      this.passwordTextBox = new System.Windows.Forms.TextBox();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 44);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(61, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "User name:";
      // 
      // passwordLabel
      // 
      this.passwordLabel.AutoSize = true;
      this.passwordLabel.Location = new System.Drawing.Point(13, 75);
      this.passwordLabel.Name = "passwordLabel";
      this.passwordLabel.Size = new System.Drawing.Size(56, 13);
      this.passwordLabel.TabIndex = 1;
      this.passwordLabel.Text = "Password:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(13, 13);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(41, 13);
      this.label3.TabIndex = 2;
      this.label3.Text = "Server:";
      // 
      // serverNameLabel
      // 
      this.serverNameLabel.AutoSize = true;
      this.serverNameLabel.Location = new System.Drawing.Point(93, 13);
      this.serverNameLabel.Name = "serverNameLabel";
      this.serverNameLabel.Size = new System.Drawing.Size(10, 13);
      this.serverNameLabel.TabIndex = 3;
      this.serverNameLabel.Text = "-";
      // 
      // userNameComboBox
      // 
      this.userNameComboBox.FormattingEnabled = true;
      this.userNameComboBox.Location = new System.Drawing.Point(96, 41);
      this.userNameComboBox.Name = "userNameComboBox";
      this.userNameComboBox.Size = new System.Drawing.Size(157, 21);
      this.userNameComboBox.TabIndex = 4;
      // 
      // passwordTextBox
      // 
      this.passwordTextBox.Location = new System.Drawing.Point(96, 72);
      this.passwordTextBox.Name = "passwordTextBox";
      this.passwordTextBox.Size = new System.Drawing.Size(157, 20);
      this.passwordTextBox.TabIndex = 5;
      this.passwordTextBox.UseSystemPasswordChar = true;
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(201, 119);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 6;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(282, 119);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 7;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // LoginDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(369, 156);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.passwordTextBox);
      this.Controls.Add(this.userNameComboBox);
      this.Controls.Add(this.serverNameLabel);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.passwordLabel);
      this.Controls.Add(this.label1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "LoginDialog";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "LogExpert SFTP Plugin";
      this.Load += new System.EventHandler(this.LoginDialog_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label passwordLabel;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label serverNameLabel;
    private System.Windows.Forms.ComboBox userNameComboBox;
    private System.Windows.Forms.TextBox passwordTextBox;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
  }
}