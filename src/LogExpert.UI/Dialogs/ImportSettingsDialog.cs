using System.Runtime.Versioning;

using LogExpert.Core.Config;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class ImportSettingsDialog : Form
{
    #region cTor

    public ImportSettingsDialog (ExportImportFlags importFlags)
    {
        SuspendLayout();

        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ApplyResources();

        ImportFlags = importFlags;
        FileName = string.Empty;

        if (ImportFlags == ExportImportFlags.HighlightSettings)
        {
            checkBoxHighlightSettings.Checked = true;
            checkBoxHighlightSettings.Enabled = false;
            checkBoxHighlightFileMasks.Checked = false;
            checkBoxHighlightFileMasks.Enabled = false;
            checkBoxColumnizerFileMasks.Checked = false;
            checkBoxColumnizerFileMasks.Enabled = false;
            checkBoxExternalTools.Checked = false;
            checkBoxExternalTools.Enabled = false;
            checkBoxOther.Checked = false;
            checkBoxOther.Enabled = false;
        }

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.ImportSettingsDialog_UI_Title;
        labelSettingsFileToImport.Text = Resources.ImportSettingsDialog_UI_Label_SettingsFileToImport;
        buttonFile.Text = Resources.ImportSettingsDialog_UI_Button_ChooseFile;
        groupBoxImportOptions.Text = Resources.ImportSettingsDialog_UI_GroupBox_ImportOptions;
        checkBoxHighlightSettings.Text = Resources.ImportSettingsDialog_UI_CheckBox_HighlightSettings;
        checkBoxHighlightFileMasks.Text = Resources.ImportSettingsDialog_UI_CheckBox_HighlightFileMasks;
        checkBoxColumnizerFileMasks.Text = Resources.ImportSettingsDialog_UI_CheckBox_ColumnizerFileMasks;
        checkBoxExternalTools.Text = Resources.ImportSettingsDialog_UI_CheckBox_ExternalTools;
        checkBoxOther.Text = Resources.ImportSettingsDialog_UI_CheckBox_Other;
        checkBoxKeepExistingSettings.Text = Resources.ImportSettingsDialog_UI_CheckBox_KeepExistingSettings;
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
    }

    #endregion

    #region Properties

    public string FileName { get; private set; }

    public ExportImportFlags ImportFlags { get; private set; }

    #endregion

    #region Events handler

    private void OnImportSettingsDialogLoad (object sender, EventArgs e)
    {
    }

    private void OnFileButtonClick (object sender, EventArgs e)
    {
        OpenFileDialog dlg = new()
        {
            Title = Resources.ImportSettingsDialog_UI_OpenFileDialog_Title,
            DefaultExt = "json",
            AddExtension = false,
            Filter = Resources.ImportSettingsDialog_UI_OpenFileDialog_Filter
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            textBoxFileName.Text = dlg.FileName;
        }
    }

    private void OnOkButtonClick (object sender, EventArgs e)
    {
        FileName = textBoxFileName.Text;

        if (ImportFlags != ExportImportFlags.HighlightSettings)
        {
            foreach (Control ctl in groupBoxImportOptions.Controls)
            {
                if (ctl.Tag != null)
                {
                    if (((CheckBox)ctl).Checked)
                    {
                        ImportFlags |= (ExportImportFlags)long.Parse(ctl.Tag as string ?? string.Empty);
                    }
                }
            }
        }
        else
        {
            if (checkBoxKeepExistingSettings.Checked)
            {
                ImportFlags |= ExportImportFlags.KeepExisting;
            }
        }
    }

    #endregion
}