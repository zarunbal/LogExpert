using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class PrivateKeyPasswordDialog : Form
    {
        #region cTor

        public PrivateKeyPasswordDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public string Password { get; private set; }

        #endregion

        #region Events handler

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            Password = passwordTextBox.Text;
        }

        private void OnLoginDialogLoad(object sender, EventArgs e)
        {
            passwordTextBox.Focus();
        }

        #endregion
    }
}