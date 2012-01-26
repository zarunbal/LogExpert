namespace LogExpert.Dialogs
{
  partial class BugzillaDialog
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BugzillaDialog));
      this.introTextLabel = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.commentBox = new System.Windows.Forms.TextBox();
      this.cancelButton = new System.Windows.Forms.Button();
      this.sendButton = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.ccTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // introTextLabel
      // 
      this.introTextLabel.Location = new System.Drawing.Point(13, 13);
      this.introTextLabel.Name = "introTextLabel";
      this.introTextLabel.Size = new System.Drawing.Size(430, 88);
      this.introTextLabel.TabIndex = 0;
      this.introTextLabel.Text = "You can create an automatic bug entry now.";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(13, 329);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(86, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Bugzilla location:";
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(99, 329);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(186, 13);
      this.linkLabel1.TabIndex = 2;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "http://www.logfile-viewer.de/bugzilla/";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // commentBox
      // 
      this.commentBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.commentBox.Location = new System.Drawing.Point(16, 128);
      this.commentBox.Multiline = true;
      this.commentBox.Name = "commentBox";
      this.commentBox.Size = new System.Drawing.Size(439, 127);
      this.commentBox.TabIndex = 3;
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(380, 361);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 4;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // sendButton
      // 
      this.sendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.sendButton.Location = new System.Drawing.Point(16, 361);
      this.sendButton.Name = "sendButton";
      this.sendButton.Size = new System.Drawing.Size(101, 23);
      this.sendButton.TabIndex = 5;
      this.sendButton.Text = "Post bug entry";
      this.sendButton.UseVisualStyleBackColor = true;
      this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 112);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(408, 13);
      this.label1.TabIndex = 6;
      this.label1.Text = "Additional comments for the bug, e.g, how to reproduce (optional, but recommended" +
          "):";
      // 
      // ccTextBox
      // 
      this.ccTextBox.Location = new System.Drawing.Point(16, 291);
      this.ccTextBox.Name = "ccTextBox";
      this.ccTextBox.Size = new System.Drawing.Size(439, 20);
      this.ccTextBox.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(13, 275);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(367, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "Bugzilla user (email) to get status updates for this bug from Bugzilla (optional)" +
          ":";
      // 
      // BugzillaDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(467, 396);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.ccTextBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.sendButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.commentBox);
      this.Controls.Add(this.linkLabel1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.introTextLabel);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "BugzillaDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Create Bugzilla bug entry";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label introTextLabel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.TextBox commentBox;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button sendButton;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox ccTextBox;
    private System.Windows.Forms.Label label3;
  }
}