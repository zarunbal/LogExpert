using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class GotoLineDialog : Form
	{
		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		public GotoLineDialog(Form parent)
		{
			InitializeComponent();
			this.Owner = parent;
		}

		private void GotoLineDialog_Load(object sender, EventArgs e)
		{
		}

		public int Line { get; private set; }

		private void okButton_Click(object sender, EventArgs e)
		{
			try
			{
				this.Line = Int32.Parse(this.lineNumberTextBox.Text);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				this.Line = -1;
			}
		}
	}
}