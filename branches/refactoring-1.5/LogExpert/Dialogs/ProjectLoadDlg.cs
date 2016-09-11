using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	public enum ProjectLoadDlgResult
	{
		Cancel,
		CloseTabs,
		NewWindow,
		IgnoreLayout
	}

	public partial class ProjectLoadDlg : Form
	{
		private ProjectLoadDlgResult projectLoadResult = ProjectLoadDlgResult.Cancel;

		public ProjectLoadDlg()
		{
			InitializeComponent();
		}

		public ProjectLoadDlgResult ProjectLoadResult
		{
			get
			{
				return projectLoadResult;
			}
			set
			{
				projectLoadResult = value;
			}
		}

		private void closeTabsButton_Click(object sender, EventArgs e)
		{
			this.ProjectLoadResult = ProjectLoadDlgResult.CloseTabs;
			Close();
		}

		private void newWindowButton_Click(object sender, EventArgs e)
		{
			this.ProjectLoadResult = ProjectLoadDlgResult.NewWindow;
			Close();
		}

		private void ignoreButton_Click(object sender, EventArgs e)
		{
			this.ProjectLoadResult = ProjectLoadDlgResult.IgnoreLayout;
			Close();
		}
	}
}