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
        #region Fields

        private readonly Image emptyImage = new Bitmap(16, 16);
        private readonly LogTabWindow logTabWin;

        private ILogExpertPluginConfigurator selectedPlugin = null;
        private ToolEntry selectedTool = null;

        #endregion

        #region cTor

        public SettingsDialog(Preferences prefs, LogTabWindow logTabWin)
        {
            Preferences = prefs;
            this.logTabWin = logTabWin;
            InitializeComponent();
        }

        public SettingsDialog(Preferences prefs, LogTabWindow logTabWin, int tabToOpen)
            : this(prefs, logTabWin)
        {
            tabControl1.SelectedIndex = tabToOpen;
        }

        #endregion

        #region Properties

        public Preferences Preferences { get; private set; }

        #endregion

        #region Private Methods

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static bool DestroyIcon(IntPtr handle);

        private void FillDialog()
        {
            if (Preferences == null)
            {
                Preferences = new Preferences();
            }

            if (Preferences.fontName == null)
            {
                Preferences.fontName = "Courier New";
            }

            if (Preferences.fontSize == 0.0)
            {
                Preferences.fontSize = 9.0f;
            }

            timestampCheckBox.Checked = Preferences.timestampControl;
            syncFilterCheckBox.Checked = Preferences.filterSync;
            filterTailCheckBox.Checked = Preferences.filterTail;
            followTailCheckBox.Checked = Preferences.followTail;
            horizRadioButton.Checked = Preferences.timestampControlDragOrientation ==
                                       DateTimeDragControl.DragOrientations.Horizontal;
            verticalRadioButton.Checked = Preferences.timestampControlDragOrientation ==
                                          DateTimeDragControl.DragOrientations.Vertical;
            verticalInvRadioButton.Checked = Preferences.timestampControlDragOrientation ==
                                             DateTimeDragControl.DragOrientations.InvertedVertical;

            singleInstanceCheckBox.Checked = Preferences.allowOnlyOneInstance;
            openLastFilesCheckBox.Checked = Preferences.openLastFiles;
            tailStateCheckBox.Checked = Preferences.showTailState;
            columnSizeCheckBox.Checked = Preferences.setLastColumnWidth;
            columnWidthUpDown.Enabled = Preferences.setLastColumnWidth;
            if (Preferences.lastColumnWidth != 0)
            {
                if (Preferences.lastColumnWidth < columnWidthUpDown.Minimum)
                {
                    Preferences.lastColumnWidth = (int) columnWidthUpDown.Minimum;
                }

                if (Preferences.lastColumnWidth > columnWidthUpDown.Maximum)
                {
                    Preferences.lastColumnWidth = (int) columnWidthUpDown.Maximum;
                }

                columnWidthUpDown.Value = Preferences.lastColumnWidth;
            }

            timeSpreadCheckBox.Checked = Preferences.showTimeSpread;
            reverseAlphaCheckBox.Checked = Preferences.reverseAlpha;
            timeViewRadioButton.Checked = Preferences.timeSpreadTimeMode;
            lineViewRadioButton.Checked = !Preferences.timeSpreadTimeMode;

            saveSessionsCheckBox.Checked = Preferences.saveSessions;
            switch (Preferences.saveLocation)
            {
                case SessionSaveLocation.OwnDir:
                    sessionSaveRadioOwn.Checked = true;
                    break;
                case SessionSaveLocation.SameDir:
                    sessionRadioSameDir.Checked = true;
                    break;
                case SessionSaveLocation.DocumentsDir:
                    sessionSaveRadioDocuments.Checked = true;
                    break;
            }

            sessionSaveOwnDirLabel.Text =
                Preferences.saveDirectory != null ? Preferences.saveDirectory : "";
            saveFilterCheckBox.Checked = Preferences.saveFilters;
            blockCountUpDown.Value = Preferences.bufferCount;
            linesPerBlockUpDown.Value = Preferences.linesPerBuffer;
            pollingIntervalUpDown.Value = Preferences.pollingInterval;
            multiThreadCheckBox.Checked = Preferences.multiThreadFilter;

            columnizerDataGridView.DataError +=
                new DataGridViewDataErrorEventHandler(columnizerDataGridView_DataError);

            FillColumnizerList();
            FillPluginList();
            DisplayFontName();
            FillHighlightMaskList();
            FillToolListbox();
            FillMultifileSettings();
            FillEncodingList();
            encodingComboBox.SelectedItem = Encoding.GetEncoding(Preferences.defaultEncoding);
            maskPrioCheckBox.Checked = Preferences.maskPrio;
            askCloseTabsCheckBox.Checked = Preferences.askForClose;
            columnFinderCheckBox.Checked = Preferences.showColumnFinder;
            legacyReaderCheckBox.Checked = Preferences.useLegacyReader;
        }

        private string NotNull(string text)
        {
            if (text == null)
            {
                return "";
            }
            else
            {
                return text;
            }
        }

        private void DisplayFontName()
        {
            fontLabel.Text = Preferences.fontName + " " + (int) Preferences.fontSize;
            fontLabel.Font = new Font(new FontFamily(Preferences.fontName), Preferences.fontSize);
        }

        private void SaveMultifileData()
        {
            if (multiOpenRadioButton1.Checked)
            {
                Preferences.multiFileOption = MultiFileOption.SingleFiles;
            }

            if (multiOpenRadioButton2.Checked)
            {
                Preferences.multiFileOption = MultiFileOption.MultiFile;
            }

            if (multiOpenRadioButton3.Checked)
            {
                Preferences.multiFileOption = MultiFileOption.Ask;
            }

            Preferences.multifileOptions.FormatPattern = multifilePattern.Text;
            Preferences.multifileOptions.MaxDayTry = (int) multifileDays.Value;
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
            ToolArgsDialog dlg = new ToolArgsDialog(logTabWin, this);
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

        private void FillColumnizerForToolsList()
        {
            if (selectedTool != null)
            {
                FillColumnizerForToolsList(columnizerComboBox, selectedTool.columnizerName);
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
                {
                    selIndex = index;
                }
            }

            //ILogLineColumnizer columnizer = Util.FindColumnizerByName(columnizerName, this.logTabWin.RegisteredColumnizers);
            //if (columnizer == null)
            //  columnizer = this.logTabWin.RegisteredColumnizers[0];
            comboBox.SelectedIndex = selIndex;
        }

        private void FillColumnizerList()
        {
            columnizerDataGridView.Rows.Clear();

            DataGridViewComboBoxColumn
                comboColumn = (DataGridViewComboBoxColumn) columnizerDataGridView.Columns[1];
            comboColumn.Items.Clear();

            DataGridViewTextBoxColumn textColumn = (DataGridViewTextBoxColumn) columnizerDataGridView.Columns[0];

            IList<ILogLineColumnizer> columnizers = PluginRegistry.GetInstance().RegisteredColumnizers;
            foreach (ILogLineColumnizer columnizer in columnizers)
            {
                int index = comboColumn.Items.Add(columnizer.GetName());
            }
            //comboColumn.DisplayMember = "Name";
            //comboColumn.ValueMember = "Columnizer";

            foreach (ColumnizerMaskEntry maskEntry in Preferences.columnizerMaskList)
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
                ILogLineColumnizer columnizer = Util.FindColumnizerByName(maskEntry.columnizerName,
                    PluginRegistry.GetInstance().RegisteredColumnizers);
                if (columnizer == null)
                {
                    columnizer = PluginRegistry.GetInstance().RegisteredColumnizers[0];
                }

                row.Cells[1].Value = columnizer.GetName();
                columnizerDataGridView.Rows.Add(row);
            }

            int count = columnizerDataGridView.RowCount;
            if (count > 0 && !columnizerDataGridView.Rows[count - 1].IsNewRow)
            {
                DataGridViewComboBoxCell comboCell =
                    (DataGridViewComboBoxCell) columnizerDataGridView.Rows[count - 1].Cells[1];
                comboCell.Value = comboCell.Items[0];
            }
        }

        private void FillHighlightMaskList()
        {
            highlightMaskGridView.Rows.Clear();

            DataGridViewComboBoxColumn comboColumn = (DataGridViewComboBoxColumn) highlightMaskGridView.Columns[1];
            comboColumn.Items.Clear();

            DataGridViewTextBoxColumn textColumn = (DataGridViewTextBoxColumn) highlightMaskGridView.Columns[0];

            IList<HilightGroup> groups = logTabWin.HilightGroupList;
            foreach (HilightGroup group in groups)
            {
                int index = comboColumn.Items.Add(group.GroupName);
            }

            foreach (HighlightMaskEntry maskEntry in Preferences.highlightMaskList)
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
                HilightGroup currentGroup = logTabWin.FindHighlightGroup(maskEntry.highlightGroupName);
                if (currentGroup == null)
                {
                    currentGroup = groups[0];
                }

                if (currentGroup == null)
                {
                    currentGroup = new HilightGroup();
                }

                row.Cells[1].Value = currentGroup.GroupName;
                highlightMaskGridView.Rows.Add(row);
            }

            int count = highlightMaskGridView.RowCount;
            if (count > 0 && !highlightMaskGridView.Rows[count - 1].IsNewRow)
            {
                DataGridViewComboBoxCell comboCell =
                    (DataGridViewComboBoxCell) highlightMaskGridView.Rows[count - 1].Cells[1];
                comboCell.Value = comboCell.Items[0];
            }
        }

        private void SaveColumnizerList()
        {
            Preferences.columnizerMaskList.Clear();
            foreach (DataGridViewRow row in columnizerDataGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    ColumnizerMaskEntry entry = new ColumnizerMaskEntry();
                    entry.mask = (string) row.Cells[0].Value;
                    entry.columnizerName = (string) row.Cells[1].Value;
                    Preferences.columnizerMaskList.Add(entry);
                }
            }
        }

        private void SaveHighlightMaskList()
        {
            Preferences.highlightMaskList.Clear();
            foreach (DataGridViewRow row in highlightMaskGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    HighlightMaskEntry entry = new HighlightMaskEntry();
                    entry.mask = (string) row.Cells[0].Value;
                    entry.highlightGroupName = (string) row.Cells[1].Value;
                    Preferences.highlightMaskList.Add(entry);
                }
            }
        }

        private void FillPluginList()
        {
            pluginListBox.Items.Clear();
            foreach (IContextMenuEntry entry in PluginRegistry.GetInstance().RegisteredContextMenuPlugins)
            {
                pluginListBox.Items.Add(entry);
                if (entry is ILogExpertPluginConfigurator)
                {
                    (entry as ILogExpertPluginConfigurator).StartConfig();
                }
            }

            foreach (IKeywordAction entry in PluginRegistry.GetInstance().RegisteredKeywordActions)
            {
                pluginListBox.Items.Add(entry);
                if (entry is ILogExpertPluginConfigurator)
                {
                    (entry as ILogExpertPluginConfigurator).StartConfig();
                }
            }

            foreach (IFileSystemPlugin entry in PluginRegistry.GetInstance().RegisteredFileSystemPlugins)
            {
                pluginListBox.Items.Add(entry);
                if (entry is ILogExpertPluginConfigurator)
                {
                    (entry as ILogExpertPluginConfigurator).StartConfig();
                }
            }

            configPluginButton.Enabled = false;
        }

        private void SavePluginSettings()
        {
            if (selectedPlugin != null)
            {
                selectedPlugin.HideConfigForm();
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
        }

        private void FillToolListbox()
        {
            toolListBox.Items.Clear();
            foreach (ToolEntry tool in Preferences.toolEntries)
            {
                toolListBox.Items.Add(tool.Clone(), tool.isFavourite);
            }

            if (toolListBox.Items.Count > 0)
            {
                toolListBox.SelectedIndex = 0;
            }
        }

        private void FillMultifileSettings()
        {
            switch (Preferences.multiFileOption)
            {
                case MultiFileOption.SingleFiles:
                    multiOpenRadioButton1.Checked = true;
                    break;
                case MultiFileOption.MultiFile:
                    multiOpenRadioButton2.Checked = true;
                    break;
                case MultiFileOption.Ask:
                    multiOpenRadioButton3.Checked = true;
                    break;
            }

            multifilePattern.Text = Preferences.multifileOptions.FormatPattern;
            multifileDays.Value = Preferences.multifileOptions.MaxDayTry;
        }

        private void GetToolListBoxData()
        {
            GetCurrentToolValues();
            Preferences.toolEntries.Clear();
            for (int i = 0; i < toolListBox.Items.Count; ++i)
            {
                Preferences.toolEntries.Add(toolListBox.Items[i] as ToolEntry);
                (toolListBox.Items[i] as ToolEntry).isFavourite = toolListBox.GetItemChecked(i);
            }
        }

        private void GetCurrentToolValues()
        {
            if (selectedTool != null)
            {
                selectedTool.name =
                    Util.IsNullOrSpaces(toolName.Text) ? cmdTextBox.Text : toolName.Text;
                selectedTool.cmd = cmdTextBox.Text;
                selectedTool.args = argsTextBox.Text;
                selectedTool.columnizerName = columnizerComboBox.Text;
                selectedTool.sysout = sysoutCheckBox.Checked;
                selectedTool.workingDir = workingDirTextBox.Text;
            }
        }

        private void ShowCurrentToolValues()
        {
            if (selectedTool != null)
            {
                toolName.Text = selectedTool.name;
                cmdTextBox.Text = selectedTool.cmd;
                argsTextBox.Text = selectedTool.args;
                columnizerComboBox.Text = selectedTool.columnizerName;
                sysoutCheckBox.Checked = selectedTool.sysout;
                columnizerComboBox.Enabled = selectedTool.sysout;
                workingDirTextBox.Text = selectedTool.workingDir;
            }
        }

        private void DisplayCurrentIcon()
        {
            if (selectedTool != null)
            {
                Icon icon = Win32.LoadIconFromExe(selectedTool.iconFile, selectedTool.iconIndex);
                if (icon != null)
                {
                    Image image = icon.ToBitmap();
                    iconButton.Image = image;
                    DestroyIcon(icon.Handle);
                    icon.Dispose();
                }
                else
                {
                    iconButton.Image = emptyImage;
                }
            }
        }

        private void FillEncodingList()
        {
            encodingComboBox.Items.Clear();

            //this.encodingComboBox.Items.Add(Encoding.ASCII.BodyName);
            //this.encodingComboBox.Items.Add(Encoding.Default.BodyName);
            //this.encodingComboBox.Items.Add(Encoding.GetEncoding("iso-8859-1").BodyName);
            //this.encodingComboBox.Items.Add(Encoding.UTF8.BodyName);
            //this.encodingComboBox.Items.Add(Encoding.Unicode.BodyName);

            encodingComboBox.Items.Add(Encoding.ASCII);
            encodingComboBox.Items.Add(Encoding.Default);
            encodingComboBox.Items.Add(Encoding.GetEncoding("iso-8859-1"));
            encodingComboBox.Items.Add(Encoding.UTF8);
            encodingComboBox.Items.Add(Encoding.Unicode);

            encodingComboBox.ValueMember = "HeaderName";
        }

        #endregion

        #region Events handler

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            FillDialog();
        }

        private void changeFontButton_Click(object sender, EventArgs e)
        {
            FontDialog dlg = new FontDialog();
            dlg.ShowEffects = false;
            dlg.AllowVerticalFonts = false;
            dlg.AllowScriptChange = false;
            dlg.Font = new Font(new FontFamily(Preferences.fontName), Preferences.fontSize);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Preferences.fontSize = dlg.Font.Size;
                Preferences.fontName = dlg.Font.FontFamily.Name;
            }

            DisplayFontName();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Preferences.timestampControl = timestampCheckBox.Checked;
            Preferences.filterSync = syncFilterCheckBox.Checked;
            Preferences.filterTail = filterTailCheckBox.Checked;
            Preferences.followTail = followTailCheckBox.Checked;
            if (verticalRadioButton.Checked)
            {
                Preferences.timestampControlDragOrientation = DateTimeDragControl.DragOrientations.Vertical;
            }
            else if (verticalInvRadioButton.Checked)
            {
                Preferences.timestampControlDragOrientation =
                    DateTimeDragControl.DragOrientations.InvertedVertical;
            }
            else
            {
                Preferences.timestampControlDragOrientation = DateTimeDragControl.DragOrientations.Horizontal;
            }

            SaveColumnizerList();
            Preferences.maskPrio = maskPrioCheckBox.Checked;
            Preferences.askForClose = askCloseTabsCheckBox.Checked;
            Preferences.allowOnlyOneInstance = singleInstanceCheckBox.Checked;
            Preferences.openLastFiles = openLastFilesCheckBox.Checked;
            Preferences.showTailState = tailStateCheckBox.Checked;
            Preferences.setLastColumnWidth = columnSizeCheckBox.Checked;
            Preferences.lastColumnWidth = (int) columnWidthUpDown.Value;
            Preferences.showTimeSpread = timeSpreadCheckBox.Checked;
            Preferences.reverseAlpha = reverseAlphaCheckBox.Checked;
            Preferences.timeSpreadTimeMode = timeViewRadioButton.Checked;

            Preferences.saveSessions = saveSessionsCheckBox.Checked;
            Preferences.saveDirectory = sessionSaveOwnDirLabel.Text;
            if (sessionSaveRadioDocuments.Checked)
            {
                Preferences.saveLocation = SessionSaveLocation.DocumentsDir;
            }
            else if (sessionSaveRadioOwn.Checked)
            {
                Preferences.saveLocation = SessionSaveLocation.OwnDir;
            }
            else
            {
                Preferences.saveLocation = SessionSaveLocation.SameDir;
            }

            Preferences.saveFilters = saveFilterCheckBox.Checked;
            Preferences.bufferCount = (int) blockCountUpDown.Value;
            Preferences.linesPerBuffer = (int) linesPerBlockUpDown.Value;
            Preferences.pollingInterval = (int) pollingIntervalUpDown.Value;
            Preferences.multiThreadFilter = multiThreadCheckBox.Checked;
            Preferences.defaultEncoding = encodingComboBox.SelectedItem != null
                ? (encodingComboBox.SelectedItem as Encoding).HeaderName
                : Encoding.Default.HeaderName;
            Preferences.showColumnFinder = columnFinderCheckBox.Checked;
            Preferences.useLegacyReader = legacyReaderCheckBox.Checked;
            SavePluginSettings();
            SaveHighlightMaskList();
            GetToolListBoxData();
            SaveMultifileData();
        }

        private void toolButtonA_Click(object sender, EventArgs e)
        {
            ToolButtonClick(cmdTextBox);
        }

        private void argButtonA_Click(object sender, EventArgs e)
        {
            ArgsButtonClick(argsTextBox);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            columnizerDataGridView.Rows.Add();
        }

        private void columnizerDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridViewComboBoxCell comboCell =
                (DataGridViewComboBoxCell) columnizerDataGridView.Rows[e.RowIndex].Cells[1];
            if (comboCell.Items.Count > 0)
            {
//        comboCell.Value = comboCell.Items[0];
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (columnizerDataGridView.CurrentRow != null && !columnizerDataGridView.CurrentRow.IsNewRow)
            {
                int index = columnizerDataGridView.CurrentRow.Index;
                columnizerDataGridView.EndEdit();
                columnizerDataGridView.Rows.RemoveAt(index);
            }
        }

        private void columnizerDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void sysoutCheckBoxA_CheckedChanged(object sender, EventArgs e)
        {
            columnizerComboBox.Enabled = sysoutCheckBox.Checked;
        }

        private void tailColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = Preferences.showTailColor;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Preferences.showTailColor = dlg.Color;
            }
        }

        private void columnSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            columnWidthUpDown.Enabled = columnSizeCheckBox.Checked;
        }

        private void timespreadColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = Preferences.timeSpreadColor;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Preferences.timeSpreadColor = dlg.Color;
            }
        }

        private void pluginListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedPlugin != null)
            {
                selectedPlugin.HideConfigForm();
            }

            object o = pluginListBox.SelectedItem;
            if (o != null)
            {
                selectedPlugin = o as ILogExpertPluginConfigurator;
                if (o is ILogExpertPluginConfigurator)
                {
                    if (selectedPlugin.HasEmbeddedForm())
                    {
                        configPluginButton.Enabled = false;
                        configPluginButton.Visible = false;
                        selectedPlugin.ShowConfigForm(pluginPanel);
                    }
                    else
                    {
                        configPluginButton.Enabled = true;
                        configPluginButton.Visible = true;
                    }
                }
            }
            else
            {
                configPluginButton.Enabled = false;
                configPluginButton.Visible = true;
            }
        }

        private void sessionSaveDirButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (Preferences.saveDirectory != null)
            {
                dlg.SelectedPath = Preferences.saveDirectory;
            }

            dlg.ShowNewFolderButton = true;
            dlg.Description = "Choose folder for LogExpert's session files";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                sessionSaveOwnDirLabel.Text = dlg.SelectedPath;
            }
        }

        private void configPluginButton_Click(object sender, EventArgs e)
        {
            if (!selectedPlugin.HasEmbeddedForm())
            {
                selectedPlugin.ShowConfigDialog(this);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void toolListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetCurrentToolValues();
            selectedTool = toolListBox.SelectedItem as ToolEntry;
            ShowCurrentToolValues();
            toolListBox.Refresh();
            FillColumnizerForToolsList();
            DisplayCurrentIcon();
        }

        private void toolUpButton_Click(object sender, EventArgs e)
        {
            int i = toolListBox.SelectedIndex;
            if (i > 0)
            {
                bool isChecked = toolListBox.GetItemChecked(i);
                object item = toolListBox.Items[i];
                toolListBox.Items.RemoveAt(i);
                i--;
                toolListBox.Items.Insert(i, item);
                toolListBox.SelectedIndex = i;
                toolListBox.SetItemChecked(i, isChecked);
            }
        }

        private void toolDownButton_Click(object sender, EventArgs e)
        {
            int i = toolListBox.SelectedIndex;
            if (i < toolListBox.Items.Count - 1)
            {
                bool isChecked = toolListBox.GetItemChecked(i);
                object item = toolListBox.Items[i];
                toolListBox.Items.RemoveAt(i);
                i++;
                toolListBox.Items.Insert(i, item);
                toolListBox.SelectedIndex = i;
                toolListBox.SetItemChecked(i, isChecked);
            }
        }

        private void toolAddButton_Click(object sender, EventArgs e)
        {
            toolListBox.Items.Add(new ToolEntry());
            toolListBox.SelectedIndex = toolListBox.Items.Count - 1;
        }

        private void toolDeleteButton_Click(object sender, EventArgs e)
        {
            int i = toolListBox.SelectedIndex;
            if (i < toolListBox.Items.Count && i >= 0)
            {
                toolListBox.Items.RemoveAt(i);
                if (i < toolListBox.Items.Count)
                {
                    toolListBox.SelectedIndex = i;
                }
                else
                {
                    if (toolListBox.Items.Count > 0)
                    {
                        toolListBox.SelectedIndex = toolListBox.Items.Count - 1;
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
                    iconFile = cmdTextBox.Text;
                }

                ChooseIconDlg dlg = new ChooseIconDlg(iconFile);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    selectedTool.iconFile = dlg.FileName;
                    selectedTool.iconIndex = dlg.IconIndex;
                    DisplayCurrentIcon();
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (selectedPlugin != null)
            {
                selectedPlugin.HideConfigForm();
            }
        }

        private void workingDirButton_Click(object sender, EventArgs e)
        {
            WorkingDirButtonClick(workingDirTextBox);
        }

        private void multifilePattern_TextChanged(object sender, EventArgs e)
        {
            string pattern = multifilePattern.Text;
            multifileDays.Enabled = pattern.Contains("$D");
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
                Preferences = ConfigManager.Settings.preferences;
                FillDialog();
                MessageBox.Show(this, "Settings imported", "LogExpert");
            }
        }

        #endregion

        private class ColumnizerEntry
        {
            #region cTor

            public ColumnizerEntry(ILogLineColumnizer columnizer)
            {
                Columnizer = columnizer;
            }

            #endregion

            #region Properties

            public string Name
            {
                get { return Columnizer.GetName(); }
            }

            public ILogLineColumnizer Columnizer { get; }

            #endregion
        }
    }
}