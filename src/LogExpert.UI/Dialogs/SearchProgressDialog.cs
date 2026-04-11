using System.Runtime.Versioning;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class SearchProgressDialog : Form
{
    #region cTor

    public SearchProgressDialog ()
    {
        SuspendLayout();

        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ApplyResources();

        ShouldStop = false;

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.SearchProgressDialog_UI_Title;
        labelSearchProgress.Text = Resources.SearchProgressDialog_UI_Label_SearchingInProgress;
        buttonCancel.Text = Resources.SearchProgressDialog_UI_Button_CancelSearch;
    }

    #endregion

    #region Properties

    public bool ShouldStop { get; private set; }

    #endregion

    #region Events handler

    private void OnButtonCancelClick (object sender, EventArgs e)
    {
        ShouldStop = true;
    }

    #endregion
}