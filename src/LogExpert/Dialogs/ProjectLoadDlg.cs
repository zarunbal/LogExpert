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

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        #endregion

        #region Properties

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
}