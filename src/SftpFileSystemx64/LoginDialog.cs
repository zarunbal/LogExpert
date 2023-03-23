using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class LoginDialog : Form
    {
        #region Private Fields

        private string _username;

        #endregion

        #region Ctor

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
                        cmbUsername.Items.Add(name);
                    }
                }
            }

            if (hidePasswordField)
            {
                txtBoxPassword.Enabled = false;
                lblPassword.Enabled = false;
            }
        }

        #endregion

        #region Properties / Indexers

        public string Password { get; private set; }

        public string Username
        {
            get => _username;
            set
            {
                _username = value ?? string.Empty;
                cmbUsername.Text = value;
            }
        }

        #endregion

        #region Event handling Methods

        private void OnBtnOKClick(object sender, EventArgs e)
        {
            Password = txtBoxPassword.Text;
            _username = cmbUsername.Text;
        }

        private void OnLoginDialogLoad(object sender, EventArgs e)
        {
            if (cmbUsername.Text.Length > 0)
            {
                txtBoxPassword.Focus();
            }
        }

        #endregion
    }
}
