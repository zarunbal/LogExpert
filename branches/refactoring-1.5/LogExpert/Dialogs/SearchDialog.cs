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
    private SearchParams  searchParams = null;
    private static int MAX_HISTORY = 30;


    public SearchDialog()
    {
      InitializeComponent();
      this.Load += new EventHandler(SearchDialog_Load);
    }

    void SearchDialog_Load(object sender, EventArgs e)
    {
      if (this.searchParams != null)
      {
        if (this.searchParams.isFromTop)
          this.fromTopRadioButton.Checked = true;
        else
          this.fromSelectedRadioButton.Checked = true;

        if (this.searchParams.isForward)
          this.forwardRadioButton.Checked = true;
        else
          this.backwardRadioButton.Checked = true;

        this.regexCheckBox.Checked = this.searchParams.isRegex;
        this.caseSensitiveCheckBox.Checked = this.searchParams.isCaseSensitive;
        foreach (string item in this.searchParams.historyList)
        {
          this.searchComboBox.Items.Add(item);
        }
        if (this.searchComboBox.Items.Count > 0)
          this.searchComboBox.SelectedIndex = 0;

      }
      else
      {
        this.fromSelectedRadioButton.Checked = true;
        this.forwardRadioButton.Checked = true;
        this.searchParams = new SearchParams();
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
      searchParams.searchText = this.searchComboBox.Text;
      searchParams.isCaseSensitive = this.caseSensitiveCheckBox.Checked;
      searchParams.isForward = this.forwardRadioButton.Checked;
      searchParams.isFromTop = this.fromTopRadioButton.Checked;
      searchParams.isRegex = this.regexCheckBox.Checked;
      searchParams.historyList.Remove(this.searchComboBox.Text);
      searchParams.historyList.Insert(0, this.searchComboBox.Text);
      if (searchParams.historyList.Count > MAX_HISTORY)
      {
        searchParams.historyList.RemoveAt(searchParams.historyList.Count - 1);
      }
    }

    public SearchParams SearchParams
    {
      get { return this.searchParams; }
      set { this.searchParams = value; }
    }
  }
}
