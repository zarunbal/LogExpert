using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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
        #region Fields

        #endregion

        #region cTor

        public ProjectLoadDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public ProjectLoadDlgResult ProjectLoadResult { get; set; } = ProjectLoadDlgResult.Cancel;

        #endregion

        #region Events handler

        private void closeTabsButton_Click(object sender, EventArgs e)
        {
            this.ProjectLoadResult = ProjectLoadDlgResult.CloseTabs;
            Close();
        }

        private void newWindowButton_Click(object sender, EventArgs e)
        {
            this.ProjectLoadResult = ProjectLoadDlgResult.NewWindow;
            Close();
        }

        private void ignoreButton_Click(object sender, EventArgs e)
        {
            this.ProjectLoadResult = ProjectLoadDlgResult.IgnoreLayout;
            Close();
        }

        #endregion
    }
}