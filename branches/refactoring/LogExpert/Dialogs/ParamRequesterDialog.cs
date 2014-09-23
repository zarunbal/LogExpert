using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class ParamRequesterDialog : Form
	{
		public string ParamName { get; set; }

		public string ParamValue { get; set; }

		public string[] Values { get; set; }

		public ParamRequesterDialog()
		{
			InitializeComponent();
		}

		private void ParamRequesterDialog_Shown(object sender, EventArgs e)
		{
			this.paramLabel.Text = ParamName;
			if (this.Values != null)
			{
				foreach (string value in this.Values)
				{
					this.valueComboBox.Items.Add(value);
				}
				this.valueComboBox.SelectedIndex = 0;
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			this.ParamValue = this.valueComboBox.Text;
		}
	}
}