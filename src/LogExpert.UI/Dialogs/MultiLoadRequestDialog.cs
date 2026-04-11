using System.Runtime.Versioning;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class MultiLoadRequestDialog : Form
{
    #region cTor

    public MultiLoadRequestDialog ()
    {
        SuspendLayout();

        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ApplyResources();

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.MultiLoadRequestDialog_UI_Title;
        labelChooseLoadingMode.Text = Resources.MultiLoadRequestDialog_UI_Label_ChooseLoadingMode;
        buttonSingleMode.Text = Resources.MultiLoadRequestDialog_UI_Button_SingleFiles;
        buttonMultiMode.Text = Resources.MultiLoadRequestDialog_UI_Button_MultiFile;
    }

    #endregion
}