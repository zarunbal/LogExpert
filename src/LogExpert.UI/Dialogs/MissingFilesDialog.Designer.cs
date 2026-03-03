using System.Runtime.Versioning;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
partial class MissingFilesDialog
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
        if (disposing)
        {
            components?.Dispose();
            imageListStatus?.Dispose();
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
        components = new System.ComponentModel.Container();
        
        listViewFiles = new ListView();
        columnFileName = new ColumnHeader();
        columnStatus = new ColumnHeader();
        columnPath = new ColumnHeader();
        buttonLoadAndUpdate = new Button();
        buttonLoad = new Button();
        buttonBrowse = new Button();
        buttonCancel = new Button();
        labelInfo = new Label();
        labelSummary = new Label();
        imageListStatus = new ImageList(components);
        panelButtons = new Panel();
        panelTop = new Panel();
        panelLayoutOptions = new Panel();
        labelLayoutInfo = new Label();
        radioButtonCloseTabs = new RadioButton();
        radioButtonNewWindow = new RadioButton();
        radioButtonIgnoreLayout = new RadioButton();
        buttonLayoutPanel = new FlowLayoutPanel();
        
        SuspendLayout();
        panelLayoutOptions.SuspendLayout();
        panelTop.SuspendLayout();
        panelButtons.SuspendLayout();
        
        // 
        // imageListStatus
        // 
        imageListStatus.ColorDepth = ColorDepth.Depth32Bit;
        imageListStatus.ImageSize = new Size(16, 16);
        CreateStatusIcons();
        
        // 
        // panelTop
        // 
        panelTop.Controls.Add(labelSummary);
        panelTop.Controls.Add(labelInfo);
        panelTop.Dock = DockStyle.Top;
        panelTop.Height = 80;
        panelTop.Padding = new Padding(10);
        panelTop.TabIndex = 0;
        
        // 
        // labelInfo
        // 
        labelInfo.AutoSize = false;
        labelInfo.Dock = DockStyle.Top;
        labelInfo.Height = 40;
        labelInfo.Text = "Some files from the session could not be found. You can browse for missing files or load only the files that were found.";
        labelInfo.TextAlign = ContentAlignment.MiddleLeft;
        labelInfo.TabIndex = 0;
        
        // 
        // labelSummary
        // 
        labelSummary.AutoSize = false;
        labelSummary.Dock = DockStyle.Top;
        labelSummary.Font = new Font(Font, FontStyle.Bold);
        labelSummary.Height = 30;
        labelSummary.TextAlign = ContentAlignment.MiddleLeft;
        labelSummary.TabIndex = 1;
        
        // 
        // labelLayoutInfo
        // 
        labelLayoutInfo.AutoSize = false;
        labelLayoutInfo.Location = new Point(10, 5);
        labelLayoutInfo.Size = new Size(400, 25);
        labelLayoutInfo.Text = "This session contains layout data. How would you like to proceed?";
        labelLayoutInfo.TextAlign = ContentAlignment.MiddleLeft;
        labelLayoutInfo.TabIndex = 0;
        
        // 
        // radioButtonCloseTabs
        // 
        radioButtonCloseTabs.AutoSize = true;
        radioButtonCloseTabs.Checked = true;
        radioButtonCloseTabs.Location = new Point(10, 35);
        radioButtonCloseTabs.Name = "radioButtonCloseTabs";
        radioButtonCloseTabs.Size = new Size(200, 24);
        radioButtonCloseTabs.TabIndex = 1;
        radioButtonCloseTabs.TabStop = true;
        radioButtonCloseTabs.Text = "Close existing tabs and restore layout";
        radioButtonCloseTabs.UseVisualStyleBackColor = true;
        
        // 
        // radioButtonNewWindow
        // 
        radioButtonNewWindow.AutoSize = true;
        radioButtonNewWindow.Location = new Point(10, 60);
        radioButtonNewWindow.Name = "radioButtonNewWindow";
        radioButtonNewWindow.Size = new Size(200, 24);
        radioButtonNewWindow.TabIndex = 2;
        radioButtonNewWindow.Text = "Open in a new window";
        radioButtonNewWindow.UseVisualStyleBackColor = true;
        
        // 
        // radioButtonIgnoreLayout
        // 
        radioButtonIgnoreLayout.AutoSize = true;
        radioButtonIgnoreLayout.Location = new Point(10, 85);
        radioButtonIgnoreLayout.Name = "radioButtonIgnoreLayout";
        radioButtonIgnoreLayout.Size = new Size(200, 24);
        radioButtonIgnoreLayout.TabIndex = 3;
        radioButtonIgnoreLayout.Text = "Ignore layout data";
        radioButtonIgnoreLayout.UseVisualStyleBackColor = true;
        
        // 
        // panelLayoutOptions
        // 
        panelLayoutOptions.Controls.Add(radioButtonIgnoreLayout);
        panelLayoutOptions.Controls.Add(radioButtonNewWindow);
        panelLayoutOptions.Controls.Add(radioButtonCloseTabs);
        panelLayoutOptions.Controls.Add(labelLayoutInfo);
        panelLayoutOptions.Dock = DockStyle.Bottom;
        panelLayoutOptions.Height = 115;
        panelLayoutOptions.TabIndex = 3;
        panelLayoutOptions.Visible = false;
        
        // 
        // listViewFiles
        // 
        listViewFiles.Columns.AddRange(columnFileName, columnStatus, columnPath);
        listViewFiles.Dock = DockStyle.Fill;
        listViewFiles.FullRowSelect = true;
        listViewFiles.GridLines = true;
        listViewFiles.MultiSelect = false;
        listViewFiles.SmallImageList = imageListStatus;
        listViewFiles.TabIndex = 1;
        listViewFiles.View = View.Details;
        listViewFiles.SelectedIndexChanged += OnListViewSelectedIndexChanged;
        listViewFiles.DoubleClick += OnListViewDoubleClick;
        
        // 
        // columnFileName
        // 
        columnFileName.Text = "File Name";
        columnFileName.Width = 200;
        
        // 
        // columnStatus
        // 
        columnStatus.Text = "Status";
        columnStatus.Width = 150;
        
        // 
        // columnPath
        // 
        columnPath.Text = "Path";
        columnPath.Width = 400;
        
        // 
        // buttonLoad
        // 
        buttonLoad.AutoSize = true;
        buttonLoad.Height = 30;
        buttonLoad.Margin = new Padding(3);
        buttonLoad.MinimumSize = new Size(100, 30);
        buttonLoad.TabIndex = 0;
        buttonLoad.Text = "Load Files";
        buttonLoad.UseVisualStyleBackColor = true;
        buttonLoad.Click += OnButtonLoadClick;
        
        // 
        // buttonBrowse
        // 
        buttonBrowse.AutoSize = true;
        buttonBrowse.Enabled = false;
        buttonBrowse.Height = 30;
        buttonBrowse.Margin = new Padding(3);
        buttonBrowse.MinimumSize = new Size(100, 30);
        buttonBrowse.TabIndex = 1;
        buttonBrowse.Text = "Browse...";
        buttonBrowse.UseVisualStyleBackColor = true;
        buttonBrowse.Click += OnButtonBrowseClick;
        
        // 
        // buttonLoadAndUpdate
        // 
        buttonLoadAndUpdate.AutoSize = true;
        buttonLoadAndUpdate.Enabled = false;
        buttonLoadAndUpdate.Height = 30;
        buttonLoadAndUpdate.Margin = new Padding(3);
        buttonLoadAndUpdate.MinimumSize = new Size(150, 30);
        buttonLoadAndUpdate.TabIndex = 2;
        buttonLoadAndUpdate.Text = "Load && Update Session";
        buttonLoadAndUpdate.UseVisualStyleBackColor = true;
        buttonLoadAndUpdate.Click += OnButtonLoadAndUpdateClick;
        
        // 
        // buttonCancel
        // 
        buttonCancel.AutoSize = true;
        buttonCancel.DialogResult = DialogResult.Cancel;
        buttonCancel.Height = 30;
        buttonCancel.Margin = new Padding(3);
        buttonCancel.MinimumSize = new Size(100, 30);
        buttonCancel.TabIndex = 3;
        buttonCancel.Text = "Cancel";
        buttonCancel.UseVisualStyleBackColor = true;
        buttonCancel.Click += OnButtonCancelClick;
        
        // 
        // buttonLayoutPanel
        // 
        buttonLayoutPanel.AutoSize = true;
        buttonLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        buttonLayoutPanel.Controls.Add(buttonLoad);
        buttonLayoutPanel.Controls.Add(buttonBrowse);
        buttonLayoutPanel.Controls.Add(buttonLoadAndUpdate);
        buttonLayoutPanel.Controls.Add(buttonCancel);
        buttonLayoutPanel.Dock = DockStyle.Right;
        buttonLayoutPanel.FlowDirection = FlowDirection.LeftToRight;
        buttonLayoutPanel.Location = new Point(0, 10);
        buttonLayoutPanel.Padding = new Padding(10, 10, 10, 10);
        buttonLayoutPanel.TabIndex = 0;
        buttonLayoutPanel.WrapContents = false;
        
        // 
        // panelButtons
        // 
        panelButtons.Controls.Add(buttonLayoutPanel);
        panelButtons.Dock = DockStyle.Bottom;
        panelButtons.Height = 60;
        panelButtons.TabIndex = 2;
        
        // 
        // MissingFilesDialog
        // 
        AcceptButton = buttonLoad;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        CancelButton = buttonCancel;
        ClientSize = new Size(840, 500);
        Controls.Add(listViewFiles);       
        Controls.Add(panelLayoutOptions);  
        Controls.Add(panelButtons);        
        Controls.Add(panelTop);            
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(600, 400);
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Missing Files";
        
        panelLayoutOptions.ResumeLayout(false);
        panelLayoutOptions.PerformLayout();
        panelTop.ResumeLayout(false);
        panelButtons.ResumeLayout(false);
        panelButtons.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private ListView listViewFiles;
    private ColumnHeader columnFileName;
    private ColumnHeader columnStatus;
    private ColumnHeader columnPath;
    private Button buttonLoad;
    private Button buttonLoadAndUpdate;
    private Button buttonBrowse;
    private Button buttonCancel;
    private Label labelInfo;
    private Label labelSummary;
    private ImageList imageListStatus;
    private Panel panelButtons;
    private Panel panelTop;
    private Panel panelLayoutOptions;
    private Label labelLayoutInfo;
    private RadioButton radioButtonCloseTabs;
    private RadioButton radioButtonNewWindow;
    private RadioButton radioButtonIgnoreLayout;
    private FlowLayoutPanel buttonLayoutPanel;
}
