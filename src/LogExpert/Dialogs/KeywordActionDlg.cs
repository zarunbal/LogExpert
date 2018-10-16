using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public struct ActEntry
    {
        #region Properties / Indexers

        public string Name { get; set; }

        public IKeywordAction Plugin { get; set; }

        #endregion
    }


    public partial class KeywordActionDlg : Form
    {
        #region Private Fields

        private readonly IDictionary<string, IKeywordAction> actionDict = new Dictionary<string, IKeywordAction>();
        private IList<IKeywordAction> keywordActionList;

        #endregion

        #region Ctor

        public KeywordActionDlg(ActionEntry entry, IList<IKeywordAction> actionList)
        {
            keywordActionList = actionList;
            ActionEntry = entry;
            InitializeComponent();
            actionComboBox.Items.Clear();
            foreach (IKeywordAction action in actionList)
            {
                actionComboBox.Items.Add(action.GetName());
                actionDict[action.GetName()] = action;
            }

            if (actionComboBox.Items.Count > 0)
            {
                if (ActionEntry.pluginName != null && actionDict.ContainsKey(ActionEntry.pluginName))
                {
                    actionComboBox.SelectedItem = ActionEntry.pluginName;
                }
                else
                {
                    actionComboBox.SelectedIndex = 0;
                }
            }

            parameterTextBox.Text = ActionEntry.actionParam;
        }

        #endregion

        #region Properties / Indexers

        public ActionEntry ActionEntry { get; private set; }

        #endregion

        #region Private Methods

        private void actionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            commentTextBox.Text = actionDict[(string)actionComboBox.SelectedItem].GetDescription();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            ActionEntry = new ActionEntry();
            ActionEntry.actionParam = parameterTextBox.Text;
            if (actionDict.ContainsKey((string)actionComboBox.SelectedItem))
            {
                ActionEntry.pluginName = (string)actionComboBox.SelectedItem;
            }
        }

        #endregion
    }
}
