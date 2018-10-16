using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class RegexHelperDialog : Form
    {
        #region Static/Constants

        private static readonly int MAX_HISTORY = 30;

        #endregion

        #region Private Fields

        private bool caseSensitive;

        #endregion

        #region Ctor

        public RegexHelperDialog()
        {
            InitializeComponent();
            Load += RegexHelperDialog_Load;
        }

        #endregion

        #region Properties / Indexers

        public bool CaseSensitive
        {
            get => caseSensitive;
            set
            {
                caseSensitive = value;
                caseSensitiveCheckBox.Checked = value;
            }
        }

        public string Pattern
        {
            get => expressionComboBox.Text;
            set => expressionComboBox.Text = value;
        }

        #endregion

        #region Private Methods

        private void button1_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "LogExpert.chm", HelpNavigator.Topic, "RegEx.htm");
        }

        private void caseSensitiveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            caseSensitive = caseSensitiveCheckBox.Checked;
            UpdateMatches();
        }

        private void expressionComboBox_TextChanged(object sender, EventArgs e)
        {
            UpdateMatches();
        }

        private void LoadHistory()
        {
            RegexHistory history = ConfigManager.Settings.regexHistory;
            if (history == null)
            {
                return;
            }

            expressionComboBox.Items.Clear();
            foreach (string item in history.expressionHistoryList)
            {
                expressionComboBox.Items.Add(item);
            }

            testTextComboBox.Items.Clear();
            foreach (string item in history.testtextHistoryList)
            {
                testTextComboBox.Items.Add(item);
            }
        }


        private void okButton_Click(object sender, EventArgs e)
        {
            string text = expressionComboBox.Text;
            expressionComboBox.Items.Remove(text);
            expressionComboBox.Items.Insert(0, text);

            text = testTextComboBox.Text;
            testTextComboBox.Items.Remove(text);
            testTextComboBox.Items.Insert(0, text);

            if (expressionComboBox.Items.Count > MAX_HISTORY)
            {
                expressionComboBox.Items.Remove(expressionComboBox.Items.Count - 1);
            }

            if (testTextComboBox.Items.Count > MAX_HISTORY)
            {
                testTextComboBox.Items.Remove(testTextComboBox.Items.Count - 1);
            }

            SaveHistory();
        }

        private void RegexHelperDialog_Load(object sender, EventArgs e)
        {
            LoadHistory();
        }

        private void SaveHistory()
        {
            RegexHistory history = new RegexHistory();
            foreach (string item in expressionComboBox.Items)
            {
                history.expressionHistoryList.Add(item);
            }

            foreach (string item in testTextComboBox.Items)
            {
                history.testtextHistoryList.Add(item);
            }

            ConfigManager.Settings.regexHistory = history;
            ConfigManager.Save(SettingsFlags.RegexHistory);
        }

        private void testTextComboBox_TextChanged(object sender, EventArgs e)
        {
            UpdateMatches();
        }

        private void UpdateMatches()
        {
            matchesTextBox.Text = string.Empty;
            try
            {
                Regex rex = new Regex(expressionComboBox.Text,
                    caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                MatchCollection matches = rex.Matches(testTextComboBox.Text);
                foreach (Match match in matches)
                {
                    matchesTextBox.Text += match.Value + "\r\n";
                }
            }
            catch (ArgumentException)
            {
                matchesTextBox.Text = "No valid regex pattern";
            }
        }

        #endregion
    }
}
