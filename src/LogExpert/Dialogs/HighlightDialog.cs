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
            hilightListBox.DrawItem += OnHighlightListBoxDrawItem;
            _applyButtonImage = applyButton.Image;
            applyButton.Image = null;
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

        private bool IsDirty => applyButton.Image == _applyButtonImage;

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
                        searchStringTextBox.Text,
                        foregroundColorBox.SelectedColor,
                        backgroundColorBox.SelectedColor,
                        regexCheckBox.Checked,
                        caseSensitiveCheckBox.Checked,
                        ledCheckBox.Checked,
                        stopTailCheckBox.Checked,
                        bookmarkCheckBox.Checked,
                        pluginCheckBox.Checked,
                        _currentActionEntry,
                        wordMatchCheckBox.Checked);

                    entry.IsBold = boldCheckBox.Checked;
                    entry.NoBackground = noBackgroundCheckBox.Checked;
                    hilightListBox.Items.Add(entry);

                    // Select the newly created item
                    _currentGroup.HilightEntryList.Add(entry);
                    hilightListBox.SelectedItem = entry;
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
                    groupComboBox.SelectedValue = group;
                    groupComboBox.SelectedIndex = HighlightGroupList.IndexOf(group);
                    break;
                }
            }

            ReEvaluateGroupButtonStates();

            FillHighlightListBox();
        }

        private void FillHighlightListBox()
        {
            hilightListBox.Items.Clear();
            if (_currentGroup != null)
            {
                foreach (HilightEntry entry in _currentGroup.HilightEntryList)
                {
                    hilightListBox.Items.Add(entry);
                }
            }
        }

        private void SaveEntry()
        {
            try
            {
                CheckRegex();

                HilightEntry entry = (HilightEntry)hilightListBox.SelectedItem;

                entry.ForegroundColor = (Color)foregroundColorBox.SelectedItem;
                entry.BackgroundColor = (Color)backgroundColorBox.SelectedItem;
                entry.SearchText = searchStringTextBox.Text;
                entry.IsRegEx = regexCheckBox.Checked;
                entry.IsCaseSensitive = caseSensitiveCheckBox.Checked;
                applyButton.Enabled = false;
                applyButton.Image = null;
                entry.IsLedSwitch = ledCheckBox.Checked;
                entry.IsSetBookmark = bookmarkCheckBox.Checked;
                entry.IsStopTail = stopTailCheckBox.Checked;
                entry.IsActionEntry = pluginCheckBox.Checked;
                entry.ActionEntry = _currentActionEntry.Copy();
                entry.BookmarkComment = _bookmarkComment;
                entry.IsWordMatch = wordMatchCheckBox.Checked;
                entry.IsBold = boldCheckBox.Checked;
                entry.NoBackground = noBackgroundCheckBox.Checked;
                hilightListBox.Refresh();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during save of save highlight entry");
                MessageBox.Show($"Error during save of entry.\r\n{ex.Message}");
            }
        }

        private void CheckRegex()
        {
            if (regexCheckBox.Checked)
            {
                if (string.IsNullOrWhiteSpace(searchStringTextBox.Text))
                {
                    throw new ArgumentException("Regex value is null or whitespace");
                }

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Regex.IsMatch("", searchStringTextBox.Text);
            }
        }

        private void StartEditEntry()
        {
            HilightEntry entry = (HilightEntry)hilightListBox.SelectedItem;

            if (entry != null)
            {
                searchStringTextBox.Text = entry.SearchText;

                foregroundColorBox.CustomColor = entry.ForegroundColor;
                backgroundColorBox.CustomColor = entry.BackgroundColor;

                if (foregroundColorBox.Items.Contains(entry.ForegroundColor))
                {
                    foregroundColorBox.SelectedIndex = foregroundColorBox.Items.Cast<Color>().ToList().LastIndexOf(entry.ForegroundColor);
                }
                else
                {
                    foregroundColorBox.SelectedItem = entry.ForegroundColor;
                }

                if (foregroundColorBox.Items.Contains(entry.ForegroundColor))
                {
                    backgroundColorBox.SelectedIndex = backgroundColorBox.Items.Cast<Color>().ToList().LastIndexOf(entry.BackgroundColor);
                }
                else
                {
                    backgroundColorBox.SelectedItem = entry.BackgroundColor;
                }

                regexCheckBox.Checked = entry.IsRegEx;
                caseSensitiveCheckBox.Checked = entry.IsCaseSensitive;
                ledCheckBox.Checked = entry.IsLedSwitch;
                bookmarkCheckBox.Checked = entry.IsSetBookmark;
                stopTailCheckBox.Checked = entry.IsStopTail;
                pluginCheckBox.Checked = entry.IsActionEntry;
                pluginButton.Enabled = pluginCheckBox.Checked;
                bookmarkCommentButton.Enabled = bookmarkCheckBox.Checked;
                _currentActionEntry = entry.ActionEntry != null ? entry.ActionEntry.Copy() : new ActionEntry();
                _bookmarkComment = entry.BookmarkComment;
                wordMatchCheckBox.Checked = entry.IsWordMatch;
                boldCheckBox.Checked = entry.IsBold;
                noBackgroundCheckBox.Checked = entry.NoBackground;
            }

            applyButton.Enabled = false;
            applyButton.Image = null;

            ReEvaluateHighlightButtonStates();
        }

        private void Dirty()
        {
            int index = hilightListBox.SelectedIndex;
            if (index > -1)
            {
                applyButton.Enabled = true;
                applyButton.Image = _applyButtonImage;
            }

            addButton.Enabled = searchStringTextBox.Text.Length > 0;
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
                groupComboBox.Items[index] = _currentGroup;
                groupComboBox.SelectedIndex = index;
                groupComboBox.SelectedItem = _currentGroup;
                FillHighlightListBox();
            }
            else
            {
                groupComboBox.SelectedItem = null;
                _currentGroup = null;
                hilightListBox.Items.Clear();
            }

            ReEvaluateHighlightButtonStates();
            ReEvaluateGroupButtonStates();
        }

        private void FillGroupComboBox()
        {
            SelectGroup(-1);

            groupComboBox.Items.Clear();

            foreach (HilightGroup group in HighlightGroupList)
            {
                groupComboBox.Items.Add(group);
            }

            ReEvaluateGroupButtonStates();
        }

        private void ReEvaluateHighlightButtonStates()
        {
            // Refresh button states based on the selection in the combobox
            bool atLeastOneSelected = hilightListBox.SelectedItem != null;
            bool moreThanOne = hilightListBox.Items.Count > 1;
            bool firstSelected = atLeastOneSelected && hilightListBox.SelectedIndex == 0;
            bool lastSelected = atLeastOneSelected &&
                                hilightListBox.SelectedIndex == hilightListBox.Items.Count - 1;

            deleteButton.Enabled = atLeastOneSelected;
            moveUpButton.Enabled = atLeastOneSelected && moreThanOne && !firstSelected;
            moveDownButton.Enabled = atLeastOneSelected && moreThanOne && !lastSelected;
        }

        private void ReEvaluateGroupButtonStates()
        {
            // Refresh button states based on the selection in the combobox
            bool atLeastOneSelected = groupComboBox.SelectedItem != null;
            bool moreThanOne = groupComboBox.Items.Count > 1;
            bool firstSelected = atLeastOneSelected && groupComboBox.SelectedIndex == 0;
            bool lastSelected = atLeastOneSelected &&
                                groupComboBox.SelectedIndex == groupComboBox.Items.Count - 1;

            delGroupButton.Enabled = atLeastOneSelected;
            copyGroupButton.Enabled = atLeastOneSelected;
            groupUpButton.Enabled = atLeastOneSelected && moreThanOne && !firstSelected;
            groupDownButton.Enabled = atLeastOneSelected && moreThanOne && !lastSelected;
        }

        #endregion

        #region Events handler

        private void OnHighlightDialogLoad(object sender, EventArgs e)
        {
            foregroundColorBox.SelectedIndex = 1;
            backgroundColorBox.SelectedIndex = 2;
            applyButton.Enabled = false;
            applyButton.Image = null;
            bookmarkCommentButton.Enabled = false;
            pluginButton.Enabled = false;

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
                OnApplyButtonClick(applyButton, EventArgs.Empty); 
            }
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            AddNewEntry();
            Dirty();
        }

        private void OnDeleteButtonClick(object sender, EventArgs e)
        {
            if (hilightListBox.SelectedIndex >= 0)
            {
                int removeIndex = hilightListBox.SelectedIndex;
                _currentGroup.HilightEntryList.RemoveAt(removeIndex);
                hilightListBox.Items.RemoveAt(removeIndex);

                // Select previous (or first if none before)
                int nextSelectIndex = removeIndex;
                if (nextSelectIndex >= hilightListBox.Items.Count)
                {
                    nextSelectIndex--; // if last item was removed, go one up
                }

                if (nextSelectIndex >= 0)
                {
                    hilightListBox.SelectedIndex = nextSelectIndex; // if still some item, select it
                }

                ReEvaluateHighlightButtonStates();
            }
        }

        private void OnHighlightListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                HilightEntry entry = (HilightEntry)hilightListBox.Items[e.Index];
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
            int index = hilightListBox.SelectedIndex;
            if (index > 0)
            {
                object item = hilightListBox.SelectedItem;
                hilightListBox.Items.RemoveAt(index); // will also clear the selection
                hilightListBox.Items.Insert(index - 1, item);
                hilightListBox.SelectedIndex = index - 1; // restore the selection
                _currentGroup.HilightEntryList.Reverse(index - 1, 2);
            }
        }

        private void OnMoveDownButtonClick(object sender, EventArgs e)
        {
            int index = hilightListBox.SelectedIndex;
            if (index > -1 && index < hilightListBox.Items.Count - 1)
            {
                object item = hilightListBox.SelectedItem;
                hilightListBox.Items.RemoveAt(index);
                hilightListBox.Items.Insert(index + 1, item);
                hilightListBox.SelectedIndex = index + 1;
                _currentGroup.HilightEntryList.Reverse(index, 2);
            }
        }

        private void OnCustomForeColorButtonClick(object sender, EventArgs e)
        {
            ChooseColor(foregroundColorBox);
            Dirty();
        }

        private void OnCustomBackColorButtonClick(object sender, EventArgs e)
        {
            ChooseColor(backgroundColorBox);
            Dirty();
        }

        private void OnRegexCheckBoxMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RegexHelperDialog dlg = new RegexHelperDialog();
                dlg.Owner = this;
                dlg.CaseSensitive = caseSensitiveCheckBox.Checked;
                dlg.Pattern = searchStringTextBox.Text;
                DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.OK)
                {
                    caseSensitiveCheckBox.Checked = dlg.CaseSensitive;
                    searchStringTextBox.Text = dlg.Pattern;
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
            pluginButton.Enabled = pluginCheckBox.Checked;
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

            if (groupComboBox.SelectedIndex >= 0 && groupComboBox.SelectedIndex < HighlightGroupList.Count)
            {
                int index = groupComboBox.SelectedIndex;
                HighlightGroupList.RemoveAt(groupComboBox.SelectedIndex);
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
            if (groupComboBox.SelectedIndex >= 0 && groupComboBox.SelectedIndex < HighlightGroupList.Count)
            {
                HilightGroup newGroup =
                    ObjectClone.Clone(HighlightGroupList[groupComboBox.SelectedIndex]);
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
            _currentGroup.GroupName = groupComboBox.Text;
        }

        private void OnGroupComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            SelectGroup(groupComboBox.SelectedIndex);
        }

        private void OnGroupUpButtonClick(object sender, EventArgs e)
        {
            int index = groupComboBox.SelectedIndex;
            if (index > 0)
            {
                _highlightGroupList.Reverse(index - 1, 2);
                groupComboBox.Refresh();
                FillGroupComboBox();
                SelectGroup(index - 1);
            }
        }

        private void OnGroupDownButtonClick(object sender, EventArgs e)
        {
            int index = groupComboBox.SelectedIndex;
            if (index > -1 && index < _highlightGroupList.Count - 1)
            {
                _highlightGroupList.Reverse(index, 2);
                groupComboBox.Refresh();
                FillGroupComboBox();
                SelectGroup(index + 1);
            }
        }

        private void OnWordMatchCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Dirty();
            noBackgroundCheckBox.Enabled = wordMatchCheckBox.Checked;
        }

        private void OnNoBackgroundCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            backgroundColorBox.Enabled = !noBackgroundCheckBox.Checked;
            customBackColorButton.Enabled = !noBackgroundCheckBox.Checked;
            Dirty();
        }

        private void OnBoldCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Dirty();
        }
        #endregion

    }
}