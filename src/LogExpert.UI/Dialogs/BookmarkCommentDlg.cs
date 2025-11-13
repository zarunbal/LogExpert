using System.ComponentModel;
using System.Runtime.Versioning;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class BookmarkCommentDlg : Form
{
    #region cTor

    public BookmarkCommentDlg ()
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
        Text = Resources.BookmarkCommentDlg_UI_Title;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
    }

    #endregion

    #region Properties

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Comment
    {
        set => textBoxComment.Text = value;
        get => textBoxComment.Text;
    }

    #endregion
}