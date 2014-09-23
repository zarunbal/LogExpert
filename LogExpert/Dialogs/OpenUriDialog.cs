using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class OpenUriDialog : Form
	{
		public OpenUriDialog()
		{
			InitializeComponent();
		}

		public string Uri
		{
			get
			{
				return this.uriComboBox.Text;
			}
		}

		public IList<string> UriHistory { get; set; }

		private void OpenUriDialog_Load(object sender, EventArgs e)
		{
			if (this.UriHistory != null)
			{
				this.uriComboBox.Items.Clear();
				foreach (string uri in this.UriHistory)
				{
					this.uriComboBox.Items.Add(uri);
				}
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			this.UriHistory = new List<string>();
			foreach (var item in this.uriComboBox.Items)
			{
				this.UriHistory.Add(item.ToString());
			}
			if (this.UriHistory.Contains(this.uriComboBox.Text))
			{
				this.UriHistory.Remove(this.uriComboBox.Text);
			}
			this.UriHistory.Insert(0, this.uriComboBox.Text);
			while (this.UriHistory.Count > 20)
			{
				this.UriHistory.RemoveAt(this.UriHistory.Count - 1);
			}
		}
	}
}