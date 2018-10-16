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

        #region Private Methods

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }

        #endregion
    }
}
