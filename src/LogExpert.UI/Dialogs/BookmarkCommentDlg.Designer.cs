namespace LogExpert.Dialogs;

partial class BookmarkCommentDlg
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
  System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookmarkCommentDlg));
  this.buttonOk = new System.Windows.Forms.Button();
  this.buttonCancel = new System.Windows.Forms.Button();
  this.textBoxComment = new System.Windows.Forms.TextBox();
  this.SuspendLayout();
  // 
  // okButton
  // 
  this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
  this.buttonOk.Location = new System.Drawing.Point(150, 86);
  this.buttonOk.Name = "buttonOk";
  this.buttonOk.Size = new System.Drawing.Size(75, 23);
  this.buttonOk.TabIndex = 1;
  this.buttonOk.Text = "&OK";
  this.buttonOk.UseVisualStyleBackColor = true;
  // 
  // cancelButton
  // 
  this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
  this.buttonCancel.Location = new System.Drawing.Point(241, 86);
  this.buttonCancel.Name = "buttonCancel";
  this.buttonCancel.Size = new System.Drawing.Size(75, 23);
  this.buttonCancel.TabIndex = 2;
  this.buttonCancel.Text = "&Cancel";
  this.buttonCancel.UseVisualStyleBackColor = true;
  // 
  // commentTextBox
  // 
  this.textBoxComment.Dock = System.Windows.Forms.DockStyle.Top;
  this.textBoxComment.Location = new System.Drawing.Point(0, 0);
  this.textBoxComment.Multiline = true;
  this.textBoxComment.Name = "textBoxComment";
  this.textBoxComment.Size = new System.Drawing.Size(324, 80);
  this.textBoxComment.TabIndex = 0;
  // 
  // BookmarkCommentDlg
  // 
  this.AcceptButton = this.buttonOk;
  this.CancelButton = this.buttonCancel;
  this.ClientSize = new System.Drawing.Size(324, 115);
  this.Controls.Add(this.textBoxComment);
  this.Controls.Add(this.buttonCancel);
  this.Controls.Add(this.buttonOk);
  this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
  this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
  this.Name = "BookmarkCommentDlg";
  this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
  this.Text = "Bookmark comment";
  this.ResumeLayout(false);
  this.PerformLayout();

}

#endregion

private System.Windows.Forms.Button buttonOk;
private System.Windows.Forms.Button buttonCancel;
private System.Windows.Forms.TextBox textBoxComment;
}