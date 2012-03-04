namespace LogExpert.Dialogs
{
  partial class ProjectLoadDlg
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectLoadDlg));
      this.label1 = new System.Windows.Forms.Label();
      this.closeTabsButton = new System.Windows.Forms.Button();
      this.newWindowButton = new System.Windows.Forms.Button();
      this.ignoreButton = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(178, 41);
      this.label1.TabIndex = 0;
      this.label1.Text = "Restoring layout requires an empty workbench. \r\n\r\n";
      // 
      // closeTabsButton
      // 
      this.closeTabsButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.closeTabsButton.Location = new System.Drawing.Point(43, 94);
      this.closeTabsButton.Name = "closeTabsButton";
      this.closeTabsButton.Size = new System.Drawing.Size(113, 23);
      this.closeTabsButton.TabIndex = 1;
      this.closeTabsButton.Text = "Close existing tabs";
      this.closeTabsButton.UseVisualStyleBackColor = true;
      this.closeTabsButton.Click += new System.EventHandler(this.closeTabsButton_Click);
      // 
      // newWindowButton
      // 
      this.newWindowButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.newWindowButton.Location = new System.Drawing.Point(43, 124);
      this.newWindowButton.Name = "newWindowButton";
      this.newWindowButton.Size = new System.Drawing.Size(113, 23);
      this.newWindowButton.TabIndex = 2;
      this.newWindowButton.Text = "Open new window";
      this.newWindowButton.UseVisualStyleBackColor = true;
      this.newWindowButton.Click += new System.EventHandler(this.newWindowButton_Click);
      // 
      // ignoreButton
      // 
      this.ignoreButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.ignoreButton.Location = new System.Drawing.Point(43, 154);
      this.ignoreButton.Name = "ignoreButton";
      this.ignoreButton.Size = new System.Drawing.Size(113, 23);
      this.ignoreButton.TabIndex = 3;
      this.ignoreButton.Text = "Ignore layout data";
      this.ignoreButton.UseVisualStyleBackColor = true;
      this.ignoreButton.Click += new System.EventHandler(this.ignoreButton_Click);
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 66);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(157, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Please choose how to proceed:";
      // 
      // ProjectLoadDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(202, 196);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.ignoreButton);
      this.Controls.Add(this.newWindowButton);
      this.Controls.Add(this.closeTabsButton);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ProjectLoadDlg";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Loading Session";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button closeTabsButton;
    private System.Windows.Forms.Button newWindowButton;
    private System.Windows.Forms.Button ignoreButton;
    private System.Windows.Forms.Label label2;
  }
}