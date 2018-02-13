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
    private ConfigData configData;

    public ConfigDialog(ConfigData configData)
    {
      InitializeComponent();
      TopLevel = false;
      this.configData = configData;
      this.pkCheckBox.Checked = this.configData.UseKeyfile;
      this.puttyKeyRadioButton.Checked = this.configData.KeyType == KeyType.Putty;
      this.sshKeyRadioButton.Checked = this.configData.KeyType == KeyType.Ssh;
      this.fileLabel.Text = this.configData.KeyFile;
    }

    public ConfigData ConfigData
    {
      get { return configData; }
    }

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
        this.ConfigData.KeyFile = dlg.FileName;
        this.fileLabel.Text = this.ConfigData.KeyFile;
      }

    }

    private void pkCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      this.configData.UseKeyfile = this.pkCheckBox.Checked;
    }

    private void puttyKeyRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      this.configData.KeyType = KeyType.Putty;
    }

    private void sshKeyRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      this.configData.KeyType = KeyType.Ssh;
    }
  }
}
