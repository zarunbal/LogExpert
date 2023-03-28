using System;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class ConfigDialog : Form
    {
        #region Ctor

        public ConfigDialog(ConfigData configData)
        {
            InitializeComponent();
            TopLevel = false;
            ConfigData = configData;
            chkBoxPK.Checked = ConfigData.UseKeyfile;
            radioBtnPuttyKey.Checked = ConfigData.KeyType == KeyType.Putty;
            radioBtnSSHKey.Checked = ConfigData.KeyType == KeyType.Ssh;
            lblFile.Text = ConfigData.KeyFile;
        }

        #endregion

        #region Properties / Indexers

        public ConfigData ConfigData { get; }

        #endregion

        #region Event handling Methods

        private void OnBtnKeyFileClick(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();
            if (DialogResult.OK == dlg.ShowDialog())
            {
                ConfigData.KeyFile = dlg.FileName;
                lblFile.Text = ConfigData.KeyFile;
            }
        }

        private void OnChkBoxPKCheckedChanged(object sender, EventArgs e)
        {
            ConfigData.UseKeyfile = chkBoxPK.Checked;
        }

        private void OnChkBoxPKCheckStateChanged(object sender, EventArgs e)
        {
            keyFileButton.Enabled = chkBoxPK.Checked;
            keyTypeGroupBox.Enabled = chkBoxPK.Checked;
        }

        private void OnRadioButtonPuttyKeyCheckedChanged(object sender, EventArgs e)
        {
            ConfigData.KeyType = KeyType.Putty;
        }

        private void OnRadioButtonSSHKeyCheckedChanged(object sender, EventArgs e)
        {
            ConfigData.KeyType = KeyType.Ssh;
        }

        #endregion
    }
}
