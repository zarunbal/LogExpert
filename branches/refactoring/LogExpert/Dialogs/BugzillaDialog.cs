using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public partial class BugzillaDialog : Form
	{
		String errorText;
		String stackTrace;

		public BugzillaDialog(String errorText, String stackTrace)
		{
			InitializeComponent();
			this.errorText = errorText;
			this.stackTrace = stackTrace;
			this.introTextLabel.Text =
									  "You can create an automatic bug entry in LogExpert's bugzilla bug tracking system. " +
									  "All you have to do is clicking the 'Post bug entry' button.\n" +
									  "The data to be transmitted is: stack trace, your OS and CLR version, " +
									  "and the comment you have entered.\n\n" +
									  "Note: Automatic reporting may not work when using proxy connections.";
			string link = "http://www.logfile-viewer.de/bugzilla/";
			this.linkLabel1.Links.Add(new LinkLabel.Link(0, link.Length, link));
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string target = e.Link.LinkData as string;
			System.Diagnostics.Process.Start(target);
		}

		private void sendButton_Click(object sender, EventArgs e)
		{
			sendBug();
		}

		private void sendBug()
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;
				this.Enabled = false;
				BugSender bugSender = new BugSender(this.errorText, this.stackTrace,
					this.commentBox.Text, this.ccTextBox.Text);
				int bugId = bugSender.SendBug();
				MessageBox.Show("Sucessfully created a new bug with ID " + bugId, "LogExpert");
				this.Cursor = Cursors.Default;
			}
			catch (Exception e)
			{
				MessageBox.Show("Bug posting failed:\n\n" + e.Message, "LogExpert");
				this.Cursor = Cursors.Default;
				this.Enabled = true;
				return;  // don't close. So user can try again
			}
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}