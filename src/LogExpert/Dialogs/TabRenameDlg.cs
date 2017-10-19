using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  public partial class TabRenameDlg : Form
  {
    public TabRenameDlg()
    {
      InitializeComponent();
    }

    public string TabName
    {
      get
      {
        return this.tabNameTextBox.Text;
      }
      set
      {
        this.tabNameTextBox.Text = value;
      }
    }

    private void TabRenameDlg_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Escape)
      {
        DialogResult = DialogResult.Cancel;
        Close();
      }
    }
  }
}
