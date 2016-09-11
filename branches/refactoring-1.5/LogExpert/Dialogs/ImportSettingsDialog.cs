using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class ImportSettingsDialog : Form
	{
		public ImportSettingsDialog()
		{
			InitializeComponent();
		}

		private void ImportSettingsDialog_Load(object sender, EventArgs e)
		{
			foreach (Control ctl in this.optionsGroupBox.Controls)
			{
				if (ctl.Tag != null)
				{
					(ctl as CheckBox).Checked = true;
				}
			}
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
				this.fileNameTextBox.Text = dlg.FileName;
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			this.ImportFlags = ExportImportFlags.None;
			this.FileName = this.fileNameTextBox.Text;
			foreach (Control ctl in this.optionsGroupBox.Controls)
			{
				if (ctl.Tag != null)
				{
					if ((ctl as CheckBox).Checked)
					{
						this.ImportFlags = this.ImportFlags | (ExportImportFlags)long.Parse(ctl.Tag as string);
					}
				}
			}
		}

		public string FileName { get; private set; }

		public ExportImportFlags ImportFlags { get; private set; }
	}
}