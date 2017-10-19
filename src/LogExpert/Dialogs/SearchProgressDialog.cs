using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  public partial class SearchProgressDialog : Form
  {
    private bool shouldStop;

    public SearchProgressDialog()
    {
      InitializeComponent();
      this.shouldStop = false;
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      this.shouldStop = true;
    }

    public bool ShouldStop
    {
      get { return this.shouldStop; }
    }
  }
}
