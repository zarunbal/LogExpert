using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class ImportSettingsDialog : Form
    {
        #region Ctor

        public ImportSettingsDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public string FileName { get; private set; }

        public ExportImportFlags ImportFlags { get; private set; }

        #endregion

        #region Private Methods

        private void fileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Load Settings from file";
            dlg.DefaultExt = "dat";
            dlg.AddExtension = false;
            dlg.Filter = "Settings (*.dat)|*.dat|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                fileNameTextBox.Text = dlg.FileName;
            }
        }

        private void ImportSettingsDialog_Load(object sender, EventArgs e)
        {
            foreach (Control ctl in optionsGroupBox.Controls)
            {
                if (ctl.Tag != null)
                {
                    (ctl as CheckBox).Checked = true;
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            ImportFlags = ExportImportFlags.None;
            FileName = fileNameTextBox.Text;
            foreach (Control ctl in optionsGroupBox.Controls)
            {
                if (ctl.Tag != null)
                {
                    if ((ctl as CheckBox).Checked)
                    {
                        ImportFlags = ImportFlags | (ExportImportFlags)long.Parse(ctl.Tag as string);
                    }
                }
            }
        }

        #endregion
    }
}
