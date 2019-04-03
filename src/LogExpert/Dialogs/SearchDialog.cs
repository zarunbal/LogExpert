using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NLog;

namespace LogExpert.Dialogs
{
    public partial class SearchDialog : Form
    {
        #region Fields

        private static readonly int MAX_HISTORY = 30;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region cTor

        public SearchDialog()
        {
            InitializeComponent();
            Load += new EventHandler(SearchDialog_Load);
        }

        #endregion

        #region Properties

        public SearchParams SearchParams { get; set; } = null;

        #endregion

        #region Events handler

        private void SearchDialog_Load(object sender, EventArgs e)
        {
            if (SearchParams != null)
            {
                if (SearchParams.isFromTop)
                {
                    fromTopRadioButton.Checked = true;
                }
                else
                {
                    fromSelectedRadioButton.Checked = true;
                }

                if (SearchParams.isForward)
                {
                    forwardRadioButton.Checked = true;
                }
                else
                {
                    backwardRadioButton.Checked = true;
                }

                regexCheckBox.Checked = SearchParams.isRegex;
                caseSensitiveCheckBox.Checked = SearchParams.isCaseSensitive;
                foreach (string item in SearchParams.historyList)
                {
                    searchComboBox.Items.Add(item);
                }

                if (searchComboBox.Items.Count > 0)
                {
                    searchComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                fromSelectedRadioButton.Checked = true;
                forwardRadioButton.Checked = true;
                SearchParams = new SearchParams();
            }
        }

        private void regexHelperButton_Click(object sender, EventArgs e)
        {
            RegexHelperDialog dlg = new RegexHelperDialog();
            dlg.Owner = this;
            dlg.CaseSensitive = caseSensitiveCheckBox.Checked;
            dlg.Pattern = searchComboBox.Text;
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                caseSensitiveCheckBox.Checked = dlg.CaseSensitive;
                searchComboBox.Text = dlg.Pattern;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (regexCheckBox.Checked)
                {
                    if (string.IsNullOrWhiteSpace(searchComboBox.Text))
                    {
                        throw new ArgumentException("Search text is empty");
                    }

                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Regex.IsMatch("", searchComboBox.Text);
                }

                SearchParams.searchText = searchComboBox.Text;
                SearchParams.isCaseSensitive = caseSensitiveCheckBox.Checked;
                SearchParams.isForward = forwardRadioButton.Checked;
                SearchParams.isFromTop = fromTopRadioButton.Checked;
                SearchParams.isRegex = regexCheckBox.Checked;
                SearchParams.historyList.Remove(searchComboBox.Text);
                SearchParams.historyList.Insert(0, searchComboBox.Text);
                if (SearchParams.historyList.Count > MAX_HISTORY)
                {
                    SearchParams.historyList.RemoveAt(SearchParams.historyList.Count - 1);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during ok click");
                MessageBox.Show($"Error during creation of search parameter\r\n{ex.Message}");
            }
        }

        #endregion
    }
}