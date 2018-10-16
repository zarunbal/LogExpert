using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// using System.Linq;
namespace LogExpert.Dialogs
{
    public partial class HilightDialog : Form
    {
        #region Private Fields

        private readonly Image applyButtonImage;
        private string bookmarkComment;
        private ActionEntry currentActionEntry = new ActionEntry();
        private HilightGroup currentGroup;
        private List<HilightGroup> hilightGroupList;

        #endregion

        #region Ctor

        public HilightDialog()
        {
            InitializeComponent();
            Load += HilightDialog_Load;
            hilightListBox.DrawItem += hilightListBox_DrawItem;
            applyButtonImage = applyButton.Image;
            applyButton.Image = null;
        }

        #endregion

        #region Properties / Indexers

        public List<HilightGroup> HilightGroupList
        {
            get => hilightGroupList;
            set => hilightGroupList = ObjectClone.Clone(value);
        }

        public IList<IKeywordAction> KeywordActionList { get; set; }

        public string PreSelectedGroupName { get; set; } = null;

        private bool IsDirty => applyButton.Image == applyButtonImage;

        #endregion

        #region Event raising Methods

        private void groupDownButton_Click(object sender, EventArgs e)
        {
            int index = groupComboBox.SelectedIndex;
            if (index > -1 && index < hilightGroupList.Count - 1)
            {
                hilightGroupList.Reverse(index, 2);
                groupComboBox.Refresh();
                FillGroupComboBox();
                SelectGroup(index + 1);
            }
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            int index = hilightListBox.SelectedIndex;
            if (index > -1 && index < hilightListBox.Items.Count - 1)
            {
                object item = hilightListBox.SelectedItem;
                hilightListBox.Items.RemoveAt(index);
                hilightListBox.Items.Insert(index + 1, item);
                hilightListBox.SelectedIndex = index + 1;
                currentGroup.HilightEntryList.Reverse(index, 2);
            }
        }

        #endregion

        #region Private Methods

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddNewEntry();
        }

        private void AddNewEntry()
        {
            if (searchStringTextBox.Text.Length > 0)
            {
                // Create a new entry
                HilightEntry entry = new HilightEntry(searchStringTextBox.Text,
                    foregroundColorBox.SelectedColor,
                    backgroundColorBox.SelectedColor,
                    regexCheckBox.Checked,
                    caseSensitiveCheckBox.Checked,
                    ledCheckBox.Checked,
                    stopTailCheckBox.Checked,
                    bookmarkCheckBox.Checked,
                    pluginCheckBox.Checked,
                    currentActionEntry,
                    wordMatchCheckBox.Checked);
                entry.IsBold = boldCheckBox.Checked;
                entry.NoBackground = noBackgroundCheckBox.Checked;
                hilightListBox.Items.Add(entry);

                // Select the newly created item
                currentGroup.HilightEntryList.Add(entry);
                hilightListBox.SelectedItem = entry;
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            SaveEntry();
        }

        private void backgroundColorBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void boldCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void bookmarkCommentButton_Click(object sender, EventArgs e)
        {
            BookmarkCommentDlg dlg = new BookmarkCommentDlg();
            dlg.Comment = bookmarkComment;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                bookmarkComment = dlg.Comment;
                Dirty();
            }
        }

        private void caseSensitiveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
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

        private void copyGroupButton_Click(object sender, EventArgs e)
        {
            if (groupComboBox.SelectedIndex >= 0 && groupComboBox.SelectedIndex < HilightGroupList.Count)
            {
                HilightGroup newGroup =
                    ObjectClone.Clone(HilightGroupList[groupComboBox.SelectedIndex]);
                newGroup.GroupName = "Copy of " + newGroup.GroupName;
                HilightGroupList.Add(newGroup);
                FillGroupComboBox();
                SelectGroup(HilightGroupList.Count - 1);
            }
        }

        private void customBackColorButton_Click(object sender, EventArgs e)
        {
            ChooseColor(backgroundColorBox);
            Dirty();
        }

        private void customForeColorButton_Click(object sender, EventArgs e)
        {
            ChooseColor(foregroundColorBox);
            Dirty();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (hilightListBox.SelectedIndex >= 0)
            {
                int removeIndex = hilightListBox.SelectedIndex;
                currentGroup.HilightEntryList.RemoveAt(removeIndex);
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

                ReEvaluateHilightButtonStates();
            }
        }

        private void delGroupButton_Click(object sender, EventArgs e)
        {
            // the last group cannot be deleted
            if (HilightGroupList.Count == 1)
            {
                return;
            }

            if (groupComboBox.SelectedIndex >= 0 && groupComboBox.SelectedIndex < HilightGroupList.Count)
            {
                int index = groupComboBox.SelectedIndex;
                HilightGroupList.RemoveAt(groupComboBox.SelectedIndex);
                FillGroupComboBox();
                if (index < HilightGroupList.Count)
                {
                    SelectGroup(index);
                }
                else
                {
                    SelectGroup(HilightGroupList.Count - 1);
                }
            }
        }

        private void Dirty()
        {
            int index = hilightListBox.SelectedIndex;
            if (index > -1)
            {
                applyButton.Enabled = true;
                applyButton.Image = applyButtonImage;
            }

            addButton.Enabled = searchStringTextBox.Text.Length > 0;
        }

        private void FillGroupComboBox()
        {
            groupComboBox.Items.Clear();
            foreach (HilightGroup group in HilightGroupList)
            {
                groupComboBox.Items.Add(group);
            }

            ReEvaluateGroupButtonStates();
        }

        private void FillHilightListBox()
        {
            hilightListBox.Items.Clear();
            if (currentGroup != null)
            {
                foreach (HilightEntry entry in currentGroup.HilightEntryList)
                {
                    hilightListBox.Items.Add(entry);
                }
            }
        }

        private void foregroundColorBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void groupComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                HilightGroup group = HilightGroupList[e.Index];
                Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

                Brush brush = new SolidBrush(SystemColors.ControlText);
                e.Graphics.DrawString(group.GroupName, e.Font, brush, new PointF(rectangle.Left, rectangle.Top));
                e.DrawFocusRectangle();
                brush.Dispose();
            }
        }

        private void groupComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SelectGroup(groupComboBox.SelectedIndex);
        }

        private void groupComboBox_TextUpdate(object sender, EventArgs e)
        {
            currentGroup.GroupName = groupComboBox.Text;
        }

        private void groupUpButton_Click(object sender, EventArgs e)
        {
            int index = groupComboBox.SelectedIndex;
            if (index > 0)
            {
                hilightGroupList.Reverse(index - 1, 2);
                groupComboBox.Refresh();
                FillGroupComboBox();
                SelectGroup(index - 1);
            }
        }

        private void HilightDialog_Load(object sender, EventArgs e)
        {
            foregroundColorBox.SelectedIndex = 1;
            backgroundColorBox.SelectedIndex = 2;
            applyButton.Enabled = false;
            applyButton.Image = null;
            bookmarkCommentButton.Enabled = false;
            pluginButton.Enabled = false;

            ReEvaluateHilightButtonStates();
        }

        private void HilightDialog_Shown(object sender, EventArgs e)
        {
            InitData();
        }

        private void hilightListBox_DrawItem(object sender, DrawItemEventArgs e)
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

        private void hilightListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartEditEntry();
        }

        private void InitData()
        {
            const string def = "[Default]";
            if (HilightGroupList == null || HilightGroupList.Count == 0)
            {
                HilightGroupList.Add(new HilightGroup());
                HilightGroupList[0].GroupName = def;
                HilightGroupList[0].HilightEntryList = new List<HilightEntry>();
            }

            FillGroupComboBox();

            currentGroup = null;
            string groupToSelect = PreSelectedGroupName;
            if (string.IsNullOrEmpty(groupToSelect))
            {
                groupToSelect = def;
            }

            foreach (HilightGroup group in HilightGroupList)
            {
                if (group.GroupName.Equals(groupToSelect))
                {
                    currentGroup = group;
                    groupComboBox.SelectedValue = group;
                    groupComboBox.SelectedIndex = HilightGroupList.IndexOf(group);
                    break;
                }
            }

            ReEvaluateGroupButtonStates();

            FillHilightListBox();
        }

        private void ledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            int index = hilightListBox.SelectedIndex;
            if (index > 0)
            {
                object item = hilightListBox.SelectedItem;
                hilightListBox.Items.RemoveAt(index); // will also clear the selection
                hilightListBox.Items.Insert(index - 1, item);
                hilightListBox.SelectedIndex = index - 1; // restore the selection
                currentGroup.HilightEntryList.Reverse(index - 1, 2);
            }
        }

        private void newGroupButton_Click(object sender, EventArgs e)
        {
            // Propose a unique name
            const string baseName = "New group";
            string name = baseName;
            bool uniqueName = false;
            int i = 1;
            while (!uniqueName)
            {
                uniqueName = HilightGroupList.FindIndex(delegate(HilightGroup g) { return g.GroupName == name; }) <
                             0;
                if (!uniqueName)
                {
                    name = string.Format("{0} #{1}", baseName, i++);
                }
            }

            HilightGroup newGroup = new HilightGroup {GroupName = name};
            HilightGroupList.Add(newGroup);
            FillGroupComboBox();
            SelectGroup(HilightGroupList.Count - 1);
        }

        private void noBackgroundCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            backgroundColorBox.Enabled = !noBackgroundCheckBox.Checked;
            customBackColorButton.Enabled = !noBackgroundCheckBox.Checked;
            Dirty();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // Apply pending changes if closing the form.
            if (IsDirty)
            {
                applyButton_Click(applyButton,
                    new EventArgs()); // cannot call 'this.applyButton.PerformClick();' because it prohibits the OK button to terminate the dialog
            }
        }

        private void pluginButton_Click(object sender, EventArgs e)
        {
            KeywordActionDlg dlg = new KeywordActionDlg(currentActionEntry, KeywordActionList);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                currentActionEntry = dlg.ActionEntry;
                Dirty();
            }
        }

        private void pluginCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
            pluginButton.Enabled = pluginCheckBox.Checked;
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

        private void ReEvaluateHilightButtonStates()
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

        private void regexCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void regexCheckBox_MouseUp(object sender, MouseEventArgs e)
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

        private void SaveEntry()
        {
            HilightEntry entry = (HilightEntry)hilightListBox.SelectedItem;

// if (entry == null)
            // {
            // AddNewEntry();
            // return;
            // }
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
            entry.ActionEntry = currentActionEntry.Copy();
            entry.BookmarkComment = bookmarkComment;
            entry.IsWordMatch = wordMatchCheckBox.Checked;
            entry.IsBold = boldCheckBox.Checked;
            entry.NoBackground = noBackgroundCheckBox.Checked;
            hilightListBox.Refresh();
        }

        private void searchStringTextBox_TextChanged(object sender, EventArgs e)
        {
            Dirty();
        }

        private void SelectGroup(int index)
        {
            if (index >= 0 && index < HilightGroupList.Count)
            {
                currentGroup = HilightGroupList[index];
                groupComboBox.Items[index] = currentGroup;
                groupComboBox.SelectedIndex = index;

// this.groupComboBox.Text = this.currentGroup.GroupName;
                groupComboBox.SelectedItem = currentGroup;
                FillHilightListBox();
            }
            else
            {
                currentGroup = null;
                hilightListBox.Items.Clear();
            }

            ReEvaluateHilightButtonStates();
            ReEvaluateGroupButtonStates();
        }

        private void StartEditEntry()
        {
            HilightEntry entry = (HilightEntry)hilightListBox.SelectedItem;
            if (entry != null)
            {
                searchStringTextBox.Text = entry.SearchText;
                foregroundColorBox.CustomColor = entry.ForegroundColor;
                backgroundColorBox.CustomColor = entry.BackgroundColor;
                foregroundColorBox.SelectedItem = entry.ForegroundColor;
                backgroundColorBox.SelectedItem = entry.BackgroundColor;
                regexCheckBox.Checked = entry.IsRegEx;
                caseSensitiveCheckBox.Checked = entry.IsCaseSensitive;
                ledCheckBox.Checked = entry.IsLedSwitch;
                bookmarkCheckBox.Checked = entry.IsSetBookmark;
                stopTailCheckBox.Checked = entry.IsStopTail;
                pluginCheckBox.Checked = entry.IsActionEntry;
                pluginButton.Enabled = pluginCheckBox.Checked;
                bookmarkCommentButton.Enabled = bookmarkCheckBox.Checked;
                currentActionEntry = entry.ActionEntry != null ? entry.ActionEntry.Copy() : new ActionEntry();
                bookmarkComment = entry.BookmarkComment;
                wordMatchCheckBox.Checked = entry.IsWordMatch;
                boldCheckBox.Checked = entry.IsBold;
                noBackgroundCheckBox.Checked = entry.NoBackground;
            }

            applyButton.Enabled = false;
            applyButton.Image = null;

            ReEvaluateHilightButtonStates();
        }

        private void wordMatchCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Dirty();
            noBackgroundCheckBox.Enabled = wordMatchCheckBox.Checked;
        }

        #endregion
    }
}
