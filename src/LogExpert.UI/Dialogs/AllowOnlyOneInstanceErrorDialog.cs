using System.Runtime.Versioning;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
public partial class AllowOnlyOneInstanceErrorDialog : Form
{
    public bool DoNotShowThisMessageAgain { get; private set; }

    public AllowOnlyOneInstanceErrorDialog ()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        ResumeLayout();
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
