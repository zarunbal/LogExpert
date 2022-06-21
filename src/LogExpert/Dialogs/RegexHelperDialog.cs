using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using LogExpert.Config;

namespace LogExpert.Dialogs
{
    public partial class RegexHelperDialog : Form
    {
        #region Fields

        private static readonly int MAX_HISTORY = 30;
        private bool caseSensitive = false;

        #endregion

        #region cTor

        public RegexHelperDialog()
        {
            InitializeComponent();
            this.Load += new EventHandler(RegexHelperDialog_Load);
        }

        #endregion

        #region Properties

        public bool CaseSensitive
        {
            get { return this.caseSensitive; }
            set
            {
                this.caseSensitive = value;
                this.caseSensitiveCheckBox.Checked = value;
            }
        }

        public string Pattern
        {
            get { return this.expressionComboBox.Text; }
            set { this.expressionComboBox.Text = value; }
        }

        #endregion

        #region Private Methods

        private void UpdateMatches()
        {
            this.matchesTextBox.Text = "";
            try
            {
                Regex rex = new Regex(this.expressionComboBox.Text,
                    this.caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                MatchCollection matches = rex.Matches(this.testTextComboBox.Text);
                foreach (Match match in matches)
                {
                    this.matchesTextBox.Text += match.Value + "\r\n";
                }
            }
            catch (ArgumentException)
            {
                this.matchesTextBox.Text = "No valid regex pattern";
            }
        }

        private void LoadHistory()
        {
            RegexHistory history = ConfigManager.Settings.regexHistory;
            if (history == null)
            {
                return;
            }
            this.expressionComboBox.Items.Clear();
            foreach (string item in history.expressionHistoryList)
            {
                this.expressionComboBox.Items.Add(item);
            }
            this.testTextComboBox.Items.Clear();
            foreach (string item in history.testtextHistoryList)
            {
                this.testTextComboBox.Items.Add(item);
            }
        }

        private void SaveHistory()
        {
            RegexHistory history = new RegexHistory();
            foreach (string item in this.expressionComboBox.Items)
            {
                history.expressionHistoryList.Add(item);
            }
            foreach (string item in this.testTextComboBox.Items)
            {
                history.testtextHistoryList.Add(item);
            }
            ConfigManager.Settings.regexHistory = history;
            ConfigManager.Save(SettingsFlags.RegexHistory);
        }

        #endregion

        #region Events handler

        private void RegexHelperDialog_Load(object sender, EventArgs e)
        {
            LoadHistory();
        }

        private void caseSensitiveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.caseSensitive = this.caseSensitiveCheckBox.Checked;
            UpdateMatches();
        }


        private void okButton_Click(object sender, EventArgs e)
        {
            string text = this.expressionComboBox.Text;
            this.expressionComboBox.Items.Remove(text);
            this.expressionComboBox.Items.Insert(0, text);

            text = this.testTextComboBox.Text;
            this.testTextComboBox.Items.Remove(text);
            this.testTextComboBox.Items.Insert(0, text);

            if (this.expressionComboBox.Items.Count > MAX_HISTORY)
            {
                this.expressionComboBox.Items.Remove(this.expressionComboBox.Items.Count - 1);
            }
            if (this.testTextComboBox.Items.Count > MAX_HISTORY)
            {
                this.testTextComboBox.Items.Remove(this.testTextComboBox.Items.Count - 1);
            }

            SaveHistory();
        }

        private void expressionComboBox_TextChanged(object sender, EventArgs e)
        {
            UpdateMatches();
        }

        private void testTextComboBox_TextChanged(object sender, EventArgs e)
        {
            UpdateMatches();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "LogExpert.chm", HelpNavigator.Topic, "RegEx.htm");
        }

        #endregion
    }
}