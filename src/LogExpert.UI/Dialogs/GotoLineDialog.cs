using System.Runtime.Versioning;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class GotoLineDialog : Form
{
    #region cTor

    public GotoLineDialog (Form parent)
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        ApplyResources();

        Owner = parent;

        ResumeLayout();
    }

    #endregion

    private void ApplyResources ()
    {
        // Dialog title
        Text = Resources.GotoLineDialog_UI_Title;

        labelLineNumber.Text = Resources.GotoLineDialog_UI_Label_LineNumber;

        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
    }

    #region Properties

    public int Line { get; private set; }

    #endregion

    #region Events handler

    private void GotoLineDialog_Load (object sender, EventArgs e)
    {
    }

    private void OnOkButtonClick (object sender, EventArgs e)
    {
        Line = int.TryParse(lineNumberTextBox.Text, out int line)
            ? line
            : -1;
    }

    #endregion
}