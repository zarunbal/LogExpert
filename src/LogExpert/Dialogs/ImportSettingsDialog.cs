using System;
using System.Windows.Forms;

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

        private void ImportSettingsDialog_Load(object sender, EventArgs e)
        {
        }

        private void fileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Load Settings from file";
            dlg.DefaultExt = "dat";
            dlg.AddExtension = false;
            dlg.Filter = "Settings (*.dat)|*.dat|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxFileName.Text = dlg.FileName;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            ImportFlags = ExportImportFlags.None;
            FileName = textBoxFileName.Text;

            foreach (Control ctl in groupBoxImportOptions.Controls)
            {
                if (ctl.Tag != null)
                {
                    if (((CheckBox)ctl).Checked)
                    {
                        ImportFlags = ImportFlags | (ExportImportFlags) long.Parse(ctl.Tag as string ?? string.Empty);
                    }
                }
            }
        }

        #endregion
    }
}