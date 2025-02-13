namespace LogExpert
{
  partial class Log4jXmlColumnizerConfigDlg
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Log4jXmlColumnizerConfigDlg));
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.localTimeCheckBox = new System.Windows.Forms.CheckBox();
      this.columnGridView = new System.Windows.Forms.DataGridView();
      this.isVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.Column = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.maxWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.columnGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(210, 231);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 0;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(117, 231);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 1;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.OkButton_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(128, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Choose columns to show:";
      // 
      // localTimeCheckBox
      // 
      this.localTimeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.localTimeCheckBox.AutoSize = true;
      this.localTimeCheckBox.Location = new System.Drawing.Point(12, 208);
      this.localTimeCheckBox.Name = "localTimeCheckBox";
      this.localTimeCheckBox.Size = new System.Drawing.Size(203, 17);
      this.localTimeCheckBox.TabIndex = 4;
      this.localTimeCheckBox.Text = "Convert timestamps to local time zone";
      this.localTimeCheckBox.UseVisualStyleBackColor = true;
      // 
      // columnGridView
      // 
      this.columnGridView.AllowUserToAddRows = false;
      this.columnGridView.AllowUserToDeleteRows = false;
      this.columnGridView.AllowUserToResizeColumns = false;
      this.columnGridView.AllowUserToResizeRows = false;
      this.columnGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.columnGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.columnGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.isVisible,
            this.Column,
            this.maxWidth});
      this.columnGridView.Location = new System.Drawing.Point(12, 29);
      this.columnGridView.Name = "columnGridView";
      this.columnGridView.RowHeadersVisible = false;
      this.columnGridView.RowTemplate.Height = 20;
      this.columnGridView.Size = new System.Drawing.Size(274, 173);
      this.columnGridView.TabIndex = 5;
      // 
      // isVisible
      // 
      this.isVisible.HeaderText = "";
      this.isVisible.Name = "isVisible";
      this.isVisible.Width = 25;
      // 
      // Column
      // 
      this.Column.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.Column.HeaderText = "Column";
      this.Column.MinimumWidth = 150;
      this.Column.Name = "Column";
      this.Column.ReadOnly = true;
      this.Column.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // maxWidth
      // 
      this.maxWidth.HeaderText = "Max len";
      this.maxWidth.MaxInputLength = 4;
      this.maxWidth.MinimumWidth = 50;
      this.maxWidth.Name = "maxWidth";
      this.maxWidth.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.maxWidth.ToolTipText = "Max chars to display (cut from left)";
      this.maxWidth.Width = 50;
      // 
      // dataGridViewTextBoxColumn1
      // 
      this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dataGridViewTextBoxColumn1.HeaderText = "Column";
      this.dataGridViewTextBoxColumn1.MinimumWidth = 150;
      this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
      this.dataGridViewTextBoxColumn1.ReadOnly = true;
      this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // dataGridViewTextBoxColumn2
      // 
      this.dataGridViewTextBoxColumn2.HeaderText = "Max";
      this.dataGridViewTextBoxColumn2.MaxInputLength = 4;
      this.dataGridViewTextBoxColumn2.MinimumWidth = 40;
      this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
      this.dataGridViewTextBoxColumn2.ReadOnly = true;
      this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.dataGridViewTextBoxColumn2.ToolTipText = "Max chars to display (cut from left)";
      this.dataGridViewTextBoxColumn2.Width = 40;
      // 
      // Log4jXmlColumnizerConfigDlg
      // 
      this.ClientSize = new System.Drawing.Size(300, 266);
      this.Controls.Add(this.columnGridView);
      this.Controls.Add(this.localTimeCheckBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "Log4jXmlColumnizerConfigDlg";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Log4j XML Columnizer";
      ((System.ComponentModel.ISupportInitialize)(this.columnGridView)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox localTimeCheckBox;
    private System.Windows.Forms.DataGridView columnGridView;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private System.Windows.Forms.DataGridViewCheckBoxColumn isVisible;
    private System.Windows.Forms.DataGridViewTextBoxColumn Column;
    private System.Windows.Forms.DataGridViewTextBoxColumn maxWidth;
  }
}