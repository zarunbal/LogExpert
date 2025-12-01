namespace LogExpert.UI.Dialogs;

partial class PluginTrustDialog
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
        this.pluginListView = new System.Windows.Forms.ListView();
        this.columnName = new System.Windows.Forms.ColumnHeader();
        this.columnHashVerified = new System.Windows.Forms.ColumnHeader();
        this.columnHashPartial = new System.Windows.Forms.ColumnHeader();
        this.columnStatus = new System.Windows.Forms.ColumnHeader();
        this.addPluginButton = new System.Windows.Forms.Button();
        this.removePluginButton = new System.Windows.Forms.Button();
        this.viewHashButton = new System.Windows.Forms.Button();
        this.saveButton = new System.Windows.Forms.Button();
        this.cancelButton = new System.Windows.Forms.Button();
        this.pluginCountLabel = new System.Windows.Forms.Label();
        this.groupBoxPlugins = new System.Windows.Forms.GroupBox();
        this.groupBoxPlugins.SuspendLayout();
        this.SuspendLayout();
        // 
        // pluginListView
        // 
        this.pluginListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.pluginListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.columnName,
        this.columnHashVerified,
        this.columnHashPartial,
        this.columnStatus});
        this.pluginListView.FullRowSelect = true;
        this.pluginListView.GridLines = true;
        this.pluginListView.HideSelection = false;
        this.pluginListView.Location = new System.Drawing.Point(15, 55);
        this.pluginListView.MultiSelect = false;
        this.pluginListView.Name = "pluginListView";
        this.pluginListView.Size = new System.Drawing.Size(640, 320);
        this.pluginListView.TabIndex = 0;
        this.pluginListView.UseCompatibleStateImageBehavior = false;
        this.pluginListView.View = System.Windows.Forms.View.Details;
        this.pluginListView.SelectedIndexChanged += new System.EventHandler(this.PluginListView_SelectedIndexChanged);
        // 
        // columnName
        // 
        this.columnName.Text = "Plugin Name";
        this.columnName.Width = 250;
        // 
        // columnHashVerified
        // 
        this.columnHashVerified.Text = "Hash Verified";
        this.columnHashVerified.Width = 100;
        // 
        // columnHashPartial
        // 
        this.columnHashPartial.Text = "Hash (Partial)";
        this.columnHashPartial.Width = 180;
        // 
        // columnStatus
        // 
        this.columnStatus.Text = "Status";
        this.columnStatus.Width = 100;
        // 
        // addPluginButton
        // 
        this.addPluginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.addPluginButton.Location = new System.Drawing.Point(15, 385);
        this.addPluginButton.Name = "addPluginButton";
        this.addPluginButton.Size = new System.Drawing.Size(120, 32);
        this.addPluginButton.TabIndex = 1;
        this.addPluginButton.Text = "&Add Plugin...";
        this.addPluginButton.UseVisualStyleBackColor = true;
        this.addPluginButton.Click += new System.EventHandler(this.AddPluginButton_Click);
        // 
        // removePluginButton
        // 
        this.removePluginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.removePluginButton.Enabled = false;
        this.removePluginButton.Location = new System.Drawing.Point(145, 385);
        this.removePluginButton.Name = "removePluginButton";
        this.removePluginButton.Size = new System.Drawing.Size(100, 32);
        this.removePluginButton.TabIndex = 2;
        this.removePluginButton.Text = "&Remove";
        this.removePluginButton.UseVisualStyleBackColor = true;
        this.removePluginButton.Click += new System.EventHandler(this.RemovePluginButton_Click);
        // 
        // viewHashButton
        // 
        this.viewHashButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.viewHashButton.Enabled = false;
        this.viewHashButton.Location = new System.Drawing.Point(255, 385);
        this.viewHashButton.Name = "viewHashButton";
        this.viewHashButton.Size = new System.Drawing.Size(120, 32);
        this.viewHashButton.TabIndex = 3;
        this.viewHashButton.Text = "&View Hash...";
        this.viewHashButton.UseVisualStyleBackColor = true;
        this.viewHashButton.Click += new System.EventHandler(this.ViewHashButton_Click);
        // 
        // saveButton
        // 
        this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.saveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.saveButton.Location = new System.Drawing.Point(485, 432);
        this.saveButton.Name = "saveButton";
        this.saveButton.Size = new System.Drawing.Size(80, 32);
        this.saveButton.TabIndex = 4;
        this.saveButton.Text = "&Save";
        this.saveButton.UseVisualStyleBackColor = true;
        this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
        // 
        // cancelButton
        // 
        this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.cancelButton.Location = new System.Drawing.Point(575, 432);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Size = new System.Drawing.Size(80, 32);
        this.cancelButton.TabIndex = 5;
        this.cancelButton.Text = "&Cancel";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
        // 
        // pluginCountLabel
        // 
        this.pluginCountLabel.AutoSize = true;
        this.pluginCountLabel.Location = new System.Drawing.Point(15, 25);
        this.pluginCountLabel.Name = "pluginCountLabel";
        this.pluginCountLabel.Size = new System.Drawing.Size(120, 20);
        this.pluginCountLabel.TabIndex = 6;
        this.pluginCountLabel.Text = "Total Plugins: 0";
        // 
        // groupBoxPlugins
        // 
        this.groupBoxPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBoxPlugins.Location = new System.Drawing.Point(5, 5);
        this.groupBoxPlugins.Name = "groupBoxPlugins";
        this.groupBoxPlugins.Size = new System.Drawing.Size(660, 415);
        this.groupBoxPlugins.TabIndex = 7;
        this.groupBoxPlugins.TabStop = false;
        this.groupBoxPlugins.Text = "Trusted Plugins";
        // 
        // PluginTrustDialog
        // 
        this.AcceptButton = this.saveButton;
        this.CancelButton = this.cancelButton;
        this.ClientSize = new System.Drawing.Size(670, 475);
        this.Controls.Add(this.pluginCountLabel);
        this.Controls.Add(this.cancelButton);
        this.Controls.Add(this.saveButton);
        this.Controls.Add(this.viewHashButton);
        this.Controls.Add(this.removePluginButton);
        this.Controls.Add(this.addPluginButton);
        this.Controls.Add(this.pluginListView);
        this.Controls.Add(this.groupBoxPlugins);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(600, 400);
        this.Name = "PluginTrustDialog";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Plugin Trust Management";
        this.groupBoxPlugins.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.ListView pluginListView;
    private System.Windows.Forms.ColumnHeader columnName;
    private System.Windows.Forms.ColumnHeader columnHashVerified;
    private System.Windows.Forms.ColumnHeader columnHashPartial;
    private System.Windows.Forms.ColumnHeader columnStatus;
    private System.Windows.Forms.Button addPluginButton;
    private System.Windows.Forms.Button removePluginButton;
    private System.Windows.Forms.Button viewHashButton;
    private System.Windows.Forms.Button saveButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Label pluginCountLabel;
    private System.Windows.Forms.GroupBox groupBoxPlugins;
}
