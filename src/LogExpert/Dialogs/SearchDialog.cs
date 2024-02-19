using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LogExpert.Entities;
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

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            Load += OnSearchDialogLoad;
        }

        #endregion

        #region Properties

        public SearchParams SearchParams { get; set; }

        #endregion

        #region Events handler

        private void OnSearchDialogLoad(object sender, EventArgs e)
        {
            if (SearchParams != null)
            {
                if (SearchParams.isFromTop)
                {
                    radioButtonFromTop.Checked = true;
                }
                else
                {
                    radioButtonFromSelected.Checked = true;
                }

                if (SearchParams.isForward)
                {
                    radioButtonForward.Checked = true;
                }
                else
                {
                    radioButtonBackward.Checked = true;
                }

                checkBoxRegex.Checked = SearchParams.isRegex;
                checkBoxCaseSensitive.Checked = SearchParams.isCaseSensitive;
                foreach (string item in SearchParams.historyList)
                {
                    comboBoxSearchFor.Items.Add(item);
                }

                if (comboBoxSearchFor.Items.Count > 0)
                {
                    comboBoxSearchFor.SelectedIndex = 0;
                }
            }
            else
            {
                radioButtonFromSelected.Checked = true;
                radioButtonForward.Checked = true;
                SearchParams = new SearchParams();
            }
        }

        private void OnButtonRegexClick(object sender, EventArgs e)
        {
            RegexHelperDialog dlg = new();
            dlg.Owner = this;
            dlg.CaseSensitive = checkBoxCaseSensitive.Checked;
            dlg.Pattern = comboBoxSearchFor.Text;

            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                checkBoxCaseSensitive.Checked = dlg.CaseSensitive;
                comboBoxSearchFor.Text = dlg.Pattern;
            }
        }

        private void OnButtonOkClick(object sender, EventArgs e)
        {
            try
            {
                if (checkBoxRegex.Checked)
                {
                    if (string.IsNullOrWhiteSpace(comboBoxSearchFor.Text))
                    {
                        throw new ArgumentException("Search text is empty");
                    }

                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Regex.IsMatch("", comboBoxSearchFor.Text);
                }

                SearchParams.searchText = comboBoxSearchFor.Text;
                SearchParams.isCaseSensitive = checkBoxCaseSensitive.Checked;
                SearchParams.isForward = radioButtonForward.Checked;
                SearchParams.isFromTop = radioButtonFromTop.Checked;
                SearchParams.isRegex = checkBoxRegex.Checked;
                SearchParams.historyList.Remove(comboBoxSearchFor.Text);
                SearchParams.historyList.Insert(0, comboBoxSearchFor.Text);
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

        private void OnButtonCancelClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}