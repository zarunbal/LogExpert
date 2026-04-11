using System.ComponentModel;
using System.Runtime.Versioning;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class TabRenameDialog : Form
{
    #region cTor

    public TabRenameDialog ()
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
        // Dialog title
        Text = Resources.TabRenameDialog_UI_Title;

        labelName.Text = Resources.TabRenameDialog_UI_Label_Name;

        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
    }

    #endregion

    #region Properties

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string TabName
    {
        get => textBoxTabName.Text;
        set => textBoxTabName.Text = value;
    }

    #endregion

    #region Events handler

    private void OnTabRenameDlgKeyDown (object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    #endregion
}