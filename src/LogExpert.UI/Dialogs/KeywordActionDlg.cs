using System.Runtime.Versioning;

using ColumnizerLib;

using LogExpert.Core.Classes.Highlight;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class KeywordActionDlg : Form
{
    #region Fields

    private readonly IDictionary<string, IKeywordAction> _actionDict = new Dictionary<string, IKeywordAction>();

    private readonly IList<IKeywordAction> _keywordActionList;

    #endregion

    #region cTor

    public KeywordActionDlg (ActionEntry entry, IList<IKeywordAction> actionList)
    {
        SuspendLayout();

        _keywordActionList = actionList;
        ActionEntry = entry;

        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ApplyResources();

        actionComboBox.Items.Clear();

        foreach (var action in _keywordActionList)
        {
            _ = actionComboBox.Items.Add(action.GetName());
            _actionDict[action.GetName()] = action;
        }

        if (actionComboBox.Items.Count > 0)
        {
            if (ActionEntry.PluginName != null && _actionDict.ContainsKey(ActionEntry.PluginName))
            {
                actionComboBox.SelectedItem = ActionEntry.PluginName;
            }
            else
            {
                actionComboBox.SelectedIndex = 0;
            }
        }

        parameterTextBox.Text = ActionEntry.ActionParam;

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.KeywordActionDlg_UI_Title;
        label1.Text = Resources.KeywordActionDlg_UI_Label_KeywordActionPlugin;
        label2.Text = Resources.KeywordActionDlg_UI_Label_Parameter;
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
    }

    #endregion

    #region Properties

    public ActionEntry ActionEntry { get; private set; }

    #endregion

    #region Events handler

    private void OnOkButtonClick (object sender, EventArgs e)
    {
        ActionEntry = new ActionEntry
        {
            ActionParam = parameterTextBox.Text
        };

        if (_actionDict.ContainsKey((string)actionComboBox.SelectedItem))
        {
            ActionEntry.PluginName = (string)actionComboBox.SelectedItem;
        }
    }

    private void OnActionComboBoxSelectedIndexChanged (object sender, EventArgs e)
    {
        commentTextBox.Text = _actionDict[(string)actionComboBox.SelectedItem].GetDescription();
    }

    #endregion
}