using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LogExpert.Classes;
using LogExpert.Controls.LogTabWindow;


namespace LogExpert.Dialogs
{
    public partial class ToolArgsDialog : Form
    {
        #region Fields

        private readonly LogTabWindow logTabWin;

        #endregion

        #region cTor

        public ToolArgsDialog(LogTabWindow logTabWin, Form parent)
        {
            this.logTabWin = logTabWin;
            parent.AddOwnedForm(this);
            this.TopMost = parent.TopMost;
            InitializeComponent();
        }

        #endregion

        #region Properties

        public string Arg { get; set; } = null;

        #endregion

        #region Events handler

        private void ToolArgsDialog_Load(object sender, EventArgs e)
        {
            this.helpLabel.Text = "" +
                                  "%L = Current line number\n" +
                                  "%N = Current log file name without path\n" +
                                  "%P = Path (directory) of current log file\n" +
                                  "%F = Full name (incl. path) of log file\n" +
                                  "%E = Extension of log file name (e.g. 'txt')\n" +
                                  "%M = Name of log file without extension\n" +
                                  "%S = User (from URI)\n" +
                                  "%R = Path (from URI)\n" +
                                  "%H = Host (from URI)\n" +
                                  "%T = Port (from URI)\n" +
                                  "?\"<name>\" = variable parameter 'name'\n" +
                                  "?\"<name>\"(def1,def2,...) = variable parameter with predefined values\n" +
                                  "\n" +
                                  "{<regex>}{<replace>}:\n" +
                                  "Regex search/replace on current selected line.";

            this.argsTextBox.Text = this.Arg;
        }

        private void regexHelpButton_Click(object sender, EventArgs e)
        {
            RegexHelperDialog regexDlg = new RegexHelperDialog();
            if (regexDlg.ShowDialog() == DialogResult.OK)
            {
                this.argsTextBox.SelectedText = regexDlg.Pattern;
            }
        }


        private void testButton_Click(object sender, EventArgs e)
        {
            if (this.logTabWin.CurrentLogWindow != null)
            {
                ILogLine line = this.logTabWin.CurrentLogWindow.GetCurrentLine();
                ILogFileInfo info = this.logTabWin.CurrentLogWindow.GetCurrentFileInfo();
                if (line != null && info != null)
                {
                    ArgParser parser = new ArgParser(this.argsTextBox.Text);
                    string args = parser.BuildArgs(line, this.logTabWin.CurrentLogWindow.GetRealLineNum() + 1, info,
                        this);
                    this.testResultLabel.Text = args;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Arg = this.argsTextBox.Text;
        }

        #endregion
    }
}