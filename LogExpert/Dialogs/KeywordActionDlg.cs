using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public struct ActEntry
	{
		public string Name { get; set; }

		public IKeywordAction Plugin { get; set; }
	}

	public partial class KeywordActionDlg : Form
	{
		IList<IKeywordAction> keywordActionList;
		IDictionary<string, IKeywordAction> actionDict = new Dictionary<string, IKeywordAction>();

		public KeywordActionDlg(ActionEntry entry, IList<IKeywordAction> actionList)
		{
			this.keywordActionList = actionList;
			this.ActionEntry = entry;
			InitializeComponent();
			this.actionComboBox.Items.Clear();
			foreach (IKeywordAction action in actionList)
			{
				this.actionComboBox.Items.Add(action.GetName());
				this.actionDict[action.GetName()] = action;
			}
			if (this.actionComboBox.Items.Count > 0)
			{
				if (this.ActionEntry.pluginName != null && this.actionDict.ContainsKey(this.ActionEntry.pluginName))
				{
					this.actionComboBox.SelectedItem = this.ActionEntry.pluginName;
				}
				else
				{
					this.actionComboBox.SelectedIndex = 0;
				}
			}
			this.parameterTextBox.Text = this.ActionEntry.actionParam;
		}

		public ActionEntry ActionEntry { get; private set; }

		private void okButton_Click(object sender, EventArgs e)
		{
			ActionEntry = new ActionEntry();
			ActionEntry.actionParam = this.parameterTextBox.Text;
			if (this.actionDict.ContainsKey((string)this.actionComboBox.SelectedItem))
			{
				ActionEntry.pluginName = (string)this.actionComboBox.SelectedItem;
			}
		}

		private void actionComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.commentTextBox.Text = this.actionDict[(string)this.actionComboBox.SelectedItem].GetDescription();
		}
	}
}