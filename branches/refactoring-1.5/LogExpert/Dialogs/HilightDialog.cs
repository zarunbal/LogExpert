using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class HilightDialog : Form
	{
		private List<HilightGroup> hilightGroupList;
		private ActionEntry currentActionEntry = new ActionEntry();
		private string bookmarkComment;
		private HilightGroup currentGroup = null;
		private string preSelectedGroupName = null;

		Image applyButtonImage;

		public HilightDialog()
		{
			InitializeComponent();
			this.Load += new EventHandler(HilightDialog_Load);
			this.hilightListBox.DrawItem += new DrawItemEventHandler(hilightListBox_DrawItem);
			applyButtonImage = this.applyButton.Image;
			this.applyButton.Image = null;
		}

		private void HilightDialog_Load(object sender, EventArgs e)
		{
			this.foregroundColorBox.SelectedIndex = 1;
			this.backgroundColorBox.SelectedIndex = 2;
			this.applyButton.Enabled = false;
			this.applyButton.Image = null;
			this.bookmarkCommentButton.Enabled = false;
			this.pluginButton.Enabled = false;

			ReEvaluateHilightButtonStates();
		}

		private void HilightDialog_Shown(object sender, EventArgs e)
		{
			InitData();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			// Apply pending changes if closing the form.
			if (IsDirty)
				applyButton_Click(applyButton, new EventArgs()); // cannot call 'this.applyButton.PerformClick();' because it prohibits the OK button to terminate the dialog
		}

		private void AddButton_Click(object sender, EventArgs e)
		{
			AddNewEntry();
		}

		private void AddNewEntry()
		{
			if (this.searchStringTextBox.Text.Length > 0)
			{
				// Create a new entry
				HilightEntry entry = new HilightEntry(this.searchStringTextBox.Text,
					this.foregroundColorBox.SelectedColor,
					this.backgroundColorBox.SelectedColor,
					this.regexCheckBox.Checked,
					this.caseSensitiveCheckBox.Checked,
					this.ledCheckBox.Checked,
					this.stopTailCheckBox.Checked,
					this.bookmarkCheckBox.Checked,
					this.pluginCheckBox.Checked,
					this.currentActionEntry,
					this.wordMatchCheckBox.Checked);
				entry.IsBold = this.boldCheckBox.Checked;
				entry.NoBackground = this.noBackgroundCheckBox.Checked;
				this.hilightListBox.Items.Add(entry);

				// Select the newly created item
				this.currentGroup.HilightEntryList.Add(entry);
				this.hilightListBox.SelectedItem = entry;
			}
		}

		private void deleteButton_Click(object sender, EventArgs e)
		{
			if (this.hilightListBox.SelectedIndex >= 0)
			{
				int removeIndex = this.hilightListBox.SelectedIndex;
				this.currentGroup.HilightEntryList.RemoveAt(removeIndex);
				this.hilightListBox.Items.RemoveAt(removeIndex);

				// Select previous (or first if none before)
				int nextSelectIndex = removeIndex;
				if (nextSelectIndex >= this.hilightListBox.Items.Count)
					nextSelectIndex--; // if last item was removed, go one up
				if (nextSelectIndex >= 0)
					this.hilightListBox.SelectedIndex = nextSelectIndex; // if still some item, select it

				ReEvaluateHilightButtonStates();
			}
		}

		private void hilightListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index >= 0)
			{
				HilightEntry entry = (HilightEntry)this.hilightListBox.Items[e.Index];
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

		private void InitData()
		{
			const string def = "[Default]";
			if (this.HilightGroupList == null || this.HilightGroupList.Count == 0)
			{
				this.HilightGroupList.Add(new HilightGroup());
				this.HilightGroupList[0].GroupName = def;
				this.HilightGroupList[0].HilightEntryList = new List<HilightEntry>();
			}
			FillGroupComboBox();

			this.currentGroup = null;
			string groupToSelect = this.PreSelectedGroupName;
			if (string.IsNullOrEmpty(groupToSelect))
				groupToSelect = def;

			foreach (HilightGroup group in this.HilightGroupList)
			{
				if (group.GroupName.Equals(groupToSelect))
				{
					this.currentGroup = group;
					this.groupComboBox.SelectedValue = group;
					this.groupComboBox.SelectedIndex = this.HilightGroupList.IndexOf(group);
					break;
				}
			}

			ReEvaluateGroupButtonStates();

			FillHilightListBox();
		}

		private void FillHilightListBox()
		{
			this.hilightListBox.Items.Clear();
			if (this.currentGroup != null)
			{
				foreach (HilightEntry entry in this.currentGroup.HilightEntryList)
				{
					this.hilightListBox.Items.Add(entry);
				}
			}
		}

		public List<HilightGroup> HilightGroupList
		{
			get
			{
				return this.hilightGroupList;
			}
			set 
			{
				this.hilightGroupList = ObjectClone.Clone<List<HilightGroup>>(value);
			}
		}

		public IList<IKeywordAction> KeywordActionList { get; set; }

		public string PreSelectedGroupName
		{
			get
			{
				return this.preSelectedGroupName;
			}
			set
			{
				this.preSelectedGroupName = value;
			}
		}

		private void applyButton_Click(object sender, EventArgs e)
		{
			SaveEntry();
		}

		private void SaveEntry()
		{
			HilightEntry entry = (HilightEntry)this.hilightListBox.SelectedItem;
			//if (entry == null)
			//{
			//  AddNewEntry();
			//  return;
			//}

			entry.ForegroundColor = (Color)this.foregroundColorBox.SelectedItem;
			entry.BackgroundColor = (Color)this.backgroundColorBox.SelectedItem;
			entry.SearchText = this.searchStringTextBox.Text;
			entry.IsRegEx = this.regexCheckBox.Checked;
			entry.IsCaseSensitive = this.caseSensitiveCheckBox.Checked;
			this.applyButton.Enabled = false;
			this.applyButton.Image = null;
			entry.IsLedSwitch = this.ledCheckBox.Checked;
			entry.IsSetBookmark = this.bookmarkCheckBox.Checked;
			entry.IsStopTail = this.stopTailCheckBox.Checked;
			entry.IsActionEntry = this.pluginCheckBox.Checked;
			entry.ActionEntry = this.currentActionEntry.Copy();
			entry.BookmarkComment = this.bookmarkComment;
			entry.IsWordMatch = this.wordMatchCheckBox.Checked;
			entry.IsBold = this.boldCheckBox.Checked;
			entry.NoBackground = this.noBackgroundCheckBox.Checked;
			this.hilightListBox.Refresh();
		}

		private void StartEditEntry()
		{
			HilightEntry entry = (HilightEntry)this.hilightListBox.SelectedItem;
			if (entry != null)
			{
				this.searchStringTextBox.Text = entry.SearchText;
				this.foregroundColorBox.CustomColor = entry.ForegroundColor;
				this.backgroundColorBox.CustomColor = entry.BackgroundColor;
				this.foregroundColorBox.SelectedItem = entry.ForegroundColor;
				this.backgroundColorBox.SelectedItem = entry.BackgroundColor;
				this.regexCheckBox.Checked = entry.IsRegEx;
				this.caseSensitiveCheckBox.Checked = entry.IsCaseSensitive;
				this.ledCheckBox.Checked = entry.IsLedSwitch;
				this.bookmarkCheckBox.Checked = entry.IsSetBookmark;
				this.stopTailCheckBox.Checked = entry.IsStopTail;
				this.pluginCheckBox.Checked = entry.IsActionEntry;
				pluginButton.Enabled = pluginCheckBox.Checked;
				this.bookmarkCommentButton.Enabled = this.bookmarkCheckBox.Checked;
				this.currentActionEntry = entry.ActionEntry != null ? entry.ActionEntry.Copy() : new ActionEntry();
				this.bookmarkComment = entry.BookmarkComment;
				this.wordMatchCheckBox.Checked = entry.IsWordMatch;
				this.boldCheckBox.Checked = entry.IsBold;
				this.noBackgroundCheckBox.Checked = entry.NoBackground;
			}
			this.applyButton.Enabled = false;
			this.applyButton.Image = null;

			ReEvaluateHilightButtonStates();
		}

		private bool IsDirty
		{
			get
			{
				return this.applyButton.Image == this.applyButtonImage;
			}
		}

		private void Dirty()
		{
			int index = this.hilightListBox.SelectedIndex;
			if (index > -1)
			{
				this.applyButton.Enabled = true;
				this.applyButton.Image = this.applyButtonImage;
			}

			this.addButton.Enabled = (this.searchStringTextBox.Text.Length > 0);
		}

		private void hilightListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			StartEditEntry();
		}

		private void moveUpButton_Click(object sender, EventArgs e)
		{
			int index = this.hilightListBox.SelectedIndex;
			if (index > 0)
			{
				object item = this.hilightListBox.SelectedItem;
				this.hilightListBox.Items.RemoveAt(index); // will also clear the selection
				this.hilightListBox.Items.Insert(index - 1, item);
				this.hilightListBox.SelectedIndex = index - 1; // restore the selection
				this.currentGroup.HilightEntryList.Reverse(index - 1, 2);
			}
		}

		private void moveDownButton_Click(object sender, EventArgs e)
		{
			int index = this.hilightListBox.SelectedIndex;
			if (index > -1 && index < this.hilightListBox.Items.Count - 1)
			{
				object item = this.hilightListBox.SelectedItem;
				this.hilightListBox.Items.RemoveAt(index);
				this.hilightListBox.Items.Insert(index + 1, item);
				this.hilightListBox.SelectedIndex = index + 1;
				this.currentGroup.HilightEntryList.Reverse(index, 2);
			}
		}

		private void customForeColorButton_Click(object sender, EventArgs e)
		{
			ChooseColor(this.foregroundColorBox);
			Dirty();
		}

		private void customBackColorButton_Click(object sender, EventArgs e)
		{
			ChooseColor(this.backgroundColorBox);
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

		private void SelectGroup(int index)
		{
			if (index >= 0 && index < this.HilightGroupList.Count)
			{
				this.currentGroup = this.HilightGroupList[index];
				this.groupComboBox.Items[index] = this.currentGroup;
				this.groupComboBox.SelectedIndex = index;
				//this.groupComboBox.Text = this.currentGroup.GroupName;
				this.groupComboBox.SelectedItem = this.currentGroup;
				FillHilightListBox();
			}
			else
			{
				this.currentGroup = null;
				this.hilightListBox.Items.Clear();
			}

			ReEvaluateHilightButtonStates();
			ReEvaluateGroupButtonStates();
		}

		private void FillGroupComboBox()
		{
			this.groupComboBox.Items.Clear();
			foreach (HilightGroup group in this.HilightGroupList)
			{
				this.groupComboBox.Items.Add(group);
			}
			ReEvaluateGroupButtonStates();
		}

		private void ReEvaluateHilightButtonStates()
		{
			// Refresh button states based on the selection in the combobox
			bool atLeastOneSelected = (this.hilightListBox.SelectedItem != null);
			bool moreThanOne = (this.hilightListBox.Items.Count > 1);
			bool firstSelected = atLeastOneSelected && (this.hilightListBox.SelectedIndex == 0);
			bool lastSelected = atLeastOneSelected && (this.hilightListBox.SelectedIndex == this.hilightListBox.Items.Count - 1);

			this.deleteButton.Enabled = atLeastOneSelected;
			this.moveUpButton.Enabled = atLeastOneSelected && moreThanOne && !firstSelected;
			this.moveDownButton.Enabled = atLeastOneSelected && moreThanOne && !lastSelected;
		}

		private void regexCheckBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				RegexHelperDialog dlg = new RegexHelperDialog();
				dlg.Owner = this;
				dlg.CaseSensitive = this.caseSensitiveCheckBox.Checked;
				dlg.Pattern = this.searchStringTextBox.Text;
				DialogResult res = dlg.ShowDialog();
				if (res == DialogResult.OK)
				{
					this.caseSensitiveCheckBox.Checked = dlg.CaseSensitive;
					this.searchStringTextBox.Text = dlg.Pattern;
				}
			}
		}

		private void searchStringTextBox_TextChanged(object sender, EventArgs e)
		{
			Dirty();
		}

		private void caseSensitiveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Dirty();
		}

		private void regexCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Dirty();
		}

		private void ledCheckBox_CheckedChanged(object sender, EventArgs e)
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

		private void foregroundColorBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Dirty();
		}

		private void backgroundColorBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Dirty();
		}

		private void pluginCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Dirty();
			pluginButton.Enabled = pluginCheckBox.Checked;
		}

		private void pluginButton_Click(object sender, EventArgs e)
		{
			KeywordActionDlg dlg = new KeywordActionDlg(this.currentActionEntry, this.KeywordActionList);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				this.currentActionEntry = dlg.ActionEntry;
				Dirty();
			}
		}

		private void bookmarkCommentButton_Click(object sender, EventArgs e)
		{
			BookmarkCommentDlg dlg = new BookmarkCommentDlg();
			dlg.Comment = this.bookmarkComment;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				this.bookmarkComment = dlg.Comment;
				Dirty();
			}
		}

		private void ReEvaluateGroupButtonStates()
		{
			// Refresh button states based on the selection in the combobox
			bool atLeastOneSelected = (this.groupComboBox.SelectedItem != null);
			bool moreThanOne = (this.groupComboBox.Items.Count > 1);
			bool firstSelected = atLeastOneSelected && (this.groupComboBox.SelectedIndex == 0);
			bool lastSelected = atLeastOneSelected && (this.groupComboBox.SelectedIndex == this.groupComboBox.Items.Count - 1);

			this.delGroupButton.Enabled = atLeastOneSelected;
			this.copyGroupButton.Enabled = atLeastOneSelected;
			this.groupUpButton.Enabled = atLeastOneSelected && moreThanOne && !firstSelected;
			this.groupDownButton.Enabled = atLeastOneSelected && moreThanOne && !lastSelected;
		}

		private void delGroupButton_Click(object sender, EventArgs e)
		{
			// the last group cannot be deleted
			if (this.HilightGroupList.Count == 1)
				return;

			if (this.groupComboBox.SelectedIndex >= 0 && this.groupComboBox.SelectedIndex < this.HilightGroupList.Count)
			{
				int index = this.groupComboBox.SelectedIndex;
				this.HilightGroupList.RemoveAt(this.groupComboBox.SelectedIndex);
				FillGroupComboBox();
				if (index < this.HilightGroupList.Count)
					SelectGroup(index);
				else
					SelectGroup(this.HilightGroupList.Count - 1);
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
				uniqueName = (this.HilightGroupList.FindIndex(delegate(HilightGroup g) { return g.GroupName == name; }) < 0);
				if (!uniqueName)
					name = string.Format("{0} #{1}", baseName, i++);
			}

			HilightGroup newGroup = new HilightGroup() { GroupName = name };
			this.HilightGroupList.Add(newGroup);
			FillGroupComboBox();
			SelectGroup(this.HilightGroupList.Count - 1);
		}

		private void copyGroupButton_Click(object sender, EventArgs e)
		{
			if (this.groupComboBox.SelectedIndex >= 0 && this.groupComboBox.SelectedIndex < this.HilightGroupList.Count)
			{
				HilightGroup newGroup = ObjectClone.Clone<HilightGroup>(this.HilightGroupList[this.groupComboBox.SelectedIndex]);
				newGroup.GroupName = "Copy of " + newGroup.GroupName;
				this.HilightGroupList.Add(newGroup);
				FillGroupComboBox();
				SelectGroup(this.HilightGroupList.Count - 1);
			}
		}

		private void groupComboBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index >= 0)
			{
				HilightGroup group = this.HilightGroupList[e.Index];
				Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

				Brush brush = new SolidBrush(SystemColors.ControlText);
				e.Graphics.DrawString(group.GroupName, e.Font, brush, new PointF(rectangle.Left, rectangle.Top));
				e.DrawFocusRectangle();
				brush.Dispose();
			}
		}

		private void groupComboBox_TextUpdate(object sender, EventArgs e)
		{
			this.currentGroup.GroupName = this.groupComboBox.Text;
		}

		private void groupComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			SelectGroup(this.groupComboBox.SelectedIndex);
		}

		private void groupUpButton_Click(object sender, EventArgs e)
		{
			int index = this.groupComboBox.SelectedIndex;
			if (index > 0)
			{
				this.hilightGroupList.Reverse(index - 1, 2);
				this.groupComboBox.Refresh();
				FillGroupComboBox();
				SelectGroup(index - 1);
			}
		}

		private void groupDownButton_Click(object sender, EventArgs e)
		{
			int index = this.groupComboBox.SelectedIndex;
			if (index > -1 && index < this.hilightGroupList.Count - 1)
			{
				this.hilightGroupList.Reverse(index, 2);
				this.groupComboBox.Refresh();
				FillGroupComboBox();
				SelectGroup(index + 1);
			}
		}

		private void wordMatchCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Dirty();
			this.noBackgroundCheckBox.Enabled = this.wordMatchCheckBox.Checked;
		}

		private void boldCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Dirty();
		}

		private void noBackgroundCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.backgroundColorBox.Enabled = !this.noBackgroundCheckBox.Checked;
			this.customBackColorButton.Enabled = !this.noBackgroundCheckBox.Checked;
			Dirty();
		}
	}
}