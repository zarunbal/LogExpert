using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class LoginDialog : Form
    {
        #region Fields

        private string userName;

        #endregion

        #region cTor

        public LoginDialog(string host, IList<string> userNames, bool hidePasswordField)
        {
            InitializeComponent();
            serverNameLabel.Text = host;
            if (userNames != null)
            {
                foreach (string name in userNames)
                {
                    if (name != null)
                    {
                        userNameComboBox.Items.Add(name);
                    }
                }
            }

            if (hidePasswordField)
            {
                passwordTextBox.Enabled = false;
                passwordLabel.Enabled = false;
            }
        }

        #endregion

        #region Properties

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value != null ? value : "";
                userNameComboBox.Text = value;
            }
        }

        public string Password { get; private set; }

        #endregion

        #region Events handler

        private void okButton_Click(object sender, EventArgs e)
        {
            Password = passwordTextBox.Text;
            userName = userNameComboBox.Text;
        }

        private void LoginDialog_Load(object sender, EventArgs e)
        {
            if (userNameComboBox.Text.Length > 0)
            {
                passwordTextBox.Focus();
            }
        }

        #endregion
    }
}