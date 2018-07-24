using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SftpFileSystem
{
    public partial class ConfigDialog : Form
    {
        #region cTor

        public ConfigDialog(ConfigData configData)
        {
            InitializeComponent();
            TopLevel = false;
            ConfigData = configData;
            pkCheckBox.Checked = ConfigData.UseKeyfile;
            puttyKeyRadioButton.Checked = ConfigData.KeyType == KeyType.Putty;
            sshKeyRadioButton.Checked = ConfigData.KeyType == KeyType.Ssh;
            sshApiCombo.Items.AddRange(new object[] { SshApiType.Chilkat, SshApiType.Renci });
            sshApiCombo.SelectedItem = ConfigData.SshApiType;
            fileLabel.Text = ConfigData.KeyFile;
        }

        #endregion

        #region Properties

        public ConfigData ConfigData { get; }

        #endregion

        #region Events handler

        private void pkCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            keyFileButton.Enabled = pkCheckBox.Checked;
            keyTypeGroupBox.Enabled = pkCheckBox.Checked;
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
        }

        private void keyFileButton_Click(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();
            if (DialogResult.OK == dlg.ShowDialog())
            {
                ConfigData.KeyFile = dlg.FileName;
                fileLabel.Text = ConfigData.KeyFile;
            }
        }

        private void pkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigData.UseKeyfile = pkCheckBox.Checked;
        }

        private void puttyKeyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ConfigData.KeyType = KeyType.Putty;
        }

        private void sshKeyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ConfigData.KeyType = KeyType.Ssh;
        }

        #endregion

        private void sshApiCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = (SshApiType)sshApiCombo.SelectedItem;
            ConfigData.SshApiType = selected;
            switch (selected)
            {
                case SshApiType.Chilkat:
                    label3.Visible = true;
                    chilkatKeyBox.Visible = true;
                    break;
                case SshApiType.Renci:
                    label3.Visible = false;
                    chilkatKeyBox.Visible = false;
                    break;
            }
        }

        private void chilkatKeyBox_TextChanged(object sender, EventArgs e)
        {
            ConfigData.ChilkatKey = chilkatKeyBox.Text;
        }
    }
}