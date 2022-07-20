using System;
using System.Windows.Forms;
using LogExpert.Config;

namespace LogExpert.Dialogs
{
    public partial class ImportSettingsDialog : Form
    {
        #region Fields

        #endregion

        #region cTor

        public ImportSettingsDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public string FileName { get; private set; }

        public ExportImportFlags ImportFlags { get; private set; }

        #endregion

        #region Events handler

        private void OnImportSettingsDialogLoad(object sender, EventArgs e)
        {
        }

        private void OnFileButtonClick(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Load Settings from file";
            dlg.DefaultExt = "json";
            dlg.AddExtension = false;
            dlg.Filter = "Settings (*.json)|*.json|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxFileName.Text = dlg.FileName;
            }
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            ImportFlags = ExportImportFlags.None;
            FileName = textBoxFileName.Text;

            foreach (Control ctl in groupBoxImportOptions.Controls)
            {
                if (ctl.Tag != null)
                {
                    if (((CheckBox)ctl).Checked)
                    {
                        ImportFlags |= (ExportImportFlags) long.Parse(ctl.Tag as string ?? string.Empty);
                    }
                }
            }
        }

        #endregion
    }
}