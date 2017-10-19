using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert
{
  public partial class EminusConfigDlg : Form
  {
    EminusConfig config;

    public EminusConfig Config
    {
      get { return config; }
      set { config = value; }
    }

    public EminusConfigDlg(EminusConfig config)
    {
      InitializeComponent();
      this.TopLevel = false;
      this.config = config;

      this.hostTextBox.Text = config.host;
      this.portTextBox.Text = "" + config.port;
      this.passwordTextBox.Text = config.password;
    }

    public void ApplyChanges()
    {
      this.config.host = this.hostTextBox.Text;
      try
      {
        this.config.port = Int16.Parse(this.portTextBox.Text);
      }
      catch (FormatException fe)
      {
        this.config.port = 0;
      }
      this.config.password = this.passwordTextBox.Text;
    }
  }
}
 