namespace LogExpert.Dialogs
{
  partial class KeywordActionDlg
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeywordActionDlg));
      this.actionComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.parameterTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.commentTextBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // actionComboBox
      // 
      this.actionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.actionComboBox.FormattingEnabled = true;
      this.actionComboBox.Location = new System.Drawing.Point(12, 36);
      this.actionComboBox.Name = "actionComboBox";
      this.actionComboBox.Size = new System.Drawing.Size(329, 21);
      this.actionComboBox.TabIndex = 0;
      this.actionComboBox.SelectedIndexChanged += new System.EventHandler(this.actionComboBox_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 20);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(114, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Keyword action plugin:";
      // 
      // parameterTextBox
      // 
      this.parameterTextBox.Location = new System.Drawing.Point(12, 188);
      this.parameterTextBox.Name = "parameterTextBox";
      this.parameterTextBox.Size = new System.Drawing.Size(331, 20);
      this.parameterTextBox.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 172);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(55, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Parameter";
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(166, 223);
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
      this.cancelButton.Location = new System.Drawing.Point(264, 223);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 5;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // commentTextBox
      // 
      this.commentTextBox.Location = new System.Drawing.Point(15, 64);
      this.commentTextBox.Multiline = true;
      this.commentTextBox.Name = "commentTextBox";
      this.commentTextBox.ReadOnly = true;
      this.commentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.commentTextBox.Size = new System.Drawing.Size(326, 95);
      this.commentTextBox.TabIndex = 6;
      // 
      // KeywordActionDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(353, 258);
      this.Controls.Add(this.commentTextBox);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.parameterTextBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.actionComboBox);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "KeywordActionDlg";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Keyword Action";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox actionComboBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox parameterTextBox;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.TextBox commentTextBox;
  }
}