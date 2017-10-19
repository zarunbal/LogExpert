using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  public partial class GotoLineDialog : Form
  {
    private int line;

    public GotoLineDialog(Form parent)
    {
      InitializeComponent();
      this.Owner = parent;
    }

    private void GotoLineDialog_Load(object sender, EventArgs e)
    {

    }


    public int Line
    {
      get { return this.line; }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      try
      {
        this.line = Int32.Parse(this.lineNumberTextBox.Text);
      }
      catch (Exception)
      {
        this.line = -1;
      }
    }
  }
}
