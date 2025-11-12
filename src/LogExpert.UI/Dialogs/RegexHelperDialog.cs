using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class RegexHelperDialog : Form
{
    #region Fields

    private const int MAX_HISTORY = 30;
    private bool _caseSensitive;

    #endregion

    #region cTor

    public RegexHelperDialog ()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        Load += OnRegexHelperDialogLoad;

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        labelRegex.Text = Resources.RegexHelperDialog_UI_Label_Regex;
        labelTestText.Text = Resources.RegexHelperDialog_UI_Label_TestText;
        labelMatches.Text = Resources.RegexHelperDialog_UI_Label_Matches;
        checkBoxCaseSensitive.Text = Resources.RegexHelperDialog_UI_CheckBox_CaseSensitive;
        buttonHelp.Text = Resources.LogExpert_Common_UI_Button_Help;
        Text = Resources.RegexHelperDialog_UI_Title;
    }

    #endregion

    #region Properties

    public bool CaseSensitive
    {
        get => _caseSensitive;
        set
        {
            _caseSensitive = value;
            checkBoxCaseSensitive.Checked = value;
        }
    }

    public string Pattern
    {
        get => comboBoxRegex.Text;
        set => comboBoxRegex.Text = value;
    }

    public List<string> ExpressionHistoryList { get; set; } = [];

    public List<string> TesttextHistoryList { get; set; } = [];

    #endregion

    #region Private Methods

    private void UpdateMatches ()
    {
        textBoxMatches.Text = string.Empty;
        try
        {
            Regex rex = new(comboBoxRegex.Text, _caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            var matches = rex.Matches(comboBoxTestText.Text);

            foreach (Match match in matches)
            {
                textBoxMatches.Text += $"{match.Value}\r\n";
            }
        }
        catch (ArgumentException)
        {
            textBoxMatches.Text = Resources.RegexHelperDialog_UI_TextBox_Matches_NoValidRegexPattern;
        }
    }

    private void LoadHistory ()
    {
        comboBoxRegex.Items.Clear();
        comboBoxRegex.DataSource = ExpressionHistoryList;

        comboBoxTestText.Items.Clear();
        comboBoxTestText.DataSource = TesttextHistoryList;
    }

    #endregion

    #region Events handler

    private void OnRegexHelperDialogLoad (object? sender, EventArgs e)
    {
        LoadHistory();
    }

    private void OnCaseSensitiveCheckBoxCheckedChanged (object sender, EventArgs e)
    {
        _caseSensitive = checkBoxCaseSensitive.Checked;
        UpdateMatches();
    }

    private void OnButtonOkClick (object sender, EventArgs e)
    {
        var text = comboBoxRegex.Text;
        comboBoxRegex.Items.Remove(text);
        comboBoxRegex.Items.Insert(0, text);

        text = comboBoxTestText.Text;
        comboBoxTestText.Items.Remove(text);
        comboBoxTestText.Items.Insert(0, text);

        if (comboBoxRegex.Items.Count > MAX_HISTORY)
        {
            comboBoxRegex.Items.Remove(comboBoxRegex.Items.Count - 1);
        }

        if (comboBoxTestText.Items.Count > MAX_HISTORY)
        {
            comboBoxTestText.Items.Remove(comboBoxTestText.Items.Count - 1);
        }
    }

    private void OnComboBoxRegexTextChanged (object sender, EventArgs e)
    {
        UpdateMatches();
    }

    private void OnComboBoxTestTextTextChanged (object sender, EventArgs e)
    {
        UpdateMatches();
    }

    private void OnButtonHelpClick (object sender, EventArgs e)
    {
        Help.ShowHelp(this, Resources.LogTabWindow_HelpFile, HelpNavigator.Topic, Resources.RegexHelperDialog_Help_Chapter);
    }

    #endregion
}