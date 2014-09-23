using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class ExceptionWindow : Form
	{
		String stackTrace;
		String errorText;

		public ExceptionWindow(String errorText, String stackTrace)
		{
			InitializeComponent();
			this.errorText = errorText;
			this.stackTrace = stackTrace;
			this.stackTraceTextBox.Text = this.errorText + "\n\n" + this.stackTrace;
			this.stackTraceTextBox.Select(0, 0);
		}

		private void bugzillaButton_Click(object sender, EventArgs e)
		{
			BugzillaDialog bugzillaDlg = new BugzillaDialog(errorText, stackTrace);
			//bugzillaDlg.Parent = this;
			if (bugzillaDlg.ShowDialog() == DialogResult.OK)
			{
				this.Close();
			}
		}

		private void copyButton_Click(object sender, EventArgs e)
		{
			CopyToClipboard();
		}

		private void CopyToClipboard()
		{
			Clipboard.SetText(this.errorText + "\n\n" + this.stackTrace);
		}
	}
}