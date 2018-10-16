using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public enum ProjectLoadDlgResult
    {
        Cancel,
        CloseTabs,
        NewWindow,
        IgnoreLayout
    }

    public partial class ProjectLoadDlg : Form
    {
        #region Ctor

        public ProjectLoadDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public ProjectLoadDlgResult ProjectLoadResult { get; set; } = ProjectLoadDlgResult.Cancel;

        #endregion

        #region Private Methods

        private void closeTabsButton_Click(object sender, EventArgs e)
        {
            ProjectLoadResult = ProjectLoadDlgResult.CloseTabs;
            Close();
        }

        private void ignoreButton_Click(object sender, EventArgs e)
        {
            ProjectLoadResult = ProjectLoadDlgResult.IgnoreLayout;
            Close();
        }

        private void newWindowButton_Click(object sender, EventArgs e)
        {
            ProjectLoadResult = ProjectLoadDlgResult.NewWindow;
            Close();
        }

        #endregion
    }
}
