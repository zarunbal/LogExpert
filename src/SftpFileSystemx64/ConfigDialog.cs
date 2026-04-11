using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]
namespace SftpFileSystem;

public partial class ConfigDialog : Form
{
    #region Ctor

    public ConfigDialog (ConfigData configData)
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        TopLevel = false;
        ConfigData = configData;
        chkBoxPK.Checked = ConfigData.UseKeyfile;
        radioBtnPuttyKey.Checked = ConfigData.KeyType == KeyType.Putty;
        radioBtnSSHKey.Checked = ConfigData.KeyType == KeyType.Ssh;
        lblFile.Text = ConfigData.KeyFile;

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.ConfigDialog_UI_Title;
        chkBoxPK.Text = Resources.ConfigDialog_UI_CheckBox_UseKeyfile;
        keyFileButton.Text = Resources.ConfigDialog_UI_Button_SelectFile;
        keyTypeGroupBox.Text = Resources.ConfigDialog_UI_GroupBox_KeyType;
        radioBtnPuttyKey.Text = Resources.ConfigDialog_UI_RadioButton_PuttyKey;
        radioBtnSSHKey.Text = Resources.ConfigDialog_UI_RadioButton_SSHKey;
        label1.Text = Resources.ConfigDialog_UI_Label_KeyInfo;
    }

    #endregion

    #region Properties / Indexers

    public ConfigData ConfigData { get; }

    #endregion

    #region Event handling Methods

    private void OnBtnKeyFileClick (object sender, EventArgs e)
    {
        FileDialog dlg = new OpenFileDialog();
        if (DialogResult.OK == dlg.ShowDialog())
        {
            ConfigData.KeyFile = dlg.FileName;
            lblFile.Text = ConfigData.KeyFile;
        }
    }

    private void OnChkBoxPKCheckedChanged (object sender, EventArgs e)
    {
        ConfigData.UseKeyfile = chkBoxPK.Checked;
    }

    private void OnChkBoxPKCheckStateChanged (object sender, EventArgs e)
    {
        keyFileButton.Enabled = chkBoxPK.Checked;
        keyTypeGroupBox.Enabled = chkBoxPK.Checked;
    }

    private void OnRadioButtonPuttyKeyCheckedChanged (object sender, EventArgs e)
    {
        ConfigData.KeyType = KeyType.Putty;
    }

    private void OnRadioButtonSSHKeyCheckedChanged (object sender, EventArgs e)
    {
        ConfigData.KeyType = KeyType.Ssh;
    }

    #endregion
}
