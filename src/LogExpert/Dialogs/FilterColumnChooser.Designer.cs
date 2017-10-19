namespace LogExpert.Dialogs
{
  partial class FilterColumnChooser
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterColumnChooser));
      this.columnListBox = new System.Windows.Forms.CheckedListBox();
      this.okButton = new System.Windows.Forms.Button();
      this.toolTipEmptyColumnNoHit = new System.Windows.Forms.ToolTip(this.components);
      this.emptyColumnNoHitRadioButton = new System.Windows.Forms.RadioButton();
      this.toolTipListBox = new System.Windows.Forms.ToolTip(this.components);
      this.exactMatchCheckBox = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.emptyColumnUsePrevRadioButton = new System.Windows.Forms.RadioButton();
      this.emptyColumnHitRadioButton = new System.Windows.Forms.RadioButton();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.toolTipSearchHit = new System.Windows.Forms.ToolTip(this.components);
      this.toolTipPrevContent = new System.Windows.Forms.ToolTip(this.components);
      this.toolTipExactMatch = new System.Windows.Forms.ToolTip(this.components);
      this.cancelButton = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // columnListBox
      // 
      this.columnListBox.CheckOnClick = true;
      this.columnListBox.FormattingEnabled = true;
      this.columnListBox.Location = new System.Drawing.Point(3, 3);
      this.columnListBox.Name = "columnListBox";
      this.columnListBox.Size = new System.Drawing.Size(150, 139);
      this.columnListBox.TabIndex = 0;
      this.toolTipListBox.SetToolTip(this.columnListBox, "Choose one ore more columns to restrict the search operations to the selected col" +
              "umns.");
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(3, 291);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 2;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // toolTipEmptyColumnNoHit
      // 
      this.toolTipEmptyColumnNoHit.ToolTipTitle = "Empty column";
      // 
      // emptyColumnNoHitRadioButton
      // 
      this.emptyColumnNoHitRadioButton.AutoSize = true;
      this.emptyColumnNoHitRadioButton.Location = new System.Drawing.Point(6, 19);
      this.emptyColumnNoHitRadioButton.Name = "emptyColumnNoHitRadioButton";
      this.emptyColumnNoHitRadioButton.Size = new System.Drawing.Size(53, 17);
      this.emptyColumnNoHitRadioButton.TabIndex = 0;
      this.emptyColumnNoHitRadioButton.TabStop = true;
      this.emptyColumnNoHitRadioButton.Text = "No hit";
      this.toolTipEmptyColumnNoHit.SetToolTip(this.emptyColumnNoHitRadioButton, "No search hit on empty columns");
      this.emptyColumnNoHitRadioButton.UseVisualStyleBackColor = true;
      // 
      // toolTipListBox
      // 
      this.toolTipListBox.ToolTipTitle = "Columns";
      // 
      // exactMatchCheckBox
      // 
      this.exactMatchCheckBox.AutoSize = true;
      this.exactMatchCheckBox.Location = new System.Drawing.Point(6, 12);
      this.exactMatchCheckBox.Name = "exactMatchCheckBox";
      this.exactMatchCheckBox.Size = new System.Drawing.Size(85, 17);
      this.exactMatchCheckBox.TabIndex = 4;
      this.exactMatchCheckBox.Text = "Exact match";
      this.toolTipExactMatch.SetToolTip(this.exactMatchCheckBox, "If selected, the search string must match exactly (no substring search)");
      this.exactMatchCheckBox.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.emptyColumnUsePrevRadioButton);
      this.groupBox1.Controls.Add(this.emptyColumnHitRadioButton);
      this.groupBox1.Controls.Add(this.emptyColumnNoHitRadioButton);
      this.groupBox1.Location = new System.Drawing.Point(3, 188);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(147, 97);
      this.groupBox1.TabIndex = 5;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "On empty columns";
      // 
      // emptyColumnUsePrevRadioButton
      // 
      this.emptyColumnUsePrevRadioButton.AutoSize = true;
      this.emptyColumnUsePrevRadioButton.Location = new System.Drawing.Point(6, 65);
      this.emptyColumnUsePrevRadioButton.Name = "emptyColumnUsePrevRadioButton";
      this.emptyColumnUsePrevRadioButton.Size = new System.Drawing.Size(107, 17);
      this.emptyColumnUsePrevRadioButton.TabIndex = 2;
      this.emptyColumnUsePrevRadioButton.TabStop = true;
      this.emptyColumnUsePrevRadioButton.Text = "Use prev content";
      this.toolTipPrevContent.SetToolTip(this.emptyColumnUsePrevRadioButton, "An empty column will be a search hit if the previous non-empty column was a searc" +
              "h hit");
      this.emptyColumnUsePrevRadioButton.UseVisualStyleBackColor = true;
      // 
      // emptyColumnHitRadioButton
      // 
      this.emptyColumnHitRadioButton.AutoSize = true;
      this.emptyColumnHitRadioButton.Location = new System.Drawing.Point(6, 42);
      this.emptyColumnHitRadioButton.Name = "emptyColumnHitRadioButton";
      this.emptyColumnHitRadioButton.Size = new System.Drawing.Size(73, 17);
      this.emptyColumnHitRadioButton.TabIndex = 1;
      this.emptyColumnHitRadioButton.TabStop = true;
      this.emptyColumnHitRadioButton.Text = "Search hit";
      this.toolTipSearchHit.SetToolTip(this.emptyColumnHitRadioButton, "An empty column will always be a search hit");
      this.emptyColumnHitRadioButton.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.exactMatchCheckBox);
      this.groupBox2.Location = new System.Drawing.Point(3, 148);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(147, 34);
      this.groupBox2.TabIndex = 6;
      this.groupBox2.TabStop = false;
      // 
      // toolTipSearchHit
      // 
      this.toolTipSearchHit.ToolTipTitle = "Empty column";
      // 
      // toolTipPrevContent
      // 
      this.toolTipPrevContent.ToolTipTitle = "Empty column";
      // 
      // toolTipExactMatch
      // 
      this.toolTipExactMatch.ToolTipTitle = "Exact match";
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(90, 291);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(60, 23);
      this.cancelButton.TabIndex = 7;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // FilterColumnChooser
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(157, 319);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.columnListBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "FilterColumnChooser";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Columns";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.CheckedListBox columnListBox;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.ToolTip toolTipEmptyColumnNoHit;
    private System.Windows.Forms.ToolTip toolTipListBox;
    private System.Windows.Forms.CheckBox exactMatchCheckBox;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton emptyColumnUsePrevRadioButton;
    private System.Windows.Forms.RadioButton emptyColumnHitRadioButton;
    private System.Windows.Forms.RadioButton emptyColumnNoHitRadioButton;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ToolTip toolTipExactMatch;
    private System.Windows.Forms.ToolTip toolTipSearchHit;
    private System.Windows.Forms.ToolTip toolTipPrevContent;
    private System.Windows.Forms.Button cancelButton;
  }
}