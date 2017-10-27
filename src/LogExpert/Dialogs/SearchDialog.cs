using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class SearchDialog : Form
    {
        #region Fields

        private static readonly int MAX_HISTORY = 30;

        #endregion

        #region cTor

        public SearchDialog()
        {
            InitializeComponent();
            this.Load += new EventHandler(SearchDialog_Load);
        }

        #endregion

        #region Properties

        public SearchParams SearchParams { get; set; } = null;

        #endregion

        #region Events handler

        private void SearchDialog_Load(object sender, EventArgs e)
        {
            if (this.SearchParams != null)
            {
                if (this.SearchParams.isFromTop)
                {
                    this.fromTopRadioButton.Checked = true;
                }
                else
                {
                    this.fromSelectedRadioButton.Checked = true;
                }

                if (this.SearchParams.isForward)
                {
                    this.forwardRadioButton.Checked = true;
                }
                else
                {
                    this.backwardRadioButton.Checked = true;
                }

                this.regexCheckBox.Checked = this.SearchParams.isRegex;
                this.caseSensitiveCheckBox.Checked = this.SearchParams.isCaseSensitive;
                foreach (string item in this.SearchParams.historyList)
                {
                    this.searchComboBox.Items.Add(item);
                }
                if (this.searchComboBox.Items.Count > 0)
                {
                    this.searchComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.fromSelectedRadioButton.Checked = true;
                this.forwardRadioButton.Checked = true;
                this.SearchParams = new SearchParams();
            }
        }

        private void regexHelperButton_Click(object sender, EventArgs e)
        {
            RegexHelperDialog dlg = new RegexHelperDialog();
            dlg.Owner = this;
            dlg.CaseSensitive = this.caseSensitiveCheckBox.Checked;
            dlg.Pattern = this.searchComboBox.Text;
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                this.caseSensitiveCheckBox.Checked = dlg.CaseSensitive;
                this.searchComboBox.Text = dlg.Pattern;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SearchParams.searchText = this.searchComboBox.Text;
            SearchParams.isCaseSensitive = this.caseSensitiveCheckBox.Checked;
            SearchParams.isForward = this.forwardRadioButton.Checked;
            SearchParams.isFromTop = this.fromTopRadioButton.Checked;
            SearchParams.isRegex = this.regexCheckBox.Checked;
            SearchParams.historyList.Remove(this.searchComboBox.Text);
            SearchParams.historyList.Insert(0, this.searchComboBox.Text);
            if (SearchParams.historyList.Count > MAX_HISTORY)
            {
                SearchParams.historyList.RemoveAt(SearchParams.historyList.Count - 1);
            }
        }

        #endregion
    }
}