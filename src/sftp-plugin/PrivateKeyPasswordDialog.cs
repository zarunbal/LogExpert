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

        private void okButton_Click(object sender, EventArgs e)
        {
            Password = passwordTextBox.Text;
        }

        private void LoginDialog_Load(object sender, EventArgs e)
        {
            passwordTextBox.Focus();
        }

        #endregion
    }
}