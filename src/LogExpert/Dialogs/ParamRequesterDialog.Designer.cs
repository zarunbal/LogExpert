namespace LogExpert.Dialogs
{
  partial class ParamRequesterDialog
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
      this.paramLabel = new System.Windows.Forms.Label();
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.valueComboBox = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // paramLabel
      // 
      this.paramLabel.AutoSize = true;
      this.paramLabel.Location = new System.Drawing.Point(13, 32);
      this.paramLabel.Name = "paramLabel";
      this.paramLabel.Size = new System.Drawing.Size(99, 13);
      this.paramLabel.TabIndex = 0;
      this.paramLabel.Text = "&Value for parameter";
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(238, 88);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 3;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(148, 88);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 2;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // valueComboBox
      // 
      this.valueComboBox.FormattingEnabled = true;
      this.valueComboBox.Location = new System.Drawing.Point(16, 48);
      this.valueComboBox.Name = "valueComboBox";
      this.valueComboBox.Size = new System.Drawing.Size(297, 21);
      this.valueComboBox.TabIndex = 1;
      // 
      // ParamRequesterDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(332, 130);
      this.Controls.Add(this.valueComboBox);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.paramLabel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ParamRequesterDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Tool parameter";
      this.Shown += new System.EventHandler(this.ParamRequesterDialog_Shown);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label paramLabel;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.ComboBox valueComboBox;
  }
}