namespace LogExpert.UI.Dialogs;

partial class ChooseIconDlg
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseIconDlg));
        this.iconListView = new System.Windows.Forms.ListView();
        this.iconFileLabel = new System.Windows.Forms.Label();
        this.buttonChooseIconFile = new System.Windows.Forms.Button();
        this.buttonOk = new System.Windows.Forms.Button();
        this.buttonCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // iconListView
        // 
        this.iconListView.Alignment = System.Windows.Forms.ListViewAlignment.Left;
        this.iconListView.AutoArrange = false;
        this.iconListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        this.iconListView.HideSelection = false;
        this.iconListView.Location = new System.Drawing.Point(18, 63);
        this.iconListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
        this.iconListView.MultiSelect = false;
        this.iconListView.Name = "iconListView";
        this.iconListView.ShowGroups = false;
        this.iconListView.Size = new System.Drawing.Size(529, 118);
        this.iconListView.TabIndex = 0;
        this.iconListView.UseCompatibleStateImageBehavior = false;
        // 
        // iconFileLabel
        // 
        this.iconFileLabel.Location = new System.Drawing.Point(18, 26);
        this.iconFileLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.iconFileLabel.Name = "iconFileLabel";
        this.iconFileLabel.Size = new System.Drawing.Size(411, 35);
        this.iconFileLabel.TabIndex = 1;
        this.iconFileLabel.Text = "IconFileLabel";
        // 
        // buttonChooseIconFile
        // 
        this.buttonChooseIconFile.Location = new System.Drawing.Point(438, 18);
        this.buttonChooseIconFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
        this.buttonChooseIconFile.Name = "buttonChooseIconFile";
        this.buttonChooseIconFile.Size = new System.Drawing.Size(112, 35);
        this.buttonChooseIconFile.TabIndex = 2;
        this.buttonChooseIconFile.Text = "Icon file...";
        this.buttonChooseIconFile.UseVisualStyleBackColor = true;
        this.buttonChooseIconFile.Click += new System.EventHandler(this.OnButtonChooseIconFileClick);
        // 
        // okButton
        // 
        this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.buttonOk.Location = new System.Drawing.Point(316, 226);
        this.buttonOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
        this.buttonOk.Name = "okButton";
        this.buttonOk.Size = new System.Drawing.Size(112, 35);
        this.buttonOk.TabIndex = 3;
        this.buttonOk.Text = "OK";
        this.buttonOk.UseVisualStyleBackColor = true;
        this.buttonOk.Click += new System.EventHandler(this.OnOkButtonClick);
        // 
        // cancelButton
        // 
        this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.buttonCancel.Location = new System.Drawing.Point(440, 226);
        this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
        this.buttonCancel.Name = "cancelButton";
        this.buttonCancel.Size = new System.Drawing.Size(112, 35);
        this.buttonCancel.TabIndex = 4;
        this.buttonCancel.Text = "Cancel";
        this.buttonCancel.UseVisualStyleBackColor = true;
        // 
        // ChooseIconDlg
        // 
        this.ClientSize = new System.Drawing.Size(568, 278);
        this.Controls.Add(this.buttonCancel);
        this.Controls.Add(this.buttonOk);
        this.Controls.Add(this.buttonChooseIconFile);
        this.Controls.Add(this.iconFileLabel);
        this.Controls.Add(this.iconListView);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ChooseIconDlg";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Choose Icon";
        this.Load += new System.EventHandler(this.ChooseIconDlg_Load);
        this.ResumeLayout(false);

}

#endregion

private System.Windows.Forms.ListView iconListView;
private System.Windows.Forms.Label iconFileLabel;
private System.Windows.Forms.Button buttonChooseIconFile;
private System.Windows.Forms.Button buttonOk;
private System.Windows.Forms.Button buttonCancel;
}