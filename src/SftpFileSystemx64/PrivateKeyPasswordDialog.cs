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

        #region Event handling Methods

        private void OnLoginDialogLoad(object sender, EventArgs e)
        {
            passwordTextBox.Focus();
        }

        private void OnBtnOkClick(object sender, EventArgs e)
        {
            Password = passwordTextBox.Text;
        }

        #endregion
    }
}
