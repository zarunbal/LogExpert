using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LogExpert.Classes;
using LogExpert.Classes.Highlight;
using LogExpert.Config;
using LogExpert.Entities;
using NLog;

namespace LogExpert.Dialogs
{
    public partial class HighlightDialog : Form
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly Image _applyButtonImage;
        private string _bookmarkComment;
        private ActionEntry _currentActionEntry = new ActionEntry();
        private HilightGroup _currentGroup;
        private List<HilightGroup> _highlightGroupList;

        #endregion

        #region cTor

        public HighlightDialog()
        {
            InitializeComponent();
            Load += OnHighlightDialogLoad;
            listBoxHighlight.DrawItem += OnHighlightListBoxDrawItem;
            _applyButtonImage = buttonApply.Image;
            buttonApply.Image = null;
        }

        #endregion

        #region Properties

        public List<HilightGroup> HighlightGroupList
        {
            get => _highlightGroupList;
            set => _highlightGroupList = ObjectClone.Clone(value);
        }

        public IList<IKeywordAction> KeywordActionList { get; set; }

        public string PreSelectedGroupName { get; set; }

        private bool IsDirty => buttonApply.Image == _applyButtonImage;

        #endregion

        #region Private Methods

        private void AddNewEntry()
        {
            {
                try
                {
                    CheckRegex();

                    // Create a new entry
                    HilightEntry entry = new HilightEntry(
                        textBoxSearchString.Text,
                        colorBoxForeground.SelectedColor,
                        colorBoxBackground.SelectedColor,
                        checkBoxRegex.Checked,
                        checkBoxCaseSensitive.Checked,
                        checkBoxDontDirtyLed.Checked,
                        checkBoxStopTail.Checked,
                        checkBoxBookmark.Checked,
                        checkBoxPlugin.Checked,
                        _currentActionEntry,
                        checkBoxWordMatch.Checked);

                    entry.IsBold = checkBoxBold.Checked;
                    entry.NoBackground = checkBoxNoBackground.Checked;
                    listBoxHighlight.Items.Add(entry);

                    // Select the newly created item
                    _currentGroup.HilightEntryList.Add(entry);
                    listBoxHighlight.SelectedItem = entry;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error during add of highlight entry");
                    MessageBox.Show($"Error during add of entry.\r\n{ex.Message}");
                }
            }
        }

        private void InitData()
        {
            const string def = "[Default]";
            if (HighlightGroupList == null)
            {
                HighlightGroupList = new List<HilightGroup>();
            }

            if (HighlightGroupList.Count == 0)
            {
                HilightGroup highlightGroup = new HilightGroup
                {
                    GroupName = def,
                    HilightEntryList = new List<HilightEntry>()
                };

                HighlightGroupList.Add(highlightGroup);
            }

            FillGroupComboBox();

            _currentGroup = null;
            string groupToSelect = PreSelectedGroupName;
            if (string.IsNullOrEmpty(groupToSelect))
            {
                groupToSelect = def;
            }

            foreach (HilightGroup group in HighlightGroupList)
            {
                if (group.GroupName.Equals(groupToSelect))
                {
                    _currentGroup = group;
                    comboBoxGroups.SelectedValue = group;
                    comboBoxGroups.SelectedIndex = HighlightGroupList.IndexOf(group);
                    break;
                }
            }

            ReEvaluateGroupButtonStates();

            FillHighlightListBox();
        }

        private void FillHighlightListBox()
        {
            listBoxHighlight.Items.Clear();
            if (_currentGroup != null)
            {
                foreach (HilightEntry entry in _currentGroup.HilightEntryList)
                {
                    listBoxHighlight.Items.Add(entry);
                }
            }
        }

        private void SaveEntry()
        {
            try
            {
                CheckRegex();

                HilightEntry entry = (HilightEntry)listBoxHighlight.SelectedItem;

                entry.ForegroundColor = (Color)colorBoxForeground.SelectedItem;
                entry.BackgroundColor = (Color)colorBoxBackground.SelectedItem;
                entry.SearchText = textBoxSearchString.Text;
                entry.IsRegEx = checkBoxRegex.Checked;
                entry.IsCaseSensitive = checkBoxCaseSensitive.Checked;
                buttonApply.Enabled = false;
                buttonApply.Image = null;
                entry.IsLedSwitch = checkBoxDontDirtyLed.Checked;
                entry.IsSetBookmark = checkBoxBookmark.Checked;
                entry.IsStopTail = checkBoxStopTail.Checked;
                entry.IsActionEntry = checkBoxPlugin.Checked;
                entry.ActionEntry = _currentActionEntry.Copy();
                entry.BookmarkComment = _bookmarkComment;
                entry.IsWordMatch = checkBoxWordMatch.Checked;
                entry.IsBold = checkBoxBold.Checked;
                entry.NoBackground = checkBoxNoBackground.Checked;
                listBoxHighlight.Refresh();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during save of save highlight entry");
                MessageBox.Show($"Error during save of entry.\r\n{ex.Message}");
            }
        }

        private void CheckRegex()
        {
            if (checkBoxRegex.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBoxSearchString.Text))
                {
                    throw new ArgumentException("Regex value is null or whitespace");
                }

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Regex.IsMatch("", textBoxSearchString.Text);
            }
        }

        private void StartEditEntry()
        {
            HilightEntry entry = (HilightEntry)listBoxHighlight.SelectedItem;

            if (entry != null)
            {
                textBoxSearchString.Text = entry.SearchText;

                colorBoxForeground.CustomColor = entry.ForegroundColor;
                colorBoxBackground.CustomColor = entry.BackgroundColor;

                if (colorBoxForeground.Items.Contains(entry.ForegroundColor))
                {
                    colorBoxForeground.SelectedIndex = colorBoxForeground.Items.Cast<Color>().ToList().LastIndexOf(entry.ForegroundColor);
                }
                else
                {
                    colorBoxForeground.SelectedItem = entry.ForegroundColor;
                }

                if (colorBoxForeground.Items.Contains(entry.ForegroundColor))
                {
                    colorBoxBackground.SelectedIndex = colorBoxBackground.Items.Cast<Color>().ToList().LastIndexOf(entry.BackgroundColor);
                }
                else
                {
                    colorBoxBackground.SelectedItem = entry.BackgroundColor;
                }

                checkBoxRegex.Checked = entry.IsRegEx;
                checkBoxCaseSensitive.Checked = entry.IsCaseSensitive;
                checkBoxDontDirtyLed.Checked = entry.IsLedSwitch;
                checkBoxBookmark.Checked = entry.IsSetBookmark;
                checkBoxStopTail.Checked = entry.IsStopTail;
                checkBoxPlugin.Checked = entry.IsActionEntry;
                buttonPlugin.Enabled = checkBoxPlugin.Checked;
                buttonBookmarkComment.Enabled = checkBoxBookmark.Checked;
                _currentActionEntry = entry.ActionEntry != null ? entry.ActionEntry.Copy() : new ActionEntry();
                _bookmarkComment = entry.BookmarkComment;
                checkBoxWordMatch.Checked = entry.IsWordMatch;
                checkBoxBold.Checked = entry.IsBold;
                checkBoxNoBackground.Checked = entry.NoBackground;
            }

            buttonApply.Enabled = false;
            buttonApply.Image = null;

            ReEvaluateHighlightButtonStates();
        }

        private void Dirty()
        {
            int index = listBoxHighlight.SelectedIndex;
            if (index > -1)
            {
                buttonApply.Enabled = true;
                buttonApply.Image = _applyButtonImage;
            }

            buttonAdd.Enabled = textBoxSearchString.Text.Length > 0;
        }

        private void ChooseColor(ColorComboBox comboBox)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.ShowHelp = false;
            colorDialog.Color = comboBox.CustomColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                comboBox.CustomColor = colorDialog.Color;
                comboBox.SelectedIndex = 0;
            }
        }

        private void SelectGroup(int index)
        {
            if (index >= 0 && index < HighlightGroupList.Count)
            {
                _currentGroup = HighlightGroupList[index];
                comboBoxGroups.Items[index] = _currentGroup;
                comboBoxGroups.SelectedIndex = index;
                comboBoxGroups.SelectedItem = _currentGroup;
                FillHighlightListBox();
            }
            else
            {
                comboBoxGroups.SelectedItem = null;
                _currentGroup = null;
                listBoxHighlight.Items.Clear();
            }

            ReEvaluateHighlightButtonStates();
            ReEvaluateGroupButtonStates();
        }

        private void FillGroupComboBox()
        {
            SelectGroup(-1);

            comboBoxGroups.Items.Clear();

            foreach (HilightGroup group in HighlightGroupList)
            {
                comboBoxGroups.Items.Add(group);
            }

            ReEvaluateGroupButtonStates();
        }

        private void ReEvaluateHighlightButtonStates()
        {
            // Refresh button states based on the selection in the combobox
            bool atLeastOneSelected = listBoxHighlight.SelectedItem != null;
            bool moreThanOne = listBoxHighlight.Items.Count > 1;
            bool firstSelected = atLeastOneSelected && listBoxHighlight.SelectedIndex == 0;
            bool lastSelected = atLeastOneSelected &&
                                listBoxHighlight.SelectedIndex == listBoxHighlight.Items.Count - 1;

            buttonDelete.Enabled = atLeastOneSelected;
            buttonMoveUp.Enabled = atLeastOneSelected && moreThanOne && !firstSelected;
            buttonMoveDown.Enabled = atLeastOneSelected && moreThanOne && !lastSelected;
        }

        private void ReEvaluateGroupButtonStates()
        {
            // Refresh button states based on the selection in the combobox
            bool atLeastOneSelected = comboBoxGroups.SelectedItem != null;
            bool moreThanOne = comboBoxGroups.Items.Count > 1;
            bool firstSelected = atLeastOneSelected && comboBoxGroups.SelectedIndex == 0;
            bool lastSelected = atLeastOneSelected &&
                                comboBoxGroups.SelectedIndex == comboBoxGroups.Items.Count - 1;

            buttonDeleteGroup.Enabled = atLeastOneSelected;
            buttonCopyGroup.Enabled = atLeastOneSelected;
            buttonMoveGroupUp.Enabled = atLeastOneSelected && moreThanOne && !firstSelected;
            buttonMoveGroupDown.Enabled = atLeastOneSelected && moreThanOne && !lastSelected;
        }

        #endregion

        #region Events handler

        private void OnHighlightDialogLoad(object sender, EventArgs e)
        {
            colorBoxForeground.SelectedIndex = 1;
            colorBoxBackground.SelectedIndex = 2;
            buttonApply.Enabled = false;
            buttonApply.Image = null;
            buttonBookmarkComment.Enabled = false;
            buttonPlugin.Enabled = false;

            ReEvaluateHighlightButtonStates();
        }

        private void OnHighlightDialogShown(object sender, EventArgs e)
        {
            InitData();
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            // Apply pending changes if closing the form.
            if (IsDirty)
            {
                // cannot call 'this.applyButton.PerformClick();' because it prohibits the OK button to terminate the dialog
                OnApplyButtonClick(buttonApply, EventArgs.Empty); 
            }
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            AddNewEntry();
            Dirty();
        }

        private void OnDeleteButtonClick(object sender, EventArgs e)
        {
            if (listBoxHighlight.SelectedIndex >= 0)
            {
                int removeIndex = listBoxHighlight.SelectedIndex;
                _currentGroup.HilightEntryList.RemoveAt(removeIndex);
                listBoxHighlight.Items.RemoveAt(removeIndex);

                // Select previous (or first if none before)
                int nextSelectIndex = removeIndex;
                if (nextSelectIndex >= listBoxHighlight.Items.Count)
                {
                    nextSelectIndex--; // if last item was removed, go one up
                }

                if (nextSelectIndex >= 0)
                {
                    listBoxHighlight.SelectedIndex = nextSelectIndex; // if still some item, select it
                }

                ReEvaluateHighlightButtonStates();
            }
        }

        private void OnHighlightListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                HilightEntry entry = (HilightEntry)listBoxHighlight.Items[e.Index];
                Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

                if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
                {
                    e.Graphics.FillRectangle(new SolidBrush(entry.BackgroundColor), rectangle);
                }

                e.Graphics.DrawString(entry.SearchText, e.Font, new SolidBrush(entry.ForegroundColor),
                    new PointF(rectangle.Left, rectangle.Top));

                e.DrawFocusRectangle();
            }
        }

        private void OnApplyButtonClick(object sender, EventArgs e)
        {
            SaveEntry();
        }

        private void OnHighlightListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            StartEditEntry();
        }

        private void OnMoveUpButtonClick(object sender, EventArgs e)
        {
            int index = listBoxHighlight.SelectedIndex;
            if (index > 0)
            {
                object item = listBoxHighlight.SelectedItem;
                listBoxHighlight.Items.RemoveAt(index); // will also clear the selection
                listBoxHighlight.Items.Insert(index - 1, item);
                listBoxHighlight.SelectedIndex = index - 1; // restore the selection
                _currentGroup.HilightEntryList.Reverse(index - 1, 2);
            }
        }

        private void OnMoveDownButtonClick(object sender, EventArgs e)
        {
            int index = listBoxHighlight.SelectedIndex;
            if (index > -1 && index < listBoxHighlight.Items.Count - 1)
            {
                object item = listBoxHighlight.SelectedItem;
                listBoxHighlight.Items.RemoveAt(index);
                listBoxHighlight.Items.Insert(index + 1, item);
                listBoxHighlight.SelectedIndex = index + 1;
                _currentGroup.HilightEntryList.Reverse(index, 2);
            }
        }

        private void OnCustomForeColorButtonClick(object sender, EventArgs e)
        {
            ChooseColor(colorBoxForeground);
            Dirty();
        }

        private void OnCustomBackColorButtonClick(object sender, EventArgs e)
        {
            ChooseColor(colorBoxBackground);
            Dirty();
        }

        private void OnRegexCheckBoxMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RegexHelperDialog dlg = new RegexHelperDialog();
                dlg.Owner = this;
                dlg.CaseSensitive = checkBoxCaseSensitive.Checked;
                dlg.Pattern = textBoxSearchString.Text;
                DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.OK)
                {
                    checkBoxCaseSensitive.Checked = dlg.CaseSensitive;
                    textBoxSearchString.Text = dlg.Pattern;
                }
            }
        }

        private void ChangeToDirty(object sender, EventArgs e)
        {
            Dirty();
        }

        private void OnPluginCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Dirty();
            buttonPlugin.Enabled = checkBoxPlugin.Checked;
        }

        private void OnPluginButtonClick(object sender, EventArgs e)
        {
            KeywordActionDlg dlg = new KeywordActionDlg(_currentActionEntry, KeywordActionList);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _currentActionEntry = dlg.ActionEntry;
                Dirty();
            }
        }

        private void OnBookmarkCommentButtonClick(object sender, EventArgs e)
        {
            BookmarkCommentDlg dlg = new BookmarkCommentDlg();
            dlg.Comment = _bookmarkComment;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _bookmarkComment = dlg.Comment;
                Dirty();
            }
        }

        private void OnDelGroupButtonClick(object sender, EventArgs e)
        {
            // the last group cannot be deleted
            if (HighlightGroupList.Count == 1)
            {
                return;
            }

            if (comboBoxGroups.SelectedIndex >= 0 && comboBoxGroups.SelectedIndex < HighlightGroupList.Count)
            {
                int index = comboBoxGroups.SelectedIndex;
                HighlightGroupList.RemoveAt(comboBoxGroups.SelectedIndex);
                FillGroupComboBox();
                if (index < HighlightGroupList.Count)
                {
                    SelectGroup(index);
                }
                else
                {
                    SelectGroup(HighlightGroupList.Count - 1);
                }
            }
        }

        private void OnImportGroupButtonClick(object sender, EventArgs e)
        {
            ImportSettingsDialog dlg = new ImportSettingsDialog();

            foreach (Control ctl in dlg.groupBoxImportOptions.Controls)
            {
                if (ctl.Tag != null)
                {
                    ((CheckBox)ctl).Checked = false;
                }
            }

            dlg.checkBoxHighlightSettings.Checked = true;
            dlg.checkBoxKeepExistingSettings.Checked = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(dlg.FileName))
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            FileInfo fileInfo;

            try
            {
                fileInfo = new FileInfo(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $@"Settings could not be imported: {ex}", @"LogExpert");
                _logger.Error($"Error while trying to access file: {dlg.FileName}: {ex}");
                return;
            }

            ConfigManager.Import(fileInfo, dlg.ImportFlags);
            Cursor.Current = Cursors.Default;

            _highlightGroupList = ConfigManager.Settings.hilightGroupList;

            FillGroupComboBox();

            MessageBox.Show(this, @"Settings imported", @"LogExpert");
        }

        private void OnNewGroupButtonClick(object sender, EventArgs e)
        {
            // Propose a unique name
            const string baseName = "New group";
            string name = baseName;
            bool uniqueName = false;
            int i = 1;
            while (!uniqueName)
            {
                uniqueName = HighlightGroupList.FindIndex(delegate (HilightGroup g) { return g.GroupName == name; }) <
                             0;
                if (!uniqueName)
                {
                    name = $"{baseName} #{i++}";
                }
            }

            HilightGroup newGroup = new HilightGroup() { GroupName = name };
            HighlightGroupList.Add(newGroup);
            FillGroupComboBox();
            SelectGroup(HighlightGroupList.Count - 1);
        }

        private void OnCopyGroupButtonClick(object sender, EventArgs e)
        {
            if (comboBoxGroups.SelectedIndex >= 0 && comboBoxGroups.SelectedIndex < HighlightGroupList.Count)
            {
                HilightGroup newGroup = ObjectClone.Clone(HighlightGroupList[comboBoxGroups.SelectedIndex]);
                newGroup.GroupName = "Copy of " + newGroup.GroupName;
                HighlightGroupList.Add(newGroup);
                FillGroupComboBox();
                SelectGroup(HighlightGroupList.Count - 1);
            }
        }

        private void OnGroupComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                HilightGroup group = HighlightGroupList[e.Index];
                Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

                Brush brush = new SolidBrush(SystemColors.ControlText);
                e.Graphics.DrawString(group.GroupName, e.Font, brush, new PointF(rectangle.Left, rectangle.Top));
                e.DrawFocusRectangle();
                brush.Dispose();
            }
        }

        private void OnGroupComboBoxTextUpdate(object sender, EventArgs e)
        {
            _currentGroup.GroupName = comboBoxGroups.Text;
        }

        private void OnGroupComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            SelectGroup(comboBoxGroups.SelectedIndex);
        }

        private void OnGroupUpButtonClick(object sender, EventArgs e)
        {
            int index = comboBoxGroups.SelectedIndex;
            if (index > 0)
            {
                _highlightGroupList.Reverse(index - 1, 2);
                comboBoxGroups.Refresh();
                FillGroupComboBox();
                SelectGroup(index - 1);
            }
        }

        private void OnGroupDownButtonClick(object sender, EventArgs e)
        {
            int index = comboBoxGroups.SelectedIndex;
            if (index > -1 && index < _highlightGroupList.Count - 1)
            {
                _highlightGroupList.Reverse(index, 2);
                comboBoxGroups.Refresh();
                FillGroupComboBox();
                SelectGroup(index + 1);
            }
        }

        private void OnWordMatchCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Dirty();
            checkBoxNoBackground.Enabled = checkBoxWordMatch.Checked;
        }

        private void OnNoBackgroundCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            colorBoxBackground.Enabled = !checkBoxNoBackground.Checked;
            buttonCustomBackColor.Enabled = !checkBoxNoBackground.Checked;
            Dirty();
        }

        private void OnBoldCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Dirty();
        }
        #endregion

        private void OnButtonExportGroupClick(object sender, EventArgs e)
        {

        }
    }
}