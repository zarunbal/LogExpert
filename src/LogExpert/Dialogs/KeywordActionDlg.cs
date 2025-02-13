using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LogExpert.Classes.Highlight;

namespace LogExpert.Dialogs
{
    public struct ActEntry
    {
        public string Name { get; set; }

        public IKeywordAction Plugin { get; set; }
    }


    public partial class KeywordActionDlg : Form
    {
        #region Fields

        private readonly IDictionary<string, IKeywordAction> _actionDict = new Dictionary<string, IKeywordAction>();

        private IList<IKeywordAction> _keywordActionList;

        #endregion

        #region cTor

        public KeywordActionDlg(ActionEntry entry, IList<IKeywordAction> actionList)
        {
            _keywordActionList = actionList;
            ActionEntry = entry;
            
            InitializeComponent();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            actionComboBox.Items.Clear();

            foreach (IKeywordAction action in actionList)
            {
                actionComboBox.Items.Add(action.GetName());
                _actionDict[action.GetName()] = action;
            }

            if (actionComboBox.Items.Count > 0)
            {
                if (ActionEntry.pluginName != null && _actionDict.ContainsKey(ActionEntry.pluginName))
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

        #region Properties

        public ActionEntry ActionEntry { get; private set; }

        #endregion

        #region Events handler

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            ActionEntry = new ActionEntry();
            ActionEntry.actionParam = parameterTextBox.Text;
            if (_actionDict.ContainsKey((string) actionComboBox.SelectedItem))
            {
                ActionEntry.pluginName = (string) actionComboBox.SelectedItem;
            }
        }

        private void actionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            commentTextBox.Text = _actionDict[(string) actionComboBox.SelectedItem].GetDescription();
        }

        #endregion
    }
}