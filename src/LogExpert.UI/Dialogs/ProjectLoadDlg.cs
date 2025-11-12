using System.Runtime.Versioning;

using LogExpert.Core.Enums;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class ProjectLoadDlg : Form
{
    #region Fields

    #endregion

    #region cTor

    public ProjectLoadDlg ()
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
        Text = Resources.ProjectLoadDlg_UI_Title;
        labelInformational.Text = Resources.ProjectLoadDlg_UI_Label_Informational;
        labelChooseHowToProceed.Text = Resources.ProjectLoadDlg_UI_Label_ChooseHowToProceed;
        buttonCloseTabs.Text = Resources.ProjectLoadDlg_UI_Button_CloseTabs;
        buttonNewWindow.Text = Resources.ProjectLoadDlg_UI_Button_NewWindow;
        buttonIgnore.Text = Resources.ProjectLoadDlg_UI_Button_Ignore;
    }

    #endregion

    #region Properties

    public ProjectLoadDlgResult ProjectLoadResult { get; set; } = ProjectLoadDlgResult.Cancel;

    #endregion

    #region Events handler

    private void OnButtonCloseTabsClick (object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.CloseTabs;
        Close();
    }

    private void OnButtonNewWindowClick (object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.NewWindow;
        Close();
    }

    private void OnButtonIgnoreClick (object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.IgnoreLayout;
        Close();
    }

    #endregion
}