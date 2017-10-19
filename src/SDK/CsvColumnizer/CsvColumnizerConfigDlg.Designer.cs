namespace CsvColumnizer
{
  partial class CsvColumnizerConfigDlg
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
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.delimiterTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.quoteCharTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.escapeCharTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.escapeCheckBox = new System.Windows.Forms.CheckBox();
      this.commentCharTextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.fieldNamesCheckBox = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(130, 172);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(211, 172);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // delimiterTextBox
      // 
      this.delimiterTextBox.Location = new System.Drawing.Point(95, 13);
      this.delimiterTextBox.MaxLength = 1;
      this.delimiterTextBox.Name = "delimiterTextBox";
      this.delimiterTextBox.Size = new System.Drawing.Size(28, 20);
      this.delimiterTextBox.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(74, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Delimiter char:";
      // 
      // quoteCharTextBox
      // 
      this.quoteCharTextBox.Location = new System.Drawing.Point(95, 40);
      this.quoteCharTextBox.Name = "quoteCharTextBox";
      this.quoteCharTextBox.Size = new System.Drawing.Size(28, 20);
      this.quoteCharTextBox.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 43);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(63, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Quote char:";
      // 
      // escapeCharTextBox
      // 
      this.escapeCharTextBox.Location = new System.Drawing.Point(95, 67);
      this.escapeCharTextBox.Name = "escapeCharTextBox";
      this.escapeCharTextBox.Size = new System.Drawing.Size(28, 20);
      this.escapeCharTextBox.TabIndex = 6;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 70);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(70, 13);
      this.label3.TabIndex = 7;
      this.label3.Text = "Escape char:";
      // 
      // escapeCheckBox
      // 
      this.escapeCheckBox.AutoSize = true;
      this.escapeCheckBox.Location = new System.Drawing.Point(130, 69);
      this.escapeCheckBox.Name = "escapeCheckBox";
      this.escapeCheckBox.Size = new System.Drawing.Size(110, 17);
      this.escapeCheckBox.TabIndex = 8;
      this.escapeCheckBox.Text = "use escape chars";
      this.escapeCheckBox.UseVisualStyleBackColor = true;
      this.escapeCheckBox.CheckedChanged += new System.EventHandler(this.escapeCheckBox_CheckedChanged);
      // 
      // commentCharTextBox
      // 
      this.commentCharTextBox.Location = new System.Drawing.Point(95, 94);
      this.commentCharTextBox.Name = "commentCharTextBox";
      this.commentCharTextBox.Size = new System.Drawing.Size(28, 20);
      this.commentCharTextBox.TabIndex = 9;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(12, 97);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(78, 13);
      this.label4.TabIndex = 10;
      this.label4.Text = "Comment char:";
      // 
      // fieldNamesCheckBox
      // 
      this.fieldNamesCheckBox.AutoSize = true;
      this.fieldNamesCheckBox.Location = new System.Drawing.Point(15, 131);
      this.fieldNamesCheckBox.Name = "fieldNamesCheckBox";
      this.fieldNamesCheckBox.Size = new System.Drawing.Size(163, 17);
      this.fieldNamesCheckBox.TabIndex = 11;
      this.fieldNamesCheckBox.Text = "First line contains field names";
      this.fieldNamesCheckBox.UseVisualStyleBackColor = true;
      // 
      // CsvColumnizerConfigDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(304, 210);
      this.ControlBox = false;
      this.Controls.Add(this.fieldNamesCheckBox);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.commentCharTextBox);
      this.Controls.Add(this.escapeCheckBox);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.escapeCharTextBox);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.quoteCharTextBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.delimiterTextBox);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.MaximizeBox = false;
      this.Name = "CsvColumnizerConfigDlg";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "CSV Columnizer Configuration";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.TextBox delimiterTextBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox quoteCharTextBox;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox escapeCharTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.CheckBox escapeCheckBox;
    private System.Windows.Forms.TextBox commentCharTextBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox fieldNamesCheckBox;
  }
}