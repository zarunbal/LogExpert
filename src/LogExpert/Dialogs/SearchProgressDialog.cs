using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class SearchProgressDialog : Form
    {
        #region Fields

        #endregion

        #region cTor

        public SearchProgressDialog()
        {
            InitializeComponent();
            this.ShouldStop = false;
        }

        #endregion

        #region Properties

        public bool ShouldStop { get; private set; }

        #endregion

        #region Events handler

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.ShouldStop = true;
        }

        #endregion
    }
}