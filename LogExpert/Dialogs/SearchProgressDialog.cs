using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class SearchProgressDialog : Form
	{
		public SearchProgressDialog()
		{
			InitializeComponent();
			this.ShouldStop = false;
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.ShouldStop = true;
		}

		public bool ShouldStop { get; private set; }
	}
}