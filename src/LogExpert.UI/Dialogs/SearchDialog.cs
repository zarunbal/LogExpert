using System.ComponentModel;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

using LogExpert.Core.Helpers;
using LogExpert.Entities;
using LogExpert.UI.Dialogs;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class SearchDialog : Form
{
    #region Fields

    private const int MAX_HISTORY = 30;

    #endregion

    #region cTor

    public SearchDialog ()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        Load += OnSearchDialogLoad;

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
        labelSearchFor.Text = Resources.SearchDialog_UI_Label_SearchFor;
        checkBoxCaseSensitive.Text = Resources.SearchDialog_UI_CheckBox_CaseSensitive;
        checkBoxRegex.Text = Resources.SearchDialog_UI_CheckBox_RegularExpression;
        buttonRegexHelper.Text = Resources.SearchDialog_UI_Button_RegexHelper;
        radioButtonFromTop.Text = Resources.SearchDialog_UI_RadioButton_FromTop;
        radioButtonFromSelected.Text = Resources.SearchDialog_UI_RadioButton_FromSelectedLine;
        groupBoxSearchStart.Text = Resources.SearchDialog_UI_GroupBox_SearchStart;
        groupBoxOptions.Text = Resources.SearchDialog_UI_GroupBox_Options;
        groupBoxDirection.Text = Resources.SearchDialog_UI_GroupBox_Direction;
        radioButtonBackward.Text = Resources.SearchDialog_UI_RadioButton_Backward;
        radioButtonForward.Text = Resources.SearchDialog_UI_RadioButton_Forward;
        Text = Resources.SearchDialog_UI_Title;
    }

    #endregion

    #region Properties

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public SearchParams SearchParams { get; set; } = new();

    #endregion

    #region Events handler

    private void OnSearchDialogLoad (object? sender, EventArgs e)
    {
        if (SearchParams != null)
        {
            if (SearchParams.IsFromTop)
            {
                radioButtonFromTop.Checked = true;
            }
            else
            {
                radioButtonFromSelected.Checked = true;
            }

            if (SearchParams.IsForward)
            {
                radioButtonForward.Checked = true;
            }
            else
            {
                radioButtonBackward.Checked = true;
            }

            checkBoxRegex.Checked = SearchParams.IsRegex;
            checkBoxCaseSensitive.Checked = SearchParams.IsCaseSensitive;
            foreach (var item in SearchParams.HistoryList)
            {
                _ = comboBoxSearchFor.Items.Add(item);
            }

            if (comboBoxSearchFor.Items.Count > 0)
            {
                comboBoxSearchFor.SelectedIndex = 0;
            }
        }
        else
        {
            radioButtonFromSelected.Checked = true;
            radioButtonForward.Checked = true;
            SearchParams = new SearchParams();
        }
    }

    private void OnButtonRegexClick (object sender, EventArgs e)
    {
        RegexHelperDialog dlg = new()
        {
            Owner = this,
            CaseSensitive = checkBoxCaseSensitive.Checked,
            Pattern = comboBoxSearchFor.Text
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            checkBoxCaseSensitive.Checked = dlg.CaseSensitive;
            comboBoxSearchFor.Text = dlg.Pattern;
        }
    }

    private void OnButtonOkClick (object sender, EventArgs e)
    {
        try
        {
            if (checkBoxRegex.Checked)
            {
                if (string.IsNullOrWhiteSpace(comboBoxSearchFor.Text))
                {
                    throw new ArgumentException(Resources.SearchDialog_UI_Error_SearchTextEmpty);
                }

                // Use RegexHelper for safer validation with timeout protection
                if (!RegexHelper.IsValidPattern(comboBoxSearchFor.Text, out var error))
                {
                    throw new ArgumentException($"Invalid regex pattern: {error}");
                }
            }

            SearchParams.SearchText = comboBoxSearchFor.Text;
            SearchParams.IsCaseSensitive = checkBoxCaseSensitive.Checked;
            SearchParams.IsForward = radioButtonForward.Checked;
            SearchParams.IsFromTop = radioButtonFromTop.Checked;
            SearchParams.IsRegex = checkBoxRegex.Checked;
            _ = SearchParams.HistoryList.Remove(comboBoxSearchFor.Text);
            SearchParams.HistoryList.Insert(0, comboBoxSearchFor.Text);

            if (SearchParams.HistoryList.Count > MAX_HISTORY)
            {
                SearchParams.HistoryList.RemoveAt(SearchParams.HistoryList.Count - 1);
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show(string.Format(CultureInfo.InvariantCulture, Resources.SearchDialog_UI_Error_CreatingSearchParameter, ex.Message));
        }
    }

    private void OnButtonCancelClick (object sender, EventArgs e)
    {
        Close();
    }

    #endregion
}