using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace LogExpert.Dialogs
{

  public partial class SettingsDialog : Form
  {
    class ColumnizerEntry
    {
      ILogLineColumnizer columnizer;

      public ColumnizerEntry(ILogLineColumnizer columnizer)
      {
        this.columnizer = columnizer;
      }

      public string Name
      {
        get { return this.columnizer.GetName(); }
      }

      public ILogLineColumnizer Columnizer
      {
        get { return this.columnizer; }
      }
      
    }

    Preferences preferences;
    LogTabWindow logTabWin;

    ILogExpertPluginConfigurator selectedPlugin = null;
    ToolEntry selectedTool = null;
    Image emptyImage = new Bitmap(16, 16);

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);


    public SettingsDialog(Preferences prefs, LogTabWindow logTabWin)
    {
      this.preferences = prefs;
      this.logTabWin = logTabWin;
      InitializeComponent();
    }

    public SettingsDialog(Preferences prefs, LogTabWindow logTabWin, int tabToOpen)
      :this(prefs, logTabWin)
    {
      this.tabControl1.SelectedIndex = tabToOpen;
    }

    public Preferences Preferences
    {
      get { return this.preferences; }
    }

    private void SettingsDialog_Load(object sender, EventArgs e)
    {
      FillDialog();
    }

    private void FillDialog()
    {
      if (this.preferences == null)
        this.preferences = new Preferences();
      if (this.preferences.fontName == null)
        this.preferences.fontName = "Courier New";
      if (this.preferences.fontSize == 0.0)
        this.preferences.fontSize = 9.0f;
      this.timestampCheckBox.Checked = this.preferences.timestampControl;
      this.syncFilterCheckBox.Checked = this.preferences.filterSync;
      this.filterTailCheckBox.Checked = this.preferences.filterTail;
      this.followTailCheckBox.Checked = this.preferences.followTail;
      this.horizRadioButton.Checked = this.preferences.timestampControlDragOrientation == DateTimeDragControl.DragOrientations.Horizontal;
      this.verticalRadioButton.Checked = this.preferences.timestampControlDragOrientation == DateTimeDragControl.DragOrientations.Vertical;
      this.verticalInvRadioButton.Checked = this.preferences.timestampControlDragOrientation == DateTimeDragControl.DragOrientations.InvertedVertical;

      this.singleInstanceCheckBox.Checked = this.preferences.allowOnlyOneInstance;
      this.openLastFilesCheckBox.Checked = this.preferences.openLastFiles;
      this.tailStateCheckBox.Checked = this.preferences.showTailState;
      this.columnSizeCheckBox.Checked = this.preferences.setLastColumnWidth;
      this.columnWidthUpDown.Enabled = this.preferences.setLastColumnWidth;
      if (this.preferences.lastColumnWidth != 0)
      {
        if (this.preferences.lastColumnWidth < this.columnWidthUpDown.Minimum)
          this.preferences.lastColumnWidth = (int)this.columnWidthUpDown.Minimum;
        if (this.preferences.lastColumnWidth > this.columnWidthUpDown.Maximum)
          this.preferences.lastColumnWidth = (int)this.columnWidthUpDown.Maximum;
        this.columnWidthUpDown.Value = this.preferences.lastColumnWidth;
      }
      this.timeSpreadCheckBox.Checked = this.preferences.showTimeSpread;
      this.reverseAlphaCheckBox.Checked = this.preferences.reverseAlpha;
      this.timeViewRadioButton.Checked = this.preferences.timeSpreadTimeMode;
      this.lineViewRadioButton.Checked = !this.preferences.timeSpreadTimeMode;

      this.saveSessionsCheckBox.Checked = this.preferences.saveSessions;
      switch (this.preferences.saveLocation)
      {
        case SessionSaveLocation.OwnDir:
          this.sessionSaveRadioOwn.Checked = true;
          break;
        case SessionSaveLocation.SameDir:
          this.sessionRadioSameDir.Checked = true;
          break;
        case SessionSaveLocation.DocumentsDir:
          this.sessionSaveRadioDocuments.Checked = true;
          break;
      }
      this.sessionSaveOwnDirLabel.Text = this.preferences.saveDirectory != null ? this.preferences.saveDirectory : "";
      this.saveFilterCheckBox.Checked = this.preferences.saveFilters;
      this.blockCountUpDown.Value = this.preferences.bufferCount;
      this.linesPerBlockUpDown.Value = this.preferences.linesPerBuffer;
      this.pollingIntervalUpDown.Value = this.preferences.pollingInterval;
      this.multiThreadCheckBox.Checked = this.preferences.multiThreadFilter;

      this.columnizerDataGridView.DataError += new DataGridViewDataErrorEventHandler(columnizerDataGridView_DataError);

      FillColumnizerList();
      FillPluginList();
      DisplayFontName();
      FillHighlightMaskList();
      FillToolListbox();
      FillMultifileSettings();
      FillEncodingList();
      this.encodingComboBox.SelectedItem = Encoding.GetEncoding(this.preferences.defaultEncoding);
      this.maskPrioCheckBox.Checked = this.preferences.maskPrio;
      this.columnFinderCheckBox.Checked = this.preferences.showColumnFinder;
      this.legacyReaderCheckBox.Checked = this.preferences.useLegacyReader;
    }


    private string NotNull(string text)
    {
      if (text == null)
        return "";
      else
        return text;
    }

    private void DisplayFontName()
    {
      this.fontLabel.Text = this.preferences.fontName + " " + (int)this.preferences.fontSize;
      this.fontLabel.Font = new Font(new FontFamily(this.preferences.fontName), this.preferences.fontSize);
    }



    private void changeFontButton_Click(object sender, EventArgs e)
    {
      FontDialog dlg = new FontDialog();
      dlg.ShowEffects = false;
      dlg.AllowVerticalFonts = false;
      dlg.AllowScriptChange = false;
      dlg.Font = new Font(new FontFamily(this.preferences.fontName), this.preferences.fontSize);
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        this.preferences.fontSize = dlg.Font.Size;
        this.preferences.fontName = dlg.Font.FontFamily.Name;
      }
      DisplayFontName();
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      this.preferences.timestampControl = this.timestampCheckBox.Checked;
      this.preferences.filterSync = this.syncFilterCheckBox.Checked;
      this.preferences.filterTail = this.filterTailCheckBox.Checked;
      this.preferences.followTail = this.followTailCheckBox.Checked;
      if (this.verticalRadioButton.Checked)
        this.preferences.timestampControlDragOrientation = DateTimeDragControl.DragOrientations.Vertical;
      else if (this.verticalInvRadioButton.Checked)
        this.preferences.timestampControlDragOrientation = DateTimeDragControl.DragOrientations.InvertedVertical;
      else
        this.preferences.timestampControlDragOrientation = DateTimeDragControl.DragOrientations.Horizontal;
      SaveColumnizerList();
      this.preferences.maskPrio = this.maskPrioCheckBox.Checked;
      this.preferences.askForClose = this.askCloseTabsCheckBox.Checked;
      this.preferences.allowOnlyOneInstance = this.singleInstanceCheckBox.Checked;
      this.preferences.openLastFiles = this.openLastFilesCheckBox.Checked;
      this.preferences.showTailState = this.tailStateCheckBox.Checked;
      this.preferences.setLastColumnWidth = this.columnSizeCheckBox.Checked;
      this.preferences.lastColumnWidth = (int)this.columnWidthUpDown.Value;
      this.preferences.showTimeSpread = this.timeSpreadCheckBox.Checked;
      this.preferences.reverseAlpha = this.reverseAlphaCheckBox.Checked;
      this.preferences.timeSpreadTimeMode = this.timeViewRadioButton.Checked;

      this.preferences.saveSessions = this.saveSessionsCheckBox.Checked;
      this.preferences.saveDirectory = this.sessionSaveOwnDirLabel.Text;
      if (this.sessionSaveRadioDocuments.Checked)
        this.preferences.saveLocation = SessionSaveLocation.DocumentsDir;
      else if (this.sessionSaveRadioOwn.Checked)
        this.preferences.saveLocation = SessionSaveLocation.OwnDir;
      else
        this.preferences.saveLocation = SessionSaveLocation.SameDir;
      this.preferences.saveFilters = this.saveFilterCheckBox.Checked;
      this.preferences.bufferCount = (int)this.blockCountUpDown.Value;
      this.preferences.linesPerBuffer = (int)this.linesPerBlockUpDown.Value;
      this.preferences.pollingInterval = (int)this.pollingIntervalUpDown.Value;
      this.preferences.multiThreadFilter = this.multiThreadCheckBox.Checked;
      this.preferences.defaultEncoding = this.encodingComboBox.SelectedItem != null
                                           ? (this.encodingComboBox.SelectedItem as Encoding).HeaderName
                                           : Encoding.Default.HeaderName;
      this.preferences.showColumnFinder = this.columnFinderCheckBox.Checked;
      this.preferences.useLegacyReader = this.legacyReaderCheckBox.Checked;
      SavePluginSettings();
      SaveHighlightMaskList();
      GetToolListBoxData();
      SaveMultifileData();
    }

    private void SaveMultifileData()
    {
      if (this.multiOpenRadioButton1.Checked)
        this.preferences.multiFileOption = MultiFileOption.SingleFiles;
      if (this.multiOpenRadioButton2.Checked)
        this.preferences.multiFileOption = MultiFileOption.MultiFile;
      if (this.multiOpenRadioButton3.Checked)
        this.preferences.multiFileOption = MultiFileOption.Ask;
      this.preferences.multifileOptions.FormatPattern = this.multifilePattern.Text;
      this.preferences.multifileOptions.MaxDayTry = (int)this.multifileDays.Value;
    }


    private void ToolButtonClick(TextBox textBox)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      if (textBox.Text != null && textBox.Text.Length > 0)
      {
        FileInfo info = new FileInfo(textBox.Text);
        if (info.Directory.Exists)
        {
          dlg.InitialDirectory = info.DirectoryName;
        }
      }
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBox.Text = dlg.FileName;
      }
    }

    private void ArgsButtonClick(TextBox textBox)
    {
      ToolArgsDialog dlg = new ToolArgsDialog(this.logTabWin, this);
      dlg.Arg = textBox.Text;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBox.Text = dlg.Arg;
      }
    
    }

    private void WorkingDirButtonClick(TextBox textBox)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.RootFolder = Environment.SpecialFolder.MyComputer;
      dlg.Description = "Select a working directory";
      if (textBox.Text != null && textBox.Text.Length > 0)
      {
        DirectoryInfo info = new DirectoryInfo(textBox.Text);
        if (info.Exists)
        {
          dlg.SelectedPath = info.FullName;
        }
      }
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBox.Text = dlg.SelectedPath;
      }
    }



    private void toolButtonA_Click(object sender, EventArgs e)
    {
      ToolButtonClick(cmdTextBox);
    }

    private void argButtonA_Click(object sender, EventArgs e)
    {
      ArgsButtonClick(this.argsTextBox);
    }


    private void addButton_Click(object sender, EventArgs e)
    {
      this.columnizerDataGridView.Rows.Add();
    }

    private void columnizerDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
      DataGridViewComboBoxCell comboCell = (DataGridViewComboBoxCell)this.columnizerDataGridView.Rows[e.RowIndex].Cells[1];
      if (comboCell.Items.Count > 0)
      {
//        comboCell.Value = comboCell.Items[0];
      }

    }

    private void deleteButton_Click(object sender, EventArgs e)
    {
      if (this.columnizerDataGridView.CurrentRow != null && !this.columnizerDataGridView.CurrentRow.IsNewRow)
      {
        int index = this.columnizerDataGridView.CurrentRow.Index;
        this.columnizerDataGridView.EndEdit();
        this.columnizerDataGridView.Rows.RemoveAt(index);
      }
    }

    private void FillColumnizerForToolsList()
    {
      if (this.selectedTool != null)
      {
        FillColumnizerForToolsList(this.columnizerComboBox, this.selectedTool.columnizerName);
      }
    }


    private void FillColumnizerForToolsList(ComboBox comboBox, string columnizerName)
    {
      int selIndex = 0;
      comboBox.Items.Clear();
      IList<ILogLineColumnizer> columnizers = PluginRegistry.GetInstance().RegisteredColumnizers;
      foreach (ILogLineColumnizer columnizer in columnizers)
      {
        int index = comboBox.Items.Add(columnizer.GetName());
        if (columnizer.GetName().Equals(columnizerName))
          selIndex = index;
      }
      //ILogLineColumnizer columnizer = Util.FindColumnizerByName(columnizerName, this.logTabWin.RegisteredColumnizers);
      //if (columnizer == null)
      //  columnizer = this.logTabWin.RegisteredColumnizers[0];
      comboBox.SelectedIndex = selIndex;
    }

    private void FillColumnizerList()
    {
      this.columnizerDataGridView.Rows.Clear();

      DataGridViewComboBoxColumn comboColumn = (DataGridViewComboBoxColumn)this.columnizerDataGridView.Columns[1];
      comboColumn.Items.Clear();

      DataGridViewTextBoxColumn textColumn = (DataGridViewTextBoxColumn)this.columnizerDataGridView.Columns[0];

      IList<ILogLineColumnizer> columnizers = PluginRegistry.GetInstance().RegisteredColumnizers;
      foreach (ILogLineColumnizer columnizer in columnizers)
      {
        int index = comboColumn.Items.Add(columnizer.GetName());
      }
      //comboColumn.DisplayMember = "Name";
      //comboColumn.ValueMember = "Columnizer";

      foreach (ColumnizerMaskEntry maskEntry in this.preferences.columnizerMaskList)
      {
        DataGridViewRow row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewTextBoxCell());
        DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();

        foreach (ILogLineColumnizer logColumnizer in columnizers)
        {
          int index = cell.Items.Add(logColumnizer.GetName());
        }
        
        row.Cells.Add(cell);
        row.Cells[0].Value = maskEntry.mask;
        ILogLineColumnizer columnizer = Util.FindColumnizerByName(maskEntry.columnizerName, PluginRegistry.GetInstance().RegisteredColumnizers);
        if (columnizer == null)
          columnizer = PluginRegistry.GetInstance().RegisteredColumnizers[0];
        row.Cells[1].Value = columnizer.GetName();
        this.columnizerDataGridView.Rows.Add(row);
      }
      int count = this.columnizerDataGridView.RowCount;
      if (count > 0 && !this.columnizerDataGridView.Rows[count - 1].IsNewRow)
      {
        DataGridViewComboBoxCell comboCell = (DataGridViewComboBoxCell)this.columnizerDataGridView.Rows[count - 1].Cells[1];
        comboCell.Value = comboCell.Items[0];
      }
    }

    private void FillHighlightMaskList()
    {
      this.highlightMaskGridView.Rows.Clear();

      DataGridViewComboBoxColumn comboColumn = (DataGridViewComboBoxColumn)this.highlightMaskGridView.Columns[1];
      comboColumn.Items.Clear();

      DataGridViewTextBoxColumn textColumn = (DataGridViewTextBoxColumn)this.highlightMaskGridView.Columns[0];

      IList<HilightGroup> groups = this.logTabWin.HilightGroupList;
      foreach (HilightGroup group in groups)
      {
        int index = comboColumn.Items.Add(group.GroupName);
      }

      foreach (HighlightMaskEntry maskEntry in this.preferences.highlightMaskList)
      {
        DataGridViewRow row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewTextBoxCell());
        DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();

        foreach (HilightGroup group in groups)
        {
          int index = cell.Items.Add(group.GroupName);
        }

        row.Cells.Add(cell);
        row.Cells[0].Value = maskEntry.mask;
        HilightGroup currentGroup = this.logTabWin.FindHighlightGroup(maskEntry.highlightGroupName);
        if (currentGroup == null)
          currentGroup = groups[0];
        if (currentGroup == null)
          currentGroup = new HilightGroup();
        row.Cells[1].Value = currentGroup.GroupName;
        this.highlightMaskGridView.Rows.Add(row);
      }
      int count = this.highlightMaskGridView.RowCount;
      if (count > 0 && !this.highlightMaskGridView.Rows[count - 1].IsNewRow)
      {
        DataGridViewComboBoxCell comboCell = (DataGridViewComboBoxCell)this.highlightMaskGridView.Rows[count - 1].Cells[1];
        comboCell.Value = comboCell.Items[0];
      }
    }



    private void SaveColumnizerList()
    {
      this.preferences.columnizerMaskList.Clear();
      foreach (DataGridViewRow row in this.columnizerDataGridView.Rows)
      {
        if (!row.IsNewRow)
        {
          ColumnizerMaskEntry entry = new ColumnizerMaskEntry();
          entry.mask = (string)row.Cells[0].Value;
          entry.columnizerName = (string)row.Cells[1].Value;
          this.preferences.columnizerMaskList.Add(entry);
        }
      }
    }

    private void SaveHighlightMaskList()
    {
      this.preferences.highlightMaskList.Clear();
      foreach (DataGridViewRow row in this.highlightMaskGridView.Rows)
      {
        if (!row.IsNewRow)
        {
          HighlightMaskEntry entry = new HighlightMaskEntry();
          entry.mask = (string)row.Cells[0].Value;
          entry.highlightGroupName = (string)row.Cells[1].Value;
          this.preferences.highlightMaskList.Add(entry);
        }
      }
    }



    void columnizerDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
      e.Cancel = true;
    }

    private void sysoutCheckBoxA_CheckedChanged(object sender, EventArgs e)
    {
      this.columnizerComboBox.Enabled = this.sysoutCheckBox.Checked;
    }


    private void tailColorButton_Click(object sender, EventArgs e)
    {
      ColorDialog dlg = new ColorDialog();
      dlg.Color = this.preferences.showTailColor;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        this.preferences.showTailColor = dlg.Color;
      }
    }

    private void columnSizeCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      this.columnWidthUpDown.Enabled = this.columnSizeCheckBox.Checked;
    }

    private void timespreadColorButton_Click(object sender, EventArgs e)
    {
      ColorDialog dlg = new ColorDialog();
      dlg.Color = this.preferences.timeSpreadColor;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        this.preferences.timeSpreadColor = dlg.Color;
      }
    }

    private void FillPluginList()
    {
      this.pluginListBox.Items.Clear();
      foreach (IContextMenuEntry entry in PluginRegistry.GetInstance().RegisteredContextMenuPlugins)
      {
        this.pluginListBox.Items.Add(entry);
        if (entry is ILogExpertPluginConfigurator)
        {
          (entry as ILogExpertPluginConfigurator).StartConfig();
        }
      }
      foreach (IKeywordAction entry in PluginRegistry.GetInstance().RegisteredKeywordActions)
      {
        this.pluginListBox.Items.Add(entry);
        if (entry is ILogExpertPluginConfigurator)
        {
          (entry as ILogExpertPluginConfigurator).StartConfig();
        }
      }
      foreach (IFileSystemPlugin entry in PluginRegistry.GetInstance().RegisteredFileSystemPlugins)
      {
        this.pluginListBox.Items.Add(entry);
        if (entry is ILogExpertPluginConfigurator)
        {
          (entry as ILogExpertPluginConfigurator).StartConfig();
        }
      }

      this.configPluginButton.Enabled = false;
    }

    private void SavePluginSettings()
    {
      if (this.selectedPlugin != null)
      {
        this.selectedPlugin.HideConfigForm();
      }
      foreach (IContextMenuEntry entry in PluginRegistry.GetInstance().RegisteredContextMenuPlugins)
      {
        if (entry is ILogExpertPluginConfigurator)
        {
          (entry as ILogExpertPluginConfigurator).SaveConfig(ConfigManager.ConfigDir);
        }
      }
      foreach (IKeywordAction entry in PluginRegistry.GetInstance().RegisteredKeywordActions)
      {
        if (entry is ILogExpertPluginConfigurator)
        {
          (entry as ILogExpertPluginConfigurator).SaveConfig(ConfigManager.ConfigDir);
        }
      }
      foreach (IFileSystemPlugin entry in PluginRegistry.GetInstance().RegisteredFileSystemPlugins)
      {
        if (entry is ILogExpertPluginConfigurator)
        {
          (entry as ILogExpertPluginConfigurator).SaveConfig(ConfigManager.ConfigDir);
        }
      }
    }


    private void pluginListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.selectedPlugin != null)
      {
        this.selectedPlugin.HideConfigForm();
      }

      object o = this.pluginListBox.SelectedItem;
      if (o != null)
      {
        this.selectedPlugin = o as ILogExpertPluginConfigurator;
        if (o is ILogExpertPluginConfigurator)
        {
          if (this.selectedPlugin.HasEmbeddedForm())
          {
            this.configPluginButton.Enabled = false;
            this.configPluginButton.Visible = false;
            this.selectedPlugin.ShowConfigForm(this.pluginPanel);
          }
          else
          {
            this.configPluginButton.Enabled = true;
            this.configPluginButton.Visible = true;
          }
        }
      }
      else
      {
        this.configPluginButton.Enabled = false;
        this.configPluginButton.Visible = true;
      }
    }

    private void sessionSaveDirButton_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      if (this.preferences.saveDirectory != null)
      {
        dlg.SelectedPath = this.preferences.saveDirectory;
      }
      dlg.ShowNewFolderButton = true;
      dlg.Description = "Choose folder for LogExpert's session files";
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        this.sessionSaveOwnDirLabel.Text = dlg.SelectedPath;
      }
    }

    private void configPluginButton_Click(object sender, EventArgs e)
    {
      if (!this.selectedPlugin.HasEmbeddedForm())
      {
        this.selectedPlugin.ShowConfigDialog(this);
      }
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {

    }

    private void FillToolListbox()
    {
      this.toolListBox.Items.Clear();
      foreach (ToolEntry tool in this.preferences.toolEntries)
      {
        this.toolListBox.Items.Add(tool.Clone(), tool.isFavourite);
      }
      if (this.toolListBox.Items.Count > 0)
      {
        this.toolListBox.SelectedIndex = 0;
      }
    }

    private void FillMultifileSettings()
    {
      switch (this.preferences.multiFileOption)
      {
        case MultiFileOption.SingleFiles:
          this.multiOpenRadioButton1.Checked = true;
          break;
        case MultiFileOption.MultiFile:
          this.multiOpenRadioButton2.Checked = true;
          break;
        case MultiFileOption.Ask:
          this.multiOpenRadioButton3.Checked = true;
          break;
      }
      this.multifilePattern.Text = this.preferences.multifileOptions.FormatPattern;
      this.multifileDays.Value = this.preferences.multifileOptions.MaxDayTry;
    }

    private void GetToolListBoxData()
    {
      GetCurrentToolValues();
      this.preferences.toolEntries.Clear();
      for (int i = 0; i < this.toolListBox.Items.Count; ++i)
      {
        this.preferences.toolEntries.Add(this.toolListBox.Items[i] as ToolEntry);
        (this.toolListBox.Items[i] as ToolEntry).isFavourite = this.toolListBox.GetItemChecked(i);
      }
    }


    private void GetCurrentToolValues()
    {
      if (this.selectedTool != null)
      {
        this.selectedTool.name = Util.IsNullOrSpaces(this.toolName.Text) ? this.cmdTextBox.Text : this.toolName.Text;
        this.selectedTool.cmd = this.cmdTextBox.Text;
        this.selectedTool.args = this.argsTextBox.Text;
        this.selectedTool.columnizerName = this.columnizerComboBox.Text;
        this.selectedTool.sysout = this.sysoutCheckBox.Checked;
        this.selectedTool.workingDir = this.workingDirTextBox.Text;
      }
    }

    private void ShowCurrentToolValues()
    {
      if (this.selectedTool != null)
      {
        this.toolName.Text = this.selectedTool.name;
        this.cmdTextBox.Text = this.selectedTool.cmd;
        this.argsTextBox.Text = this.selectedTool.args;
        this.columnizerComboBox.Text = this.selectedTool.columnizerName;
        this.sysoutCheckBox.Checked = this.selectedTool.sysout;
        this.columnizerComboBox.Enabled = this.selectedTool.sysout;
        this.workingDirTextBox.Text = this.selectedTool.workingDir;
      }
    }


    private void toolListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      GetCurrentToolValues();
      this.selectedTool = this.toolListBox.SelectedItem as ToolEntry;
      ShowCurrentToolValues();
      this.toolListBox.Refresh();
      FillColumnizerForToolsList();
      DisplayCurrentIcon();
    }

    private void toolUpButton_Click(object sender, EventArgs e)
    {
      int i = this.toolListBox.SelectedIndex;
      if (i > 0)
      {
        bool isChecked = this.toolListBox.GetItemChecked(i);
        object item = this.toolListBox.Items[i];
        this.toolListBox.Items.RemoveAt(i);
        i--;
        this.toolListBox.Items.Insert(i, item);
        this.toolListBox.SelectedIndex = i;
        this.toolListBox.SetItemChecked(i, isChecked);
      }
    }

    private void toolDownButton_Click(object sender, EventArgs e)
    {
      int i = this.toolListBox.SelectedIndex;
      if (i < this.toolListBox.Items.Count - 1)
      {
        bool isChecked = this.toolListBox.GetItemChecked(i);
        object item = this.toolListBox.Items[i];
        this.toolListBox.Items.RemoveAt(i);
        i++;
        this.toolListBox.Items.Insert(i, item);
        this.toolListBox.SelectedIndex = i;
        this.toolListBox.SetItemChecked(i, isChecked);
      }
    }

    private void toolAddButton_Click(object sender, EventArgs e)
    {
      this.toolListBox.Items.Add(new ToolEntry());
      this.toolListBox.SelectedIndex = this.toolListBox.Items.Count - 1;
    }

    private void toolDeleteButton_Click(object sender, EventArgs e)
    {
      int i = this.toolListBox.SelectedIndex;
      if (i < this.toolListBox.Items.Count && i >= 0)
      {
        this.toolListBox.Items.RemoveAt(i);
        if (i < this.toolListBox.Items.Count)
        {
          this.toolListBox.SelectedIndex = i;
        }
        else
        {
          if (this.toolListBox.Items.Count > 0)
          {
            this.toolListBox.SelectedIndex = this.toolListBox.Items.Count - 1;
          }
        }
      }

    }

    private void iconButton_Click(object sender, EventArgs e)
    {
      if (selectedTool != null)
      {
        string iconFile = selectedTool.iconFile;
        if (Util.IsNullOrSpaces(iconFile))
        {
          iconFile = this.cmdTextBox.Text;
        }
        ChooseIconDlg dlg = new ChooseIconDlg(iconFile);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          this.selectedTool.iconFile = dlg.FileName;
          this.selectedTool.iconIndex = dlg.IconIndex;
          DisplayCurrentIcon();
        }
      }
    }

    private void DisplayCurrentIcon()
    {
      if (this.selectedTool != null)
      {
        Icon icon = Win32.LoadIconFromExe(this.selectedTool.iconFile, this.selectedTool.iconIndex);
        if (icon != null)
        {
          Image image = icon.ToBitmap();
          this.iconButton.Image = image;
          DestroyIcon(icon.Handle);
          icon.Dispose();
        }
        else
        {
          this.iconButton.Image = emptyImage;
        }
      }
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      if (this.selectedPlugin != null)
      {
        this.selectedPlugin.HideConfigForm();
      }

    }

    private void workingDirButton_Click(object sender, EventArgs e)
    {
      WorkingDirButtonClick(this.workingDirTextBox);
    }

    private void multifilePattern_TextChanged(object sender, EventArgs e)
    {
      string pattern = this.multifilePattern.Text;
      this.multifileDays.Enabled = pattern.Contains("$D");
    }

    private void FillEncodingList()
    {
      this.encodingComboBox.Items.Clear();

      //this.encodingComboBox.Items.Add(Encoding.ASCII.BodyName);
      //this.encodingComboBox.Items.Add(Encoding.Default.BodyName);
      //this.encodingComboBox.Items.Add(Encoding.GetEncoding("iso-8859-1").BodyName);
      //this.encodingComboBox.Items.Add(Encoding.UTF8.BodyName);
      //this.encodingComboBox.Items.Add(Encoding.Unicode.BodyName);

      this.encodingComboBox.Items.Add(Encoding.ASCII);
      this.encodingComboBox.Items.Add(Encoding.Default);
      this.encodingComboBox.Items.Add(Encoding.GetEncoding("iso-8859-1"));
      this.encodingComboBox.Items.Add(Encoding.UTF8);
      this.encodingComboBox.Items.Add(Encoding.Unicode);

      this.encodingComboBox.ValueMember = "HeaderName";
    }

    private void exportButton_Click(object sender, EventArgs e)
    {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Title = "Export Settings to file";
      dlg.DefaultExt = "dat";
      dlg.AddExtension = true;
      dlg.Filter = "Settings (*.dat)|*.dat|All files (*.*)|*.*";
      DialogResult result = dlg.ShowDialog();
      if (result == DialogResult.OK)
      {
        Stream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
        ConfigManager.Export(fs);
        fs.Close();
      }
    }

    private void importButton_Click(object sender, EventArgs e)
    {
      ImportSettingsDialog dlg = new ImportSettingsDialog();
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        Stream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
        ConfigManager.Import(fs, dlg.ImportFlags);
        fs.Close();
        this.preferences = ConfigManager.Settings.preferences;
        FillDialog();
        MessageBox.Show(this, "Settings imported", "LogExpert");
      }
    }

  }
}
