using System;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class PrivateKeyPasswordDialog : Form
    {
        #region Ctor

        public PrivateKeyPasswordDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public string Password { get; private set; }

        #endregion

        #region Private Methods

        private void LoginDialog_Load(object sender, EventArgs e)
        {
            passwordTextBox.Focus();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Password = passwordTextBox.Text;
        }

        #endregion
    }
}
