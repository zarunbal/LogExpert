using System.Runtime.Versioning;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
public partial class AllowOnlyOneInstanceErrorDialog : Form
{
    public bool DoNotShowThisMessageAgain { get; private set; }

    public AllowOnlyOneInstanceErrorDialog ()
    {
        InitializeComponent();
        ApplyResources();
    }

    private void ApplyResources ()
    {
        labelErrorText.Text = Resources.AllowOnlyOneInstanceErrorDialog_UI_Label_labelErrorText;
        checkBoxIgnoreMessage.Text = Resources.AllowOnlyOneInstanceErrorDialog_UI_CheckBox_checkBoxIgnoreMessage;
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
    }

    private void OnButtonOkClick (object sender, EventArgs e)
    {
        DoNotShowThisMessageAgain = checkBoxIgnoreMessage.Checked;
    }
}
