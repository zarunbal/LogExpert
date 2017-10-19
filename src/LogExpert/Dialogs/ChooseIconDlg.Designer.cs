namespace LogExpert.Dialogs
{
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
      this.button1 = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // iconListView
      // 
      this.iconListView.Alignment = System.Windows.Forms.ListViewAlignment.Left;
      this.iconListView.AutoArrange = false;
      this.iconListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.iconListView.Location = new System.Drawing.Point(12, 41);
      this.iconListView.MultiSelect = false;
      this.iconListView.Name = "iconListView";
      this.iconListView.ShowGroups = false;
      this.iconListView.Size = new System.Drawing.Size(354, 78);
      this.iconListView.TabIndex = 0;
      this.iconListView.UseCompatibleStateImageBehavior = false;
      // 
      // iconFileLabel
      // 
      this.iconFileLabel.Location = new System.Drawing.Point(12, 17);
      this.iconFileLabel.Name = "iconFileLabel";
      this.iconFileLabel.Size = new System.Drawing.Size(274, 23);
      this.iconFileLabel.TabIndex = 1;
      this.iconFileLabel.Text = "label1";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(292, 12);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 2;
      this.button1.Text = "Icon file...";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(211, 147);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(293, 147);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 4;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // ChooseIconDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(379, 181);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.iconFileLabel);
      this.Controls.Add(this.iconListView);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
  }
}