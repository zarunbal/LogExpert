namespace LogExpert.Dialogs
{
  partial class ImportSettingsDialog
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportSettingsDialog));
      this.fileButton = new System.Windows.Forms.Button();
      this.fileNameTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.optionsGroupBox = new System.Windows.Forms.GroupBox();
      this.checkBox5 = new System.Windows.Forms.CheckBox();
      this.checkBox4 = new System.Windows.Forms.CheckBox();
      this.checkBox3 = new System.Windows.Forms.CheckBox();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.highlightSettingsCheckBox = new System.Windows.Forms.CheckBox();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.optionsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // fileButton
      // 
      this.fileButton.Location = new System.Drawing.Point(306, 25);
      this.fileButton.Name = "fileButton";
      this.fileButton.Size = new System.Drawing.Size(94, 23);
      this.fileButton.TabIndex = 0;
      this.fileButton.Text = "Choose file...";
      this.fileButton.UseVisualStyleBackColor = true;
      this.fileButton.Click += new System.EventHandler(this.fileButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Location = new System.Drawing.Point(13, 27);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(287, 20);
      this.fileNameTextBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(107, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Settings file to import:";
      // 
      // optionsGroupBox
      // 
      this.optionsGroupBox.Controls.Add(this.checkBox5);
      this.optionsGroupBox.Controls.Add(this.checkBox4);
      this.optionsGroupBox.Controls.Add(this.checkBox3);
      this.optionsGroupBox.Controls.Add(this.checkBox2);
      this.optionsGroupBox.Controls.Add(this.highlightSettingsCheckBox);
      this.optionsGroupBox.Location = new System.Drawing.Point(16, 67);
      this.optionsGroupBox.Name = "optionsGroupBox";
      this.optionsGroupBox.Size = new System.Drawing.Size(284, 143);
      this.optionsGroupBox.TabIndex = 3;
      this.optionsGroupBox.TabStop = false;
      this.optionsGroupBox.Text = "Import options";
      // 
      // checkBox5
      // 
      this.checkBox5.AutoSize = true;
      this.checkBox5.Location = new System.Drawing.Point(7, 112);
      this.checkBox5.Name = "checkBox5";
      this.checkBox5.Size = new System.Drawing.Size(52, 17);
      this.checkBox5.TabIndex = 0;
      this.checkBox5.Tag = "16";
      this.checkBox5.Text = "Other";
      this.checkBox5.UseVisualStyleBackColor = true;
      // 
      // checkBox4
      // 
      this.checkBox4.AutoSize = true;
      this.checkBox4.Location = new System.Drawing.Point(7, 89);
      this.checkBox4.Name = "checkBox4";
      this.checkBox4.Size = new System.Drawing.Size(89, 17);
      this.checkBox4.TabIndex = 0;
      this.checkBox4.Tag = "8";
      this.checkBox4.Text = "External tools";
      this.checkBox4.UseVisualStyleBackColor = true;
      // 
      // checkBox3
      // 
      this.checkBox3.AutoSize = true;
      this.checkBox3.Location = new System.Drawing.Point(7, 66);
      this.checkBox3.Name = "checkBox3";
      this.checkBox3.Size = new System.Drawing.Size(126, 17);
      this.checkBox3.TabIndex = 0;
      this.checkBox3.Tag = "2";
      this.checkBox3.Text = "Columnizer file masks";
      this.checkBox3.UseVisualStyleBackColor = true;
      // 
      // checkBox2
      // 
      this.checkBox2.AutoSize = true;
      this.checkBox2.Location = new System.Drawing.Point(7, 43);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(116, 17);
      this.checkBox2.TabIndex = 0;
      this.checkBox2.Tag = "4";
      this.checkBox2.Text = "Highlight file masks";
      this.checkBox2.UseVisualStyleBackColor = true;
      // 
      // highlightSettingsCheckBox
      // 
      this.highlightSettingsCheckBox.AutoSize = true;
      this.highlightSettingsCheckBox.Location = new System.Drawing.Point(7, 20);
      this.highlightSettingsCheckBox.Name = "highlightSettingsCheckBox";
      this.highlightSettingsCheckBox.Size = new System.Drawing.Size(106, 17);
      this.highlightSettingsCheckBox.TabIndex = 0;
      this.highlightSettingsCheckBox.Tag = "1";
      this.highlightSettingsCheckBox.Text = "Highlight settings";
      this.highlightSettingsCheckBox.UseVisualStyleBackColor = true;
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(325, 187);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(325, 154);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 5;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // ImportSettingsDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(412, 224);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.optionsGroupBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.fileNameTextBox);
      this.Controls.Add(this.fileButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ImportSettingsDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Import Settings";
      this.Load += new System.EventHandler(this.ImportSettingsDialog_Load);
      this.optionsGroupBox.ResumeLayout(false);
      this.optionsGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button fileButton;
    private System.Windows.Forms.TextBox fileNameTextBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox optionsGroupBox;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.CheckBox checkBox4;
    private System.Windows.Forms.CheckBox checkBox3;
    private System.Windows.Forms.CheckBox checkBox2;
    private System.Windows.Forms.CheckBox highlightSettingsCheckBox;
    private System.Windows.Forms.CheckBox checkBox5;
  }
}