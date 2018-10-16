using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class SearchDialog : Form
    {
        #region Static/Constants

        private static readonly int MAX_HISTORY = 30;

        #endregion

        #region Ctor

        public SearchDialog()
        {
            InitializeComponent();
            Load += SearchDialog_Load;
        }

        #endregion

        #region Properties / Indexers

        public SearchParams SearchParams { get; set; }

        #endregion

        #region Private Methods

        private void okButton_Click(object sender, EventArgs e)
        {
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

        #endregion
    }
}
