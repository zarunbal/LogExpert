namespace LogExpert
{
  partial class DebugWindow
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
      this.closeButton = new System.Windows.Forms.Button();
      this.listBox = new System.Windows.Forms.ListBox();
      this.clearButton = new System.Windows.Forms.Button();
      this.levelComboBox = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // closeButton
      // 
      this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.closeButton.Location = new System.Drawing.Point(424, 286);
      this.closeButton.Name = "closeButton";
      this.closeButton.Size = new System.Drawing.Size(75, 23);
      this.closeButton.TabIndex = 0;
      this.closeButton.Text = "Close";
      this.closeButton.UseVisualStyleBackColor = true;
      this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
      // 
      // listBox
      // 
      this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.listBox.FormattingEnabled = true;
      this.listBox.HorizontalScrollbar = true;
      this.listBox.ItemHeight = 14;
      this.listBox.Location = new System.Drawing.Point(13, 13);
      this.listBox.Name = "listBox";
      this.listBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
      this.listBox.Size = new System.Drawing.Size(486, 242);
      this.listBox.TabIndex = 1;
      // 
      // clearButton
      // 
      this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.clearButton.Location = new System.Drawing.Point(311, 286);
      this.clearButton.Name = "clearButton";
      this.clearButton.Size = new System.Drawing.Size(75, 23);
      this.clearButton.TabIndex = 2;
      this.clearButton.Text = "Clear";
      this.clearButton.UseVisualStyleBackColor = true;
      this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
      // 
      // levelComboBox
      // 
      this.levelComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.levelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.levelComboBox.FormattingEnabled = true;
      this.levelComboBox.Location = new System.Drawing.Point(13, 286);
      this.levelComboBox.Name = "levelComboBox";
      this.levelComboBox.Size = new System.Drawing.Size(121, 21);
      this.levelComboBox.TabIndex = 3;
      this.levelComboBox.SelectedIndexChanged += new System.EventHandler(this.levelComboBox_SelectedIndexChanged);
      // 
      // DebugWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(511, 321);
      this.ControlBox = false;
      this.Controls.Add(this.levelComboBox);
      this.Controls.Add(this.clearButton);
      this.Controls.Add(this.listBox);
      this.Controls.Add(this.closeButton);
      this.MaximizeBox = false;
      this.Name = "DebugWindow";
      this.Text = "DebugWindow";
      this.Load += new System.EventHandler(this.DebugWindow_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button closeButton;
    private System.Windows.Forms.ListBox listBox;
    private System.Windows.Forms.Button clearButton;
    private System.Windows.Forms.ComboBox levelComboBox;
  }
}