using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class LoginDialog : Form
    {
        #region Private Fields

        private string _userName;

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

        #region Properties / Indexers

        public string Password { get; private set; }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value != null ? value : string.Empty;
                userNameComboBox.Text = value;
            }
        }

        #endregion

        #region Private Methods

        private void LoginDialog_Load(object sender, EventArgs e)
        {
            if (userNameComboBox.Text.Length > 0)
            {
                passwordTextBox.Focus();
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Password = passwordTextBox.Text;
            _userName = userNameComboBox.Text;
        }

        #endregion
    }
}
