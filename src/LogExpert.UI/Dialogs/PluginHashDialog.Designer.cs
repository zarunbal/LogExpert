namespace LogExpert.UI.Dialogs;

partial class PluginHashDialog
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
        this.labelPluginName = new System.Windows.Forms.Label();
        this.labelHash = new System.Windows.Forms.Label();
        this.textBoxHash = new System.Windows.Forms.TextBox();
        this.buttonCopy = new System.Windows.Forms.Button();
        this.buttonClose = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // pluginNameLabel
        // 
        this.labelPluginName.AutoSize = true;
        this.labelPluginName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.labelPluginName.Location = new System.Drawing.Point(15, 20);
        this.labelPluginName.Name = "labelPluginName";
        this.labelPluginName.Size = new System.Drawing.Size(100, 20);
        this.labelPluginName.TabIndex = 0;
        this.labelPluginName.Text = "Plugin: ";
        // 
        // hashLabel
        // 
        this.labelHash.AutoSize = true;
        this.labelHash.Location = new System.Drawing.Point(15, 50);
        this.labelHash.Name = "labelHash";
        this.labelHash.Size = new System.Drawing.Size(100, 20);
        this.labelHash.TabIndex = 1;
        this.labelHash.Text = "SHA256 Hash:";
        // 
        // hashTextBox
        // 
        this.textBoxHash.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.textBoxHash.Font = new System.Drawing.Font("Consolas", 9F);
        this.textBoxHash.Location = new System.Drawing.Point(15, 75);
        this.textBoxHash.Multiline = true;
        this.textBoxHash.Name = "textBoxHash";
        this.textBoxHash.ReadOnly = true;
        this.textBoxHash.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.textBoxHash.Size = new System.Drawing.Size(530, 100);
        this.textBoxHash.TabIndex = 2;
        this.textBoxHash.WordWrap = true;
        // 
        // copyButton
        // 
        this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.buttonCopy.Location = new System.Drawing.Point(375, 190);
        this.buttonCopy.Name = "buttonCopy";
        this.buttonCopy.Size = new System.Drawing.Size(80, 32);
        this.buttonCopy.TabIndex = 3;
        this.buttonCopy.Text = "&Copy";
        this.buttonCopy.UseVisualStyleBackColor = true;
        this.buttonCopy.Click += new System.EventHandler(this.OnButtonCopyClick);
        // 
        // closeButton
        // 
        this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.buttonClose.Location = new System.Drawing.Point(465, 190);
        this.buttonClose.Name = "buttonClose";
        this.buttonClose.Size = new System.Drawing.Size(80, 32);
        this.buttonClose.TabIndex = 4;
        this.buttonClose.Text = "&Close";
        this.buttonClose.UseVisualStyleBackColor = true;
        this.buttonClose.Click += new System.EventHandler(this.OnButtonCloseClick);
        // 
        // PluginHashDialog
        // 
        this.AcceptButton = this.buttonClose;
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(560, 235);
        this.Controls.Add(this.buttonClose);
        this.Controls.Add(this.buttonCopy);
        this.Controls.Add(this.textBoxHash);
        this.Controls.Add(this.labelHash);
        this.Controls.Add(this.labelPluginName);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "PluginHashDialog";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Plugin Hash";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label labelPluginName;
    private System.Windows.Forms.Label labelHash;
    private System.Windows.Forms.TextBox textBoxHash;
    private System.Windows.Forms.Button buttonCopy;
    private System.Windows.Forms.Button buttonClose;
}
