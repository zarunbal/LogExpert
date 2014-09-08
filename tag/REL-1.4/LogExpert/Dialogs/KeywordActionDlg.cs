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
    private string name;
    private IKeywordAction plugin;

    public string Name
    {
      get { return this.name; }
      set { this.name = value; }
    }

    public IKeywordAction Plugin
    {
      get { return this.plugin; }
      set { this.plugin = value; }
    }
  }


  public partial class KeywordActionDlg : Form
  {
    IList<IKeywordAction>   keywordActionList;
    private ActionEntry   actionEntry;
    IDictionary<string, IKeywordAction> actionDict = new Dictionary<string, IKeywordAction>();

    public KeywordActionDlg(ActionEntry entry, IList<IKeywordAction> actionList)
    {
      this.keywordActionList = actionList;
      this.actionEntry = entry;
      InitializeComponent();
      this.actionComboBox.Items.Clear();
      foreach (IKeywordAction action in actionList)
      {
        this.actionComboBox.Items.Add(action.GetName());
        this.actionDict[action.GetName()] = action;
      }
      if (this.actionComboBox.Items.Count > 0)
      {
        if (this.actionEntry.pluginName != null && this.actionDict.ContainsKey(this.actionEntry.pluginName))
        {
          this.actionComboBox.SelectedItem = this.actionEntry.pluginName;
        }
        else
        {
          this.actionComboBox.SelectedIndex = 0;
        }
      }
      this.parameterTextBox.Text = this.actionEntry.actionParam;
    }


    public ActionEntry ActionEntry
    {
      get { return this.actionEntry; }
    }


    private void okButton_Click(object sender, EventArgs e)
    {
      actionEntry = new ActionEntry();
      actionEntry.actionParam = this.parameterTextBox.Text;
      if (this.actionDict.ContainsKey((string)this.actionComboBox.SelectedItem))
      {
        actionEntry.pluginName = (string)this.actionComboBox.SelectedItem;
      }
    }

    private void actionComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.commentTextBox.Text = this.actionDict[(string)this.actionComboBox.SelectedItem].GetDescription();
    }

  }
}
