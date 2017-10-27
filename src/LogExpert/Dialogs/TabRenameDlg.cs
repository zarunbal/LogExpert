using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class TabRenameDlg : Form
    {
        #region cTor

        public TabRenameDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public string TabName
        {
            get { return this.tabNameTextBox.Text; }
            set { this.tabNameTextBox.Text = value; }
        }

        #endregion

        #region Events handler

        private void TabRenameDlg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        #endregion
    }
}