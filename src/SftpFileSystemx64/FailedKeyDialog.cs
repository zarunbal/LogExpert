using System;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class FailedKeyDialog : Form
    {
        #region Ctor

        public FailedKeyDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handling Methods

        private void OnBtnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnBtnRetryClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void OnBtnUsePasswordAuthenticationClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion
    }
}
