using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LogExpert.Dialogs
{
	public partial class MultiFileMaskDialog : Form
	{
		public MultiFileMaskDialog(Form parent, string fileName)
		{
			InitializeComponent();
			this.syntaxHelpLabel.Text = "" +
										"Pattern syntax:\n\n" +
										"* = any characters (wildcard)\n" +
										"$D(<date>) = Date pattern\n" +
										"$I = File index number\n" +
										"$J = File index number, hidden when zero\n" +
										"$J(<prefix>) = Like $J, but adding <prefix> when non-zero\n" +
										"\n" +
										"<date>:\n" +
										"DD = day\n" +
										"MM = month\n" +
										"YY[YY] = year\n" +
										"all other chars will be used as given"
			;
			this.fileNameLabel.Text = fileName;
		}

		public string FileNamePattern { get; set; }

		public int MaxDays { get; set; }

		private void buttonOK_Click(object sender, EventArgs e)
		{
			this.FileNamePattern = this.fileNamePatternTextBox.Text;
			this.MaxDays = (int)this.maxDaysUpDown.Value;
		}

		private void MultiFileMaskDialog_Load(object sender, EventArgs e)
		{
			this.fileNamePatternTextBox.Text = this.FileNamePattern;
			this.maxDaysUpDown.Value = this.MaxDays;
		}
	}
}