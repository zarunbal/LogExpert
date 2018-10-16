using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class ToolArgsDialog : Form
    {
        #region Private Fields

        private readonly LogTabWindow logTabWin;

        #endregion

        #region Ctor

        public ToolArgsDialog(LogTabWindow logTabWin, Form parent)
        {
            this.logTabWin = logTabWin;
            parent.AddOwnedForm(this);
            TopMost = parent.TopMost;
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public string Arg { get; set; }

        #endregion

        #region Private Methods

        private void button1_Click(object sender, EventArgs e)
        {
            Arg = argsTextBox.Text;
        }

        private void regexHelpButton_Click(object sender, EventArgs e)
        {
            RegexHelperDialog regexDlg = new RegexHelperDialog();
            if (regexDlg.ShowDialog() == DialogResult.OK)
            {
                argsTextBox.SelectedText = regexDlg.Pattern;
            }
        }


        private void testButton_Click(object sender, EventArgs e)
        {
            if (logTabWin.CurrentLogWindow != null)
            {
                ILogLine line = logTabWin.CurrentLogWindow.GetCurrentLine();
                ILogFileInfo info = logTabWin.CurrentLogWindow.GetCurrentFileInfo();
                if (line != null && info != null)
                {
                    ArgParser parser = new ArgParser(argsTextBox.Text);
                    string args = parser.BuildArgs(line, logTabWin.CurrentLogWindow.GetRealLineNum() + 1, info,
                        this);
                    testResultLabel.Text = args;
                }
            }
        }

        private void ToolArgsDialog_Load(object sender, EventArgs e)
        {
            helpLabel.Text = string.Empty +
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

            argsTextBox.Text = Arg;
        }

        #endregion
    }
}
