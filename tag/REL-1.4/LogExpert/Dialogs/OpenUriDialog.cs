using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  public partial class OpenUriDialog : Form
  {
    private IList<string> uriHistoryList;

    public OpenUriDialog()
    {
      InitializeComponent();
    }

    public string Uri
    {
      get { return this.uriComboBox.Text; }
    }

    public IList<string> UriHistory
    {
      get { return this.uriHistoryList; }
      set { this.uriHistoryList = value; }
    }

    private void OpenUriDialog_Load(object sender, EventArgs e)
    {
      if (this.uriHistoryList != null)
      {
        this.uriComboBox.Items.Clear();
        foreach (string uri in this.uriHistoryList)
        {
          this.uriComboBox.Items.Add(uri);
        }
      }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      this.uriHistoryList = new List<string>();
      foreach (var item in this.uriComboBox.Items)
      {
        this.uriHistoryList.Add(item.ToString());
      }
      if (this.uriHistoryList.Contains(this.uriComboBox.Text))
      {
        this.uriHistoryList.Remove(this.uriComboBox.Text);
      }
      this.uriHistoryList.Insert(0, this.uriComboBox.Text);
      while (this.uriHistoryList.Count > 20)
      {
        this.uriHistoryList.RemoveAt(this.uriHistoryList.Count - 1);
      }
    }
  }
}
