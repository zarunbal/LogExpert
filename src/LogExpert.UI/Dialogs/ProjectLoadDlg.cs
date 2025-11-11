using System.ComponentModel;
using System.Runtime.Versioning;

using LogExpert.Core.Enums;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class ProjectLoadDlg : Form
{
    #region Fields

    #endregion

    #region cTor

    public ProjectLoadDlg()
    {
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
    }

    #endregion

    #region Properties

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public ProjectLoadDlgResult ProjectLoadResult { get; set; } = ProjectLoadDlgResult.Cancel;

    #endregion

    #region Events handler

    private void OnButtonCloseTabsClick(object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.CloseTabs;
        Close();
    }

    private void OnButtonNewWindowClick(object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.NewWindow;
        Close();
    }

    private void OnButtonIgnoreClick(object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.IgnoreLayout;
        Close();
    }

    #endregion
}